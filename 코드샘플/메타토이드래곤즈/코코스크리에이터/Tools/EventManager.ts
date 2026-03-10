
import { _decorator } from 'cc';
import { EventData, EventListener, IManagerBase } from 'sb';
import { GameManager } from '../GameManager';
import { ObjectCheck } from './SandboxTools';

/**
 * Predefined variables
 * Name = EventManager
 * DateTime = Thu Feb 17 2022 19:07:02 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = EventManager.ts
 * FileBasenameNoExtension = EventManager
 * URL = db://assets/Scripts/Tools/EventManager.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
export class EventManager implements IManagerBase {
    static Name: string = "EventManager";

    private static instance: EventManager = null;
    public static get Instance() {
        if(EventManager.instance == null) {
            return EventManager.instance = new EventManager();
        }
        return EventManager.instance;
    }

    private static eventList: {} = null;
    
    Init(): void {
        EventManager.eventList = {};
        GameManager.Instance.AddManager(this, false);
    }
    GetManagerName(): string {
        return EventManager.Name;
    }
    Update(deltaTime: number): void {
        return;
    }

    static AddEvent<T extends EventData>(listener: EventListener<T>): void {
        if(!ObjectCheck(EventManager.eventList, listener.GetID())) {
            EventManager.eventList[listener.GetID()] = [];
        }

        if(EventManager.eventList[listener.GetID()].length > 0) {
            const count = EventManager.eventList[listener.GetID()].length;
            var exists = false;

            for(var i = 0 ; i < count ; i++) {
                if(EventManager.eventList[listener.GetID()][i] == listener) {
                    exists = true;
                    break;
                }
            }

            if(!exists) {
                EventManager.eventList[listener.GetID()].push(listener);
            }
        } else {
            EventManager.eventList[listener.GetID()].push(listener);
        }
    }

    static RemoveEvent<T extends EventData>(listener: EventListener<T>): void {
        if(!ObjectCheck(EventManager.eventList, listener.GetID())) {
            return;
        }
        let list = EventManager.eventList[listener.GetID()];
        const count = list.length;
        if(count > 0) {
            EventManager.eventList[listener.GetID()] = list.filter(element => element != listener);
        }
    }

    static TriggerEvent<T extends EventData>(data: T): void {
        if(!ObjectCheck(EventManager.eventList, data.GetID())) {
            return;
        }

        let list = EventManager.eventList[data.GetID()];

        if(list != null && list.length > 0) {
            list.forEach(element => {
                (element as EventListener<T>)?.OnEvent(data);
            });
        }
    }

    static EventStartListening<T extends EventListener<T>>(caller: EventListener<T>) {
        EventManager.AddEvent(caller)
    }

    static EventStopListening<T extends EventListener<T>>(caller: EventListener<T>) {
        EventManager.RemoveEvent(caller)
    }
}

/**
 * [1] Class member could be defined like this.
 * [2] Use `property` decorator if your want the member to be serializable.
 * [3] Your initialization goes here.
 * [4] Your update function goes here.
 *
 * Learn more about scripting: https://docs.cocos.com/creator/3.3/manual/en/scripting/
 * Learn more about CCClass: https://docs.cocos.com/creator/3.3/manual/en/scripting/ccclass.html
 * Learn more about life-cycle callbacks: https://docs.cocos.com/creator/3.3/manual/en/scripting/life-cycle-callbacks.html
 */

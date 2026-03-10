
import { _decorator, Component, Node } from 'cc';
import { EventData } from 'sb';
import { EventManager } from '../Tools/EventManager';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = ShadowEvent
 * DateTime = Thu May 26 2022 11:10:23 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = ShadowEvent.ts
 * FileBasenameNoExtension = ShadowEvent
 * URL = db://assets/Scripts/Character/ShadowEvent.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('ShadowEvent')
export class ShadowEvent implements EventData {
    private static instance: ShadowEvent = null;
    private jsonData: any = null;
    public get Data(): any {
        return this.jsonData;
    }
    GetID(): string {
        return 'ShadowEvent';
    }

    public static TriggerEvent(jsonData) {
        if(ShadowEvent.instance == null) {
            ShadowEvent.instance = new ShadowEvent();
        }

        ShadowEvent.instance.jsonData = jsonData;
        EventManager.TriggerEvent(ShadowEvent.instance);
    }
}

/**
 * [1] Class member could be defined like this.
 * [2] Use `property` decorator if your want the member to be serializable.
 * [3] Your initialization goes here.
 * [4] Your update function goes here.
 *
 * Learn more about scripting: https://docs.cocos.com/creator/3.4/manual/en/scripting/
 * Learn more about CCClass: https://docs.cocos.com/creator/3.4/manual/en/scripting/decorator.html
 * Learn more about life-cycle callbacks: https://docs.cocos.com/creator/3.4/manual/en/scripting/life-cycle-callbacks.html
 */

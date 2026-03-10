
import { _decorator } from 'cc';
import { IManagerBase } from 'sb';
import { TimeString } from '../Tools/SandboxTools';
import { GameManager } from './../GameManager';
import { TimeObject } from './ITimeObject';

/**
 * Predefined variables
 * Name = TimeManager
 * DateTime = Wed Jan 12 2022 17:24:25 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = TimeManager.ts
 * FileBasenameNoExtension = TimeManager
 * URL = db://assets/Scripts/TimeManager.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
export class TimeManager implements IManagerBase {
    public static Name: string = "TimeManager";
    protected static instance: TimeManager = null;

    protected curTime: number = 0;
    protected tempTime: number = 0;

    protected timeObjects: TimeObject[] = null;

    public static get Instance() {
        if(TimeManager.instance == null) {
            return TimeManager.instance = new TimeManager();
        }
        return TimeManager.instance;
    }

    Init(): void {
        this.timeObjects = [];
        GameManager.Instance.AddManager(this, true);
    }

    GetManagerName(): string {
        return TimeManager.Name;
    }

    public static TimeRefresh(time: number): void {
        TimeManager.Instance.curTime = time;
        TimeManager.Instance.tempTime = 0;
    }

    public static GetTime(): number {
        return TimeManager.Instance.curTime;
    }

    public static GetTimeCompare(target: number): number {
        return target - TimeManager.Instance.curTime;
    }

    public static GetTimeCompareString(target: number): string {
        return TimeString(TimeManager.GetTimeCompare(target));
    }

    public static AddObject(target: TimeObject): void {
        if(target == null) {
            return;
        }
        TimeManager.Instance.timeObjects.push(target);
    }

    public static DelObject(target: TimeObject): void {
        if(target == null) {
            return;
        }
        TimeManager.Instance.timeObjects = TimeManager.Instance.timeObjects.filter((val) => val != target);
    }

    Update(deltaTime: number): void {
        this.tempTime += deltaTime;
        if(this.tempTime >= 1) {
            this.tempTime -= 1;
            this.curTime += 1;
            this.timeObjects.forEach(element => {
                if(element == null || element.Refresh == undefined) {
                    return;
                }
                element.Refresh();
            });
        }
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

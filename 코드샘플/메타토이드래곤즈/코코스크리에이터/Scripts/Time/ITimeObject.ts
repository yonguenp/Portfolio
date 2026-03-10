
import { _decorator, Component } from 'cc';
import { ITimeObject } from 'sb';
import { TimeManager } from './TimeManager';
const { ccclass } = _decorator;

/**
 * Predefined variables
 * Name = ITimeObject
 * DateTime = Wed Jan 19 2022 17:34:05 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = ITimeObject.ts
 * FileBasenameNoExtension = ITimeObject
 * URL = db://assets/Scripts/Time/ITimeObject.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */

@ccclass("TimeObject")
export class TimeObject extends Component implements ITimeObject {
    curTime: number;
    Refresh : () => void;

    start () {
        this.Init();
    }

    onDestroy () {
        this.Destroy();
    }

    Init(): void {
        TimeManager.AddObject(this);
    }
    Destroy(): void {
        TimeManager.DelObject(this);
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

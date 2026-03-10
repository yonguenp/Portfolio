
import { _decorator, Component, Node } from 'cc';
import { TimeObject } from './ITimeObject';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = TimeEnabled
 * DateTime = Wed May 25 2022 12:06:41 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = TimeEnabled.ts
 * FileBasenameNoExtension = TimeEnabled
 * URL = db://assets/Scripts/Time/TimeEnabled.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('TimeEnabled')
export class TimeEnabled extends TimeObject {
    start () {
    }

    onDestroy () {
    }

    onEnable() {
        this.Init();
    }
    onDisable() {
        this.Destroy();
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

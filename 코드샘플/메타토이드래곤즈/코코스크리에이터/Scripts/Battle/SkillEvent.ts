
import { _decorator, Component, Node } from 'cc';
import { EventData } from 'sb';
import { EventManager } from '../Tools/EventManager';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = SkillEvent
 * DateTime = Thu May 19 2022 16:19:34 GMT+0900 (대한민국 표준시)
 * Author = jinwonjun
 * FileBasename = SkillEvent.ts
 * FileBasenameNoExtension = SkillEvent
 * URL = db://assets/Scripts/Battle/SkillEvent.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */

export class SkillEvent implements EventData {
    private static instance: SkillEvent = null;
    private jsonData: any = null;
    public get Data(): any {
        return this.jsonData;
    }
    GetID(): string {
        return 'SkillEvent';
    }

    public static TriggerEvent(jsonData) {
        if(SkillEvent.instance == null) {
            SkillEvent.instance = new SkillEvent();
        }

        SkillEvent.instance.jsonData = jsonData;
        EventManager.TriggerEvent(SkillEvent.instance);
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

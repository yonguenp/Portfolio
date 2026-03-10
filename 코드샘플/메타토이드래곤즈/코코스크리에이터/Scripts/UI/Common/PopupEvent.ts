
import { _decorator } from 'cc';
import { EventData } from 'sb';
const { ccclass } = _decorator;

/**
 * Predefined variables
 * Name = PopupEvent
 * DateTime = Tue Apr 26 2022 18:51:15 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = PopupEvent.ts
 * FileBasenameNoExtension = PopupEvent
 * URL = db://assets/Scripts/UI/Common/PopupEvent.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */

//마찬가지로 미사용.
export enum ePopupEventType { 
    None = 0,
    PopupOpen = 1,
    PopupClose = 2,
    PopupAllClose = 3,
    PopupRefresh = 4,
}
 
@ccclass('PopupEvent')
export class PopupEvent implements EventData {
    private data: any = null;
    public get Data(): any {
        return this.data;
    }

    constructor(data: any) {
        this.data = data;
    }

    GetID(): string {
        return "PopupEvent";
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

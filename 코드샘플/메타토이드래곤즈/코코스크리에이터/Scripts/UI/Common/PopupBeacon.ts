
import { _decorator, Component, Node } from 'cc';
import { PopupManager } from './PopupManager';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = PopupBeacon
 * DateTime = Thu May 19 2022 21:44:23 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = PopupBeacon.ts
 * FileBasenameNoExtension = PopupBeacon
 * URL = db://assets/Scripts/UI/PopupBeacon.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('PopupBeacon')
export class PopupBeacon extends Component {
    onLoad() {
        PopupManager.GetInstance.Init(this);
    }

    onDestroy() {
        PopupManager.AllStackPop();
    }

    onEnable() {
        PopupManager.GetInstance.Init(this, false);
    }

    update (deltaTime: number) {
        PopupManager.GetInstance?.Update(deltaTime);
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


import { _decorator, Component, Node } from 'cc';
import { IPopupExtension } from 'sb';
import { Popup } from './Popup';
const { ccclass, requireComponent } = _decorator;

/**
 * Predefined variables
 * Name = PopupExtension
 * DateTime = Tue Apr 26 2022 19:50:50 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = PopupExtension.ts
 * FileBasenameNoExtension = PopupExtension
 * URL = db://assets/Scripts/UI/Common/PopupExtension.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
@ccclass('PopupExtension')// 상속 받아서 팝업의 확장 기능 구현
@requireComponent(Popup)
export class PopupExtension extends Component implements IPopupExtension {
    protected popup: Popup = null;
    Init(): void {// 시작 시 팝업 Init에서 같이 동작하도록 구현됨.
        this.popup = this.getComponent(Popup);
    }

    ForceUpdate(): void {//팝업의 Refresh에서 호출됨
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

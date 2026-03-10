
import { _decorator, Component, Node, AnimationComponent, AnimationClip } from 'cc';
import { Popup } from '../UI/Common/Popup';
const { ccclass, property } = _decorator;
 
@ccclass('TutorialFinishPopup')
export class TutorialFinishPopup extends Popup {

    OnClickOkButton() {
        this.ClosePopup();
    }
}

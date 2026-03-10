
import { _decorator, Component, Node, Sprite } from 'cc';
import { CapsuleInfo } from './CapsuleInfo';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = GachaTotalResultSlot
 * DateTime = Wed Mar 16 2022 11:15:36 GMT+0900 (대한민국 표준시)
 * Author = blacktopaz
 * FileBasename = GachaTotalResultSlot.ts
 * FileBasenameNoExtension = GachaTotalResultSlot
 * URL = db://assets/Scripts/Gacha/GachaTotalResultSlot.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('GachaTotalResultSlot')
export class GachaTotalResultSlot extends Component {

    @property(Sprite)
    iconSprite: Sprite = null;

    curCapsuleInfo: CapsuleInfo = null;

    start () {
        // [3]
    }

    SetSlotInfo(curCapsule: CapsuleInfo) {
        if (curCapsule == null) {return;}

        this.curCapsuleInfo = curCapsule;

        // this.iconSprite = this.curCapsuleInfo.objectIcon;
    }

    // update (deltaTime: number) {
    //     // [4]
    // }
}
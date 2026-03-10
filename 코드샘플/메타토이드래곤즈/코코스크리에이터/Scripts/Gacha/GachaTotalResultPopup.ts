
import { _decorator, Component, Node, Prefab } from 'cc';
import { CapsuleInfo } from './CapsuleInfo';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = GachaTotalResultPopup
 * DateTime = Wed Mar 16 2022 10:54:14 GMT+0900 (대한민국 표준시)
 * Author = blacktopaz
 * FileBasename = GachaTotalResultPopup.ts
 * FileBasenameNoExtension = GachaTotalResultPopup
 * URL = db://assets/Scripts/Gacha/GachaTotalResultPopup.ts
 *
 */
 
@ccclass('GachaTotalResultPopup')
export class GachaTotalResultPopup extends Component {
    
    @property(Node)
    gachaEffectNode: Node = null;

    @property(Node)
    parentNode: Node = null;    // 결과 아이콘을 생성할 부모 Node

    @property(Prefab)
    resultSlot: Prefab = null;  // 결과 아이콘 Slot

    resultCapsuleInfoList: Array<CapsuleInfo> = new Array<CapsuleInfo>();     // 뽑기 연출을 위한 뽑기 결과 리스트

    // Init() 
    //{
    //     if (this.gachaEffectNode == null) {return;}

    //     this.RefreshPopup();

    //     this.resultCapsuleInfoList = this.gachaEffectNode.getComponent(GachaEffect).originCapsuleInfoList;

    //     for (let capsuleData of this.resultCapsuleInfoList) {
    //         let resultSlotNode: Node = null;
    //         resultSlotNode = instantiate(this.resultSlot);

    //         resultSlotNode.setParent(this.parentNode);

    //         resultSlotNode.getComponent(GachaTotalResultSlot).SetSlotInfo(capsuleData);
    //     }

    //     this.node.active = true;
    // }

    // onClickExitButton() {
    //     this.node.active = false;

    //     this.gachaEffectNode.getComponent(GachaEffect).BackToGachaUI();
    // }

    RefreshPopup() 
    {
        for (let childNode of this.parentNode.children) 
            childNode.destroy();
    }
}
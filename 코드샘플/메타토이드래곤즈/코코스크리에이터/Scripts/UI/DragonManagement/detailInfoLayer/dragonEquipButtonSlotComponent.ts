
import { _decorator, Component, Label, Sprite, SpriteFrame , Node, Animation} from 'cc';
import { GameManager } from '../../../GameManager';
import { ResourceManager, ResourcesType } from '../../../ResourceManager';
import { User } from '../../../User/User';
import { dragonEquipDetailInfoComponent } from '../equipItemLayer/dragonEquipDetailInfoComponent';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = dragonEquipButtonSlotComponent
 * DateTime = Mon Mar 21 2022 16:49:31 GMT+0900 (대한민국 표준시)
 * Author = wonjun_gs
 * FileBasename = dragonEquipButtonSlotComponent.ts
 * FileBasenameNoExtension = dragonEquipButtonSlotComponent
 * URL = db://assets/Scripts/UI/DragonManagement/detailInfoLayer/dragonEquipButtonSlotComponent.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 //dragonInfo에서의 equipmet Slot 버튼
@ccclass('dragonEquipButtonSlotComponent')
export class dragonEquipButtonSlotComponent extends Component {
    
    @property(Sprite)
    slotSprite : Sprite = null;

    @property(SpriteFrame)
    emptySpriteRes : SpriteFrame = null;

    @property(SpriteFrame)
    lockSpriteRes : SpriteFrame = null;

    @property(Node)
    slotAnimNode : Node = null;

    slotPartTag : number = 0;//고유 번호
    public get SlotPartTag()
    {
        return this.slotPartTag;
    }

    slotPartID : number = 0;//아이템 id
    public get SlotPartID()
    {
        return this.slotPartID;
    }

    refreshSlot(isUnLock : boolean, partTag : number, partID : number)
    {
        this.slotPartID = partID;
        this.slotPartTag = partTag;
        
        this.initAnimNode();
        this.slotSprite.node.active = true;
        if(isUnLock){
            this.unLockSlot();
            if(partTag > 0 && partID > 0){
                this.refreshSlotSprite(partTag);
            }
        }
        else{
            this.LockSlot();//잠긴 슬롯 표시 리소스 필요 임시로 꺼둠
        }
    }

    initAnimNode()
    {
        if(this.slotAnimNode != null){
            this.slotAnimNode.active = false;
        }
    }

    ShowAnimArrowNode()
    {
        if(this.slotAnimNode != null && this.slotAnimNode.activeInHierarchy == false){
            this.slotAnimNode.active = true;

            let animComponent = this.slotAnimNode.getComponent(Animation);
            if(animComponent == null){
                return;
            }
            animComponent.play();
        }
    }

    HideAnimArrowNode()
    {
        if(this.slotAnimNode != null && this.slotAnimNode.activeInHierarchy == true){
            this.slotAnimNode.active = false;
        }
    }

    refreshSlotSprite(partTag : number)
    {
        let partData = User.Instance.partData.GetPart(partTag);
        if(partData == null)
        {
            console.log("partData is null");
            return;
        }

        let partDesignData = partData.GetItemDesignData();
        this.slotSprite.spriteFrame = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.PARTICON_SPRITEFRAME)[partDesignData.ICON]
    }
    //각 상태에 따른 슬롯 데이터 및 이미지 갱신
    unLockSlot()
    {
        if(this.lockSpriteRes != null){
            this.slotSprite.spriteFrame = this.emptySpriteRes;
            return;
        }
    }
    LockSlot()
    {
        if(this.lockSpriteRes != null){
            this.slotSprite.spriteFrame = this.lockSpriteRes;
            return;
        }
    }

    onClickSlotDetailData(dragonPartNode : Node)
    {
        if(this.SlotPartTag > 0)
        {
            let dragonPartInfo  = dragonPartNode.getComponent(dragonEquipDetailInfoComponent);
            if(dragonPartInfo != null)
            {
                dragonPartInfo.ShowDetailInfo(Number(this.SlotPartTag));
            }
        }
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

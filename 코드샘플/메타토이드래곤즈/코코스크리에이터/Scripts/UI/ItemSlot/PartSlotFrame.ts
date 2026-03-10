
import { _decorator, Component, Node, Sprite, Label } from 'cc';
import { CharBaseTable } from '../../Data/CharTable';
import { TableManager } from '../../Data/TableManager';
import { GameManager } from '../../GameManager';
import { ResourceManager, ResourcesType } from '../../ResourceManager';
import { User } from '../../User/User';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = PartSlotFrame
 * DateTime = Wed Mar 23 2022 20:46:43 GMT+0900 (대한민국 표준시)
 * Author = wonjun_gs
 * FileBasename = PartSlotFrame.ts
 * FileBasenameNoExtension = PartSlotFrame
 * URL = db://assets/Scripts/UI/ItemSlot/PartSlotFrame.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('PartSlotFrame')
export class PartSlotFrame extends Component {
    @property(Sprite)
    partIcon : Sprite = null
    @property(Label)
    partlevelLabel : Label = null;
    @property(Node)
    dragonThumbnNailNode : Node = null;
    @property(Sprite)
    dragonIcon : Sprite = null;

    private clickCallback: (CustomEventData? : string) => void = null;

    private partTag : number = 0;//장비 tag 값
    
    /**
     * @param partTag //파츠 tag (고유번호)
     * @param level //파츠 강화 상태 수준 (0이면 미표시)
     */
    SetPartSlotFrame(partTag : number, level : number = 0)
    {
        let dragonTag = this.getDragonTag(partTag);

        let isDragonTag = (dragonTag > 0);
        this.dragonThumbnNailNode.active = isDragonTag;
        if(isDragonTag)
        {
            this.drawDragonIcon(dragonTag);//드래곤 초상화 세팅
        }

        this.drawPartIcon(partTag);//파츠 이미지 세팅

        if(level > 0)
        {
            this.partlevelLabel.string = ('+' + level.toString());//파츠 강화 수치 세팅
        }
        else{
            this.partlevelLabel.node.active = false;
        }
        
        this.partTag = partTag;
    }
    
    setCallback(ok_cb? : (CustomEventData? : string) => void)
    {
        if(ok_cb != null)
        {
            this.clickCallback = ok_cb;
        }
    }

    //현재 파츠가 드래곤에 링크되있으면 드래곤 태그 리턴
    getDragonTag(partTag : number) : number
    {
        return User.Instance.partData.GetPartLink(partTag);
    }

    drawDragonIcon(dragonTag : number)
    {
        let dragonInfo = TableManager.GetTable<CharBaseTable>(CharBaseTable.Name).Get(dragonTag);
        let icon : any = null;
        if(dragonInfo != null){
            icon = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.CHARACTORICON_SPRITEFRAME)[dragonInfo.THUMBNAIL]
        
            if(icon == null){
                icon = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.CHARACTORICON_SPRITEFRAME)["character_01"]
            }
        }
        this.dragonIcon.spriteFrame = icon;
    }

    drawPartIcon(partTag : number)
    {
        let image : any = null;
        let partData = User.Instance.partData.GetPart(partTag);
        if(partData != null)
        {
            let designData = partData.GetItemDesignData();
            if(designData != null)
            {
                let imageStr = designData.ICON;
                image = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.PARTICON_SPRITEFRAME)[imageStr]
            }
        }
        this.partIcon.spriteFrame = image;
    }

    onClickFrame()
    {   
        if(this.clickCallback != null){
            this.clickCallback(this.partTag.toString());//버튼 클릭시 태그값 던짐
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

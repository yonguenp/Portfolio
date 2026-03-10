
import { _decorator, Component, Sprite, Label, SpriteFrame } from 'cc';
import { ItemBaseTable } from '../../Data/ItemTable';
import { TableManager } from '../../Data/TableManager';
import { GameManager } from '../../GameManager';
import { NetworkManager } from '../../NetworkManager';
import { ResourceManager, ResourcesType } from '../../ResourceManager';
import { StringBuilder } from '../../Tools/SandboxTools';
import { eLandmarkType, LandmarkSubway, User } from '../../User/User';
import { PopupManager } from '../Common/PopupManager';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = TrainItemFrame
 * DateTime = Tue Feb 08 2022 17:26:30 GMT+0900 (대한민국 표준시)
 * Author = seungmin
 * FileBasename = TrainItemFrame.ts
 * FileBasenameNoExtension = TrainItemFrame
 * URL = db://assets/Scripts/UI/ItemSlot/TrainItemFrame.ts
 *
 */
 
@ccclass('TrainItemFrame')
export class TrainItemFrame extends Component 
{
    @property(Sprite)
    sprIcon : Sprite = null

    @property(Label)
    labelAmount : Label = null

    @property(SpriteFrame)
    frameChecker : SpriteFrame = null
    
    private slotCheck : boolean = false
    private slotid : number = 0
    private frameid : number = 0

    SetFrame(slotID : number, frameID : number)
    {
        //본인 인덱스 검사해서 slotCheck 최신화
        
        let slotInfo = (User.Instance.GetLandmarkData(eLandmarkType.SUBWAY) as LandmarkSubway).PlatsData[slotID]
        let itemInfo = slotInfo.slots[frameID]

        this.slotid = slotInfo.id
        this.frameid = frameID
        
        if(itemInfo[2] == itemInfo[1])
        {
            this.sprIcon.spriteFrame = this.frameChecker
            this.labelAmount.node.active = false
            this.slotCheck == true
        }
        else
        {
            this.sprIcon.spriteFrame = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.ITEMICON_SPRITEFRAME)[TableManager.GetTable<ItemBaseTable>(ItemBaseTable.Name).Get(itemInfo[0]).ICON]
            this.labelAmount.string = StringBuilder("{0}/{1}", itemInfo[2], itemInfo[1])
        }
    }

    onClickSlot()
    {
        if(this.slotCheck)
        {
            return
        }

        let params = 
        {
            plat : this.slotid,
            slot : this.frameid
        }

        NetworkManager.Send("subway/input", params, ()=>
        {
            PopupManager.ForceUpdate()
        })
    }
}
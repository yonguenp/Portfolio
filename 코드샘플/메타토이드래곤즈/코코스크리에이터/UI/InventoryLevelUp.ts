
import { _decorator, Node, Label, Sprite, instantiate, UITransform, math } from 'cc';
import { InventoryTable } from '../Data/InventoryTable';
import { StringTable } from '../Data/StringTable';
import { TableManager } from '../Data/TableManager';
import { GameManager } from '../GameManager';
import { NetworkManager } from '../NetworkManager';
import { ResourceManager, ResourcesType } from '../ResourceManager';
import { ObjectCheck } from '../Tools/SandboxTools';
import { User } from '../User/User';
import { PopupManager } from './Common/PopupManager';
import { ItemFrame } from './ItemSlot/ItemFrame';
import { SystemPopup } from './SystemPopup';
const { ccclass, property } = _decorator;
 
@ccclass('InventoryLevelUp')
export class InventoryLevelUp extends SystemPopup
{
    @property(Node)
    contentNode : Node = null

    @property(Label)
    labelBefSlot : Label = null
    
    @property(Label)
    labelAfSlot : Label = null

    @property(Node)
    costNode : Node = null

    @property(Sprite)
    sprCost : Sprite = null

    @property(Label)
    labelCost : Label = null

    init()
    {
        //만렙일때 팝업 예외처리 필요
        let invenLevel = User.Instance.InvenStep
        let invenData = TableManager.GetTable(InventoryTable.Name).Get(invenLevel)
        let invenNextData = TableManager.GetTable(InventoryTable.Name).Get(invenLevel +1)
        
        if(invenNextData == null)//만렙일 경우
        {
            let popup = PopupManager.OpenPopup("SystemPopupOK") as SystemPopup;
            popup.setMessage(StringTable.GetString(100000248), StringTable.GetString(100000622));
            this.ClosePopup();
            return;
        }   

        this.labelBefSlot.string = invenData.SLOT.toString()
        this.labelAfSlot.string = invenNextData.SLOT.toString()

        if(invenData.NEED_ITEM.length > 0)
        {
            for(let i = 0; i < invenData.NEED_ITEM.length; i++)
            {
                let itemClone : Node = instantiate(GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB)['item'])
                itemClone.getComponent(UITransform).setContentSize(new math.Size(50, 50))
                itemClone.getComponent(ItemFrame).setFrameReceipeInfo(invenData.NEED_ITEM[i].ITEM_NO, invenData.NEED_ITEM[i].ITEM_COUNT)
                itemClone.parent = this.contentNode
            }
        }

        //추후 코스트 타입 정해지면 변경해야함
        if(invenData.COST_NUM > 0)
        {
            this.costNode.active = true
            //this.sprCost.spriteFrame = ""     //코스트 타입에 따라 아이콘 변경해야함
            this.labelCost.string = invenData.COST_NUM.toString()
        }
    }

    private onClickLevelUp()
    {
        let params = {}
        NetworkManager.Send("item/expand", params, (jsonObj) => {

            if(ObjectCheck(jsonObj, "inventory_step"))
                User.Instance.InvenStep = jsonObj.inventory_step
            this.ClosePopup()
            PopupManager.ForceUpdate()
        })
    }
}


import { _decorator, Node, Label, Prefab, instantiate, Button, ScrollView } from 'cc';
import { InventoryData } from '../../Data/InventoryData';
import { InventoryTable } from '../../Data/InventoryTable';
import { ItemBaseData } from '../../Data/ItemData';
import { ItemBaseTable } from '../../Data/ItemTable';
import { TableManager } from '../../Data/TableManager';
import { GameManager } from '../../GameManager';
import { ResourceManager, ResourcesType } from '../../ResourceManager';
import { User } from '../../User/User';
import { PopupManager } from '../Common/PopupManager';
import { TapLayer } from '../Common/TapLayer';
import { InventoryLevelUp } from '../InventoryLevelUp';
import { ItemFrame } from '../ItemSlot/ItemFrame';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = InventoryLayer
 * DateTime = Mon Jan 10 2022 19:46:46 GMT+0900 (대한민국 표준시)
 * Author = seungmin
 * FileBasename = InventoryLayer.ts
 * FileBasenameNoExtension = InventoryLayer
 * URL = db://assets/Scripts/UI/BuildingManagement/InventoryLayer.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */

const consumeType = mergeBinaryType([1,4,7,8]);
const adv_materialType = mergeBinaryType([9]);
const productType = mergeBinaryType([5,6]);
const ectType = mergeBinaryType([2,3]);

function mergeBinaryType(array : number[])
{
    let binary : number = 0
    array.forEach((element)=>
    {
        binary += Math.pow(2, element)
    })
    return binary
}
 
enum INVENSORT
{
    ALL = consumeType + adv_materialType + productType + ectType,
    CONSUMABLE = consumeType,
    ADV_MATERIAL = adv_materialType,
    PRODUCT = productType,
    ETC = ectType
}

@ccclass('InventoryLayer')
export class InventoryLayer extends TapLayer
{
    @property(Node)
    nodeInvenContent : Node = null

    @property(Label)
    labelSlot : Label = null

    @property(Node)
    btnArray : Node[] = []

    @property(ScrollView)
    scrollInven : ScrollView = null

    itemFrameClone : Prefab = null

    Init()
    {
        this.deActivateBtn(0)
        this.drawInven(INVENSORT.ALL)
    }

    drawInven(sort : INVENSORT = INVENSORT.ALL)
    {
        this.nodeInvenContent.removeAllChildren()
        let curLevel = User.Instance.InvenStep // 유저 창고 레벨 불러오기
        let invenSlot = (TableManager.GetTable(InventoryTable.Name).Get(curLevel) as InventoryData).SLOT
        let invenUsage = 0
        this.itemFrameClone = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB)["item"]

        let items = User.Instance.GetAllItems()

        for(let i = 0; i < items.length; i++)
        {
            let itemInfo : ItemBaseData = TableManager.GetTable(ItemBaseTable.Name).Get(items[i].ItemNo)
            
            invenUsage++
            if(itemInfo.SLOT_USE != "YES" || (Math.pow(2, itemInfo.KIND) & sort) == 0)
            {
                continue;
            }

            let itemRemain = items[i].Count
            let slotCount = 0

            while(itemRemain != 0)
            {
                let clone : Node = instantiate(this.itemFrameClone)
                clone.parent = this.nodeInvenContent

                if(itemRemain > itemInfo.MERGE)
                {
                    slotCount = itemInfo.MERGE
                    itemRemain -= itemInfo.MERGE
                }
                else
                {
                    slotCount = itemRemain
                    itemRemain -= slotCount
                }

                clone.getComponent(ItemFrame).setFrameItemInfo(items[i].ItemNo, slotCount)
                clone.getComponent(ItemFrame).setInventoryFunc()
            }
        }

        if(sort == INVENSORT.ALL)
        {
            for(let i = invenUsage; i < invenSlot; i++)
            {
                let clone : Node = instantiate(this.itemFrameClone)
                clone.parent = this.nodeInvenContent

                clone.getComponent(ItemFrame).setFrameBlank()
            }
        }

        this.labelSlot.string = invenUsage + " / " + invenSlot
    }

    onClickFilter(event, CustomEventData)
    {
        let jsonData = JSON.parse(CustomEventData)
        let sort : INVENSORT;
        switch(jsonData.type)
        {
            case "all": sort = INVENSORT.ALL; this.deActivateBtn(0); break
            case "consume": sort = INVENSORT.CONSUMABLE; this.deActivateBtn(1); break
            case "adv_material": sort = INVENSORT.ADV_MATERIAL; this.deActivateBtn(2); break
            case "product": sort = INVENSORT.PRODUCT; this.deActivateBtn(3); break
            case "etc": sort = INVENSORT.ETC; this.deActivateBtn(4); break
        }
        this.drawInven(sort)
    }

    private deActivateBtn(_index : number = 0)
    {
        this.btnArray.forEach((element : Node, index : number) =>
        {
            if(index == _index)
            {
                element.getComponent(Button).interactable = false
            }
            else
            {
                element.getComponent(Button).interactable = true
            }
        })
    }
    
    onClickLevelUp()
    {
        let popup = PopupManager.OpenPopup("InventoryLevelUp") as InventoryLevelUp
        popup.init()
    }

    ForceUpdate()
    {
        this.drawInven()
        this.deActivateBtn()
    }
}
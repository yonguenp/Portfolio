
import { _decorator, Node, Label, Sprite, instantiate } from 'cc';
import { BuildingBaseData, BuildingLevelData } from '../Data/BuildingData';
import { BuildingBaseTable, BuildingLevelTable } from '../Data/BuildingTable';
import { ItemBaseData } from '../Data/ItemData';
import { ItemBaseTable } from '../Data/ItemTable';
import { ProductData } from '../Data/ProductData';
import { ProductTable } from '../Data/ProductTable';
import { StringTable } from '../Data/StringTable';
import { TableManager } from '../Data/TableManager';
import { GameManager } from '../GameManager';
import { NetworkManager } from '../NetworkManager';
import { ResourceManager, ResourcesType } from '../ResourceManager';
import { ObjectCheck, StringBuilder, TimeString } from '../Tools/SandboxTools';
import { BuildingInstance, User } from '../User/User';
import { PopupManager } from './Common/PopupManager';
import { ItemFrame } from './ItemSlot/ItemFrame';
import { SystemPopup } from './SystemPopup';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = BuildingTypeOne
 * DateTime = Fri Jan 14 2022 13:55:16 GMT+0900 (대한민국 표준시)
 * Author = seungmin
 * FileBasename = BuildingTypeOne.ts
 * FileBasenameNoExtension = BuildingTypeOne
 * URL = db://assets/Scripts/UI/BuildingTypeOne.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
@ccclass('BuildingTypeOne')
export class BuildingTypeOne extends SystemPopup 
{
    /** 건물 이름 */
    @property(Label)
    labelBuildingName : Label = null

    /** before Level */
    @property(Label)
    labelBefLevel : Label = null

    /** after Level */
    @property(Label)
    labelAfLevel : Label = null

    /** 생산품 아이콘 */
    @property(Sprite)
    spriteProduct :Sprite = null

    /** 생산품 라벨 */
    @property(Label)
    labelProduct : Label = null

    /** 재료 부모 노드 */
    @property(Node)
    materialParentNode : Node = null

    /** 소요시간 라벨 */
    @property(Label)
    labelReqTime : Label = null

    /** 재화 아이콘 */
    @property(Sprite)
    spriteCostIcon : Sprite = null

    /** 재화 라벨 */
    @property(Label)
    labelCost : Label = null

    /** 재화 부모 노드 */
    @property(Node)
    costParentNode : Node = null

    @property(Node)
    costMessageNode : Node = null

    private buildingTag : number;

    static PopupBuildingLevelup(buildingIndex : string, targetTag : number) : BuildingTypeOne
    {
        let systemPopup = PopupManager.OpenPopup("BuildingType1", false) as BuildingTypeOne
        systemPopup.setMessage(buildingIndex, targetTag.toString())

        return systemPopup
    }

    /**
     * 
     * @param title building_base 의 index
     * @param body 건물의 고유번호, 최초 건설시 공백
     */
    setMessage(title : string, body : string)
    {
        let levelInfoArray : BuildingLevelData[] = []
        let buildingInfo : BuildingBaseData = TableManager.GetTable<BuildingBaseTable>(BuildingBaseTable.Name).Get(title)
        let buildingInstance : BuildingInstance = User.Instance.GetUserBuildingList().find(value => value.Tag == Number(body))
        let levelInfo : BuildingLevelData
        let newProductInfo : ProductData
        let newProductItemInfo : ItemBaseData

        this.buildingTag = buildingInstance.Tag

        TableManager.GetTable<BuildingLevelTable>(BuildingLevelTable.Name).GetAll().forEach((element : BuildingLevelData)=>
        {
            if(element.BUILDING_GROUP == title)
            {
                levelInfoArray.push(element)
            }
        })

        TableManager.GetTable(ProductTable.Name).Get(buildingInfo.Index).forEach((element)=>
        {
            if(element.BuildingLevel == buildingInstance.Level+1)
            {
                newProductInfo = element;
                return;
            }
        })
        newProductItemInfo = TableManager.GetTable(ItemBaseTable.Name).Get(newProductInfo.ProductItemID)

        levelInfo = levelInfoArray.sort((a, b)=> a.LEVEL - b.LEVEL).find(value => value.LEVEL == buildingInstance.Level)
        
        this.labelBuildingName.string = TableManager.GetTable<StringTable>(StringTable.Name).Get(buildingInfo._NAME).TEXT
        this.labelBefLevel.string = StringBuilder(TableManager.GetTable<StringTable>(StringTable.Name).Get(100000056).TEXT, levelInfo.LEVEL)
        this.labelAfLevel.string = StringBuilder(TableManager.GetTable<StringTable>(StringTable.Name).Get(100000056).TEXT, levelInfo.LEVEL+1)
        this.labelProduct.string =  StringBuilder(TableManager.GetTable<StringTable>(StringTable.Name).Get(100000076).TEXT, TableManager.GetTable<StringTable>(StringTable.Name).Get(newProductItemInfo._NAME).TEXT)
        this.spriteProduct.spriteFrame = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.ITEMICON_SPRITEFRAME)[newProductItemInfo.ICON]
        this.labelReqTime.string = TimeString(levelInfo.UPGRADE_TIME)

        for(let i = 0; i < levelInfo.NEED_ITEM.length; i++)
        {
            let clone : Node = instantiate(GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB)["item"])
            clone.getComponent(ItemFrame).setFrameReceipeInfo(levelInfo.NEED_ITEM[i].ITEM_NO, levelInfo.NEED_ITEM[i].ITEM_COUNT)
            clone.parent = this.materialParentNode
        }

        //업그레이드 시간, 재료 수량 추가

        if(levelInfo.COST_NUM > 0)
        {
            //Cost 타입에 따라 Cost 아이콘 변경
            this.costMessageNode.active = false
            this.costParentNode.active = true
            this.labelCost.string = `${levelInfo.COST_NUM}`
        }
    }

    onClickLevelUp(event, CustomEventData)
    {
        let params = 
        {
            tag : this.buildingTag
        }

        NetworkManager.Send("building/levelup", params, (jsonObj) => 
        {
            if(ObjectCheck(jsonObj, "rs") && jsonObj.rs == 0)
            {
                PopupManager.ForceUpdate()
            }
            else if(ObjectCheck(jsonObj, "rs") && jsonObj.rs == 207)
            {
                let popup = PopupManager.OpenPopup("SystemPopupOK") as SystemPopup;
                popup.setMessage(StringTable.GetString(100000248), StringTable.GetString(100000556));
            }

            this.ClosePopup()
        })
    }
}

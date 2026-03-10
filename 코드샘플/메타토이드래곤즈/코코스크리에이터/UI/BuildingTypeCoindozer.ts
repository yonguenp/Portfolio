
import { _decorator, Node, Label, Sprite, instantiate } from 'cc';
import { BuildingBaseData, BuildingLevelData } from '../Data/BuildingData';
import { BuildingBaseTable, BuildingLevelTable } from '../Data/BuildingTable';
import { ProductAutoData } from '../Data/ProductData';
import { ProductAutoTable } from '../Data/ProductTable';
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
 * Name = BuildingTypeCoindozer
 * DateTime = Fri Jan 14 2022 13:55:16 GMT+0900 (대한민국 표준시)
 * Author = seungmin
 * FileBasename = BuildingTypeCoindozer.ts
 * FileBasenameNoExtension = BuildingTypeCoindozer
 * URL = db://assets/Scripts/UI/BuildingTypeCoindozer.ts
 *
 */

export enum CoinDozerRewardType 
{
    RESOURCE = 0,
    ITEM = 1,
    ITEM_GROUP = 2
}

export class CoinDozerAutoProductInfo
{
    private level : number = 0
    private data : ProductAutoData[] = null

    public set Level(value : number)
    {
        this.level = value
    }

    public get Level()
    {
        return this.level
    }

    public get Resource()
    {
        return this.data[CoinDozerRewardType.RESOURCE]
    }

    public get Item()
    {
        return this.data[CoinDozerRewardType.ITEM]
    }
    
    public get ItemGroup()
    {
        return this.data[CoinDozerRewardType.ITEM_GROUP]
    }

    AddData(data : ProductAutoData)
    {
        if(this.data == null)
            this.data = new Array(3)

        switch(data.TYPE)
        {
            case "GOLD":
                this.data[CoinDozerRewardType.RESOURCE] = data
            break;

            case "ITEM":
                this.data[CoinDozerRewardType.ITEM] = data
            break;

            case "ITEM_GROUP":
                this.data[CoinDozerRewardType.ITEM_GROUP] = data
            break;
        }
    }
}

@ccclass('BuildingTypeCoindozer')
export class BuildingTypeCoindozer extends SystemPopup 
{
    /** before Level */
    @property(Label)
    labelBefLevel : Label = null

    /** after Level */
    @property(Label)
    labelAfLevel : Label = null

    /** before Product Req Time */
    @property(Label)
    labelBefReqProductTime : Label = null

    /** after Product Req Time */
    @property(Label)
    labelAfReqProductTime : Label = null

    /** before Product */
    @property(Label)
    labelBefBatProduct : Label = null

    /** after Product */
    @property(Label)
    labelAfBatProduct : Label = null

    /** before Product */
    @property(Label)
    labelBefGoldProduct : Label = null

    /** after Product */
    @property(Label)
    labelAfGoldProduct : Label = null

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


    @property(Label)
    labelBatPropertyTitle : Label = null

    @property(Label)
    labelGoldPropertyTitle : Label = null

    private buildingTag : number

    static PopupBuildingLevelup(buildingIndex : string, targetLevel : number) : BuildingTypeCoindozer
    {
        let systemPopup = PopupManager.OpenPopup("BuildingType2", false) as BuildingTypeCoindozer
        systemPopup.setMessage(buildingIndex, targetLevel.toString())

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
        this.buildingTag = buildingInstance.Tag
        let curBuildingInfo : CoinDozerAutoProductInfo = new CoinDozerAutoProductInfo()
        let newBuildingInfo : CoinDozerAutoProductInfo = new CoinDozerAutoProductInfo()

        TableManager.GetTable<BuildingLevelTable>(BuildingLevelTable.Name).GetAll().forEach((element : BuildingLevelData)=>
        {
            if(element.BUILDING_GROUP == title)
            {
                levelInfoArray.push(element)
            }
        })

        curBuildingInfo.Level = buildingInstance.Level
        newBuildingInfo.Level = buildingInstance.Level + 1

        TableManager.GetTable<ProductAutoTable>(ProductAutoTable.Name).Get(buildingInfo.Index).forEach((element)=>
        {
            if(element.LEVEL == buildingInstance.Level+1)
            {
                newBuildingInfo.AddData(element)
            }

            if(element.LEVEL == buildingInstance.Level)
            {
                curBuildingInfo.AddData(element)
            }
        })

        levelInfo = levelInfoArray.sort((a, b)=> a.LEVEL - b.LEVEL).find(value => value.LEVEL == buildingInstance.Level)
        
        this.labelBefLevel.string = StringBuilder(TableManager.GetTable<StringTable>(StringTable.Name).Get(100000056).TEXT, curBuildingInfo.Level)
        this.labelAfLevel.string = StringBuilder(TableManager.GetTable<StringTable>(StringTable.Name).Get(100000056).TEXT, newBuildingInfo.Level)

        this.labelBefBatProduct.string = StringBuilder(TableManager.GetTable<StringTable>(StringTable.Name).Get(100000241).TEXT, (curBuildingInfo.Item.MAX_TIME / curBuildingInfo.Item.TERM) * curBuildingInfo.Item.NUM)
        this.labelAfBatProduct.string = StringBuilder(TableManager.GetTable<StringTable>(StringTable.Name).Get(100000241).TEXT, (newBuildingInfo.Item.MAX_TIME / newBuildingInfo.Item.TERM) * newBuildingInfo.Item.NUM)
        this.labelBefGoldProduct.string = StringBuilder(TableManager.GetTable<StringTable>(StringTable.Name).Get(100000241).TEXT, (curBuildingInfo.Resource.MAX_TIME / curBuildingInfo.Resource.TERM) * curBuildingInfo.Resource.NUM) // 100000254
        this.labelAfGoldProduct.string = StringBuilder(TableManager.GetTable<StringTable>(StringTable.Name).Get(100000241).TEXT, (newBuildingInfo.Resource.MAX_TIME / newBuildingInfo.Resource.TERM) * newBuildingInfo.Resource.NUM) // 100000254

        this.labelBefReqProductTime.string = TimeString(curBuildingInfo.Item.MAX_TIME)
        this.labelAfReqProductTime.string = TimeString(newBuildingInfo.Item.MAX_TIME)

        this.labelBatPropertyTitle.string = StringBuilder(TableManager.GetTable<StringTable>(StringTable.Name).Get(100000109).TEXT, curBuildingInfo.Item.TERM / 60)
        this.labelGoldPropertyTitle.string = StringBuilder(TableManager.GetTable<StringTable>(StringTable.Name).Get(100000110).TEXT, curBuildingInfo.Resource.TERM / 60)

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
                popup.setMessage(StringTable.GetString(100000248), StringTable.GetString(100000555));
            }

            this.ClosePopup()
        })
    }
}

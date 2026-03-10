
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
import { TutorialManager } from '../Tutorial/TutorialManager';
import { BuildingInstance, User } from '../User/User';
import { PopupManager } from './Common/PopupManager';
import { ItemFrame } from './ItemSlot/ItemFrame';
import { SystemPopup } from './SystemPopup';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = BuildingTypeTwo
 * DateTime = Fri Jan 14 2022 13:55:16 GMT+0900 (대한민국 표준시)
 * Author = seungmin
 * FileBasename = BuildingTypeTwo.ts
 * FileBasenameNoExtension = BuildingTypeTwo
 * URL = db://assets/Scripts/UI/BuildingTypeTwo.ts
 *
 */
 
@ccclass('BuildingTypeTwo')
export class BuildingTypeTwo extends SystemPopup 
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

    /** before Product */
    @property(Label)
    labelBefProduct : Label = null

    /** after Product */
    @property(Label)
    labelAfProduct : Label = null

    /** before Product Req Time */
    @property(Label)
    labelBefReqProductTime : Label = null

    /** after Product Req Time */
    @property(Label)
    labelAfReqProductTime : Label = null

    /** 재화 부모 노드 */
    @property(Node)
    costParentNode : Node = null

    @property(Node)
    costMessageNode : Node = null

    private buildingTag : number

    static PopupBuildingLevelup(buildingIndex : string, targetLevel : number) : BuildingTypeTwo
    {
        let systemPopup = PopupManager.OpenPopup("BuildingType2", false) as BuildingTypeTwo
        systemPopup.setMessage(buildingIndex, targetLevel.toString())

        // 튜토리얼 실행
        if (TutorialManager.GetInstance.IsNowPlayingTutorialByGroup(113)) {
            TutorialManager.GetInstance.OnTutorialEvent(113, 6);
        }
        
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
        let curBuildingInfo : ProductAutoData
        let newBuildingInfo : ProductAutoData
        let bfLevel : number, afLevel : number

        TableManager.GetTable<BuildingLevelTable>(BuildingLevelTable.Name).GetAll().forEach((element : BuildingLevelData)=>
        {
            if(element.BUILDING_GROUP == title)
            {
                levelInfoArray.push(element)
            }
        })

        TableManager.GetTable<ProductAutoTable>(ProductAutoTable.Name).Get(buildingInfo.Index).forEach((element)=>
        {
            if(element.LEVEL == buildingInstance.Level+1)
            {
                newBuildingInfo = element;
            }

            if(element.LEVEL == buildingInstance.Level)
            {
                curBuildingInfo = element;
            }
        })

        levelInfo = levelInfoArray.sort((a, b)=> a.LEVEL - b.LEVEL).find(value => value.LEVEL == buildingInstance.Level)
        
        this.labelBuildingName.string = TableManager.GetTable<StringTable>(StringTable.Name).Get(buildingInfo._NAME).KOR
        this.labelBefLevel.string = StringBuilder(TableManager.GetTable<StringTable>(StringTable.Name).Get(100000056).TEXT, levelInfo.LEVEL)
        this.labelAfLevel.string = StringBuilder(TableManager.GetTable<StringTable>(StringTable.Name).Get(100000056).TEXT, levelInfo.LEVEL+1)

        this.labelBefProduct.string = StringBuilder(TableManager.GetTable<StringTable>(StringTable.Name).Get(100000241).TEXT, (curBuildingInfo.MAX_TIME / curBuildingInfo.TERM) * curBuildingInfo.NUM)
        this.labelAfProduct.string = StringBuilder(TableManager.GetTable<StringTable>(StringTable.Name).Get(100000241).TEXT, (newBuildingInfo.MAX_TIME / newBuildingInfo.TERM) * newBuildingInfo.NUM)
        this.labelBefReqProductTime.string = TimeString(curBuildingInfo.MAX_TIME)
        this.labelAfReqProductTime.string = TimeString(newBuildingInfo.MAX_TIME)

        this.labelReqTime.string = TimeString(levelInfo.UPGRADE_TIME)

        for(let i = 0; i < levelInfo.NEED_ITEM.length; i++)
        {
            let clone : Node = instantiate(GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB)["item"])
            clone.getComponent(ItemFrame).setFrameReceipeInfo(levelInfo.NEED_ITEM[i].ITEM_NO, levelInfo.NEED_ITEM[i].ITEM_COUNT)
            clone.parent = this.materialParentNode
        }

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
                popup.setMessage(StringTable.GetString(100000248), StringTable.GetString(100000555));
            }

            this.ClosePopup()
        })
    }
}


import { _decorator, Node, Label, HorizontalTextAlignment, instantiate } from 'cc';
import { BuildingLevelTable, BuildingOpenTable } from '../../Data/BuildingTable';
import { ProductAutoTable } from '../../Data/ProductTable';
import { StringTable } from '../../Data/StringTable';
import { TableManager } from '../../Data/TableManager';
import { GameManager } from '../../GameManager';
import { NetworkManager } from '../../NetworkManager';
import { ResourceManager, ResourcesType } from '../../ResourceManager';
import { TimeObject } from '../../Time/ITimeObject';
import { TimeManager } from '../../Time/TimeManager';
import { GetChild, StringBuilder } from '../../Tools/SandboxTools';
import { BuildingInstance, eBuildingState, eGoodType, eLandmarkType, LandmarkCoindozer, User } from '../../User/User';
import { eAccelerationType } from '../AccelerationMainPopup';
import { BuildingTypeCoindozer, CoinDozerAutoProductInfo } from '../BuildingTypeCoindozer';
import { BuildingTypeCommon } from '../BuildingTypeCommon';
import { PopupManager } from '../Common/PopupManager';
import { TapLayer } from '../Common/TapLayer';
import { ItemFrame } from '../ItemSlot/ItemFrame';
import { SystemRewardPopup } from '../SystemRewardPopup';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = CoindozerLayer
 * DateTime = Mon Feb 07 2022 14:47:23 GMT+0900 (대한민국 표준시)
 * Author = seungmin
 * FileBasename = CoindozerLayer.ts
 * FileBasenameNoExtension = CoindozerLayer
 * URL = db://assets/Scripts/UI/Landmark/CoindozerLayer.ts
 *
 */
 
@ccclass('CoindozerLayer')
export class CoindozerLayer extends TapLayer
{
    @property(Node)
    nodeConstruct : Node = null

    @property(Node)
    nodeConstructFinish : Node = null

    @property(Node)
    nodeLevelUp : Node = null

    @property(Node)
    nodeContent : Node = null

    @property(Label)
    labelConstructTimer : Label = null

    @property(Label)
    labelLayerTitle : Label = null

    @property(Label)
    labelRemainTimer : Label = null

    @property(Label)
    labelRewardGold : Label = null

    @property(Label)
    labelRewardBat : Label = null

    @property(Label)
    labelRewardItem : Label = null

    @property(Node)
    levelupButtonNode : Node = null;

    private timeObj : TimeObject = null
    private buildingInfo : BuildingInstance = null
    private labelConstLabel : Label = null

    Init()
    {
        if(this.buildingInfo == null)
            this.buildingInfo = User.Instance.GetUserBuildingByTag(101)

        this.nodeContent?.removeAllChildren()
        
        this.nodeConstruct.active = false
        this.nodeLevelUp.active = false
        
        this.labelLayerTitle.string = StringBuilder(TableManager.GetTable<StringTable>(StringTable.Name).Get(100000057).TEXT, this.buildingInfo.Level, TableManager.GetTable<StringTable>(StringTable.Name).Get(100000024).TEXT)
        if(this.buildingInfo.Level == 0)
            this.labelLayerTitle.string = TableManager.GetTable<StringTable>(StringTable.Name).Get(100000024).TEXT

        this.labelRemainTimer.string = "-"
        this.labelRewardGold.string = "-"
        this.labelRewardBat.string = "-"
        this.labelRewardItem.string = "-"
        this.labelRemainTimer.horizontalAlign = HorizontalTextAlignment.CENTER
        this.labelRewardGold.horizontalAlign = HorizontalTextAlignment.CENTER
        this.labelRewardBat.horizontalAlign = HorizontalTextAlignment.CENTER
        this.labelRewardItem.horizontalAlign = HorizontalTextAlignment.CENTER
        this.labelConstLabel = GetChild(this.node, ["construct", "blockLabel"]).getComponent(Label)

        if(this.buildingInfo.State == eBuildingState.LOCKED)
        {
            let buildingBaseInfo = TableManager.GetTable<BuildingLevelTable>(BuildingLevelTable.Name).GetAll().find(value => value.BUILDING_GROUP == "dozer")
            let constStr : string = StringBuilder(TableManager.GetTable<StringTable>(StringTable.Name).Get(100000059).TEXT, buildingBaseInfo.NEED_AREA_LEVEL)
            this.labelConstLabel.string = constStr
            this.nodeConstruct.active = true;
            this.labelConstLabel.node.active = true;
            GetChild(this.node, ["construct", "btnConstruct"]).active = false;

            if(this.levelupButtonNode != null){
                this.levelupButtonNode.active = false;
            }
        }
        else if(this.buildingInfo.State == eBuildingState.NOT_BUILT)
        {
            this.labelConstLabel.node.active = false;
            this.nodeConstruct.active = true
            GetChild(this.node, ["construct", "btnConstruct"]).active = true;

            if(this.levelupButtonNode != null){
                this.levelupButtonNode.active = false;
            }
        }
        else if(this.buildingInfo.State == eBuildingState.CONSTRUCTING || this.buildingInfo.State == eBuildingState.CONSTRUCT_FINISHED)
        {
            let buildingInfo = User.Instance.GetUserBuildingByTag(101)
            
            if(TimeManager.GetTimeCompare(buildingInfo.ActiveTime) > 0 && this.buildingInfo.State != eBuildingState.CONSTRUCT_FINISHED)
            {
                this.timeObj = this.getComponent(TimeObject);
                this.timeObj.curTime = TimeManager.GetTime();
                this.timeObj.Refresh = () => 
                {
                    this.labelConstructTimer.string = TimeManager.GetTimeCompareString(buildingInfo.ActiveTime)
                    if(TimeManager.GetTimeCompare(buildingInfo.ActiveTime) <= 0)
                    {
                        //완료
                        if(this.timeObj != null) {
                            this.timeObj.Refresh = null;
                        }
                        this.nodeLevelUp.active = false;
                        this.nodeConstructFinish.active = true;
                    }
                }
                this.timeObj.Refresh();
                this.nodeLevelUp.active = true;
            }
            else
            {
                //완료 노드 
                this.nodeLevelUp.active = false
                this.nodeConstructFinish.active = true
            }

            if(this.levelupButtonNode != null){
                this.levelupButtonNode.active = false;
            }
        }
        else
        {
            let dozerInfo : CoinDozerAutoProductInfo = new CoinDozerAutoProductInfo()

            TableManager.GetTable<ProductAutoTable>(ProductAutoTable.Name).Get("dozer").forEach((element)=>
            {
                if(element.LEVEL == this.buildingInfo.Level)
                {
                    dozerInfo.AddData(element)
                }
            })

            if(TableManager.GetTable<BuildingLevelTable>(BuildingLevelTable.Name).GetAll().find(value => value.LEVEL == this.buildingInfo.Level).UPGRADE_TIME == 0)
            {
                //최대 레벨
                if(this.levelupButtonNode != null){
                    this.levelupButtonNode.active = false;
                }
            }else{
                if(this.levelupButtonNode != null){
                    this.levelupButtonNode.active = true;
                }
            }

            this.labelRewardGold.string = StringBuilder(TableManager.GetTable<StringTable>(StringTable.Name).Get(100000109).TEXT, dozerInfo.Resource.TERM / 60, dozerInfo.Resource.NUM)
            this.labelRewardBat.string = StringBuilder(TableManager.GetTable<StringTable>(StringTable.Name).Get(100000109).TEXT, dozerInfo.Item.TERM / 60, dozerInfo.Item.NUM)
            this.labelRewardItem.string = StringBuilder(TableManager.GetTable<StringTable>(StringTable.Name).Get(100000109).TEXT, dozerInfo.ItemGroup.TERM / 60, dozerInfo.ItemGroup.NUM)

            this.timeObj = this.getComponent(TimeObject)
            this.timeObj.curTime = TimeManager.GetTime()
            let dozer : LandmarkCoindozer = User.Instance.GetLandmarkData(eLandmarkType.COINDOZER) as LandmarkCoindozer
            
            if(TimeManager.GetTime() < dozer.ExpireTime)
            {
                if(this.timeObj.Refresh == undefined) 
                {
                    this.timeObj.Refresh = () => 
                    {
                        if(dozer.Recall())
                        {
                            let params = {}
                            NetworkManager.Send("dozer/state", params, ()=>
                            {
                                PopupManager.ForceUpdate();
                            })
                        }
    
                        this.labelRemainTimer.string = TimeManager.GetTimeCompareString(dozer.ExpireTime)
    
                        if(TimeManager.GetTimeCompare(dozer.ExpireTime) <= 0)
                        {
                            this.timeObj.Refresh = null
                            this.ForceUpdate()
                        }
                    }
                    this.timeObj.Refresh()
                }
                else
                    this.labelRemainTimer.string = TimeManager.GetTimeCompareString(dozer.ExpireTime)
            }
            else
            {
                this.labelRemainTimer.string = TimeManager.GetTimeCompareString(0)
            }

            if(dozer.Reward?.length > 0)
            {
                dozer.Reward.forEach((element) => 
                {
                    let itemClone : Node = instantiate(GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB)["item"])
                    itemClone.parent = this.nodeContent

                    switch(element[0])
                    {
                        case 1: // 캐시
                        itemClone.getComponent(ItemFrame).setFrameCashInfo(element[1], element[2])
                        break;

                        case 2: // 에너지
                        itemClone.getComponent(ItemFrame).setFrameEnergyInfo(element[2])
                        break;

                        case 3: // 아이템
                        itemClone.getComponent(ItemFrame).setFrameItemInfo(element[1], element[2])
                        break;
                    }
                })
            }
        }
    }

    ForceUpdate()
    {
        this.Init()
    }

    onDisable()
    {
        if(this.timeObj != null) {
            this.timeObj.Refresh = null;
        }
    }

    onClickGetReward()
    {
        let params = {}
        NetworkManager.Send("dozer/harvest", params, (jsonData) => 
        {
            this.nodeContent.removeAllChildren()
            let popup = PopupManager.OpenPopup("SystemRewardPopup") as SystemRewardPopup
            popup.initWithList(jsonData.rewards)
            PopupManager.ForceUpdate()
        })
    }

    onClickUpgrade()
    {
        let popup = PopupManager.OpenPopup("BuildingType3") as BuildingTypeCoindozer
        popup.setMessage("dozer", String(eLandmarkType.COINDOZER))
    }

    onClickLevelUpAccel()
    {
        let buildingInstance = User.Instance.GetUserBuildingByTag(eLandmarkType.COINDOZER)
        let buildingInfo = TableManager.GetTable<BuildingOpenTable>(BuildingOpenTable.Name).GetAll(5).find(value => value.INSTALL_TAG == eLandmarkType.COINDOZER)
        let buildingLevelInfo = TableManager.GetTable<BuildingLevelTable>(BuildingLevelTable.Name).GetAll().find(value => value.BUILDING_GROUP == buildingInfo.BUILDING && value.LEVEL == (buildingInstance != null ? buildingInstance.Level : 0))

        PopupManager.OpenPopup("AccelerationMainPopup", false, {
            title:"가속 사용", 
            body:"외형 레벨 업 중 ...", 
            type: eAccelerationType.LEVELUP, 
            tag:eLandmarkType.COINDOZER,
            time:buildingLevelInfo.UPGRADE_TIME,
            time_end:buildingInstance.ActiveTime,
            prices:[
                {type:eGoodType.CASH, type_value:0, count:100},
                {type:eGoodType.ITEM, type_value:70000001, count:1}, 
                {type:eGoodType.ITEM, type_value:70000002, count:1}, 
                {type:eGoodType.ITEM, type_value:70000003, count:1}, 
                {type:eGoodType.ITEM, type_value:70000004, count:1}, 
                {type:eGoodType.ITEM, type_value:70000005, count:1}, 
                {type:eGoodType.ITEM, type_value:70000006, count:1}, 
                {type:eGoodType.ITEM, type_value:70000007, count:1}, 
                {type:eGoodType.ITEM, type_value:70000008, count:1}, 
                {type:eGoodType.ITEM, type_value:70000009, count:1}
            ]
        });
    }

    onClickConstruct()
    {
        let popup = PopupManager.OpenPopup("BuildingTypeCommon") as BuildingTypeCommon
        popup.setMessage("dozer")
    }

    onClickConstructFinish()
    {
        let params = 
        {
            tag : 101
        }

        NetworkManager.Send("building/complete", params, () =>
        {
            this.nodeConstructFinish.active = false
            if(this.timeObj != null) {
                this.timeObj.Refresh = null;
            }
            PopupManager.ForceUpdate()
        })
    }
}
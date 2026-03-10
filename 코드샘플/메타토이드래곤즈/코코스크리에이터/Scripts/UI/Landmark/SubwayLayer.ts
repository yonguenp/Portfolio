
import { _decorator, Node, Label, Button, instantiate, EventHandler } from 'cc';
import { BuildingLevelTable } from '../../Data/BuildingTable';
import { StringTable } from '../../Data/StringTable';
import { SubwayDeliveryTable, SubwayPlatformTable } from '../../Data/SubwayTable';
import { TableManager } from '../../Data/TableManager';
import { GameManager } from '../../GameManager';
import { NetworkManager } from '../../NetworkManager';
import { ResourceManager, ResourcesType } from '../../ResourceManager';
import { TimeObject } from '../../Time/ITimeObject';
import { TimeManager } from '../../Time/TimeManager';
import { StringBuilder } from '../../Tools/SandboxTools';
import { eBuildingState, eGoodType, eLandmarkType, LandmarkSubway, LandmarkSubwayPlantState, User } from '../../User/User';
import { eAccelerationType } from '../AccelerationMainPopup';
import { BuildingTypeCommon } from '../BuildingTypeCommon';
import { PopupManager } from '../Common/PopupManager';
import { TapLayer } from '../Common/TapLayer';
import { ItemFrame } from '../ItemSlot/ItemFrame';
import { TrainItemFrame } from '../ItemSlot/TrainItemFrame';
import { SubwaySlotAdd } from '../SubwaySlotAdd';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = SubwayLayer
 * DateTime = Tue Feb 08 2022 11:22:15 GMT+0900 (대한민국 표준시)
 * Author = seungmin
 * FileBasename = SubwayLayer.ts
 * FileBasenameNoExtension = SubwayLayer
 * URL = db://assets/Scripts/UI/Landmark/SubwayLayer.ts
 *
 */
 
@ccclass('SubwayLayer')
export class SubwayLayer extends TapLayer 
{
    @property(Node)
    nodeBlock : Node = null

    @property(Node)
    nodeConstruct : Node = null

    @property(Node)
    nodeNormal : Node = null

    @property(Label)
    labelConstructTitle : Label = null

    @property(Label)
    labelConstructBtnTitle : Label = null

    @property(Label)
    labelNormalTitle : Label = null

    @property(Node)
    nodeTrainSlot : Node[] = []

    private arrTrainSlot : TrainSlot[] = []

    Init()
    {
        this.initChildeNode()

        if(this.arrTrainSlot?.length > 0)
            this.arrTrainSlot.forEach(element => element.remove())
        this.arrTrainSlot = []

        let buildingBaseInfo = TableManager.GetTable<BuildingLevelTable>(BuildingLevelTable.Name).GetAll().find(value => value.BUILDING_GROUP == "subway")
        let builindInfo = User.Instance.GetUserBuildingByTag(eLandmarkType.SUBWAY)

        if(builindInfo.State != eBuildingState.NORMAL)
        {
            this.nodeConstruct.active = true
            this.nodeNormal.active = false

            if(builindInfo.State == eBuildingState.LOCKED)
            {
                this.labelConstructTitle.string = StringBuilder(TableManager.GetTable<StringTable>(StringTable.Name).Get(100000059).TEXT, buildingBaseInfo.NEED_AREA_LEVEL)
                this.nodeBlock.active = true
                return
            }
            else if(builindInfo.State == eBuildingState.CONSTRUCT_FINISHED || builindInfo.State == eBuildingState.CONSTRUCTING && builindInfo.ActiveTime <= TimeManager.GetTime())
            {
                let params = 
                {
                    tag : eLandmarkType.SUBWAY
                }
                NetworkManager.Send("building/complete", params, ()=>
                {
                    PopupManager.ForceUpdate()
                })
                return
            }
            else
            {
                //건설 가능
                this.labelConstructTitle.node.active = false
                this.nodeBlock.active = false
            }
        }
        else
        {
            this.nodeConstruct.active = false
            this.nodeNormal.active = true
            this.labelNormalTitle.string = TableManager.GetTable<StringTable>(StringTable.Name).Get(100000025).TEXT

            let userSlotData = (User.Instance.GetLandmarkData(eLandmarkType.SUBWAY) as LandmarkSubway).PlatsData

            for(let i = 0; i < this.nodeTrainSlot.length; i++)
            {
                this.arrTrainSlot.push(new TrainSlot())

                if(userSlotData[i].state >= LandmarkSubwayPlantState.READY)
                {
                    if(userSlotData[i].state == LandmarkSubwayPlantState.READY)
                    {
                        this.nodeTrainSlot[i].getChildByName("normal").getChildByName("itemBG").getChildByName("btnSet").active = true

                        let parent :Node = this.nodeTrainSlot[i].getChildByName("normal").getChildByName("itemBG").getChildByName("itemList")
                        parent.removeAllChildren()

                        userSlotData[i].slots.forEach((element, index) => 
                        {
                            let tItemClone : Node = instantiate(GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB)["trainItem"])
                            tItemClone.parent = parent

                            tItemClone.getComponent(TrainItemFrame).SetFrame(i, index)
                        })
                    }
                    else
                    {
                        this.nodeTrainSlot[i].getChildByName("normal").getChildByName("itemBG").getChildByName("btnSet").active = false
                        
                        if(userSlotData[i].state == LandmarkSubwayPlantState.DELIVERING)
                        {
                            //가속이벤트 심어줌
                            this.nodeTrainSlot[i].getChildByName("normal").getChildByName("itemBG").getChildByName("itemList").active = false
                            this.nodeTrainSlot[i].getChildByName("normal").getChildByName("itemBG").getChildByName("timer").active = true

                            let labelTime : Label = this.nodeTrainSlot[i].getChildByName("normal").getChildByName("itemBG").getChildByName("timer").getChildByName("sprTimer").getComponentInChildren(Label)
                            let timer : TimeObject = this.nodeTrainSlot[i].getChildByName("normal").getChildByName("itemBG").getChildByName("timer").getComponent(TimeObject)
                            timer.curTime = TimeManager.GetTime()
                            timer.Refresh = () => 
                            {
                                labelTime.string = TimeManager.GetTimeCompareString(userSlotData[i].expire)
                                if(TimeManager.GetTimeCompare(userSlotData[i].expire) <= 0)
                                {
                                    timer.Refresh = null
                                }
                            }
                            timer.Refresh()

                            let customData = {
                                platform : userSlotData[i].id,
                                time : 0,
                                time_end : userSlotData[i].expire,   
                            }

                            let delivery = TableManager.GetTable<SubwayDeliveryTable>(SubwayDeliveryTable.Name).GetAll()

                            console.log(delivery)

                            for(let j = 0; j < userSlotData[i].slots.length; j++)
                            {
                                for(let u = 0; u < delivery.length; u++)
                                {
                                    if(userSlotData[i].slots[j][0] == delivery[u].NEED_ITEM)
                                    {
                                        customData.time += delivery[u].DELIVERY_TIME
                                        break;
                                    }
                                }
                            }
                            console.log(customData.time)

                            let eHandler = new EventHandler()
                            eHandler.target = this.node
                            eHandler.component = "SubwayLayer"
                            eHandler.handler = "onClickAccelerate"
                            eHandler.customEventData = JSON.stringify(customData)

                            this.nodeTrainSlot[i].getChildByName("normal").getChildByName("itemBG").getChildByName("timer").getChildByName("btnAccel").getComponent(Button).clickEvents = []
                            this.nodeTrainSlot[i].getChildByName("normal").getChildByName("itemBG").getChildByName("timer").getChildByName("btnAccel").getComponent(Button).clickEvents.push(eHandler)
                        }
                        else
                        {
                            //완료버튼 켜줌
                            this.nodeTrainSlot[i].getChildByName("normal").getChildByName("itemBG").getChildByName("btnGet").active = true
                            let parent :Node = this.nodeTrainSlot[i].getChildByName("normal").getChildByName("itemBG").getChildByName("itemList")
                            parent.removeAllChildren()

                            userSlotData[i].rewards.forEach((element, index) => 
                            {
                                let tItemClone : Node = instantiate(GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB)["item"])
                                tItemClone.parent = parent

                                switch(element[0])
                                {
                                    case 1:
                                        tItemClone.getComponent(ItemFrame).setFrameCashInfo(element[1], element[2])
                                    break;

                                    case 2:
                                        tItemClone.getComponent(ItemFrame).setFrameEnergyInfo(element[2])
                                    break;

                                    case 3:
                                        tItemClone.getComponent(ItemFrame).setFrameItemInfo(element[1], element[2])
                                    break;
                                }
                            })
                        }
                    }
                }
                else
                {
                    this.nodeTrainSlot[i].getChildByName("normal").active = false
                    this.nodeTrainSlot[i].getChildByName("block").active = true
                    let platformInfo = TableManager.GetTable<SubwayPlatformTable>(SubwayPlatformTable.Name).GetAll().find(value => value.PLATFORM == userSlotData[i].id)

                    if(userSlotData[i].state == LandmarkSubwayPlantState.CAN_UNLOCK)
                    {
                        this.nodeTrainSlot[i].getChildByName("block").getChildByName("sale").active = true
                        this.nodeTrainSlot[i].getChildByName("block").getChildByName("sale").getChildByName("btnUnlock").getComponentInChildren(Label).string = String(platformInfo.COST_NUM)
                    }
                    else
                    {
                        this.nodeTrainSlot[i].getChildByName("block").getChildByName("lock").active = true
                        this.nodeTrainSlot[i].getChildByName("block").getChildByName("lock").getChildByName("labelLockTitle").getComponent(Label).string = StringBuilder(TableManager.GetTable<StringTable>(StringTable.Name).Get(100000059).TEXT, platformInfo.OPEN_LEVEL)
                    }
                }
            }
        }
    }

    private initChildeNode()
    {
        for(let i = 0; i < 3; i++)
        {
            this.nodeTrainSlot[i].getChildByName("block").getChildByName("sale").active = false
            this.nodeTrainSlot[i].getChildByName("block").getChildByName("lock").active = false
            this.nodeTrainSlot[i].getChildByName("normal").active = true
            this.nodeTrainSlot[i].getChildByName("block").active = false
            this.nodeTrainSlot[i].getChildByName("normal").getChildByName("itemBG").getChildByName("btnGet").active = false
            this.nodeTrainSlot[i].getChildByName("normal").getChildByName("itemBG").getChildByName("itemList").active = true
            this.nodeTrainSlot[i].getChildByName("normal").getChildByName("itemBG").getChildByName("timer").active = false
            this.nodeTrainSlot[i].getChildByName("normal").getChildByName("itemBG").getChildByName("btnSet").active = true
        }
    }

    onClickConstruct()
    {
        let popup = PopupManager.OpenPopup("BuildingTypeCommon") as BuildingTypeCommon
        popup.setMessage("subway")
    }

    onClickUnlockSlot(event, CustomEventData)
    {
        //슬릇 index 받아오기
        let jsonData = JSON.parse(CustomEventData)
        let platformInfo = TableManager.GetTable<SubwayPlatformTable>(SubwayPlatformTable.Name).GetAll().find(value => value.PLATFORM == jsonData.index)
        let popup = PopupManager.OpenPopup("SubwaySlotAdd") as SubwaySlotAdd

        let params =
        {
            cost : platformInfo.COST_NUM,
            plat : jsonData.index
        }
        popup.Init(params)
    }

    ForceUpdate()
    {
        this.Init()
    }

    onClickSetAll(event, CustomEventData)
    {
        let jsonData = JSON.parse(CustomEventData)
        let params = 
        {
            plat : jsonData.index,
            all : 1
        }

        NetworkManager.Send("subway/input", params, ()=>
        {
            PopupManager.ForceUpdate()
        })
    }

    onClickGetReward(event, CustomEventData)
    {
        let jsonData = JSON.parse(CustomEventData)
        let params = 
        {
            plat : jsonData.index
        }

        NetworkManager.Send("subway/finish", params, ()=>
        {
            PopupManager.ForceUpdate()
        })
    }

    onClickAccelerate(event, CustomEventData)
    {
        let jsonData = JSON.parse(CustomEventData)

        PopupManager.OpenPopup("AccelerationMainPopup", false, {
            title:"가속 사용", 
            body:"외형 레벨 업 중 ...", 
            type: eAccelerationType.JOB, 
            tag:eLandmarkType.SUBWAY,
            time:jsonData.time,
            time_end:jsonData.time_end,
            platform:jsonData.platform,
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
}

class TrainSlot
{
    arrItemSlot : TrainItemFrame[] = []
    
    remove()
    {
        this.arrItemSlot.forEach(element => {
            element.node.removeFromParent()
        });
    }
}
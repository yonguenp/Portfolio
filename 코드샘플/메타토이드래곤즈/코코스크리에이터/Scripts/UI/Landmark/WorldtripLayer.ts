
import { _decorator, Node, Label, Button, EventHandler, ScrollView, instantiate, Layers, sys, UITransform, Size } from 'cc';
import { WorldTripTable } from '../../Data/AreaTable';
import { BuildingOpenData } from '../../Data/BuildingData';
import { BuildingOpenTable } from '../../Data/BuildingTable';
import { ItemGroupTable } from '../../Data/ItemTable';
import { StringTable } from '../../Data/StringTable';
import { TableManager } from '../../Data/TableManager';
import { GameManager } from '../../GameManager';
import { NetworkManager } from '../../NetworkManager';
import { ResourceManager, ResourcesType } from '../../ResourceManager';
import { TimeObject } from '../../Time/ITimeObject';
import { TimeManager } from '../../Time/TimeManager';
import { ChangeLayer, ObjectCheck, StringBuilder, TimeString } from '../../Tools/SandboxTools';
import { BuildingInstance, eBuildingState, eGoodType, eLandmarkType, eWorldTripState, LandmarkWorldtrip, User, UserDragon } from '../../User/User';
import { eAccelerationType } from '../AccelerationMainPopup';
import { BuildingTypeCommon } from '../BuildingTypeCommon';
import { PopupManager } from '../Common/PopupManager';
import { TapLayer } from '../Common/TapLayer';
import { ItemFrame } from '../ItemSlot/ItemFrame';
import { SystemRewardPopup } from '../SystemRewardPopup';
import { ToastMessage } from '../ToastMessage';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = WorldtripLayer
 * DateTime = Wed Feb 09 2022 15:16:24 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = WorldtripLayer.ts
 * FileBasenameNoExtension = WorldtripLayer
 * URL = db://assets/Scripts/UI/Landmark/WorldtripLayer.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
@ccclass('WorldtripLayer')
export class WorldtripLayer extends TapLayer
{
    private nodeConstruct: Node = null;
    private nodeBlock: Node = null;
    private labelContructBtn: Label = null;
    private btnConstruct: Button = null;
    private labelConstructTitle: Label = null;

    private nodeLevelUp : Node = null;
    private labelLevelUpTitle : Label = null;
    private labelConstructTimer : Label = null;
    private hasteBtn: Button = null;

    private nodeConstructFinish : Node = null;
    private btnFinish: Button = null;
    private labelBtnFinish: Label = null;

    private nodeNormal: Node = null;
    private labelLayerTitle : Label = null;
    private labelNormalTimer : Label = null;

    private timeObj : TimeObject = null;
    private buildingInfo : BuildingInstance = null;
    private buildingOpenData: BuildingOpenData = null;
    private characterSlots: Object[] = null;;
    private fillSlotLabel: Label = null;
    private btnHaste: Button = null;
    private btnPick: Button = null;
    private btnPickBlock: Node = null;
    private btnAuto: Button = null;
    private btnAutoBlock: Node = null;
    private mainRewardScroll: ScrollView = null;
    private addRewardScroll: ScrollView = null;
    private btnWorld: Button = null;
    private btnWorldBlock: Node = null;
    private btnStart: Button = null;
    private btnStartBlock: Node = null;
    private pickDragons: {} = null;
    private timeObject: TimeObject = null;
    private energyLabel: Label = null;

    private curWorld = 1;
    private uiWorld = -1;
    private worldTripTable: WorldTripTable = null;
    private resource: ResourceManager = null;
    private itemGroupTable: ItemGroupTable = null;

    onLoad() {
        this.characterSlots = [];
        this.pickDragons = {};

        for(var i = 0 ; i < 5 ; i++) {
            let tag = sys.localStorage.getItem(`trip${i}`);
            if(tag == undefined || tag == "") {
                continue;
            }
            let dragon = User.Instance.DragonData.GetDragon(tag);
            if (dragon != null) {
                this.pickDragons[i] = dragon;
            }
        }

        this.nodeConstruct = this.node.getChildByName('construct');
        if(this.nodeConstruct != null) {
            this.nodeBlock = this.nodeConstruct.getChildByName('block');
            let bodyNode = this.nodeConstruct.getChildByName('body');
            if(bodyNode != null) {
                let constructTitle = bodyNode.getChildByName('labelTitle');
                if(constructTitle != null) {
                    this.labelConstructTitle = constructTitle.getComponent(Label);
                }
                let btnConstruct = bodyNode.getChildByName('btnConstruct');
                if(btnConstruct != null) {
                    this.btnConstruct = btnConstruct.getComponent(Button);
                    if(this.btnConstruct != null) {
                        let newEvent = new EventHandler();
                        newEvent.target = this.node;
                        newEvent.component = "WorldtripLayer"
                        newEvent.handler = "OnClickConstruct";
                        this.btnConstruct.clickEvents.push(newEvent);
                    }
                    let labelBtn = btnConstruct.getChildByName('Label');
                    if(labelBtn != null) {
                        this.labelContructBtn = labelBtn.getComponent(Label);
                    }
                }
            }
        }
        this.nodeLevelUp = this.node.getChildByName('levelUp');
        if(this.nodeLevelUp != null) {
            let bodyNode = this.nodeLevelUp.getChildByName('body');
            if(bodyNode != null) {
                let labelLevelUpTitle = bodyNode.getChildByName('Label');
                if(labelLevelUpTitle != null) {
                    this.labelLevelUpTitle = labelLevelUpTitle.getComponent(Label);
                }
                let timeNode = bodyNode.getChildByName('nodeTimer');
                if(timeNode != null) {
                    let labelTimer = timeNode.getChildByName('labeltimer');
                    if(labelTimer != null ) {
                        this.labelConstructTimer = labelTimer.getComponent(Label);
                    }
                    let hasteBtn = timeNode.getChildByName('btnAccel');
                    if(hasteBtn != null) {
                        this.hasteBtn = hasteBtn.getComponent(Button);
                        if(this.hasteBtn != null) {
                            let newEvent = new EventHandler();
                            newEvent.target = this.node;
                            newEvent.component = "WorldtripLayer"
                            newEvent.handler = "OnClickHaste";
                            this.hasteBtn.clickEvents.push(newEvent);
                        }
                    }
                }
            }
        }
        this.nodeConstructFinish = this.node.getChildByName('constructFinish');
        if(this.nodeConstructFinish != null) {
            let btnFinish = this.nodeConstructFinish.getChildByName('btnConstruct');
            if(btnFinish != null) {
                this.btnFinish = btnFinish.getComponent(Button);
                if(this.btnFinish != null) {
                    let newEvent = new EventHandler();
                    newEvent.target = this.node;
                    newEvent.component = "WorldtripLayer"
                    newEvent.handler = "OnClickConstructFinish";
                    this.btnFinish.clickEvents.push(newEvent);
                }
                let btnLabel = btnFinish.getChildByName('labelFinish');
                if(btnLabel != null) {
                    this.labelBtnFinish = btnLabel.getComponent(Label);
                }
            }
        }

        this.nodeNormal = this.node.getChildByName('normal');
        if(this.nodeNormal != null) {
            let bodyNode = this.nodeNormal.getChildByName('body');
            if(bodyNode != null) {
                let topNode = bodyNode.getChildByName('Top');
                if(topNode != null) {
                    let titleNode = topNode.getChildByName('TitleNode');
                    if(titleNode != null) {
                        let titleLabel = titleNode.getChildByName('labelTitle');
                        if(titleLabel != null) {
                            this.labelLayerTitle = titleLabel.getComponent(Label);
                        }
                        let timeNode = titleNode.getChildByName('remainTime');
                        if(timeNode != null) {
                            let timeLabel = timeNode.getChildByName('labelTimer');
                            if(timeLabel != null) {
                                this.labelNormalTimer = timeLabel.getComponent(Label);
                            } 
                        }
                        let btnHaste = titleNode.getChildByName('btnAccel');
                        if(btnHaste != null) {
                            this.btnHaste = btnHaste.getComponent(Button);
                            if(this.btnHaste != null) {
                                let newEvent = new EventHandler();
                                newEvent.target = this.node;
                                newEvent.component = "WorldtripLayer"
                                newEvent.handler = "OnClickHaste";
                                this.btnHaste.clickEvents.push(newEvent);
                            }
                        }
                    }
                }
                let contentNode = bodyNode.getChildByName('CharacterLayout');
                if(contentNode != null) {
                    let slotsNode = contentNode.getChildByName('character');
                    if(slotsNode != null) {
                        let slotNodes = slotsNode.getChildByName('Character');
                        if(slotNodes != null) {
                            for(var i = 1 ; i <= 5 ; i++) {
                                let curSlot = slotNodes.getChildByName(`${i}`);
                                if(curSlot != null) {
                                    let select = curSlot.getChildByName('Select');
                                    if(select != null) {
                                        select.active = false;
                                        let container = select.getChildByName('character');
                                        let pos = select.getChildByName('deco_shadow');
                                        if(container != null && pos != null) {
                                            this.characterSlots.push({slot:curSlot, target:select, container:container, pos:pos});
                                        }
                                    }
                                }
                            }
                        }
                        let fillSlot = slotsNode.getChildByName('Fill_slot');
                        if(fillSlot != null) {
                            let fillSlotLabel = fillSlot.getChildByName('Label');
                            if(fillSlotLabel != null) {
                                this.fillSlotLabel = fillSlotLabel.getComponent(Label);
                            }
                        }
                    }

                    let btnPick = contentNode.getChildByName('btnCharacterPick');
                    if(btnPick != null) {
                        this.btnPick = btnPick.getComponent(Button);
                        if(this.btnPick != null) {
                            let newEvent = new EventHandler();
                            newEvent.target = this.node;
                            newEvent.component = "WorldtripLayer"
                            newEvent.handler = "OnClickPick";
                            this.btnPick.clickEvents.push(newEvent);
                        }
                        this.btnPickBlock = btnPick.getChildByName('block');
                    }
                    let btnAuto = contentNode.getChildByName('btnAuto');
                    if(btnAuto != null) {
                        this.btnAuto = btnAuto.getComponent(Button);
                        if(this.btnAuto != null) {
                            let newEvent = new EventHandler();
                            newEvent.target = this.node;
                            newEvent.component = "WorldtripLayer"
                            newEvent.handler = "OnClickAuto";
                            this.btnAuto.clickEvents.push(newEvent);
                        }
                        this.btnAutoBlock = btnAuto.getChildByName('block');
                    }
                }

                let rewardNode = bodyNode.getChildByName('rewardLayout');
                if(rewardNode != null) {
                    let MainRewardNode = rewardNode.getChildByName('MainReward');
                    if(MainRewardNode != null) {
                        let mainScroll = MainRewardNode.getChildByName('mainRewardScroll');
                        if(mainScroll != null) {
                            this.mainRewardScroll = mainScroll.getComponent(ScrollView);
                        }
                    }
                    let AddRewardNode = rewardNode.getChildByName('AddReward');
                    if(AddRewardNode != null) {
                        let addScroll = AddRewardNode.getChildByName('addRewardScroll');
                        if(addScroll != null) {
                            this.addRewardScroll = addScroll.getComponent(ScrollView);
                        }
                    }
                    let btnWorld = rewardNode.getChildByName('btnWorldChange');
                    if(btnWorld != null) {
                        this.btnWorld = btnWorld.getComponent(Button);
                        if(this.btnWorld != null) {
                            let newEvent = new EventHandler();
                            newEvent.target = this.node;
                            newEvent.component = "WorldtripLayer"
                            newEvent.handler = "OnClickWorld";
                            this.btnWorld.clickEvents.push(newEvent);
                        }
                        this.btnWorldBlock = btnWorld.getChildByName('block');
                    }
                    let btnStart = rewardNode.getChildByName('btnStart');
                    if(btnStart != null) {
                        this.btnStart = btnStart.getComponent(Button);
                        if(this.btnStart != null) {
                            let newEvent = new EventHandler();
                            newEvent.target = this.node;
                            newEvent.component = "WorldtripLayer"
                            newEvent.handler = "OnClickStart";
                            this.btnStart.clickEvents.push(newEvent);
                        }
                        this.btnStartBlock = btnStart.getChildByName('block');
                        let icon = btnStart.getChildByName('icon');
                        if(icon != null) {
                            let energy = icon.getChildByName('Label');
                            if(energy != null) {
                                this.energyLabel = energy.getComponent(Label);
                            }
                        }
                    }
                }
            }
        }

        if(this.mainRewardScroll != null) {
            this.mainRewardScroll.content.removeAllChildren();
        }
        if(this.addRewardScroll != null) {
            this.addRewardScroll.content.removeAllChildren();
        }

        this.timeObject = this.getComponent(TimeObject);
        if(this.timeObject == null) {
            this.timeObject = this.addComponent(TimeObject);
        }

        this.worldTripTable = TableManager.GetTable(WorldTripTable.Name);
        this.resource = GameManager.GetManager(ResourceManager.Name);
        this.itemGroupTable = TableManager.GetTable(ItemGroupTable.Name);
    }

    start() {
        this.SetDragon();
    }

    Init()
    {
        if(this.buildingOpenData == null) {
            this.buildingOpenData = TableManager.GetTable<BuildingOpenTable>(BuildingOpenTable.Name).GetWithTag(eLandmarkType.WORLDTRIP)
        }
        
        if(this.buildingInfo == null) {
            this.buildingInfo = User.Instance.GetUserBuildingByTag(eLandmarkType.WORLDTRIP);
        }
        
        this.nodeNormal.active = true;
        this.nodeConstruct.active = false;
        this.nodeConstructFinish.active = false;
        this.nodeLevelUp.active = false;
        this.btnHaste.node.active = false;
        
        this.labelLayerTitle.string = TableManager.GetTable<StringTable>(StringTable.Name).Get(100000026).TEXT;

        var curCount = 0;
        const maxCount = 5;
        if(ObjectCheck(this.pickDragons, 0)) { curCount++; }
        if(ObjectCheck(this.pickDragons, 1)) { curCount++; }
        if(ObjectCheck(this.pickDragons, 2)) { curCount++; }
        if(ObjectCheck(this.pickDragons, 3)) { curCount++; }
        if(ObjectCheck(this.pickDragons, 4)) { curCount++; }

        if(this.fillSlotLabel != null) {
            this.fillSlotLabel.string = `${curCount}/${maxCount}`;
        }

        let worldData = this.worldTripTable.GetByWorldID(this.curWorld);
        if(worldData != null) {
            if(this.energyLabel != null) {
                this.energyLabel.string = `-${worldData.COST_STAMINA}`
            }

            if(this.uiWorld != this.curWorld) {
                if(this.mainRewardScroll != null) {
                    this.mainRewardScroll.content.removeAllChildren();

                    if(worldData.REWARD_GOLD > 0) {
                        let newItem : Node = instantiate(this.resource.GetResource(ResourcesType.UI_PREFAB)["item"]);
                        if(newItem != null) {
                            let frame = newItem.getComponent(ItemFrame);
                            if(frame != null) {
                                frame.setFrameCashInfo(0, worldData.REWARD_GOLD);
                            }
                            let transform = newItem.getComponent(UITransform);
                            if(transform != null) {
                                transform.setContentSize(new Size(56, 56));
                            }
                            newItem.parent = this.mainRewardScroll.content;
                        }
                    }
                }
                if(this.addRewardScroll != null) {
                    this.addRewardScroll.content.removeAllChildren();

                    if(worldData.REWARD_BONUS > 0) {
                        let itemGroup = this.itemGroupTable.Get(worldData.REWARD_BONUS);
                        if(itemGroup != null) {
                            itemGroup.forEach(element => {
                                switch(element.TYPE) {
                                    case 'RESOURCE': {
                                        switch(element.VALUE) {
                                            case eGoodType.GOLD: {
                                                let newItem : Node = instantiate(this.resource.GetResource(ResourcesType.UI_PREFAB)["item"]);
                                                if(newItem != null) {
                                                    let frame = newItem.getComponent(ItemFrame);
                                                    if(frame != null) {
                                                        frame.setFrameCashInfo(eGoodType.GOLD, element.NUM);
                                                    }
                                                    let transform = newItem.getComponent(UITransform);
                                                    if(transform != null) {
                                                        transform.setContentSize(new Size(56, 56));
                                                    }
                                                    newItem.parent = this.addRewardScroll.content;
                                                }
                                            } break;
                                            case eGoodType.ENERGY: {
                                                let newItem : Node = instantiate(this.resource.GetResource(ResourcesType.UI_PREFAB)["item"]);
                                                if(newItem != null) {
                                                    let frame = newItem.getComponent(ItemFrame);
                                                    if(frame != null) {
                                                        frame.setFrameEnergyInfo(element.NUM);
                                                    }
                                                    let transform = newItem.getComponent(UITransform);
                                                    if(transform != null) {
                                                        transform.setContentSize(new Size(56, 56));
                                                    }
                                                    newItem.parent = this.addRewardScroll.content;
                                                }
                                            } break;
                                        }
                                    } break;
                                    case 'ENERGY': {
                                        let newItem : Node = instantiate(this.resource.GetResource(ResourcesType.UI_PREFAB)["item"]);
                                        if(newItem != null) {
                                            let frame = newItem.getComponent(ItemFrame);
                                            if(frame != null) {
                                                frame.setFrameEnergyInfo(element.NUM);
                                            }
                                            let transform = newItem.getComponent(UITransform);
                                            if(transform != null) {
                                                transform.setContentSize(new Size(56, 56));
                                            }
                                            newItem.parent = this.addRewardScroll.content;
                                        }
                                    } break;
                                    case 'GOLD': {
                                        let newItem : Node = instantiate(this.resource.GetResource(ResourcesType.UI_PREFAB)["item"]);
                                        if(newItem != null) {
                                            let frame = newItem.getComponent(ItemFrame);
                                            if(frame != null) {
                                                frame.setFrameCashInfo(eGoodType.GOLD, element.NUM);
                                            }
                                            let transform = newItem.getComponent(UITransform);
                                            if(transform != null) {
                                                transform.setContentSize(new Size(56, 56));
                                            }
                                            newItem.parent = this.addRewardScroll.content;
                                        }
                                    } break;
                                    case 'ITEM': {
                                        let newItem : Node = instantiate(this.resource.GetResource(ResourcesType.UI_PREFAB)["item"]);
                                        if(newItem != null) {
                                            let frame = newItem.getComponent(ItemFrame);
                                            if(frame != null) {
                                                frame.setFrameItemInfo(element.VALUE, element.NUM);
                                            }
                                            let transform = newItem.getComponent(UITransform);
                                            if(transform != null) {
                                                transform.setContentSize(new Size(56, 56));
                                            }
                                            newItem.parent = this.addRewardScroll.content;
                                        }
                                    }
                                }
                            });
                        }
                    }
                }
            }
        }

        switch(this.buildingInfo.State) {
            case eBuildingState.CONSTRUCTING:
            case eBuildingState.CONSTRUCT_FINISHED: {
                if(TimeManager.GetTimeCompare(this.buildingInfo.ActiveTime) > 0 && this.buildingInfo.State != eBuildingState.CONSTRUCT_FINISHED)
                {
                    this.timeObj = this.getComponent(TimeObject);
                    this.timeObj.curTime = TimeManager.GetTime();
                    this.timeObj.Refresh = () => {
                        const time = TimeManager.GetTimeCompare(this.buildingInfo.ActiveTime);
                        this.labelConstructTimer.string = TimeString(time);
                        if(time <= 0) {
                            this.timeObj.Refresh = undefined;
                            this.nodeLevelUp.active = false;
                            this.nodeConstructFinish.active = true;
                        }
                    }
                    this.timeObj.Refresh();
                    this.nodeLevelUp.active = true;
                }
                else
                {
                    this.OnClickConstructFinish();
                    //완료 노드
                    // this.nodeConstructFinish.active = true;
                }
            } break;
            case eBuildingState.LOCKED: {
                this.nodeConstruct.active = true;
                this.nodeBlock.active = true;
                this.labelConstructTitle.node.active = true;
                this.labelConstructTitle.string = StringBuilder(TableManager.GetTable<StringTable>(StringTable.Name).Get(100000059).TEXT, this.buildingOpenData.OPEN_LEVEL);
            } break;
            case eBuildingState.NOT_BUILT: {
                this.nodeConstruct.active = true;
                this.labelConstructTitle.node.active = false;
                this.nodeBlock.active = false;
            } break;
            case eBuildingState.NORMAL: {
                this.nodeNormal.active = true;

                let tripData = User.Instance.GetLandmarkData(eLandmarkType.WORLDTRIP) as LandmarkWorldtrip;
                if(tripData != null) {
                    switch(tripData.TripState) {
                        case eWorldTripState.Normal: {
                            this.btnPickBlock.active = false;
                            this.btnAutoBlock.active = false;
                            this.btnWorldBlock.active = false;
                            this.btnStartBlock.active = true;

                            if(maxCount == curCount) {
                                this.btnAutoBlock.active = true;
                                this.btnStartBlock.active = false;
                            }
                            if(this.labelNormalTimer != null) {
                                this.labelNormalTimer.string = TimeString(-1);
                                if(worldData != null) {
                                    this.labelNormalTimer.string = TimeString(worldData.TIME);
                                }
                            }
                        } break;
                        case eWorldTripState.Complete: {
                            this.btnPickBlock.active = true;
                            this.btnAutoBlock.active = true;
                            this.btnWorldBlock.active = true;
                            this.btnStartBlock.active = true;
                            this.btnHaste.node.active = true;
                            
                            this.OnFinish();
                        } break;
                        case eWorldTripState.Trip: {
                            this.btnPickBlock.active = true;
                            this.btnAutoBlock.active = true;
                            this.btnWorldBlock.active = true;
                            this.btnStartBlock.active = true;
                            this.btnHaste.node.active = true;

                            if(this.timeObject != null && this.timeObject.Refresh == undefined) {
                                this.timeObject.Refresh = () => {
                                    let timeData = User.Instance.GetLandmarkData(eLandmarkType.WORLDTRIP) as LandmarkWorldtrip;
                                    if(timeData != null) {
                                        const time = TimeManager.GetTimeCompare(timeData.TripTime);
                                        if(time < 0) {
                                            this.timeObject.Refresh = undefined;
                                            timeData.TripState = eWorldTripState.Complete;
                                            this.ForceUpdate();
                                        } else {
                                            if(this.labelNormalTimer != null) {
                                                this.labelNormalTimer.string = TimeString(time);
                                            }
                                        }
                                    }
                                }
                                this.timeObject.Refresh();
                            }
                        } break;
                    }
                }
            } break;
        }
    }

    ForceUpdate() {
        this.Init()
    }

    SetDragon() {
        const pickKeys = Object.keys(this.pickDragons);
        const pickCount = pickKeys.length;
        for(var i = 0 ; i < pickCount ; i++) {
            this.SetSlot(Number(pickKeys[i]), this.pickDragons[pickKeys[i]]);
        }
    }

    OnClickHaste() {
        let tripData = User.Instance.GetLandmarkData(eLandmarkType.WORLDTRIP) as LandmarkWorldtrip;
        let worldData = this.worldTripTable.GetByWorldID(tripData.TripWorld);

        if(tripData == null || worldData == null) {
            return;
        }
        
        PopupManager.OpenPopup("AccelerationMainPopup", false, {
            title:"가속 사용", 
            body:"여행중 ...", 
            type: eAccelerationType.JOB, 
            tag:eLandmarkType.WORLDTRIP,
            time:worldData.TIME,
            time_end:tripData.TripTime,
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

    OnClickConstruct() {
        let popup = PopupManager.OpenPopup("BuildingTypeCommon") as BuildingTypeCommon
        popup.setMessage("travel")
    }

    OnClickConstructFinish()
    {
        let params = {
            tag : eLandmarkType.WORLDTRIP
        }

        NetworkManager.Send("building/complete", params, () =>
        {
            PopupManager.ForceUpdate();
        })
    }

    OnClickPick() {
        let textData = TableManager.GetTable<StringTable>(StringTable.Name).Get(100000631);
        if(textData != null)
        {
            ToastMessage.Set(textData.TEXT, null, -52);
        }
    }

    OnClickAuto() {
        let dragonData = User.Instance.DragonData;
        if(dragonData != null) {
            let tempCount = 5;
            let allDragons = dragonData.GetAllUserDragons();
            if(allDragons.length < 5) {
                tempCount = allDragons.length;
            }
            let dragons = dragonData.GetRandomDragon(tempCount, this.pickDragons);
            const count = dragons.length;
            // if(count < 5) {
            //     return;
            // }

            for(var i = 0 ; i < count ; i++) {
                if(dragons[i] != null) {
                    this.pickDragons[i] = dragons[i];
                }
            }

            const pickKeys = Object.keys(this.pickDragons);
            const pickCount = pickKeys.length;
            for(var i = 0 ; i < pickCount ; i++) {
                this.SetSlot(Number(pickKeys[i]), this.pickDragons[pickKeys[i]]);
            }
        }

        this.ForceUpdate();
    }

    OnClickWorld() {
        let textData = TableManager.GetTable<StringTable>(StringTable.Name).Get(100000632);
        if(textData != null)
        {
            ToastMessage.Set(textData.TEXT, null, -52);
        }
    }

    OnClickStart() {
        NetworkManager.Send('travel/start', {
            world: this.curWorld,
            dragon: [
                this.pickDragons[0].Tag,
                this.pickDragons[1].Tag,
                this.pickDragons[2].Tag,
                this.pickDragons[3].Tag,
                this.pickDragons[4].Tag
            ]
        }, (jsonData) => {
            if(jsonData['rs'] == 0) {
                const keys = Object.keys(this.pickDragons);
                const keysCount = keys.length;
    
                for(var i = 0 ; i < keysCount ; i++) {
                    this.pickDragons[keys[i]].State = eWorldTripState.Trip;
                    sys.localStorage.setItem(`trip${keys[i]}`, this.pickDragons[keys[i]].Tag);
                }

                this.ForceUpdate();
            }
        });
    }

    OnFinish() {
        this.timeObject.Refresh = undefined;
        NetworkManager.Send('travel/finish', {}, (jsonData) => {
            if(jsonData['err'] == 0) {
                const keys = Object.keys(this.pickDragons);
                const keysCount = keys.length;
    
                for(var i = 0 ; i < keysCount ; i++) {
                    this.pickDragons[keys[i]].State = eWorldTripState.Normal;
                }
    
                for(var i = 0 ; i < 5 ; i++) {
                    this.DelSlot(i);
                    sys.localStorage.removeItem(`trip${i}`);
                }

                this.ForceUpdate();

                let rewards:any = [];

                if(ObjectCheck(jsonData, 'rewards')) {
                    const arrayItems = jsonData['rewards'];
                    if(arrayItems != null) {
                        const count = arrayItems.length;

                        for(var i = 0 ; i < count ; i++) {
                            const item = arrayItems[i];
                            if(item == undefined) {
                                continue;
                            }

                            rewards.push(item);
                        }
                    }
                }

                if(ObjectCheck(jsonData, 'bonus')) {
                    const arrayItems = jsonData['bonus'];
                    if(arrayItems != null) {
                        const count = arrayItems.length;

                        for(var i = 0 ; i < count ; i++) {
                            const item = arrayItems[i];
                            if(item == undefined) {
                                continue;
                            }

                            rewards.push(item);
                        }
                    }
                }
                
                if(rewards.length > 0) {
                    let popup = PopupManager.OpenPopup("SystemRewardPopup") as SystemRewardPopup
                    if(popup != null) {
                        popup.initWithList(rewards);
                    }
                }
            }
        });
    }

    SetSlot(slot: number, data: UserDragon) {
        if(this.characterSlots == null) {
            return;
        }
        const slotCount = this.characterSlots.length;
        if(slot < 0 || slot >= slotCount) {
            return;
        }
        let curSlot = this.characterSlots[slot];
        if(curSlot == null) {
            return;
        }

        if(data == null) {
            if(curSlot['target'] != null) {
                curSlot['target'].active = false;
            }

            if(curSlot['container'] != null) {
                curSlot['container'].removeAllChildren();
            }
            return;
        }

        if(curSlot['target'] != null) {
            curSlot['target'].active = true;
        }

        if(curSlot['container'] != null) {
            curSlot['container'].removeAllChildren();
            let node = instantiate(data.Prefab);
            const layer = 1 << Layers.nameToLayer('UI_2D');
            ChangeLayer(node, layer);

            node.parent = curSlot['container'];
            node.worldPosition = curSlot['pos'].worldPosition;
        }
    }

    DelSlot1() {
        this.DelSlot(0);
    }
    DelSlot2() {
        this.DelSlot(1);
    }
    DelSlot3() {
        this.DelSlot(2);
    }
    DelSlot4() {
        this.DelSlot(3);
    }
    DelSlot5() {
        this.DelSlot(4);
    }
    DelSlot(slot: number) {
        delete this.pickDragons[slot];
        this.SetSlot(slot, null);
    }
}

/**
 * [1] Class member could be defined like this.
 * [2] Use `property` decorator if your want the member to be serializable.
 * [3] Your initialization goes here.
 * [4] Your update function goes here.
 *
 * Learn more about scripting: https://docs.cocos.com/creator/3.3/manual/en/scripting/
 * Learn more about CCClass: https://docs.cocos.com/creator/3.3/manual/en/scripting/ccclass.html
 * Learn more about life-cycle callbacks: https://docs.cocos.com/creator/3.3/manual/en/scripting/life-cycle-callbacks.html
 */

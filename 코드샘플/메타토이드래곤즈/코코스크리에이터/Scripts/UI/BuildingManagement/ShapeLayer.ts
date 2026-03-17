
import { _decorator, Node, Label, Sprite, Button, ScrollView, Prefab, EventHandler, ProgressBar, instantiate, UITransform, math } from 'cc';
import { AreaExpansionTable, AreaLevelTable } from '../../Data/AreaTable';
import { StringTable } from '../../Data/StringTable';
import { TableManager } from '../../Data/TableManager';
import { GameManager } from '../../GameManager';
import { NetworkManager } from '../../NetworkManager';
import { ResourceManager, ResourcesType } from '../../ResourceManager';
import { TimeObject } from '../../Time/ITimeObject';
import { TimeManager } from '../../Time/TimeManager';
import { GetChild, StringBuilder, TimeString } from '../../Tools/SandboxTools';
import { eBuildingState, eGoodType, User } from '../../User/User';
import { eAccelerationType } from '../AccelerationMainPopup';
import { PopupManager } from '../Common/PopupManager';
import { TapLayer } from '../Common/TapLayer';
import { ItemFrame } from '../ItemSlot/ItemFrame';
import { ToastMessage } from '../ToastMessage';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = ShapeLayer
 * DateTime = Mon Jan 10 2022 19:46:37 GMT+0900 (대한민국 표준시)
 * Author = seungmin
 * FileBasename = ShapeLayer.ts
 * FileBasenameNoExtension = ShapeLayer
 * URL = db://assets/Scripts/UI/BuildingManagement/ShapeLayer.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
@ccclass('ShapeLayer')
export class ShapeLayer extends TapLayer
{
    //왼쪽
    private targetNameLabel: Label = null;
    private targetSpr: Sprite = null;
    private targetLeftBtn: Button = null;
    private targetRightBtn: Button = null;
    private targetBuyBtn: Button = null;
    private targetBuyLabel: Label = null;

    //오른쪽
    private titleLabel1: Label = null;
    private titleLabel2: Label = null;
    private calumLabel1: Label = null;
    private calumLabel2: Label = null;
    private levelCurLabel: Label = null;
    private levelNextLabel: Label = null;
    private floorCurLabel: Label = null;
    private floorNextLabel: Label = null;

    private maxLevelNode: Node = null;
    private maxLevelLabel: Label = null;

    private productNode: Node = null;
    private timeTitleLabel: Label = null;
    private timeLabel: Label = null;
    private timeProgress: ProgressBar = null;
    private accelerationBtn: Button = null;

    private productFinish: Node = null;
    private finishTitleLabel: Label = null;
    private finishLabel: Label = null;
    private finishProgress: ProgressBar = null;
    private completeBtn: Button = null;

    private levelUpNode: Node = null;

    private missionTitleLabel: Label = null;
    private missionScroll: ScrollView = null;
    private missionPrefab: Prefab = null;
    private missionObjects: [] = null;

    private needItemTitleLabel: Label = null
    private needItemScroll: ScrollView = null;
    private needItemPrefab: Prefab = null;
    private produceBtn: Button = null;
    private producePriceLabel: Label = null;
    private producePriceSpr: Sprite = null; 

    private stringTable: StringTable = null;
    private areaLevelTable: AreaLevelTable = null;
    private areaExpansionTable: AreaExpansionTable = null;
    private timeObject: TimeObject = null;

    private curLevel: number = -1;

    onLoad() {
        let parentNode = this.node.getChildByName('Shape');
        if(parentNode != null) {
            let leftNode = parentNode.getChildByName('background2');
            if(leftNode != null) {
                let targetNameNode = leftNode.getChildByName('Label_000');
                if(targetNameNode != null) {
                    this.targetNameLabel = targetNameNode.getComponent(Label);
                }

                let buildingButtonNode = leftNode.getChildByName('buildingButton');
                if(buildingButtonNode != null) {
                    this.targetBuyBtn = buildingButtonNode.getComponent(Button);
                    if(this.targetBuyBtn != null) {
                        let newEvent = new EventHandler();
                        newEvent.target = this.node;
                        newEvent.component = "ShapeLayer"
                        newEvent.handler = "BuyBtn";
                        this.targetBuyBtn.clickEvents.push(newEvent);
                    }
                    let labelMsg = buildingButtonNode.getChildByName('labelMsg');
                    if(labelMsg != null) {
                        this.targetBuyLabel = labelMsg.getComponent(Label);
                    }
                }

                let imageNode = leftNode.getChildByName('image');
                if(imageNode != null) {
                    this.targetSpr = imageNode.getComponent(Sprite);
                }

                let leftBtnNode = leftNode.getChildByName('left_btn');
                if(leftBtnNode != null) {
                    this.targetLeftBtn = leftBtnNode.getComponent(Button);
                    if(this.targetLeftBtn != null) {
                        let newEvent = new EventHandler();
                        newEvent.target = this.node;
                        newEvent.component = "ShapeLayer"
                        newEvent.handler = "LeftBtn";
                        this.targetLeftBtn.clickEvents.push(newEvent);
                    }
                }
                let rifgtBtnNode = leftNode.getChildByName('right_btn');
                if(rifgtBtnNode != null) {
                    this.targetRightBtn = rifgtBtnNode.getComponent(Button);
                    if(this.targetRightBtn != null) {
                        let newEvent = new EventHandler();
                        newEvent.target = this.node;
                        newEvent.component = "ShapeLayer"
                        newEvent.handler = "RightBtn";
                        this.targetRightBtn.clickEvents.push(newEvent);
                    }
                }
            }
            
            let rightNode = parentNode.getChildByName('background3');
            if(rightNode != null) {
                let Label_000Node = rightNode.getChildByName('Label_000');
                if(Label_000Node != null) {
                    this.titleLabel1 = Label_000Node.getComponent(Label);
                }
                let Label_001Node = rightNode.getChildByName('Label_001');
                if(Label_001Node != null) {
                    this.titleLabel2 = Label_001Node.getComponent(Label);
                }
                let LayoutInfo = rightNode.getChildByName('LayoutInfo');
                if(LayoutInfo != null) {
                    let Layout_001Node = LayoutInfo.getChildByName('Layout_001');
                    if(Layout_001Node != null) {
                        let Label_002Node = Layout_001Node.getChildByName('Label_002');
                        if(Label_002Node != null) {
                            this.calumLabel1 = Label_002Node.getComponent(Label);
                        }
                        let Label_003Node = Layout_001Node.getChildByName('Label_003');
                        if(Label_003Node != null) {
                            this.calumLabel2 = Label_003Node.getComponent(Label);
                        }
                    }
                    let Layout_002Node = LayoutInfo.getChildByName('Layout_002');
                    if(Layout_002Node != null) {
                        let Label_004Node = Layout_002Node.getChildByName('Label_004');
                        if(Label_004Node != null) {
                            this.levelCurLabel = Label_004Node.getComponent(Label);
                        }
                        let Label_005Node = Layout_002Node.getChildByName('Label_005');
                        if(Label_005Node != null) {
                            this.floorCurLabel = Label_005Node.getComponent(Label);
                        }
                    }
                    let Layout_004Node = LayoutInfo.getChildByName('Layout_004');
                    if(Layout_004Node != null) {
                        let Label_006Node = Layout_004Node.getChildByName('Label_006');
                        if(Label_006Node != null) {
                            this.levelNextLabel = Label_006Node.getComponent(Label);
                        }
                        let Label_007Node = Layout_004Node.getChildByName('Label_007');
                        if(Label_007Node != null) {
                            this.floorNextLabel = Label_007Node.getComponent(Label);
                        }
                    }
                }

                this.maxLevelNode = rightNode.getChildByName('MaxLevel');
                if(this.maxLevelNode != null) {
                    let bg_top_05Node = this.maxLevelNode.getChildByName('bg_top_05');
                    if(bg_top_05Node != null) {
                        let Label_002Node = bg_top_05Node.getChildByName('Label_002');
                        if(Label_002Node != null) {
                            this.maxLevelLabel = Label_002Node.getComponent(Label);
                        }
                    }
                }
                
                this.productNode = rightNode.getChildByName('Product');
                if(this.productNode != null) {
                    let bg_top_05Node = this.productNode.getChildByName('bg_top_05');
                    if(bg_top_05Node != null) {
                        let timerNode = bg_top_05Node.getChildByName('timerNode');
                        if(timerNode != null) {
                            let labelTitle = timerNode.getChildByName('labelTitle');
                            if(labelTitle != null) {
                                this.timeTitleLabel = labelTitle.getComponent(Label);
                            }
                            let progressBar = timerNode.getChildByName('ProgressBar');
                            if(progressBar != null) {
                                this.timeProgress = progressBar.getComponent(ProgressBar);
                                if(this.timeProgress != null) {
                                    let layoutNode = this.timeProgress.node.getChildByName('Layout');
                                    if(layoutNode != null) {
                                        let timeLabel = layoutNode.getChildByName('Label');
                                        if(timeLabel != null) {
                                            this.timeLabel = timeLabel.getComponent(Label);
                                        }
                                    }
                                }
                            }
                            let btn = timerNode.getChildByName('btn');
                            if(btn != null) {
                                this.accelerationBtn = btn.getComponent(Button);
                                if(this.accelerationBtn != null) {
                                    let newEvent = new EventHandler();
                                    newEvent.target = this.node;
                                    newEvent.component = "ShapeLayer"
                                    newEvent.handler = "AccelerationBtn";
                                    this.accelerationBtn.clickEvents.push(newEvent);
                                }
                            }
                        }
                    }
                }

                this.productFinish = rightNode.getChildByName('ProductFinish');
                if(this.maxLevelNode != null) {
                    let bg_top_05Node = this.productFinish.getChildByName('bg_top_05');
                    if(bg_top_05Node != null) {
                        let timerNode = bg_top_05Node.getChildByName('timerNode');
                        if(timerNode != null) {
                            let labelTitle = timerNode.getChildByName('labelTitle');
                            if(labelTitle != null) {
                                this.finishTitleLabel = labelTitle.getComponent(Label);
                            }
                            let progressBar = timerNode.getChildByName('ProgressBar');
                            if(progressBar != null) {
                                this.finishProgress = progressBar.getComponent(ProgressBar);
                                if(this.finishProgress != null) {
                                    let layoutNode = this.finishProgress.node.getChildByName('Layout');
                                    if(layoutNode != null) {
                                        let timeLabel = layoutNode.getChildByName('Label');
                                        if(timeLabel != null) {
                                            this.finishLabel = timeLabel.getComponent(Label);
                                        }
                                    }
                                }
                            }
                            let btn = timerNode.getChildByName('btn');
                            if(btn != null) {
                                this.completeBtn = btn.getComponent(Button);
                                if(this.completeBtn != null) {
                                    let newEvent = new EventHandler();
                                    newEvent.target = this.node;
                                    newEvent.component = "ShapeLayer"
                                    newEvent.handler = "CompleteBtn";
                                    this.completeBtn.clickEvents.push(newEvent);
                                }
                            }
                        }
                    }
                }

                let level_up = rightNode.getChildByName('LevelUp');
                if(level_up != null) {
                    this.levelUpNode = level_up;
                    let buildingButtonNode = GetChild(level_up , ['mission','buildingButton']);
                    if(buildingButtonNode != null) {
                        this.produceBtn = buildingButtonNode.getComponent(Button);
                        if(this.produceBtn != null) {
                            let newEvent = new EventHandler();
                            newEvent.target = this.node;
                            newEvent.component = "ShapeLayer"
                            newEvent.handler = "LevelUpBtn";
                            this.produceBtn.clickEvents.push(newEvent);
                        }
    
                        let layout_h = buildingButtonNode.getChildByName('layout_h');
                        if(layout_h != null) {
                            let layout_v = layout_h.getChildByName('layout_v');
                            if(layout_v != null) {
                                let cost = layout_v.getChildByName('cost');
                                if(cost != null) {
                                    let sprCost = cost.getChildByName('sprCost');
                                    if(sprCost != null) {
                                        this.producePriceSpr = sprCost.getComponent(Sprite);
                                    }
                                    let labelCost = cost.getChildByName('labelCost');
                                    if(labelCost != null) {
                                        this.producePriceLabel = labelCost.getComponent(Label);
                                    }
                                }
                            }
                        }
                    }
                    
                    let itemScroll = GetChild(level_up , ['mission','itemScroll']);
                    if(itemScroll != null) {
                        this.needItemScroll = itemScroll.getComponent(ScrollView);
                    }
                    let buildingScrollNode = GetChild(level_up , ['mission','background','buildingScroll']);
                    if(buildingScrollNode != null) {
                        this.missionScroll = buildingScrollNode.getComponent(ScrollView);
                    }
                    let Label_002Node = GetChild(level_up , ['mission','background','Label_002']);
                    if(Label_002Node != null) {
                        this.missionTitleLabel = Label_002Node.getComponent(Label);
                    }

                    let bg_top_05Node = level_up.getChildByName('bg_top_05');
                    if(bg_top_05Node != null) 
                    {
                        let bg_need = bg_top_05Node.getChildByName('bg_need');
                        if(bg_need != null) {
                            let Label_006Node = bg_need.getChildByName('Label_006');
                            if(Label_006Node != null) {
                                this.needItemTitleLabel = Label_006Node.getComponent(Label);
                            }
                        }
                    }
                }
            }
        }

        this.stringTable = TableManager.GetTable(StringTable.Name);
        this.areaLevelTable = TableManager.GetTable(AreaLevelTable.Name);
        this.areaExpansionTable = TableManager.GetTable(AreaExpansionTable.Name);
        this.timeObject = this.addComponent(TimeObject);
    }

    start() {
        this.curLevel = User.Instance.ExteriorData.ExteriorLevel;
        
        if(this.titleLabel1 != null) {
            let data = this.stringTable.Get(100000250);
            if(data != null) {
                this.titleLabel1.string = data.TEXT;
            }
        }

        if(this.titleLabel2 != null) {
            let data = this.stringTable.Get(100000251);
            if(data != null) {
                this.titleLabel2.string = data.TEXT;
            }
        }

        if(this.missionTitleLabel != null) {
            let data = this.stringTable.Get(100000252);
            if(data != null) {
                this.missionTitleLabel.string = data.TEXT;
            }
        }

        if(this.needItemTitleLabel != null) {
            let data = this.stringTable.Get(100000253);
            if(data != null) {
                this.needItemTitleLabel.string = data.TEXT;
            }
        }

        if(this.targetBuyLabel != null) {
            let data = this.stringTable.Get(100000102);
            if(data != null) {
                this.targetBuyLabel.string = data.TEXT;
            }
        }

        if(this.maxLevelLabel != null) {
            let data = this.stringTable.Get(100000255);
            if(data != null) {
                this.maxLevelLabel.string = data.TEXT;
            }
        }

        if(this.timeTitleLabel != null) {
            let data = this.stringTable.Get(100000256);
            if(data != null) {
                this.timeTitleLabel.string = data.TEXT;
            }
        }

        if(this.calumLabel1 != null) {
            let data = this.stringTable.Get(100000240);
            if(data != null) {
                this.calumLabel1.string = data.TEXT;
            }
        }

        if(this.calumLabel2 != null) {
            let data = this.stringTable.Get(100000258);
            if(data != null) {
                this.calumLabel2.string = data.TEXT;
            }
        }

        this.ForceUpdate();
    }

/**
 * @name this.popupData 팝업의 데이터 내용
 * @title 가속 제목
 * @body 건물 내용
 * @type 가속 대상 타입
 * @tag 가속 대상 번호
 * @time 가속 대상 목표 시간
 * @prices [{}] 가속 대상 필요 제화들
 ** @type 가속 대상 필요 제화 타입
 ** @price 가속 대상 필요 제화 수량
 */
    AccelerationBtn() {
        console.log('AccelerationBtnClick');

        let targetTime = -1;
        let areaData = this.areaLevelTable.Get(10000000 + User.Instance.ExteriorData.ExteriorLevel);
        if(areaData != null) {
            targetTime = areaData.UPGRADE_TIME;
        }

        PopupManager.OpenPopup("AccelerationMainPopup", false, {
            title:"가속 사용", 
            body:"외형 레벨 업 중 ...", 
            type: eAccelerationType.LEVELUP, 
            tag:1,
            time_end:User.Instance.ExteriorData.ExteriorTime,
            time:targetTime,
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

    CompleteBtn() {
        console.log('CompleteClick');
        NetworkManager.Send('building/complete', { tag: 1 }, (jsonData) => { this.CompleteCallBack(jsonData); });
    }

    LeftBtn() {
        console.log('LeftBtnClick');
    
        this.curLevel--;
        if(this.curLevel < 1) {
            this.curLevel = 1;
        }
        this.ForceUpdate();
    }

    RightBtn() {
        console.log('RightBtnClick');
    
        this.curLevel++;
        const maxLevel = this.areaLevelTable.GetMaxLevel();
        if(this.curLevel > maxLevel) {
            this.curLevel = maxLevel;
        }
        this.ForceUpdate();
    }

    BuyBtn() {
        console.log('BuyBtnClick');
        let userData = User.Instance.ExteriorData;
        let targetFloor = userData.ExteriorFloor + 1;
        let floorData = this.areaExpansionTable.GetFloorData(targetFloor);
        if(floorData != null) {
            if(floorData.OPEN_LEVEL <= userData.ExteriorLevel) {
                PopupManager.OpenPopup('FloorBuyPopup', false, {floor:targetFloor, type:floorData.COST_TYPE, count:floorData.COST_NUM});
            } else {
                let sData = this.stringTable.Get(100000261);
                if(sData != null) {
                    ToastMessage.Set(sData.TEXT, null, -52);
                } else {
                    ToastMessage.Set('레벨이 부족합니다.', null, -52);
                }
            }
        } else {
            let sData = this.stringTable.Get(100000262);
            if(sData != null) {
                ToastMessage.Set(sData.TEXT, null, -52);
            } else {
                ToastMessage.Set('최대 층입니다.', null, -52);
            }
        }
    }

    LevelUpBtn() {
        console.log('LevelUpBtnClick');
        NetworkManager.Send('building/levelup', { tag: 1 }, (jsonData) => { this.LevelCallBack(jsonData); });
    }

    CompleteCallBack(jsonData: any) {
        this.ForceUpdate();
    }

    LevelCallBack(jsonData: any) {
        this.ForceUpdate();
    }

    ForceUpdate() {
        if(this.stringTable == null || this.areaLevelTable == null || this.areaExpansionTable == null) {
            return;
        }
    
        let curData = User.Instance.ExteriorData;
        if(curData != null) {
            if(this.levelCurLabel != null) {
                this.levelCurLabel.string = `Lv.${curData.ExteriorLevel}`;
            }

            if(this.levelNextLabel != null) {
                this.levelNextLabel.string = `Lv.${curData.ExteriorLevel+1}`;
            }

            let floor = this.stringTable.Get(100000259);
            if(this.floorCurLabel != null) {
                if(floor != null) {
                    this.floorCurLabel.string = StringBuilder(floor.TEXT, User.Instance.GetMapData().w + 1);
                } else {
                    this.floorCurLabel.string = String(User.Instance.GetMapData().w + 1);
                }
            }

            if(this.floorNextLabel != null) {
                if(floor != null) {
                    this.floorNextLabel.string = StringBuilder(floor.TEXT, User.Instance.GetMapData().w + 1);
                } else {
                    this.floorNextLabel.string = String(User.Instance.GetMapData().w + 1);
                }
                let areaNextData = this.areaLevelTable.Get(curData.ExteriorLevel + 1);
                if(areaNextData != null) {
                    let areaNextExpansionData = this.areaExpansionTable.GetBetweenFloor(areaNextData.EXPANSION_AREA, areaNextData.LEVEL);
                    if(areaNextExpansionData != null) {
                        if(floor != null) {
                            this.floorNextLabel.string = StringBuilder(floor.TEXT, areaNextExpansionData.w);
                        } else {
                            this.floorNextLabel.string = String(areaNextExpansionData.w);
                        }
                    }
                }
            }
    
            if(this.calumLabel2 != null) {
                let data = this.stringTable.Get(100000258);
                if(data != null) {
                    this.calumLabel2.string = data.TEXT;
                }
            }

            if(this.targetNameLabel != null) {
                this.targetNameLabel.string = `Lv.${this.curLevel} 장난감성`;
            }

            if(this.areaLevelTable.GetMaxLevel() <= curData.ExteriorLevel && curData.ExteriorState != eBuildingState.CONSTRUCT_FINISHED) { //최대 레벨 입니다.
                this.levelUpNode.active = false;
                this.maxLevelNode.active = true;
                this.productNode.active = false;
                this.productFinish.active = false;
                this.targetBuyBtn.node.active = true;

                if(this.levelNextLabel != null) {
                    this.levelNextLabel.string = `Lv.MAX`;
                }
                return;
            }
            
            switch(curData.ExteriorState) {
                case eBuildingState.CONSTRUCT_FINISHED: {
                    this.timeObject.Refresh = undefined;
                    this.levelUpNode.active = false;
                    this.maxLevelNode.active = false;
                    this.productNode.active = false;
                    this.productFinish.active = true;
                    this.targetBuyBtn.node.active = false;
                } break;// 렙업 중 !
                case eBuildingState.CONSTRUCTING: {
                    this.levelUpNode.active = false;
                    this.maxLevelNode.active = false;
                    this.productNode.active = true;
                    this.productFinish.active = false;
                    this.targetBuyBtn.node.active = false;
                    this.timeObject.Refresh = () => {
                        var time = TimeManager.GetTimeCompare(User.Instance.ExteriorData.ExteriorTime);
                        const stringTime = TimeString(time);

                        if(this.timeLabel != null) {
                            this.timeLabel.string = stringTime;
                        }

                        let areaData = this.areaLevelTable.GetAll().find(value => value.LEVEL == User.Instance.ExteriorData.ExteriorLevel);
                        if(areaData != null) {
                            this.timeProgress.progress =  (areaData.UPGRADE_TIME - time) / areaData.UPGRADE_TIME;
                        }

                        if(time <= 0) {
                            User.Instance.ExteriorData.ExteriorState = eBuildingState.CONSTRUCT_FINISHED;
                            this.ForceUpdate();
                        }
                    };
                    this.timeObject.Refresh();
                } break;
                case eBuildingState.NOT_BUILT: // 정상 상태!
                case eBuildingState.NORMAL: {
                    this.levelUpNode.active = true;
                    this.maxLevelNode.active = false;
                    this.productNode.active = false;
                    this.productFinish.active = false;
                    this.targetBuyBtn.node.active = true;
                    this.SetNeedItems()

                    let areaData = this.areaLevelTable.GetAll().find(value => value.LEVEL == User.Instance.ExteriorData.ExteriorLevel);
                    if(areaData != null && this.producePriceLabel != null) {
                        this.producePriceLabel.string = areaData.NEED_GOLD.toLocaleString('ko-KR');
                    }
                } break;
                case eBuildingState.NONE: // 버근가?
                case eBuildingState.LOCKED: {
                } break;
            }
        }
    }

    private SetNeedItems()
    {
        if(this.needItemScroll == null || this.needItemScroll.content == null){
            return;
        }
        
        this.needItemScroll.content.removeAllChildren();
        let shapeLevelInfo = this.areaLevelTable.GetAll().find(value => value.LEVEL == User.Instance.ExteriorData.ExteriorLevel);
        console.log(shapeLevelInfo);
        for(let i = 0; i < shapeLevelInfo.NEED_ITEM.length; i++)
        {
            let itemClone : Node = instantiate(GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB)["item"]);
            itemClone.parent = this.needItemScroll.content;

            itemClone.getComponent(ItemFrame).setFrameReceipeInfo(shapeLevelInfo.NEED_ITEM[i].ITEM_NO, shapeLevelInfo.NEED_ITEM[i].ITEM_COUNT);
            //itemClone.getComponent(UITransform).setContentSize(new math.Size(55, 55));
        }
    }

    GetFloorCheck(): boolean {
        let userData = User.Instance.ExteriorData;
        let targetFloor = userData.ExteriorFloor + 1;
        let floorData = this.areaExpansionTable.GetFloorData(targetFloor);
        if(floorData != null) {
            if(floorData.OPEN_LEVEL <= userData.ExteriorLevel) {
                return true;
            }
        }
        
        return false;
    }

    Init()
    {
        this.ForceUpdate();
    }
}

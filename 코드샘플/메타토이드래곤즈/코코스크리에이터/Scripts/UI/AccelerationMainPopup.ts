
import { _decorator, Node, Label, Button, ProgressBar, ScrollView, instantiate, EventHandler } from 'cc';
import { ItemBaseData } from '../Data/ItemData';
import { ItemBaseTable } from '../Data/ItemTable';
import { TableManager } from '../Data/TableManager';
import { GameManager } from '../GameManager';
import { ResourceManager, ResourcesType } from '../ResourceManager';
import { TimeObject } from '../Time/ITimeObject';
import { TimeManager } from '../Time/TimeManager';
import { TimeString } from '../Tools/SandboxTools';
import { TutorialManager } from '../Tutorial/TutorialManager';
import { eGoodType, User } from '../User/User';
import { AccelerationMainClone } from './AccelerationMainClone';
import { Popup } from './Common/Popup';
import { PopupManager } from './Common/PopupManager';
const { ccclass } = _decorator;

/**
 * Predefined variables
 * Name = AccelerationMainPopup
 * DateTime = Thu Jan 27 2022 16:01:44 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = AccelerationMainPopup.ts
 * FileBasenameNoExtension = AccelerationMainPopup
 * URL = db://assets/Scripts/UI/AccelerationMainPopup.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */

export enum eAccelerationType {
    CONSTRUCT = 1,  // 건설
    LEVELUP = 2,    // 레벨업
    JOB = 3         // 생산작업
}

@ccclass('AccelerationMainPopup')
export class AccelerationMainPopup extends Popup {
    private titleLabel: Label = null;
    private closeBtn: Button = null;
    private timeObject: TimeObject = null;
    private timeTitleLabel: Label = null;
    private timeLabel: Label = null;
    private timeProgress: ProgressBar = null;
    private scrollView: ScrollView = null;
    private isInit: boolean = false;
    public get IsInit(): boolean {
        return this.isInit;
    }
    private resource: ResourceManager = null;
    private isExit: boolean = false;
    public set IsExit(value: boolean) {
        this.isExit = value;
    }
    public get IsExit(): boolean {
        return this.isExit;
    }

    OnLoad() { // UI 정보 대입
        let body = this.node.getChildByName('body');
        if(body != null) {
            let top = body.getChildByName('Top');
            if(top != null) {
                let labelTitle = top.getChildByName('labelTitle');
                if(labelTitle != null) {
                    this.titleLabel = labelTitle.getComponent(Label);
                }
                let btnClose = top.getChildByName('btnClose');
                if(btnClose != null) {
                    this.closeBtn = btnClose.getComponent(Button);
                    if(this.closeBtn != null) {
                        let newEvent = new EventHandler();
                        newEvent.target = this.node;
                        newEvent.component = "AccelerationMainPopup"
                        newEvent.handler = "CloseBtn";
                        this.closeBtn.clickEvents.push(newEvent);
                    }
                }
            }
            let content = body.getChildByName('Content');
            if (content != null) {
                let timerNode = content.getChildByName('timerNode');
                if (timerNode != null) {
                    let labelTitleNode = timerNode.getChildByName('labelTitle');
                    if (labelTitleNode != null) {
                        this.timeTitleLabel = labelTitleNode.getComponent(Label);
                    }

                    let bar =  timerNode.getChildByName('ProgressBar');
                    if (bar != null) {
                        this.timeProgress = bar.getComponent(ProgressBar);
                        let LayoutNode = bar.getChildByName('Layout');
                        if (LayoutNode != null) {
                            let label = LayoutNode.getChildByName('Label');
                            if (label != null) {
                                this.timeLabel = label.getComponent(Label);
                            }
                        }
                    }

                }
                let bot = content.getChildByName('Bot');
                if (bot != null) {
                    let receipeScroll = bot.getChildByName('receipeScroll');
                    if (receipeScroll != null) {
                        this.scrollView = receipeScroll.getComponent(ScrollView);
                    }
                }
            }
        }

        this.isExit = false;
        this.timeObject = this.getComponent(TimeObject);
        this.resource = GameManager.GetManager(ResourceManager.Name);
    }

    CloseBtn() {
        if(this.IsExit || !this.IsInit) {
            return;
        }
        PopupManager.ClosePopup(this);
    }

    Init(data?: any) {
        if(this.isInit) {
            return;
        }

        if(data != undefined || data != undefined) {
            this.SetData(data);
        }

        if(!this.InData()) {
            this.scheduleOnce(this.Init, 0.01);
        } else {
            this.InitExtenstion();
        }

        if (TutorialManager.GetInstance.IsNowPlayingTutorialByGroup(103)) {
            TutorialManager.GetInstance.OnTutorialEvent(103, 7);
        }
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
 ** @type_value 가속 대상 param
 ** @count 가속 대상 필요 제화 수량
 */
    SetData(popupData: any) {
        super.SetData(popupData);
    }

    private InData(): boolean {
        if(this.resource == null) {
            return false;
        }
        if(this.popupData == null || this.scrollView == null) {
            return false;
        }
        if(this.popupData['title'] != undefined) {
            if (this.titleLabel != null) {
                this.titleLabel.string = this.popupData['title'];
            }
        }
        if(this.popupData['body'] != undefined) {
            if (this.timeTitleLabel != null) {
                this.timeTitleLabel.string = this.popupData['body'];
            }
        }
        
        this.scrollView.content.removeAllChildren();
        let clonePrice = this.resource.GetResource(ResourcesType.UI_PREFAB)['AccelerationMainClone'];

        if(this.popupData['prices'] != undefined) {
            let arrayPrices:[] = this.popupData['prices'];
            const priceCount = arrayPrices.length;
            for(var i = 0 ; i < priceCount; i++) {
                let targetPrice:{} = arrayPrices[i];

                if(targetPrice['type'] != undefined && targetPrice['type_value'] != undefined && targetPrice['count'] != undefined) {
                    switch(targetPrice['type']) {
                        case eGoodType.ITEM: {
                            let items = User.Instance.GetAllItems();
                    
                            let itemData:ItemBaseData = null;
                            let itemTable:ItemBaseTable = TableManager.GetTable(ItemBaseTable.Name);
                            for(let i = 0; i < items.length; i++)
                            {
                                if(items[i].ItemNo != targetPrice['type_value']) {
                                    continue;
                                }
                                if(items[i].Count < 1) {
                                    continue;
                                }
    
                                let tempData = itemTable.Get(items[i].ItemNo);
                                if(tempData == null) {
                                    continue;
                                }
                                
                                itemData = tempData;
                                break;
                            }
    
                            if(itemData == null) {
                                continue;
                            }
                        } break;
                    }

                    let item = instantiate<Node>(clonePrice);
                    item.parent = this.scrollView.content;
                    let clone = item.getComponent(AccelerationMainClone);

                    if(clone != null) {
                        clone.SetData({popup:this, type:targetPrice['type'], type_value:targetPrice['type_value'], count:targetPrice['count'], onclick: null, time:this.popupData['time_end'], tag:this.popupData['tag'], acceleration_type:this.popupData['type'],platform:this.popupData['platform']});
                    }
                }
            }
        }

        if(this.timeObject != null && this.popupData['time_end'] != undefined) {
            this.timeObject.Refresh = () => {
                var time = TimeManager.GetTimeCompare(this.popupData['time_end']);
                const stringTime = TimeString(time);
    
                if(this.timeLabel != null) {
                    this.timeLabel.string = stringTime;
                }
    
                this.timeProgress.progress =  (this.popupData['time'] - time) / this.popupData['time'];
    
                if(time <= 0) {
                    this.timeObject.Refresh = undefined;
                }
            };
            this.timeObject.Refresh();
        }

        this.isInit = true;
        return true;
    }
}
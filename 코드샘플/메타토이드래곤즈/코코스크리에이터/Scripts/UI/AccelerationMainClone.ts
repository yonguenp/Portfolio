
import { _decorator, Component, Node, Label, Sprite, Button, EventHandler } from 'cc';
import { ItemBaseData } from '../Data/ItemData';
import { DefineResourceTable, ItemBaseTable } from '../Data/ItemTable';
import { StringTable } from '../Data/StringTable';
import { TableManager } from '../Data/TableManager';
import { NetworkManager } from '../NetworkManager';
import { TimeManager } from '../Time/TimeManager';
import { DataPriceChange } from '../Tools/SandboxTools';
import { TutorialManager } from '../Tutorial/TutorialManager';
import { eGoodType } from '../User/User';
import { PopupManager } from './Common/PopupManager';
const { ccclass } = _decorator;

/**
 * Predefined variables
 * Name = AccelerationMainClone
 * DateTime = Thu Feb 03 2022 16:13:19 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = AccelerationMainClone.ts
 * FileBasenameNoExtension = AccelerationMainClone
 * URL = db://assets/Scripts/UI/AccelerationMainClone.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
@ccclass('AccelerationMainClone')
export class AccelerationMainClone extends Component {
    private title: Label = null;
    private button: Button = null;
    private selectNode: Node = null;
    private selectLabel: Label = null;
    private selectLeftButton: Button = null;
    private selectRightbutton: Button = null;
    private priceIcon: Sprite = null;
    private priceLabel: Label = null;
    private data: {} = null;
    private isInit: boolean = false;
    private itemTable: ItemBaseTable = null;
    private typeTable: DefineResourceTable = null;
    private stringTable: StringTable = null;
    private itemData: ItemBaseData = null;

    private curSelect = 1;
    onLoad() {
        let titleNode = this.node.getChildByName('title');
        if(titleNode != null) {
            this.title = titleNode.getComponent(Label);
        }

        let sortNode = this.node.getChildByName('sort');
        if(sortNode != null) {
            let buttonNode = sortNode.getChildByName('Button');
            if(buttonNode != null) {
                this.button = buttonNode.getComponent(Button);
                if(this.button != null) {
                    let newEvent = new EventHandler();
                    newEvent.target = this.node;
                    newEvent.component = "AccelerationMainClone"
                    newEvent.handler = "ActiveBtn";
                    this.button.clickEvents.push(newEvent);
                }
    
                let iconNode = buttonNode.getChildByName('btnIcon');
                if(iconNode != null) {
                    this.priceIcon = iconNode.getComponent(Sprite);
                }
                
                let labelNode = buttonNode.getChildByName('btnLabel');
                if(labelNode != null) {
                    this.priceLabel = labelNode.getComponent(Label);
                }
            }

            this.selectNode = sortNode.getChildByName('select');
            if(this.selectNode != null) {
                let selectLabelNode = this.selectNode.getChildByName('selectLabel');
                if(selectLabelNode != null) {
                    this.selectLabel = selectLabelNode.getComponent(Label);
                }
                
                let select_left = this.selectNode.getChildByName('select_left');
                if(select_left != null) {
                    this.selectLeftButton = select_left.getComponent(Button);
                    if(this.selectLeftButton != null) {
                        let newEvent = new EventHandler();
                        newEvent.target = this.node;
                        newEvent.component = "AccelerationMainClone"
                        newEvent.handler = "LeftBtn";
                        this.selectLeftButton.clickEvents.push(newEvent);
                    }
                }
    
                let select_right = this.selectNode.getChildByName('select_right');
                if(select_right != null) {
                    this.selectRightbutton = select_right.getComponent(Button);
                    if(this.selectRightbutton != null) {
                        let newEvent = new EventHandler();
                        newEvent.target = this.node;
                        newEvent.component = "AccelerationMainClone"
                        newEvent.handler = "RightBtn";
                        this.selectRightbutton.clickEvents.push(newEvent);
                    }
                }
    
                this.selectNode.active = false;
            }
        }

        this.typeTable = TableManager.GetTable(DefineResourceTable.Name);
        this.stringTable = TableManager.GetTable(StringTable.Name);
        this.itemTable = TableManager.GetTable(ItemBaseTable.Name);
    }

    private Init() {
        if(this.isInit) {
            return;
        }

        if(!this.InData()) {
            this.scheduleOnce(this.Init, 0.01);
        }
    }

    private InData(): boolean {
        if(this.data == null || this.data['type'] == undefined || this.data['type_value'] == undefined || this.data['count'] == undefined) {
            return false;
        }
        if(this.title == null || this.button == null || this.priceIcon == null || this.priceLabel == null) {
            return false;
        }
        if(this.typeTable == null || this.stringTable == null) {
            return false;
        }

        switch(this.data['type']) {
            case eGoodType.ITEM: {
                this.itemData = this.itemTable.Get(this.data['type_value']);
                if(this.title != null && this.itemData != null) {
                    let data = this.stringTable.Get(this.itemData._NAME);
                    if(data != null) {
                        this.title.string = data.TEXT;
                    }
                }
                this.selectNode.active = true;
            } break;
            case eGoodType.CASH: {
                if(this.title != null) {
                    let data = this.stringTable.Get(100000087);
                    if(data != null) {
                        this.title.string = data.TEXT;
                    }
                }

                this.selectNode.active = false;
            } break;
        }

        DataPriceChange({type:this.data['type'], type_value:this.data['type_value'], count:this.data['count']}, this.priceIcon, this.priceLabel);
        this.Refresh();
        this.isInit = true;
        return true;
    }

    /**
     * @data 내부 구성 및 표기
     * @acceleration_type 가속 타입
     * @type 제화 타입
     * @type_value 제화 param
     * @count 가격
     * @time 남은 시간
     */
    public SetData(data: any) {
        this.data = data;
        this.Init();
    }

    Refresh() {
        switch(this.data['type']) {
            case eGoodType.ITEM: {
                if(this.selectLabel != null) {
                    this.selectLabel.string = String(this.curSelect);
                }
            } break;
            case eGoodType.CASH: {
            } break;
        }
    }

    ActiveBtn() {
        if(this.data['popup'].IsExit || !this.data['popup'].IsInit) {
            return;
        }
        switch(this.data['type']) {
            case eGoodType.ITEM: {
                this.data['popup'].IsExit = true;
                NetworkManager.Send('building/haste', {type:this.data['acceleration_type'],tag:this.data['tag'], item:this.data['type_value'], count:this.curSelect, platform:this.data['platform']}, (jsonData) => {
                    PopupManager.ClosePopup(this.data['popup']);
                    PopupManager.ForceUpdate()
                });

                if (TutorialManager.GetInstance.IsNowPlayingTutorialByGroup(103)) {
                    TutorialManager.GetInstance.OnTutorialEvent(103, 9);
                }
            } break;
            case eGoodType.CASH: {
                PopupManager.OpenPopup('AccelerationCashPopup', false, this.data);
            } break;
        }
    }

    LeftBtn() {
        if(this.data['popup'].IsExit || !this.data['popup'].IsInit) {
            return;
        }

        var minCount = 1;
        var maxCount = 99;
        var time = TimeManager.GetTimeCompare(this.data['time']);
        if(this.itemData != null) {
            if(time > 0) {
                var count = Math.floor(time / this.itemData.VALUE);
                maxCount = count + 1;
            } else {
                minCount = 0;
                maxCount = 0;
            }
        }

        this.curSelect--;
        if(this.curSelect < minCount) {
            this.curSelect = maxCount;
        } else if(this.curSelect > maxCount) {
            this.curSelect = minCount;
        }
        this.Refresh();
    }

    RightBtn() {
        if(this.data['popup'].IsExit || !this.data['popup'].IsInit) {
            return;
        }

        var minCount = 1;
        var maxCount = 99;
        var time = TimeManager.GetTimeCompare(this.data['time']);
        if(this.itemData != null) {
            if(time > 0) {
                var count = Math.floor(time / this.itemData.VALUE);
                maxCount = count + 1;
            } else {
                minCount = 0;
                maxCount = 0;
            }
        }

        this.curSelect++;
        if(this.curSelect > maxCount) {
            this.curSelect = minCount;
        } else if(this.curSelect < minCount) {
            this.curSelect = maxCount;
        }
        this.Refresh();
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

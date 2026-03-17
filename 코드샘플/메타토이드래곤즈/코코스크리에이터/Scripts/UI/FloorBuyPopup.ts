
import { _decorator, Label, Sprite, Button, EventHandler } from 'cc';
import { DefineResourceTable } from '../Data/ItemTable';
import { StringTable } from '../Data/StringTable';
import { TableManager } from '../Data/TableManager';
import { NetworkManager } from '../NetworkManager';
import { DataPriceChange, StringBuilder } from '../Tools/SandboxTools';
import { Popup } from './Common/Popup';
import { PopupManager } from './Common/PopupManager';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = FloorBuyPopup
 * DateTime = Tue Feb 08 2022 18:04:26 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = FloorBuyPopup.ts
 * FileBasenameNoExtension = FloorBuyPopup
 * URL = db://assets/Scripts/UI/Landmark/FloorBuyPopup.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
@ccclass('FloorBuyPopup')
export class FloorBuyPopup extends Popup {
    private isInit: boolean = false;
    private titleLabel: Label = null;
    private contentLabel1: Label = null;
    private contentLabel2: Label = null;
    private priceLabel: Label = null;
    private priceIcon: Sprite = null;
    private closeBtn: Button = null;
    private activeBtn: Button = null;

    private typeTable: DefineResourceTable = null;
    private stringTable: StringTable = null;

    private isExit: boolean = false;

    OnLoad() {
        super.OnLoad();
        let bodyNode = this.node.getChildByName('body');
        if(bodyNode != null) {
            let ContentNode = bodyNode.getChildByName('Content');
            if(ContentNode != null) {
                let contentLabel_001 = bodyNode.getChildByName('contentLabel_001');
                if(contentLabel_001 != null) {
                    this.contentLabel1 = contentLabel_001.getComponent(Label);
                }
                let contentLabel_002 = ContentNode.getChildByName('contentLabel_002');
                if(contentLabel_002 != null) {
                    this.contentLabel2 = contentLabel_002.getComponent(Label);
                }
            }
            let TopNode = bodyNode.getChildByName('Top');
            if(TopNode != null) {
                let titleLabel = TopNode.getChildByName('titleLabel');
                if(titleLabel != null) {
                    this.titleLabel = titleLabel.getComponent(Label);
                }
                let btnClose = TopNode.getChildByName('btnClose');
                if(btnClose != null) {
                    this.closeBtn = btnClose.getComponent(Button);
                    if(this.closeBtn != null) {
                        let newEvent = new EventHandler();
                        newEvent.target = this.node;
                        newEvent.component = "FloorBuyPopup"
                        newEvent.handler = "CloseBtn";
                        this.closeBtn.clickEvents.push(newEvent);
                    }
                }
            }
            let BotNode = bodyNode.getChildByName('Bot');
            if(BotNode != null) {
                let btnOkNode = BotNode.getChildByName('btnOk');
                if(btnOkNode != null) {
                    this.activeBtn = btnOkNode.getComponent(Button);
                    if(this.activeBtn != null) {
                        let newEvent = new EventHandler();
                        newEvent.target = this.node;
                        newEvent.component = "FloorBuyPopup"
                        newEvent.handler = "ActiveBtn";
                        this.activeBtn.clickEvents.push(newEvent);
                    }
                    let layout = btnOkNode.getChildByName('layout');
                    if(layout != null) {
                        let layout_h = layout.getChildByName('layout_h');
                        if(layout_h != null) {
                            let sprCost = layout_h.getChildByName('sprCost');
                            if(sprCost != null) {
                                this.priceIcon = sprCost.getComponent(Sprite);
                            }
                            let labelCost = layout_h.getChildByName('labelCost');
                            if(labelCost != null) {
                                this.priceLabel = labelCost.getComponent(Label);
                            }
                        }
                    }
                }
            }
        }

        this.typeTable = TableManager.GetTable(DefineResourceTable.Name);
        this.stringTable = TableManager.GetTable(StringTable.Name);
    }

    ActiveBtn() {
        if(!this.isInit || this.isExit) {
            return;
        }

        this.isExit = true;
        NetworkManager.Send('building/floor', {}, (jsonData) => {
            if(jsonData['err'] == 0) {
                this.isExit = true;
                PopupManager.ClosePopup(this);
            } else {
                this.isExit = false;
            }
        });
    }

    CloseBtn() {
        if(!this.isInit || this.isExit) {
            return;
        }
        this.isExit = true;

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
    }

    /**
     * @data 내부 구성 및 표기
     * @floor 구매 층
     * @type 제화 타입
     * @count 가격
     */
    private InData(): boolean {
        if(this.popupData == null || this.popupData['type'] == undefined || this.popupData['floor'] == undefined  || this.popupData['count'] == undefined) {
            return false;
        }
        if(this.titleLabel == null || this.contentLabel1 == null || this.contentLabel2 == null || this.priceLabel == null || this.activeBtn == null || this.closeBtn == null || this.priceIcon == null) {
            return false;
        }
        if(this.typeTable == null || this.stringTable == null) {
            return false;
        }

        if(this.titleLabel != null) {
            let data = this.stringTable.Get(100000260);
            if(data != null) {
                this.titleLabel.string = data.TEXT;
            }
        }

        if(this.contentLabel1 != null) {
            let data = this.stringTable.Get(100000101);
            if(data != null) {
                this.contentLabel1.string = StringBuilder(data.TEXT, this.popupData['floor']);
            }
        }
        if(this.contentLabel2 != null) {
            let data = this.stringTable.Get(100000100);
            if(data != null) {
                this.contentLabel2.string = data.TEXT;
            }
        }

        DataPriceChange({type:this.popupData['type'], type_value:0, count:this.popupData['count']}, this.priceIcon, this.priceLabel);
        this.isInit = true;
        return true;
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

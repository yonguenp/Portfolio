
import { _decorator, Label, Sprite, Button, EventHandler } from 'cc';
import { DefineResourceTable } from '../Data/ItemTable';
import { StringTable } from '../Data/StringTable';
import { TableManager } from '../Data/TableManager';
import { GameManager } from '../GameManager';
import { NetworkManager } from '../NetworkManager';
import { ResourceManager } from '../ResourceManager';
import { DataPriceChange } from '../Tools/SandboxTools';
import { eGoodType } from '../User/User';
import { Popup } from './Common/Popup';
import { PopupManager } from './Common/PopupManager';
const { ccclass } = _decorator;

/**
 * Predefined variables
 * Name = AccelerationCashPopup
 * DateTime = Thu Jan 27 2022 16:01:59 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = AccelerationCashPopup.ts
 * FileBasenameNoExtension = AccelerationCashPopup
 * URL = db://assets/Scripts/UI/AccelerationCashPopup.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
@ccclass('AccelerationCashPopup')
export class AccelerationCashPopup extends Popup {
    private isInit: boolean = false;
    private titleLabel: Label = null;
    private contentLabel: Label = null;
    private priceLabel: Label = null;
    private priceIcon: Sprite = null;
    private closeBtn: Button = null;
    private activeBtn: Button = null;

    private resource: ResourceManager = null;
    private typeTable: DefineResourceTable = null;
    private stringTable: StringTable = null;

    OnLoad() {
        let bodyNode = this.node.getChildByName('body');
        if(bodyNode != null) {
            let ContentNode = bodyNode.getChildByName('Content');
            if(ContentNode != null) {
                let contentLabel = ContentNode.getChildByName('contentLabel');
                if(contentLabel != null) {
                    this.contentLabel = contentLabel.getComponent(Label);
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
                        newEvent.component = "AccelerationCashPopup"
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
                        newEvent.component = "AccelerationCashPopup"
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
        this.resource = GameManager.GetManager(ResourceManager.Name);
    }

    ActiveBtn() {
        if(this.popupData['popup'].IsExit || !this.popupData['popup'].IsInit) {
            return;
        }
        this.popupData['popup'].IsExit = true;
        switch(this.popupData['type']) {
            case eGoodType.ITEM: {
                NetworkManager.Send('building/haste', {type:this.popupData['acceleration_type'],tag:this.popupData['tag'], item:this.popupData['type_value'], count:this.popupData['count'], platform:this.popupData['platform']}, (jsonData) => {
                    PopupManager.ClosePopup(this.popupData['popup']);
                    PopupManager.ClosePopup(this);
                    PopupManager.ForceUpdate()
                });
            } break;
            case eGoodType.CASH: {
                NetworkManager.Send('building/haste', {type:this.popupData['acceleration_type'],tag:this.popupData['tag'], item:this.popupData['type_value'], count:this.popupData['count'], platform:this.popupData['platform']}, (jsonData) => {
                    PopupManager.ClosePopup(this.popupData['popup']);
                    PopupManager.ClosePopup(this);
                    PopupManager.ForceUpdate()
                });
            } break;
        }
    }

    CloseBtn() {
        if(this.popupData['popup'].IsExit || !this.popupData['popup'].IsInit) {
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
    }

    /**
     * @data 내부 구성 및 표기
     * @acceleration_type 가속 타입
     * @type 제화 타입
     * @type_value 제화 param
     * @count 가격
     * @time 남은 시간
     * @popup 대상 팝업
     */
    private InData(): boolean {
        if(this.popupData == null || this.popupData['type'] == undefined || this.popupData['count'] == undefined) {
            return false;
        }
        if(this.titleLabel == null || this.contentLabel == null || this.priceLabel == null || this.activeBtn == null || this.closeBtn == null || this.priceIcon == null) {
            return false;
        }
        if(this.typeTable == null || this.stringTable == null) {
            return false;
        }

        if(this.titleLabel != null) {
            let data = this.stringTable.Get(100000087);
            if(data != null) {
                this.titleLabel.string = data.TEXT;
            }
        }

        DataPriceChange({type:this.popupData['type'], type_value:this.popupData['type_value'], count:this.popupData['count']}, this.priceIcon, this.priceLabel);

        return true;
    }
}
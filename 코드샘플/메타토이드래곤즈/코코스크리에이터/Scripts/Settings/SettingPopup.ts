
import { _decorator, Component, Node, Toggle } from 'cc';
import { Popup } from '../UI/Common/Popup';
import { PopupExtension } from '../UI/Common/PopupExtension';
import { SettingAccountLayer } from './SettingAccountLayer';
import { SettingGameLayer } from './SettingGameLayer';
import { SettingLanguageLayer } from './SettingLanguageLayer';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = SettingPopup
 * DateTime = Tue Apr 26 2022 18:08:38 GMT+0900 (대한민국 표준시)
 * Author = blacktopaz
 * FileBasename = SettingPopup.ts
 * FileBasenameNoExtension = SettingPopup
 * URL = db://assets/Scripts/Settings/SettingPopup.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */

// 세팅 탭 종류
 enum SettingTabType {
    GAME = 1,
    LANGUAGE,
    ACCOUNT
}
 
@ccclass('SettingPopup')
export class SettingPopup extends PopupExtension {

    // 현재 선택한 탭
    curTabType: SettingTabType = SettingTabType.GAME;

    // 탭 공통
    @property({ group: { name: 'Tab Button'}, type: Toggle })
    tabButtonGame: Toggle = null;
    @property({ group: { name: 'Tab Button'}, type: Toggle })
    tabButtonLanguage: Toggle = null;
    @property({ group: { name: 'Tab Button'}, type: Toggle })
    tabButtonAccount: Toggle = null;

    @property({ group: { name: 'Tab Layer'}, type: Node })
    tabLayerGame: Node = null;
    @property({ group: { name: 'Tab Layer'}, type: Node })
    tabLayerLanguage: Node = null;
    @property({ group: { name: 'Tab Layer'}, type: Node })
    tabLayerAccount: Node = null;

    @property(Number)
    defaultTap : number = 0

    Init() {
        if (this.tabLayerGame == null || this.tabLayerLanguage == null || this.tabLayerAccount == null) {return;}

        super.Init();

        this.tabLayerGame.getComponent(SettingGameLayer).Init();
        this.tabLayerLanguage.getComponent(SettingLanguageLayer).Init();
        this.tabLayerAccount.getComponent(SettingAccountLayer).Init();
    }

    onClickTapButton(event:Event, customEventData) {
        if (this.tabButtonGame == null || this.tabButtonLanguage == null || this.tabButtonAccount == null) {return;}
        if (this.tabLayerGame == null || this.tabLayerLanguage == null || this.tabLayerAccount == null) {return;}

        this.tabButtonGame.interactable = true;
        this.tabButtonLanguage.interactable = true;
        this.tabButtonAccount.interactable = true;

        this.tabLayerGame.active = false;
        this.tabLayerLanguage.active = false;
        this.tabLayerAccount.active = false;
        
        switch(customEventData) {
            case "1":
                this.curTabType = SettingTabType.GAME;
                this.tabButtonGame.interactable = false;
                this.tabLayerGame.active = true;
                break;
            case "2":
                this.curTabType = SettingTabType.LANGUAGE;
                this.tabButtonLanguage.interactable = false;
                this.tabLayerLanguage.active = true;
                break;
            case "3":
                this.curTabType = SettingTabType.ACCOUNT;
                this.tabButtonAccount.interactable = false;
                this.tabLayerAccount.active = true;
                break;
        }
    }

    forceUpdate() {

    }

    OnClickCloseButton() {
        this.popup?.ClosePopup();
    }
}

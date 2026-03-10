
import { _decorator, Component, Toggle } from 'cc';
import { SettingEvent } from './SettingManager';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = SettingLanguageLayer
 * DateTime = Wed Apr 27 2022 11:28:09 GMT+0900 (대한민국 표준시)
 * Author = blacktopaz
 * FileBasename = SettingLanguageLayer.ts
 * FileBasenameNoExtension = SettingLanguageLayer
 * URL = db://assets/Scripts/Settings/SettingLanguageLayer.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('SettingLanguageLayer')
export class SettingLanguageLayer extends Component {
    
    // 언어 관련
    @property({ group: { name: 'Language'}, type: Toggle })
    korToggle: Toggle = null;
    @property({ group: { name: 'Language'}, type: Toggle })
    EngToggle: Toggle = null;

    Init() {
        // 언어 관련 Init
        this.korToggle.isChecked = false;
        this.EngToggle.isChecked = false;

        var LanguageData = localStorage.getItem('Setting_Language');
        if (LanguageData != null && LanguageData != undefined) {
            switch(LanguageData) {
                case 'kor':
                    this.korToggle.isChecked = true;
                    break;
                case 'eng':
                    this.EngToggle.isChecked = true;
                    break;
                default:
            }
        }
        else {
            switch (navigator.language) {
                case "ko":
                case "kor":
                case "ko-KR":
                    this.korToggle.isChecked = true;
                default:
                    this.EngToggle.isChecked = true;
            }
        }
    }

    OnChangeLanguageState(event, customEventData: string) {
        localStorage.setItem('Setting_Language', customEventData);

        SettingEvent.TriggerEvent({type:'Setting_Language', value:customEventData})
    }
}
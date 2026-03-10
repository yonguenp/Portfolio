
import { _decorator, Component, Node } from 'cc';
import { EventData } from 'sb';
import { SoundMixer } from '../SoundMixer';
import { EventManager } from '../Tools/EventManager';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = SettingManager
 * DateTime = Wed Apr 27 2022 12:21:16 GMT+0900 (대한민국 표준시)
 * Author = blacktopaz
 * FileBasename = SettingManager.ts
 * FileBasenameNoExtension = SettingManager
 * URL = db://assets/Scripts/Settings/SettingManager.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */

// 세팅 변경 시 이벤트 필요할때 사용
export class SettingEvent implements EventData {
    private static instance: SettingEvent = null;1
    private jsonData: any = null;
    public get Data(): any {
        return this.jsonData;
    }
    GetID(): string {
        return 'SettingEvent';
    }

    public static TriggerEvent(jsonData) {
        if(SettingEvent.instance == null) {
            SettingEvent.instance = new SettingEvent();
        }

        SettingEvent.instance.jsonData = jsonData;
        EventManager.TriggerEvent(SettingEvent.instance);
    }
    
}
 
export class SettingManager extends Component {
     // 인스턴스
     static instance: SettingManager = null;
     static get GetInstance() : Readonly <SettingManager> {
         if (SettingManager.instance == null){
            SettingManager.instance = new SettingManager();
         }
 
         return SettingManager.instance;
     }


     Init() {
        // 사운드 세팅
        var bgmData = JSON.parse(localStorage.getItem('Setting_BGM'));
        if(bgmData != null){
            SoundMixer.ChangeBGMVolumeState(bgmData.isOn, bgmData.volume);
        }
        else {
            SoundMixer.ChangeBGMVolumeState(true, 1.0);
        }

        var sfxData = JSON.parse(localStorage.getItem('Setting_SFX'));
        if(sfxData != null){
            SoundMixer.ChangeSFXVolumeState(sfxData.isOn, sfxData.volume);
        }
        else {
            SoundMixer.ChangeSFXVolumeState(true, 1.0);
        }
     }
}
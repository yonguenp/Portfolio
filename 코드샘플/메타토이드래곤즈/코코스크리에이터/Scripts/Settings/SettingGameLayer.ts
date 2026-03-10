
import { _decorator, Component, Node, Slider, Toggle, Button, sys, Label } from 'cc';
import { SoundMixer } from '../SoundMixer';
import { BattleSpeedValue, eBattleSpeed } from '../Tools/CommonEnum';
import { StringBuilder } from '../Tools/SandboxTools';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = SettingGameLayer
 * DateTime = Wed Apr 27 2022 11:27:43 GMT+0900 (대한민국 표준시)
 * Author = blacktopaz
 * FileBasename = SettingGameLayer.ts
 * FileBasenameNoExtension = SettingGameLayer
 * URL = db://assets/Scripts/Settings/SettingGameLayer.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('SettingGameLayer')
export class SettingGameLayer extends Component {
    

    // 사운드 관련
    @property({ group: { name: 'Sound'}, type: Toggle })
    BGMToggle: Toggle = null;
    @property({ group: { name: 'Sound'}, type: Slider })
    BGMSlider: Slider = null;
    @property({ group: { name: 'Sound'}, type: Toggle })
    SFXToggle: Toggle = null;
    @property({ group: { name: 'Sound'}, type: Slider })
    SFXSlider: Slider = null;

    @property(Button)
    btnBSpeed : Button = null;

    @property(Button)
    btnBRState : Button = null;

    Init() {
        // 사운드 관련 Init
        var bgmData = JSON.parse(localStorage.getItem('Setting_BGM'));
        if (bgmData != null) {
            this.BGMSlider.progress = bgmData.volume;       // 순서 중요 -> 슬라이더먼저
            this.BGMToggle.isChecked = bgmData.isOn;
        }
        else {
            this.BGMToggle.isChecked = true;
            this.BGMSlider.progress = 1.0;
        }
        
        var sfxData = JSON.parse(localStorage.getItem('Setting_SFX'));
        if (sfxData != null) {
            this.SFXSlider.progress = sfxData.volume;       // 순서 중요 -> 슬라이더먼저
            this.SFXToggle.isChecked = sfxData.isOn;
        }
        else {
            this.SFXToggle.isChecked = true;
            this.SFXSlider.progress = 1.0;
        }

        this.btnBSpeed.getComponentInChildren(Label).string = StringBuilder("x {0}", BattleSpeedValue[sys.localStorage.getItem("BSpeed")]);
    }

    //#region 사운드 관련

    onClickBGMToggle() {
        if (this.BGMToggle == null || this.BGMSlider == null) {return;}

        SoundMixer.ChangeBGMVolumeState(this.BGMToggle.isChecked, this.BGMSlider.progress);

        this.SaveBGMLocalData();
    }

    onBGMValueChanged() {
        if (this.BGMToggle == null || this.BGMSlider == null) {return;}

        SoundMixer.ChangeBGMVolumeState(this.BGMToggle.isChecked, this.BGMSlider.progress);

        this.SaveBGMLocalData();
    }

    onClickSFXToggle() {
        if (this.SFXToggle == null || this.SFXSlider == null) {return;}

        SoundMixer.ChangeSFXVolumeState(this.SFXToggle.isChecked, this.SFXSlider.progress);

        this.SaveSFXLocalData();
    }

    onSFXValueChanged() {
        if (this.SFXToggle == null || this.SFXSlider == null) {return;}

        SoundMixer.ChangeSFXVolumeState(this.SFXToggle.isChecked, this.SFXSlider.progress);

        this.SaveSFXLocalData();
    }

    SaveBGMLocalData() {
        if (this.BGMToggle == null || this.BGMSlider == null) {return;}

        var soundData = {
            isOn: this.BGMToggle.isChecked,
            volume: this.BGMSlider.progress
        }

        var soundDataString = JSON.stringify(soundData);
        localStorage.setItem('Setting_BGM', soundDataString);
    }

    SaveSFXLocalData() {
        if (this.SFXToggle == null || this.SFXSlider == null) {return;}

        var soundData = {
            isOn: this.SFXToggle.isChecked,
            volume: this.SFXSlider.progress
        }

        var soundDataString = JSON.stringify(soundData);
        localStorage.setItem('Setting_SFX', soundDataString);
    }

    onClickBSpeed(event)
    {
        let bSpeed : number = sys.localStorage.getItem("BSpeed") != null ? Number(sys.localStorage.getItem("BSpeed")) : 0;
        bSpeed = Math.floor((bSpeed+1)%3);
        sys.localStorage.setItem("BSpeed", (bSpeed).toString())

        this.btnBSpeed.getComponentInChildren(Label).string = StringBuilder("x {0}", BattleSpeedValue[sys.localStorage.getItem("BSpeed")]);
    }

    //#endregion
}
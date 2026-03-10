
import { _decorator, Component, Node, AudioClip } from 'cc';
import { IBGMChange } from 'sb';
import { SoundMixer, SOUND_TYPE } from '../SoundMixer';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = BGMChange
 * DateTime = Thu May 12 2022 14:13:27 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = BGMChange.ts
 * FileBasenameNoExtension = BGMChange
 * URL = db://assets/Scripts/Settings/BGMChange.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */

@ccclass('BGMData')
class BGMData {
    @property(AudioClip)
    private clip: AudioClip = null;
    public get Clip() {
        return this.clip;
    }
    @property
    private loop: boolean = true;
    public get Loop() {
        return this.loop;
    }
}
 
@ccclass('BGMChange')
export class BGMChange extends Component implements IBGMChange {
    @property(BGMData)
    clips: BGMData[] = []

    start () {
        SoundMixer.DestroyBGMClip();
        this.Change();
    }

    Change(): void {
        this.clips.forEach(element => {
            if(element == null || element.Clip == null) {
                return;
            }
            SoundMixer.PlayBGM(SOUND_TYPE.BGM, element.Clip, element.Loop);
        });
    }
    
    onDestroy() {
        SoundMixer.DestroyBGMClip();
    }
}

/**
 * [1] Class member could be defined like this.
 * [2] Use `property` decorator if your want the member to be serializable.
 * [3] Your initialization goes here.
 * [4] Your update function goes here.
 *
 * Learn more about scripting: https://docs.cocos.com/creator/3.4/manual/en/scripting/
 * Learn more about CCClass: https://docs.cocos.com/creator/3.4/manual/en/scripting/decorator.html
 * Learn more about life-cycle callbacks: https://docs.cocos.com/creator/3.4/manual/en/scripting/life-cycle-callbacks.html
 */

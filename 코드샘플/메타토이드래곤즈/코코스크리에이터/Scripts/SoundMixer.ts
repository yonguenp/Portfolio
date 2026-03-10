
import { _decorator, Node, AudioSource, AudioClip } from 'cc';
import { IManagerBase } from 'sb';
import { GameManager } from './GameManager';
import { ResourceManager, ResourcesType } from './ResourceManager';
const { ccclass, property } = _decorator;

class ClipInfo 
{
    clipNode: Node = null;
    soundType: CLIP_SOUND_TYPE = CLIP_SOUND_TYPE.NONE
}

export enum CLIP_SOUND_TYPE 
{
    NONE,
    BGM,
    SFX
}

export enum SOUND_TYPE
{
    BGM = 0,
    BGM_VILLAGE,
    BGM_WORLD,
    BGM_EXPLORE,
    BGM_INTRO,
    BGM_BATTLE_VICTORY,
    BGM_BATTLE_DEFEAT,

    BGM_MAX,

    FX = 1000,
    FX_NOTICE,
    FX_MOUSEOVER,
    FX_CONSTRIUCT,
    FX_BUTTON_OK,
    FX_BUTTON_CANCEL,
    FX_ERROR,
    FX_CELEBRATE,
    FX_BATCH,
    FX_TUTORIAL_START,
}

@ccclass('SoundMixer')
export class SoundMixer implements IManagerBase 
{
    GetManagerName(): string {
        return SoundMixer.Name
    }
    
    public static Name: string = "SoundMixer";
    private static instance : SoundMixer = null
    private static clipref :ClipInfo[] = []

    private static bgmVolume = 1.0;
    private static sfxVolume = 1.0;

    Update(deltaTime: number): void { }

    public static get Instance() 
    {
        if(SoundMixer.instance == null) 
        {
            return SoundMixer.instance = new SoundMixer();
        }
        return SoundMixer.instance;
    }

    static PlayBGM(type : SOUND_TYPE, tempClip: AudioClip = null, loop: boolean = true)
    {
        let name = ""
        let clip : AudioClip = null

        switch(type)
        {
            case SOUND_TYPE.BGM: if(tempClip == null) { return; } break;
            case SOUND_TYPE.BGM_INTRO : name = "Intro_01"; break;
            case SOUND_TYPE.BGM_WORLD : name = "Battle Highlands"; break;
            case SOUND_TYPE.BGM_VILLAGE : name = "bensound-littleidea"; break;
            case SOUND_TYPE.BGM_EXPLORE : name = "grove battle"; break;
            case SOUND_TYPE.BGM_BATTLE_VICTORY : name = "battle_victory_bgm_02"; break;
            case SOUND_TYPE.BGM_BATTLE_DEFEAT : name = "battle_defeat_bgm_02"; break;
            default: console.log("존재하지 않는 배경음악"); return;
        }

        if(tempClip != null) {
            clip = tempClip;
        } else {
            clip = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.BGM_AUDIOCLIP)[name];
        }

        if(clip != null)
        {
            SoundMixer.Instance.CreateClipNode(type, clip, loop)
        }
    }

    static PlayAudio() {

    }

    static PlaySoundFX(type : SOUND_TYPE, tempClip: AudioClip = null)
    {
        let name = ""
        let clip : AudioClip = null

        switch(type)
        {
            case SOUND_TYPE.FX: if(tempClip == null) { return; } break;
            case SOUND_TYPE.FX_NOTICE : name = "Ui Popup 9"; break;
            case SOUND_TYPE.FX_MOUSEOVER : name = "Ui Hover over button 5"; break;
            case SOUND_TYPE.FX_CONSTRIUCT : name = "288908__littlerobotsoundfactory__click-heavy-00"; break;
            case SOUND_TYPE.FX_BUTTON_OK : name = ""; break;
            case SOUND_TYPE.FX_BUTTON_CANCEL : name = ""; break;
            case SOUND_TYPE.FX_ERROR : name = ""; break;
            case SOUND_TYPE.FX_CELEBRATE : name = ""; break;
            case SOUND_TYPE.FX_BATCH : name = "structure_batch_fx"; break;
            case SOUND_TYPE.FX_TUTORIAL_START : name = "571502_kagateni_cute1_cut"; break;
            default: console.log("존재하지 않는 효과음"); return;
        }

        //효과음 중복 재생 검사
        //동시 재생 여부

        if(tempClip != null) {
            clip = tempClip;
        } else {
            let resourceManager = GameManager.GetManager<ResourceManager>(ResourceManager.Name);
            if(resourceManager != null){
                let loadCheck = resourceManager.LoadComplete;
                if(loadCheck){
                    clip = resourceManager.GetResource(ResourcesType.FX_AUDIOCLIP)[name];
                }
            }
        }

        if(clip != null)
        {
            SoundMixer.Instance.CreateClipNode(type, clip, false)
        }
    }

    private CreateClipNode(type : SOUND_TYPE, clip : AudioClip, isLoop : boolean)
    {
        const isBGM = type < SOUND_TYPE.BGM_MAX;

        let newNode = new Node()
        let source = newNode.addComponent(AudioSource)
        source.clip = clip;
        source.loop = isLoop;
        source.playOnAwake = true;
        source.volume = isBGM ? SoundMixer.bgmVolume : SoundMixer.sfxVolume;

        let newClipInfo = new ClipInfo();
        newClipInfo.clipNode = newNode;
        newClipInfo.soundType = isBGM ? CLIP_SOUND_TYPE.BGM : CLIP_SOUND_TYPE.SFX;

        SoundMixer.clipref.push(newClipInfo);

        newNode.on(AudioSource.EventType.ENDED, () => 
        {
            newNode.destroy();
            SoundMixer.clipref = SoundMixer.clipref.filter(info => info.clipNode != newNode);
        });

        source.play();
    }

    static EnableMouseClickSFX(target : Node)
    {
        target.on(Node.EventType.MOUSE_UP, ()=>{SoundMixer.PlaySoundFX(SOUND_TYPE.FX_MOUSEOVER)});
    }

    static DisableMouseClickSFX(target : Node)
    {
        target.off(Node.EventType.MOUSE_UP);
    }

    static DestroyBGMClip()
    {
        SoundMixer.clipref.forEach((element)=>
        {
            if(element.soundType < SOUND_TYPE.BGM_MAX) {
                element.clipNode.getComponent(AudioSource).stop();
                element.clipNode.destroy();
                element = null;
            }
        })
        SoundMixer.clipref = SoundMixer.clipref.filter(element => element.soundType > SOUND_TYPE.BGM_MAX);
    }

    static DestroyAllClip()
    {
        SoundMixer.clipref.forEach((element)=>
        {
            element.clipNode.getComponent(AudioSource).stop()
            element.clipNode.destroy()
            element = null;
        })
        SoundMixer.clipref = []
    }

    // BGM 볼륨 설정
    static ChangeBGMVolumeState(isOn:boolean, volume: number) 
    {
        let resultVolume = isOn ? volume : 0;

        SoundMixer.clipref.forEach((element)=>
        {
            if (element.soundType == CLIP_SOUND_TYPE.BGM) {
                element.clipNode.getComponent(AudioSource).volume = resultVolume;
            }
        })

        SoundMixer.bgmVolume = resultVolume;
    }

    // SFX 볼륨 설정
    static ChangeSFXVolumeState(isOn:boolean, volume: number) 
    {
        let resultVolume = isOn ? volume : 0;

        SoundMixer.clipref.forEach((element)=>
        {
            if (element.soundType == CLIP_SOUND_TYPE.SFX) {
                element.clipNode.getComponent(AudioSource).volume = resultVolume;
            }
        })

        SoundMixer.sfxVolume = resultVolume;
    }
}

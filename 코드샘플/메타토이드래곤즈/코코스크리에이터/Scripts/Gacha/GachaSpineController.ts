
import { _decorator, Component, Node, sp, Toggle, ToggleContainer } from 'cc';
import { EventListener } from 'sb';
import { EventManager } from '../Tools/EventManager';
import { GachaEvent } from './GachaEvent';
const { ccclass, property } = _decorator;

export enum eGachaSpineState
{
    IDLE,
    PLAY,
    IDLE_HIT,
    HIT,
    HIT_IDLE
}

let eGachaStateString = [
    "idle",
    "play",
    "idle_hit",
    "hit",
    "result"
]

@ccclass('GachaSpineController')
export class GachaSpineController extends Component implements EventListener<GachaEvent>
{
    @property({group : "Spine Ani", type : sp.Skeleton})
    skstart : sp.Skeleton;

    @property({group : "Spine Ani", type : sp.Skeleton})
    skpaint : sp.Skeleton;

    @property({group : "Spine Ani", type : sp.Skeleton})
    skxray : sp.Skeleton;

    @property({group : "Spine Ani", type : sp.Skeleton})
    sktable : sp.Skeleton;

    private state : eGachaSpineState = eGachaSpineState.IDLE;
    public get State() : eGachaSpineState
    {
        return this.state;
    }

    onLoad()
    {
        EventManager.AddEvent(this);
    } 

    GetID(): string 
    {
        return "GachaEvent";
    }

    OnEvent(eventType: GachaEvent): void 
    {
        if(eventType.Data['state'] == null)
            return;

        switch(eventType.Data['state'])
        {
            case "Main_Dragon" : 
                this.State = eGachaSpineState.IDLE;
                break;

            case "Dragon_Play" :
                this.State = eGachaSpineState.PLAY;
                break;

            case "Dragon_Idle_Hit" : 
                this.State = eGachaSpineState.IDLE_HIT;
                break;

            case "Dragon_Hit" :
                this.State = eGachaSpineState.HIT;
                break;

            case "Dragon_Hit_Idle" :
                this.State = eGachaSpineState.HIT_IDLE;
                break;

            default :
                break;
        }
    }

    public set State(value : eGachaSpineState)
    {
        this.state = value;

        switch(this.state)
        {
            case eGachaSpineState.IDLE :
                this.IdleGacha();
                break;

            case eGachaSpineState.PLAY :
                this.PlayGacha();
                break;

            case eGachaSpineState.IDLE_HIT :
                this.IdleHitGacha();
                break;

            case eGachaSpineState.HIT :
                this.HitGacha();
                break;

            case eGachaSpineState.HIT_IDLE :
                this.AfterHitOnlyTableResult();
                break;
        }
    }

    Animation(AniName : string)
    {
        
        this.skstart.animation =  this.skpaint.animation =  this.sktable.animation = this.skxray.animation = AniName;
    }

    OnlyTableAnimation(AniName : string)
    {
        this.sktable.animation = AniName;
    }

    Loop(value : boolean)
    {
        this.skstart.loop =  this.skpaint.loop =  this.sktable.loop = this.skxray.loop = value;
    }

    IdleGacha()
    {
        this.Loop(true);
        this.setCompleteListener(null);
        this.Animation(eGachaStateString[eGachaSpineState.IDLE]);
    }

    PlayGacha()
    {
        this.Loop(false);
        this.Animation(eGachaStateString[eGachaSpineState.PLAY]);
        this.setCompleteListener(() =>
        {
            GachaEvent.TriggerEvent("Dragon_Idle_Hit");
        })
    }

    IdleHitGacha()
    {
        this.Loop(true);
        this.setCompleteListener(null);
        this.Animation(eGachaStateString[eGachaSpineState.IDLE_HIT]);
    }

    HitGacha()
    {
        this.Loop(false);
        //this.setMix(eGachaStateString[eGachaSpineState.IDLE_HIT], eGachaStateString[eGachaSpineState.HIT]);
        this.Animation(eGachaStateString[eGachaSpineState.HIT]);
        this.setCompleteListener(() =>
        {
            GachaEvent.TriggerEvent("Dragon_Hit_Idle");
        })
    }

    AfterHitOnlyTableResult()
    {
        this.Loop(true);
        this.OnlyTableAnimation(eGachaStateString[eGachaSpineState.HIT_IDLE]);
        this.setCompleteListener(null)
    }

    setMix(originAni, nextAni : string)
    {
        this.skstart.setMix(originAni, nextAni, 0.5);
        this.skpaint.setMix(originAni, nextAni, 0.5);
        this.sktable.setMix(originAni, nextAni, 0.5);
        this.skxray.setMix(originAni, nextAni, 0.5);
    }

    setCompleteListener(cb : () => void)
    {
        this.skstart.setCompleteListener(cb);
        this.skpaint.setCompleteListener(cb);
        this.sktable.setCompleteListener(cb);
        this.skxray.setCompleteListener(cb);
    }



    onDestroy()
    {
        EventManager.RemoveEvent(this);
    }
}
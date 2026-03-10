
import { _decorator, Component, Node } from 'cc';
import { EventListener } from 'sb';
import { ShakeEffect } from '../ShakeEffect';
import { EventManager } from '../Tools/EventManager';
import { GachaEvent } from './GachaEvent';
const { ccclass, property } = _decorator;


/**
 * Predefined variables
 * Name = GachaEffect
 * DateTime = Mon Mar 14 2022 10:56:28 GMT+0900 (대한민국 표준시)
 * Author = blacktopaz
 * FileBasename = GachaEffect.ts
 * FileBasenameNoExtension = GachaEffect
 * URL = db://assets/Scripts/GachaEffect.ts
 *
 */
 
@ccclass('GachaEffect')
export class GachaEffect extends Component implements EventListener<GachaEvent>
{
    @property(Node)
    gachaEffectUICanvas: Node = null;
    
    @property(Node)
    resultScript: Node = null;             // 단일 결과 팝업

    @property(ShakeEffect)
    shakeComponent: ShakeEffect = null;

    @property(Node)
    private canvas : Node = null;

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
            case "Dragon_Play":
                this.Active(true);
            break;

            default :
            case "Dragon_Idle_Hit" : 
            case "Dragon_Hit": 
                this.Active(false);
            break;
        }
    }

    Active(act : boolean)
    {
        this.canvas.active = act;
        this.gachaEffectUICanvas.active = act;
    }

    OnClickSkipButton() 
    {
        // 스킵 누르면 바로 건너뛰게 변경
        if (this.resultScript != null) 
        {
            GachaEvent.TriggerEvent("Dragon_Hit");
        }
    }

    onDestroy()
    {
        EventManager.RemoveEvent(this);
    }
}
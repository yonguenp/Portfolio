
import { _decorator, Component, Node } from 'cc';
import { EventListener } from 'sb';
import { EventManager } from '../Tools/EventManager';
import { GachaEvent } from './GachaEvent';
const { ccclass, property } = _decorator;
 
@ccclass('GachaEffectResult')
export class GachaEffectResult extends Component implements EventListener<GachaEvent>
{
    @property(Node)
    private btnHit : Node = null;

    @property(Node)
    private canvas : Node = null;

    GetID(): string 
    {
        return "GachaEvent";
    }

    onLoad()
    {
        EventManager.AddEvent(this);
    }

    OnEvent(eventType: GachaEvent): void 
    {
        if(eventType.Data['state'] == null)
            return;

        switch(eventType.Data['state'])
        {
            case "Dragon_Idle_Hit" : 
                this.IdleHit();
                break;

            case "Dragon_Hit" : 
            default :
                this.canvas.active = false;
                break;
        }
    }

    IdleHit()
    {
        this.canvas.active = true;
    }

    onClickHit()
    {
        GachaEvent.TriggerEvent("Dragon_Hit");
    }

    disableHitButton()
    {

    }

    onDestroy()
    {
        EventManager.RemoveEvent(this);
    }
}

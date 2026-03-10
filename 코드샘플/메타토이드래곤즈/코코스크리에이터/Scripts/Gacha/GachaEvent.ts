import { EventData } from 'sb';
import { EventManager } from '../Tools/EventManager';

export class GachaEvent implements EventData
{
    private static instance: GachaEvent = null;
    private jsonData : { state : string, resultObj : any } = {state : "", resultObj : {}};
    public get Data(): { state : string, resultObj : any }
    {
        return this.jsonData;
    }

    GetID(): string 
    {
        return "GachaEvent";
    }

    public static SetResultData(obj : any)
    {
        GachaEvent.instance.jsonData.resultObj = obj;
    }

    public static TriggerEvent(state : string)
    {
        if(GachaEvent.instance == null)
        {
            GachaEvent.instance = new GachaEvent();
        }

        GachaEvent.instance.jsonData.state = state;
        EventManager.TriggerEvent(GachaEvent.instance);
    }
}
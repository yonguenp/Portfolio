
import { _decorator } from 'cc';
import { EventData, IStateData, IWaveBase } from 'sb';
import { DataManager } from '../Tools/DataManager';
import { EventManager } from '../Tools/EventManager';
import { StageData } from './Stage';

/**
 * Predefined variables
 * Name = IWaveBase
 * DateTime = Thu Feb 17 2022 18:14:31 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = IWaveBase.ts
 * FileBasenameNoExtension = IWaveBase
 * URL = db://assets/Scripts/Battle/IWaveBase.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
export class WaveEvent implements EventData {
    private static instance: WaveEvent = null;
    private jsonData: any = null;
    public get Data(): any {
        return this.jsonData;
    }
    GetID(): string {
        return 'WaveEvent';
    }

    public static TriggerEvent(jsonData) {
        if(WaveEvent.instance == null) {
            WaveEvent.instance = new WaveEvent();
        }

        WaveEvent.instance.jsonData = jsonData;
        EventManager.TriggerEvent(WaveEvent.instance);
    }
    
}

export abstract class WaveBase implements IStateData, IWaveBase {
    protected stageData: StageData = null;
    GetID(): string {
        return 'WaveBase';
    }
    OnEnter(): boolean {
        this.stageData = DataManager.GetData('BattleStageData');
        return true;
    }
    OnExit(): boolean {
        return true;
    }
    Update(dt: number): string {
        return "";
    }
    Start(): void {
    }
    End(): void {
    }

}
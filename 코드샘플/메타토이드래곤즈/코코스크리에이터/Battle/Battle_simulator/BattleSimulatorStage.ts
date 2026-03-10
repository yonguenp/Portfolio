
import { _decorator, Component, Node } from 'cc';
import { GameManager } from '../../GameManager';
import { NetworkManager } from '../../NetworkManager';
import { SceneManager } from '../../SceneManager';
import { SoundMixer, SOUND_TYPE } from '../../SoundMixer';
import { DataManager } from '../../Tools/DataManager';
import { EventManager } from '../../Tools/EventManager';
import { StringBuilder } from '../../Tools/SandboxTools';
import { eBattleState, StageData } from '../Stage';
import { WaveEvent } from '../WaveBase';
import { WaveStateMachine } from '../WaveStateMachine';
import { EventListener } from 'sb';
import { BattleSimulatorUI } from './BattleSimulatorUI';
import { BattleSimulatorStageData } from './BattleSimulatorStageData';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = BattleSimulatorStage
 * DateTime = Wed Apr 13 2022 14:05:21 GMT+0900 (대한민국 표준시)
 * Author = wonjun_gs
 * FileBasename = BattleSimulatorStage.ts
 * FileBasenameNoExtension = BattleSimulatorStage
 * URL = db://assets/Scripts/Battle/Battle_simulator/BattleSimulatorStage.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('BattleSimulatorStage')
export class BattleSimulatorStage extends Component implements EventListener<WaveEvent> {
    private stateMachine: WaveStateMachine = null;
    private gameManager: GameManager = null;
    private speed: number = 1;
    private stageData:BattleSimulatorStageData = null;
    private isInit = false;
    private isDataInit = false;
    private pause = false;

    get Pause() : boolean
    {
        return this.pause
    }
    set Pause(value)
    {
        this.pause = value
    }

    onLoad () {
    }

    start () {
        if(!this.isInit) {
            this.Init();
        }
    }

    public Init() {
        this.gameManager = GameManager.Instance;
        EventManager.AddEvent(this);
        if(this.stateMachine == null) {
            this.stateMachine = new WaveStateMachine();
            this.stateMachine.StateInit();
        }
        if(this.stageData == null) {
            this.stageData = new BattleSimulatorStageData();
            DataManager.AddData('BattleStageData', this.stageData);
        }
        SoundMixer.EnableMouseClickSFX(this.node);
        NetworkManager.Instance.UIUpdater = null;
        this.isInit = true;
    }

    onDestroy() {
        NetworkManager.Instance.UIUpdater = null;
        EventManager.RemoveEvent(this);
        SoundMixer.DisableMouseClickSFX(this.node);
        DataManager.DelData('BattleStageData');
        this.stageData.Delete();
        delete this.stageData;
        this.stageData = null;
    }

    update (deltaTime: number) {
        if(!this.isInit || !this.isDataInit) {
            return;
        }
        // this.gameManager.Update(deltaTime * this.speed);

        if(!this.pause) {
            this.stageData.UpdateCloud(deltaTime * this.stageData.StageSpeed);
            switch(this.stageData.BattleState) {
                case eBattleState.Playing: {
                    this.stateMachine.Update(deltaTime * this.stageData.StageSpeed);
                    BattleSimulatorUI.UIUpdate();
                } break;
                case eBattleState.Win: {//this.stageData.Rewards에 리워드 정보 있음 { star:스타 갯수, rewards:아이템들{type:rewardType,no:번호(아이템 등),count:갯수} }.
                    this.pause = true;
                    DataManager.AddData("StageResultData", this.stageData.Rewards);
                    SoundMixer.DestroyAllClip();
                    SceneManager.SceneChange("battleReward");
                } break;
                case eBattleState.Lose: {//패배시
                    DataManager.AddData("StageResultData", this.stageData.Rewards);
                    SoundMixer.DestroyAllClip();
                    SceneManager.SceneChange("battleReward");
                    this.pause = true;
                } break;
            }
        }
    }

    GetID(): string {
        return 'WaveEvent';
    }

    OnEvent(eventType: WaveEvent): void {
        if(eventType.Data['state'] == undefined) {
            return;
        }

        switch(eventType.Data['state']) {
            case 'Init': {
                this.stageData.StageSpeed = this.speed;
                this.stageData.Init(eventType.Data['mapNode'], eventType.Data['stageData']);
                //BattleSimulatorUI.init(this.stageData, eventType.Data['stageData']);
                WaveEvent.TriggerEvent({
                    state: 'WaveMove_simulator'
                });
                this.isDataInit = true;
            } break;
            case 'datas': {
                this.stageData.SetData(eventType.Data);
            } break;
            case 'WaveIdle': {
                this.stateMachine.Change(eventType.Data['state']);
            } break;
            case 'WaveMove': {
                BattleSimulatorUI.SetWaveLabel(StringBuilder("{0}/{1}", this.stageData.CurWave, this.stageData.MaxWave))
                this.stateMachine.Change(eventType.Data['state']);
            } break;
            case 'WaveMove_simulator': {
                BattleSimulatorUI.SetWaveLabel(StringBuilder("{0}/{1}", this.stageData.CurWave, this.stageData.MaxWave))
                this.stateMachine.Change(eventType.Data['state']);
            } break;
            case 'WaveBattle': {
                this.stateMachine.Change(eventType.Data['state']);
            } break;
            case 'WaveBattle_simulator': {
                this.stateMachine.Change(eventType.Data['state']);
            } break;
            case 'WaveEnd': {
                this.stateMachine.Change(eventType.Data['state']);
            } break;
            case 'WaveClear' :{
                this.stageData.ClearField();
            }
        }
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

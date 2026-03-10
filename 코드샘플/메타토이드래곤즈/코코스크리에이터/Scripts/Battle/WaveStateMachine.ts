
import { _decorator } from 'cc';
import { ObjectCheck } from '../Tools/SandboxTools';
import { SimpleStateMachine } from '../Tools/StateMachine/SimpleStateMachine';
import { WaveBase } from './WaveBase';
import { WaveBattle } from './WaveBattle';
import { WaveEnd } from './WaveEnd';
import { WaveIdle } from './WaveIdle';
import { WaveMove } from './WaveMove';

/**
 * Predefined variables
 * Name = WaveStateMachine
 * DateTime = Thu Feb 17 2022 18:17:52 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = WaveStateMachine.ts
 * FileBasenameNoExtension = WaveStateMachine
 * URL = db://assets/Scripts/Battle/WaveStateMachine.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */

export class WaveStateMachine extends SimpleStateMachine<WaveBase> {
    Change(type: string): boolean {
        return this.ChangeState(this.GetState(type));
    }

    ChangeState(state: WaveBase): boolean {
        if (state == null) {
            return false;
        }
        if (this.curState == null) {
            if (state.OnEnter()) {
                this.curState = state;
                state.Start();
                return true;
            }
        }
        else {
            if (this.curState.OnExit() && state.OnEnter()) {
                this.curState.End();
                this.curState = state;
                state.Start();
                return true;
            }
        }

        return false;
    }

    GetState(type: string): WaveBase {
        if (ObjectCheck(this.states, type)) {
            return this.states[type];
        }
        return null;
    }

    StateInit() {
        this.states = {};
        this.AddState(new WaveIdle());
        this.AddState(new WaveMove());
        this.AddState(new WaveBattle());
        this.AddState(new WaveEnd());
    }

    Update(dt: number): void {
        if(this.curState == null) {
            return;
        }
        var stateName = this.curState.Update(dt);
        if(stateName != "" && this.Change(stateName)) {//상태 전환 성공
        }
    }
}

/**
 * [1] Class member could be defined like this.
 * [2] Use `property` decorator if your want the member to be serializable.
 * [3] Your initialization goes here.
 * [4] Your update function goes here.
 *
 * Learn more about scripting: https://docs.cocos.com/creator/3.3/manual/en/scripting/
 * Learn more about CCClass: https://docs.cocos.com/creator/3.3/manual/en/scripting/ccclass.html
 * Learn more about life-cycle callbacks: https://docs.cocos.com/creator/3.3/manual/en/scripting/life-cycle-callbacks.html
 */

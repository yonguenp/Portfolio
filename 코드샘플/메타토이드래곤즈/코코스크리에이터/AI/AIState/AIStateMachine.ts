
import { _decorator } from 'cc';
import { WorldAIDragon } from '../../Character/WorldAIDragon';
import { AIState, AIStateElevator, AIStateEscalator, AIStateIdle, AIStateMove } from './AIState';
import { SimpleStateMachine } from '../../Tools/StateMachine/SimpleStateMachine';

/**
 * Predefined variables
 * Name = AIStateMachine
 * DateTime = Wed Dec 29 2021 21:40:16 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = AIStateMachine.ts
 * FileBasenameNoExtension = AIStateMachine
 * URL = db://assets/Scripts/Tools/StateMachine/AIStateMachine.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 

export class AIStateMachine extends SimpleStateMachine<AIState> {
    targetDragon: WorldAIDragon = null;
    states: {};

    Set(dragon: WorldAIDragon): void {
        this.targetDragon = dragon;
    }

    ChangeState(state: AIState): boolean {
        if (state == null) {
            return false;
        }

        state.Set({
            dragon: this.targetDragon
        });
        if (this.curState == null) {
            if (state.OnEnter()) {
                this.curState = state;
                return true;
            }
        }
        else {
            if (this.curState.OnExit() && state.OnEnter()) {
                this.curState = state;
                return true;
            }
        }

        return false;
    }

    Update(dt): boolean {
        if (this.curState == null) {
            return false;
        }

        return this.curState.Update(dt);
    }

    StateInit() {
        if (this.states == undefined) this.states = {};
        this.AddState(new AIStateIdle());
        this.AddState(new AIStateMove());
        this.AddState(new AIStateEscalator());
        this.AddState(new AIStateElevator());
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

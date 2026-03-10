
import { _decorator } from 'cc';
import { Character } from '../Character/Dragon';
import { SimpleStateMachine } from '../Tools/StateMachine/SimpleStateMachine';
import { BattleState, BattleStateIdle, BattleStatePosition } from './BattleAIDragon';

/**
 * Predefined variables
 * Name = BattleAIStateMachine
 * DateTime = Tue Feb 22 2022 16:41:15 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = BattleAIStateMachine.ts
 * FileBasenameNoExtension = BattleAIStateMachine
 * URL = db://assets/Scripts/Battle/BattleAIStateMachine.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
export class BattleAIStateMachine extends SimpleStateMachine<BattleState> {
    targetDragon: Character = null;
    states: {};

    Set(dragon: Character): void {
        this.targetDragon = dragon;
    }

    ChangeState(state: BattleState): boolean {
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
        this.AddState(new BattleStateIdle());
        this.AddState(new BattleStatePosition());
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

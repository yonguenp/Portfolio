
import { _decorator } from 'cc';
import { WorldAIElevator } from '../../Character/WorldAIElevator';
import { SimpleStateMachine } from '../../Tools/StateMachine/SimpleStateMachine';
import { AIElevatorIdle, AIElevatorMoveDown, AIElevatorState, AIElevatorMoveUp, AIElevatorOpen, AIElevatorClose, AIElevatorExitIdle, AIElevatorInIdle } from './ElevatorState';

/**
 * Predefined variables
 * Name = ElevatorMachine
 * DateTime = Fri Jan 14 2022 17:37:46 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = ElevatorMachine.ts
 * FileBasenameNoExtension = ElevatorMachine
 * URL = db://assets/Scripts/AI/AIState/ElevatorMachine.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
export class AIElevatorMachine extends SimpleStateMachine<AIElevatorState> {
    elevator: WorldAIElevator = null;
    states: {};

    Set(elevator: WorldAIElevator): void {
        this.elevator = elevator;
    }

    ChangeState(state: AIElevatorState): boolean {
        if (state == null) {
            return false;
        }

        state.Set(this.elevator);
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

    Update(dt): string {
        if (this.curState == null) {
            return AIElevatorIdle.ID;
        }

        return this.curState.Update(dt);
    }

    StateInit() {
        if (this.states == undefined) this.states = {};
        this.AddState(new AIElevatorIdle());
        this.AddState(new AIElevatorOpen());
        this.AddState(new AIElevatorExitIdle());
        this.AddState(new AIElevatorInIdle());
        this.AddState(new AIElevatorClose());
        this.AddState(new AIElevatorMoveUp());
        this.AddState(new AIElevatorMoveDown());
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

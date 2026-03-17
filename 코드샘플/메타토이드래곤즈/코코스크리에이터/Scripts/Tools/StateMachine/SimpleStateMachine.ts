
import { _decorator } from 'cc';
import { IStateData, IStateMachine } from 'sb';
import { ObjectCheck } from '../SandboxTools';

export class SimpleStateMachine<T extends IStateData> implements IStateMachine {
    curState: T = null;
    states: {} = null;

    AddState(state: T): boolean {
        const stateType = state.GetID();

        if (ObjectCheck(this.states, stateType))
        {
            return false;
        }

        this.states[stateType] = state;
        return true;
    }

    Change(type: string): boolean {
        return this.ChangeState(this.GetState(type));
    }

    ChangeState(state: T): boolean {
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

    GetState(type: string): T {
        if (ObjectCheck(this.states, type)) {
            return this.states[type];
        }
        return null;
    }

    StateInit() {

    }
}
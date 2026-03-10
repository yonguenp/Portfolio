
import { _decorator } from 'cc';
import { IStateData, IStateMachine } from 'sb';
import { ObjectCheck } from '../SandboxTools';

export class MultiStateMachine<T extends IStateData> implements IStateMachine {
    curState: T[] = [];
    states: {};

    AddState(state: T): boolean {
        const stateType = typeof(state);

        if (ObjectCheck(this.states, stateType))
        {
            return false;
        }

        this.states[stateType] = state;
        return true;
    }

    Push(type: string): boolean {
        return this.PushState(this.GetState(type));
    }

    PushState(state: T): boolean {
        if (state == null) {
            return false;
        }

        if (state.OnEnter()) {
            this.curState.push(state);
            return true;
        }

        return false;
    }

    Pop(type: string): boolean {
        return this.PopState(this.GetState(type));
    }

    PopState(deleteState: T): boolean {
        if (deleteState == null) {
            return false;
        }

        if (deleteState.OnExit()) {
            this.curState = this.curState.filter((state) => state != deleteState);
            return true;
        }

        return false;
    }

    GetState(state: string): T {
        if (ObjectCheck(this.states, state)) {
            return this.states[state];
        }
        return null;
    }

    StateInit() {

    }
}
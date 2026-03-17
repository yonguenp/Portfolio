
import { _decorator } from 'cc';
import { ICharacter } from 'sb';
import { AIElevatorMachine } from '../AI/AIState/ElevatorMachine';
import { AIElevatorIdle } from '../AI/AIState/ElevatorState';
import { Elevator } from '../Map/Elevator';
import { ObjectCheck } from '../Tools/SandboxTools';
import { eElevatorInType, WorldAIDragon } from './WorldAIDragon';

/**
 * Predefined variables
 * Name = ElevatorAI
 * DateTime = Fri Jan 14 2022 17:51:06 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = ElevatorAI.ts
 * FileBasenameNoExtension = ElevatorAI
 * URL = db://assets/Scripts/Character/ElevatorAI.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */

class ElevatorInDragon {
    protected targetFloor: number = undefined;
    public get TargetFloor(): number { return this.targetFloor; }
    protected startFloor: number = undefined;
    public get StartFloor(): number { return this.startFloor; }
    protected dragon: WorldAIDragon = null;
    public get Dragon(): WorldAIDragon { return this.dragon; }

    Set(arr: any) {
        if (ObjectCheck(arr, 'targetFloor')) {
            this.targetFloor = arr['targetFloor'];
        }
        if (ObjectCheck(arr, 'startFloor')) {
            this.startFloor = arr['startFloor'];
        }
        if (ObjectCheck(arr, 'dragon')) {
            this.dragon = arr['dragon'];
        }
    }
}

export enum eElevatorType {
    Left = "left",
    Right = "right"
};

export enum eElevatorMoveType {
    None = 0,
    Up = 1,
    Down = 2,
};

export class WorldAIElevator implements ICharacter {
    protected type: string = "";
    protected moveType: number = eElevatorMoveType.None;
    public get MoveType(): number { return this.moveType; }
    public set MoveType(value: number) { this.moveType = value; }
    protected curFloor: number = 0;
    protected minFloor: number = 0;
    protected maxFloor: number = 0;
    protected targets: ElevatorInDragon[] = null;
    protected state: AIElevatorMachine = null;
    protected elevators: {} = null;
    protected openElevator: Elevator[] = null;
    protected container: Elevator = null;
    public get Container(): Elevator {
        return this.container;
    }

    init(type: string) {
        switch(type) {
            case eElevatorType.Left:
            case eElevatorType.Right:
                this.type = type;
                break;
        }
        this.targets = [];
        this.elevators = {};
        this.openElevator = [];
        this.moveType = eElevatorMoveType.None;
        this.state = new AIElevatorMachine();
        this.state.Set(this);
        this.state.StateInit();
        this.state.Change(AIElevatorIdle.ID);
    }

    public SetElevator(elevator: Elevator): void {
        const pos = elevator.GetPosition();

        if(elevator.IsContainer) {
            this.container = elevator;
        } else {
            if (!ObjectCheck(this.elevators, pos.y)) {
                this.elevators[pos.y] = elevator;
                if(this.minFloor >= pos.y) {
                    this.minFloor = pos.y;
                }
                if(this.maxFloor <= pos.y) {
                    this.maxFloor = pos.y;
                }
            }
        }
    }

    GetFloor(): number {
        return this.curFloor;
    }

    Set(element: any) {
        if (element == null) {
            return;
        }

        let data = new ElevatorInDragon();
        data.Set(element);
        this.targets.push(data);
    }

    Unset() {
        this.targets = this.targets.filter((target) => target.Dragon.ElevatorIn != eElevatorInType.Exit);
    }

    GetTargets(): ElevatorInDragon[] {
        return this.targets;
    }

    GetState(): string {
        return this.state.curState.GetID();
    }

    FloorCheck() {
        switch(this.MoveType) {
            case eElevatorMoveType.Up:
                if(this.maxFloor <= this.curFloor) {
                    this.MoveType = eElevatorMoveType.Down;
                    return false;
                }
                break;
            case eElevatorMoveType.Down:
                if(this.minFloor >= this.curFloor) {
                    this.MoveType = eElevatorMoveType.Up;
                    return false;
                }
                break;
            default:
                return false;
        }
        return true;
    }

    FloorUp(): boolean {
        if(!this.FloorCheck()) {
            return false;
        }
        this.curFloor++;
        return true;
    }

    FloorDown(): boolean {
        if(!this.FloorCheck()) {
            return false;
        }
        this.curFloor--;
        return true;
    }

    GetCurElevator(): Elevator {
        return this.elevators[this.curFloor];
    }

    GetElevator(floor: number): Elevator {
        return this.elevators[floor];
    }

    ElevatorOpen() {
        let elevator = this.GetCurElevator();
        if(elevator != null) {
            // elevator.AnimationStart('elevator_open');
            this.openElevator.push(elevator);
        }
    }

    ElevatorClose() {
        this.openElevator.forEach(element => {
            // element.AnimationStart('elevator_close');
        });

        this.openElevator = [];
    }

    FloorEnd() {

    }

    Update(dt: number) {
        const state = this.state.Update(dt);
        if(state != "") {
            this.state.Change(state);
        }

        return;
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

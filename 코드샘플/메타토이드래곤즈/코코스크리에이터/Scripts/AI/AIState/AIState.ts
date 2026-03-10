
import { _decorator, Node, Animation, Vec3 } from 'cc';
import { IStateData } from 'sb';
import { Character, DragonDefaultSpeed } from '../../Character/Dragon';
import { eElevatorInType, WorldAIDragon } from '../../Character/WorldAIDragon';
import { eElevatorMoveType, WorldAIElevator } from '../../Character/WorldAIElevator';
import { GameManager } from '../../GameManager';
import { CellNode } from '../../Map/CellNode';
import { MapManager } from '../../Map/MapManager';
import { BezierCurveVec3, ObjectCheck, RandomFloat, RandomInt } from '../../Tools/SandboxTools';
import { SBController } from '../../Tools/SBController';
import { AIElevatorClose, AIElevatorIdle, AIElevatorOpen, AIElevatorExitIdle, AIElevatorInIdle } from './ElevatorState';
 
export abstract class AIState implements IStateData {
    protected target: Character = null;
    protected skipFrame: boolean = false;

    GetID() {
        return "AIState";
    }
    Set(arr: {}) {
        this.skipFrame = true;
    }

    OnEnter(): boolean {
        return true;
    }

    OnExit(): boolean {
        return true;
    }

    Update(dt: number): boolean {
        if(this.skipFrame) {
            this.skipFrame = false;
            return false;
        }
        return true;
    }
}
 
export class AIStateIdle extends AIState {
    static readonly ID: string = "AIStateIdle";
    protected target: WorldAIDragon = null;
    protected curCell: CellNode = null;
    protected isActive: boolean = false;
    protected curTime: number = 0;
    protected curSpeed: number = 1;
    protected isWalk: boolean = false;
    protected isRight: boolean = false;
    protected randX: number = null;

    GetID() {
        return AIStateIdle.ID;
    }
    Set(arr: {dragon: WorldAIDragon, curCell: CellNode}) {
        super.Set(arr);
        if (ObjectCheck(arr, "dragon")) {
            this.target = arr.dragon;
        }
        if (ObjectCheck(arr, "curCell")) {
            this.curCell = arr.curCell;
        }
    }

    OnEnter(): boolean {
        if (this.isActive) {
            return false;
        }

        var isEnter = true;
        this.target.AnimationStart('idle');
        this.curTime = 0;
        this.skipFrame = true;

        this.isActive = isEnter;
        return isEnter;
    }

    OnExit(): boolean {
        if (!this.isActive) {
            return false;
        }

        var isExit = true;
        this.curCell = null;
        this.isActive = !isExit;
        this.curTime = 0;
        this.skipFrame = true;
        return isExit;
    }

    Update(dt: number): boolean {
        if (!super.Update(dt)) {
            return true;
        }

        var isAction = true;
        
        if (this.curCell != null) {
            this.curTime -= dt;
            if(this.curTime < 0) {
                this.isWalk = false;
                this.curTime = RandomFloat(1, 5);
                this.target.SetSpeed(RandomInt(50, 80));

                if (RandomInt(0,10) < 4) {
                    const targetPos = this.curCell.node.getWorldPosition();
                    const move = this.curCell.GetWorldMoveSize();
                    this.randX = RandomFloat(move.minX, move.maxX) - move.minX;
                    const curPos = this.target.GetWorldPosition();
                    if(curPos.x > targetPos.x + this.randX) {
                        this.isWalk = true;
                        this.isRight = false;
                        this.target.Controller.onEnterEvent('left');
                        this.target.Controller.onExitEvent('right');
                    } else if(curPos.x < targetPos.x + this.randX) {
                        this.isWalk = true;
                        this.isRight = true;
                        this.target.Controller.onEnterEvent('right');
                        this.target.Controller.onExitEvent('left');
                    }

                    if(this.isWalk) {
                        this.target.AnimationStart('walk');
                    } else {
                        this.target.AnimationStart('idle');
                    }
                } else {
                    this.target.AnimationStart('idle');
                }
            } 
            this.UpdateController();
        } else {
            console.log("error : idle no curCell");
            this.target.Controller.onExitEvent('right');
            this.target.Controller.onExitEvent('left');
            this.target.AnimationStart('idle');
        }

        return isAction;
    }

    UpdateController() {
        if(this.isWalk) {
            const targetPos = this.curCell.node.getWorldPosition();
            const curPos = this.target.GetWorldPosition();
            if(!this.isRight && curPos.x <= targetPos.x + this.randX) {
                this.curTime = 0;
                this.target.node.setWorldPosition(new Vec3(targetPos.x + this.randX, curPos.y, curPos.z));
            } else if(this.isRight && curPos.x >= targetPos.x + this.randX) {
                this.curTime = 0;
                this.target.node.setWorldPosition(new Vec3(targetPos.x + this.randX, curPos.y, curPos.z));
            }
        } else {
            this.target.Controller.onExitEvent('right');
            this.target.Controller.onExitEvent('left');
        }
    }
}

export enum eMoveStep {
    StateStart = 0,
    Check = 1,
    Move = 2,
    StateEnd = 3,
};

export class AIStateMove extends AIState {
    static readonly ID: string = "AIStateMove";
    protected target: WorldAIDragon = null;
    protected curCell: CellNode = null;
    protected targetCell: CellNode = null;
    protected isActive: boolean = false;
    protected isWalk: boolean = false;
    protected isRight = true;
    protected isGoal = false;
    protected randX: number = null;
    protected step: number = eMoveStep.StateStart;
    protected moveState: number = 0;
    protected moveSpeed: number = 0;

    GetID() {
        return AIStateMove.ID;
    }
    Set(arr: {dragon: WorldAIDragon, curCell: CellNode, targetCell: CellNode}) {
        super.Set(arr);
        if (ObjectCheck(arr, "dragon")) {
            this.target = arr.dragon;
        }
        if (ObjectCheck(arr, "curCell")) {
            this.curCell = arr.curCell;
        }
        if (ObjectCheck(arr, "targetCell")) {
            this.targetCell = arr.targetCell;
            // console.log(this.curCell, this.targetCell);
            const targetPos = this.curCell.node.getWorldPosition();
            const move = this.targetCell.GetWorldMoveSize();
            this.randX = RandomFloat(move.minX, move.maxX) - move.minX;
            const curPos = this.target.GetWorldPosition();
            if(curPos.x >= targetPos.x + this.randX) {
                this.isRight = false;
            } else if(curPos.x < targetPos.x + this.randX) {
                this.isRight = true;
            }
        }
    }

    OnEnter(): boolean {
        if (this.isActive) {
            return false;
        }

        var isEnter = true;
        this.isGoal = false;
        this.isWalk = false;

        this.step = eMoveStep.StateStart;

        const rnd = RandomInt(0, 10);
        if(rnd < 2) {
            this.moveSpeed = RandomInt(110, 180);
        } else if(rnd < 3) {
            this.moveSpeed = RandomInt(50, 90);
        } else {
            this.moveSpeed = DragonDefaultSpeed;
        }

        this.target.SetSpeed(this.moveSpeed);
        this.skipFrame = true;

        this.isActive = isEnter;
        return isEnter;
    }

    OnExit(): boolean {
        if (!this.isActive) {
            return false;
        }

        var isExit = true;
        
        this.curCell = null;
        this.targetCell = null;

        this.skipFrame = true;
        this.isActive = !isExit;

        return isExit;
    }

    Update(dt: number): boolean {
        if (!super.Update(dt)) {
            return true;
        }

        if(UpdateController(this.target.Controller, this.moveState)) {
            switch (this.step) {
                case eMoveStep.StateStart: {
                    if(this.curCell == null || this.targetCell == null) {
                        this.moveState = eMoveState.None;
                        return true;
                    }
                    this.step = eMoveStep.Check;
                    return true;
                } break;
                case eMoveStep.Check: {
                    if(this.isRight) {
                        AnimationDragon(this, true, eMoveState.Right);
                    } else {
                        AnimationDragon(this, true, eMoveState.Left);
                    }
                    this.step = eMoveStep.Move;
                    return true;
                } break;
                case eMoveStep.Move: {
                    if (!this.isGoal) {
                        const targetPos = this.curCell.node.getWorldPosition();
                        const curPos = this.target.GetWorldPosition();
                        if(StateCheck(this.moveState, eMoveState.Right) && curPos.x >= targetPos.x + this.randX) {
                            this.isGoal = true;
                        } else if(StateCheck(this.moveState, eMoveState.Left) && curPos.x <= targetPos.x + this.randX) {
                            this.isGoal = true;
                        }

                        if (this.isGoal) {
                            this.target.node.setWorldPosition(new Vec3(targetPos.x + this.randX, curPos.y, curPos.z));
                            this.curCell = this.targetCell;
                            this.target.curCell = this.curCell;
                            AnimationDragon(this, false, eMoveState.None);
                            UpdateController(this.target.Controller, this.moveState);
                        }
                        return true;
                    } else {
                        this.step = eMoveStep.StateEnd;
                    }
                    return true;
                } break;
                case eMoveStep.StateEnd: {
                    return false;
                } break;
            }
        }
        return true;
    }
}

enum eEscalatorStep {
    StateStart = 0,
    StartCheck = 1,
    StartMove = 2,
    EndCheck = 3,
    EndMove = 4,
    TargetCheck = 5,
    TargetMove = 6,
    StateEnd = 7,
};

export enum eMoveState {
    None = 0,
    Left = 1 << 0,
    Right = 1 << 1,
    Up = 1 << 2,
    Down = 1 << 3,
};
export class AIStateEscalator extends AIState {
    static readonly ID: string = "AIStateEscalator";
    protected target: WorldAIDragon = null;
    protected curCell: CellNode = null;
    protected targetCell: CellNode = null;
    protected startNode: Node[] = null;
    protected endNode: Node[] = null;
    protected curStartNode: Node = null;
    protected curEndNode: Node = null;
    protected isActive: boolean = false;
    protected isWalk: boolean = false;
    protected isGoal: boolean = false;
    protected moveState: number = 0;
    protected moveSpeed: number = 0;
    protected escalatorSpeed: number = 0;
    protected step: number = eEscalatorStep.StateStart;

    GetID() {
        return AIStateEscalator.ID;
    }
    Set(arr: {dragon: WorldAIDragon, curCell: CellNode, startNode: Node[], endNode: Node[], targetCell: CellNode}) {
        super.Set(arr);
        if (ObjectCheck(arr, "dragon")) {
            this.target = arr.dragon;
        }
        if (ObjectCheck(arr, "curCell")) {
            this.curCell = arr.curCell;
        }
        if (ObjectCheck(arr, "startNode")) {
            this.startNode = arr.startNode;
        }
        if (ObjectCheck(arr, "endNode")) {
            this.endNode = arr.endNode;
        }
        if (ObjectCheck(arr, "targetCell")) {
            this.targetCell = arr.targetCell;
        }
    }

    OnEnter(): boolean {
        if (this.isActive) {
            return false;
        }

        var isEnter = true;

        this.step = eEscalatorStep.StateStart;
        this.isWalk = false;
        this.isGoal = false;
        this.moveState = 0;

        const rnd = RandomInt(0, 10);
        if(rnd < 2) {
            this.moveSpeed = RandomInt(110, 180);
        } else if(rnd < 3) {
            this.moveSpeed = RandomInt(50, 90);
        } else {
            this.moveSpeed = DragonDefaultSpeed;
        }

        this.escalatorSpeed = 60;
        this.target.SetSpeed(this.moveSpeed);
        this.skipFrame = true;
        this.isActive = isEnter;
        return isEnter;
    }

    OnExit(): boolean {
        if (!this.isActive) {
            return false;
        }

        var isExit = true;

        this.step = eEscalatorStep.StateStart;
        this.isWalk = false;
        this.isGoal = false;
        this.isActive = !isExit;
        this.startNode = null;
        this.endNode = null;
        this.curCell = null;
        this.targetCell = null;
        this.moveState = 0;
        this.skipFrame = true;

        return isExit;
    }

    Update(dt: number): boolean {
        if (!super.Update(dt)) {
            return true;
        }

        if(UpdateController(this.target.Controller, this.moveState)) {
            switch (this.step) {
                case eEscalatorStep.StateStart: { //시작 무결성 확인중 !
                    if(this.curCell == null || this.startNode == null || this.endNode == null || this.targetCell == null) {
                        this.moveState = eMoveState.None;
                        this.target.AnimationStart('idle');
                        return true;
                    }
                    this.step = eEscalatorStep.StartCheck;
                    return true;
                } break;
    
                case eEscalatorStep.StartCheck: {// 시작 에스컬레이터 가즈아 !
                    this.isGoal = false;
                    
                    this.target.SetSpeed(this.moveSpeed);
                    if(this.startNode.length == 0) {//목적 층에 도달하였습니다 !
                        this.step = eEscalatorStep.TargetCheck;
                        return true;
                    }
                    this.curStartNode = this.startNode.shift();
                    const curPos = this.target.GetWorldPosition();
                    const curTargetPos = this.curStartNode.getWorldPosition();
                    if(curPos.x >= curTargetPos.x) {
                        AnimationDragon(this, true, eMoveState.Left);
                    } else if(curPos.x < curTargetPos.x) {
                        AnimationDragon(this, true, eMoveState.Right);
                    }
                    UpdateController(this.target.Controller, this.moveState);
                    this.step = eEscalatorStep.StartMove;
                    return true;
                } break;
    
                case eEscalatorStep.StartMove: {//에스컬레이터 가즈아 !
                    if (!this.isGoal) {
                        const curTargetPos = this.curStartNode.getWorldPosition();
                        const curPos = this.target.GetWorldPosition();
                        if(StateCheck(this.moveState, eMoveState.Right) && curPos.x >= curTargetPos.x) {
                            this.isGoal = true;
                        } else if(StateCheck(this.moveState, eMoveState.Left) && curPos.x <= curTargetPos.x) {
                            this.isGoal = true;
                        }

                        if(this.isGoal) {
                            AnimationDragon(this, false, eMoveState.None);
                            this.target.node.setWorldPosition(new Vec3(curTargetPos.x, curTargetPos.y, curTargetPos.z));
                        }
                        UpdateController(this.target.Controller, this.moveState);
                    } else {
                        this.step = eEscalatorStep.EndCheck;
                    }
                    return true;
                } break;
    
                case eEscalatorStep.EndCheck: {//내려가기 계산중 !
                    this.isGoal = false;
                    
                    this.target.SetSpeed(this.escalatorSpeed);
                    if(this.endNode.length == 0) {
                        this.step = eEscalatorStep.TargetCheck;
                        return true;
                    }
                    this.curEndNode = this.endNode.shift();
                    const curTargetPos = this.curEndNode.getWorldPosition();
                    const curPos = this.target.GetWorldPosition();
                    var number = 0;
                    if(curPos.x >= curTargetPos.x) {
                        number = number | eMoveState.Left;
                    } else if(curPos.x < curTargetPos.x) {
                        number = number | eMoveState.Right;
                    }
                    if(curPos.y >= curTargetPos.y) {
                        number = number | eMoveState.Down;
                    } else if(curPos.y < curTargetPos.y) {
                        number = number | eMoveState.Up;
                    }

                    AnimationDragon(this, false, number);
                    UpdateController(this.target.Controller, this.moveState);
                    this.step = eEscalatorStep.EndMove;
                    return true;
                } break;
    
                case eEscalatorStep.EndMove: {//내려가즈아 !
                    if (!this.isGoal) {
                        var goal = true;
                        const curTargetPos = this.curEndNode.getWorldPosition();
                        const curPos = this.target.GetWorldPosition();
                        if(StateCheck(this.moveState, eMoveState.Right) && curPos.x <= curTargetPos.x) {
                            goal = goal && false;
                        } else if(StateCheck(this.moveState, eMoveState.Left) && curPos.x >= curTargetPos.x) {
                            goal = goal && false;
                        }
                        if(StateCheck(this.moveState, eMoveState.Down) && curPos.y >= curTargetPos.y) {
                            goal = goal && false;
                        } else if(StateCheck(this.moveState, eMoveState.Up) && curPos.y <= curTargetPos.y) {
                            goal = goal && false;
                        }

                        if(goal) {
                            AnimationDragon(this, false, eMoveState.None);
                            UpdateController(this.target.Controller, this.moveState);
                            this.target.node.setWorldPosition(new Vec3(curTargetPos.x, curTargetPos.y, curPos.z));
                            this.isGoal = true;
                        }
                    } else {
                        AnimationDragon(this, false, eMoveState.None);
                        UpdateController(this.target.Controller, this.moveState);
                        this.step = eEscalatorStep.StartCheck;
                    }
                    return true;
                } break;
    
                case eEscalatorStep.TargetCheck: {//목표를 확인하는중
                    this.isGoal = false;

                    this.target.SetSpeed(this.moveSpeed);
                    const move = this.targetCell.GetWorldMoveSize();
                    const targetFloat = RandomFloat(move.minX, move.maxX);
                    const curPos = this.target.GetWorldPosition();
                    if(curPos.x >= targetFloat) {
                        AnimationDragon(this, true, eMoveState.Left);
                    } else if(curPos.x < targetFloat) {
                        AnimationDragon(this, true, eMoveState.Right);
                    }
                    UpdateController(this.target.Controller, this.moveState);
                    this.step = eEscalatorStep.TargetMove;
                    return true;
                } break;
    
                case eEscalatorStep.TargetMove: {//목적지에 가즈아 !
                    if (!this.isGoal) {
                        const move = this.targetCell.GetWorldMoveSize();
                        const targetFloat = RandomFloat(move.minX, move.maxX);
                        const curPos = this.target.GetWorldPosition();
                        if(StateCheck(this.moveState, eMoveState.Right) && curPos.x >= targetFloat) {
                            this.isGoal = true;
                        } else if(StateCheck(this.moveState, eMoveState.Left) && curPos.x <= targetFloat) {
                            this.isGoal = true;
                        }

                        if(this.isGoal) {
                            AnimationDragon(this, false, eMoveState.None);
                            UpdateController(this.target.Controller, this.moveState);
                            this.target.node.setWorldPosition(new Vec3(targetFloat, curPos.y, curPos.z));
                            this.curCell = this.targetCell;
                            this.target.curCell = this.curCell;
                        }
                    } else {
                        this.step = eEscalatorStep.StateEnd;
                    }
                    return true;
                } break;
    
                case eEscalatorStep.StateEnd: {//도착했습니다 호갱님
                    return false;
                } break;
    
                default: {
                    return false;
                } break;
            }
        }
        return false;
    }
}

export function AnimationDragon(obj: any, isWalk: boolean, moveState: number) {
    obj.isWalk = isWalk;
    obj.moveState = moveState;
    if(obj.isWalk) {
        obj.target.AnimationStart('walk');
    } else {
        obj.target.AnimationStart('idle');
    }
}

export function UpdateController(controller: SBController, moveState: number): boolean {//움직이쉴?
    if (StateCheck(moveState, eMoveState.Left)) {
        controller.onEnterEvent('left');
    } else {
        controller.onExitEvent('left');
    }

    if (StateCheck(moveState, eMoveState.Right)) {
        controller.onEnterEvent('right');
    } else {
        controller.onExitEvent('right');
    }

    if (StateCheck(moveState, eMoveState.Up)) {
        controller.onEnterEvent('up');
    } else {
        controller.onExitEvent('up');
    }

    if (StateCheck(moveState, eMoveState.Down)) {
        controller.onEnterEvent('down');
    } else {
        controller.onExitEvent('down');
    }
    return true;
}

export function StateCheck(state: number, check: number): boolean {
    if((state & check) == check) {
        return true;
    }
    return false;
}

enum eElevatorStep {
    StateStart = 0,
    StartCheck = 1,
    StartMove = 2,
    StartEscalator = 3,
    StartMoveEscalator = 4,
    StartCheckEscalator = 5,
    ElevatorIdle = 6,
    ElevatorMove = 7,
    EndCheckEscalator = 8,
    EndStartEscalator = 9,
    EndMoveEscalator = 10,
    TargetCheck = 11,
    TargetMove = 12,
    StateEnd = 13
};

const EscalatorTime = 2;

export class AIStateElevator extends AIState {
    static readonly ID: string = "AIStateElevator";
    protected target: WorldAIDragon = null;
    protected curCell: CellNode = null;
    protected targetCell: CellNode = null;
    protected isActive: boolean = false;
    protected isWalk: boolean = false;
    protected isSubGoal: boolean = false;
    protected isGoal: boolean = false;
    protected isEscalator: boolean = false;
    protected moveState: number = 0;
    protected randX: number = 0;
    protected moveSpeed: number = 0;
    protected escalatorSpeed: number = 0;
    protected escalatorTime: number = 0;
    protected step: number = eElevatorStep.StateStart;
    protected targetElevator: WorldAIElevator = null;
    private mapManager: MapManager = null;

    GetID() {
        return AIStateElevator.ID;
    }

    Set(arr: {dragon: WorldAIDragon, curCell: CellNode, targetCell: CellNode, target: WorldAIElevator}) {
        super.Set(arr);
        if (ObjectCheck(arr, "dragon")) {
            this.target = arr.dragon;
        }
        if (ObjectCheck(arr, "curCell")) {
            this.curCell = arr.curCell;
        }
        if (ObjectCheck(arr, "targetCell")) {
            this.targetCell = arr.targetCell;
        }
        if (ObjectCheck(arr, "target")) {
            this.targetElevator = arr.target;
        }
    }

    OnEnter(): boolean {
        if (this.isActive) {
            return false;
        }

        if(this.mapManager == null) {
            this.mapManager = GameManager.GetManager<MapManager>(MapManager.Name);
        }
        var isEnter = true;
        this.step = eEscalatorStep.StateStart;
        this.isWalk = false;
        this.isGoal = false;
        this.isSubGoal = false;
        this.moveState = eElevatorInType.None;
        this.moveSpeed = RandomInt(70, 150);

        this.escalatorSpeed = 60;
        this.escalatorTime = 0;
        this.target.SetSpeed(this.moveSpeed);
        this.skipFrame = true;
        this.isActive = isEnter;
        return isEnter;
    }

    OnExit(): boolean {
        if (!this.isActive) {
            return false;
        }

        var isExit = true;

        this.step = eEscalatorStep.StateStart;
        this.isWalk = false;
        this.isGoal = false;
        this.isSubGoal = false;
        this.isActive = !isExit;
        this.curCell = null;
        this.targetCell = null;
        this.targetElevator = null;
        this.escalatorTime = 0;
        this.moveState = eElevatorInType.None;
        this.skipFrame = true;

        return isExit;
    }

    Update(dt: number): boolean {
        if (!super.Update(dt)) {
            return true;
        }

        if(this.UpdateController()) {
            switch (this.step) {
                case eElevatorStep.StateStart: {//시작 무결성 확인중 !
                    if(this.curCell == null || this.targetElevator == null || this.targetCell == null) {
                        this.moveState = eMoveState.None;
                        this.target.AnimationStart('idle');
                        return true;
                    }
                    this.targetElevator.Set({
                        startFloor: this.curCell.GetFloor(),
                        targetFloor: this.targetCell.GetFloor(),
                        dragon: this.target
                     });
                    this.step = eElevatorStep.StartCheck;
                    return true;
                } break;
    
                case eElevatorStep.StartCheck: {//엘베 가즈아
                    this.isGoal = false;
                    
                    if(this.mapManager.IsStartFloor(this.curCell.GetFloor()) && this.target.node.parent == this.mapManager.nodeContainer['Character']) {
                        this.isEscalator = true;
                        this.target.SetSpeed(this.moveSpeed);
                        let escalator = this.mapManager.GetFloorEscalatorDir(this.curCell.GetFloor(), this.targetElevator.GetCurElevator().ElevatorType);
                        if(escalator == null) {
                            return false;
                        }
                        const move = escalator.GetStartNode().worldPosition;
                        const curPos = this.target.GetWorldPosition();
                        if(curPos.x >= move.x) {
                            this.AnimationDragon(true, eMoveState.Left);
                        } else if(curPos.x < move.x) {
                            this.AnimationDragon(true, eMoveState.Right);
                        }
                        this.step = eElevatorStep.StartEscalator;
                    } else {
                        this.target.SetSpeed(this.moveSpeed);
                        const move = this.targetElevator.GetCurElevator().GetDelayPosition();
                        this.randX = this.targetElevator.GetCurElevator().GetRand();
                        const curPos = this.target.GetWorldPosition();
                        if(curPos.x >= move.x) {
                            this.AnimationDragon(true, eMoveState.Left);
                        } else if(curPos.x < move.x) {
                            this.AnimationDragon(true, eMoveState.Right);
                        }
                        this.step = eElevatorStep.StartMove;
                    }
                    return true;
                } break;
                case eElevatorStep.StartEscalator: {//에스컬레이터 까지 움직이자 !
                    if (!this.isGoal) {
                        let escalator = this.mapManager.GetFloorEscalatorDir(this.curCell.GetFloor(), this.targetElevator.GetCurElevator().ElevatorType);
                        if(escalator == null) {
                            return false;
                        }
                        const move = escalator.GetStartNode().worldPosition;
                        const curPos = this.target.GetWorldPosition();
                        if(StateCheck(this.moveState, eMoveState.Right) && curPos.x >= move.x) {
                            this.isGoal = true;
                        } else if(StateCheck(this.moveState, eMoveState.Left) && curPos.x <= move.x) {
                            this.isGoal = true;
                        }

                        if(this.isGoal) {
                            this.AnimationDragon(false, eMoveState.None);
                            this.target.node.setWorldPosition(new Vec3(move.x, curPos.y, curPos.z));
                            this.isGoal = false;
                            this.step = eElevatorStep.StartMoveEscalator;
                            this.escalatorTime = 0;
                        }
                    }
                    return true;
                } break;
                case eElevatorStep.StartMoveEscalator: {//에스컬레이터 움직이자 !
                    if (!this.isGoal) {
                        let escalator = this.mapManager.GetFloorEscalatorDir(this.curCell.GetFloor(), this.targetElevator.GetCurElevator().ElevatorType);
                        if(escalator == null) {
                            return false;
                        }
                        const moveStart = escalator.GetStartNode().worldPosition;
                        const moveGoal = escalator.GetEndNode().worldPosition;
                        const curPos = this.target.GetWorldPosition();

                        this.escalatorTime += dt;
                        
                        if(this.escalatorTime >= EscalatorTime) {
                            this.isGoal = true;
                        } else {
                            this.target.node.setWorldPosition(BezierCurveVec3(moveStart, moveGoal, this.escalatorTime, EscalatorTime));
                        }

                        if(this.isGoal) {
                            this.target.node.setWorldPosition(new Vec3(moveGoal.x, moveGoal.y, curPos.z));
                            this.isGoal = false;
                            this.isEscalator = false;
                            this.escalatorTime = 0;
                            this.step = eElevatorStep.StartCheckEscalator;
                        }
                    }
                    return true;
                } break;
                case eElevatorStep.StartCheckEscalator: {//에스컬레이터  !
                    this.target.SetSpeed(this.moveSpeed);
                    const move = this.targetElevator.GetCurElevator().GetDelayPosition();
                    this.randX = this.targetElevator.GetCurElevator().GetRand();
                    const curPos = this.target.GetWorldPosition();
                    if(curPos.x >= move.x) {
                        this.AnimationDragon(true, eMoveState.Left);
                    } else if(curPos.x < move.x) {
                        this.AnimationDragon(true, eMoveState.Right);
                    }
                    this.step = eElevatorStep.StartMove;
                    return true;
                } break;
    
                case eElevatorStep.StartMove: {//움직이는중 !
                    if (!this.isGoal) {
                        const move = this.targetElevator.GetCurElevator().GetDelayPosition();
                        const curPos = this.target.GetWorldPosition();
                        if(StateCheck(this.moveState, eMoveState.Right) && curPos.x >= move.x) {
                            this.isGoal = true;
                        } else if(StateCheck(this.moveState, eMoveState.Left) && curPos.x <= move.x) {
                            this.isGoal = true;
                        }

                        if(this.isGoal) {
                            this.AnimationDragon(false, eMoveState.None);
                            this.target.node.setWorldPosition(new Vec3(move.x, curPos.y, curPos.z));
                            this.target.ElevatorIn = eElevatorInType.InIng;
                            this.step = eElevatorStep.ElevatorIdle;
                            this.isGoal = false;
                        }
                    }
                    return true;
                } break;
    
                case eElevatorStep.ElevatorIdle: {//엘리베이터 탑승 기다리기 및 오더 변경 탑승
                    const StartFloor = this.curCell.GetFloor(); 
                    const TargetFloor = this.targetCell.GetFloor();
                    if(this.targetElevator.GetFloor() == this.curCell.GetFloor() 
                    && ((StartFloor < TargetFloor && this.targetElevator.MoveType == eElevatorMoveType.Up) 
                    || (StartFloor > TargetFloor && this.targetElevator.MoveType == eElevatorMoveType.Down) 
                    || this.targetElevator.MoveType == eElevatorMoveType.None)) {
                        if(this.targetElevator.GetState() == AIElevatorInIdle.ID) {
                            let cElevator = this.targetElevator.GetCurElevator();
                            const move = cElevator.GetWorldPosition();
                            const curPos = this.target.GetWorldPosition();
                            switch(cElevator.ElevatorType) {
                                case 'left': {
                                    this.AnimationDragon(true, eMoveState.Left);
                                } break;
                                case 'right': {
                                    this.AnimationDragon(true, eMoveState.Right);
                                } break;
                            }

                            if(!this.isSubGoal) {
                                const subMove = cElevator.GetSwitchPosition();
                                if(StateCheck(this.moveState, eMoveState.Right) && curPos.x >= subMove.x) {
                                    this.isSubGoal = true;
                                } else if(StateCheck(this.moveState, eMoveState.Left) && curPos.x <= subMove.x) {
                                    this.isSubGoal = true;
                                }

                                if(this.isSubGoal) {
                                    cElevator.PushContainer(this.target);
                                }
                            }
    
                            if(StateCheck(this.moveState, eMoveState.Right) && curPos.x >= move.x + this.randX) {
                                this.isGoal = true;
                            } else if(StateCheck(this.moveState, eMoveState.Left) && curPos.x <= move.x + this.randX) {
                                this.isGoal = true;
                            }
    
                            if(this.isGoal && this.target.ElevatorIn == eElevatorInType.InIng) {
                                this.AnimationDragon(false, eMoveState.None);
                                this.target.node.setWorldPosition(new Vec3(move.x + this.randX, curPos.y, curPos.z));
                                this.target.ElevatorIn = eElevatorInType.In;
                                this.step = eElevatorStep.ElevatorMove;
                                this.isGoal = false;
                                this.isSubGoal = false;
                            }
                        }
                    }

                    return true;
                } break;
    
                case eElevatorStep.ElevatorMove: {//오더 변경 및 엘리베이터 내리기
                    if(this.targetElevator.GetFloor() == this.targetCell.GetFloor() && this.targetElevator.GetState() == AIElevatorExitIdle.ID) {
                        switch(this.target.ElevatorIn) {
                            case eElevatorInType.ExitIng: {
                                let cElevator = this.targetElevator.GetCurElevator();
                                const move = cElevator.GetDelayPosition();
                                const curPos = this.target.GetWorldPosition();
                                switch(cElevator.ElevatorType) {
                                    case 'left': {
                                        this.AnimationDragon(true, eMoveState.Right);
                                    } break;
                                    case 'right': {
                                        this.AnimationDragon(true, eMoveState.Left);
                                    } break;
                                }

                                if(!this.isSubGoal) {
                                    const subMove = cElevator.GetSwitchPosition();
                                    if(StateCheck(this.moveState, eMoveState.Right) && curPos.x >= subMove.x) {
                                        this.isSubGoal = true;
                                    } else if(StateCheck(this.moveState, eMoveState.Left) && curPos.x <= subMove.x) {
                                        this.isSubGoal = true;
                                    }

                                    if(this.isSubGoal) {
                                        if(this.mapManager.IsStartFloor(this.targetCell.GetFloor())) {
                                            if(RandomInt(0, 2) == 0) {
                                                cElevator.PopContainer(this.target);
                                                this.isEscalator = true;
                                            } else {
                                                cElevator.PopContainer(this.target, this.mapManager.nodeContainer['BackCellCharacter'], this.mapManager.nodeContainer['BackCellShadow']);
                                                this.isEscalator = false;
                                            }
                                        } else {
                                            cElevator.PopContainer(this.target);
                                        }
                                    }
                                }
                                
                                if(StateCheck(this.moveState, eMoveState.Right) && curPos.x >= move.x) {
                                    this.isGoal = true;
                                } else if(StateCheck(this.moveState, eMoveState.Left) && curPos.x <= move.x) {
                                    this.isGoal = true;
                                }
        
                                if(this.isGoal) {
                                    this.AnimationDragon(false, eMoveState.None);
                                    this.target.node.setWorldPosition(new Vec3(move.x, curPos.y, curPos.z));
                                    this.target.ElevatorIn = eElevatorInType.Exit;
                                    if(this.isEscalator) {
                                        this.step = eElevatorStep.EndCheckEscalator;
                                    } else {
                                        this.step = eElevatorStep.TargetCheck;
                                    }
                                    this.isSubGoal = false;
                                    this.isGoal = false;
                                }
                            } break;
                            case eElevatorInType.Exit: {
                                this.target.ElevatorIn = eElevatorInType.Exit;
                                if(this.isEscalator) {
                                    this.step = eElevatorStep.EndCheckEscalator;
                                } else {
                                    this.step = eElevatorStep.TargetCheck;
                                }
                            } break;
                        }
                        // if(this.target.ElevatorIn == eElevatorInType.OnBoard) {
                        //     if(this.targetElevator.GetState() == AIElevatorOpen.ID) {
                        //         this.AnimationDragon(true, eMoveState.None)
                        //         this.target.SetScale(this.target.GetScale() + dt * 0.1, 0.9, 1);
                        //     };
                        // }
                    }

                    return true;
                } break;

                case eElevatorStep.EndCheckEscalator: {
                    this.isGoal = false;
                    this.target.SetSpeed(this.moveSpeed);
                    let escalator = this.mapManager.GetFloorEscalatorDir(this.targetCell.GetFloor(), this.targetElevator.GetCurElevator().ElevatorType);
                    if(escalator == null) {
                        return false;
                    }
                    const move = escalator.GetEndNode().worldPosition;
                    const curPos = this.target.GetWorldPosition();
                    if(curPos.x >= move.x) {
                        this.AnimationDragon(true, eMoveState.Left);
                    } else if(curPos.x < move.x) {
                        this.AnimationDragon(true, eMoveState.Right);
                    }
                    this.step = eElevatorStep.EndStartEscalator;
                    return true;
                } break;

                case eElevatorStep.EndStartEscalator: {
                    if (!this.isGoal) {
                        let escalator = this.mapManager.GetFloorEscalatorDir(this.targetCell.GetFloor(), this.targetElevator.GetCurElevator().ElevatorType);
                        if(escalator == null) {
                            return false;
                        }

                        const move = escalator.GetEndNode().worldPosition;
                        const curPos = this.target.GetWorldPosition();
                        if(StateCheck(this.moveState, eMoveState.Right) && curPos.x >= move.x) {
                            this.isGoal = true;
                        } else if(StateCheck(this.moveState, eMoveState.Left) && curPos.x <= move.x) {
                            this.isGoal = true;
                        }

                        if(this.isGoal) {
                            this.AnimationDragon(false, eMoveState.None);
                            this.target.node.setWorldPosition(new Vec3(move.x, curPos.y, curPos.z));
                            this.escalatorTime = 0;
                            this.step = eElevatorStep.EndMoveEscalator;
                            this.isGoal = false;
                        }
                    }
                    return true;
                } break;

                case eElevatorStep.EndMoveEscalator: {
                    if (!this.isGoal) {
                        let escalator = this.mapManager.GetFloorEscalatorDir(this.targetCell.GetFloor(), this.targetElevator.GetCurElevator().ElevatorType);
                        if(escalator == null) {
                            return false;
                        }
                        const moveStart = escalator.GetEndNode().worldPosition;
                        const moveGoal = escalator.GetStartNode().worldPosition;
                        const curPos = this.target.GetWorldPosition();

                        this.escalatorTime += dt;
                        
                        if(this.escalatorTime >= EscalatorTime) {
                            this.isGoal = true;
                        } else {
                            this.target.node.setWorldPosition(BezierCurveVec3(moveStart, moveGoal, this.escalatorTime, EscalatorTime));
                        }

                        if(this.isGoal) {
                            this.target.node.setWorldPosition(new Vec3(moveGoal.x, moveGoal.y, curPos.z));
                            this.isGoal = false;
                            this.isEscalator = false;
                            this.escalatorTime = 0;
                            this.step = eElevatorStep.TargetCheck;
                        }
                    }
                    return true;
                } break;
    
                case eElevatorStep.TargetCheck: {//목적지까지 가즈아
                    this.target.ElevatorIn = eElevatorInType.None;
                    this.isGoal = false;

                    this.target.SetSpeed(this.moveSpeed);
                    const targetPos = this.targetCell.node.getWorldPosition();
                    const move = this.targetCell.GetWorldMoveSize();
                    this.randX = RandomFloat(move.minX, move.maxX) - move.minX;
                    const curPos = this.target.GetWorldPosition();
                    if(curPos.x >= targetPos.x + this.randX) {
                        this.AnimationDragon(true, eMoveState.Left);
                    } else if(curPos.x < targetPos.x + this.randX) {
                        this.AnimationDragon(true, eMoveState.Right);
                    }
                    this.step = eElevatorStep.TargetMove;
                    return true;
                } break;
    
                case eElevatorStep.TargetMove: {//가즈아
                    if (!this.isGoal) {
                        const targetPos = this.targetCell.node.getWorldPosition();
                        const curPos = this.target.GetWorldPosition();
                        if(StateCheck(this.moveState, eMoveState.Right) && curPos.x >= targetPos.x + this.randX) {
                            this.isGoal = true;
                        } else if(StateCheck(this.moveState, eMoveState.Left) && curPos.x <= targetPos.x + this.randX) {
                            this.isGoal = true;
                        }

                        if(this.isGoal) {
                            this.AnimationDragon(false, eMoveState.None);
                            this.target.node.setWorldPosition(new Vec3(targetPos.x + this.randX, curPos.y, curPos.z));
                            this.curCell = this.targetCell;
                            this.target.curCell = this.curCell;
                            this.isGoal = false;
                            this.step = eElevatorStep.StateEnd;
                        }
                    }
                    return true;
                } break;
    
                case eElevatorStep.StateEnd: {//도오착
                    return false;
                } break;
    
                default: {
                    return false;
                } break;
            }
        }
        return false;
    }

    AnimationDragon(isWalk: boolean, moveState: number) {
        this.isWalk = isWalk;
        this.moveState = moveState;
        if(this.isWalk) {
            this.target.AnimationStart('walk');
        } else {
            this.target.AnimationStart('idle');
        }
        this.UpdateController();
    }

    UpdateController(): boolean {
        if (StateCheck(this.moveState, eMoveState.Left)) {
            this.target.Controller.onEnterEvent('left');
        } else {
            this.target.Controller.onExitEvent('left');
        }

        if (StateCheck(this.moveState, eMoveState.Right)) {
            this.target.Controller.onEnterEvent('right');
        } else {
            this.target.Controller.onExitEvent('right');
        }

        if (StateCheck(this.moveState, eMoveState.Up)) {
            this.target.Controller.onEnterEvent('up');
        } else {
            this.target.Controller.onExitEvent('up');
        }

        if (StateCheck(this.moveState, eMoveState.Down)) {
            this.target.Controller.onEnterEvent('down');
        } else {
            this.target.Controller.onExitEvent('down');
        }
        return true;
    }
}
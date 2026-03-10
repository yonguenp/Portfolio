
import { _decorator, Vec3 } from 'cc';
import { IStateData } from 'sb';
import { eElevatorInType } from '../../Character/WorldAIDragon';
import { eElevatorMoveType, WorldAIElevator } from '../../Character/WorldAIElevator';
import { ElevatorY } from '../../Map/MapManager';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = ElevatorState
 * DateTime = Fri Jan 14 2022 17:37:32 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = ElevatorState.ts
 * FileBasenameNoExtension = ElevatorState
 * URL = db://assets/Scripts/AI/AIState/ElevatorState.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
export abstract class AIElevatorState implements IStateData {
    protected skipFrame: boolean = false;
    protected elevator: WorldAIElevator = null;
    protected isActive: boolean = false;

    GetID() {
        return "ElevatorMoveState";
    }
    Set(elevator: WorldAIElevator, data?: any): boolean {
        if(elevator != null) {
            this.elevator = elevator;
            this.skipFrame = true;
            return true;
        }
        return false;
    }

    OnEnter(): boolean {
        if(this.isActive) {
            return false;
        }
        this.isActive = true;
        return true;
    }

    OnExit(): boolean {
        if(!this.isActive) {
            return false;
        }
        this.isActive = false;
        return true;
    }

    Update(dt: number): string {
        if(this.skipFrame) {
            this.skipFrame = false;
            return undefined;
        }
        return "";
    }
}
 
export class AIElevatorIdle extends AIElevatorState {
    static readonly ID: string = "AIElevatorIdle";
    GetID() {
        return AIElevatorIdle.ID;
    }

    Set(elevator: WorldAIElevator, data?: any): boolean {
        if(super.Set(elevator)) {
            return true;
        }
        return false;
    }

    OnEnter(): boolean {
        if(super.OnEnter()) {
            return true;
        }
        return false;
    }

    OnExit(): boolean {
        if(super.OnExit()) {
            return true;
        }
        return false;
    }

    Update(dt: number): string {
        if(super.Update(dt) != undefined) {
            let targets = this.elevator.GetTargets();
            var isOpen = false;
            var isUp = 0;
            var isDown = 0;
            var isDone = 0;
            if(targets != null) {
                let elevator = this.elevator.GetCurElevator();
                targets.forEach(element => {
                    switch(element.Dragon.ElevatorIn) {
                        case eElevatorInType.OnBoard: {
                            if (elevator != undefined) {
                                elevator.PushContainer(element.Dragon);
                            }

                            if(element.TargetFloor == this.elevator.GetFloor()) {
                                isOpen = true;
                                element.Dragon.ElevatorIn = eElevatorInType.ExitIng;
                            } else {
                                if(element.TargetFloor > this.elevator.GetFloor()) {
                                    isUp++;
                                }
            
                                if(element.TargetFloor < this.elevator.GetFloor()) {
                                    isDown++;
                                }
                            }
                        } break;
                        case eElevatorInType.In: case eElevatorInType.InIng: {
                            if(element.StartFloor == this.elevator.GetFloor()) {
                                if((element.StartFloor < element.TargetFloor && this.elevator.MoveType == eElevatorMoveType.Up) 
                                || (element.StartFloor > element.TargetFloor && this.elevator.MoveType == eElevatorMoveType.Down) 
                                || this.elevator.MoveType == eElevatorMoveType.None) {
                                    isOpen = true;
                                }
                            }
        
                            if(element.StartFloor == this.elevator.GetFloor()) {
                                isDone++;
                            }
        
                            if(element.StartFloor > this.elevator.GetFloor()) {
                                isUp++;
                            }
        
                            if(element.StartFloor < this.elevator.GetFloor()) {
                                isDown++;
                            }
                        } break;
                        case eElevatorInType.Exit: case eElevatorInType.ExitIng: {
                            if(element.TargetFloor == this.elevator.GetFloor()) {
                                isOpen = true;
                            }
                        } break;
                    }
                });
            }

            if(isUp <= 0 && isDown <= 0 && isDone > 0) {
                isOpen = true;
            }

            if(isOpen) {
                return AIElevatorOpen.ID;
            }

            switch(this.elevator.MoveType) {
                case eElevatorMoveType.Up:
                    if(isUp > 0) {
                        return AIElevatorMoveUp.ID;
                    }
        
                    if(isDown > 0) {
                        return AIElevatorMoveDown.ID;
                    }
                    break;
                case eElevatorMoveType.Down:
                    if(isDown > 0) {
                        return AIElevatorMoveDown.ID;
                    }

                    if(isUp > 0) {
                        return AIElevatorMoveUp.ID;
                    }
                    break;
                default:
                    if(isUp > 0 && isUp >= isDown) {
                        return AIElevatorMoveUp.ID;
                    }
        
                    if(isDown > 0 && isDown >= isUp) {
                        return AIElevatorMoveDown.ID;
                    }
                    break;
            }
        }
        return "";
    }
}

const ElevatorAnimationTime: number = 0.5;
 
export class AIElevatorOpen extends AIElevatorState {
    static readonly ID: string = "AIElevatorOpen";

    curTime: number = 0;
    GetID() {
        return AIElevatorOpen.ID;
    }

    Set(elevator: WorldAIElevator, data?: any): boolean {
        if(super.Set(elevator)) {
            return true;
        }
        return false;
    }

    OnEnter(): boolean {
        if(super.OnEnter()) {
            this.elevator.ElevatorOpen();
            this.curTime = ElevatorAnimationTime;
            return true;
        }
        return false;
    }

    OnExit(): boolean {
        if(super.OnExit()) {
            this.curTime = ElevatorAnimationTime;
            return true;
        }
        return false;
    }

    Update(dt: number): string {
        if(super.Update(dt) != undefined) {
            this.curTime -= dt;
            if(this.curTime <= 0) {
                return AIElevatorExitIdle.ID;
            }
        }
        return "";
    }
}
 
export class AIElevatorExitIdle extends AIElevatorState {
    static readonly ID: string = "AIElevatorExitIdle";
    GetID() {
        return AIElevatorExitIdle.ID;
    }

    Set(elevator: WorldAIElevator, data?: any): boolean {
        if(super.Set(elevator)) {
            return true;
        }
        return false;
    }

    OnEnter(): boolean {
        if(super.OnEnter()) {
            return true;
        }
        return false;
    }

    OnExit(): boolean {
        if(super.OnExit()) {
            return true;
        }
        return false;
    }

    Update(dt: number): string {
        if(super.Update(dt) != undefined) {
            var isSkip = false;
            let targets = this.elevator.GetTargets();
            if(targets != null) {
                const floor = this.elevator.GetFloor();
                targets.forEach(element => {
                    if(element.TargetFloor == floor && element.Dragon.ElevatorIn == eElevatorInType.OnBoard) {
                        element.Dragon.ElevatorIn = eElevatorInType.ExitIng;
                        isSkip = true;
                    }
                });

                targets.forEach(element => {
                    if(element.Dragon.ElevatorIn == eElevatorInType.ExitIng) {
                        isSkip = true;
                    }
                });
                this.elevator.Unset();
            }
            if(isSkip) {
                return "";
            }
            return AIElevatorInIdle.ID;
        }
        return "";
    }
}
 
export class AIElevatorInIdle extends AIElevatorState {
    static readonly ID: string = "AIElevatorInIdle";
    GetID() {
        return AIElevatorInIdle.ID;
    }

    Set(elevator: WorldAIElevator, data?: any): boolean {
        if(super.Set(elevator)) {
            return true;
        }
        return false;
    }

    OnEnter(): boolean {
        if(super.OnEnter()) {
            return true;
        }
        return false;
    }

    OnExit(): boolean {
        if(super.OnExit()) {
            return true;
        }
        return false;
    }

    Update(dt: number): string {
        if(super.Update(dt) != undefined) {
            var isSkip = false;
            let targets = this.elevator.GetTargets();
            if(targets != null) {
                const floor = this.elevator.GetFloor();
                let elevator = this.elevator.GetCurElevator();

                targets.forEach(element => {
                    if(element.StartFloor == floor && element.Dragon.ElevatorIn == eElevatorInType.InIng) {
                        if(elevator != undefined 
                        && ((element.StartFloor < element.TargetFloor && this.elevator.MoveType == eElevatorMoveType.Up) 
                        || (element.StartFloor > element.TargetFloor && this.elevator.MoveType == eElevatorMoveType.Down) 
                        || this.elevator.MoveType == eElevatorMoveType.None)) {
                            isSkip = true;
                        }
                    }
                    if(element.Dragon.ElevatorIn == eElevatorInType.In) {
                        element.Dragon.ElevatorIn = eElevatorInType.OnBoard;
                        isSkip = true;
                    }
                });
                this.elevator.Unset();
            }
            if(isSkip) {
                return "";
            }
            return AIElevatorClose.ID;
        }
        return "";
    }
}
 
export class AIElevatorClose extends AIElevatorState {
    static readonly ID: string = "AIElevatorClose";

    curTime: number = 0;
    GetID() {
        return AIElevatorClose.ID;
    }

    Set(elevator: WorldAIElevator, data?: any): boolean {
        if(super.Set(elevator)) {
            return true;
        }
        return false;
    }

    OnEnter(): boolean {
        if(super.OnEnter()) {
            this.elevator.ElevatorClose();
            this.curTime = ElevatorAnimationTime;
            return true;
        }
        return false;
    }

    OnExit(): boolean {
        if(super.OnExit()) {
            this.curTime = ElevatorAnimationTime;
            return true;
        }
        return false;
    }

    Update(dt: number): string {
        if(super.Update(dt) != undefined) {
            this.curTime -= dt;
            if(this.curTime <= 0) {
                return AIElevatorIdle.ID;
            }
        }
        return "";
    }
}

const ElevatorMoveTime: number = 1;
const ElevatorPulleySpeed: number = 360;
 
export class AIElevatorMoveUp extends AIElevatorState {
    static readonly ID: string = "AIElevatorMoveUp";

    curTime: number = 0;
    GetID() {
        return AIElevatorMoveUp.ID;
    }

    Set(elevator: WorldAIElevator): boolean {
        if(super.Set(elevator)) {
            if(elevator != null) {
                this.elevator = elevator;
                return true;
            }
        }
        return false;
    }

    OnEnter(): boolean {
        if(super.OnEnter()) {
            this.elevator.MoveType = eElevatorMoveType.Up;
            this.curTime = 0;
            return true;
        }
        return false;
    }

    OnExit(): boolean {
        if(super.OnExit()) {
            this.curTime = 0;
            return true;
        }
        return false;
    }

    Update(dt: number): string {
        
        if(super.Update(dt) != undefined) {
            this.curTime -= dt;
            if(this.curTime <= 0) {
                if(this.elevator.Container != null) {
                    let curElevator = this.elevator.GetCurElevator();
                    let goalY = curElevator.GetWorldPosition().y;
                    let curContainer = this.elevator.Container;
                    var worldPos = new Vec3(curContainer.node.worldPosition);
                    worldPos.y = goalY;
                    curContainer.node.setWorldPosition(worldPos);
                }
                this.curTime = ElevatorMoveTime;
                let targets = this.elevator.GetTargets();
                var isStop = false;
                var isInDragon = true;
                if(targets != null) {
                    var upDragonCount = 0;
                    var downDragonCount = 0;

                    targets.forEach(element => {
                        switch(element.Dragon.ElevatorIn) {
                            case eElevatorInType.In: case eElevatorInType.InIng: {
                                if(element.StartFloor == this.elevator.GetFloor()) {
                                    switch(this.elevator.MoveType) {
                                        case eElevatorMoveType.Up: {
                                            if(this.elevator.GetFloor() < element.TargetFloor) {
                                                upDragonCount++;
                                                isStop = true;
                                            }
                                        } break;
                                        case eElevatorMoveType.Down: {
                                            if(this.elevator.GetFloor() > element.TargetFloor) {
                                                downDragonCount++;
                                                isStop = true;
                                            }
                                        } break;
                                        case eElevatorMoveType.None: {
                                            upDragonCount++;
                                            downDragonCount++;
                                            isStop = true;
                                        } break;
                                    }
                                } else {
                                    if(this.elevator.GetFloor() < element.StartFloor) {
                                        upDragonCount++;
                                    }
                                    if(this.elevator.GetFloor() > element.StartFloor) {
                                        downDragonCount++;
                                    }
                                }
                            } break;
                            case eElevatorInType.OnBoard: {
                                if(element.TargetFloor == this.elevator.GetFloor()) {
                                    upDragonCount++;
                                    downDragonCount++;
                                    isStop = true;
                                } else {
                                    if(element.TargetFloor > this.elevator.GetFloor()) {
                                        upDragonCount++;
                                    }
                                    if(element.TargetFloor < this.elevator.GetFloor()) {
                                        downDragonCount++;
                                    }
                                }
                            } break;
                        }
                    });
                    
                    switch(this.elevator.MoveType) {
                        case eElevatorMoveType.Up: {
                            if(upDragonCount == 0) {
                                this.elevator.MoveType = eElevatorMoveType.None;
                                isStop = true;
                            }
                        } break;
                        case eElevatorMoveType.Down: {
                            if(downDragonCount == 0) {
                                this.elevator.MoveType = eElevatorMoveType.None;
                                isStop = true;
                            }
                        } break;
                    }
                }
                if(isStop) {
                    this.elevator.FloorCheck();
                    return AIElevatorIdle.ID;
                }

                return this.elevator.FloorUp() == true ? "" : AIElevatorIdle.ID;
            } else if(this.elevator.Container != null) {//엘리베이터 움직임 구현
                var remain = ElevatorMoveTime - this.curTime;
                if(remain == 0) {
                    return "";
                }
                let curElevator = this.elevator.GetCurElevator();
                var goalY = curElevator.GetWorldPosition().y;
                var startY = goalY - ElevatorY;
                var remainY = ElevatorY;
                var curTime = remain / ElevatorMoveTime;
                var curY = remainY * curTime;
                let curContainer = this.elevator.Container;
                var worldPos = new Vec3(curContainer.node.worldPosition);
                worldPos.y = startY + curY;
                if(this.elevator.Container.PulleyTop != null) {
                    this.elevator.Container.PulleyTop.angle = this.elevator.Container.PulleyTop.angle - dt * ElevatorPulleySpeed;
                }
                if(this.elevator.Container.PulleyBottom != null) {
                    this.elevator.Container.PulleyBottom.angle = this.elevator.Container.PulleyBottom.angle - dt * ElevatorPulleySpeed;
                }
                curContainer.node.setWorldPosition(worldPos);
            }
        }
        return "";
    }
}
 
export class AIElevatorMoveDown extends AIElevatorState {
    static readonly ID: string = "AIElevatorMoveDown";

    curTime: number = 0;
    GetID() {
        return AIElevatorMoveDown.ID;
    }

    Set(elevator: WorldAIElevator): boolean {
        if(super.Set(elevator)) {
            if(elevator != null) {
                this.elevator = elevator;
                return true;
            }
        }
        return false;
    }

    OnEnter(): boolean {
        if(super.OnEnter()) {
            this.elevator.MoveType = eElevatorMoveType.Down;
            this.curTime = 0;
            return true;
        }
        return false;
    }

    OnExit(): boolean {
        if(super.OnExit()) {
            this.curTime = 0;
            return true;
        }
        return false;
    }

    Update(dt: number): string {
        if(super.Update(dt) != undefined) {
            this.curTime -= dt;
            if(this.curTime <= 0) {
                if(this.elevator.Container != null) {
                    let curElevator = this.elevator.GetCurElevator();
                    let goalY = curElevator.GetWorldPosition().y;
                    let curContainer = this.elevator.Container;
                    var worldPos = new Vec3(curContainer.node.worldPosition);
                    worldPos.y = goalY;
                    curContainer.node.setWorldPosition(worldPos);
                }
                this.curTime = ElevatorMoveTime;
                let targets = this.elevator.GetTargets();
                var isStop = false;
                if(targets != null) {
                    // targets.forEach(element => {
                    //     if(element.StartFloor == this.elevator.GetFloor() && element.Dragon.ElevatorIn == eElevatorInType.In 
                    //     && ((element.StartFloor < element.TargetFloor && this.elevator.MoveType == eElevatorMoveType.Up) 
                    //     || (element.StartFloor > element.TargetFloor && this.elevator.MoveType == eElevatorMoveType.Down) 
                    //     || this.elevator.MoveType == eElevatorMoveType.None)) {
                    //         isStop = true;
                    //     }
                    //     if(element.TargetFloor == this.elevator.GetFloor() && element.Dragon.ElevatorIn == eElevatorInType.OnBoard) {
                    //         isStop = true;
                    //     }
                    // });
                    var upDragonCount = 0;
                    var downDragonCount = 0;

                    targets.forEach(element => {
                        switch(element.Dragon.ElevatorIn) {
                            case eElevatorInType.In: case eElevatorInType.InIng: {
                                if(element.StartFloor == this.elevator.GetFloor()) {
                                    switch(this.elevator.MoveType) {
                                        case eElevatorMoveType.Up: {
                                            if(this.elevator.GetFloor() < element.TargetFloor) {
                                                upDragonCount++;
                                                isStop = true;
                                            }
                                        } break;
                                        case eElevatorMoveType.Down: {
                                            if(this.elevator.GetFloor() > element.TargetFloor) {
                                                downDragonCount++;
                                                isStop = true;
                                            }
                                        } break;
                                        case eElevatorMoveType.None: {
                                            upDragonCount++;
                                            downDragonCount++;
                                            isStop = true;
                                        } break;
                                    }
                                } else {
                                    if(this.elevator.GetFloor() < element.StartFloor) {
                                        upDragonCount++;
                                    }
                                    if(this.elevator.GetFloor() > element.StartFloor) {
                                        downDragonCount++;
                                    }
                                }
                            } break;
                            case eElevatorInType.OnBoard: {
                                if(element.TargetFloor == this.elevator.GetFloor()) {
                                    upDragonCount++;
                                    downDragonCount++;
                                    isStop = true;
                                } else {
                                    if(element.TargetFloor > this.elevator.GetFloor()) {
                                        upDragonCount++;
                                    }
                                    if(element.TargetFloor < this.elevator.GetFloor()) {
                                        downDragonCount++;
                                    }
                                }
                            } break;
                        }
                    });
                    
                    switch(this.elevator.MoveType) {
                        case eElevatorMoveType.Up: {
                            if(upDragonCount == 0) {
                                this.elevator.MoveType = eElevatorMoveType.None;
                                isStop = true;
                            }
                        } break;
                        case eElevatorMoveType.Down: {
                            if(downDragonCount == 0) {
                                this.elevator.MoveType = eElevatorMoveType.None;
                                isStop = true;
                            }
                        } break;
                    }
                }
                if(isStop) {
                    this.elevator.FloorCheck();
                    return AIElevatorIdle.ID;
                }

                return this.elevator.FloorDown() == true ? "" : AIElevatorIdle.ID;
            } else if(this.elevator.Container != null) {//엘리베이터 움직임 구현
                var remain = ElevatorMoveTime - this.curTime;
                if(remain == 0) {
                    return "";
                }
                let curElevator = this.elevator.GetCurElevator();
                var goalY = curElevator.GetWorldPosition().y;
                var startY = goalY + ElevatorY;
                var remainY = ElevatorY;
                var curTime = remain / ElevatorMoveTime;
                var curY = remainY * curTime;
                let curContainer = this.elevator.Container;
                var worldPos = new Vec3(curContainer.node.worldPosition);
                worldPos.y = startY - curY;
                if(this.elevator.Container.PulleyTop != null) {
                    this.elevator.Container.PulleyTop.angle = this.elevator.Container.PulleyTop.angle + dt * ElevatorPulleySpeed;
                }
                if(this.elevator.Container.PulleyBottom != null) {
                    this.elevator.Container.PulleyBottom.angle = this.elevator.Container.PulleyBottom.angle + dt * ElevatorPulleySpeed;
                }
                curContainer.node.setWorldPosition(worldPos);
            }
        }
        return "";
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

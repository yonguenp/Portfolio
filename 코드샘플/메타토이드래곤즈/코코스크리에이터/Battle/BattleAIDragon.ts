
import { _decorator, Vec3 } from 'cc';
import { IStateData } from 'sb';
import { AIState, AnimationDragon, eMoveState, eMoveStep, StateCheck, UpdateController } from '../AI/AIState/AIState';
import { Character, DragonDefaultSpeed } from '../Character/Dragon';
import { ObjectCheck, RandomInt } from '../Tools/SandboxTools';

/**
 * Predefined variables
 * Name = BattleAIDragon
 * DateTime = Tue Feb 22 2022 16:40:58 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = BattleAIDragon.ts
 * FileBasenameNoExtension = BattleAIDragon
 * URL = db://assets/Scripts/Battle/BattleAIDragon.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */

 export abstract class BattleState implements IStateData {
    protected target: Character = null;
    protected skipFrame: boolean = false;

    GetID() {
        return "BattleState";
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

 export class BattleStateIdle extends BattleState {
    static readonly ID: string = "BattleStateIdle";
    protected isActive: boolean = false;
    protected isWalk: boolean = false;
    protected isRight = true;

    GetID() {
        return BattleStateIdle.ID;
    }
    Set(arr: {dragon: Character, walk: boolean}) {
        super.Set(arr);
        if (ObjectCheck(arr, "dragon")) {
            this.target = arr.dragon;
        }
        if (ObjectCheck(arr, "walk")) {
            this.isWalk = arr.walk;
        }
    }

    OnEnter(): boolean {
        if (this.isActive) {
            return false;
        }

        var isEnter = false;
        
        if(this.target.Animation != null)
        {
            isEnter = true;
            this.isWalk = false;
            AnimationDragon(this, this.isWalk, eMoveState.Right);
            const scale = this.target.node.getScale();
            this.target.node.setScale(new Vec3(-Math.abs(scale.x), scale.y, scale.z));
        }
        
        this.skipFrame = true;

        this.isActive = isEnter;
        return isEnter;
    }

    OnExit(): boolean {
        if (!this.isActive) {
            return false;
        }

        var isExit = true;

        this.skipFrame = true;
        this.isActive = !isExit;

        return isExit;
    }

    Update(dt: number): boolean {
        if (!super.Update(dt)) {
            return true;
        }
        
        return false;
    }
}

export class BattleStatePosition extends BattleState {
    static readonly ID: string = "BattleStatePosition";
    protected isActive: boolean = false;
    protected isWalk: boolean = false;
    protected isRight = true;
    protected isGoal = false;
    protected goal: Vec3 = null;
    protected step: number = eMoveStep.StateStart;
    protected moveState: number = 0;
    protected moveSpeed: number = 0;

    GetID() {
        return BattleStatePosition.ID;
    }
    Set(arr: {dragon: Character, targetPos: Vec3}) {
        super.Set(arr);
        if (ObjectCheck(arr, "dragon")) {
            this.target = arr.dragon;
        }
        if (ObjectCheck(arr, "targetPos")) {
            // console.log(this.curCell, this.targetCell);
            this.goal = arr.targetPos;
            const curPos = this.target.GetWorldPosition();
            if(curPos.x >= this.goal.x) {
                this.isRight = false;
            } else if(curPos.x < this.goal.x) {
                this.isRight = true;
            }
        }
    }

    OnEnter(): boolean {
        if (this.isActive) {
            return false;
        }

        var isEnter = false;
        
        if(this.target.Animation != null)
        {
            isEnter = true;
            this.isGoal = false;
            this.isWalk = false;
        }
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
        
        this.goal = null;
        this.isGoal = false;

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
                    if(this.target == null || this.goal == null) {
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
                        const curPos = this.target.GetWorldPosition();
                        if(StateCheck(this.moveState, eMoveState.Right) && curPos.x >= this.goal.x) {
                            this.isGoal = true;
                        } else if(StateCheck(this.moveState, eMoveState.Left) && curPos.x <= this.goal.x) {
                            this.isGoal = true;
                        }

                        if (this.isGoal) {
                            this.target.node.setWorldPosition(new Vec3(this.goal.x, curPos.y, curPos.z));
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

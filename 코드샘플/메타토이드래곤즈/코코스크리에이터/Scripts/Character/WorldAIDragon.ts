
import { Node, sp, Vec4, _decorator } from 'cc';
import { CellNode } from '../Map/CellNode';
import { ObjectCheck } from '../Tools/SandboxTools';
import { SBController } from '../Tools/SBController';
import { AIStateIdle } from '../AI/AIState/AIState';
import { AIStateMachine } from '../AI/AIState/AIStateMachine';
import { AIManager } from '../AI/AIManager';
import { Character, DragonDefaultSpeed } from './Dragon';
import { eDragonState, UserDragon } from '../User/User';
import { ShadowEvent } from './ShadowEvent';
const { ccclass, requireComponent } = _decorator;

/**
 * Predefined variables
 * Name = WorldAIDragon
 * DateTime = Tue Dec 28 2021 16:45:45 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = WorldAIDragon.ts
 * FileBasenameNoExtension = WorldAIDragon
 * URL = db://assets/Scripts/Character/WorldAIDragon.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
export enum eElevatorInType {
    None = 0,
    InIng = 1,
    In = 2,
    OnBoard = 3,
    ExitIng = 4,
    Exit = 5,
};
 
@ccclass('WorldAIDragon')
@requireComponent(SBController)
export class WorldAIDragon extends Character {
    shadow: Node = null;
    spine: sp.Skeleton = null;
    ai: AIStateMachine = null;
    curCell: CellNode = null;
    targetCell: CellNode = null;
    protected elevatorIn: number = eElevatorInType.None;
    public get ElevatorIn(): number { return this.elevatorIn; }
    public set ElevatorIn(isIn: number) { this.elevatorIn = isIn; }
    public data: UserDragon = null;
    public get Data(): UserDragon {
        return this.data;
    }
    public set Data(value: UserDragon) {
        this.data = value;
    }

    Init() {
        if(this.spine == null) {
            let spineNode = this.node.getChildByName('dragon');
            if(spineNode != null) {
                this.spine = spineNode.getComponent(sp.Skeleton);
            }
        }

        this.shadow = this.node.getChildByName('deco_shadow');
        if(this.shadow != null) {
            this.shadow.active = false;
        }

        ShadowEvent.TriggerEvent({state:"Add", target: this.node, info: new Vec4(0, 0, 30, 12)});

        this.ai = new AIStateMachine();
        this.ai.Set(this);

        super.Init();
        this.ai.StateInit();

        AIManager.AddDragon(this);
        this.ai.Change(AIStateIdle.ID);
    }

    onDestroy() {
        ShadowEvent.TriggerEvent({state:"Del", target: this.node});
        AIManager.DelDragon(this);
    }

    Update (deltaTime: number): boolean {
        if(this.Data == null || this.Data.State != eDragonState.Normal) {
            this.node.active = false;
            return false;
        }

        if(!this.node.active) {
            this.node.active = true;
        }
        
        if (!this.ai?.Update(deltaTime))
        {
            return false;
        }

        this.Controller?.onController(deltaTime);
        return true;
    }
    SetState(stateName: string, data?: {}) {
        this.ai.Change(stateName);
        this.SetStateData(data);
    }

    SetStateData(data: {}): void {
        if(ObjectCheck(data, 'curCell')) {
            this.curCell = data['curCell'];       
        } 
        if(ObjectCheck(data, 'targetCell')) {
            this.targetCell = data['targetCell'];
        }
        this.ai.curState.Set(data);
    }

    SetSpeed(newSpeed: number) {
        if (this.speed == newSpeed) {
            return;
        }

        if(newSpeed < 50) {
            console.log(`speed : ${newSpeed}`);
        }

        this.speed = newSpeed;
        var animSpeed = newSpeed / DragonDefaultSpeed;
        if (animSpeed < 0.5) {
            animSpeed = 0.5;
        } else if (animSpeed > 2.5) {
            animSpeed = 2.5;
        }
        this.Controller.speed = newSpeed;
        
        if(this.spine != null) {
            this.spine.timeScale = animSpeed;
        }
    }

    //스파인 사용중
    AnimationStart(name: string) {
        let loop = false;
        switch(name) {
            case 'attack': {
                name = 'dragon_attack';
            } break;
            case 'skill': {
                name = 'dragon_skill';
            } break;
            case 'walk': {
                name = 'dragon_walk';
                loop = true;
            } break;
            case 'idle': {
                name = 'dragon_idle';
                loop = true;
            } break;
            case 'win': {
                name = 'dragon_lose';
                loop = true;
            } break;
            case 'lose': {
                name = 'dragon_win';
                loop = true;
            } break;
            case 'casting': {
                name = 'dragon_casting';
            } break;
            case 'death': {
                name = 'dragon_death';
            } break;
            case 'hit': {
                name = 'dragon_hit';
            } break;
        }

        if(this.spine.animation == name) {
            return;
        }

        this.spine.setAnimation(0, name, loop);
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

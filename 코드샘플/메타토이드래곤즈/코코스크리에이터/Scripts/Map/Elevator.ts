
import { _decorator, Node, Vec3, Animation, Vec4 } from 'cc';
import { AIManager } from '../AI/AIManager';
import { ShadowEvent } from '../Character/ShadowEvent';
import { WorldAIDragon } from '../Character/WorldAIDragon';
import { WorldAIElevator } from '../Character/WorldAIElevator';
import { GameManager } from '../GameManager';
import { RandomInt } from '../Tools/SandboxTools';
import { CellSize, CellSizeBothX, ElevatorContainerLeft, ElevatorContainerRight, ElevatorY, MapManager } from './MapManager';
import { ObjectData } from './ObjectData';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = ElevatorData
 * DateTime = Mon Jan 10 2022 11:00:32 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = ElevatorData.ts
 * FileBasenameNoExtension = ElevatorData
 * URL = db://assets/Scripts/Map/ElevatorData.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
const ElevatorRandX = 15;

@ccclass('Elevator')
export class Elevator extends ObjectData { 
    @property(Node)
    protected container: Node = null;
    public get Container(): Node {
        return this.container;
    }
    @property(Node)
    protected delayNode: Node = null;
    public get DelayNode(): Node {
        return this.delayNode;
    }
    @property(Node)
    protected switchNode: Node = null;
    public get SwitchNode(): Node {
        return this.switchNode;
    }
    @property
    protected type: string = "";
    public set ElevatorType(value: string) {
        this.type = value;
    }
    public get ElevatorType(): string {
        return this.type;
    }
    @property(Node)
    protected containerShadow: Node = null;
    public get ContainerShadow(): Node {
        return this.containerShadow;
    }
    protected isContainer: boolean = false;
    public set IsContainer(value: boolean) {
        this.isContainer = value;
    }
    public get IsContainer(): boolean {
        return this.isContainer;
    }
    protected elevator: WorldAIElevator = null;
    public get Elevator(): WorldAIElevator {
        return this.elevator;
    }
    protected animation: Animation = null;
    protected mapManager: MapManager = null;

    protected pulleyTop: Node = null;
    public get PulleyTop(): Node {
        return this.pulleyTop;
    }
    public set PulleyTop(value: Node) {
        this.pulleyTop = value;
    }
    protected pulleyBottom: Node = null;
    public get PulleyBottom(): Node {
        return this.pulleyBottom;
    }
    public set PulleyBottom(value: Node) {
        this.pulleyBottom = value;
    }
    onLoad () {
        this.animation = this.getComponent(Animation);
    }
    Init() {
        super.Init();

        let aiManager = GameManager.GetManager<AIManager>(AIManager.Name);
        this.mapManager = GameManager.GetManager(MapManager.Name);
        this.elevator = aiManager.GetElevator(this.ElevatorType);
        if (this.elevator != null) {
            this.elevator.SetElevator(this);
        }
    }
    SetPosition(x: number, y:number = undefined, offset: number = 0): void {
        let position = 0;
        if(y == undefined) {
            y = this.pos.y;
            position = this.node.getPosition().y;
        } else {
            position = y * ElevatorY;
        }
        super.SetPosition(x, y);
        switch(this.ElevatorType) {
            case 'left': {
                const pos = new Vec3(x * CellSize.width + ElevatorContainerLeft - offset, position, 0);
                this.node.setPosition(pos);
            } break;
            case 'right': {
                const pos = new Vec3((x - 2) * CellSize.width + 2 * CellSizeBothX + ElevatorContainerRight - offset, position, 0);
                this.node.setPosition(pos);
            } break;
        }
    }
    
    GetWorldPosition(): Vec3 {
        let worldPos = this.container.getWorldPosition();
        return new Vec3(worldPos.x, worldPos.y, worldPos.z);
    }

    GetDelayPosition(): Vec3 {
        let worldPos = this.delayNode.getWorldPosition();
        return new Vec3(worldPos.x, worldPos.y, worldPos.z);
    }

    GetSwitchPosition(): Vec3 {
        let worldPos = this.switchNode.getWorldPosition();
        return new Vec3(worldPos.x, worldPos.y, worldPos.z);
    }
    
    GetRand(): number {
        return RandomInt(-ElevatorRandX, ElevatorRandX);
    }
    
    PushContainer(target: WorldAIDragon) {
        if (target == null || this.container == null || this.elevator == null) {
            return;
        }

        if(this.elevator.Container == null) {
            target.node.setParent(this.container, true);
            ShadowEvent.TriggerEvent({state:"ChangeParent", target: target.node, parent:this.ContainerShadow, info: new Vec4(0, 0, 30, 12)});
            // target.node.setPosition(new Vec3(target.node.position.x, 0, 0));
        } else {
            target.node.setParent(this.elevator.Container.Container, true);
            ShadowEvent.TriggerEvent({state:"ChangeParent", target: target.node, parent:this.elevator.Container.ContainerShadow, info: new Vec4(0, 0, 30, 12)});
            // target.node.setPosition(new Vec3(target.node.position.x, 0, 0));
        }
    }
    
    PopContainer(target: WorldAIDragon, parent: Node = null, parentShadow: Node = null) {
        if (target == null || this.mapManager == null || this.mapManager.nodeContainer['Character'] == null) {
            return;
        }

        if(parent == null) {
            parent = this.mapManager.nodeContainer['Character'];
            ShadowEvent.TriggerEvent({state:"ReturnParent", target:target.node, info: new Vec4(0, 0, 30, 12)});
        } else {
            if(parentShadow != null) {
                ShadowEvent.TriggerEvent({state:"ChangeParent", target:target.node, parent: parentShadow, info: new Vec4(0, 0, 30, 12)});
            } else {
                ShadowEvent.TriggerEvent({state:"ChangeParent", target:target.node, parent: parent, info: new Vec4(0, 0, 30, 12)});
            }
        }
        
        target.node.setParent(parent, true);
    }

    // AnimationStart(name: string) {
    //     this.unscheduleAllCallbacks();
    //     if (this.animation == null) {
    //         this.scheduleOnce(() => {
    //             this.AnimationStart(name)
    //         }, 0.1);
    //         return;
    //     }
    //     let anim = this.animation.getState(name);
    //     if (anim == null) {
    //         this.scheduleOnce(() => {
    //             this.AnimationStart(name)
    //         }, 0.1);
    //         return;
    //     }

    //     if (!anim.isPlaying) {
    //         this.animation.play(name);
    //     }
    // }
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

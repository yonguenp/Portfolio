
import { _decorator, Component, Node, Prefab, instantiate, math } from 'cc';
import { GameManager } from '../GameManager';
import { WorldAIElevator, eElevatorType } from '../Character/WorldAIElevator';
import { MapManager } from '../Map/MapManager';
import { RandomInt } from '../Tools/SandboxTools';
import { AIStateElevator, AIStateEscalator, AIStateIdle, AIStateMove } from './AIState/AIState';
import { WorldAIDragon } from '../Character/WorldAIDragon';
import { User, UserDragon } from '../User/User';
import { IManagerBase } from 'sb';
const { ccclass, property, executionOrder } = _decorator;

/**
 * Predefined variables
 * Name = AIManager
 * DateTime = Wed Dec 29 2021 12:16:21 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = AIManager.ts
 * FileBasenameNoExtension = AIManager
 * URL = db://assets/Scripts/Character/AI/AIManager.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */

@ccclass('AIManager')
@executionOrder(-1)
export class AIManager extends Component implements IManagerBase {
    public static Name: string = "AIManager";
    @property({ range: Prefab[1000], type:Prefab })
    prefabs: Prefab[] = [];

    dragons: WorldAIDragon[] = null;
    dragonDic: {} = null;

    myMap: MapManager = null;

    stateChangeTime: number = 5;
    curChangeTime: number = 0; 

    protected elevator_left: WorldAIElevator = null;
    protected elevator_right: WorldAIElevator = null;

    onLoad () {
        this.node.name = this.GetManagerName();
        this.dragons = [];
        this.dragonDic = {};
        this.elevator_left = new WorldAIElevator();
        this.elevator_left.init(eElevatorType.Left);
        this.elevator_right = new WorldAIElevator();
        this.elevator_right.init(eElevatorType.Right);

        GameManager.Instance.AddManager(this, true);
    }

    onDestroy() {
        GameManager.Instance.DelManager(this);
    }
    
    start () {
        this.myMap = GameManager.GetManager(MapManager.Name);
        this.curChangeTime = 0;
    }

    GetRandomDragon(tag: number): Node {
        if (this.prefabs == null || this.prefabs.length < 1) {
            return null;
        }
        const target = RandomInt(0, this.prefabs.length);
        let prefab = this.GetDragon(target);
        let node = instantiate(prefab);
        let targetData: UserDragon = null;

        let dragonData = User.Instance.DragonData;
        if(dragonData != null) {
            targetData = dragonData.AddDragonPrefab(tag, prefab, node);
        }
        let dragonAI = node.getComponent(WorldAIDragon);
        if (dragonAI == null) {
            dragonAI = node.addComponent(WorldAIDragon);
        }
        if (dragonAI != null) {
            dragonAI.Init();
            dragonAI.Data = targetData;
        }

        return node;
    }

    GetUserDragon(tag: number): Node {
        if (this.prefabs == null || this.prefabs.length < 1) {
            return null;
        }
        const target = RandomInt(0, this.prefabs.length);
        let prefab = this.GetDragon(target);
        let targetData: UserDragon = null;

        let dragonData = User.Instance.DragonData;
        if(dragonData != null) {
            targetData = dragonData.GetDragon(tag)
            if(targetData != null && targetData.Prefab != null) {
                prefab = targetData.Prefab;
            }
        }
        
        let node = instantiate(prefab);
        let dragonAI = node.getComponent(WorldAIDragon);
        if (dragonAI == null) {
            dragonAI = node.addComponent(WorldAIDragon);
        }
        if (dragonAI != null) {
            dragonAI.Init();
            dragonAI.Data = targetData;
        }

        return node;
    }

    GetDragon(target: number): Prefab {
        if (this.prefabs == null || this.prefabs.length < 1 || target < 0 || target >= this.prefabs.length) {
            return null;
        }

        return this.prefabs[target];
    }
    
    static AddDragon(dragon: WorldAIDragon): void {
        let myManager: AIManager = GameManager.GetManager(AIManager.Name)
        if (myManager == null || myManager.dragons == null || dragon == null) {
            return;
        }

        myManager.dragons.push(dragon);
        myManager.dragonDic[dragon.node.name] = dragon;//추후 DragonTag로
    }

    static DelDragon(dragon: WorldAIDragon): void {
        let myManager: AIManager = GameManager.GetManager(AIManager.Name)
        if (myManager == null || myManager.dragons == null || dragon == null) {
            return;
        }

        myManager.dragons = myManager.dragons.filter((cur) => dragon != cur);
        delete myManager.dragonDic[dragon.node.name];//추후 DragonTag로
    }

    GetManagerName(): string {
        return AIManager.Name;
    }


    Update(deltaTime: number): void {
        if (this.dragons != null && this.dragons.length > 0) {
            var stateChange = false;
            this.curChangeTime -= deltaTime;
            if(this.curChangeTime < 0) {
                this.curChangeTime = this.stateChangeTime;
                stateChange = true;
            }
    
            this.dragons.forEach(element => {
                if (stateChange && element.ai.curState.GetID() == AIStateIdle.ID) {
                    const rndState = RandomInt(0, 20);
    
                    switch(true) {
                        case (rndState >= 0 && rndState <= 3): {
                            this.ChangeMoveStateDragon(element);
                        } break;
                        // case 4: case 5: {
                        //     this.ChangeEscalatorStateDragon(element);
                        // } break;
                        case (rndState > 3 && rndState < 10): {
                            this.ChangeElevatorStateDragon(element);
                        } break;
                    }
                }
    
                if (!element.Update(deltaTime)) {
                    this.ChangeIdleStateDragon(element);
                }
            });
        }

        this.elevator_left.Update(deltaTime);
        this.elevator_right.Update(deltaTime);
    }

    protected ChangeIdleStateDragon(dragon: WorldAIDragon) {
        if (dragon.curCell == null) {
            dragon.SetState(AIStateIdle.ID);
            return;
        }

        dragon.SetState(AIStateIdle.ID, {
            curCell: dragon.curCell
        });
    }
    
    protected ChangeMoveStateDragon(dragon: WorldAIDragon) {
        const rndCell = this.myMap.GetFloorRandomCell(dragon.curCell.cellSize['y']);

        if (rndCell == dragon.curCell) {
            return;
        }

        dragon.SetState(AIStateMove.ID, {
            curCell: dragon.curCell,
            targetCell: rndCell,
        });
    }

    protected ChangeEscalatorStateDragon(dragon: WorldAIDragon) {
        const rndCell = this.myMap.GetRandomCell();
        if (rndCell == dragon.curCell) {
            return;
        }

        const curCell = dragon.curCell;
        const curFloor = curCell.cellSize['y'] as number;
        const rndFloor = rndCell.cellSize['y'] as number;
        if (curFloor == rndFloor) { 
            return;
        }

        let startNode = [];
        let endNode = [];

        if(curFloor > rndFloor) {
            var cFloor = curFloor;
            
            while(cFloor > rndFloor) {
                let escalator = this.myMap.GetFloorEscalator(cFloor - 1);
                startNode.push(escalator.GetEndNode());
                endNode.push(escalator.GetStartNode());
                cFloor--;
            }
        } else {
            var cFloor = curFloor;
            
            while(cFloor < rndFloor) {
                let escalator = this.myMap.GetFloorEscalator(cFloor);
                startNode.push(escalator.GetStartNode());
                endNode.push(escalator.GetEndNode());
                cFloor++;
            }
        }

        dragon.SetState(AIStateEscalator.ID, {
            curCell: dragon.curCell,
            targetCell: rndCell,
            startNode: startNode,
            endNode: endNode,
        });
    }

    protected ChangeElevatorStateDragon(dragon: WorldAIDragon) {
        const rndCell = this.myMap.GetRandomCell();
        if (rndCell == dragon.curCell) {
            return;
        }

        const curCell = dragon.curCell;
        const curFloor = curCell.cellSize['y'] as number;
        const rndFloor = rndCell.cellSize['y'] as number;
        if (curFloor == rndFloor) { 
            return;
        }

        let targetElevator: WorldAIElevator = null;
        let right =  this.elevator_right.GetElevator(curFloor);
        let left =  this.elevator_left.GetElevator(curFloor);

        const curPos = dragon.GetWorldPosition();
        
        const rightPos = math.Vec3.distance(curPos, right.GetWorldPosition());
        const leftPos = math.Vec3.distance(curPos, left.GetWorldPosition());

        if(rightPos > leftPos) {
            targetElevator = this.elevator_left;
        } else {
            targetElevator = this.elevator_right;
        }

        dragon.SetState(AIStateElevator.ID, {
            dragon: dragon,
            curCell: dragon.curCell,
            targetCell: rndCell,
            target: targetElevator,
        });
    }

    public GetElevator(type: string): WorldAIElevator {
        switch(type) {
            case 'left': {
                return this.elevator_left;
            }
            case 'right': {
                return this.elevator_right;
            }
        }
        return null;
    }
}
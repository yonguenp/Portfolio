
import { _decorator, Component, Node, Vec2, Vec3, UITransform, Size, Vec4, Prefab, instantiate, Animation, Label, input, Input, EventKeyboard, KeyCode } from 'cc';
import { WorldAIDragon } from '../Character/WorldAIDragon';
import { BuildingBaseData } from '../Data/BuildingData';
import { BuildingBaseTable, BuildingOpenTable } from '../Data/BuildingTable';
import { TableManager } from '../Data/TableManager';
import { GameManager } from '../GameManager';
import { ObjectCheck, RandomFloat, RandomInt, TimeString } from '../Tools/SandboxTools';
import { eBuildingState, eLandmarkType, User } from '../User/User';
import { Building } from './Building';
import { CellNode } from './CellNode';
import { Elevator } from './Elevator';
import { Escalator } from './Escalator';
import { ResourceManager, ResourcesType } from '../ResourceManager';
import { TimeObject } from '../Time/ITimeObject';
import { TimeManager } from '../Time/TimeManager';
import { StringTable } from '../Data/StringTable';
import { IManagerBase } from 'sb';
import { ShadowEvent } from '../Character/ShadowEvent';
const { ccclass, property, executionOrder, executeInEditMode, playOnFocus } = _decorator;

/**
 * Predefined variables
 * Name = MapManager
 * DateTime = Thu Dec 30 2021 16:48:48 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = MapManager.ts
 * FileBasenameNoExtension = MapManager
 * URL = db://assets/Scripts/Map/MapManager.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */

export const MetodHeadY: number = 102;
export const CellSizeX: number = 324;
export const CellSizeBothX: number = 348;
export const CellSizeY: number = 214;
export const UnerCellSizeY: number = 371;
export const ElevatorContainerLeft: number = -203;
export const ElevatorContainerRight: number = 37;
export const ElevatorFrameLeft: number = -120;
export const ElevatorFrameRight: number = 120;
export const ElevatorX: number = 252;
export const ElevatorY: number = 214;
export const PulleyTopY: number = 136;
export const PulleyBottomY: number = -80;
export const ElevatorSize: Size = new Size(ElevatorX, ElevatorY);
export const FloorSize: Size = new Size(2530, 160);
export const CellSize: Size = new Size(CellSizeX, CellSizeY);
export const CellSizeBoth: Size = new Size(CellSizeBothX, CellSizeY);
export const UnderCellSize: Size = new Size(CellSizeX, UnerCellSizeY);
export const UnderCellSizeBoth: Size = new Size(CellSizeBothX, UnerCellSizeY);
export const CellWallPositionY: number = 55;
export const CellWallSize: Size = new Size(24, 159);
export const SmallCellSpancing: Vec2 = new Vec2(46, 0);

enum eElevator {
    Left = 0,
    Right = 1,
    Container_Left = 2,
    Container_Right = 3
}

@ccclass('MapManager')
@executionOrder(-1)
// @executeInEditMode(true)
// @playOnFocus(true)
export class MapManager extends Component implements IManagerBase {
    public static Name: string = "MapManager";
    private isInit = false;
    objectWall: Prefab = null;
    @property(Prefab)
    default_building: Prefab = null;
    @property(Prefab)
    building_background: Prefab = null;
    nodeContainer: {} = null;
    emptyNode: Node = null;

    private prefabList: Prefab[] = null;

    buildingBaseTable: BuildingBaseTable = null;
    buildingOpenTable: BuildingOpenTable = null;

    protected mapSize: Vec4 = null;

    gridData: {} = {};
    mapData: {} = {};
    tempData: {} = {};
    buildingData: {} = {};
    wellData: {} = {};
    lockData: {} = {};
    groundData: {} = {};
    objectData: {} = {};

    myResource: ResourceManager = null;
    
    onLoad() {
        this.node.name = this.GetManagerName();

        GameManager.Instance.AddManager(this, false);
    }

    onDestroy() {
        // this.PVEMOVIE_END();
        GameManager.Instance.DelManager(this);
    }

    start() {
        // this.PVEMOVIE();
        this.Init();
    }

    // PVEMOVIE() {
    //     input.on(Input.EventType.KEY_UP, this.KEYEVENT);
    // }

    // PVEMOVIE_END() {
    //     input.off(Input.EventType.KEY_UP, this.KEYEVENT);
    // }

    static dummyTrain: Animation = null;
    // static anim: number = 0;
    // static start: boolean = false;
    // static playing: boolean = false;
    // static startValue: {} = null;
    // static endValue: {} = null;
    // static curTime: number = 0;
    // static maxTime: number = 0;
    // static animTime1: number = 2;
    // static animTime2: number = 7;
    // KEYEVENT(event: EventKeyboard) {
    //     console.log(event);

    //     switch(event.keyCode) {
    //         case KeyCode.SPACE: {
    //             UICanvas.globalMainCamRef.orthoHeight = 100;
    //         } break;
    //         case KeyCode.KEY_Q: {
    //             MapManager.dummyTrain.pause();
    //         } break;
    //         case KeyCode.KEY_W: {
    //             MapManager.dummyTrain.resume();
    //         } break;
    //         case KeyCode.KEY_E: {
    //             MapManager.anim = 1;
    //             MapManager.start = true;
    //         } break;
    //         case KeyCode.KEY_R: {
    //             MapManager.anim = 2;
    //             MapManager.start = true;
    //         } break;
    //     }
    // }

    // update(dt) {
    //     if(MapManager.start) {
    //         MapManager.start = false;
    //         MapManager.curTime = 0;
    //         switch(MapManager.anim) {
    //             case 1: {
    //                 MapManager.maxTime = MapManager.animTime1;
    //                 MapManager.startValue = {};
    //                 MapManager.startValue['startOrtho'] = UICanvas.globalMainCamRef.orthoHeight;
    //                 MapManager.startValue['targetOrtho'] = 480;
    //                 MapManager.startValue['startPosition'] = new Vec3(UICanvas.globalMainCamRef.node.position);
    //                 MapManager.startValue['targetPosition'] = new Vec3(1150, 465, 1000);
    //             } break;
    //             case 2: {
    //                 MapManager.maxTime = MapManager.animTime2;
    //                 MapManager.startValue = {};
    //                 MapManager.startValue['startPosition'] = new Vec3(UICanvas.globalMainCamRef.node.position);
    //                 MapManager.startValue['targetPosition'] = new Vec3(1150, 2200, 1000);
    //             } break;
    //         }
    //         MapManager.playing = true;
    //     }

    //     if(MapManager.playing) {
    //         MapManager.curTime += dt;
    //         switch(MapManager.anim) {
    //             case 1: {
    //                 if(MapManager.curTime >= MapManager.maxTime) {
    //                     UICanvas.globalMainCamRef.orthoHeight = BezierCurve(MapManager.startValue['startOrtho'], MapManager.startValue['targetOrtho'], MapManager.maxTime, MapManager.maxTime);
    //                     UICanvas.globalMainCamRef.node.setPosition(BezierCurveVec3(MapManager.startValue['startPosition'], MapManager.startValue['targetPosition'], MapManager.maxTime, MapManager.maxTime));
    //                     MapManager.playing = false;
    //                     return;
    //                 }
    //                 UICanvas.globalMainCamRef.orthoHeight = BezierCurve(MapManager.startValue['startOrtho'], MapManager.startValue['targetOrtho'], MapManager.curTime, MapManager.maxTime);
    //                 UICanvas.globalMainCamRef.node.setPosition(BezierCurveVec3(MapManager.startValue['startPosition'], MapManager.startValue['targetPosition'], MapManager.curTime, MapManager.maxTime));
    //             } break;
    //             case 2: {
    //                 if(MapManager.curTime >= MapManager.maxTime) {
    //                     UICanvas.globalMainCamRef.node.setPosition(BezierCurveVec3(MapManager.startValue['startPosition'], MapManager.startValue['targetPosition'], MapManager.maxTime, MapManager.maxTime));
    //                     MapManager.playing = false;
    //                     return;
    //                 }
    //                 UICanvas.globalMainCamRef.node.setPosition(BezierCurveVec3(MapManager.startValue['startPosition'], MapManager.startValue['targetPosition'], MapManager.curTime, MapManager.maxTime));
    //             } break;
    //         }
    //     }
    // }

    Init() {
        if(this.isInit) {
            return;
        }
        this.isInit = true;
        this.myResource = GameManager.GetManager(ResourceManager.Name);

        this.buildingBaseTable = TableManager.GetTable(BuildingBaseTable.Name);
        this.buildingOpenTable = TableManager.GetTable(BuildingOpenTable.Name);

        this.InitCell();
    }

    // private GetObjectData() {
    //     this.mapSize = User.Instance.GetMapData();
    //     const posX1 = 8;
    //     const posX2 = (this.mapSize.z - 1) * 5 + 2;
    //     return [
    //         {x:posX2,y:-1,prefab:1,type:'escalator'},
    //         {x:posX1,y:0,prefab:0,type:'escalator'},
    //         {x:posX2,y:1,prefab:1,type:'escalator'},
    //         {x:posX1,y:2,prefab:0,type:'escalator'},
    //         {x:posX2,y:3,prefab:1,type:'escalator'},
    //         {x:posX1,y:4,prefab:0,type:'escalator'},
    //         {x:posX2,y:5,prefab:1,type:'escalator'},
    //         {x:posX1,y:6,prefab:0,type:'escalator'},
    //         {x:posX2,y:7,prefab:1,type:'escalator'},
    //         {x:posX1,y:8,prefab:0,type:'escalator'},
    //         {x:posX2,y:9,prefab:1,type:'escalator'},
    //         {x:posX1,y:10,prefab:0,type:'escalator'},
    //     ];
    // }

    GetManagerName(): string {
        return MapManager.Name;
    }

    Update(deltaTime: number): void {
    }

    InitGrid() {
        this.gridData = {};
        this.mapSize = User.Instance.GetMapData();
        const mapOpenSize = User.Instance.GetOpenFloorData();

        this.gridData['floorStart'] = this.mapSize.y;
        this.gridData['cellStart'] = this.mapSize.x;
        this.gridData['floorCount'] = this.mapSize.w;
        this.gridData['cellCount'] = this.mapSize.z;
        this.gridData['openData'] = mapOpenSize;
        this.gridData['cellData'] = User.Instance.GetActiveGridData();

        console.log(this.gridData);
    }

    InitCell() 
    {
        this.node.removeAllChildren();

        this.InitGrid();
        this.buildingData = {};
        this.mapData = {};
        this.wellData = {};
        this.lockData = {};
        this.groundData = {};
        this.objectData = {};
        this.nodeContainer = {};

        const floorStart = this.gridData['floorStart'];
        const cellStart = this.gridData['cellStart'];
        const floorCount = this.gridData['floorCount'];
        const cellCount = this.gridData['cellCount'];
        const cellData = this.gridData['cellData'];
        const openData: Vec2 = this.gridData['openData'];
        const offset = this.GetOffset();

        //BG
        this.nodeContainer['BG'] = new Node('BG');
        this.node.addChild(this.nodeContainer['BG']);
        this.nodeContainer['BG'].setPosition(new Vec3(0, 0, 0));
        //
        //Back
        this.nodeContainer['Back'] = new Node('Back');
        this.node.addChild(this.nodeContainer['Back']);
        this.nodeContainer['Back'].setPosition(new Vec3(0, 0, 0));

        this.nodeContainer['BackElevator_Left'] = new Node('BackElevator_Left');
        this.nodeContainer['Back'].addChild(this.nodeContainer['BackElevator_Left']);
        this.nodeContainer['BackElevator_Left'].setPosition(new Vec3(cellStart * CellSize.width + ElevatorFrameLeft - offset, 0, 0));

        this.nodeContainer['BackElevator_Right'] = new Node('BackElevator_Right');
        this.nodeContainer['Back'].addChild(this.nodeContainer['BackElevator_Right']);
        this.nodeContainer['BackElevator_Right'].setPosition(new Vec3((cellCount - 2) * CellSize.width + 2 * CellSizeBothX + ElevatorFrameRight - offset, 0, 0));

        this.nodeContainer['BackCell'] = new Node('BackCell');
        this.nodeContainer['Back'].addChild(this.nodeContainer['BackCell']);
        this.nodeContainer['BackCell'].setPosition(new Vec3(0, 0, 0));

        this.nodeContainer['BackCellCharacter'] = new Node('BackCellCharacter');
        this.nodeContainer['Back'].addChild(this.nodeContainer['BackCellCharacter']);
        this.nodeContainer['BackCellCharacter'].setPosition(new Vec3(0, 0, 0));

        this.nodeContainer['BackCellShadow'] = new Node('BackCellShadow');
        this.nodeContainer['BackCellCharacter'].addChild(this.nodeContainer['BackCellShadow']);
        this.nodeContainer['BackCellShadow'].setPosition(new Vec3(0, 0, 0));

        this.nodeContainer['BackBuilding'] = new Node('BackBuilding');
        this.nodeContainer['Back'].addChild(this.nodeContainer['BackBuilding']);
        this.nodeContainer['BackBuilding'].setPosition(new Vec3(0, 0, 0));

        this.nodeContainer['BackFront'] = new Node('BackFront');
        this.nodeContainer['Back'].addChild(this.nodeContainer['BackFront']);
        this.nodeContainer['BackFront'].setPosition(new Vec3(0, 0, 0));
        //
        this.nodeContainer['Well'] = new Node('Well');
        this.node.addChild(this.nodeContainer['Well']);
        this.nodeContainer['Well'].setPosition(new Vec3(0, 0, 0));
        //Character
        this.nodeContainer['Shadow'] = instantiate(this.myResource.GetResource(ResourcesType.TOWN_PREFAB)['ShadowBeacon']);
        this.node.addChild(this.nodeContainer['Shadow']);
        this.nodeContainer['Shadow'].setPosition(new Vec3(0, 0, 0));
        this.nodeContainer['Character'] = new Node('Character');
        this.node.addChild(this.nodeContainer['Character']);
        this.nodeContainer['Character'].setPosition(new Vec3(0, 0, 0));
        //
        //Elevator
        this.nodeContainer['Elevator'] = new Node('Elevator');
        this.node.addChild(this.nodeContainer['Elevator']);
        this.nodeContainer['Elevator'].setPosition(new Vec3(0, 0, 0));
        //
        //Front
        this.nodeContainer['Front'] = new Node('Front');
        this.node.addChild(this.nodeContainer['Front']);
        this.nodeContainer['Front'].setPosition(new Vec3(0, 0, 0));

        this.nodeContainer['FrontElevator_Left'] = new Node('FrontElevator_Left');
        this.nodeContainer['Front'].addChild(this.nodeContainer['FrontElevator_Left']);
        this.nodeContainer['FrontElevator_Left'].setPosition(new Vec3(cellStart * CellSize.width + ElevatorFrameLeft - offset, 0, 0));

        this.nodeContainer['FrontElevator_Right'] = new Node('FrontElevator_Right');
        this.nodeContainer['Front'].addChild(this.nodeContainer['FrontElevator_Right']);
        this.nodeContainer['FrontElevator_Right'].setPosition(new Vec3((cellCount - 2) * CellSize.width + 2 * CellSizeBothX + ElevatorFrameRight - offset, 0, 0));

        this.nodeContainer['FrontCell'] = new Node('FrontCell');
        this.nodeContainer['Front'].addChild(this.nodeContainer['FrontCell']);
        this.nodeContainer['FrontCell'].setPosition(new Vec3(0, 0, 0));       
        
        this.emptyNode = new Node('EmptyNode');
        this.nodeContainer['Front'].addChild(this.emptyNode);
        this.emptyNode.setPosition(new Vec3(0, 0, 0));
        //

        this.prefabList = this.myResource.GetResource<Prefab>(ResourcesType.TOWN_PREFAB);
        if(this.prefabList != null) {
            this.objectWall = this.prefabList['wall_pillar'];
        }
        // if(this.building_background != null) {
        //     let target = instantiate(this.building_background);
        //     target.parent = this.backNode;
        //     var pos = target.getPosition();
        //     target.setPosition(cellStart * CellSize.width, floorStart * CellSize.height, pos.z);
        //     this.backGround = target.getComponent(UITransform);
        //     this.backGround.setContentSize(CellSize.width * (cellCount + 1 - cellStart), CellSize.height * (floorCount + 1 - floorStart));
        //     this.backGround.setAnchorPoint(Vec2.ZERO);
        // }

        this.PushElevatorContainer(eElevator.Container_Left, cellStart);
        this.PushElevatorContainer(eElevator.Container_Right, cellCount);

        var floor: number;
        for(floor = floorStart; floor <= floorCount; floor++) {
            if(openData.x <= floor && openData.y >= floor) {
                for(var cell = cellStart; cell < cellCount; cell++) {
                    this.PushCell(floor, cell);
                }
                for(var cell = cellStart; cell < cellCount;) {
                    cell = this.PushBuilding(cellData[floor][cell], floor, cell);
                }

                this.PushElevator(eElevator.Left, floor, cellStart);
                this.PushElevator(eElevator.Right, floor, cellCount);
            } else {
                this.PushLocked(floor, cellStart, cellCount);
            }
        }

        this.PushDeco();

        // const startPosX = cellStart * CellSize.width;
        // const endPosX = (cellCount + 1) * CellSize.width;
        // const startPosY = floorStart * CellSize.height - (CellSize.height - CellSize.y);
        // const sizeY = (floorCount + 1 - floorStart) * CellSize.height + (CellSize.height - CellSize.y);

        // if(this.wall_left != null) {
        //     let target = instantiate(this.wall_left);
        //     target.parent = this.node;
        //     var pos = target.getPosition();
        //     target.setPosition(startPosX, startPosY, pos.z);
        //     this.wallLeft = target.getComponent(UITransform);
        //     if(this.wallLeft != null) {
        //         this.wallLeft.setContentSize(new Size(this.wallLeft.contentSize.width, sizeY));
        //     }
        // }

        // if(this.wall_right != null) {
        //     let target = instantiate(this.wall_right);
        //     target.parent = this.node;
        //     var pos = target.getPosition();
        //     target.setPosition(endPosX, startPosY, pos.z);
        //     this.wallRight = target.getComponent(UITransform);
        //     if(this.wallRight != null) {
        //         this.wallRight.setContentSize(new Size(this.wallRight.contentSize.width, sizeY));
        //     }
        // }
    }

    // PushObject() {
    //     const openData = User.Instance.GetOpenFloorData();
    //     const objectData = this.GetObjectData();
    //     objectData.forEach(element => {
    //         switch(element.type) {
    //             case 'escalator':
    //                 if (!IsBetween(element.y, openData.x, openData.y - 1)) {
    //                     return;
    //                 }
    //                 break;
    //             default:
    //                 if (!IsBetween(element.y, openData.x, openData.y)) {
    //                     return;
    //                 }
    //                 break;
    //         }

    //         const targetObject = this.objects[element.prefab];
    //         let target = instantiate(targetObject);

    //         let objData = null;
    //         switch(element.type) {
    //             case 'escalator':
    //                 objData = target.getComponent(Escalator);
    //                 target.parent = this.frontNode;
            
    //                 objData.Init();
    //                 objData.SetPosition(element.x, element.y);
    //                 break;
    //         }

    //         if(this.objectData[element.type] == undefined) {
    //             this.objectData[element.type] = [];
    //         }
    //         this.objectData[element.type].push({node: target, data: objData});
    //     });
    // }

    // PushObjectFloor(floor: number) {
    //     const openData = User.Instance.GetOpenFloorData();
    //     const objectData = this.GetObjectData();
    //     objectData.forEach(element => {
    //         if((floor - 1) != element.y) {
    //             return;
    //         }

    //         switch(element.type) {
    //             case 'escalator':
    //                 if (!IsBetween(element.y, openData.x, openData.y - 1)) {
    //                     return;
    //                 }
    //                 break;
    //             default:
    //                 if (!IsBetween(element.y, openData.x, openData.y)) {
    //                     return;
    //                 }
    //                 break;
    //         }

    //         const targetObject = this.objects[element.prefab];
    //         let target = instantiate(targetObject);

    //         let objData = null;
    //         switch(element.type) {
    //             case 'escalator':
    //                 objData = target.getComponent(Escalator);
    //                 target.parent = this.frontNode;
            
    //                 objData.Init();
    //                 objData.SetPosition(element.x, element.y);
    //                 break;
    //         }

    //         if(this.objectData[element.type] == undefined) {
    //             this.objectData[element.type] = [];
    //         }
    //         this.objectData[element.type].push({node: target, data: objData});
    //     });
    // }

    // RefreshObjectFloor(floor: number) {
    //     const openData = User.Instance.GetOpenFloorData();
    //     const objectData = this.GetObjectData();
    //     objectData.forEach(element => {
    //         if((floor - 1) != element.y) {
    //             return;
    //         }

    //         switch(element.type) {
    //             case 'escalator':
    //                 if (!IsBetween(element.y, openData.x, openData.y - 1)) {
    //                     return;
    //                 }
    //                 break;
    //             default:
    //                 return;
    //         }

    //         if(this.objectData[element.type] != undefined) {
    //             this.objectData[element.type].forEach(escalrator => {
    //                 if(escalrator != null && escalrator.data.GetPosition().y == element.y) {
    //                     escalrator.data.SetPosition(element.x, element.y);
    //                 }
    //             });
    //         }
    //     });
    // }

    // PushGround(floor: number, cell: number, cellCount: number) {
    //     if(this.groundData[floor] != undefined) {
    //         this.groundData[floor].removeFromParent();
    //         delete this.groundData[floor];
    //     }
    //     if(this.ground != null) {
    //         let target = instantiate(this.ground);
    //         target.parent = this.backNode;
    //         var pos = target.getPosition();
    //         target.setPosition(cell * CellSize.width, floor * CellSize.height, pos.z);
    //         let uTarget = target.getComponent(UITransform);
    //         uTarget.setContentSize(cellCount * CellSize.width, uTarget.contentSize.height);

    //         this.groundData[floor] = target; 
    //     }
    // }
    PushDeco() {
        if(this.objectData['DecoNodes'] == null) {
            this.objectData['DecoNodes'] = [];
        } else {
            this.objectData['DecoNodes'].forEach(element => {
                element.destroy();
            });
            this.objectData['DecoNodes'] = [];
        }

        const cellStart = this.gridData['cellStart'];
        const cellCount = this.gridData['cellCount'];
        const floor = this.gridData['floorCount'] + 1;
        const offset = this.GetOffset();

        let fNode = this.nodeContainer['FrontCell'];
        if(fNode == null) {
            return;
        }

        let headPrefab = this.prefabList['metod_head'];
        if(headPrefab != null) {
            let node = instantiate(headPrefab);
            node.parent = fNode;

            const pos = new Vec3(7 + ((cellCount - 2) * CellSizeX + 2 * CellSizeBothX) * 0.5 - offset, floor * CellSize.height + MetodHeadY, 0);
            node.setPosition(pos);
            this.objectData['DecoNodes'].push(node);
        }

        this.PushDecoElevator('left');
        this.PushDecoElevator('right');

        for(var cell = cellStart ; cell < cellCount ; cell++) {
            let topPrefab: Prefab = null;
            var posX = cell;
            var bothSize = CellSizeBothX;
            switch(true) {
                case (cellStart == cell): {
                    bothSize = 0;
                    topPrefab = this.prefabList['top_l'];
                } break;
                case ((cellCount - 1) == cell): {
                    posX = cell - 1;
                    topPrefab = this.prefabList['top_r'];
                } break;
                default: {
                    posX = cell - 1;
                    topPrefab = this.prefabList['top_m'];
                } break;
            }

            if(topPrefab == null) {
                continue;
            }

            let node = instantiate(topPrefab);
            node.parent = fNode;

            const pos = new Vec3(posX * CellSize.width + bothSize - offset, floor * CellSize.height, 0);
            node.setPosition(pos);
            this.objectData['DecoNodes'].push(node);
        }
    }

    PushDecoElevator(dir: string) {
        const floor = this.gridData['floorCount'];

        let bParent = null;
        let fParent = null;
        switch(dir) {
            case 'left': {
                bParent = this.nodeContainer['BackElevator_Left'];
                fParent = this.nodeContainer['FrontElevator_Left'];
            } break;
            case 'right': {
                bParent = this.nodeContainer['BackElevator_Right'];
                fParent = this.nodeContainer['FrontElevator_Right'];
            } break;
        }

        if(bParent == null || fParent == null) {
            return;
        }

        let prefabInPulley = this.prefabList['elevator_pulley'];
        let posPulleyB = new Vec3(0, PulleyBottomY, 0);
        let posPulleyT = new Vec3(0, floor * CellSizeY + PulleyTopY, 0);
        if(prefabInPulley != null) {
            let bNode = instantiate(prefabInPulley);
            let tNode = instantiate(prefabInPulley);
            bParent.addChild(bNode);
            bParent.addChild(tNode);

            bNode.setPosition(posPulleyB);
            tNode.setPosition(posPulleyT);
            this.objectData['DecoNodes'].push(bNode);
            this.objectData['DecoNodes'].push(tNode);

            if(this.objectData['elevator_container'] != null) {
                let targetContainer: Elevator = null;
                const count = this.objectData['elevator_container'].length;
                for(var i = 0 ; i < count ; i++) {
                    if(this.objectData['elevator_container'][i] == null) {
                        continue;
                    }
                    if(dir == this.objectData['elevator_container'][i].data.ElevatorType) {
                        targetContainer = this.objectData['elevator_container'][i].data;
                        break;
                    }
                }

                if (targetContainer != null) {
                    targetContainer.PulleyBottom = bNode;
                    targetContainer.PulleyTop = tNode;
                }
            }
        }

        if(fParent != null) {
            let prefabOutTop = this.prefabList['top_elevator_out'];
            if(prefabOutTop != null) {
                let node = instantiate(prefabOutTop);
                fParent.addChild(node);
    
                node.setPosition(new Vec3(0, (floor + 1) * CellSizeY, 0));
                this.objectData['DecoNodes'].push(node);
            }
        }
    }

    PushWall(floor: number, cell: number, cellCount: number) {
        if(this.objectWall != null && !this.IsStartFloor(floor)) {
            if(this.wellData[floor] == undefined) {
                this.wellData[floor] = {};
            }
            if(this.wellData[floor][cell] != undefined) {
                this.wellData[floor][cell].removeFromParent();
                delete this.wellData[floor][cell];
            }
            let target = instantiate(this.objectWall);
            target.parent = this.nodeContainer['Well'];

            const isStart = this.IsStartCell(cell + cellCount);
            var width = (cell + cellCount - 1) * CellSize.width + CellSizeBothX;
            if(isStart) {
                width = CellSizeBothX;
            }
            
            target.setPosition(width - this.GetOffset(), floor * CellSize.height + CellWallPositionY, 0);
            let uTarget = target.getComponent(UITransform);
            uTarget.setContentSize(CellWallSize.width, CellWallSize.height);

            this.wellData[floor][cell] = target;
        }
    }

    RefreshWall(tag: string, floor: number, cell: number, cellCount: number) {
        if(this.wellData[tag] != undefined) {
            this.wellData[tag].forEach(element => {
                element.setPosition((cell + cellCount) * CellSize.width - this.GetOffset(), floor * CellSize.height, 0);
            });
        }
    }

    PushLocked(floor: number, cell: number, cellCount: number) {
        if(this.lockData[floor] == undefined) {
            this.lockData[floor] = [];
        }
        if(this.lockData[floor].length > 0) {
            this.lockData[floor].forEach(element => {
                element.destroy();
            });
            this.lockData[floor] = [];
        }

        let bNode: Node = this.nodeContainer['BackCell'];
        let bfNode: Node = this.nodeContainer['BackFront'];
        let fNode: Node = this.nodeContainer['FrontCell'];
        
        if(bNode == null || fNode == null || bfNode == null) {
            return;
        }

        const offset = this.GetOffset();

        for(var x = cell; x < cellCount ; x++) {
            this.PushCellBG(floor, x);
            const isStartCell = this.IsStartCell(x);
            const isEndCell = this.IsEndCell(x);
            if(!isStartCell && !isEndCell) {
                if(this.objectWall != null && !this.IsStartFloor(floor)) {
                    let target = instantiate(this.objectWall);
                    target.parent = this.nodeContainer['BackFront'];
        
                    var width = (x - 1) * CellSize.width + CellSizeBothX;
                    if(isStartCell) {
                        width = CellSizeBothX;
                    }
        
                    target.setPosition(width - offset, floor * CellSize.height + CellWallPositionY, 0);
                    let uTarget = target.getComponent(UITransform);
                    if(uTarget != null) {
                        uTarget.setContentSize(CellWallSize.width, CellWallSize.height);
                    }
        
                    this.lockData[floor].push(target);
                }
            }

            this.PushElevatorBG(eElevator.Left, floor, cell, this.lockData[floor]);
            this.PushElevatorBG(eElevator.Right, floor, cellCount, this.lockData[floor]);
        }

        if(this.prefabList['locked_background'] != null) {
            let target = instantiate(this.prefabList['locked_background']);
            if(target != null) {
                target.parent = bfNode;
                var pos = target.getPosition();
                target.setPosition(cell * CellSize.width - offset, floor * CellSize.height, pos.z);
                let uTarget = target.getComponent(UITransform);
                if(uTarget != null) {
                    uTarget.setContentSize((cellCount - 2) * CellSize.width + 2 * CellSizeBothX, CellSize.height);
                }
    
                this.lockData[floor].push(target);
            }
        }
    }

    PushCellBG(floor: number, cell: number) {
        if(this.objectData['CellBG'] == undefined) {
            this.objectData['CellBG'] = {};
        }
        if(this.objectData['CellBG'][floor] == undefined) {
            this.objectData['CellBG'][floor] = {};
        }
        if(this.objectData['CellBG'][floor][cell] == undefined) {
            this.objectData['CellBG'][floor][cell] = [];
        } else {
            this.objectData['CellBG'][floor][cell].forEach(element => {
                element.destroy();
            });
            this.objectData['CellBG'][floor][cell] = [];
        }

        let bNode: Node = this.nodeContainer['BackCell'];
        let bfNode: Node = this.nodeContainer['BackFront'];
        let fNode: Node = this.nodeContainer['FrontCell'];

        const isStartFloor = this.IsStartFloor(floor);
        const isStartCell = this.IsStartCell(cell);
        const isEndCell = this.IsEndCell(cell, -1);
        var cellX = cell;
        var bothSize = CellSizeBothX;
        if(isStartCell) {
            bothSize = 0;
        } else {
            cellX = cell - 1;
        }
        let cellSize = CellSize;
        if(isStartFloor) {
            cellSize = UnderCellSize;
            if(isStartCell) {
                cellSize = UnderCellSizeBoth;
            }
        }

        const pos = new Vec3(cellX * cellSize.width + bothSize - this.GetOffset(), floor * cellSize.height, 0);
        
        let prefabInFrontBG: Prefab = null;
        let prefabInBackFrontBG: Prefab = null;
        let prefabInBackBG: Prefab = null;
        let prefabOutBG: Prefab = null;
        let prefabOutBG_UnderLight: Prefab = null;
        let prefabEscalatorBack: Prefab = null;
        let prefabEscalatorFront: Prefab = null;
        let prefabWire: Prefab = null;
        let prefabWireEnd: Prefab = null;
        var dir = "";

        if(isStartFloor) {
            if(isStartCell) {
                prefabInBackBG = this.prefabList['under_c_b_l'];
                prefabInBackFrontBG = this.prefabList['under_c_f_l'];
                prefabOutBG = this.prefabList['under_c_o_l'];
                prefabOutBG_UnderLight = this.prefabList['under_c_o_l_light'];
                prefabEscalatorBack = this.prefabList['escalator_c_b_l'];
                prefabEscalatorFront = this.prefabList['escalator_c_f_l'];
                prefabWire = this.prefabList['wire_c_b_l'];
                prefabWireEnd = this.prefabList['wire_end_l'];
                dir = "left";
            } else if(isEndCell) {
                prefabInBackBG = this.prefabList['under_c_b_r'];
                prefabInBackFrontBG = this.prefabList['under_c_f_r'];
                prefabOutBG = this.prefabList['under_c_o_r'];
                prefabOutBG_UnderLight = this.prefabList['under_c_o_r_light'];
                prefabEscalatorBack = this.prefabList['escalator_c_b_r'];
                prefabEscalatorFront = this.prefabList['escalator_c_f_r'];
                prefabWire = this.prefabList['wire_c_b_r'];
                prefabWireEnd = this.prefabList['wire_end_r'];
                dir = "right";
            } else {
                prefabInBackBG = this.prefabList['under_c_b_m'];
                prefabInBackFrontBG = this.prefabList['under_c_f_m'];
                prefabOutBG = this.prefabList['under_c_o_m'];
                prefabOutBG_UnderLight = this.prefabList['under_c_o_m_light'];
                prefabWire = this.prefabList['wire_c_b_m'];
            }
        } else {
            if(isStartCell) {
                prefabInFrontBG = this.prefabList['body_in_l'];
                prefabOutBG = this.prefabList['body_out_l'];
            } else if(isEndCell) {
                prefabInFrontBG = this.prefabList['body_in_r'];
                prefabOutBG = this.prefabList['body_out_r'];
            } else {
                prefabInFrontBG = this.prefabList['body_in_m'];
                prefabOutBG = this.prefabList['body_out_m'];
            }
        }

        if(prefabInBackBG != null) {
            let node = instantiate(prefabInBackBG);
            bNode.addChild(node);

            node.setPosition(pos);
        }

        if(prefabInFrontBG != null) {
            let node = instantiate(prefabInFrontBG);
            bNode.addChild(node);

            node.setPosition(pos);
            this.objectData['CellBG'][floor][cell].push(node);
        }

        if(prefabWire != null) {
            let node = instantiate(prefabWire);
            bNode.addChild(node);

            node.setPosition(pos);
            this.objectData['CellBG'][floor][cell].push(node);
        }

        if(prefabInBackFrontBG != null) {
            let node = instantiate(prefabInBackFrontBG);
            bfNode.addChild(node);

            node.setPosition(pos);
            this.objectData['CellBG'][floor][cell].push(node);
        }

        if(prefabWireEnd != null) {
            let node = instantiate(prefabWireEnd);
            bfNode.addChild(node);

            node.setPosition(pos);
            this.objectData['CellBG'][floor][cell].push(node);
        }

        if(prefabEscalatorBack != null) {
            let node = instantiate(prefabEscalatorBack);
            bfNode.addChild(node);

            node.setPosition(pos);
            this.objectData['CellBG'][floor][cell].push(node);
        }

        if(prefabEscalatorFront != null) {
            let node: Node = null;
            let escalator: Escalator = null;
            if(this.objectData['escalator'] == undefined) {
                this.objectData['escalator'] = [];
            } else {
                this.objectData['escalator'].forEach(element => {
                    let e: {node: Node, data: Escalator} = element;
                    if(e == null) {
                        return;
                    }
                    if(e.data.DIR == dir) {
                        node = e.node;
                        escalator = e.data;
                    }
                });
            }
            if(node == null) {
                node = instantiate(prefabEscalatorFront);
                fNode.addChild(node);
    
                node.setPosition(pos);
                escalator = node.getComponent(Escalator);
                if(escalator != null) {
                    escalator.DIR = dir;
                    escalator.SetPosition(cell, floor);
                }
                this.objectData['escalator'].push({node: node, data: escalator});
            } else {
                node.setPosition(pos);
                if(escalator != null) {
                    escalator.SetPosition(cell, floor);
                }
            }
        }

        if(prefabOutBG != null) {
            let node = instantiate(prefabOutBG);
            fNode.addChild(node);

            node.setPosition(pos);
            this.objectData['CellBG'][floor][cell].push(node);
        }

        let subwayData = User.Instance.GetUserBuildingByTag(eLandmarkType.SUBWAY);
        if(prefabOutBG_UnderLight != null && subwayData != null && subwayData.State == eBuildingState.NORMAL) {
            let node = instantiate(prefabOutBG_UnderLight);
            fNode.addChild(node);

            node.setPosition(pos);
            this.objectData['CellBG'][floor][cell].push(node);
        }
    }

    PushCell(floor: number, cell: number) {
        if(this.mapData[floor] == undefined) {
            this.mapData[floor] = {};
        }
        this.PushCellBG(floor, cell);
        const isStart = this.IsStartCell(cell);
        const isEnd = this.IsEndCell(cell, -1);
        const isStartFloor = this.IsStartFloor(floor);
        if(this.mapData[floor][cell] != undefined) {
            this.mapData[floor][cell].SetCellPosition(cell, floor, isStart, isStartFloor, this.GetOffset());
            return;
        }

        let cellNode = new Node(`Cell`);
        let cellTransform = cellNode.addComponent(CellNode);
        cellTransform.SetAnchorPoint(Vec2.ZERO);
        cellTransform.SetCellPosition(cell, floor, isStart, isStartFloor, this.GetOffset());
        cellTransform.SetCellSize(1, 1, isStart, isEnd, isStartFloor);
        this.nodeContainer['BackCell'].addChild(cellNode);

        this.mapData[floor][cell] = cellTransform;
    }

    PushBuilding(tag: string, floor: number, cell: number) {
        const isStart = this.IsStartCell(cell);
        var isEnd = this.IsEndCell(cell, -1);
        var isBuilding = true;
        let parentNode: Node = this.nodeContainer['BackBuilding'];
        let targetBuilding: Prefab = null;
        let openData = this.buildingOpenTable.GetWithTag(tag);
        if(openData != null) {
            targetBuilding = this.myResource.GetResource(ResourcesType.BUILDING_PREFAB)[openData.BUILDING];
        }

        if(openData == null || targetBuilding == null) {
            if(!isEnd) {
                this.PushWall(floor, cell, 1);
            }
            return cell + 1;
        }

        let targetCell = this.GetCell(floor, cell);
        let target: Node = null;
        let image: Node = null;
        
        let buildingData = User.Instance.GetUserBuildingByTag(tag);
        if(buildingData != null) {
            switch(buildingData.State) {
                case eBuildingState.NORMAL:
                    target = instantiate(targetBuilding);
                    if(Number(tag) == eLandmarkType.SUBWAY) {
                        MapManager.dummyTrain = target.getComponent(Animation);
                    }
                    break;
                case eBuildingState.CONSTRUCTING: {
                    target = instantiate(this.myResource.GetResource(ResourcesType.UI_PREFAB)["constructionTimer"]);
                    let temp = this.myResource.GetResource(ResourcesType.BUILDING_PREFAB)['block_cloud'];
                    if(temp != null) {
                        image = instantiate(temp);
                    } else {
                        image = instantiate(targetBuilding);
                    }
                    let parentClone = target.getChildByName('container');
                    if(image != null) {
                        let parent = target;
                        if(parentClone != null) {
                            parent = parentClone;
                        }
                        image.setParent(parent, false);
                        image.setPosition(Vec3.ZERO);
                        let animation = image.getComponent(Animation);
                        if(animation != null) {
                            animation.stop();
                            animation.enabled = false;
                        }
                        if(temp == null) {
                            let building = image.getComponent(Building);
                            if(building != null) {
                                if(building.Spine != null) {
                                    building.Spine.paused = true;
                                }
                            }
                        }
                    }
                    let timer = target.getChildByName("timer");
                    if (timer != null) {
                        let block = target.getChildByName('block');
                        if(block != null) {
                            block.active = false;
                        }
                        let labelTitle = target.getChildByName("labelTitle").getComponent(Label);
                        if(labelTitle != null) {
                            labelTitle.node.active = false;
                            labelTitle.string = TableManager.GetTable<StringTable>(StringTable.Name).Get(100000068).TEXT;
                        }
                        let label = timer.getChildByName("labelTimer").getComponent(Label);
                        let timeObj = target.getComponent(TimeObject);
                        if(timeObj != null && label != null) {
                            timeObj.curTime = buildingData.ActiveTime;
                            timeObj.Refresh = () => 
                            {
                                const time = TimeManager.GetTimeCompare(timeObj.curTime);
    
                                if(time < 0) {
                                    timeObj.Refresh = undefined;
                                    // let label = target.getChildByName("timer");
                                    // if(label != null) {
                                    //     label.active = false;
                                    // }
                                    // let block = target.getChildByName('block');
                                    // if(block != null) {
                                    //     block.active = false;
                                    // }
                                    // let labelTitle = target.getChildByName("labelTitle").getComponent(Label);
                                    // if(labelTitle != null) {
                                    //     labelTitle.string = TableManager.GetTable<StringTable>(StringTable.Name).Get(100000073).TEXT;
                                    // }
                                    if(buildingData.State == eBuildingState.CONSTRUCTING) {
                                        buildingData.State = eBuildingState.CONSTRUCT_FINISHED;
                                        this.UpdateFloor();
                                    }
                                } else {
                                    label.string = TimeString(time);
                                }
                            }
                            timeObj.Refresh();
                        }
                    }
                } break;
                case eBuildingState.CONSTRUCT_FINISHED: {
                    target = instantiate(this.myResource.GetResource(ResourcesType.UI_PREFAB)["constructionTimer"]);
                    image = instantiate(targetBuilding);
                    let parentClone = target.getChildByName('container');
                    if(image != null) {
                        let parent = target;
                        if(parentClone != null) {
                            parent = parentClone;
                        }
                        image.setParent(parent, false);
                        image.setPosition(Vec3.ZERO);
                        let animation = image.getComponent(Animation);
                        if(animation != null) {
                            animation.stop();
                            animation.enabled = false;
                        }
                        let building = image.getComponent(Building);
                        if(building != null) {
                            if(building.Spine != null) {
                                building.Spine.paused = true;
                            }
                        }
                    }
                    let block = target.getChildByName('block');
                    if(block != null) {
                        block.active = true;
                    }
                    let label = target.getChildByName("timer");
                    if(label != null) {
                        label.active = false;
                    }
                    let labelTitle = target.getChildByName("labelTitle").getComponent(Label);
                    if(labelTitle != null) {
                        labelTitle.string = TableManager.GetTable<StringTable>(StringTable.Name).Get(100000073).TEXT;
                    }
                } break;
                case eBuildingState.LOCKED:{
                    target = instantiate(this.myResource.GetResource(ResourcesType.UI_PREFAB)["constructionTimer"]);
                    image = instantiate(targetBuilding);
                    let parentClone = target.getChildByName('container');
                    if(image != null) {
                        let parent = target;
                        if(parentClone != null) {
                            parent = parentClone;
                        }
                        image.setParent(parent, false);
                        image.setPosition(Vec3.ZERO);
                        let animation = image.getComponent(Animation);
                        if(animation != null) {
                            animation.stop();
                            animation.enabled = false;
                        }
                        let building = image.getComponent(Building);
                        if(building != null) {
                            if(building.Spine != null) {
                                building.Spine.paused = true;
                            }
                        }
                    }
                    let block = target.getChildByName('block');
                    if(block != null) {
                        block.active = false;
                    }
                    let label = target.getChildByName("timer");
                    if(label != null) {
                        label.active = false;
                    }
                    let labelTitle = target.getChildByName("labelTitle").getComponent(Label);
                    if(labelTitle != null) {
                        labelTitle.node.active = false;
                    }
                }
                case eBuildingState.NOT_BUILT:{
                    target = instantiate(this.myResource.GetResource(ResourcesType.UI_PREFAB)["constructionTimer"]);
                    image = instantiate(targetBuilding);
                    let parentClone = target.getChildByName('container');
                    if(image != null) {
                        let parent = target;
                        if(parentClone != null) {
                            parent = parentClone;
                        }
                        image.setParent(parent, false);
                        image.setPosition(Vec3.ZERO);
                        let animation = image.getComponent(Animation);
                        if(animation != null) {
                            animation.stop();
                            animation.enabled = false;
                        }
                        let building = image.getComponent(Building);
                        if(building != null) {
                            if(building.Spine != null) {
                                building.Spine.paused = true;
                            }
                        }
                    }
                    let block = target.getChildByName('block');
                    if(block != null) {
                        block.active = false;
                    }
                    let label = target.getChildByName("timer");
                    if(label != null) {
                        label.active = false;
                    }
                    let labelTitle = target.getChildByName("labelTitle").getComponent(Label);
                    if(labelTitle != null) {
                        labelTitle.node.active = false;
                    }
                }
                break;
                default: {
                    return cell + 1;
                }
            }
        }

        if(target == null) {
            return cell + 1;
        }

        let dataObj = image != null ? image.getComponent(Building) : target.getComponent(Building);
        let targetTransform = target.getComponent(UITransform);
        let objData: BuildingBaseData = null;
        if(dataObj != null) {
            objData = this.buildingBaseTable.Get(dataObj.BuildingNo);
            dataObj.SetPosition(cell, floor);
        }
        var targetSizeX = objData == null ? 1 : objData.SIZE;
        const targetSizeY = 1;

        isEnd = (cell + targetSizeX) == (this.gridData['cellCount']);

        var x = 0;
        if(isStart) {
            x++;
        }
        if(Number(tag) == eLandmarkType.SUBWAY) {
            targetSizeX = this.gridData['cellCount'];
            isEnd = (cell + targetSizeX) == (this.gridData['cellCount']);
            if(isEnd) {
                x++;
            }
        }

        if(this.IsStartFloor(floor)) {
            if(targetTransform != null) {
                targetTransform.setContentSize((targetSizeX - x) * CellSizeX + x * CellSizeBothX, targetSizeY * UnderCellSize.y);
            }
            if(image != null) {
                let imageTransform = image.getComponent(UITransform);
                if(imageTransform != null) {
                    imageTransform.setContentSize((targetSizeX - x) * CellSizeX + x * CellSizeBothX, targetSizeY * UnderCellSize.y);
                }
            }
        } else {
            if(targetTransform != null) {
                targetTransform.setContentSize((targetSizeX - x) * CellSizeX + x * CellSizeBothX, targetSizeY * CellSize.y);
            }
            if(image != null) {
                let imageTransform = image.getComponent(UITransform);
                if(imageTransform != null) {
                    imageTransform.setContentSize((targetSizeX - x) * CellSizeX + x * CellSizeBothX, targetSizeY * CellSize.y);
                }
            }
        }

        target.setParent(parentNode, false);
        if(targetCell != null) {
            target.worldPosition = targetCell.node.worldPosition;
        }

        const count = cell + targetSizeX;
        if(ObjectCheck(this.buildingData, tag) && this.buildingData[tag] != null) {
            this.RemoveBuilding(tag);
        }

        if (isBuilding) {
            // this.PushWall(tag, floor, cell, 0);
            if(this.gridData['cellCount'] != count) {
                this.PushWall(floor, cell, targetSizeX);
            }
        }

        this.buildingData[tag] = {node:target, dataObj:dataObj};
        return count;
    }

    PushElevatorContainer(type: number, cell: number) {
        let prefab: Prefab = null;
        switch (type) {
            case eElevator.Container_Left:
                prefab = this.prefabList['elevator_container_left'];
                break;
            case eElevator.Container_Right:
                prefab = this.prefabList['elevator_container_right'];
                break;
        }

        if(prefab == null) {
            return;
        }

        let target = instantiate(prefab);
        let objData = target.getComponent(Elevator);
        objData.ElevatorType = eElevator.Container_Left == type ? 'left' : 'right';
        objData.IsContainer = true;
        target.parent = this.nodeContainer['Elevator'];

        objData.SetPosition(cell, 0, this.GetOffset());
        objData.Init();

        if(this.objectData['elevator_container'] == undefined) {
            this.objectData['elevator_container'] = [];
        }
        this.objectData['elevator_container'].push({node: target, data: objData});
    }

    PushElevator(type: number, floor: number, cell: number) {
        let prefab: Prefab = null;
        switch (type) {
            case 0:
                prefab = this.prefabList['elevator_left'];
                break;
            case 1:
                prefab = this.prefabList['elevator_right'];
                break;
        }

        if(prefab == null) {
            return;
        }

        let target = instantiate(prefab);
        let objData = target.getComponent(Elevator);
        target.parent = this.nodeContainer['Elevator'];

        objData.SetPosition(cell, floor, this.GetOffset());
        objData.Init();

        this.PushElevatorBG(type, floor, cell);

        if(this.objectData['elevator'] == undefined) {
            this.objectData['elevator'] = [];
        }
        this.objectData['elevator'].push({node: target, data: objData});
    }

    PushElevatorBG(type: number, floor: number, cell: number, array: any = null) {
        let bNode: Node = null;
        let fNode: Node = null;
        switch (type) {
            case 0:
                bNode = this.nodeContainer['BackElevator_Left'];
                fNode = this.nodeContainer['FrontElevator_Left'];
                break;
            case 1:
                bNode = this.nodeContainer['BackElevator_Right'];
                fNode = this.nodeContainer['FrontElevator_Right'];
                break;
        }

        if(bNode == null || fNode == null) {
            return;
        }

        const isStartFloor = this.IsStartFloor(floor);
        const isEndFloor = this.IsEndFloor(floor);

        let prefabInBG: Prefab = null;
        let prefabInRail: Prefab = null;
        let prefabOutFrame: Prefab = null;
        var pos: Vec3 = null;

        if(isStartFloor) {
            prefabInBG = this.prefabList['under_elevator_in'];
            prefabInRail = this.prefabList['under_elevator_rail'];
            prefabOutFrame = this.prefabList['under_elevator_out'];
            pos = new Vec3(0, 0, 0);
        } else if(isEndFloor) {
            prefabInBG = this.prefabList['top_elevator_in'];
            prefabInRail = this.prefabList['top_elevator_rail'];
            prefabOutFrame = this.prefabList['middle_elevator_out'];
            pos = new Vec3(0, floor * CellSizeY, 0);
        } else {
            prefabInBG = this.prefabList['top_elevator_in'];
            prefabInRail = this.prefabList['middle_elevator_rail'];
            prefabOutFrame = this.prefabList['middle_elevator_out'];
            pos = new Vec3(0, floor * CellSizeY, 0);
        }

        if(prefabInBG != null) {
            let node = instantiate(prefabInBG);
            bNode.addChild(node);

            node.setPosition(pos);
            if(array != null) {
                array.push(node);
            }
        }

        if(prefabInRail != null) {
            let node = instantiate(prefabInRail);
            bNode.addChild(node);

            node.setPosition(pos);
            if(array != null) {
                array.push(node);
            }
        }

        if(prefabOutFrame != null) {
            let node = instantiate(prefabOutFrame);
            fNode.addChild(node);

            node.setPosition(pos);
            if(array != null) {
                array.push(node);
            }
        }
    }

    RefreshElevator(type: number, floor: number, cell: number, refreshCell: number) {
        this.objectData['elevator'].forEach(element => {
            if(element == null) {
                return;
            }
            let target:Elevator = element.data;
            if(target == null) {
                return;
            }
            let pos = target.GetPosition();
            if( pos.y == floor && pos.x == cell) {
                target.SetPosition(refreshCell, floor, this.GetOffset());
            }
        });
    }

    RefreshElevatorContainer(type: number, cell: number, refreshCell: number) {
        this.objectData['elevator_container'].forEach(element => {
            if(element == null) {
                return;
            }
            let target: Elevator = element.data;
            if(target == null) {
                return;
            }
            let pos = target.GetPosition();
            if(pos.x == cell) {
                target.SetPosition(refreshCell, undefined, this.GetOffset());
            }
        });
    }

    RefreshElevatorDeco(type: number, refreshCell: number) {
        const offset = this.GetOffset();
        switch(type) {
            case eElevator.Left: {
                let targetBNode = this.nodeContainer['BackElevator_Left'];
                if(targetBNode != null) {
                    targetBNode.setPosition(new Vec3(refreshCell * CellSize.width + ElevatorFrameLeft - offset, 0, 0));
                }
                let targetFNode = this.nodeContainer['FrontElevator_Left'];
                if(targetFNode != null) {
                    targetFNode.setPosition(new Vec3(refreshCell * CellSize.width + ElevatorFrameLeft - offset, 0, 0));
                }
            } break;
            case eElevator.Right: {
                let targetBNode = this.nodeContainer['BackElevator_Right'];
                if(targetBNode != null) {
                    targetBNode.setPosition(new Vec3((refreshCell - 2) * CellSize.width + 2 * CellSizeBothX + ElevatorFrameRight - offset, 0, 0));
                }
                let targetFNode = this.nodeContainer['FrontElevator_Right'];
                if(targetFNode != null) {
                    targetFNode.setPosition(new Vec3((refreshCell - 2) * CellSize.width + 2 * CellSizeBothX + ElevatorFrameRight - offset, 0, 0));
                }
            } break;
        }
    }

    GetFloorEscalator(floor: number): Escalator {
        if(this.objectData['escalator'] == undefined) {
            return null;
        }

        let floorEscalator = [];
        this.objectData['escalator'].forEach(element => {
            let data = element as {node: Node, data: Escalator};
            if(data != null && data.data.GetFloor() == floor) {
                floorEscalator.push(data.data);
            }
        });

        if(floorEscalator.length < 1) {
            return null;
        }

        const rndEsc = RandomInt(0, floorEscalator.length);

        return floorEscalator[rndEsc] as Escalator;
    }

    GetFloorEscalatorDir(floor: number, dir: string): Escalator {
        if(this.objectData['escalator'] == undefined) {
            return null;
        }

        let floorEscalator = [];
        this.objectData['escalator'].forEach(element => {
            let data = element as {node: Node, data: Escalator};
            if(data != null && data.data.GetFloor() == floor && data.data.DIR == dir) {
                floorEscalator.push(data.data);
            }
        });

        if(floorEscalator.length < 1) {
            return null;
        }

        const rndEsc = RandomInt(0, floorEscalator.length);

        return floorEscalator[rndEsc] as Escalator;
    }

    AddDragon(dragon: Node): void {//추가 및 시작 방 지정 AI동작 설정
        let rCell = this.GetRandomCell();
        const worldPos = rCell.node.getWorldPosition();
        const cellSize = rCell.GetCellSize();
        let randomWorld = new Vec3(RandomFloat(worldPos.x, worldPos.x + cellSize.width), worldPos.y + 50, worldPos.z);
        if(this.IsStartFloor(rCell.GetFloor())) {
            if(RandomInt(0, 2) == 0) {
                randomWorld = new Vec3(RandomFloat(worldPos.x, worldPos.x + cellSize.width), worldPos.y + 207, worldPos.z);
                dragon.setParent(this.nodeContainer['BackCellCharacter'], false);
                ShadowEvent.TriggerEvent({state: "ChangeParent", target: dragon, parent: this.nodeContainer['BackCellShadow'], info: new Vec4(0, 0, 30, 12)});
            } else {
                randomWorld = new Vec3(RandomFloat(worldPos.x, worldPos.x + cellSize.width), worldPos.y + 77, worldPos.z);
                dragon.setParent(this.nodeContainer['Character'], false);
                ShadowEvent.TriggerEvent({state: "ReturnParent", target: dragon, info: new Vec4(0, 0, 30, 12)});
            }
        } else {
            dragon.setParent(this.nodeContainer['Character'], false);
        }

        let dragonAI = dragon.getComponent(WorldAIDragon);
        if (dragonAI != null) {
            dragonAI.SetStateData({ curCell: rCell });
        }

        dragon.setScale(0.5, 0.5);
        dragon.setWorldPosition(randomWorld);
    }

    UpdateBuilding(jsonData: any) {
        if (ObjectCheck(jsonData, 'level') && ObjectCheck(jsonData, 'state') && ObjectCheck(jsonData, 'tag')) {
            let building = this.GetBuilding(jsonData['tag']);
            if(building != null) {
                const pos = building.GetPosition();
                this.PushBuilding(jsonData['tag'], pos.y, pos.x);
            }
        }
    }

    private SetFloor(floor: number, cellStart: number, cellCount:number) {
        const mapOpenSize = User.Instance.GetOpenFloorData();

        // this.PushGround(floor, cellStart, cellCount - cellStart + 1);
        // this.PushGround(floor + 1, cellStart, cellCount - cellStart + 1);
        if(this.mapData[floor] == undefined) {
            if(mapOpenSize.x <= floor && mapOpenSize.y >= floor) {
                for(var cell = cellStart; cell < cellCount; cell++) {
                    this.PushCell(floor, cell);
                }

                this.PushElevator(eElevator.Left, floor, cellStart);
                this.PushElevator(eElevator.Right, floor, cellCount);
                if(this.lockData[floor] != undefined) {
                    this.lockData[floor].forEach(element => {
                        element.removeFromParent();
                    });
                    this.lockData[floor] = [];
                }
            } else {
                this.PushLocked(floor, cellStart, cellCount);
            }
        } else {
            if(mapOpenSize.x <= floor && mapOpenSize.y >= floor) {
                for(var cell = cellStart; cell < cellCount; cell++) {
                    this.PushCell(floor, cell);
                }
                
                if(this.lockData[floor] != undefined) {
                    this.lockData[floor].forEach(element => {
                        element.removeFromParent();
                    });
                    this.lockData[floor] = [];
                }

                // this.RefreshObjectFloor(floor);
            }
        }
    }

    UpdateFloor() {
        this.mapSize = User.Instance.GetMapData();
        const cellStart = this.mapSize.x;
        const floorStart = this.mapSize.y;
        const cellCount = this.mapSize.z;
        const floorCount = this.mapSize.w;

        let gridData = User.Instance.GetActiveGridData();
        const keys = Object.keys(gridData);
        const keysCount = keys.length;

        const startPosX = cellStart * CellSize.width;
        const endPosX = (cellCount + 1 - cellStart) * CellSize.width;
        const startPosY = floorStart * CellSize.height - (CellSize.height - CellSize.y);
        const sizeY = (floorCount + 1 - floorStart) * CellSize.height + (CellSize.height - CellSize.y);

        const prevCellStart = this.gridData['cellStart'];
        const prevCellCount = this.gridData['cellCount'];

        this.InitGrid();

        for(var floor = floorStart; floor <= floorCount; floor++) {
            this.SetFloor(floor, cellStart, cellCount);
        }

        if(cellStart != prevCellStart || cellCount != prevCellCount) {
            for(var floor = floorStart; floor <= floorCount; floor++) {
                this.RefreshElevator(eElevator.Left, floor, prevCellStart, cellStart);
                this.RefreshElevator(eElevator.Right, floor, prevCellCount, cellCount);
            }
            this.RefreshElevatorDeco(eElevator.Left, cellStart);
            this.RefreshElevatorDeco(eElevator.Right, cellCount);
            this.RefreshElevatorContainer(eElevator.Left, prevCellStart, cellStart);
            this.RefreshElevatorContainer(eElevator.Right, prevCellCount, cellCount);
        }

        for(var i = 0; i < keysCount; i++) {
            const floor = keys[i];

            if(gridData[floor] == undefined) {
                continue;
            }

            let floorData = gridData[floor];
            const floorKeys = Object.keys(floorData);
            const floorKeysCount = floorKeys.length;

            for(var j = 0 ; j < floorKeysCount ; j++) {
                this.RemoveBuilding(floorData[floorKeys[j]]);
            }

            for(var cell = cellStart; cell < cellCount;) {
                cell = this.PushBuilding(gridData[floor][cell], Number(floor), cell);
            }
        }

        this.PushDeco();
    }

    GetBuilding(tag: number): Building {
        if(this.buildingData[tag] == undefined) {
            return null;
        }

        return this.buildingData[tag].dataObj;
    }

    RemoveBuilding(tag: number | string) {
        if(this.buildingData[tag] == undefined && this.buildingData[tag] == null) {
            return;
        }

        this.buildingData[tag].node.removeFromParent();
        if(this.wellData[tag] != undefined) {
            const wallCount = this.wellData[tag].length;
            for(var i = 0 ; i < wallCount ; i++) {
                this.wellData[tag][i].removeFromParent();
            }
            delete this.wellData[tag];
        }
        delete this.buildingData[tag];
    }

    GetRandomCell(): CellNode{
        const mapKeys = Object.keys(this.mapData);
        const randomFloor = mapKeys[RandomInt(0, mapKeys.length)];

        let floor = this.mapData[randomFloor];
        if(this.IsStartFloor(Number(randomFloor))) {
            const floorKeys = Object.keys(floor);
            const randomCell = RandomInt(1, floorKeys.length - 1);

            return this.GetCell(randomFloor, floorKeys[randomCell]);
        } else {
            const floorKeys = Object.keys(floor);
            const randomCell = RandomInt(0, floorKeys.length);

            return this.GetCell(randomFloor, floorKeys[randomCell]);
        }
    }

    GetFloorRandomCell(floor: number): CellNode{
        let floorData = this.mapData[floor];
        const floorKeys = Object.keys(floorData);
        const randomCell = RandomInt(0, floorKeys.length);

        return this.GetCell(floor, floorKeys[randomCell]);
    }

    GetCell(floor: number | string, cell: number | string): CellNode {
        const floorStart = this.mapSize.y;
        const cellStart = this.mapSize.x;
        const floorCount = this.mapSize.w;
        const cellCount = this.mapSize.z;
        if (floor < floorStart || floor > floorCount) {
            console.log(`floor None: ${floor}_${cell}`);
            return null;
        }
        if (cell < cellStart || cell > cellCount) {
            console.log(`cell None: ${floor}_${cell}`);
            return null;
        }
        return this.mapData[floor][cell] as CellNode;
    }

    GetCellByWorldPosition(floor: number, cell: number): Vec3 {
        let targetCell = this.GetCell(floor, cell);
        if (targetCell != null) {
            return targetCell.node.getWorldPosition();
        }

        return undefined;
    }

    IsStartFloor(floor: number): boolean {
        return this.gridData['openData'].x == floor;
    }

    IsEndFloor(floor: number, dummy: number = 0): boolean {
        return (this.gridData['openData'].y + dummy) == floor;
    }

    IsStartCell(cell: number): boolean {
        return this.gridData['cellStart'] == cell;
    }

    IsEndCell(cell: number, dummy: number = 0): boolean {
        return (this.gridData['cellCount'] + dummy) == cell;
    }

    GetOffset() {
        return ((this.gridData['cellCount'] - 2) * CellSize.width + 2 * CellSizeBoth.width) * 0.5;
    }
}
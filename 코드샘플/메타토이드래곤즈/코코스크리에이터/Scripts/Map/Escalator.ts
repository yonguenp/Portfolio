
import { _decorator, Component, Node, Vec3, Vec2 } from 'cc';
import { ObjectData } from './ObjectData';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = EscalatorData
 * DateTime = Wed Jan 05 2022 13:51:28 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = EscalatorData.ts
 * FileBasenameNoExtension = EscalatorData
 * URL = db://assets/Scripts/Map/EscalatorData.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
@ccclass('Escalator')
export class Escalator extends ObjectData {
    @property(Node)
    protected startNode: Node = null;
    @property(Node)
    protected endNode: Node = null;
    protected dir: string = "";
    public get DIR() {
        return this.dir;
    }
    public set DIR(value: string) {
        this.dir = value;
    }
    SetPosition(x: number, y:number): void {
        super.SetPosition(x, y);
    }
    GetWorldStartPosition(): Vec3 {
        return this.startNode.getWorldPosition();
    }
    GetWorldEndPosition(): Vec3 {
        return this.endNode.getWorldPosition();
    }
    GetFloor(): number {
        return this.GetPosition().y;
    }
    GetStartNode(): Node {
        return this.startNode;
    }
    GetEndNode(): Node {
        return this.endNode;
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

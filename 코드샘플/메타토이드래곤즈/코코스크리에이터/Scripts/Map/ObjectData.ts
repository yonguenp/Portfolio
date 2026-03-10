
import { _decorator, Component, Node, Vec2, Vec3 } from 'cc';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = ObjectData
 * DateTime = Mon Jan 10 2022 11:00:19 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = ObjectData.ts
 * FileBasenameNoExtension = ObjectData
 * URL = db://assets/Scripts/Map/ObjectData.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
@ccclass('ObjectData')
export abstract class ObjectData extends Component {
    protected pos: Vec2 = null;
    Init(): void {
    }

    SetPosition(x: number, y:number): void {
        this.pos = new Vec2(x, y);
    }

    GetPosition(): Vec2 {
        return this.pos;
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

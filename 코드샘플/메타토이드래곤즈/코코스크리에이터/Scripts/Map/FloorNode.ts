
import { _decorator, Component, UITransform, Size, Vec2 } from 'cc';
import { FloorSize } from './MapManager';
const { ccclass, requireComponent } = _decorator;

/**
 * Predefined variables
 * Name = FloorNode
 * DateTime = Mon Jan 03 2022 13:47:15 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = FloorNode.ts
 * FileBasenameNoExtension = FloorNode
 * URL = db://assets/Scripts/Map/FloorNode.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
@ccclass('FloorNode')
@requireComponent(UITransform)
export class FloorNode extends Component {
    floorTransform: UITransform = null;
    isInit: boolean = false;

    onLoad () {
        this.Init();
    }

    Init () {
        if(this.isInit) {
            return;
        }

        this.isInit = true;
        this.floorTransform = this.getComponent(UITransform);
    }

    SetSize(size: Size)
    {
        this.Init();
        this.floorTransform.setContentSize(size);
    }

    SetFloor() {
        this.Init();
        this.floorTransform.setContentSize(FloorSize)
    }

    SetAnchorPoint(anchor: Vec2) {
        this.Init();
        this.floorTransform.setAnchorPoint(anchor);
    }
}
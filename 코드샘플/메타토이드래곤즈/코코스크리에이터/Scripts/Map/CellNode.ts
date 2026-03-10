
import { _decorator, Component, UITransform, Size, Vec2, Vec3 } from 'cc';
import { Building } from './Building';
import { CellSize, CellSizeBoth, CellSizeBothX, UnderCellSize, UnderCellSizeBoth } from './MapManager';
const { ccclass, requireComponent } = _decorator;

/**
 * Predefined variables
 * Name = CellNode
 * DateTime = Mon Jan 03 2022 13:47:23 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = CellNode.ts
 * FileBasenameNoExtension = CellNode
 * URL = db://assets/Scripts/Map/CellNode.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
@ccclass('CellNode')
@requireComponent(UITransform)
export class CellNode extends Component {
    cellTransform: UITransform = null;
    isInit: boolean = false;
    cellSize: {} = {};
    protected curBuilding: Building = null;

    onLoad () {
        this.Init();
    }

    Init (): void {
        if(this.isInit) {
            return;
        }

        this.isInit = true;
        this.cellTransform = this.getComponent(UITransform);
    }

    SetCellPosition(x: number, y: number, isStart: boolean, isFloorStart: boolean, offset: number): void {
        this.Init();

        var cell = x;
        var bothSize = CellSizeBothX;
        if(isStart) {
            bothSize = 0;
        } else {
            cell = x - 1;
            this.cellSize['x'] = x;
        }
        this.cellSize['y'] = y;
        let cellSize = CellSize;
        if(isFloorStart) {
            cellSize = UnderCellSize;
            if(isStart) {
                cellSize = UnderCellSizeBoth;
            }
        }

        const pos = new Vec3(cell * cellSize.width + bothSize - offset, y * cellSize.height, 0);
        this.node.setPosition(pos);
    }

    SetSize(size: Size): void {
        this.Init();
        this.cellTransform.setContentSize(size);
    }

    SetCellSize(width: number, height: number, isStart: boolean, isEnd: boolean, isFloorStart: boolean): void {
        this.Init();

        if (height < 1) {
            height = 1;
        }
        
        let cellSize = CellSize;
        if(isStart || isEnd) {
            cellSize = CellSizeBoth;
        }
        if(isFloorStart) {
            cellSize = UnderCellSize;
            if(isStart || isEnd) {
                cellSize = UnderCellSizeBoth;
            }
        }

        this.cellSize['width'] = width;
        this.cellSize['height'] = height;

        const size = new Size(cellSize.width * width, cellSize.height * height);
        this.cellTransform.setContentSize(size);
    }

    SetAnchorPoint(anchor: Vec2): void {
        this.Init();
        this.cellTransform.setAnchorPoint(anchor);
    }

    GetAnchorPoint(): Vec2 {
        return this.cellTransform.anchorPoint;
    }

    GetCellSize(): Size {
        return this.cellTransform.contentSize;
    }

    GetWorldMoveSize(): { minX: number, maxX: number } {
        const minX = this.node.getWorldPosition().x;
        const maxX = minX + this.cellTransform.contentSize.x;

        return { minX: minX, maxX: maxX };
    }

    GetFloor(): number {
        return this.cellSize['y'];
    }

    AddBuilding(target: Building): void {
        this.curBuilding = target
        target.node.parent = this.node;
    }

    GetBuilding(): Building {
        return this.curBuilding;
    }
}
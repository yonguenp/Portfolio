
import { _decorator, Component, Node } from 'cc';
import { DataBase, NEED_ITEM } from './DataBase';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = InventoryData
 * DateTime = Wed Jan 12 2022 19:19:15 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = InventoryData.ts
 * FileBasenameNoExtension = InventoryData
 * URL = db://assets/Scripts/Data/InventoryData.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */

export class InventoryData extends DataBase {
    protected step: number = -1;
    public get STEP(): number {
        return this.step;
    }
    public set STEP(value: number) {
        this.step = value;
    }
    
    protected slot: number = -1;
    public get SLOT(): number {
        return this.slot;
    }
    public set SLOT(value: number) {
        this.slot = value;
    }

    protected need_item: NEED_ITEM[] = [];
    public get NEED_ITEM(): NEED_ITEM[] {
        return this.need_item;
    }
    public set NEED_ITEM(value: NEED_ITEM[]) {
        this.need_item = value;
    }
    
    protected cost_type: number = -1;
    public get COST_TYPE(): number {
        return this.cost_type;
    }
    public set COST_TYPE(value: number) {
        this.cost_type = value;
    }
    
    protected cost_num: number = -1;
    public get COST_NUM(): number {
        return this.cost_num;
    }
    public set COST_NUM(value: number) {
        this.cost_num = value;
    }
}

export class SlotCostData extends DataBase {
    protected buy_slot_count: number = -1;
    public get BUY_SLOT_COUNT(): number {
        return this.buy_slot_count;
    }
    public set BUY_SLOT_COUNT(value: number) {
        this.buy_slot_count = value;
    }
    
    protected cost_type: number = -1;
    public get COST_TYPE(): number {
        return this.cost_type;
    }
    public set COST_TYPE(value: number) {
        this.cost_type = value;
    }
    
    protected cost_num: number = -1;
    public get COST_NUM(): number {
        return this.cost_num;
    }
    public set COST_NUM(value: number) {
        this.cost_num = value;
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

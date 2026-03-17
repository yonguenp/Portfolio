
import { _decorator } from 'cc';
import { DataBase } from './DataBase';

/**
 * Predefined variables
 * Name = ItemData
 * DateTime = Wed Jan 12 2022 19:19:29 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = ItemData.ts
 * FileBasenameNoExtension = ItemData
 * URL = db://assets/Scripts/Data/ItemData.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */

 export class ItemBaseData extends DataBase {
    protected kind: number = -1;
    public get KIND(): number {
        return this.kind;
    }
    public set KIND(value: number) {
        this.kind = value;
    }
    
    protected icon: string = "";
    public get ICON(): string {
        return this.icon;
    }
    public set ICON(value: string) {
        this.icon = value;
    }
    
    protected grade: number = -1;
    public get GRADE(): number {
        return this.grade;
    }
    public set GRADE(value: number) {
        this.grade = value;
    }
    
    protected slot_use: string = "";
    public get SLOT_USE(): string {
        return this.slot_use;
    }
    public set SLOT_USE(value: string) {
        this.slot_use = value;
    }
    
    protected _name: number = -1;
    public get _NAME(): number {
        return this._name;
    }
    public set _NAME(value: number) {
        this._name = value;
    }
    
    protected _desc: number = -1;
    public get _DESC(): number {
        return this._desc;
    }
    public set _DESC(value: number) {
        this._desc = value;
    }
    
    protected sort: number = -1;
    public get SORT(): number {
        return this.sort;
    }
    public set SORT(value: number) {
        this.sort = value;
    }
    
    protected merge: number = -1;
    public get MERGE(): number {
        return this.merge;
    }
    public set MERGE(value: number) {
        this.merge = value;
    }
    
    protected sell: number = -1;
    public get SELL(): number {
        return this.sell;
    }
    public set SELL(value: number) {
        this.sell = value;
    }
    
    protected value: number = -1;
    public get VALUE(): number {
        return this.value;
    }
    public set VALUE(value: number) {
        this.value = value;
    }
}

export class ItemGroupData extends DataBase {
    protected group: number = -1;
    public get GROUP(): number {
        return this.group;
    }
    public set GROUP(value: number) {
        this.group = value;
    }
    
    protected type: string = "";
    public get TYPE(): string {
        return this.type;
    }
    public set TYPE(value: string) {
        this.type = value;
    }
    
    protected value: number = -1;
    public get VALUE(): number {
        return this.value;
    }
    public set VALUE(value: number) {
        this.value = value;
    }
    
    protected num: number = -1;
    public get NUM(): number {
        return this.num;
    }
    public set NUM(num: number) {
        this.num = num;
    }
    
    protected item_rate: number = -1;
    public get ITEM_RATE(): number {
        return this.item_rate;
    }
    public set ITEM_RATE(value: number) {
        this.item_rate = value;
    }
}

export class DefineResourceData extends DataBase {
    protected define: string = "";
    public get DEFINE(): string {
        return this.define;
    }
    public set DEFINE(value: string) {
        this.define = value;
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

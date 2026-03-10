
import { _decorator} from 'cc';
import { DataBase } from './DataBase';

/**
 * Predefined variables
 * Name = PartData
 * DateTime = Tue Mar 22 2022 17:43:53 GMT+0900 (대한민국 표준시)
 * Author = wonjun_gs
 * FileBasename = PartData.ts
 * FileBasenameNoExtension = PartData
 * URL = db://assets/Scripts/Data/PartData.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
//드래곤 장비 슬롯에 들어가는 장비 기본 데이터
 export class PartBaseData extends DataBase {
    protected item: number = -1
    public get ITEM():number{
        return this.item;
    }
    public set ITEM(value : number) {
        this.item = value
    }

    protected stat_type: string = "";
    public get STAT_TYPE(): string {
        return this.stat_type;
    }
    public set STAT_TYPE(value: string) {
        this.stat_type = value;
    }

    protected value: number = -1;
    public get VALUE(): number {
        return this.value;
    }
    public set VALUE(value: number) {
        this.value = value;
    }

    protected value_grow: number = -1;
    public get VALUE_GROW(): number {
        return this.value_grow;
    }
    public set VALUE_GROW(value: number) {
        this.value_grow = value;
    }

    protected sub : number[] = Array<number>(3)
    public get SUB() : number[] {
        return this.sub;
    }
    public set SUB(value : number[]) {
        this.sub = value;
    }

    protected sub_step : number[] = Array<number>(3)
    public get SUB_STEP() : number[] {
        return this.sub_step;
    }
    public set SUB_STEP(value : number[]) {
        this.sub_step = value;
    }

    protected set_group: number = -1;
    public get SET_GROUP(): number {
        return this.set_group;
    }
    public set SET_GROUP(value: number) {
        this.set_group = value;
    }

    protected unequip_cost_type: string = "";
    public get UNEQUIP_COST_TYPE(): string {
        return this.unequip_cost_type;
    }
    public set UNEQUIP_COST_TYPE(value: string) {
        this.unequip_cost_type = value;
    }
    protected unequip_cost_num: number = -1;
    public get UNEQUIP_COST_NUM(): number {
        return this.unequip_cost_num;
    }
    public set UNEQUIP_COST_NUM(value: number) {
        this.unequip_cost_num = value;
    }

    protected inf : number = -1
    public get INF(): number {
        return this.inf;
    }
    public set INF(value: number) {
        this.inf = value;
    }
}

/**
 * [1] Class member could be defined like this.
 * [2] Use `property` decorator if your want the member to be serializable.
 * [3] Your initialization goes here.
 * [4] Your update function goes here.
 *
 * Learn more about scripting: https://docs.cocos.com/creator/3.4/manual/en/scripting/
 * Learn more about CCClass: https://docs.cocos.com/creator/3.4/manual/en/scripting/decorator.html
 * Learn more about life-cycle callbacks: https://docs.cocos.com/creator/3.4/manual/en/scripting/life-cycle-callbacks.html
 */

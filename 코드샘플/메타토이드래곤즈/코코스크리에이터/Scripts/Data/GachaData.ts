
import { DataBase } from './DataBase';

/**
 * Predefined variables
 * Name = GachaData
 * DateTime = Thu Mar 17 2022 18:08:06 GMT+0900 (대한민국 표준시)
 * Author = blacktopaz
 * FileBasename = GachaData.ts
 * FileBasenameNoExtension = GachaData
 * URL = db://assets/Scripts/Data/GachaData.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
export class GachaShopData extends DataBase {
    protected key: number = -1;
    public get KEY(): number {
        return this.key;
    }
    public set KEY(value: number) {
        this.key = value;
    }
    protected _name: number = -1;
    public get _NAME(): number {
        return this._name;
    }
    public set _NAME(value: number) {
        this._name = value;
    }
    protected sort: number = -1;
    public get SORT(): number {
        return this.sort;
    }
    public set SORT(value: number) {
        this.sort = value;
    }
    protected group1: number = -1;
    public get GROUP1(): number {
        return this.group1;
    }
    public set GROUP1(value: number) {
        this.group1 = value;
    }
    protected group1_rate: number = -1;
    public get GROUP1_RATE(): number {
        return this.group1_rate;
    }
    public set GROUP1_RATE(value: number) {
        this.group1_rate = value;
    }
    protected group2: number = -1;
    public get GROUP2(): number {
        return this.group2;
    }
    public set GROUP2(value: number) {
        this.group2 = value;
    }
    protected group2_rate: number = -1;
    public get GROUP2_RATE(): number {
        return this.group2_rate;
    }
    public set GROUP2_RATE(value: number) {
        this.group2_rate = value;
    }
    protected cost_type: string = "";
    public get COST_TYPE(): string {
        return this.cost_type;
    }
    public set COST_TYPE(value: string) {
        this.cost_type = value;
    }
    protected cost_num: number = -1;
    public get COST_NUM(): number {
        return this.cost_num;
    }
    public set COST_NUM(value: number) {
        this.cost_num = value;
    }
    protected ticket: number = -1;
    public get TICKET(): number {
        return this.ticket;
    }
    public set TICKET(value: number) {
        this.ticket = value;
    }
}

export class GachaListData extends DataBase {
    protected key: number = -1;
    public get KEY(): number {
        return this.key;
    }
    public set KEY(value: number) {
        this.key = value;
    }
    protected group: number = -1;
    public get GROUP(): number {
        return this.group;
    }
    public set GROUP(value: number) {
        this.group = value;
    }
    protected type: number = -1;
    public get TYPE(): number {
        return this.type;
    }
    public set TYPE(value: number) {
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
    public set NUM(value: number) {
        this.num = value;
    }
    protected rate: number = -1;
    public get RATE(): number {
        return this.rate;
    }
    public set RATE(value: number) {
        this.rate = value;
    }
    protected fbx: number = -1;
    public get FBX(): number {
        return this.fbx;
    }
    public set FBX(value: number) {
        this.fbx = value;
    }
    protected grade: string = "";
    public get GRADE(): string {
        return this.grade;
    }
    public set GRADE(value: string) {
        this.grade = value;
    }
}
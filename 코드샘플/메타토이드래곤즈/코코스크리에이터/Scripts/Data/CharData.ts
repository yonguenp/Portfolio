import { DataBase } from "./DataBase";

/**
 * Predefined variables
 * Name = CharData
 * DateTime = Mon Feb 21 2022 15:56:29 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = CharData.ts
 * FileBasenameNoExtension = CharData
 * URL = db://assets/Scripts/Data/CharData.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
export class CharBaseData extends DataBase {
    protected factor: number = -1;
    public get FACTOR(): number {
        return this.factor;
    }
    public set FACTOR(value: number) {
        this.factor = value;
    }
    protected grade: number = -1;
    public get GRADE(): number {
        return this.grade;
    }
    public set GRADE(value: number) {
        this.grade = value;
    }
    protected element: number = -1;
    public get ELEMENT(): number {
        return this.element;
    }
    public set ELEMENT(value: number) {
        this.element = value;
    }
    protected image: string = "";
    public get IMAGE(): string {
        return this.image;
    }
    public set IMAGE(value: string) {
        this.image = value;
    }
    protected thumbnail: string = "";
    public get THUMBNAIL(): string {
        return this.thumbnail;
    }
    public set THUMBNAIL(value: string) {
        this.thumbnail = value;
    }
    protected position: number = -1;
    public get POSITION(): number {
        return this.position;
    }
    public set POSITION(value: number) {
        this.position = value;
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
    protected atk: number = -1;
    public get ATK(): number {
        return this.atk;
    }
    public set ATK(value: number) {
        this.atk = value;
    }
    protected def: number = -1;
    public get DEF(): number {
        return this.def;
    }
    public set DEF(value: number) {
        this.def = value;
    }
    protected hp: number = -1;
    public get HP(): number {
        return this.hp;
    }
    public set HP(value: number) {
        this.hp = value;
    }
    protected critical: number = -1;
    public get CRITICAL(): number {
        return this.critical;
    }
    public set CRITICAL(value: number) {
        this.critical = value;
    }
    protected critical_dmg: number = -1;
    public get CRITICAL_DMG(): number {
        return this.critical_dmg;
    }
    public set CRITICAL_DMG(value: number) {
        this.critical_dmg = value;
    }
    protected speed: number = -1;
    public get SPEED(): number {
        return this.speed;
    }
    public set SPEED(value: number) {
        this.speed = value;
    }
    protected normal_skill: number = -1;
    public get NORMAL_SKILL(): number {
        return this.normal_skill;
    }
    public set NORMAL_SKILL(value: number) {
        this.normal_skill = value;
    }
    protected skill1: number = -1;
    public get SKILL1(): number {
        return this.skill1;
    }
    public set SKILL1(value: number) {
        this.skill1 = value;
    }
    protected skill2: number = -1;
    public get SKILL2(): number {
        return this.skill2;
    }
    public set SKILL2(value: number) {
        this.skill2 = value;
    }
}

export class CharGradeData extends DataBase {
    protected _name: number = -1;
    public get _NAME(): number {
        return this._name;
    }
    public set _NAME(value: number) {
        this._name = value;
    }
    protected stat_point: number = -1;
    public get STAT_POINT(): number {
        return this.stat_point;
    }
    public set STAT_POINT(value: number) {
        this.stat_point = value;
    }
}

export class CharExpData extends DataBase {
    protected level: number = -1;
    public get LEVEL(): number {
        return this.level;
    }
    public set LEVEL(value: number) {
        this.level = value;
    }
    protected exp: number = -1;
    public get EXP(): number {
        return this.exp;
    }
    public set EXP(value: number) {
        this.exp = value;
    }
    protected total_exp: number = -1;
    public get TOTAL_EXP(): number {
        return this.total_exp;
    }
    public set TOTAL_EXP(value: number) {
        this.total_exp = value;
    }
    protected open_equip_slot: number = -1;
    public get OPEN_EQUIP_SLOT(): number {
        return this.open_equip_slot;
    }
    public set OPEN_EQUIP_SLOT(value: number) {
        this.open_equip_slot = value;
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

import { DataBase } from "./DataBase";

/**
 * Predefined variables
 * Name = StatData
 * DateTime = Mon Feb 21 2022 15:56:08 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = StatData.ts
 * FileBasenameNoExtension = StatData
 * URL = db://assets/Scripts/Data/StatData.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */

export enum UserType {
    None = 0,
    User = 1,
    Monster = 2
}

export class StatFactorData extends DataBase {
    protected user: UserType = -1;
    public get USER(): UserType {
        return this.user;
    }
    public set USER(value: UserType) {
        this.user = value;
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

import { DataBase } from './DataBase';

/**
 * Predefined variables
 * Name = AccountData
 * DateTime = Mon Feb 21 2022 15:54:15 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = AccountData.ts
 * FileBasenameNoExtension = AccountData
 * URL = db://assets/Scripts/Data/AccountData.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
export class AccountData extends DataBase {
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
    protected max_stamina: number = -1;
    public get MAX_STAMINA(): number {
        return this.max_stamina;
    }
    public set MAX_STAMINA(value: number) {
        this.max_stamina = value;
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

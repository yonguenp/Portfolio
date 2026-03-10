
import { _decorator } from 'cc';

/**
 * Predefined variables
 * Name = DataBase
 * DateTime = Tue Jan 11 2022 10:38:23 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = DataBase.ts
 * FileBasenameNoExtension = DataBase
 * URL = db://assets/Scripts/Data/DataBase.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
export abstract class DataBase {
    protected index: PropertyKey;
    public get Index() { return this.index; }
    public set Index(value) { this.index = value; }
}

export class NEED_ITEM {
    protected item_no: number = -1; 
    public get ITEM_NO(): number {
        return this.item_no;
    }
    public set ITEM_NO(value: number) {
        this.item_no = value;
    }
    protected item_count: number = 1;
    public get ITEM_COUNT(): number {
        return this.item_count;
    }
    public set ITEM_COUNT(value: number) {
        this.item_count = value;
    }
};

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

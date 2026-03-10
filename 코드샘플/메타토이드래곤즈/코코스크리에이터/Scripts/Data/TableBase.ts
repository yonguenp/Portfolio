
import { _decorator } from 'cc';
import { ITableBase } from 'sb';
import { DataBase } from './DataBase';

/**
 * Predefined variables
 * Name = TableBase
 * DateTime = Tue Jan 11 2022 10:38:10 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = TableBase.ts
 * FileBasenameNoExtension = TableBase
 * URL = db://assets/Scripts/Data/TableBase.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
export abstract class TableBase<T extends DataBase> implements ITableBase {
    protected datas: {} = null;

    public Init (): void {
        this.DataClear();
    }
    public DataClear(): void {
        this.datas = {};
    }
    public Get(index: PropertyKey): T {
        if (this.datas == undefined) return null;
        if (Object.getOwnPropertyDescriptor(this.datas, index) == undefined) return null;
        return this.datas[index] as T;
    }
    public GetAll() : T[]
    {
        const array = Object.keys(this.datas).map(index => {
            let data = this.datas[index];
            return data;
        });
        return array
    }
    public Add(data: T): void {
        if (this.datas == undefined) return;
        if (Object.getOwnPropertyDescriptor(this.datas, data.Index) != undefined) return;
        this.datas[data.Index] = data;
    }
    public SetTable(jsonData: any): void {
        
    }
}
 
export abstract class MultiTableBase<T extends DataBase> implements ITableBase {
    protected datas: {} = null;

    public Init (): void {
        this.DataClear();
    }
    public DataClear(): void {
        this.datas = {};
    }
    public Get(index: PropertyKey): T[] {
        if (this.datas == undefined) return null;
        if (Object.getOwnPropertyDescriptor(this.datas, index) == undefined) return null;
        return this.datas[index] as T[];
    }
    public Add(data: T): void {
        if (this.datas == undefined) this.datas = {};
        if (Object.getOwnPropertyDescriptor(this.datas, data.Index) == undefined) this.datas[data.Index] = [];
        this.datas[data.Index].push(data)
    }
    public SetTable(jsonData: any): void {
        
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


import { _decorator } from 'cc';
import { DefineResourceData, ItemBaseData, ItemGroupData } from './ItemData';
import { MultiTableBase, TableBase } from './TableBase';

/**
 * Predefined variables
 * Name = ItemTable
 * DateTime = Wed Jan 12 2022 19:19:35 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = ItemTable.ts
 * FileBasenameNoExtension = ItemTable
 * URL = db://assets/Scripts/Data/ItemTable.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
export class ItemBaseTable extends TableBase<ItemBaseData> {
    public static Name: string = "ItemBaseTable";

    public SetTable(dataArray: Array<Array<string>>): void {
        if(dataArray == null || dataArray.length < 1) {
            return;
        }

        this.DataClear();
        const rowName = dataArray[0];
        const arrayCount = dataArray.length;
        const dataCount = rowName.length;
        let table = this;

        for(var i = 1 ; i < arrayCount ; i++) {
            const datas = dataArray[i];
            
            let data = new ItemBaseData();
            for (var curData = 0 ; curData < dataCount ; curData++) {
                const curRow = rowName[curData];
                const target = datas[curData];

                switch (curRow) {
                    case 'KEY':
                        data.Index = target;
                        break;
                    case 'KIND':
                        data.KIND = Number(target);
                        break;
                    case 'ICON':
                        data.ICON = target;
                        break;
                    case 'GRADE':
                        data.GRADE = Number(target);
                        break;
                    case 'SLOT_USE':
                        data.SLOT_USE = target;
                        break;
                    case '_NAME':
                        data._NAME = Number(target);
                        break;
                    case '_DESC':
                        data._DESC = Number(target);
                        break;
                    case 'SORT':
                        data.SORT = Number(target);
                        break;
                    case 'MERGE':
                        data.MERGE = Number(target);
                        break;
                    case 'SELL':
                        data.SELL = Number(target);
                        break;
                    case 'VALUE':
                        data.VALUE = Number(target);
                        break;
                }
            }
            table.Add(data);
        }
    }
}
 
export class ItemGroupTable extends MultiTableBase<ItemGroupData> {
    public static Name: string = "ItemGroupTable";

    public SetTable(dataArray: Array<Array<string>>): void {
        if(dataArray == null || dataArray.length < 1) {
            return;
        }
        this.DataClear();
        const rowName = dataArray[0];
        const arrayCount = dataArray.length;
        const dataCount = rowName.length;
        let table = this;

        for(var i = 1 ; i < arrayCount ; i++) {
            const datas = dataArray[i];
            
            let data = new ItemGroupData();
            for (var curData = 0 ; curData < dataCount ; curData++) {
                const curRow = rowName[curData];
                const target = datas[curData];

                switch (curRow) {
                    case 'GROUP':
                        data.Index = target;
                        data.GROUP = Number(target);
                        break;
                    case 'TYPE':
                        data.TYPE = target;
                        break;
                    case 'VALUE':
                        data.VALUE = Number(target);
                        break;
                    case 'NUM':
                        data.NUM = Number(target);
                        break;
                    case 'ITEM_RATE':
                        data.ITEM_RATE = Number(target);
                        break;
                }
            }
            table.Add(data);
        }
    }
}
 
export class DefineResourceTable extends TableBase<DefineResourceData> {
    public static Name: string = "DefineResourceTable";

    public SetTable(dataArray: Array<Array<string>>): void {
        if(dataArray == null || dataArray.length < 1) {
            return;
        }
        this.DataClear();
        const rowName = dataArray[0];
        const arrayCount = dataArray.length;
        const dataCount = rowName.length;
        let table = this;

        for(var i = 1 ; i < arrayCount ; i++) {
            const datas = dataArray[i];
            
            let data = new DefineResourceData();
            for (var curData = 0 ; curData < dataCount ; curData++) {
                const curRow = rowName[curData];
                const target = datas[curData];

                switch (curRow) {
                    case 'KEY':
                        data.Index = target;
                        break;
                    case 'DEFINE':
                        data.DEFINE = target;
                        break;
                }
            }
            table.Add(data);
        }
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


import { _decorator } from 'cc';
import { NEED_ITEM } from './DataBase';
import { InventoryData, SlotCostData } from './InventoryData';
import { TableBase } from './TableBase';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = InventoryTable
 * DateTime = Wed Jan 12 2022 19:19:23 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = InventoryTable.ts
 * FileBasenameNoExtension = InventoryTable
 * URL = db://assets/Scripts/Data/InventoryTable.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
export class InventoryTable extends TableBase<InventoryData> {
    public static Name: string = "InventoryTable";

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
            
            let data = new InventoryData();
            let item1: NEED_ITEM = new NEED_ITEM();
            let item2: NEED_ITEM = new NEED_ITEM();
            let item3: NEED_ITEM = new NEED_ITEM();
            for (var curData = 0 ; curData < dataCount ; curData++) {
                const curRow = rowName[curData];
                const target = datas[curData];
                switch (curRow) {
                    case 'KEY':
                        data.Index = target;
                        break;
                    case 'STEP':
                        data.STEP = Number(target);
                        break;
                    case 'SLOT':
                        data.SLOT = Number(target);
                        break;
                    case 'NEED_ITEM_1':
                        if(item1 != null) {
                            item1.ITEM_NO = Number(target);
                        }
                        break;
                    case 'NEED_ITEM_2':
                        if(item2 != null) {
                            item2.ITEM_NO = Number(target);
                        }
                        break;
                    case 'NEED_ITEM_3':
                        if(item3 != null) {
                            item3.ITEM_NO = Number(target);
                        }
                        break;
                    case 'NEED_ITEM_1_NUM':
                        const count1 = Number(target);
                        if(item1 != null && count1 > 0) {
                            item1.ITEM_COUNT = count1;
                            data.NEED_ITEM.push(item1);
                        }
                        break;
                    case 'NEED_ITEM_2_NUM':
                        const count2 = Number(target);
                        if(item2 != null && count2 > 0) {
                            item2.ITEM_COUNT = count2;
                            data.NEED_ITEM.push(item2);
                        }
                        break;
                    case 'NEED_ITEM_3_NUM':
                        const count3 = Number(target);
                        if(item3 != null && count3 > 0) {
                            item3.ITEM_COUNT = count3;
                            data.NEED_ITEM.push(item3);
                        }
                        break;
                    case 'COST_TYPE':
                        data.COST_TYPE = Number(target);
                        break;
                    case 'COST_NUM':
                        data.COST_NUM = Number(target);
                        break;
                }
            }
            table.Add(data);
        }
    }
}
 
export class SlotCostTable extends TableBase<SlotCostData> {
    public static Name: string = "SlotCostTable";

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
            
            let data = new SlotCostData();
            for (var curData = 0 ; curData < dataCount ; curData++) {
                const curRow = rowName[curData];
                const target = datas[curData];
                
                switch (curRow) {
                    case 'KEY':
                        data.Index = target;
                        break;
                    case 'BUY_SLOT_COUNT':
                        data.BUY_SLOT_COUNT = Number(target);
                        break;
                    case 'COST_TYPE':
                        data.COST_TYPE = Number(target);
                        break;
                    case 'COST_NUM':
                        data.COST_NUM = Number(target);
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

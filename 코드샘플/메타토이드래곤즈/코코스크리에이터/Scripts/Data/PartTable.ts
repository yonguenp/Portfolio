
import { _decorator} from 'cc';
import { PartBaseData } from './PartData';
import { TableBase } from './TableBase';

/**
 * Predefined variables
 * Name = PartTable
 * DateTime = Tue Mar 22 2022 17:44:03 GMT+0900 (대한민국 표준시)
 * Author = wonjun_gs
 * FileBasename = PartTable.ts
 * FileBasenameNoExtension = PartTable
 * URL = db://assets/Scripts/Data/PartTable.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */

 export class PartTable extends TableBase<PartBaseData> {
    public static Name: string = "PartTable";

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
            
            let data = new PartBaseData();
            for (var curData = 0 ; curData < dataCount ; curData++) {
                const curRow = rowName[curData];
                const target = datas[curData];
                let index = 0;
                switch (curRow) {
                    case 'KEY':
                        data.Index = target;
                        break;
                    case 'ITEM':
                        data.ITEM = Number(target);
                        break;
                    case 'STAT_TYPE':
                        data.STAT_TYPE = target;
                        break;
                    case 'VALUE':
                        data.VALUE = Number(target);
                        break;
                    case 'VALUE_GROW':
                        data.VALUE_GROW = Number(target);
                        break;
                    case 'SUB_1_STEP': case 'SUB_2_STEP': case 'SUB_3_STEP':
                        index = Number(curRow.split('_')[1]) - 1; //SUB_n_STEP 의 n만 빼옴
                        data.SUB_STEP[index] = Number(target);
                        break;
                    case 'SUB_1': case 'SUB_2': case 'SUB_3':
                        index = Number(curRow.split('_')[1]) - 1; //SUB_n_STEP 의 n만 빼옴
                        data.SUB[index] = Number(target);
                        break;
                    case 'SET_GROUP':
                        data.SET_GROUP = Number(target);
                        break;
                    case 'UNEQUIP_COST_TYPE':
                        data.UNEQUIP_COST_TYPE = target;
                        break;
                    case 'UNEQUIP_COST_NUM':
                        data.UNEQUIP_COST_NUM = Number(target);
                        break;
                    case 'INF':
                        data.INF = Number(target);
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
 * Learn more about scripting: https://docs.cocos.com/creator/3.4/manual/en/scripting/
 * Learn more about CCClass: https://docs.cocos.com/creator/3.4/manual/en/scripting/decorator.html
 * Learn more about life-cycle callbacks: https://docs.cocos.com/creator/3.4/manual/en/scripting/life-cycle-callbacks.html
 */

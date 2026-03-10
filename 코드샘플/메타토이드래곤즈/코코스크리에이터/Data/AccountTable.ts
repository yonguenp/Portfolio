
import { ObjectCheck } from '../Tools/SandboxTools';
import { AccountData } from './AccountData';
import { TableBase } from './TableBase';

/**
 * Predefined variables
 * Name = AccountTable
 * DateTime = Mon Feb 21 2022 15:54:23 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = AccountTable.ts
 * FileBasenameNoExtension = AccountTable
 * URL = db://assets/Scripts/Data/AccountTable.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
export class AccountTable extends TableBase<AccountData> {
    public static Name: string = "AccountTable";

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
            
            let data = new AccountData();
            for (var curData = 0 ; curData < dataCount ; curData++) {
                const curRow = rowName[curData];
                const target = datas[curData];

                switch (curRow) {
                    case 'LEVEL':
                        data.Index = target;
                        data.LEVEL = Number(target);
                        break;
                    case 'EXP':
                        data.EXP = Number(target);
                        break;
                    case 'TOTAL_EXP':
                        data.TOTAL_EXP = Number(target);
                        break;
                    case 'MAX_STAMINA':
                        data.MAX_STAMINA = Number(target);
                        break;
                }
            }
            table.Add(data);
        }
    }

    public Get(level: PropertyKey): AccountData {
        if(ObjectCheck(this.datas, level)) {
            return this.datas[level] as AccountData;
        }

        return null;
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

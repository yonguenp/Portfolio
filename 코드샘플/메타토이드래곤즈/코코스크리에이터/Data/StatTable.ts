import { StatFactorData } from './StatData';
import { TableBase } from './TableBase';

/**
 * Predefined variables
 * Name = StatTable
 * DateTime = Mon Feb 21 2022 15:56:19 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = StatTable.ts
 * FileBasenameNoExtension = StatTable
 * URL = db://assets/Scripts/Data/StatTable.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
export class StatFactorTable extends TableBase<StatFactorData> {
    public static Name: string = "StatFactorTable";

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
            
            let data = new StatFactorData();
            for (var curData = 0 ; curData < dataCount ; curData++) {
                const curRow = rowName[curData];
                const target = datas[curData];

                switch (curRow) {
                    case 'KEY':
                        data.Index = target;
                        break;
                    case 'USER':
                        data.USER = Number(target);
                        break;
                    case 'ATK':
                        data.ATK = Number(target);
                        break;
                    case 'DEF':
                        data.DEF = Number(target);
                        break;
                    case 'HP':
                        data.HP = Number(target);
                        break;
                    case 'CRITICAL':
                        data.CRITICAL = Number(target);
                        break;
                    case 'CRITICAL_DMG':
                        data.CRITICAL_DMG = Number(target);
                        break;
                    case 'SPEED':
                        data.SPEED = Number(target);
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

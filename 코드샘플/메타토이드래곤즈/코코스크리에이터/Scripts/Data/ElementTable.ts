import { ElementRateData, eElementType } from './ElementData';
import { TableBase } from './TableBase';

/**
 * Predefined variables
 * Name = ElementTable
 * DateTime = Mon Feb 21 2022 15:55:50 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = ElementTable.ts
 * FileBasenameNoExtension = ElementTable
 * URL = db://assets/Scripts/Data/ElementTable.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
export class ElementRateTable extends TableBase<ElementRateData> {
    public static Name: string = "ElementRateTable";

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
            
            let data = new ElementRateData();
            for (var curData = 0 ; curData < dataCount ; curData++) {
                const curRow = rowName[curData];
                const target = datas[curData];

                switch (curRow) {
                    case 'KEY':
                        data.Index = target;
                        break;
                    case 'A_ELEMENT':
                        data.A_ELEMENT = target;
                        break;
                    case 'T_FIRE':
                        data.T_FIRE = Number(target);
                        break;
                    case 'T_WATER':
                        data.T_WATER = Number(target);
                        break;
                    case 'T_EARTH':
                        data.T_EARTH = Number(target);
                        break;
                    case 'T_WIND':
                        data.T_WIND = Number(target);
                        break;
                    case 'T_LIGHT':
                        data.T_LIGHT = Number(target);
                        break;
                    case 'T_DARK':
                        data.T_DARK = Number(target);
                        break;
                }
            }
            table.Add(data);
        }
    }

    public Get(type: eElementType): ElementRateData {
        return super.Get(type);
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


import { _decorator } from 'cc';
import { StringData } from './StringData';
import { TableBase } from './TableBase';
import { TableManager } from './TableManager';

/**
 * Predefined variables
 * Name = StringTable
 * DateTime = Tue Jan 11 2022 16:23:28 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = StringTable.ts
 * FileBasenameNoExtension = StringTable
 * URL = db://assets/Scripts/Data/StringTable.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
export class StringTable extends TableBase<StringData> {
    public static Name: string = "StringTable";
    private static instance: StringTable = null;
    public static get Instance() {
        if(StringTable.instance == null) {
            StringTable.instance = TableManager.GetTable<StringTable>(StringTable.Name);
        }
        return StringTable.instance;
    } 

    public SetTable(dataArray: Array<Array<string>>): void {
        if(dataArray == null || dataArray.length < 1) {
            return;
        }
        this.DataClear();
        const rowName = dataArray[0];
        const arrayCount = dataArray.length;
        const dataCount = rowName.length;
        let stringTable = this;

        for(var i = 1 ; i < arrayCount ; i++) {
            const datas = dataArray[i];
            
            let stringData = new StringData();
            for (var curData = 0 ; curData < dataCount ; curData++) {
                const curRow = rowName[curData];
                const data = datas[curData];

                switch (curRow) {
                    case 'KEY':
                        stringData.Index = data;
                        break;
                    case 'TYPE':
                        stringData.TYPE = Number(data);
                        break;
                    case 'KOR':
                        stringData.KOR = data;
                        break;
                    case 'ENG':
                        stringData.ENG = data;
                        break;
                }
            }
            stringTable.Add(stringData);
        }
    }

    public static GetString(index: number, defaultString: string = ""): string {
        if(StringTable.Instance == null) {
            return defaultString;
        }

        let data = StringTable.Instance.Get(index);
        if(data == null) {
            return defaultString;
        }

        let string = data.TEXT.replace('\\n','\n');
        return string;
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

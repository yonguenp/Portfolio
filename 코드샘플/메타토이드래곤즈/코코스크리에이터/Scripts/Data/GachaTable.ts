import { ObjectCheck } from '../Tools/SandboxTools';
import { GachaShopData, GachaListData } from './GachaData';
import { MultiTableBase } from './TableBase';

/**
 * Predefined variables
 * Name = GachaTable
 * DateTime = Thu Mar 17 2022 18:07:35 GMT+0900 (대한민국 표준시)
 * Author = blacktopaz
 * FileBasename = GachaTable.ts
 * FileBasenameNoExtension = GachaTable
 * URL = db://assets/Scripts/Data/GachaTable.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
export class GachaShopTable extends MultiTableBase<GachaShopData> {
    public static Name: string = "GachaShopTable";
    private keysData: {} = null;
    private gachaShopDatas: Array<GachaShopData> = new Array<GachaShopData>();

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
            
            let data = new GachaShopData();
            for (var curData = 0 ; curData < dataCount ; curData++) {
                const curRow = rowName[curData];
                const target = datas[curData];

                switch (curRow) {
                    case 'KEY':
                        data.KEY = Number(target);
                        break;
                    case '_NAME':
                        data._NAME = Number(target);
                        break;
                    case 'SORT':
                        data.SORT = Number(target);
                        break;
                    case 'GROUP1':
                        data.GROUP1 = Number(target);
                        break;
                    case 'GROUP1_RATE':
                        data.GROUP1_RATE = Number(target);
                        break;
                    case 'GROUP2':
                        data.GROUP2 = Number(target);
                        break;
                    case 'GROUP2_RATE':
                        data.GROUP2_RATE = Number(target);
                        break;
                    case 'COST_TYPE':
                        data.COST_TYPE = target;
                        break;
                    case 'COST_NUM':
                        data.COST_NUM = Number(target);
                        break;
                    case 'TICKET':
                        data.TICKET = Number(target);
                        break;
                }
            }
            table.Add(data);
            table.AddKey(data);

            this.gachaShopDatas.push(data);
        }
    }

    public AddKey(data: GachaShopData): void {
        if (this.keysData == undefined) return;
        if (Object.getOwnPropertyDescriptor(this.keysData, data.KEY) != undefined) return;
        this.keysData[data.KEY] = data;
    }

    public GetKey(key: number): GachaShopData {
        if (this.keysData == undefined) return null;
        if (Object.getOwnPropertyDescriptor(this.keysData, key) == undefined) return null;
        return this.keysData[key];
    }

    public GetDataBySort(sort: number): GachaShopData {
        if(this.gachaShopDatas == null) {return null;}

        let resultData: GachaShopData = null;
        this.gachaShopDatas.forEach(element => {
            if(element == null) {return;}

            if(element.SORT == sort) {
                resultData = element;
            }
        });

        return resultData;
    }

    public DataClear(): void {
        this.datas = {};
        this.keysData = {};
    }
}

export class GachaListTable extends MultiTableBase<GachaListData> {
    public static Name: string = "GachaListTable";
    private keysData: {} = null;

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
            
            let data = new GachaListData();
            for (var curData = 0 ; curData < dataCount ; curData++) {
                const curRow = rowName[curData];
                const target = datas[curData];

                switch (curRow) {
                    case 'KEY':
                        data.KEY = Number(target);
                        break;
                    case 'GROUP':
                        data.GROUP = Number(target);
                        break;
                    case 'TYPE':
                        data.TYPE = Number(target);
                        break;
                    case 'VALUE':
                        data.VALUE = Number(target);
                        break;
                    case 'NUM':
                        data.NUM = Number(target);
                        break;
                    case 'RATE':
                        data.RATE = Number(target);
                        break;
                    case 'FBX':
                        data.FBX = Number(target);
                        break;
                    case 'GRADE':
                        data.GRADE = target;
                        break;
                }
            }
            table.Add(data);
            table.AddKey(data);
        }
    }

    public AddKey(data: GachaListData): void {
        if (this.keysData == undefined) return;
        if (Object.getOwnPropertyDescriptor(this.keysData, data.KEY) != undefined) return;
        this.keysData[data.KEY] = data;
    }

    public GetKey(key: number): GachaListData {
        if (this.keysData == undefined) return null;
        if (Object.getOwnPropertyDescriptor(this.keysData, key) == undefined) return null;
        return this.keysData[key];
    }

    public GetAll(index: PropertyKey): GachaListData[] {
        var i = 0;
        var returnDatas: GachaListData[] = [];
        for( ; i < Number(index)+1 ; i++) {
            if(ObjectCheck(this.datas, i)) 
            {
                const datas = this.datas[i] as GachaListData[];
                
                datas.forEach(data => 
                {
                    returnDatas.push(data);
                });
            }
        }

        return returnDatas;
    }

    public DataClear(): void {
        this.datas = {};
        this.keysData = {};
    }
}
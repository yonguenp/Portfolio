
import { _decorator} from 'cc';
import { PartOptionData, PartSetData, PartReinforceData } from './PartOptionData';
import { TableBase } from './TableBase';

/**
 * Predefined variables
 * Name = PartOptionTable
 * DateTime = Tue Mar 22 2022 18:00:35 GMT+0900 (대한민국 표준시)
 * Author = wonjun_gs
 * FileBasename = PartOptionTable.ts
 * FileBasenameNoExtension = PartOptionTable
 * URL = db://assets/Scripts/Data/PartOptionTable.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 export class PartOptionTable extends TableBase<PartOptionData> {
    public static Name: string = "PartOptionTable";

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
            
            let data = new PartOptionData();
            for (var curData = 0 ; curData < dataCount ; curData++) {
                const curRow = rowName[curData];
                const target = datas[curData];

                switch (curRow) {
                    case 'KEY':
                        data.Index = target;
                        break;
                    case 'GROUP':
                        data.GROUP = Number(target);
                        break;
                    case 'STAT_TYPE':
                        data.STAT_TYPE = target;
                        break;
                    case 'VALUE_MIN':
                        data.VALUE_MIN = Number(target);
                        break;
                    case 'VALUE_MAX':
                        data.VALUE_MAX = Number(target);
                        break;
                    case 'VALUE_STEP':
                        data.VALUE_STEP = Number(target);
                        break;
                    case 'WEIGHT':
                        data.WEIGHT = Number(target);
                        break;
                    case 'RATE':
                        data.RATE = Number(target);
                        break;
                }
            }
            table.Add(data);
        }
    }

    public GetOptionByGroup(groupKey : number) : PartOptionData[]   {
        let arrGroup : PartOptionData[] = []
        
        Object.keys(this.datas).forEach(element => {
            if((this.datas[element] as PartOptionData).GROUP == groupKey)
                arrGroup.push(this.datas[element])
        })

        return arrGroup;
    }
}

export class PartSetTable extends TableBase<PartSetData> {
    public static Name: string = "PartSetTable";

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
            
            let data = new PartSetData();
            for (var curData = 0 ; curData < dataCount ; curData++) {
                const curRow = rowName[curData];
                const target = datas[curData];

                switch (curRow) {
                    case 'KEY':
                        data.Index = target;
                        break;
                    case 'GROUP':
                        data.GROUP = Number(target);
                        break;
                    case 'NUM':
                        data.NUM = Number(target);
                        break;
                    case 'STAT_TYPE':
                        data.STAT_TYPE = target;
                        break;
                    case 'VALUE':
                        data.VALUE = Number(target);
                        break;
                }
            }
            table.Add(data);
        }
    }

    public GetSetOptionByGroup(groupKey : number) : PartSetData[]   {
        let arrGroup : PartSetData[] = []
        
        Object.keys(this.datas).forEach(element => {
            if((this.datas[element] as PartSetData).GROUP == groupKey)
                arrGroup.push(this.datas[element])
        })

        return arrGroup;
    }
}

export class PartReinforceTable extends TableBase<PartReinforceData> {
    public static Name: string = "PartReinforceTable";

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
            
            let data = new PartReinforceData();
            for (var curData = 0 ; curData < dataCount ; curData++) {
                const curRow = rowName[curData];
                const target = datas[curData];

                switch (curRow) {
                    case 'KEY':
                        data.Index = target;
                        break;
                    case 'GRADE':
                        data.GRADE = Number(target);
                        break;
                    case 'STEP':
                        data.STEP = Number(target);
                        break;
                    case 'RATE':
                        data.RATE = Number(target);
                        break;
                    case 'ITEM':
                        data.ITEM = Number(target);
                        break;
                    case 'ITEM_NUM':
                        data.ITEM_NUM = Number(target);
                        break;
                    case 'COST_TYPE':
                        data.COST_TYPE = target;
                        break;
                    case 'COST_NUM':
                        data.COST_NUM = Number(target);
                        break;
                    case 'DESTROY':
                        data.DESTROY = Number(target);
                        break;
                    case 'DESTROY_REWARD':
                        data.DESTROY_REWARD = Number(target);
                        break;
                    case 'DESTROY_REWARD_NUM':
                        data.DESTROY_REWARD_NUM = Number(target);
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

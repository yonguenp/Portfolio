
import { _decorator } from 'cc';
import { SubwayDeliveryData, SubwayPlatformData } from './SubwayData';
import { TableBase } from './TableBase';

/**
 * Predefined variables
 * Name = SubwayTable
 * DateTime = Thu Jan 27 2022 14:51:24 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = SubwayTable.ts
 * FileBasenameNoExtension = SubwayTable
 * URL = db://assets/Scripts/Data/SubwayTable.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
export class SubwayPlatformTable extends TableBase<SubwayPlatformData> {
    public static Name: string = "SubwayPlatformTable";

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
            
            let data = new SubwayPlatformData();
            for (var curData = 0 ; curData < dataCount ; curData++) {
                const curRow = rowName[curData];
                const target = datas[curData];

                switch (curRow) {
                    case 'KEY':
                        data.Index = target;
                        break;
                    case 'PLATFORM':
                        data.PLATFORM = Number(target);
                        break;
                    case 'OPEN_LEVEL':
                        data.OPEN_LEVEL = Number(target);
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

export class SubwayDeliveryTable extends TableBase<SubwayDeliveryData> {
    public static Name: string = "SubwayDeliveryTable";

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
            
            let data = new SubwayDeliveryData();
            for (var curData = 0 ; curData < dataCount ; curData++) {
                const curRow = rowName[curData];
                const target = datas[curData];

                switch (curRow) {
                    case 'KEY':
                        data.Index = target;
                        break;
                    case 'NEED_ITEM':
                        data.NEED_ITEM = Number(target);
                        break;
                    case 'NEED_NUM':
                        data.NEED_NUM = Number(target);
                        break;
                    case 'NEED_PRODUCT_COUNT':
                        data.NEED_PRODUCT_COUNT = Number(target);
                        break;
                    case 'DELIVERY_TIME':
                        data.DELIVERY_TIME = Number(target);
                        break;
                    case 'REWARD_GROUP':
                        data.REWARD_GROUP = Number(target);
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

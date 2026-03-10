
import { _decorator } from 'cc';
import { ObjectCheck } from '../Tools/SandboxTools';
import { BuildingBaseData, BuildingLevelData, BuildingOpenData } from './BuildingData';
import { NEED_ITEM } from './DataBase';
import { MultiTableBase, TableBase } from './TableBase';

/**
 * Predefined variables
 * Name = BuildingTable
 * DateTime = Wed Jan 12 2022 18:21:55 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = BuildingTable.ts
 * FileBasenameNoExtension = BuildingTable
 * URL = db://assets/Scripts/Data/BuildingTable.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
export class BuildingBaseTable extends TableBase<BuildingBaseData> {
    public static Name: string = "BuildingBaseTable";

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
            
            let data = new BuildingBaseData();
            for (var curData = 0 ; curData < dataCount ; curData++) {
                const curRow = rowName[curData];
                const target = datas[curData];

                switch (curRow) {
                    case 'KEY':
                        data.Index = target;
                        break;
                    case 'TYPE':
                        data.TYPE = Number(target);
                        break;
                    case '_NAME':
                        data._NAME = Number(target);
                        break;
                    case '_DESC':
                        data._DESC = Number(target);
                        break;
                    case 'SIZE':
                        data.SIZE = Number(target);
                        break;
                    case 'START_SLOT':
                        data.START_SLOT = Number(target);
                        break;
                    case 'MAX_SLOT':
                        data.MAX_SLOT = Number(target);
                        break;
                    case 'BUILD_AREA':
                        data.BUILD_AREA = target;
                        break;
                }
            }
            table.Add(data);
        }
    }

    public Get(index: PropertyKey): BuildingBaseData {
        if(ObjectCheck(this.datas, index)) {
            return this.datas[index] as BuildingBaseData;
        }

        return this.GetIndex(index);
    }

    public GetIndex(index: PropertyKey): BuildingBaseData {
        var target = index;
        const keys = Object.keys(this.datas);
        const count = keys.length;
        for(var i = 0 ; i < count ; i++) {
            let obj = this.datas[keys[i]] as BuildingBaseData;
            if(obj._NAME == target) {
                return obj;
            }
        }

        return null;
    }
}
 
export class BuildingLevelTable extends TableBase<BuildingLevelData> {
    public static Name: string = "BuildingLevelTable";

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
            
            let data = new BuildingLevelData();
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
                    case 'BUILDING_GROUP':
                        data.BUILDING_GROUP = target;
                        break;
                    case 'LEVEL':
                        data.LEVEL = Number(target);
                        break;
                    case 'IMAGE':
                        data.IMAGE = target;
                        break;
                    case 'UPGRADE_TIME':
                        data.UPGRADE_TIME = Number(target);
                        break;
                    case 'NEED_AREA_LEVEL':
                        data.NEED_AREA_LEVEL = Number(target);
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

    GetBuildingMaxLevelByGroup(buildingGroup : string)
    {
        let tempLevel = -1;
        let keys = Object.keys(this.datas);
        if(keys == null || keys.length <= 0){
            return tempLevel;
        }

        let buildingLevelList : BuildingLevelData[] = []
        this.GetAll().forEach((element)=>
        {
            if(element.BUILDING_GROUP == buildingGroup)
            {
                buildingLevelList.push(element)
            }
        })
        buildingLevelList.sort((a,b)=> a.LEVEL - b.LEVEL)

        if(buildingLevelList == null || buildingLevelList.length <= 0){
            return tempLevel;
        }

        return buildingLevelList[buildingLevelList.length-1];
    }
}
 
export class BuildingOpenTable extends MultiTableBase<BuildingOpenData> {
    public static Name: string = "BuildingOpenTable";
    protected tagDatas: {} = null;

    public DataClear(): void {
        super.DataClear();
        this.tagDatas = {};
    }

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
            
            let data = new BuildingOpenData();
            for (var curData = 0 ; curData < dataCount ; curData++) {
                const curRow = rowName[curData];
                const target = datas[curData];

                switch (curRow) {
                    case 'KEY':
                        data.KEY = Number(target);
                        break;
                    case 'OPEN_LEVEL':
                        data.Index = target;
                        data.OPEN_LEVEL = Number(target);
                        break;
                    case 'BUILDING':
                        data.BUILDING = target;
                        break;
                    case 'COUNT':
                        data.COUNT = Number(target);
                        break;
                    case 'INSTALL_TAG':
                        data.INSTALL_TAG = Number(target);
                        break;
                }
            }
            this.tagDatas[data.INSTALL_TAG] = data;
            table.Add(data);
        }
    }

    public Get(index: PropertyKey): BuildingOpenData[] {
        var i = Number(index);
        var returnDatas: BuildingOpenData[] = [];
        for( ; i > 0 ; i--) {
            if(ObjectCheck(this.datas, i)) {
                const datas = this.datas[i] as BuildingOpenData[];
                
                datas.forEach(data => {
                    returnDatas.push(data);
                });
            }
        }
        return returnDatas;
    }

    public GetWithTag(tag: PropertyKey): BuildingOpenData {
        if (ObjectCheck(this.tagDatas, tag)) {
            return this.tagDatas[tag] as BuildingOpenData;
        }
        return null;
	}
	
    public GetAll(index: PropertyKey): BuildingOpenData[] {
        var i = 0;
        var returnDatas: BuildingOpenData[] = [];
        for( ; i < Number(index)+1 ; i++) {
            if(ObjectCheck(this.datas, i)) 
            {
                const datas = this.datas[i] as BuildingOpenData[];
                
                datas.forEach(data => 
                {
                    returnDatas.push(data);
                });
            }
        }

        return returnDatas;
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

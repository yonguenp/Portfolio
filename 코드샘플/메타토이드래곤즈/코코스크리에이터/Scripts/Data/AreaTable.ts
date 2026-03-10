
import { _decorator, Vec4 } from 'cc';
import { AreaExpansionData, AreaLevelData, WorldTripData } from './AreaData';
import { NEED_ITEM } from './DataBase';
import { MultiTableBase, TableBase } from './TableBase';

/**
 * Predefined variables
 * Name = AreaExplosion
 * DateTime = Wed Jan 12 2022 16:49:47 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = AreaExplosion.ts
 * FileBasenameNoExtension = AreaExplosion
 * URL = db://assets/Scripts/Data/AreaExplosion.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
export class AreaExpansionTable extends MultiTableBase<AreaExpansionData> {
    public static Name: string = "AreaExpansionTable";
    public GetFloorData(floor: number): AreaExpansionData {
        var target = floor - 1;
        let pickData: AreaExpansionData = null;

        if(this.datas != null) {
            const keys = Object.keys(this.datas);
            const dataCount = keys.length;

            for(var i = 0 ; i < dataCount ; i++) {
                let group = this.datas[keys[i]];
                if(group != null) {
                    group.forEach(value => {
                        let cur: AreaExpansionData = value;
                        if(cur.FLOOR >= target && (pickData == null || (pickData != null && pickData.FLOOR > cur.FLOOR))) {
                            pickData = cur;
                        }
                    });
                }
            }
        }

        return pickData;
    }
    public GetFloor(areaGroup: number, floor: number, ground: string): AreaExpansionData{
        let pickData: AreaExpansionData = null;
        const group = this.Get(areaGroup);

        if(group != null) {
            group.forEach(value => {
                if (pickData != null) {
                    return;
                }
    
                if (value != null && value.FLOOR == floor && value.GROUND == ground) {
                    pickData = value;
                    return;
                }
            });
        }

        return pickData;
    }
    public GetBetweenFloor(areaGroup: number, areaLevel: number): Vec4 {
        let vec = new Vec4(0, 0, 0, 0);

        if(this.datas != null) {
            const keys = Object.keys(this.datas);
            const dataCount = keys.length;

            for(var i = 0 ; i < dataCount ; i++) {
                let group: AreaExpansionData[] = this.datas[keys[i]];
                if(group != null) {
                    group.forEach(value => {
                        if(areaGroup >= Number(value.Index)) {
                            if(vec.x >= value.FLOOR) {
                                vec.x = value.FLOOR;
                            }
                            if(vec.y <= value.FLOOR) {
                                vec.y = value.FLOOR;
                            }
                            if(areaLevel >= value.OPEN_LEVEL) {
                                if(vec.z >= value.FLOOR) {
                                    vec.z = value.FLOOR;
                                }
                                if(vec.w <= value.FLOOR) {
                                    vec.w = value.FLOOR;
                                }
                            }
                        }
                    });
                }
            }
        }

        return vec;
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
            
            let data = new AreaExpansionData();
            for (var curData = 0 ; curData < dataCount ; curData++) {
                const curRow = rowName[curData];
                const target = datas[curData];

                switch (curRow) {
                    case 'KEY':
                        data.Index = target;
                        break;
                    case 'AREA_GROUP':
                        data.Index = Number(target);
                        data.AREA_GROUP = Number(target);
                        break;
                    case 'FLOOR':
                        data.FLOOR = Number(target);
                        break;
                    case 'OPEN_LEVEL':
                        data.OPEN_LEVEL = Number(target);
                        break;
                    case 'GROUND':
                        data.GROUND = target;
                        break;
                    case 'COST_TYPE':
                        data.COST_TYPE = target;
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
 
export class AreaLevelTable extends TableBase<AreaLevelData> {
    public static Name: string = "AreaLevelTable";

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

            let data = new AreaLevelData();
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
                    case 'LEVEL':
                        data.LEVEL = Number(target);
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
                    case 'NEED_GOLD':
                        data.NEED_GOLD = Number(target);
                        break;
                    case 'NEED_MISSION':
                        data.NEED_MISSION = Number(target);
                        break;
                    case 'EXPANSION_AREA':
                        data.EXPANSION_AREA = Number(target);
                        break;
                    case 'UPGRADE_TIME':
                        data.UPGRADE_TIME = Number(target);
                        break;
                    case 'WIDTH':
                        data.WIDTH = Number(target);
                        break;
                }
            }
            table.Add(data);
        }
    }

    public GetMaxLevel(): number {
        const keys = Object.keys(this.datas);
        const keyCount = keys.length;

        var level: number = -1;
        for(var i = 0 ; i < keyCount ; i++) {
            let curData: AreaLevelData = this.datas[keys[i]];
            if (curData == null) {
                continue;
            }

            if(level < curData.LEVEL) {
                level = curData.LEVEL;
            }
        }
        return level;
    }
}
 
export class WorldTripTable extends TableBase<WorldTripData> {
    public static Name: string = "WorldTripTable";

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
            
            let data = new WorldTripData();
            for (var curData = 0 ; curData < dataCount ; curData++) {
                const curRow = rowName[curData];
                const target = datas[curData];
                switch (curRow) {
                    case 'KEY':
                        data.Index = target;
                        break;
                    case 'WORLD':
                        data.WORLD = Number(target);
                        break;
                    case '_NAME':
                        data._NAME = Number(target);
                        break;
                    case 'CHAR_NUM':
                        data.CHAR_NUM = Number(target);
                        break;
                    case 'TIME':
                        data.TIME = Number(target);
                        break;
                    case 'COST_STAMINA':
                        data.COST_STAMINA = Number(target);
                        break;
                    case 'REWARD_ACCOUNT_EXP':
                        data.REWARD_ACCOUNT_EXP = Number(target);
                        break;
                    case 'REWARD_CHAR_EXP':
                        data.REWARD_CHAR_EXP = Number(target);
                        break;
                    case 'REWARD_GOLD':
                        data.REWARD_GOLD = Number(target);
                        break;
                    case 'REWARD_BONUS':
                        data.REWARD_BONUS = Number(target);
                        break;
                }
            }
            table.Add(data);
        }
    }

    public GetByWorldID(world: number) {
        const keys = Object.keys(this.datas);
        const keysCount = keys.length;
        for(var i = 0 ; i < keysCount ; i++) {
            let data = this.Get(keys[i]);
            if(data == null) {
                continue;
            }

            if(data.WORLD == world) {
                return data;
            }
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

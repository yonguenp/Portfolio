import { MonsterBaseData, MonsterSpawnData } from './MonsterData';
import { MultiTableBase, TableBase } from './TableBase';

/**
 * Predefined variables
 * Name = MonsterTable
 * DateTime = Mon Feb 21 2022 15:55:31 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = MonsterTable.ts
 * FileBasenameNoExtension = MonsterTable
 * URL = db://assets/Scripts/Data/MonsterTable.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
export class MonsterBaseTable extends TableBase<MonsterBaseData> {
    public static Name: string = "MonsterBaseTable";

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
            
            let data = new MonsterBaseData();
            for (var curData = 0 ; curData < dataCount ; curData++) {
                const curRow = rowName[curData];
                const target = datas[curData];

                switch (curRow) {
                    case 'KEY':
                        data.Index = target;
                        break;
                    case 'SIZE':
                        data.SIZE = Number(target);
                        break;
                    case 'IMAGE':
                        data.IMAGE = target;
                        break;
                    case '_NAME':
                        data._NAME = Number(target);
                        break;
                    case '_DESC':
                        data._DESC = Number(target);
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
                    case 'NORMAL_SKILL':
                        data.NORMAL_SKILL = Number(target);
                        break;
                    case 'SKILL1':
                        data.SKILL1 = Number(target);
                        break;
                    case 'SKILL2':
                        data.SKILL2 = Number(target);
                        break;
                }
            }
            table.Add(data);
        }
    }
}
 
export class MonsterSpawnTable extends MultiTableBase<MonsterSpawnData> {
    protected keyDatas: {} = null;
    public static Name: string = "MonsterSpawnTable";

    public DataClear(): void {
        this.datas = {};
        this.keyDatas = {};
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
            
            let data = new MonsterSpawnData();
            for (var curData = 0 ; curData < dataCount ; curData++) {
                const curRow = rowName[curData];
                const target = datas[curData];

                switch (curRow) {
                    case 'KEY':
                        data.KEY = Number(target);
                        break;
                    case 'SPAWN_GROUP':
                        data.Index = target;
                        data.SPAWN_GROUP = Number(target);
                        break;
                    case 'WAVE':
                        data.WAVE = Number(target);
                        break;
                    case 'GROUP':
                        data.GROUP = Number(target);
                        break;
                    case 'POSITION':
                        data.POSITION = Number(target);
                        break;
                    case 'MONSTER':
                        data.MONSTER = Number(target);
                        break;
                    case 'IS_BOSS':
                        data.IS_BOSS = Number(target);
                        break;
                    case 'SCALE':
                        data.SCALE = Number(target);
                        break;
                    case 'LEVEL':
                        data.LEVEL = Number(target);
                        break;
                    case 'FACTOR':
                        data.FACTOR = Number(target);
                        break;
                    case 'GRADE':
                        data.GRADE = Number(target);
                        break;
                    case 'ELEMENT':
                        data.ELEMENT = Number(target);
                        break;
                    case 'RATE':
                        data.RATE = Number(target);
                        break;
                    case 'INF':
                        data.INF = Number(target);
                        break;
                }
            }
            table.Add(data);
            if (table.keyDatas == undefined) continue;
            if (Object.getOwnPropertyDescriptor(table.keyDatas, data.KEY) != undefined) continue;
            table.keyDatas[data.KEY] = data;
        }
    }

    public GetKey(key: number): MonsterSpawnData {
        if (this.keyDatas == undefined) return null;
        if (Object.getOwnPropertyDescriptor(this.keyDatas, key) == undefined) return null;
        return this.keyDatas[key] as MonsterSpawnData;
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

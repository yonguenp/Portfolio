import { SkillCharData, SkillEffectData, SkillProjectileData } from './SkillData';
import { MultiTableBase, TableBase } from './TableBase';

/**
 * Predefined variables
 * Name = SkillTable
 * DateTime = Mon Feb 21 2022 15:55:02 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = SkillTable.ts
 * FileBasenameNoExtension = SkillTable
 * URL = db://assets/Scripts/Data/SkillTable.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
export class SkillCharTable extends MultiTableBase<SkillCharData> {
    public static Name: string = "SkillCharTable";
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
            
            let data = new SkillCharData();
            for (var curData = 0 ; curData < dataCount ; curData++) {
                const curRow = rowName[curData];
                const target = datas[curData];

                switch (curRow) {
                    case 'KEY':
                        data.KEY = Number(target);
                        break;
                    case 'SKILL_ID':
                        data.Index = target;
                        data.SKILL_ID = Number(target);
                        break;
                    case 'ICON':
                        data.ICON = target;
                        break;
                    case 'LEVEL':
                        data.LEVEL = Number(target);
                        break;
                    case '_NAME':
                        data._NAME = Number(target);
                        break;
                    case '_DESC1':
                        data._DESC1 = Number(target);
                        break;
                    case 'START_COOL_TIME':
                        data.START_COOL_TIME = Number(target);
                        break;
                    case 'COOL_TIME':
                        data.COOL_TIME = Number(target);
                        break;
                    case 'CASTING_TYPE':
                        data.CASTING_TYPE = target;
                        break;
                    case 'RANGE':
                    case 'RANGE_X':
                        data.RANGE_X = Number(target);
                        break;
                    case 'RANGE_Y':
                        data.RANGE_Y = Number(target);
                        break;
                    case 'PROJECTILE_KEY':
                        data.PROJECTILE_KEY = Number(target);
                        break;
                    case 'EFFECT_GROUP':
                        data.EFFECT_GROUP = Number(target);
                        break;
                    case 'TRIGGER_DELAY':
                        data.TRIGGER_DELAY = Number(target);
                        break;
                    case 'MID_DELAY':
                        data.Mid_delay = Number(target);
                        break;
                    case 'AFTER_DELAY':
                        data.AFTER_DELAY = Number(target);
                        break;
                    case 'INF':
                        data.INF = Number(target);
                        break;
                    case 'ITEM':
                        data.ITEM = Number(target);
                        break;
                    case 'ITEM_VALUE':
                        data.ITEM_VALUE = Number(target);
                        break;
                    case 'COST_TYPE':
                        data.COST_TYPE = target;
                        break;
                    case 'COST_VALUE':
                        data.COST_VALUE = Number(target);
                        break;
                    case 'NEXT_LEVEL_SKILL':
                        data.NEXT_LEVEL_SKILL = Number(target);
                        break;
                    case 'ANI_PREFAB':
                        data.ANI_PREFAB = target;
                        break;
                }
            }
            table.Add(data);
            table.AddKey(data);
        }
    }

    public AddKey(data: SkillCharData): void {
        if (this.keysData == undefined) return;
        if (Object.getOwnPropertyDescriptor(this.keysData, data.KEY) != undefined) return;
        this.keysData[data.KEY] = data;
    }

    public GetKey(key: number): SkillCharData {
        if (this.keysData == undefined) return null;
        if (Object.getOwnPropertyDescriptor(this.keysData, key) == undefined) return null;
        return this.keysData[key];
    }

    public GetSkillByLevel(skill_id: number, level: number): SkillCharData {
        let items = this.Get(skill_id);
        if(items == null) {
            return null;
        }

        let skill: SkillCharData = null;
        items.forEach(element => {
            if(element == null) {
                return;
            }

            if(element.LEVEL == level) {
                skill = element;
            }
        });

        return skill;
    }

    public DataClear(): void {
        this.datas = {};
        this.keysData = {};
    }
}
 
export class SkillEffectTable extends MultiTableBase<SkillEffectData> {
    public static Name: string = "SkillEffectTable";

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
            
            let data = new SkillEffectData();
            for (var curData = 0 ; curData < dataCount ; curData++) {
                const curRow = rowName[curData];
                const target = datas[curData];

                switch (curRow) {
                    case 'KEY':
                        data.KEY = Number(target);
                        break;
                    case 'GROUP':
                        data.Index = target;
                        data.GROUP = Number(target);
                        break;
                    case 'SKILL':
                        data.SKILL = target;
                        break;
                    case 'START_POSITION':
                        data.START_POSITION = Number(target);
                        break;
                    case 'DIRECTION':
                        data.DIRECTION = Number(target);
                        break;
                    case 'RANGE':
                    case 'RANGE_X':
                        data.RANGE_X = Number(target);
                        break;
                    case 'RANGE_Y':
                        data.RANGE_Y = Number(target);
                        break;
                    case 'TARGET':
                        data.TARGET = Number(target);
                        break;
                    case 'COUNT':
                        data.COUNT = Number(target);
                        break;
                    case 'VALUE':
                        data.VALUE = Number(target);
                        break;
                    case '_DESC1':
                        data._DESC1 = Number(target);
                        break;
                    case '_DESC2':
                        data._DESC2 = Number(target);
                        break;
                    case 'EFFECT_PREFAB':
                        data.EFFECT_PREFAB = target;
                        break;
                    case 'TERM':
                        data.TERM = Number(target);
                        break;
                    case 'NEST_COUNT':
                        data.NEST_COUNT = Number(target);
                        break;
                    case 'MAX_TIME':
                        data.MAX_TIME = Number(target);
                        break;
                }
            }
            table.Add(data);
        }
    }
}
 
export class SkillProjectileTable extends TableBase<SkillProjectileData> {
    public static Name: string = "SkillProjectileTable";

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
            
            let data = new SkillProjectileData();
            for (var curData = 0 ; curData < dataCount ; curData++) {
                const curRow = rowName[curData];
                const target = datas[curData];

                switch (curRow) {
                    case 'KEY':
                        data.Index = target;
                        data.KEY = Number(target);
                        break;
                    case 'TYPE':
                        data.TYPE = target;
                        break;
                    case 'ARRIVAL_TIME':
                        data.ARRIVAL_TIME = Number(target);
                        break;
                    case 'HEIGHT':
                        data.HEIGHT = Number(target);
                        break;
                    case 'START_X':
                        data.START_X = Number(target);
                        break;
                    case 'START_Y':
                        data.START_Y = Number(target);
                        break;
                    case 'GOAL_X':
                        data.GOAL_X = Number(target);
                        break;
                    case 'GOAL_Y':
                        data.GOAL_Y = Number(target);
                        break;
                    case 'PROJECTILE_ORDER':
                        data.PROJECTILE_ORDER = target;
                        break;
                    case 'PROJECTILE_IMAGE':
                        data.PROJECTILE_IMAGE = target;
                        break;
                    case 'EFFECT_ORDER':
                        data.EFFECT_ORDER = target;
                        break;
                    case 'EFFECT_IMAGE':
                        data.EFFECT_IMAGE = target;
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

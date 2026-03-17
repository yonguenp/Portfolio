
import { _decorator} from 'cc';
import { DataBase } from './DataBase';

/**
 * Predefined variables
 * Name = PartOptionData
 * DateTime = Tue Mar 22 2022 18:00:26 GMT+0900 (대한민국 표준시)
 * Author = wonjun_gs
 * FileBasename = PartOptionData.ts
 * FileBasenameNoExtension = PartOptionData
 * URL = db://assets/Scripts/Data/PartOptionData.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
//드래곤 파츠(슬롯) 강화에 필요한 슬롯 부가 옵션 데이터
export class PartOptionData extends DataBase {
    protected group: number = -1;
    public get GROUP(): number {
        return this.group;
    }
    public set GROUP(value: number) {
        this.group = value;
    }

    protected stat_type: string = "";
    public get STAT_TYPE(): string {
        return this.stat_type;
    }
    public set STAT_TYPE(value: string) {
        this.stat_type = value;
    }

    protected value_min: number = -1;
    public get VALUE_MIN(): number {
        return this.value_min;
    }
    public set VALUE_MIN(value: number) {
        this.value_min = value;
    }

    protected value_max: number = -1;
    public get VALUE_MAX(): number {
        return this.value_max;
    }
    public set VALUE_MAX(value: number) {
        this.value_max = value;
    }

    protected value_step: number = -1;
    public get VALUE_STEP(): number {
        return this.value_step;
    }
    public set VALUE_STEP(value: number) {
        this.value_step = value;
    }
    
    protected weight: number = -1;
    public get WEIGHT(): number {
        return this.weight;
    }
    public set WEIGHT(value: number) {
        this.weight = value;
    }

    protected rate: number = -1;
    public get RATE(): number {
        return this.rate;
    }
    public set RATE(value: number) {
        this.rate = value;
    }
}

//드래곤 파츠(슬롯) 세트 옵션 데이터
export class PartSetData extends DataBase {
    protected group: number = -1;
    public get GROUP(): number {
        return this.group;
    }
    public set GROUP(value: number) {
        this.group = value;
    }

    protected num: number = -1;
    public get NUM(): number {
        return this.num;
    }
    public set NUM(value: number) {
        this.num = value;
    }

    protected stat_type: string = "";
    public get STAT_TYPE(): string {
        return this.stat_type;
    }
    public set STAT_TYPE(value: string) {
        this.stat_type = value;
    }

    protected value: number = -1;
    public get VALUE(): number {
        return this.value;
    }
    public set VALUE(value: number) {
        this.value = value;
    }
}

//드래곤 파츠(슬롯) 강화 옵션 데이터
export class PartReinforceData extends DataBase {
    protected grade: number = -1;
    public get GRADE(): number {
        return this.grade;
    }
    public set GRADE(value: number) {
        this.grade = value;
    }

    protected step: number = -1;
    public get STEP(): number {
        return this.step;
    }
    public set STEP(value: number) {
        this.step = value;
    }

    protected rate : number = -1;
    public get RATE(): number {
        return this.rate;
    }
    public set RATE(value: number) {
        this.rate = value;
    }

    protected item : number = -1;
    public get ITEM(): number {
        return this.item;
    }
    public set ITEM(value: number) {
        this.item = value;
    }

    protected item_num : number = -1;
    public get ITEM_NUM(): number {
        return this.item_num;
    }
    public set ITEM_NUM(value: number) {
        this.item_num = value;
    }

    protected cost_type : string = "";
    public get COST_TYPE(): string {
        return this.cost_type;
    }
    public set COST_TYPE(value: string) {
        this.cost_type = value;
    }

    protected cost_num : number = -1;
    public get COST_NUM(): number {
        return this.cost_num;
    }
    public set COST_NUM(value: number) {
        this.cost_num = value;
    }

    protected destory : number = -1;
    public get DESTROY(): number {
        return this.destory;
    }
    public set DESTROY(value: number) {
        this.destory = value;
    }

    protected destory_reword : number = -1;
    public get DESTROY_REWARD(): number {
        return this.destory_reword;
    }
    public set DESTROY_REWARD(value: number) {
        this.destory_reword = value;
    }

    protected destory_reword_num : number = -1;
    public get DESTROY_REWARD_NUM(): number {
        return this.destory_reword_num;
    }
    public set DESTROY_REWARD_NUM(value: number) {
        this.destory_reword_num = value;
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

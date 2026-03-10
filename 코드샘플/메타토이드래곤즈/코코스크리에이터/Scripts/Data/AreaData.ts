
import { _decorator } from 'cc';
import { DataBase, NEED_ITEM } from './DataBase';

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
 
export class AreaExpansionData extends DataBase {
    protected area_group: number = -1;
    public get AREA_GROUP() {
        return this.area_group;
    }
    public set AREA_GROUP(value: number) {
        this.area_group = value;
    }
    
    protected floor: number = -1;
    public get FLOOR(): number {
        switch (this.ground) {
            case 'Ground':
                return this.floor - 1;
            case 'Basement':
                return -this.floor;
        }

        return this.floor;
    }
    public set FLOOR(value: number) {
        this.floor = value;
    }
    
    protected open_level: number = -1;
    public get OPEN_LEVEL(): number {
        return this.open_level;
    }
    public set OPEN_LEVEL(value: number) {
        this.open_level = value;
    }
    
    protected ground: string = "";
    public get GROUND(): string {
        return this.ground;
    }
    public set GROUND(value: string) {
        this.ground = value;
    }
    
    protected cost_type: string = "";
    public get COST_TYPE(): string {
        return this.cost_type;
    }
    public set COST_TYPE(value: string) {
        this.cost_type = value;
    }
    
    protected cost_num: number = -1;
    public get COST_NUM(): number {
        return this.cost_num;
    }
    public set COST_NUM(value: number) {
        this.cost_num = value;
    }
}
 
export class AreaLevelData extends DataBase {
    protected level: number = -1;
    public get LEVEL() {
        return this.level;
    }
    public set LEVEL(value: number) {
        this.level = value;
    }

    protected need_item: NEED_ITEM[] = [];
    public get NEED_ITEM(): NEED_ITEM[] {
        return this.need_item;
    }

    public set NEED_ITEM(value: NEED_ITEM[]) {
        this.need_item = value;
    }
    
    protected need_gold: number = -1;
    public get NEED_GOLD(): number {
        return this.need_gold;
    }
    public set NEED_GOLD(value: number) {
        this.need_gold = value;
    }
    
    protected need_mission: number = -1;
    public get NEED_MISSION(): number {
        return this.need_mission;
    }
    public set NEED_MISSION(value: number) {
        this.need_mission = value;
    }
    
    protected expansion_area: number = -1;
    public get EXPANSION_AREA(): number {
        return this.expansion_area;
    }
    public set EXPANSION_AREA(value: number) {
        this.expansion_area = value;
    }
    
    protected upgrade_time: number = -1;
    public get UPGRADE_TIME(): number {
        return this.upgrade_time;
    }
    public set UPGRADE_TIME(value: number) {
        this.upgrade_time = value;
    }
    
    protected width: number = -1;
    public get WIDTH(): number {
        if(this.width < 0) {
            return 4;
        }
        return this.width;
    }
    public set WIDTH(value: number) {
        this.width = value;
    }
}
 
export class WorldTripData extends DataBase {
    protected world: number = -1;
    public get WORLD() {
        return this.world;
    }
    public set WORLD(value: number) {
        this.world = value;
    }

    protected _name: number = -1;
    public get _NAME(): number {
        return this._name;
    }

    public set _NAME(value: number) {
        this._name = value;
    }
    
    protected char_num: number = -1;
    public get CHAR_NUM(): number {
        return this.char_num;
    }
    public set CHAR_NUM(value: number) {
        this.char_num = value;
    }
    
    protected time: number = -1;
    public get TIME(): number {
        return this.time;
    }
    public set TIME(value: number) {
        this.time = value;
    }
    
    protected cost_stamina: number = -1;
    public get COST_STAMINA(): number {
        return this.cost_stamina;
    }
    public set COST_STAMINA(value: number) {
        this.cost_stamina = value;
    }
    
    protected reward_account_exp: number = -1;
    public get REWARD_ACCOUNT_EXP(): number {
        return this.reward_account_exp;
    }
    public set REWARD_ACCOUNT_EXP(value: number) {
        this.reward_account_exp = value;
    }
    
    protected reward_char_exp: number = -1;
    public get REWARD_CHAR_EXP(): number {
        return this.reward_char_exp;
    }
    public set REWARD_CHAR_EXP(value: number) {
        this.reward_char_exp = value;
    }
    
    protected reward_gold: number = -1;
    public get REWARD_GOLD(): number {
        return this.reward_gold;
    }
    public set REWARD_GOLD(value: number) {
        this.reward_gold = value;
    }
    
    protected reward_bonus: number = -1;
    public get REWARD_BONUS(): number {
        return this.reward_bonus;
    }
    public set REWARD_BONUS(value: number) {
        this.reward_bonus = value;
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

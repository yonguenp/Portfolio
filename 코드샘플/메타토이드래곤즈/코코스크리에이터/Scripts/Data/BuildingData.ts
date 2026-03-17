
import { _decorator } from 'cc';
import { DataBase, NEED_ITEM } from './DataBase';

/**
 * Predefined variables
 * Name = BuildingData
 * DateTime = Wed Jan 12 2022 18:21:46 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = BuildingData.ts
 * FileBasenameNoExtension = BuildingData
 * URL = db://assets/Scripts/Data/BuildingData.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
export class BuildingBaseData extends DataBase {
    protected type: number = -1;
    public get TYPE(): number {
        return this.type;
    }
    public set TYPE(value: number) {
        this.type = value;
    }
    
    protected _name: number = -1;
    public get _NAME(): number {
        return this._name;
    }
    public set _NAME(value: number) {
        this._name = value;
    }

    protected _desc: number = -1;
    public get _DESC(): number {
        return this._desc;
    }
    public set _DESC(value: number) {
        this._desc = value;
    }
    
    protected size: number = -1;
    public get SIZE(): number {
        return this.size;
    }
    public set SIZE(value: number) {
        this.size = value;
    }
    
    protected start_slot: number = -1;
    public get START_SLOT(): number {
        return this.start_slot;
    }
    public set START_SLOT(value: number) {
        this.start_slot = value;
    }
    
    protected max_slot: number = -1;
    public get MAX_SLOT(): number {
        return this.max_slot;
    }
    public set MAX_SLOT(value: number) {
        this.max_slot = value;
    }
    
    protected building_area: string = "";
    public get BUILD_AREA(): string {
        return this.building_area;
    }
    public set BUILD_AREA(value: string) {
        this.building_area = value;
    }
}
 
export class BuildingLevelData extends DataBase {
    protected building_group: string = "";
    public get BUILDING_GROUP(): string {
        return this.building_group;
    }
    public set BUILDING_GROUP(value: string) {
        this.building_group = value;
    }
    
    protected level: number = -1;
    public get LEVEL(): number {
        return this.level;
    }
    public set LEVEL(value: number) {
        this.level = value;
    }
    
    protected image: string = "";
    public get IMAGE(): string {
        return this.image;
    }
    public set IMAGE(value: string) {
        this.image = value;
    }
    
    protected upgrade_time: number = -1;
    public get UPGRADE_TIME(): number {
        return this.upgrade_time;
    }
    public set UPGRADE_TIME(value: number) {
        this.upgrade_time = value;
    }

    protected need_area_level: number = -1;
    public get NEED_AREA_LEVEL(): number {
        return this.need_area_level;
    }
    public set NEED_AREA_LEVEL(value: number) {
        this.need_area_level = value;
    }

    protected need_item: NEED_ITEM[] = [];
    public get NEED_ITEM(): NEED_ITEM[] {
        return this.need_item;
    }
    public set NEED_ITEM(value: NEED_ITEM[]) {
        this.need_item = value;
    }
    
    protected cost_type: number = -1;
    public get COST_TYPE(): number {
        return this.cost_type;
    }
    public set COST_TYPE(value: number) {
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
 
export class BuildingOpenData extends DataBase {
    protected key: number = -1;
    public get KEY(): number {
        return this.key;
    }
    public set KEY(value: number) {
        this.key = value;
    }


    protected open_level: number = -1;
    public get OPEN_LEVEL(): number {
        return this.open_level;
    }
    public set OPEN_LEVEL(value: number) {
        this.open_level = value;
    }
    
    protected building: string = "";
    public get BUILDING(): string {
        return this.building;
    }
    public set BUILDING(value: string) {
        this.building = value;
    }
    
    protected count: number = -1;
    public get COUNT(): number {
        return this.count;
    }
    public set COUNT(value: number) {
        this.count = value;
    }
    
    protected install_tag: number = -1;
    public get INSTALL_TAG(): number {
        return this.install_tag;
    }
    public set INSTALL_TAG(value: number) {
        this.install_tag = value;
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

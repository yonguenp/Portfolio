
import { _decorator } from 'cc';
import { DataBase } from './DataBase';

/**
 * Predefined variables
 * Name = SubwayData
 * DateTime = Thu Jan 27 2022 14:51:13 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = SubwayData.ts
 * FileBasenameNoExtension = SubwayData
 * URL = db://assets/Scripts/Data/SubwayData.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
export class SubwayPlatformData extends DataBase {
    protected platform: number = -1;
    public get PLATFORM() {
        return this.platform;
    }
    public set PLATFORM(value: number) {
        this.platform = value;
    }
    
    protected open_level: number = -1;
    public get OPEN_LEVEL(): number {
        return this.open_level;
    }
    public set OPEN_LEVEL(value: number) {
        this.open_level = value;
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
 
export class SubwayDeliveryData extends DataBase {
    protected need_itel: number = -1;
    public get NEED_ITEM() {
        return this.need_itel;
    }
    public set NEED_ITEM(value: number) {
        this.need_itel = value;
    }
    
    protected need_num: number = -1;
    public get NEED_NUM(): number {
        return this.need_num;
    }
    public set NEED_NUM(value: number) {
        this.need_num = value;
    }
    
    protected need_product_count: number = -1;
    public get NEED_PRODUCT_COUNT(): number {
        return this.need_product_count;
    }
    public set NEED_PRODUCT_COUNT(value: number) {
        this.need_product_count = value;
    }
    
    protected delivery_time: number = -1;
    public get DELIVERY_TIME(): number {
        return this.delivery_time;
    }
    public set DELIVERY_TIME(value: number) {
        this.delivery_time = value;
    }
    
    protected reward_group: number = -1;
    public get REWARD_GROUP(): number {
        return this.reward_group;
    }
    public set REWARD_GROUP(value: number) {
        this.reward_group = value;
    }
}
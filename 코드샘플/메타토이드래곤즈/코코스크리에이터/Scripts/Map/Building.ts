
import { sp, _decorator } from 'cc';
import { ObjectData } from './ObjectData';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = BuildingData
 * DateTime = Mon Jan 03 2022 15:41:37 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = BuildingData.ts
 * FileBasenameNoExtension = BuildingData
 * URL = db://assets/Scripts/Map/BuildingData.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
@ccclass('Building')
export class Building extends ObjectData {
    @property
    protected buildingNo: string = "-1";
    public get BuildingNo() {
        return this.buildingNo;
    }
    @property(sp.Skeleton)
    protected spine: sp.Skeleton = null;
    public get Spine() {
        return this.spine;
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

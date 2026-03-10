
import { _decorator, Component, Node, Asset } from 'cc';
import { EventData, EventListener, IManagerBase } from 'sb';
import { GameManager } from '../GameManager';
import { EventManager } from './EventManager';
import { ObjectCheck } from './SandboxTools';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = DataManager
 * DateTime = Wed Jan 19 2022 21:37:34 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = DataManager.ts
 * FileBasenameNoExtension = DataManager
 * URL = db://assets/Scripts/Tools/DataManager.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */

export class DataManager implements IManagerBase {
    public static Name: string = "DataManager";
    protected static instance: DataManager = null;
    protected datas: {} = null;
    public static get Instance() {
        if(DataManager.instance == null) {
            return DataManager.instance = new DataManager();
        }
        return DataManager.instance;
    }

    Init(): void {
        this.datas = {};
        GameManager.Instance.AddManager(this, false);
    }

    GetManagerName(): string {
        return DataManager.Name;
    }

    public static AddData(key: PropertyKey, data: any): void {
        if(ObjectCheck(DataManager.Instance.datas, key)) {
            console.log('is Not Empty', DataManager.Instance.datas[key]);
            return;
        }
        DataManager.Instance.datas[key] = data;
    }

    public static DelData(key: PropertyKey): void {
        if(ObjectCheck(DataManager.Instance.datas, key)) {
            delete DataManager.Instance.datas[key];
        }
    }

    public static GetData<T>(key: PropertyKey): T {
        if(ObjectCheck(DataManager.Instance.datas, key)) {
            return DataManager.Instance.datas[key] as T;
        }
        return null;
    }

    //사용 안함
    Update(deltaTime: number): void {
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

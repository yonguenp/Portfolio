
import { _decorator } from 'cc';
import { WaveBase } from './WaveBase';

/**
 * Predefined variables
 * Name = WaveIdle
 * DateTime = Thu Feb 17 2022 18:16:53 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = WaveIdle.ts
 * FileBasenameNoExtension = WaveIdle
 * URL = db://assets/Scripts/Battle/WaveIdle.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
export class WaveIdle extends WaveBase {
    GetID(): string {
        return 'WaveIdle';
    }
    
    Update(dt: number): string {
        return "";
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

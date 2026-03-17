
import { _decorator } from 'cc';
import { WaveBase } from './WaveBase';

/**
 * Predefined variables
 * Name = WaveEnd
 * DateTime = Thu Feb 17 2022 18:17:16 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = WaveEnd.ts
 * FileBasenameNoExtension = WaveEnd
 * URL = db://assets/Scripts/Battle/WaveEnd.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
export class WaveEnd extends WaveBase {
    GetID(): string {
        return 'WaveEnd';
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

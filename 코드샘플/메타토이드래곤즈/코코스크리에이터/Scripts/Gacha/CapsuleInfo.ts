
import { _decorator, Sprite, SpriteFrame } from 'cc';
const { ccclass } = _decorator;

/**
 * Predefined variables
 * Name = CapsuleInfo
 * DateTime = Tue Mar 15 2022 13:56:09 GMT+0900 (대한민국 표준시)
 * Author = blacktopaz
 * FileBasename = CapsuleInfo.ts
 * FileBasenameNoExtension = CapsuleInfo
 * URL = db://assets/Scripts/Gacha/CapsuleInfo.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */

// 캡슐 등급
export enum CabsuleGrade {
    NONE,
    NORMAL,
    RARE,
    SUPER_RARE
}
 
@ccclass('CapsuleInfo')
export class CapsuleInfo {
    
    objectID: number = 0;
    
    capsuleGrade: CabsuleGrade = CabsuleGrade.NONE;
    objectIconFrame: SpriteFrame = null;
    objectName: string = "";
}

import { _decorator } from 'cc';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = TutorialBaseData
 * DateTime = Tue Mar 29 2022 18:01:48 GMT+0900 (대한민국 표준시)
 * Author = blacktopaz
 * FileBasename = TutorialBaseData.ts
 * FileBasenameNoExtension = TutorialBaseData
 * URL = db://assets/Scripts/Tutorial/TutorialBaseData.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */

export enum TUTORIAL_EVENT_TYPE {
    NONE,
    TOUCH,
    BUTTON,
    WAITING,
    FIND_NODE,
}

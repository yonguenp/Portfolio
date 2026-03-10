
import { _decorator, Node } from 'cc';
import { Popup } from './Common/Popup';
import { DragonLevelUpDescComponent } from './DragonLevelUpDescComponent';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = DragonLevelUpPopup
 * DateTime = Thu Mar 17 2022 12:11:22 GMT+0900 (대한민국 표준시)
 * Author = wonjun_gs
 * FileBasename = DragonLevelUpPopup.ts
 * FileBasenameNoExtension = DragonLevelUpPopup
 * URL = db://assets/Scripts/UI/DragonLevelUpPopup.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('DragonLevelUpPopup')
export class DragonLevelUpPopup extends Popup
{
    //현재 및 다음 레벨 변화 표시 노드 (컴포넌트 따로 뺌)
    @property(Node)
    currentLevelNode : Node = null;
    @property(Node)
    nextLevelNode : Node = null;

    Init(data?: any)
    {
        let currentLevel = data.currentLevel;
        let nextLevel = data.nextLevel;

        this.SetDetailData(currentLevel, nextLevel);

        super.Init(data);
    }

    SetDetailData(currentLevel : number, nextLevel : number)
    {
        let currentComp = this.currentLevelNode.getComponent(DragonLevelUpDescComponent);
        if(currentComp != null){
            currentComp.init(currentLevel);
        }

        let nextComp = this.nextLevelNode.getComponent(DragonLevelUpDescComponent);
        if(nextComp != null){
            nextComp.init(nextLevel);
        }
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

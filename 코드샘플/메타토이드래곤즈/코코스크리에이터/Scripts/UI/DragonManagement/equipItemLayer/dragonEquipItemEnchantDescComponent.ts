
import { _decorator, Component, Node, Label } from 'cc';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = dragonEquipItemEnchantDescComponent
 * DateTime = Thu Mar 24 2022 17:50:52 GMT+0900 (대한민국 표준시)
 * Author = wonjun_gs
 * FileBasename = dragonEquipItemEnchantDescComponent.ts
 * FileBasenameNoExtension = dragonEquipItemEnchantDescComponent
 * URL = db://assets/Scripts/UI/DragonManagement/equipItemLayer/dragonEquipItemEnchantDescComponent.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
/**
 * 레퍼런스는 장비 파츠 등급에 따라서 강화 등급 최대 수치 및 부가 하위 옵션 갯수와 값이 전부 달랐음.
 */
@ccclass('dragonEquipItemEnchantDescComponent')
export class dragonEquipItemEnchantDescComponent extends Component {
    
    @property(Label)//각 옵션 별 스크롤 뷰가 될것...
    desclabel : Label = null;

    init(partGrade : number)
    {

    }

    refreshEnchantLevelData(partGrade : number)
    {

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

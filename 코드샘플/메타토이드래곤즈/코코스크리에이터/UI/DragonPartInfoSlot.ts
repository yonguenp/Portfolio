
import { _decorator, Component, Node, Label } from 'cc';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = DragonPartInfoSlot
 * DateTime = Tue Mar 29 2022 16:12:26 GMT+0900 (대한민국 표준시)
 * Author = wonjun_gs
 * FileBasename = DragonPartInfoSlot.ts
 * FileBasenameNoExtension = DragonPartInfoSlot
 * URL = db://assets/Scripts/UI/DragonPartInfoSlot.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 /**
  * 드래곤 장착 정보 스크롤을 구성하는 구성 성분
  */
@ccclass('DragonPartInfoSlot')
export class DragonPartInfoSlot extends Component {

    @property(Label)
    typeLabel : Label = null;//타입 명시 라벨

    @property(Label)
    valueLabel : Label = null;//값 명시 라벨

    SetData(typeStr : string, value : number)
    {
        if(this.typeLabel == null || this.valueLabel == null){
            return;
        }

        this.typeLabel.string = typeStr;
        this.valueLabel.string = "+"+value.toFixed(2)+"%";
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

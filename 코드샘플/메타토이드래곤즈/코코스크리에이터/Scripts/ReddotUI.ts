
import { _decorator, Component, Node } from 'cc';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = ReddotUI
 * DateTime = Mon May 02 2022 18:56:51 GMT+0900 (대한민국 표준시)
 * Author = jinwonjun
 * FileBasename = ReddotUI.ts
 * FileBasenameNoExtension = ReddotUI
 * URL = db://assets/Scripts/ReddotUI.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */

@ccclass('ReddotUI')
export class ReddotUI extends Component {
    @property(Node)
    InventoryReddotList : Node[] = [];

    @property(Node)
    buildingReddotList : Node[] = [];

    onVisibleInventoryList()
    {
        if(this.InventoryReddotList == null || this.InventoryReddotList.length <= 0){
            return;
        }

        this.InventoryReddotList.forEach((element)=>{
            if(element == null){
                return;
            }
            element.active = true;
        })
    }

    onVisibleBuildingList()
    {
        if(this.buildingReddotList == null || this.buildingReddotList.length <= 0){
            return;
        }

        this.buildingReddotList.forEach((element)=>{
            if(element == null){
                return;
            }
            element.active = true;
        })
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

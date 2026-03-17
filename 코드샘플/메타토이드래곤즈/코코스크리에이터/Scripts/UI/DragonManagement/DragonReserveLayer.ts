
import { _decorator, Component, Node } from 'cc';
import { SubLayer } from '../Common/SubLayer';
import { TapLayer } from '../Common/TapLayer';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = DragonReserveLayer
 * DateTime = Tue Apr 26 2022 19:45:42 GMT+0900 (대한민국 표준시)
 * Author = jinwonjun
 * FileBasename = DragonReserveLayer.ts
 * FileBasenameNoExtension = DragonReserveLayer
 * URL = db://assets/Scripts/UI/DragonManagement/DragonReserveLayer.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('DragonReserveLayer')
export class DragonReserveLayer extends TapLayer {
    
    @property(Node)
    subLayerList : Node[] = [];

    currentLayerIndex : number = -1;

    Init()
    {
        this.moveLayer({"index" : 0});
    }

    ForceUpdate()
    {
        this.subLayerList[this.currentLayerIndex].getComponent(SubLayer).ForceUpdate();
    }

    moveLayer(jsonData)
    {
        if(this.currentLayerIndex == jsonData.index) {
            return;
        }
        this.subLayerList.forEach(element => 
        {
            element.active = false;    
        });

        let tapLayer : SubLayer = this.subLayerList[jsonData.index].getComponent(SubLayer)
        tapLayer.node.active = true;
        tapLayer.Init()        
        this.currentLayerIndex = jsonData.index;
    }

    onClickChangeLayer(event : Event, customEventData)
    {
        let jsonData = JSON.parse(customEventData)
        
        this.moveLayer(jsonData)
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

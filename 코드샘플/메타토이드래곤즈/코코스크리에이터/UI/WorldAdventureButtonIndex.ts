
import { _decorator, Component, Node, CCInteger, Label } from 'cc';
import { StringTable } from '../Data/StringTable';
import { TableManager } from '../Data/TableManager';
import { GetChild, StringBuilder } from '../Tools/SandboxTools';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = WorldAdventureButtonIndex
 * DateTime = Wed May 04 2022 17:33:48 GMT+0900 (대한민국 표준시)
 * Author = jinwonjun
 * FileBasename = WorldAdventureButtonIndex.ts
 * FileBasenameNoExtension = WorldAdventureButtonIndex
 * URL = db://assets/Scripts/UI/WorldAdventureButtonIndex.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('WorldAdventureButtonIndex')
export class WorldAdventureButtonIndex extends Component {
    @property(CCInteger)
    worldIndex : number;

    start() {
        let stringTable = TableManager.GetTable<StringTable>(StringTable.Name);
        if(stringTable != null) {
            let worldText = stringTable.Get(100000602);
            if(worldText != null) {
                let labelNode = this.node.getChildByName("Label");
                if(labelNode != null) {
                    let label = labelNode.getComponent(Label);
                    if(label != null) {
                        label.string = StringBuilder(worldText.TEXT, this.worldIndex);
                    }
                }
            }
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

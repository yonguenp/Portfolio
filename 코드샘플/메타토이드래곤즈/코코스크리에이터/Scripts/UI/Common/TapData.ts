
import { _decorator, Node, Button, EventHandler } from 'cc';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = TapData
 * DateTime = Tue Apr 26 2022 20:01:01 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = TapData.ts
 * FileBasenameNoExtension = TapData
 * URL = db://assets/Scripts/UI/Common/TapData.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('TapData')
export class TapData {
    protected tapIndex = -1;
    public get TabIndex() {
        return this.tapIndex;
    }
    @property(Node)
    protected targetLayer: Node = null;
    public get TargetLayer(): Node {
        return this.targetLayer;
    }
    @property(Button)
    protected targetBtn: Button = null;
    public get TargetBtn(): Button {
        return this.targetBtn;
    }
    @property(Button)
    protected targetClose: Button = null;
    public get TargetClose(): Button {
        return this.targetClose;
    }

    Init(parent: Node, index: number) {
        this.tapIndex = index;
        if(this.tapIndex < 0) {
            return;
        }
        if(this.targetBtn != null) {;
            let newHandler = new EventHandler();
            newHandler.target = parent;
            newHandler.component = "TapExtension";
            newHandler.handler = "OnClickTab";
            newHandler.customEventData = JSON.stringify({index: this.tapIndex});
            this.targetBtn.clickEvents.push(newHandler);
        }
        if(this.targetClose != null) {;
            let newHandler = new EventHandler();
            newHandler.target = parent;
            newHandler.component = "TapExtension";
            newHandler.handler = "OnClickClose";
            newHandler.customEventData = JSON.stringify({index: this.tapIndex});
            this.targetClose.clickEvents.push(newHandler);
        }
    }

    SetVisible(visible: boolean) {
        if(this.targetLayer != null) {
            this.targetLayer.active = visible;
        }
        if(this.targetBtn != null) {
            this.targetBtn.interactable = !visible;
        }
    }

    SetActive(active: boolean) {
        if(this.targetBtn != null) {
            this.targetBtn.node.active = active;
        }
        if(this.targetLayer != null) {
            this.targetLayer.active = false;
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

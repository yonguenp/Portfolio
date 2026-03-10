
import { _decorator, Component, Node, Button, EventHandler } from 'cc';
import { PopupExtension } from './PopupExtension';
import { TapData } from './TapData';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = TapExtension
 * DateTime = Tue Apr 26 2022 19:52:22 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = TapExtension.ts
 * FileBasenameNoExtension = TapExtension
 * URL = db://assets/Scripts/UI/Common/TapExtension.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */

@ccclass('TapExtension')
export class TapExtension extends PopupExtension {
    @property
    protected defaultTap: number = 0;
    @property(Button)
    protected targetExit: Button = null;
    @property({range: TapData[20], type:TapData})
    protected data: TapData[] = [];
    protected stackTap: TapData[] = null;
    
    Init() {
        super.Init();
        if(this.data != null) {
            this.data.forEach((element, index) => {
                if(element == null) {
                    return;
                }
                element.Init(this.node, index);
            });
        }
        this.stackTap = [];
        if(this.targetExit != null) {;
            let newHandler = new EventHandler();
            newHandler.target = this.node;
            newHandler.component = "TapExtension";
            newHandler.handler = "OnClickExit";
            newHandler.customEventData = "";
            this.targetExit.clickEvents.push(newHandler);
        }

        let tempTab = this.popup.Data['index'];
        this.defaultTap = 0;
        if(tempTab != undefined && this.data.length > tempTab && tempTab >= 0) {
            this.defaultTap = tempTab;
        }

        this.OnTab({index: this.data[this.defaultTap].TabIndex});
    }

    ForceUpdate() {
        super.ForceUpdate();
    }

    private OnClickTab(event : Event, data: any) {
        data = JSON.parse(data);
        this.OnTab(data);
    }

    OnTab(data: any) {
        if(this.stackTap == null) {
            this.stackTap = [];
        }
        if(this.data == null || this.data.length <= data['index'] || data['index'] < 0 || this.data[data['index']] == null) {
            return;
        }

        if(this.stackTap.length > 0) {
            this.stackTap.forEach(element => {
                element.SetVisible(false)
            });
        }

        let tabData = this.data[data['index']];
        tabData.SetVisible(true);
        this.stackTap.push(tabData);
    }

    private OnClickClose(event : Event, data: any) {
        data = JSON.parse(data);
        this.OnClose(data);
    }

    OnClose(data: any) {
        if(this.stackTap == null) {
            this.stackTap = [];
        }
        if(this.data == null || this.data.length <= data['index'] || data['index'] < 0 || this.data[data['index']] == null) {
            return;
        }
        let tabData = this.data[data['index']];
        if(tabData == null) {
            if(this.stackTap.length > 0) {
                let target = this.stackTap.pop();
                if(target != null) {
                    this.stackTap[data['index']].SetVisible(false);
                }
            }
            if(this.stackTap.length > 0) {
                let target = this.stackTap[this.stackTap.length - 1];
                if(target != null) {
                    target.SetVisible(true);
                }
            }
            return;
        } else {
            this.stackTap = this.stackTap.filter(element => element != tabData);
            if(this.stackTap.length > 0) {
                this.stackTap.forEach(element => {
                    element.SetVisible(false)
                });
                let target = this.stackTap[this.stackTap.length - 1];
                if(target != null) {
                    target.SetVisible(true);
                }
            }
        }
    }

    private OnClickExit(event : Event, data: any) {
        this.OnExit(data);
    }

    OnExit(data: any) {
        this.stackTap = [];
        this.popup?.ClosePopup();
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

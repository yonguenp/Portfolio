
import { _decorator, Component } from 'cc';
import { PopupExtension } from './PopupExtension';
import { PopupManager } from './PopupManager';
const { ccclass } = _decorator;

/**
 * Predefined variables
 * Name = Popup
 * DateTime = Wed Dec 29 2021 18:38:03 GMT+0900 (대한민국 표준시)
 * Author = seungmin
 * FileBasename = Popup.ts
 * FileBasenameNoExtension = Popup
 * URL = db://assets/Scripts/Popup.ts
 *
 */
@ccclass('Popup')
export class Popup extends Component
{
    protected popupData: {} = null;
    public get Data(): any {
        return this.popupData;
    }
    protected extension: PopupExtension[] = null;

    onLoad() {
        this.OnLoad();
    }

    start() {
        this.OnStart();
    }

    onDestroy() {
        this.OnClose();
    }
    
    /**
     * class PopupManager에서 호출됨
     */
    Init(popupData?: any) {
        if(popupData != undefined || popupData != undefined) {
            this.SetData(popupData);
        }

        this.InitExtenstion();
    }

    protected InitExtenstion() {
        this.extension = this.getComponents(PopupExtension);
        if(this.extension != null) {
            this.extension.forEach(element => {
                element.Init();
            });
        }
    }

    protected OnLoad() {

    }

    protected OnStart() {

    }
    
    /**
     * class PopupManager에서 호출됨
     */
    SetData(popupData: any) {
        this.popupData = popupData;
    }

    /**
     * class Popup의 onDestroy에서 호출됨
     */
    OnClose() {
        
    }

    ForceUpdate() {
        this.extension = this.getComponents(PopupExtension);
        if(this.extension != null) {
            this.extension.forEach(element => {
                element.ForceUpdate();
            });
        }
    }

    ClosePopup() {
        PopupManager.ClosePopup(this);
    }
}

/**
 * [1] Class member could be defined like this.
 * [2] Use `property` decorator if your want the member to be serializable.
 * [3] Your initialization goes here.
 * [4] Your update function goes here.
 *
 * Learn more about scripting: https://docs.cocos.com/creator/3.3/manual/en/scripting/
 * Learn more about CCClass: https://docs.cocos.com/creator/3.3/manual/en/scripting/ccclass.html
 * Learn more about life-cycle callbacks: https://docs.cocos.com/creator/3.3/manual/en/scripting/life-cycle-callbacks.html
 */


import { Button, CCInteger, Color,  Label,  Node,  _decorator } from 'cc';
import { StringTable } from '../Data/StringTable';
import { TableManager } from '../Data/TableManager';
import { Popup } from './Common/Popup';
import { TapLayer } from './Common/TapLayer';
import { ToastMessage } from './ToastMessage';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = DragonManagePopup
 * DateTime = Tue Mar 08 2022 12:00:01 GMT+0900 (대한민국 표준시)
 * Author = wonjun_gs
 * FileBasename = DragonManagePopup.ts
 * FileBasenameNoExtension = DragonManagePopup
 * URL = db://assets/Scripts/UI/DragonManagePopup.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('DragonManagePopup')
export class DragonManagePopup extends Popup {
    @property(Node)
    tapObj : Node[] = []

    @property(Node)
    tapBtn : Node[] = []

    @property(CCInteger)
    defaultTap : number = 0

    currentTop : number = 0
    @property({type : Color})
    public activateColor : Color = new Color();

    @property(Color)
    public deActivateColor : Color = new Color();

    initialTap : number = 0;
    public get InitialTap() { return this.initialTap;}

    Init(data?: any)
    {
        if(data == undefined || data == null) {
            this.initialTap = 0;
        } else {
            if(data['index'] != undefined) {
                this.initialTap = data['index'];
            }
        }
        if(this.initialTap < 0){
            this.initialTap = 0;
        }
        
        if(this.tapObj != null && this.tapObj.length > 0)
        {
            if(this.initialTap == 0)
            {
                this.tapBtn.forEach(element => 
                {
                    element.getComponent(Button).interactable = true;
                    element.getComponentInChildren(Label).color = this.deActivateColor;
                });
            }

            this.tapObj.forEach(element => 
            {
                element.active = false;    
            });

            let tapLayer : TapLayer = null;
            tapLayer = this.tapObj[this.defaultTap].getComponent(TapLayer)
            tapLayer.node.active = true;
            tapLayer.Init()
            this.currentTop = this.initialTap

            this.tapBtn[this.defaultTap].getComponent(Button).interactable = false;
            this.tapBtn[this.defaultTap].getComponentInChildren(Label).color = this.activateColor;
            this.tapBtn[this.defaultTap].active = true;
        }
    }

    onClickTapBtn(event : Event, customEventData)
    {
        let jsonData = JSON.parse(customEventData)
        
        this.moveTap(jsonData)
    }

    ForceUpdate()
    {
        // this.tapBtn.forEach((element, index) => 
        // {
        //     if(index == 0){
        //         element.active = true;
        //     }
        //     else{
        //         element.active = false;
        //     }
        // });

        // if(DataManager.GetData("MainPopupTap") != null)
        // {
        //     this.moveTap(DataManager.GetData("MainPopupTap"))
        //     DataManager.DelData("MainPopupTap")
        // }
        // else
        // {
        //     this.tapObj[this.currentTop].getComponent(TapLayer).forceUpdate()
        // }
        this.tapObj[this.currentTop].getComponent(TapLayer).ForceUpdate();
    }

    moveTap(jsonData)
    {
        this.tapObj.forEach(element => 
        {
            element.active = false;    
        });
        
        if(jsonData.index == 0)
        {
            this.tapBtn.forEach(element => 
            {
                element.getComponent(Button).interactable = true;
                element.getComponentInChildren(Label).color = this.deActivateColor;
            });
            
            this.tapBtn[jsonData.index].getComponent(Button).interactable = false;
            this.tapBtn[jsonData.index].getComponentInChildren(Label).color = this.activateColor;
        }

        let tapLayer : TapLayer = this.tapObj[jsonData.index].getComponent(TapLayer)
        tapLayer.node.active = true;
        tapLayer.Init()        
        this.currentTop = jsonData.index;
    }
    onClickExpectGameAlphaUpdate()
    {
        let emptyCheck = ToastMessage.isToastEmpty();
        if(emptyCheck == false){
            return;
        }
        
        let textData = TableManager.GetTable<StringTable>(StringTable.Name).Get(100000326);
        if(textData != null)
        {
            ToastMessage.Set(textData.TEXT, null, -52);
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

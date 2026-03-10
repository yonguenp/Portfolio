
import { _decorator, Component, Node, Button, SystemEvent, Color, Label, CCString, CCBoolean } from 'cc';
import { DragonManagePopup } from '../DragonManagePopup';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = DragonManageTapButton
 * DateTime = Fri Apr 08 2022 11:06:19 GMT+0900 (대한민국 표준시)
 * Author = wonjun_gs
 * FileBasename = DragonManageTapButton.ts
 * FileBasenameNoExtension = DragonManageTapButton
 * URL = db://assets/Scripts/UI/DragonManagement/DragonManageTapButton.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('DragonManageTapButton')
export class DragonManageTapButton extends Component {

    @property(DragonManagePopup)
    parent : DragonManagePopup = null;
    
    @property(CCString)
    buttonIndex : String;

    @property({type : Color})
    public activateColor : Color = new Color();

    @property(Color)
    public deActivateColor : Color = new Color();

    // start () {
        
    //     this.node.on(Node.EventType.MOUSE_ENTER, this.ChangeLabelColorMouseEnter,this);
        
    //     // this.node.on(Node.EventType.MOUSE_MOVE , function (event) {
    //     //     console.log("detected hover move!");
    //     // });
    //     this.node.on(Node.EventType.MOUSE_LEAVE , this.ChangeLabelColorMouseLeave,this);
    // }

    onLoad()
    {
        this.node.on(Node.EventType.MOUSE_ENTER, this.ChangeLabelColorMouseEnter,this);
        
        // this.node.on(Node.EventType.MOUSE_MOVE , function (event) {
        //     console.log("detected hover move!");
        // });
        this.node.on(Node.EventType.MOUSE_LEAVE , this.ChangeLabelColorMouseLeave,this);

        let labelComponent = this.node.getComponentInChildren(Label);
        if(labelComponent != null){
            labelComponent.color = this.deActivateColor;
        }
    }

    ChangeLabelColorMouseEnter()
    {
        let labelComponent = this.node.getComponentInChildren(Label);
        if(labelComponent != null){
            labelComponent.color = this.activateColor;
        }
    }
    ChangeLabelColorMouseLeave()
    {
        if(Number(this.buttonIndex) >= 0)
        {
            if(Number(this.buttonIndex) == this.parent.InitialTap){
                return;
            }
        }

        let labelComponent = this.node.getComponentInChildren(Label);
        if(labelComponent != null){
            labelComponent.color = this.deActivateColor;
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


import { _decorator, Component, Node, Button, Color, Label, Sprite } from 'cc';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = ButtonChangeChildColor
 * DateTime = Thu May 19 2022 12:05:41 GMT+0900 (대한민국 표준시)
 * Author = jinwonjun
 * FileBasename = ButtonChangeChildColor.ts
 * FileBasenameNoExtension = ButtonChangeChildColor
 * URL = db://assets/Scripts/UI/ButtonChangeChildColor.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('ButtonChangeChildColor')
export class ButtonChangeChildColor extends Component {
    
    @property({type : Color})
    public activateColor : Color = new Color();

    @property(Color)
    public deActivateColor : Color = new Color();

    @property(Label)
    targetLabel : Label = null;

    @property(Sprite)
    targetSprite : Sprite = null;

    start () {
        
    }

    refreshColor()
    {
        let currentButton = this.node.getComponent(Button);
        if(currentButton != null)
        {
            let isInteract = currentButton.interactable;
            if(this.targetLabel != null){
                if(isInteract){
                    this.targetLabel.color = this.activateColor;
                }else{
                    this.targetLabel.color = this.deActivateColor;
                }
            }
            if(this.targetSprite != null){
                if(isInteract){
                    this.targetSprite.color = this.activateColor;
                }else{
                    this.targetSprite.color = this.deActivateColor;
                }
            }
        }
    }

    // update (deltaTime: number) {
    //     // [4]
    // }
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

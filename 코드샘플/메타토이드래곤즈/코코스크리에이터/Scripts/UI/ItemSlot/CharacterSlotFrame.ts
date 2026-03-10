
import { _decorator, Component, Node, instantiate, Layers, Vec3, Animation } from 'cc';
import { ChangeLayer, GetChild } from '../../Tools/SandboxTools';
import { User } from '../../User/User';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = CharacterSlotFrame
 * DateTime = Tue Apr 05 2022 10:49:40 GMT+0900 (대한민국 표준시)
 * Author = wonjun_gs
 * FileBasename = CharacterSlotFrame.ts
 * FileBasenameNoExtension = CharacterSlotFrame
 * URL = db://assets/Scripts/UI/ItemSlot/CharacterSlotFrame.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('CharacterSlotFrame')
export class CharacterSlotFrame extends Component {
    
    @property(Node)
    slotAnimNode : Node = null;
    
    private dragonTag : number = 0;


    private clickCallback: (CustomEventData? : string) => void = null;


    setDragonData(dragonTag : number)
    {
        let element = User.Instance.DragonData.GetDragon(dragonTag)
        if(element == null){
            return;
        }
        let dragonClone = instantiate(User.Instance.DragonData.GetNameDragonSpine(element.GetDesignData().IMAGE))
        const layer = 1 << Layers.nameToLayer('UI_2D');
        ChangeLayer(dragonClone, layer);
        
        dragonClone.scale = new Vec3(2, 2)
        dragonClone.position = new Vec3(0, -40)

        this.HideShadow();
        
            //테스트 라벨
            //GetChild(dragonslot, ['dev_pos', 'labelPos']).getComponent(Label).string = "Pos : " + element.Position;
            //GetChild(dragonslot, ['dev_def', 'labelDef']).getComponent(Label).string = "Def : " + element.GetDragonALLStat().DEF;
        dragonClone.parent = GetChild(this.node, ['Content'])

        this.dragonTag = dragonTag;

        this.HideAnimArrowNode();
    }

    setCallback(ok_cb? : (CustomEventData? : string) => void)
    {
        //this.eFuncType = FrameFunctioal.CallBack;

        if(ok_cb != null)
        {
            this.clickCallback = ok_cb;
        }
    }

    ShowShadow()
    {
        let shadowObj = GetChild(this.node, ['Shadow']);
        if(shadowObj != null){
            shadowObj.active = true;
        }
    }

    HideShadow()
    {
        let shadowObj = GetChild(this.node, ['Shadow']);
            if(shadowObj != null){
                shadowObj.active = false;
            }
    }

    ShowAnimArrowNode()
    {
        if(this.slotAnimNode != null && this.slotAnimNode.activeInHierarchy == false){
            this.slotAnimNode.active = true;

            let animComponent = this.slotAnimNode.getComponent(Animation);
            if(animComponent == null){
                return;
            }
            animComponent.play();
        }

        this.ShowShadow();
    }

    HideAnimArrowNode()
    {
        if(this.slotAnimNode != null && this.slotAnimNode.activeInHierarchy == true){
            this.slotAnimNode.active = false;
        }

        this.HideShadow();
    }

    onClickFrame()
    {   
        if(this.clickCallback != null){
            this.clickCallback(this.dragonTag.toString());
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


import { _decorator, Node, Label, math, UITransform, Vec3 } from 'cc';
import { Popup } from './Common/Popup';
import { ItemFrame } from './ItemSlot/ItemFrame';
const { ccclass, property } = _decorator;
 
@ccclass('ItemTooltip')
export class ItemTooltip extends Popup
{
    @property(Label)
    labelTitle : Label = null

    @property(Label)
    labelBody : Label = null

    @property(Node)
    nodeBg : Node = null

    @property(Node)
    nodeBody : Node = null

    Init(data?: any)
    {
        //위치, 사이즈 조정
        this.nodeBg.worldPosition = new math.Vec3(640, 360);
        super.Init(data);
    }

    setMessage(title : string, body : string)
    {
        this.labelTitle.string = title;
        this.labelBody.string = body;
    }

    setTooltipPosition(pos : Vec3)
    {
        this.nodeBody.worldPosition = new Vec3(pos.x + this.nodeBody.getComponent(UITransform).contentSize.x/2, pos.y + this.nodeBody.getComponent(UITransform).contentSize.y/2)
    }

    getBodyContentSize()
    {
        return this.nodeBody.getComponent(UITransform).contentSize
    }

    OnClose()
    {
        super.OnClose();

        if(this.Data == null){
            return;
        }

        if(this.Data.targetItemFrame == null){
            return;
        }
        
        let data = this.Data.targetItemFrame as ItemFrame;
        if(data != null){
            data.setFrameNormal();
        }
    }
}

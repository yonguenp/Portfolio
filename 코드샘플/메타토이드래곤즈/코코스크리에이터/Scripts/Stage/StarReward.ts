
import { _decorator, Component, Node, Label, color, Color, Sprite, Vec3, ProgressBar } from 'cc';
const { ccclass, property } = _decorator;
 
@ccclass('StarReward')
export class StarReward extends Component 
{
    @property(Label)
    labelReqStarAmount : Label = null

    @property(Node)
    nodeBtnReward : Node = null

    @property(Node)
    clearNode : Node = null;

    private disableYellowDotColor : string = "#524F3F"
    private enbleYellowDotColor : string = "#FFDC51"

    init(reqStarAmount : number, rewardState : number, maxStarAmount : number)
    {
        //this.nodeBtnReward.active = true
        //this.clearNode.active = false;
        this.labelReqStarAmount.string = String(reqStarAmount)
        if(rewardState == 0)
        {
            this.nodeBtnReward.active = false
            this.clearNode.active = false;
            //this.getComponent(Sprite).color = new Color(this.disableYellowDotColor)
        }
        else if(rewardState == 2)
        {
            this.nodeBtnReward.active = false
            this.clearNode.active = true;
            //this.getComponent(Sprite).color = new Color(this.enbleYellowDotColor)
        }
        else if(rewardState == 1)
        {
            this.nodeBtnReward.active = true;
            this.clearNode.active = false;
        }

        let totalLength = this.node.parent.getComponent(ProgressBar).totalLength;//284
        let xPos = Math.round(reqStarAmount / maxStarAmount * totalLength)
        this.node.position = new Vec3(totalLength * (-1) * (0.5) + xPos);
    }

    YellowDot()
    {
        //this.getComponent(Sprite).color = new Color(this.enbleYellowDotColor)
        this.clearNode.active = true;
    }
}
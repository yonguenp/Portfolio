
import { _decorator, Component, Node, SpriteFrame, Sprite, CCInteger, Label } from 'cc';
import { StringBuilder } from '../Tools/SandboxTools';
import { PopupManager } from '../UI/Common/PopupManager';
import { StageInfoPopup } from '../UI/StageInfoPopup';
const { ccclass, property } = _decorator;
 
@ccclass('StageNode')
export class StageNode extends Component 
{
    @property(SpriteFrame)
    sprframeActive : SpriteFrame = null
    
    @property(Sprite)
    sprBtnImage : Sprite = null

    @property(Node)
    nodeLock : Node = null

    @property(Node)
    nodeStarLayout : Node = null

    @property(Node)
    nodeStageLabel : Node = null

    @property(Node)
    nodeStar : Node[] = []

    @property({type:CCInteger})
    stage : number = 0;

    private world : number = 0
    private isLock : boolean = false;
    private activeStar : number = 0;

    init()
    {
        this.nodeLock.active = true

        this.nodeStarLayout.active = false
        this.nodeStar.forEach((element)=>{element.active = false})
        this.nodeStageLabel.active = false
    }

    SetStage(isLock : boolean, activeStar : number = 0, world : number = 0, stage : number = 0)
    {
        this.init()
        if(isLock)
        {
            this.isLock = isLock
            return
        }

        this.isLock = isLock
        this.world = world

        this.nodeStarLayout.active = true
        this.nodeStageLabel.active = true
        this.nodeStageLabel.getComponent(Label).string = StringBuilder("{0}-{1}", world, stage)

        if(activeStar > 0)
            this.sprBtnImage.spriteFrame = this.sprframeActive
        
        this.activeStar = activeStar
        
        this.nodeLock.active = false
        this.nodeStar.forEach((element, index)=>
        {
            if(activeStar >= index +1)
            {
                element.active = true    
            }
        })
    }

    onClickStage()
    {
        if(this.isLock)
            return
        
        let params = { stage : this.stage }
        PopupManager.OpenPopup("StageinfoPopup", true, params) as StageInfoPopup
    }
}
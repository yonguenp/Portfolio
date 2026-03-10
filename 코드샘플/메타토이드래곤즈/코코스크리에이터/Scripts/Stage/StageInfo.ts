
import { _decorator, Component, Node, CCInteger, Vec3 } from 'cc';
import { StageNode } from './StageNode';
const { ccclass, property } = _decorator;
 
@ccclass('StageInfo')
export class StageInfo extends Component 
{
    @property(Node)
    arrStageNode : Node[] = []

    @property(Node)
    nodePole : Node = null

    @property(CCInteger)
    world : number = 0

    private totalStar = 0
    private targetPole = 0

    init(stages : number[] = [])
    {
        let firstZero : boolean = false
        
        for(let i = 0; i < this.arrStageNode.length; i++)
        {
            if(!firstZero)
            {
                this.nodePole.parent = this.arrStageNode[i]
                this.nodePole.position = Vec3.ZERO
            }

            if(stages[i] > 0)
            {
                this.arrStageNode[i].getComponent(StageNode).SetStage(false, stages[i], this.world, i+1)
            }                
            else if(!firstZero)
            {
                firstZero = true
                this.arrStageNode[i].getComponent(StageNode).SetStage(false, stages[i], this.world, i+1)
            }
            else
            {
                this.arrStageNode[i].getComponent(StageNode).SetStage(true)
            }
            
            this.totalStar += stages[i]
        }
    }

    get TotalStar() : number
    {
        return this.totalStar
    }
}


import { _decorator, Node, Label, instantiate } from 'cc';
import { MonsterSpawnTable } from '../Data/MonsterTable';
import { StageBaseTable } from '../Data/StageTable';
import { TableManager } from '../Data/TableManager';
import { GameManager } from '../GameManager';
import { ResourceManager, ResourcesType } from '../ResourceManager';
import { DataManager } from '../Tools/DataManager';
import { StringBuilder } from '../Tools/SandboxTools';
import { World } from '../World';
import { Popup } from './Common/Popup';
import { PopupManager } from './Common/PopupManager';
import { ExpFrame, ExpType } from './ItemSlot/ExpFrame';
import { ItemFrame } from './ItemSlot/ItemFrame';
import { StagePreparePopup } from './StagePreparePopup';
const { ccclass, property } = _decorator;
 
@ccclass('StageInfoPopup')
export class StageInfoPopup extends Popup
{
    @property(Label)
    labelStage : Label = null

    @property(Node)
    nodeContentParent : Node = null

    @property(Label)
    labelBattlePoint : Label = null
    
    @property(Node)
    arrNodeStar : Node[] = []

    private stage : number = 0

    Init(data?: any)
    {
        let worldInfo : any = (DataManager.GetData("GlobalStageData") as any).stageinfo
        this.stage = data.stage
        let stageInfo = TableManager.GetTable<StageBaseTable>(StageBaseTable.Name).GetByWorldStage(worldInfo.world, this.stage)

        this.labelStage.string = StringBuilder("{0}-{1}", worldInfo.world, this.stage)
        this.arrNodeStar.forEach((element, index) => 
        {
            if(worldInfo.stages[this.stage-1] > index)
            {
                element.active = true
            }
        })

        let spawnInfo = TableManager.GetTable<MonsterSpawnTable>(MonsterSpawnTable.Name).Get(stageInfo.SPAWN)
        let enemyTotalBp : number = 0

        spawnInfo.forEach((element)=>
        {
            enemyTotalBp += element.INF
        })

        if(stageInfo.ACCOUNT_EXP > 0)
        {
            let clone : Node = instantiate(GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB)["ExpSlot"])
            clone.getComponent(ExpFrame).setFrameExpInfo(ExpType.ACCOUNT_EXP, stageInfo.ACCOUNT_EXP)

            clone.parent = this.nodeContentParent
        }
        
        if(stageInfo.CHAR_EXP > 0)
        {
            let clone : Node = instantiate(GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB)["ExpSlot"])
            clone.getComponent(ExpFrame).setFrameExpInfo(ExpType.CHAR_EXP, stageInfo.CHAR_EXP)

            clone.parent = this.nodeContentParent
        }

        if(stageInfo.REWARD_GOLD > 0)
        {
            let clone : Node = instantiate(GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB)["item"])
            clone.getComponent(ItemFrame).setFrameCashInfo(0, stageInfo.REWARD_GOLD)

            clone.parent = this.nodeContentParent
        }

        if(stageInfo.REWARD_ITEM > 0)
        {
            let clone : Node = instantiate(GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB)["item"])
            clone.getComponent(ItemFrame).setFrameItemInfo(stageInfo.REWARD_ITEM, stageInfo.REWARD_ITEM_COUNT)

            clone.parent = this.nodeContentParent
        }

        //필요 전투력 적용
        this.labelBattlePoint.string = enemyTotalBp.toString();

        super.Init(data);
    }

    onClickDeploy()
    {
        let params = {stage : this.stage}
        let popup = PopupManager.OpenPopup("StagePreparePopup", true, params) as StagePreparePopup

        World.SetUIStageView(popup.GetUIParent())
        this.ClosePopup()
    }
}

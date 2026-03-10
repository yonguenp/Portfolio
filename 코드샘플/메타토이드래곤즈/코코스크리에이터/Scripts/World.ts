
import { _decorator, Component, Label, Node, instantiate, ProgressBar, Animation, Sprite, Button, EventHandler, Color } from 'cc';
import { AccountTable } from './Data/AccountTable';
import { TableManager } from './Data/TableManager';
import { WorldBaseTable } from './Data/WorldTable';
import { GameManager } from './GameManager';
import { NetworkManager } from './NetworkManager';
import { QuestManager, QuestObject } from './QuestManager';
import { ResourceManager, ResourcesType } from './ResourceManager';
import { SceneManager } from './SceneManager';
import { SoundMixer, SOUND_TYPE } from './SoundMixer';
import { StageInfo } from './Stage/StageInfo';
import { StarReward } from './Stage/StarReward';
import { DataManager } from './Tools/DataManager';
import { GetChild, GetType, ObjectCheck, StringBuilder, Type } from './Tools/SandboxTools';
import { PopupManager } from './UI/Common/PopupManager';
import { StagePreparePopup } from './UI/StagePreparePopup';
import { SystemRewardPopup } from './UI/SystemRewardPopup';
import { User } from './User/User';
const { ccclass, property } = _decorator;

@ccclass('World')
export class World extends Component 
{
    private static instance : World = null;

    @property(Label)
    labelUserLevel : Label = null

    @property(Label)
    labelUserName : Label = null

    @property(Label)
    labelUserExp : Label = null

    @property(ProgressBar)
    progressUserExp : ProgressBar = null

    @property(Label)
    labelGold : Label = null

    @property(Label)
    labelCurEnergy : Label = null

    @property(Label)
    labelMaxEnergy : Label = null

    @property(Node)
    nodeFog : Node = null

    @property(Node)
    nodeWorldParent = null

    @property(ProgressBar)
    progStarReward : ProgressBar = null

    @property(Node)
    arrBtnStarReward : Node[] = []

    @property(Node)
    difficultDropdown : Node = null

    @property(Node)
    nodePopupParent : Node = null

    @property(Node)
    nodeDefaultUIParent : Node = null
    @property(Node)
    nodeStageUserBundle : Node = null
    @property(Node)
    nodeStageCashBundle : Node = null

    private gameManager: GameManager = null
    private stageInfo : StageInfo = null
    private maxStarAmount : number = 0
    private curStarAmount : number = 0
    private StageData : {[key: string]: any}

    onLoad() 
    {
        this.gameManager = GameManager.Instance;
    }

    start() 
    {
        if(World.instance == null)
            World.instance = this

        SoundMixer.DestroyAllClip()
        SoundMixer.EnableMouseClickSFX(this.node)
        //PopupManager.GetInstance.Init(this.nodePopupParent)
        NetworkManager.Instance.UIUpdater = World.RefershUI;

        let world = 1;
        this.StageData  = DataManager.GetData("StageEndData")
        if(this.StageData != null)
        {
            world = this.StageData.world
        }
        else
        {
            this.StageData = {};
            DataManager.AddData("GlobalStageData", this.StageData)
        }

        //임시 코드
        //나중에 World선택에서 넘어와야 함
        let params = {world : world}
        
        NetworkManager.Send("adventure/progress", params, (jsonData) => 
        {
            if(ObjectCheck(jsonData, "info"))
            {
                (DataManager.GetData("GlobalStageData") as any).stageinfo  = jsonData.info[0]

                let world : Node = instantiate(GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.STAGE_PREFAB)["stage" + jsonData.info[0].world])
                world.parent = this.nodeWorldParent
                this.stageInfo = world.getComponent(StageInfo)
                this.StageData = jsonData.info[0]
            }

            World.RefershUI();
            this.FadeOutFog()
        })
    }

    FadeInFog()
    {
        this.nodeFog.getComponent(Animation).play("fog_fadeIn")
    }

    FadeOutFog()
    {
        this.nodeFog.getComponent(Animation).play("fog_fadeOut")
    }

    update (deltaTime: number) 
    {
        this.gameManager.Update(deltaTime);
    }

    onDestroy()
    {
        SoundMixer.DisableMouseClickSFX(this.node)
        NetworkManager.Instance.UIUpdater = null;
        World.instance = null
    }

    private static RefershUI() : void {
        if (World.instance == null) {
            return;
        }
        World.instance.UpdateUICanvas()
    }


    UpdateUICanvas()
    {
        this.labelUserLevel.string = StringBuilder("Lv. {0}", User.Instance.UserData.Level)
        this.labelUserName.string = StringBuilder("{0}", User.Instance.UserData.UserID)
        
        this.labelGold.string = StringBuilder("{0}", User.Instance.UserData.Gold)
        this.labelCurEnergy.string = StringBuilder("{0} / ", User.Instance.UserData.Energy)
        this.labelMaxEnergy.string = StringBuilder("{0}", TableManager.GetTable<AccountTable>(AccountTable.Name).Get(User.Instance.UserData.Level).MAX_STAMINA)

        let accountLevelData = TableManager.GetTable<AccountTable>(AccountTable.Name).Get(User.Instance.UserData.Level).TOTAL_EXP
        let devider = TableManager.GetTable<AccountTable>(AccountTable.Name).Get(User.Instance.UserData.Level).EXP

        this.labelUserExp.string = String(User.Instance.UserData.Exp - accountLevelData)
        this.progressUserExp.progress = Math.fround((User.Instance.UserData.Exp - accountLevelData) / (devider))

        this.stageInfo.init(this.StageData.stages)
        this.maxStarAmount = TableManager.GetTable<WorldBaseTable>(WorldBaseTable.Name).Get(1001).STAR[2]

        this.curStarAmount = 0
        this.StageData.stages.forEach((element) => {this.curStarAmount += element})

        this.progStarReward.progress =  this.curStarAmount / this.maxStarAmount

        let worldStarReward = TableManager.GetTable<WorldBaseTable>(WorldBaseTable.Name).Get(1001).STAR
        
        this.arrBtnStarReward.forEach((element, index) => 
        {
            element.getComponent(StarReward).init(worldStarReward[index], this.StageData.rewarded[index], worldStarReward[2])
        })

        if(DataManager.GetData("StageEndData") != null && (DataManager.GetData("StageEndData") as any).next)
        {
            let params = {stage :  (DataManager.GetData("StageEndData") as any).stage}
            let popup = PopupManager.OpenPopup("StagePreparePopup", true, params) as StagePreparePopup

            World.SetUIStageView(popup.GetUIParent())
            DataManager.DelData("StageEndData")
        }
    }
    //월드 변경 onClick func

    onClickVillage()
    {
        // this.FadeInFog()
        // this.scheduleOnce(()=>
        // {
        // }, 1)
        SoundMixer.DestroyAllClip()
        SceneManager.SceneChange("game");
    }

    onClickStarReward(event, CustomEventData)
    {
        let params = { step : Number(CustomEventData), world : this.StageData.world, diff : 1}
        NetworkManager.Send("adventure/reward", params, (jsonData) => 
        {
            let popup : SystemRewardPopup = PopupManager.OpenPopup("SystemRewardPopup", true) as SystemRewardPopup
            popup.initWithList(jsonData.rewards)
            event.currentTarget.active = false
            event.currentTarget.parent.getComponent(StarReward).YellowDot()
        })
    }

    onClickChangeDifficult()
    {
        this.difficultDropdown.active = !this.difficultDropdown.active
    }

    private StageView(newUiParentNode : Node)
    {
        this.nodeStageUserBundle.parent = newUiParentNode
        this.nodeStageCashBundle.parent = newUiParentNode

        // this.nodeStageNameBundle.active = false
        // this.nodeStageStarBundle.active = false
        // this.nodeStageButtonBundle.active = false
    }

    private WorldView()
    {
        this.nodeStageUserBundle.parent =(this.nodeDefaultUIParent)
        this.nodeStageCashBundle.parent =(this.nodeDefaultUIParent)

        // this.nodeStageNameBundle.active = true
        // this.nodeStageStarBundle.active = true
        // this.nodeStageButtonBundle.active = true
    }

    static SetUIStageView(newUiParentNode : Node)
    {
        World.instance.StageView(newUiParentNode)
    }

    static SetUIWorldView()
    {
        World.instance.WorldView()
    }
}

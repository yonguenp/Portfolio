import { _decorator, Component, Label, Node, instantiate, ProgressBar, Animation, Sprite, Button, EventHandler, Color, Prefab, Event } from 'cc';
import { AccountTable } from './Data/AccountTable';
import { StringTable } from './Data/StringTable';
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
import { TutorialManager } from './Tutorial/TutorialManager';
import { PopupManager } from './UI/Common/PopupManager';
import { StagePrepareComponent } from './UI/StagePrepareComponent';
import { StagePreparePopup } from './UI/StagePreparePopup';
import { SystemRewardPopup } from './UI/SystemRewardPopup';
import { ToastMessage } from './UI/ToastMessage';
import { WorldAdventureButtonIndex } from './UI/WorldAdventureButtonIndex';
import { User } from './User/User';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = WorldAdventure
 * DateTime = Tue May 03 2022 14:12:01 GMT+0900 (대한민국 표준시)
 * Author = jinwonjun
 * FileBasename = WorldAdventure.ts
 * FileBasenameNoExtension = WorldAdventure
 * URL = db://assets/Scripts/WorldAdventure.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */

@ccclass('WorldAdventure')
export class WorldAdventure extends Component 
{
    private static instance : WorldAdventure = null;

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

    @property(ProgressBar)
    progStarReward : ProgressBar = null

    @property(Node)
    arrBtnStarReward : Node[] = []

    @property(Node)
    nodePopupParent : Node = null

    @property(Node)
    nodeDefaultUIParent : Node = null
    @property(Node)
    nodeStageUserBundle : Node = null
    @property(Node)
    nodeStageCashBundle : Node = null

    @property(StagePrepareComponent)
    stagePrepare : StagePrepareComponent = null;


    @property(Node)
    worldChangeDropDown : Node = null;

    @property(Label)
    worldButtonLabel : Label = null;

    @property(Label)
    worldNameLabel : Label = null;

    private gameManager: GameManager = null
    //private stageInfo : StageInfo = null
    private maxStarAmount : number = 0
    private curStarAmount : number = 0
    private StageData : {[key: string]: any}

    onLoad() 
    {
        this.gameManager = GameManager.Instance;
    }

    start() 
    {
        if(WorldAdventure.instance == null)
            WorldAdventure.instance = this

        SoundMixer.DestroyAllClip()
        SoundMixer.EnableMouseClickSFX(this.node)
        NetworkManager.Instance.UIUpdater = WorldAdventure.RefreshUI;

        //this.SetWorldAndStageByCursorData();//서버에 저장된 커서 기반
        this.SetWorldAndStageByUserProgress();//유저가 가장 마지막에 전투 가능한 위치

        WorldAdventure.RefreshUI();
        this.FadeOutFog();
    }

    SetWorldAndStageByCursorData()//서버에서 온 가장 마지막 전투 위치로 세팅하기
    {
        let cursorData = User.Instance.PrefData.AdventureProgressData.GetCurrentLastCursor();//커서 데이터 연결
        let world = cursorData[0];
        let stage = cursorData[1];
        this.StageData  = DataManager.GetData("StageEndData")
        if(this.StageData != null)//이전 전투 내역 있음
        {
            world = this.StageData.world
        }
        else
        {
            this.StageData = {};
            DataManager.AddData("GlobalStageData", this.StageData)
        }

        this.SetWorldDataByUserData(world);
        this.StageData['stage'] = stage;
    }

    SetWorldAndStageByUserProgress()//월드 스테이지 데이터 기준으로 진행할 가장 마지막 스테이지 세팅하기
    {
        //world 및 stage 인덱스 가져오기
        let worldAndStageList = User.Instance.PrefData.AdventureProgressData.GetUserAvailableWorldAndStage();
        let world = worldAndStageList[0];
        let stage = worldAndStageList[1];
        this.StageData  = DataManager.GetData("StageEndData")
        if(this.StageData != null)//이전 전투 내역 있음
        {
            world = this.StageData.world;
            stage = this.StageData.stage;
            DataManager.DelData("StageEndData");
        }
        else
        {
            this.StageData = {};
            DataManager.AddData("GlobalStageData", this.StageData)
        }

        this.SetWorldDataByUserData(world);
        this.StageData['stage'] = stage;
    }

    SetWorldDataByUserData(worldIndex : number)
    {
        let WorldStageInfoData = User.Instance.PrefData.AdventureProgressData.GetWorldInfoData(worldIndex);
        (DataManager.GetData("GlobalStageData") as any).stageinfo = WorldStageInfoData;
        this.StageData = WorldStageInfoData;
        if(this.worldNameLabel != null) {
            let worldData = TableManager.GetTable<WorldBaseTable>(WorldBaseTable.Name).GetAll().find(element => element.NUM == worldIndex);
            if(worldData != null) {
                this.worldNameLabel.string = StringTable.GetString(worldData.NAME, '월드');
            }
        }
    }

    RefreshServerData(worldIndex : number , showFogAnim : boolean)//서버에서 받은 데이터가 없으면 호출
    {
        let params = {world : worldIndex}
        
        NetworkManager.Send("adventure/progress", params, (jsonData) => 
        {
            if(ObjectCheck(jsonData, "info"))
            {
                (DataManager.GetData("GlobalStageData") as any).stageinfo  = jsonData.info[0]

                this.StageData = jsonData.info[0]
            }

            WorldAdventure.RefreshUI();
            if(showFogAnim){
                this.FadeOutFog()
            }
        })
    }

    FadeInFog()
    {
        if(this.nodeFog.activeInHierarchy == false){
            this.nodeFog.active = true;
        }
        this.nodeFog.getComponent(Animation).play("fog_fadeIn")
    }

    FadeOutFog()
    {
        if(this.nodeFog.activeInHierarchy == false){
            this.nodeFog.active = true;
        }
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
        WorldAdventure.instance = null
    }

    static RefreshUI() : void {
        if (WorldAdventure.instance == null) {
            return;
        }
        WorldAdventure.instance.UpdateUICanvas()
    }


    UpdateUICanvas()
    {
        this.UpdateUserUI()
        this.UpdateWorldStarUI();
        this.UpdateStageInfo();
        this.RefreshWorldButtonLabel();
        this.GotoDirectWorld();
        this.UpdateNextStage();
        this.UpdateQuestDirectWorld();
        this.UpdateTutorialHardCoding();
    }

    UpdateNextStage()
    {
        //stageReward에서 다음 스테이지 누를시에
        if(DataManager.GetData("StageEndData") != null && (DataManager.GetData("StageEndData") as any).next)
        {
            let params = {stage :  (DataManager.GetData("StageEndData") as any).stage}
            //let popup = PopupManager.OpenPopup("StagePreparePopup", true, params) as StagePreparePopup
            //WorldAdventure.SetUIStageView(popup.GetUIParent())

            if(this.stagePrepare != null)
            {
                this.stagePrepare.directClickStage(params.stage);
            }

            DataManager.DelData("StageEndData")
        }
    }

    UpdateUserUI()
    {
        this.labelUserLevel.string = StringBuilder("Lv. {0}", User.Instance.UserData.Level)
        this.labelUserName.string = StringBuilder("{0}", User.Instance.UserData.UserID)
        
        this.labelGold.string = StringBuilder("{0}", User.Instance.UserData.Gold)
        this.labelCurEnergy.string = StringBuilder("{0} / ", User.Instance.UserData.Energy)
        this.labelMaxEnergy.string = StringBuilder("{0}", TableManager.GetTable<AccountTable>(AccountTable.Name).Get(User.Instance.UserData.Level).MAX_STAMINA)

        let accountLevelData = TableManager.GetTable<AccountTable>(AccountTable.Name).Get(User.Instance.UserData.Level).TOTAL_EXP
        let devider = TableManager.GetTable<AccountTable>(AccountTable.Name).Get(User.Instance.UserData.Level+1).EXP

        this.labelUserExp.string = String(User.Instance.UserData.Exp - accountLevelData)
        this.progressUserExp.progress = Math.fround((User.Instance.UserData.Exp - accountLevelData) / devider)
    }

    UpdateWorldStarUI()
    {
        this.maxStarAmount = TableManager.GetTable<WorldBaseTable>(WorldBaseTable.Name).Get(1001).STAR[2]

        this.curStarAmount = 0
        if(this.StageData.stages != null && this.StageData.stages != undefined && this.StageData.stages.length > 0){
            this.StageData.stages.forEach((element) => {this.curStarAmount += element})
        }

        this.progStarReward.progress =  this.curStarAmount / this.maxStarAmount

        let worldStarReward = TableManager.GetTable<WorldBaseTable>(WorldBaseTable.Name).Get(1001).STAR
        
        this.arrBtnStarReward.forEach((element, index) => 
        {
            element.getComponent(StarReward).init(worldStarReward[index], this.StageData.rewarded[index], worldStarReward[2])
        })
    }

    UpdateStageInfo()
    {
        if(this.stagePrepare == null || this.StageData == null){
            return;
        }
 
        this.StageData['this'] = this;
        if(this.StageData['stage'] == undefined) 
        {
            //this.StageData['stage'] = 1;
            //현재 열려있는 스테이지 인덱스 가져오기
            let currentOpenStage =this.stagePrepare.GetCurrentOpenStage();
            this.StageData['stage'] = currentOpenStage;
        }
        this.stagePrepare.SetData(this.StageData);
    }

    UpdateQuestDirectWorld()
    {
        let directWorldData : any = DataManager.GetData("QuestDirectWorld");
        if(directWorldData == null){
            return;
        }

        let world = directWorldData.world as number;
        let stage = directWorldData.stage as number;

        DataManager.DelData("QuestDirectWorld");

        let worldAvailableCheck = User.Instance.PrefData.AdventureProgressData.isAvailableWorld(world);
        if(worldAvailableCheck == false){
            ToastMessage.Set(StringTable.GetString(100000628));
            return;
        }

        if(this.stagePrepare != null){
            let isLock = this.stagePrepare.IsLockWorldStage(world, stage);
            if(!isLock)//잠금 해제 상태 - 진입 가능
            {
                this.ChangeWorldAndSelectStage(world, stage);
            }
            else//진입 불가능
            {
                let gotoLastIndex = this.stagePrepare.GetFirstZeroWorldIndex(world);
                this.ChangeWorldAndSelectStage(world, gotoLastIndex);
                ToastMessage.Set(StringTable.GetString(100000629));
            }
        }
    }

    UpdateTutorialHardCoding()
    {
        if(TutorialManager.GetInstance.IsNowPlayingTutorialByGroup(106))
        {
            this.stagePrepare.directClickStage(1);
        }
    }

    ChangeWorldAndSelectStage(worldIndex : number , stageIndex : number)
    {
        if(worldIndex == this.StageData.world)//기존 스테이지가 같으면 스테이지만 열어놓기
        {
            this.stagePrepare.directClickStage(stageIndex);     
        }else{
            let changeCheck = this.ChangeWorld(worldIndex);//월드 이동
            if(changeCheck)
            {
                this.stagePrepare.directClickStage(stageIndex);//진행가능 가장 마지막 스테이지 이동
            }
        }
    }

    GotoDirectWorld()
    {
        let stage = this.StageData.stage;
        this.stagePrepare.directClickStage(stage);
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
            this.SetWorldDataByUserData(params.world);
        })
    }

    onClickChangeWorldDropDown()
    {
        this.worldChangeDropDown.active = !this.worldChangeDropDown.active
    }

    onClickChangeWorld(event : Event, customEventData)
    {
        this.onClickChangeWorldDropDown();

        let clickNode = event.currentTarget as Node;
        if(clickNode == null){
            return;
        }

        let clickIndexComp = clickNode.getComponent(WorldAdventureButtonIndex);
        if(clickIndexComp == null){
            return;
        }

        let clickIndex = clickIndexComp.worldIndex;
        this.ChangeWorld(clickIndex);
    }

    RefreshWorldButtonLabel()
    {
        if(this.worldButtonLabel == null){
            return;
        }

        let stringTable = TableManager.GetTable<StringTable>(StringTable.Name);
        if(stringTable != null) {
            let worldText = stringTable.Get(100000602);
            if(worldText != null) {
                this.worldButtonLabel.string = StringBuilder(worldText.TEXT, this.StageData.world);
            }
        } else {
            this.worldButtonLabel.string = `World ${this.StageData.world}`;
        }
    }

    ChangeWorld(worldindex : number) : boolean
    {
        let currentWorldData : any = DataManager.GetData("GlobalStageData");
        if(currentWorldData != null){
            let stageData = currentWorldData.stageinfo;
            if(stageData != null)
            {   
                let worldIndex = stageData.world;
                
                if(worldindex == worldIndex)//중복 클릭 처리
                {
                    return false;
                }
            }
        }

        let worldAvailableCheck = User.Instance.PrefData.AdventureProgressData.isAvailableWorld(worldindex);
        if(worldAvailableCheck == false){
            ToastMessage.Set(StringTable.GetString(100000628));
            return false;
        }

        let world = worldindex;
        if(DataManager.GetData("GlobalStageData") != null){
            DataManager.DelData("GlobalStageData");
        }
        this.StageData = {};
        DataManager.AddData("GlobalStageData", this.StageData)

        //this.RefreshServerData(world, false);
        this.SetWorldDataByUserData(world);
        this.StageData['stage'] = 1;//월드 변경시에는 1로 세팅
        WorldAdventure.RefreshUI();
        this.FadeOutFog();
        return true;
    }

    //일단 임시로 박아놓음
    onClickLeftWorldButton()
    {
        let world = this.StageData.world;
        if(world == 1){
            this.ChangeWorld(2);
        }
        else{
            this.ChangeWorld(1);
        }
    }

    onClickRightWorldButton()
    {
        let world = this.StageData.world;
        if(world == 1){
            this.ChangeWorld(2);
        }
        else{
            this.ChangeWorld(1);
        }
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
        WorldAdventure.instance.StageView(newUiParentNode);
    }

    static SetUIWorldView()
    {
        WorldAdventure.instance.WorldView()
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

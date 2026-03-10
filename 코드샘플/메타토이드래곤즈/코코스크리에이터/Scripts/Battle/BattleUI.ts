import { _decorator, Component, Node, Label, ProgressBar, director, Animation, instantiate, Prefab, Canvas, Vec3, sys, Button } from 'cc';
import { EventListener } from 'sb';
import { NetworkManager } from '../NetworkManager';
import { DataManager } from '../Tools/DataManager';
import { GetChild, StringBuilder, TimeStringMinute } from '../Tools/SandboxTools';
import { Stage, StageData } from './Stage';
import { SoundMixer, SOUND_TYPE } from '../SoundMixer';
import { ResourceManager, ResourcesType } from '../ResourceManager';
import { GameManager } from '../GameManager';
import { WaveEvent } from './WaveBase';
import { TableManager } from '../Data/TableManager';
import { StageBaseTable } from '../Data/StageTable';
import { StageBaseData } from '../Data/StageData';
import { SceneManager } from '../SceneManager';
import { SkillTimerSlotFrame } from '../UI/ItemSlot/SkillTimerSlotFrame';
import { PopupManager } from '../UI/Common/PopupManager';
import { StringTable } from '../Data/StringTable';
import { SystemPopup } from '../UI/SystemPopup';
import { SkillEvent } from './SkillEvent';
import { EventManager } from '../Tools/EventManager';
import { BattleSpeedValue } from '../Tools/CommonEnum';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = BattleUI
 * DateTime = Mon Feb 21 2022 21:29:58 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = BattleUI.ts
 * FileBasenameNoExtension = BattleUI
 * URL = db://assets/Scripts/Battle/BattleUI.ts
 *
 */
export class DragonStatUI {
    private cooltime_Label: Label = null;
    public get Cooltime_Label(): Label {
        return this.cooltime_Label;
    }
    public set Cooltime_Label(value: Label) {
        this.cooltime_Label = value;
    }
    private cooltime_handle: ProgressBar = null;
    public get Cooltime_handle(): ProgressBar {
        return this.cooltime_handle;
    }
    public set Cooltime_handle(value: ProgressBar) {
        this.cooltime_handle = value;
    }
    private stroke: Node = null;
    public get Stroke(): Node {
        return this.stroke;
    }
    public set Stroke(value: Node) {
        this.stroke = value;
    }
    private hpBar: ProgressBar = null;
    public get HpBar(): ProgressBar {
        return this.hpBar;
    }
    public set HpBar(value: ProgressBar) {
        this.hpBar = value;
    }
    private skull : Node = null;
    public get Skull() : Node {
        return this.skull;
    }
    public set Skull(node : Node) {
        this.skull = node;
    }
}

@ccclass('BattleUI')
export class BattleUI extends Component implements EventListener<SkillEvent>
{
    private static instance : BattleUI = null
    static GetInstance(){return this.instance;}

    @property(Node)
    mapCanvas : Node = null
    mapNode : Node = null

    @property(Node)
    nodePausePopup : Node = null

    @property(Node)
    nodeSkillObjParent : Node = null

    @property(Prefab)
    prefSkillObj : Prefab = null

    @property(Stage)
    stageObj : Stage = null

    @property(Label)
    labelStageName : Label = null

    @property(Label)
    labelStageNumber : Label = null

    @property(Label)
    labelTimer : Label = null

    @property(Label)
    labelWave : Label = null

    @property(Node)
    arrNodeCooltime : Node[] = []

    @property(Node)
    autoIconAnimNode : Node = null;

    @property(Label)
    labelBSpeed : Label = null;

    private arrDragonStatUI: DragonStatUI[] = null;

    private arrCooltime : number[] = null;
    private arrCurCooltime : number[] = null;
    private stageData : StageData = null;
    private targetData : StageBaseData = null;
    private flashAnimation: Animation = null;
    private resources: ResourceManager = null;
    private autoAdventureActive : boolean = false;

    private autoCurrentCount : Label = null;
    private autoTotalCount : Label = null;

    onLoad() {
        if(BattleUI.instance == null)
        {
            BattleUI.instance = this;
        }
        this.arrCooltime = new Array(5);
        this.arrCurCooltime = new Array(5);
        let effectNode = this.node.getChildByName('effect');
        if(effectNode != null) {
            this.flashAnimation = effectNode.getComponent(Animation);
        }
    }

    start()
    {
        EventManager.AddEvent(this);
        
        this.arrDragonStatUI = [];

        if(this.resources == null) {
            this.resources = GameManager.GetManager<ResourceManager>(ResourceManager.Name);
        }

        if(this.stageObj != null) {
            this.stageObj.Init();
        }

        let stageData : any = DataManager.GetData('stageStartData');
        if(stageData != null) {
            const world = stageData.world;
            const stage = stageData.stage;
            this.targetData = TableManager.GetTable<StageBaseTable>(StageBaseTable.Name)?.GetByWorldStage(world, stage);
            var stageName = `STAGE_${world}`;
            if (this.targetData != null && this.targetData.IMAGE != 'NONE') {
                stageName = this.targetData.IMAGE;
            }
            let mapPrefab = this.resources.GetResource(ResourcesType.STAGE_PREFAB)[stageName];
            this.mapNode = instantiate(mapPrefab);
            if(this.mapNode != null) {
                this.mapCanvas.addChild(this.mapNode);
        
                let map = this.mapNode.getChildByName('MAP');
                if(map != null && stageData != null) {
                    if(this.resources != null && this.resources.LoadComplete) {
                        WaveEvent.TriggerEvent({
                            state: 'Init',
                            mapNode: map,
                            stageData: stageData
                        });
                    }
                }
            }
        }

        this.SetAutoAdventureAnim();

        DataManager.DelData('stageStartData');

        BattleUI.UIResize();

        this.labelBSpeed.string = StringBuilder("x {0}", BattleSpeedValue[sys.localStorage.getItem("BSpeed") != null ? Number(sys.localStorage.getItem("BSpeed")) : 0]);
    }

    static UIUpdate()
    {
        BattleUI.instance.arrDragonStatUI.forEach((element, index)=>
        {
            if(element == null) {
                return;
            }
            let curDragon = BattleUI.instance.stageData.Dragons[index];
            if (curDragon.Death) {
                BattleUI.instance.OnDragonDeath(index)
                return
            } else if (null == curDragon) {
                BattleUI.instance.DeactiveSkillCoolTime(index)
                return
            }

            element.HpBar.progress = curDragon.HP / curDragon.MAXHP;

            if(curDragon.DelaySkill1 > 0)
            {
                element.Cooltime_Label.string = String(Math.round(curDragon.DelaySkill1+0.5))
                element.Cooltime_handle.node.active = true;
                element.Cooltime_Label.node.active = true;
                element.Stroke.active = false;

                if(curDragon.DelaySkill1 <= 0)
                {
                    element.Cooltime_handle.node.active = false;
                    element.Cooltime_Label.node.active = false;
                    element.Stroke.active = true;
                }
                else
                {
                    element.Cooltime_handle.progress = curDragon.DelaySkill1 / BattleUI.instance.arrCooltime[index]
                }
            }
            else
            {
                element.Cooltime_handle.node.active = false;
                element.Cooltime_Label.node.active = false;
                element.Stroke.active = true;
            }            
        });
        
        BattleUI.UIResize();
    }

    static UIResize() {
        if(BattleUI.instance != null && BattleUI.instance.mapCanvas != null) {
            let canvas = BattleUI.instance.mapCanvas.getComponent(Canvas);
            if(canvas != null && BattleUI.instance.mapNode != null) {
                canvas.cameraComponent.node.worldPosition = new Vec3(BattleUI.instance.mapNode.worldPosition.x - 286, BattleUI.instance.mapNode.worldPosition.y + 50, 10) ;
            }
        }
    }

    private OnDragonDeath(index : number) {
        this.DeactiveSkillCoolTime(index)
        this.ActivateDeadIndicator(index)
        this.ClearBuffIcon(index);
    }

    private DeactiveSkillCoolTime(index : number)
    {
        BattleUI.instance.arrDragonStatUI[index].Cooltime_Label.string = ""
        BattleUI.instance.arrDragonStatUI[index].HpBar.progress = 0
        BattleUI.instance.arrDragonStatUI[index].Cooltime_handle.progress = 1
        BattleUI.instance.arrDragonStatUI[index].Stroke.active = false
    }

    private ActivateDeadIndicator(index : number) {
        BattleUI.instance.arrDragonStatUI[index].Skull.active = true
    }

    static init(stageData : StageData, worldData: any)
    {
        BattleUI.instance.stageData = stageData;

        BattleUI.instance.labelStageName.string = "";//"테스트 스테이지"
        BattleUI.instance.labelStageNumber.string = StringBuilder("{0}-{1}", worldData.world, worldData.stage)
        BattleUI.instance.labelWave.string = StringBuilder("{0}/{1}", stageData.CurWave, stageData.MaxWave)        
        BattleUI.instance.CreateSkillObj(stageData)
        
        stageData.Dragons.forEach((element, index)=>
        {
            BattleUI.RegistSkiil(index, element.Skill1.COOL_TIME, element.Skill1.START_COOL_TIME)
        })

        BattleUI.instance.arrNodeCooltime.forEach((element, index)=>
        {
            if(element == null) {
                return;
            }
            let curDragonUI = new DragonStatUI();
            let Cooltime_handle = element.getChildByName("Cooltime_handle");
            if(Cooltime_handle != null) {
                curDragonUI.Cooltime_handle = Cooltime_handle.getComponent(ProgressBar);
            }
            let Cooltime_Label = element.getChildByName("Cooltime_Label");
            if(Cooltime_Label != null) {
                curDragonUI.Cooltime_Label = Cooltime_Label.getComponent(Label);
            }
            curDragonUI.Stroke = element.getChildByName("Stroke");
            let HpBar = element.getChildByName("character_hp_green");
            if(HpBar != null) {
                let progress = HpBar.getChildByName("bar");
                if(progress != null) {
                    curDragonUI.HpBar = progress.getComponent(ProgressBar);
                }
            }

            let skull = element.getChildByName("Dead_indicator")
            if (null !== skull) {
                skull.active = false
                curDragonUI.Skull = skull
            }

            BattleUI.instance.arrDragonStatUI.push(curDragonUI);
            if(BattleUI.instance.arrCurCooltime[index] <= 0 && curDragonUI != null)
            {
                curDragonUI.Cooltime_handle.node.active = false;
                curDragonUI.Cooltime_Label.node.active = false;
                curDragonUI.Stroke.active = true;          
            }
        })
        this.BattleTime(stageData.Time);
    }

    static RegistSkiil(index : number, cooltime : number, startcooltime : number = 0)
    {
        console.log("index", index)
        console.log("BattleUI.instance.arrNodeCooltime[index]", BattleUI.instance.arrNodeCooltime[index])
        BattleUI.instance.arrCooltime[index] = cooltime
        BattleUI.instance.arrCurCooltime[index] = startcooltime
        BattleUI.instance.arrNodeCooltime[index].getChildByName("Stroke").active = false;
    }

    static SetWaveLabel(str : string)
    {
        if(BattleUI.instance != null && BattleUI.instance.labelWave != null)
        {
            BattleUI.instance.labelWave.string = str
        }
    }

    static BattleTime(str)
    {
        if(BattleUI.instance != null && BattleUI.instance.labelTimer != null)
        {
            BattleUI.instance.labelTimer.string = TimeStringMinute(str)
        }
    }

    private CreateSkillObj(stageData)
    {
        console.log(stageData.Dragons)

        for(let i = 0; i < stageData.Dragons.length; i++) 
        {
            let newNode : Node = instantiate(BattleUI.instance.prefSkillObj)
            newNode.getComponent(SkillTimerSlotFrame).SetData(stageData.Dragons[i].dragonTag as number);
            newNode.parent = this.nodeSkillObjParent
            this.arrNodeCooltime[i] = newNode
        }
    }

    onClickPause()
    {
        this.stageObj.Pause = true

        this.nodePausePopup.active = true
        this.nodePausePopup.getChildByName("body").active = true
        this.nodePausePopup.getChildByName("body-001").active = false

        let labelContinueNode = GetChild(this.nodePausePopup , ['body','product','btnOk','Label']);
        let labelExitNode = GetChild(this.nodePausePopup , ['body','product','btnOk-001','Label']);
        if(this.autoAdventureActive == true)//자동 전투 중이면
        {
            if(labelContinueNode != null){
                let labelComp = labelContinueNode.getComponent(Label);
                labelComp.string = StringTable.GetString(100000776, "자동전투\n 계속하기");
            }

            if(labelExitNode != null){
                let labelComp = labelExitNode.getComponent(Label);
                labelComp.string = StringTable.GetString(100000777, "자동전투\n 그만하기");
            }
        }else{
            if(labelContinueNode != null){
                let labelComp = labelContinueNode.getComponent(Label);
                labelComp.string = StringTable.GetString(100000778, "계속하기");
            }

            if(labelExitNode != null){
                let labelComp = labelExitNode.getComponent(Label);
                labelComp.string = StringTable.GetString(100000779, "그만하기");
            }
        }
    }

    isAutoAdventureCheck()
    {
        let dataCheck = DataManager.GetData("AutoAdventureCount");
        if(dataCheck == null || dataCheck == undefined){
            return false;
        }else{
            return true;
        }
    }

    SetAutoAdventureAnim()
    {
        let isAuto = this.isAutoAdventureCheck();
        if(this.autoIconAnimNode == null)
        {
            this.autoAdventureActive = false;
            return;
        }
        if(isAuto)
        {
            this.ShowAutoAdventureAnim();
            let animComp = this.autoIconAnimNode.getComponent(Animation);
            if(animComp != null){
                animComp.play();
                this.autoAdventureActive = true;
                this.SetCurrentAutoCount();
            }
        }
        else
        {
            this.HideAutoAdventureAnim();
            this.autoAdventureActive = false;
        }
    }

    SetCurrentAutoCount()
    {
        let totalCount : any = DataManager.GetData("AutoAdventureTotalCount");
        let currentCount : any = DataManager.GetData("AutoAdventureCount");

        if(this.autoCurrentCount == null){
            let currentCountNode = GetChild(this.autoIconAnimNode,['adventureCount', 'roundCount']);
            if(currentCountNode != null){
                this.autoCurrentCount = currentCountNode.getComponent(Label);
                this.autoCurrentCount.string = (totalCount - currentCount).toString();
            }
        }

        if(this.autoTotalCount == null){
            let totalCountNode = GetChild(this.autoIconAnimNode,['adventureCount', 'totalCount']);
            if(totalCountNode != null){
                this.autoTotalCount = totalCountNode.getComponent(Label);
                this.autoTotalCount.string = totalCount.toString();
            }
        }
    }

    HideAutoAdventureAnim()
    {
        if(this.autoIconAnimNode == null){
            return;
        }

        if(this.autoIconAnimNode.activeInHierarchy){
            this.autoIconAnimNode.active = false;
        }
    }

    ShowAutoAdventureAnim()
    {
        if(this.autoIconAnimNode == null){
            return;
        }

        if(this.autoIconAnimNode.activeInHierarchy == false){
            this.autoIconAnimNode.active = true;
        }
    }

    onClickExit()
    {
        if(this.autoAdventureActive == true)//자동 전투 중이면
        {
            this.nodePausePopup.getChildByName("body").active = false;
            let popup = PopupManager.OpenPopup("SystemPopupOKCANCEL") as SystemPopup;
            popup.setMessage(StringTable.GetString(100000248, "알림"),StringTable.GetString(100000780, "자동 전투를 해제할까요?"));
            popup.setCallback(()=>{
                
                if(DataManager.GetData("AutoAdventureCount") != null){
                    DataManager.DelData("AutoAdventureCount");
                }
                if(DataManager.GetData("AutoAdventureCancle") != null){
                    DataManager.DelData("AutoAdventureCancle");
                }
                DataManager.AddData("AutoAdventureCancle", true);
                DataManager.AddData("AutoAdventureCount" , 0);
                this.autoAdventureActive = false;
                this.HideAutoAdventureAnim();
                popup.ClosePopup();
                this.onClickResume();//아니오시 재개
            },
            ()=>{
                popup.ClosePopup();
                this.onClickResume();//아니오시 재개
            });
            return;
        }
        this.nodePausePopup.getChildByName("body").active = false
        this.nodePausePopup.getChildByName("body-001").active = true
    }

    onClickExitConfirm()
    {
        let stageData:StageData = DataManager.GetData('BattleStageData');
        if(stageData != null) {
            stageData.SetData({});
            NetworkManager.Send('adventure/abort', {}, (jsonData) => {//탈주 씬 넣기
                let rewards :any= this.stageData.Rewards;
                rewards.star = 0;
                DataManager.AddData("StageResultData", rewards);
                SoundMixer.DestroyAllClip();
                SceneManager.SceneChange("battleReward");
            });
        }
    }

    onClickResume()
    {
        this.stageObj.Pause = false
        this.nodePausePopup.active = false
    }

    onStageEnd()
    {

    }

    onDestroy()
    {
        EventManager.RemoveEvent(this);
        if(BattleUI.instance == this)
            BattleUI.instance = null;
    }

    public static OnFlash() {
        if(BattleUI.instance != null && BattleUI.instance.flashAnimation != null) {
            BattleUI.instance.flashAnimation.play('flash');
        }
    }

    public static SetVisibleCharacterSkillTimer(isVisible : boolean)
    {
        if(BattleUI.instance.arrNodeCooltime == null || BattleUI.instance.arrNodeCooltime.length <=0){
            return;
        }

        let childCount = BattleUI.instance.arrNodeCooltime.length;

        for(let i = 0; i < childCount; i++) 
        {
            if(BattleUI.instance.arrNodeCooltime[i] == null){
                continue;
            }
            BattleUI.instance.arrNodeCooltime[i].active = isVisible;
        }
    }

    public static SetVisibleCharacterHpBar(isVisible : boolean)
    {
        let stageData = BattleUI.instance.stageObj.StageData;
        if(stageData == null){
            return;
        }

        let dragonArr = stageData.Dragons;
        if(dragonArr == null){
            return;
        }

        dragonArr.forEach((element)=>{
            let dragonUINode = element.Node;
            if(dragonUINode == null){
                return;
            }
            let hpNode = dragonUINode.node.getChildByName('character_hp_green');
            if(hpNode != null)
            {
                hpNode.active = isVisible;
            }
        });
    }

    public static SetVisibleMonsterHpBar(isVisible : boolean)
    {
        let stageData = BattleUI.instance.stageObj.StageData;
        if(stageData == null){
            return;
        }

        let monsterArr = stageData.Monsters;
        if(monsterArr == null){
            return;
        }

        monsterArr.forEach((element)=>{
            let monsterUINode = element.Node;
            if(monsterUINode == null){
                return;
            }
            let hpNode = monsterUINode.node.getChildByName('character_hp_red');
            if(hpNode != null)
            {
                hpNode.active = isVisible;
            }
        });
    }

    public static SetVisibleTeamParts(isVisible : boolean)
    {
        let teamPart = GetChild(BattleUI.instance.node,['UI','Bottom','slot']);
        if(teamPart != null){
            teamPart.active = isVisible;
        }
    }

    public static SetVisibleAutoUI(isVisible : boolean)
    {
        let autoUI = GetChild(BattleUI.instance.node,['UI','Bottom','Auto']);
        if(autoUI != null){
            autoUI.active = isVisible;
        }
    }

    public static SetVisibleTimeUI(isVisible : boolean)
    {
        let TimeUI = GetChild(BattleUI.instance.node,['UI','Top','Time']);
        if(TimeUI != null){
            TimeUI.active = isVisible;
        }
    }

    public static SetVisibleTopUI(isVisible : boolean)
    {
        let TopUI = GetChild(BattleUI.instance.node,['UI','Top']);
        if(TopUI != null){
            TopUI.active = isVisible;
        }
    }
    
    public static isWaveStart() : boolean
    {
        let stageData = BattleUI.instance.stageObj.StageData;
        if(stageData == null){
            return false;
        }
        return stageData.IsWaveStart();
    }

    GetID(): string {
        return 'SkillEvent';
    }

    OnEvent(eventType: SkillEvent): void {
        if(eventType.Data['state'] == undefined) {
            return;
        }

        switch(eventType.Data['state']) {
            case 'Regist': {
                let dragonTag = eventType.Data['tag'];
                if(dragonTag < 0 || dragonTag == undefined){
                    return;
                }
                
                let skillName = eventType.Data['skillname'];
                //console.log('regist skill : ' + skillName + ' dragonTag : ' + dragonTag);
                this.SetbuffIcon(dragonTag , skillName , true);
            } break;
            case 'Delete': {
                let dragonTag = eventType.Data['tag'];
                if(dragonTag < 0 || dragonTag == undefined){
                    return;
                }
                let skillName = eventType.Data['skillname'];
                //console.log('delete skill : ' + skillName + ' dragonTag : ' + dragonTag);

                this.SetbuffIcon(dragonTag , skillName , false);
            } break;
            case 'Refresh': {
                

            } break;
        }
    }

    SetbuffIcon(dragonTag : number, skillName : string, isRegist : boolean)
    {
        if(this.arrNodeCooltime == null || this.arrNodeCooltime.length <= 0){
            return;
        }

        this.arrNodeCooltime.forEach(element=>{
            if(element == null){
                return;
            }
            let slot = element.getComponent(SkillTimerSlotFrame);
            if(slot == null){
                return;
            }
            let slotTag = slot.DragonTag;
            if(slotTag == dragonTag){
                if(isRegist){
                    slot.addBuff(skillName);
                }else{
                    slot.removeBuff(skillName);
                }
            }
        })
    }

    ClearBuffIcon(index : number)
    {
        this.arrNodeCooltime[index].getComponent(SkillTimerSlotFrame).ClearBuff();
    }

    onClickBspeed(event)
    {
        let bSpeed : number = sys.localStorage.getItem("BSpeed") != null ? Number(sys.localStorage.getItem("BSpeed")) : 0;
        bSpeed = Math.floor((bSpeed+1)%3);
        sys.localStorage.setItem("BSpeed", (bSpeed).toString())
        
        //시간 적용
        this.stageData.StageSpeed = BattleSpeedValue[bSpeed];
        this.labelBSpeed.string = StringBuilder("x {0}", BattleSpeedValue[bSpeed]);
    }
}


import { _decorator, Component, Node, Label, ProgressBar, director, Animation, instantiate, Prefab, Canvas, Vec3, Button, EventHandler, Event, Toggle, CCBoolean, WorldNode3DToWorldNodeUI, EditBox } from 'cc';
import { CharBaseTable } from '../../Data/CharTable';
import { MonsterBaseData } from '../../Data/MonsterData';
import { MonsterBaseTable } from '../../Data/MonsterTable';
import { StageBaseData } from '../../Data/StageData';
import { StageBaseTable } from '../../Data/StageTable';
import { TableManager } from '../../Data/TableManager';
import { GameManager } from '../../GameManager';
import { NetworkManager } from '../../NetworkManager';
import { SceneManager } from '../../SceneManager';
import { SoundMixer, SOUND_TYPE } from '../../SoundMixer';
import { DataManager } from '../../Tools/DataManager';
import { StringBuilder, TimeStringMinute } from '../../Tools/SandboxTools';
import { DragonStatUI } from '../BattleUI';
import { Stage, StageData } from '../Stage';
import { WaveEvent } from '../WaveBase';
import { BattleSimulatorResourceManager } from './BattleSimulatorResourceManager';
import { BattleSimulatorStage } from './BattleSimulatorStage';
import { BattleSimulatorTrigger } from './BattleSimulatorTrigger';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = BattleSimulatorUI
 * DateTime = Wed Apr 13 2022 13:49:28 GMT+0900 (대한민국 표준시)
 * Author = wonjun_gs
 * FileBasename = BattleSimulatorUI.ts
 * FileBasenameNoExtension = BattleSimulatorUI
 * URL = db://assets/Scripts/Battle/Battle_simulator/BattleSimulatorUI.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */

@ccclass('BattleSimulatorUI')
export class BattleSimulatorUI extends Component 
{
    private static instance : BattleSimulatorUI = null
    public static get Instance(){return this.instance;}

    @property(BattleSimulatorTrigger)
    simulationStartTrigger : BattleSimulatorTrigger = null;

    @property(Node)
    mapCanvas : Node = null

    @property(Node)
    mapNode : Node = null

    @property(Node)
    nodePausePopup : Node = null

    @property(Node)
    nodeSkillObjParent : Node = null

    @property(Prefab)
    prefSkillObj : Prefab = null

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

    @property(Prefab)
    dragonScrollPrefab : Prefab = null;

    @property(Node)
    monsterScrollNode : Node = null;
    @property(Node)
    monsterButtonContentNode : Node = null;
    @property(Prefab)
    monsterScrollPrefab : Prefab = null;
    @property(Button)
    monsterButton : Button = null;

    stageObj : Stage = null

    @property(Node)
    stageObjNode : Node = null;

    @property(Node)
    NewDragonTotalBoard : Node = null;
    @property(Node)
    NewdragonScrollNode : Node[] = [];
    @property(Node)
    NewdragonButton : Node[] = [];
    @property(Node)
    NewdragonButtonContentNode : Node[] = [];

    @property(Label)
    totalBattlePointLabel : Label = null;


    private arrDragonStatUI: DragonStatUI[] = null;

    private arrCooltime : number[] = null;
    private arrCurCooltime : number[] = null;
    private stageData : StageData = null;
    private targetData : StageBaseData = null;
    private flashAnimation: Animation = null;
    private resources: BattleSimulatorResourceManager = null;

    @property(CCBoolean)
    isNextWave : boolean = false;

    isSimulatorOn : boolean = false;
    onLoad() {
        if(BattleSimulatorUI.instance == null)
        {
            BattleSimulatorUI.instance = this;
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
        for(let i = 0 ; i < this.NewdragonButton.length ; i++)
        {
            this.setCurrentDragonLabel(i);
        }
        
        this.setCurrentMonsterLabel();

        if(this.stageObj == null){
            this.stageObj =  this.stageObjNode.getComponent(Stage);
        }
    }

    onClickStart()
    {
        this.simulateStart();
    }

    simulateStart()
    {
        this.arrDragonStatUI = [];

        let res = BattleSimulatorResourceManager.Instance;
        res.Init();
        res.SetPrefabList(this.simulationStartTrigger.GetPrefabList());

        if(this.resources == null) {
            this.resources = GameManager.GetManager<BattleSimulatorResourceManager>(BattleSimulatorResourceManager.Name);
        }

        if(this.stageObj != null) {
            this.stageObj.Init();
        }

        if(this.isSimulatorOn){
            return;
        }

        this.isSimulatorOn = true;
        this.simulationStartTrigger.setData();

        let stageData : any = DataManager.GetData('stageStartData_simulator');
        if(stageData != null) {

            let map = this.mapNode.getChildByName('MAP');
            if(map != null && stageData != null) {
                WaveEvent.TriggerEvent({
                    state: 'Init',
                    mapNode: map,
                    stageData: stageData
                });
            }
        }

        DataManager.DelData('stageStartData_simulator');

        BattleSimulatorUI.UIResize();
    }

    onClickRestartSimulator()
    {
        if(!this.isSimulatorOn){
            return;
        }

        NetworkManager.GetAllData();

        WaveEvent.TriggerEvent({
            state: 'WaveEnd',
        });
        WaveEvent.TriggerEvent({
            state: 'WaveClear',
        });

        this.mapCanvas.removeAllChildren();
         let mapPrefab = this.resources.GetResourceByName("STAGE_1");
            this.mapNode = instantiate(mapPrefab);
            if(this.mapNode != null) {
                this.mapCanvas.addChild(this.mapNode);
        }

        this.nodeSkillObjParent.removeAllChildren();
        this.arrDragonStatUI = [];

        if(this.stageObj != null) {
            this.stageObj.Init();
        }
        this.simulationStartTrigger.setData();

        let stageData : any = DataManager.GetData('stageStartData_simulator');
        if(stageData != null) {

            let map = this.mapNode.getChildByName('MAP');
            if(map != null && stageData != null) {
                WaveEvent.TriggerEvent({
                    state: 'Init',
                    mapNode: map,
                    stageData: stageData
                });
            }
        }
        BattleSimulatorUI.UIResize();
        DataManager.DelData('stageStartData_simulator');
    }

    readAndSetTableData()
    {


    }

    GetMonsterWaveData()
    {
        if(this.simulationStartTrigger != null)
        {
            let data = {};
            if(this.isNextWave)
            {
                this.simulationStartTrigger.SetNextWave();
                this.setCurrentMonsterLabel();
            }
            data['enemy'] = this.simulationStartTrigger.CreateWaveIndex();
            return data;
        }
    }

    setCurrentDragonLabel(index : number)
    {
        let currentDragonTag = 0;
        let currentdragonList= this.simulationStartTrigger.GetCurrentDragonList();
        let data :any = null;
        switch(index)
        {
            case 0 :
            {
                if(currentdragonList.length <= 0 || currentdragonList[0].length <= 0)
                {
                    currentDragonTag = -1;
                    break;
                }
                data = currentdragonList[0][0];
                currentDragonTag = data.dtag;
            }break;
            case 1 :
            {
                if(currentdragonList.length <= 0 || currentdragonList[0].length <= 1)
                {
                    currentDragonTag = -1;
                    break;
                }
                data = currentdragonList[0][1];
                currentDragonTag = data.dtag;
            }break;
            case 2 :
            {
                if(currentdragonList.length <= 1 || currentdragonList[1].length <= 0)
                {
                    currentDragonTag = -1;
                    break;
                }
                data = currentdragonList[1][0];
                currentDragonTag = data.dtag;
            }break;
            case 3 :
            {
                if(currentdragonList.length <= 1 || currentdragonList[1].length <= 1)
                {
                    currentDragonTag = -1;
                    break;
                }
                data = currentdragonList[1][1];
                currentDragonTag = data.dtag;
            }break;
            case 4 :
            {
                if(currentdragonList.length <= 2 || currentdragonList[2].length <= 0)
                {
                    currentDragonTag = -1;
                    break;
                }
                data = currentdragonList[2][0];
                currentDragonTag = data.dtag;
            }break;
        }

        let chartable = TableManager.GetTable<CharBaseTable>(CharBaseTable.Name);

        if(currentDragonTag <= 0 || currentDragonTag == null)
        {
            this.NewdragonButton[index].getComponentInChildren(Label).string = "드래곤 선택없음";
            return;
        }

        let dragoninfo = chartable.Get(currentDragonTag.toString());
        if(dragoninfo == null)
        {
            this.NewdragonButton[index].getComponentInChildren(Label).string = "드래곤 새로고침";
        }else{
            this.NewdragonButton[index].getComponentInChildren(Label).string = currentDragonTag+ " " + dragoninfo.IMAGE;
        }
    }

    setCurrentMonsterLabel()
    {
        // let monsterTag = 0;
        // let currentMonsterList= this.simulationStartTrigger.CreateEnemyData();
        // let keys = Object.keys(currentMonsterList);
        // keys.forEach((element)=>{

        //     let data = currentMonsterList[element][0];
        //     monsterTag = data.mid;
        // });

        // let chartable = TableManager.GetTable<MonsterBaseTable>(MonsterBaseTable.Name);
        // let monsterinfo = chartable.Get(monsterTag.toString());
        
        // if(monsterinfo == null)
        // {
        //     this.monsterButton.getComponentInChildren(Label).string = "몬스터 새로고침";
        // }else{
        //     this.monsterButton.getComponentInChildren(Label).string = monsterTag+ " " + monsterinfo.IMAGE;
        // }

        let waveinfo = this.simulationStartTrigger.SelectWave;

        if(waveinfo == null || waveinfo == ""){
            this.monsterButton.getComponentInChildren(Label).string = "웨이브 새로고침";
        }else{
            this.monsterButton.getComponentInChildren(Label).string = waveinfo;
        }
    }

    onClickVisibleDragonScrollview(event : Event = null, CustomEventData)
    {
        let checkData = JSON.parse(CustomEventData);
        let index = Number(checkData.index);

        this.NewdragonScrollNode[index].active = !this.NewdragonScrollNode[index].active;

            if(this.NewdragonScrollNode[index].activeInHierarchy)
            {
                this.drawScrollNode(index);
            }else{
    
            }
    
            this.setCurrentDragonLabel(index);
    }

    onClickVisibleMonsterScrollview()
    {
        this.monsterScrollNode.active = !this.monsterScrollNode.active;

        if(this.monsterScrollNode.activeInHierarchy)
        {
            this.drawMonsterScrollNode();
        }else{

        }

        this.setCurrentMonsterLabel();
    }
    
    drawScrollNode(index : number)
    {
        this.NewdragonButtonContentNode[index].removeAllChildren();

        let chartable = TableManager.GetTable<CharBaseTable>(CharBaseTable.Name);
        if(chartable != null)
        {
            let charList = chartable.GetAll();
            if(charList == null || charList.length <= 0){
                return;
            }

            let clone = instantiate(this.dragonScrollPrefab);
            clone.getComponentInChildren(Label).string = "드래곤 빼기";
            clone.parent = this.NewdragonButtonContentNode[index];

            let cloneButton = clone.getComponent(Button);
            let newEvent = new EventHandler();
            newEvent.target = this.node;
            newEvent.component = "BattleSimulatorUI"
            newEvent.handler = "onClickScrollViewButton";
            newEvent.customEventData = `${index}_-1`;
            cloneButton.clickEvents.push(newEvent);

            for(let i = 0; i < charList.length ; i++)
            {
                let data =  charList[i];
                let tag = data.Index;
                let name = data.IMAGE;

                let clone = instantiate(this.dragonScrollPrefab);
                clone.getComponentInChildren(Label).string = tag.toString() + " "+ name;
                clone.parent = this.NewdragonButtonContentNode[index];

                let cloneButton = clone.getComponent(Button);
                let newEvent = new EventHandler();
                newEvent.target = this.node;
                newEvent.component = "BattleSimulatorUI"
                newEvent.handler = "onClickScrollViewButton";
                newEvent.customEventData = `${index}_${tag.toString()}`;
                cloneButton.clickEvents.push(newEvent);
            }
        }
    }

    drawMonsterScrollNode()
    {
        this.monsterButtonContentNode.removeAllChildren();
        let waveData = this.simulationStartTrigger.MonsterWave;

        if(waveData == null){
            return;
        }
        let waveInfoList = waveData.getMonsterWaveList();
        if(waveInfoList == null || waveInfoList.length <= 0){
            return;
        }

        for(let i = 0 ; i< waveInfoList.length ; i++)
        {
            let clone = instantiate(this.monsterScrollPrefab);
            clone.getComponentInChildren(Label).string =waveInfoList[i].worldWaveName;
            clone.parent = this.monsterButtonContentNode;

            let cloneButton = clone.getComponent(Button);
            let newEvent = new EventHandler();
            newEvent.target = this.node;
            newEvent.component = "BattleSimulatorUI"
            newEvent.handler = "onClickMonsterScrollViewButton";
            newEvent.customEventData = waveInfoList[i].worldWaveName;
            cloneButton.clickEvents.push(newEvent);
        }

        // let chartable = TableManager.GetTable<MonsterBaseTable>(MonsterBaseTable.Name);
        // if(chartable != null)
        // {
        //     let charList = chartable.GetAll();
        //     if(charList == null || charList.length <= 0){
        //         return;
        //     }
        //     for(let i = 0; i < charList.length ; i++)
        //     {
        //         let data =  charList[i];
        //         let tag = data.Index;
        //         let name = data.IMAGE;

        //         let clone = instantiate(this.monsterScrollPrefab);
        //         clone.getComponentInChildren(Label).string = tag.toString() + " "+ name;
        //         clone.parent = this.monsterButtonContentNode;

        //         let cloneButton = clone.getComponent(Button);
        //         let newEvent = new EventHandler();
        //         newEvent.target = this.node;
        //         newEvent.component = "BattleSimulatorUI"
        //         newEvent.handler = "onClickMonsterScrollViewButton";
        //         newEvent.customEventData = tag.toString();
        //         cloneButton.clickEvents.push(newEvent);
        //     }
        // }
    }


    onClickScrollViewButton(event : Event, customeventData)
    {
        console.log(event);
        console.log(customeventData);
        let str = customeventData as string;
        let stringList = str.split('_');

        let index = Number(stringList[0]);
        let tag = Number(stringList[1]);
        //드래곤 prefab있는지 선체크
        let chartable = TableManager.GetTable<CharBaseTable>(CharBaseTable.Name);

        if(tag <= 0)
        {
            this.simulationStartTrigger.SetPlayerFirstData(0, Number(index));
            this.onClickVisibleDragonScrollview(null, JSON.stringify({"index" : index}));
            return;
        }

        let dragoninfo = chartable.Get(stringList[1].toString());
        if(dragoninfo == null || dragoninfo.IMAGE == 'NONE' || dragoninfo.IMAGE == ""){
            
            this.onClickVisibleDragonScrollview(null, JSON.stringify({"index" : index}));
            return;
        }
        
        this.simulationStartTrigger.SetPlayerFirstData(Number(tag), Number(index));
        this.onClickVisibleDragonScrollview(null, JSON.stringify({"index" : index}));
        this.RefreshBattlePoint();
    }

    onClickMonsterScrollViewButton(event : Event, customeventData)
    {
        console.log(event);
        console.log(customeventData);

        // let chartable = TableManager.GetTable<MonsterBaseTable>(MonsterBaseTable.Name);
        // let monsterinfo = chartable.Get(customeventData.toString());

        // if(monsterinfo == null || monsterinfo.IMAGE == 'NONE' || monsterinfo.IMAGE == ""){
        //     this.onClickVisibleMonsterScrollview();
        //     return;
        //}
        
        this.simulationStartTrigger.SetCurrentWaveStage(customeventData);
        this.onClickVisibleMonsterScrollview();
    }

    static UIUpdate()
    {
        BattleSimulatorUI.instance.arrDragonStatUI.forEach((element, index)=>
        {
            if(element == null) {
                return;
            }
            let curDragon = BattleSimulatorUI.instance.stageData.Dragons[index];
            if(curDragon == null || curDragon.Death) {
                BattleSimulatorUI.instance.DeactiveSkillCoolTime(index)
                return;
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
                    element.Cooltime_handle.progress = curDragon.DelaySkill1 / BattleSimulatorUI.instance.arrCooltime[index]
                }
            }
            else
            {
                element.Cooltime_handle.node.active = false;
                element.Cooltime_Label.node.active = false;
                element.Stroke.active = true;
            }            
        });
        
        BattleSimulatorUI.UIResize();
    }

    static UIResize() {
        if(BattleSimulatorUI.instance != null && BattleSimulatorUI.instance.mapCanvas != null) {
            let canvas = BattleSimulatorUI.instance.mapCanvas.getComponent(Canvas);
            if(canvas != null && BattleSimulatorUI.instance.mapNode != null) {
                canvas.cameraComponent.node.worldPosition = new Vec3(BattleSimulatorUI.instance.mapNode.worldPosition.x - 286, BattleSimulatorUI.instance.mapNode.worldPosition.y + 50, 10) ;
            }
        }
    }

    private DeactiveSkillCoolTime(index : number)
    {
        BattleSimulatorUI.instance.arrDragonStatUI[index].Cooltime_Label.string = ""
        BattleSimulatorUI.instance.arrDragonStatUI[index].HpBar.progress = 0
        BattleSimulatorUI.instance.arrDragonStatUI[index].Cooltime_handle.progress = 1
        BattleSimulatorUI.instance.arrDragonStatUI[index].Stroke.active = false
    }

    static init(stageData : StageData, worldData: any)
    {
        BattleSimulatorUI.instance.stageData = stageData;

        BattleSimulatorUI.instance.labelStageName.string = "테스트 스테이지"
        BattleSimulatorUI.instance.labelStageNumber.string = StringBuilder("{0}-{1}", worldData.world, worldData.stage)
        BattleSimulatorUI.instance.labelWave.string = StringBuilder("{0}/{1}", stageData.CurWave, stageData.MaxWave)        
        BattleSimulatorUI.instance.CreateSkillObj(stageData)
        
        stageData.Dragons.forEach((element, index)=>
        {
            BattleSimulatorUI.RegistSkiil(index, element.Skill1.COOL_TIME, element.Skill1.START_COOL_TIME)
        })

        BattleSimulatorUI.instance.arrNodeCooltime.forEach((element, index)=>
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

            BattleSimulatorUI.instance.arrDragonStatUI.push(curDragonUI);
            if(BattleSimulatorUI.instance.arrCurCooltime[index] <= 0 && curDragonUI != null)
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
        console.log("BattleUI.instance.arrNodeCooltime[index]", BattleSimulatorUI.instance.arrNodeCooltime[index])
        BattleSimulatorUI.instance.arrCooltime[index] = cooltime
        BattleSimulatorUI.instance.arrCurCooltime[index] = startcooltime
        BattleSimulatorUI.instance.arrNodeCooltime[index].getChildByName("Stroke").active = false;
    }

    static SetWaveLabel(str : string)
    {
        if(BattleSimulatorUI.instance != null && BattleSimulatorUI.instance.labelWave != null){
            BattleSimulatorUI.instance.labelWave.string = str
        }
    }

    static BattleTime(str)
    {
        if(BattleSimulatorUI.instance != null && BattleSimulatorUI.instance.labelTimer != null){
            BattleSimulatorUI.instance.labelTimer.string = TimeStringMinute(str)
        }
    }

    private CreateSkillObj(stageData)
    {
        console.log(stageData.Dragons)

        for(let i = 0; i < stageData.Dragons.length; i++) 
        {
            let newNode : Node = instantiate(BattleSimulatorUI.instance.prefSkillObj)
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
    }

    onClickExit()
    {
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
        if(BattleSimulatorUI.instance == this)
        BattleSimulatorUI.instance = null;
    }

    public static OnFlash() {
        if(BattleSimulatorUI.instance != null && BattleSimulatorUI.instance.flashAnimation != null) {
            BattleSimulatorUI.instance.flashAnimation.play('flash');
        }
    }


    onClickToggleInfiniteTime(event : Event)
    {
        let currentNode = event.currentTarget as Node;
        if(currentNode == null){
            return;
        }

        let toggleComp = currentNode.getComponent(Toggle);
        if(toggleComp == null){
            return;
        }
        let isChecked = toggleComp.isChecked;

        if(!isChecked)
        {
            this.simulationStartTrigger.setTime(99999999);
        }
        else
        {
            this.simulationStartTrigger.setTime(0);
        }
    }
    onClickToggleInfinitePlayerHP(event : Event, customEventData)
    {
        let checkData = JSON.parse(customEventData);
        let index = Number(checkData.index);

        let currentNode = event.currentTarget as Node;
        if(currentNode == null){
            return;
        }

        let toggleComp = currentNode.getComponent(Toggle);
        if(toggleComp == null){
            return;
        }
        let isChecked = toggleComp.isChecked;

        if(!isChecked)
        {
            this.simulationStartTrigger.setDragonHP(99999999);
        }
        else
        {
            this.simulationStartTrigger.setDragonHP(0);
        }
        
    }
    onClickToggleInfiniteEnemyHP(event : Event)
    {
        let currentNode = event.currentTarget as Node;
        if(currentNode == null){
            return;
        }

        let toggleComp = currentNode.getComponent(Toggle);
        if(toggleComp == null){
            return;
        }
        let isChecked = toggleComp.isChecked;

        if(!isChecked)
        {
            this.simulationStartTrigger.setEnemyHP(99999999);
        }
        else
        {
            this.simulationStartTrigger.setEnemyHP(0);
        }
    }

    setSimulatorDragonLevel(param: EditBox , customEventData)
    {
        this.simulationStartTrigger.setSimulatorDragonLevel(param,customEventData);
        this.RefreshBattlePoint();
    }

    onClickToggleMaxDragonLevel(event : Event , customEventData)
    {
        let checkData = JSON.parse(customEventData);
        let index = Number(checkData.index);

        let currentNode = event.currentTarget as Node;
        if(currentNode == null){
            return;
        }

        let toggleComp = currentNode.getComponent(Toggle);
        if(toggleComp == null){
            return;
        }
        let isChecked = toggleComp.isChecked;

        if(!isChecked)
        {
            this.simulationStartTrigger.setDragonLevel(60, index);
        }
        else
        {
            this.simulationStartTrigger.setDragonLevel(1, index);
        }
        this.RefreshBattlePoint();
    }

    setSimulatorDragonsLevel(param: EditBox, customEventData)
    {
        this.simulationStartTrigger.setSimulatorDragonsLevel(param,customEventData);
        this.RefreshBattlePoint();
    }

    onClickToggleMaxDragonsLevel(event : Event, customEventData)
    {
        let checkData = JSON.parse(customEventData);
        let index = Number(checkData.index);

        let currentNode = event.currentTarget as Node;
        if(currentNode == null){
            return;
        }

        let toggleComp = currentNode.getComponent(Toggle);
        if(toggleComp == null){
            return;
        }
        let isChecked = toggleComp.isChecked;

        if(!isChecked)
        {
            this.simulationStartTrigger.setDragonsLevel(60, index);
        }
        else
        {
            this.simulationStartTrigger.setDragonsLevel(1, index);
        }
        this.RefreshBattlePoint();
    }

    onClickTotalDragonBoard()
    {
        this.NewDragonTotalBoard.active = !this.NewDragonTotalBoard.active;
    }

    RefreshBattlePoint()
    {
        if(this.totalBattlePointLabel != null){
            this.totalBattlePointLabel.string = this.simulationStartTrigger.TotalINF.toString();
        }
    }

    onClickPlaySpeedToggleEvent(toggleEvent : Event,customeventData)
    {
        let checkPlaySpeed = JSON.parse(customeventData);
        let speed= Number(checkPlaySpeed.index);

        if(this.simulationStartTrigger != null)
        {
            this.simulationStartTrigger.BattleSpeed = speed;
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

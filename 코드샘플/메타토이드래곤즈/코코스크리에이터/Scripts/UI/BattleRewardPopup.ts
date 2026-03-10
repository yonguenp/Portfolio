
import { _decorator, Component, Node, Label, instantiate, Layers, Vec3, math, director, sys, ScrollView, UITransform, Layout, Animation, CCInteger, Sprite, Color, AnimationState} from 'cc';
import { BattleDragonSpine } from '../Character/BattleDragonSpine';
import { CharBaseTable , CharExpTable} from '../Data/CharTable';
import { StageBaseTable } from '../Data/StageTable';
import { StringTable } from '../Data/StringTable';
import { TableManager } from '../Data/TableManager';
import { GameManager } from '../GameManager';
import { NetworkManager } from '../NetworkManager';
import { ResourceManager, ResourcesType } from '../ResourceManager';
import { SceneManager } from '../SceneManager';
import { SoundMixer, SOUND_TYPE } from '../SoundMixer';
import { DataManager } from '../Tools/DataManager';
import { ChangeLayer, StringBuilder, TimeStringMinute } from '../Tools/SandboxTools';
import { TutorialManager } from '../Tutorial/TutorialManager';
import { User } from '../User/User';
import { AdventureDamageStatisticPopup } from './AdventureDamageStatisticPopup';
import { PopupManager } from './Common/PopupManager';
import { ItemFrame } from './ItemSlot/ItemFrame';
import { BattleLine } from './StagePreparePopup';
import { SystemPopup } from './SystemPopup';
import { SystemRewardPopup } from './SystemRewardPopup';
const { ccclass, property } = _decorator;
 
const starMaxCount = 3;

@ccclass('BattleRewardPopup')
export class BattleRewardPopup extends Component 
{
    @property(Label)
    labelBattleTime : Label = null

    @property(Label)
    labelReqEnergy : Label = null

    @property(ScrollView)
    rewardScroll : ScrollView = null

    @property(Node)
    nodeRewardContent : Node = null

    @property(Node)
    nodeWin : Node = null

    @property(Node)
    nodeLoose : Node = null

    @property(Node)
    nodeReward : Node = null

    @property(Node)
    nodeBtnNext : Node = null

    @property(Node)
    arrStarNode : Node[] = []

    @property(Node)
    arrDragonNode : Node[] = []

    @property(Node)
    arrLvBG : Node[] = []

    @property(Node)
    autoButtonNode : Node = null;

    @property(Label)
    autoButtonLabel : Label = null;

    @property(CCInteger)
    defaultAutoCount : number = 3;

    @property(Animation)
    animationBg: Animation = null;

    @property(Animation)
    winnerClearAnim: Animation = null;

    @property(Sprite)
    winnerUnderBG : Sprite = null;
    @property(Color)
    failureLvColor : Color = null;
    @property(Color)
    winnerLvColor : Color = null;

    private curWorld : number = 1
    private curDiff : number = 1
    private curStage : number = 1
    charExpTable : CharExpTable = null;
    stageBaseTable : StageBaseTable = null;
    currentAutoCount : number = 0;
    autoAdventureReward : any = {};
    battleDamageStatistic : any = {};
    dragonList : any = {};
    time : any = {};
    start()
    {
        let stageData : any = DataManager.GetData("StageResultData");

        let charData = stageData['charData'];
        if(charData != null) {
            const charKeys = Object.keys(charData);
            const charsCount = charKeys.length;

            for(var i = 0 ; i < charsCount ; i++) {
                const bTag = Number(charKeys[i]);
                if(charData[bTag] != undefined) {
                    let data = TableManager.GetTable<CharBaseTable>(CharBaseTable.Name).Get(charData[bTag]['dtag']);
                    if(data != null) {
                        charData[bTag]['prefab'] = User.Instance.DragonData.GetNameDragonSpine(data.IMAGE);
                    }
                }
            }

            //데이터 없으면 뒤에서부터 꺼버림
            for(let j = charsCount ; j < this.arrDragonNode.length ; j++)
            {
                let characterNode = this.arrDragonNode[j];
                if(characterNode == null){
                    continue;
                }
                characterNode.active = false;
            }
        }

        let levelUpListData = stageData['levelup'];
        let levelupList : number[] = [];
        if(levelUpListData != null){
            let dTagKey = Object.keys(levelUpListData);
            dTagKey.forEach((element)=>{
                levelupList.push(Number(element));
            })
        }

        let stars = stageData?.star != null ? stageData.star : 0;
        let isCompleteBattle = stageData?.deserveReward != undefined ? stageData.deserveReward : false;
        DataManager.DelData("StageResultData");

        this.curWorld = stageData.world;
        this.curDiff = stageData.diff;
        this.curStage = stageData.stage;
        
        if(this.stageBaseTable == null){
            this.stageBaseTable = TableManager.GetTable<StageBaseTable>(StageBaseTable.Name);
        }

        if(this.charExpTable == null){
            this.charExpTable = TableManager.GetTable<CharExpTable>(CharExpTable.Name);
        }

        let curStageData = this.stageBaseTable.GetByWorldStage(this.curWorld, this.curStage);
        let charExp = 0;
        if(stageData != null)
        {
            charExp= curStageData.CHAR_EXP as number;
        }
        
        const dragonsKeys = Object.keys(charData);
        const dragonsCount = dragonsKeys.length;
        for(var i = 0 ; i < dragonsCount ; i++) {
            const bTag = Number(dragonsKeys[i]);
            if(charData[bTag] == null) {
                continue;
            }
            var index = dragonsCount - bTag;
            let dragonClone = instantiate(charData[bTag]['prefab']);
            const layer = 1 << Layers.nameToLayer('UI_2D');
            ChangeLayer(dragonClone, layer);
            dragonClone.parent = this.arrDragonNode[index];
            dragonClone.scale = new Vec3(-1, 1);
            dragonClone.position = new Vec3(0, -54);

            let spine = dragonClone.addComponent(BattleDragonSpine);
            if(spine != null) {
                spine.Init();
                spine.shadow.active = false;
                charData[bTag]['spine'] = spine;
            }

            let dragonInfo = User.Instance.DragonData.GetDragon(charData[bTag]['dtag']);
            let dragonLevel = dragonInfo.Level;
            let isMaxLevel = (this.charExpTable.GetDragonMaxLevel() == dragonLevel);
            if(isCompleteBattle)
            {
                if(dragonInfo != null)
                {
                    if(isMaxLevel){
                        this.arrDragonNode[index].getChildByName("EXP_Label").getComponent(Label).string = "LEVEL MAX"
                    }else{
                        this.arrDragonNode[index].getChildByName("EXP_Label").getComponent(Label).string = StringBuilder("EXP + {0}", charExp)
                    }
                    this.arrDragonNode[index].getChildByName("Levelup_Label").active = false;//Math.round(math.randomRange(0, 1)) == 1 ? true : false
                    this.arrDragonNode[index].getChildByName("Level_info").getComponentInChildren(Label).string = StringBuilder("Lv.{0}", dragonLevel); 
                    
                    this.PlaydragonLevelUpAnim(index,Number(dragonInfo.Tag), levelupList);
                }
            }else{
                this.arrDragonNode[index].getChildByName("EXP_Label").getComponent(Label).string = "";
                this.arrDragonNode[index].getChildByName("Levelup_Label").active = false;
                this.arrDragonNode[index].getChildByName("Level_info").getComponentInChildren(Label).string = StringBuilder("Lv.{0}", dragonLevel);
            }
        }

        //드래곤마다 경험치 획득량, 스테이지 진행 시간
        this.labelBattleTime.string = TimeStringMinute(stageData.time)
        this.labelReqEnergy.string = StringBuilder("-{0}", curStageData.COST_VALUE)
        
        this. autoAdventureReward = {};
        //성공 실패 여부
        if(stars > 0)
        {
            let nextStageData = TableManager.GetTable<StageBaseTable>(StageBaseTable.Name).GetByWorldStage(this.curWorld, this.curStage+1)
            if(nextStageData!=null)
            {
                this.nodeBtnNext.active = true
            }
            this.nodeWin.active = true
            this.nodeReward.active = true
            //별 몇개인지
            for(let i = 0; i < starMaxCount; i++)
            {
                this.arrStarNode[i].active = true
                let animNode = this.arrStarNode[i].parent;

                if(animNode != null)
                {
                    let animComp = animNode.getComponent(Animation);
                    if(animComp != null){
                        animComp.play();
                    }
                }

                let childSprite = this.arrStarNode[i].getComponentInChildren(Sprite);
                if(childSprite == null){
                    continue;
                }
                if(i < stars)
                {
                    childSprite.node.active = true;
                }else{
                    childSprite.node.active = false;
                }
            }

            for(var i = 0 ; i < dragonsCount ; i++) {
                const bTag = Number(dragonsKeys[i]);
                if(charData[bTag] == null) {
                    continue;
                }
                
                let element: BattleDragonSpine = charData[bTag]['spine'];
                if(element != null) {
                    if(charData[bTag]['death']) {
                        element.AnimationStart('lose');
                    } else {
                        element.AnimationStart('win');
                    }
                }
            }
            
            var count = 0;
            var itemSize = 0;
            stageData.rewards.forEach((element)=>
            {
                let newItem : Node = instantiate(GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB)['item'])
                this.rewardScroll.content.addChild(newItem);
                let transformItem = newItem.getComponent(UITransform);
                if(transformItem != null) {
                    itemSize += transformItem.width;
                }
                count++;

                switch(element.type)
                {
                    case 1:
                        newItem.getComponent(ItemFrame).setFrameCashInfo(element.no, element.count)
                    break

                    case 2:
                        newItem.getComponent(ItemFrame).setFrameEnergyInfo(element.count)
                    break
                    
                    case 3:
                        newItem.getComponent(ItemFrame).setFrameItemInfo(element.no, element.count)
                    break
                }

                if(this.isAutoAdventured())
                {
                    this.setTotalAutoAdventureReward(element.no, element.count, element.type);
                }
            });

            if(this.isAutoAdventured())
            {
                this.saveTotalAutoAdventureRewardData();
            }

            let scrollTransform = this.rewardScroll.getComponent(UITransform);
            let layout = this.rewardScroll.content.getComponent(Layout);
            if(scrollTransform != null && layout != null) {
                var scrollSize = itemSize + (count - 1) * layout.spacingX + layout.paddingLeft + layout.paddingRight;
                if(scrollSize > scrollTransform.width) {
                    scrollSize = scrollTransform.width;
                }
                this.rewardScroll.view.setContentSize(scrollSize, this.rewardScroll.view.contentSize.height);
            }
            this.rewardScroll.scrollToLeft();

            if(this.animationBg != null) {
                this.animationBg.play('rewardBGWin');
            }

            if(this.winnerUnderBG != null) {
                this.winnerUnderBG.node.active = true;
            }

            if(this.arrLvBG != null) {
                this.arrLvBG.forEach(element => {
                    if(element == null) {
                        return;
                    }
                    let spr = element.getComponent(Sprite);
                    if(spr != null) {
                        spr.color = this.winnerLvColor;
                    }
                });
            }

            if(this.winnerClearAnim != null) {
                this.winnerClearAnim.on(Animation.EventType.FINISHED, (event, type: AnimationState) => {
                    switch(type.name) {
                        case 'stageclear': {
                            this.winnerClearAnim.play('stageclear_loop');
                        } break;
                    }
                }, this);
            }

            //승리 사운드
            SoundMixer.PlayBGM(SOUND_TYPE.BGM_BATTLE_VICTORY, null, false);

            //튜토리얼 관련
            let stageTuorialData : any = DataManager.GetData("StageTutorialData");
            if (stageTuorialData.newRecord == 1) {      // 최초 클리어일 경우 체크
                if (curStageData.WORLD == 1 && curStageData.STAGE == 1) {
                    TutorialManager.GetInstance.OnTutorialEvent(107, 1);
                }
                else if (curStageData.WORLD == 1 && curStageData.STAGE == 3) {
                    TutorialManager.GetInstance.OnTutorialEvent(110, 1);
                }
                else if (curStageData.WORLD == 1 && curStageData.STAGE == 8) {
                    TutorialManager.GetInstance.OnTutorialEvent(119, 1);
                }
                else if (curStageData.WORLD == 2 && curStageData.STAGE == 5) {
                    TutorialManager.GetInstance.OnTutorialEvent(115, 1);
                }
                else if (curStageData.WORLD == 2 && curStageData.STAGE == 8) {
                    TutorialManager.GetInstance.OnTutorialEvent(120, 1);
                }
            }
            DataManager.DelData("StageTutorialData");
        }
        else
        {
            for(var i = 0 ; i < dragonsCount ; i++) {
                const bTag = Number(dragonsKeys[i]);
                if(charData[bTag] == null) {
                    continue;
                }
                
                let element: BattleDragonSpine = charData[bTag]['spine'];
                if(element != null) {
                    element.AnimationStart('lose');
                }
            }
            
            this.nodeLoose.active = true;

            if(this.winnerUnderBG != null) {
                this.winnerUnderBG.node.active = false;
            }

            if(this.animationBg != null) {
                this.animationBg.play('rewardBGLose');
            }

            if(this.arrLvBG != null) {
                this.arrLvBG.forEach(element => {
                    if(element == null) {
                        return;
                    }
                    let spr = element.getComponent(Sprite);
                    if(spr != null) {
                        spr.color = this.failureLvColor;
                    }
                });
            }

            //패배 사운드
            SoundMixer.PlayBGM(SOUND_TYPE.BGM_BATTLE_DEFEAT, null, false);

            if(this.isAutoAdventured())//자동 사냥 실패 시 잔여 횟수 0으로 만듬
            {
                this.ReleaseAutoAdventure();
            }
        }

        this.initAutoButton(stars > 0);//자동 사냥 세팅

        this.battleDamageStatistic = DataManager.GetData("AdventureDamageStatistic");
        this.dragonList = stageData['charData'];
        this.time = stageData.time;
        DataManager.DelData("AdventureDamageStatistic");
    }

    PlaydragonLevelUpAnim( arrIndex : number, dTag : number , dragonLevelUpList : number[])
    {
        if(dragonLevelUpList == null || dragonLevelUpList.length <= 0){
            return;
        }

        let containCheck = dragonLevelUpList.filter(element=> element == dTag);
        if(containCheck != null && containCheck.length > 0){

            let dragonLevelUpAnim = this.arrDragonNode[arrIndex].getChildByName("Levelup_Label").getComponent(Animation);
            dragonLevelUpAnim.node.active = true;
            dragonLevelUpAnim.play();
        }
    }

    onClickToWorld()
    {
        if(this.isAutoAdventured()){
            this.OpenAutoAdventurePopup();
            return;
        }

        //현재 스테이지 저장
        let params =
        {
            world : this.curWorld,
            stage : this.curStage,
            next : false
        }
        if(DataManager.GetData("StageEndData") != null)
        {
            DataManager.DelData("StageEndData");
        }

        DataManager.AddData("StageEndData", params)

        SoundMixer.DestroyAllClip()
        SceneManager.SceneChange("WorldAdventure");
    }

    onClickToVillage()
    {
        if(this.isAutoAdventured()){
            this.OpenAutoAdventurePopup();
            return;
        }

        SoundMixer.DestroyAllClip()
        SceneManager.SceneChange("game");
    }

    onClickRetry()
    {
        if(this.isAutoAdventured()){
            this.OpenAutoAdventurePopup();
            return;
        }

        let tempUpdater = NetworkManager.Instance.UIUpdater;
        NetworkManager.Instance.UIUpdater = null;
        let battleLine : BattleLine = JSON.parse(sys.localStorage.getItem('battleLineInfo'))
        NetworkManager.Send('adventure/start', {
            world:this.curWorld,
            diff:this.curDiff,
            stage:this.curStage,
            deck:battleLine.battleLine
        }, (jsonData) => {
            if(jsonData['err'] != undefined && jsonData['err'] == 0 && jsonData['state'] != undefined && jsonData['tag'] != undefined && jsonData['wave'] != undefined && jsonData['player'] != undefined && jsonData['enemy'] != undefined) {
                SoundMixer.DestroyAllClip();
                
                let data = {};
                data['world'] = this.curWorld;
                data['stage'] = this.curStage;
                data['jsonData'] = jsonData;
                DataManager.AddData('stageStartData', data);
                SceneManager.SceneChange("battle");
                return;
            }
            NetworkManager.Instance.UIUpdater = tempUpdater;
        });
    }

    AutoRetry()
    {
        if(this.isRemainAutoCount())//자동 사냥중 체크 조건 어떻게 할지
        {
            this.minusAutoCount();
        }

        let tempUpdater = NetworkManager.Instance.UIUpdater;
        NetworkManager.Instance.UIUpdater = null;
        let battleLine : BattleLine = JSON.parse(sys.localStorage.getItem('battleLineInfo'))
        NetworkManager.Send('adventure/start', {
            world:this.curWorld,
            diff:this.curDiff,
            stage:this.curStage,
            deck:battleLine.battleLine
        }, (jsonData) => {
            if(jsonData['err'] != undefined && jsonData['err'] == 0 && jsonData['state'] != undefined && jsonData['tag'] != undefined && jsonData['wave'] != undefined && jsonData['player'] != undefined && jsonData['enemy'] != undefined) {
                SoundMixer.DestroyAllClip();
                
                let data = {};
                data['world'] = this.curWorld;
                data['stage'] = this.curStage;
                data['jsonData'] = jsonData;
                DataManager.AddData('stageStartData', data);
                SceneManager.SceneChange("battle");
                return;
            }
            NetworkManager.Instance.UIUpdater = tempUpdater;
        });
    }

    onClickNextStage()
    {
        if(this.isAutoAdventured()){
            this.OpenAutoAdventurePopup();
            return;
        }

        //현재 스테이지 저장
        let params =
        {
            world : this.curWorld,
            stage : this.curStage+1,
            next : true
        }
        if(DataManager.GetData("StageEndData") != null)
        {
            DataManager.DelData("StageEndData");
        }
        DataManager.AddData("StageEndData", params)

        SoundMixer.DestroyAllClip()
        SceneManager.SceneChange("WorldAdventure");
    }

    initAutoButton(isSuccess : boolean)
    {
        this.currentAutoCount = this.defaultAutoCount;

        if(this.isAutoAdventured())//값 자체가 있는지
        {
            if(this.isRemainAutoCount())
            {
                this.ShowAutoButton();
                this.reduceCountSchedule();
            }
            else
            {
                this.HideAutoButton();
                this.ShowTotalRewardPopup();//전체 보상 UI 팝업

                var cancle = DataManager.GetData("AutoAdventureCancle");
                if(cancle == null) {
                    cancle = false;
                } else {
                    DataManager.DelData("AutoAdventureCancle");
                }
                //탐험 실패 노티 팝업 출력
                if(!isSuccess && !cancle){
                    this.ShowAdventureFailPopup();
                }

                DataManager.DelData("AutoAdventureCount");
                DataManager.DelData("AutoAdventureTotalCount");//전체 판수 삭제
            }
        }
        else
        {
            this.HideAutoButton();
        }
    }

    ShowAutoButton()
    {
        if(this.autoButtonNode.activeInHierarchy == false){
            this.autoButtonNode.active = true;
        }
    }

    HideAutoButton()
    {
        if(this.autoButtonNode.activeInHierarchy == true){
            this.autoButtonNode.active = false;
        }
    }

    reduceCountSchedule()
    {
        this.schedule(this.SetLabelCount,1,this.defaultAutoCount);
    }

    SetLabelCount() 
    {
        this.currentAutoCount -= 1;

        if(this.currentAutoCount < 0){
            this.onClickAutoClick();
        }else{
            this.autoButtonLabel.string = this.currentAutoCount.toString();
        }
    }

    onClickAutoClick()
    {
        this.unschedule(this.SetLabelCount);
        this.AutoRetry();
    }

    PauseLabelSchedule()
    {
        director.getScheduler().pauseTarget(this);
    }

    resumeLabelSchedule()
    {
        director.getScheduler().resumeTarget(this);
    }

    isRemainAutoCount()
    {
        let remainCount = DataManager.GetData("AutoAdventureCount") as number;
        return remainCount > 0;
    }

    isAutoAdventured()
    {
        if(DataManager.GetData("AutoAdventureCount") == null || DataManager.GetData("AutoAdventureCount") == undefined){
            return false;
        }
        else{
            return true;
        }
    }

    minusAutoCount()
    {
        let remainCount = DataManager.GetData("AutoAdventureCount") as number;
        if(remainCount > 0)
        {
            DataManager.DelData("AutoAdventureCount");
        }
        DataManager.AddData("AutoAdventureCount", Number(remainCount - 1));
    }

    ShowTotalRewardPopup()//자동 전투 종료시 전체 보상 팝업 출력
    {
        let prevDataCheck : any = DataManager.GetData("AutoAdventureReward");
        if(prevDataCheck == null || prevDataCheck == undefined){
            DataManager.DelData("AutoAdventureReward");
            return;
        }

        let keyLength = Object.keys(prevDataCheck);
        if(keyLength == null || keyLength.length <= 0){
            DataManager.DelData("AutoAdventureReward");
            return;
        }

        let popup : SystemRewardPopup = PopupManager.OpenPopup("SystemRewardPopup", true) as SystemRewardPopup;
        popup.DirectRewardList(DataManager.GetData("AutoAdventureReward"));
        DataManager.DelData("AutoAdventureReward");
    }
    
    setTotalAutoAdventureReward(elementNo : number , elementCount : number, elementType: number)//누적 보상 데이터 쌓기
    {
        let keys = Object.keys(this.autoAdventureReward);
        let keyCheckList = keys.filter(element => element == elementNo.toString());
        if(keyCheckList == null || keyCheckList.length <= 0){
            this.autoAdventureReward[elementNo] = {no : elementNo , count : elementCount, type : elementType};
        }else{
            let amount = Number(this.autoAdventureReward[elementNo].count);
            this.autoAdventureReward[elementNo] ={no : elementNo , count : amount + elementCount, type : elementType};
        }
    }
    saveTotalAutoAdventureRewardData()
    {
        let rewardData : any = DataManager.GetData("AutoAdventureReward");
        if(rewardData == null || rewardData == undefined){
            DataManager.AddData("AutoAdventureReward" , this.autoAdventureReward);
        }
        else//겹치는 데이터 정제하기
        {
            let tempAdventureReward : any = {};
            let prevDataKeys = Object.keys(rewardData);//기존 데이터 복사
            prevDataKeys.forEach(key=>{
                tempAdventureReward[key] = rewardData[key];
            });

            let currentDataKeys = Object.keys(this.autoAdventureReward);
            currentDataKeys.forEach(currentKey =>{
                let isKeyContain = prevDataKeys.filter(element=>element == currentKey);
                if(isKeyContain != null && isKeyContain.length > 0)//중복된 데이터가 있다면
                {
                    let tempRewardData = tempAdventureReward[currentKey];
                    let currentData = this.autoAdventureReward[currentKey];

                    tempAdventureReward[currentKey].count = tempRewardData.count + currentData.count;
                }
                else//신규
                {
                    tempAdventureReward[currentKey] = this.autoAdventureReward[currentKey];
                }
            });

            DataManager.DelData("AutoAdventureReward");
            DataManager.AddData("AutoAdventureReward" , tempAdventureReward);
        }
    }

    //자동 전투 시에 팝업 노티
    OpenAutoAdventurePopup()
    {
        this.PauseLabelSchedule();

        let popup = PopupManager.OpenPopup("SystemPopupOKCANCEL") as SystemPopup;
        popup.setMessage(StringTable.GetString(100000248, "알림"),StringTable.GetString(100000781, "현재 자동 전투 중 입니다.\n자동 전투를 중단할까요?"));
        popup.setCallback(()=>{
            //확인 버튼 누를 시
            popup.ClosePopup();
            this.HideAutoButton();
            this.ShowTotalRewardPopup();//전체 보상 UI 팝업
            DataManager.DelData("AutoAdventureCount");
            DataManager.DelData("AutoAdventureTotalCount");
        },
        ()=>{
            popup.ClosePopup();
            this.resumeLabelSchedule();
        });
    }

    //실패 시 자동전투 해제
    ReleaseAutoAdventure()
    {
        let remainCount = DataManager.GetData("AutoAdventureCount");
        if(remainCount != null){
            DataManager.DelData("AutoAdventureCount");
        }
        DataManager.AddData("AutoAdventureCount", 0);
    }

    ShowAdventureFailPopup()
    {
        let popup = PopupManager.OpenPopup("SystemPopupOK") as SystemPopup;
        popup.setMessage(StringTable.GetString(100000248, "알림"), StringTable.GetString(100000782, "전투 실패로 인하여 반복 전투가 종료되었습니다."));
    }

    ShowStatisticPopup()
    {
        let params = {dragons :  this.dragonList , damage : this.battleDamageStatistic , time : this.time}
        PopupManager.OpenPopup("AdventureDamageStatisticPopup", true, params) as AdventureDamageStatisticPopup;
    }
}

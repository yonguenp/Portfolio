
import { _decorator, Node, Prefab, instantiate, ProgressBar, Label, ScrollView, tween, Tween, Vec3 } from 'cc';
import { CharExpTable } from '../../Data/CharTable';
import { ItemBaseData } from '../../Data/ItemData';
import { ItemBaseTable } from '../../Data/ItemTable';
import { StringTable } from '../../Data/StringTable';
import { TableManager } from '../../Data/TableManager';
import { GameManager } from '../../GameManager';
import { NetworkManager } from '../../NetworkManager';
import { ResourceManager, ResourcesType } from '../../ResourceManager';
import { DataManager } from '../../Tools/DataManager';
import { FillZero, GetDragonStat, GetIncrementDragonStat } from '../../Tools/SandboxTools';
import { TutorialManager } from '../../Tutorial/TutorialManager';
import { User } from '../../User/User';
import { PopupManager } from '../Common/PopupManager';
import { SubLayer } from '../Common/SubLayer';
import { DragonLevelUpPopup } from '../DragonLevelUpPopup';
import { ItemFrame } from '../ItemSlot/ItemFrame';
import { ToastMessage } from '../ToastMessage';
import { dragonCharacterSlotComponent } from './detailInfoLayer/dragonCharacterSlotComponent';
import { dragonDescComponent } from './detailInfoLayer/dragonDescComponent';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = DragonLevelUpLayer
 * DateTime = Fri Mar 11 2022 12:20:57 GMT+0900 (대한민국 표준시)
 * Author = wonjun_gs
 * FileBasename = DragonLevelUpLayer.ts
 * FileBasenameNoExtension = DragonLevelUpLayer
 * URL = db://assets/Scripts/UI/DragonManagement/DragonLevelUpLayer.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('DragonLevelUpLayer')
export class DragonLevelUpLayer extends SubLayer {

    @property(ScrollView)
    invenScroll : ScrollView = null;

    @property(Node)
    invenNodeContent : Node = null//

    @property(ScrollView)
    selectScroll : ScrollView = null;

    @property(Node)
    selectNodeContent : Node = null

    itemFrameClone : Prefab = null
    currentDragonLevel : number = 0;//현재 선택된 드래곤 레벨
    currentDragonExp : number = 0;//현재 선택된 드래곤 경험치의 잔여 경험치 (총경험치 아님!)
    currentDragonTag : number = 0;
    charExpTable : CharExpTable = null;
    

    @property(ProgressBar)
    currentLevelBar : ProgressBar = null;
    @property(Label)
    currentExpLabel : Label = null;
    @property(ProgressBar)
    levelUpBar : ProgressBar = null;
    @property(Label)
    currentLevelLabel : Label = null;
    @property(Label)
    currentBattlePointLabel : Label = null;
    @property(Label)
    nextLevelLabel : Label = null;
    @property(Label)
    nextBattlePointLabel : Label = null;

    tempMaxLevelCheckFlag : boolean = false;//드래곤 레벨 최대치 체크
    tempPredictableLevel : number = 0;//다음 예상 레벨 임시 변수 - 레벨 라벨 표시용도
    tempLevelCheck : number = -1;//레벨업 연출 할때 다음예상 레벨 몇인지 임시 변수
    @property(dragonDescComponent)
    dragonDesc : dragonDescComponent = null;

    @property(dragonCharacterSlotComponent)
    dragonCharacter : dragonCharacterSlotComponent = null;

    @property(Node)
    tempTweenNode : Node = null;

    Init()
    {
        if(this.dragonDesc != null){
            this.dragonDesc.init();
        }
        if(this.dragonCharacter != null){
            this.dragonCharacter.init();
        }
        this.tempMaxLevelCheckFlag = false;
        this.tempLevelCheck = -1;
        this.setCharExpTable();
        this.AllClearScollNode();
        this.drawAllInven();
        this.RefreshDragonInfo();
    }

    ForceUpdate()
    {
        this.Init();
    }

    setCharExpTable()
    {
        if(this.charExpTable == null){
            this.charExpTable = TableManager.GetTable<CharExpTable>(CharExpTable.Name);
        }
    }

    AllClearScollNode()
    {
        if(this.invenNodeContent != null){
            this.invenNodeContent.removeAllChildren();
        }
        if(this.selectNodeContent != null){
            this.selectNodeContent.removeAllChildren();
        }
    }

    drawAllInven()//현재 갖고 있는 배터리 타입 전체 갱신
    {
        this.invenNodeContent.removeAllChildren();
        this.itemFrameClone = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB)["item"]

        let items = User.Instance.GetAllItems();
        for(let i = 0; i < items.length; i++)
        {
            let itemNo = items[i].ItemNo;

            let isBatteryCheck = this.isBatteryType(itemNo);
            if(isBatteryCheck){

                let itemInfo : ItemBaseData = TableManager.GetTable(ItemBaseTable.Name).Get(items[i].ItemNo);
                if(itemInfo == null || itemInfo.SLOT_USE == null)
                {
                    console.log(items[i].ItemNo)
                }
    
                let itemRemain = items[i].Count
                let slotCount = 0
    
                while(itemRemain != 0)
                {
                    let clone : Node = instantiate(this.itemFrameClone)
                    clone.parent = this.invenNodeContent;
    
                    if(itemRemain > itemInfo.MERGE)
                    {
                        slotCount = itemInfo.MERGE
                        itemRemain -= itemInfo.MERGE
                    }
                    else
                    {
                        slotCount = itemRemain
                        itemRemain -= slotCount
                    }
    
                    clone.getComponent(ItemFrame).setFrameItemExpInfo(items[i].ItemNo, slotCount,itemInfo.VALUE);
                    clone.getComponent(ItemFrame).setCallback((param)=>{
                        this.onClickInvenFrame(param);
                    });
                }
            }
        }

        if(this.invenScroll != null){
            this.invenScroll.scrollToLeft();//전부 그렸다면 맨 왼쪽으로 초기화
        }
    }

    isBatteryType(itemNo : number):boolean{
        if(itemNo >= 40000001 && itemNo < 50000000){
            return true;
        }
        else{
            return false;
        }
    }

    onClickInvenFrame(selectIdx : string)//선택된 인벤토리에 선택한 노드가 없으면 생성, 있으면 카운트만 리프레시
    {
        if(this.tempMaxLevelCheckFlag)
        {
            console.log('level Max Already');
            let emptyCheck = ToastMessage.isToastEmpty();
            if(emptyCheck == false){
                return;
            }
            
            let text = TableManager.GetTable<StringTable>(StringTable.Name).Get(100000210).TEXT;
            ToastMessage.Set(text, null, -52);
            return;
        }

        this.makeSelectItemFrameByIndex(Number(selectIdx));
        this.RefreshDragonProgressBar();//현재 선택된 노드 기준 경험치량 계산

        // 튜토리얼 실행
        // if (TutorialManager.GetInstance.IsNowPlayingTutorialByGroup(106)){

        //     // 건전지 1개 선택 시
        //     TutorialManager.GetInstance.OnTutorialEvent(106, 7);

        //     // 건전지 2개선택 시
        //     let nodeList = this.selectNodeContent.children;
        //     if(nodeList != null || nodeList.length > 0){
        //         nodeList.forEach((element)=>{
        //             let itemFrameComp = element.getComponent(ItemFrame);
        //             if(itemFrameComp != null)
        //             {
        //                 let itemInfo : ItemBaseData = TableManager.GetTable(ItemBaseTable.Name).Get(itemFrameComp.getItemID());
        //                 let Amount = Number(itemFrameComp.getItemAmountString());

        //                 if (Amount >= 2){
        //                     TutorialManager.GetInstance.OnTutorialEvent(106, 8);
        //                 }
        //             }
        //         });   
        //     }
        // }
    }
    
    makeSelectItemFrameByIndex(selectIdx : number)
    {
        if(this.selectNodeContent == null) 
        {
            console.log("select Node is Null");
            return;
        }
        //선택 건전지 스크롤 뷰에 세팅
        let checkNode = this.getItemFrameByIndex(selectIdx,this.selectNodeContent.children) as ItemFrame;
        if(checkNode == null)//생성 인벤슬롯이 없음
        {
            let clone : Node = instantiate(this.itemFrameClone)
            clone.parent = this.selectNodeContent;

            clone.getComponent(ItemFrame).setFrameItemInfo(selectIdx, 1);
            clone.getComponent(ItemFrame).setCallback((param)=>{
                
                let selectNode = this.getItemFrameByIndex(Number(param),this.selectNodeContent.children) as ItemFrame;
                let invenNode = this.getItemFrameByIndex(Number(param),this.invenNodeContent.children) as ItemFrame;
                
                if(this.SelectInvenCountOne(selectNode)){
                    this.SetCountItemFrame(-1, selectNode);
                }
                else{
                    selectNode.node.removeFromParent();
                    //selectNode.node.destroy();
                }

                if(invenNode == null)//노드 전부 삭제시 프레임 재생성
                {
                    let clone : Node = instantiate(this.itemFrameClone);
                    clone.parent = this.invenNodeContent;

                    let checkSiblingIndex = this.GetChildIndexByItemID(Number(param));//현재 들어온 itemID가 스크롤뷰 어느 위치인지 판단
                    if(checkSiblingIndex >= 0){
                        clone.setSiblingIndex(checkSiblingIndex);
                    }

                    let itemInfo : ItemBaseData = TableManager.GetTable(ItemBaseTable.Name).Get(Number(param));
                    clone.getComponent(ItemFrame).setFrameItemExpInfo(Number(param), 1,itemInfo.VALUE);
                    clone.getComponent(ItemFrame).setCallback((param)=>{
                        this.onClickInvenFrame(param);
                    });

                    if(this.invenScroll != null){
                        this.invenScroll.scrollToLeft();//전부 그렸다면 맨 왼쪽으로 초기화
                    }
                }
                else{
                    this.SetCountItemFrame(1, invenNode);
                }
                this.RefreshDragonProgressBar();//현재 선택된 노드 기준 경험치량 계산
            });

            //생성되는 노드 내림 차순 정렬
            let checkSiblingIndex = this.GetSelectNodeChildIndexByItemID(selectIdx);//현재 들어온 itemID가 스크롤뷰 어느 위치인지 판단
            if(checkSiblingIndex >= 0){
                clone.setSiblingIndex(checkSiblingIndex);
            }
            if(this.selectScroll != null){
                this.selectScroll.scrollToLeft();//전부 그렸다면 맨 왼쪽으로 초기화
            }
        }
        else//인벤 슬롯이 null이 아님 (있음)
        {
            this.SetCountItemFrame(1,checkNode);
        }
        //현재 선택 인벤 노드의 아이템 갯수 줄이기
        let invenNode = this.getItemFrameByIndex(selectIdx,this.invenNodeContent.children) as ItemFrame;
        if(invenNode != null)
        {
            if(this.SelectInvenCountOne(invenNode)){
                this.SetCountItemFrame(-1, invenNode);
            }
            else{
                this.SetCountItemFrame(-1, invenNode);
                invenNode.node.destroy();
            }
        }
    }
    getItemFrameByIndex(selectIdx : number, nodeList :Node[]) : ItemFrame
    {
        if(nodeList == null || nodeList.length <= 0){
            return null;
        }

        let tempFrame : ItemFrame = null;

        nodeList.forEach((Element)=>{
            let itemFrameData = Element.getComponent(ItemFrame);
            if(itemFrameData != null){
                let frameItemID = itemFrameData.getItemID();
                if(frameItemID == selectIdx)
                {
                    tempFrame = itemFrameData;
                }
            }
        });
        return tempFrame;
        //return nodeList.find((element)=>{element.getComponent(ItemFrame).getItemID() == selectIdx}).getComponent(ItemFrame);
    }

    SetCountItemFrame(count : number , frameNode : ItemFrame)
    {  
        let itemAmount =Number(frameNode.getItemAmountString());
        frameNode.itemAmount.string = (itemAmount + count).toString();
    }
    
    //1보다 작으면 - 0이되면 해당 노드 삭제
    SelectInvenCountOne( itemNode :ItemFrame) : boolean
    {
        let itemAmount =Number(itemNode.getItemAmountString());
        if(itemAmount > 1){
            return true;
        }
        else{
            return false;
        }
    }

    initCurrentDragonData()
    {
        if(DataManager.GetData("DragonInfo") != null)//드래곤 태그값
        {
            let dragonTag = DataManager.GetData("DragonInfo") as number;
            let dragonData = User.Instance.DragonData;
            if(dragonData == null){
                console.log("user's dragon Data is null");
                return;         
            }

            let userDragonInfo = dragonData.GetDragon(dragonTag);
            if(userDragonInfo == null){
                console.log("user Dragon is null");
                return;
            }

            this.currentDragonTag = dragonTag;
            this.currentDragonLevel = userDragonInfo.Level;
            this.currentDragonExp = this.calcModifyReduceEXP(userDragonInfo.Exp);//누적 경험치로옴
        }
    }
    calcModifyReduceEXP(sendtotalExp : number) : number
    {
        let requireTotalEXP =  this.charExpTable.GetCurrentAccumulateLevelExp(this.currentDragonLevel);//현재 레벨 요구 경험치
        return sendtotalExp - requireTotalEXP;
    }

    RefreshDragonInfo()
    {
        this.initCurrentDragonData();//현재 드래곤 레벨, 경험치 세팅
        this.initLevelUPLabel();
        this.initProgressBar();
        this.initLevelLabel();
        this.RefreshDragonProgressBar();//현재 프로그래스 갱신- 초기화
    }

    RefreshDragonProgressBar()//현재 경험치 총량 계산 - 경험치 아이템 클릭시 들어옴
    {
        let calcTotalExp = this.CalcTotalSelectNodeExp();
        if(this.charExpTable != null)
        {
            let final = this.charExpTable.GetLevelAddExp(this.currentDragonLevel,this.currentDragonExp,calcTotalExp);
            let level  = JSON.parse(final.finallevel);//계산 완료 후 결과 레벨
            let reduceExp = JSON.parse(final.reduceExp);//계산 완료 후 나머지 경험치
            
            let checkMaxLevel = this.charExpTable.GetDragonMaxLevel();
            let maxLevelFlag = (checkMaxLevel == level);
            let levelChangeFlag = (level != this.currentDragonLevel);
            
            this.RefreshLevelUPProgressBar(levelChangeFlag,level,reduceExp);
            this.RefreshLevelLabel(level);
            this.RefreshLevelUPLabel(maxLevelFlag,level,reduceExp);
            this.tempMaxLevelCheckFlag = maxLevelFlag;
        }
    }
    //현재 선택한 아이템의 경험치 총량 계산
    CalcTotalSelectNodeExp() : number
    {
        let nodeList = this.selectNodeContent.children;
        if(nodeList == null || nodeList.length <= 0){
            return 0;
        }
        
        let totalExp : number = 0;
        nodeList.forEach((element)=>{
            let itemFrameComp = element.getComponent(ItemFrame);
            if(itemFrameComp != null)
            {
                let itemInfo : ItemBaseData = TableManager.GetTable(ItemBaseTable.Name).Get(itemFrameComp.getItemID());
                let itemValue = itemInfo.VALUE;
                let Amount = Number(itemFrameComp.getItemAmountString());
                totalExp +=(itemValue * Amount);
            }
        });

        return totalExp;
    }

    //배터리 인벤 자동 정렬 기능 구현하기
    SortInvenScrollview()
    {
        let childrenNode = this.invenNodeContent.children;//itemID 순으로 재정렬
        childrenNode.sort((a : Node , b : Node)=>{

            let AitemNo = a.getComponent(ItemFrame).getItemID();
            let BitemNo = b.getComponent(ItemFrame).getItemID();
            return AitemNo - BitemNo;
        });
    }
    //현재 선택된 배터리를 오름차순으로 정렬
    GetSelectNodeChildIndexByItemID(param : number) : number
    {
        let childrenNode = this.selectNodeContent.children;//현재 선택된 아이템
        if(childrenNode == null || childrenNode.length <=0){
            return -1;
        }

        //value 값으로 비교해야함
        let currentItemInfo : ItemBaseData = TableManager.GetTable(ItemBaseTable.Name).Get(param);
        let itemValue = currentItemInfo.VALUE;

        let tempValueArr : number[] = [];
        childrenNode.forEach((element)=>{
            let elementItemID = element.getComponent(ItemFrame).getItemID();
            let elementItemInfo : ItemBaseData = TableManager.GetTable(ItemBaseTable.Name).Get(elementItemID);
            let elementValue = elementItemInfo.VALUE;
            tempValueArr.push(elementValue);
        });

        for(let i = 0 ; i < tempValueArr.length ; i++){
            let checkNumber = tempValueArr[i];
            if(itemValue > checkNumber){
                return i;
            }
        }

        return tempValueArr.length;
    }

    //선택한 아이템을 취소에서 인벤에 다시 들어가는 상황에서 들어갈 위치 세팅하기
    GetChildIndexByItemID(param : number) : number
    {
        let childrenNode = this.invenNodeContent.children;//현재 아이템 순서

        if(childrenNode == null || childrenNode.length <=0){
            return -1;
        }

        let tempIndexArr : number[] = [];
        childrenNode.forEach((element)=>{
            let elementItemID = element.getComponent(ItemFrame).getItemID();
            tempIndexArr.push(elementItemID);
        });

        for(let i = 0 ; i < tempIndexArr.length ; i++){
            let checkNumber = tempIndexArr[i];
            if(param < checkNumber){
                return i;
            }
        }

        return tempIndexArr.length;
    }

    /**
     * 
     * @param isLevelChangeFlag // 레벨 변경 플래그
     * @param level //변경 후 레벨
     * @param reduceExp //잔여 경험치
     * @returns 
     */

    tweenAnim: Tween<Node> = null;

    RefreshLevelUPLabel(maxLevelFlag : boolean ,level : number, reduceExp : number)
    {
        if(this.currentExpLabel == null){
            return;
        }

        if(this.tweenAnim != null) {
            this.tweenAnim.stop();
        } 
        if(maxLevelFlag){
            this.currentExpLabel.string = TableManager.GetTable<StringTable>(StringTable.Name).Get(100000113).TEXT;
        }
        else{
            let totalExp =  this.charExpTable.GetCurrentRequireLevelExp(level);//현재 레벨 요구 경험치
            if(level == this.currentDragonLevel)
            {
                //this.currentExpLabel.string = `${reduceExp + this.currentDragonExp} / ${totalExp}`;
                this.tweenAnim = tween(this.tempTweenNode).to(0.2, {position : new Vec3(reduceExp + this.currentDragonExp , 0, 0)},{onUpdate : ()=>
                    {
                        this.currentExpLabel.string = ((this.tempTweenNode.position.x.toFixed(0)).toString() + ` / ${totalExp}`);
                    }}).union().start();
            }
            else{
                //this.currentExpLabel.string = `${reduceExp} / ${totalExp}`;
                this.tweenAnim = tween(this.tempTweenNode).to(0.2, {position : new Vec3(0 , reduceExp, totalExp)},{onUpdate : ()=>
                    {
                        this.currentExpLabel.string = ((this.tempTweenNode.position.y.toFixed(0)).toString() +" / " + this.tempTweenNode.position.z.toFixed(0).toString());
                    }}).union().start();
            }
        }
    }

    initLevelUPLabel()
    {
        if(this.currentExpLabel == null){
            return;
        }
        let totalExp =  this.charExpTable.GetCurrentRequireLevelExp(this.currentDragonLevel);//현재 레벨 요구 경험치
        this.currentExpLabel.string = `${this.currentDragonExp} / ${totalExp}`;
        this.tempTweenNode.position = new Vec3(this.currentDragonExp,0,0);
    }

    initProgressBar()//현재 레벨 및 경험치 기반으로 세팅
    {
        let totalExp =  this.charExpTable.GetCurrentRequireLevelExp(this.currentDragonLevel);//현재 레벨 요구 경험치
        let mod = this.currentDragonExp / totalExp;
        
        if(this.levelUpBar != null && this.currentLevelBar)
        {
            this.levelUpBar.progress = mod;
            this.currentLevelBar.progress = mod;
        }
    }

    /**
     * @param levelChangeFlag //현재의 드래곤 레벨과 다른지 - 단순 동렙 체크용
     * @param level //들어온 레벨
     * @param reduceExp //잔여 경험치
     * @param isAutoClick //자동 선택버튼을 눌렀는지
     */
    RefreshLevelUPProgressBar(levelChangeFlag : boolean, level : number , reduceExp : number)//레벨업 노티
    {
        let totalExp =  this.charExpTable.GetCurrentRequireLevelExp(level);//현재 레벨 요구 경험치
        let maxLevelCheck = this.charExpTable.GetDragonMaxLevel();//드래곤 맥스 레벨 체크
        let isDragonMaxLevel = (maxLevelCheck == level);//맥렙 체크
        let isDragonPrevLastLevel = (maxLevelCheck -1 == level);//현재 들어온 레벨이 맥렙 바로 전인가 - 추후에 한계 레벨도 분류해야함
        let mod = 0;

        let levelup : boolean = false;
        let leveldown : boolean = false;
        if(this.tempLevelCheck < 0){
            this.tempLevelCheck = level;
        }
        if(this.tempLevelCheck < level)//레벨이 증가하려함
        {
            leveldown = false;
            levelup = true;
        }
        else if(this.tempLevelCheck > level)//레벨이 떨어지려함
        {
            leveldown = true;
            levelup = false;
        }
        else
        {
            leveldown = false;
            levelup = false;
        }
        this.tempLevelCheck = level;


        if(totalExp != 0)
        {
            mod = reduceExp / totalExp;
        }

        if(isDragonMaxLevel)//맥렙일때 - 맥렙이 된 경우
        {
            if(this.currentDragonLevel == maxLevelCheck)//이미 드래곤이 맥스레벨
            {
                this.currentLevelBar.barSprite.node.active = false;
                this.levelUpBar.progress = 1;
                this.levelUpBar.barSprite.node.active = true;
                return;
            }
            if(leveldown == false && levelup == false){
                return;
            }
            this.playTweenAllMotion(mod, 5, levelChangeFlag);
        }
        else{
            if(levelChangeFlag){
                if(this.levelUpBar != null && this.currentLevelBar){
                    if(levelup == true && leveldown == false){
                        this.playTweenAllMotion(mod, 2, levelChangeFlag);
                    }
                    else if(levelup == false && leveldown == true)
                    {
                        if(isDragonPrevLastLevel){
                            this.playTweenAllMotion(mod, 4, levelChangeFlag);
                        }else{
                            this.playTweenAllMotion(mod, 3, levelChangeFlag);
                        }
                    }
                    else
                    {
                        this.playTweenAllMotion(mod, 1, levelChangeFlag);
                    }
                }
            }
            else{
                if(this.levelUpBar != null && this.currentLevelBar){
                    
                    mod = (reduceExp + this.currentDragonExp) / totalExp;
                    let currentExp =  this.charExpTable.GetCurrentRequireLevelExp(this.currentDragonLevel);//현재 레벨 요구 경험치
                    let currentMode = this.currentDragonExp / currentExp;
                    this.currentLevelBar.progress = currentMode;
                    if(leveldown)
                    {
                        if(isDragonPrevLastLevel)//맥렙 바로 전일때
                        {
                            this.playTweenAllMotion(mod, 4, levelChangeFlag);  
                        }
                        else{
                            this.playTweenAllMotion(mod, 3, levelChangeFlag);    
                        }
                    }
                    else{
                        this.playTweenAllMotion(mod, 1, levelChangeFlag);
                    }
                }
            }
        }
    }

    tweenProgressAnim: Tween<ProgressBar> = null;
    playTweenAllMotion(modifyAmount : number , caseIndex : number, isChangeCurrentLevel: boolean)
    {
        this.levelUpBar.barSprite.node.active = true;

        if(this.tweenProgressAnim != null){
            this.tweenProgressAnim.stop();
        }
        switch(caseIndex)
        {
            case 1 :
                this.currentLevelBar.barSprite.node.active = !isChangeCurrentLevel;
                this.tweenProgressAnim = tween(this.levelUpBar).to(0.2, {progress : modifyAmount}).union().start();
                break;
            case 2 ://레벨 업 평상렙
                this.tweenProgressAnim = tween(this.levelUpBar).to(0.3, {progress : 1},{onComplete : ()=>{this.currentLevelBar.barSprite.node.active = false}}).to(0,{progress : 0}).to(0.3, {progress : modifyAmount}).union().start();
                break;
            case 3 ://레벨 다운 평상렙
                this.tweenProgressAnim = tween(this.levelUpBar).to(0.3, {progress : 0}).to(0,{progress : 1}).call(()=>{
                    if(isChangeCurrentLevel == false){
                        this.currentLevelBar.barSprite.node.active = true;
                    }
                }).to(0.3, {progress : modifyAmount}).union().start();
                break;
            case 4 ://레벨 다운(맥렙에서 맥렙 바로전)
                this.tweenProgressAnim = tween(this.levelUpBar).to(0, {progress : 1},{onComplete : ()=>{
                    this.levelUpBar.barSprite.node.active = true;
                    //this.levelUpBar.progress = 1;
                    if(!isChangeCurrentLevel){
                        this.currentLevelBar.barSprite.node.active = true;
                    }}}).to(0.3, {progress : modifyAmount}).union().start();
                break;
            case 5 : //맥렙으로 레벨업
                this.tweenProgressAnim =  tween(this.levelUpBar).to(0.2, {progress : 1},{onComplete : ()=>
                    {
                        this.currentLevelBar.barSprite.node.active = false;
                        this.levelUpBar.progress = 1;
                        this.levelUpBar.barSprite.node.active = true;
                    }})//.to(0,{progress : 0})
                    .to(0.2, {progress : 1}).union().start();
                break;
        }
        //console.log(this.tweens);
    }

    initLevelLabel()
    {
        if(this.currentLevelLabel != null && this.nextLevelLabel != null)
        {
            let levelMod = FillZero(2,this.currentDragonLevel);
            this.currentLevelLabel.string = `Lv . ${levelMod}`;
            this.nextLevelLabel.string = `Lv . ${levelMod}`;
            this.tempPredictableLevel = this.currentDragonLevel;
        }
        if(this.currentBattlePointLabel != null && this.nextBattlePointLabel != null)
        {
            let dragonINF = this.GetCurrentDragonINF();
            
            this.currentBattlePointLabel.string = TableManager.GetTable<StringTable>(StringTable.Name).Get(100000177).TEXT + " "+ dragonINF;
            this.nextBattlePointLabel.string = TableManager.GetTable<StringTable>(StringTable.Name).Get(100000177).TEXT + " "+ dragonINF;
        }
    }
    RefreshLevelLabel(nextLevel : number)
    {
        if(this.currentLevelLabel != null && this.nextLevelLabel != null)
        {
            let levelMod = FillZero(2,nextLevel);
            this.nextLevelLabel.string = `Lv . ${levelMod}`;
            this.tempPredictableLevel = nextLevel;
        }

        if(this.currentBattlePointLabel != null && this.nextBattlePointLabel != null)
        {
            let dragonINF = (this.GetCurrentDragonINF() + this.GetIncrementDragonINF(nextLevel));
            
            this.nextBattlePointLabel.string = TableManager.GetTable<StringTable>(StringTable.Name).Get(100000177).TEXT + " "+ dragonINF;
        }
    }
    //현재 레벨의 총합 전투력
    GetCurrentDragonINF() : number
    {
        let dragonInfo = User.Instance.DragonData.GetDragon(this.currentDragonTag);
        if(dragonInfo == null){
            return;
        }
        return dragonInfo.GetDragonALLStat().INF;
    }
    //증가분 투력치
    GetIncrementDragonINF(nextLevel : number): number
    {
        let incrementStat = GetIncrementDragonStat(this.currentDragonTag ,this.currentDragonLevel , nextLevel);
        return incrementStat.INF;
    }

    SendLevelUpRequest()
    {
        let checkStr = this.makeSendItemListByString();
        if(checkStr == '')
        {
            console.log('not select battery');
            let text = TableManager.GetTable<StringTable>(StringTable.Name).Get(100000207).TEXT;
            ToastMessage.Set(text, null, -52);
            return;
        }

        NetworkManager.Send('dragon/battery', {
            did : this.currentDragonTag,
            item : checkStr,
        }, (jsonData) => {
            console.log('levelup success callback');
            let params = { 
                currentLevel : this.currentDragonLevel, 
                nextLevel : this.tempPredictableLevel,
            }

            if (this.tempPredictableLevel > this.currentDragonLevel) {
                PopupManager.OpenPopup("DragonLevelUpPopup", true, params) as DragonLevelUpPopup;
            }
            
            this.ForceUpdate();
        });
    }

    makeSendItemListByString() : string
    {
        let nodeList = this.selectNodeContent.children;
        if(nodeList == null || nodeList.length <= 0){
            return '';
        }
        
        let totalStr : string = '';
        nodeList.forEach((element)=>{
            let itemFrameComp = element.getComponent(ItemFrame);
            if(itemFrameComp != null)
            {
                let Amount = Number(itemFrameComp.getItemAmountString());
                totalStr +=(itemFrameComp.getItemID().toString() + ":"+ Amount+",");
            }
        });
        return totalStr;
    }

    /**
     * 자동 선택 버튼 +1 레벨업 기준으로 자동 건전지 등록
     */
    
    onClickAutoSelectButton()
    {
        let nodeList = this.invenNodeContent.children;
        if(nodeList == null || nodeList.length <= 0){
            let checkToast = ToastMessage.isToastEmpty();
            if(checkToast){
                let text = TableManager.GetTable<StringTable>(StringTable.Name).Get(100000208).TEXT;
                ToastMessage.Set(text, null, -52);
            }
            return;
        }

        let reduceExp = this.currentDragonExp;
        let totalExp =  this.charExpTable.GetCurrentRequireLevelExp(this.tempPredictableLevel);//현재 레벨 요구 경험치
        let maxLevel =  this.charExpTable.GetDragonMaxLevel();

        if(maxLevel == this.currentDragonLevel)
        {
            let checkToast = ToastMessage.isToastEmpty();
            if(checkToast){
                let text = TableManager.GetTable<StringTable>(StringTable.Name).Get(100000210).TEXT;
                ToastMessage.Set(text, null, -52);
                }
            return;
        }
        else{
            let currentSelectedEXP = this.CalcTotalSelectNodeExp();//현재 걸려있는 경험치총량
            let currentDragonExp = User.Instance.DragonData.GetDragon(this.currentDragonTag).Exp;//현재 드래곤 경험치 총량
            let predictDragonExp = (currentSelectedEXP+currentDragonExp);//예상 총 경험치

            let predictData = this.charExpTable.GetLevelAndExpByTotalExp(predictDragonExp);
            let predictReduce = predictData.reduceExp;
            
            reduceExp = predictReduce;
        }

        let requireExp = totalExp - reduceExp;

        nodeList.forEach((element)=>{
            let itemFrameComp = element.getComponent(ItemFrame);
            if(itemFrameComp != null)
            {
                if(requireExp <= 0){
                    return;
                }
                let itemID = itemFrameComp.getItemID();
                let itemInfo : ItemBaseData = TableManager.GetTable(ItemBaseTable.Name).Get(itemFrameComp.getItemID());
                let itemValue = itemInfo.VALUE;
                let itemAmount = Number(itemFrameComp.getItemAmountString());
                
                while(itemAmount > 0)
                {
                    this.onClickInvenFrame(itemID.toString());
                    itemAmount = Number(itemFrameComp.getItemAmountString());
                    requireExp -= itemValue;
                    if(requireExp <= 0){
                        break;
                    }
                }
            }
        });
    }


    /**
    * //[보유 중인 경험치 아이템] 중 현재 [선택해야 하는 경험치]와 제일 차이가 적은 경험치 아이템 선택
    * //onClickAutoSelectButton <-- 이 함수는 가장 왼쪽(가장 적은 경험치)기준으로 채워넣음
    */
    onClickAutoSelectButton_effectiveUse()
    {
        let nodeList = this.invenNodeContent.children;
        if(nodeList == null || nodeList.length <= 0){
            let checkToast = ToastMessage.isToastEmpty();
            if(checkToast){
                let text = TableManager.GetTable<StringTable>(StringTable.Name).Get(100000208).TEXT;
                ToastMessage.Set(text, null, -52);
            }
            return;
        }

        let reduceExp = this.currentDragonExp;
        let totalExp =  this.charExpTable.GetCurrentRequireLevelExp(this.tempPredictableLevel);//현재 레벨 요구 경험치
        let maxLevel =  this.charExpTable.GetDragonMaxLevel();

        if(maxLevel == this.currentDragonLevel)
        {
            let checkToast = ToastMessage.isToastEmpty();
            if(checkToast){
                let text = TableManager.GetTable<StringTable>(StringTable.Name).Get(100000210).TEXT;
                ToastMessage.Set(text, null, -52);
                }
            return;
        }
        else{
            let currentSelectedEXP = this.CalcTotalSelectNodeExp();//현재 걸려있는 경험치총량
            let currentDragonExp = User.Instance.DragonData.GetDragon(this.currentDragonTag).Exp;//현재 드래곤 경험치 총량
            let predictDragonExp = (currentSelectedEXP+currentDragonExp);//예상 총 경험치

            let predictData = this.charExpTable.GetLevelAndExpByTotalExp(predictDragonExp);
            let predictReduce = predictData.reduceExp;
            
            reduceExp = predictReduce;
        }

        let requireExp = totalExp - reduceExp;//레벨업에 필요한 요구 경험치 - calcTotalExp

        while(requireExp > 0)
        {
            let getCheckItemID = this.CalcEffectiveDiffBattery(requireExp);
            if(getCheckItemID < 0){
                break;
            }
            this.onClickInvenFrame(getCheckItemID.toString());
            let itemInfo : ItemBaseData = TableManager.GetTable(ItemBaseTable.Name).Get(getCheckItemID);
            let itemValue = itemInfo.VALUE;
            requireExp -= itemValue;
        }
    }

    /**
     * 남은 경험치 (reduceExp)기준으로 현재 인벤토리에서 가장 차이가 적은 아이템 태그를 반환해줌
     */
    CalcEffectiveDiffBattery(reduceExp : number) : number
    {
        let nodeList = this.invenNodeContent.children;
        if(nodeList == null || nodeList.length <= 0){
            return -1;
        }

        let diff = this.charExpTable.GetDragonMaxTotalExp();
        let tempItemID = -1;
        nodeList.forEach((element)=>{
            let itemFrameComp = element.getComponent(ItemFrame);
            if(itemFrameComp != null)
            {
                let itemID = itemFrameComp.getItemID();
                let itemInfo : ItemBaseData = TableManager.GetTable(ItemBaseTable.Name).Get(itemFrameComp.getItemID());
                let itemValue = itemInfo.VALUE;
                let itemAmount = Number(itemFrameComp.getItemAmountString());

                if(itemAmount <= 0){
                    return;
                }

                let calcDiff = Math.abs(reduceExp - itemValue);
                if(calcDiff < diff)
                {
                    diff = calcDiff;
                    tempItemID = itemID;
                }
            }
        });

        return tempItemID;
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

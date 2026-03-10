
import { _decorator, Component, Node, instantiate, Label, Prefab, sys, Event, ScrollView, EventHandler, Button, Vec2, Layout, UITransform, Vec3 } from 'cc';
import { MonsterBaseData } from '../Data/MonsterData';
import { MonsterSpawnTable, MonsterBaseTable } from '../Data/MonsterTable';
import { StageBaseData } from '../Data/StageData';
import { StageBaseTable } from '../Data/StageTable';
import { StringTable } from '../Data/StringTable';
import { TableManager } from '../Data/TableManager';
import { GameManager } from '../GameManager';
import { NetworkManager } from '../NetworkManager';
import { ResourceManager, ResourcesType } from '../ResourceManager';
import { SceneManager } from '../SceneManager';
import { SoundMixer, SOUND_TYPE } from '../SoundMixer';
import { StageDragonList } from '../Stage/StageDragonList';
import { StageSelectBG } from '../Stage/StageSelectBG';
import { DataManager } from '../Tools/DataManager';
import { StringBuilder } from '../Tools/SandboxTools';
import { TutorialManager } from '../Tutorial/TutorialManager';
import { User } from '../User/User';
import { PopupManager } from './Common/PopupManager';
import { CharacterSlotFrame } from './ItemSlot/CharacterSlotFrame';
import { EnemyFrame } from './ItemSlot/EnemyFrame';
import { ExpFrame, ExpType } from './ItemSlot/ExpFrame';
import { ItemFrame } from './ItemSlot/ItemFrame';
import { SystemPopup } from './SystemPopup';
import { ToastMessage } from './ToastMessage';
import { WorldStageInfo } from './WorldStageInfo';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = StagePrepareComponent
 * DateTime = Tue May 03 2022 14:23:51 GMT+0900 (대한민국 표준시)
 * Author = jinwonjun
 * FileBasename = StagePrepareComponent.ts
 * FileBasenameNoExtension = StagePrepareComponent
 * URL = db://assets/Scripts/UI/StagePrepareComponent.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 /**
 * @dragons number[] 참여한 드래곤 태그 배열
 * @preset number 참여한 드래곤 프리셋 번호, 커스텀 프리셋은 -1
 * @battleLine number[][] 참여한 드래곤 전투 대형
 */
export class BattleLine
{
    userID : number = 0
    dragons : number[] = []
    preset : number = -1
    battleLine : number[][] = [[], [], []]
    
    public AddDragon(tag : PropertyKey)
    {
        let tempList = this.dragons.slice();
        tempList.push(Number(tag));
        
        this.ClearDragon();
        this.dragons = tempList;
        this.dragons.slice();
    }

    public AddDragonPosition(pos : number, tag : number)
    {
        //tag 값으로 현재 드래곤 있는지 전체 탐색
        let keyObject = Object.keys(this.battleLine);
        keyObject.forEach((key)=>{
            let dragonTagList = this.battleLine[Number(key)];
            if(Number(key) == pos)
            {
                this.battleLine[pos].push(tag);
            }
        })
    }

    public DeletePositionDragon(pos : number)//해당 포지션의 드래곤 삭제
    {
        if(this.battleLine[pos] != null)
        {
            this.battleLine[pos] = [];
            return true;
        }
    }

    public DeleteDragon(tag : number)
    {
        this.dragons = this.dragons.filter(value => value != tag);
    }

    public DeleteAllDragon()
    {
        this.dragons = [] as number[];
        this.ClearBattleLine();
    }

    public ClearDragon()
    {
        this.dragons = [] as number[];
    }

    initBattleLine()
    {
        this.ClearBattleLine();

        let sortWeight : {tag : number, pos : number, def : number}[] = [];
        let dragonArr = this.dragons;//일단 임시로 첫 인덱스 드래곤 리스트로 세팅
        dragonArr.forEach((tag)=>
        {
            let dragonData = User.Instance.DragonData.GetDragon(tag);
            if(dragonData == null){
                return;
            }
            sortWeight.push({tag : tag, pos : dragonData.Position, def : dragonData.GetDragonALLStat().DEF});
        })

        sortWeight = sortWeight.sort((a, b) => this.SortByDefAtk(a.tag, b.tag))
        this.SortFromFront(sortWeight, this.battleLine)
        this.SortFromBehind(sortWeight, this.battleLine)
        this.SortFromCenter(sortWeight, this.battleLine)
    }

    CheckBattleLine() : boolean
    {
        //이후 dragons의 길이 뿐만아니라 드래곤 소지 태그 검사, 프리셋 유무 검사 필요

        let adventureData = User.Instance.PrefData.AdventureFormationData;//전체 빈값 체크 포함해야됨.
        if(adventureData == null){
            return false;
        }

        this.ClearDragon();
        let tempDragonTagList = adventureData.TeamFormation[0];//각 탭별로 세팅해줘야함.
        if(tempDragonTagList == null || tempDragonTagList.length <=0){
            return;
        }

        tempDragonTagList = tempDragonTagList.slice();

        tempDragonTagList.forEach((element)=>{
            this.dragons.push(element);
        });

        this.dragons =  this.dragons.slice();
        this.preset = -1;

        return true;
    }

    ClearBattleLine()
    {
        this.battleLine = [[],[],[]];
    }

    private SortByDefAtk(tagA : number, tagB : number) : number
    {
        let dragonA = User.Instance.DragonData.GetDragon(tagA);
        let dragonB = User.Instance.DragonData.GetDragon(tagB);
        let statA = dragonA.GetDragonALLStat();
        let statB = dragonB.GetDragonALLStat();

        if(dragonA.Position > dragonB.Position)
            return 1;
        else if(dragonA.Position < dragonB.Position)
            return -1;

        if(statA.DEF > statB.DEF)
        {
            return -1;
        }

        return 0;
    }

    private SortFromFront(weight : {tag : number, pos : number, def : number}[], arrBl : number[][]) : number[][]
    {
        const limitLine = 2;

        let l = 0;
        let target = weight.filter(element => element.pos == 1)

        for(let i = 0 ; i < target.length ; i++)
        {
            if(arrBl[l].length >= limitLine)
            {
                ++l;
            }

            arrBl[l].push(target[i].tag);
        }

        return arrBl;
    }

    private SortFromBehind(weight : {tag : number, pos : number, def : number}[], arrBl : number[][]) : number[][]
    {
        const limitLine = 2;

        let l = 2;
        let target = weight.filter(element => element.pos == 3)

        for(let i = target.length - 1; i >= 0 ; i--)
        {
            if(arrBl[l].length >= limitLine)
            {
                --l;
            }

            arrBl[l].push(target[i].tag);
        }

        return arrBl;
    }

    private SortFromCenter(weight : {tag : number, pos : number, def : number}[], arrBl : number[][]) : number[][]
    {
        const limitLine = 2;

        let l = 2;
        let target = weight.filter(element => element.pos == 2)

        let cslot = limitLine - arrBl[1].length;
        
        if(target.length > cslot)
        {
            //중열 초과슬릇이 후열 여유슬릇보다 많다면 => 후열 초과분 만큼 전열로 당겨야함 => 후열부터 역순 입력
            if(target.length - cslot > limitLine - arrBl[2].length)
            {
                let slotLimit = limitLine - arrBl[2].length
                for(let i = 0; i < slotLimit && target.length > 0; i++)
                {
                    arrBl[2].push(target.pop().tag);                    
                }

                slotLimit = limitLine - arrBl[1].length
                for(let i = 0; i < slotLimit && target.length > 0; i++)
                {
                    arrBl[1].push(target.pop().tag);
                }

                slotLimit = limitLine - arrBl[0].length
                for(let i = 0; i < slotLimit && target.length > 0; i++)
                {
                    arrBl[0].push(target.pop().tag);
                }
            }
            else
            {
                target = target.sort((a, b) => a.def - b.def);

                let slotLimit = limitLine - arrBl[1].length
                for(let i = 0; i < slotLimit && target.length; i++)
                {
                    arrBl[1].push(target.pop().tag);
                }

                slotLimit = limitLine - arrBl[2].length
                for(let i = 0; i < slotLimit && target.length; i++)
                {
                    arrBl[2].push(target.pop().tag);
                }
            }
        }
        else
        {
            for(let i = 0; i < target.length; i++)
            {
                arrBl[1].push(target[i].tag);
            }
        }

        return arrBl;
    }

    isDragonPosFull(pos : number)
    {
        if(this.battleLine[pos].length >= 2)
            return true;
        
        return false;
    }

    GetSerializeBattleLine() : number[]//dragonTag list return
    {
        return this.dragons;
    }

    isDragonDeckFull()
    {
        return this.dragons.length >= 5;
    }

    isDragonDeckEmpty()
    {
        return this.dragons.length <= 0;
    }
}
@ccclass('StagePrepareComponent')
export class StagePrepareComponent extends Component 
{
    @property(Label)
    labelStage : Label = null

    @property(Node)
    nodePopupUIParent : Node = null

    @property(Node)
    nodeMonsterSlotParent : Node = null

    @property(Node)
    nodeRewardSlotParent : Node = null

    @property(Label)
    labelEnemyBattlePoint : Label = null

    @property(Label)
    labelMyBattlePoint : Label = null

    @property(Label)
    labelReqEnergy : Label = null

    @property(Label)
    teamSettingButtonLable : Label = null;

    @property(Prefab)
    prefDragonSlot : Prefab = null

    @property(Node)
    arrDragonParent : Node[] = []

    @property(StageDragonList)
    stageDragonList : StageDragonList = null;

    @property(Node)
    enemyNode : Node = null;

    @property(ScrollView)
    worldInfoScrollview : ScrollView = null;

    @property(Node)
    worldInfoContent : Node = null;

    @property(Prefab)
    worldInfoPrefab : Prefab = null;

    @property(Prefab)
    worldDetailInfoPrefab : Prefab = null;

    @property(Node)
    worldBGParentNode : Node = null;
    @property(Prefab)
    worldPrefabList : Prefab[] = [];

    protected worldInfo : any = {}
    private battleLine : BattleLine = new BattleLine()

    private currentDragonDeckList : CharacterSlotFrame[]  = [];
    private currentClickDragonTag : number = -1;//드래곤 풀인 상태에서의 클릭 체크

    private StageData : {[key: string]: any}

    private drawBG: string = "";

    WorldStageInfoList : WorldStageInfo[] = [];

    start()
    {
        this.HideEnemyUI();
    }

    SetData(StageData : {[key: string]: any})
    {
        this.StageData = {};
        
        let keys = Object.keys(StageData);
        keys.forEach(element=>{
            this.StageData[element] = StageData[element];
        })

        this.DrawWorldBG();
        this.DrawStageInfo();
        this.DrawDragon();
    }

    DrawWorldBG(worldindex : number = 0 , stageIndex : number = 0)
    {
        let currentWorld = worldindex;
        let currentStage = stageIndex;

        if(worldindex <= 0)
        {
            currentWorld = this.StageData.world as number;
        }

        if(stageIndex <= 0)
        {
            currentStage = this.StageData.stage as number;
        }
        
        let resource = GameManager.GetManager<ResourceManager>(ResourceManager.Name);
        let table = TableManager.GetTable<StageBaseTable>(StageBaseTable.Name);
        let data: StageBaseData = null;
        if(table != null) {
            data = table.GetByWorldStage(currentWorld, currentStage);
        }

        let e: Prefab = null;
        if(data == null || resource == null) {
            e = this.worldPrefabList[currentWorld - 1];
        } else {
            e = resource.GetResource(ResourcesType.WORLD_PREFAB)[data.IMAGE];
        }
        
        if(data != null && e != null) {
            if(data.IMAGE == this.drawBG) {
                return;
            }
            this.drawBG = data.IMAGE;
        }

        if(this.StageData != null && this.StageData['this'] != undefined) {
            this.StageData.this.FadeOutFog();
        }

        if(this.worldBGParentNode != null){
            this.worldBGParentNode.removeAllChildren();
        }
        let clone = instantiate(e);
        clone.parent = this.worldBGParentNode;

        let bg = clone.getComponent(StageSelectBG);
        if(bg != null) {
            bg.Init();
        }
    }

    DrawStageInfo()
    {
        this.worldInfoContent.removeAllChildren();
        this.WorldStageInfoList = [];

        let stageArr =  this.StageData.stages;//0은 미해금, 1~3까지 별갯수
        if(stageArr == null || stageArr.length <= 0){
            return;
        }

        let currentWorld = this.StageData.world as number;
        let firstZero : boolean = false
        
        for(let i = 0; i < stageArr.length; i++)
        {
            let clone = instantiate(this.worldInfoPrefab);
            clone.parent = this.worldInfoContent;

            let starCount = stageArr[i];
            let worldstageinfo = clone.getComponent(WorldStageInfo);
            if(worldstageinfo != null)
            {
                if(starCount > 0)
                {
                    worldstageinfo.SetData(currentWorld , i + 1 , starCount , false);
                }                
                else if(!firstZero)
                {
                    firstZero = true;
                    worldstageinfo.SetData(currentWorld , i + 1 , starCount , false);
                }
                else
                {
                    worldstageinfo.SetData(currentWorld , i + 1 , starCount , true);
                }

                let targetBtn = clone.getComponent(Button);
                if(targetBtn == null){
                    continue;
                }

                let event = new EventHandler();
                event.target = this.node;
                event.component = "StagePrepareComponent"
                event.handler = "onClickStageRedrawScroll";
                targetBtn.clickEvents.push(event);

                this.WorldStageInfoList.push(worldstageinfo);
            }
        }

        if(this.worldInfoScrollview != null){
            this.worldInfoScrollview.scrollToTop();
        }
    }

    ReDrawStageInfo(detailIndex : number , isAlreadyOpen : boolean)
    {
        this.worldInfoContent.removeAllChildren();
        this.WorldStageInfoList = [];

        let stageArr =  this.StageData.stages;//0은 미해금, 1~3까지 별갯수
        if(stageArr == null || stageArr.length <= 0){
            return;
        }

        let currentWorld = this.StageData.world as number;
        let firstZero : boolean = false
        let detailInfoMode : boolean = false;
        for(let i = 0; i < stageArr.length; i++)
        {
            let clone : Node = null;

            let detailCheck = detailIndex >= 0 && detailIndex == i;
            if(detailCheck)
            {
                if(isAlreadyOpen)
                {
                    clone = instantiate(this.worldInfoPrefab);
                }
                else{
                    clone = instantiate(this.worldDetailInfoPrefab);
                    this.DrawWorldBG(currentWorld, i + 1);
                }
                detailInfoMode = !isAlreadyOpen;
                detailCheck = !isAlreadyOpen;
            }else{
                clone = instantiate(this.worldInfoPrefab);
                detailInfoMode = false;
            }
            clone.parent = this.worldInfoContent;

            let starCount = stageArr[i];
            let worldstageinfo = clone.getComponent(WorldStageInfo);
            if(worldstageinfo != null)
            {
                if(starCount > 0)
                {
                    worldstageinfo.SetData(currentWorld , i + 1 , starCount , false ,detailCheck,detailInfoMode,this);
                }                
                else if(!firstZero)
                {
                    firstZero = true;
                    worldstageinfo.SetData(currentWorld , i + 1 , starCount , false, detailCheck,detailInfoMode,this);
                }
                else
                {
                    worldstageinfo.SetData(currentWorld , i + 1 , starCount , true, detailCheck,detailInfoMode,this);
                }

                let targetBtn = clone.getComponent(Button);
                if(targetBtn == null){
                    continue;
                }

                let event = new EventHandler();
                event.target = this.node;
                event.component = "StagePrepareComponent"
                event.handler = "onClickStageRedrawScroll";
                targetBtn.clickEvents.push(event);

                this.WorldStageInfoList.push(worldstageinfo);
            }
        }
    }

    /**
     * 
     * @param worldIndex = 0이 아닌 숫자 그대로임!
     * @param stageIndex = 0이 아닌 숫자 그대로임! - 안쪽에서 -1 해서 계산
     * @returns 
     */
    IsLockWorldStage(worldIndex : number , stageIndex : number)
    {
        let isAvailableWorld = User.Instance.PrefData.AdventureProgressData.isAvailableWorld(worldIndex);
        if(isAvailableWorld == false){
            ToastMessage.Set(StringTable.GetString(100000628));
            return true;
        }

        if(this.WorldStageInfoList == null || this.WorldStageInfoList.length <= 0)//월드 데이터 없다면
        {
            return true;
        }

        if(this.StageData.world > worldIndex)//현재 월드 인덱스보다 이전 월드 선택시
        {
            return false;
        }

        if(stageIndex - 1 >= this.WorldStageInfoList.length)
        {
            return true;
        }

        if(this.StageData.world == worldIndex)
        {
            return this.WorldStageInfoList[stageIndex -1].isLock;
        }
        else
        {
            let worldList = User.Instance.PrefData.AdventureProgressData.GetWorldStages(worldIndex);
            let availableIndex = this.GetFirstZeroIndex(worldList);//입장 가능 인덱스
            if(availableIndex < 0)//전부 입장 가능
            {
                return false;
            }
            let modifyIndex = stageIndex - 1;
            if(modifyIndex > availableIndex)
            {
                return true;
            }else{
                return false;
            }
        }
    }

    GetFirstZeroIndex(worldList : number[])//첫 0 인덱스 가져오기
    {
        if(worldList == null || worldList.length <= 0){
            return 0;
        }
        let firstZero : boolean = false
        let currentIndex = -1;
        for(let i = 0; i < worldList.length; i++)
        {
            let starCount = worldList[i];
            if(starCount > 0)
            {
                continue;
            }                
            else if(!firstZero)
            {
                firstZero = true;
                currentIndex = i;
            }
            else
            {
                continue;
            }
        }
        return currentIndex;
    }

    GetFirstZeroWorldIndex(worldIndex : number) : number
    {
        let worldList = User.Instance.PrefData.AdventureProgressData.GetWorldStages(worldIndex);
        let availableIndex = this.GetFirstZeroIndex(worldList);//입장 가능 인덱스
        if(availableIndex < 0)//전부 입장 가능
        {
            return worldList.length;
        }
        else{
            return availableIndex + 1;
        }
    }

    GetCurrentOpenStage(): number//현재 열려있는 상태의 인덱스 가져오기
    {
        let currentOpenStage = 1;
        if(this.WorldStageInfoList == null || this.WorldStageInfoList.length <= 0){
            return currentOpenStage;
        }

        for(let i = 0 ; i< this.WorldStageInfoList.length ; i++)
        {
            let worldInfo = this.WorldStageInfoList[i];
            if(worldInfo == null){
                continue;
            }
            let openCheck = worldInfo.IsDetailAlreadyOpen;
            if(openCheck)
            {
                currentOpenStage = (i+1);
                break;
            }
        }
        return currentOpenStage;
    }

    DrawDragon()
    {
        this.battleLine.CheckBattleLine();//서버 기반 데이터 호출
        this.drawTeamDragon();
    }

    onClickStageNewWindow(event : Event , customEventData)//창전환 버전
    {
        let currentNode = event.currentTarget as Node;
        if(currentNode == null){
            return;
        }

        let stageInfoNode = currentNode.getComponent(WorldStageInfo);
        if(stageInfoNode == null){
            return;
        }

        let clickStageIndex = stageInfoNode.StageIndex;
        let params = {stage : clickStageIndex}

        this.SetDataEnemyUI(params);
        this.SetDataRewardUI();
        this.ShowEnemyUI();
    }

    DirectStageNewWindow(stageIndex : number)
    {
        let params = {stage : stageIndex}
        this.SetDataEnemyUI(params);
        this.SetDataRewardUI();
        this.ShowEnemyUI();
    }

    onClickStageRedrawScroll(event : Event , customEventData)//스크롤뷰 확장 버전
    {
        let currentNode = event.currentTarget as Node;
        if(currentNode == null){
            return;
        }

        let stageInfoNode = currentNode.getComponent(WorldStageInfo);
        if(stageInfoNode == null){
            return;
        }

        let isLockCheck = stageInfoNode.isLock;
        let clickStageIndex = stageInfoNode.StageIndex;
        if(isLockCheck)
        {
            this.ToastMessageStageUnlock(clickStageIndex);
            return;
        }
        
        let isAlreadyOpen = stageInfoNode.IsDetailAlreadyOpen;
        this.ReDrawStageInfo(clickStageIndex - 1,  isAlreadyOpen);

        this.SetPosScrollView(clickStageIndex);

        this.worldInfo = (DataManager.GetData("GlobalStageData") as any).stageinfo;
        this.worldInfo.stage = clickStageIndex;

        //this.DrawWorldBG();
    }

    ToastMessageStageUnlock(currentStage : number)
    {
        let world = this.StageData.world;
        let conditionStage = currentStage - 1;
        let emptyCheck = ToastMessage.isToastEmpty();
        if(emptyCheck == false){
            return;
        }

        let stageStr = `stage ${world}-${conditionStage} `;
        let textData = TableManager.GetTable<StringTable>(StringTable.Name).Get(100000216);
        if(textData != null)
        {
            let totalStr = StringBuilder(textData.TEXT, stageStr);
            ToastMessage.Set(totalStr);
        }
    }

    DirectStageRedrawScroll(stageIndex : number)
    {
        let clickStageIndex = stageIndex;
        let isAlreadyOpen = false;
        this.ReDrawStageInfo(clickStageIndex - 1,  isAlreadyOpen);

        this.SetPosScrollView(clickStageIndex, false);

        this.worldInfo = (DataManager.GetData("GlobalStageData") as any).stageinfo;
        this.worldInfo.stage = clickStageIndex;
    }

    SetPosScrollView(clickStageIndex : number, productionSet : boolean = true)
    {
        let layoutComponent= this.worldInfoContent.getComponent(Layout);
        let spacingY = layoutComponent.spacingY;
        let paddingTop =layoutComponent.paddingTop;
        let prefabNodeData = this.worldInfoPrefab.data as Node;
        let uitransformComponent = prefabNodeData.getComponent(UITransform);
        let height = uitransformComponent.height;
        let slotIndex = (clickStageIndex - 1);
        let offsetY = height * slotIndex + paddingTop + spacingY * slotIndex;
        

        let productionTime = 0.1;
        if(productionSet)
        {
            this.ShowProductionScroll(slotIndex, offsetY , productionTime);
        }else{
            this.ShowProductionScroll(slotIndex, offsetY , 0);
        }
    }
    ShowProductionScroll(slotIndex : number, offsetY, productionTime)
    {
        if(slotIndex == 0)
        {
            this.worldInfoScrollview.scrollToOffset(new Vec2(0, 0), productionTime);
        }else{
            this.worldInfoScrollview.scrollToOffset(new Vec2(0, 0), productionTime);
            this.worldInfoScrollview.scrollToOffset(new Vec2(0,offsetY), productionTime);
        }
    }

    directClickStage(stageIndex : number)
    {
        //this.DirectStageNewWindow(stageIndex);

        this.DirectStageRedrawScroll(stageIndex);
    }

    SetDataEnemyUI(data)
    {
        this.nodeMonsterSlotParent.removeAllChildren();

        this.worldInfo = (DataManager.GetData("GlobalStageData") as any).stageinfo;
        this.worldInfo.stage = data.stage;
        let stageInfo = TableManager.GetTable<StageBaseTable>(StageBaseTable.Name).GetByWorldStage(this.worldInfo.world, this.worldInfo.stage)
        let spawnInfo = TableManager.GetTable<MonsterSpawnTable>(MonsterSpawnTable.Name).Get(stageInfo.SPAWN)
        let monsterList : {[index:string] : MonsterBaseData } = {}
        let enemyTotalBp : number = 0
        
        spawnInfo.forEach((element)=>
        {
            if(monsterList[element.MONSTER] == null)
            {
                monsterList[element.MONSTER] = TableManager.GetTable<MonsterBaseTable>(MonsterBaseTable.Name).Get(element.MONSTER)
            }
            enemyTotalBp += element.INF
        })

        //웨이브당 등장 몬스터 중복 제거하여 아이콘 표시
        let keyList = Object.keys(monsterList)
        keyList.forEach(key => 
        {
            let frameClone : Node = instantiate(GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB)["EnemySlot"])
            frameClone.getComponent(EnemyFrame).SetEnemyFrame(monsterList[key].Index as number)

            frameClone.parent = this.nodeMonsterSlotParent
        })

        this.labelStage.string = StringBuilder("{0}-{1}", this.worldInfo.world, data.stage)
        this.labelEnemyBattlePoint.string = enemyTotalBp.toString()
        this.labelReqEnergy.string = StringBuilder("-{0}",stageInfo.COST_VALUE)
    }

    SetDataRewardUI()
    {
        let worldInfo : any = (DataManager.GetData("GlobalStageData") as any).stageinfo
        let stageInfo = TableManager.GetTable<StageBaseTable>(StageBaseTable.Name).GetByWorldStage(worldInfo.world, this.worldInfo.stage)
        this.nodeRewardSlotParent.removeAllChildren();

        if(stageInfo.ACCOUNT_EXP > 0)
        {
            let clone : Node = instantiate(GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB)["ExpSlot"])
            clone.getComponent(ExpFrame).setFrameExpInfo(ExpType.ACCOUNT_EXP, stageInfo.ACCOUNT_EXP)

            clone.parent = this.nodeRewardSlotParent
        }
        
        if(stageInfo.CHAR_EXP > 0)
        {
            let clone : Node = instantiate(GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB)["ExpSlot"])
            clone.getComponent(ExpFrame).setFrameExpInfo(ExpType.CHAR_EXP, stageInfo.CHAR_EXP)

            clone.parent = this.nodeRewardSlotParent
        }

        if(stageInfo.REWARD_GOLD > 0)
        {
            let clone : Node = instantiate(GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB)["item"])
            clone.getComponent(ItemFrame).setFrameCashInfo(0, stageInfo.REWARD_GOLD)

            clone.parent = this.nodeRewardSlotParent
        }

        if(stageInfo.REWARD_ITEM > 0)
        {
            let clone : Node = instantiate(GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB)["item"])
            clone.getComponent(ItemFrame).setFrameItemInfo(stageInfo.REWARD_ITEM, stageInfo.REWARD_ITEM_COUNT)

            clone.parent = this.nodeRewardSlotParent
        }
    }

    ShowEnemyUI()
    {
        if(this.enemyNode.activeInHierarchy == false)
        {
            this.enemyNode.active = true;
        }
    }

    HideEnemyUI()
    {
        if(this.enemyNode.activeInHierarchy == true)
        {
            this.enemyNode.active = false;
        }
    }

    //드래곤 배치에 맞게 그리기
    drawTeamDragon()
    {
        let myTotalBp : number = 0

        this.RemoveAllDragonPrefab();
        this.battleLine.initBattleLine();
        
        let i = 0, l = 0;
        let lineLimit = 2;

        while(l < 3)
        {
            let element = User.Instance.DragonData.GetDragon(this.battleLine.battleLine[l][i])
            if(element != null){
                myTotalBp += element.GetDragonALLStat().INF;
                
                let dragonslot = instantiate(this.prefDragonSlot);
                dragonslot.parent = this.arrDragonParent[l];
                dragonslot.setScale(0.5, 0.5);
                dragonslot.setRotationFromEuler(new Vec3(0,0,75));

                let charcterSlotComp = dragonslot.getComponent(CharacterSlotFrame);
                if(charcterSlotComp != null)
                {
                    charcterSlotComp.setDragonData(Number(element.Tag));//세부 드래곤 데이터 세팅(슬롯 스크립트 추가)
                    charcterSlotComp.setCallback((param)=>{

                        if(this.currentClickDragonTag < 0)//배치클릭을 한 상태인지 체크
                        {
                            return;
                        }

                        let isDeckFull = this.battleLine.isDragonDeckFull();
                        if(isDeckFull){
                            this.onClickReleaseTeam(Number(param));
                            this.onClickRegistTeam(this.currentClickDragonTag);

                            this.initClickFullDragonTag();
                        }
                    });

                    charcterSlotComp.name = element.Tag.toString();
                    this.currentDragonDeckList.push(charcterSlotComp);
                }
            }

            i++;

            if(i >= lineLimit)
            {
                i = 0;
                l++;
            }
        }

        this.labelMyBattlePoint.string = myTotalBp.toString()//투력 라벨 갱신
    }
    
    onClickDragon(event : Event , customEventData)//클릭해서 들어온 데이터 체크
    {
        let currentClickData = event.currentTarget as Node;
        console.log(currentClickData);
        console.log(customEventData);
    }

    ClosePopup()
    {
        //WorldAdventure.SetUIWorldView();
    }    

    onClickBattleStart()
    {
        //최소 1개이상의 드래곤을 등록해야함
        let isEmpty = this.battleLine.isDragonDeckEmpty();
        if(isEmpty)
        {
            let popup = PopupManager.OpenPopup("SystemPopupOK") as SystemPopup;
            popup.setMessage(StringTable.GetString(100000248, "알림"), StringTable.GetString(100000393, "하나 이상의 드래곤이 배치되어야 합니다."));
            return;
        }

        if(this.worldInfo != null) {
            let tempUpdater = NetworkManager.Instance.UIUpdater;
            NetworkManager.Instance.UIUpdater = null;
            NetworkManager.Send('adventure/start', {
                world:this.worldInfo.world,
                diff:this.worldInfo.diff,
                stage:this.worldInfo.stage,
                deck:this.battleLine.battleLine
            }, (jsonData) => {
                if(jsonData['err'] != undefined && jsonData['err'] == 0 && jsonData['state'] != undefined && jsonData['tag'] != undefined && jsonData['wave'] != undefined && jsonData['player'] != undefined && jsonData['enemy'] != undefined) {
                    SoundMixer.DestroyAllClip();

                    this.battleLine.userID = User.Instance.UNO
                    sys.localStorage.removeItem("battleLineInfo")
                    sys.localStorage.setItem("battleLineInfo", JSON.stringify(this.battleLine))
                    
                    let data = {};
                    data['world'] = this.worldInfo.world;
                    data['stage'] = this.worldInfo.stage;
                    data['jsonData'] = jsonData;
                    User.Instance.PrefData.AdventureProgressData.SetCursorDataIndex(this.worldInfo.world ,this.worldInfo.stage);
                    DataManager.DelData('stageStartData');
                    DataManager.AddData('stageStartData', data);
                    this.ClosePopup();
                    SceneManager.SceneChange("battle");
                    return;
                }
                NetworkManager.Instance.UIUpdater = tempUpdater;
            });
        }
    }

    GetUIParent() : Node
    {
        return this.nodePopupUIParent
    }

    RemoveAllDragonPrefab()
    {
        let keyarr = this.arrDragonParent;
        if(keyarr == null || keyarr.length <= 0)
        {
            return;
        }

        keyarr.forEach((key)=>{
            if(key == null){
                return;
            }
            key.removeAllChildren();
        });
    }
    onShowDragonList()
    {
        //현재 소유한 드래곤이 없으면 팀설정 못하게 막음
        if(User.Instance.DragonData.GetAllUserDragons().length == 0){
            //let text = TableManager.GetTable(StringTable.Name).Get(100000393).TEXT;
            let popup = PopupManager.OpenPopup("SystemPopupOK") as SystemPopup;
            popup.setMessage(StringTable.GetString(100000248), StringTable.GetString(100000623));
            return;
        }

        let isVisible = this.isShowDragonList();

        if(isVisible)
        {
            this.onClickReleaseAllDragon();
        }
        else
        {
            if(this.stageDragonList != null)
            {
                this.stageDragonList.onShowList();
                this.RegistDragonListCallback();
                this.stageDragonList.init(this.battleLine.dragons);
                this.changeDragonButtonLabel(true);
            }
        }
    }

    onHideDragonList()//그냥 끌 경우 - 이전 상태 되돌리기?
    {
        if(this.currentClickDragonTag > 0)//현재 교체 상태일경우 - 풀덱에서 교체 드래곤 클릭시
        {
            this.HideAllArrowAnimation();
            return;
        }

        let isEmpty = this.battleLine.isDragonDeckEmpty();
        if(isEmpty)
        {
            let text = TableManager.GetTable(StringTable.Name).Get(100000393).TEXT;
            let popup = PopupManager.OpenPopup("SystemPopupOK") as SystemPopup;
            popup.setMessage(StringTable.GetString(100000248, "알림"), StringTable.GetString(100000393, "하나 이상의 드래곤이 배치되어야 합니다."));
            return;
        }

        let isEqual = this.isChangeDragonList();//이전 데이터와 같은지 체크
        if(isEqual)
        {
            if(this.stageDragonList != null){
                this.stageDragonList.onHideList();
                this.changeDragonButtonLabel(false);
                this.HideAllArrowAnimation();
            }
        }
        else
        {
            this.dragonOverWriteDataPopup();
        }
    }

    RegistDragonListCallback()
    {
        this.stageDragonList.setRegistCallback((param)=>{

            //console.log('regist param : ' + param);
            let tag = Number(param);
            this.onClickRegistTeam(tag);
        });
        this.stageDragonList.setReleaseCallback((param)=>{

            //console.log('release param : ' + param);
            let tag = Number(param);
            this.onClickReleaseTeam(tag);
        });
    }

    dragonOverWriteDataPopup()
    {
        let popup = PopupManager.OpenPopup("SystemPopupOKCANCEL") as SystemPopup;

        popup.setMessage(StringTable.GetString(100000248, "알림"), StringTable.GetString(100000648, "이전 드래곤 배치와 다릅니다. 저장하시겠습니까?"));
        popup.setCallback(()=>
        {
            this.onClickSaveCurrentDragonFormation();
            
            if(this.stageDragonList != null){
                this.stageDragonList.onHideList();
                this.changeDragonButtonLabel(false);
                this.HideAllArrowAnimation();
            }

            popup.ClosePopup();
        },
        () =>
        {
            //이전 데이터 배치로 되돌리기
            this.battleLine.CheckBattleLine();//서버 기반 데이터 호출
            this.drawTeamDragon();

            if(this.stageDragonList != null){
                this.stageDragonList.onHideList();
                this.changeDragonButtonLabel(false);
                this.HideAllArrowAnimation();
            }

            popup.ClosePopup();
        });
    }

    isChangeDragonList() : boolean//현재 세팅된 드래곤 리스트와 최초 로그인 서버에서 가져온 드래곤이 다름(변경내역있음)
    {
        let adventureData = User.Instance.PrefData.AdventureFormationData;
        if(adventureData == null){
            return false;
        }

        let teamArr = adventureData.TeamFormation;//각 탭별로 세팅해줘야함.
        let dragonArr = teamArr[0];//일단 임시로 첫 인덱스 드래곤 리스트로 세팅
        if(dragonArr == null || dragonArr.length <= 0)//서버에서 받아온 드래곤리스트가 미등록(빈값) 일 때
        {
            return false;
        }
        let currentDragonArr = this.battleLine.dragons;//현재 등록된 드래곤 리스트
        if(dragonArr.length != currentDragonArr.length)//길이 체크 먼저
        {
            return false;
        }
        //비교를 하기 위해 오름차순 소팅
        let PrevSortDragonArr = dragonArr.sort((a,b)=>a-b);
        let currentSortDragonArr = currentDragonArr.sort((a,b)=>a-b);

        let isEqual = this.arrayEquals(PrevSortDragonArr,currentSortDragonArr);

        return isEqual;
    }

    arrayEquals(a, b) {
        return Array.isArray(a) &&
          Array.isArray(b) &&
          a.length === b.length &&
          a.every((val, index) => val === b[index]);
    }

    isShowDragonList()
    {
        if(this.stageDragonList != null){
            return this.stageDragonList.isShowList();
        }
        return false;
    }   

    changeDragonButtonLabel(isOpen : boolean)//드래곤 창이 닫힌상태 : 팀설정 버튼, 드래곤 창이 열린 상태 : 전체 해제 버튼 이름 변경
    {
        if(this.teamSettingButtonLable != null)
        {
            let buttonLableStr = "";
            if(isOpen){
                buttonLableStr = TableManager.GetTable<StringTable>(StringTable.Name).Get(100000394).TEXT;
            }else{
                buttonLableStr = TableManager.GetTable<StringTable>(StringTable.Name).Get(100000221).TEXT;
            }
            this.teamSettingButtonLable.string = buttonLableStr;
        }
    }

    onClickReleaseAllDragon()//현재 드래곤배치 전체 삭제
    {
        if(this.battleLine != null)
        {
            if(this.battleLine.dragons.length == 0)
            {
                return;
            }

            this.battleLine.DeleteAllDragon();
            this.drawTeamDragon();

            this.stageDragonList.RefreshList(this.battleLine.dragons);

            this.HideAllArrowAnimation();
        }
    }

    onClickRegistTeam(dragonTag : number)//현재 battle Line 팀 추가
    {
        let isVisible = this.isShowDragonList();
        if(!isVisible){
            return;
        }

        if(this.battleLine.isDragonDeckFull()){
            this.onClickFullDragonDeckProcess(dragonTag);
            return;
        }

        this.battleLine.AddDragon(dragonTag);
        this.drawTeamDragon();

        this.stageDragonList.RefreshList(this.battleLine.dragons);

        // 튜토리얼-106
        if (TutorialManager.GetInstance.IsNowPlayingTutorialByGroup(106)) {
            if (this.battleLine.dragons.length >= 3) {
                TutorialManager.GetInstance.OnTutorialEvent(106, 7);
            }
        }
    }
    onClickReleaseTeam(dragonTag : number)//현재 battle Line 팀 해제
    {
        let isVisible = this.isShowDragonList();
        if(!isVisible){
            return;
        }

        this.battleLine.DeleteDragon(dragonTag);
        this.drawTeamDragon();

        this.stageDragonList.RefreshList(this.battleLine.dragons);
    }

    onClickSaveCurrentDragonFormation()
    {
        this.HideAllArrowAnimation();

        let isEmpty = this.battleLine.isDragonDeckEmpty();
        if(isEmpty)
        {
            let popup = PopupManager.OpenPopup("SystemPopupOK") as SystemPopup;
            popup.setMessage(StringTable.GetString(100000248, "알림"), StringTable.GetString(100000393, "하나 이상의 드래곤이 배치되어야 합니다."));
            return;
        }
        console.log('save send dragon formation');

        let params = {
            teamNo:0,//임시임! - 페이지 인덱스 넘겨야함
            dragons: this.battleLine.dragons,
            items:[0,0,0],//팀장비 갱신 데이터 넘겨야함
        };

        let tempUpdater = NetworkManager.Instance.UIUpdater;
            NetworkManager.Instance.UIUpdater = null;
            NetworkManager.Send('preference/adventure', params, (jsonData) => {
                if(jsonData['err'] != undefined && jsonData['err'] == 0 && jsonData['rs'] >= 0) {
                    
                    User.Instance.PrefData.AdventureFormationData.ClearFomationData(params.teamNo as number);
                    User.Instance.PrefData.AdventureFormationData.SetFormationData(params.teamNo as number, this.battleLine.dragons);

                    let popup = PopupManager.OpenPopup("SystemPopupOK") as SystemPopup;
                    popup.setMessage(StringTable.GetString(100000248, "알림"), StringTable.GetString(100000624, "저장이 완료 되었습니다."));
                    let func = this.SaveComplete.bind(this, popup);
                    popup.setCallback(func, func, func);

                    // 튜토리얼-106
                    if (TutorialManager.GetInstance.IsNowPlayingTutorialByGroup(106)) {
                        TutorialManager.GetInstance.OnTutorialEvent(106, 8);
                    }
                }
                NetworkManager.Instance.UIUpdater = tempUpdater;
            });
    }

    SaveComplete(popup: SystemPopup) {
        if(this.isShowDragonList()) {
            if(this.stageDragonList != null){
                this.stageDragonList.onHideList();
                this.changeDragonButtonLabel(false);
                this.HideAllArrowAnimation();
            }
        }
        popup?.ClosePopup();
    }

    initClickFullDragonTag()
    {
        this.currentClickDragonTag = -1;
        //console.log('this.currentClickDragonTag = -1');
    }
    onClickFullDragonDeckProcess(currentClickTag : number)//드래곤 전부 다 찼을 때 교체 UI 및 처리
    {
        this.currentClickDragonTag = currentClickTag;//임시저장
        this.ShowAllArrowAnimation();//전체 화살표 애니메이션 켜기
        //console.log('this.currentClickDragonTag : ' +this.currentClickDragonTag);
    }

    ShowAllArrowAnimation()
    {
        if(this.currentDragonDeckList == null || this.currentDragonDeckList.length <= 0){
            return;
        }

        this.currentDragonDeckList.forEach((element)=>{
            if(element == null){
                return;
            }
            element.ShowAnimArrowNode();
        });
    }

    HideAllArrowAnimation()
    {
        this.initClickFullDragonTag();

        if(this.currentDragonDeckList == null || this.currentDragonDeckList.length <= 0){
            return;
        }

        this.currentDragonDeckList.forEach((element)=>{
            if(element == null){
                return;
            }
            element.HideAnimArrowNode();
        });
    }

    onClickAutoStart(event : Event , customEventData)
    {
        let countCheck = DataManager.GetData("AutoAdventureCount") as number;//카운트 데이터 세팅하기
        //여기서 시작할때 한번 깎고, BattleRewardPopup에서 다시하기 활성화 시키면서 체크
        let modifyCount = countCheck - 1;
        if(DataManager.GetData("AutoAdventureCount") != null){
            DataManager.DelData("AutoAdventureCount");
        }
        DataManager.AddData("AutoAdventureCount", modifyCount);

        this.onClickBattleStart();
    }

    /**
     * //업데이트 기대해달라는 문구
     */
     onClickExpectGameAlphaUpdate()
     {
         let emptyCheck = ToastMessage.isToastEmpty();
         if(emptyCheck == false){
             return;
         }
         
         let textData = TableManager.GetTable<StringTable>(StringTable.Name).Get(100000326);
         if(textData != null)
         {
             ToastMessage.Set(textData.TEXT);
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


import { _decorator, Component, Node, SpriteFrame, Label, Sprite, instantiate, ScrollView, Prefab } from 'cc';
import { ItemGroupTable } from '../Data/ItemTable';
import { MonsterBaseData } from '../Data/MonsterData';
import { MonsterBaseTable, MonsterSpawnTable } from '../Data/MonsterTable';
import { StageBaseData } from '../Data/StageData';
import { StageBaseTable } from '../Data/StageTable';
import { TableManager } from '../Data/TableManager';
import { GameManager } from '../GameManager';
import { ResourceManager, ResourcesType } from '../ResourceManager';
import { StringBuilder } from '../Tools/SandboxTools';
import { TutorialManager } from '../Tutorial/TutorialManager';
import { AutoAdventurePopup } from './AutoAdventurePopup';
import { PopupManager } from './Common/PopupManager';
import { EnemyFrame } from './ItemSlot/EnemyFrame';
import { ExpFrame, ExpType } from './ItemSlot/ExpFrame';
import { ItemFrame } from './ItemSlot/ItemFrame';
import { StagePrepareComponent } from './StagePrepareComponent';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = WorldStageInfo
 * DateTime = Tue May 03 2022 13:46:15 GMT+0900 (대한민국 표준시)
 * Author = jinwonjun
 * FileBasename = WorldStageInfo.ts
 * FileBasenameNoExtension = WorldStageInfo
 * URL = db://assets/Scripts/UI/WorldStageInfo.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */

const starMaxCount = 3;
const constraintSlotCount = 5;//스크롤 슬롯 제한 갯수 보다 작거나 같으면 스크롤 끄기

export enum eWorldImage
{
    none ='',
    dusk = '_DUSK',
    night = '_NIGHT',
    max = 'max',
}

@ccclass('WorldStageInfo')
export class WorldStageInfo extends Component {
    
    @property(SpriteFrame)
    starSpriteList : SpriteFrame[] = [];

    @property(Node)
    starSpriteTargetNodeList : Node[] = [];

    @property(Label)
    stagelabel : Label = null;

    @property(Node)
    starNode : Node = null;
    @property(Node)
    lockNode : Node = null;

    @property(Node)
    autoAdventureNode : Node = null;

    @property(ScrollView)
    enemyScrollview : ScrollView = null;
    @property(Node)
    enemyContentNode : Node = null;
    @property(Label)
    labelEnemyBattlePoint : Label = null;
    @property(Label)
    labelReqEnergy : Label = null;
    
    @property(ScrollView)
    rewardScrollview : ScrollView = null;
    @property(Node)
    rewardContentNode : Node = null;

    @property(Sprite)
    stageIconSprite : Sprite = null;

    @property(SpriteFrame)
    unLockIconList : SpriteFrame[] = [];
    
    @property(SpriteFrame)
    lockIconList : SpriteFrame[] = [];

    @property(Node)
    bgList : Node[] = [];


    isLock : boolean = false;
    public get IsLock() :boolean{return this.isLock;}

    worldIndex : number = 0;
    
    stageIndex : number = 0;
    public get StageIndex() : number {return this.stageIndex;}

    isDetailAlreadyOpen : boolean = false;
    public get IsDetailAlreadyOpen() : boolean {return this.isDetailAlreadyOpen;}

    parent : StagePrepareComponent  = null;
    private uiResources: any = null;
    private rewardTable: ItemGroupTable = null;

    SetData(worldNumber : number , stageNumber : number , starCount : number, isLock : boolean, isAlreadyOpen? : boolean, isDetailInfo? : boolean , parent? : StagePrepareComponent)
    {
        this.worldIndex = worldNumber;
        this.stageIndex = stageNumber;
        this.isLock = isLock;
        this.isDetailAlreadyOpen = isAlreadyOpen;
        this.parent = parent;

        this.setVisibleState(isLock);
        this.setStarCount(starCount, isLock);
        this.setStageLabel(worldNumber , stageNumber);
        this.setDesignByWorldMapName();
        
        if(isDetailInfo)
        {
            this.setEnemyScrollview();
            this.setRewardScrollview();
        }
    }

    setDesignByWorldMapName()
    {
        let table = TableManager.GetTable<StageBaseTable>(StageBaseTable.Name);
        let data: StageBaseData = null;
        if(table != null) {
            data = table.GetByWorldStage(this.worldIndex, this.stageIndex);
            let currentMapName = data.IMAGE;
            let keys = Object.keys(eWorldImage);

            let tempList : string[] = [];

            //키값 기준 통리스트 만들기
            keys.forEach(element=>{
                let tempString = `STAGE_${this.worldIndex}${eWorldImage[element]}`;
                tempList.push(tempString);
            })

            let checkIndex = 0;

            for(let i = 0 ; i< tempList.length ; i++)
            {
                let keyID = tempList[i];
                if(keyID == currentMapName){
                    checkIndex = i;
                    break;
                }
            }

            //아이콘 세팅
            if(this.stageIconSprite != null)
            {
                if(this.isLock)
                {
                    if(this.lockIconList != null && this.lockIconList.length > 0 && this.lockIconList.length > checkIndex)
                    {
                        this.stageIconSprite.spriteFrame = this.lockIconList[checkIndex];
                    }
                }
                else
                {
                    if(this.unLockIconList != null && this.unLockIconList.length > 0 && this.unLockIconList.length > checkIndex)
                    {
                        this.stageIconSprite.spriteFrame = this.unLockIconList[checkIndex];
                    }
                }    
            }

            //bg 세팅
            if(this.bgList != null && this.bgList.length > 0 && this.bgList.length > checkIndex)
            {
                for(let j = 0; j < this.bgList.length ; j++)
                {
                    if(checkIndex == j && !this.isLock)
                    {
                        this.bgList[j].active = true;
                    }else{
                        this.bgList[j].active = false;
                    }
                }
            }
        }
        
    }

    setStageLabel(worldNumber : number , stageNumber : number)
    {
        if(this.stagelabel != null){
            this.stagelabel.string = `stage ${worldNumber}-${stageNumber} `;
        }
    }

    setStarCount(StarCount : number, isLock : boolean)
    {
        if(isLock == true)
        {
            for(let i = 0 ; i < starMaxCount ; i++)
            {
                this.starSpriteTargetNodeList[i].getComponentInChildren(Sprite).node.active = false;
            }
            return;
        }

        for(let i = 0 ; i < starMaxCount ; i++)
        {
            this.starSpriteTargetNodeList[i].getComponentInChildren(Sprite).node.active = i < StarCount;
        }

        if(StarCount >= 1){
            if(this.autoAdventureNode != null){
                this.autoAdventureNode.active = true;
            }
        }else{
            if(this.autoAdventureNode != null){
                this.autoAdventureNode.active = false;
            }
        }
    }

    setVisibleState(isLock : boolean)
    {
        this.lockNode.active = isLock;
        this.starNode.active = true;
    }

    setEnemyScrollview()
    {
        if(this.uiResources == null) {
            this.uiResources = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB);
        }
        this.enemyContentNode.removeAllChildren();
        let stageInfo = TableManager.GetTable<StageBaseTable>(StageBaseTable.Name).GetByWorldStage(this.worldIndex, this.stageIndex);
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
            let frameClone : Node = instantiate(this.uiResources["EnemySlot"])
            frameClone.getComponent(EnemyFrame).SetEnemyFrame(monsterList[key].Index as number)

            frameClone.parent = this.enemyContentNode;
        })

        this.labelEnemyBattlePoint.string = enemyTotalBp.toString()
        this.labelReqEnergy.string = StringBuilder("-{0}",stageInfo.COST_VALUE)

        let checkSlotCount = this.enemyContentNode.children.length;
        if(checkSlotCount <= constraintSlotCount)
        {
            this.enemyScrollview.enabled = false;
        }
    }

    setRewardScrollview()
    {
        if(this.uiResources == null) {
            this.uiResources = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB);
        }
        if(this.rewardTable == null) {
            this.rewardTable = TableManager.GetTable<ItemGroupTable>(ItemGroupTable.Name);
        }
        let stageInfo = TableManager.GetTable<StageBaseTable>(StageBaseTable.Name).GetByWorldStage(this.worldIndex, this.stageIndex);
        this.rewardContentNode.removeAllChildren();

        if(this.uiResources != null) {
            if(stageInfo.ACCOUNT_EXP > 0)
            {
                let clone : Node = instantiate(this.uiResources["ExpSlot"])
                clone.getComponent(ExpFrame).setFrameExpInfo(ExpType.ACCOUNT_EXP, stageInfo.ACCOUNT_EXP)
    
                clone.parent = this.rewardContentNode;
            }
            
            if(stageInfo.CHAR_EXP > 0)
            {
                let clone : Node = instantiate(this.uiResources["ExpSlot"])
                clone.getComponent(ExpFrame).setFrameExpInfo(ExpType.CHAR_EXP, stageInfo.CHAR_EXP)
    
                clone.parent = this.rewardContentNode;
            }
    
            if(stageInfo.REWARD_GOLD > 0)
            {
                let clone : Node = instantiate(this.uiResources["item"])
                clone.getComponent(ItemFrame).setFrameCashInfo(0, stageInfo.REWARD_GOLD)
    
                clone.parent = this.rewardContentNode;
            }
    
            if(stageInfo.REWARD_ITEM > 0)
            {
                let rewardDatas = this.rewardTable.Get(stageInfo.REWARD_ITEM);
                if(rewardDatas != null) {
                    const dataCount = rewardDatas.length;

                    for (let index = 0; index < dataCount; index++) {
                        const element = rewardDatas[index];
                        if(element == null) {
                            continue;
                        }
                        let clone : Node = instantiate(this.uiResources["item"]);
                        clone.getComponent(ItemFrame).setFrameItemInfo(element.VALUE, element.NUM);
            
                        clone.parent = this.rewardContentNode;
                    }
                }
            }
        }

        let checkSlotCount = this.rewardContentNode.children.length;
        if(checkSlotCount <= constraintSlotCount)
        {
            this.rewardScrollview.enabled = false;
        }
    }

    onClickBattleStart()
    {
        if(this.parent != null){
            this.parent.onClickBattleStart();
        }

        // 튜토리얼-106
        if (TutorialManager.GetInstance.IsNowPlayingTutorialByGroup(106)) {
            TutorialManager.GetInstance.OffTutorialEvent();
        }
    }

    onClickAutoAdventureButton()
    {
        let params = { 
            world : this.worldIndex,
            stage : this.stageIndex,
        }
        PopupManager.OpenPopup("AutoAdventurePopup", true, params) as AutoAdventurePopup
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

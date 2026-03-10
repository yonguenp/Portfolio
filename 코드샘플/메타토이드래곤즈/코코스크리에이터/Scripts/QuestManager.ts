//퀘스트

import { IManagerBase } from "sb";
import { BuildingBaseTable } from "./Data/BuildingTable";
import { ItemBaseTable } from "./Data/ItemTable";
import { ProductTable } from "./Data/ProductTable";
import { QuestData, QuestTriggerData } from "./Data/QuestData";
import { QuestTable, QuestTriggerTable } from "./Data/QuestTable";
import { StageBaseTable } from "./Data/StageTable";
import { StringTable } from "./Data/StringTable";
import { TableManager } from "./Data/TableManager";
import { SceneManager } from "./SceneManager";
import { SoundMixer, SOUND_TYPE } from "./SoundMixer";
import { DataManager } from "./Tools/DataManager";
import { GetType, ObjectCheck, StringBuilder, Type } from "./Tools/SandboxTools";
import { TutorialManager } from "./Tutorial/TutorialManager";
import { PopupManager } from "./UI/Common/PopupManager";
import { QuestCanister } from "./UI/QuestCanister";
import { UICanvas } from "./UI/UICanvas";
import { WorldAdventure } from "./WorldAdventure";

export enum eQuestType 
{
    MAIN = 0,
    SUB,
    DAILY,
    EVENT
}

enum eQuestCondType 
{
    CHECK_DRAGON,
    STAGE_COMPLETE,
    BUILD,
    GAIN_ITEM,
    DRAGON_LEVEL,
    EXTERIOR_BUILD,
    GAIN_DOGER,
    SKILL_LEVEL,
    BUILD_START,
    PRODUCE,
    CHECKITEM,
    EXP_BUILD_LEVEL
}

const stringToQuestCond : string[] = 
[
    "CHECK_DRAGON", 
    "STAGE_COMPLETE", 
    "BUILD", 
    "GAIN_ITEM", 
    "DRAGON_LEVEL", 
    "EXTERIOR_BUILD", 
    "GAIN_DOGER", 
    "SKILL_LEVEL",
    "BUILD_START",
    "PRODUCE",
    "CHECKITEM",
    "EXP_BUILD_LEVEL"
];

export class QuestObject
{
    qID : number;
    qData : { cond : number, value : number, target : number, type_key : string|number }[] = [];

    designData : QuestData;

    Init(id : number)
    {
        this.qID = id;
        this.designData = TableManager.GetTable(QuestTable.Name).Get(this.qID);
    }

    AddCondition(cid : number, value : number)
    {
        let target = TableManager.GetTable<QuestTriggerTable>(QuestTriggerTable.Name).Get(this.designData.CONDITION_GROUP).find(value => value.KEY == cid);
        if(target == null || target == undefined){
            return;
        }
        this.qData.push({cond : cid, value : value, target : target.TYPE_KEY_VALUE, type_key : target.TYPE_KEY });
    }

    GetIcon() : string
    {
        let iconName = "q"+this.designData.ICON+"_";

        switch(this.designData.QTYPE)
        {
            case eQuestType.MAIN:
                iconName += "main";
                break;
            case eQuestType.SUB:
                iconName += "sub";
                break;  
            case eQuestType.DAILY:
                break;
            case eQuestType.EVENT:
                break;
        }

        return iconName;
    }

    GetProgress() : { cur : number, target : number }
    {
        if(this.qData.length < 1)
            return null;

        if(this.qData.length == 1)
        {
            return { cur : this.qData[0].value, target : this.qData[0].target };
        }   
        else
        {
            let cur = 0;
            this.qData.forEach( cElement => 
            {
                if(cElement.value >= cElement.target)
                    cur++;
            });

            return { cur : cur, target : this.qData.length }   
        }
    }

    UpdateCondition(cData : { cid : number, value : number}[])
    {
        cData.forEach(uElement => 
        {
            this.qData.forEach(cElement =>
            {
                if(cElement.cond == uElement.cid)
                {
                    cElement.value = uElement.value
                }  
            })
        })
    }

    GetQuestDesc() : string
    {
        let stringTable = TableManager.GetTable<StringTable>(StringTable.Name);
        let triggerTable = TableManager.GetTable<QuestTriggerTable>(QuestTriggerTable.Name);
        let qTable = TableManager.GetTable<QuestTable>(QuestTable.Name);
        let desc : string = "";
        let keyDesc : number = qTable.Get(this.qID).NOTI;

        this.qData.forEach((qElement, index) => 
        {
            if(index > 0)
                desc += "\n";
            
            let targetName : string = "";
            let cond = triggerTable.GetTrigger(qElement.cond);

            switch(stringToQuestCond.indexOf(cond.TYPE))
            {
                case eQuestCondType.CHECK_DRAGON:
                    desc += StringBuilder(stringTable.Get(keyDesc).TEXT, qElement.value, qElement.target);
                    break;

                case eQuestCondType.BUILD:
                    targetName = stringTable.Get(TableManager.GetTable<BuildingBaseTable>(BuildingBaseTable.Name).Get(qElement.type_key)._NAME).TEXT;

                    desc += StringBuilder(stringTable.Get(keyDesc).TEXT, targetName, qElement.value, qElement.target);
                    break;
                case eQuestCondType.DRAGON_LEVEL:
                    desc += StringBuilder(stringTable.Get(keyDesc).TEXT, qElement.type_key, qElement.value, qElement.target);
                    break;

                case eQuestCondType.EXTERIOR_BUILD:
                    desc += StringBuilder(stringTable.Get(keyDesc).TEXT, qElement.type_key, qElement.value, qElement.target);
                    break;

                case eQuestCondType.GAIN_DOGER:
                    desc += StringBuilder(stringTable.Get(keyDesc).TEXT, qElement.target, qElement.value, qElement.target);
                    break;

                case eQuestCondType.GAIN_ITEM:
                    targetName = stringTable.Get(TableManager.GetTable<ItemBaseTable>(ItemBaseTable.Name).Get(qElement.type_key)._NAME).TEXT;

                    desc += StringBuilder(stringTable.Get(keyDesc).TEXT, targetName, qElement.value, qElement.target);
                    break;

                case eQuestCondType.SKILL_LEVEL:
                    desc += StringBuilder(stringTable.Get(keyDesc).TEXT, qElement.target, qElement.value, qElement.target);
                    break;

                case eQuestCondType.STAGE_COMPLETE:
                    let stageData = TableManager.GetTable<StageBaseTable>(StageBaseTable.Name).Get(qElement.type_key);
                    targetName = `${stageData.WORLD}-${stageData.STAGE}`;

                    desc += StringBuilder(stringTable.Get(keyDesc).TEXT, targetName, qElement.value, qElement.target);
                    break;

                case eQuestCondType.BUILD_START:
                    targetName = stringTable.Get(TableManager.GetTable<BuildingBaseTable>(BuildingBaseTable.Name).Get(qElement.type_key)._NAME).TEXT;

                    desc += StringBuilder(stringTable.Get(keyDesc).TEXT, targetName, qElement.value, qElement.target);
                    break;

                case eQuestCondType.PRODUCE:
                    targetName = stringTable.Get(TableManager.GetTable<ItemBaseTable>(ItemBaseTable.Name).Get(qElement.type_key)._NAME).TEXT;

                    desc += StringBuilder(stringTable.Get(keyDesc).TEXT, targetName, qElement.value, qElement.target);
                    break;

                case eQuestCondType.CHECKITEM:
                    targetName = stringTable.Get(TableManager.GetTable<ItemBaseTable>(ItemBaseTable.Name).Get(qElement.type_key)._NAME).TEXT;

                    desc += StringBuilder(stringTable.Get(keyDesc).TEXT, targetName, qElement.value, qElement.target);
                    break;

                case eQuestCondType.EXP_BUILD_LEVEL:
                    desc += StringBuilder(stringTable.Get(keyDesc).TEXT, qElement.type_key, qElement.value, qElement.target);
                    break;

                default:
                    desc = "";
                    console.log("해당 퀘스트 타입 찾을 수 없음 : ", cond.TYPE);
                    break;
            }
        });

        return desc;
    }

    GetQuestSubjectTitle() : string
    {
        return StringTable.GetString(this.designData.SUBJECT , "");
    }
}

export class QuestManager implements IManagerBase
{
    public static Name : string = "QuestManager";
    private static instance : QuestManager = null
    
    // property
    public static qList : {} = {};
    private static  achiveList : Array<number> = null;
    public static isGetNew : boolean = false;
    private static initial : boolean = false;
    private static contentLayout : QuestCanister = null;
    private static autoCheck : boolean = true;

    public static get AutoCheck() : boolean
    {
        return this.autoCheck;
    }

    public static set AutoCheck(check : boolean)
    {
        this.autoCheck = check;
        if(QuestManager.autoCheck)
        {
            QuestManager.contentLayout?.CheckQuestState();
        }
    }

    public static get IsInitial() : boolean
    {
        return this.initial;
    }
    public static nList : number[] = [];

    //data
    private questTable : QuestTable;
    private triggerTable : QuestTriggerTable;

    public static get ContentLayout() : QuestCanister
    {
        return this.contentLayout;
    }

    public static set ContentLayout(content : QuestCanister)
    {
        this.contentLayout = content;
    }

    public static get Instance() 
    {
        if(QuestManager.instance == null) 
        {
            return QuestManager.instance = new QuestManager();
        }
        return QuestManager.instance;
    }
    GetManagerName() : string { return QuestManager.Name;}
    Update(deltaTime: number): void { }

    Init()
    {
        if(QuestManager.qList !== null && Object.keys(QuestManager.qList).length > 0)
        {
            delete QuestManager.qList
        }

        if(QuestManager.achiveList !== null && QuestManager.achiveList.length > 0)
        {
            delete QuestManager.achiveList
        }

        QuestManager.qList = {};
        QuestManager.achiveList = [];
        this.questTable = TableManager.GetTable<QuestTable>(QuestTable.Name)
        this.triggerTable = TableManager.GetTable<QuestTriggerTable>(QuestTriggerTable.Name);
        QuestManager.initial = true;
    }

    public static GetQuestList() : string[]
    {
        return Object.keys(QuestManager.qList);
    }

    public GetQuest(qID) : QuestObject
    {
        if(QuestManager.qList != null&& Object.keys(QuestManager.qList).length > 0 && QuestManager.qList[qID] != null)
        {
            return QuestManager.qList[qID]
        }

        return null;
    }

    public static AddAchive(qid : number) : void
    {
        this.achiveList.push(qid);
    }

    static NewQuestCheck(jsonData)
    {
        if(ObjectCheck(jsonData, "new") && GetType(jsonData["new"]) == Type.Array)
        {
            this.nList = jsonData["new"];
            this.nList.sort((a, b) => b - a);
        }
    }

    static ProgressUpdate(jsonData)
    {
        let updates : { quest : number, data : string }[] = jsonData['data'];

        updates.forEach(qElement =>
        {
            let cDatas : { cid : number, value : number}[] = JSON.parse(qElement.data);
            
            if(QuestManager.qList[qElement.quest] == null)
            {    
                let quest : QuestObject = new QuestObject();
                quest.Init(qElement.quest);
                
                cDatas.forEach(cElement => 
                {
                    quest.AddCondition(cElement.cid, cElement.value);
                });

                this.qList[qElement.quest] = quest;
            }
            else
                (QuestManager.qList[qElement.quest] as QuestObject).UpdateCondition(cDatas);
        })

        this.ContentLayout?.QuestContentUpdate();
    }

    static SetData(jsonData)
    {
        if(ObjectCheck(jsonData, 'current') && GetType(jsonData['current']) == Type.Object)
        {
            let qKeys = Object.keys(jsonData['current']);        

            qKeys.forEach(qElement => 
            {
                let obj : QuestObject = new QuestObject();
                let cKeys = Object.keys(jsonData['current'][qElement]);
                let qId = Number(qElement);

                obj.Init(qId);
                cKeys.forEach(cElement => 
                {
                    obj.AddCondition(Number(cElement), Number(jsonData['current'][qElement][cElement]));
                });
                
                QuestManager.qList[qId] = obj;
            });
        }

        if(ObjectCheck(jsonData, 'accomplish') && GetType(jsonData['accomplish']) == Type.Array)
        {
            jsonData['accomplish'].forEach(element => 
            {
                QuestManager.achiveList.push(Number(element));
            });
        }
    }

    CheckQuestCondition(qID) : boolean
    {
        //퀘스트 완료 검사
        let quest : QuestObject = QuestManager.qList[qID];
        let cGroup = this.triggerTable.Get(quest.designData.CONDITION_GROUP);
        
        if(!quest || !cGroup || cGroup.length == 0)
        {
            return false;
        }

        quest.qData.forEach(cElement => 
        {
            cGroup.forEach(dElement =>
            {
                if(cElement.value < dElement.TYPE_KEY_VALUE)
                {
                    return false;
                }
            });
        });

        return true;
    }

    //호출 위치
    /*
    마을 또는 월드 진입 직후
    퀘스트 내용 확인 직후
    퀘스트 보상 받기
    */
    static GetAcceptableQuest() : {arr : number[], length : number}
    {
        let quests : number[] = [];

        QuestManager.instance.questTable.GetAll().forEach(qElement => 
        {
            if( QuestManager.achiveList.indexOf(Number(qElement.Index)) >= 0 || ObjectCheck(QuestManager.qList, qElement.Index) )
                return;
                        
            let ret = true;
    
            QuestManager.instance.triggerTable.Get(QuestManager.instance.questTable.Get(qElement.Index).START_GROUP).forEach(cElement =>
            {
                switch(cElement.TYPE)
                {
                    case "TUTORIAL" :
                        if(TutorialManager.achiveList.indexOf(Number(cElement.TYPE_KEY)) < 0)
                        {
                            ret = false;
                        }
                        break;

                    case "CLEAR_QUEST" :
                        if(QuestManager.achiveList.indexOf(Number(cElement.TYPE_KEY)) < 0)
                        {
                            ret = false;
                        }
                        break;
                    default : 
                        ret = false;
                    break;
                }
            });

            if(ret)
                quests.push(Number(qElement.Index));
        })

        if(quests.length > 0)
            return {arr : quests, length : quests.length};

        return {arr : [], length : 0};
    }

    static GetDone() : number
    {
        let quests : number[] = [];

        let keys = Object.keys(QuestManager.qList);
        keys.forEach(key => 
        {
            let quest = (QuestManager.qList[key] as QuestObject).GetProgress();
            
            if(quest.cur >= quest.target)
            {
                quests.push(Number(key));
            }
        })

        if(quests.length > 0)
            return quests[0];
        return -1;
    }

    static onClickQuestPopup(qinfo)
    {
        //PopupManager.AllClosePopup();

        console.log("qinfo", qinfo);
        PopupManager.OpenPopup("questInfoPopup", true, qinfo);

        UICanvas.RefreshSideButton();
    }

    public static RemoveQuest(qid)
    {
        if( this.qList[qid] != null )
        {
            this.contentLayout.RemoveQuestIcon(qid);
            delete this.qList[qid];
        } 
    }

    public static onClickGotoTarget(questtriggerData : QuestTriggerData)
    {
        let type = questtriggerData.TYPE;
        switch(stringToQuestCond.indexOf(type))
            {
                case eQuestCondType.CHECK_DRAGON:
                    SoundMixer.DestroyAllClip();
                    SceneManager.SceneChange("gacha", () => { 
                    SoundMixer.PlayBGM(SOUND_TYPE.BGM_VILLAGE);
                    TutorialManager.GetInstance.OnTutorialEvent(101,4);
                    });
                    break;
                case eQuestCondType.BUILD:
                    {
                        let targetSceneName = "game";
                        let isGameScene = QuestManager.isTargetScene(targetSceneName);
                        if(!isGameScene){
                            QuestManager.ChangeScene(targetSceneName, ()=>{
                                if(questtriggerData.TYPE_KEY == 'dozer' || questtriggerData.TYPE_KEY == 'subway' ||questtriggerData.TYPE_KEY == 'travel')
                                {
                                    PopupManager.OpenPopup("MainPopup", true, {index: 1});
                                }else{
                                    PopupManager.OpenPopup("MainPopup", true, {index: 2});
                                }    
                            });
                        }
                        else{
                            if(questtriggerData.TYPE_KEY == 'dozer' || questtriggerData.TYPE_KEY == 'subway' ||questtriggerData.TYPE_KEY == 'travel')
                            {
                                PopupManager.OpenPopup("MainPopup", true, {index: 1});
                            }else{
                                PopupManager.OpenPopup("MainPopup", true, {index: 2});
                            }
                        }
                    }
                    break;
                case eQuestCondType.DRAGON_LEVEL:
                    {
                        let targetSceneName = "game";
                        let isGameScene = QuestManager.isTargetScene(targetSceneName);
                        if(!isGameScene){
                            QuestManager.ChangeScene(targetSceneName, ()=>{
                                PopupManager.OpenPopup("DragonManagePopup", true, {index: -1});
                            });
                        }else{
                            PopupManager.OpenPopup("DragonManagePopup", true, {index: -1});
                        }
                    }
                    break;
                case eQuestCondType.EXTERIOR_BUILD:
                    {
                        let targetSceneName = "game";
                        let isGameScene = QuestManager.isTargetScene(targetSceneName);
                        if(!isGameScene){
                            QuestManager.ChangeScene(targetSceneName, ()=>{
                                PopupManager.OpenPopup("MainPopup", true, {index: 0});
                            });
                        }else{
                            PopupManager.OpenPopup("MainPopup", true, {index: 0});
                        }
                    }
                    
                    break;
                case eQuestCondType.GAIN_DOGER:
                    {
                        let targetSceneName = "game";
                        let isGameScene = QuestManager.isTargetScene(targetSceneName);
                        if(!isGameScene){
                            QuestManager.ChangeScene(targetSceneName,()=>{
                                PopupManager.OpenPopup("MainPopup", true, {index: 1});
                            });
                        }else{
                            PopupManager.OpenPopup("MainPopup", true, {index: 1});
                        }
                    }
                    break;
                case eQuestCondType.GAIN_ITEM:
                    {
                        let targetSceneName = "game";
                        let isGameScene = QuestManager.isTargetScene(targetSceneName);
                        if(!isGameScene){
                            QuestManager.ChangeScene(targetSceneName,()=>{
                                PopupManager.OpenPopup("MainPopup", true, {index: 2});    
                            });
                        }else{
                            PopupManager.OpenPopup("MainPopup", true, {index: 2});    
                        }
                    }
                    
                    break;
                case eQuestCondType.SKILL_LEVEL:
                    {
                        let targetSceneName = "game";
                        let isGameScene = QuestManager.isTargetScene(targetSceneName);
                        if(!isGameScene){
                            QuestManager.ChangeScene(targetSceneName, ()=>{
                                PopupManager.OpenPopup("DragonManagePopup", true, {index: -1});
                            });
                        }else{
                            PopupManager.OpenPopup("DragonManagePopup", true, {index: -1});
                        }
                    }
                    break;
                case eQuestCondType.STAGE_COMPLETE:
                    {
                        let stageData = TableManager.GetTable<StageBaseTable>(StageBaseTable.Name).Get(questtriggerData.TYPE_KEY);
                        let world = stageData.WORLD;
                        let stage = stageData.STAGE;
    
                        let params =
                        {
                            world : world,
                            stage : stage,
                        }

                        let targetSceneName = "WorldAdventure";
                        let isWorldScene = QuestManager.isTargetScene(targetSceneName);
                        if(!isWorldScene)
                        {
                            DataManager.AddData("QuestDirectWorld" , params);
                            QuestManager.ChangeScene(targetSceneName);
                        }else{
                            DataManager.AddData("QuestDirectWorld" , params);
                            WorldAdventure.RefreshUI();
                        }
                    }
                    break;
                case eQuestCondType.BUILD_START:
                    {
                        let targetSceneName = "game";
                        let isGameScene = QuestManager.isTargetScene(targetSceneName);
                        if(!isGameScene){
                            QuestManager.ChangeScene(targetSceneName, (()=>{
                                PopupManager.OpenPopup("MainPopup", true, {index: 2});    
                            }));
                        }else{
                            PopupManager.OpenPopup("MainPopup", true, {index: 2});    
                        }
                    }
                    break;
                case eQuestCondType.PRODUCE://다이렉트 페이지 세팅하기
                    {
                        let targetSceneName = "game";
                        let isGameScene = QuestManager.isTargetScene(targetSceneName);
                        if(!isGameScene){
                            QuestManager.ChangeScene(targetSceneName,()=>{
                                PopupManager.OpenPopup("MainPopup", true, {index: 2});    
                            });
                        }else{
                            PopupManager.OpenPopup("MainPopup", true, {index: 2});    
                        }
                    }
                    break;
                case eQuestCondType.CHECKITEM:
                    {
                        let materialKey = Number(questtriggerData.TYPE_KEY);//획득 해야되는 재료 키값
                        let buildingIndex = TableManager.GetTable<ProductTable>(ProductTable.Name).GetBuildingGroupByProductItem(materialKey);
                        if(buildingIndex != "")
                        {
                            if(DataManager.GetData("ProductLayerTap") != null)
                            {
                                DataManager.DelData("ProductLayerTap")
                            }
                            DataManager.AddData("ProductLayerTap", buildingIndex);
                        }
                        let targetSceneName = "game";
                        let isGameScene = QuestManager.isTargetScene(targetSceneName);
                        if(!isGameScene){
                            QuestManager.ChangeScene(targetSceneName,()=>{
                                PopupManager.OpenPopup("MainPopup", true, {index: 3});    
                            });
                        }else{
                            PopupManager.OpenPopup("MainPopup", true, {index: 3});    
                        }
                    }
                    break;
                case eQuestCondType.EXP_BUILD_LEVEL:
                    break;
                default:
                    break;
            }
    }

    static isTargetScene(targetSceneName : string)
    {
        let checkSceneName = SceneManager.Instance.GetSceneTargetName();
        if(checkSceneName == targetSceneName)
        {
            return true;
        }else{
            return false;
        }
    }

    static ChangeScene(targetSceneName : string, func? : (CustomEventData? : string) => void)
    {
        SoundMixer.DestroyAllClip();
        SceneManager.SceneChange(targetSceneName, () => 
        {
            if(func != null && func != undefined)
            {
                func();
            }
            
            if(targetSceneName != "game")
            {
                SoundMixer.PlayBGM(SOUND_TYPE.BGM_VILLAGE)   
            }
        });
    }
}
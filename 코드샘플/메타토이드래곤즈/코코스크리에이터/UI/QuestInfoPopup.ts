import { _decorator, Node, Label, Button, ScrollView, instantiate, EventHandler } from 'cc';
import { QuestTriggerTable } from '../Data/QuestTable';
import { StringTable } from '../Data/StringTable';
import { TableManager } from '../Data/TableManager';
import { GameManager } from '../GameManager';
import { NetworkManager } from '../NetworkManager';
import { QuestManager } from '../QuestManager';
import { ResourceManager, ResourcesType } from '../ResourceManager';
import { TimeObject } from '../Time/ITimeObject';
import { TimeManager } from '../Time/TimeManager';
import { GetChild } from '../Tools/SandboxTools';
import { TutorialManager } from '../Tutorial/TutorialManager';
import { Popup } from './Common/Popup';
import { ExpFrame, ExpType } from './ItemSlot/ExpFrame';
import { ItemFrame } from './ItemSlot/ItemFrame';
import { ToastMessage } from './ToastMessage';
const { ccclass, property } = _decorator;
 
const qDone = "qMark_Done";

@ccclass('QuestInfoPopup')
export class QuestInfoPopup extends Popup
{
    @property(Node)
    tutorialButtonNode: Node = null;

    private qid : number = 0;
    private done : boolean = false;

    Init(popupData: any)
    {
        super.Init(popupData);

        this.qid = popupData.quest;

        let questInfo = QuestManager.Instance.GetQuest(this.qid);
        let qProgress = questInfo.GetProgress();

        if(qProgress == null)
            this.ClosePopup();

        //임시 퀘스트 내용
        GetChild(this.node, ["body", "labelSubject"]).getComponent(Label).string = questInfo.GetQuestSubjectTitle();
        GetChild(this.node, ["body", "labelBody"]).getComponent(Label).string = questInfo.GetQuestDesc();
        GetChild(this.node, ["body", "btnDir"]).getComponent(Button).interactable = popupData.isDone || popupData.isNew || (!popupData.isDone && !popupData.isNew);

        if(qProgress.cur >= qProgress.target)
        {
            let stringNumb : number = 100000413;
            
            if(popupData.isDone)
            {
                GetChild(this.node, ["body", "Top_bg", "btnClose"]).active = false;
                stringNumb = 100000413;
            }
            GetChild(this.node, ["body", "btnDir", "labelMsg"]).getComponent(Label).string = TableManager.GetTable<StringTable>(StringTable.Name).Get(stringNumb).TEXT;

            this.done = true;

            if(Object.keys(QuestManager.nList).indexOf(String(this.qid)) < 0 && popupData.isDone)
            {
                QuestManager.ContentLayout.ActiveQMark(this.qid, qDone);
            }
        }
        else
        {
            let btnGotoTarget = GetChild(this.node, ["body", "btnDir"]).getComponent(Button)
            
            let stringNumb = 100000414;
            let eventHandler : EventHandler = new EventHandler();
            eventHandler.target = this.node;
            eventHandler.component = "QuestInfoPopup";
            eventHandler.handler = "onClickGetGoToTarget";

            btnGotoTarget.clickEvents.pop();
            btnGotoTarget.clickEvents.push(eventHandler);

            GetChild(this.node, ["body", "btnDir", "labelMsg"]).getComponent(Label).string = TableManager.GetTable<StringTable>(StringTable.Name).Get(stringNumb).TEXT;
        }

        let rewardScroll : ScrollView = GetChild(this.node, ["body", "rewardScroll"]).getComponent(ScrollView);
        let content = rewardScroll.content

        if(questInfo.designData.REWARD_ACCOUNT_EXP > 0)
        {
            let clone : Node = instantiate(GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB)["ExpSlot"])
            clone.getComponent(ExpFrame).setFrameExpInfo(ExpType.ACCOUNT_EXP, questInfo.designData.REWARD_ACCOUNT_EXP);

            clone.parent = content;
        }

        if(questInfo.designData.REWARD_GOLD > 0)
        {
            let newReward = instantiate(GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB)["item"]);
            let itemframe : ItemFrame = newReward.getComponent(ItemFrame);
            itemframe.setFrameCashInfo(0, questInfo.designData.REWARD_GOLD);

            newReward.parent = content;
        }

        if(questInfo.designData.REWARD_ENERGY > 0)
        {
            let newReward = instantiate(GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB)["item"]);
            let itemframe : ItemFrame = newReward.getComponent(ItemFrame);
            itemframe.setFrameEnergyInfo(questInfo.designData.REWARD_ENERGY);

            newReward.parent = content;
        }

        if(questInfo.designData.REWARD_ITEMS_ID.length > 0)
        {
            questInfo.designData.REWARD_ITEMS_ID.forEach((rElement, index) => 
            {
                let newReward = instantiate(GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB)["item"]);
                let itemframe : ItemFrame = newReward.getComponent(ItemFrame);
                itemframe.setFrameItemInfo(rElement, questInfo.designData.REWARD_ITEMS_VALUE[index]);

                newReward.parent = content;
            });
        }
        //rewardScroll.scrollToLeft();

        if(!this.Data.isDone && qProgress.cur >= qProgress.target)
        {
            this.ClosePopup();
        }
        else if(this.Data.isManual == undefined)
        {
            let tObj : TimeObject = this.addComponent(TimeObject);
            let time = 2;
            let btnDirInterect = GetChild(this.node, ["body", "btnDir"]).getComponent(Button).interactable;
            let btnCloseInterect = GetChild(this.node, ["body", "Top_bg", "btnClose"]).active
            GetChild(this.node, ["body", "btnDir"]).getComponent(Button).interactable = false;
            GetChild(this.node, ["body", "Top_bg", "btnClose"]).getComponent(Button).interactable = false;

            tObj.Refresh = () => 
            {
                if(time <= 0)
                {
                    tObj.Refresh = null;

                    GetChild(this.node, ["body", "btnDir"]).getComponent(Button).interactable = btnDirInterect;
                    GetChild(this.node, ["body", "Top_bg", "btnClose"]).getComponent(Button).interactable = btnCloseInterect;
                }
                time --;
            };

            TimeManager.AddObject(tObj);
        }

        // 튜토리얼 체크 - 튜토를 해야하면 바로가기버튼기능 off
        GetChild(this.node, ["body", "btnDir"]).active = true;
        this.tutorialButtonNode.active = false;

        if (TutorialManager.GetInstance.CheckTutorialStateByQuestKey(this.qid)
            && popupData.isDone == false) {
            
            GetChild(this.node, ["body", "btnDir"]).active = false;
            this.tutorialButtonNode.active = true;

            GetChild(this.node, ["body", "Top_bg", "btnClose"]).active = false;
        }
    }

    onClickGetGoToTarget()
    {
        let questInfo = QuestManager.Instance.GetQuest(this.qid);
        let cGroup = TableManager.GetTable<QuestTriggerTable>(QuestTriggerTable.Name).Get(questInfo.designData.CONDITION_GROUP);
        QuestManager.onClickGotoTarget(cGroup[0]);
        this.ClosePopup();
        return;
    }

    onClickGetReward()
    {
        NetworkManager.Send("quest/accomplish", {quest : this.qid}, (jsonData) => 
        {
            if(jsonData['rs'] != 0)
                return;

            QuestManager.AddAchive(this.qid);

            if(QuestManager.qList != null)
            {
                QuestManager.RemoveQuest(this.qid);
                this.ClosePopup();
            }

            // 튜토리얼 체크 (마지막 퀘스트 일 경우)
            if (this.qid == 31) {
                TutorialManager.GetInstance.OnTutorialEvent(118, 1);
            }
        });
    }

    OnClickStartTutorialButton()
    {
        if(Object.keys(QuestManager.qList).indexOf(String(this.qid)) >= 0 && !this.done)
        {
            QuestManager.ContentLayout.DeactiveQMark(this.qid);
        }

        QuestManager.isGetNew = false;
        if(QuestManager.AutoCheck)
            QuestManager.ContentLayout.CheckQuestState();

        // 튜토리얼 체크
        TutorialManager.GetInstance.OnTutorialEventWithCurrentQuest(this.qid);

        super.ClosePopup();
    }
    
    ClosePopup()
    {
        if(Object.keys(QuestManager.qList).indexOf(String(this.qid)) >= 0 && !this.done)
        {
            QuestManager.ContentLayout.DeactiveQMark(this.qid);
        }

        QuestManager.isGetNew = false;
        if(QuestManager.AutoCheck)
            QuestManager.ContentLayout.CheckQuestState();

        super.ClosePopup();
    }
}
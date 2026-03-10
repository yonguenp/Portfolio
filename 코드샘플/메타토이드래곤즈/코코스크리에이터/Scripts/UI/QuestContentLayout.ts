
import { _decorator, Component, Node, Layout, Sprite, Label, Prefab, instantiate, Button, EventHandler, ProgressBar, Color, SpriteFrame, ScrollView } from 'cc';
import { GameManager } from '../GameManager';
import { QuestManager, QuestObject } from '../QuestManager';
import { ResourceManager, ResourcesType } from '../ResourceManager';
import { GetChild, StringBuilder } from '../Tools/SandboxTools';
import { QuestCanister, QuestObjectList } from './QuestCanister';
const { ccclass, property } = _decorator;
 
const qNew = "qMark_New";
const qDone = "qMark_Done";

@ccclass('QuestContentLayout')
export class QuestContentLayout extends QuestCanister 
{
    private contentNode : Node = null;
    private layout : Layout = null;
    private capContent : QuestObjectList;

    start()
    {
        if(QuestManager.ContentLayout != this)
        {
            QuestManager.ContentLayout = this;
            this.init();
        }

        this.QuestContentUpdate();
    }

    private init()
    {
        this.contentNode = GetChild(this.node, ["questBtn", "viewport", "content"]);
        this.layout = this.contentNode.getComponent(Layout);
        this.capContent = new QuestObjectList();
    }

    public QuestContentUpdate()
    {
        if(!QuestManager.IsInitial)
            return;   

        let qList = QuestManager.GetQuestList();
        let keys = Object.keys(this.capContent.CapInstance);

        qList.forEach(qid =>
        {
            let questInfo : QuestObject = QuestManager.Instance.GetQuest(qid);
            let progressLabel = null;
            let progress = null;
            let targetValue : number = 0;
            let curValue : number = 0;
            let qProgress = questInfo.GetProgress();

            if(keys.indexOf(qid) < 0)
            {
                let pref : Prefab = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource<Prefab>(ResourcesType.UI_PREFAB)["questClone"];
                let parent = this.contentNode;
                let clone : Node = instantiate(pref);
                clone.parent = parent;

                let btnQuest : Button = clone.getComponent(Button);
                let newHandler = new EventHandler();
                newHandler.target = this.node;
                newHandler.component = "QuestContentLayout";
                newHandler.handler = "onClickQuestPopup";
                newHandler.customEventData = JSON.stringify({ quest : qid, isDone : false, isManual : true });

                btnQuest.clickEvents.push(newHandler);

                GetChild(clone, ["icon"]).getComponent(Sprite).spriteFrame = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.QUESTICON_SPRITEFRAME)[questInfo.GetIcon()];
                progressLabel = GetChild(clone, ["progressBg", "labelProgress"]).getComponent(Label);
                progress = GetChild(clone, ["progressBg", "ProgressBar"]);

                this.capContent.addCap(Number(qid), clone);

                //신규 검사
                if(QuestManager.nList.indexOf(Number(qid)) >= 0)
                {
                    this.ActiveQMark(Number(qid), qNew);
                }
            }
            else
            {
                progressLabel = GetChild(this.capContent.CapInstance[qid].node, ["progressBg", "labelProgress"]).getComponent(Label);
                progress = GetChild(this.capContent.CapInstance[qid].node, ["progressBg", "ProgressBar"]);
            }

            curValue = qProgress.cur;
            targetValue = qProgress.target;
            progressLabel.string = StringBuilder("{0} / {1}", curValue, targetValue);
            progress.getComponent(ProgressBar).progress = curValue / targetValue;

            if(curValue >= targetValue)
            {
                progress.getComponent(Sprite).color = new Color(255, 255, 0, 255);

                if(QuestManager.nList.indexOf(Number(qid)) < 0)
                {
                    this.ActiveQMark(Number(qid), qDone);
                }
            }
        })
        let originList = this.capContent.CapInstanceList;
        originList.sort((a, b) =>
            QuestManager.Instance.GetQuest(a.name).designData.QTYPE - QuestManager.Instance.GetQuest(b.name).designData.QTYPE
        ).forEach((element, index) =>
        {
            element.node.setSiblingIndex(index);
        })
        
        this.getComponent(ScrollView).scrollToTop()
        this.CheckQuestState();
    }

    ActiveQMark(qid : number, spriteName : string)
    {
        let sprite :SpriteFrame = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.QUESTICON_SPRITEFRAME)[spriteName];
        let node = GetChild(this.capContent.getCap(qid).node, ["questionMark"]);
        node.getComponent(Sprite).spriteFrame = sprite;
        node.active = true;
    }

    DeactiveQMark(qid : number)
    {
        GetChild(this.capContent.getCap(qid).node, ["questionMark"]).active = false;
    }

    RemoveQuestIcon(qid : number)
    {
        if(this.capContent.getCap(qid) != null)
        {
            (this.capContent.getCap(qid).node as Node).removeFromParent();
            this.capContent.removeCap(qid);
            this.layout.updateLayout();
        }
    }
}
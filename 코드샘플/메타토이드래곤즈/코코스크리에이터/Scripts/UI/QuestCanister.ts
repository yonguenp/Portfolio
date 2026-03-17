
import { _decorator, Component, Node } from 'cc';
import { NetworkManager } from '../NetworkManager';
import { QuestManager } from '../QuestManager';
const { ccclass, property } = _decorator;
 
export class QuestObjectList
{
    private capContentInstance : {name : number, node : Node} = null;
    get CapInstance() : {name : number, node : Node}
    {
        return this.capContentInstance;
    }

    getCap(name : number) : {name : number, node : Node}
    {
        return this.capContentInstance[name];
    }
    removeCap(name : number) : void
    {
        delete this.capContentInstance[name];
    }

    get CapInstanceList() : {name : number, node : Node}[]
    {
        let list : {name : number, node : Node}[] = new Array<{name : number, node : Node}>();
        let keys = Object.keys(this.capContentInstance);
        keys.forEach((element) => 
        {
            list.push(this.capContentInstance[element]);
        });

        return list;
    }

    addCap(name : number, node : Node) : void
    {
        this.capContentInstance[name] = {name : name, node : node}
    }

    constructor()
    {
        this.capContentInstance = JSON.parse("{}");
    }
}

@ccclass('QuestCanister')
export class QuestCanister extends Component 
{
    start()
    {
        if(QuestManager.ContentLayout != this)
        {
            QuestManager.ContentLayout = this;
        }

        this.QuestContentUpdate();
    }

    public QuestContentUpdate() 
    {
        if(QuestManager.AutoCheck)
            this.CheckQuestState()
    }

    CheckQuestState()
    {
        let checkstate : any;
        if( !QuestManager.isGetNew && QuestManager.nList.length > 0)
        {
            QuestManager.isGetNew = true;
            QuestManager.onClickQuestPopup({ quest : QuestManager.nList.pop(), isDone : false, isNew : true});
        } 
        else if( !QuestManager.isGetNew && (checkstate = QuestManager.GetAcceptableQuest()).length > 0)
        {
            let params = { quest : checkstate.arr };
            NetworkManager.Send("quest/accept", params, () => { });
        } 
        else if(!QuestManager.isGetNew && QuestManager.GetDone() > 0)
        {
            QuestManager.isGetNew = true;
            QuestManager.onClickQuestPopup({ quest : QuestManager.GetDone(), isDone : true });
        }
    }

    private onClickQuestPopup(event, qinfo)
    {
        QuestManager.onClickQuestPopup(JSON.parse(qinfo));
    }


    //쓰읍.. 이게 최선..? wwwwww
    ActiveQMark(qid : number, spriteName : string)
    {
        
    }

    DeactiveQMark(qid : number)
    {
        
    }

    RemoveQuestIcon(qid : number)
    {
        
    }

    onDestroy()
    {
        QuestManager.ContentLayout = null;
    }
}
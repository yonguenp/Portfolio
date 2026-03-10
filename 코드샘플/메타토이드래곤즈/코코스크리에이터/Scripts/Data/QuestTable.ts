import { eQuestType } from "../QuestManager";
import { QuestData, QuestTriggerData } from "./QuestData";
import { MultiTableBase, TableBase } from "./TableBase";

export class QuestTable extends TableBase<QuestData> {
    public static Name: string = "QuestTable";

    public SetTable(jsonData : Array<Array<string>>)
    {
        if(jsonData == null || jsonData.length < 1) {
            return;
        }

        this.DataClear();
        const columName = jsonData[0]
        const rowLength = jsonData.length
        const columLength = columName.length
        let data = this;

        for(let i = 1; i < rowLength; i++)
        {
            let rowData = jsonData[i]
            let quest = new QuestData()
            let reward_items = [];
            let reward_item_keys = [];

            for(let j = 0; j < columLength; j++)
            {
                switch(columName[j])
                {
                    case "KEY":
                        quest.Index = Number(rowData[j])
                        break;

                    case "TYPE":
                        let t : eQuestType;
                        switch(rowData[j])
                        {
                            case 'MAIN':
                                t = eQuestType.MAIN;
                            break;

                            case 'SUB':
                                t = eQuestType.SUB;
                            break;

                            case 'EVENT':
                                t = eQuestType.EVENT;
                            break;

                            case 'DAILY':
                                t = eQuestType.DAILY;
                            break;
                        }

                        quest.QTYPE = Number(t)
                        break

                    case "ICON":
                        quest.ICON = rowData[j]
                        break

                    case "_SUBJECT":
                        quest.SUBJECT = Number(rowData[j])
                        break

                    case "_NOTIE":
                        quest.NOTI = Number(rowData[j])
                        break

                    case "START_GROUP":
                        quest.START_GROUP = Number(rowData[j])
                        break

                    case "CONDITION_GROUP":
                        quest.CONDITION_GROUP = Number(rowData[j])
                        break

                    case "REWARD_ACCOUNT_EXP":
                        quest.REWARD_ACCOUNT_EXP = Number(rowData[j])
                        break

                    case "REWARD_GOLD":
                        quest.REWARD_GOLD = Number(rowData[j])
                        break

                    case "REWARD_ENERGY":
                        quest.REWARD_ENERGY = Number(rowData[j])
                        break

                    case "REWARD_ITEM1": case "REWARD_ITEM2": case "REWARD_ITEM3":
                        if(Number(rowData[j]) > 0)    
                            reward_item_keys.push(Number(rowData[j]))
                        break

                    case "VALUE1": case "VALUE2": case "VALUE3":
                        if(Number(rowData[j]) > 0)    
                            reward_items.push(Number(rowData[j]))
                        break
                }
            }
            quest.REWARD_ITEMS_ID = reward_item_keys;
            quest.REWARD_ITEMS_VALUE = reward_items;

            data.Add(quest)
        }
    }
}

export class QuestTriggerTable extends MultiTableBase<QuestTriggerData> {
    public static Name: string = "QuestTriggerTable";

    public SetTable(jsonData : Array<Array<string>>)
    {
        if(jsonData == null || jsonData.length < 1) {
            return;
        }

        this.DataClear();
        const columName = jsonData[0]
        const rowLength = jsonData.length
        const columLength = columName.length
        let data = this;

        for(let i = 1; i < rowLength; i++)
        {
            let rowData = jsonData[i]
            let trigger = new QuestTriggerData()

            for(let j = 0; j < columLength; j++)
            {
                switch(columName[j])
                {
                    case "KEY":
                        trigger.KEY = Number(rowData[j])
                        break;

                    case "TRIGGER_TYPE":
                        trigger.TRIGGER_TYPE = Number(rowData[j])
                        break

                    case "GROUP":
                        trigger.Index = Number(rowData[j])
                        trigger.GROUP = Number(rowData[j])
                        break

                    case "TYPE":
                        trigger.TYPE = rowData[j]
                        break

                    case "TYPE_KEY":
                        trigger.TYPE_KEY = rowData[j]
                        break

                    case "TYPE_KEY_VALUE":
                        trigger.TYPE_KEY_VALUE = Number(rowData[j])
                        break
                }
            }

            data.Add(trigger)
        }
    }

    public GetTrigger(triggerID : number) : QuestTriggerData
    {
        let keys = Object.keys(this.datas);
        let ret : QuestTriggerData = null;

        keys.forEach(aElement =>
        {
            this.datas[aElement].forEach( data => 
            {
                if((data as QuestTriggerData).KEY == triggerID)
                {
                    ret = data;
                }                
            });
        });

        return ret
    }
}
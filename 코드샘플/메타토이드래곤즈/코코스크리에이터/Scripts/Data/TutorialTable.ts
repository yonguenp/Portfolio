import { _decorator} from 'cc';
import { TutorialData } from './TutorialData';
import { TableBase } from './TableBase';


export class TutorialTable extends TableBase<TutorialData>  {

    public static Name: string = "TutorialTable";

    public SetTable(jsonData: Array<string>): void 
    {
        if(jsonData == null || jsonData.length < 1) 
        {
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
            let tutorial = new TutorialData()

            for(let j = 0; j < columLength; j++)
            {
                switch(columName[j])
                {
                    case "KEY":
                        tutorial.Index = Number(rowData[j])
                        break;

                    case "GROUP":
                        tutorial.GROUP = Number(rowData[j])
                        break

                    case "SEQUENCE":
                        tutorial.SEQUENCE = Number(rowData[j])
                        break

                    case "QUEST_COND_KEY":
                        tutorial.QUEST_COND_KEY = Number(rowData[j])
                        break

                    case "EVENT_TYPE":
                        tutorial.EVENT_TYPE = Number(rowData[j])
                        break

                    case "DELAY":
                        tutorial.DELAY = Number(rowData[j])
                        break

                    case "_DESC":
                        tutorial._DESC = Number(rowData[j])
                        break
                }
            }

            data.Add(tutorial)
        }
    }

    public GetByGroup(group: number): TutorialData[] {

        let list = this.GetAll();

        return list.filter(element => element.GROUP == group);
    }

    public GetByGroupSeq(group: number, seq: number): TutorialData {
        
        let list = this.GetAll();

        return list.find(element => (element.GROUP == group && element.SEQUENCE == seq));
    }

    public GetMaxSequence(group: number): number {

        let result = this.GetByGroup(group);

        return result.length;
    }

    public GetTutorialByQuestKey(questKey: number, seq: number = 1): TutorialData {

        let list = this.GetAll();

        return list.find(element => (element.QUEST_COND_KEY == questKey && element.SEQUENCE == seq));
    }
}
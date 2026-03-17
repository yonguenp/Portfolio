
import { _decorator} from 'cc';
import { StageBaseData } from './StageData';
import { TableBase } from './TableBase';

export class StageBaseTable extends TableBase<StageBaseData> 
{
    public static Name: string = "StageBaseTable";

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
            let stage = new StageBaseData()

            for(let j = 0; j < columLength; j++)
            {
                switch(columName[j])
                {
                    case "KEY":
                        stage.Index = rowData[j]
                        break;

                    case "TYPE":
                        stage.TYPE = Number(rowData[j])
                        break

                    case "DIFFICULT":
                        stage.DIFFICULT = Number(rowData[j])
                        break

                    case "WORLD":
                        stage.WORLD = Number(rowData[j])
                        break

                    case "STAGE":
                        stage.STAGE = Number(rowData[j])
                        break

                    case "_NAME":
                        stage.NAME = Number(rowData[j])
                        break

                    case "IMAGE":
                        stage.IMAGE = rowData[j]
                        break

                    case "COST_TYPE":
                        stage.COST_TYPE = rowData[j]
                        break
                    
                    case "COST_VALUE":
                        stage.COST_VALUE = Number(rowData[j])
                        break

                    case "CLEAR_COUNT":
                        stage.CLEAR_COUNT = Number(rowData[j])
                        break

                    case "TIME":
                        stage.TIME = Number(rowData[j])
                        break

                    case "SPAWN":
                        stage.SPAWN = Number(rowData[j])
                        break


                    case "REWARD_ACCOUNT_EXP":
                        stage.ACCOUNT_EXP = Number(rowData[j])
                        break
                    
                    case "REWARD_CHAR_EXP":
                        stage.CHAR_EXP = Number(rowData[j])
                        break

                    case "REWARD_GOLD":
                        stage.REWARD_GOLD = Number(rowData[j])
                        break

                    case "REWARD_ITEM":
                        stage.REWARD_ITEM = Number(rowData[j])
                        break

                    case "REWARD_ITEM_COUNT":
                        stage.REWARD_ITEM_COUNT = Number(rowData[j])
                        break
                    
                    case "FIRST_REWARD_STAR_1":
                        stage.REWARD_STAR[0] = Number(rowData[j])
                        break

                    case "FIRST_REWARD_STAR_2":
                        stage.REWARD_STAR[1] = Number(rowData[j])
                        break

                    case "FIRST_REWARD_STAR_3":
                        stage.REWARD_STAR[2] = Number(rowData[j])
                        break

                    case "CHARGE_COST_TYPE":
                        stage.CHARGE_COST_TYPE = rowData[j]
                        break
                    
                    case "CHARGE_COST_VALUE":
                        stage.CHARGE_COST_VALUE = Number(rowData[j])
                        break

                    case "CHARGE_COUNT":
                        stage.CHARGE_COUNT = Number(rowData[j])
                        break

                    case "CHARGE_MAX":
                        stage.CHARGE_MAX = Number(rowData[j])
                        break

                    case "UNLOCK_MISSION":
                        stage.UNLOCK_MISSION = Number(rowData[j])
                        break
                }
            }

            data.Add(stage)
        }
    }

    public GetByWorld(world: number): StageBaseData[] {
        let list = this.GetAll();

        return list.filter(element => element.WORLD == world);
    }

    public GetByWorldStage(world: number, stage: number): StageBaseData {
        let list = this.GetAll();
        let cur: StageBaseData = null;
        list.forEach(element => {
            if(element.WORLD == world && element.STAGE == stage) {
                cur = element;
            }
        });

        return cur;
    }
}


import { _decorator } from 'cc';
import { TableBase } from './TableBase';
import { WorldData as WorldBaseData } from './WorldData';
const { ccclass } = _decorator;
 
@ccclass('WorldTable')
export class WorldBaseTable extends TableBase<WorldBaseData> 
{
    public static Name: string = "WorldBaseTable";
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
            let world = new WorldBaseData()

            for(let j = 0; j < columLength; j++)
            {
                switch(columName[j])
                {
                    case "KEY":
                        world.Index = Number(rowData[j])
                        break;

                    case "NUM":
                        world.NUM = Number(rowData[j])
                        break

                    case "_NAME":
                        world.NAME = Number(rowData[j])
                        break

                    case "BACKGROUND":
                        world.BACKGROUND = rowData[j]
                        break

                    case "IMAGE":
                        world.IMAGE = rowData[j]
                        break

                    case "STAR_1":
                        world.STAR[0] = Number(rowData[j])
                        break

                    case "STAR_2":
                        world.STAR[1] = Number(rowData[j])
                        break

                    case "STAR_3":
                        world.STAR[2] = Number(rowData[j])
                        break
                    
                    case "REWARD_STAR_1":
                        world.STAR_REWARD[0] = Number(rowData[j])
                        break

                    case "REWARD_STAR_2":
                        world.STAR_REWARD[1] = Number(rowData[j])
                        break

                    case "REWARD_STAR_3":
                        world.STAR_REWARD[2] = Number(rowData[j])
                        break
                }
            }

            data.Add(world)
        }
    }
}

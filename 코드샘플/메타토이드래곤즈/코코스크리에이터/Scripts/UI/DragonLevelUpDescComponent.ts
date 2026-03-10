
import { _decorator, Component, Label } from 'cc';
import { CharBaseTable } from '../Data/CharTable';
import { StringTable } from '../Data/StringTable';
import { TableManager } from '../Data/TableManager';
import { DataManager } from '../Tools/DataManager';
import { GetDragonStat, StringBuilder } from '../Tools/SandboxTools';
import { User } from '../User/User';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = DragonLevelUpDescComponent
 * DateTime = Thu Mar 17 2022 12:29:24 GMT+0900 (대한민국 표준시)
 * Author = wonjun_gs
 * FileBasename = DragonLevelUpDescComponent.ts
 * FileBasenameNoExtension = DragonLevelUpDescComponent
 * URL = db://assets/Scripts/UI/DragonLevelUpDescComponent.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 /**
  * 각 노드의 레벨, 투력 공격력 방어력 체력 크리티컬 표시부
  */
@ccclass('DragonLevelUpDescComponent')
export class DragonLevelUpDescComponent extends Component 
{
    charDataTable : CharBaseTable = null;

    @property(Label)
    dragonLevelLabel : Label;
    @property(Label)
    battleLabel : Label;
    @property(Label)
    AtkLabel : Label;
    @property(Label)
    DefLabel : Label;
    @property(Label)
    HealthLabel : Label;
    @property(Label)
    critLabel : Label;

    init(level : number)
    {
        if(this.charDataTable == null){
            this.charDataTable = TableManager.GetTable<CharBaseTable>(CharBaseTable.Name);
        }

        this.refreshCurrentDragonData(level);
    }
    refreshCurrentDragonData(param : number)
    {
        if(DataManager.GetData("DragonInfo") != null)//드래곤 태그값
        {
            let dragonTag = DataManager.GetData("DragonInfo") as number;
            let dragonData = User.Instance.DragonData;
            if(dragonData == null){
                console.log("user's dragon Data is null");
                return;         
            }

            let userDragonInfo = dragonData.GetDragon(dragonTag);
            if(userDragonInfo == null){
                console.log("user Dragon is null");
                return;
            }
            
            this.RefreshDragonStat(dragonTag,param);
        }
    }
    RefreshDragonStat(dragonTag : number, Level : number)
    {
        let dragonStat = GetDragonStat(dragonTag, Level);
        let dragonBattlePoint = dragonStat.INF;
        let dragonAtk = dragonStat.ATK;
        let dragonDef = dragonStat.DEF;
        let dragonHealth = dragonStat.HP;
        let dragonCri = dragonStat.CRI;

        this.dragonLevelLabel.string = StringBuilder(TableManager.GetTable<StringTable>(StringTable.Name).Get(100000056).TEXT, Level);
        this.battleLabel.string = dragonBattlePoint.toString();
        this.AtkLabel.string = dragonAtk.toString();
        this.DefLabel.string = dragonDef.toString();
        this.HealthLabel.string = dragonHealth.toString();
        this.critLabel.string = dragonCri.toFixed(2) + "%";
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


import { _decorator } from 'cc';
import { IManagerBase, ITableBase } from 'sb';
import { GameManager } from '../GameManager';
import { ObjectCheck } from '../Tools/SandboxTools';
import { AccountTable } from './AccountTable';
import { AreaExpansionTable, AreaLevelTable, WorldTripTable } from './AreaTable';
import { BuildingBaseTable, BuildingLevelTable, BuildingOpenTable } from './BuildingTable';
import { CharBaseTable, CharExpTable, CharGradeTable } from './CharTable';
import { ElementRateTable } from './ElementTable';
import { GachaListTable, GachaShopTable } from './GachaTable';
import { InventoryTable, SlotCostTable } from './InventoryTable';
import { DefineResourceTable, ItemBaseTable, ItemGroupTable } from './ItemTable';
import { MonsterBaseTable, MonsterSpawnTable } from './MonsterTable';
import { PartOptionTable, PartReinforceTable, PartSetTable } from './PartOptionTable';
import { PartTable } from './PartTable';
import { ProductAutoTable, ProductTable } from './ProductTable';
import { QuestTable, QuestTriggerTable } from './QuestTable';
import { SkillCharTable, SkillEffectTable, SkillProjectileTable } from './SkillTable';
import { StageBaseTable } from './StageTable';
import { StatFactorTable } from './StatTable';
import { StringTable } from './StringTable';
import { SubwayDeliveryTable, SubwayPlatformTable } from './SubwayTable';
import { TutorialTable } from './TutorialTable';
import { WorldBaseTable } from './WorldTable';

/**
 * Predefined variables
 * Name = TableManager
 * DateTime = Wed Jan 12 2022 16:05:08 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = TableManager.ts
 * FileBasenameNoExtension = TableManager
 * URL = db://assets/Scripts/Data/TableManager.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */

export class TableManager implements IManagerBase {
    public static Name: string = "TableManager";
    protected tables: {} = null;
    protected static instance: TableManager = null;

    public static get Instance() {
        if(TableManager.instance == null) {
            return TableManager.instance = new TableManager();
        }
        return TableManager.instance;
    }


    public Init(): void {
        this.tables = {};

        let table1 = new AreaExpansionTable();
        table1.Init();
        this.tables[AreaExpansionTable.Name] = table1;

        let table2 = new AreaLevelTable();
        table2.Init();
        this.tables[AreaLevelTable.Name] = table2;

        let table3 = new StringTable();
        table3.Init();
        this.tables[StringTable.Name] = table3;

        let table4 = new ProductTable();
        table4.Init();
        this.tables[ProductTable.Name] = table4;

        let table5 = new ProductAutoTable();
        table5.Init();
        this.tables[ProductAutoTable.Name] = table5;

        let table6 = new BuildingBaseTable();
        table6.Init();
        this.tables[BuildingBaseTable.Name] = table6;

        let table7 = new BuildingLevelTable();
        table7.Init();
        this.tables[BuildingLevelTable.Name] = table7;

        let table8 = new BuildingOpenTable();
        table8.Init();
        this.tables[BuildingOpenTable.Name] = table8;

        let table10 = new ItemBaseTable();
        table10.Init();
        this.tables[ItemBaseTable.Name] = table10;

        let table11 = new ItemGroupTable();
        table11.Init();
        this.tables[ItemGroupTable.Name] = table11;

        let table12 = new DefineResourceTable();
        table12.Init();
        this.tables[DefineResourceTable.Name] = table12;

        let table13 = new InventoryTable();
        table13.Init();
        this.tables[InventoryTable.Name] = table13;

        let table14 = new SlotCostTable();
        table14.Init();
        this.tables[SlotCostTable.Name] = table14;

        let table15 = new WorldTripTable();
        table15.Init();
        this.tables[WorldTripTable.Name] = table15;

        let table16 = new SubwayPlatformTable();
        table16.Init();
        this.tables[SubwayPlatformTable.Name] = table16;

        let table17 = new SubwayDeliveryTable();
        table17.Init();
        this.tables[SubwayDeliveryTable.Name] = table17;

        let table18 = new AccountTable();
        table18.Init();
        this.tables[AccountTable.Name] = table18;

        let table19 = new CharBaseTable();
        table19.Init();
        this.tables[CharBaseTable.Name] = table19;

        let table20 = new CharGradeTable();
        table20.Init();
        this.tables[CharGradeTable.Name] = table20;

        let table21 = new CharExpTable();
        table21.Init();
        this.tables[CharExpTable.Name] = table21;

        let table22 = new StatFactorTable();
        table22.Init();
        this.tables[StatFactorTable.Name] = table22;

        let table23 = new ElementRateTable();
        table23.Init();
        this.tables[ElementRateTable.Name] = table23;

        let table24 = new MonsterBaseTable();
        table24.Init();
        this.tables[MonsterBaseTable.Name] = table24;

        let table25 = new MonsterSpawnTable();
        table25.Init();
        this.tables[MonsterSpawnTable.Name] = table25;

        let table26 = new SkillCharTable();
        table26.Init();
        this.tables[SkillCharTable.Name] = table26;

        let table27 = new SkillEffectTable();
        table27.Init();
        this.tables[SkillEffectTable.Name] = table27;

        let table28 = new WorldBaseTable();
        table28.Init();
        this.tables[WorldBaseTable.Name] = table28;

        let table29 = new StageBaseTable();
        table29.Init();
        this.tables[StageBaseTable.Name] = table29;

        let table30 = new GachaShopTable();
        table30.Init();
        this.tables[GachaShopTable.Name] = table30;

        let table31 = new GachaListTable();
        table31.Init();
        this.tables[GachaListTable.Name] = table31;

        let table32 = new PartTable();
        table32.Init();
        this.tables[PartTable.Name] = table32;

        let table33 = new PartOptionTable();
        table33.Init();
        this.tables[PartOptionTable.Name] = table33;

        let table34 = new SkillProjectileTable();
        table34.Init();
        this.tables[SkillProjectileTable.Name] = table34;

        let table35 = new TutorialTable();
        table35.Init();
        this.tables[TutorialTable.Name] = table35;

        let table36 = new PartSetTable();
        table36.Init();
        this.tables[PartSetTable.Name] = table36;

        let table37 = new PartReinforceTable();
        table37.Init();
        this.tables[PartReinforceTable.Name] = table37;

        let table38 = new QuestTable();
        table38.Init();
        this.tables[QuestTable.Name] = table38;

        let table39 = new QuestTriggerTable();
        table39.Init();
        this.tables[QuestTriggerTable.Name] = table39;

        GameManager.Instance.AddManager(this, false);
    }

    // public static GetTable<T extends ITableBase>(name: string): T{
    //     if (ObjectCheck(TableManager.instance.tables, name)) {
    //         return TableManager.instance.tables[name] as T;
    //     }
    //     return null;        
    // }

    // public static GetTable<T extends ITableBase>(ctor:TypeConstructor<T>): T{
    //     if (ObjectCheck(TableManager.instance.tables, ctor.name)) {
    //         return TableManager.instance.tables[ctor.name] as T;
    //     }
    //     return null;        
    // }

    public static GetTable<T extends ITableBase>(name: string): T{
        if (ObjectCheck(TableManager.instance.tables, name)) {
            return TableManager.instance.tables[name] as T;
        }
        return null;        
    }

    public GetManagerName(): string {
        return TableManager.Name;
    }

    public Update(deltaTime: number): void {
    }
}

/**
 * [1] Class member could be defined like this.
 * [2] Use `property` decorator if your want the member to be serializable.
 * [3] Your initialization goes here.
 * [4] Your update function goes here.
 *
 * Learn more about scripting: https://docs.cocos.com/creator/3.3/manual/en/scripting/
 * Learn more about CCClass: https://docs.cocos.com/creator/3.3/manual/en/scripting/ccclass.html
 * Learn more about life-cycle callbacks: https://docs.cocos.com/creator/3.3/manual/en/scripting/life-cycle-callbacks.html
 */

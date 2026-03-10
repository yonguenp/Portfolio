
import { _decorator, Component, Label, Node} from 'cc';
import { CharBaseTable, CharExpTable } from '../../../Data/CharTable';
import { StringTable } from '../../../Data/StringTable';
import { TableManager } from '../../../Data/TableManager';
import { DataManager } from '../../../Tools/DataManager';
import { numberWithCommas } from '../../../Tools/SandboxTools';
import { User } from '../../../User/User';
import { DragonPartInfoAllPopup } from '../../DragonPartInfoAllPopup';
import { ToastMessage } from '../../ToastMessage';
import { DragonReserveLayer } from '../DragonReserveLayer';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = dragonStatComponent
 * DateTime = Tue Mar 15 2022 22:40:28 GMT+0900 (대한민국 표준시)
 * Author = wonjun_gs
 * FileBasename = dragonStatComponent.ts
 * FileBasenameNoExtension = dragonStatComponent
 * URL = db://assets/Scripts/UI/DragonManagement/detailInfoLayer/dragonStatComponent.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 /**
  * 드래곤 능력치 표시창 (전투력, 공격력 체력 등등)
  */
@ccclass('dragonStatComponent')
export class dragonStatComponent extends Component {
    charDataTable : CharBaseTable = null;
    charExpTable : CharExpTable = null;

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

    @property(DragonReserveLayer)
    taplayer : DragonReserveLayer = null;

    @property(DragonPartInfoAllPopup)
    dragonpartInfoPopup : DragonPartInfoAllPopup = null;

    tempDragonTag : number = 0;
    tempDragonLevel : number = 0;

    init()
    {
        if(this.charDataTable == null){
            this.charDataTable = TableManager.GetTable<CharBaseTable>(CharBaseTable.Name);
        }

        if(this.charExpTable == null){
            this.charExpTable = TableManager.GetTable<CharExpTable>(CharExpTable.Name);
        }

        //console.log(this.charExpTable);
        
        this.refreshCurrentDragonData();
    }
    refreshCurrentDragonData()
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
            
            this.RefreshDragonStat(dragonTag,userDragonInfo.Level);
            this.onHidePartInfoData();
        }
    }
    RefreshDragonStat(dragonTag : number, currentLevel : number)
    {
        let dragonInfo = User.Instance.DragonData.GetDragon(dragonTag);
        if(dragonInfo == null){
            return;
        }

        this.tempDragonLevel = currentLevel;
        this.tempDragonTag = dragonTag;

        let dragonStat = dragonInfo.GetDragonALLStat();//스킬 반영 전투력
        let dragonBattlePoint = dragonStat.INF;
        let dragonAtk = dragonStat.ATK;
        let dragonDef = dragonStat.DEF;
        let dragonHealth = dragonStat.HP;
        let dragonCri = dragonStat.CRI;

        this.battleLabel.string = /*TableManager.GetTable<StringTable>(StringTable.Name).Get(100000177).TEXT + " "+*/numberWithCommas(dragonBattlePoint.toString());
        this.AtkLabel.string = /*TableManager.GetTable<StringTable>(StringTable.Name).Get(100000178).TEXT + " "+*/numberWithCommas(dragonAtk.toFixed(0));
        this.DefLabel.string = /*TableManager.GetTable<StringTable>(StringTable.Name).Get(100000179).TEXT+ " "+*/numberWithCommas(dragonDef.toString());
        this.HealthLabel.string = /*TableManager.GetTable<StringTable>(StringTable.Name).Get(100000180).TEXT+ " "+ */numberWithCommas( dragonHealth.toString());
        this.critLabel.string = /*TableManager.GetTable<StringTable>(StringTable.Name).Get(100000181).TEXT+ " "+ */numberWithCommas(dragonCri.toFixed(2))+ "(%)";
    }

    onClickLevelUpButton()
    {
        if(this.isDragonMaxLevel()){

            let emptyCheck = ToastMessage.isToastEmpty();
            if(emptyCheck == false){
                return;
            }
            
            let text = TableManager.GetTable<StringTable>(StringTable.Name).Get(100000099).TEXT;
            ToastMessage.Set(text, null, -52);
            return;
        }

        if(this.taplayer != null)
        {
            this.taplayer.moveLayer({"index":3});
        }
    }

    isDragonMaxLevel() : boolean
    {
        let maxLevel = this.charExpTable.GetDragonMaxLevel();
        return maxLevel == this.tempDragonLevel;
    }

    //장착 정보(드래곤이 장착 중인 장비 옵션 모두 표시) 팝업 표시 
    onClickDragonPartData()
    {
        //console.log('onClickDragonPartData');
        let params = { dragonTag : this.tempDragonTag }
        //PopupManager.OpenDataPopup("DragonPartInfoAllPopup", params, true) as DragonPartInfoAllPopup;

        if(this.dragonpartInfoPopup != null &&  this.dragonpartInfoPopup.node.activeInHierarchy == false)
        {
            this.dragonpartInfoPopup.node.active = true;
            this.dragonpartInfoPopup.Init(params);
        }
        else
        {
            this.onHidePartInfoData();        
        }
    }

    onHidePartInfoData()
    {
        if(this.dragonpartInfoPopup != null &&  this.dragonpartInfoPopup.node.activeInHierarchy == true)
        {
            this.dragonpartInfoPopup.node.active = false;
        }
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

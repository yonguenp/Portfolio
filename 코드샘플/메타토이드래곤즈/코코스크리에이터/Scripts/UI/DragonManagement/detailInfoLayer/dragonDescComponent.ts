
import { _decorator, Component, Label, Sprite, SpriteFrame } from 'cc';
import { CharBaseTable, CharGradeTable } from '../../../Data/CharTable';
import { StringTable } from '../../../Data/StringTable';
import { TableManager } from '../../../Data/TableManager';
import { GameManager } from '../../../GameManager';
import { ResourceManager, ResourcesType } from '../../../ResourceManager';
import { DataManager } from '../../../Tools/DataManager';
import { StringBuilder } from '../../../Tools/SandboxTools';
import { User, UserDragon } from '../../../User/User';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = dragonDescComponent
 * DateTime = Tue Mar 15 2022 22:50:38 GMT+0900 (대한민국 표준시)
 * Author = wonjun_gs
 * FileBasename = dragonDescComponent.ts
 * FileBasenameNoExtension = dragonDescComponent
 * URL = db://assets/Scripts/UI/DragonManagement/detailInfoLayer/dragonDescComponent.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('dragonDescComponent')
export class dragonDescComponent extends Component {
    charDataTable : CharBaseTable = null;
    charGradeTable :CharGradeTable = null;
    stringTable: StringTable = null;


    @property(Label)
    dragonNameLabel : Label;
    @property(Sprite)
    dragonElementSprite : Sprite;
    @property(Sprite)
    dragonGrade : Sprite;

    

    init()
    {
        if(this.stringTable == null){
            this.stringTable = TableManager.GetTable<StringTable>(StringTable.Name);
        }
        if(this.charDataTable == null){
            this.charDataTable = TableManager.GetTable<CharBaseTable>(CharBaseTable.Name);
        }
        if(this.charGradeTable == null){
            this.charGradeTable = TableManager.GetTable<CharGradeTable>(CharGradeTable.Name);
        }

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
            
            this.RefreshDragonDesc(userDragonInfo);
        }
    }
    RefreshDragonDesc(dragonInfo : UserDragon)
    {
        let data = this.stringTable.Get(100000056);
        if(data != null) {
            this.dragonNameLabel.string = (StringBuilder(data.TEXT, dragonInfo.Level) + `   [${dragonInfo.Name}]`);
        }
        this.dragonElementSprite.spriteFrame = this.GetElementIconSpriteByIndex(dragonInfo.Element);

        let gradeNameStrIndex = this.charGradeTable.Get(dragonInfo.Grade)._NAME;
        //this.dragonGrade.string = this.stringTable.Get(gradeNameStrIndex).TEXT;
        
        let icon = ResourceManager.Instance.GetResource(ResourcesType.DRAGON_GRADETAG_SPRITEFRAME)[this.stringTable.Get(gradeNameStrIndex).TEXT];
        if(icon != null){
            this.dragonGrade.spriteFrame = icon;
        }
    }

    GetElementIconSpriteByIndex(e_type : number) : SpriteFrame
    {
        let elementStr = "";
        switch(e_type)
        {
            case 1 :
                elementStr = "fire";
                break;
            case 2 :
                elementStr = "water";
                break;
            case 3 :
                elementStr = "soil";
                break;
            case 4 :
                elementStr = "wind";
                break;
            case 5 :
                elementStr = "light";
                break;
            case 6 :
                elementStr = "dark";
                break;
            default:
                break;
        }

        return GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.ELEMENTICON_SPRITEFRAME)[`type_${elementStr}`]
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

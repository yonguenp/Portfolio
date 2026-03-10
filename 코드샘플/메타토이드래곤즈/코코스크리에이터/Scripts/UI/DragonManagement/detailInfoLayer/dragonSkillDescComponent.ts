
import { _decorator, Component, Node, Label, Sprite } from 'cc';
import { CharBaseTable } from '../../../Data/CharTable';
import { SkillCharTable } from '../../../Data/SkillTable';
import { StringTable } from '../../../Data/StringTable';
import { TableManager } from '../../../Data/TableManager';
import { ResourceManager, ResourcesType } from '../../../ResourceManager';
import { DataManager } from '../../../Tools/DataManager';
import { StringBuilder } from '../../../Tools/SandboxTools';
import { User, UserDragon } from '../../../User/User';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = dragonSkillDescComponent
 * DateTime = Fri Mar 18 2022 18:38:27 GMT+0900 (대한민국 표준시)
 * Author = wonjun_gs
 * FileBasename = dragonSkillDescComponent.ts
 * FileBasenameNoExtension = dragonSkillDescComponent
 * URL = db://assets/Scripts/UI/DragonManagement/detailInfoLayer/dragonSkillDescComponent.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('dragonSkillDescComponent')
export class dragonSkillDescComponent extends Component {

    skilltable : SkillCharTable = null;
    charDataTable : CharBaseTable = null;
    
    @property(Node)
    skillDescNode : Node = null;
    @property(Label)
    skillLevelLabel : Label = null;
    @property(Label)
    skillDescLabel : Label= null;

    @property(Sprite)
    skillIconFrame : Sprite = null;
    @property(Label)
    skillIconLevelLabel : Label = null;
    
    init()
    {
        if(this.charDataTable == null){
            this.charDataTable = TableManager.GetTable<CharBaseTable>(CharBaseTable.Name);
        }
        if(this.skilltable == null){
            this.skilltable = TableManager.GetTable<SkillCharTable>(SkillCharTable.Name);
        }
        this.refreshCurrentDragonData();
    }
    onClickSkillIcon()
    {
        if(this.skillDescNode != null){
            this.skillDescNode.active = true;
        }
    }
    hideSkillDetail()
    {
        if(this.skillDescNode != null){
            this.skillDescNode.active = false;
        }
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
            
            this.setSkillDescData(userDragonInfo);
        }
    }
    setSkillDescData(dragonInfo : UserDragon)
    {
        let sLevel = dragonInfo.SLevel;
        let dragonSkillIndex= this.charDataTable.Get(dragonInfo.Tag).SKILL1;
        let skillData = this.skilltable.GetSkillByLevel(dragonSkillIndex,sLevel);
        if(skillData == null){
            return;
        }
        let skillName = TableManager.GetTable<StringTable>(StringTable.Name).Get(skillData._NAME).TEXT;
        
        this.skillLevelLabel.string = StringBuilder(TableManager.GetTable<StringTable>(StringTable.Name).Get(100000057).TEXT, sLevel,skillName);

        let skillDescName = TableManager.GetTable<StringTable>(StringTable.Name).Get(skillData._DESC1).TEXT;
        this.skillDescLabel.string = skillDescName;

        let skillImage = skillData.ICON;
        this.setSkillIconImage(skillImage);
        this.setSkillLevelLable(sLevel);
    }

    setSkillIconImage(skillIcon : string)
    {
        this.skillIconFrame.spriteFrame = ResourceManager.Instance.GetResource(ResourcesType.SKILLICON_SPRITEFRAME)[skillIcon];
    }
    setSkillLevelLable(slevel : number)
    {
        this.skillIconLevelLabel.string = StringBuilder(TableManager.GetTable<StringTable>(StringTable.Name).Get(100000056).TEXT, slevel);
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

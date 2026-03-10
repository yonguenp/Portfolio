
import { _decorator, Component, Node, Label, Sprite, Button, instantiate } from 'cc';
import { CharBaseData } from '../../Data/CharData';
import { CharBaseTable } from '../../Data/CharTable';
import { SkillCharData } from '../../Data/SkillData';
import { SkillCharTable } from '../../Data/SkillTable';
import { StringTable } from '../../Data/StringTable';
import { TableManager } from '../../Data/TableManager';
import { GameManager } from '../../GameManager';
import { NetworkManager } from '../../NetworkManager';
import { ResourceManager, ResourcesType } from '../../ResourceManager';
import { DataManager } from '../../Tools/DataManager';
import { GetChild, StringBuilder } from '../../Tools/SandboxTools';
import { eGoodType, InventoryItem, User, UserDragon } from '../../User/User';
import { ButtonChangeChildColor } from '../ButtonChangeChildColor';
import { SubLayer } from '../Common/SubLayer';
import { ItemFrame } from '../ItemSlot/ItemFrame';
import { dragonCharacterSlotComponent } from './detailInfoLayer/dragonCharacterSlotComponent';
import { dragonDescComponent } from './detailInfoLayer/dragonDescComponent';
const { ccclass, property } = _decorator;

@ccclass('DragonSkillLayer')
export class DragonSkillLayer extends SubLayer
{
    private sprMySkillIcon : Sprite = null
    private labelMySkillLevel : Label = null
    private labelMySkillName : Label = null
    private labelMySkillDesc : Label = null
    private sprNewSkillIcon : Sprite = null
    private labelNewSkillLevel : Label = null
    private labelNewSkillDesc : Label = null
    private labelSkillLevel : Label = null;
    private btnNextSkill : Button = null
    private btnBefSkill : Button = null
    private btnMaxSkill : Button = null
    private nodeContent : Node = null

    @property(dragonCharacterSlotComponent)
    dragonCharacter : dragonCharacterSlotComponent = null;

    @property(dragonDescComponent)
    dragonDesc : dragonDescComponent = null;

    private dragon : UserDragon = null
    private dragonBase : CharBaseData = null

    private charTable : CharBaseTable = null
    private stringTable : StringTable = null
    private skilltable : SkillCharTable = null
    private resourceManager : ResourceManager = null

    private nextLevel : number = 0
    private maxLevel : number = 0
    private totalLevel : number = 0
    private reqItems : {[id : string] : number} = {}
    private reqCash : {[id : string] : number}

    private newskilldata : SkillCharData = null
    private myskilldata : SkillCharData = null
    
    Init()
    {
        this.charTable = TableManager.GetTable<CharBaseTable>(CharBaseTable.Name)
        this.stringTable = TableManager.GetTable<StringTable>(StringTable.Name)
        this.skilltable = TableManager.GetTable<SkillCharTable>(SkillCharTable.Name)
        this.resourceManager = GameManager.GetManager<ResourceManager>(ResourceManager.Name)

        let boardParent : Node = GetChild(this.node, ["Shape", "dragonLevelup", "levelUpBoard"])
        this.sprMySkillIcon = GetChild(boardParent, ["MySkill", "layout", "icon"]).getComponent(Sprite)
        this.labelMySkillLevel = GetChild(boardParent, ["MySkill", "layout", "level"]).getComponent(Label)
        this.labelMySkillName = GetChild(boardParent, ["MySkill", "layout", "Skill_name"]).getComponent(Label)
        this.labelMySkillDesc = GetChild(boardParent, ["MySkill", "desc"]).getComponent(Label)
        //this.sprNewSkillIcon = GetChild(boardParent, ["NextSkill", "layout", "icon"]).getComponent(Sprite)
        this.labelNewSkillLevel = GetChild(boardParent, ["MySkill", "layout", "LevelNode","level"]).getComponent(Label)
        //this.labelNewSkillDesc = GetChild(boardParent, ["NextSkill", "diff"]).getComponent(Label)
        this.labelSkillLevel = GetChild(boardParent, ["NextSkill", "LevelUpButtons", "Level_label"]).getComponent(Label)
        this.nodeContent = GetChild(boardParent, ["NextSkill", "item_Node", "scroll", "content"]);

        this.btnBefSkill = GetChild(boardParent, ["NextSkill", "LevelUpButtons", "btnBefLevel"]).getComponent(Button)
        this.btnNextSkill = GetChild(boardParent, ["NextSkill", "LevelUpButtons", "btnNextLevel"]).getComponent(Button)
        this.btnMaxSkill = GetChild(boardParent, ["NextSkill", "LevelUpButtons", "btnMaxLevel"]).getComponent(Button)

        this.Refresh()
    }

    Refresh()
    {
        this.dragon = User.Instance.DragonData.GetDragon(DataManager.GetData("DragonInfo") as number)
        this.dragonBase = this.charTable.Get(this.dragon.Tag)
        this.newskilldata = null;

        if(this.myskilldata == null || this.myskilldata.KEY != this.skilltable.GetSkillByLevel(this.dragonBase.SKILL1, this.dragon.SLevel).KEY)
        {
            this.totalLevel = this.skilltable.Get(this.dragonBase.SKILL1).length
            this.dragonCharacter.init()
            this.dragonDesc.init()
            this.myskilldata = this.skilltable.GetSkillByLevel(this.dragonBase.SKILL1, this.dragon.SLevel)
            this.nextLevel = this.myskilldata.LEVEL+1
            this.CalculateMaxLevel()
        }

        this.labelMySkillLevel.string = StringBuilder("Lv.{0}", this.dragon.SLevel)
        this.labelMySkillName.string = this.stringTable.Get(this.myskilldata._NAME).TEXT
        this.labelMySkillDesc.string = this.stringTable.Get(this.myskilldata._DESC1).TEXT
        this.sprMySkillIcon.spriteFrame = this.resourceManager.GetResource(ResourcesType.SKILLICON_SPRITEFRAME)[this.myskilldata.ICON]
     
        if(this.totalLevel == this.myskilldata.LEVEL)
        {
            //스킬 만렙
            this.btnBefSkill.interactable = false
            this.btnNextSkill.interactable = false
            this.btnMaxSkill.interactable = false
            this.labelNewSkillLevel.string = StringBuilder("Lv.{0}", this.dragon.SLevel)
            this.labelNewSkillDesc.string = "최대 레벨입니다."
            this.sprNewSkillIcon.spriteFrame = this.resourceManager.GetResource(ResourcesType.SKILLICON_SPRITEFRAME)[this.myskilldata.ICON]
            this.RefreshButtonState([this.btnBefSkill, this.btnNextSkill,this.btnMaxSkill]);
            return
        }
        this.InitCalculateSkill()
    }

    InitCalculateSkill()
    {
        this.newskilldata = this.skilltable.GetKey(this.myskilldata.KEY)
        this.btnNextSkill.interactable = true
        this.btnMaxSkill.interactable = true
        this.btnBefSkill.interactable = true
        this.reqItems = {}
        this.reqCash = {}
        let i = this.myskilldata.LEVEL

        //최종 선택 레벨, 재료 정보
        while(this.calculateNextLevel())
        {
            if(i == this.nextLevel)
                break
        }

        this.labelNewSkillLevel.string = StringBuilder("Lv.{0}", this.newskilldata.LEVEL)
        this.labelSkillLevel.string = StringBuilder("Lv.{0}", this.newskilldata.LEVEL)

        //재료 아이템이 복수개로 늘었을 경우를 대비해 키 검사 및 재료 아이템 클론 생성
        {
            let keys = Object.keys(this.reqCash);
            this.nodeContent.removeAllChildren();

            keys.forEach((element : any) =>
            {
                let itemClone : Node = instantiate<Node>(this.resourceManager.GetResource<Node>(ResourcesType.UI_PREFAB)['item']);
                let type = eGoodType.NONE;

                switch(element)
                {
                    case "CASH" :
                        type = eGoodType.CASH;
                        break;

                    case "GOLD" :
                        type = eGoodType.GOLD;
                        break;
                }

                itemClone.getComponent(ItemFrame).setFrameCashInfo(type, this.reqCash[element]);
                itemClone.parent = this.nodeContent;
            });

            keys = Object.keys(this.reqItems);
            keys.forEach((element) =>
            {
                let itemClone : Node = instantiate<Node>(this.resourceManager.GetResource<Node>(ResourcesType.UI_PREFAB)['item']);
                itemClone.getComponent(ItemFrame).setFrameReceipeInfo(Number(element), this.reqItems[element]);

                itemClone.parent = this.nodeContent;
            });  
        }
        
        //this.labelNewSkillDesc.string = this.stringTable.Get(this.newskilldata._DESC1).TEXT
        //this.sprNewSkillIcon.spriteFrame = this.resourceManager.GetResource(ResourcesType.SKILLICON_SPRITEFRAME)[this.newskilldata.ICON]

        // +2 일경우 이전 버튼을 활성
        this.btnBefSkill.interactable = this.newskilldata.KEY != this.myskilldata.NEXT_LEVEL_SKILL && this.myskilldata.LEVEL != this.totalLevel
        this.btnMaxSkill.interactable = this.newskilldata.LEVEL < this.maxLevel && this.newskilldata.LEVEL != this.totalLevel
        this.btnNextSkill.interactable = this.newskilldata.LEVEL != this.totalLevel
        this.btnNextSkill.interactable =  this.newskilldata.LEVEL < this.dragon.Level;

        this.RefreshButtonState([this.btnBefSkill, this.btnNextSkill,this.btnMaxSkill]);
    }

    calculateNextLevel() : boolean
    {
        let next = this.skilltable.GetKey(this.newskilldata.NEXT_LEVEL_SKILL);
        if(this.newskilldata.NEXT_LEVEL_SKILL == 0 || this.nextLevel < next.LEVEL)
        {
            return false
        }

        if(this.reqItems[this.newskilldata.ITEM] == null)
            this.reqItems[this.newskilldata.ITEM] = this.newskilldata.ITEM_VALUE
        else
            this.reqItems[this.newskilldata.ITEM] += this.newskilldata.ITEM_VALUE

        if(this.reqCash[this.newskilldata.COST_TYPE] == null)
            this.reqCash[this.newskilldata.COST_TYPE] = this.newskilldata.COST_VALUE
        else
            this.reqCash[this.newskilldata.COST_TYPE] += this.newskilldata.COST_VALUE

        this.newskilldata = next;
        return true
    }

    CalculateMaxLevel()
    {
        let result : SkillCharData = this.skilltable.GetKey(this.myskilldata.KEY)
        let curLevel : number = this.myskilldata.LEVEL
        let needItems : {[id:string] : number} = {}
        let needCashs : {[id:string] : number} = {}
        let userItems : InventoryItem[] = User.Instance.GetAllItems()

        while(result != null && result.NEXT_LEVEL_SKILL != 0)
        {            
            if(needItems[result.ITEM] == null)
                needItems[result.ITEM] = result.ITEM_VALUE
            else
                needItems[result.ITEM] += result.ITEM_VALUE

            if(needCashs[result.COST_TYPE] == null)
                needCashs[result.COST_TYPE] = result.COST_VALUE
            else
                needCashs[result.COST_TYPE] += result.COST_VALUE

            let checkItems = function(userItems, needItems) : boolean
            {
                let costKeys = Object.keys(needItems)
                for(let i = 0; i < costKeys.length; i++)
                {
                    let targetItem = userItems.find(value => value.ItemNo == Number(costKeys[i]));
                    if(targetItem == null || targetItem == undefined || needItems[costKeys[i]] > targetItem.Count)
                    {
                        return false;
                    }                    
                }

                return true;
            }

            let checkCashes = function(needCashs) : boolean
            {
                let costKeys : string[] = Object.keys(needCashs)
                for(let i = 0; i < costKeys.length; i++)
                {
                    switch(costKeys[i])
                    {
                        case "GOLD":
                            if(needCashs[costKeys[i]] > User.Instance.GOLD)
                            {
                                return false;
                            }
                            break;
                    }
                }

                return true;
            }

            if(!checkCashes(needCashs) || !checkItems(userItems, needItems))
                break;

            curLevel = result.LEVEL+1
            result = this.skilltable.GetKey(result.NEXT_LEVEL_SKILL)
        }

        this.maxLevel = curLevel
    }

    onClickBefLevel()
    {
        this.nextLevel -= 1
        this.InitCalculateSkill()
    }

    onClickNewLevel()
    {
        this.nextLevel += 1
        this.InitCalculateSkill()
    }

    onClickMaxLevel()
    {
        this.nextLevel = this.maxLevel
        this.InitCalculateSkill()
    }

    onClickLevelUp()
    {
        let params = {
            did : this.dragon.GetDesignData().Index,
            targetlvl : this.nextLevel
        }
        
        NetworkManager.Send("dragon/skill", params, () =>
        {
            this.ForceUpdate()        
        })
    }

    ForceUpdate()
    {
        this.Refresh()
    }

    RefreshButtonState(buttonList : Button[])
    {
        if(buttonList == null || buttonList.length <= 0)
        {
            return;
        }

        buttonList.forEach(element=>{
            let component= element.node;
            if(component == null){
                return;
            }
            let changeLabelComp = component.getComponent(ButtonChangeChildColor);
            if(changeLabelComp != null){}
            changeLabelComp.refreshColor();
        })
    }
}
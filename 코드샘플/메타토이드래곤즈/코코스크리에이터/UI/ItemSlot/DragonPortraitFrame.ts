
import { _decorator, Component, Sprite, randomRange, Label, Node, SpriteFrame, Color } from 'cc';
import { CharBaseTable, CharGradeTable } from '../../Data/CharTable';
import { StringTable } from '../../Data/StringTable';
import { TableManager } from '../../Data/TableManager';
import { GameManager } from '../../GameManager';
import { ResourceManager, ResourcesType } from '../../ResourceManager';
import { GetChild } from '../../Tools/SandboxTools';
import { UserDragon } from '../../User/User';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = DragonPortraitFrame
 * DateTime = Thu Mar 10 2022 15:22:02 GMT+0900 (대한민국 표준시)
 * Author = wonjun_gs
 * FileBasename = DragonPortraitFrame.ts
 * FileBasenameNoExtension = DragonPortraitFrame
 * URL = db://assets/Scripts/UI/ItemSlot/DragonPortraitFrame.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('DragonPortraitFrame')
export class DragonPortraitFrame extends Component {
    @property({type : Color})
    public gradeColor : Color[] = [];

    @property(Sprite)
    sprFrame : Sprite = null

    @property(Sprite)
    sprIcon : Sprite = null

    @property(Label)
    levelNode : Label = null;

    @property(Sprite)
    elementIcon : Sprite = null;

    @property(Node)
    selectCheckNode : Node = null;//선택되어진 상태면 딤드 + 체크 처리

    private charGradeTable :CharGradeTable = null;
    private stringTable : StringTable = null;

    private bgSprite : Sprite = null;

    private clickCallback: (CustomEventData? : string) => void = null;

    private dragonID : number = 0
    /**
     * 슬릇 초기화 func
     * @param index index번호
     */

    SetDragonPortraitFrame( dragonData : UserDragon , isSelectCheck : boolean = false)
    {
        if(this.charGradeTable == null){
            this.charGradeTable = TableManager.GetTable<CharGradeTable>(CharGradeTable.Name);
        }
        if(this.stringTable == null){
            this.stringTable = TableManager.GetTable<StringTable>(StringTable.Name);
        }

        //index : number, level : number = 0, elementType : number = 0
        let level = dragonData.Level as number;
        let index = dragonData.Tag as number;
        let elementType = dragonData.Element as number;
        let grade = dragonData.GetDesignData().GRADE;
        if(this.selectCheckNode != null)
        {
            this.selectCheckNode.active = isSelectCheck;
        }

        if(this.levelNode != null)
        {
            if(level <= 0){
                this.levelNode.node.active = false;
            }
            else{
                this.levelNode.node.active = true;
            }

            let levelStr = level.toString();
            this.levelNode.string = levelStr;
        }

        if(this.elementIcon != null)
        {
            if(elementType <= 0){
                this.elementIcon.node.active = false;
            }
            else{
                this.elementIcon.node.active = true;
            }
            let elementFrame = this.GetElementIconSpriteByIndex(elementType);
            this.elementIcon.spriteFrame = elementFrame;
        }
        
        let dragonInfo = TableManager.GetTable<CharBaseTable>(CharBaseTable.Name).Get(index);
        let icon : any = null;
        if(dragonInfo != null){
            icon = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.CHARACTORICON_SPRITEFRAME)[dragonInfo.THUMBNAIL]
        
            if(icon == null){
                icon = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.CHARACTORICON_SPRITEFRAME)["character_01"]
            }
        }
        else{
            let randomStr = this.createDummyRandomDragon();
            icon = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.CHARACTORICON_SPRITEFRAME)[`character_0${randomStr}`]
        }

        if(this.bgSprite == null)
        {
            let bgNode = GetChild(this.node, ['Background']);
            if(bgNode != null){
                this.bgSprite = bgNode.getComponent(Sprite);
                this.bgSprite.spriteFrame = this.GetBGIconSpriteByIndex(grade,elementType);
            }
        }

        this.sprIcon.spriteFrame = icon
        this.dragonID = index;
        this.SetGradeFrameColor(Number(dragonInfo.GRADE));
    }

    SetGradeFrameColor(gradeIndex : number)//grade는 1부터 시작이니 -1 처리
    {
        if(this.gradeColor == null || this.gradeColor.length <= 0){
            return;
        }
        if(this.sprFrame == null){
            return;
        }

        this.sprFrame.color = this.gradeColor[gradeIndex - 1];
    }
    
    setCallback(ok_cb? : (CustomEventData? : string) => void)
    {
        //this.eFuncType = FrameFunctioal.CallBack;

        if(ok_cb != null)
        {
            this.clickCallback = ok_cb;
        }
    }
    onClickFrame()
    {   
        if(this.clickCallback != null){
            this.clickCallback(this.dragonID.toString());
        }
    }
    createDummyRandomDragon():string
    {
        let randomStr = Math.round(randomRange(1,5));
        return randomStr.toString();
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


    GetBGIconSpriteByIndex(grade : number , element : number) : SpriteFrame
    {
        let fullStr = this.MakeStringByGradeAndElement(grade, element);

        return GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.DRAGON_GRADETAG_SPRITEFRAME)[fullStr];
    }

    GetGradeConvertString(grade : number)
    {
        let gradeNameStrIndex = this.charGradeTable.Get(grade)._NAME;
        let gradeString = this.stringTable.Get(gradeNameStrIndex).TEXT.toLowerCase();
        return gradeString;
    }

    GetElementConvertString(e_type : number)
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
        return elementStr;
    }

    MakeStringByGradeAndElement(grade : number , element : number)
    {
        let elementString = this.GetElementConvertString(element);
        let gradeString = this.GetGradeConvertString(grade);


        return `${elementString}_${gradeString}_infobg`;
    }
    
}

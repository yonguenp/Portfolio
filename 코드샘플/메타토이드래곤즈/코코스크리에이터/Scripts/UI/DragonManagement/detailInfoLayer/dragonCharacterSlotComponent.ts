
import { _decorator, Component, Node, instantiate, Vec3, Layers, Sprite, Button, EventHandler, sp } from 'cc';
import { CharBaseTable, CharGradeTable } from '../../../Data/CharTable';
import { StringTable } from '../../../Data/StringTable';
import { TableManager } from '../../../Data/TableManager';
import { ResourceManager, ResourcesType } from '../../../ResourceManager';
import { DataManager } from '../../../Tools/DataManager';
import { ChangeLayer, RandomInt } from '../../../Tools/SandboxTools';
import { User, UserDragon } from '../../../User/User';
import { SubLayer } from '../../Common/SubLayer';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = dragonCharacterSlotComponent
 * DateTime = Fri Mar 18 2022 10:47:44 GMT+0900 (대한민국 표준시)
 * Author = wonjun_gs
 * FileBasename = dragonCharacterSlotComponent.ts
 * FileBasenameNoExtension = dragonCharacterSlotComponent
 * URL = db://assets/Scripts/UI/DragonManagement/detailInfoLayer/dragonCharacterSlotComponent.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('dragonCharacterSlotComponent')
export class dragonCharacterSlotComponent extends Component {
    
    @property(Node)
    currentTapLayerNode : Node = null;

    @property(Node)
    dragonImageParentNode : Node = null;

    @property(Sprite)
    dragonBGTarget : Sprite = null;

    charDataTable : CharBaseTable = null;
    stringTable: StringTable = null;
    charGradeTable :CharGradeTable = null;

    @property(Vec3)
    characterScale : Vec3 = new Vec3(4,4,4);

    @property(Vec3)
    characterPos : Vec3 = new Vec3(0,-125,0);

    clickBtn: Button = null;
    dragonSpine: sp.Skeleton = null;

    start() {
        if(this.dragonBGTarget != null) {
            this.clickBtn = this.dragonBGTarget.addComponent(Button);

            if(this.clickBtn != null) {
                let event = new EventHandler();
                event.target = this.node;
                event.component = "dragonCharacterSlotComponent"
                event.handler = "OnClickBG";
                this.clickBtn.clickEvents.push(event);
            }
        }
    }

    OnClickBG() {
        if(this.dragonSpine == null) {
            return;
        }

        if(this.dragonSpine.animation != 'dragon_idle') {
            return;
        }

        switch(RandomInt(0, 5)) {
            case 0: case 1: case 2: {
                this.dragonSpine.setAnimation(0, 'dragon_attack', false);
            } break;
            case 3: case 4: {
                this.dragonSpine.setAnimation(0, 'dragon_casting', false);
            } break;
        }
    }

    init()
    {
        if(this.charDataTable == null){
            this.charDataTable = TableManager.GetTable<CharBaseTable>(CharBaseTable.Name);
        }
        if(this.stringTable == null){
            this.stringTable = TableManager.GetTable<StringTable>(StringTable.Name);
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
            
            this.RefreshDragonCharacterSlot(userDragonInfo);
            this.RefreshDragonCharacterBG(userDragonInfo);
        }
    }
    RefreshDragonCharacterSlot(dragonInfo : UserDragon)
    {
        if(this.dragonImageParentNode == null){
            return;
        }

        this.dragonImageParentNode.removeAllChildren();
        let dragonImageName = this.GetDragonImageName(dragonInfo.Tag as number);
        if(dragonImageName != '')
        {
            let dragonPrefab = User.Instance.DragonData.GetNameDragonSpine(dragonImageName);
            let clone: Node = instantiate(dragonPrefab);
            clone.parent = this.dragonImageParentNode;
            
            clone.setPosition(this.characterPos);
            clone.setScale(this.characterScale);
            ChangeLayer(clone, 1 << Layers.nameToLayer("UI_2D"));
            this.dragonSpine = clone.getComponent(sp.Skeleton);
            if(this.dragonSpine != null) {
                this.dragonSpine.setCompleteListener((trackEntry) => {
                    switch(trackEntry.animation.name) {
                        case 'dragon_casting': {
                            this.dragonSpine.setAnimation(0, 'dragon_skill', false);
                        } break;
                        case 'dragon_attack':
                        case 'dragon_skill':
                        default: {
                            this.dragonSpine.setAnimation(0, 'dragon_idle', true);
                        } break;
                    }
                });
            }
        }
    }

    RefreshDragonCharacterBG(dragonInfo : UserDragon)
    {
        let designData = dragonInfo.GetDesignData();
        let dragonGrade = designData.GRADE;
        let dragonElement = designData.ELEMENT;

        let resourceString = this.MakeStringByGradeAndElement(dragonGrade, dragonElement);

        if(this.dragonBGTarget != null){
            let icon = ResourceManager.Instance.GetResource(ResourcesType.DRAGON_GRADETAG_SPRITEFRAME)[resourceString];
            if(icon == null){
                return;
            }
            this.dragonBGTarget.spriteFrame = icon;
        }
    }

    GetDragonImageName(dragonTag : number) : string
    {
        let dragonData = this.charDataTable.Get(dragonTag);
        if(dragonData != null){
            return dragonData.IMAGE;
        }
        return '';
    }

    onClickLeftCharcterButton()
    {
        let nextdragonTag = this.GetNextDragonIndex(false);
        if(nextdragonTag > 0){
            this.ResetAndRefreshDragonTagData(nextdragonTag);
        }
    }
    onClickRightCharcterButton()
    {
        let nextdragonTag = this.GetNextDragonIndex(true);
        if(nextdragonTag > 0){
            this.ResetAndRefreshDragonTagData(nextdragonTag);
        }
    }
    
    ResetAndRefreshDragonTagData(nextDragonTag : number)
    {
        if(DataManager.GetData("DragonInfo") != null)//드래곤 태그값
        {
            DataManager.DelData("DragonInfo");
        }
        DataManager.AddData("DragonInfo", nextDragonTag);//눌렀을 때 tag값 세팅 및 refresh

        let tapLayerComponent = this.currentTapLayerNode.getComponent(SubLayer);
        if(tapLayerComponent != null){
            tapLayerComponent.ForceUpdate();
        }
    }

    GetNextDragonIndex(isRight : boolean): number
    {
        let dragonList = User.Instance.DragonData.GetAllUserDragons();
        if(dragonList == null || dragonList.length <= 0)
        {
            return -1;
        }

        let indexArray : number[] = [];

        //일단 전투력 내림차순으로 정렬
        let tempBattleDescendList = dragonList.sort(function(a: UserDragon,b : UserDragon)
        {
            let aInf = a.GetDragonALLStat().INF;
            let bInf = b.GetDragonALLStat().INF;
            return bInf - aInf;
        });

        tempBattleDescendList.forEach((element)=>{
            let dragonID = Number(element.Tag);
            indexArray.push(dragonID);
        });

        let currentDragonTag =this.GetCurrentDragonTag();
        if(currentDragonTag < 0){
            return -1;
        }

        let currentIndex = indexArray.indexOf(Number(currentDragonTag));

        let modifyIndex = 0;
        //오른쪽 버튼 누르면
        if(isRight){
            modifyIndex = currentIndex + 1;
            if(modifyIndex == indexArray.length){
                modifyIndex = 0;
            }
        }
        else{
            modifyIndex = currentIndex - 1;
            if(modifyIndex < 0){
                modifyIndex = indexArray.length -1;
            }
        }

        return indexArray[modifyIndex];
    }

    GetCurrentDragonTag() : number
    {
        if(DataManager.GetData<number>("DragonInfo") != null)//드래곤 태그값
        {
            return DataManager.GetData("DragonInfo") as number;
        }
        else{
            return -1;
        }
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

    GetElementIcon(e_type : number)
    {
        let elementStr = this.GetElementConvertString(e_type);
        return ResourceManager.Instance.GetResource(ResourcesType.DRAGON_GRADETAG_SPRITEFRAME)[`type_${elementStr}`]
    }

    MakeStringByGradeAndElement(grade : number , element : number)
    {
        let elementString = this.GetElementConvertString(element);
        let gradeString = this.GetGradeConvertString(grade);


        return `${elementString}_sr_bg`;//등급별 리소스는 추후 추가라 일단 sr 하드 코딩
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

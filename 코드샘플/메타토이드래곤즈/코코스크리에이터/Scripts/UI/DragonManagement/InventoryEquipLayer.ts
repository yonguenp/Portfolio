
import { _decorator, Node, instantiate, Prefab, Event,CCString, Label, ScrollView } from 'cc';
import { PartTable } from '../../Data/PartTable';
import { StringTable } from '../../Data/StringTable';
import { TableManager } from '../../Data/TableManager';
import { GameManager } from '../../GameManager';
import { ResourceManager, ResourcesType } from '../../ResourceManager';
import { TutorialManager } from '../../Tutorial/TutorialManager';
import { PartData, User, UserPart} from '../../User/User';
import { InventoryLayer } from '../BuildingManagement/InventoryLayer';
import { SubLayer } from '../Common/SubLayer';
import { PartSlotFrame } from '../ItemSlot/PartSlotFrame';
import { ToastMessage } from '../ToastMessage';
import { dragonCharacterSlotComponent } from './detailInfoLayer/dragonCharacterSlotComponent';
import { dragonEquipmentSlotComponent } from './detailInfoLayer/dragonEquipmentSlotComponent';
import { dragonStatComponent } from './detailInfoLayer/dragonStatComponent';
import { dragonEquipDetailInfoComponent } from './equipItemLayer/dragonEquipDetailInfoComponent';
import { InventoryEquipDetailComponent } from './InventoryEquipDetailComponent';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = InventoryEquipLayer
 * DateTime = Thu Mar 10 2022 14:14:24 GMT+0900 (대한민국 표준시)
 * Author = wonjun_gs
 * FileBasename = InventoryEquipLayer.ts
 * FileBasenameNoExtension = InventoryEquipLayer
 * URL = db://assets/Scripts/UI/DragonManagement/InventoryEquipLayer.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('InventoryEquipLayer')
export class InventoryEquipLayer extends SubLayer {
    @property(Node)
    nodeInvenContent : Node = null

    @property(ScrollView)
    scrollInven : ScrollView = null

    @property(dragonCharacterSlotComponent)
    dragonCharacter : dragonCharacterSlotComponent = null;
    
    @property(dragonEquipmentSlotComponent)
    dragonEquipSlot : dragonEquipmentSlotComponent = null;

    @property(dragonStatComponent)
    dragonStat : dragonStatComponent = null;

    @property(dragonEquipDetailInfoComponent)
    dragonPartInfo : dragonEquipDetailInfoComponent = null;

    @property(Node)
    sortDropdown : Node = null;
    
    @property(Label)
    sortButtonLabel : Label;
    
    @property(CCString)
    sortinitLabelStr : string;

    partFrameClone : Prefab;

    Init()
    {
        this.initDropDown();
        //this.drawPartList();//initCustomSort에서 기본 값 세팅
        this.initCustomSort();

        if(this.dragonCharacter != null){
            this.dragonCharacter.init();
        }
        if(this.dragonEquipSlot != null){
            this.dragonEquipSlot.init();
        }
        if(this.dragonStat != null){
            this.dragonStat.init();
        }
        if(this.dragonPartInfo != null)
        {
            //this.dragonPartInfo.init();            
            this.dragonPartInfo.HideDetailInfo();
        }
    }
    
    //정렬 기본 형태 제작
    drawPartList()
    {
        this.nodeInvenContent.removeAllChildren()
        let partList = User.Instance.partData.GetAllUserParts();
        
        this.createDragonByUserList(partList);
    }

    createDragonByUserList(partList : UserPart[])
    {
        this.nodeInvenContent.removeAllChildren();
        
        this.partFrameClone = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB)["partslot"];

        for(let i = 0; i < partList.length; i++)
        {
            let partTableInfo : PartData = TableManager.GetTable(PartTable.Name).Get(partList[i].ID);
            if(partTableInfo == null)
            {
                continue;
            }
            let userPartInfo = partList[i];
            let partTag = userPartInfo.Tag as number;
            let partLevel = userPartInfo.Level as number;

            // let userPartCheck = this.isDragonPartBelonging(partTag);
            // //드래곤이 장착된 상태면 표시 안함
            // if(userPartCheck){
            //     continue;
            // }

            let clone : Node = instantiate(this.partFrameClone);
            clone.parent = this.nodeInvenContent;
            clone.getComponent(PartSlotFrame).SetPartSlotFrame(partTag, partLevel);
            clone.getComponent(PartSlotFrame).setCallback((param)=>{
                //console.log('check slot');
                if(this.dragonPartInfo != null)
                {
                    this.dragonPartInfo.ShowDetailInfo(Number(param));
                }
            });
        }
    }

    //드래곤이 장착한 상태라면 표시 안하게 
    isDragonPartBelonging(partTag : number) :boolean
    {
        let dragonTag = User.Instance.partData.GetPartLink(partTag);//드래곤이 장착 상태인지 체크
        return dragonTag > 0;
    }

    ForceUpdate()
    {
        this.initDropDown();
        this.drawPartList();
        this.initCustomSort();

        if(this.dragonCharacter != null){
            this.dragonCharacter.init();
        }
        if(this.dragonEquipSlot != null){
            this.dragonEquipSlot.init();
        }
        if(this.dragonStat != null){
            this.dragonStat.init();
        }
        if(this.dragonPartInfo != null)
        {
            //this.dragonPartInfo.init();            
            this.dragonPartInfo.HideDetailInfo();
        }
    }
    initDropDown()
    {
        if(this.sortDropdown.activeInHierarchy){
            this.sortDropdown.active = false;
        }
    }

    initCustomSort()
    {
        //this.sortinitLabelStr = this.sortButtonLabel.string;
        let tempSortIndex ={
            index : 0,
        }
        this.onClickSortButton(null, JSON.stringify(tempSortIndex));
    }

    onClickVisibleDropDown()
    {
        this.sortDropdown.active = !this.sortDropdown.active
    }

    RefreshButtonLableByEventData(event : Event)
    {
        if(event != null)
        {
            let selectButtonNode = event.currentTarget as Node;
            if(selectButtonNode != null)
            {
                let childbuttonLabel = selectButtonNode.getComponentInChildren(Label);
                if(this.sortButtonLabel != null){
                    this.sortButtonLabel.string = childbuttonLabel.string;
                }
            }
        }
        else{
            if(this.sortButtonLabel != null){
                this.sortButtonLabel.string = this.sortinitLabelStr;
            }
        }
    }
    onClickSortButton(event : Event, customEventData)
    {
        this.RefreshButtonLableByEventData(event);//클릭한 버튼을 가지고 상단 라벨 string 변경

        let checker = JSON.parse(customEventData);
        let sortIndex = checker.index as number;

        let partList = User.Instance.partData.GetAllUserParts();
        if(partList == null || partList.length <= 0){
            this.initDropDown();//일단 임시로 끄기
            return;
        }

        //유저리스트, 빈 장비 리스트 분리 - 기본 빈 장비가 우선
        let dragonPartList : UserPart[] = [];
        let emptyPartList : UserPart[] = [];

        partList.forEach((Element)=>{
            if(Element == null){
                return;
            }

            let isbelonged = (Element.belongingDragonTag > 0);//-1또는 0이면 귀속
            if(isbelonged){
                dragonPartList.push(Element);
            }
            else{
                emptyPartList.push(Element);
            }
        });
        
        let sortDragonPartList = this.makeListBySortCondition(dragonPartList,sortIndex);
        let sortEmptyPartList = this.makeListBySortCondition(emptyPartList,sortIndex);

        let totalList = sortEmptyPartList.concat(sortDragonPartList);//귀속 드래곤을 전부 뒤로 뺀 결과 array

        this.createDragonByUserList(totalList);
        this.initDropDown();//일단 임시로 끄기
    }
    
    /**
     * sortIndex 
     * 0 : 강화 > 등급 > 최신 - 내림차순
     * 1 : 강화 > 등급 > 최신 - 오름차순
     * 2 : 강화 > 최신 > 등급 - 내림차순
     * 3 : 강화 > 최신 > 등급 - 오름차순
     */
    makeListBySortCondition(partList : UserPart[], sortIndex :number) : UserPart[]
    {
        let mapped = partList.map(function(dragonData, i) {
            return { index: i, value: dragonData };
          });
          switch(sortIndex)
          {
              case 0://init 시에 기본형 소팅형태
              {
                mapped.sort(function(a,b){
                    let checker = InventoryEquipLayer.SortLevelDescend(a,b);
                    if(checker == 0){
                        let underChecker = InventoryEquipLayer.SortGradeDescend(a,b);
                        if(underChecker == 0){
                        return InventoryEquipLayer.SortObtainTimeDescend(a,b);
                        }
                        else{
                            return underChecker;
                        }
                    }
                    else{
                        return checker;
                    }
                });
              }
              break;
              case 1:
              {
                mapped.sort(function(a,b){
                    let checker = InventoryEquipLayer.SortLevelAscend(a,b);
                    if(checker == 0){
                        let underChecker = InventoryEquipLayer.SortGradeAscend(a,b);
                        if(underChecker == 0){
                          return InventoryEquipLayer.SortObtainTimeAscend(a,b);
                        }
                        else{
                            return underChecker;
                        }
                    }
                    else{
                        return checker;
                    }
                });
              }
              break;
              case 2:
              {
                mapped.sort(function(a,b){
                    let checker = InventoryEquipLayer.SortLevelDescend(a,b);
                    if(checker == 0){
                        let underChecker = InventoryEquipLayer.SortObtainTimeDescend(a,b);
                        if(underChecker == 0){
                          return InventoryEquipLayer.SortGradeDescend(a,b);
                        }
                        else{
                            return underChecker;
                        }
                    }
                    else{
                        return checker;
                    }
                });
              }
              break;
              case 3:
              {
                mapped.sort(function(a,b){
                    let checker = InventoryEquipLayer.SortLevelAscend(a,b);
                    if(checker == 0){
                        let underChecker = InventoryEquipLayer.SortObtainTimeAscend(a,b);
                        if(underChecker == 0){
                          return InventoryEquipLayer.SortGradeAscend(a,b);
                        }
                        else{
                            return underChecker;
                        }
                    }
                    else{
                        return checker;
                    }
                });
              }
              break;
              default :
              {
  
              }
              break;
          }
          // 결과 순서를 위한 컨테이너
          let result = mapped.map(function(el){
              return partList[el.index];
          });

          return result;
    }

    //등급 내림차순
    static SortGradeDescend(param_a : {index : number, value : UserPart},param_b :{index : number, value : UserPart}): number
    {
        let aGrade = param_a.value.Grade;
        let bGrade = param_b.value.Grade;
        return bGrade - aGrade;
    }
    //등급 오름차순
    static SortGradeAscend(param_a : {index : number, value : UserPart},param_b :{index : number, value : UserPart})
    {
        let aGrade = param_a.value.Grade;
        let bGrade = param_b.value.Grade;
        return aGrade - bGrade;
    }
    //강화레벨 내림차순
    static SortLevelDescend(param_a : {index : number, value : UserPart},param_b :{index : number, value : UserPart})
    {
        let aLevel = param_a.value.Level;
        let bLevel = param_b.value.Level;
        return bLevel - aLevel;
    }
    //강화레벨 오름차순
    static SortLevelAscend(param_a : {index : number, value : UserPart},param_b :{index : number, value : UserPart})
    {
        let aLevel = param_a.value.Level;
        let bLevel = param_b.value.Level;
        return aLevel - bLevel;
    }
    //최신 획득 내림 차순
    static SortObtainTimeDescend(param_a : {index : number, value : UserPart},param_b :{index : number, value : UserPart})
    {
        let aObtainTime = param_a.value.Obtain as number;
        let bObtaionTime = param_b.value.Obtain as number;
        return bObtaionTime - aObtainTime;
    }
    //최신 획득 오름 차순
    static SortObtainTimeAscend(param_a : {index : number, value : UserPart},param_b :{index : number, value : UserPart})
    {
        let aObtainTime = param_a.value.Obtain as number;
        let bObtaionTime = param_b.value.Obtain as number;
        return aObtainTime - bObtaionTime;
    }

    /**
     * //업데이트 기대해달라는 문구
     */
     onClickExpectGameAlphaUpdate()
     {
         let emptyCheck = ToastMessage.isToastEmpty();
         if(emptyCheck == false){
             return;
         }
          
         let textData = TableManager.GetTable<StringTable>(StringTable.Name).Get(100000326);
         if(textData != null)
         {
             ToastMessage.Set(textData.TEXT, null, -52);
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

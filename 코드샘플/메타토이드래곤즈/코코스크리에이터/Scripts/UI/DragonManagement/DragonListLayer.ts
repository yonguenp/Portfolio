
import { _decorator, Node, instantiate, Prefab, Event, Label, CCString } from 'cc';
import { CharBaseTable } from '../../Data/CharTable';
import { StringTable } from '../../Data/StringTable';
import { TableManager } from '../../Data/TableManager';
import { GameManager } from '../../GameManager';
import { ResourceManager, ResourcesType } from '../../ResourceManager';
import { DataManager } from '../../Tools/DataManager';
import { GetDragonStat } from '../../Tools/SandboxTools';
import { TutorialManager } from '../../Tutorial/TutorialManager';
import { User, UserDragon } from '../../User/User';
import { SubLayer } from '../Common/SubLayer';
import { DragonManagePopup } from '../DragonManagePopup';
import { DragonPortraitFrame } from '../ItemSlot/DragonPortraitFrame';
import { ToastMessage } from '../ToastMessage';
import { DragonReserveLayer } from './DragonReserveLayer';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = DragonListLayer
 * DateTime = Thu Mar 10 2022 14:03:29 GMT+0900 (대한민국 표준시)
 * Author = wonjun_gs
 * FileBasename = DragonListLayer.ts
 * FileBasenameNoExtension = DragonListLayer
 * URL = db://assets/Scripts/UI/DragonManagement/DragonListLayer.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('DragonListLayer')
export class DragonListLayer extends SubLayer {
    
    @property(DragonReserveLayer)
    DragonTap : DragonReserveLayer = null;

    @property(Node)
    nodeFrameContent : Node = null
    
    @property(Node)
    sortDropdown : Node = null

    @property(Label)
    sortButtonLabel : Label;

    @property(CCString)
    sortinitLabelStr : string;

    dragonFrameClone : Prefab = null

    charDataTable : CharBaseTable = null;

    @property(Node)
    buttonNodeList : Node[] = [];

    currentClickSortIndex : number = -1;

    Init()
    {
        //super.init();
        if(this.charDataTable == null){
            this.charDataTable = TableManager.GetTable<CharBaseTable>(CharBaseTable.Name);
        }

        this.initDragonInfoData();
        this.initDropDown();
        //this.drawInven();//initCustomSort에서 기본 세팅
        this.initCurrenctClickSortIndex();
        this.initCustomSort();

        // 튜토리얼 실행
        TutorialManager.GetInstance.OnTutorialEvent(109, 4);
        TutorialManager.GetInstance.OnTutorialEvent(114, 6);
        TutorialManager.GetInstance.OnTutorialEvent(116, 4);
    }
    drawInven()
    {
        let dragonsArray = this.getUserDragonList();
        if(dragonsArray.length <= 0){
            console.log("user Dragon List count is 0");
            //this.createDummyPortrait();
            return;
        }
        else{
            this.createDragonByUserList(dragonsArray);
        }
    }
    initDragonInfoData()
    {
        if(DataManager.GetData("DragonInfo") != null)
        {
            DataManager.DelData("DragonInfo");
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
            index : this.currentClickSortIndex,
        }
        this.onClickCustomSort(null, JSON.stringify(tempSortIndex));
        this.RefreshButtonLabelForce(this.currentClickSortIndex);
    }

    initCurrenctClickSortIndex()
    {
        let dragonClickIndex = DataManager.GetData("DragonListClickIndex") as number;
        if(DataManager.GetData("DragonListClickIndex") != null)
        {
            this.currentClickSortIndex = dragonClickIndex;
        }
        else{
            this.currentClickSortIndex = 0;
        }
    }

    setCurrentClickSortIndex(sortIndex : number)
    {
        if(DataManager.GetData("DragonListClickIndex") != null)
        {
            DataManager.DelData("DragonListClickIndex");
        }
        DataManager.AddData("DragonListClickIndex", sortIndex);
        this.currentClickSortIndex = sortIndex;
    }

    forceUpdate()
    {
        this.drawInven();
        //this.deActivateBtn()
    }
    onClickChangeSort()
    {
        this.sortDropdown.active = !this.sortDropdown.active
    }

    getUserDragonList() : UserDragon[]
    {
        return User.Instance.DragonData.GetAllUserDragons();
    }

    createDummyPortrait()
    {
        // this.nodeFrameContent.removeAllChildren()
        // this.dragonFrameClone = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB)["DragonPortraitSlot"]

        // let clone : Node = instantiate(this.dragonFrameClone);
        // clone.parent = this.nodeFrameContent

        // clone.getComponent(DragonPortraitFrame).SetDragonPortraitFrame(101001);
        // clone.getComponent(DragonPortraitFrame).setCallback((param)=>{
        //     DataManager.AddData("DragonInfo",param);
        //     this.DragonTap.moveLayer({"index" : 1});
        // });
    }

    addDummyUserDragonData()
    {
        let targetData = new UserDragon();
        targetData.Level = 2;
        targetData.Tag = 101001;
        targetData.Exp = 20;

        User.Instance.DragonData.AddUserDragon(targetData.Tag,targetData);
    }
    createDragonByUserList(dragonList : UserDragon[])
    {
        this.nodeFrameContent.removeAllChildren();
        
        dragonList.forEach((element)=>{
            let dragonTag = element.Tag;
            let dragonlevel = element.Level;
            let dragonTableData = this.charDataTable.Get(dragonTag);
            let elementType = 0;
            if(dragonTableData != null){
                elementType = this.charDataTable.Get(dragonTag).ELEMENT;
            }
            this.dragonFrameClone = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB)["DragonPortraitSlot"]

            let clone : Node = instantiate(this.dragonFrameClone);
            clone.parent = this.nodeFrameContent;

            clone.getComponent(DragonPortraitFrame).SetDragonPortraitFrame(element);
            clone.getComponent(DragonPortraitFrame).setCallback((param)=>{
            
                DataManager.AddData("DragonInfo",param);
                this.DragonTap.moveLayer({"index" : 1});
            });
        });
    }

    /**
     * @param param 소트 타입 으로 제공
     * 0 : all
     * 1 : fire
     * 2 : water
     * 3 : soil
     * 4 : wind
     * 5 : light
     * 6 : dark
     */
    onClickElementSort(param,customEventData)
    {
        let checker = JSON.parse(customEventData);
        let elementIndex = checker.index;
        
        if(elementIndex == 0){
            //this.createDragonByUserList(allDragonList);
            //this.onClickCustomSort(null,JSON.stringify({"index" : 0}));
            this.initCustomSort();
        }
        else{
            //현재 전체 정렬방식(등급순 전투력순 등등)인덱스 기준으로 정렬을 먼저 한번 침
            let result = this.GetListCustomSort(this.currentClickSortIndex);
            let modDragonList = result.filter((Element)=>Element.Element == elementIndex);
            this.createDragonByUserList(modDragonList);
        }
    }

/**
 * 
 * @param param 
 * @param customEventData 
 * 
 * 정렬 타입
 * 0 : 등급 내림차순 (default)
 * 1 : 등급 오름차순
 * 2 : 레벨 내림차순
 * 3 : 레벨 오름차순
 * 4 : 전투력 내림차순
 * 5 : 전투력 오름차순
 * 6 : 최신 획득 내림 차순
 * 7 : 최신 획득 오름 차순
 */
    onClickCustomSort(event : Event, customEventData)
    {
        this.RefreshButtonLableByEventData(event);//클릭한 버튼을 가지고 상단 라벨 string 변경

        let checker = JSON.parse(customEventData);
        let sortIndex = checker.index as number;

        this.setCurrentClickSortIndex(sortIndex);//현재 클릭한 정렬인덱스 글로벌 저장

        let result = this.GetListCustomSort(sortIndex);//클릭인덱스 기준 정렬 완료 데이터 받아오기

        this.createDragonByUserList(result);
        this.initDropDown();//일단 임시로 끄기
    }

    GetListCustomSort(sortIndex : number){
        //소팅 구성 데이터 map 세팅 - 소팅 하기전 기본 map 형태
        let allDragonList = this.getUserDragonList();//일단 전체 리스트
        let mapped = allDragonList.map(function(dragonData, i) {
            return { index: i, value: dragonData };
          });

        switch(sortIndex)
        {
            case 0://init 시에 기본형 소팅형태
            {
                mapped.sort(function(a,b){
                    let checker = DragonListLayer.SortGradeDescend(a,b);
                    if(checker == 0){
                        let underChecker = DragonListLayer.SortLevelDescend(a,b);
                        if(underChecker == 0){
                            let underCheckerLower = DragonListLayer.SortBattlePointDescend(a,b);
                            if(underCheckerLower == 0){
                                return DragonListLayer.SortObtainTimeDescend(a,b);
                            }
                            else{
                                return underCheckerLower;
                            }
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
                    let checker = DragonListLayer.SortGradeAscend(a,b);
                    if(checker == 0){
                        let underChecker = DragonListLayer.SortLevelAscend(a,b);
                        if(underChecker == 0){
                            let underCheckerLower = DragonListLayer.SortBattlePointAscend(a,b);
                            if(underCheckerLower == 0){
                                return DragonListLayer.SortObtainTimeAscend(a,b);
                            }
                            else{
                                return underCheckerLower;
                            }
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
                    let checker = DragonListLayer.SortLevelDescend(a,b);
                    if(checker == 0){
                        let underChecker = DragonListLayer.SortGradeDescend(a,b);
                        if(underChecker == 0){
                            let underCheckerLower = DragonListLayer.SortBattlePointDescend(a,b);
                            if(underCheckerLower == 0){
                                return DragonListLayer.SortObtainTimeDescend(a,b);
                            }
                            else{
                                return underCheckerLower;
                            }
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
                    let checker = DragonListLayer.SortLevelAscend(a,b);
                    if(checker == 0){
                        let underChecker = DragonListLayer.SortGradeAscend(a,b);
                        if(underChecker == 0){
                            let underCheckerLower = DragonListLayer.SortBattlePointAscend(a,b);
                            if(underCheckerLower == 0){
                                return DragonListLayer.SortObtainTimeAscend(a,b);
                            }
                            else{
                                return underCheckerLower;
                            }
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
            case 4:
            {
                mapped.sort(function(a,b){
                    let checker = DragonListLayer.SortBattlePointDescend(a,b);
                    if(checker == 0){
                        let underChecker = DragonListLayer.SortGradeDescend(a,b);
                        if(underChecker == 0){
                            let underCheckerLower = DragonListLayer.SortLevelDescend(a,b);
                            if(underCheckerLower == 0){
                                return DragonListLayer.SortObtainTimeDescend(a,b);
                            }
                            else{
                                return underCheckerLower;
                            }
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
            case 5:
            {
                mapped.sort(function(a,b){
                    let checker = DragonListLayer.SortBattlePointAscend(a,b);
                    if(checker == 0){
                        let underChecker = DragonListLayer.SortGradeAscend(a,b);
                        if(underChecker == 0){
                            let underCheckerLower = DragonListLayer.SortLevelAscend(a,b);
                            if(underCheckerLower == 0){
                                return DragonListLayer.SortObtainTimeAscend(a,b);
                            }
                            else{
                                return underCheckerLower;
                            }
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
            case 6://최신순은 물어볼것 - 임시로 등급과 같은 결과 처리
            {
                mapped.sort(function(a,b){
                    let checker = DragonListLayer.SortObtainTimeDescend(a,b);
                    if(checker == 0){
                        let underChecker = DragonListLayer.SortGradeDescend(a,b);
                        if(underChecker == 0){
                            let underCheckerLower = DragonListLayer.SortLevelDescend(a,b);
                            if(underCheckerLower == 0){
                                return DragonListLayer.SortBattlePointDescend(a,b);
                            }
                            else{
                                return underCheckerLower;
                            }
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
            case 7:
            {
                mapped.sort(function(a,b){
                    let checker = DragonListLayer.SortObtainTimeAscend(a,b);
                    if(checker == 0){
                        let underChecker = DragonListLayer.SortGradeAscend(a,b);
                        if(underChecker == 0){
                            let underCheckerLower = DragonListLayer.SortLevelAscend(a,b);
                            if(underCheckerLower == 0){
                                return DragonListLayer.SortBattlePointAscend(a,b);
                            }
                            else{
                                return underCheckerLower;
                            }
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
            return allDragonList[el.index];
        });
        return result;
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

    RefreshButtonLabelForce(index : number)
    {
        if(this.buttonNodeList == null || this.buttonNodeList.length <= 0)
        {
            if(this.sortButtonLabel != null){
                this.sortButtonLabel.string = this.sortinitLabelStr;
            }
        }

        let length = this.buttonNodeList.length;
        if(length > index)
        {
            let childbuttonLabel = this.buttonNodeList[index].getComponentInChildren(Label);
            if(this.sortButtonLabel != null){
                this.sortButtonLabel.string = childbuttonLabel.string;
            }
        }
    }

    //등급 내림차순
    static SortGradeDescend(param_a : {index : number, value : UserDragon},param_b :{index : number, value : UserDragon}): number
    {
        let aGrade = param_a.value.Grade;
        let bGrade = param_b.value.Grade;
        return bGrade - aGrade;
    }
    //등급 오름차순
    static SortGradeAscend(param_a : {index : number, value : UserDragon},param_b :{index : number, value : UserDragon})
    {
        let aGrade = param_a.value.Grade;
        let bGrade = param_b.value.Grade;
        return aGrade - bGrade;
    }
    //레벨 내림차순
    static SortLevelDescend(param_a : {index : number, value : UserDragon},param_b :{index : number, value : UserDragon})
    {
        let aLevel = param_a.value.Level;
        let bLevel = param_b.value.Level;
        return bLevel - aLevel;
    }
    //레벨 오름차순
    static SortLevelAscend(param_a : {index : number, value : UserDragon},param_b :{index : number, value : UserDragon})
    {
        let aLevel = param_a.value.Level;
        let bLevel = param_b.value.Level;
        return aLevel - bLevel;
    }
    //전투력 내림차순
    static SortBattlePointDescend(param_a : {index : number, value : UserDragon},param_b :{index : number, value : UserDragon})
    {
        let aTag = param_a.value.Tag as number;
        let aLevel = param_a.value.Level;

        let bTag = param_b.value.Tag as number;
        let bLevel = param_b.value.Level;

        let adragonStat = GetDragonStat(aTag , aLevel);
        let bdragonStat = GetDragonStat(bTag , bLevel);

        let aInf = 0;
        let bInf = 0;

        if(adragonStat != null && adragonStat.INF != null){
            aInf = adragonStat.INF;
        }
        if(bdragonStat != null && bdragonStat.INF != null){
            bInf = bdragonStat.INF;
        }
        return bInf - aInf;
    }
    //전투력 오름차순
    static SortBattlePointAscend(param_a : {index : number, value : UserDragon},param_b :{index : number, value : UserDragon})
    {
        let aTag = param_a.value.Tag as number;
        let aLevel = param_a.value.Level;

        let bTag = param_b.value.Tag as number;
        let bLevel = param_b.value.Level;

        let adragonStat = GetDragonStat(aTag , aLevel);
        let bdragonStat = GetDragonStat(bTag , bLevel);

        let aInf = 0;
        let bInf = 0;

        if(adragonStat != null && adragonStat.INF != null){
            aInf = adragonStat.INF;
        }
        if(bdragonStat != null && bdragonStat.INF != null){
            bInf = bdragonStat.INF;
        }
        return aInf - bInf;
    }
    //최신 획득 내림 차순
    static SortObtainTimeDescend(param_a : {index : number, value : UserDragon},param_b :{index : number, value : UserDragon})
    {
        let aObtainTime = param_a.value.Obtain as number;
        let bObtaionTime = param_b.value.Obtain as number;
        return bObtaionTime - aObtainTime;
    }
    //최신 획득 오름 차순
    static SortObtainTimeAscend(param_a : {index : number, value : UserDragon},param_b :{index : number, value : UserDragon})
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

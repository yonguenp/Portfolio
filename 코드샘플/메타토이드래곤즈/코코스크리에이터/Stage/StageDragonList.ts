
import { _decorator, Component, Label, CCString, Prefab, instantiate, Node, Event } from 'cc';
import { CharBaseTable } from '../Data/CharTable';
import { StringTable } from '../Data/StringTable';
import { TableManager } from '../Data/TableManager';
import { GameManager } from '../GameManager';
import { ResourceManager, ResourcesType } from '../ResourceManager';
import { GetDragonStat } from '../Tools/SandboxTools';
import { DragonPortraitFrame } from '../UI/ItemSlot/DragonPortraitFrame';
import { ToastMessage } from '../UI/ToastMessage';
import { User, UserDragon } from '../User/User';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = StageDragonList
 * DateTime = Thu Mar 31 2022 19:00:44 GMT+0900 (대한민국 표준시)
 * Author = wonjun_gs
 * FileBasename = StageDragonList.ts
 * FileBasenameNoExtension = StageDragonList
 * URL = db://assets/Scripts/Stage/StageDragonList.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('StageDragonList')
export class StageDragonList extends Component {

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

    @property(Node)
    stagePrepareNode : Node = null;

    dragonTagList : number[];

    tempElementSortIndex :number = 0;
    tempCustomSortIndex : number = 0;

    private clickRegistCallback: (CustomEventData? : string) => void = null;
    setRegistCallback(ok_cb? : (CustomEventData? : string) => void)
    {
        if(ok_cb != null)
        {
            this.clickRegistCallback = ok_cb;
        }
    }
    private clickReleaseCallback: (CustomEventData? : string) => void = null;
    setReleaseCallback(ok_cb? : (CustomEventData? : string) => void)
    {
        if(ok_cb != null)
        {
            this.clickReleaseCallback = ok_cb;
        }
    }

    onShowList()
    {
        if(this.node.activeInHierarchy == false){
            this.node.active = true;
        }
    }

    onHideList()
    {
        if(this.node.activeInHierarchy == true){
            this.node.active = false;
        }
    }

    isShowList() : boolean
    {
        return this.node.activeInHierarchy;
    }

    init(dragonTagList : number[])
    {
        //super.init();
        if(this.charDataTable == null){
            this.charDataTable = TableManager.GetTable<CharBaseTable>(CharBaseTable.Name);
        }

        this.initSortIndex();
        this.initDropDown();
        this.initDragonTagList(dragonTagList);
        this.initCustomSort();
    }

    RefreshList(dragonTagList : number[])
    {
        this.initDropDown();
        this.initDragonTagList(dragonTagList);

        if(this.tempElementSortIndex > 0)
        {
            let tempSortIndex ={
                index : this.tempElementSortIndex,
            }

            this.onClickElementSort(null, JSON.stringify(tempSortIndex));
        }
        else
        {
            let tempSortIndex ={
                index : this.tempCustomSortIndex,
            }
            this.onClickCustomSort(null, JSON.stringify(tempSortIndex));
            this.RefreshButtonLabelForce(this.tempCustomSortIndex);
        }
    }

    drawInven()
    {
        let dragonsArray = this.getUserDragonList();
        this.createDragonByUserList(dragonsArray);
    }
    
    initDragonTagList(dragonTagList : number[])
    {
        this.dragonTagList = [];
        this.dragonTagList = dragonTagList;
    }

    isInTeamDragon(tag :number) : boolean
    {
        if(this.dragonTagList == null || this.dragonTagList.length <= 0)
        {
            return false;
        }
        let length = this.dragonTagList.length;

        for(let i = 0; i < length ; i++)
        {
            let tempTag = this.dragonTagList[i];
            if(tempTag == tag){
                return true;
            }
        }
        return false;
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
        this.onClickCustomSort(null, JSON.stringify(tempSortIndex));
    }

    initSortIndex()
    {
        this.tempCustomSortIndex = 0;
        this.tempElementSortIndex = 0;
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

            let isRegist = this.isInTeamDragon(Number(dragonTag));

            clone.getComponent(DragonPortraitFrame).SetDragonPortraitFrame(element, isRegist);
            clone.getComponent(DragonPortraitFrame).setCallback((param)=>{
                let check = this.isInTeamDragon(Number(param));//현재 드래곤 상태 체크 (등록된 상태, 미등록 상태)해서 처리
                if(check)
                {
                    if(this.clickReleaseCallback != null)
                    {
                        this.clickReleaseCallback(dragonTag.toString());
                    }
                }
                else{
                    if(this.clickRegistCallback != null)
                    {
                        this.clickRegistCallback(dragonTag.toString());
                    }
                }
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
        this.tempElementSortIndex = elementIndex;
        //this.tempCustomSortIndex = 0;
        
        if(elementIndex == 0){
            //this.createDragonByUserList(allDragonList);
            this.onClickCustomSort(null,JSON.stringify({"index" : this.tempCustomSortIndex}));
            this.RefreshButtonLabelForce(this.tempCustomSortIndex);
        }
        else{
            //현재 전체 정렬방식(등급순 전투력순 등등)인덱스 기준으로 정렬을 먼저 한번 침
            let result = this.GetListCustomSort(this.tempCustomSortIndex);
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
        this.RefreshButtonLabelByEventData(event);//클릭한 버튼을 가지고 상단 라벨 string 변경

        let checker = JSON.parse(customEventData);
        let sortIndex = checker.index as number;

        this.tempCustomSortIndex = sortIndex;
        this.tempElementSortIndex = 0;

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
                    let checker = StageDragonList.SortGradeDescend(a,b);
                    if(checker == 0){
                        let underChecker = StageDragonList.SortLevelDescend(a,b);
                        if(underChecker == 0){
                            let underCheckerLower = StageDragonList.SortBattlePointDescend(a,b);
                            if(underCheckerLower == 0){
                                return StageDragonList.SortObtainTimeDescend(a,b);
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
                    let checker = StageDragonList.SortGradeAscend(a,b);
                    if(checker == 0){
                        let underChecker = StageDragonList.SortLevelAscend(a,b);
                        if(underChecker == 0){
                            let underCheckerLower = StageDragonList.SortBattlePointAscend(a,b);
                            if(underCheckerLower == 0){
                                return StageDragonList.SortObtainTimeAscend(a,b);
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
                    let checker = StageDragonList.SortLevelDescend(a,b);
                    if(checker == 0){
                        let underChecker = StageDragonList.SortGradeDescend(a,b);
                        if(underChecker == 0){
                            let underCheckerLower = StageDragonList.SortBattlePointDescend(a,b);
                            if(underCheckerLower == 0){
                                return StageDragonList.SortObtainTimeDescend(a,b);
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
                    let checker = StageDragonList.SortLevelAscend(a,b);
                    if(checker == 0){
                        let underChecker = StageDragonList.SortGradeAscend(a,b);
                        if(underChecker == 0){
                            let underCheckerLower = StageDragonList.SortBattlePointAscend(a,b);
                            if(underCheckerLower == 0){
                                return StageDragonList.SortObtainTimeAscend(a,b);
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
                    let checker = StageDragonList.SortBattlePointDescend(a,b);
                    if(checker == 0){
                        let underChecker = StageDragonList.SortGradeDescend(a,b);
                        if(underChecker == 0){
                            let underCheckerLower = StageDragonList.SortLevelDescend(a,b);
                            if(underCheckerLower == 0){
                                return StageDragonList.SortObtainTimeDescend(a,b);
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
                    let checker = StageDragonList.SortBattlePointAscend(a,b);
                    if(checker == 0){
                        let underChecker = StageDragonList.SortGradeAscend(a,b);
                        if(underChecker == 0){
                            let underCheckerLower = StageDragonList.SortLevelAscend(a,b);
                            if(underCheckerLower == 0){
                                return StageDragonList.SortObtainTimeAscend(a,b);
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
                    let checker = StageDragonList.SortObtainTimeDescend(a,b);
                    if(checker == 0){
                        let underChecker = StageDragonList.SortGradeDescend(a,b);
                        if(underChecker == 0){
                            let underCheckerLower = StageDragonList.SortLevelDescend(a,b);
                            if(underCheckerLower == 0){
                                return StageDragonList.SortBattlePointDescend(a,b);
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
                    let checker = StageDragonList.SortObtainTimeAscend(a,b);
                    if(checker == 0){
                        let underChecker = StageDragonList.SortGradeAscend(a,b);
                        if(underChecker == 0){
                            let underCheckerLower = StageDragonList.SortLevelAscend(a,b);
                            if(underCheckerLower == 0){
                                return StageDragonList.SortBattlePointAscend(a,b);
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

    RefreshButtonLabelByEventData(event : Event)
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

        let aInf = GetDragonStat(aTag , aLevel).INF;
        let bInf = GetDragonStat(bTag , bLevel).INF;
        return bInf - aInf;
    }
    //전투력 오름차순
    static SortBattlePointAscend(param_a : {index : number, value : UserDragon},param_b :{index : number, value : UserDragon})
    {
        let aTag = param_a.value.Tag as number;
        let aLevel = param_a.value.Level;

        let bTag = param_b.value.Tag as number;
        let bLevel = param_b.value.Level;

        let aInf = GetDragonStat(aTag , aLevel).INF;
        let bInf = GetDragonStat(bTag , bLevel).INF;
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
        
        ToastMessage.Set(StringTable.GetString(100000326));
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

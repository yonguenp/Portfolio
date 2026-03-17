
import { _decorator, Component, Sprite, Label, Button, Node, ProgressBar, math } from 'cc';
import { BuildingBaseData, BuildingLevelData } from '../Data/BuildingData';
import { BuildingBaseTable, BuildingLevelTable } from '../Data/BuildingTable';
import { ProductAutoTable } from '../Data/ProductTable';
import { StringTable } from '../Data/StringTable';
import { TableManager } from '../Data/TableManager';
import { GameManager } from '../GameManager';
import { NetworkManager } from '../NetworkManager';
import { ResourceManager, ResourcesType } from '../ResourceManager';
import { TimeObject } from '../Time/ITimeObject';
import { TimeManager } from '../Time/TimeManager';
import { DataManager } from '../Tools/DataManager';
import { StringBuilder, TimeString } from '../Tools/SandboxTools';
import { TutorialManager } from '../Tutorial/TutorialManager';
import { BuildingInstance, eBuildingState, eGoodType, User } from '../User/User';
import { eAccelerationType } from './AccelerationMainPopup';
import { BuildingTypeCommon } from './BuildingTypeCommon';
import { BuildingTypeOne } from './BuildingTypeOne';
import { BuildingTypeTwo } from './BuildingTypeTwo';
import { PopupManager } from './Common/PopupManager';
import { SystemPopup } from './SystemPopup';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = BuildingCard
 * DateTime = Thu Jan 13 2022 14:31:15 GMT+0900 (대한민국 표준시)
 * Author = seungmin
 * FileBasename = BuildingCard.ts
 * FileBasenameNoExtension = BuildingCard
 * URL = db://assets/Scripts/UI/BuildingCard.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
@ccclass('BuildingCard')
export class BuildingCard extends Component 
{
    @property(Sprite)
    spriteIcon : Sprite = null

    @property(Label)
    labelName : Label = null

    @property(Button)
    btnProduction : Button = null

    @property(Button)
    btnLevelUp : Button = null

    @property(Button)
    btnConstruction : Button = null

    @property(Node)
    block : Node = null

    @property(Label)
    labelBlock : Label = null

    @property(Node)
    nodeTimer : Node = null

    @property(Label)
    labelTimer : Label = null

    @property(Button)
    btnConstructComplete: Button = null

    @property(Label)
    labelBatteryAmount : Label = null

    @property(Label)
    labelBatteryMax : Label = null

    @property(ProgressBar)
    progressBatteryProd : ProgressBar = null

    private timeObj :TimeObject = null
    private buildingInfo : BuildingBaseData = null
    private buildingInstance : BuildingInstance = null
    private buildingLevelData : BuildingLevelData = null


    /**
     * 기획 DB로 건물카드 생성
     * 아직 건설하지 않았거나 건설할 수 없는 건물 카드
     * @param buildingIndex 
     * @param canConstruct 
     */
    initwithDesignData(buildingIndex : string, canConstruct : boolean, unlockCondition? : number)
    {
        this.initBuildingBaseInfo(buildingIndex)

        if(canConstruct)
        {
            //지을 수 있는 건물, 기본 빌딩 정보
            this.btnConstruction.node.active = true
        }
        else
        {
            //지을 수 없음, 기본 빌딩 정보
            this.block.active = true
            this.labelBlock.string = StringBuilder(TableManager.GetTable<StringTable>(StringTable.Name).Get(100000059).TEXT, unlockCondition)
        }
    }

    /**
     * 유저가 가진 건물 정보로 건물카드 생성
     * 건물의 레벨, 상태를 읽어와 세팅
     * @param buildingIndex 
     * @param buildingID 
     */
    initwithUserData(buildingIndex : string, buildingID : number)
    {
        //유저 정보에서 건물 정보 가져오기
        this.initBuildingBaseInfo(buildingIndex)
        this.buildingInstance = User.Instance.GetUserBuildingList().find(value => value.Tag == buildingID)
        this.buildingLevelData = TableManager.GetTable<BuildingLevelTable>(BuildingLevelTable.Name).GetAll().find(value => value.LEVEL == this.buildingInstance.Level && value.BUILDING_GROUP == buildingIndex)
        this.labelName.string =  StringBuilder(TableManager.GetTable<StringTable>(StringTable.Name).Get(100000057).TEXT, this.buildingInstance.Level, this.labelName.string)

        switch(this.buildingInstance.State)
        {
            case eBuildingState.CONSTRUCTING:
                if(this.buildingInstance.ActiveTime <= TimeManager.GetTime())
                {
                    this.initwithConstructComplete()
                    return;
                }
                this.initwithConstruct()
                return;
            case eBuildingState.CONSTRUCT_FINISHED:
                this.initwithConstructComplete()
                return;
        }
        
        let buildingLevelList : BuildingLevelData[] = []
        TableManager.GetTable<BuildingLevelTable>(BuildingLevelTable.Name).GetAll().forEach((element)=>
        {
            if(element.BUILDING_GROUP == this.buildingInfo.Index)
            {
                buildingLevelList.push(element)
            }
        })
        buildingLevelList.sort((a,b)=> a.LEVEL - b.LEVEL)

        if(buildingLevelList[buildingLevelList.length-1].LEVEL > User.Instance.GetUserBuildingList().find((value)=> value.Tag == this.buildingInstance.Tag).Level)
        {
            this.btnLevelUp.node.active = true
        }

        if(this.buildingInfo.TYPE == 2)
        {
            //건전지
            this.progressBatteryProd.node.active = true
            let timeObj = this.progressBatteryProd.getComponent(TimeObject)
            let autoProductInfo = TableManager.GetTable<ProductAutoTable>(ProductAutoTable.Name).Get("exp_battery").find(value => value.LEVEL == this.buildingInstance.Level)
            let prodInfo = User.Instance.GetProduces(this.buildingInstance.Tag)
            let maxProd = autoProductInfo.MAX_TIME/autoProductInfo.TERM
            let compProd = prodInfo.Items[0].RecipeID
            let endTime = prodInfo.Items[0].ProductionExp
            let curTick = endTime - TimeManager.GetTime()   //남은 시간


            while(curTick < 0 && compProd < maxProd) //이미 종료된 틱 총 합산
            {
                endTime += autoProductInfo.TERM
                curTick = endTime - TimeManager.GetTime()
                compProd++;
                if(maxProd == compProd)
                    break;
            }

            this.labelBatteryAmount.node.parent.active = false
            if(compProd > 0)
            {
                this.labelBatteryAmount.string = `${compProd}`
                this.labelBatteryAmount.node.parent.active = true
            }
            

            timeObj.curTime = autoProductInfo.TERM - curTick
            this.labelBatteryMax.string = TimeString(curTick)
            timeObj.Refresh = () => 
            {
                timeObj.curTime++
                this.progressBatteryProd.progress = math.clamp01(( timeObj.curTime / autoProductInfo.TERM))
                this.labelBatteryMax.string = TimeString(autoProductInfo.TERM-timeObj.curTime)

                if(timeObj.curTime / autoProductInfo.TERM == 1)
                {
                    compProd++;

                    if(compProd > 0)
                    {
                        this.labelBatteryAmount.string = `${compProd}`
                        this.labelBatteryAmount.node.parent.active = true
                    }
                    timeObj.curTime = 0
                }

                if(maxProd == compProd)
                {
                    this.progressBatteryProd.progress = 1
                    this.labelBatteryMax.string = TableManager.GetTable<StringTable>(StringTable.Name).Get(100000113).TEXT
                    timeObj.Refresh = undefined
                }
            }
            timeObj.Refresh()
        }
        else
        {
            this.btnProduction.node.active = true
        }
    }

    initwithConstruct()
    {
        //완료 시간 - 서버 시간으로 시간 나타냄
        this.timeObj = this.nodeTimer.addComponent(TimeObject)
        this.nodeTimer.active = true

        this.timeObj.curTime = this.buildingInstance.ActiveTime
        this.timeObj.Refresh = () =>
        {
            this.labelTimer.string = TimeManager.GetTimeCompareString(this.timeObj.curTime)
            if(TimeManager.GetTimeCompare(this.timeObj.curTime) <= 0)
            {
                this.timeObj.Refresh = undefined
                this.initwithConstructComplete()
                PopupManager.ForceUpdate()
            }
        }
        this.timeObj.Refresh()
    }

    initwithConstructComplete()
    {
        //완료 버튼만 있음
        this.nodeTimer.active = false
        this.btnConstructComplete.node.active = true
    }

    /**
     * 공통 건물 정보 가져오기 - 이름, 이미지 등
     * @param buildingIndex 
     */
    private initBuildingBaseInfo(buildingIndex : string)
    {
        this.buildingInfo = TableManager.GetTable<BuildingBaseTable>(BuildingBaseTable.Name).Get(buildingIndex)

        this.spriteIcon.spriteFrame = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.BUILDINGICON_SPRITEFRAME)[buildingIndex]
        this.labelName.string = TableManager.GetTable<StringTable>(StringTable.Name).Get(this.buildingInfo._NAME).KOR

        this.block.active = false
        this.btnConstruction.node.active = false
        this.btnLevelUp.node.active = false
        this.btnProduction.node.active = false
        this.nodeTimer.active = false
        this.btnConstructComplete.node.active = false
    }

    onClickBtnProduction(event, CustomEventData)
    {
        //생산 탭 - 해당 건물 레이어로 이동

        let data = {
            index : 3
        }

        DataManager.AddData("MainPopupTap", data)
        DataManager.AddData("ProductLayerTap", this.buildingInfo.Index)
        PopupManager.ForceUpdate()
    }

    onClickBtnLevelUp(event, CustomEventData)
    {
        //타입에 따라 레벨업 팝업 띄움
        let popup : BuildingTypeOne | BuildingTypeTwo = null

        if(this.buildingInfo.TYPE != 2)
        {
            popup = BuildingTypeOne.PopupBuildingLevelup(this.buildingInfo.Index as string, this.buildingInstance.Tag)
        }
        else
        {
            popup = BuildingTypeTwo.PopupBuildingLevelup(this.buildingInfo.Index as string, this.buildingInstance.Tag)
        }
    }

    onClickBtnConstruction(event, CustomEventData)
    {
        if(this.buildingInfo == null) {
            return;
        }
        if(this.CheckBuilding(this.buildingInfo.SIZE)) {
            var popup = PopupManager.OpenPopup("BuildingTypeCommon", true) as BuildingTypeCommon
            popup.setMessage(this.buildingInfo.Index as string)
    
            // 튜토리얼 실행
            if (this.buildingInfo?.TYPE == 3) {
                if (TutorialManager.GetInstance.IsNowPlayingTutorialByGroup(102)) {
                    TutorialManager.GetInstance.OnTutorialEvent(102, 4);
                }  
            }
        } else {
            var title = "알림";
            var message = "건물을 배치할 공간이 부족합니다.";
            let stringTable = TableManager.GetTable<StringTable>(StringTable.Name);
            if(stringTable != null) {
                title = stringTable.Get(100000248).TEXT;
                message = stringTable.Get(100000601).TEXT;
            }
            (PopupManager.OpenPopup("SystemPopupOK") as SystemPopup)?.setMessage(title, message);
        }
    }
 
    CheckBuilding(needSize: number = 1): boolean {
        let sizeCount = 0; //확보한 셀
        let gridData = User.Instance.GetGridData();
        
        let gridKeys = Object.keys(gridData);
        const floorCount = gridKeys.length

        for(let i = 0; i < floorCount ; i++) {
            const floor = gridKeys[i];
            if(gridData[floor] == null) {
                continue;
            }
            let cellkeys = Object.keys(gridData[floor]);
            const cellCount = cellkeys.length;
            for(let k = 0 ; k < cellCount ; k++) {
                const cell = cellkeys[k]
                let target = gridData[floor][cell];

                if(needSize <= sizeCount) {
                    return true;
                }

                if(target != 0) {
                    sizeCount = 0;
                    continue;
                }

                sizeCount++;
            }
        }

        return false;
    }

    onClickBtnAccelerate(event, CustomEventData)
    {
        PopupManager.OpenPopup("AccelerationMainPopup", false, {
            title:"가속 사용", 
            body:TableManager.GetTable(StringTable.Name).Get(this.buildingInfo._NAME).TEXT, 
            type: eAccelerationType.LEVELUP, 
            tag:this.buildingInstance.Tag,
            time:this.buildingLevelData.UPGRADE_TIME,
            time_end:this.buildingInstance.ActiveTime,
            prices:[
                {type:eGoodType.CASH, type_value:0, count:100},
                {type:eGoodType.ITEM, type_value:70000001, count:1}, 
                {type:eGoodType.ITEM, type_value:70000002, count:1}, 
                {type:eGoodType.ITEM, type_value:70000003, count:1}, 
                {type:eGoodType.ITEM, type_value:70000004, count:1}, 
                {type:eGoodType.ITEM, type_value:70000005, count:1}, 
                {type:eGoodType.ITEM, type_value:70000006, count:1}, 
                {type:eGoodType.ITEM, type_value:70000007, count:1}, 
                {type:eGoodType.ITEM, type_value:70000008, count:1}, 
                {type:eGoodType.ITEM, type_value:70000009, count:1}
            ]
        });
    }

    onClickBtnConstructComplete(event, CustomEventData)
    {
        let params = 
        {
            tag : this.buildingInstance.Tag
        }

        NetworkManager.Send("building/complete", params, ()=>
        {
            this.initwithUserData(this.buildingInfo.Index as string, this.buildingInstance.Tag)
            PopupManager.ForceUpdate()
        })

        // 튜토리얼-103
        if (TutorialManager.GetInstance.IsNowPlayingTutorialByGroup(103)) {
            TutorialManager.GetInstance.OffTutorialEvent();
        }
    }

    GetBuildingInfo() {
        return this.buildingInfo;
    }
}
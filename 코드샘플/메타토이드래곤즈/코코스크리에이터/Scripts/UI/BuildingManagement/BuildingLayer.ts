
import { _decorator, Node, Prefab, instantiate, ScrollView } from 'cc';
import { BuildingBaseTable, BuildingOpenTable } from '../../Data/BuildingTable';
import { TableManager } from '../../Data/TableManager';
import { GameManager } from '../../GameManager';
import { NetworkManager } from '../../NetworkManager';
import { ResourceManager, ResourcesType } from '../../ResourceManager';
import { TimeManager } from '../../Time/TimeManager';
import { DataManager } from '../../Tools/DataManager';
import { TutorialManager } from '../../Tutorial/TutorialManager';
import { eBuildingState, eLandmarkType, User } from '../../User/User';
import { BuildingCard } from '../BuildingCard';
import { PopupManager } from '../Common/PopupManager';
import { TapLayer } from '../Common/TapLayer';
import { SystemRewardPopup } from '../SystemRewardPopup';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = BuildingLayer
 * DateTime = Mon Jan 10 2022 19:45:06 GMT+0900 (대한민국 표준시)
 * Author = seungmin
 * FileBasename = BuildingLayer.ts
 * FileBasenameNoExtension = BuildingLayer
 * URL = db://assets/Scripts/UI/BuildingLayer.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
@ccclass('BuildingLayer')
export class BuildingLayer extends TapLayer
{
    @property(Node)
    buildingContent : Node = null

    @property(ScrollView)
    buildingScrollView: ScrollView = null;
    
    onEnable() {
        // 튜토리얼 관련
        TutorialManager.GetInstance.OnTutorialEvent(102,3);
        TutorialManager.GetInstance.OnTutorialEvent(103,6);
    }

    Init()
    {
        this.buildingContent.removeAllChildren()

        let clone : Prefab = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB)["buildingCard"]
        let buildingOpenArray = (TableManager.GetTable(BuildingOpenTable.Name) as BuildingOpenTable).GetAll(5)
        let userBuildings = User.Instance.GetUserBuildingList()
        let userBuildingTagList : number[] = []

        buildingOpenArray.sort((a, b) => a.KEY - b.KEY)
        
        for(let i = 0; i < userBuildings.length; i++)
        {
            if([eLandmarkType.COINDOZER, eLandmarkType.WORLDTRIP, eLandmarkType.SUBWAY].indexOf(userBuildings[i].Tag) >= 0)
                continue

            if(userBuildings[i].State == eBuildingState.CONSTRUCT_FINISHED || userBuildings[i].State == eBuildingState.CONSTRUCTING && userBuildings[i].ActiveTime <= TimeManager.GetTime())
            {
                let buildingInfo = buildingOpenArray.find(value => value.INSTALL_TAG == userBuildings[i].Tag)
                let newNode : Node = instantiate(clone)
                newNode.parent = this.buildingContent
                newNode.getComponent(BuildingCard).initwithUserData(buildingInfo.BUILDING, buildingInfo.INSTALL_TAG)

                // 튜토리얼 관련 - 아래에 들어올경우 이미 건설이 완료 된 상태인것
                if (TutorialManager.GetInstance.IsNowPlayingTutorialByGroup(103)) {
                    if (buildingOpenArray[i].KEY == 5) {
                        newNode.setSiblingIndex(0);

                        TutorialManager.GetInstance.ForceOffTutorialEvent();
                    }
                }
            }
            else
                userBuildingTagList.push(userBuildings[i].Tag)

                
        }
            

        for(let i = 0; i < buildingOpenArray.length; i++)
        {
            if(TableManager.GetTable(BuildingBaseTable.Name).Get(buildingOpenArray[i].BUILDING).TYPE == 1)
                continue

            if(User.Instance.GetAreaLevel() >= buildingOpenArray[i].OPEN_LEVEL && userBuildings.find(value=>value.Tag == buildingOpenArray[i].INSTALL_TAG) == null)
            {
                let newNode : Node = instantiate(clone)
                newNode.parent = this.buildingContent
                newNode.getComponent(BuildingCard).initwithDesignData(buildingOpenArray[i].BUILDING, true)

                // 튜토리얼 관련
                if (TutorialManager.GetInstance.IsNowPlayingTutorialByGroup(102)) {
                    if (buildingOpenArray[i].KEY == 5) {
                        newNode.setSiblingIndex(0);
                    }
                }
            }
        }

        for(let i = 0; i < buildingOpenArray.length; i++)
        {
            if(TableManager.GetTable(BuildingBaseTable.Name).Get(buildingOpenArray[i].BUILDING).TYPE == 1)
                continue

            if(User.Instance.GetAreaLevel() >= buildingOpenArray[i].OPEN_LEVEL && userBuildingTagList.indexOf(buildingOpenArray[i].INSTALL_TAG) >= 0)
            {
                let newNode : Node = instantiate(clone)
                newNode.parent = this.buildingContent
                newNode.getComponent(BuildingCard).initwithUserData(buildingOpenArray[i].BUILDING, buildingOpenArray[i].INSTALL_TAG)    
                
                // 튜토리얼 관련
                if (TutorialManager.GetInstance.IsNowPlayingTutorialByGroup(113)) {
                    if (buildingOpenArray[i].KEY <= 4) {
                        newNode.setSiblingIndex(0);
                    }
                }

                // 튜토리얼 관련
                if (TutorialManager.GetInstance.IsNowPlayingTutorialByGroup(103)) {
                    if (buildingOpenArray[i].KEY == 5) {
                        newNode.setSiblingIndex(0);
                    }
                }
            }
        }

        for(let i = 0; i < buildingOpenArray.length; i++)
        {
            if(TableManager.GetTable(BuildingBaseTable.Name).Get(buildingOpenArray[i].BUILDING).TYPE == 1)
                continue

            if(User.Instance.GetAreaLevel() < buildingOpenArray[i].OPEN_LEVEL)
            {
                let newNode : Node = instantiate(clone)
                newNode.parent = this.buildingContent
                newNode.getComponent(BuildingCard).initwithDesignData(buildingOpenArray[i].BUILDING, false, buildingOpenArray[i].OPEN_LEVEL)
            }
        }
    }

    ForceUpdate()
    {
        this.Init()
    }

    onClickGetAll(event, CustomEventData)
    {
        let params = {}
        NetworkManager.Send("produce/harvest", params, (jsonObj) => {
            switch(jsonObj.rs)
            {
                case 14:
                    //console.log("가방이 가득 참")
                    break;
            }
            PopupManager.ForceUpdate();

            let popup = PopupManager.OpenPopup("SystemRewardPopup") as SystemRewardPopup;
            popup.initWithList(jsonObj.rewards)
        })
    }
}
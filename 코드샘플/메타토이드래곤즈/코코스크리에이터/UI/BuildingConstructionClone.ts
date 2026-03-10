
import { _decorator, Component, Button } from 'cc';
import { BuildingBaseData, BuildingOpenData } from '../Data/BuildingData';
import { BuildingOpenTable } from '../Data/BuildingTable';
import { StringTable } from '../Data/StringTable';
import { TableManager } from '../Data/TableManager';
import { NetworkManager } from '../NetworkManager';
import { TutorialManager } from '../Tutorial/TutorialManager';
import { User } from '../User/User';
import { PopupManager } from './Common/PopupManager';
import { SystemPopup } from './SystemPopup';
import { ToastMessage } from './ToastMessage';
import { UICanvas } from './UICanvas';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = BuildingConstructionClone
 * DateTime = Tue Jan 18 2022 15:23:26 GMT+0900 (대한민국 표준시)
 * Author = seungmin
 * FileBasename = BuildingConstructionClone.ts
 * FileBasenameNoExtension = BuildingConstructionClone
 * URL = db://assets/Scripts/UI/BuildingConstructionClone.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
@ccclass('BuildingConstructionClone')
export class BuildingConstructionClone extends Component 
{
    @property(Button)
    btnOK : Button = null;

    @property(Button)
    btnCancel : Button = null;

    private x : number = 0
    private y : number = 0
    private buildingBaseInfo : BuildingBaseData = null

    cb_offConstruct : () => void = null;

    onEnable() {
        // 튜토리얼 실행
        TutorialManager.GetInstance.OnTutorialEvent(102,6);
    }

    onClickOK(event, CustomEventData)
    {
        UICanvas.ActiveUI = true
        this.btnOK.node.active = false
        this.btnCancel.node.active = false
        
        PopupManager.GetPopupByIndex(1).ClosePopup()
        PopupManager.GetPopupByIndex(0).ClosePopup()
        let popup = PopupManager.OpenPopup("SystemPopupOKCANCEL") as SystemPopup;
        popup.setMessage(StringTable.GetString(100000019), StringTable.GetString(100000608));

        popup.setCallback(this.OK_CALLBACK.bind(this, popup), this.CANCEL_CALLBACK.bind(this, popup), this.CANCEL_CALLBACK.bind(this, popup));

        // 튜토리얼 실행
        TutorialManager.GetInstance.OnTutorialEvent(102,7);
    }

    OK_CALLBACK(popup: SystemPopup) {var targetBuilding : BuildingOpenData = null
        let buildings = TableManager.GetTable<BuildingOpenTable>(BuildingOpenTable.Name).GetAll(5)
        
        console.log(buildings)
        console.log(this.buildingBaseInfo)
        console.log(User.Instance.GetUserBuildingList())

        for(let i = 0; i < buildings.length; i++)
        {
            if(["dozer", "travel", "subway"].indexOf(buildings[i].BUILDING) >= 0 || this.buildingBaseInfo.Index != buildings[i].BUILDING)
                continue;
                

            targetBuilding = buildings[i]
            for(let j = 0; j < User.Instance.GetUserBuildingList().length; j++)
            {
                if(User.Instance.GetUserBuildingList()[j].Tag == targetBuilding.INSTALL_TAG)
                {
                    targetBuilding = null
                    break;
                }
            }

            if(targetBuilding != null)
                break;
        }

        if(targetBuilding == null)
        {
            console.log("targetBuilding == null", targetBuilding)
            this.cb_offConstruct()
            popup.ClosePopup()
            return;
        }

        console.log(targetBuilding)
        let params = {
            tag : targetBuilding.INSTALL_TAG,
            x : this.x,
            y : this.y
        }

        NetworkManager.Send("building/construct", params, (jsonData)=>
        {
            switch(jsonData.rs)
            {
                case 0:
                    // let myMap = GameManager.GetManager(MapManager);
                    // let clone : Node = instantiate(GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB)["constructionTimer"])
                    // let label = clone.getChildByName("labelTimer").getComponent(Label);
                    // let timeObj = clone.getComponent(TimeObject);
                    // timeObj.curTime = jsonData.construct_exp;

                    // timeObj.Refresh = () => 
                    // {
                    //     label.string = TimeManager.GetTimeCompareString(timeObj.curTime)
                    // }
                    // timeObj.Refresh()

                    // clone.setParent(myMap.emptyNode, false);
                    // clone.worldPosition = parentWPos
                    // clone.setSiblingIndex(9999)
                    //clone.getComponent(UITransform).setContentSize(CellSize.x * jsonData.size, CellSize.y)

                    break;
                case 201:
                    ToastMessage.Set(StringTable.GetString(100000633), null, -52);
                    break;
                case 202:
                    ToastMessage.Set(StringTable.GetString(100000630), null, -52);
                    break;
            }
            this.node.removeFromParent()
        })

        this.cb_offConstruct()
        popup.ClosePopup()
    }

    CANCEL_CALLBACK(popup: SystemPopup) {
        ToastMessage.Set(StringTable.GetString(100000609), null, -52);
        this.cb_offConstruct();
        this.node.removeFromParent();
        popup.ClosePopup();
    }

    setBuildingBaseInfo(info : BuildingBaseData, x : number, y : number)
    {
        this.buildingBaseInfo = info
        this.x = x 
        this.y = y
    }

    onClickCancel(event, CustomEventData)
    {
        UICanvas.ActiveUI = true
        this.cb_offConstruct()
        this.node.removeFromParent()
    }
}
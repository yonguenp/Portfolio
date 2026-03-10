
import { _decorator, Component, Node } from 'cc';
import { BuildingOpenTable } from '../Data/BuildingTable';
import { TableManager } from '../Data/TableManager';
import { DataManager } from '../Tools/DataManager';
import { User } from '../User/User';
import { TapExtension } from './Common/TapExtension';
import { TapLayer } from './Common/TapLayer';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = MainPopupTap
 * DateTime = Wed Apr 27 2022 16:14:25 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = MainPopupTap.ts
 * FileBasenameNoExtension = MainPopupTap
 * URL = db://assets/Scripts/UI/MainPopupTap.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('MainPopupTap')
export class MainPopupTap extends TapExtension {
    Init() {
        if(this.data != null)
        {
            this.data.forEach((element, index) => 
            {
                let buildingList = User.Instance.GetUserBuildingList()

                if(index == 3)
                {
                    let state = false;
                    let openDataList = TableManager.GetTable<BuildingOpenTable>(BuildingOpenTable.Name).GetAll(5);

                    for(let i = 0; i < buildingList.length; i++)
                    {
                        if(buildingList[i].Tag > 999 && buildingList[i].State == 5)
                        {
                            let data = openDataList.find(value => value.INSTALL_TAG == buildingList[i].Tag);
                            if(data != null) {
                                switch(data.BUILDING) {
                                    case 'dozer':case 'subway':case 'travel':case 'exp_battery': {
                                    } break;
                                    default: {
                                        state = true;
                                    } break;
                                }
                            }
                        }
                    }
                    
                    element.SetActive(state);
                } 
            });
        }
        super.Init();
    }

    OnTab(data: any) {
        if(this.stackTap == null) {
            this.stackTap = [];
        }
        if(this.data == null || this.data.length <= data['index'] || data['index'] < 0 || this.data[data['index']] == null) {
            return;
        }

        let tabData = this.data[data['index']];
        if(tabData != null) {
            if(this.stackTap.length > 0) {
                this.stackTap.forEach(element => {
                    element.SetVisible(false)
                });
            }

            tabData.SetVisible(true);
            let tapLayer = tabData.TargetLayer?.getComponent(TapLayer);
            if(tapLayer != null) {
                tapLayer.Init();
            }
            this.stackTap.push(tabData);
        }
    }

    ForceUpdate()
    {
        super.ForceUpdate();
        if(this.data != null)
        {
            this.data.forEach((element, index) => 
            {
                let buildingList = User.Instance.GetUserBuildingList()

                if(index == 3)
                {
                    let state = false;
                    let openDataList = TableManager.GetTable<BuildingOpenTable>(BuildingOpenTable.Name).GetAll(5);

                    for(let i = 0; i < buildingList.length; i++)
                    {
                        if(buildingList[i].Tag > 999 && buildingList[i].State == 5)
                        {
                            let data = openDataList.find(value => value.INSTALL_TAG == buildingList[i].Tag);
                            if(data != null) {
                                switch(data.BUILDING) {
                                    case 'dozer':case 'subway':case 'travel':case 'exp_battery': {
                                    } break;
                                    default: {
                                        state = true;
                                    } break;
                                }
                            }
                        }
                    }
                    element.TargetBtn.node.active = state;
                } 
            });
        }

        if(DataManager.GetData("MainPopupTap") != null)
        {
            this.OnTab(DataManager.GetData("MainPopupTap"));
            DataManager.DelData("MainPopupTap");
        }
        else
        {
            let target = this.stackTap[this.stackTap.length - 1];
            if(target != null) {
                let tabLayer = target.TargetLayer.getComponent(TapLayer);
                if(tabLayer != null) {
                    tabLayer.ForceUpdate();
                }
            }
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

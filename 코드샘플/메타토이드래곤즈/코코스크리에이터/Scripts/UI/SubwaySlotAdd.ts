
import { _decorator, Component, Node, Label } from 'cc';
import { NetworkManager } from '../NetworkManager';
import { ObjectCheck } from '../Tools/SandboxTools';
import { eLandmarkType } from '../User/User';
import { Popup } from './Common/Popup';
import { PopupManager } from './Common/PopupManager';
const { ccclass, property } = _decorator;
 
@ccclass('SubwaySlotAdd')
export class SubwaySlotAdd extends Popup
{
    @property(Label)
    labelCost : Label = null;

    Init(popupData: any) {
        super.Init(popupData);
        this.labelCost.string = this.popupData['cost'].toString();
    }

    onClickAdd()
    {
        let params = 
        {
            plat : this.popupData['plat']
        };

        NetworkManager.Send("subway/unlock", params, (jsonObj) => 
        {
            if(ObjectCheck(jsonObj, "rs") && jsonObj.rs == 0)
            {
                PopupManager.ForceUpdate();
            }
            this.ClosePopup();
        });
    }
}

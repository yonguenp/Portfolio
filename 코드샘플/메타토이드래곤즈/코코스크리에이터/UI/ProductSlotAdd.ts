
import { _decorator, Label } from 'cc';
import { NetworkManager } from '../NetworkManager';
import { ObjectCheck } from '../Tools/SandboxTools';
import { Popup } from './Common/Popup';
import { PopupManager } from './Common/PopupManager';
const { ccclass, property } = _decorator;
 
@ccclass('ProductSlotAdd')
export class ProductSlotAdd extends Popup
{
    @property(Label)
    labelCost : Label = null;

    private buildingTag : number;
    set BuildingTag(value)
    {
        this.buildingTag = value;
    }

    Cost(value : number)
    {
        //나중에 타입에 따른 재화 아이콘 변경해야함
        this.labelCost.string = value.toString();
    }

    onClickAdd()
    {
        let params = 
        {
            tag : this.buildingTag
        };
        NetworkManager.Send("building/expand", params, (jsonObj) => 
        {
            if(ObjectCheck(jsonObj, "rs") && jsonObj.rs == 0)
            {
                PopupManager.ForceUpdate();
            }
            this.ClosePopup();
        });
    }
}

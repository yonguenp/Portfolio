import { _decorator, Node, Button, Sprite } from 'cc';
import { StringTable } from '../../Data/StringTable';
import { DataManager } from '../../Tools/DataManager';
import { TapLayer } from '../Common/TapLayer';
import { ToastMessage } from '../ToastMessage';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = LandmarkLayer
 * DateTime = Mon Jan 10 2022 19:45:55 GMT+0900 (대한민국 표준시)
 * Author = seungmin
 * FileBasename = LandmarkLayer.ts
 * FileBasenameNoExtension = LandmarkLayer
 * URL = db://assets/Scripts/UI/BuildingManagement/LandmarkLayer.ts
 *
 */
 
@ccclass('LandmarkLayer')
export class LandmarkLayer extends TapLayer
{
    @property(Node)
    tapBtn : Node[] = []

    @property(Node)
    tapTap : Node[] = []

    private defaultTap : number = 0
    private currentTap : number = 0

    Init(initialTap : number = 0)
    {
        if(this.tapTap != null && this.tapTap.length > 0)
        {
            this.tapTap.forEach((element)=>{element.active = false})
        }

        let targetTap = DataManager.GetData("LandmarkTap")
        targetTap = targetTap == null ? initialTap : targetTap

        this.moveTap(Number(targetTap))
    }

    moveTap(targetTap : number = 0)
    {
        this.currentTap = targetTap
        this.tapTap.forEach((element)=>{element.active = false})
        this.tapBtn.forEach((element)=>{
            element.getComponent(Button).interactable = true; 
            //element.getComponentInChildren(Sprite).enabled = false;
        })

        this.tapTap[targetTap].active = true
        this.tapTap[targetTap].getComponent(TapLayer)?.Init()
        this.tapBtn[targetTap].getComponent(Button).interactable = false
        this.tapBtn[targetTap].getComponentInChildren(Sprite).enabled = true
    }

    onClickTapBtn(event, CustomEventData)
    {
        let jsonData = JSON.parse(CustomEventData)
        let index = Number(jsonData.index);
        
        //임시 - 여행사 , 지하철 클릭 안되게 처리
        if(index != 0)
        {
            this.onClickExpectGameAlphaUpdate();
            return;
        }
        
        this.moveTap(jsonData.index)
    }

    ForceUpdate()
    {
        this.tapTap[this.currentTap].getComponent(TapLayer).ForceUpdate()
    }

    onClickExpectGameAlphaUpdate()
    {
        let emptyCheck = ToastMessage.isToastEmpty();
        if(emptyCheck == false){
            return;
        }
        
        ToastMessage.Set(StringTable.GetString(100000326));
    }
}
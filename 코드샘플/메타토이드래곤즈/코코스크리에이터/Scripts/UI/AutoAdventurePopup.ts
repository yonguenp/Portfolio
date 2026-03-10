
import { _decorator, Component, Node, Button, Label, EventHandler } from 'cc';
import { StageBaseData } from '../Data/StageData';
import { StageBaseTable } from '../Data/StageTable';
import { TableManager } from '../Data/TableManager';
import { DataManager } from '../Tools/DataManager';
import { User } from '../User/User';
import { ButtonChangeChildColor } from './ButtonChangeChildColor';
import { Popup } from './Common/Popup';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = AutoAdventurePopup
 * DateTime = Fri May 13 2022 17:03:06 GMT+0900 (대한민국 표준시)
 * Author = jinwonjun
 * FileBasename = AutoAdventurePopup.ts
 * FileBasenameNoExtension = AutoAdventurePopup
 * URL = db://assets/Scripts/UI/AutoAdventurePopup.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('AutoAdventurePopup')
export class AutoAdventurePopup extends Popup {
    
    stageTable : StageBaseTable = null;
    stageData : StageBaseData = null;

    userStemina : number = 0;
    maxCount : number = 0;

    @property(Button)
    leftButton : Button = null;

    @property(Button)
    rightButton : Button = null;

    @property(Button)
    maxButton : Button = null;

    @property(Button)
    okButton : Button = null;

    @property(Label)
    autoCountLabel : Label = null;

    currentClickNumber : number = 1;

    //월드 및 스테이지 데이터 가져와서 스테미너 요구치 계산
    Init(jsonData : any){
        this.SetButtonEvent();
        this.RefreshUserStemina();
        this.RefreshStageData(jsonData);
        this.SetCalculateMaxCount();
        this.InitButton();
    }

    SetButtonEvent()
    {
        if(this.okButton == null){
            return;
        }
        let popupRootNode = this.node.getParent().getParent();
        let stagePrepareNode = popupRootNode.getChildByName('StagePrepareNode');
        if(stagePrepareNode != null)
        {
            let newEvent = new EventHandler();
            newEvent.target = this.node;
            newEvent.component = "AutoAdventurePopup"
            newEvent.handler = "onClickOK";
            this.okButton.clickEvents.push(newEvent);

            let newEvent2 = new EventHandler();
            newEvent2.target = stagePrepareNode;
            newEvent2.component = "StagePrepareComponent"
            newEvent2.handler = "onClickAutoStart";
            this.okButton.clickEvents.push(newEvent2);
        }
    }

    RefreshStageData(jsonData : any){
        if(this.stageTable == null){
            this.stageTable =  TableManager.GetTable<StageBaseTable>(StageBaseTable.Name);
        }

        let world = jsonData.world;
        let stage = jsonData.stage;

        this.stageData = TableManager.GetTable<StageBaseTable>(StageBaseTable.Name).GetByWorldStage(world, stage);
    }
    GetCurrentClickNumber()
    {
        return this.currentClickNumber;
    }

    RefreshUserStemina()//현재 유저의 스테미너량 가져오기
    {
        this.userStemina = User.Instance.UserData.Energy;
    }

    InitButton()
    {
        let ableCheck = this.isAvailableCount();
        if(ableCheck == false){
            this.currentClickNumber = 0;
            this.leftButton.interactable = false;
            this.rightButton.interactable = false;
            this.maxButton.interactable = false;
        }else{
            this.leftButton.interactable = false;
            this.currentClickNumber = 1;
        }

        if(this.maxCount == 1)
        {
            this.maxButton.interactable = false;
        }

        this.RefreshButtonState([this.leftButton, this.rightButton, this.maxButton]);
    }

    RefreshButton()
    {
        if(this.currentClickNumber <= 1)
        {
            this.leftButton.interactable = false;
            this.rightButton.interactable = true;
        }
        else if(this.currentClickNumber >= this.maxCount)
        {
            this.leftButton.interactable = true;
            this.rightButton.interactable = false;
        }else{
            this.leftButton.interactable = true;
            this.rightButton.interactable = true;
        }

        if(this.currentClickNumber == this.maxCount){
            this.maxButton.interactable = false;
        }else{
            this.maxButton.interactable = true;
        }

        this.RefreshButtonState([this.leftButton, this.rightButton, this.maxButton]);
    }

    RefreshCountLabel()
    {
        if(this.autoCountLabel != null){
            this.autoCountLabel.string = this.currentClickNumber.toString();
        }
    }

    SetCalculateMaxCount()
    {
        let tempMaxCount = 0;
        if(this.stageData == null){
            this.maxCount = tempMaxCount;
            return;
        }

        let currentStageRequireAmount = this.stageData.COST_VALUE;//요구 스테미너량
        if(currentStageRequireAmount > this.userStemina){
            this.maxCount = tempMaxCount;
            return;
        }
        
        let AvailableRound = Math.floor(this.userStemina / currentStageRequireAmount);
        this.maxCount = AvailableRound;
    }

    SetClickNumber(modifyNumber : number)
    {
        if(modifyNumber <= 0){
            modifyNumber = 1;
        }
        else if(modifyNumber > this.maxCount){
            modifyNumber = this.maxCount;
        }
        this.currentClickNumber = modifyNumber;
    }

    isAvailableCount() :boolean
    {
        let currentStageRequireAmount = this.stageData.COST_VALUE;//요구 스테미너량
        if(currentStageRequireAmount > this.userStemina){
            return false;
        }
        return true;
    }

    onClickLeftButton()
    {
        this.SetClickNumber(this.currentClickNumber - 1);
        this.RefreshButton();
        this.RefreshCountLabel();
    }
    onClickRightButton()
    {
        this.SetClickNumber(this.currentClickNumber + 1);
        this.RefreshButton();
        this.RefreshCountLabel();
    }
    onClickMaxButton()
    {
        this.SetClickNumber(this.maxCount);
        this.RefreshButton();
        this.RefreshCountLabel();
    }
    onClickCancel()
    {
        this.ClosePopup();
    }
    onClickOK()//전투 진행으로 넘기기
    {
        DataManager.AddData("AutoAdventureCount", this.currentClickNumber);
        DataManager.AddData("AutoAdventureTotalCount", this.currentClickNumber);
        this.ClosePopup();
    }

    RefreshButtonState(buttonList : Button[])
    {
        if(buttonList == null || buttonList.length <= 0)
        {
            return;
        }

        buttonList.forEach(element=>{
            let component= element.node;
            if(component == null){
                return;
            }
            let changeLabelComp = component.getComponent(ButtonChangeChildColor);
            if(changeLabelComp != null){}
            changeLabelComp.refreshColor();
        })
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

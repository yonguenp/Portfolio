
import { _decorator, Component, Sprite, Label, ProgressBar, math, UITransform } from 'cc';
import { ItemBaseTable } from '../../Data/ItemTable';
import { ProductData } from '../../Data/ProductData';
import { StringTable } from '../../Data/StringTable';
import { TableManager } from '../../Data/TableManager';
import { GameManager } from '../../GameManager';
import { NetworkManager } from '../../NetworkManager';
import { ResourceManager, ResourcesType } from '../../ResourceManager';
import { TimeObject } from '../../Time/ITimeObject';
import { TimeManager } from '../../Time/TimeManager';
import { TimeString } from '../../Tools/SandboxTools';
import { TutorialManager } from '../../Tutorial/TutorialManager';
import { eGoodType } from '../../User/User';
import { eAccelerationType } from '../AccelerationMainPopup';
import { Popup } from '../Common/Popup';
import { PopupManager } from '../Common/PopupManager';
import { SystemRewardPopup } from '../SystemRewardPopup';
import { ToastMessage } from '../ToastMessage';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = receipeFrame
 * DateTime = Tue Jan 11 2022 15:09:07 GMT+0900 (대한민국 표준시)
 * Author = seungmin
 * FileBasename = receipeFrame.ts
 * FileBasenameNoExtension = receipeFrame
 * URL = db://assets/Scripts/UI/receipeFrame.ts
 *
 */

@ccclass('receipeFrame')
export class receipeFrame extends Component
{
    @property(Sprite)
    receipeFrame : Sprite = null

    @property(Sprite)
    receipeIcon : Sprite = null

    @property(ProgressBar)
    progressTimer : ProgressBar = null

    @property(Label)
    labelTimer : Label = null

    @property(Label)
    labelProdCnt : Label = null

    @property(Sprite)
    sprChecker : Sprite = null

    private frameIndex : number  = -1
    private timeObj : TimeObject = null 
    private productData : ProductData = null
    private inInit : boolean = false
    private buildingTag : number = 0
    private tPopup : Popup = null;

    setFrameBlank()
    {
        this.inInit = false
        this.productData = null
        this.progressTimer.node.active = false
        this.timeObj = this.progressTimer.getComponent(TimeObject)
        this.labelTimer.string = "-"
        this.receipeIcon.spriteFrame = null
        this.sprChecker.node.active = false
    }

    /**
     * 아이콘만 있는 레시피 프레임
     * @param pData 레시피 정보 : ProductData
     */
    setReceipeIcon(index : number, pData : ProductData, reqTime : number, tag : number = 0) : void
    {
        this.buildingTag = tag;
        this.frameIndex = index;
        this.inInit = true;
        this.productData = pData;
        this.timeObj = this.progressTimer.getComponent(TimeObject);
        this.progressTimer.node.active = false;
        this.timeObj.curTime = reqTime;
        this.labelTimer.string = TimeManager.GetTimeCompareString(this.timeObj.curTime);
        this.receipeIcon.spriteFrame = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.ITEMICON_SPRITEFRAME)[this.productData.ProductIcon];
        this.sprChecker.node.active = reqTime == -1 || reqTime != 0 && reqTime <= TimeManager.GetTime();
        
        if(this.productData.ProductAmount > 1)
        {
            this.labelProdCnt.string = String(this.productData.ProductAmount);
        }else{
            this.labelProdCnt.string = "";
        }

        let totalTime = pData.ProductReqTime;
        this.progressTimer.progress = math.clamp01( (totalTime - (reqTime - TimeManager.GetTime())) / totalTime);
        this.progressTimer.totalLength = this.progressTimer.node.getComponent(UITransform).width;
        if(this.timeObj.curTime - TimeManager.GetTime() > 0)
        {
            this.timeObj.Refresh = () => 
            {
                let remain = this.timeObj.curTime - TimeManager.GetTime();
                this.progressTimer.progress = math.clamp01( (totalTime - (reqTime - TimeManager.GetTime())) / totalTime);
                this.labelTimer.string = TimeString(remain);
    
                if(remain <= 0)
                {
                    if(this.tPopup != null && this.tPopup?.node != null)
                    {
                        //팝업 닫기 호출
                        this.tPopup.ClosePopup();
                        ToastMessage.Set("시간이 완료되어 자동으로 닫았습니다.", null, -52);
                    }
                    this.tPopup = null;
                    this.progressTimer.node.active = false;
                    this.timeObj.Refresh = undefined;
                    this.sprChecker.node.active = true;
                    PopupManager.ForceUpdate();
                }
            }
        }
    }

    /**
     * 레시피 프레임의 타이머 시작
     * setFrameBlank() 으로 이니셜라이징 된 경우 아무 효과없음
     */
    timerStart()
    {
        if(!this.inInit)
            return
        this.progressTimer.node.active = true
    }

    /**
     * 레시피 프레임 클릭 시 이벤트
     * @param event 
     * @param custom 
     */
    onClick(event, custom)
    {
        //isQueueFrame 에 따라 서버 탈지 안탈지
        if(this.buildingTag == 0)
            return;

        if(this.timeObj?.curTime - TimeManager.GetTime() > 0 && this.timeObj?.curTime <= TimeManager.GetTime() + this.productData.ProductReqTime)
        {
            let itemName = TableManager.GetTable<ItemBaseTable>(ItemBaseTable.Name).Get(this.productData.ProductItemID)._NAME;
            this.tPopup = PopupManager.OpenPopup("AccelerationMainPopup", false, {
                title:"가속 사용", 
                body:TableManager.GetTable(StringTable.Name).Get(itemName).TEXT, 
                type: eAccelerationType.JOB, 
                tag: this.buildingTag,
                time: this.productData.ProductReqTime,
                time_end: this.timeObj.curTime,
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
            return;
        }
        let params = {
            tag : this.buildingTag,
            slot : this.frameIndex
        }

        NetworkManager.Send("produce/harvest", params, (jsonObj) => {
            PopupManager.ForceUpdate()

            let popup = PopupManager.OpenPopup("SystemRewardPopup") as SystemRewardPopup;
            popup.initWithList(jsonObj.rewards);
        })

        if (TutorialManager.GetInstance.IsNowPlayingTutorialByGroup(108)) {
            TutorialManager.GetInstance.OffTutorialEvent();
        }
    }
}
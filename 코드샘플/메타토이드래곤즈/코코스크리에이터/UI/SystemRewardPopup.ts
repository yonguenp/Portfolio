
import { _decorator, Node, instantiate, Widget, UITransform, ScrollView, Layout } from 'cc';
import { GameManager } from '../GameManager';
import { ResourceManager, ResourcesType } from '../ResourceManager';
import { Popup } from './Common/Popup';
import { ItemFrame } from './ItemSlot/ItemFrame';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = SystemRewardPopup
 * DateTime = Fri Jan 21 2022 21:56:18 GMT+0900 (대한민국 표준시)
 * Author = seungmin
 * FileBasename = SystemRewardPopup.ts
 * FileBasenameNoExtension = SystemRewardPopup
 * URL = db://assets/Scripts/UI/SystemRewardPopup.ts
 */
 
@ccclass('SystemRewardPopup')
export class SystemRewardPopup extends Popup
{
    @property(ScrollView)
    scrollView : ScrollView = null

    @property(Node)
    sprArrowLeft : Node = null

    @property(Node)
    sprArrowRight : Node = null

    initWithList(itemList : [] = [])
    {
        console.log(itemList)

        if(itemList.length == 0)
        {
            this.ClosePopup()
            return
        }

        let rewardList : { [id:string] : { type : number, amount : number} } = { }
        itemList.forEach(element =>
        {
            let idString = `${element[0]}_${element[1]}`

            if(rewardList[idString] == null)
                rewardList[idString] = { type : element[0], amount : element[2] }
            else
                rewardList[idString].amount += element[2]
        })

        var count = 0;
        var itemSize = 0;
        let keyList = Object.keys(rewardList)

        for(let i = 0; i < keyList.length; i++)
        {
            let newItem : Node = instantiate(GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB)["item"])
            this.scrollView.content.addChild(newItem);
            let transformItem = newItem.getComponent(UITransform);
            if(transformItem != null) {
                itemSize += transformItem.width;
            }
            count++;
            
            let id = keyList[i].split("_")[1]
            switch(rewardList[keyList[i]].type)
            {
                case 1:
                    newItem.getComponent(ItemFrame).setFrameCashInfo(Number(id), rewardList[keyList[i]].amount)
                break

                case 2:
                    newItem.getComponent(ItemFrame).setFrameEnergyInfo(rewardList[keyList[i]].amount)
                break
                
                case 3:
                    newItem.getComponent(ItemFrame).setFrameItemInfo(Number(id), rewardList[keyList[i]].amount)
                break
            }   
        }

        let scrollTransform = this.scrollView.getComponent(UITransform);
        let layout = this.scrollView.content.getComponent(Layout);
        let containerTransform = this.scrollView.content.getComponent(UITransform);
        if(scrollTransform != null && layout != null) {
            var scrollSize = itemSize + (count - 1) * layout.spacingX + layout.paddingLeft + layout.paddingRight;
            if(scrollSize > scrollTransform.width) {
                scrollSize = scrollTransform.width;
            }
            this.scrollView.view.setContentSize(scrollSize, this.scrollView.view.contentSize.height);
            if(containerTransform != null && scrollTransform.width < containerTransform.contentSize.width)
            {
                this.sprArrowRight.active = true;   
            }
        }
        this.scrollView.scrollToLeft();
    }

    onScrolling(event, CustomEventData)
    {
        this.showArrow()
    }

    showArrow()
    {
        let parentSize : number = this.scrollView.content.parent.getComponent(UITransform).contentSize.width
        let compareSize : number = this.scrollView.content.getComponent(UITransform).contentSize.width - parentSize

        if(compareSize < 0)
            return;

        let sideEnd = (compareSize / 2)

        if(Math.round(this.scrollView.content.position.x) < Math.round(sideEnd))
            this.sprArrowLeft.active = true
        else
            this.sprArrowLeft.active = false

        if(Math.round(this.scrollView.content.position.x) > Math.round(-sideEnd))
            this.sprArrowRight.active = true
        else
            this.sprArrowRight.active = false
    }

    DirectRewardList(jsonData : any)
    {
        var count = 0;
        var itemSize = 0;
        let keyList = Object.keys(jsonData)

        for(let i = 0; i < keyList.length; i++)
        {
            let newItem : Node = instantiate(GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB)["item"])
            this.scrollView.content.addChild(newItem);
            let transformItem = newItem.getComponent(UITransform);
            if(transformItem != null) {
                itemSize += transformItem.width;
            }
            count++;

            let type = jsonData[keyList[i]].type;
            let amount = jsonData[keyList[i]].count;
            let no = jsonData[keyList[i]].no;
            
            switch(type)
            {
                case 1:
                    newItem.getComponent(ItemFrame).setFrameCashInfo(no, amount);
                break

                case 2:
                    newItem.getComponent(ItemFrame).setFrameEnergyInfo(amount);
                break
                
                case 3:
                    newItem.getComponent(ItemFrame).setFrameItemInfo(no, amount);
                break
            }   
        }

        let scrollTransform = this.scrollView.getComponent(UITransform);
        let layout = this.scrollView.content.getComponent(Layout);
        let containerTransform = this.scrollView.content.getComponent(UITransform);
        if(scrollTransform != null && layout != null) {
            var scrollSize = itemSize + (count - 1) * layout.spacingX + layout.paddingLeft + layout.paddingRight;
            if(scrollSize > scrollTransform.width) {
                scrollSize = scrollTransform.width;
            }
            this.scrollView.view.setContentSize(scrollSize, this.scrollView.view.contentSize.height);
            if(containerTransform != null && scrollTransform.width < containerTransform.contentSize.width)
            {
                this.sprArrowRight.active = true;   
            }
        }
        this.scrollView.scrollToLeft();
    }

    ForceUpdate()
    {

    }
}

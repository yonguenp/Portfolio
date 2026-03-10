
import { _decorator, Node, instantiate, Button, Label, Sprite, EventHandler } from 'cc';
import { BuildingBaseData, BuildingOpenData } from '../../Data/BuildingData';
import { BuildingBaseTable, BuildingOpenTable } from '../../Data/BuildingTable';
import { SlotCostTable } from '../../Data/InventoryTable';
import { ProductData } from '../../Data/ProductData';
import { ProductTable } from '../../Data/ProductTable';
import { StringTable } from '../../Data/StringTable';
import { TableManager } from '../../Data/TableManager';
import { GameManager } from '../../GameManager';
import { NetworkManager } from '../../NetworkManager';
import { ResourceManager, ResourcesType } from '../../ResourceManager';
import { TimeManager } from '../../Time/TimeManager';
import { DataManager } from '../../Tools/DataManager';
import { GetChild } from '../../Tools/SandboxTools';
import { TutorialManager } from '../../Tutorial/TutorialManager';
import { BuildingInstance, eBuildingState, User } from '../../User/User';
import { PopupManager } from '../Common/PopupManager';
import { TapLayer } from '../Common/TapLayer';
import { receipeFrame } from '../ItemSlot/receipeFrame';
import { ProductSlotAdd } from '../ProductSlotAdd';
import { ReceipeCard } from '../ReceipeCard';
import { SystemRewardPopup } from '../SystemRewardPopup';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = ProductLayer
 * DateTime = Tue Jan 11 2022 15:32:54 GMT+0900 (대한민국 표준시)
 * Author = seungmin
 * FileBasename = ProductLayer.ts
 * FileBasenameNoExtension = ProductLayer
 * URL = db://assets/Scripts/UI/ProductLayer.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
@ccclass('ProductLayer')
export class ProductLayer extends TapLayer
{
    @property(Node)
    productQueueContent : Node = null

    @property(Node)
    receipeContent : Node = null
    
    @property(Node)
    sideButtonClone : Node = null

    private buildingIndex : string = ""
    private buildingTag : number
    private buildingLevel : number
    private cloneChild : Node[] = []
    private prevSpriteNode : Node = null

    Init()
    {
        super.Init()

        if(DataManager.GetData("ProductLayerTap"))
        {
            this.buildingIndex = DataManager.GetData("ProductLayerTap")
            DataManager.DelData("ProductLayerTap")
        }

        this.setLayerInfo()
        this.refreshProductQueueScroll()
        this.refreshReceipeScroll()
    }

    refreshProductQueueScroll()
    {
        this.productQueueContent.removeAllChildren()

        let queueLength = User.Instance.GetProduces(this.buildingTag) ? User.Instance.GetProduces(this.buildingTag).Slot : 4
        let queueList = User.Instance.GetProduces(this.buildingTag)?  User.Instance.GetProduces(this.buildingTag).Items : null
        let localCheck = 0  //오래된 생산큐 정보 내부 갱신용

        if(queueList != null)
            queueList.sort((a,b)=> b.State - a.State)

        for(let i = 0; i < queueLength; i++)
        {
            let itemClone : Node = instantiate(GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB)["receipeTimer"])
            itemClone.parent = this.productQueueContent

            if(queueList && queueList.length > i)
            {
                let receipeArray : ProductData[] = TableManager.GetTable(ProductTable.Name).Get(this.buildingIndex)
                let itemInfo = receipeArray.find(value=>value.ProductKey == queueList[i].RecipeID)
                
                if(queueList[i].State < 3)
                {
                    localCheck += itemInfo.ProductReqTime

                    if(queueList[i].State == 2)
                        localCheck = queueList[i].ProductionExp

                    itemClone.getComponent(receipeFrame).setReceipeIcon(i, itemInfo, localCheck, this.buildingTag)
                    if((0 < TimeManager.GetTimeCompare(localCheck) && TimeManager.GetTimeCompare(localCheck) <= itemInfo.ProductReqTime))
                        itemClone.getComponent(receipeFrame).timerStart();
                }
                else
                {
                    itemClone.getComponent(receipeFrame).setReceipeIcon(i, itemInfo, -1, this.buildingTag)
                }
            }
            else
            {
                itemClone.getComponent(receipeFrame).setFrameBlank()
            }
        }

        if(this.buildingTag != null)
        {
            let buildingOpenInfo = TableManager.GetTable<BuildingOpenTable>(BuildingOpenTable.Name).GetWithTag(this.buildingTag)
            let buildingBaseInfo = TableManager.GetTable<BuildingBaseTable>(BuildingBaseTable.Name).Get(buildingOpenInfo.BUILDING)
            let slotCostInfo = TableManager.GetTable<SlotCostTable>(SlotCostTable.Name).Get((queueLength - buildingBaseInfo.START_SLOT) + 1)

            if(queueLength < buildingBaseInfo.MAX_SLOT && slotCostInfo != null)
            {
                let slotClone : Node = instantiate(GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB)["emptySlot"])
                slotClone.parent = this.productQueueContent

                let customData = 
                {
                    tag : this.buildingTag,
                    type : slotCostInfo.COST_TYPE,
                    cost : slotCostInfo.COST_NUM
                }

                let newEvent : EventHandler = new EventHandler()
                newEvent.target = this.node
                newEvent.component = "ProductLayer"
                newEvent.handler = "onClickAddSlot"
                newEvent.customEventData = JSON.stringify(customData)

                slotClone.getComponent(Button).clickEvents.push(newEvent)
            }
        }
    }

    refreshReceipeScroll()
    {
        this.receipeContent.removeAllChildren()
        let receipeArray : ProductData[] = TableManager.GetTable(ProductTable.Name).Get(this.buildingIndex)

        //건물의 최대 수가 1개씩 이므로 배열이 아님, 추후에 건물 최대치가 늘어나면 배열로 수정 필요
        if(receipeArray == null || receipeArray.length <= 0){
            return;
        }

        receipeArray.forEach((element : ProductData)=>
        {
            let newReceipeCard = instantiate(GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB)["receipeCard"])
            newReceipeCard.getComponent(ReceipeCard).init(String(element.Index), element.ProductKey, this.buildingTag, this.buildingLevel >= element.BuildingLevel)

            newReceipeCard.parent = this.receipeContent;

            // 튜토리얼 체크
            if (element.ProductKey == 3000101) {
                if (TutorialManager.GetInstance.IsNowPlayingTutorialByGroup(104)) {
                    let findKey = TutorialManager.GetInstance.GetTutorialDataPropertyKey(104, 9);
                    let buttonNode = newReceipeCard?.getChildByName('btnProduct')?.getComponent(Button);
                    DataManager.AddData(findKey, buttonNode);

                    TutorialManager.GetInstance.OnTutorialEvent(104, 9);
                }
            }
        })
    }

    onClickProductBuilding(event, customEventData)
    {
        let jsonData = JSON.parse(customEventData)
        this.buildingIndex = jsonData.index

        if(this.prevSpriteNode != null)
        {
            this.prevSpriteNode.active = false
            this.prevSpriteNode = event.currentTarget.getComponentInChildren(Sprite).node
            this.prevSpriteNode.active = true
        }

        this.setLayerInfo()
        this.refreshReceipeScroll()
        this.refreshProductQueueScroll()
    }

    setLayerInfo()
    {
        while(this.cloneChild.length > 0)
            this.cloneChild.pop().removeFromParent()

        let buildingInstance : BuildingInstance
        let buildingTag : number
        let buildingIndexList : string[] = []

        buildingIndexList = this.GetUserBuildingList();

        if(this.buildingIndex == "")
        {
            this.buildingIndex = buildingIndexList[0]
        }

        TableManager.GetTable(BuildingOpenTable.Name).Get(5).forEach((element : BuildingOpenData)=>
        {
            if(element.BUILDING == this.buildingIndex)
            {
                buildingTag = element.INSTALL_TAG
            }
        })

        // for(let i = 0; i < buildingIndexList.length; i++)
        // {
        //     var newButton = instantiate(this.sideButtonClone)
        //     newButton.active = true
        //     newButton.parent = this.sideButtonClone.parent
        //     newButton.getComponentInChildren(Label).string = TableManager.GetTable(StringTable.Name).Get(TableManager.GetTable(BuildingBaseTable.Name).Get(buildingIndexList[i])._NAME).TEXT
        //     newButton.getComponent(Button).clickEvents[0].customEventData = JSON.stringify({index:buildingIndexList[i]})
            
        //     if(this.buildingIndex == buildingIndexList[i])
        //     {
        //         newButton.getComponent(Button).interactable = false;
        //         newButton.getComponentInChildren(Sprite).node.active = true;
        //         this.prevSpriteNode = newButton.getComponentInChildren(Sprite).node;
        //     }

        //     this.cloneChild.push(newButton)
        // }

        this.RefreshBuildingThumbnail(buildingIndexList);

        buildingInstance = User.Instance.GetUserBuildingList().find(value => value.Tag == Number(buildingTag))

        if(buildingInstance != null && buildingInstance != undefined)
        {
            this.buildingTag = buildingInstance.Tag
            this.buildingLevel = buildingInstance.Level
        }
    }
    GetUserBuildingList()
    {
        let buildingInfoList : BuildingBaseData[] = (TableManager.GetTable(BuildingBaseTable.Name) as BuildingBaseTable).GetAll()
        let buildingIndexList : string[] = [];
        TableManager.GetTable(BuildingOpenTable.Name).Get(5).forEach((element : BuildingOpenData)=>
        {
            for(let i = 0; i < buildingInfoList.length; i++)
            {
                if(buildingInfoList[i].TYPE > 2 && element.BUILDING == buildingInfoList[i].Index)
                {
                    let userBuildingList = User.Instance.GetUserBuildingList()
                    for(let j = 0; j < userBuildingList.length; j++)
                    {
                        if(userBuildingList[j].State == eBuildingState.NORMAL && userBuildingList[j].Tag == element.INSTALL_TAG)
                        {
                            buildingIndexList.push(buildingInfoList[i].Index as string)
                        }
                    }
                }
            }
        })
        return buildingIndexList;
    }

    RefreshBuildingThumbnail(buildingIndexList : string[])
    {
        //this.buildingIndex//현재 클릭한 빌딩 데이터
        for(let i = 0; i < buildingIndexList.length; i++)
        {
            if(this.buildingIndex == buildingIndexList[i])
            {
                let iconNode = GetChild(this.sideButtonClone, ['buildingIcon']);
                if(iconNode != null){
                    let iconSprite = iconNode.getComponent(Sprite);
                    if(iconSprite != null)
                    {
                        iconSprite.spriteFrame = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.BUILDINGICON_SPRITEFRAME)[this.buildingIndex];
                    }
                }
                let labelNode = GetChild(this.sideButtonClone, ['Label']);
                if(labelNode != null){
                    let label = labelNode.getComponent(Label);
                    if(label != null){
                        label.string = TableManager.GetTable(StringTable.Name).Get(TableManager.GetTable(BuildingBaseTable.Name).Get(buildingIndexList[i])._NAME).TEXT
                    }
                }
            }
        }
    }

    ForceUpdate()
    {
        this.setLayerInfo()
        this.refreshReceipeScroll()
        this.refreshProductQueueScroll()
    }

    onClickGetAll(event, CustomEventData)
    {
        let params = {}
        NetworkManager.Send("produce/harvest", params, (jsonObj) => 
        {
            PopupManager.ForceUpdate()

            let popup = PopupManager.OpenPopup("SystemRewardPopup") as SystemRewardPopup
            popup.initWithList(jsonObj.rewards)
        })
    }

    onClickAddSlot(event, CustomEventData)
    {
        //팝업 추가, 팝업 연결
        let jsonData = JSON.parse(CustomEventData)
        let popup = PopupManager.OpenPopup("ProductSlotAdd") as ProductSlotAdd
        popup.BuildingTag = jsonData.tag
        popup.Cost(jsonData.cost)
    }

    onClickRightButton()
    {
        let data = this.GetBuildingIndex(true);
        if(DataManager.GetData("ProductLayerTap"))
        {
            DataManager.DelData("ProductLayerTap")
        }
        DataManager.AddData("ProductLayerTap",data);
        
        this.Init();
    }

    onClickLeftButton()
    {
        let data = this.GetBuildingIndex(false);
        if(DataManager.GetData("ProductLayerTap"))
        {
            DataManager.DelData("ProductLayerTap")
        }
        DataManager.AddData("ProductLayerTap",data);
        
        this.Init();
    }

    GetBuildingIndex(isRightClick : boolean) : string
    {
        let buildingIndexList = this.GetUserBuildingList();
        if(buildingIndexList == null || buildingIndexList.length <= 0){
            return "";
        }

        let currentIndex = 0;
        for(let i = 0; i < buildingIndexList.length ; i++)
        {
            let buildingIndex = buildingIndexList[i];
            if(buildingIndex == this.buildingIndex)
            {
                currentIndex = i;
                break;
            }
        }

        let modifyIndex = 0;
        if(isRightClick)
        {
            modifyIndex = currentIndex + 1;
            if(modifyIndex >= buildingIndexList.length){
                modifyIndex = 0;
            }
        }else{
            modifyIndex = currentIndex - 1;
            if(modifyIndex < 0){
                modifyIndex = buildingIndexList.length - 1;
            }
        }
        return buildingIndexList[modifyIndex];
    }
}

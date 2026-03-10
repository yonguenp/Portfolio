
import { _decorator, Component, Node, Label, Prefab, instantiate } from 'cc';
import { ItemBaseData } from '../Data/ItemData';
import { ItemBaseTable } from '../Data/ItemTable';
import { ProductData } from '../Data/ProductData';
import { ProductTable } from '../Data/ProductTable';
import { StringData } from '../Data/StringData';
import { StringTable } from '../Data/StringTable';
import { TableManager } from '../Data/TableManager';
import { GameManager } from '../GameManager';
import { NetworkManager } from '../NetworkManager';
import { ResourceManager, ResourcesType } from '../ResourceManager';
import { TutorialManager } from '../Tutorial/TutorialManager';
import { PopupManager } from './Common/PopupManager';
import { ItemFrame } from './ItemSlot/ItemFrame';
import { receipeFrame } from './ItemSlot/receipeFrame';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = ReceipeCard
 * DateTime = Wed Jan 12 2022 15:41:03 GMT+0900 (대한민국 표준시)
 * Author = seungmin
 * FileBasename = ReceipeCard.ts
 * FileBasenameNoExtension = ReceipeCard
 * URL = db://assets/Scripts/UI/ReceipeCard.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
@ccclass('ReceipeCard')
export class ReceipeCard extends Component 
{
    @property(receipeFrame)
    productReceipeFrame : receipeFrame

    @property(Node)
    materialNodeParent : Node

    @property(Label)
    labelProductName : Label

    @property(Label)
    labelProductReqTime : Label

    @property(Node)
    blockNode : Node

    @property(Label)
    labelBlock : Label

    private buildingTAG : number
    private receipeID : number


    init(index : string, receipeID : number, targetBuildingTAG : number, canProduct : boolean)
    {
        //receipeID를 기준으로 레시피 테이블에서 해당 레시피에 필요한 정보들을 가져와 세팅
        let receipeInfoArray : ProductData[] = TableManager.GetTable(ProductTable.Name).Get(index);
        let receipeInfo : ProductData = null
        this.buildingTAG = targetBuildingTAG
        this.receipeID = receipeID
        
        receipeInfoArray.forEach((element)=>
        {
            if(element.ProductKey == receipeID)
            {
                receipeInfo = element
            }
        })

        if(receipeInfo == null)
        {
            console.error("ReceipeCard.init.receipeInfo == null")
            this.node.destroy()
            return
        }
        let itemInfo : ItemBaseData = TableManager.GetTable(ItemBaseTable.Name).Get(receipeInfo.ProductItemID)

        this.productReceipeFrame.setReceipeIcon(0, receipeInfo, 0, 0)
        this.labelProductName.string = (TableManager.GetTable(StringTable.Name).Get(itemInfo._NAME) as StringData).KOR
        this.labelProductReqTime.string = receipeInfo.ProductReqTime + "s"

        if(!canProduct)
        {
            this.blockNode.active = true
            this.labelBlock.string = `Lv.${receipeInfo.BuildingLevel} 필요`
        }

        //골드가 들어가는지? 재료가 들어가는지?
        //일단 작동하는지 확인위해 비효율적으로 구현
        if(receipeInfo.NeedGold > 0)
        {
            let clone : Prefab = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB)["item"]

            clone.createNode((err, material)=>{
                material.parent = this.materialNodeParent
                material.getComponent(ItemFrame).setFrameCashInfo(0, receipeInfo.NeedGold)
            })
        }
        else
        {
            let clone = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB)["item"]

            for(let i = 0; i < receipeInfo.NeedItemLength; i++) 
            {
                let material : Node = instantiate(clone)
                material.parent = this.materialNodeParent
                material.getComponent(ItemFrame).setFrameReceipeInfo(receipeInfo.NeedItemIDArray[i], receipeInfo.NeedItemAmountArray[i])
            }
        }
    }
    
    onClickProduct(customEventData : object)
    {
        //생산 시도, 대기열이 꽉차거나 재료가 부족한지 검사
        let params = {
            tag : this.buildingTAG,
            recipe : this.receipeID
        }

        NetworkManager.Send("produce/enqueue", params, (jsonObj)=>
        {
            PopupManager.ForceUpdate()
        })

        // 튜토리얼-104
        if (TutorialManager.GetInstance.IsNowPlayingTutorialByGroup(104)) {
            TutorialManager.GetInstance.OffTutorialEvent();
        }
    }
}
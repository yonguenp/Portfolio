
import { _decorator, Node, Label, Sprite, Slider } from 'cc';
import { ItemBaseData } from '../Data/ItemData';
import { ItemBaseTable } from '../Data/ItemTable';
import { StringTable } from '../Data/StringTable';
import { TableManager } from '../Data/TableManager';
import { GameManager } from '../GameManager';
import { NetworkManager } from '../NetworkManager';
import { ResourceManager, ResourcesType } from '../ResourceManager';
import { GetType, StringBuilder, Type } from '../Tools/SandboxTools';
import { User } from '../User/User';
import { PopupManager } from './Common/PopupManager';
import { ItemFrame } from './ItemSlot/ItemFrame';
import { SystemPopup } from './SystemPopup';
const { ccclass, property } = _decorator;
 
@ccclass('ItemInfoPopup')
export class ItemInfoPopup extends SystemPopup
{
    @property(Label)
    labelItemName : Label = null

    @property(Sprite)
    sprItemIcon : Sprite = null

    @property(Label)
    labelItemAmount : Label = null

    @property(Label)
    labelItemDesc : Label = null

    @property(Node)
    nodeBtnSale : Node = null

    @property(Node)
    nodeLabelCantSale : Node = null

    @property(Node)
    nodeDesc : Node = null

    @property(Node)
    nodeSale : Node = null

    @property(Label)
    labelSaleAmount : Label = null

    @property(Slider)
    sliderSaleAmount : Slider = null

    @property(Label)
    labelSalePrice : Label = null

    private itemInfo : ItemBaseData = null
    private sellAmount : number = 0

    Init(data: any = null)
    {
        super.Init(data);
        let itemID = 0; 
        if(data != null && data['itemID'] != undefined) {
            itemID = data['itemID'];
        }
        let itemData = User.Instance.GetAllItems().find(value => value.ItemNo == itemID);
        var itemCount = 0;
        if(itemData != null) {
            itemCount = itemData.Count;
        }
        this.itemInfo = TableManager.GetTable(ItemBaseTable.Name).Get(itemID)
        this.labelItemName.string = TableManager.GetTable<StringTable>(StringTable.Name).Get(this.itemInfo._NAME).TEXT
        this.sprItemIcon.spriteFrame = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.ITEMICON_SPRITEFRAME)[this.itemInfo.ICON]
        this.labelItemAmount.string = `${itemCount}`
        this.labelItemDesc.string = TableManager.GetTable<StringTable>(StringTable.Name).Get(this.itemInfo._DESC).TEXT

        if(this.itemInfo.SELL > 0)
            this.nodeBtnSale.active = true
        else
            this.nodeLabelCantSale.active = true
    }

    private onClickSale()
    {
        this.nodeDesc.active = false;
        this.nodeSale.active = true;
        this.sliderSaleAmount.progress = 1
        this.sellAmount = User.Instance.GetAllItems().find(value => value.ItemNo == this.itemInfo.Index).Count
        this.onChangeSaleAmount()
    }

    onChangeSaleAmount(CustomEventData : number = 0)
    {
        if(GetType(CustomEventData) == Type.Object)
            CustomEventData = 0

        let totalAmount = User.Instance.GetAllItems().find(value => value.ItemNo == this.itemInfo.Index).Count
        let salePerc = this.sliderSaleAmount.progress
        this.sellAmount = Math.round(totalAmount * salePerc) + CustomEventData

        if(this.sellAmount <= 0)
            this.sellAmount = 1
        else if(this.sellAmount >= totalAmount)
            this.sellAmount = totalAmount
            
        this.labelSaleAmount.string = StringBuilder("판매 수량 : {0}", this.sellAmount)
        this.sliderSaleAmount.progress = this.sellAmount / totalAmount
        this.labelSalePrice.string = (this.sellAmount * this.itemInfo.SELL).toString()
    }

    onClickPlus()
    {
        this.onChangeSaleAmount(1)
    }

    onClickMinus()
    {
        this.onChangeSaleAmount(-1)
    }

    OnClose()
    {
        super.OnClose();
        console.log(this.Data);
        (this.Data.targetItemFrame as ItemFrame).setFrameNormal();
    }

    private onClickConfirmSale()
    {
        let params =
        {
            item : this.itemInfo.Index,
            count :  this.sellAmount
        }

        NetworkManager.Send("item/sell", params, ()=>
        {
            this.ClosePopup()
            PopupManager.ForceUpdate()
        })
    }
}
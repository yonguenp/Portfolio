
import { _decorator, Component, Sprite, Label, Node, Vec3, Color, SpriteFrame} from 'cc';
import { ItemBaseData } from '../../Data/ItemData';
import { ItemBaseTable } from '../../Data/ItemTable';
import { StringTable } from '../../Data/StringTable';
import { TableManager } from '../../Data/TableManager';
import { GameManager } from '../../GameManager';
import { ResourceManager, ResourcesType } from '../../ResourceManager';
import { StringBuilder } from '../../Tools/SandboxTools';
import { User } from '../../User/User';
import { PopupManager } from '../Common/PopupManager';
import { ItemInfoPopup } from '../ItemInfoPopup';
import { ItemTooltip } from '../ItemTooltip';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = ItemFrame
 * DateTime = Tue Jan 11 2022 15:09:07 GMT+0900 (대한민국 표준시)
 * Author = seungmin
 * FileBasename = ItemFrame.ts
 * FileBasenameNoExtension = ItemFrame
 * URL = db://assets/Scripts/UI/ItemFrame.ts
 */

export enum  FrameFunctioal 
{
    NONE = 0,
    POPUP,
    TOOLTIP,
    CallBack,
}

export enum itemFrameType
{
    NONE,
    ITEM,
    GOLD,
    STEMINA,
    MAX
}

@ccclass('ItemFrame')
export class ItemFrame extends Component
{
    @property(Sprite)
    itemFrame : Sprite;

    @property(Sprite)
    itemIcon : Sprite;

    @property(Label)
    itemAmount : Label;

    @property(Node)
    expNode : Node;

    @property(SpriteFrame)
    spriteNormal : SpriteFrame = null;

    @property(SpriteFrame)
    spriteSelect : SpriteFrame = null;

    @property(SpriteFrame)
    spriteDisable : SpriteFrame = null;


    isInit : boolean = false
    eFuncType : FrameFunctioal = FrameFunctioal.NONE
    itemID : number = 0
    itemtype : itemFrameType = itemFrameType.NONE;

    isSufficientAmount : boolean = false;
    public get IsSufficientAmount(){return this.isSufficientAmount;}

    getItemID(){
        return this.itemID;
    }
    getItemAmountString(){
        return this.itemAmount.string;
    }

    clickItemIDCallback: (CustomEventData? : string) => void = null;

    onLoad()
    {
        if(!this.isInit)
        {
            this.setFrameBlank()
        }
    }

    init(isEmpty : boolean = false)
    {
        this.isInit = true
        if(isEmpty)
        {
            this.itemFrame.spriteFrame = this.spriteDisable
            return
        }
        this.itemFrame.spriteFrame = this.spriteNormal
    }

    /**
     * 비어있는 아이템 슬릇으로 설정
     */
    setFrameBlank()
    {
        this.init(true)
        if(this.itemIcon != null){
            this.itemIcon.spriteFrame = null
        }
        if(this.itemAmount != null){
            this.itemAmount.string = ""
        }
    }

    /**
     * 일반 아이템 슬릇으로 설정
     * @param itemID 아이템 번호
     * @param itemAmount 아이템 수량 - 기본값 = 유저 소지 수량
     */
    setFrameItemInfo(itemID : number, itemAmount? : number) : void
    {
        this.init()
        this.eFuncType = FrameFunctioal.TOOLTIP
        //itemID으로 Icon 가져오기
        //
        if( itemID == null || itemID == 0)
        {
            this.setFrameBlank()
            return;
        }

        let itemInfo :ItemBaseData = TableManager.GetTable<ItemBaseTable>(ItemBaseTable.Name).Get(itemID)
        if(Math.floor(itemID/10000000) == 10)
            this.itemIcon.spriteFrame = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.PARTICON_SPRITEFRAME)[itemInfo.ICON]
        else
            this.itemIcon.spriteFrame = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.ITEMICON_SPRITEFRAME)[itemInfo.ICON]
        this.itemID = itemID

        if(itemAmount != null)
            this.itemAmount.string = itemAmount.toString()
        //else
        //  this.itemAmount.string = 유저 소지 수량 가져오기
        
        this.itemtype = itemFrameType.ITEM;
    }
/**
 * 
 * @param itemID 아이템 번호
 * @param itemAmount 아이템 수량
 * @param expAmount 해당 아이템 경험치 수량
 */
    setFrameItemExpInfo(itemID : number, itemAmount : number, expAmount : number)
    {
        this.init();
        //itemID으로 Icon 가져오기
        //
        if( itemID == null || itemID == 0)
        {
            this.setFrameBlank();
            return;
        }

        let itemInfo :ItemBaseData = TableManager.GetTable<ItemBaseTable>(ItemBaseTable.Name).Get(itemID)
        this.itemIcon.spriteFrame = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.ITEMICON_SPRITEFRAME)[itemInfo.ICON]
        this.itemID = itemID

        if(itemAmount != null){
            this.itemAmount.string = itemAmount.toString();
        }
            
        if(expAmount > 0)
        {
            if(this.expNode != null)
            {
                this.expNode.active = true;
                let expLabelNode = this.expNode.getChildByName('expLabel');
                if(expLabelNode != null)
                {
                    expLabelNode.getComponent(Label).string = `EXP ${expAmount.toString()}`;
                }
            }
        }
    }

    /**
     * 재료 아이템 슬릇으로 설정
     * 요구 수량 / 소지 수량 으로 변경 됨, 요구 수량 불충족시, ItemFrame을 Invaild이미지로 변경
     * @param requireItemID 요구 아이템 번호
     * @param requireAmount 요구 아이템 수량
     */
    setFrameReceipeInfo(requireItemID : number, requireAmount : number)
    {
        this.init()
        this.eFuncType = FrameFunctioal.TOOLTIP
        if(requireItemID == 0)
        {
            
        }
        let itemInfo :ItemBaseData = TableManager.GetTable<ItemBaseTable>(ItemBaseTable.Name).Get(requireItemID)
        let item = User.Instance.GetAllItems().find(value => value.ItemNo == requireItemID);
        let userItem = 0;

        if(item != null) {
            userItem = item.Count;
        }

        this.itemIcon.spriteFrame = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.ITEMICON_SPRITEFRAME)[itemInfo.ICON]
        this.itemID = requireItemID
        this.itemAmount.string = StringBuilder("{0}/{1}", userItem, requireAmount)
        this.isSufficientAmount = true;
        if(userItem < requireAmount)
        {
            this.itemAmount.color = new Color(255,0,0,255)
            this.isSufficientAmount = false;
        }
        this.itemtype = itemFrameType.ITEM;
    }
    
    

    /**
     * 재화 슬릇으로 설정
     * 재화는 아이템이 아니므로 재화전용 슬릇으로 변경
     * 수량을 명시할 경우 요구 수량 / 소지 수량으로 변경 됨
     * @param cashType 재화 타입
     * @param cashAmount 재화 수량 - 기본값 = 유저 소지 수량
     */
    setFrameCashInfo(cashType : number, cashAmount?:number)
    {
        this.init()
        this.eFuncType = FrameFunctioal.TOOLTIP
        this.itemIcon.spriteFrame = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.ITEMICON_SPRITEFRAME)["gold"];
        if(cashAmount != null)
        {
            this.itemAmount.string = cashAmount.toString();//`${cashAmount}g`
        }
        //else
        //유저 데이터에서 가져오기
        //this.itemID
        this.itemtype = itemFrameType.GOLD;
    }

    setFrameEnergyInfo(Amount : number)
    {
        this.init()
        this.eFuncType = FrameFunctioal.TOOLTIP
        this.itemIcon.spriteFrame = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.ITEMICON_SPRITEFRAME)["energy"];
        
        if(Amount != null)
        {
            this.itemAmount.string = String(Amount)
        }
        this.itemtype = itemFrameType.STEMINA;
    }

    /**
     * 인벤토리 내부 기능 이용하기
     */
    setInventoryFunc()
    {
        this.eFuncType = FrameFunctioal.POPUP
    }
    setCallback(ok_cb? : (CustomEventData? : string) => void)
    {
        this.eFuncType = FrameFunctioal.CallBack;

        if(ok_cb != null)
        {
            this.clickItemIDCallback = ok_cb;
        }
    }

    public setFrameSelect()
    {
        this.itemFrame.spriteFrame = this.spriteSelect
    }

    public setFrameNormal()
    {
        this.itemFrame.spriteFrame = this.spriteNormal
    }

    onClick(event, custom)
    {
        switch(this.eFuncType)
        {
            case FrameFunctioal.NONE:
                return;

            case FrameFunctioal.POPUP:
            {
                this.setFrameSelect();
                PopupManager.OpenPopup("ItemInfoPopup", true, {itemID: this.itemID, targetItemFrame : this}) as ItemInfoPopup;
                break;   
            }
                
            case FrameFunctioal.TOOLTIP:
            {
                if(this.itemID < 0)
                    return;

                let itemName = "";
                let itemDESC = "";
                this.setFrameSelect();
                switch(this.itemtype)
                {
                    case itemFrameType.ITEM:
                    {
                        if(this.itemID <= 0){
                            return;
                        }
                        let itemInfo :ItemBaseData = TableManager.GetTable<ItemBaseTable>(ItemBaseTable.Name).Get(this.itemID)
                        if(itemInfo != null) 
                        {
                            itemName = StringTable.GetString(itemInfo._NAME);
                            itemDESC = StringTable.GetString(itemInfo._DESC);
                        }
                    }break;
                    case itemFrameType.GOLD:
                    {
                        let itemInfo :ItemBaseData = TableManager.GetTable<ItemBaseTable>(ItemBaseTable.Name).Get(10000001);
                        if(itemInfo != null)
                        {
                            itemName = StringTable.GetString(itemInfo._NAME);
                            itemDESC = StringTable.GetString(itemInfo._DESC);
                        }else{
                            itemName = StringTable.GetString(100000004);
                            itemDESC = StringTable.GetString(100000802);
                        }
                    }break;
                    case itemFrameType.STEMINA:
                    {
                        let itemInfo :ItemBaseData = TableManager.GetTable<ItemBaseTable>(ItemBaseTable.Name).Get(10000002);
                        if(itemInfo != null)
                        {
                            itemName = StringTable.GetString(itemInfo._NAME);
                            itemDESC = StringTable.GetString(itemInfo._DESC);
                        }else{
                            itemName = StringTable.GetString(100000004);
                            itemDESC = StringTable.GetString(100000802);
                        }
                    }break;
                }

                let popup = PopupManager.OpenPopup("tooltip", true, {targetItemFrame : this}) as ItemTooltip;
                let btnWorldpos = this.node.worldPosition;
                btnWorldpos = new Vec3(btnWorldpos.x - 20, btnWorldpos.y + 10);
                popup.setMessage(itemName, itemDESC);
                popup.setTooltipPosition(btnWorldpos);
                break;
            }    
            case FrameFunctioal.CallBack:
            {
                if(this.clickItemIDCallback != null){
                    this.clickItemIDCallback(this.itemID.toString());
                }
                break;
            }
        }
    }
}
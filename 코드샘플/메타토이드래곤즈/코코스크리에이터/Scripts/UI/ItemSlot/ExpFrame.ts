
import { _decorator, Component, SpriteFrame, Sprite, Label, Vec3 } from 'cc';
import { ItemBaseData } from '../../Data/ItemData';
import { ItemBaseTable } from '../../Data/ItemTable';
import { StringTable } from '../../Data/StringTable';
import { TableManager } from '../../Data/TableManager';
import { PopupManager } from '../Common/PopupManager';
import { ItemInfoPopup } from '../ItemInfoPopup';
import { ItemTooltip } from '../ItemTooltip';
import { FrameFunctioal, ItemFrame } from './ItemFrame';
const { ccclass, property } = _decorator;

export enum ExpType
{
    ACCOUNT_EXP,
    CHAR_EXP
}

@ccclass('ExpFrame')
export class ExpFrame extends ItemFrame 
{
    @property(SpriteFrame)
    sprAccountExp : SpriteFrame = null

    @property(SpriteFrame)
    sprCharExp : SpriteFrame = null

    @property(Sprite)
    sprIcon : Sprite = null

    @property(Label)
    labelAmount : Label = null

    slottype : ExpType = null;
    
    init(isEmpty : boolean = false)
    {
        this.isInit = true;
        this.itemFrame.spriteFrame = this.spriteNormal;
    }

    setFrameExpInfo(expType : ExpType, amount : number)
    {
        this.eFuncType = FrameFunctioal.TOOLTIP;
        this.labelAmount.string = amount.toString()

        switch(expType)
        {
            case ExpType.ACCOUNT_EXP:
            {
                this.sprIcon.spriteFrame = this.sprAccountExp
            }
            break

            case ExpType.CHAR_EXP:
            {
                this.sprIcon.spriteFrame = this.sprCharExp
            }
            break
        }
        this.slottype = expType;
        this.itemFrame.spriteFrame = this.spriteNormal;
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
                let infoName = "";
                let infoDESC = "";
                if(this.slottype == ExpType.CHAR_EXP)
                {
                    infoName = StringTable.GetString(100000018,"캐릭터 경험치");
                    infoDESC = StringTable.GetString(100000801,"캐릭터 경험치 설명");
                }else{
                    infoName = StringTable.GetString(100000799,"계정 경험치");
                    infoDESC = StringTable.GetString(100000800,"계정 경험치 설명");
                }

                this.setFrameSelect();
                let popup = PopupManager.OpenPopup("tooltip", true, {targetItemFrame : this}) as ItemTooltip;
                let btnWorldpos = this.node.worldPosition;
                btnWorldpos = new Vec3(btnWorldpos.x - 20, btnWorldpos.y + 10);

                popup.setMessage(infoName, infoDESC);
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
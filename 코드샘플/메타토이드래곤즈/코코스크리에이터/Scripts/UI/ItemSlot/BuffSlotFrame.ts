
import { _decorator, Component, Node, Sprite, SpriteFrame } from 'cc';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = BuffSlotFrame
 * DateTime = Thu May 19 2022 20:57:01 GMT+0900 (대한민국 표준시)
 * Author = jinwonjun
 * FileBasename = BuffSlotFrame.ts
 * FileBasenameNoExtension = BuffSlotFrame
 * URL = db://assets/Scripts/UI/ItemSlot/BuffSlotFrame.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
enum skillIndex
{
    INCREASE_ATK,
    INCREASE_ATK_PER,
    INCREASE_DEF,
    INCREASE_DEF_PER,
    INCREASE_CRI_RATE_PER,
    INCREASE_CRI_DMG_PER, 
    INCREASE_DODGE_PER,
    INCREASE_HIT_PER,
    DECREASE_ATK,
    DECREASE_ATK_PER,
    DECREASE_DEF,
    DECREASE_DEF_PER,
    DECREASE_CRI_RATE_PER,
    DECREASE_CRI_DMG_PER,
    DECREASE_DODGE_PER,
    DECREASE_HIT_PER,
    STUN,
    AIRBORNE,
    INVINCIBILITY,
    SILENCE,
    TICK_DMG,
    MAX,
}

@ccclass('BuffSlotFrame')
export class BuffSlotFrame extends Component {
    @property(Sprite)
    iconTarget : Sprite = null;

    @property(SpriteFrame)
    buffSpriteList : SpriteFrame[] = [];

    start () {
        // [3]
    }

    setIcon(skillName : string)
    {
        let checkIndex = 0;
        for(let i = skillIndex.INCREASE_ATK ; i < skillIndex.MAX ; i++)
        {
            let isContain = skillName.includes(skillIndex[i]);
            if(isContain){
                checkIndex = i;
                break;
            }
        }
        

        if(this.buffSpriteList == null || this.buffSpriteList.length <= 0){
            return;
        }

        if(this.buffSpriteList.length <= checkIndex){
            return;
        }

        this.iconTarget.spriteFrame = this.buffSpriteList[checkIndex];
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

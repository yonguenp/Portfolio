
import { _decorator, Component, Node, SpriteFrame, Sprite, ScrollView, Prefab, instantiate } from 'cc';
import { CharBaseTable } from '../../Data/CharTable';
import { TableManager } from '../../Data/TableManager';
import { GameManager } from '../../GameManager';
import { ResourceManager, ResourcesType } from '../../ResourceManager';
import { BuffSlotFrame } from './BuffSlotFrame';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = SkillTimerSlotFrame
 * DateTime = Tue Apr 26 2022 12:06:30 GMT+0900 (대한민국 표준시)
 * Author = jinwonjun
 * FileBasename = SkillTimerSlotFrame.ts
 * FileBasenameNoExtension = SkillTimerSlotFrame
 * URL = db://assets/Scripts/UI/ItemSlot/SkillTimerSlotFrame.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('SkillTimerSlotFrame')
export class SkillTimerSlotFrame extends Component {

    @property(Sprite)
    dragonThumbNail : Sprite = null;

    @property(Node)
    deadIndicator : Node = null;

    @property(Node)
    buffContentNode : Node = null;

    @property(Prefab)
    buffPrefab : Prefab = null;

    private dragonTag : number = -1;
    public get DragonTag(){return this.dragonTag;}

    SetData(dragonTag : number)
    {
        if(dragonTag > 0)
        {
            this.dragonTag = dragonTag;

            this.SetThumbnail();
        }
    }

    SetThumbnail()
    {
        if(this.dragonTag < 0){
            return;
        }

        let dragonInfo = TableManager.GetTable<CharBaseTable>(CharBaseTable.Name).Get(this.dragonTag);
        let icon : any = null;
        if(dragonInfo != null){
            icon = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.CHARACTORICON_SPRITEFRAME)[dragonInfo.THUMBNAIL]
        
            if(icon == null){
                icon = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.CHARACTORICON_SPRITEFRAME)["character_01"]
            }
        }

        this.dragonThumbNail.spriteFrame = icon;
    }

    addBuff(skillname : string)
    {
        if(this.buffPrefab != null){
            let isExist = this.existBuff(skillname);
            if(!isExist)
            {
                let clone = instantiate(this.buffPrefab);
                clone.parent = this.buffContentNode;
                clone.name = skillname;
                let buffFrame = clone.getComponent(BuffSlotFrame);
                buffFrame.setIcon(skillname);
            }
        }
    }

    removeBuff(skillname : string)
    {
        let contentList = this.buffContentNode.children;
        if(contentList == null || contentList.length <= 0){
            return;
        }

        for(let i = 0 ; i< contentList.length ; i++){
            let child = contentList[i];
            if(child.name == skillname){
                child.removeFromParent();
                break;
            }
        }
    }

    existBuff(skillname : string)
    {
        let contentList = this.buffContentNode.children;
        if(contentList == null || contentList.length <= 0){
            return false;
        }

        for(let i = 0 ; i< contentList.length ; i++){
            let child = contentList[i];
            if(child.name == skillname){
                return true;
            }
        }
        return false;
    }

    ClearBuff()
    {
        this.buffContentNode.removeAllChildren();
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

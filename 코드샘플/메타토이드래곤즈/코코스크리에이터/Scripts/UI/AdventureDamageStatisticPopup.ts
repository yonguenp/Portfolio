
import { _decorator, Component, Node, instantiate, Label, ProgressBar, tween } from 'cc';
import { CharBaseTable } from '../Data/CharTable';
import { TableManager } from '../Data/TableManager';
import { GameManager } from '../GameManager';
import { ResourceManager, ResourcesType } from '../ResourceManager';
import { GetChild, TimeStringMinute } from '../Tools/SandboxTools';
import { User } from '../User/User';
import { Popup } from './Common/Popup';
import { DragonPortraitFrame } from './ItemSlot/DragonPortraitFrame';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = AdventureDamageStatisticPopup
 * DateTime = Tue May 24 2022 21:21:20 GMT+0900 (대한민국 표준시)
 * Author = jinwonjun
 * FileBasename = AdventureDamageStatisticPopup.ts
 * FileBasenameNoExtension = AdventureDamageStatisticPopup
 * URL = db://assets/Scripts/UI/AdventureDamageStatisticPopup.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */

 const dragonMaxCount : number = 5;
@ccclass('AdventureDamageStatisticPopup')
export class AdventureDamageStatisticPopup extends Popup {
    
    @property(Node)
    layoutContent : Node = null;

    @property(ProgressBar)
    progressBarList : ProgressBar[] = [];

    @property(Label)
    percentLabel : Label[] = [];

    @property(Label)
    timeLabel : Label = null;

    private damageList :any = {};
    totalDamage : number = 0;

    
    Init(data?: any)
    {
        super.Init(data);

        this.damageList = data.damage;
        this.SetTotalDamage();

        let charData = data.dragons;
        if(charData != null) {
            let dragonsKeys = Object.keys(charData);
            dragonsKeys = dragonsKeys.reverse();
            const dragonsCount = dragonsKeys.length;
            for(var i = 0 ; i < dragonMaxCount ; i++) 
            {
                if(i < dragonsCount)
                {
                    const bTag = Number(dragonsKeys[i]);
                    if(charData[bTag] == null) 
                    {
                        continue;
                    }
        
                    let dragonInfo = User.Instance.DragonData.GetDragon(charData[bTag]['dtag']);
                    let prefabClone = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.UI_PREFAB)["DragonPortraitSlot"]
                    let clone : Node = instantiate(prefabClone);
                    clone.parent = this.layoutContent;
                    clone.getComponent(DragonPortraitFrame).SetDragonPortraitFrame(dragonInfo);

                    this.progressBarList[i].node.active = true;
                    this.percentLabel[i].node.active = true;

                    let currentDamage = this.GetDamage(Number(dragonInfo.Tag));
                    let calcNumber = "";
                    if(this.totalDamage > 0)
                    {
                        calcNumber = ((currentDamage / this.totalDamage).toFixed(2));
                    }
                     
                    //this.progressBarList[i].progress = Number(calcNumber);
                    this.percentLabel[i].string =`${(Number(calcNumber) * 100).toFixed(0)}%`;

                    let dmgLabel = GetChild(this.progressBarList[i].node, ['exp_label']).getComponent(Label);
                    dmgLabel.string = `${currentDamage} / ${this.totalDamage}`

                    tween(this.progressBarList[i]).to(0,{progress : 0}).to(0.3, {progress : Number(calcNumber)}).union().start();
                }
                else
                {
                    let progressParent = this.progressBarList[i].node.parent;
                    let strokeNode = GetChild(progressParent,['Stroke']);
                    if(strokeNode != null){
                        strokeNode.active = false;
                    }
                    this.progressBarList[i].node.active = false;
                    this.percentLabel[i].node.active = false;
                }
            }    
        }
        this.timeLabel.string = TimeStringMinute(data.time);
    }

    GetDamage(tag : number)
    {
        if(this.damageList == null){
            return 0;
        }
        return this.damageList[tag];
    }
    SetTotalDamage()
    {
        if(this.damageList == null){
            this.totalDamage = 0;
            return;
        }
        let tempDamage = 0;
        let keys = Object.keys(this.damageList);
        if(keys == null || keys.length <= 0){
            this.totalDamage = 0;
            return;
        }

        keys.forEach(element =>{
            let tag = Number(element);
            if(tag <= 0){
                return;
            }
            tempDamage += Number(this.damageList[tag]);
        })

        this.totalDamage = tempDamage;
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

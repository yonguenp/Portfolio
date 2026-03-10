
import { _decorator} from 'cc';
import { TutorialManager } from '../../Tutorial/TutorialManager';
import { SubLayer } from '../Common/SubLayer';
import { dragonCharacterSlotComponent } from './detailInfoLayer/dragonCharacterSlotComponent';
import { dragonDescComponent } from './detailInfoLayer/dragonDescComponent';
import { dragonEquipmentSlotComponent } from './detailInfoLayer/dragonEquipmentSlotComponent';
import { dragonSkillDescComponent } from './detailInfoLayer/dragonSkillDescComponent';
import { dragonStatComponent } from './detailInfoLayer/dragonStatComponent';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = DragonDetailInfoLayer
 * DateTime = Thu Mar 10 2022 17:22:30 GMT+0900 (대한민국 표준시)
 * Author = wonjun_gs
 * FileBasename = DragonDetailInfoLayer.ts
 * FileBasenameNoExtension = DragonDetailInfoLayer
 * URL = db://assets/Scripts/UI/DragonManagement/DragonDetailInfoLayer.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('DragonDetailInfoLayer')
export class DragonDetailInfoLayer extends SubLayer {
    
    @property(dragonStatComponent)
    dragonStat : dragonStatComponent = null;

    @property(dragonDescComponent)
    dragonDesc : dragonDescComponent = null;

    @property(dragonCharacterSlotComponent)
    dragonCharacter : dragonCharacterSlotComponent = null;

    @property(dragonSkillDescComponent)
    dragonSkillDesc : dragonSkillDescComponent = null;

    @property(dragonEquipmentSlotComponent)
    dragonEquipSlot : dragonEquipmentSlotComponent = null;

    Init()
    {
        if(this.dragonStat != null){
            this.dragonStat.init();
        }
        if(this.dragonDesc != null){
            this.dragonDesc.init();
        }
        if(this.dragonCharacter != null){
            this.dragonCharacter.init();
        }
        if(this.dragonSkillDesc != null){
            this.dragonSkillDesc.init();
        }
        if(this.dragonEquipSlot != null){
            this.dragonEquipSlot.init();
        }

        // 튜토리얼 진행
        TutorialManager.GetInstance.OnTutorialEvent(109, 5);
        TutorialManager.GetInstance.OnTutorialEvent(114, 7);
        TutorialManager.GetInstance.OnTutorialEvent(116, 5);
    }
    ForceUpdate()
    {
        this.Init();   
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

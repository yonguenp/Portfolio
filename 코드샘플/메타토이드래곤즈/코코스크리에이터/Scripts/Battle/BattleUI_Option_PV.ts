
import { _decorator, Component, Node, CCBoolean } from 'cc';
import { BattleUI } from './BattleUI';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = BattleUI_Option_PV
 * DateTime = Fri Apr 22 2022 12:08:13 GMT+0900 (대한민국 표준시)
 * Author = wonjun_gs
 * FileBasename = BattleUI_Option_PV.ts
 * FileBasenameNoExtension = BattleUI_Option_PV
 * URL = db://assets/Scripts/Battle/BattleUI_Option_PV.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('BattleUI_Option_PV')
export class BattleUI_Option_PV extends Component {

    @property(CCBoolean)
    isVisibleCharacterHPBar : boolean;

    @property(CCBoolean)
    isVisibleMonsterHPBar : boolean;

    @property(CCBoolean)
    isVisibleSkillTimer : boolean;
    
    @property(CCBoolean)
    isVisibleTeamPartUI : boolean;

    @property(CCBoolean)
    isVisibleTimeUI : boolean;

    @property(CCBoolean)
    isVisibleAutoUI : boolean;

    @property(CCBoolean)
    isVisibleTopUI : boolean;

    start () {
        BattleUI.SetVisibleCharacterHpBar(this.isVisibleCharacterHPBar);
        BattleUI.SetVisibleCharacterSkillTimer(this.isVisibleSkillTimer);
        BattleUI.SetVisibleTeamParts(this.isVisibleTeamPartUI);
        BattleUI.SetVisibleTimeUI(this.isVisibleTimeUI);
        BattleUI.SetVisibleAutoUI(this.isVisibleAutoUI);
        BattleUI.SetVisibleTopUI(this.isVisibleTopUI);

        //BattleUI.SetVisibleMonsterHpBar(this.isVisibleMonsterHPBar);
    }
    update()
    {
        //console.log(!BattleUI.isWaveStart());
        if(!BattleUI.isWaveStart())
        {
            BattleUI.SetVisibleMonsterHpBar(this.isVisibleMonsterHPBar);
        }
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


import { _decorator, Component, Sprite, Vec3 } from 'cc';
import { MonsterBaseTable } from '../../Data/MonsterTable';
import { StringTable } from '../../Data/StringTable';
import { TableManager } from '../../Data/TableManager';
import { GameManager } from '../../GameManager';
import { ResourceManager, ResourcesType } from '../../ResourceManager';
import { PopupManager } from '../Common/PopupManager';
import { ItemTooltip } from '../ItemTooltip';
const { ccclass, property } = _decorator;
 
@ccclass('EnemyFrame')
export class EnemyFrame extends Component 
{
    @property(Sprite)
    sprFrame : Sprite = null

    @property(Sprite)
    sprIcon : Sprite = null


    monsterIndex : number = 0;
    enemyInfoTable : MonsterBaseTable = null;
    /**
     * 슬릇 초기화 func
     * @param index Enemy index번호
     */
    SetEnemyFrame(index : number = 0)
    {
        if(this.enemyInfoTable == null){
            this.enemyInfoTable = TableManager.GetTable<MonsterBaseTable>(MonsterBaseTable.Name);
        }
        let enemyInfo = this.enemyInfoTable.Get(index);
        let icon = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.CHARACTORICON_SPRITEFRAME)[enemyInfo.IMAGE]
        
        if(icon == null)
        {
            icon = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.CHARACTORICON_SPRITEFRAME)["enemy_0"]
        }
        this.sprIcon.spriteFrame = icon
        this.monsterIndex = index;
    }

    onClickFrame()
    {
        let enemyInfo = this.enemyInfoTable.Get(this.monsterIndex);
        let popup = PopupManager.OpenPopup("tooltip", true) as ItemTooltip;
        let btnWorldpos = this.node.worldPosition;
        btnWorldpos = new Vec3(btnWorldpos.x - 20, btnWorldpos.y + 10);
        popup.setMessage(StringTable.GetString(enemyInfo._NAME), StringTable.GetString(enemyInfo._DESC));
        popup.setTooltipPosition(btnWorldpos);
    }
}

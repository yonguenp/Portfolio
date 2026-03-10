
import { _decorator, Component, Node, Label } from 'cc';
import { AccountTable } from '../Data/AccountTable';
import { TableManager } from '../Data/TableManager';
import { StringBuilder } from '../Tools/SandboxTools';
import { User } from '../User/User';
import { Popup } from './Common/Popup';
const { ccclass, property } = _decorator;
 
@ccclass('AccountLevelUp')
export class AccountLevelUp extends Popup
{
    @property(Label)
    labelUpdateLevel : Label = null

    @property(Label)
    labelbody : Label = null

    Init(data?: any)
    {
        let level = User.Instance.UserData.Level
        let maxStemina = TableManager.GetTable<AccountTable>(AccountTable.Name).Get(level)
        
        this.labelUpdateLevel.string = StringBuilder("Lv. {0} >> Lv. {1}", level-1, level)
        this.labelbody.string = StringBuilder(this.labelbody.string, maxStemina.MAX_STAMINA)

        super.Init(data);
    }
}
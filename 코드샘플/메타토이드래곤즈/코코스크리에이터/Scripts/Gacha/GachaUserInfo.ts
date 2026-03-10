
import { _decorator, Component, Label, director, Node, ProgressBar } from 'cc';
import { AccountData } from '../Data/AccountData';
import { AccountTable } from '../Data/AccountTable';
import { TableManager } from '../Data/TableManager';
import { ObjectCheck, StringBuilder } from '../Tools/SandboxTools';
import { User } from '../User/User';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = GachaUserInfo
 * DateTime = Fri Mar 11 2022 18:30:22 GMT+0900 (대한민국 표준시)
 * Author = blacktopaz
 * FileBasename = GachaUserInfo.ts
 * FileBasenameNoExtension = GachaUserInfo
 * URL = db://assets/Scripts/GachaUserInfo.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('GachaUserInfo')
export class GachaUserInfo extends Component {
    
    @property(Label)
    labelUserLevel : Label = null

    @property(Label)
    labelUserName : Label = null

    @property(Label)
    labelUserExp : Label = null

    @property(ProgressBar)
    progressUserExp : ProgressBar = null

    @property(Label)
    labelGold : Label = null

    @property(Label)
    labelCurEnergy : Label = null

    @property(Label)
    labelMaxEnergy : Label = null

    start () {
        this.UpdateUICanvas();
    }

    RefershUI() : void {
        this.UpdateUICanvas();    
    }


    UpdateUICanvas()
    {
        this.labelUserLevel.string = StringBuilder("Lv. {0}", User.Instance.UserData.Level)
        this.labelUserName.string = StringBuilder("{0}", User.Instance.UserData.UserID)
        
        this.labelGold.string = StringBuilder("{0}", User.Instance.UserData.Gold)
        this.labelCurEnergy.string = StringBuilder("{0} / ", User.Instance.UserData.Energy)
        this.labelMaxEnergy.string = StringBuilder("{0}", TableManager.GetTable<AccountTable>(AccountTable.Name).Get(User.Instance.UserData.Level).MAX_STAMINA)

        let accountLevelData : AccountData = TableManager.GetTable<AccountTable>(AccountTable.Name).Get(User.Instance.UserData.Level)

        this.labelUserExp.string = StringBuilder("{0}", accountLevelData.EXP - User.Instance.UserData.Exp)
        this.progressUserExp.progress = Math.fround(User.Instance.UserData.Exp / accountLevelData.EXP)
    }

    // update (deltaTime: number) {
    //     // [4]
    // }
}


import { _decorator, Component, Node, Label } from 'cc';
import { User } from '../User/User';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = SettingAccountLayer
 * DateTime = Wed Apr 27 2022 11:28:27 GMT+0900 (대한민국 표준시)
 * Author = blacktopaz
 * FileBasename = SettingAccountLayer.ts
 * FileBasenameNoExtension = SettingAccountLayer
 * URL = db://assets/Scripts/Settings/SettingAccountLayer.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('SettingAccountLayer')
export class SettingAccountLayer extends Component 
{
    @property(Label)
    labelAccNo : Label = null;
    
    Init() 
    {
        this.labelAccNo.string += User.Instance.UNO;
    }
}
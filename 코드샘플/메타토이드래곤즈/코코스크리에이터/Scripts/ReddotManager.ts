
import { _decorator, Component} from 'cc';
import { IManagerBase } from 'sb';
import { GameManager } from './GameManager';
import { ReddotUI } from './ReddotUI';
import { TimeManager } from './Time/TimeManager';
import { eBuildingState, eLandmarkType, User } from './User/User';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = ReddotManager
 * DateTime = Mon May 02 2022 17:33:41 GMT+0900 (대한민국 표준시)
 * Author = jinwonjun
 * FileBasename = ReddotManager.ts
 * FileBasenameNoExtension = ReddotManager
 * URL = db://assets/Scripts/ReddotManager.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('ReddotManager')
export class ReddotManager extends Component implements IManagerBase {
    public static Name: string = "ReddotManager";

    reddotUI : ReddotUI = null;

    GetManagerName(): string {
        return ReddotManager.Name;
    }
    Update(deltaTime: number): void {
    }
    
    onLoad () {
        this.node.name = this.GetManagerName();

        GameManager.Instance.AddManager(this, false);
    }

    start()
    {
        ReddotManager.onShowBuildingReddot();
    }

    onDestroy() {
        GameManager.Instance.DelManager(this);
    }

    isCurrentNewItem() : boolean
    {
        return true;
    }
    
    static onShowInventoryReddot()//아이템 태그로?
    {

    }

    isCurrentNewBuilding() : boolean
    {
        let CheckFlag = false;
        let userBuildings = User.Instance.GetUserBuildingList();
        for(let i = 0; i < userBuildings.length; i++)
        {
            if([eLandmarkType.COINDOZER, eLandmarkType.WORLDTRIP, eLandmarkType.SUBWAY].indexOf(userBuildings[i].Tag) >= 0)
                continue

            if(userBuildings[i].State == eBuildingState.CONSTRUCT_FINISHED || userBuildings[i].State == eBuildingState.CONSTRUCTING && userBuildings[i].ActiveTime <= TimeManager.GetTime())
            {
                CheckFlag = true;
                return CheckFlag;
            }
        }
        return CheckFlag;
    }
    onVisibleBuildingList()
    {
        if(this.node != null){
            if(this.reddotUI == null)
            {
                this.reddotUI = this.node.getComponent(ReddotUI);
            }
            this.reddotUI.onVisibleBuildingList();
        }
    }

    public static onShowBuildingReddot()
    {
        let myManager: ReddotManager = GameManager.GetManager(ReddotManager.Name)
        let isNewBuilding = myManager.isCurrentNewBuilding();
        if(isNewBuilding)
        {
            myManager.onVisibleBuildingList();
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

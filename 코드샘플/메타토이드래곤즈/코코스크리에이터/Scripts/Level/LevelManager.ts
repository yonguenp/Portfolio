
import { _decorator, Component } from 'cc';
import { IManagerBase } from 'sb';
import { AIManager } from '../AI/AIManager';
import { GameManager } from '../GameManager';
import { MapManager } from '../Map/MapManager';
import { User } from '../User/User';
const { ccclass, executionOrder } = _decorator;

/**
 * Predefined variables
 * Name = LevelManager
 * DateTime = Tue Jan 04 2022 17:32:50 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = LevelManager.ts
 * FileBasenameNoExtension = LevelManager
 * URL = db://assets/Scripts/LevelManager.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
@ccclass('LevelManager')
@executionOrder(-1)
export class LevelManager extends Component implements IManagerBase {
    public static Name: string = "LevelManager";

    myMap: MapManager = null;
    myAI: AIManager = null;
    
    onLoad () {
        this.node.name = this.GetManagerName();

        GameManager.Instance.AddManager(this, false);
    }

    onDestroy() {
        GameManager.Instance.DelManager(this);
    }

    start () {
        this.myMap = GameManager.GetManager<MapManager>(MapManager.Name);
        this.myAI = GameManager.GetManager<AIManager>(AIManager.Name);

        let dragonList = User.Instance.DragonData.GetAllUserDragons();
        for (var i = 0; i < dragonList.length; i++) {
            this.SpawnDragon(Number(dragonList[i].Tag));
        }
    }

    SpawnDragon(tag: number): void {
        let dragon = this.myAI.GetUserDragon(tag);
        this.myMap.AddDragon(dragon);
    }



    GetManagerName(): string {
        return LevelManager.Name;
    }
    Update(deltaTime: number): void {
    }

    createDummySpawnDragon()
    {
        for (var i = 0; i < 25; i++) {
            let tag = i +1;
            if(tag < 10){
                if(tag < 7){
                    tag  = Number(`10${tag}001`);
                }
                else{
                    tag  = Number(`10${tag - 6}002`);
                }
            }
            let randomDragon = this.myAI.GetRandomDragon(tag);
        
            this.myMap.AddDragon(randomDragon);
        }
    }
}
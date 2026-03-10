
import { _decorator, director, screen, Size, view, game, Node, Component, ResolutionPolicy } from 'cc';
import { IManagerBase } from 'sb';
import { TableManager } from './Data/TableManager';
import { NetworkManager } from './NetworkManager';
import { QuestManager } from './QuestManager';
import { ResourceManager } from './ResourceManager';
import { SettingManager } from './Settings/SettingManager';
import { SoundMixer } from './SoundMixer';
import { TimeManager } from './Time/TimeManager';
import { DataManager } from './Tools/DataManager';
import { EventManager } from './Tools/EventManager';
import { TutorialManager } from './Tutorial/TutorialManager';

/**
 * Predefined variables
 * Name = GameManager
 * DateTime = Thu Dec 30 2021 18:50:33 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = GameManager.ts
 * FileBasenameNoExtension = GameManager
 * URL = db://assets/Scripts/GameManager.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
export class GameManager {
    protected static instance: GameManager = null;
    updateManagers: IManagerBase[] = null;
    managers: IManagerBase[] = null;
    isSceneLoaded: boolean = false;
    isNetworkLoaded: boolean = false;
    protected updateNode: Component = null;
    protected skipFrame: number = 0;
    public set SkipFrame(value: number) {
        this.skipFrame = value;
    }

    protected isLogin: boolean = false;
    public get IsLogin(): boolean {
        return this.isLogin;
    }
    public set IsLogin(value: boolean) {
        this.isLogin = value;
    }

    protected isSplash: boolean = false;
    public get IsSplash(): boolean {
        return this.isSplash;
    }
    public set IsSplash(value: boolean) {
        this.isSplash = value;
    }

    public resizeFunc: Function = null;

    public static get Instance() {
        if(GameManager.instance == null) {
            GameManager.instance = new GameManager();
    
            GameManager.instance.managers = [];
            GameManager.instance.updateManagers = [];
            GameManager.instance.isSceneLoaded = false;
            GameManager.instance.isNetworkLoaded = false;
            GameManager.instance.isSplash = false;
            let data = DataManager.Instance;
            data.Init();
            let tables = TableManager.Instance;
            tables.Init();
            let time = TimeManager.Instance;
            time.Init();
            console.log("Ststem Time", (new Date()).getTime()/1000)
            let net = NetworkManager.Instance;
            net.Init();
            let res = ResourceManager.Instance
            res.Init();
            let event = EventManager.Instance;
            event.Init();
            let sound = SoundMixer.Instance;
            let quest = QuestManager.Instance;
            quest.Init();

            let tuto = TutorialManager.GetInstance;
            tuto.Init();

            let setting = SettingManager.GetInstance;
            setting.Init();
            
            window.addEventListener('resize', GameManager.onWindowResize);
            GameManager.onWindowResize();

            NetworkManager.GetAllData();
            director.preloadScene("game", function () {
                GameManager.instance.isSceneLoaded = true;
            });
        }
        return this.instance;
    }

    AddManager(manager: IManagerBase, isUpdate?: boolean): void {
        if (manager == null) {
            return;
        }

        if(isUpdate) {
            this.updateManagers.push(manager);
        }
        this.managers.push(manager);
    }

    DelManager(manager: IManagerBase): void {
        if (manager == null) {
            return;
        }

        console.log(manager.GetManagerName(), " Manager 제거")
        this.managers = this.managers.filter(curManager => curManager != manager);
        this.updateManagers = this.updateManagers.filter(curManager => curManager != manager);
    }

    static GetManager<T extends IManagerBase>(name: string): T {
        let manager: T = null;
        this.instance.managers.forEach(element => {
            if (element.GetManagerName() != name) {
                return;
            }

            manager = element as T;
        });

        return manager;
    }

    Update (deltaTime: number) {
        if(this.skipFrame > 0) {
            this.skipFrame -= deltaTime;
            return;
        }
        this.updateManagers.forEach(element => {
            element.Update(deltaTime);
        });
    }

    static onWindowResize()
    {
        // var windowY = window.innerHeight;

        // let scope = windowY / 9;

        // var scopeX = scope * 16;
        // var scopeY = scope * 9;
        // view.setDesignResolutionSize(scopeX, 720, ResolutionPolicy.FIXED_HEIGHT);
        // game.frame.style.width = "100%";
        // game.frame.style.height = "100%";
        // game.frame.style.transform = "rotate(0deg)";
        GameManager.onResize();
    }

    static onResize() {
        // var windowX = window.innerWidth;
        var windowX = window.innerWidth;
        var windowY = window.innerHeight;

        screen.windowSize = new Size(windowX, windowY);
        // screen.requestFullScreen();

        // let scope = windowY / 9;

        // var scopeX = scope * 16;
        // var scopeY = scope * 9;
        view.setDesignResolutionSize(windowX, 720, ResolutionPolicy.FIXED_HEIGHT);

        if(GameManager.instance != null && GameManager.instance.resizeFunc != null) {
            GameManager.instance.resizeFunc();
        }
    }
}
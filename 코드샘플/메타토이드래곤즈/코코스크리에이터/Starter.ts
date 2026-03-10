
import { _decorator, Component, Node, game, EditBox, sys, Button, Sprite, Color, Toggle, Label, AudioClip, Animation } from 'cc';
import { StringTable } from './Data/StringTable';
import { GameManager } from './GameManager';
import { NetworkManager } from './NetworkManager';
import { ResourceManager } from './ResourceManager';
import { SceneManager } from './SceneManager';
import { SoundMixer, SOUND_TYPE } from './SoundMixer';
import { TutorialManager } from './Tutorial/TutorialManager';
import { PopupManager } from './UI/Common/PopupManager';
import { SystemPopup } from './UI/SystemPopup';
import { User } from './User/User';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = Starter
 * DateTime = Mon Jan 10 2022 15:45:53 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = Starter.ts
 * FileBasenameNoExtension = Starter
 * URL = db://assets/Scripts/Starter.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
@ccclass('Starter')
export class Starter extends Component {
    private static instance : Starter = null
    @property
    protected isLogin: boolean = false;
    @property(Node)
    protected clickNode: Node = null;
    @property(Node)
    protected login: Node = null;
    @property(EditBox)
    protected editBox: EditBox = null;
    @property(Button)
    protected loginBtn: Button = null;
    @property(Sprite)
    protected loginBtnImage: Sprite = null;
    @property(Toggle)
    protected toggleLogin : Toggle = null;
    @property(Node)
    protected popupParent: Node = null; 
    @property(Label)
    protected alertMainLabel: Label = null;
    @property(Label)
    protected alertSubLabel: Label = null;
    @property(AudioClip)
    protected introClip: AudioClip = null;
    @property(Animation)
    protected splash: Animation = null;
    @property(AudioClip)
    protected splashClip: AudioClip = null;
    gameManager: GameManager = null;
    protected isLoginClick: boolean = false;
    protected isInit: boolean = false;

    protected time: number = 0;

    start () 
    {
        if(Starter.instance == null)
        {
            Starter.instance = this;

            game.on("game_on_show", () => {
                if(User.Instance.UNO > 0) {
                    NetworkManager.Send('ping', {}, (jsonData) => {
                        //코인도저를 포함한 게임 전체의 시간 갱신을 위해
                        PopupManager.ForceUpdate()
                    });
                }
            });
        }
        
        this.gameManager = GameManager.Instance;
        this.init();

        if(GameManager.Instance.IsSplash) {
            if(this.splash != null) {
                this.splash.play("SplashEnd");
            }
            SoundMixer.DestroyBGMClip();
            SoundMixer.PlayBGM(SOUND_TYPE.BGM_INTRO, this.introClip);
        } else {
            if(this.splash != null) {
                if(this.splash != null) {
                    this.splash.on(Animation.EventType.FINISHED, () => {
                        this.splash.play("SplashEnd");
                        SoundMixer.DestroyBGMClip();
                        SoundMixer.PlayBGM(SOUND_TYPE.BGM_INTRO, this.introClip);
                    }, this);
                }
                SoundMixer.PlaySoundFX(SOUND_TYPE.FX, this.splashClip);
                GameManager.Instance.IsSplash = true;
            }
        }
    }

    static loginTry()
    {
        const UserID = sys.localStorage.getItem('UserID');
        console.log(`user_id : ${UserID}`, `isLogin : ${Starter.instance.isLogin}`);
        Starter.instance.isLogin = Starter.instance.toggleLogin.isChecked;
        Starter.instance.login.active = true;
        if(!Starter.instance.isLogin && UserID != undefined) {
            Starter.instance.login.active = false;
            NetworkManager.SideIn(UserID);
        }
    }

    static SigninSuccess()
    {
        Starter.instance.isLoginClick = true;
    }

    static signinFailed()
    {
        let popup = PopupManager.OpenPopup("SystemPopupOK") as SystemPopup;
        popup.setMessage(StringTable.GetString(100000248, "Alert"), StringTable.GetString(100000607, "No such account"));

        Starter.instance.isLoginClick = false;

        if(Starter.instance.clickNode != null) {
            Starter.instance.clickNode.active = false;
        }
    }

    static SignupSuccess()
    {
        Starter.instance.isLoginClick = true;
    }

    static signupFailed()
    {
        let popup = PopupManager.OpenPopup("SystemPopupOK") as SystemPopup;
        popup.setMessage(StringTable.GetString(100000248, "Alert"), StringTable.GetString(100000606, "Failed to create account"));
        Starter.instance.isLoginClick = false;

        if(Starter.instance.clickNode != null) {
            Starter.instance.clickNode.active = false;
        }
    }

    init()
    {
        const UserID = sys.localStorage.getItem('UserID');
        //this.isLogin = this.toggleLogin.isChecked;
        this.isLoginClick = false;
        if(UserID != undefined) {
            // this.login.active = false;
            // NetworkManager.SideIn(UserID);
            // this.isLoginClick = true;
            if(this.editBox != null){
                this.editBox.string = UserID;
            }
            this.ChangeTextID();
        }

        if(this.login != null) {
            this.login.active = false;
        }
    }

    update(dt: number) {
        this.time += dt * 2;
        const cur = Math.floor(this.time % 4);
        if(this.gameManager.isNetworkLoaded && ResourceManager.Instance.LoadComplete) {
            if (this.login != null) {
                this.login.active = !this.isLoginClick;
            }
            if (this.alertMainLabel != null) {
                this.alertMainLabel.node.active = false;
            }
        } else if(!ResourceManager.Instance.LoadComplete) {
            if (this.alertMainLabel != null) {
                this.alertMainLabel.string = ResourceManager.Instance.LoadString;
                switch(cur) {
                    case 0: this.alertSubLabel.string = '.'; break;
                    case 1: this.alertSubLabel.string = '..'; break;
                    case 2: this.alertSubLabel.string = '...'; break;
                    case 3: this.time -= 3; break;
                }
            }
        } else if(!this.gameManager.isNetworkLoaded) {
            if (this.alertMainLabel != null) {
                this.alertMainLabel.string = 'Connecting Network';
                switch(cur) {
                    case 0: this.alertSubLabel.string = '.'; break;
                    case 1: this.alertSubLabel.string = '..'; break;
                    case 2: this.alertSubLabel.string = '...'; break;
                    case 3: this.time -= 3; break;
                }
            }
        }

        if (this.gameManager.isSceneLoaded && this.gameManager.isNetworkLoaded && this.isLoginClick) {
            if(this.clickNode != null) {
                this.clickNode.active = true;
            }
        }
    }

    Login() {
        if(this.editBox == null || this.editBox.string == "") {
            return;
        }

        sys.localStorage.setItem('UserID', this.editBox.string);
        if (this.toggleLogin.isChecked) {
            NetworkManager.SideUp(this.editBox.string);
        } else {
            NetworkManager.SideIn(this.editBox.string);
        }
    }

    ChangeTextID() {
        if(this.editBox == null || this.editBox.string == "") 
        {
            if(this.loginBtnImage != null){
                this.loginBtnImage.color = Color.GRAY;
            }
            if(this.loginBtn != null){
                this.loginBtn.interactable = false;
            }
            return;
        }
        this.loginBtnImage.color = Color.WHITE;
        this.loginBtn.interactable = true;
    }

    GameStart() {
        PopupManager.AllClosePopup();
        SoundMixer.DestroyAllClip();
        GameManager.Instance.IsLogin = true;
        SceneManager.SceneChange("game", () => {
            // 튜토리얼 실행
            TutorialManager.GetInstance.OnTutorialEvent(101,1);
        });
    }
}

/**
 * [1] Class member could be defined like this.
 * [2] Use `property` decorator if your want the member to be serializable.
 * [3] Your initialization goes here.
 * [4] Your update function goes here.
 *
 * Learn more about scripting: https://docs.cocos.com/creator/3.3/manual/en/scripting/
 * Learn more about CCClass: https://docs.cocos.com/creator/3.3/manual/en/scripting/ccclass.html
 * Learn more about life-cycle callbacks: https://docs.cocos.com/creator/3.3/manual/en/scripting/life-cycle-callbacks.html
 */

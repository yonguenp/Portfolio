
import { _decorator, Component, instantiate, Sprite, game, director, Button, Color, Director, Animation, AnimationState, view, ResolutionPolicy } from 'cc';
import { GameManager } from './GameManager';
import { ResourceManager, ResourcesType } from './ResourceManager';
const { ccclass, property, executionOrder } = _decorator;

/**
 * Predefined variables
 * Name = SceneManager
 * DateTime = Tue Apr 12 2022 18:07:39 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = SceneManager.ts
 * FileBasenameNoExtension = SceneManager
 * URL = db://assets/Scripts/SceneManager.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */

enum SceneManagerState {
    None = 0,
    Start = 1,
    Change = 2,
    End = 3,
}

@ccclass('SceneManager')
@executionOrder(-5)
export class SceneManager extends Component {
    @property(Sprite)
    private back: Sprite = null;
    @property(Animation)
    private backAnimation: Animation = null;
    private static TargetName: string = "";
    GetSceneTargetName(){
        return SceneManager.TargetName;
    }

    private static isLoaded: boolean = false;
    private static CallBack: Director.OnSceneLaunched = null;
    private static state: SceneManagerState = SceneManagerState.None;
    private static AnimName: string = null;
    private static instance: SceneManager = null;
    public static get Instance(): SceneManager {
        return SceneManager.instance;
    }

    onLoad() {
        SceneManager.instance = this;
    }

    start() {
        SceneManager.instance.backAnimation.on(Animation.EventType.FINISHED, SceneManager.OnFinished, this);
        switch(SceneManager.state) {
            case SceneManagerState.None: {
                SceneManager.instance.backAnimation.play('sceneChange_1');
            } break;
            case SceneManagerState.Start: {
                SceneManager.instance.backAnimation.play('sceneChange_2');
            } break;
            case SceneManagerState.Change: {
                SceneManager.instance.backAnimation.play('sceneChange_3');
            } break;
            case SceneManagerState.End: {
                SceneManager.instance.backAnimation.play('sceneChange_4');
            } break;
        }
    }

    onDestroy() {
        SceneManager.instance = null;
    }

    static OnFinished(event, type: AnimationState) {
        switch(type.name) {
            case 'sceneChange_1': {

            } break;
            case 'sceneChange_2': {
                SceneManager.state = SceneManagerState.Change;
                SceneManager.AnimName = 'sceneChange_3';
                SceneManager.instance.backAnimation.play(SceneManager.AnimName);
            } break;
            case 'sceneChange_3': {

            } break;
            case 'sceneChange_4': {
                SceneManager.state = SceneManagerState.None;
                SceneManager.AnimName = 'sceneChange_1';
                SceneManager.instance.backAnimation.play(SceneManager.AnimName);
            } break;
        }
    }

    public static SceneChange(sceneName: string, func: Director.OnSceneLaunched = null): void {
        if(SceneManager.instance == null) {
            // SceneManager.Instance;
            director.loadScene(sceneName);
            return;
        }
        if(SceneManager.state != SceneManagerState.None) {
            return;
        }
        SceneManager.CallBack = func;
        SceneManager.TargetName = sceneName;
        SceneManager.state = SceneManagerState.Start;
        SceneManager.isLoaded = false;
        director.preloadScene(sceneName, function () {
            SceneManager.isLoaded = true;
        });
    }

    BtnClick() {
        console.log('scenemanager error');
    }

    update(deltaTime: number): void {
        if(this.back == null) {
            return;
        }

        switch(SceneManager.state) {
            case SceneManagerState.None: {
            } break;
            case SceneManagerState.Start: {
                if(this.backAnimation != null && SceneManager.AnimName != 'sceneChange_2') {
                    SceneManager.AnimName = 'sceneChange_2';
                    this.backAnimation.play(SceneManager.AnimName);
                }
            } break;
            case SceneManagerState.Change: {
                if(SceneManager.isLoaded) {
                    SceneManager.state = SceneManagerState.End;
                    GameManager.onWindowResize();
                    director.loadScene(SceneManager.TargetName, SceneManager.CallBack);
                }
            } break;
            case SceneManagerState.End: {
                if(this.backAnimation != null && SceneManager.AnimName != 'sceneChange_4') {
                    SceneManager.AnimName = 'sceneChange_4';
                    this.backAnimation.play(SceneManager.AnimName);
                }
            } break;
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

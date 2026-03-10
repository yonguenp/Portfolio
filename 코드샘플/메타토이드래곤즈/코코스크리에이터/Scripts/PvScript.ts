
import { _decorator, Component, screen, view, ResolutionPolicy, Size, input, Input, Animation, EventKeyboard, KeyCode } from 'cc';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = PvScript
 * DateTime = Fri Apr 22 2022 12:15:07 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = PvScript.ts
 * FileBasenameNoExtension = PvScript
 * URL = db://assets/Scripts/PvScript.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('PvScript')
export class PvScript extends Component {
    static dummy: Animation = null;

    start () {
        screen.windowSize = new Size(1918, 1078);
        view.setDesignResolutionSize(1280, 720, ResolutionPolicy.FIXED_HEIGHT);
        PvScript.dummy = this.getComponent(Animation);
        this.PVEMOVIE();
    }

    onDestroy() {
        this.PVEMOVIE_END();
    }
    PVEMOVIE() {
        input.on(Input.EventType.KEY_UP, this.KEYEVENT);
    }

    PVEMOVIE_END() {
        input.off(Input.EventType.KEY_UP, this.KEYEVENT);
    }

    // static anim: number = 0;
    // static start: boolean = false;
    // static playing: boolean = false;
    // static startValue: {} = null;
    // static endValue: {} = null;
    // static curTime: number = 0;
    // static maxTime: number = 0;
    // static animTime1: number = 2;
    // static animTime2: number = 7;
    KEYEVENT(event: EventKeyboard) {
        console.log(event);

        switch(event.keyCode) {
            case KeyCode.KEY_Q: {
                PvScript.dummy.pause();
            } break;
            case KeyCode.KEY_W: {
                PvScript.dummy.resume();
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

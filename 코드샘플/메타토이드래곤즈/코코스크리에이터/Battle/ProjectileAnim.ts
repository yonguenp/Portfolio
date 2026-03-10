
import { _decorator, Component, Node, Animation, AnimationState, AnimationClip } from 'cc';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = ProjectileAnim
 * DateTime = Wed Apr 13 2022 19:18:53 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = ProjectileAnim.ts
 * FileBasenameNoExtension = ProjectileAnim
 * URL = db://assets/Scripts/Battle/ProjectileAnim.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('ProjectileAnim')
export class ProjectileAnim extends Component {
    private animation: Animation = null;
    private animTarget: number = 0;
    private isCurEnd: boolean = false;
    public get IsCurEND() {
        if(this.isLoop) {
            return true;
        }
        return this.isCurEnd;
    }
    private isLoop: boolean = false;
    public get IsAnim() {
        if(!this.isLoop) {
            return this.isCurEnd;
        }
        return false;
    }
    private isEnd: boolean = false;
    public get IsEND() {
        return this.isEnd;
    }
    onLoad() {
        this.animation = this.getComponent(Animation);
    }

    start () {
        this.animation.on(Animation.EventType.PLAY, this.OnStarted, this);
        this.animation.on(Animation.EventType.FINISHED, this.OnFinished, this);
    }

    OnStarted(event, type: AnimationState) {
        switch(type.wrapMode) {
            case AnimationClip.WrapMode.Loop: case AnimationClip.WrapMode.LoopReverse:
            case AnimationClip.WrapMode.PingPong: case AnimationClip.WrapMode.PingPongReverse: {
                this.isLoop = true;
            } break;
            default: {
                this.isLoop = false;
            } break;
        }
    }

    OnFinished(event, type: AnimationState) {
        this.isCurEnd = true;
    }

    NextAnim() {
        this.animTarget++;
        if(this.animation.clips.length <= this.animTarget) {
            this.isEnd = true;
            this.isCurEnd = true;
            return;
        }

        this.animation.play(this.animation.clips[this.animTarget].name);
        this.isCurEnd = false;
    }

    Death(): void {
        this.isEnd = true;
        this.isCurEnd = true;
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

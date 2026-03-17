
import { _decorator, Component, Animation, AnimationState } from 'cc';
const { ccclass } = _decorator;

/**
 * Predefined variables
 * Name = EffectAnim
 * DateTime = Mon Feb 28 2022 14:30:37 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = EffectAnim.ts
 * FileBasenameNoExtension = EffectAnim
 * URL = db://assets/Scripts/Battle/EffectAnim.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('EffectAnim')
export class EffectAnim extends Component {
    private animation: Animation = null;
    onLoad() {
        this.animation = this.getComponent(Animation);
    }

    start () {
        this.animation.on(Animation.EventType.FINISHED, this.OnFinished, this);
    }

    OnFinished(event, type: AnimationState) {
        this.node.destroy();
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

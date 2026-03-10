
import { _decorator, Component, Node, Animation, Vec3, Color, Sprite } from 'cc';
import { ICharacter } from 'sb';
import { SBController } from '../Tools/SBController';
const { ccclass } = _decorator;

/**
 * Predefined variables
 * Name = Dragon
 * DateTime = Tue Dec 28 2021 16:34:10 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = Dragon.tsW
 * FileBasenameNoExtension = Dragon
 * URL = db://assets/Scripts/Character/Dragon.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */

export const DragonDefaultSpeed: number = 75;
@ccclass('Character')
export abstract class Character extends Component implements ICharacter {
    protected controller: SBController = null;
    public get Controller() :SBController {
        return this.controller;
    }
    protected animation: Animation = null;
    public get Animation(): Animation {
        return this.animation;
    }
    protected speed: number = 1;
    onLoad () {
        if(this.controller == null) {
            this.controller = this.getComponent(SBController);
        }
        if(this.animation == null) {
            this.animation = this.getComponent(Animation);
        }
    }

    start () {
    }

    Update(deltaTime: number) {

    }

    Init() {
        if(this.controller == null) {
            this.controller = this.getComponent(SBController);
        }
        if(this.animation == null) {
            this.animation = this.getComponent(Animation);
        }

        this.SetSpeed(DragonDefaultSpeed);
    }

    AnimationStart(name: string) {
        let anim = this.animation.getState(name);
        if (anim == null) {
            return;
        }

        if (!anim.isPlaying) {
            this.animation.play(name);
        }
    }

    SetSpeed(newSpeed: number) {
        if (this.speed == newSpeed) {
            return;
        }

        if(newSpeed < 50) {
            console.log(`speed : ${newSpeed}`);
        }

        this.speed = newSpeed;
        var animSpeed = newSpeed / DragonDefaultSpeed;
        if (animSpeed < 0.2) {
            animSpeed = 0.2;
        } else if (animSpeed > 5) {
            animSpeed = 5;
        }
        this.Controller.speed = newSpeed;
        
        if(this.animation != null) {
            let state = this.animation.getState("walk");
            if(state != null) {
                state.speed = animSpeed;
            }
        }
        // this.animation.clips = clips;
    }

    GetPosition(): Vec3 {
        return this.node.getPosition();
    }

    GetWorldPosition(): Vec3 {
        return this.node.getWorldPosition();
    }

    protected scale: number = 1;
    SetScale(value: number, min: number, max: number): void {
        if(value > max) {
            value = max;
        }
        if(value < min) {
            value = min;
        }
        this.scale = value;
        var scale = this.node.getScale();
        scale.x = this.scale;
        scale.y = this.scale;
        scale.z = this.scale;
        this.node.setScale(scale);
    }

    GetScale(): number {
        return this.scale;
    }

    ChangeColor(color: Color) {
        this.ChildrenChangeColor(this.node, color);
    }

    protected ChildrenChangeColor(target: Node, color: Color) {
        if(target == null) {
            return;
        }

        let sprite = target.getComponent(Sprite);
        if(sprite != null) {
            sprite.color = color;
        }

        if(target.children.length > 0) {
            target.children.forEach(element => {
                this.ChildrenChangeColor(element, color);
            });
        }
    }

    GetAnimationLength(AnimName : string)
    {
        let duration = 0;
        if(this.animation == null || this.animation.clips.length <= 0){
            return duration;
        }
        
        let animationClips = this.animation.clips;
        animationClips.forEach((clip)=>{
            let clipName = clip.name;
            if(clipName == AnimName)
            {
                duration = clip.duration;
            }
        })
        return duration;
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

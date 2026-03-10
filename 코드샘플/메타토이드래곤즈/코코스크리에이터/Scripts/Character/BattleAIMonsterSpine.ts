
import { _decorator, sp, Color, Node, UITransform, Vec4 } from 'cc';
import { BattleAIMonster } from './BattleAIMonster';
import { DragonDefaultSpeed } from './Dragon';
import { ShadowEvent } from './ShadowEvent';
const { ccclass } = _decorator;

/**
 * Predefined variables
 * Name = BattleAIMonsterSpine
 * DateTime = Wed Mar 02 2022 13:59:35 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = BattleAIMonsterSpine.ts
 * FileBasenameNoExtension = BattleAIMonsterSpine
 * URL = db://assets/Scripts/Character/BattleAIMonsterSpine.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('BattleAIMonsterSpine')
export class BattleAIMonsterSpine extends BattleAIMonster {
    shadow: Node = null;
    spine: sp.Skeleton = null;
    onLoad () {
        if(this.spine == null) {
            let spineNode = this.node.getChildByName('monster');
            if(spineNode != null) {
                this.spine = spineNode.getComponent(sp.Skeleton);
            }
        }

        this.shadow = this.node.getChildByName('deco_shadow');
    }

    start () {
        super.start();
        this.spine.setCompleteListener((trackEntry) => {
            switch(trackEntry.animation.name) {
                case 'monster_attack': {
                    this.AnimationStart('idle');
                } break;
                case 'monster_casting': {
                    this.AnimationStart('skill');
                } break;
                case 'monster_skill': {
                    this.AnimationStart('idle');
                } break;
                case 'monster_hit': {
                    this.AnimationStart('idle');
                } break;
                case 'monster_death': {
                    ShadowEvent.TriggerEvent({state:"Del", target: this.node});
                    this.node.active = false;
                } break;
            }
        });
        this.spine.setMix('monster_idle', 'monster_walk',0.5);
        this.spine.setMix('monster_walk','monster_idle',0.5);

        if(this.shadow.active && this.shadow != null) {
            let shadowSize = this.shadow.getComponent(UITransform);
            if(shadowSize != null) {
                this.shadow.active = false;

                ShadowEvent.TriggerEvent({state:"Add", target: this.node, info:new Vec4(this.shadow.position.x, this.shadow.position.y, shadowSize.width, shadowSize.height), scale: this.node.scale})
            }
        }
    }

    Init() {
        super.Init();
        if(this.spine == null) {
            this.spine = this.getComponent(sp.Skeleton);
        }
    }

    Update(deltaTime: number) {
        super.Update(deltaTime);
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
        if (animSpeed < 0.5) {
            animSpeed = 0.5;
        } else if (animSpeed > 2.5) {
            animSpeed = 2.5;
        }
        this.Controller.speed = newSpeed;
        
        if(this.spine != null) {
            this.spine.timeScale = animSpeed;
        }
        // this.animation.clips = clips;
    }

    GetSpeed(): number {
        if(this.spine == null) {
            return 1;
        }
        return this.spine.timeScale;
    }

    AnimationStart(name: string) {
        let loop = false;

        switch(this.spine.animation) {
            case 'monster_casting': 
            case 'monster_attack': 
            case 'monster_skill': 
            {
                if(name == 'hit') {
                    return;
                }
            } break;
        }

        switch(name) {
            case 'attack': {
                name = 'monster_attack';
            } break;
            case 'skill': {
                name = 'monster_skill1';
            } break;
            case 'walk': {
                name = 'monster_walk';
                loop = true;
            } break;
            case 'idle': {
                name = 'monster_idle';
            } break;
            case 'death': {
                name = 'monster_death';
            } break;
            case 'casting': {
                name = 'monster_casting';
            } break;
        }

        if(this.spine.animation == name) {
            return;
        }

        this.spine.setAnimation(0, name, loop);
    }

    Hit() {
        this.unschedule(this.HitCallBack);
        this.spine.useTint = true;
        this.scheduleOnce(this.HitCallBack, 0.25);
    }

    HitCallBack() {
        this.spine.useTint = false;
    }

    Death() {
        this.data.Death = true;
    }

    ChangeColor(color: Color) {
        if(this.spine != null) {
            this.spine.color = color
        }
    }
    
    public ActiveFlag(value: boolean) {
        if(!value) {
            this.spine.paused = true;
        } else {
            this.spine.paused = false;
        }
    }

    GetAnimationLength(AnimName : string)
    {
        let duration = 0;
        if(this.spine == null){
            return duration;
        }
        
        switch(AnimName) {
            case 'attack': {
                AnimName = 'monster_attack';
            } break;
            case 'skill': {
                AnimName = 'monster_skill1';
            } break;
            case 'walk': {
                AnimName = 'monster_walk';
            } break;
            case 'idle': {
                AnimName = 'monster_idle';
            } break;
            case 'death': {
                AnimName = 'monster_death';
            } break;
            case 'casting': {
                AnimName = 'monster_casting';
            } break;
        }

        let anim = this.spine.findAnimation(AnimName);
        if(anim == null){
            return duration;
        }
        return anim.duration;
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

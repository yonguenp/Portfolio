
import { _decorator, sp, Color, Vec3, Node, UITransform, Vec4 } from 'cc';
import { BezierCurve2Speed, BezierCurve2Vec3 } from '../Tools/SandboxTools';
import { BattleDragon } from './BattleDragon';
import { DragonDefaultSpeed } from './Dragon';
import { ShadowEvent } from './ShadowEvent';
const { ccclass } = _decorator;

/**
 * Predefined variables
 * Name = BattleDragonSpine
 * DateTime = Thu Mar 03 2022 15:56:21 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = BattleDragonSpine.ts
 * FileBasenameNoExtension = BattleDragonSpine
 * URL = db://assets/Scripts/Character/BattleDragonSpine.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('BattleDragonSpine')
export class BattleDragonSpine extends BattleDragon {
    shadow: Node = null;
    private spine: sp.Skeleton = null;
    private lastPos: Vec3 = null;
    private deathCurTime: number = 0;
    private deathTime: number = 0.9;
    onLoad () {
        if(this.spine == null) {
            let spineNode = this.node.getChildByName('dragon');
            if(spineNode != null) {
                this.spine = spineNode.getComponent(sp.Skeleton);
            }
        }

        this.shadow = this.node.getChildByName('deco_shadow');
    }

    start () {
        super.start();
        this.spine.setStartListener((trackEntry) => {
            switch(trackEntry.animation.name) {
                case 'dragon_death': {
                    ShadowEvent.TriggerEvent({state:"Del", target: this.node});
                } break;
            }
        });
        this.spine.setCompleteListener((trackEntry) => {
            switch(trackEntry.animation.name) {
                case 'dragon_attack': {
                    this.AnimationStart('idle');
                } break;
                case 'dragon_casting': {
                    this.AnimationStart('skill');
                } break;
                case 'dragon_skill': {
                    this.AnimationStart('idle');
                } break;
                case 'dragon_hit': {
                    this.AnimationStart('idle');
                } break;
                case 'dragon_death': {
                    this.node.active = false;
                } break;
            }
        });

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
    }

    GetSpeed(): number {
        if(this.spine == null) {
            return super.GetSpeed();
        }
        return this.spine.timeScale;
    }

    AnimationStart(name: string) {
        let loop = false;

        switch(this.spine.animation) {
            case 'dragon_casting': 
            case 'dragon_attack': 
            case 'dragon_skill': 
            {
                if(name == 'hit') {
                    return;
                }
            } break;
        }
        
        switch(name) {
            case 'attack': {
                name = 'dragon_attack';
            } break;
            case 'skill': {
                name = 'dragon_skill';
            } break;
            case 'walk': {
                name = 'dragon_walk';
                loop = true;
            } break;
            case 'idle': {
                name = 'dragon_idle';
                loop = true;
            } break;
            case 'win': {
                name = 'dragon_win';
                loop = true;
            } break;
            case 'lose': {
                name = 'dragon_lose';
                loop = true;
            } break;
            case 'casting': {
                name = 'dragon_casting';
            } break;
            case 'death': {
                name = 'dragon_death';
            } break;
            case 'hit': {
                name = 'dragon_hit';
            } break;
        }

        if(this.spine.animation == name) {
            return;
        }

        this.spine.setAnimation(0, name, loop);
    }

    Hit() {
        this.AnimationStart('hit');
        // this.unschedule(this.HitCallBack);
        // this.ChangeColor(Color.RED);
        // this.scheduleOnce(this.HitCallBack, 0.5);
    }

    HitCallBack() {
        // this.ChangeColor(Color.WHITE);
    }

    Death() {
        if(!this.data.Death) {
            this.lastPos = new Vec3(this.node.position);
            this.data.Death = true;
        }
    }

    ChangeColor(color: Color) {
        if(this.spine != null) {
            this.spine.color = color
        }
    }

    public StunEffect() {
        if(this.data == null) {
            return;
        }
        if(this.data.Death) {
            this.Data.SetEffectActive('STUN', false);
            this.Data.SetEffectTime('STUN', 0);
        }
        const lastTime = this.data.GetEffectTime('STUN');
        var isStun = this.Data.GetEffectActive('STUN');
        if(lastTime > 0 && !isStun) {
            this.Data.SetEffectActive('STUN', true);
            this.spine.paused = true;
        } else if(lastTime <= 0 && isStun) {
            this.Data.SetEffectActive('STUN', false);
            this.spine.paused = false;
        }
    }

    public AirborneEffect() {
        if(this.data == null) {
            return;
        }
        if(this.data.Death) {
            this.Data.SetEffectActive('AIRBORNE', false);
            this.Data.SetEffectTime('AIRBORNE', 0);
        }

        var isAirborne = this.Data.GetEffectActive('AIRBORNE');
        const lastTime = this.data.GetEffectTime('AIRBORNE');
        if(lastTime > 0 && !isAirborne) {
            this.Data.SetEffectActive('AIRBORNE', true);
            this.controller.onExitEvent('left');
            this.controller.onExitEvent('right');
            this.spine.paused = true;
        } else if(lastTime <= 0 && this.spine.paused) {
            this.spine.paused = false;
            this.Data.SetEffectActive('AIRBORNE', false);
            this.node.setPosition(new Vec3(this.node.position.x, this.data.StartPos.y, this.node.position.z));
        } else if(lastTime > 0) {
            this.spine.paused = true;
            const maxTime = this.data.GetEffectMaxTime('AIRBORNE');
            const bezierCurve = BezierCurve2Speed(this.data.StartPos.y, this.data.StartPos.y + this.data.GetEffectParam('AIRBORNE'), this.data.StartPos.y, maxTime - lastTime, maxTime);
            this.node.setPosition(new Vec3(this.node.position.x, bezierCurve, this.node.position.z));
        }
    }

    update(dt: number) {
        if(this.data != null && this.data.Death && this.deathCurTime < this.deathTime) {
            this.deathCurTime += dt;
            let vec1 = new Vec3(this.lastPos.x - 30, this.lastPos.y + 150, this.lastPos.z);
            let vec2 = new Vec3(this.lastPos.x - 150, this.lastPos.y + 120, this.lastPos.z);
    
            let pos = BezierCurve2Vec3(this.lastPos, vec1, vec2, this.deathCurTime, this.deathTime);
            this.node.position = pos;
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
                AnimName = 'dragon_attack';
            } break;
            case 'skill': {
                AnimName = 'dragon_skill';
            } break;
            case 'walk': {
                AnimName = 'dragon_walk';
            } break;
            case 'idle': {
                AnimName = 'dragon_idle';
            } break;
            case 'win': {
                AnimName = 'dragon_win';
            } break;
            case 'lose': {
                AnimName = 'dragon_lose';
            } break;
            case 'casting': {
                AnimName = 'dragon_casting';
            } break;
            case 'death': {
                AnimName = 'dragon_death';
            } break;
            case 'hit': {
                AnimName = 'dragon_hit';
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

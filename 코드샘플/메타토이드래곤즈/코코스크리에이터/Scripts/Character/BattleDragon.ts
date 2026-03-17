
import { _decorator, Color, AnimationState, __private, Animation, ProgressBar, instantiate, Vec3 } from 'cc';
import { BattleSimulatorResourceManager } from '../Battle/Battle_simulator/BattleSimulatorResourceManager';
import { DamageManager, eDamageType } from '../DamageManager';
import { ResourceManager, ResourcesType } from '../ResourceManager';
import { BezierCurve2Speed } from '../Tools/SandboxTools';
import { BattleDragonData } from './BattleData';
import { Character } from './Dragon';
const { ccclass } = _decorator;
/**
 * Predefined variables
 * Name = BattleDragon
 * DateTime = Mon Feb 21 2022 19:30:47 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = BattleDragon.ts
 * FileBasenameNoExtension = BattleDragon
 * URL = db://assets/Scripts/Character/BattleDragon.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('BattleDragon')
export class BattleDragon extends Character {
    protected data: BattleDragonData = null;
    public get Data(): BattleDragonData {
        return this.data;
    }
    protected hp_bar: ProgressBar = null;
    Set(battleData: BattleDragonData) {
        this.data = battleData;

        let hp_bar = ResourceManager.Instance.GetResource(ResourcesType.UI_PREFAB)['character_hp_green'];
        let hp_bar_node = instantiate(hp_bar);
        this.node.addChild(hp_bar_node);
        let bar_node = hp_bar_node.getChildByName('bar');
        if(bar_node != null) {
            this.hp_bar = bar_node.getComponent(ProgressBar);
        }
        let currentScale = this.node.scale;
        hp_bar_node.setScale(new Vec3(1/currentScale.x * (-1) , 1/ currentScale.y, 1/ currentScale.z));
    }

    SetSimulatorDragon(battleData: BattleDragonData) {
        this.data = battleData;

        let hp_bar = BattleSimulatorResourceManager.Instance.GetResourceByName('character_hp_green');
        let hp_bar_node = instantiate(hp_bar);
        this.node.addChild(hp_bar_node);
        let bar_node = hp_bar_node.getChildByName('bar');
        if(bar_node != null) {
            this.hp_bar = bar_node.getComponent(ProgressBar);
        }
        let currentScale = this.node.scale;
        hp_bar_node.setScale(new Vec3(1/currentScale.x , 1/ currentScale.y, 1/ currentScale.z));
    }

    Update(dt: number): void {
        this.data?.Update(dt);
        this.RefreshHP();
        this.StunEffect();
        this.AirborneEffect();
    }

    Hit() {
        this.unschedule(this.HitCallBack);
        this.ChangeColor(Color.RED);
        this.scheduleOnce(this.HitCallBack, 0.5);
    }

    HitCallBack() {
        this.ChangeColor(Color.WHITE);
    }

    start () {
        super.start();
        if(this.animation != null) {
            this.animation.on(Animation.EventType.FINISHED, this.OnFinished, this);
        }
    }

    Death() {
        this.data.Death = true;
        this.ChangeColor(Color.BLACK);
    }

    OnFinished(event, type: AnimationState) {
        if(type.name == 'dragon_death') {
            this.node.active = false;
        }
        if(type.name == 'dragon_attack') {
            this.AnimationStart('dragon_idle');
        }
    }

    RefreshHP() {
        if(this.hp_bar == null || this.data == null) {
            return;
        }

        const curHP = this.data.HP;
        const maxHP = this.data.MAXHP;

        this.hp_bar.progress = curHP / maxHP;
    }

    AnimationStart(name: string) {
        switch(name) {
            case 'skill': {
                name = 'attack';
            } break;
        }
        super.AnimationStart(name);
    }

    public IsDeath(): boolean {
        return this.data.Death;
    }
    
    public ActiveFlag(value: boolean) {
        if(!value) {
            this.animation.pause();
        } else {
            this.animation.resume();
        }
    }

    public StunEffect() {
        if(this.data == null) {
            return;
        }
        if(this.data.Death) {
            this.ActiveFlag(true);
            this.Data.SetEffectActive('STUN', false);
            this.Data.SetEffectTime('STUN', 0);
        }
        const lastTime = this.data.GetEffectTime('STUN');
        var isStun = this.Data.GetEffectActive('STUN');
        if(lastTime > 0 && !isStun) {
            this.Data.SetEffectActive('STUN', true);
            this.ActiveFlag(false);
        } else if(lastTime <= 0 && isStun) {
            this.Data.SetEffectActive('STUN', false);
            this.ActiveFlag(true);
        }
    }

    public AirborneEffect() {
        if(this.data == null) {
            return;
        }

        if(this.data.Death) {
            this.ActiveFlag(true);
            this.Data.SetEffectActive('AIRBORNE', false);
            this.Data.SetEffectTime('AIRBORNE', 0);
        }

        var isAirborne = this.Data.GetEffectActive('AIRBORNE');
        const lastTime = this.data.GetEffectTime('AIRBORNE');
        if(lastTime > 0 && !isAirborne) {
            this.ActiveFlag(false);
            this.Data.SetEffectActive('AIRBORNE', true);
            this.controller.onExitEvent('left');
            this.controller.onExitEvent('right');
        } else if(lastTime <= 0 && isAirborne) {
            this.ActiveFlag(true);
            this.Data.SetEffectActive('AIRBORNE', false);
            this.node.setPosition(new Vec3(this.node.position.x, this.data.StartPos.y, this.node.position.z));
        } else if(lastTime > 0) {
            const maxTime = this.data.GetEffectMaxTime('AIRBORNE');
            const bezierCurve = BezierCurve2Speed(this.data.StartPos.y, this.data.StartPos.y + this.data.GetEffectParam('AIRBORNE'), this.data.StartPos.y, maxTime - lastTime, maxTime);
            this.node.setPosition(new Vec3(this.node.position.x, bezierCurve, this.node.position.z));
        }
    }

    public GetPosition(isSize: boolean = false): Vec3 {
        return super.GetPosition();
    }

    public GetWorldPosition(isSize: boolean = false): Vec3 {
        return super.GetWorldPosition();
    }

    GetSpeed(): number {
        return 1;
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

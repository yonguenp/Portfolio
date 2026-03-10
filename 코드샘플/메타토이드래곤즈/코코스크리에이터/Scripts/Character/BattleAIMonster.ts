
import { _decorator, Animation, Vec3, Color, AnimationState, instantiate, ProgressBar } from 'cc';
// import { BattleUI } from '../Battle/BattleUI';
// import { BattleSimulatorResourceManager } from '../Battle/Battle_simulator/BattleSimulatorResourceManager';
import { ResourceManager, ResourcesType } from '../ResourceManager';
import { BezierCurve2Speed } from '../Tools/SandboxTools';
import { BattleAIMonsterData, BattleTileX } from './BattleData';
import { Character } from './Dragon';
const { ccclass, property } = _decorator;
/**
 * Predefined variables
 * Name = BattleAIMonster
 * DateTime = Mon Feb 21 2022 19:30:40 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = BattleAIMonster.ts
 * FileBasenameNoExtension = BattleAIMonster
 * URL = db://assets/Scripts/Character/BattleAIMonster.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 
@ccclass('BattleAIMonster')
export class BattleAIMonster extends Character {
    @property
    protected Name: string = "";
    protected data: BattleAIMonsterData = null;
    public get Data(): BattleAIMonsterData {
        return this.data;
    }

    protected hp_bar: ProgressBar = null;
    public set Hp_bar(value : ProgressBar) {this.hp_bar = value;}

    Set(battleData: BattleAIMonsterData) {
        this.data = battleData;
    }

    SetSimulatorMonster(battleData: BattleAIMonsterData) {
        this.data = battleData;
    }

    onLoad () {
        
    }

    start () {
        super.start();
        if(this.animation != null) {
            this.animation.on(Animation.EventType.FINISHED, this.OnFinished, this);
        }

        let hp_bar = ResourceManager.Instance.GetResource(ResourcesType.UI_PREFAB)['character_hp_red'];
        let hp_bar_node = instantiate(hp_bar);
        this.node.addChild(hp_bar_node);
        let bar_node = hp_bar_node.getChildByName('bar');
        if(bar_node != null) {
            this.hp_bar = bar_node.getComponent(ProgressBar);
        }

        let currentScale = this.data.Node.scale;
        hp_bar_node.setScale(new Vec3(1/currentScale , 1/ currentScale, 1/ currentScale));
    }

    Update(deltaTime: number) {
        this.data?.Update(deltaTime);
        this.RefreshHP();
        this.StunEffect();
        this.AirborneEffect();
    }

    AnimationStart(name: string) {
        const animationName = `${this.Name}_${name}`;
        super.AnimationStart(animationName);
    }

    Hit() {
        this.unschedule(this.HitCallBack);
        this.ChangeColor(Color.RED);
        this.scheduleOnce(this.HitCallBack, 0.5);
    }

    HitCallBack() {
        this.ChangeColor(Color.WHITE);
    }

    Death() {
        this.data.Death = true;
    }

    OnFinished(event, type: AnimationState) {
        if(type.name == `${this.Name}_death`) {
            this.node.active = false;
        }
        if(type.name == `${this.Name}_attack`) {
            this.AnimationStart('idle');
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
        let local = super.GetPosition();
        if(isSize && this.Data != null && this.Data.BaseData == null && this.Data.BaseData.SIZE > 1) {
            local.x -= (this.Data.BaseData.SIZE - 1) * BattleTileX;
        }
        return local;
    }

    public GetWorldPosition(isSize: boolean = false): Vec3 {
        let world = super.GetWorldPosition();
        if(isSize && this.Data != null && this.Data.BaseData == null && this.Data.BaseData.SIZE > 1) {
            world.x -= (this.Data.BaseData.SIZE - 1) * BattleTileX;
        }
        return world;
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

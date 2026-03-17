import { DataBase } from './DataBase';

/**
 * Predefined variables
 * Name = SkillData
 * DateTime = Mon Feb 21 2022 15:54:55 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = SkillData.ts
 * FileBasenameNoExtension = SkillData
 * URL = db://assets/Scripts/Data/SkillData.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
export class SkillCharData extends DataBase {
    protected key: number = -1;
    public get KEY(): number {
        return this.key;
    }
    public set KEY(value: number) {
        this.key = value;
    }
    protected skill_id: number = -1;
    public get SKILL_ID(): number {
        return this.skill_id;
    }
    public set SKILL_ID(value: number) {
        this.skill_id = value;
    }
    protected icon: string = "";
    public get ICON(): string {
        return this.icon;
    }
    public set ICON(value: string) {
        this.icon = value;
    }
    protected level: number = -1;
    public get LEVEL(): number {
        return this.level;
    }
    public set LEVEL(value: number) {
        this.level = value;
    }
    protected _name: number = -1;
    public get _NAME(): number {
        return this._name;
    }
    public set _NAME(value: number) {
        this._name = value;
    }
    protected _desc1: number = -1;
    public get _DESC1(): number {
        return this._desc1;
    }
    public set _DESC1(value: number) {
        this._desc1 = value;
    }
    protected start_cool_time: number = -1;
    public get START_COOL_TIME(): number {
        return this.start_cool_time;
    }
    public set START_COOL_TIME(value: number) {
        this.start_cool_time = value;
    }
    protected cool_time: number = -1;
    public get COOL_TIME(): number {
        return this.cool_time;
    }
    public set COOL_TIME(value: number) {
        this.cool_time = value;
    }
    protected casting_type: string = "";
    public get CASTING_TYPE(): string {
        return this.casting_type;
    }
    public set CASTING_TYPE(value: string) {
        this.casting_type = value;
    }
    protected range_x: number = -1;
    public get RANGE_X(): number {
        return this.range_x;
    }
    public set RANGE_X(value: number) {
        this.range_x = value;
    }
    protected range_y: number = -1;
    public get RANGE_Y(): number {
        return this.range_y;
    }
    public set RANGE_Y(value: number) {
        this.range_y = value;
    }
    protected projectile_key: number = -1;
    public get PROJECTILE_KEY(): number {
        return this.projectile_key;
    }
    public set PROJECTILE_KEY(value: number) {
        this.projectile_key = value;
    }
    protected effect_group: number = -1;
    public get EFFECT_GROUP(): number {
        return this.effect_group;
    }
    public set EFFECT_GROUP(value: number) {
        this.effect_group = value;
    }
    protected trigger_delay: number = -1;
    public get TRIGGER_DELAY(): number {
        return this.trigger_delay;
    }
    public set TRIGGER_DELAY(value: number) {
        this.trigger_delay = value;
    }

    protected mid_delay: number = -1;
    public get Mid_delay(): number {
        return this.mid_delay;
    }
    public set Mid_delay(value: number) {
        this.mid_delay = value;
    }

    protected after_delay: number = -1;
    public get AFTER_DELAY(): number {
        return this.after_delay;
    }
    public set AFTER_DELAY(value: number) {
        this.after_delay = value;
    }
    protected inf: number = -1;
    public get INF(): number {
        return this.inf;
    }
    public set INF(value: number) {
        this.inf = value;
    }
    protected item: number = -1;
    public get ITEM(): number {
        return this.item;
    }
    public set ITEM(value: number) {
        this.item = value;
    }
    protected item_value: number = -1;
    public get ITEM_VALUE(): number {
        return this.item_value;
    }
    public set ITEM_VALUE(value: number) {
        this.item_value = value;
    }
    protected cost_type: string = "";
    public get COST_TYPE(): string {
        return this.cost_type;
    }
    public set COST_TYPE(value: string) {
        this.cost_type = value;
    }
    protected cost_value: number = -1;
    public get COST_VALUE(): number {
        return this.cost_value;
    }
    public set COST_VALUE(value: number) {
        this.cost_value = value;
    }

    protected next_level_skill: number = -1;
    public get NEXT_LEVEL_SKILL(): number {
        return this.next_level_skill;
    }
    public set NEXT_LEVEL_SKILL(value: number) {
        this.next_level_skill = value;
    }

    protected ani_prefab: string = "";
    public get ANI_PREFAB(): string {
        return this.ani_prefab;
    }
    public set ANI_PREFAB(value: string) {
        this.ani_prefab = value;
    }
}

export enum eSkillEffectDirectionType {
    All = 0,
    Front = 1,
    Back = 2
}

export enum eSkillEffectTarget {
    All = 0,
    Team = 1,
    TeamExceptMe = 2,
    Enemy = 3
}

export enum eSkillEffectStartType {
    None = 0,
    Caster = 1,
    Enemy = 2
}

export class SkillEffectData extends DataBase {
    protected key: number = -1;
    public get KEY(): number {
        return this.key;
    }
    public set KEY(value: number) {
        this.key = value;
    }
    protected group: number = -1;
    public get GROUP(): number {
        return this.group;
    }
    public set GROUP(value: number) {
        this.group = value;
    }
    protected skill: string = "";
    public get SKILL(): string {
        return this.skill;
    }
    public set SKILL(value: string) {
        this.skill = value;
    }
    protected start_position: eSkillEffectStartType = eSkillEffectStartType.Caster;
    public get START_POSITION(): eSkillEffectStartType {
        return this.start_position;
    }
    public set START_POSITION(value: eSkillEffectStartType) {
        if(value > eSkillEffectStartType.Enemy || value < eSkillEffectStartType.Caster) {
            value = eSkillEffectStartType.Caster;
        }
        this.start_position = value;
    }
    protected direction: eSkillEffectDirectionType = eSkillEffectDirectionType.Front;
    public get DIRECTION(): eSkillEffectDirectionType {
        return this.direction;
    }
    public set DIRECTION(value: eSkillEffectDirectionType) {
        this.direction = value;
    }
    protected range_x: number = -1;
    public get RANGE_X(): number {
        return this.range_x;
    }
    public set RANGE_X(value: number) {
        this.range_x = value;
    }
    protected range_y: number = -1;
    public get RANGE_Y(): number {
        return this.range_y;
    }
    public set RANGE_Y(value: number) {
        this.range_y = value;
    }
    protected target: eSkillEffectTarget = eSkillEffectTarget.Enemy;
    public get TARGET(): eSkillEffectTarget {
        return this.target;
    }
    public set TARGET(value: eSkillEffectTarget) {
        this.target = value;
    }
    protected count: number = -1;
    public get COUNT(): number {
        return this.count;
    }
    public set COUNT(value: number) {
        this.count = value;
    }
    protected value: number = -1;
    public get VALUE(): number {
        return this.value;
    }
    public set VALUE(value: number) {
        this.value = value;
    }
    protected _desc1: number = -1;
    public get _DESC1(): number {
        return this._desc1;
    }
    public set _DESC1(value: number) {
        this._desc1 = value;
    }
    protected _desc2: number = -1;
    public get _DESC2(): number {
        return this._desc2;
    }
    public set _DESC2(value: number) {
        this._desc2 = value;
    }
    protected effect_prefab: string = "";
    public get EFFECT_PREFAB(): string {
        return this.effect_prefab;
    }
    public set EFFECT_PREFAB(value: string) {
        this.effect_prefab = value;
    }
    protected term: number = -1;
    public get TERM(): number {
        return this.term;
    }
    public set TERM(value: number) {
        this.term = value;
    }
    protected nest_count: number = -1;
    public get NEST_COUNT(): number {
        return this.nest_count;
    }
    public set NEST_COUNT(value: number) {
        this.nest_count = value;
    }
    protected max_time: number = -1;
    public get MAX_TIME(): number {
        return this.max_time;
    }
    public set MAX_TIME(value: number) {
        this.max_time = value;
    }
}
 
export class SkillProjectileData extends DataBase {
    protected key: number = -1;
    public get KEY(): number {
        return this.key;
    }
    public set KEY(value: number) {
        this.key = value;
    }
    protected type: string = "";
    public get TYPE(): string {
        return this.type;
    }
    public set TYPE(value: string) {
        this.type = value;
    }
    protected arrival_time: number = -1;
    public get ARRIVAL_TIME(): number {
        return this.arrival_time;
    }
    public set ARRIVAL_TIME(value: number) {
        this.arrival_time = value;
    }
    protected height: number = -1;
    public get HEIGHT(): number {
        return this.height;
    }
    public set HEIGHT(value: number) {
        this.height = value;
    }
    protected start_x: number = -1;
    public get START_X(): number {
        return this.start_x;
    }
    public set START_X(value: number) {
        this.start_x = value;
    }
    protected start_y: number = -1;
    public get START_Y(): number {
        return this.start_y;
    }
    public set START_Y(value: number) {
        this.start_y = value;
    }
    protected goal_x: number = -1;
    public get GOAL_X(): number {
        return this.goal_x;
    }
    public set GOAL_X(value: number) {
        this.goal_x = value;
    }
    protected goal_y: number = -1;
    public get GOAL_Y(): number {
        return this.goal_y;
    }
    public set GOAL_Y(value: number) {
        this.goal_y = value;
    }
    protected projectile_order: string = "";
    public get PROJECTILE_ORDER(): string {
        return this.projectile_order;
    }
    public set PROJECTILE_ORDER(value: string) {
        this.projectile_order = value;
    }
    protected projectile_image: string = "";
    public get PROJECTILE_IMAGE(): string {
        return this.projectile_image;
    }
    public set PROJECTILE_IMAGE(value: string) {
        this.projectile_image = value;
    }
    protected effect_order: string = "";
    public get EFFECT_ORDER(): string {
        return this.effect_order;
    }
    public set EFFECT_ORDER(value: string) {
        this.effect_order = value;
    }
    protected effect_image: string = "";
    public get EFFECT_IMAGE(): string {
        return this.effect_image;
    }
    public set EFFECT_IMAGE(value: string) {
        this.effect_image = value;
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

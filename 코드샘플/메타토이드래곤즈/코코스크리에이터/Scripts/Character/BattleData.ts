import { effects, Node, Vec2, Vec3 } from "cc";
import { ProjectileAnim } from "../Battle/ProjectileAnim";
import { SkillEvent } from "../Battle/SkillEvent";
import { eBattleSkillType } from "../Battle/WaveBattle";
import { eDamageType } from "../DamageManager";
import { CharBaseData, CharGradeData } from "../Data/CharData";
import { MonsterBaseData, MonsterSpawnData } from "../Data/MonsterData";
import { SkillCharData, SkillEffectData, SkillProjectileData } from "../Data/SkillData";
import { StatFactorData } from "../Data/StatData";
import { BezierCurve2, DragonStat } from "../Tools/SandboxTools";
import { BattleAIMonster } from "./BattleAIMonster";
import { BattleDragon } from "./BattleDragon";

/**
 * Predefined variables
 * Name = BattleData
 * DateTime = Tue Mar 22 2022 14:09:03 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = BattleData.ts
 * FileBasenameNoExtension = BattleData
 * URL = db://assets/Scripts/Character/BattleData.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */

export const globalHIT: number = 1000;
export const globalDODGE: number = 80;
export const teamDragon: number = 1;
export const teamMonster: number = 2;
export const BattleTileX = 45;
export const BattleTileY = 35;
export const BattleDragonScale = 0.375;
export const BattleMonsterScale = 0.375;
export const BattleEffectScale = 0.75;

export enum eBattleAttackState {
    None,
    Casting,
    AttackAnim,
    Attack,
    AttackDelay
}

export class BattleData {
    protected dragonTag: number = -1;
    public get DragonTag(): number {
        return this.dragonTag;
    }
    public set DragonTag(value: number){
        this.dragonTag = value;
    }
    protected battleTag: number = -1;
    public get BattleTag(): number {
        return this.battleTag;
    }
    public set BattleTag(value: number){
        this.battleTag = value;
    }
    protected level: number = -1;
    public get LEVEL(): number {
        return this.level;
    }
    public set LEVEL(value: number){
        this.level = value;
    }
    protected hp: number = -1;
    public get HP(): number {
        return this.hp;
    }
    public set HP(value: number){
        this.hp = value;
    }
    protected maxHp: number = -1;
    public get MAXHP(): number {
        return this.maxHp;
    }
    public set MAXHP(value: number) {
        this.maxHp = value;
    }
    protected att: number = -1;
    public get ATK(): number {
        var buff: number = 0;
        var param1: number = this.GetEffectParam('DECREASE_ATK');
        if(param1 != null) {
            buff -= param1;
        }
        var param2: number = this.GetEffectParam('INCREASE_ATK');
        if(param2 != null) {
            buff += param2;
        }
        var per_buff: number = 0;
        var param3: number = this.GetEffectParam('DECREASE_ATK_PER');
        if(param3 != null) {
            per_buff -= (this.att + buff) * param3 * 0.01;
        }
        var param4: number = this.GetEffectParam('INCREASE_ATK_PER');
        if(param4 != null) {
            per_buff += (this.att + buff) * param4 * 0.01;
        }
        buff += per_buff;

        return this.att + buff;
    }
    public set ATK(value: number){
        this.att = value;
    }
    protected def: number = -1;
    public get DEF(): number {
        var buff: number = 0;
        var param1: number = this.GetEffectParam('DECREASE_DEF');
        if(param1 != null) {
            buff -= param1;
        }
        var param2: number = this.GetEffectParam('INCREASE_DEF');
        if(param2 != null) {
            buff += param2;
        }
        var per_buff: number = 0;
        var param3: number = this.GetEffectParam('DECREASE_DEF_PER');
        if(param3 != null) {
            per_buff -= (this.att + buff) * param3 * 0.01;
        }
        var param4: number = this.GetEffectParam('INCREASE_DEF_PER');
        if(param4 != null) {
            per_buff += (this.att + buff) * param4 * 0.01;
        }
        buff += per_buff;

        return this.def + buff;
    }
    public set DEF(value: number){
        this.def = value;
    }
    protected cri: number = -1;
    public get CRI(): number {
        var buff: number = 0;
        var param1: number = this.GetEffectParam('DECREASE_CRI_RATE_PER');
        if(param1 != null) {
            buff -= param1;
        }
        var param2: number = this.GetEffectParam('INCREASE_CRI_RATE_PER');
        if(param2 != null) {
            buff += param2;
        }

        return this.cri + buff;
    }
    public set CRI(value: number){
        this.cri = value;
    }
    protected cri_dmg: number = -1;
    public get CRI_DMG(): number {
        var buff: number = 0;
        var param1: number = this.GetEffectParam('DECREASE_CRI_DMG_PER');
        if(param1 != null) {
            buff -= param1;
        }
        var param2: number = this.GetEffectParam('INCREASE_CRI_DMG_PER');
        if(param2 != null) {
            buff += param2;
        }

        return this.cri_dmg + buff;
    }
    public set CRI_DMG(value: number){
        this.cri_dmg = value;
    }
    protected speed: number = -1;
    public get SPEED(): number {
        return this.speed;
    }
    public set SPEED(value: number){
        this.speed = value;
    }
    protected hit: number = -1;
    public get HIT(): number {
        var buff: number = 0;
        var param1: number = this.GetEffectParam('DECREASE_HIT_PER');
        if(param1 != null) {
            buff -= param1;
        }
        var param2: number = this.GetEffectParam('INCREASE_HIT_PER');
        if(param2 != null) {
            buff += param2;
        }

        return this.hit + buff;
    }
    public set HIT(value: number){
        this.hit = value;
    }
    protected dodge: number = -1;
    public get DODGE(): number {
        var buff: number = 0;
        var param1: number = this.GetEffectParam('DECREASE_DODGE_PER');
        if(param1 != null) {
            buff -= param1;
        }
        var param2: number = this.GetEffectParam('INCREASE_DODGE_PER');
        if(param2 != null) {
            buff += param2;
        }

        return this.dodge + buff;
    }
    public set DODGE(value: number){
        this.dodge = value;
    }
    protected death: boolean = false;
    public get Death(): boolean {
        return this.death;
    }
    public set Death(value: boolean){
        this.death = value;
    }
    protected delayAttack: number = -1;
    public get DelayAttack(): number {
        return this.delayAttack;
    }
    public set DelayAttack(value: number){
        this.delayAttack = value;
    }
    protected delayNormal: number = -1;
    public get DelayNormal(): number {
        return this.delayNormal;
    }
    public set DelayNormal(value: number){
        this.delayNormal = value;
    }
    protected delaySkill1: number = -1;
    public get DelaySkill1(): number {
        return this.delaySkill1;
    }
    public set DelaySkill1(value: number){
        this.delaySkill1 = value;
    }
    protected delaySkill2: number = -1;
    public get DelaySkill2(): number {
        return this.delaySkill2;
    }
    public set DelaySkill2(value: number){
        this.delaySkill2 = value;
    }
    protected normalSkill: SkillCharData = null;
    public get NormalSkill(): SkillCharData {
        return this.normalSkill;
    }
    public set NormalSkill(value: SkillCharData) {
        this.normalSkill = value;
    }
    protected normalProjectile: SkillProjectileData = null;
    public get NormalProjectile(): SkillProjectileData {
        return this.normalProjectile;
    }
    public set NormalProjectile(value: SkillProjectileData) {
        this.normalProjectile = value;
    }
    protected normalEffect: SkillEffectData[] = null;
    public get NormalEffect(): SkillEffectData[] {
        return this.normalEffect;
    }
    public set NormalEffect(value: SkillEffectData[]) {
        this.normalEffect = value;
    }
    protected skill1: SkillCharData = null;
    public get Skill1(): SkillCharData {
        return this.skill1;
    }
    public set Skill1(value: SkillCharData) {
        this.skill1 = value;
    }
    protected skill1Projectile: SkillProjectileData = null;
    public get Skill1Projectile(): SkillProjectileData {
        return this.skill1Projectile;
    }
    public set Skill1Projectile(value: SkillProjectileData) {
        this.skill1Projectile = value;
    }
    protected effect1: SkillEffectData[] = null;
    public get Effect1(): SkillEffectData[] {
        return this.effect1;
    }
    public set Effect1(value: SkillEffectData[]) {
        this.effect1 = value;
    }
    protected skill2: SkillCharData = null;
    public get Skill2(): SkillCharData {
        return this.skill2;
    }
    public set Skill2(value: SkillCharData) {
        this.skill2 = value;
    }
    protected skill2Projectile: SkillProjectileData = null;
    public get Skill2Projectile(): SkillProjectileData {
        return this.skill2Projectile;
    }
    public set Skill2Projectile(value: SkillProjectileData) {
        this.skill2Projectile = value;
    }
    protected effect2: SkillEffectData[] = null;
    public get Effect2(): SkillEffectData[] {
        return this.effect2;
    }
    public set Effect2(value: SkillEffectData[]) {
        this.effect2 = value;
    }
    protected activeSkillType: eBattleSkillType = eBattleSkillType.None;
    public get ActiveSkillType(): eBattleSkillType {
        return this.activeSkillType;
    }
    public set ActiveSkillType(value: eBattleSkillType) {
        this.activeSkillType = value;
    }
    protected triggerDelay: number = -1;
    public get TriggerDelay(): number {
        return this.triggerDelay;
    }
    public set TriggerDelay(value: number){
        this.triggerDelay = value;
    }

    protected midDelay: number = -1;
    public get MidDelay(): number {
        return this.midDelay;
    }
    public set MidDelay(value: number){
        this.midDelay = value;
    }

    protected pos: Vec2 = null;
    public get Pos(): Vec2 {
        return this.pos;
    }
    protected maxY: number = null;
    public get MaxY(): number {
        return this.maxY;
    }
    protected posTear: number = null;
    public get PosTear(): number {
        return this.posTear;
    }
    protected startPos: Vec3 = null;
    public get StartPos(): Vec3 {
        return this.startPos;
    }
    protected effectObject: {} = null;
    public get EffectObject() {
        if(this.effectObject == null) {
            this.effectObject = {};
        }
        return this.effectObject;
    }
    protected curAttackState: eBattleAttackState = eBattleAttackState.None;
    public get CurAttackState(): eBattleAttackState {
        return this.curAttackState;
    }
    public set CurAttackState(value: eBattleAttackState){
        this.curAttackState = value;
    }

    public SetEffectActive(name: string, active: boolean): void {
        if(this.EffectObject[name] == undefined) {
            this.EffectObject[name] = {};
        }

        this.EffectObject[name]['active'] = active;

        if(active == true){
            SkillEvent.TriggerEvent({
                state: 'Regist',
                tag: this.DragonTag,
                skillname: name,
                isPlay : active,
            });
        }
        else
        {
            SkillEvent.TriggerEvent({
                state: 'Delete',
                tag: this.DragonTag,
                skillname: name,
                isPlay : active,
            });
        }
    }

    public SetEffectTime(name: string, time: number, param?: number): void {
        if(this.EffectObject[name] == undefined) {
            this.EffectObject[name] = {};
        }

        this.EffectObject[name]['time'] = time;
        this.EffectObject[name]['max_time'] = time;
        if(param != undefined) {
            this.EffectObject[name]['param'] = param;
        }

        if(time > 0){
            SkillEvent.TriggerEvent({
                state: 'Regist',
                tag: this.DragonTag,
                skillname: name,
                isPlay : true,
            });
        }
        else
        {
            SkillEvent.TriggerEvent({
                state: 'Delete',
                tag: this.DragonTag,
                skillname: name,
                isPlay : false,
            });
        }
    }

    public SetEffectTick(name: string, caster: BattleData, skill: SkillEffectData, skillType: eBattleSkillType, e: Node, move_dir: string): void {
        if(this.EffectObject[name] == undefined) {
            this.EffectObject[name] = {};
            this.EffectObject[name]['time'] = skill.MAX_TIME;
            this.EffectObject[name]['active'] = true;
        } else {
            this.EffectObject[name]['time'] = skill.MAX_TIME - this.EffectObject[name]['time'] % skill.TERM;
            this.EffectObject[name]['active'] = true;
        }

        //임시 데이터 세팅
        if(this.EffectObject[name]['time'] <= 0)
        {
            this.EffectObject[name]['time'] = 1;
        }

        this.EffectObject[name]['tick'] = skill.TERM;
        this.EffectObject[name]['last_tick'] = 0;
        this.EffectObject[name]['max_time'] = skill.MAX_TIME;
        this.EffectObject[name]['param'] = skill.VALUE;
        this.EffectObject[name]['skill'] = skill;
        this.EffectObject[name]['skillType'] = skillType;
        this.EffectObject[name]['e'] = e;
        this.EffectObject[name]['move_dir'] = move_dir;
        this.EffectObject[name]['caster'] = caster;

        SkillEvent.TriggerEvent({
            state: 'Regist',
            tag: this.DragonTag,
            skillname: name,
        });
    }

    public GetEffectParam(name: string): number | null {
        if(this.EffectObject[name] == undefined) {
            return null;
        }

        if(this.EffectObject[name]['time'] <= 0) {
            return null;
        }

        if(this.EffectObject[name]['param'] == undefined) {
            return null;
        }
        return this.EffectObject[name]['param'];
    }

    public GetEffectTime(name: string): number | null {
        if(this.EffectObject[name] == undefined) {
            return null;
        }

        if(this.EffectObject[name]['time'] <= 0) {
            return null;
        }

        return this.EffectObject[name]['time'];
    }

    public GetEffectActive(name: string): boolean {
        if(this.EffectObject[name] == undefined) {
            return false;
        }

        return this.EffectObject[name]['active'];
    }

    public GetEffectMaxTime(name: string): number | null {
        if(this.EffectObject[name] == undefined) {
            return null;
        }

        if(this.EffectObject[name]['max_time'] == undefined) {
            return null;
        }

        return this.EffectObject[name]['max_time'];
    }

    public GetEffectTick(name: string): number | null {
        if(this.EffectObject[name] == undefined) {
            return null;
        }

        if(this.EffectObject[name]['tick'] <= 0) {
            return null;
        }

        return this.EffectObject[name]['tick'];
    }

    public GetEffectLastTick(name: string): number | null {
        if(this.EffectObject[name] == undefined) {
            return null;
        }

        if(this.EffectObject[name]['last_tick'] == undefined) {
            return null;
        }

        return this.EffectObject[name]['last_tick'];
    }

    public GetTickDmgList(): any[] {
        let list = [];

        const keys = Object.keys(this.EffectObject);
        const keysCount = keys.length;

        for(var i = 0 ; i < keysCount ; i++) {
            const key = keys[i];

            if(key.indexOf("TICK_DMG") == -1) {
                continue;
            }

            list.push(this.EffectObject[key]);
        }

        return list;
    }

    public IsTurnSkip(): boolean {
        if(this.death) {
            return true;
        }
        if(this.GetEffectActive('AIRBORNE') || this.GetEffectTime('AIRBORNE') > 0) {
            return true;
        }
        if(this.GetEffectActive('STUN') || this.GetEffectTime('STUN') > 0) {
            return true;
        }
        if(this.DelayAttack > 0) {
            return true;
        }
        if(this.TriggerDelay > 0) {
            return true;
        }
        if(this.MidDelay > 0) {
            return true;
        }
        return false;
    }

    public IsTargetSkip(): boolean {
        if(this.death) {
            return true;
        }
        if(this.GetEffectActive('AIRBORNE')) {
            return true;
        }
        return false;
    }

    Update(dt: number): void {
        if(this.EffectObject != null) {
            const keys = Object.keys(this.EffectObject);
            const keysCount = keys.length;

            for(var i = 0 ; i < keysCount ; i++) {
                const curKey = keys[i];
                let curEffect = this.EffectObject[curKey];

                if(curEffect != undefined && curEffect['time'] != undefined) {
                    curEffect['time'] -= dt;
                    if(curEffect['time'] < 0){
                        curEffect['time'] = 0;

                        let buffCheck = this.isBuffCheck(curKey);
                        if(buffCheck)
                        {
                            this.SetEffectTime(curKey,0);
                        }
                    }
                }
            }
        }

        this.delayNormal -= dt;
        if(this.delayNormal < 0){
            this.delayNormal = 0;
        }
        this.delaySkill1 -= dt;
        if(this.delaySkill1 < 0){
            this.delaySkill1 = 0;
        }
        this.delaySkill2 -= dt;
        if(this.delaySkill2 < 0){
            this.delaySkill2 = 0;
        }
        this.delayAttack -= dt;
        if(this.delayAttack < 0){
            this.delayAttack = 0;
        }
        this.triggerDelay -= dt;
        if(this.triggerDelay < 0){
            this.midDelay += this.triggerDelay;
            this.triggerDelay = 0;
            if(this.midDelay < 0){
                this.midDelay = 0;
            }
        }
    }

    WaveInit() {
        this.delayNormal = 0;
        this.delayAttack = 0;
        this.triggerDelay = 0;
        this.midDelay = 0;
        this.activeSkillType = eBattleSkillType.None;
    }

    isBuffCheck(skillName : string) : boolean
    {
        let buffList = ['INCREASE_ATK',
        'INCREASE_ATK_PER',
        'INCREASE_DEF',
        'INCREASE_DEF_PER',
        'INCREASE_CRI_RATE_PER',
        'INCREASE_CRI_DMG_PER', 
        'INCREASE_DODGE_PER',
        'INCREASE_HIT_PER',
        'DECREASE_ATK',
        'DECREASE_ATK_PER',
        'DECREASE_DEF',
        'DECREASE_DEF_PER',
        'DECREASE_CRI_RATE_PER',
        'DECREASE_CRI_DMG_PER',
        'DECREASE_DODGE_PER',
        'DECREASE_HIT_PER'];

        for(let i = 0 ; i < buffList.length ; i++){
            let checkSkill = buffList[i];
            let isContain = skillName.includes(checkSkill);
            if(isContain){
                return true;
            }
        }
        return false;
    }
}

export class BattleMonsterBaseData extends MonsterBaseData {
    protected factor: number = -1;
    public get FACTOR(): number {
        return this.factor;
    }
    public set FACTOR(value: number) {
        this.factor = value;
    }
    protected grade: number = -1;
    public get GRADE(): number {
        return this.grade;
    }
    public set GRADE(value: number) {
        this.grade = value;
    }
    protected element: number = -1;
    public get ELEMENT(): number {
        return this.element;
    }
    public set ELEMENT(value: number) {
        this.element = value;
    }

    SetBase(data: MonsterBaseData) {
        this.ATK = data.ATK;
        this.CRITICAL = data.CRITICAL;
        this.CRITICAL_DMG = data.CRITICAL_DMG;
        this.DEF = data.DEF;
        this.HP = data.HP;
        this.IMAGE = data.IMAGE;
        this.Index = data.Index;
        this.NORMAL_SKILL = data.NORMAL_SKILL;
        this.SIZE = data.SIZE;
        this.SKILL1 = data.SKILL1;
        this.SKILL2 = data.SKILL2;
        this.SPEED = data.SPEED;
        this._DESC = data._DESC;
        this._NAME = data._NAME;
    }

    SetSpawn(data: MonsterSpawnData) {
        this.ELEMENT = data.ELEMENT;
        this.FACTOR = data.FACTOR;
        this.GRADE = data.GRADE;
    }
}

export class BattleDragonData extends BattleData {
    protected baseData: CharBaseData = null;
    public get BaseData(): CharBaseData {
        return this.baseData;
    }
    public set BaseData(value: CharBaseData) {
        this.baseData = value;
    }
    protected gradeData: CharGradeData = null;
    public get GradeData(): CharGradeData {
        return this.gradeData;
    }
    public set GradeData(value: CharGradeData) {
        this.gradeData = value;
    }
    protected statData: StatFactorData = null;
    public get StatData(): StatFactorData {
        return this.statData;
    }
    public set StatData(value: StatFactorData) {
        this.statData = value;
    }
    protected node: BattleDragon = null;
    public get Node(): BattleDragon {
        return this.node;
    }
    public get Team(): number {
        return teamDragon;
    }
    public scale: number = BattleDragonScale;
    public get Scale() {
        return this.scale;
    }

    Init (dragon: BattleDragon, x: number ,y: number, maxY: number) {
        this.node = dragon;
        if (this.normalSkill != null) {
            this.delayNormal = this.normalSkill.START_COOL_TIME;
        }
        if (this.skill1 != null) {
            this.delaySkill1 = this.skill1.START_COOL_TIME;
        }
        if (this.skill2 != null) {
            this.delaySkill2 = this.skill2.START_COOL_TIME;
        }

        if(this.gradeData != null && this.statData != null) {
            let data = DragonStat(this.LEVEL, this.BaseData, this.GradeData, this.StatData);
            this.HP = data.HP;
            this.MAXHP = Number(data.HP);
            this.ATK = data.ATK;
            this.DEF = data.DEF;
            this.CRI = data.CRI;
            this.CRI_DMG = this.BaseData.CRITICAL_DMG;
            this.SPEED = this.BaseData.SPEED;
            this.HIT = globalHIT;
            this.DODGE = globalDODGE;
        } else {
            this.HP = this.BaseData.HP;
            this.MAXHP = this.HP;
            this.ATK = this.BaseData.ATK;
            this.DEF = this.BaseData.DEF;
            this.CRI = this.BaseData.CRITICAL;
            this.CRI_DMG = this.BaseData.CRITICAL_DMG;
            this.SPEED = this.BaseData.SPEED;
            this.HIT = globalHIT;
            this.DODGE = globalDODGE;
        }
        this.pos = new Vec2(x, y);
        this.maxY = maxY;
        var defaultY = 15;
        switch(this.maxY) {
            case 1: {
                defaultY = 15;
                this.posTear = 3;
            } break;
            case 2: {
                defaultY = 15 - BattleTileY * 0.75;
                switch(this.pos.y) {
                    case 0: {
                        this.posTear = 2;
                    } break;
                    case 1: {
                        this.posTear = 4;
                    } break;
                }
            } break;
            // case 3: {
            //     defaultY = -15;
            //     switch(this.pos.y) {
            //         case 0: {
            //             this.posTear = 1;
            //         } break;
            //         case 1: {
            //             this.posTear = 3;
            //         } break;
            //         case 2: {
            //             this.posTear = 5;
            //         } break;
            //     }
            // } break;
        }

        this.startPos = new Vec3();
        this.startPos.x = -450 - x * BattleTileX;
        this.startPos.y = defaultY + (maxY - y - 1) * BattleTileY * 1.5;
        this.startPos.z = 0;
        this.Node.node.setPosition(new Vec3(this.startPos.x, this.startPos.y, this.startPos.z));
        this.Node.node.setScale(new Vec3(-this.Scale, this.Scale, this.Scale));
    }
}

export class BattleAIMonsterData extends BattleData {
    protected baseData: BattleMonsterBaseData = null;
    public get BaseData(): BattleMonsterBaseData {
        return this.baseData;
    }
    public set BaseData(value: BattleMonsterBaseData) {
        this.baseData = value;
    }
    protected spawnData: MonsterSpawnData = null;
    public get SpawnData(): MonsterSpawnData {
        return this.spawnData;
    }
    public set SpawnData(value: MonsterSpawnData) {
        this.spawnData = value;
    }
    protected gradeData: CharGradeData = null;
    public get GradeData(): CharGradeData {
        return this.gradeData;
    }
    public set GradeData(value: CharGradeData) {
        this.gradeData = value;
    }
    protected statData: StatFactorData = null;
    public get StatData(): StatFactorData {
        return this.statData;
    }
    public set StatData(value: StatFactorData) {
        this.statData = value;
    }
    protected node: BattleAIMonster = null;
    public get Node(): BattleAIMonster {
        return this.node;
    }
    public get Team(): number {
        return teamMonster;
    }
    public scale: number = BattleMonsterScale;
    public get Scale() {
        return this.scale;
    }

    Init (monster: BattleAIMonster, x: number ,y: number, maxY: number) {
        this.node = monster;

        if (this.normalSkill != null) {
            this.delayNormal = this.normalSkill.START_COOL_TIME;
        }
        if (this.skill1 != null) {
            this.delaySkill1 = this.skill1.START_COOL_TIME;
        }
        if (this.skill2 != null) {
            this.delaySkill2 = this.skill2.START_COOL_TIME;
        }
        if(this.gradeData != null && this.statData != null) {
            let data = DragonStat(this.LEVEL, this.BaseData, this.GradeData, this.StatData);
            this.HP = data.HP;
            this.MAXHP = Number(data.HP);
            this.ATK = data.ATK;
            this.DEF = data.DEF;
            this.CRI = data.CRI;
            this.CRI_DMG = this.BaseData.CRITICAL_DMG;
            this.SPEED = this.BaseData.SPEED;
            this.HIT = globalHIT;
            this.DODGE = globalDODGE;
        } else {
            this.HP = this.BaseData.HP;
            this.MAXHP = this.HP;
            this.ATK = this.BaseData.ATK;
            this.DEF = this.BaseData.DEF;
            this.CRI = this.BaseData.CRITICAL;
            this.CRI_DMG = this.BaseData.CRITICAL_DMG;
            this.SPEED = this.BaseData.SPEED;
            this.HIT = globalHIT;
            this.DODGE = globalDODGE;
        }
        this.pos = new Vec2(x, y);
        this.maxY = maxY;
        
        var defaultY = 15;
        switch(this.maxY) {
            case 1: {
                defaultY = 15;
                this.posTear = 3;
            } break;
            case 2: {
                defaultY = 0;
                switch(this.pos.y) {
                    case 0: {
                        this.posTear = 2;
                    } break;
                    case 1: {
                        this.posTear = 4;
                    } break;
                }
            } break;
            case 3: case 4: case 5: case 6: case 7: {
                defaultY = -15;
                switch(this.pos.y) {
                    case 0: {
                        this.posTear = 1;
                    } break;
                    case 1: {
                        this.posTear = 3;
                    } break;
                    case 2: {
                        this.posTear = 5;
                    } break;
                }
            } break;
        }

        this.startPos = new Vec3();
        this.startPos.x = 150 + x * BattleTileX;
        this.startPos.y = defaultY + (maxY - y - 1) * 32;
        this.startPos.z = 0;
        this.Node.node.setPosition(new Vec3(this.startPos.x, this.startPos.y, this.startPos.z));
        this.Node.node.setScale(new Vec3(-this.SpawnData.SCALE * this.Scale, this.SpawnData.SCALE * this.Scale, this.SpawnData.SCALE * this.Scale));
    }
}

export class ProjectileInfo {
    private skillType: eBattleSkillType = eBattleSkillType.None;
    public get SkillType(): eBattleSkillType {
        return this.skillType;
    }
    private caster: BattleDragonData | BattleAIMonsterData = null;
    public get Caster(): BattleDragonData | BattleAIMonsterData {
        return this.caster;
    }
    private move_dir: string = "";
    public get MoveDIR(): string {
        return this.move_dir;
    }
    private target: BattleDragonData | BattleAIMonsterData = null;
    private projectileNode: Node = null;
    public get ProjectileNode(): Node {
        return this.projectileNode;
    }
    private projectileAnim: ProjectileAnim = null;
    public get ProjectileAnim(): ProjectileAnim {
        return this.projectileAnim;
    }
    private projectile: SkillProjectileData = null;
    public get ProjectileData(): SkillProjectileData {
        return this.projectile;
    }
    private startPos: Vec3 = null;
    private goalPos: Vec3 = null;
    private curTime: number = 0;
    private isInit: boolean = false;
    private isMove: boolean = false;
    private effects: SkillEffectData[] = null;
    public get Effects(): SkillEffectData[] {
        return this.effects;
    }
    private targets: BattleDragonData[] | BattleAIMonsterData[] = null;
    public get Targets(): BattleDragonData[] | BattleAIMonsterData[] {
        return this.targets;
    }

    constructor(caster: BattleDragonData | BattleAIMonsterData, skillType: eBattleSkillType, target: BattleDragonData | BattleAIMonsterData, node: Node, projectile: SkillProjectileData, targets: BattleDragonData[] | BattleAIMonsterData[], move_dir: string) {
        this.caster = caster;
        this.target = target;
        this.projectileNode = node;
        this.projectileAnim = node.getComponent(ProjectileAnim);
        this.projectile = projectile;
        this.skillType = skillType;
        this.move_dir = move_dir;
        switch(this.skillType) {
            case eBattleSkillType.Normal: {
                this.effects = this.caster.NormalEffect;
            } break;
            case eBattleSkillType.Skill1: {
                this.effects = this.caster.Effect1;
            } break;
            case eBattleSkillType.Skill2: {
                this.effects = this.caster.Effect2;
            } break;
        }
        this.targets = targets;
    }

    Init() {
        if(this.projectile == null) {
            return;
        }
        this.startPos = new Vec3(this.caster.Node.node.worldPosition.x + this.projectile.START_X, this.caster.Node.node.worldPosition.y + this.projectile.START_Y, 0);
        this.projectileNode.setWorldPosition(this.startPos);
        this.goalPos = new Vec3(this.target.Node.node.worldPosition.x + this.projectile.GOAL_X, this.target.Node.node.worldPosition.y + this.projectile.GOAL_Y, 0);
        switch(this.projectile.TYPE) {
            case 'PROJECTILE': {
                this.isMove = true;
            } break;
            case 'NON_TARGET': {
                this.isMove = false;
            } break;
            case 'ANIM': {
                this.isMove = false;
                this.goalPos = this.startPos;
            } break;
            case 'ARRIVE_ANIM': {
                this.curTime = this.projectile.ARRIVAL_TIME;
            } break;
        }

        this.isInit = true;
    }

    Update(dt: number): void {
        if(!this.isInit) {
            return;
        }
        if(this.curTime >= this.projectile.ARRIVAL_TIME) {
            this.Bezier(this.projectile.ARRIVAL_TIME);
            return;
        }
        if(this.isMove) {
            this.goalPos = new Vec3(this.target.Node.node.worldPosition.x + this.projectile.GOAL_X, this.target.Node.node.worldPosition.y + this.projectile.GOAL_Y, 0);
        }
        this.curTime += dt;

        this.Bezier(this.curTime);
    }

    Bezier(time: number) {
        let startX: number = this.startPos.x;
        let wayX: number = this.startPos.x + (this.goalPos.x - this.startPos.x) * 0.5;
        let goalX: number = this.goalPos.x;

        const bezierX = BezierCurve2(startX, wayX, goalX, time, this.projectile.ARRIVAL_TIME);

        let startY: number = this.startPos.y;
        let wayY: number = this.startPos.y + this.projectile.HEIGHT;
        let goalY: number = this.goalPos.y;

        const bezierY = BezierCurve2(startY, wayY, goalY, time, this.projectile.ARRIVAL_TIME);

        this.projectileNode.setWorldPosition(new Vec3(bezierX, bezierY, 0));
    }

    Arrival(): boolean {
        if(this.curTime >= this.projectile.ARRIVAL_TIME) {
            return true;
        }
        return false;
    }

    Death(): void {
        this.curTime = this.projectile.ARRIVAL_TIME;
        this.projectileAnim?.Death();
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

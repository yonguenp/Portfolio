
import { _decorator, Node, Vec3, instantiate, tween, tweenUtil, Collider, Vec2 } from 'cc';
import { BattleAIMonsterData, BattleDragonData, BattleTileX, BattleTileY, ProjectileInfo, BattleEffectScale } from '../Character/BattleData';
import { DragonDefaultSpeed } from '../Character/Dragon';
import { DamageManager, eDamageType } from '../DamageManager';
import { eElementType } from '../Data/ElementData';
import { eSkillEffectDirectionType, eSkillEffectStartType, eSkillEffectTarget, SkillCharData, SkillEffectData, SkillProjectileData } from '../Data/SkillData';
import { NetworkManager } from '../NetworkManager';
import { ResourceManager, ResourcesType } from '../ResourceManager';
import { DataManager } from '../Tools/DataManager';
import { Attack, Defense, HitCheck, Offense, RandomInt } from '../Tools/SandboxTools';
import { Circle } from '../Tools/SBObject';
import { ToastMessage } from '../UI/ToastMessage';
import { User } from '../User/User';
import { BattleUI } from './BattleUI';
import { BattleSimulatorUI } from './Battle_simulator/BattleSimulatorUI';
import { SkillEvent } from './SkillEvent';
import { WaveBase, WaveEvent } from './WaveBase';
/**
 * Predefined variables
 * Name = WaveBattle
 * DateTime = Thu Feb 17 2022 18:17:02 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = WaveBattle.ts
 * FileBasenameNoExtension = WaveBattle
 * URL = db://assets/Scripts/Battle/WaveBattle.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */

export enum eWaveBattleState {
    Start = 0,
    Spawn,
    MonsterMove,
    Battle,
    Win,
    Lose,
    TimeOver,
    End
}

export enum eBattleSkillType {
    None,
    Normal,
    Skill1,
    Skill2
}

class BattleSkill {
    constructor(skill: SkillCharData, projectile: SkillProjectileData, effects: SkillEffectData[], skillType: eBattleSkillType) {
        this.skill = skill;
        this.projectile = projectile;
        this.effects = effects;
        this.skillType = skillType;
    }

    protected skill:SkillCharData = null;
    public get Skill(): SkillCharData {
        return this.skill;
    }
    protected skillType: eBattleSkillType = null;
    public get SkillType(): eBattleSkillType {
        return this.skillType;
    }
    protected projectile: SkillProjectileData = null;
    public get Projectile(): SkillProjectileData {
        return this.projectile;
    }
    protected effects: SkillEffectData[] = null;
    public get Effects(): SkillEffectData[] {
        return this.effects;
    }
}

export class WaveBattle extends WaveBase {
    private battleState: eWaveBattleState = eWaveBattleState.Start;
    private playingData: {} = null;

    GetID(): string {
        return 'WaveBattle';
    }

    Start(): void {
        this.battleState = eWaveBattleState.Start;
        let dragons = this.stageData.Dragons;
        const dragonsCount = dragons.length;
        for(var i = 0 ; i < dragonsCount ; i++) {
            let dragon = dragons[i];

            if(dragon.Death) {
                continue;
            }
            dragon.Node.AnimationStart('walk');
            dragon.Node.Controller.onExitEvent('up');
            dragon.Node.Controller.onExitEvent('down');
            dragon.Node.Controller.onExitEvent('left');
            dragon.Node.Controller.onExitEvent('right');
            dragon.WaveInit();
        }
        this.playingData = {};
        this.playingData['win_delay'] = 0;
        DamageManager.AllClear();
    }
    End(): void {
        DamageManager.AllClear();
        super.End();
    }
    
    Update(dt: number): string {
        this.stageData.UpdateTime(dt);
        this.stageData.UpdateCharacter(dt);

        if(this.stageData.Time <= 0) {
            this.battleState = eWaveBattleState.TimeOver;
        }
        
        switch (this.battleState) {
            case eWaveBattleState.Start: {
                this.stageData.MoveRight(dt);
                this.battleState = eWaveBattleState.Spawn;
            } break;
            case eWaveBattleState.Spawn: {
                this.stageData.MoveRight(dt);
                this.stageData.CurrentWaveStart();
                this.battleState = eWaveBattleState.MonsterMove;
            } break;
            case eWaveBattleState.MonsterMove: {
                this.stageData.MoveRight(dt);
                var complete = 0;
                let monsters = this.stageData.Monsters;
                const monsterCount = monsters.length;
                for(var i = 0 ; i < monsterCount ; i++) {
                    let monster = monsters[i];
                    if(monster == null || monster.Death) {
                        continue;
                    }

                    monster.Node.AnimationStart('idle');
                    monster.Node.Controller.onEnterEvent('left');
                    monster.Node.SetSpeed(DragonDefaultSpeed * this.stageData.StageSpeed);
                    monster.Node.Controller.onController(dt, true, 150 * this.stageData.StageSpeed);

                    const targetPosX = -150 + monster.Pos.x * BattleTileX;
                    if(monster.Node.node.position.x <= targetPosX) {
                        var pos = new Vec3(targetPosX, monster.Node.node.position.y, monster.Node.node.position.z);
                        monster.Node.node.position = pos;
                        monster.Node.AnimationStart('idle');
                        monster.Node.Controller.onExitEvent('left');
                        monster.Node.Controller.onController(dt);
                        complete++;
                    }
                }

                if(complete == monsterCount) {
                    for(var i = 0 ; i < monsterCount ; i++) {
                        let monster = monsters[i];
                        if(monster == null) {
                            continue;
                        }
    
                        monster.Node.SetSpeed(DragonDefaultSpeed * this.stageData.StageSpeed);
                    }
                    this.battleState = eWaveBattleState.Battle;
                }
            } break;
            case eWaveBattleState.Battle: {
                var isAnimation = false;
                var isDelay = false;
                var dragonsDeathCount = 0;
                var monstersDeathCount = 0;
                
                let dragonDeath = true;
                let monsterDeath = true;
                const dragonsCount = this.stageData.Dragons.length;
                for(var i = 0 ; i < dragonsCount ; i++) {
                    if(this.stageData.Dragons[i].DelayAttack > 0) {
                        isAnimation = true;
                    }
                    if(this.stageData.Dragons[i].Death) {
                        dragonDeath = dragonDeath && !this.stageData.Dragons[i].Node.node.active;
                        dragonsDeathCount++;
                    }
                    if(this.stageData.Dragons[i].GetEffectTime('STUN') > 0) {
                        isDelay = true;
                    }
                }
                const monstersCount = this.stageData.Monsters.length;
                for(var i = 0 ; i < monstersCount ; i++) {
                    if(this.stageData.Monsters[i].DelayAttack > 0) {
                        isAnimation = true;
                    }
                    if(this.stageData.Monsters[i].Death) {
                        monsterDeath = monsterDeath && !this.stageData.Monsters[i].Node.node.active;
                        monstersDeathCount++;
                    }
                }

                if(this.playingData['projectile'] != undefined && this.playingData['projectile'].length > 0) {
                    isAnimation = true;
                }

                if(monstersDeathCount == monstersCount || dragonsDeathCount == dragonsCount) {
                    if((this.stageData.CurWave == this.stageData.MaxWave && monstersDeathCount == monstersCount) || dragonsDeathCount == dragonsCount) {
                        if(this.playingData['flash'] == undefined || !this.playingData['flash']) {
                            this.playingData['flash'] = true;
                            BattleUI.OnFlash();
                        }
    
                        if(isDelay) {
                            this.ProjectileEvent('right', dt);
                            this.ProjectileEvent('left', dt);
                            return "";
                        }

                        this.stageData.StageSpeed = 0.5;
                        for(var i = 0 ; i < dragonsCount ; i++) {
                            if(this.stageData.Dragons[i].Death) {
                                continue;
                            }
                            this.stageData.Dragons[i].Node.SetSpeed(DragonDefaultSpeed * this.stageData.StageSpeed);
                        }
                        for(var i = 0 ; i < monstersCount ; i++) {
                            if(this.stageData.Monsters[i].Death) {
                                continue;
                            }
                            this.stageData.Monsters[i].Node.SetSpeed(DragonDefaultSpeed * this.stageData.StageSpeed);
                        }
                    }
    
                    if(isDelay) {
                        this.ProjectileEvent('right', dt);
                        this.ProjectileEvent('left', dt);
                        return "";
                    }

                    if(!(dragonDeath && monsterDeath)) {
                        this.CasterEvent(this.stageData.Dragons, this.stageData.Monsters, 'right', dt);
                        this.CasterEvent(this.stageData.Monsters, this.stageData.Dragons, 'left', dt);

                        this.TickDamageEvent(this.stageData.Dragons);
                        this.TickDamageEvent(this.stageData.Monsters);

                        this.ProjectileEvent('right', dt);
                        this.ProjectileEvent('left', dt);
                        return "";
                    }
                }

                this.CasterEvent(this.stageData.Dragons, this.stageData.Monsters, 'right', dt);
                this.CasterEvent(this.stageData.Monsters, this.stageData.Dragons, 'left', dt);

                this.TickDamageEvent(this.stageData.Dragons);
                this.TickDamageEvent(this.stageData.Monsters);

                this.ProjectileEvent('right', dt);
                this.ProjectileEvent('left', dt);
                
                if(isAnimation) {
                    return "";
                }
                
                if(dragonsDeathCount == dragonsCount) {
                    this.stageData.StageSpeed = 0.5;
                    this.battleState = eWaveBattleState.Lose;

                    if(User.Instance.UNO > 0){
                        NetworkManager.Send('adventure/result', {
                            tag: this.stageData.BattleTag,
                            result: 3,
                            log: this.stageData.Logs[this.stageData.CurWave]
                        }, (jsonData) => {
                            if(jsonData['err'] == 0) {
                                this.stageData.SetData(jsonData);
                            } else {
                                ToastMessage.Set('Error !!!!');
                            }
                        });
                    }
                } else if(monstersDeathCount == monstersCount) {
                    this.battleState = eWaveBattleState.Win;
                    const isLastWave = this.stageData.CurWave == this.stageData.MaxWave;
                    const result = isLastWave ? 2 : 1;
                    if(isLastWave) {
                        this.battleState = eWaveBattleState.End;
                        for(var i = 0 ; i < dragonsCount ; i++) {
                            this.stageData?.Dragons[i]?.Node?.AnimationStart('win');
                        }
                    }
                    if(User.Instance.UNO > 0){
                        NetworkManager.Send('adventure/result', {
                            tag: this.stageData.BattleTag,
                            result: result,
                            log: this.stageData.Logs[this.stageData.CurWave],
                            alives: this.stageData.GetNumDragonAlive()
                        }, (jsonData) => {
                            if(jsonData['err'] == 0) {
                                this.stageData.SetData(jsonData);
                            } else {
                                ToastMessage.Set('Error !!!!');
                            }
                        });
                    }else{
                        let jsonData = BattleSimulatorUI.Instance.GetMonsterWaveData();
                        this.stageData.SetData(jsonData);
                    }
                }
            } break;
            case eWaveBattleState.Win: { //승리
                const dragonsCount = this.stageData.Dragons.length;
                for(var i = 0 ; i < dragonsCount ; i++) {
                    if(this.stageData.Dragons[i].Death) {
                        continue;
                    }
                    this.stageData.Dragons[i].Node.AnimationStart('idle');
                }
                if(this.playingData['win_delay'] >= 0.3) {
                    WaveEvent.TriggerEvent({state:'WaveMove'});
                }
                this.playingData['win_delay'] += dt;
            } break;
            case eWaveBattleState.Lose: { //패배
            } break;
            case eWaveBattleState.TimeOver: {//타임오버
                console.log('TIMEOVER !!!!!!!');
                if(User.Instance.UNO > 0){
                    NetworkManager.Send('adventure/result', {
                        tag: this.stageData.BattleTag,
                        result: 4,
                        log: this.stageData.Logs[this.stageData.CurWave]
                    }, (jsonData) => {
                        if(jsonData['err'] == 0) {
                            this.stageData.SetData(jsonData);
                        } else {
                            ToastMessage.Set('Error !!!!');
                        }
                    });
                }
            } break;
        }
        return "";
    }

    private GetDistanceList(caster: BattleDragonData | BattleAIMonsterData, targets: any): BattleDragonData[] | BattleAIMonsterData[] {
        targets = targets.filter((element) => !element.Death);
        return targets.sort((elementA, elementB) => {
            let distanceA = Vec2.distance(caster.Node.GetPosition(true), elementA.Node.GetPosition(true));
            let distanceB = Vec2.distance(caster.Node.GetPosition(true), elementB.Node.GetPosition(true));
            return distanceA - distanceB;
        });
    }

    private CasterEvent(casters: BattleDragonData[] | BattleAIMonsterData[], targets: BattleDragonData[] | BattleAIMonsterData[], move_dir: string, dt: number) {//로그는 이곳에서
        const casterCount = casters.length;
        const targetCount = targets.length;
        for(var i = 0 ; i < casterCount ; i++) {
            let curCaster = casters[i];
            if(curCaster.IsTurnSkip()) {
                continue;
            }
            if(this.playingData[move_dir] != undefined && this.playingData[move_dir].Death) {
                delete this.playingData[move_dir];
            }
            
            //발동 스킬 체크 부분
            let battleSkill = this.CheckSkill(curCaster, move_dir);
            if(battleSkill == null) {
                continue;
            }
            //

            //이동 할지 말지 체크 부분
            let sortTarget = this.GetDistanceList(curCaster, targets);
            const sortCount = sortTarget.length;
            var moveTarget: BattleDragonData | BattleAIMonsterData = null;
            var isNotMove: boolean = true;
            for(var j = 0 ; j < sortCount ; j++) {
                let curTarget = sortTarget[j];
                if(curTarget.Death) {
                    continue;
                }

                isNotMove = this.CheckCircle(curCaster, curTarget, battleSkill.Skill.RANGE_X, battleSkill.Skill.RANGE_Y);
                if(!isNotMove) {
                    moveTarget = curTarget;
                    break;
                } else {
                    break;
                }
            }
            //

            if(!isNotMove) {
                curCaster.Node.AnimationStart('walk');
                curCaster.Node.Controller.MoveTarget(dt, moveTarget.Node.GetPosition(true));
            } else {
                //실제 사용 데이터 부분
                let checkDatas = this.RangeCheckCaster(curCaster, casters, targets, move_dir, battleSkill);
                //
                if(checkDatas['caster'] != undefined && checkDatas['targetList'] != undefined) {
                    curCaster.Node.Controller.onExitEvent(move_dir);
                    curCaster.Node.Controller.onController(dt);
                    
                    let list = checkDatas['targetList'] as any[];
                    const listCount = list.length;
    
                    var isSkip = false;
                    for(var x = 0 ; x < listCount ; x++) {
                        let curTargets = list[x][1] as BattleDragonData[] | BattleAIMonsterData[];
                        const targetCount = curTargets.length;
                        for(var y = 0 ; y < targetCount ; y++) {
                            let curTarget = curTargets[y];
                            if(curTarget.IsTargetSkip()) {
                                continue;
                            }
    
                            isSkip = true;
                        }
                    }
    
                    if(!isSkip) {
                        continue;
                    }
    
                    let bSkill = checkDatas['skill'] as BattleSkill;
    
                    if(curCaster.ActiveSkillType == eBattleSkillType.None) {
                        curCaster.Node.AnimationStart('idle');
                        curCaster.ActiveSkillType = bSkill.SkillType;
                        if(this.playingData[move_dir] == undefined) {
                            switch(curCaster.ActiveSkillType) {
                                case eBattleSkillType.Normal: {
                                    curCaster.TriggerDelay = curCaster.NormalSkill.TRIGGER_DELAY;
                                    curCaster.MidDelay = curCaster.NormalSkill.Mid_delay;
                                    if(curCaster.TriggerDelay > 0) {
                                        continue;
                                    } else if(curCaster.MidDelay > 0) {
                                        curCaster.Node.AnimationStart('attack');
                                        continue;
                                    }
                                } break;
                                case eBattleSkillType.Skill1: {
                                    this.playingData[move_dir] = curCaster;
                                    //curCaster.TriggerDelay = curCaster.Skill1.TRIGGER_DELAY;
                                    curCaster.TriggerDelay = curCaster.Node.GetAnimationLength('casting') / curCaster.Node.GetSpeed();
                                    curCaster.MidDelay = curCaster.Skill1.Mid_delay;
                                    curCaster.DelaySkill1 = curCaster.Skill1.COOL_TIME;
                                    if(curCaster.TriggerDelay > 0) {
                                        curCaster.Node.AnimationStart('casting');
                                        continue;
                                    } else if(curCaster.MidDelay > 0) {
                                        curCaster.Node.AnimationStart('skill');
                                        continue;
                                    }
                                } break;
                                case eBattleSkillType.Skill2: {
                                    this.playingData[move_dir] = curCaster;
                                    //curCaster.TriggerDelay = curCaster.Skill2.TRIGGER_DELAY;
                                    curCaster.TriggerDelay = curCaster.Node.GetAnimationLength('casting') / curCaster.Node.GetSpeed();
                                    curCaster.MidDelay = curCaster.Skill2.Mid_delay;
                                    curCaster.DelaySkill2 = curCaster.Skill2.COOL_TIME;
                                    if(curCaster.TriggerDelay > 0) {
                                        curCaster.Node.AnimationStart('casting');
                                        continue;
                                    } else if(curCaster.MidDelay > 0) {
                                        curCaster.Node.AnimationStart('skill');
                                        continue;
                                    }
                                } break;
                            }
                        } else if(this.playingData[move_dir] != curCaster) {
                            curCaster.ActiveSkillType = eBattleSkillType.Normal;
                            curCaster.TriggerDelay = curCaster.NormalSkill.TRIGGER_DELAY;
                            curCaster.MidDelay = curCaster.NormalSkill.Mid_delay;
                            if(curCaster.MidDelay > 0) {
                                curCaster.Node.AnimationStart('attack');
                                continue;
                            }
                        }
                    }
    
                    var afDelay: number = 1.2;
                    let projectile: SkillProjectileData = null;
    
                    var animName = 'attack';
                    switch(curCaster.ActiveSkillType) {
                        case eBattleSkillType.Normal: {
                            curCaster.DelayNormal = curCaster.NormalSkill.COOL_TIME;
                            animName = 'attack';
                            afDelay = curCaster.NormalSkill.AFTER_DELAY;
                            projectile = curCaster.NormalProjectile;
                        } break;
                        case eBattleSkillType.Skill1: {
                            animName = 'skill';
                            afDelay = curCaster.Skill1.AFTER_DELAY;
                            projectile = curCaster.Skill1Projectile;
                            delete this.playingData[move_dir];
                        } break;
                        case eBattleSkillType.Skill2: {
                            animName = 'skill';
                            afDelay = curCaster.Skill2.AFTER_DELAY;
                            projectile = curCaster.Skill2Projectile;
                            delete this.playingData[move_dir];
                        } break;
                        default: {
                        } break;
                    }
    
                    let targetSkill = curCaster.ActiveSkillType;
                    curCaster.Node.AnimationStart(animName);
                    curCaster.DelayAttack = afDelay;
                    curCaster.ActiveSkillType = eBattleSkillType.None;
    
                    switch(true) {
                        case (projectile == null || projectile.TYPE == 'IMMEDIATE'): {
                            this.SkillTrigger(curCaster, targetSkill, move_dir, list);
                        } break;
                        case (projectile.TYPE == 'NON_TARGET'):
                        case (projectile.TYPE == 'PROJECTILE'): 
                        case (projectile.TYPE == 'ANIM'):
                        case (projectile.TYPE == 'ARRIVE_ANIM'):{
                            if(this.playingData['projectile'] == undefined) {
                                this.playingData['projectile'] = [];
                            }
                            
                            let prefabTile = ResourceManager.Instance.GetResource<Node>(ResourcesType.EFFECT_PREFAB)[projectile.PROJECTILE_IMAGE];
                            if(prefabTile == null) {
                                this.SkillTrigger(curCaster, targetSkill, move_dir, list);
                            } else {
                                let curTile: Node = instantiate(prefabTile);
                                switch(projectile.PROJECTILE_ORDER) {
                                    case 'BACK': {
                                        curTile.setParent(this.stageData.BackEffect, true);
                                    } break;
                                    case 'FRONT':
                                    default: {
                                        curTile.setParent(this.stageData.FrontEffect, true);
                                    } break;
                                }
                                if(move_dir == 'left') {
                                    curTile.setScale(-curTile.scale.x * BattleEffectScale, curTile.scale.y * BattleEffectScale, curTile.scale.z * BattleEffectScale); 
                                } else {
                                    curTile.setScale(curTile.scale.x * BattleEffectScale, curTile.scale.y * BattleEffectScale, curTile.scale.z * BattleEffectScale); 
                                }
                                let projectileInfo = new ProjectileInfo(curCaster, targetSkill, checkDatas['target'], curTile, projectile, list, move_dir);
                                projectileInfo.Init();
    
                                this.playingData['projectile'].push(projectileInfo);
                            }
                        } break;
                    }
                } else if(!curCaster.Death) {
                    curCaster.Node.AnimationStart('idle');
                    curCaster.Node.Controller.onExitEvent(move_dir);
                } else if(curCaster.Death) {
                    curCaster.Node.AnimationStart('death');
                }
            } 
        }
    }

    private ProjectileEvent(move_dir: string, dt: number) {
        if(this.playingData['projectile'] != undefined) {
            this.playingData['projectile'].forEach(element => {
                let info: ProjectileInfo = element;
                if(info != null) {
                    info.Update(dt);
                    switch(true) {
                        case (info.ProjectileData.TYPE != 'ANIM' && info.ProjectileData.TYPE != 'ARRIVE_ANIM'):{
                            if(info.ProjectileAnim.IsAnim) {
                                info.ProjectileAnim.NextAnim();
                            }
                        } break;
                    }
                }
            });
            
            let arrival = this.playingData['projectile'].filter(element => element.Arrival());
            if(arrival != null && arrival.length > 0) {
                arrival.forEach(element => {
                    let info: ProjectileInfo = element;
                    if(info != null && info.MoveDIR == move_dir) {
                        if(info.Caster.Death) {
                            info.Death();
                            info.ProjectileNode.destroy();
                            return;
                        }
                        if(info.ProjectileAnim == null) {
                            this.CreateEffect(ResourceManager.Instance.GetResource<Node>(ResourcesType.EFFECT_PREFAB)[info.ProjectileData.EFFECT_IMAGE], info.ProjectileNode, move_dir, info.ProjectileData);
    
                            info.ProjectileNode.destroy();
                            this.SkillTrigger(info.Caster, info.SkillType, move_dir, info.Targets);
                        } else if(element.Arrival() && info.ProjectileAnim.IsCurEND) {
                            info.ProjectileAnim.NextAnim();
                        
                            if(info.ProjectileAnim.IsEND) {
                                this.CreateEffect(ResourceManager.Instance.GetResource<Node>(ResourcesType.EFFECT_PREFAB)[info.ProjectileData.EFFECT_IMAGE], info.ProjectileNode, move_dir, info.ProjectileData);
        
                                info.ProjectileNode.destroy();
                                this.SkillTrigger(info.Caster, info.SkillType, move_dir, info.Targets);
                            }
                        }
                    }
                });
                this.playingData['projectile'] = this.playingData['projectile'].filter(element => !element.Arrival() || (element.ProjectileAnim != null && !element.ProjectileAnim.IsEND));
            }
        }
    }

    public TickDamageEvent(targets: BattleDragonData[] | BattleAIMonsterData[]) {
        if(targets == null) {
            return;
        }

        let logs = [];
        targets.forEach(element => {
            if(element.Death) {
                return;
            }
            let tickList = element.GetTickDmgList();
            tickList.forEach(elementData => {
                const maxTime = elementData['max_time'];
                const tick = elementData['tick'];
                const lastTime = elementData['time'];
                const isActive = elementData['active'];
                if(lastTime > 0 && !isActive) {
                    elementData['active'] = true;
                } else if(lastTime <= 0 && isActive) {
                    elementData['active'] = false;

                    SkillEvent.TriggerEvent({
                        state: 'Delete',
                        tag: element.DragonTag,
                        skillname: elementData['skill'].skill + elementData['skill'].index,
                        isPlay : elementData['active'],
                    });
                }
    
                if(elementData['active']) {
                    const curTime = maxTime - lastTime;
                    const curTick = Math.floor(curTime / tick);
                    if(elementData['last_tick'] != curTick) {
                        elementData['last_tick'] = curTick;

                        this.TickSkillDamage(true, elementData['caster'], element, elementData['skill'], elementData['skillType'], elementData['e'], elementData['move_dir'], logs);
                    }
                }
            });
        });

        //로그
        if(logs.length > 0) {
            const time = this.stageData.Time * 1000;
            const fTime = Math.floor(time);
            if(this.stageData.Logs[this.stageData.CurWave] == undefined) {
                this.stageData.Logs[this.stageData.CurWave] = {};
            }
            if(this.stageData.Logs[this.stageData.CurWave][fTime] == undefined) {
                this.stageData.Logs[this.stageData.CurWave][fTime] = logs;
            }
            else {
                const logCount = logs.length;
                for(var i = 0 ; i < logCount ; i++) {
                    this.stageData.Logs[this.stageData.CurWave][fTime].push(logs[i]);
                }
            }
        }
        //
    }

    private SkillTrigger(curCaster: BattleDragonData | BattleAIMonsterData, targetSkill: eBattleSkillType, move_dir: string, list: any[]) {
        const listCount = list.length;

        //여기부턴 투사체가 처리할 부분으로 이동필요.
        let logs = [];
        let hitCheck = {};
        for(var x = 0 ; x < listCount ; x++) {
            let curEffect = list[x][0] as SkillEffectData;
            let curTargets = list[x][1] as BattleDragonData[] | BattleAIMonsterData[];
            const targetCount = curTargets.length;
            let e: Node = undefined
            if(curEffect.EFFECT_PREFAB != 'NONE') {
                e = ResourceManager.Instance.GetResource<Node>(ResourcesType.EFFECT_PREFAB)[curEffect.EFFECT_PREFAB];;
            }
            for(var y = 0 ; y < targetCount ; y++) {
                let curTarget = curTargets[y];
                if(curTarget.IsTargetSkip()) {
                    continue;
                }

                if(hitCheck[curTarget.BattleTag] == undefined) {
                    if(curCaster.Team == curTarget.Team) {
                        hitCheck[curTarget.BattleTag] = true;
                    } else {
                        hitCheck[curTarget.BattleTag] = HitCheck(curCaster.LEVEL, curCaster.HIT, curTarget.LEVEL, curTarget.DODGE);
                    }
                }
                switch(curEffect.SKILL) {
                    case 'NORMAL_DMG':
                    case 'SKILL_DMG': {
                        this.SkillDamage(hitCheck[curTarget.BattleTag], curCaster, curTarget, curEffect, targetSkill, e, move_dir, logs);
                    } break;
                    case 'INCREASE_ATK':
                    case 'INCREASE_ATK_PER': 
                    case 'INCREASE_DEF':
                    case 'INCREASE_DEF_PER': 
                    case 'INCREASE_DODGE_PER':
                    case 'INCREASE_HIT_PER': 
                    case 'INCREASE_CRI_RATE_PER':
                    case 'INCREASE_CRI_DMG_PER': 
                    case 'DECREASE_ATK':
                    case 'DECREASE_ATK_PER': 
                    case 'DECREASE_DEF':
                    case 'DECREASE_DEF_PER': 
                    case 'DECREASE_DODGE_PER':
                    case 'DECREASE_HIT_PER': 
                    case 'DECREASE_CRI_RATE_PER':
                    case 'DECREASE_CRI_DMG_PER': {
                        this.EffectSkill(hitCheck[curTarget.BattleTag], curCaster, curTarget, curEffect, targetSkill, e, move_dir, logs);
                    } break;
                    case 'STUN': {
                        this.EffectSkill(hitCheck[curTarget.BattleTag], curCaster, curTarget, curEffect, targetSkill, e, move_dir, logs);
                    } break;
                    case 'AIRBORNE': {
                        this.EffectSkill(hitCheck[curTarget.BattleTag], curCaster, curTarget, curEffect, targetSkill, e, move_dir, logs);
                    } break;
                    case 'INVINCIBILITY': {
                        this.EffectSkill(hitCheck[curTarget.BattleTag], curCaster, curTarget, curEffect, targetSkill, e, move_dir, logs);
                    } break;
                    case 'SILENCE': {
                        this.EffectSkill(hitCheck[curTarget.BattleTag], curCaster, curTarget, curEffect, targetSkill, e, move_dir, logs);
                    } break;
                    case 'TICK_DMG': {
                        this.EffectSkillTick(hitCheck[curTarget.BattleTag], curCaster, curTarget, curEffect, targetSkill, e, move_dir, logs);
                    } break;
                }
            }
        }
        //

        //로그도 투사체 날렸다 및 타격되었다 등 으로 나뉘어야 할 것으로 보임.
        if(logs.length > 0) {
            const time = this.stageData.Time * 1000;
            const fTime = Math.floor(time);
            if(this.stageData.Logs[this.stageData.CurWave] == undefined) {
                this.stageData.Logs[this.stageData.CurWave] = {};
            }
            if(this.stageData.Logs[this.stageData.CurWave][fTime] == undefined) {
                this.stageData.Logs[this.stageData.CurWave][fTime] = logs;
            }
            else {
                const logCount = logs.length;
                for(var i = 0 ; i < logCount ; i++) {
                    this.stageData.Logs[this.stageData.CurWave][fTime].push(logs[i]);
                }
            }
        }
        //
    }
    
    private EffectSkillTick(HIT: boolean, curCaster: BattleDragonData | BattleAIMonsterData, curTarget: BattleDragonData | BattleAIMonsterData, curEffect: SkillEffectData, skillType: eBattleSkillType, e: Node, move_dir: string, logs: any) {
        if(!HIT) {
            var log = `${curCaster.BattleTag}_${skillType}_${curEffect.KEY}_${curEffect.SKILL}_${curTarget.BattleTag}_${0}`;
            return;
        }
        var log = `${curCaster.BattleTag}_${skillType}_${curEffect.KEY}_${curEffect.SKILL}${curEffect.GROUP}_${curTarget.BattleTag}_${curEffect.MAX_TIME}`;
        logs.push(log);
        // this.CreateEffect(e, curTarget.Node.node, move_dir);
        curTarget.SetEffectTick(`${curEffect.SKILL}${curEffect.GROUP}`, curCaster, curEffect, skillType, e, move_dir);
        // console.log(`${curEffect.SKILL}${curEffect.GROUP}`, curTarget.BattleTag, curEffect.MAX_TIME);
    }
    
    private EffectSkill(HIT: boolean, curCaster: BattleDragonData | BattleAIMonsterData, curTarget: BattleDragonData | BattleAIMonsterData, curEffect: SkillEffectData, skillType: eBattleSkillType, e: Node, move_dir: string, logs: any) {
        if(!HIT) {
            var log = `${curCaster.BattleTag}_${skillType}_${curEffect.KEY}_${curEffect.SKILL}_${curTarget.BattleTag}_${0}`;
            return;
        }
        var log = `${curCaster.BattleTag}_${skillType}_${curEffect.KEY}_${curEffect.SKILL}_${curTarget.BattleTag}_${curEffect.MAX_TIME}`;
        logs.push(log);
        this.CreateEffect(e, curTarget.Node.node, move_dir);
        curTarget.SetEffectTime(curEffect.SKILL, curEffect.MAX_TIME, curEffect.VALUE);
        // console.log(`${curEffect.SKILL}`, curTarget.BattleTag, curEffect.MAX_TIME);
    }

    private SkillDamage(HIT: boolean, curCaster: BattleDragonData | BattleAIMonsterData, curTarget: BattleDragonData | BattleAIMonsterData, curEffect: SkillEffectData, skillType: eBattleSkillType, e: Node, move_dir: string, logs: any) {
        const isINVINCIBILITY = curTarget.GetEffectTime('INVINCIBILITY') > 0;
        if(!isINVINCIBILITY) {
            curTarget.Node?.Hit();
        }
    
        if(curEffect != null) { //추후 속성대미지 추가
            var log = `${curCaster.BattleTag}_${skillType}_${curEffect.KEY}_`;
            var elementBonus = 100;
            let elementData = this.stageData.ElementTable.Get(curCaster.BaseData.ELEMENT); 
            if (elementData != null) {
                switch(curTarget.BaseData.ELEMENT) {
                    case eElementType.FIRE: {
                        elementBonus = elementData.T_FIRE;
                    } break;
                    case eElementType.WATER: {
                        elementBonus = elementData.T_WATER;
                    } break;
                    case eElementType.EARTH: {
                        elementBonus = elementData.T_EARTH;
                    } break;
                    case eElementType.WIND: {
                        elementBonus = elementData.T_WIND;
                    } break;
                    case eElementType.LIGHT: {
                        elementBonus = elementData.T_LIGHT;
                    } break;
                    case eElementType.DARK: {
                        elementBonus = elementData.T_DARK;
                    } break;
                }
            }

            const DMG = Attack(curCaster.ATK, elementBonus, 0, 0, 0, curEffect.VALUE, 0);
            const DEF_RATE = Defense(curTarget.DEF, 0, 0, 0, 0, 0);
            const Damage = !isINVINCIBILITY && HIT ? Offense(DMG, DEF_RATE) : 0;

            DamageManager.SetDamageLayer(curTarget.Node.node);
            const rnd = RandomInt(0, 100);
            if(rnd < curCaster.CRI) {
                const cri = Math.floor(Damage * (curCaster.CRI_DMG * 0.01));
                curTarget.HP -= cri;
                DamageManager.DamageByNode(this.stageData.FrontEffect, curTarget.Node.GetWorldPosition(), eDamageType.CRITICAL, cri);
            
                // if(skillType == eBattleSkillType.Normal) {
                //     e = ResourceManager.Instance.GetResource<Node>(ResourcesType.EFFECT_PREFAB)['3'];
                // }
                const temp = HIT ? 'C' : 'M';
                log += `${temp}_${curTarget.BattleTag}_${cri}`;
            } else {
                curTarget.HP -= Damage;
                DamageManager.DamageByNode(this.stageData.FrontEffect, curTarget.Node.GetWorldPosition(), curCaster.BaseData.ELEMENT, Damage);
            
                const temp = HIT ? 'N' : 'M';
                log += `${temp}_${curTarget.BattleTag}_${Damage}`;
            }


            this.CreateEffect(e, curTarget.Node.node, move_dir);

            this.stageData.SetDamage(curCaster.DragonTag, Damage);

            logs.push(log);

            if(curTarget.HP <= 0) {
                curTarget.HP = 0;
                curTarget.Node.Death();
                curTarget.Node.AnimationStart('death');
                logs.push(`${curCaster.BattleTag}_${skillType}_${curEffect.KEY}_${curTarget.BattleTag}_D_0`);
                if(this.stageData.Rewards['charData'][curTarget.BattleTag] != undefined) {
                    this.stageData.Rewards['charData'][curTarget.BattleTag]['death'] = true;
                }
            }
        }
    }

    private TickSkillDamage(HIT: boolean, curCaster: BattleDragonData | BattleAIMonsterData, curTarget: BattleDragonData | BattleAIMonsterData, curEffect: SkillEffectData, skillType: eBattleSkillType, e: Node, move_dir: string, logs: any) {
        const isINVINCIBILITY = curTarget.GetEffectTime('INVINCIBILITY') > 0;
        if(!isINVINCIBILITY) {
            curTarget.Node?.Hit();
        }
    
        if(curEffect != null) { //추후 속성대미지 추가
            var log = `${curCaster.BattleTag}_${skillType}_${curEffect.KEY}_`;
            var elementBonus = 100;
            let elementData = this.stageData.ElementTable.Get(curCaster.BaseData.ELEMENT); 
            if (elementData != null) {
                switch(curTarget.BaseData.ELEMENT) {
                    case eElementType.FIRE: {
                        elementBonus = elementData.T_FIRE;
                    } break;
                    case eElementType.WATER: {
                        elementBonus = elementData.T_WATER;
                    } break;
                    case eElementType.EARTH: {
                        elementBonus = elementData.T_EARTH;
                    } break;
                    case eElementType.WIND: {
                        elementBonus = elementData.T_WIND;
                    } break;
                    case eElementType.LIGHT: {
                        elementBonus = elementData.T_LIGHT;
                    } break;
                    case eElementType.DARK: {
                        elementBonus = elementData.T_DARK;
                    } break;
                }
            }

            const DMG = Attack(curCaster.ATK, elementBonus, 0, 0, 0, curEffect.VALUE, 0);
            const DEF_RATE = Defense(curTarget.DEF, 0, 0, 0, 0, 0);
            const Damage = !isINVINCIBILITY && HIT ? Offense(DMG, DEF_RATE) : 0;

            DamageManager.SetDamageLayer(curTarget.Node.node);
            curTarget.HP -= Damage;
            DamageManager.DamageByNode(this.stageData.FrontEffect, curTarget.Node.GetWorldPosition(), curCaster.BaseData.ELEMENT, Damage);
        
            const temp = HIT ? 'T' : 'T';
            log += `${temp}_${curTarget.BattleTag}_${Damage}`;

            this.CreateEffect(e, curTarget.Node.node, move_dir);
            this.stageData.SetDamage(curCaster.DragonTag, Damage);
            logs.push(log);

            if(curTarget.HP <= 0) {
                curTarget.HP = 0;
                curTarget.Node.Death();
                curTarget.Node.AnimationStart('death');
                logs.push(`${curCaster.BattleTag}_${skillType}_${curEffect.KEY}_${curTarget.BattleTag}_D_0`);
                if(this.stageData.Rewards['charData'][curTarget.BattleTag] != undefined) {
                    this.stageData.Rewards['charData'][curTarget.BattleTag]['death'] = true;
                }
            }
        }
    }

    private CreateEffect(e: Node, targetNode: Node, move_dir?: string, projectileData?: SkillProjectileData) {
        if(e != undefined) {
            let eNode: Node = instantiate(e);

            if(move_dir == 'left') {
                eNode.setScale(new Vec3(-eNode.scale.x * BattleEffectScale, eNode.scale.y * BattleEffectScale, eNode.scale.z * BattleEffectScale));
            } else {
                eNode.setScale(new Vec3(eNode.scale.x * BattleEffectScale, eNode.scale.y * BattleEffectScale, eNode.scale.z * BattleEffectScale));
            }

            if(projectileData == undefined) {
                this.stageData.FrontEffect.addChild(eNode);
            } else {
                switch(projectileData.EFFECT_ORDER) {
                    case 'BACK': {
                        this.stageData.BackEffect.addChild(eNode);
                    } break;
                    case 'FRONT':
                    default: {
                        this.stageData.FrontEffect.addChild(eNode);
                    } break;
                }
            }
            eNode.setWorldPosition(targetNode.worldPosition);
        }
    }

    private CheckSkill(caster: BattleDragonData | BattleAIMonsterData, move_dir: string): BattleSkill {        
        const normalSkillIng = caster.NormalSkill != null && caster.DelayNormal <= 0;
        const skill1Ing = caster.Skill1 != null && caster.DelaySkill1 <= 0;
        const skill2Ing = caster.Skill2 != null && caster.DelaySkill2 <= 0;
        const silence = caster.GetEffectTime('SILENCE') > 0;

        let skill:SkillCharData = caster.NormalSkill;
        let projectile: SkillProjectileData = caster.NormalProjectile;
        let effects: SkillEffectData[] = caster.NormalEffect;
        let skillType: eBattleSkillType = eBattleSkillType.Normal;
        if(this.playingData[move_dir] == caster) {
            if(silence) {
                delete this.playingData[move_dir];
                return new BattleSkill(skill, projectile, effects, skillType);
            }
            skillType = caster.ActiveSkillType;
            switch(caster.ActiveSkillType) {
                case eBattleSkillType.Skill1: {
                    skill = caster.Skill1;
                    projectile = caster.Skill1Projectile;
                    effects = caster.Effect1;
                } break;
                case eBattleSkillType.Skill2: {
                    skill = caster.Skill2;
                    projectile = caster.Skill2Projectile;
                    effects = caster.Effect2;
                } break;
            }
        } else if(this.playingData[move_dir] == undefined && !silence) {
            if(skill1Ing) {
                skillType = eBattleSkillType.Skill1;
                skill = caster.Skill1;
                projectile = caster.Skill1Projectile;
                effects = caster.Effect1;
            } else if(skill2Ing) {
                skillType = eBattleSkillType.Skill2;
                skill = caster.Skill2;
                projectile = caster.Skill2Projectile;
                effects = caster.Effect2;
            }
        }

        
        return new BattleSkill(skill, projectile, effects, skillType);
    }

    private RangeCheckCaster(caster: BattleDragonData | BattleAIMonsterData, casters: BattleDragonData[] | BattleAIMonsterData[], enemys: BattleDragonData[] | BattleAIMonsterData[], move_dir: string, battleSkill: BattleSkill): any {
        let data = {};
        let targetData = this.TargetCheck(caster, casters, enemys, battleSkill, move_dir);
        if(targetData != null && targetData.length > 0) {
            data['caster'] = caster;
            data['skill'] = battleSkill;
            data['targetList'] = targetData;
            data['target'] = targetData[0][1][0];
            return data;
        }

        return data;
    }

    private TargetCheck(caster: BattleDragonData | BattleAIMonsterData, casters: BattleDragonData[] | BattleAIMonsterData[], enemys: BattleDragonData[] | BattleAIMonsterData[], battleSkill: BattleSkill, move_dir:string): any[] {
        let effects: SkillEffectData[] = battleSkill.Effects;

        let targetData = [];
        let sortList = [];

        var isDragon = move_dir == 'right';
        
        for(var x = 0 ; x < effects.length ; x++) {
            let effect = effects[x];

            switch(effect.TARGET) {
                case eSkillEffectTarget.All: {
                    sortList = casters;
                    sortList.push(enemys);
                } break;
                case eSkillEffectTarget.TeamExceptMe: {
                    sortList = casters;
                    sortList = sortList.filter(element => element != caster);
                } break;
                case eSkillEffectTarget.Team: {
                    sortList = casters;
                } break;
                case eSkillEffectTarget.Enemy: {
                    sortList = enemys;
                } break;
            }
            
            sortList = this.GetDistanceList(caster, sortList);

            var dir = effect.DIRECTION;
            var position: Vec3 = null;
            switch(effect.START_POSITION) {
                case eSkillEffectStartType.Caster: {
                    position = caster.Node.GetWorldPosition(true);
                } break;
                case eSkillEffectStartType.Enemy: {
                    if(sortList.length > 0) {
                        position = sortList[0].Node.GetWorldPosition(true);
                    } else {
                        continue;
                    }
                } break;
                case eSkillEffectStartType.None:
                default: {
                    continue;
                } break;
            }

            var checkX = effect.RANGE_X * BattleTileX;
            var checkY = effect.RANGE_Y * BattleTileY;
            if(!isDragon) {
                if(dir == eSkillEffectDirectionType.Back) {
                    dir = eSkillEffectDirectionType.Front;
                }
                else if(dir == eSkillEffectDirectionType.Front) {
                    dir = eSkillEffectDirectionType.Back;
                }
            }
            let circle: Circle = new Circle(position, checkX, checkY);
            let targetList: BattleDragonData[] | BattleAIMonsterData[] = [];
            switch(dir) {
                case eSkillEffectDirectionType.Back: {
                    sortList.forEach(element => {
                        let targetPos = element.Node.GetWorldPosition(true);
                        var remain = isDragon ? targetPos.x - position.x : position.x - targetPos.x;
                        if(remain >= 0 && circle.IsContain(targetPos)) {
                            if(effect.COUNT == 0) {
                                targetList.push(element);
                            } else if(targetList.length < effect.COUNT) {
                                targetList.push(element);
                            }
                        }
                    });
                } break;
                case eSkillEffectDirectionType.Front: {
                    sortList.forEach(element => {
                        let targetPos = element.Node.GetWorldPosition(true);
                        var remain = isDragon ? targetPos.x - position.x : position.x - targetPos.x;
                        if(remain >= 0 && circle.IsContain(targetPos)) {
                            if(effect.COUNT == 0) {
                                targetList.push(element);
                            } else if(targetList.length < effect.COUNT) {
                                targetList.push(element);
                            }
                        }
                    });
                } break;
                case eSkillEffectDirectionType.All: {
                    sortList.forEach(element => {
                        let targetPos = element.Node.GetWorldPosition(true);
                        if(circle.IsContain(targetPos)) {
                            if(effect.COUNT == 0) {
                                targetList.push(element);
                            } else if(targetList.length < effect.COUNT) {
                                targetList.push(element);
                            }
                        }
                    });
                } break;
            }
            if(targetList.length > 0) {
                targetData.push([effect, targetList]);
            }
        }

        return targetData;
    }
    
    private CheckCircle(caster: BattleDragonData | BattleAIMonsterData, target: BattleDragonData | BattleAIMonsterData, RangeX, RangeY): boolean {
        let circle = new Circle(caster.Node.GetWorldPosition(true), RangeX * BattleTileX, RangeY * BattleTileY);

        return circle.IsContain(target.Node.GetWorldPosition(true));
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

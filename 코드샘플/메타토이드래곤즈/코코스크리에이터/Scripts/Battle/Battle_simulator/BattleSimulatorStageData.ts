
import { _decorator, Component, Node, instantiate, Prefab, math } from 'cc';
import { BattleAIMonster } from '../../Character/BattleAIMonster';
import { BattleAIMonsterSpine } from '../../Character/BattleAIMonsterSpine';
import { BattleDragonData, BattleAIMonsterData, BattleMonsterBaseData } from '../../Character/BattleData';
import { BattleDragonSpine } from '../../Character/BattleDragonSpine';
import { CharBaseTable, CharGradeTable } from '../../Data/CharTable';
import { ElementRateTable } from '../../Data/ElementTable';
import { MonsterSpawnData, MonsterBaseData } from '../../Data/MonsterData';
import { MonsterBaseTable, MonsterSpawnTable } from '../../Data/MonsterTable';
import { SkillCharTable, SkillEffectTable, SkillProjectileTable } from '../../Data/SkillTable';
import { StageBaseData } from '../../Data/StageData';
import { StageBaseTable } from '../../Data/StageTable';
import { StatFactorTable } from '../../Data/StatTable';
import { TableManager } from '../../Data/TableManager';
import { GameManager } from '../../GameManager';
import { ResourceManager, ResourcesType } from '../../ResourceManager';
import { RandomInt } from '../../Tools/SandboxTools';
import { UserDragon, User } from '../../User/User';
import { eBattleState } from '../Stage';
import { BattleSimulatorResourceManager } from './BattleSimulatorResourceManager';
import { BattleSimulatorUI } from './BattleSimulatorUI';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = BattleSimulatorStageData
 * DateTime = Thu Apr 14 2022 13:43:54 GMT+0900 (대한민국 표준시)
 * Author = wonjun_gs
 * FileBasename = BattleSimulatorStageData.ts
 * FileBasenameNoExtension = BattleSimulatorStageData
 * URL = db://assets/Scripts/Battle/Battle_simulator/BattleSimulatorStageData.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */

export class BattleSimulatorStageData {
    private positionX1: number = 0;
    private positionX2: number = 0;
    private positionX3: number = 0;
    private positionX4: number = 0;
    private cloudPosition: number = 0;

    private backGrounds_1: Node[] = null;
    private backGrounds_2: Node[] = null;
    private backGrounds_3: Node[] = null;
    private frontGrounds_1: Node[] = null;
    private clouds: Node[] = null;
    private characterField: Node = null;
    private backEffect: Node = null;
    public get BackEffect(): Node {
        return this.backEffect;
    }
    private frontEffect: Node = null;
    public get FrontEffect(): Node {
        return this.frontEffect;
    }

    private charaters: any[] = null;
    public get Charaters(): any[] {
        return this.charaters;
    }

    private orderNode: Node[] = null;
    public get OrderNode(): Node[] {
        return this.orderNode;
    }

    private dragons: BattleDragonData[] = null;
    public get Dragons(): BattleDragonData[] {
        return this.dragons;
    }

    private monsters: BattleAIMonsterData[] = null;
    public get Monsters(): BattleAIMonsterData[] {
        return this.monsters;
    }
    private stageData: StageBaseData = null;
    private spawnData: MonsterSpawnData[] = null;

    private speed: number = 150;
    private groundSpeed: number = 150;
    private time: number = -1;
    public get Time(): number {
        return this.time;
    }
    private battleTag: number = -1;
    public get BattleTag(): number {
        return this.battleTag;
    }
    private battleState: eBattleState = eBattleState.None;
    public get BattleState(): eBattleState {
        return this.battleState;
    }
    private curWave: number = -1;
    public get CurWave(): number {
        return this.curWave;
    }
    private stageSpeed: number = -1;
    public get StageSpeed(): number {
        return this.stageSpeed;
    }
    public set StageSpeed(value: number) {
        this.stageSpeed = value;
    }
    private maxWave: number = -1;
    public get MaxWave(): number {
        return this.maxWave;
    }
    private waveList: {} = null;
    private players = null;
    private curWaves: {} = null;
    private rewards: {} = null;
    public get Rewards(): {} {
        return this.rewards;
    }
    
    private monsterBaseTable: MonsterBaseTable = null;
    private monsterSpawnTable: MonsterSpawnTable = null;
    private dataTable: CharBaseTable = null;
    private skillTable: SkillCharTable = null;
    private effectTable: SkillEffectTable = null;
    private projectileTable: SkillProjectileTable = null;
    private statTable: StatFactorTable = null;
    public get StatTable(): StatFactorTable {
        return this.statTable;
    }
    private gradeTable: CharGradeTable = null;
    public get GradeTable(): CharGradeTable {
        return this.gradeTable;
    }
    private elementTable: ElementRateTable = null;
    public get ElementTable(): ElementRateTable {
        return this.elementTable;
    }
    private resources: BattleSimulatorResourceManager = null;
    private logs: {} = null;
    public get Logs() {
        return this.logs;
    }

    playerSimulatorHP : number = -1;
    enemySimulatorHP : number = -1;
    elapseTime : number = -1;

    Init(mapNode: Node, stageData: any): void {
        if(mapNode == null) {
            return;
        }

        this.logs = {};
        this.players = [];
        this.curWaves = {};
        this.rewards = {};

        this.resources = GameManager.GetManager<BattleSimulatorResourceManager>(BattleSimulatorResourceManager.Name);
        this.dataTable = TableManager.GetTable<CharBaseTable>(CharBaseTable.Name);
        this.skillTable = TableManager.GetTable<SkillCharTable>(SkillCharTable.Name);
        this.effectTable = TableManager.GetTable<SkillEffectTable>(SkillEffectTable.Name);
        this.projectileTable = TableManager.GetTable<SkillProjectileTable>(SkillProjectileTable.Name);
        this.monsterBaseTable = TableManager.GetTable<MonsterBaseTable>(MonsterBaseTable.Name);
        this.monsterSpawnTable = TableManager.GetTable<MonsterSpawnTable>(MonsterSpawnTable.Name);
        this.statTable = TableManager.GetTable<StatFactorTable>(StatFactorTable.Name);
        this.gradeTable = TableManager.GetTable<CharGradeTable>(CharGradeTable.Name);
        this.elementTable = TableManager.GetTable<ElementRateTable>(ElementRateTable.Name);

        if(stageData != null) {
            this.SetData(stageData['jsonData']);

            this.stageData = TableManager.GetTable<StageBaseTable>(StageBaseTable.Name).GetByWorldStage(stageData['world'], stageData['stage']);
            if(this.stageData != null) {
                this.spawnData = this.monsterSpawnTable.Get(this.stageData.SPAWN);

                if(stageData['jsonData']['lapTime'] != undefined)
                {
                    this.elapseTime = stageData['jsonData']['lapTime'];
                    if(this.elapseTime > 0){
                        this.stageData.TIME = this.elapseTime;
                    }
                }
            }
        } else {
            this.stageData = TableManager.GetTable<StageBaseTable>(StageBaseTable.Name).Get(110100101);
            if(this.stageData != null) {
                this.spawnData = this.monsterSpawnTable.Get(this.stageData.SPAWN);
            }
        }

        if(this.stageData == null || this.spawnData == null) {
            console.error('noData');
            return;
        }
        
        this.rewards['star'] = 0;
        this.rewards['world'] = this.stageData.WORLD;
        this.rewards['diff'] = this.stageData.DIFFICULT;
        this.rewards['stage'] = this.stageData.STAGE;

        this.time = this.stageData.TIME;
        this.waveList = {};
        for(var wave = 1 ;; wave++) {
            let curWave = this.spawnData.filter(element => element.WAVE == wave);
            if(curWave == null || curWave.length < 1) {
                break;
            }
            if(this.curWave < 1) {
                this.curWave = wave;
            }
            if(this.maxWave < wave) {
                this.maxWave = wave;
            }
            if(this.waveList[wave] == undefined) {
                this.waveList[wave] = {};
            }
            for(var group = 1 ;; group++) {
                let curGroup = curWave.filter(element => element.GROUP == group);
                if(curGroup == null || curGroup.length < 1) {
                    break;
                }
                
                this.waveList[wave][group] = curGroup;
            }
        }

        this.charaters = [];
        this.orderNode = [];
        this.monsters = [];
        this.dragons = [];
        this.backGrounds_1 = [];
        this.backGrounds_2 = [];
        this.backGrounds_3 = [];
        this.frontGrounds_1 = [];
        this.clouds = [];

        this.characterField = mapNode.getChildByName('Character');
        if(this.characterField != null) {
            for(var i = 0 ; i < 30 ; i++) {
                let orderNode = this.characterField.getChildByName(i.toString());
                this.orderNode.push(orderNode);
            }
        }
        let back1 = mapNode.getChildByName('BackGround1');
        if(back1 != null) {
            this.backGrounds_1.push(back1);
        }
        let back2 = mapNode.getChildByName('BackGround2');
        if(back2 != null) {
            this.backGrounds_1.push(back2);
        }
        let back3 = mapNode.getChildByName('BackGround3');
        if(back3 != null) {
            this.backGrounds_1.push(back3);
        }
        let back4 = mapNode.getChildByName('BackGround4');
        if(back4 != null) {
            this.backGrounds_2.push(back4);
        }
        let back5 = mapNode.getChildByName('BackGround5');
        if(back5 != null) {
            this.backGrounds_2.push(back5);
        }
        let back6 = mapNode.getChildByName('BackGround6');
        if(back6 != null) {
            this.backGrounds_2.push(back6);
        }
        let back7 = mapNode.getChildByName('BackGround7');
        if(back7 != null) {
            this.backGrounds_3.push(back7);
        }
        let back8 = mapNode.getChildByName('BackGround8');
        if(back8 != null) {
            this.backGrounds_3.push(back8);
        }
        let back9 = mapNode.getChildByName('BackGround9');
        if(back9 != null) {
            this.backGrounds_3.push(back9);
        }
        let front1 = mapNode.getChildByName('FrontGround1');
        if(front1 != null) {
            this.frontGrounds_1.push(front1);
        }
        let front2 = mapNode.getChildByName('FrontGround2');
        if(front2 != null) {
            this.frontGrounds_1.push(front2);
        }
        let front3 = mapNode.getChildByName('FrontGround3');
        if(front3 != null) {
            this.frontGrounds_1.push(front3);
        }

        this.frontEffect = mapNode.getChildByName('EffectFront');
        if(this.frontEffect == null) {
            this.frontEffect = front3;
        }
        this.backEffect = mapNode.getChildByName('EffectBack');
        if(this.backEffect == null) {
            this.backEffect = back3;
        }

        let bg = mapNode.getChildByName('BG');
        if(bg != null) {
            let cloud1 = bg.getChildByName('Cloud1');
            if(cloud1 != null) {
                this.clouds.push(cloud1);
            }
            let cloud2 = bg.getChildByName('Cloud2');
            if(cloud2 != null) {
                this.clouds.push(cloud2);
            }
            let cloud3 = bg.getChildByName('Cloud3');
            if(cloud3 != null) {
                this.clouds.push(cloud3);
            }
        }

        if(this.rewards['charData'] == undefined) {
            this.rewards['charData'] = {};
        }

        if(stageData['jsonData']['playerHP'] != undefined){
            this.playerSimulatorHP = Number(stageData['jsonData']['playerHP']);
        }

        if(stageData['jsonData']['enemyHP'] != undefined){
            this.enemySimulatorHP = Number(stageData['jsonData']['enemyHP']);
        }

        if(this.resources != null) {
            const playerCount = this.players.length;
            if(playerCount > 0) {
                for(var x = playerCount - 1 ; x >= 0 ; x--) {
                    let array = this.players[x];
                    const arrayCount = array.length;
                    for(var y = arrayCount - 1 ; y >= 0 ; y--) {
                        let player = array[y];

                        this.CheckAndCreateUserDragon(player,x,y,arrayCount);
                    }
                }
            } else {
                this.CreateDummyDragons();
            }
        }
    }
    CheckAndCreateUserDragon(player : any , x, y , arrayCount)
    {
        let dragonTag = player['dtag'] as number;
            let dragonData = this.GetUserDragonData(dragonTag);
            if(dragonData != null) {
                this.CreateDragon(player['dtag'], player['btag'], x, y, arrayCount, dragonData.Level, dragonData.SLevel);
                if(this.rewards['charData'][player['btag']] == undefined) {
                    this.rewards['charData'][player['btag']] = {};
                }
                    this.rewards['charData'][player['btag']]['dtag'] = player['dtag'];
                    this.rewards['charData'][player['btag']]['death'] = false;
            }
    }

    GetUserDragonData(tag : number): UserDragon
    {
        return User.Instance.DragonData.GetDragon(tag);
    }

    CreateDummyDragons() {
        for(var i = 1 ; i <= 5 ; i++) {
            this.CreateDragon(i, i, i, 1, 1);
        }
    }

    CreateDragon(dtag: number, btag: number, x: number, y: number, yMax: number, level: number = 1, skillLevel: number = 1) {
        let baseData = this.dataTable.Get(dtag);
        let dragonData = User.Instance.DragonData.GetDragon(dtag);
        let statData: { HP : number, ATK : number, DEF : number, CRI : number } = null;
        if(dragonData != null) {
            statData = dragonData.GetDragonALLStat();
        }
        
        let prefab = BattleSimulatorResourceManager.Instance.GetResourceByName(dragonData.Image);
        let clone: Node = instantiate(prefab);
        // let battleDragon = clone.addComponent(BattleDragon);
        let battleDragon = clone.addComponent(BattleDragonSpine);
        let battleDragonData = new BattleDragonData();
        if(battleDragon != null) {
            battleDragon.Init();
            battleDragon.SetSimulatorDragon(battleDragonData);
            battleDragonData.BaseData = baseData;
            battleDragonData.DragonTag = dtag;
            battleDragonData.BattleTag = btag;
            battleDragonData.LEVEL = level;
            if(battleDragonData.BaseData != null) {
                if(this.skillTable != null) {
                    battleDragonData.NormalSkill = this.skillTable.GetSkillByLevel(battleDragonData.BaseData.NORMAL_SKILL, 1);
                    if(battleDragonData.NormalSkill != null) {
                        battleDragonData.NormalProjectile = this.projectileTable.Get(battleDragonData.NormalSkill.PROJECTILE_KEY);
                        battleDragonData.NormalEffect = this.effectTable.Get(battleDragonData.NormalSkill.EFFECT_GROUP);
                    }
                    battleDragonData.Skill1 = this.skillTable.GetSkillByLevel(battleDragonData.BaseData.SKILL1, skillLevel);
                    if(battleDragonData.Skill1 != null) {
                        battleDragonData.Skill1Projectile = this.projectileTable.Get(battleDragonData.Skill1.PROJECTILE_KEY);
                        battleDragonData.Effect1 = this.effectTable.Get(battleDragonData.Skill1.EFFECT_GROUP);
                    }
                    battleDragonData.Skill2 = this.skillTable.GetSkillByLevel(battleDragonData.BaseData.SKILL2, skillLevel);
                    if(battleDragonData.Skill2 != null) {
                        battleDragonData.Skill2Projectile = this.projectileTable.Get(battleDragonData.Skill2.PROJECTILE_KEY);
                        battleDragonData.Effect2 = this.effectTable.Get(battleDragonData.Skill2.EFFECT_GROUP);
                    }
                }
                if(this.statTable != null) {
                    battleDragonData.StatData = this.statTable.Get(battleDragonData.BaseData.FACTOR);
                }
                if(this.gradeTable != null) {
                    battleDragonData.GradeData = this.gradeTable.Get(battleDragonData.BaseData.GRADE);
                }
            }
        
            clone.parent = this.characterField;
            this.dragons.unshift(battleDragonData);
            battleDragonData.Init(battleDragon, x, y, yMax);
            if(statData != null) 
            {
                if(this.playerSimulatorHP > 0){
                    battleDragonData.HP = this.playerSimulatorHP;
                    battleDragonData.MAXHP = this.playerSimulatorHP;
                }else{
                    battleDragonData.HP = statData.HP;
                    battleDragonData.MAXHP = statData.HP;
                }
                battleDragonData.ATK = statData.ATK;
                battleDragonData.DEF = statData.DEF;
                battleDragonData.CRI = statData.CRI;
            }
            this.charaters.push(battleDragonData.Node.node);
        }
    }

    SortOrderCharacter() {
        this.charaters = this.charaters.filter(element => element != null && element.active);
        this.charaters.sort((elementA, elementB) => {
            let aPos = elementA.getWorldPosition();
            let bPos = elementB.getWorldPosition();
            if(aPos == null || bPos == null) {
                return -1;
            }
            return aPos.y - bPos.y;
        })
        this.charaters.forEach((element, index) => {
            if(this.orderNode.length < index || index < 0) {
                return;
            }
            if(this.orderNode[index] == null || element == null) {
                return;
            }
            element.parent = this.orderNode[index];
        });
    }

    CurrentWaveStart(): void {
        if(this.monsters != null) {
            const mCount = this.monsters.length;
            for(var x = 0 ; x < mCount ; x++) {
                this.monsters[x].Node.node.destroy();
                delete this.monsters[x];
            }
            this.monsters = [];
        }

        let keys = Object.keys(this.curWaves['wave']);
        const keysCount = keys.length;
    
        let prefabs = ['slime_blue', 'slime_green', 'slime_pink'];

        if(keysCount > 0 && !this.curWaves['is_wave_init']) {
            const wavesCount = this.curWaves['wave'].length;
            for(var x = 0 ; x < wavesCount ; x++) {
                let array = this.curWaves['wave'][x];
                if(array == null || !Array.isArray(array)) {
                    continue;
                }
                const arrayCount = array.length;
                for(var y = 0 ; y < arrayCount ; y++) {
                    let data = array[y];

                    if(data['spawn'] != undefined) {
                        const randomPrefab = RandomInt(0, prefabs.length);
                        let targetSpawnData:MonsterSpawnData = data['spawn'];
                        let targetMonsterData:MonsterBaseData = this.monsterBaseTable.Get(targetSpawnData.MONSTER);
        
                        let prefab = this.resources.GetResource(ResourcesType.MONSTER_PREFAB)[targetMonsterData.IMAGE];
                        this.CreateMonster(x, y, arrayCount, data['btag'], targetMonsterData, targetSpawnData, prefab);
                    } else {
                        const randomPrefab = RandomInt(0, prefabs.length);
                        let targetSpawnData:MonsterSpawnData = new MonsterSpawnData();
                        targetSpawnData.IS_BOSS = data['boss'];
                        targetSpawnData.FACTOR = data['fct'];
                        targetSpawnData.GRADE = data['grd'];
                        targetSpawnData.GROUP = data['grp'];
                        targetSpawnData.LEVEL = data['lvl'];
                        targetSpawnData.MONSTER = data['mid'];
                        targetSpawnData.ELEMENT = data['ele'];
                        let targetMonsterData:MonsterBaseData = this.monsterBaseTable.Get(data['mid']);
        
                        let prefab = this.resources.GetResource(ResourcesType.MONSTER_PREFAB)[prefabs[randomPrefab]];
                        this.CreateMonster(x, y, arrayCount, data['btag'], targetMonsterData, targetSpawnData, prefab);
                    }
                }
            }

            this.curWaves['is_wave_init'] = true;
            //this.SetSiblingMonster();
        }
    }

    CreateMonster(x: number, y: number, yMax: number, btag: number, targetMonsterData: MonsterBaseData, targetSpawnData: MonsterSpawnData, randomPrefab: Prefab){
        let clone: Node = instantiate(randomPrefab);
        let battleMonster = clone.getComponent(BattleAIMonster);
        let battleAIMonsterData = new BattleAIMonsterData();
        battleAIMonsterData.SpawnData = targetSpawnData;
        battleAIMonsterData.BattleTag = btag;
        battleAIMonsterData.BaseData = new BattleMonsterBaseData();
        battleAIMonsterData.LEVEL = targetSpawnData.LEVEL;
        battleAIMonsterData.BaseData.SetBase(targetMonsterData);
        battleAIMonsterData.BaseData.SetSpawn(targetSpawnData);
        if(battleMonster != null) {
            battleMonster.Init();
            battleMonster.Set(battleAIMonsterData);
            if(this.dataTable != null) {
                if(battleAIMonsterData.BaseData != null) {
                    if(this.enemySimulatorHP > 0)
                    {
                        battleAIMonsterData.HP = this.enemySimulatorHP;
                    }else{
                        battleAIMonsterData.HP = battleAIMonsterData.BaseData.HP;
                    }
                    if(this.skillTable != null) {
                        battleAIMonsterData.NormalSkill = this.skillTable.GetKey(battleAIMonsterData.BaseData.NORMAL_SKILL);
                        if(battleAIMonsterData.NormalSkill != null) {
                            battleAIMonsterData.NormalProjectile = this.projectileTable.Get(battleAIMonsterData.NormalSkill.PROJECTILE_KEY);
                            battleAIMonsterData.NormalEffect = this.effectTable.Get(battleAIMonsterData.NormalSkill.EFFECT_GROUP);
                        }
                        battleAIMonsterData.Skill1 = this.skillTable.GetKey(battleAIMonsterData.BaseData.SKILL1);
                        if(battleAIMonsterData.Skill1 != null) {
                            battleAIMonsterData.Skill1Projectile = this.projectileTable.Get(battleAIMonsterData.Skill1.PROJECTILE_KEY);
                            battleAIMonsterData.Effect1 = this.effectTable.Get(battleAIMonsterData.Skill1.EFFECT_GROUP);
                        }
                        battleAIMonsterData.Skill2 = this.skillTable.GetKey(battleAIMonsterData.BaseData.SKILL2);
                        if(battleAIMonsterData.Skill2 != null) {
                            battleAIMonsterData.Skill2Projectile = this.projectileTable.Get(battleAIMonsterData.Skill2.PROJECTILE_KEY);
                            battleAIMonsterData.Effect2 = this.effectTable.Get(battleAIMonsterData.Skill2.EFFECT_GROUP);
                        }
                    }
                }
                battleAIMonsterData.StatData = this.statTable.Get(targetSpawnData.FACTOR);
                battleAIMonsterData.GradeData = this.gradeTable.Get(targetSpawnData.GRADE);
            }
            this.monsters.unshift(battleAIMonsterData);
            // let index = (3*x + y);
            // if(yMax == 1)//열에 하나만 있을 때 중간으로 수정 - 다음 열에 2개 이상일 경우 맨 아래로 파묻힘현상 발생
            // {
            //     let prevArray = this.curWaves['wave'][x - 1];//앞열 크기 체크(앞열, 후열에 2개 이상이면 적용)
            //     let nextArray = this.curWaves['wave'][x + 1];//후열
            //     let waveTotalCount = this.curWaves['wave'].length;

            //     if(x == 0)//전열
            //     {
            //         if(nextArray != undefined && nextArray.length >=2)
            //         {
            //             index += 1;
            //         }
            //     }
            //     else if(x == waveTotalCount - 1)//맨 마지막
            //     {

            //     }
            //     else
            //     {
            //         if(nextArray != undefined && nextArray.length >=2 && prevArray.length <= 1)
            //         {
            //             index += 1;//전열 사이에 들어가야하는데 앞으로 튀어나오는게 더 이상해서 일단 이렇게 처리
            //         }
            //     }
            // }
            // clone.parent = this.monsterField.getChildByName(index.toString());
            battleAIMonsterData.Init(battleMonster, x, y, yMax);
            if(this.enemySimulatorHP > 0)
            {
                battleAIMonsterData.HP = this.enemySimulatorHP;
            }else{
                battleAIMonsterData.HP = battleAIMonsterData.BaseData.HP;
            }
            let currentposTier = battleAIMonsterData.PosTear;
            clone.parent = this.characterField.getChildByName(currentposTier.toString());
            this.charaters.push(battleAIMonsterData.Node.node);
        }
    }

    MoveRight(dt: number) {
        const pos1 = dt * this.groundSpeed;
        this.positionX1 -= pos1;
        this.backGrounds_1.forEach(element => {
            let node = element as Node;
            let position = node.getPosition();
            position.x -= pos1;
            if(position.x < -1920) {
                position.x += 3840;
                this.positionX1 += 3840;
            }
            node.setPosition(position);
        });
        const pos2 = dt * this.groundSpeed * 0.5;
        this.positionX2 -= pos2;
        this.backGrounds_2.forEach(element => {
            let node = element as Node;
            let position = node.getPosition();
            position.x -= pos2;
            if(position.x < -1920) {
                position.x += 3840;
                this.positionX2 += 3840;
            }
            node.setPosition(position);
        });
        const pos3 = dt * this.groundSpeed * 0.25;
        this.positionX3 -= pos3;
        this.backGrounds_3.forEach(element => {
            let node = element as Node;
            let position = node.getPosition();
            position.x -= pos3;
            if(position.x < -1920) {
                position.x += 3840;
                this.positionX3 += 3840;
            }
            node.setPosition(position);
        });
        const pos4 = dt * this.groundSpeed;
        this.positionX4 -= pos4;
        this.frontGrounds_1.forEach(element => {
            let node = element as Node;
            let position = node.getPosition();
            position.x -= pos4;
            if(position.x < -1920) {
                position.x += 3840;
                this.positionX4 += 3840;
            }
            node.setPosition(position);
        });
    }

    MoveLeft(dt: number) {
        const pos1 = dt * this.groundSpeed;
        this.positionX1 += pos1;
        this.backGrounds_1.forEach(element => {
            let node = element as Node;
            let position = node.getPosition();
            position.x += pos1;
            if(position.x > 1920) {
                position.x -= 3840;
                this.positionX1 -= 3840;
            }
            node.setPosition(position);
        });
        const pos2 = dt * this.groundSpeed * 0.5;
        this.positionX2 += pos2;
        this.backGrounds_2.forEach(element => {
            let node = element as Node;
            let position = node.getPosition();
            position.x += pos2;
            if(position.x > 1920) {
                position.x -= 3840;
                this.positionX2 -= 3840;
            }
            node.setPosition(position);
        });
        const pos3 = dt * this.groundSpeed * 0.25;
        this.positionX3 += pos3;
        this.backGrounds_3.forEach(element => {
            let node = element as Node;
            let position = node.getPosition();
            position.x += pos3;
            if(position.x > 1920) {
                position.x -= 3840;
                this.positionX3 -= 3840;
            }
            node.setPosition(position);
        });
        const pos4 = dt * this.groundSpeed;
        this.positionX4 += pos4;
        this.frontGrounds_1.forEach(element => {
            let node = element as Node;
            let position = node.getPosition();
            position.x += pos4;
            if(position.x > 1920) {
                position.x -= 3840;
                this.positionX4 -= 3840;
            }
            node.setPosition(position);
        });
    }

    UpdateCharacter(dt: number) {
        this.dragons?.forEach(dragon => {
            dragon?.Node?.Update(dt);
        });
        this.monsters?.forEach(monster => {
            monster?.Node?.Update(dt);
        });
    }

    UpdateTime(dt: number) {
        this.time -= dt;
        BattleSimulatorUI.BattleTime(Math.round(this.time+0.5));
    }

    UpdateCloud(dt: number) {
        if(this.clouds == null) {
            return;
        }
        const pos = dt * this.groundSpeed * 0.05;
        this.cloudPosition -= pos;
        this.clouds.forEach(element => {
            let node = element as Node;
            let position = node.getPosition();
            position.x -= pos;
            if(position.x < -1920) {
                position.x += 3840;
                this.cloudPosition += 3840;
            }
            node.setPosition(position);
        });
    }


    NextWave(): boolean {
        if(this.curWave >= this.maxWave) {
            return false;
        }
        return true;
    }

    Delete() {
        const count = this.dragons.length;
        for(var i = 0 ; i < count ; i++) {
            delete this.dragons[i];
        }
        delete this.dragons;
        this.dragons = null;
        delete this.logs;
        this.logs = null;
    }

    SetData(jsonData: any) {
        if(jsonData['wave'] != undefined) {
            this.curWave = jsonData['wave'];
        }
        if(jsonData['tag'] != undefined) {
            this.battleTag = jsonData['tag'];
        }
        if(jsonData['state'] != undefined) {
            this.battleState = jsonData['state'];
        }
        if(jsonData['player'] != undefined) {
            let array = jsonData['player'];
            const playerCount = array.length;
            for(var i = 0 ; i < playerCount ; i++) {
                let data = [];

                let rowArray = array[i];
                const rowCount = rowArray.length;
                for(var j = 0 ; j < rowCount ; j++) {
                    data.push(rowArray[j]);
                }

                this.players.push(data);
            }
        }

        if(jsonData['enemy'] != undefined) {
            delete this.curWaves;
            this.curWaves = {};
            this.curWaves['wave'] = [];
            this.curWaves['is_wave_init'] = false;

            let enemy = jsonData['enemy'];
            const enemyCount = enemy.length;
            for(var i = 0 ; i < enemyCount ; i++) {
                let data = [];

                let rowArray = enemy[i];
                const rowCount = rowArray.length;
                for(var j = 0 ; j < rowCount ; j++) {
                    let row = {};
                    row['btag'] = rowArray[j]['btag'];
                    row['type'] = rowArray[j]['type'];
                    row['spawn'] = this.monsterSpawnTable.GetKey(rowArray[j]['id']);

                    if(rowArray[j]['mid'] > 0){
                        row['spawn'].monster = rowArray[j]['mid'];
                    }
                    data.push(row);
                }
                this.curWaves['wave'].push(data)
            }
        }

        if(this.stageData != null) {
            this.rewards['world'] = this.stageData.WORLD;
            this.rewards['diff'] = this.stageData.DIFFICULT;
            this.rewards['stage'] = this.stageData.STAGE;
            this.rewards['time'] = this.stageData.TIME - Math.floor(this.Time);
        }

        if(jsonData['star'] != undefined) {
            this.rewards['star'] = jsonData['star'];
        }

        if(jsonData['rewards'] != undefined) {
            this.rewards['rewards'] = [];
            const jsonCount = jsonData['rewards'].length;
            for(var i = 0 ; i < jsonCount ; i++) {
                let reward = jsonData['rewards'][i];
                if(reward.length >= 3) {
                    this.rewards['rewards'].push({
                        type: reward[0],
                        no: reward[1],
                        count: reward[2],
                    });
                }
            }
        }
    }

    IsWaveStart(): boolean {
        if(this.curWaves == null || this.curWaves['is_wave_init'] == undefined) {
            return true;
        }
        return !this.curWaves['is_wave_init'];
    }

    ClearField()
    {
        this.characterField.removeAllChildren();
        this.backEffect.removeAllChildren();
        this.frontEffect.removeAllChildren();
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

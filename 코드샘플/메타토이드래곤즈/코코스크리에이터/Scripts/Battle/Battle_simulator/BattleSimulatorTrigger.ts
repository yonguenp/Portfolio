
import { _decorator, Component, Node, CCInteger, CCString, Prefab, EditBox } from 'cc';
import { CharBaseTable } from '../../Data/CharTable';
import { SkillCharTable } from '../../Data/SkillTable';
import { TableManager } from '../../Data/TableManager';
import { DataManager } from '../../Tools/DataManager';
import { GetDragonStat, InfStat } from '../../Tools/SandboxTools';
import { User, UserDragon } from '../../User/User';
import { BattleSimulatorResourceManager } from './BattleSimulatorResourceManager';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = BattleSimulatorTrigger
 * DateTime = Wed Apr 13 2022 13:57:34 GMT+0900 (대한민국 표준시)
 * Author = wonjun_gs
 * FileBasename = BattleSimulatorTrigger.ts
 * FileBasenameNoExtension = BattleSimulatorTrigger
 * URL = db://assets/Scripts/Battle/Battle_simulator/BattleSimulatorTrigger.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */


@ccclass('dragonStructSet')
 export class dragonStructSet {
    @property(Number)
    btag:number;
    @property(Number)
    dtag:number;
}

@ccclass('monsterStructSet')
 export class monsterStructSet {
    @property(Number)
    btag:number;

    @property(Number)
    type : number;

    @property(Number)
    grp : number;

    @property(Number)
    id : number;

    @property(Number)
    mid : number;
}

@ccclass('dragonDataArray')
 export class dragonDataArray {
    @property(dragonStructSet)
    public positionTagList : Array<dragonStructSet> = new Array<dragonStructSet>();

    public getPostionTagList(){return this.positionTagList;}
}

@ccclass('monsterDataArray')
 export class monsterDataArray {
    @property(monsterStructSet)
    public positionTagList : Array<monsterStructSet> = new Array<monsterStructSet>();

    public getPostionTagList(){return this.positionTagList;}
}

@ccclass('monsterWaveInfo')
 export class monsterWaveInfo {
    @property(CCString)
    worldWaveName : string;

    @property(Number)
    waveIndexList : number[] =[];
}

@ccclass('monsterWaveData')
export class monsterWaveData {
    @property(monsterWaveInfo)
    public monsterWaveList : Array<monsterWaveInfo> = new Array<monsterWaveInfo>();

    public getMonsterWaveList(){return this.monsterWaveList;}
}


@ccclass('BattleSimulatorTrigger')
export class BattleSimulatorTrigger extends Component {

    @property(Prefab)
    public prefabList : Prefab[] = [];

    @property(CCInteger)
    public worldNumber : number = 1;

    @property(CCInteger)
    public stageNumber : number = 1;

    @property(dragonDataArray)
    public player : dragonDataArray[] = [];
    
    @property(monsterDataArray)
    public enemy : monsterDataArray[] = [];

    @property(monsterWaveData)
    public monsterWave : monsterWaveData[] = [];

    public get MonsterWave() {return this.monsterWave[0];}
    
    playerHP : number = 0;
    dragonLevel : number[] = [1,1,1,1,1];
    dragonsLevel : number[] = [1,1,1,1,1];
    dragonTagList : number[] =[0,0,0,0,0];

    enemyHP : number = 0;
    elapseTime : number = 0;

    selectDragonTag : number = 0;

    selectWave : string = "1-1-1";
    public get SelectWave(){return this.selectWave;}

    totalINF : number = 0;
    public get TotalINF(){return this.totalINF;}

    battleSpeed : number = 1;
    public set BattleSpeed(value : number){ this.battleSpeed= value;}

    GetPrefabList()
    {
        return this.prefabList;
    }

    setSimulatorDragonHP(param : EditBox)
    {
        let convertNumber = Number(param.string);
        let numberCheck = Number.isInteger(convertNumber);
        if(numberCheck){
            this.setDragonHP(convertNumber);
        }
        console.log('set player hp' + this.playerHP);
    }
    setDragonHP(hp : number)
    {
        this.playerHP = hp;
    }
    
    setSimulatorEnemyHP(param: EditBox)
    {
        let convertNumber = Number(param.string);
        let numberCheck = Number.isInteger(convertNumber);
        if(numberCheck){
            this.setEnemyHP(convertNumber);
        }
        console.log('set enemy hp' + this.enemyHP);
    }

    setEnemyHP(hp : number)
    {
        this.enemyHP = hp;
    }

    setSimulatorClockTime(param: EditBox)
    {
        let convertNumber = Number(param.string);
        let numberCheck = Number.isInteger(convertNumber);
        if(numberCheck){
            this.setTime(convertNumber);
        }
        console.log('set time' + this.elapseTime);
    }

    setTime(time : number)
    {
        this.elapseTime = time;
    }

    setSimulatorDragonLevel(param: EditBox , customEventData)
    {
        let checkData = JSON.parse(customEventData);
        let index = Number(checkData.index);

        let convertNumber = Number(param.string);
        let numberCheck = Number.isInteger(convertNumber);
        if(numberCheck){
            this.setDragonLevel(convertNumber, index);
        }
        console.log('set dragon Level' + this.dragonLevel);
    }

    setDragonLevel(level : number , index : number)
    {
        if(level <= 0)
        {
            level = 1;
        }
        this.dragonLevel[index] = level;

        this.CalcTotalINF();
    }

    setSimulatorDragonsLevel(param: EditBox, customEventData)
    {
        let checkData = JSON.parse(customEventData);
        let index = Number(checkData.index);

        let convertNumber = Number(param.string);
        let numberCheck = Number.isInteger(convertNumber);
        if(numberCheck){
            this.setDragonsLevel(convertNumber, index);
        }
        console.log('set dragon Level' + this.dragonLevel);
    }

    setDragonsLevel(level : number, index : number)
    {
        if(level <= 0)
        {
            level = 1;
        }
        this.dragonsLevel[index] = level;

        this.CalcTotalINF();
    }


    onClickDragonPrefabButton(dragonTag : number)
    {

    }


    //stageStartData_simulator 세팅하기
    setData()
    {
        User.Instance.Init();//user 깡통 데이터 세팅
    
        let data = {};
        data = this.CreateWorldData();
        data['jsonData'] = {};
        data['jsonData']['player'] = this.CreatePlayerData();
        data['jsonData']['enemy'] = this.CreateWaveIndex();
        data['jsonData']['state'] = 1;//state 재생하기

        data['jsonData']['playerHP'] = this.playerHP;
        data['jsonData']['enemyHP'] = this.enemyHP;
        data['jsonData']['lapTime'] = this.elapseTime;
        data['jsonData']['battleSpeed'] = this.battleSpeed;

        console.log(data);
        
        DataManager.AddData('stageStartData_simulator',data);
    }

    CreateWorldData():{}
    {
        let data = {};
        data['world'] = this.worldNumber;
        data['stage'] = this.stageNumber;
        return data;
    }

    CreatePlayerData()
    {
        let playerData = [];
        let playerPosLength = this.player.length;
        for(let i = 0 ; i< playerPosLength; i++){
            let tempData = [];
            let positionTagData = this.player[i] as dragonDataArray;
            if(positionTagData == null){
                continue;
            }
            let list = positionTagData.getPostionTagList();
            if(list == null || list.length <= 0){
                continue;
            }
            
            let dTag = 0;
            let bTag = 0;
            for(let k = 0 ; k < list.length ; k++)
            {
                dTag = list[k].dtag;
                bTag = list[k].btag;

                let dragonTagdata = {
                    btag : bTag,
                    dtag : dTag,
                }

                if(dTag == null || dTag <= 0)
                {
                    continue;
                }
                tempData.push(dragonTagdata);
                this.CreateDragon(dTag,this.dragonLevel[2*i + k],this.dragonsLevel[2*i +k]);//드래곤 레벨과 스킬레벨 세팅필요함
            }
            playerData.push(tempData);
        }

        return playerData;
    }

    SetPlayerFirstData(dragonTag : number, index : number)
    {
        switch(index)
        {
            case 0 :
            {
                this.player[0].positionTagList[0].dtag = dragonTag;
            }break;
            case 1 :
            {
                this.player[0].positionTagList[1].dtag = dragonTag;
            }break;
            case 2 :
            {
                this.player[1].positionTagList[0].dtag = dragonTag;
            }break;
            case 3 :
            {
                this.player[1].positionTagList[1].dtag = dragonTag;
            }break;
            case 4 :
            {
                this.player[2].positionTagList[0].dtag = dragonTag;
            }break;
        }
        this.dragonTagList[index] = dragonTag;

        this.CalcTotalINF();
    }

    GetCurrentDragonList()
    {
        let playerData = [];
        let playerPosLength = this.player.length;
        for(let i = 0 ; i< playerPosLength; i++){
            let tempData = [];
            let positionTagData = this.player[i] as dragonDataArray;
            if(positionTagData == null){
                continue;
            }
            let list =positionTagData.getPostionTagList();
            if(list == null || list.length <= 0){
                continue;
            }
            
            let dTag = 0;
            let bTag = 0;
            for(let k = 0 ; k < list.length ; k++)
            {
                dTag = list[k].dtag;
                bTag = list[k].btag;

                let dragonTagdata = {
                    btag : bTag,
                    dtag : dTag,
                }
                tempData.push(dragonTagdata);
            }
            playerData.push(tempData);
        }
        return playerData;
    }

    CreateDragon(dtag : number , level : number, slevel : number)//user data 에 강제로 삽입
    {
        const dragonTag = dtag;
        const dragonEXP = 0;//경험치는 레벨에 맞게 조절 (필요없을듯)
        const dragonLevel = level;
        const dragonState = 1;//normal 상태
        const dragonParts = [];//장착 장비 태그 (장비 고유번호)리스트로 들고있음
        const dragonSkillLevel = slevel;
        const dragonObtainTime = 0;//획득 시간

        let userdragonTempData = new UserDragon();
        userdragonTempData.Tag = dragonTag;
        userdragonTempData.Exp = dragonEXP;
        userdragonTempData.Level = dragonLevel;
        userdragonTempData.State = dragonState;
        userdragonTempData.Parts = dragonParts;
        //parts 기준으로 link 세팅
        userdragonTempData.Parts.forEach((element)=>{
            if(element == null){return;}
            let partTag = element;
            User.Instance.partData.SetPartLink(partTag as number, dragonTag as number);
        });

        userdragonTempData.SLevel = dragonSkillLevel;
        userdragonTempData.Obtain = dragonObtainTime;
        User.Instance.DragonData.AddUserDragon(dragonTag,userdragonTempData);
    }

    CreateEnemyData() :{}
    {
        let enemyData = [];
        let enemyLength = this.enemy.length;
        for(let i = 0 ; i< enemyLength; i++){
            let tempData = [];
            let positionTagData = this.enemy[i] as monsterDataArray;
            if(positionTagData == null){
                continue;
            }
            let list =positionTagData.getPostionTagList();
            if(list == null || list.length <= 0){
                continue;
            }
            
            let bTag = 0;
            let m_type = 0;
            let m_grp = 0;
            let m_id = 0;
            let m_tag = 0;
            for(let k = 0 ; k < list.length ; k++)
            {
                bTag = list[k].btag;
                m_type = list[k].type;
                m_grp = list[k].grp;
                m_id = list[k].id;
                m_tag = list[k].mid;

                let enemyTagdata = {
                    btag : bTag,
                    type : m_type,
                    grp : m_grp,
                    id : m_id,   
                    mid : m_tag,       
                }
                tempData.push(enemyTagdata);
            }
            enemyData.push(tempData);
        }
        return enemyData;
    }

    CreateWaveIndex() : number[]
    {
        let totalList = this.monsterWave[0].getMonsterWaveList();

        for(let i = 0 ; i< totalList.length ; i++){
            let checkWave = totalList[i].worldWaveName;

            if(checkWave == this.selectWave){
                return totalList[i].waveIndexList;
            }
        }
        return [1,2,3];
    }

    isAvailableWave(waveString : string) : boolean
    {
        let totalList = this.monsterWave[0].getMonsterWaveList();

        for(let i = 0 ; i< totalList.length ; i++){
            let checkWave = totalList[i].worldWaveName;

            if(checkWave == waveString){
                return true;
            }
        }
        return false;
    }

    SetMonsterFirstData(monsterTag : number)
    {
        this.enemy[0].positionTagList[0].mid = monsterTag;
    }

    SetCurrentWaveStage(wave : string)
    {
        this.selectWave = wave;
    }

    SetNextWave()
    {
        let currentWave = this.selectWave.charAt(this.selectWave.length - 1);
        let modifyNumber = Number(currentWave) + 1;
        if(modifyNumber > 3){
            return;
        }else{
            let tempString = this.selectWave.substring(0,this.selectWave.length - 1) + modifyNumber.toString();
            let check = this.isAvailableWave(tempString);
            if(check){
                this.selectWave = tempString;
            }
        }
    }

    public GetDragonALLStat(dragonTag : number, level : number , sLevel : number) 
    : { HP : number, ATK : number, DEF : number, CRI : number, INF : number,    //기본 스탯
        HP_ADD : number, ATK_ADD : number, DEF_ADD : number, CRI_ADD : number   //추가 스탯 
    } // 스탯 총량
    {
        let myStat : any = GetDragonStat(dragonTag as number, level);
        let baseData = TableManager.GetTable<CharBaseTable>(CharBaseTable.Name).Get(dragonTag);
        let skillData = TableManager.GetTable<SkillCharTable>(SkillCharTable.Name).GetSkillByLevel(baseData.SKILL1, sLevel)
       
        // myStat.HP += myStat.HP_ADD
        // myStat.ATK += myStat.ATK_ADD
        // myStat.DEF += myStat.DEF_ADD
        // myStat.CRI += myStat.CRI_ADD

        let skillInf = 0;
        if(skillData != null)
        {
            skillInf = skillData.INF;
        }

        myStat.INF = InfStat(myStat.HP, myStat.ATK, myStat.DEF, myStat.CRI) + skillInf //스킬 추가 계산

        return myStat 
    }

    CalcTotalINF()
    {
        let tempINF = 0;
        for(let i = 0 ; i< this.dragonTagList.length; i++)
        {
            let dragonTag = this.dragonTagList[i];
            let dragonLevel = this.dragonLevel[i];
            let dragonsLevel = this.dragonsLevel[i];

            if(dragonTag > 0)
            {
                tempINF += this.GetDragonALLStat(dragonTag, dragonLevel, dragonsLevel).INF;
            }
        }

        this.totalINF = tempINF;
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

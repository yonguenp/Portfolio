import { CharBaseData, CharExpData, CharGradeData } from "./CharData";
import { TableBase } from "./TableBase";

/**
 * Predefined variables
 * Name = CharTable
 * DateTime = Mon Feb 21 2022 15:56:35 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = CharTable.ts
 * FileBasenameNoExtension = CharTable
 * URL = db://assets/Scripts/Data/CharTable.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
export class CharBaseTable extends TableBase<CharBaseData> {
    public static Name: string = "CharBaseTable";

    public SetTable(dataArray: Array<Array<string>>): void {
        if(dataArray == null || dataArray.length < 1) {
            return;
        }
        this.DataClear();
        const rowName = dataArray[0];
        const arrayCount = dataArray.length;
        const dataCount = rowName.length;
        let table = this;

        for(var i = 1 ; i < arrayCount ; i++) {
            const datas = dataArray[i];
            
            let data = new CharBaseData();
            for (var curData = 0 ; curData < dataCount ; curData++) {
                const curRow = rowName[curData];
                const target = datas[curData];

                switch (curRow) {
                    case 'KEY':
                        data.Index = target;
                        break;
                    case 'FACTOR':
                        data.FACTOR = Number(target);
                        break;
                    case 'GRADE':
                        data.GRADE = Number(target);
                        break;
                    case 'ELEMENT':
                        data.ELEMENT = Number(target);
                        break;
                    case 'IMAGE':
                        data.IMAGE = target;
                        break;
                    case 'THUMBNAIL':
                        data.THUMBNAIL = target;
                        break;
                    case 'POSITION':
                        data.POSITION = Number(target);
                        break;
                    case '_NAME':
                        data._NAME = Number(target);
                        break;
                    case '_DESC':
                        data._DESC = Number(target);
                        break;
                    case 'ATK':
                        data.ATK = Number(target);
                        break;
                    case 'DEF':
                        data.DEF = Number(target);
                        break;
                    case 'HP':
                        data.HP = Number(target);
                        break;
                    case 'CRITICAL':
                        data.CRITICAL = Number(target);
                        break;
                    case 'CRITICAL_DMG':
                        data.CRITICAL_DMG = Number(target);
                        break;
                    case 'SPEED':
                        data.SPEED = Number(target);
                        break;
                    case 'NORMAL_SKILL':
                        data.NORMAL_SKILL = Number(target);
                        break;
                    case 'SKILL1':
                        data.SKILL1 = Number(target);
                        break;
                    case 'SKILL2':
                        data.SKILL2 = Number(target);
                        break;
                }
            }
            table.Add(data);
        }
    }
}
 
export class CharGradeTable extends TableBase<CharGradeData> {
    public static Name: string = "CharGradeTable";

    public SetTable(dataArray: Array<Array<string>>): void {
        if(dataArray == null || dataArray.length < 1) {
            return;
        }
        this.DataClear();
        const rowName = dataArray[0];
        const arrayCount = dataArray.length;
        const dataCount = rowName.length;
        let table = this;

        for(var i = 1 ; i < arrayCount ; i++) {
            const datas = dataArray[i];
            
            let data = new CharGradeData();
            for (var curData = 0 ; curData < dataCount ; curData++) {
                const curRow = rowName[curData];
                const target = datas[curData];

                switch (curRow) {
                    case 'KEY':
                        data.Index = target;
                        break;
                    case '_NAME':
                        data._NAME = Number(target);
                        break;
                    case 'STAT_POINT':
                        data.STAT_POINT = Number(target);
                        break;
                }
            }
            table.Add(data);
        }
    }
}
 
export class CharExpTable extends TableBase<CharExpData> {
    public static Name: string = "CharExpTable";

    public SetTable(dataArray: Array<Array<string>>): void {
        if(dataArray == null || dataArray.length < 1) {
            return;
        }
        this.DataClear();
        const rowName = dataArray[0];
        const arrayCount = dataArray.length;
        const dataCount = rowName.length;
        let table = this;

        for(var i = 1 ; i < arrayCount ; i++) {
            const datas = dataArray[i];
            
            let data = new CharExpData();
            for (var curData = 0 ; curData < dataCount ; curData++) {
                const curRow = rowName[curData];
                const target = datas[curData];

                switch (curRow) {
                    case 'LEVEL':
                        data.Index = target;
                        data.LEVEL = Number(target);
                        break;
                    case 'EXP':
                        data.EXP = Number(target);
                        break;
                    case 'TOTAL_EXP':
                        data.TOTAL_EXP = Number(target);
                        break;
                    case 'OPEN_EQUIP_SLOT':
                        data.OPEN_EQUIP_SLOT = Number(target);
                        break;
                }
            }
            table.Add(data);
        }
    }

/**
 * @param currentLevel //현재 레벨
 * @param currentExp // 현재 드래곤 경험치
 * @param obtainExp // 획득 총 경험치
 * @param finallevel // 결과 레벨
 * @param reduceExp // 최종 레벨 도달 이후 잔여 경험치
 */
 
    public GetLevelAddExp(currentLevel : number, currentExp : number , obtainExp : number) :any
    {
        this.GetDragonMaxLevel();
        let levelData = this.Get(currentLevel);
        let requireExp = levelData.EXP;//다음 렙업을 위한 총 경치량
        let currentLevelRequireExp = requireExp - currentExp;// 현재 레벨에서 렙업을 위한 경치량

        if(requireExp == 0)//최대값 도달
        {
            let maxLevel = this.GetDragonMaxLevel();
            let returnData ={
                finallevel  : maxLevel,
                reduceExp : this.Get(maxLevel).EXP,
            }
            return returnData;
        }

        if(obtainExp >= currentLevelRequireExp)
        {
            return this.GetLevelAddExp(currentLevel + 1 , 0, obtainExp - currentLevelRequireExp);
        }
        else{
            let returnData ={
                finallevel  : currentLevel,
                reduceExp : obtainExp,
            }
            return returnData;
        }
    }
    //다음 레벨을 올리기 위한 필요 경험치
    public GetCurrentRequireLevelExp(level : number) : number
    {
        let levelData = this.Get(level);
        return levelData.EXP;
    }
    
    //현재 레벨 총 누적 필요 경험치량
    public GetCurrentAccumulateLevelExp(level : number) :number
    {
        let levelData = this.Get(level);
        return levelData.TOTAL_EXP;
    }

    //경험치를 가지고 현재 레벨과 잔여 경험치 구하기
    public GetLevelAndExpByTotalExp(inComeTotalExp : number)
    {
        let keys = Object.keys(this.datas);

        let checkIndex = 0;
        let reduceExp = 0;
        let isSearch : boolean = false;

        keys.forEach((element)=>{
            if(isSearch){
                return;
            }
            let totalEXP = this.Get(element).TOTAL_EXP;
            let level = this.Get(element).LEVEL;
            if(totalEXP > inComeTotalExp)
            {
                checkIndex = level;
                isSearch = true;
            }
        });

        let expectLevel = checkIndex - 1;
        if(expectLevel < 0)//만렙일 경우
        {
            let returnData ={
                finallevel  : this.GetDragonMaxLevel(),
                reduceExp : 0,
            }
            return returnData;
        }
        let expectEXP = inComeTotalExp - this.Get(expectLevel).TOTAL_EXP;

        let returnData ={
            finallevel  : expectLevel,
            reduceExp : expectEXP,
        }
        return returnData;
    }

    public GetDragonMaxLevel() : number
    {
        let keys = Object.keys(this.datas);

        let numberArray = [];
        for(let i = 0 ; i< keys.length ; i++)
        {
            numberArray.push(parseInt(keys[i]));
        }
        
        numberArray.sort((a: number, b: number): number => {
            return b - a;
        });
        
        return numberArray[0];
    }

    //드래곤 최대 누적 경험치량
    public GetDragonMaxTotalExp() : number
    {
        let maxLevel = this.GetDragonMaxLevel();
        return this.GetCurrentAccumulateLevelExp(maxLevel);
    }

    //현재 레벨에 따른 해금 장비 슬롯 카운트
    public GetSlotCountByDragonLevel(dragonLevel : number)
    {
        let levelData = this.Get(dragonLevel);
        return levelData.OPEN_EQUIP_SLOT;
    }
    
    //슬롯 인덱스에 따른 해금 레벨 - 슬롯 인덱스 자체는 외부에서 제어 최소 1이상
    public GetUnLockLevelBySlotIndex(slotIndex : number) :number
    {
        let CheckList = this.GetAll().filter(Element => Element.OPEN_EQUIP_SLOT == slotIndex);
        if(CheckList == null || CheckList.length <= 0){
            return -1;
        }
        //키에 따른 오름차순 정렬 [0]가 최소 요구 레벨
        CheckList.sort((a: CharExpData, b: CharExpData): number => {
            let aParam = a.LEVEL;
            let bParam = b.LEVEL;
            return aParam - bParam;
        });

        return CheckList[0].LEVEL;
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

import { Asset, Label, Node, Sprite, SpriteFrame, Vec2, Vec3 } from "cc";
import { CharBaseData, CharGradeData } from "../Data/CharData";
import { CharBaseTable, CharGradeTable } from "../Data/CharTable";
import { ItemBaseData } from "../Data/ItemData";
import { ItemBaseTable } from "../Data/ItemTable";
import { MonsterBaseData } from "../Data/MonsterData";
import { StatFactorData } from "../Data/StatData";
import { StatFactorTable } from "../Data/StatTable";
import { TableManager } from "../Data/TableManager";
import { GameManager } from "../GameManager";
import { ResourceManager, ResourcesType } from "../ResourceManager";
import { eGoodType } from "../User/User";

export const AudioClipBGMPath = "sound/bg";
export const AudioClipFXPath = "sound/fx";

export const PrefabRootPath = "prefab";
export const PopupPrefabPath = "prefab/Popup";
export const BuildingClonePath = "prefab/Building"; //기본 프리팹 클론 모음 폴더
export const PrefabClonePath = "prefab/UIClone"; //기본 프리팹 클론 모음 폴더
export const DragonClonePath = "prefab/Dragon"; //기본 프리팹 클론 모음 폴더
export const MonsterClonePath = "prefab/Monster"; //기본 프리팹 클론 모음 폴더
export const StagePrefabPath = "prefab/stage"; //월드 프리팹 모음 폴더
export const WorldPrefabPath = "prefab/world"; //월드 프리팹 모음 폴더
export const EffectPrefabPath = "prefab/Effect"; //월드 프리팹 모음 폴더
export const TownPrefabPath = "prefab/Town";

export const ItemIconPath = "sprite/Ui/item"; //아이템 아이콘 sprite 모음 폴더
export const CharIconPath = "sprite/Ui/charactor" //캐릭터 또는 적 아이콘 모음 폴더
export const ElementIconPath = "sprite/Ui/element_icon" //속성 아이콘 모음 폴더
export const BuildingIconPath = "sprite/Ui/building"; //건물 아이콘 sprite 모음 폴더
export const SkillIconPath = "sprite/Ui/skill"; //스킬 sprite 모음 폴더
export const PartsIconPath = "sprite/Ui/part"; //장비(파츠) sprite 모음 폴더
export const DragonGradeTagIconPath = "sprite/Ui/character_grade_tag"; //드래곤 등급 태그(노말, 레어 등등)이미지 폴더
export const QuestIconPath = "sprite/Ui/quest"; //퀘스트 아이콘 sprite 모음 폴더


export function ObjectCheck(object: Object, key: PropertyKey): boolean {
    if (object == undefined) return false;
    if (Object.getOwnPropertyDescriptor(object, key) == undefined) return false;
    return true;
}

/*
    min ~ max 까지의 숫자 max 포함하지 않으며 float가 들어와도 Int만 반환.
 */
export function IsBetween(target: number, min: number, max: number): boolean {
    if (min > target || max < target) {
        return false;
    }
    return true;
}

export function RandomInt(min: number, max: number): number {
    min = Math.ceil(min);
    max = Math.floor(max);
    return Math.floor(Random(min, max)); 
}
export function RandomFloat(min: number, max: number): number {
    return Random(min, max);
}
function Random(min: number, max: number): number {
    return Math.random() * (max - min) + min;
}

export function HitCheck(attackerLevel: number, attackerHit: number, defenderLevel: number, defenderEvasion: number): boolean {
    var check = ((attackerLevel - defenderLevel) * 0.08) + ((attackerHit - defenderEvasion) * 0.1);
    if(check > 100) {
        check = 100;
    } else if (check < 0) { 
        check = 0;
    }
    const remain = check;

    const random = RandomInt(0, 100);

    if(remain > random) {
        return true;
    }
    return false;
}

export function Attack(charATK: number, charElement: number, equipATK?: number, increATK?: number, decreATK?: number, increPerATK?: number, decrePerATK?: number): number {
    var STAT_ATK = charATK;
    if(equipATK != undefined) {
        STAT_ATK += equipATK;
    }
    if(increATK != undefined) {
        STAT_ATK += increATK;
    }
    if(decreATK != undefined) {
        STAT_ATK -= decreATK;
    }

    var BUFF = 0;
    if(increPerATK != undefined) {
        BUFF += increPerATK;
    }
    if(decrePerATK != undefined) {
        BUFF -= decrePerATK;
    }

    return STAT_ATK * (1 * BUFF * 0.01 * charElement * 0.000001);
}

export function Defense(charDEF: number, equipDEF?: number, increDEF?: number, decreDEF?: number, increPerDEF?: number, decrePerDEF?: number): number {
    var STAT_DEF = charDEF;
    if(equipDEF != undefined) {
        STAT_DEF += equipDEF;
    }
    if(increDEF != undefined) {
        STAT_DEF += increDEF;
    }
    if(decreDEF != undefined) {
        STAT_DEF -= decreDEF;
    }

    var BUFF = 100;
    if(increPerDEF != undefined) {
        BUFF += increPerDEF;
    }
    if(decrePerDEF != undefined) {
        BUFF -= decrePerDEF;
    }

    const DEF = STAT_DEF * (1 * BUFF * 0.01);

    return DEF / (500 + DEF);
}

export function Offense(DMG: number, DEF_RATE: number) {
    return Math.floor(DMG - (DMG * DEF_RATE * 0.8));
}

export function GetDragonStat(tag: number, level: number): { HP:number, ATK:number, DEF:number, CRI:number, INF:number } {
    let datas = GetDragonDatas(tag);
    if(datas != null) {
        return DragonStat(level, datas.BaseData, datas.GradeData, datas.FactorData);
    } else {
        return null;
    }
}

export function DragonStat(level: number, BaseData:CharBaseData | MonsterBaseData, GradeData:CharGradeData, FactorData:StatFactorData): { HP:number, ATK:number, DEF:number, CRI:number , INF:number } {
    if(level < 1 || BaseData == null || GradeData == null || FactorData == null) {
        console.log(`dataError >>> level > ${level}, baseData > ${BaseData}, gradeData > ${GradeData}, statData > ${FactorData}`);
        return null;
    }

    let returnData: any = {};
    returnData['HP'] = Math.floor(BaseData.HP + ((level - 1) * GradeStat(GradeData.STAT_POINT, FactorData.HP) * 2));
    returnData['ATK'] = Math.floor(BaseData.ATK + ((level - 1) * GradeStat(GradeData.STAT_POINT, FactorData.ATK) * 0.1));
    returnData['DEF'] = Math.floor(BaseData.DEF + ((level - 1) * GradeStat(GradeData.STAT_POINT, FactorData.DEF) * 0.01));
    returnData['CRI'] = Math.floor((BaseData.CRITICAL + ((level - 1) * GradeStat(GradeData.STAT_POINT, FactorData.CRITICAL) * 0.001)) * 100) * 0.01;
    returnData['INF'] = InfStat(returnData['HP'], returnData['ATK'], returnData['DEF'], returnData['CRI']);

    return returnData;
}

//레벨 증가시에 따른 테이블 상승분(증가값)
export function GetIncrementDragonStat(tag : number,level : number, nextLevel : number): { HP:number, ATK:number, DEF:number, CRI:number, INF:number }
{
    let datas = GetDragonDatas(tag);
    if(datas != null) {
        let currentLevelStat = DragonStat(level, datas.BaseData, datas.GradeData, datas.FactorData);
        let nextLevelStat = DragonStat(nextLevel, datas.BaseData, datas.GradeData, datas.FactorData);

        let returnData: any = {};
        returnData['HP'] = nextLevelStat.HP - currentLevelStat.HP;
        returnData['ATK'] = nextLevelStat.ATK - currentLevelStat.ATK;
        returnData['DEF'] = nextLevelStat.DEF - currentLevelStat.DEF;
        returnData['CRI'] = nextLevelStat.CRI - currentLevelStat.CRI;
        returnData['INF'] = nextLevelStat.INF - currentLevelStat.INF;

        return returnData;
    } else {
        return null;
    }
}

export function InfStat(hp:number, atk:number, def:number, cri:number) {
    return Math.floor(hp * 0.05 + atk * 1.1 + def * 0.8 + cri * 0.5);
}

function GradeStat(GradeStat: number, Factor: number) {
    return (GradeStat * Factor) * 0.01;
}

export function GetDragonDatas(tag: number): { BaseData:CharBaseData, GradeData:CharGradeData, FactorData:StatFactorData } {
    let returnData: any = {};
    let baseData = TableManager.GetTable<CharBaseTable>(CharBaseTable.Name).Get(tag);
    returnData['BaseData'] = baseData;
    if(baseData != null) {
        returnData['GradeData'] = TableManager.GetTable<CharGradeTable>(CharGradeTable.Name).Get(baseData.GRADE);
        returnData['FactorData'] = TableManager.GetTable<StatFactorTable>(StatFactorTable.Name).Get(baseData.FACTOR);
    } else {
        return null;
    }
    return returnData;
}

export function TimeStringMinute(sec: number): string {
    const day = sec >= 86400 ? Math.floor(sec / 86400) : 0;
    sec -= day * 86400;
    const hour = sec >= 3600 ? Math.floor(sec / 3600) : 0;
    sec -= hour * 3600;
    const min = sec >= 60 ? Math.floor(sec / 60) : 0;
    sec -= min * 60;

    if(sec < 0) {
        sec = 0;
    }

    if(day > 0) {
        return `${day}D ${FillZero(2, hour)}:${FillZero(2, min)}:${FillZero(2, sec)}`;
    }

    if(hour > 0) {
        return `${FillZero(2, hour)}:${FillZero(2, min)}:${FillZero(2, sec)}`;
    }
    
    return `${FillZero(2, min)}:${FillZero(2, sec)}`
}

export function TimeString(sec: number): string {
    const day = sec >= 86400 ? Math.floor(sec / 86400) : 0;
    sec -= day * 86400;
    const hour = sec >= 3600 ? Math.floor(sec / 3600) : 0;
    sec -= hour * 3600;
    const min = sec >= 60 ? Math.floor(sec / 60) : 0;
    sec -= min * 60;

    if(sec < 0) {
        sec = 0;
    }

    if(day > 0) {
        return `${day}D ${FillZero(2, hour)}:${FillZero(2, min)}:${FillZero(2, sec)}`;
    }
    
    return `${FillZero(2, hour)}:${FillZero(2, min)}:${FillZero(2, sec)}`
}

export function FillZero(width: number, val: string | number){
    const str = String(val);
    return str.length >= width ? str:new Array(width - str.length + 1).join('0')+str;//남는 길이만큼 0으로 채움
}

export function StringBuilder(str : string, ...args) : string
{
    let count = 0;
    while(count < args.length)
    {
        str = str.replace(`{${count}}`, args[count])
        count++
    }

    return str
}

export enum Type 
{
    String = "String",
    Number = "Number",
    Boolean = "Boolean",
    Undefined = "Undefined",
    Null = "Null",
    Object = "Object",
    Array = "Array",
    RegExp = "RegExp",
    Math = "Math",
    Date = "Date",
    Function  = "Function"
}

export function GetType(target) : Type
{
    return Object.prototype.toString.call(target).slice(8, -1) as Type;
}

export function DataPriceChange(typeDefine: { type:any, type_value:number, count:number}, priceIcon: Sprite, priceLabel: Label): number {
    var stringIndex = -1;
    var iconName = "";
    switch(typeDefine.type) {
        case eGoodType.CASH:
        case eGoodType.GOLD:
        case 'CASH'://임시 위치
        case 'GOLD': {
            stringIndex = 100000004;
            iconName = 'gold';

            if(priceLabel != null) {
                const price: number = typeDefine.count;
                priceLabel.string = price.toLocaleString('ko-KR');
            }
        } break;

        case eGoodType.ENERGY:
        case 'ENERGY': {
            stringIndex = 100000003;
            iconName = 'enamel';

            if(priceLabel != null) {
                const price: number = typeDefine.count;
                priceLabel.string = price.toLocaleString('ko-KR');
            }
        } break;

        case eGoodType.TOKEN:
        case 'TOKEN': {
            stringIndex = 100000005;
            iconName = 'metalbrick';

            if(priceLabel != null) {
                const price: number = typeDefine.count;
                priceLabel.string = price.toLocaleString('ko-KR');
            }
        } break;
        
        case eGoodType.COIN:
        case 'COIN': {
            stringIndex = 100000006;
            iconName = 'brick';

            if(priceLabel != null) {
                const price: number = typeDefine.count;
                priceLabel.string = price.toLocaleString('ko-KR');
            }
        } break;
        
        case eGoodType.ITEM:
        case 'ITEM': {
            let itemData: ItemBaseData  = null;
            let table = TableManager.GetTable(ItemBaseTable.Name);
            if(table != null) {
                itemData = table.Get(typeDefine.type_value);
            }
            if(itemData == null) {
                return;
            }
            stringIndex = itemData._NAME;
            iconName = itemData.ICON;

            if(priceLabel != null) {
                const price: number = typeDefine.count;
                priceLabel.string = String(price);
            }
        } break;
    }

    let icon = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource<SpriteFrame>(ResourcesType.ITEMICON_SPRITEFRAME)[iconName];
    if(icon == undefined) {
        icon = null;
    }
    priceIcon.spriteFrame = icon;

    return stringIndex;
}

export function ChangeLayer(target: Node, layer: number) {
    if(target == null) {
        return;
    }
    target.layer = layer;
    if(target.children.length > 0) {
        target.children.forEach(element => {
            ChangeLayer(element, layer);
        });
    }
}

export type TypeConstructor<T = unknown> = new (...args: any[]) => T;
export type AssetConstructor<T = Asset> = TypeConstructor<T>;

export type BPStat =
{
    att : number,
    def : number,
    hp : number,
    cri : number
}

export function BattlePoint(bpState : BPStat) : number
{
    let attBp : number = Math.round(bpState.att * 0.8)
    let defBp : number = Math.round(bpState.def)
    let hpBp : number = Math.round(bpState.hp * 0.01)
    let criBp : number = Math.round(bpState.cri * 50)
    let totalBp : number = attBp + defBp + hpBp + criBp

    return totalBp;
}

export function GetChild(target: Node, names: string[]): Node {
    if(names == null || names.length <= 0 || target == null) {
        return null;
    }

    let targetNode: Node = null;
    names.forEach(element => {
        if(targetNode == null) {
            targetNode = target.getChildByName(element);
        } else {
            targetNode = targetNode.getChildByName(element)
        }
    });

    return targetNode;
}

export function GetPartOption(arrOptions : {key : PropertyKey, value : number}[]) : 
 { HP : number, ATK : number, DEF : number, CRI : number, 
    HP_PER : number, ATK_PER : number, DEF_PER : number, CRI_PER : number }
{
    let result : { HP : number, ATK : number, DEF : number, CRI : number, HP_PER : number, ATK_PER : number, DEF_PER : number, CRI_PER : number } = {
        HP : 0, ATK : 0, DEF : 0, CRI : 0, HP_PER : 0, ATK_PER : 0, DEF_PER : 0, CRI_PER : 0 }

    if(GetType(arrOptions) == Type.Array && arrOptions.length > 0)
    {
        for(let i = 0; i < arrOptions.length; i++)
        {
            result[arrOptions[i].key] += arrOptions[i].value
        }
    }

    return result
}

export function GatherPartOption(statA: {key : PropertyKey, value : number}, statB : {key : PropertyKey, value : number}) : 
    {HP : number, ATK : number, DEF : number, CRI : number, HP_PER : number, ATK_PER : number, DEF_PER : number, CRI_PER : number }
{
    let result : 
        {HP : number, ATK : number, DEF : number, CRI : number, HP_PER : number, ATK_PER : number, DEF_PER : number, CRI_PER : number } = 
        {HP : 0, ATK : 0, DEF : 0, CRI : 0, HP_PER : 0, ATK_PER : 0, DEF_PER : 0, CRI_PER : 0 }

    result[statA.key] += statA.value + statB.value

    return result
}

export function BezierCurve(start: number, end: number, curTime: number, maxTime: number): number {
    const normalT = curTime / maxTime;
    const normalS = 1 - normalT;
    const startS = start * normalS;
    const endT = end * normalT;
    return startS + endT;
}

export function BezierCurveVec3(start: Vec3, end: Vec3, curTime: number, maxTime: number): Vec3 {
    const normalT = curTime / maxTime;
    const normalS = 1 - normalT;
    const startS = new Vec3(start.x * normalS, start.y * normalS, start.z * normalS);
    const endT = new Vec3(end.x * normalT, end.y * normalT, end.z * normalT);
    return new Vec3(startS.x + endT.x, startS.y + endT.y, startS.z + endT.z);
}

export function BezierCurve2(start: number, wayPoint: number, end: number, curTime: number, maxTime: number): number {
    const normalT = curTime / maxTime;
    const normalS = 1 - normalT;
    const startS = Math.pow(normalS, 2) * start;
    const wayPointST = 2 * normalS * normalT * wayPoint;
    const endT = Math.pow(normalT, 2) * end;
    return startS + wayPointST + endT;
}

export function BezierCurve2Vec3(start: Vec3, wayPoint: Vec3, end: Vec3, curTime: number, maxTime: number): Vec3 {
    const normalT = curTime / maxTime;
    const normalS = 1 - normalT;
    const S = Math.pow(normalS, 2);
    const W = 2 * normalS * normalT;
    const T = Math.pow(normalT, 2);
    const startS = new Vec3(start.x * S, start.y * S, start.z * S);
    const wayPointST = new Vec3(wayPoint.x * W, wayPoint.y * W, wayPoint.z * W);
    const endT = new Vec3(end.x * T, end.y * T, end.z * T);
    return new Vec3(startS.x + wayPointST.x + endT.x, startS.y + wayPointST.y + endT.y, startS.z + wayPointST.z + endT.z);
}

export function BezierCurve3(start: number, wayPoint1: number, wayPoint2: number, end: number, curTime: number, maxTime: number): number {
    const normalT = curTime / maxTime;
    const normalS = 1 - normalT;
    const startS = Math.pow(normalS, 3) * start;
    const wayPointST = 3 * Math.pow(normalS, 2) * normalT * wayPoint1;
    const wayPointTS = 3 * normalS * Math.pow(normalT, 2) * wayPoint2;
    const endT = Math.pow(normalT, 3) * end;
    return startS + wayPointST + wayPointTS + endT;
}

export function BezierCurve3Vec2(start: Vec2, wayPoint1: Vec2, wayPoint2: Vec2, end: Vec2, curTime: number, maxTime: number): Vec2 {
    const normalT = curTime / maxTime;
    const normalS = 1 - normalT;
    const S = Math.pow(normalS, 3);
    const W1 = 3 * Math.pow(normalS, 2) * normalT;
    const W2 = 3 * Math.pow(normalT, 2) * normalS;
    const T = Math.pow(normalT, 3);
    const startS: Vec2 = new Vec2(S * start.x, S * start.y);
    const wayPointST: Vec2 = new Vec2(W1 * wayPoint1.x, W1 * wayPoint1.y);
    const wayPointTS: Vec2 = new Vec2(W2 * wayPoint2.x, W2 * wayPoint2.y);
    const endT: Vec2 = new Vec2(T * end.x, T * end.y);
    return new Vec2(startS.x + wayPointST.x + wayPointTS.x + endT.x, startS.y + wayPointST.y + wayPointTS.y + endT.y);
}

export type BezierSpeedVec = {
    way1X: number,
    way1Y: number,
    way2X: number,
    way2Y: number
}

export function BezierCurve2Speed(start: number, wayPoint: number, end: number, curTime: number, maxTime: number, bezierSpeedVec?: BezierSpeedVec): number {
    if(bezierSpeedVec == undefined) {
        bezierSpeedVec = {way1X: 0, way1Y: 0.675, way2X: 1, way2Y: 0.325};
    }
    const normalT = BezierCurve3Vec2(new Vec2(0, 0), new Vec2(bezierSpeedVec.way1X, bezierSpeedVec.way1Y), new Vec2(bezierSpeedVec.way2X, bezierSpeedVec.way2Y), new Vec2(1, 1), curTime, maxTime).y;
    const normalS = 1 - normalT;
    const startS = Math.pow(normalS, 2) * start;
    const wayPointST = 2 * normalS * normalT * wayPoint;
    const endT = Math.pow(normalT, 2) * end;
    return startS + wayPointST + endT;
}

export function numberWithCommas(x) {
    return x.toString().replace(/\B(?<!\.\d*)(?=(\d{3})+(?!\d))/g, ",");
}
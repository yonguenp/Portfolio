
import { _decorator, Node, Vec4, Vec2, Prefab } from 'cc';
import { AccountTable } from '../Data/AccountTable';
import { AreaExpansionTable, AreaLevelTable } from '../Data/AreaTable';
import { BuildingOpenTable } from '../Data/BuildingTable';
import { CharBaseData } from '../Data/CharData';
import { CharBaseTable, CharExpTable } from '../Data/CharTable';
import { ItemBaseData } from '../Data/ItemData';
import { ItemBaseTable } from '../Data/ItemTable';
import { PartBaseData } from '../Data/PartData';
import { PartOptionData } from '../Data/PartOptionData';
import { PartTable } from '../Data/PartTable';
import { SkillCharTable } from '../Data/SkillTable';
import { StringTable } from '../Data/StringTable';
import { TableManager } from '../Data/TableManager';
import { GameManager } from '../GameManager';
import { QuestManager } from '../QuestManager';
import { ResourceManager, ResourcesType } from '../ResourceManager';
import { TimeManager } from '../Time/TimeManager';
import { FillZero, GatherPartOption, GetDragonStat, GetPartOption, GetType, InfStat, ObjectCheck, RandomInt, Type } from '../Tools/SandboxTools';
import { TutorialManager } from '../Tutorial/TutorialManager';
import { PopupManager } from '../UI/Common/PopupManager';

/**
 * Predefined variables
 * Name = NewComponent
 * DateTime = Thu Jan 13 2022 14:22:32 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = NewComponent.ts
 * FileBasenameNoExtension = NewComponent
 * URL = db://assets/Scripts/User/NewComponent.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
export enum eBuildingState{
    NONE = 0,                 // ERROR
    LOCKED = 1,               // 요구조건 미비
    NOT_BUILT = 2,            // 건설 가능
    CONSTRUCTING = 3,         // 건설/레벨업 중
    CONSTRUCT_FINISHED = 4,   // 건설 완료
    NORMAL = 5,               // 배치됨
};
export enum eGoodType
{
    NONE = 0,
    GOLD = 1,
    ENERGY = 2,
    ITEM = 3,
    CHARACTER = 4,
    CASH = 5,
    MILEAGE = 6,
    ITEMGROUP = 7,
    TOKEN = 998,
    COIN = 999
}
export enum eLandmarkType
{
    COINDOZER = 101,
    WORLDTRIP = 201,
    SUBWAY = 301
}

class UserData {
    protected user_id: string = "";
    public get UserID(): string {
        return this.user_id;
    }
    protected gold: number = -1;
    public get Gold(): number {
        return this.gold;
    }
    public set Gold(value: number) {
        this.gold = value;
    }
    protected energy: number = -1;
    public get Energy(): number {
        return this.energy;
    }
    public set Energy(value: number) {
        this.energy = value;
    }
    protected energy_exp: number = -1;
    public get Energy_Exp(): number {
        return this.energy_exp;
    }
    public set Energy_Exp(value: number) {
        this.energy_exp = value;
    }
    protected exp: number = -1;
    public get Exp(): number {
        return this.exp;
    }
    public set Exp(value: number) {
        this.exp = value;
    }
    protected level: number = -1;
    public get Level(): number {
        return this.level;
    }
    public set Level(value: number) {
        this.level = value;
    }
    
    public Init() {
        this.user_id = "";
        this.gold = -1;
        this.energy = -1;
        this.exp = -1;
        this.level = -1;
    }

    public Set(jsonData: any) {
        if (ObjectCheck(jsonData, 'nick')) {
            this.user_id = jsonData['nick'];
        }
        if (ObjectCheck(jsonData, 'gold')) {
            this.Gold = jsonData['gold'];
        }
        if (ObjectCheck(jsonData, 'energy')) {
            this.Energy = jsonData['energy'];
        }
        if (ObjectCheck(jsonData, 'energy_tick')) {
            this.energy_exp = jsonData['energy_tick'];
        }
        if (ObjectCheck(jsonData, 'exp')) {
            this.Exp = jsonData['exp'];
        }
        if (ObjectCheck(jsonData, 'level')) {
            this.Level = jsonData['level'];
        }
    }
}

//유저 preference Data (각 컨텐츠별 팀 배치 + 각 팀의 팀장비 태그, 현재 스테이지 진행 내역, 최근 스테이지 등등)
export class PrefData{

    protected adventureFormation_data : AdventureFormationData = null;
    public get AdventureFormationData(): AdventureFormationData {
        return this.adventureFormation_data;
    }

    protected adventureProgress_data : AdventureProgressData = null;
    public get AdventureProgressData(): AdventureProgressData {
        return this.adventureProgress_data;
    }

    init()
    {
        if(this.adventureFormation_data == null){
            this.adventureFormation_data = new AdventureFormationData();
        }
        this.adventureFormation_data.init();

        if(this.adventureProgress_data == null){
            this.adventureProgress_data = new AdventureProgressData();
        }
        this.adventureProgress_data.init();
    }

    public Set(jsonData: any)
    {
        if(jsonData['adventure'] != null){
            this.SetAdventureFormation(jsonData['adventure']);
        }
    }

    public SetAdventureFormation(jsonData: any)//탐험 데이터 세팅 각 팀별 포메이션 및 팀 장비 세팅
    {
        this.adventureFormation_data.SetData(jsonData);
    }

    public SetAdventureWorldProgress(jsonData: any)//월드 진행 상태 저장
    {
        this.adventureProgress_data.SetData(jsonData);
    }
}

export class AdventureFormationData
{
    //팀배치 데이터
    protected team_formation :  number[][] = [[], [], []];
    public get TeamFormation(): number[][] {
        return this.team_formation;
    }

    //팀장비 데이터
    protected team_part :  number[][] = [[], [], []];
    public get TeamPart(): number[][] {
        return this.team_part;
    }

    init()
    {
        this.team_formation = [[], [], []];
        this.team_part =  [[], [], []];
    }

    SetData(jsonData: any)
    {
        let keyObj = Object.keys(jsonData);
        keyObj.forEach((key,index)=>{

            let deckArr = JSON.parse(jsonData[key]['deck']) as number[];
            this.team_formation[index] = deckArr;
            
            let itemArr = JSON.parse(jsonData[key]['items'])as number[];
            this.team_part[index] = itemArr;
        });
    }

    AllClearFormationData()
    {
        this.team_formation = [[], [], []];
    }

    ClearFomationData(tag : number)
    {
        this.team_formation[tag] = [];
    }

    SetFormationData(tag :number , dragonList : number[])
    {
        this.team_formation[tag] = dragonList;
    }
}

export class AdventureProgressData
{
    //월드 입장 가능 리스트 - stageInfo들고있음. 
    /**
     * diff : stage 난이도
     * rewarded[3] : 월드 획득 별갯수 0 미해금 1 해금 2 획득완료
     * stages[8] : 스테이지 진행 별 갯수 0은 미해금 1~3 별갯수
     * world : 월드 인덱스
     */
    protected worldList : any = {};
    public get WorldList(): any {
        return this.worldList;
    }

    //커서 (array [world, stage, mode] - 0번 인덱스 부터 순서대로 - 마지막 클리어 월드 , 스테이지, 난이도)
    protected worldCuror : number[] = [];
    public get WorldCuror(): number[] {
        return this.worldCuror;
    }

    init()
    {
        this.worldList = {};
        this.worldCuror = [];
    }

    SetData(jsonData: any)
    {
        //worlds 데이터(key - world string , value - 1)와 , cursor 데이터(array 형태) 들어옴
        if(jsonData['worlds'] != null){
            this.SetWorldData(jsonData['worlds']);
        }

        if(jsonData['cursor'] != null){
            this.SetCursorData(jsonData['cursor']);
        }
        
    }
    /**
     * key : 0101 // 01(월드인덱스) 01(난이도)
     */
    SetWorldData(jsonData: any)
    {
        this.worldList = {};
        let keys = Object.keys(jsonData);
        if(keys == null || keys.length <= 0){
            return;
        }

        keys.forEach((element)=>{
            let convertNumber = Number(element);
            let numberCheck = Number.isInteger(convertNumber);
            if(numberCheck == false){
                return;
            }

            let value = jsonData[element];
            let worldIndex = value.world;
            let diff = value.diff;
            let key = this.MakeKey(worldIndex,diff);

            this.worldList[key] = value;
        });
    }

    MakeKey(worldIndex : number , diff : number) : number
    {
        let firstKey = FillZero(2, worldIndex);
        let secondKey = FillZero(2, diff);
        
        let key = firstKey + secondKey;
        return Number(key);
    }

    SetCursorData(jsonData: any)
    {
        this.worldCuror = [];
        let CursorArr = jsonData as number[];
        if(CursorArr == null || CursorArr.length <= 0){
            return;
        }

        CursorArr.forEach(element => {
            this.worldCuror.push(element);
        });
    }

    SetCursorDataIndex(world : number , stage : number, diff : number = 1)
    {
        this.worldCuror[0] = world;
        this.worldCuror[1] = stage;
        this.worldCuror[2] = diff;
    }

    /**
     * 가장 마지막 월드 가져오기 world 인덱스 제일 큰 것으로 비교
     */
    GetLatestWorld(): number
    {
        if(this.worldList == null || this.worldList.length <= 0){
            return 1;
        }

        let keys = Object.keys(this.worldList);

        let array : number[] = [];
        
        keys.forEach((element)=>
        {
            let convertNumber = Number(element);
            let numberCheck = Number.isInteger(convertNumber);
            if(numberCheck == false){
                return;
            }
            let worldIndex = this.worldList[convertNumber].world;
            array.push(worldIndex);
        });

        //array 높은 차순 정렬 (0번 인덱스 리턴)
        array.sort(function(a,b){
            return b - a;
        })
        return array[0];
    }

    SetWorldInfoData(worldNumber : number , diff : number, info : any)
    {
        let key = this.MakeKey(worldNumber,diff);
        this.worldList[key] = info;

        // console.log(key);
        // console.log(info);
        // console.log(this.worldList);
    }

    /**
     * 월드 데이터 가져오기
     * @param worldNumber : 가져올 월드 인덱스 (ex 1 월드 1 = 1)
     * @param diff : 월드 난이도 (1 : 노멀 , 2 : 하드)
     * @returns 
     */
    GetWorldInfoData(worldNumber : number , diff : number = 1) : any
    {
        if(this.worldList == null || this.worldList.length <= 0){
            return null;
        }

        let Worldinfo : any = null;
        let keys = Object.keys(this.worldList);
        if(keys == null || keys.length <= 0){
            return null;
        }

        keys.forEach(element => {
            let worldInfo = this.worldList[element];
            if(worldInfo == null){
                return;
            }

            let worldIndex = worldInfo.world;
            let worldDiff = worldInfo.diff;
            if(worldIndex == worldNumber && worldDiff == diff){
                Worldinfo = worldInfo;
            }
        });

        return Worldinfo;
    }

    GetWorldStarReward(worldIndex : number, diff : number = 1) : number[]
    {
        let tempRewardArr : number[]= [];
        let worldInfo = this.GetWorldInfoData(worldIndex);
        if(worldInfo == null || worldInfo == undefined){
            return tempRewardArr;
        }

        tempRewardArr = worldInfo.rewarded;
        return tempRewardArr;
    }

    SetWorldStarReward(worldIndex : number , starArray : number[])
    {
        let tempRewardArr : number[]= [];
        let worldInfo = this.GetWorldInfoData(worldIndex);
        if(worldInfo == null || worldInfo == undefined){
            return;
        }

        worldInfo.rewarded = starArray;
    }

    GetWorldStages(worldIndex : number, diff : number = 1) : number[]
    {
        let tempStageArr : number[]= [];
        let worldInfo = this.GetWorldInfoData(worldIndex);
        if(worldInfo == null || worldInfo == undefined){
            return tempStageArr;
        }

        tempStageArr = worldInfo.stages;
        return tempStageArr;
    }

    GetTotalWorldInfoData()
    {
        return this.worldList;
    }

    /**
     * 입장 가능한 월드 인지 체크
     * @param worldIndex 월드 인덱스 (world1 은 1)
     * @returns 
     */
    isAvailableWorld(worldIndex : number, diff : number = 1) : boolean
    {
        if(this.worldList == null || this.worldList.length <= 0){
            return false;
        }

        let key = this.MakeKey(worldIndex , diff);

        if(this.worldList[key] == null || this.worldList[key] == undefined){
            return false;
        }
        return true;
    }

    GetTotalWorldCount()
    {
        let keys = Object.keys(this.worldList);
        return keys.length;
    }

    /**
     * 서버에 기록된 가장 마지막 전투 위치
     */
    GetCurrentLastWorldCursor(diff : number = 1)
    {
        return this.worldCuror[0];
    }
    GetCurrentLastCursor()
    {
        return this.worldCuror;
    }
    GetUserAvailableWorldAndStage()//유저가 입장 가능한 월드 및 스테이지 찾기
    {
        let worldStageList = [1,1];
        let keys = Object.keys(this.worldList);
        if(keys == null || keys.length <= 0)
        {
            return worldStageList;
        }

        let convertKeyNumbers :number[] =[];
        keys.forEach(element=>{
            convertKeyNumbers.push(Number(element));
        })

        let sortKeyList = convertKeyNumbers.sort(function(a,b){return a - b;});//키 오름차순 정렬 - 월드별 정렬 / 나중에 난이도 제거 해야함

        let worldIndex = -1;
        let stageIndex = -1;
        let findCheck : boolean = false;

        sortKeyList.forEach(element=>{
            let worldInfo = this.worldList[element];
            let stageList = worldInfo.stages;//이 또한 배열
            let worldCheck = Number((element / 100).toFixed(0));

            if(stageList == null || stageList.length <= 0){
                return;
            }

            if(findCheck){
                return;
            }

            for(let i = 0 ; i< stageList.length; i++)
            {
                let starCount = stageList[i];
                if(starCount <= 0)
                {
                    worldIndex = worldCheck;
                    stageIndex = (i+1);
                    findCheck = true;
                    break;
                }
            }
        })

        if(worldIndex < 0 || stageIndex < 0)//둘중 하나도 값을 못찾았으면(다깼단 의미), 마지막월드, 스테이지로 넘김
        {
            let lastKey = sortKeyList[sortKeyList.length -1];
            let worldInfo = this.worldList[lastKey];
            let worldCheckIndex = worldInfo.world;
            let stageLength= worldInfo.stages.length;
            
            worldStageList[0] = worldCheckIndex;
            worldStageList[1] = stageLength;
            return worldStageList;
        }else{
            worldStageList[0] = worldIndex;
            worldStageList[1] = stageIndex;
            return worldStageList;
        }
    }
}

export class ExteriorData {
    protected exterior_level: number = -1;
    public get ExteriorLevel(): number {
        return this.exterior_level;
    }
    public set ExteriorLevel(value: number) {
        this.exterior_level = value;
    }

    protected exterior_floor: number = -1;
    public get ExteriorFloor(): number {
        return this.exterior_floor;
    }
    public set ExteriorFloor(value: number) {
        this.exterior_floor = value;
    }

    protected exterior_state: eBuildingState = -1;
    public get ExteriorState(): eBuildingState {
        return this.exterior_state;
    }
    public set ExteriorState(value: eBuildingState) {
        this.exterior_state = value;
    }

    protected exterior_time: number = -1;
    public get ExteriorTime(): number {
        return this.exterior_time;
    }
    public set ExteriorTime(value: number) {
        this.exterior_time = value;
    }

    protected active_grid: {} = {};
    public get ActiveGrid(): {} {
        return this.active_grid;
    }
    public set ActiveGrid(value: {}) {
        this.active_grid = value;
    }

    protected exterior_grid: {} = {};
    public get ExteriorGrid(): {} {
        return this.exterior_grid;
    }
    public set ExteriorGrid(value: {}) {
        this.exterior_grid = value;
    }
    
    public Init() {
        this.ExteriorLevel = -1;
        this.ExteriorFloor = -1;
        this.ExteriorGrid = {};
    }

    public Set(jsonData: any) {
        if (ObjectCheck(jsonData, 'exterior') && ObjectCheck(jsonData['exterior'], 'state')) {
            this.ExteriorState = jsonData['exterior']['state'];
        }
        if (ObjectCheck(jsonData, 'state')) {
            this.ExteriorState = jsonData['state'];
        }

        if (ObjectCheck(jsonData, 'exterior') && ObjectCheck(jsonData['exterior'], 'level')) {
            this.ExteriorLevel = jsonData['exterior']['level'];
        
            if(this.ExteriorState == eBuildingState.CONSTRUCT_FINISHED) {
                this.ExteriorLevel--;
            }
        }
        if (ObjectCheck(jsonData, 'level')) {
            this.ExteriorLevel = jsonData['level'];
        
            if(this.ExteriorState == eBuildingState.CONSTRUCT_FINISHED) {
                this.ExteriorLevel--;
            }
        }

        if(ObjectCheck(jsonData, 'exterior') && ObjectCheck(jsonData['exterior'], 'floor')) {
            this.ExteriorFloor = jsonData['exterior']['floor'];
        }
        if(ObjectCheck(jsonData, 'floor')) {
            this.ExteriorFloor = jsonData['floor'];
        }

        if(ObjectCheck(jsonData, 'exterior') && ObjectCheck(jsonData['exterior'], 'construct_exp')) {
            this.ExteriorTime = jsonData['exterior']['construct_exp'];
        }
        if(ObjectCheck(jsonData, 'construct_exp')) {
            this.ExteriorTime = jsonData['construct_exp'];
        }

        this.SetGrid(jsonData);
    }

    public SetGrid(jsonData: any) {
        if (ObjectCheck(jsonData, 'exterior') && ObjectCheck(jsonData['exterior'], 'grid') || ObjectCheck(jsonData, 'grid')) {
            let grid = null
            if(ObjectCheck(jsonData['exterior'], 'grid'))
                grid = jsonData['exterior']['grid']
            else
                grid = jsonData['grid']

            const keys = Object.keys(grid);
            const count = keys.length;

            for (var i = 0 ; i < count ; i++) {
                var key = keys[i];
                let array = grid[key];
                var numKey = Number(key.replace('B', '-'));
                if(numKey > 0) {
                    numKey -= 1;
                }
                this.ActiveGrid[numKey] = {};
                this.ExteriorGrid[numKey] = {};

                const dataCount = array.length;
                for (var j = 0 ; j < dataCount ; j++) {
                    this.ExteriorGrid[numKey][j] = array[j];
                    if(array[j] == 0 || array[j] == 1) {
                        continue;
                    }
                    this.ActiveGrid[numKey][j] = array[j];
                }
            }
        }
    }
}

export class BuildingInstance {
    protected tag: number = -1;
    public get Tag(): number {
        return this.tag;
    }
    public set Tag(tag: number) {
        this.tag = tag;
    }
    protected state: eBuildingState = -1;
    public get State(): eBuildingState {
        return this.state;
    }
    public set State(state: eBuildingState) {
        this.state = state;
    }
    protected level: number = -1;
    public get Level(): number {
        return this.level;
    }
    public set Level(level: number) {
        this.level = level;
    }
    protected active_time: number = -1;
    public get ActiveTime(): number {
        return this.active_time;
    }
    public set ActiveTime(value: number) {
        this.active_time = value;
    }
}

export class InventoryItem {
    protected item_no: number = -1;
    public get ItemNo(): number {
        return this.item_no;
    }
    public set ItemNo(item_no: number) {
        this.item_no = item_no;
    }
    protected item_count: number = -1;
    public get Count(): number {
        return this.item_count;
    }
    public set Count(item_count: number) {
        this.item_count = item_count;
    }
}

export class ProducesBuilding {
    protected building_tag: number = -1;
    public get Tag(): number {
        return this.building_tag;
    }
    public set Tag(value: number) {
        this.building_tag = value;
    }
    protected slot: number = -1;
    public get Slot(): number {
        return this.slot;
    }
    public set Slot(value: number) {
        this.slot = value;
    }
    protected items: ProducesRecipe[] = null;
    public get Items(): ProducesRecipe[] {
        return this.items;
    }
    public set Items(value: ProducesRecipe[]) {
        this.items = value;
    }
}

export enum eProducesState {
    None = 0,
    Idle = 1,
    Ing = 2,
    Complete = 3,
}

export class ProducesRecipe {
    protected recipe_id: number = -1;
    public get RecipeID(): number {
        return this.recipe_id;
    }
    public set RecipeID(item_no: number) {
        this.recipe_id = item_no;
    }
    protected state: eProducesState = -1;
    public get State(): eProducesState {
        return this.state;
    }
    public set State(value: eProducesState) {
        this.state = value;
    }
    protected production_exp: number = -1;
    public get ProductionExp(): number {
        return this.production_exp;
    }
    public set ProductionExp(value: number) {
        this.production_exp = value;
    }
}

export class Landmark
{
    protected tag : number = 0
    get Tag()
    {
        return this.tag
    }

    SetData(jsonData)
    {
        if(ObjectCheck(jsonData, "tag"))
        {
            this.tag = jsonData.tag
        }
    }
}

export class LandmarkCoindozer extends Landmark
{
    protected reward : []
    get Reward() : readonly []
    {
        return this.reward
    }
    
    protected recent : number = 0
    protected recentCheck : boolean = false
    

    protected expire : number = 0
    get ExpireTime()
    {
        return this.expire
    }

    Recall() : boolean
    {
        if((TimeManager.GetTime() - this.recent > 60 || this.expire - TimeManager.GetTime() > 0 && ((this.expire - TimeManager.GetTime()) % 60) == 0) && !this.recentCheck && this.recent != TimeManager.GetTime())
        {
            this.recentCheck = true
            return true
        }
        
        return false
    }

    SetData(jsonData)
    {
        super.SetData(jsonData)
        this.recentCheck = false
        this.recent = TimeManager.GetTime()
        
        if(ObjectCheck(jsonData, "rewards"))
        {
            this.reward = jsonData.rewards
        }
        if(ObjectCheck(jsonData, "expire"))
        {
            this.expire = jsonData.expire
        }
    }
}

export enum LandmarkSubwayPlantState
{
    LOCKED = 1,
    CAN_UNLOCK,
    READY ,
    DELIVERING,
    DELIVER_COMPLETE
}

export class LandmarkSubwayPlantData
{
    id : number = 0
    state : LandmarkSubwayPlantState = LandmarkSubwayPlantState.LOCKED
    expire: number = 0
    rewards: [] = []
    slots: [] = []

    SetData(jsonData)
    {
        this.id = jsonData.id
        this.state = jsonData.state
        this.expire = ObjectCheck(jsonData, "expire") ? jsonData.expire : 0
        this.rewards = ObjectCheck(jsonData, "rewards") ? jsonData.rewards : []
        this.slots = ObjectCheck(jsonData, "slots") ? jsonData.slots : []
    }
}

export class LandmarkSubway extends Landmark
{
    protected platsData : LandmarkSubwayPlantData[] = []

    get PlatsData() : readonly LandmarkSubwayPlantData[] 
    {
        return this.platsData
    }

    SetData(jsonData)
    {
        if(this.platsData.length == 0)
            for(let i = 0; i < 3; i++)
                this.platsData.push(new LandmarkSubwayPlantData())

        super.SetData(jsonData)
        if(ObjectCheck(jsonData, "plats") && GetType(jsonData.plats) == Type.Array)
        {
            jsonData.plats.forEach((element)=>
            {
                let index = element.id-1
                this.platsData[index].SetData(element)
            })
        }
    }
}

export class LandmarkWorldtrip extends Landmark
{
    protected trip_world: number = -1;
    public get TripWorld(): number {
        return this.trip_world;
    }
    public set TripWorld(value: number) {
        this.trip_world = value;
    }

    protected trip_state: eWorldTripState = -1;
    public get TripState(): eWorldTripState {
        return this.trip_state;
    }
    public set TripState(value: eWorldTripState) {
        this.trip_state = value;
    }

    protected trip_time: number = -1;
    public get TripTime(): number {
        return this.trip_time;
    }
    public set TripTime(value: number) {
        this.trip_time = value;
    }

    protected trip_dragon: UserDragon[] = [];
    public get TripDragon(): UserDragon[] {
        return this.trip_dragon;
    }
    
    public Init() {
        this.TripWorld = -1;
        this.TripTime = -1;
        this.trip_state = eWorldTripState.Normal;
        this.trip_dragon = [];
    }

    public SetData(jsonData: any) {
        super.SetData(jsonData)
        if (ObjectCheck(jsonData, 'trip') && ObjectCheck(jsonData['trip'], 'state')) {
            this.TripState = jsonData['trip']['state'];
        }
        if (ObjectCheck(jsonData, 'state')) {
            this.TripState = jsonData['state'];
        }

        if (ObjectCheck(jsonData, 'trip') && ObjectCheck(jsonData['trip'], 'world')) {
            this.TripWorld = jsonData['trip']['world'];
        }
        if (ObjectCheck(jsonData, 'world')) {
            this.TripWorld = jsonData['world'];
        }

        if(this.TripState == eWorldTripState.Complete) {
            this.TripTime = 0;
        } else {
            if(ObjectCheck(jsonData, 'trip') && ObjectCheck(jsonData['trip'], 'expire')) {
                this.TripTime = jsonData['trip']['expire'];
            }
            if(ObjectCheck(jsonData, 'expire')) {
                this.TripTime = jsonData['expire'];
            }
        }

        this.SetDragon(jsonData);
    }

    private SetDragon(jsonData: any) {
        if(this.trip_dragon != null) {
            const tripCount = this.trip_dragon.length;
            for(var i = 0 ; i < tripCount ; i++) {
                if(this.trip_dragon[i] != null) {
                    this.trip_dragon[i].State = eDragonState.Normal;
                }
            }
        }
        this.trip_dragon = [];
        if(ObjectCheck(jsonData, 'trip') && ObjectCheck(jsonData['trip'], 'dragons')) {
            const tripCount = jsonData['trip']['dragons'].length;
            for(var i = 0 ; i < tripCount ; i++) {
                let dragonTag =  jsonData['trip']['dragons'][i];
                let dragon = User.Instance.DragonData.GetDragon(dragonTag);
                if(dragon != null) {
                    dragon.State = eDragonState.WorldTrip;
                    this.trip_dragon.push(dragon);
                }
            }
        }
        if(ObjectCheck(jsonData, 'dragons')) {
            const tripCount = jsonData['dragons'].length;
            for(var i = 0 ; i < tripCount ; i++) {
                let dragonTag =  jsonData['dragons'][i];
                let dragon = User.Instance.DragonData.GetDragon(dragonTag);
                if(dragon != null) {
                    dragon.State = eDragonState.WorldTrip;
                    this.trip_dragon.push(dragon);
                }
            }
        }
    }
}

export enum eDragonState {
    None = 0,
    Normal,
    WorldTrip
}

export class UserDragon {
    protected tag: PropertyKey = -1;
    public get Tag(): PropertyKey {
        return this.tag;
    }
    public set Tag(value: PropertyKey) {
        this.tag = value;
    }

    protected state: eDragonState = eDragonState.None;
    public get State(): eDragonState {
        return this.state;
    }
    public set State(value: eDragonState) {
        this.state = value;
    }

    protected exp: number = -1;
    public get Exp(): number{
        return this.exp;
    }
    public set Exp(value: number){
        this.exp = value;        
    }

    protected level: number = -1;
    public get Level(): number{
        return this.level;
    }
    public set Level(value: number){
        this.level = value;        
    }

    protected parts: number[] = Array(6);
    /**
     * @return 장착한 parts들의 태그 번호
     */
    public get Parts(): number[]{
        return this.parts;
    }
    public set Parts(value: number[]){
        this.parts = value;        
    }

    /**
     * 고정 길이 6
     *@return 드래곤이 장착한 파츠 리스트를 리턴
     */
    public GetPartsList() : UserPart[]
    {
        let arrParts : UserPart[] = Array<UserPart>(6)

        this.parts.forEach((element, index) => {
            arrParts[index] = User.Instance.partData.GetPart(element)
        })
        
        return arrParts
    }
    /**
     * @return 장비를 장착하기 위해서 가장 앞의 인덱스를 보냄
     */
    public GetEmptySlotIndex() : number
    {
        let tempSlotIndex = -1;
        let partList = this.GetPartsList();
        if(partList == null || partList.length <= 0){
            return tempSlotIndex;
        }

        for(let i = 0 ; i< partList.length ; i++)
        {
            let partTag = partList[i];
            if(partTag == null)
            {
                tempSlotIndex = i;
                break;
            }
        }

        if(tempSlotIndex == -1)//전부 차있다.
        {
            tempSlotIndex = this.GetCurrentSlotOpenCount();
        }

        let currentSlotMaxCount = this.GetCurrentSlotOpenCount() - 1;//해금된 최대 슬롯 인덱스
        
        if(tempSlotIndex > currentSlotMaxCount){
            tempSlotIndex = -1;//가득 차있음
        }
        return tempSlotIndex;
    }

    /**
     * @returns 현재 드래곤 레벨 기준으로 해금되는 장비 슬롯 갯수
     */
    public GetCurrentSlotOpenCount(): number
    {
        return TableManager.GetTable<CharExpTable>(CharExpTable.Name).GetSlotCountByDragonLevel(this.level);
    }

    protected slevel: number = -1;
    public get SLevel(): number{
        return this.slevel;
    }
    public set SLevel(value: number){
        this.slevel = value;        
    }
    protected obtain: number = -1;//획득 시간
    public get Obtain(): number{
        return this.obtain;
    }
    public set Obtain(value: number){
        this.obtain = value;        
    }

    protected prefab: Prefab = null;
    public get Prefab(): Prefab {
        if(this.prefab == null) {
            const image = this.GetDesignData().IMAGE;
            if(image == 'NONE') {
                this.prefab = ResourceManager.Instance.GetResource(ResourcesType.DRAGON_PREFAB)['dragon000']
            } else {
                this.prefab = ResourceManager.Instance.GetResource(ResourcesType.DRAGON_PREFAB)[this.GetDesignData().IMAGE]
            }
        }
        return this.prefab;
    }
    public set Prefab(value: Prefab) {
        this.prefab = value;
    }

    protected mapDragon: Node = null;
    public get MapDragon(): Node {
        return this.mapDragon;
    }
    public set MapDragon(value: Node) {
        this.mapDragon = value;
    }

    public GetDesignData() : CharBaseData
    {
        return TableManager.GetTable<CharBaseTable>(CharBaseTable.Name).Get(this.tag)
    }

    public get Element() : number
    {
        if(this.GetDesignData() == null)
            return -1

        return this.GetDesignData().ELEMENT
    }

    public get Image() : string
    {
        if(this.GetDesignData() == null)
            return ""

        return this.GetDesignData().IMAGE
    }

    public get Grade() : number
    {
        if(this.GetDesignData() == null)
            return -1

        return this.GetDesignData().GRADE
    }

    public get Name() : string
    {
        if(this.GetDesignData() == null)
            return ""

        return TableManager.GetTable<StringTable>(StringTable.Name).Get(this.GetDesignData()._NAME).TEXT
    }

    public get Desc() : string
    {
        if(this.GetDesignData() == null)
            return ""

        return TableManager.GetTable<StringTable>(StringTable.Name).Get(this.GetDesignData()._DESC).TEXT
    }

    public get Position() : number
    {
        if(this.GetDesignData() == null)
            return -1

        return this.GetDesignData().POSITION
    }

    public GetDragonALLStat() 
    : { HP : number, ATK : number, DEF : number, CRI : number, INF : number,    //기본 스탯
        HP_ADD : number, ATK_ADD : number, DEF_ADD : number, CRI_ADD : number   //추가 스탯 
    } // 스탯 총량
    {
        let myStat : any = GetDragonStat(this.tag as number, this.level)
        let skillData = TableManager.GetTable<SkillCharTable>(SkillCharTable.Name).GetSkillByLevel(this.GetDesignData().SKILL1, this.slevel)
        let equipedParts : UserPart[] = []

        this.parts.forEach((element, index)=>{
            if(element > 0)
                equipedParts.push(User.Instance.partData.GetPart(element))
        })
        myStat.HP_ADD = 0
        myStat.ATK_ADD = 0
        myStat.DEF_ADD = 0
        myStat.CRI_ADD = 0

        equipedParts.forEach((element)=>
        {
            if(element == null){
                return;
            }
            
            let stat = element.GetALLStat()

            //임시 : 고정 증가량은 %증가로 대체
            //퍼센트 증가량만 적용
            //고정 증가량은 논의 필요
            myStat.HP_ADD +=  Math.floor((myStat.HP   * (( 100 + stat.HP ) / 100 )) - myStat.HP);
            myStat.ATK_ADD += Math.floor((myStat.ATK   * (( 100 + stat.ATK ) / 100 )) - myStat.ATK);
            myStat.DEF_ADD += Math.floor((myStat.DEF   * (( 100 + stat.DEF ) / 100 )) - myStat.DEF);
            myStat.CRI_ADD += stat.CRI_PER
        })

        myStat.HP += myStat.HP_ADD
        myStat.ATK += myStat.ATK_ADD
        myStat.DEF += myStat.DEF_ADD
        myStat.CRI += myStat.CRI_ADD

        let skillInf = 0;
        if(skillData != null)
        {
            skillInf = skillData.INF;
        }

        myStat.INF = InfStat(myStat.HP, myStat.ATK, myStat.DEF, myStat.CRI) + skillInf //스킬 추가 계산

        return myStat 
    }
}

export class DragonData {
    protected dragons: {} = null;
    
    public Init() {
        this.dragons = {};
    }

    public AddDragonPrefab(tag: PropertyKey, prefab?: Prefab, mapDragon?: Node): UserDragon {
        let dragon = this.dragons[tag]
        if(prefab != undefined) {
            dragon.Prefab = prefab;
        }
        if(mapDragon != undefined) {
            dragon.MapDragon = mapDragon;
        }
        this.dragons[tag] = dragon;
        return dragon;
    }

    public AddUserDragon(tag : PropertyKey, dragon : UserDragon): UserDragon
    {
        this.dragons[tag] = dragon;
        return this.dragons[tag]
    }

    public GetDragon(tag: PropertyKey): UserDragon {
        if(ObjectCheck(this.dragons, tag)) {
            return this.dragons[tag] as UserDragon;
        }
        return null;
    }

    public GetRandomDragon(count: number, pickDragons?: {}): UserDragon[] {
        const keys = Object.keys(this.dragons);
        if(keys.length < count) {
            return null;
        }

        let returnDragons = [];
        let checkKeys = {};

        if(pickDragons != null && pickDragons != undefined) {
            const pickKeys = Object.keys(pickDragons);
            const pickCount = pickKeys.length;

            for(var i = 0 ; i < pickCount ; i++) {
                if(pickDragons[pickKeys[i]] == null) {
                    continue;
                }
                checkKeys[pickDragons[pickKeys[i]].Tag] = pickDragons[pickKeys[i]].Tag;
            }
        }
        const keysCount = keys.length;
        for(var i = 0 ; i < count ;) {
            var random = RandomInt(0, keysCount);
            var tag = keys[random];
            if(ObjectCheck(checkKeys, tag)) {
                i++;
                continue;
            }
            checkKeys[tag] = tag;
            if(this.dragons[tag] == null || this.dragons[tag] == undefined) {
                continue;
            }

            returnDragons.push(this.dragons[tag]);
            i++;
        }

        return returnDragons;
    }

    public GetIndexDragonSpines(start: number, end: number): Prefab[] {
        let resources = GameManager.GetManager<ResourceManager>(ResourceManager.Name);
        let returnDragons = [];
        if(resources != null) {
            for(var i = start ; i <= end ; i++) {
                let prefab:Prefab = this.GetIndexDragonSpine(i);
                if(prefab != null) {
                    returnDragons.push(prefab);
                }
            }
        }

        return returnDragons;
    }

    public GetIndexDragonSpine(index: number): Prefab {
        let resources = GameManager.GetManager<ResourceManager>(ResourceManager.Name);
        if(resources != null) {
            const nameNo = FillZero(3, index);
            return resources.GetResource(ResourcesType.DRAGON_PREFAB)[`dragon${nameNo}`];
        }

        return null;
    }

    public GetNameDragonSpine(prefabName: string): Prefab {
        let resources: ResourceManager = GameManager.GetManager<ResourceManager>(ResourceManager.Name);
        if(resources != null) {
            if(prefabName == 'NONE') {
                return resources.GetResource(ResourcesType.DRAGON_PREFAB)['dragon000'];
            }
            return resources.GetResource(ResourcesType.DRAGON_PREFAB)[prefabName];
        }

        return null;
    }
    
    public GetAllUserDragons() : UserDragon[]
    {
        let dragonDataLists = this.dragons;
        let keys = Object.keys(this.dragons)
        let array : UserDragon[] = [];
        
        keys.forEach((element)=>
        {
            array.push(dragonDataLists[element]);
        });
        return array;
    }
}

export enum eUserPartState {
    None = 0,
    UnEquip,
    Equip,
}
export class UserPart{
    protected tag: PropertyKey = -1;//itemTag - 고유번호
    public get Tag(): PropertyKey {
        return this.tag;
    }
    public set Tag(value: PropertyKey) {
        this.tag = value;
    }

    protected id: PropertyKey = -1;//itemTag - 파츠 아이템 번호 - PartTable과 PartOptionTable의 키
    public get ID(): PropertyKey {
        return this.id;
    }
    public set ID(value: PropertyKey) {
        this.id = value;
    }

    protected obtain: number = -1;//획득 시간
    public get Obtain(): number{
        return this.obtain;
    }
    public set Obtain(value: number){
        this.obtain = value;        
    }

    public belongingDragonTag : number = -1;//장착했다면 장비한 드래곤 태그
    public get BelongingDragonTag(): number {
        return this.belongingDragonTag;
    }
    public set BelongingDragonTag(value: number) {
        this.belongingDragonTag = value;
    }

    protected level: number = -1;//장비 레벨
    public get Level(): number{
        return this.level;
    }
    public set Level(value: number){
        this.level = value;        
    }

    protected partOptionList : {key : number, value : number}[] = []; //부가 옵션 리스트
    public get PartOptionList(): {}{
        return this.partOptionList;
    }
    protected partDesignData : PartBaseData = null
    protected itemDesignData : ItemBaseData = null

    public setPartOptionList(partOptionList : {key : number, value : number}[] ){
        this.partOptionList = [];

        for(let i = 0 ; i < partOptionList.length ; i++)
        {
            let optionData = partOptionList[i];
            if(optionData == null){
                continue;
            }
            let slotOptionData = {
                "key" : optionData.key,
                "value" : optionData.value,
            };

            this.partOptionList.push(slotOptionData)
        }
    }

    public GetPartDesignData() : PartBaseData
    {
        if(this.partDesignData == null)
            this.partDesignData = TableManager.GetTable<PartTable>(PartTable.Name).Get(this.id);

        return this.partDesignData;
    }

    public GetItemDesignData() : ItemBaseData
    {
        if(this.itemDesignData == null)
            this.itemDesignData = TableManager.GetTable<ItemBaseTable>(ItemBaseTable.Name).Get(this.GetPartDesignData().ITEM);
            
        return this.itemDesignData;
    }

    public get Image() : string
    {
        if(this.GetItemDesignData() == null)
            return ""

        return this.GetItemDesignData().ICON
    }

    public get Grade() : number
    {
        if(this.GetItemDesignData() == null)
            return -1

        return this.GetItemDesignData().GRADE
    }

    public get Name() : string
    {
        if(this.GetItemDesignData() == null)
            return ""

        return TableManager.GetTable<StringTable>(StringTable.Name).Get(this.GetItemDesignData()._NAME).TEXT
    }

    public get Desc() : string
    {
        if(this.GetItemDesignData() == null)
            return ""

        return TableManager.GetTable<StringTable>(StringTable.Name).Get(this.GetItemDesignData()._DESC).TEXT
    }

    public GetALLStat() : 
    {HP : number, ATK : number, DEF : number, CRI : number, HP_PER : number, ATK_PER : number, DEF_PER : number, CRI_PER : number }
    {
        let stat : 
        {HP : number, ATK : number, DEF : number, CRI : number, HP_PER : number, ATK_PER : number, DEF_PER : number, CRI_PER : number }  = 
        GetPartOption(this.partOptionList)

        stat[this.GetPartDesignData().STAT_TYPE] += this.GetValue()

        return stat
    }

    public GetValue() : number
    {
        let value : number = this.GetPartDesignData().VALUE + (this.GetPartDesignData().VALUE_GROW * this.Level)
        
        if(Number.isInteger(value))
            return value
        else
            return Number(value.toFixed(2))
    }
}

export class PartData {
    protected parts: {} = null;
    
    public Init() {
        this.parts = {};
    }
    /**
     * @param tag 고유 번호
     * @returns 
     */
    public AddUserPart(jsonData): UserPart
    {
        let newPart : UserPart = new UserPart()

        if (ObjectCheck(jsonData, 'tag')) 
        {
            newPart.Tag = jsonData['tag'];
        }

        if (ObjectCheck(jsonData, 'id')) 
        {
            newPart.ID = jsonData['id'];
        }

        if (ObjectCheck(jsonData, 'lvl')) 
        {
            newPart.Level = jsonData['lvl'];
        }
        if (ObjectCheck(jsonData, 'obtain')) 
        {
            newPart.Obtain = jsonData['obtain'];
        }

        if (ObjectCheck(jsonData, 'subs') && GetType(ObjectCheck(jsonData, 'subs')) == Type.Array) 
        {
            let partOpt : { key : number, value : number}[] = jsonData['subs']
            newPart.setPartOptionList(partOpt);
        }

        this.parts[newPart.Tag] = newPart;
        return this.parts[newPart.Tag];
    }

    public DeleteUserPart(tag : PropertyKey)
    {
        delete this.parts[tag];
    }

    /**
     * 
     * @param tag 입력 형식이 기본 타입, 숫자 태그 배열로 전달 가능
     * @returns 검색 결과에 만족하는 형식으로 리턴 (단일 또는 배열)
     */
    public GetPart(tag : PropertyKey ): UserPart
    {
        if(ObjectCheck(this.parts, Number(tag))) {
            return this.parts[Number(tag)] as UserPart;
        }
        return null;
    }
    //id로 동일 종류 아이템 탐색
    public GetPartListByID(id : PropertyKey): UserPart[] {

        let checkID = Number(id);
        let partsDataList = this.parts;
        let keys = Object.keys(this.parts)
        let array : UserPart[] = [];
        
        keys.forEach((element)=>
        {
            let userPart = partsDataList[element] as UserPart;
            if(userPart == null){
                return;
            }
            let userID = userPart.ID as number;
            if(userID == checkID){
                array.push(userPart);
            }
        });
        return array;
    }
    
    public GetAllUserParts() : UserPart[]
    {
        let partsDataList = this.parts;
        let keys = Object.keys(this.parts)
        let array : UserPart[] = [];
        
        keys.forEach((element)=>
        {
            array.push(partsDataList[element]);
        });
        return array;
    }

    public SetPartLink(partTag, dragonTag)
    {
        if(this.parts[partTag] == null)
        {
            //태그 매치가 이루어지지 않음 = 심각한 문제
            console.log("[심각] 태그매치 불가능")
            return
        }   
        (this.parts[partTag] as UserPart).belongingDragonTag = dragonTag
    }

    //현재 파츠 태그로 링크된 드래곤 태그값 가져옴
    public GetPartLink(partTag : number) : number
    {
        return (this.parts[partTag] as UserPart).belongingDragonTag;
    }
}

export enum eWorldTripState {
    None = 0,
    Normal,
    Trip,
    Complete
}

export class User {
    protected static instance: User = null;
    protected uno: number = -1;
    public get UNO(): number {
        return this.uno;
    }
    protected user_id: string = "";
    protected user_data: UserData = null;
    public get UserData(): UserData {
        return this.user_data;
    }

    protected pref_data : PrefData = null;
    public get PrefData() : PrefData{
        return this.pref_data;
    }

    protected exterior_data: ExteriorData = null;
    public get ExteriorData(): ExteriorData {
        return this.exterior_data;
    }
    protected dragon_data: DragonData = null;
    public get DragonData(): DragonData {
        return this.dragon_data;
    }

    protected part_data: PartData = null;
    public get partData(): PartData {
        return this.part_data;
    }

    protected landmark_data : {} = {};
    public GetLandmarkData(type : eLandmarkType) : LandmarkCoindozer | LandmarkSubway | LandmarkWorldtrip
    {
        switch(type)
        {
            case eLandmarkType.COINDOZER:
                return this.landmark_data[type] as LandmarkCoindozer;
            case eLandmarkType.WORLDTRIP:
                return this.landmark_data[type] as LandmarkWorldtrip;
            case eLandmarkType.SUBWAY:
                return this.landmark_data[type] as LandmarkSubway;
        }
    }
    protected areaLvTable: AreaLevelTable = null;
    protected areaExTable: AreaExpansionTable = null;
    protected buildingOpenTable: BuildingOpenTable = null;
    protected curData: {} = null;
    protected produces: {} = null;
    public static get Instance() {
        if (this.instance == null) {
            this.instance = new User();
        }
        return this.instance;
    }

    public get GOLD(): number {
        return this.user_data.Gold;
    }

    public set GOLD(value: number) {
        this.user_data.Gold = value;
    }

    //inventory
    protected inven_step: number = -1;
    public get InvenStep(): number {
        return this.inven_step;
    }
    public set InvenStep(value : number) {
        this.inven_step = value;
    }
    protected items: InventoryItem[] = null;
    protected buildings: BuildingInstance[] = null;
    //

    public Init() {
        this.uno = -1;
        this.user_id = "";
        if(this.user_data == null) {
            this.user_data = new UserData();
        }
        this.user_data.Init();

        if(this.exterior_data == null) {
            this.exterior_data = new ExteriorData();
        }
        this.exterior_data.Init();
        
        if(this.dragon_data == null) {
            this.dragon_data = new DragonData();
        }
        this.dragon_data.Init();

        if(this.part_data == null){
            this.part_data = new PartData();
        }
        this.part_data.Init();


        if(this.pref_data == null){
            this.pref_data = new PrefData();
        }
        this.pref_data.init();

        //inventory
        this.inven_step = -1;
        this.items = [];
        this.buildings = [];
        //

        this.areaLvTable = TableManager.GetTable(AreaLevelTable.Name);
        this.areaExTable = TableManager.GetTable(AreaExpansionTable.Name);
        this.buildingOpenTable = TableManager.GetTable(BuildingOpenTable.Name);
    }

    public SetData() {
        this.curData = {};
        this.curData['areaLevel'] = this.exterior_data.ExteriorLevel;

        //AreaData
        const areaKey = 10000000 + this.curData['areaLevel'];
        const areaLvData = this.areaLvTable.Get(areaKey);
        if(areaLvData != null) {
            this.curData['areaExLevel'] = areaLvData.EXPANSION_AREA;
            const areaExData = this.areaExTable.GetBetweenFloor(this.curData['areaExLevel'], this.curData['areaLevel']);
    
            this.curData['width'] = areaLvData.WIDTH;
            if (areaExData != undefined) {
                this.curData['minFloor'] = areaExData.x;
                this.curData['maxFloor'] =  areaExData.y;
                this.curData['areaOpenMinLevel'] = areaExData.z;
                this.curData['areaOpenMaxLevel'] = this.exterior_data.ExteriorFloor - 1;
            }
        }

        //BuildingData
        this.curData['building'] = {};
        const buildingDatas = this.buildingOpenTable.Get(this.curData['areaLevel']);
        if(buildingDatas != null) {
            buildingDatas.forEach(building => {
                if(this.curData['building'][building.BUILDING] == undefined) {
                    this.curData['building'][building.BUILDING] = [];
                }

                this.curData['building'][building.BUILDING].push(building);
            });
        }
    }

    public SetBase(jsonData: any) {
        if (ObjectCheck(jsonData, 'id')) {
            this.user_id = jsonData['id'];
        }
        if (ObjectCheck(jsonData, 'uno')) {
            this.uno = jsonData['uno'];
        }

        if (ObjectCheck(jsonData, 'user_base')) {
            this.user_data.Set(jsonData['user_base']);
        }

        //inventory
        if (ObjectCheck(jsonData, 'inven_step')) {
            this.inven_step = jsonData['inven_step'];
        }

        if (ObjectCheck(jsonData, 'items')) {
            const items: any[] = jsonData['items'];

            items.forEach(element => {
                if(element.length == 2) {
                    var item = new InventoryItem();
                    item.ItemNo = element[0];
                    item.Count = element[1];
                    this.items.push(item);
                }
            });
        }

        if (ObjectCheck(jsonData, 'buildings')) {
            const arrays: any[] = jsonData['buildings'];

            arrays.forEach(element => {
                if (ObjectCheck(element, 'tag')) {
                    var building = new BuildingInstance();
                    building.Tag = element['tag'];
                    
                    if (ObjectCheck(element, 'state')) {
                        building.State = element['state'];
                    }
                    
                    if (ObjectCheck(element, 'level')) {
                        building.Level = element['level'];
                    }
                    
                    if (ObjectCheck(element, 'construct_exp')) {
                        building.ActiveTime = element['construct_exp'];
                    }

                    this.buildings.push(building);
                }
            });
        }

        if (this.exterior_data != null) {
            this.exterior_data.Set(jsonData);

            console.log(this.exterior_data)
        }
        
        this.produces = {};
        if (ObjectCheck(jsonData, 'produces')) {
            const arrays: any[] = jsonData['produces'];

            arrays.forEach(element => {
                this.UpdateProduces(element);
            });
        }

        this.landmark_data = {}
        this.landmark_data[eLandmarkType.COINDOZER] = new LandmarkCoindozer();
        this.landmark_data[eLandmarkType.WORLDTRIP] = new LandmarkWorldtrip();
        this.landmark_data[eLandmarkType.WORLDTRIP].Init();
        this.landmark_data[eLandmarkType.SUBWAY] = new LandmarkSubway();

        if (ObjectCheck(jsonData, 'landmarks')) {
            const arrays: any[] = jsonData['landmarks'];

            arrays.forEach((element) => {
                switch(element.tag)
                {
                    case eLandmarkType.COINDOZER:
                        (this.landmark_data[element.tag] as LandmarkCoindozer).SetData(element)
                    break;
                    case eLandmarkType.WORLDTRIP:
                        (this.landmark_data[element.tag] as LandmarkWorldtrip).SetData(element)
                    break;
                    case eLandmarkType.SUBWAY:
                        (this.landmark_data[element.tag] as LandmarkSubway).SetData(element)
                    break;
                }  
            });
        }

        if (ObjectCheck(jsonData, 'parts')) {
            const arrays: any[] = jsonData['parts'];

            //console.log("jsonData['parts']", jsonData['parts'])

            arrays.forEach((element) => 
            {
                this.partData.AddUserPart(element);
            });
        }

        if (ObjectCheck(jsonData, 'dragons')) {
            const arrays: any[] = jsonData['dragons'];

            console.log("jsonData['dragons']", jsonData['dragons'])

            arrays.forEach((element) => 
            {
                if (ObjectCheck(element, 'id')) 
                {
                    let newDragon : UserDragon = new UserDragon()
                    newDragon.Tag = element['id'];
                    
                    if (ObjectCheck(element, 'exp')) 
                    {
                        newDragon.Exp = element['exp'];
                    }
                    
                    if (ObjectCheck(element, 'lvl')) 
                    {
                        newDragon.Level = element['lvl'];
                    }
                    
                    if (ObjectCheck(element, 'state')) 
                    {
                        newDragon.State = element['state'];
                    }

                    if (ObjectCheck(element, 'slvl')) 
                    {
                        newDragon.SLevel = element['slvl'];
                    }
                    
                    if (ObjectCheck(element, 'parts')) 
                    {
                        newDragon.Parts = element['parts'];
                        newDragon.Parts.forEach((element)=>{
                            if(element > 0)
                                this.partData.SetPartLink(element, newDragon.Tag)
                        })
                    }

                    this.dragon_data.AddUserDragon(newDragon.Tag, newDragon);
                }  
            }); 
        }
        //유저 기본 데이터
        if (ObjectCheck(jsonData, 'pref')) {
            if(jsonData['pref'] != null){
                this.pref_data.Set(jsonData['pref']);
            }
        }

        //월드 진행 데이터 - 일단 prefs에 진행 내역 넣어둠
        if (ObjectCheck(jsonData, 'adventure')) {
            if(jsonData['adventure'] != null){
                this.pref_data.SetAdventureWorldProgress(jsonData['adventure']);
            }
        }

        if (ObjectCheck(jsonData, 'tutorial')) {
            if(GetType(jsonData['tutorial']) == Type.Array)
            {
                TutorialManager.achiveList = jsonData['tutorial'];
            }
        }

        if(ObjectCheck(jsonData, 'quest'))
        {
            QuestManager.SetData(jsonData['quest']);
        }
        // this.pref_data.Set({
        //     "adventure":{"team_1":{"deck":[1,2,3,4,5], "items":[1,2,3]}}
        //     });
    }

    public UpdateItem(itemNo: number, itemCount: number) {
        let index = this.items.findIndex(value => value.ItemNo == itemNo)
        if(index < 0)
        {
            let newItem = new InventoryItem()
            newItem.ItemNo = itemNo
            newItem.Count = itemCount
            this.items.push(newItem)
            return
        }
            
        this.items[index].Count = itemCount
    }

    public UpdateProduces(obj: any) {
        if (ObjectCheck(obj, 'tag') && ObjectCheck(obj, 'slots')) {
            const tag = obj['tag'];
            const slot = obj['slots'];
            let items = undefined;
            if (ObjectCheck(obj, 'items')) {
                items = obj['items'];
            }
            let building: ProducesBuilding = null;
            if(this.produces == null) {
                this.produces = {};
            }
            if(this.produces[tag] == undefined) {
                building = new ProducesBuilding();
                this.produces[tag] = building;
            } else {
                building = this.produces[tag];
            }
            
            building.Tag = tag;
            building.Slot = slot;
            building.Items = [];

            if(1001 <= tag && tag <= 1010)
            {
                    if(items != undefined) {
                    const itemCount = items.length;
            
                    let Item = new ProducesRecipe();
                    Item.RecipeID = items[0];
                    Item.ProductionExp = items[1];
                    building.Items.push(Item);
                }
            }
            else
            {
                if(items != undefined) {
                    const itemCount = items.length;
            
                    for(var i = 0 ; i < itemCount ; i++) {
                        const array: any = items[i];
                        const arrayCount: number = array.length;
                        if(array.length < 2) {
                            continue;
                        }
                        const itemNo = array[0];
                        const itemCount = array[1];
                        const itemTime = arrayCount > 2 ? array[2] : 0;
            
                        let Item = new ProducesRecipe();
                        Item.RecipeID = itemNo;
                        Item.State = itemCount;
                        Item.ProductionExp = itemTime;
                        building.Items.push(Item);
                    }
                }
            }
        }
    }

    public UpdateBuilding(tag: number, state: number, level: number, construct_exp?: number) {
        var isNew = true;
        this.buildings.forEach(element => {
            if(element.Tag == tag) {
                element.State = state;
                element.Level = level;
                if (construct_exp != undefined) {
                    element.ActiveTime = construct_exp;
                }
                isNew = false;
            }
        });

        if(isNew) {
            let building = new BuildingInstance();
            building.Tag = tag;
            building.State = state;
            building.Level = level;
            if (construct_exp != undefined) {
                building.ActiveTime = construct_exp;
            }
            this.buildings.push(building);
        }
    }

    public UpdateGrid(jsonData) {
        if (this.exterior_data != null) {
            this.exterior_data.Set(jsonData);
            PopupManager.ForceUpdate();
        }
    }

    public UpdateLandmark(jsonData) 
    {
        jsonData.data.forEach((element) =>
        {
            if(ObjectCheck(element, "tag"))
            {
                switch(element.tag)
                {
                    case eLandmarkType.COINDOZER :
                        (this.landmark_data[eLandmarkType.COINDOZER] as LandmarkSubway).SetData(element)
                    break;
                    case eLandmarkType.WORLDTRIP :
                        (this.landmark_data[eLandmarkType.WORLDTRIP] as LandmarkWorldtrip).SetData(element)
                    break;
                    case eLandmarkType.SUBWAY :
                        (this.landmark_data[eLandmarkType.SUBWAY] as LandmarkSubway).SetData(element)
                    break;
                }
            }
        })
    }

    public GetAllItems(): InventoryItem[] {
        return this.items;
    }

    public GetProduces(buildingTag: number): ProducesBuilding {
        if(this.produces == null) {
            return null;
        }
        if(this.produces[buildingTag] == undefined) {
            return null;
        }

        return this.produces[buildingTag];
    }

    public GetBuildingCount(buildingName: string): number {
        if(this.curData['building'] != undefined && this.curData['building'][buildingName] != undefined) {
            var count = 0;
            const arrayCount = this.curData['building'][buildingName].length;

            for(var i = 0 ; i < arrayCount ; i++) {
                count += this.curData['building'][buildingName][i].COUNT;
            }

            return count;
        }
        return undefined;
    }

    public GetMapData(): Vec4 {
        var mapData = new Vec4(0, -1, 3, 2);

        if(ObjectCheck(this.curData, 'minFloor')) {
            mapData.y = this.curData['minFloor'];
        }

        if(ObjectCheck(this.curData, 'maxFloor')) {
            mapData.w = this.curData['maxFloor'];
        }

        if(ObjectCheck(this.curData, 'width')) {
            mapData.z = this.curData['width'];
        }

        return mapData;
    }

    public GetOpenFloorData(): Vec2 {
        var floorData = new Vec2(0, 0);

        if(ObjectCheck(this.curData, 'areaOpenMinLevel')) {
            floorData.x = this.curData['areaOpenMinLevel'];
        }

        if(ObjectCheck(this.curData, 'areaOpenMaxLevel')) {
            floorData.y = this.curData['areaOpenMaxLevel'];
        }

        return floorData;
    }

    public GetActiveGridData(): any {
        if(this.exterior_data == null) {
            return {'-1': {'0':0,'1':0,'2':0}, '0': {'0':0,'1':0,'2':0}, '1': {'0':0,'1':0,'2':0}};
        }
        return this.exterior_data.ActiveGrid;
	}

    public GetGridData(): any {
        return this.exterior_data.ExteriorGrid;
	}
	
    public GetAreaLevel() : number
    {
        return this.curData["areaLevel"];
    }

    public GetUserBuildingByTag(buildingTag: number | string) : BuildingInstance
    {
        let building: BuildingInstance = null;
        this.buildings.forEach(element => {
            if(element.Tag == buildingTag) {
                building = element;
            }
        });
        return building;
    }

    public GetUserBuildingList() : BuildingInstance[]
    {
        return this.buildings;
    }

    public GetNextEnergyExpire() : {exp : number, energy : number}
    {
        let exp = this.UserData.Energy_Exp
        let maxStamina = TableManager.GetTable<AccountTable>(AccountTable.Name).Get(User.Instance.UserData.Level).MAX_STAMINA
        let totalTick = exp + ((maxStamina - this.UserData.Energy) * 300)

        if(this.UserData.Energy >= maxStamina)
        {
            return {exp :-1, energy : this.UserData.Energy}
        }
        else if(exp < 0 || totalTick <= TimeManager.GetTime() )
        {
            this.UserData.Energy = maxStamina
            return {exp :-1, energy : this.UserData.Energy}
        }
        else if( exp > TimeManager.GetTime() )
        {
            return {exp :exp, energy : this.UserData.Energy}
        }
        else if( totalTick > TimeManager.GetTime())
        {
            for(let i = this.UserData.Energy+1; i < maxStamina; i++)
            {
                this.UserData.Energy_Exp += 300
                this.UserData.Energy = i
                if(this.UserData.Energy_Exp > TimeManager.GetTime())
                {
                    break
                }
            }
            return {exp : this.UserData.Energy_Exp, energy : this.UserData.Energy}
        }
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


/**
 *  Api의 Rs 코드 enum
 */

export enum eApiResCode 
{
    /** [알 수 없음] */
    GENERIC_SERVER_FAIL = 1,
    /** sql db 오류 */
    SQL_ERROR = 2,             
    /** 서버간 통신 오류 */
    NETWORK_ERROR = 3,         
    /** 서버 프로젝트 내부 오류 */
    SCRIPT_INCLUDE_FAIL = 4,   
    /** redis server error */
    REDIS_ERROR = 5,           
    /** 특정 구간 타임아웃 */
    SERVER_BUSY = 6,           

    // 10 ~ : common errors
    /** 세션아이디 오류 */
    SESSIONID_NOT_MATCH = 11,  
    /** INVALID_PARAM */
    PARAM_ERROR = 12,          
    /** 변경사항 없음 */
    //NO_CHANGE = 13,            
    
    /** 인벤 가득참 */
    INVENTORY_FULL = 14,       
    /** 비용 부족하여 진행 불가 */
    COST_SHORT = 15,           

    // 100 ~ : accounts
    // /** JWT 인증 실패 */
    // AUTH_FAILED = 101,         
    // /** 계정 생성 요청시 : 이미 생성됨 */
    // ACCOUNT_EXISTS = 102,      
    // /** 닉네임 중복되어 생성 불가 */
    // NICKNAME_DUPLICATES = 103, 
    // /** 닉네임 길이 부적절 */
    // INVALID_NICK_LENGTH = 104, 
    // /** 닉네임에 사용 불가능한 문자 포함 */
    // INVALID_NICK_CHAR = 105,   
    // /** 로그인 요청시 : 계정 미생성 상태 */
    // ACCOUNT_NOT_EXISTS = 106,  
    // /** ID 길이 짧음 */
    // ID_STRING_TOO_SHORT = 107,
    // /** ID 길이 긺 */
    // ID_STRING_TOO_LONG = 108,
    // /** ID 조건 미달성 */
    // ID_STRING_INVALID_ENCODING = 109,
    // /** ID 조건 미달성 */
    // ID_STRING_INVALID_NUMERIC = 110,
    // /** ID 조건 미달성 */
    // ID_STRING_MIXED_LANGS = 111,
    // /** ID 적절하지 않은 단어 사용 */
    // ID_STRING_FILTERED = 112,
    // /** dev signup 호출에서 id 중복 */
    // ID_DUPLICATES = 113,       

    // 200 ~ : building
    /** 이미 건설하였음 */
    ALREADY_BUILT = 201,       
    /** 해당 장소에는 설치할 수 없음 */
    YOU_CANT_BUILD_THERE = 202,   
    /** 외형 레벨 요구 조건 불충족 */
    EXTERIOR_LEVEL_TOO_LOW = 203, 
    /** 설치/레벨업 비용 부족 */
    CONSTRUCT_COST_NOT_MET = 204, 
    /** 현재 건설중임 */
    CONSTRUCTION_ONGOING = 205,   
    /** 건물이 존재하지 않음 */
    BUILDING_NOT_EXISTS = 206, 
    /** 현재 진행중인 작업이 있음 */
    BUILDING_QUEUE_NOT_EMPTY = 207,
    /** 건물의 생산슬롯이 이미 최대치 */
    BUILDING_SLOT_FULLY_EXPANDED = 208,
    /** 건물에서 현재 진행중인 작업이 없음 */
    BUILDING_NO_JOB_IS_RUNNING = 209,
    /** 현재 최고 레벨 */
    BUILDING_LEVEL_FULL = 210,

    // 300 ~ : produce
    /** 해당 건물 상태가 eBuildingState::NORMAL이 아님 */
    BUILDING_STATE_NOT_MET = 301, 
    /** 존재하지 않거나 해당 건물에 귀속되지 않은 레시피 */
    INVALID_RECIPE_ID = 302,   
    /** 건물 레벨 문제로 생산 불가 */
    REQUIRED_LEVEL_NOT_MET = 303, 
    /** 건물의 생산 슬롯이 가득참 */
    PRODUCE_SLOT_FULL = 304,   
    /** 생산물 획득 시도했으나 완료된 작업이 없음 */
    NOTHING_TO_HARVEST = 305,  

    // 400 ~ : items
    /** 이미 최대로 확장되었음 */ 
    INVENTORY_FULLY_EXPANDED = 401,
    /** 판매 시도했으나 보유량 부족 */
    NOT_ENOUGH_ITEM_TO_SELL = 402,
    /** 판매할 수 없는 아이템 */
    ITEM_IS_NOT_FOR_SALE = 403,

    // 500 ~ : landmark
    /** 랜드마크 정보가 없음 */
    LANDMARK_NOT_EXISTS = 501,
    /** 도저에서 수확 시도했으나 보상 없음 */
    DOZER_EMPTY = 502,
    /** 여행 출발할 수 있는 상태 아님 */
    TRAVEL_CANNOT_START_NOW = 503,
    /** 대상지 파라미터 오류 */
    TRAVEL_INVALID_WORLD = 504,
    /** 드래곤 부족 */
    TRAVEL_NOT_ENOUGH_DRAGONS = 505,
    /** 존재하지 않는 드래곤 */
    TRAVEL_DRAGON_NOT_EXISTS = 506,
    /** 에너지 부족 */
    TRAVEL_NOT_ENOUGH_ENERGY = 507,
    /** 아직 귀환하지 않음 */
    TRAVEL_NOT_FINISHED = 508,
    /** 여행 진행중이 아님 */
    TRAVEL_NOT_RUNNING = 509,
    /** 플랫폼이 잠김 상태 */
    SUBWAY_PLAT_LOCKED = 510,
    /** 플랫폼 건설 전 */
    SUBWAY_PLAT_NOT_BUILT = 511,    
    /** 플랫폼 이미 건설됨 */
    SUBWAY_PLAT_BUILT = 512,        
    /** 납품 진행중 */
    SUBWAY_PLAT_RUNNING = 513,      
    /** 이미 슬롯 재료 가득참 */
    SUBWAY_SLOT_FULL = 514,         
    /** 슬롯 넘버 오류 */
    SUBWAY_INVALID_SLOT = 515,      
    /** 슬롯 가득 차지 않음 */
    SUBWAY_SLOT_NOT_FULL = 516,     

    // 600 ~ : adventure
    /** 존재하지 않는 월드 */
    ADV_NO_SUCH_WORLD = 601,
    /** 존재하지 않는 스테이지 */
    ADV_NO_SUCH_STAGE = 602,
    /** 잠긴 월드 */
    ADV_WORLD_LOCKED = 603,
    /** 잠긴 스테이지 */
    ADV_STAGE_LOCKED = 604,
    /** 드래곤 참여 미달 */
    ADV_DECK_TOO_SMALL = 605,
    /** 드래곤 조건 미달 */
    ADV_INVALID_DRAGON = 606,
    /** 개방되지 않은 스테이지 */
    ADV_STAGE_NOT_RUNNING = 607,
    /** 보상 조건 미달 */
    ADV_REWARD_CONDITION_NOT_MET = 608,
    /** 이미 수령한 보상 */
    ADV_ALREADY_REWARDED = 609,

    // 700 ~ : dragon
    /** 소지하지 않은 드래곤 */
    DRA_NO_SUCH_DRAGON = 701,
    /** 드래곤 스킬 최대치 도달 */
    DRA_SKILL_LEVEL_MAX = 702,

    // 800 ~ : quest
    /** 찾는 퀘스트가 없음 */
    QUEST_NO_SUCH_QUEST = 801,
    /** 이미 수락한 퀘스트 */
    QUEST_ALEADY_ACCEPT = 802,
    /** 이미 완료한 퀘스트 */
    QUEST_ALEADY_ACCOMPLISH = 803,
    /** 조건 미달성 */
    QUEST_UNDER_CONDITION = 804,
    /** 이미 완료한 튜토리얼 */
    TUTORIAL_ALEADY_ACCOMPLISH = 805,

    /** API 코드 오류 */
    NOT_IMPLEMENTED_ERROR = 999999,   // TODO 블럭을 완료해야
}     

export enum eBattleSpeed
{
    NORMAL,
    FAST,
    SUPERFAST
}

export const BattleSpeedValue : number[] = 
[
    1.0,
    1.2,
    1.5
]
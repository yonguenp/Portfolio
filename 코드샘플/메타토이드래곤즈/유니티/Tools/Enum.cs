using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public enum eApiResCode
    {
        //성공
        OK = 0,
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
        /** 기획데이터 오류 */
        DATA_ERROR = 7,
        VERSION_ERROR = 8,//버전미스매칭

        // 10 ~ : common errors
        /** 세션아이디 오류 */
        SESSIONID_NOT_MATCH = 11,
        /** INVALID_PARAM */
        PARAM_ERROR = 12,
        /** 변경사항 없음 */
        NO_CHANGE = 13,            

        /** 인벤 가득참 */
        INVENTORY_FULL = 14,
        /** 비용 부족하여 진행 불가 */
        COST_SHORT = 15,
        INVALID_CONDITION = 16,         // 조건이 맞지 않음 (범용적으로 사용할 수 있을 경우에만 사용. 구체적인 내용이 필요하다면 콘텐츠에 맞게 별도로 만들어서 사용할 것)
        // 20 ~ : server status
        SERVICE_UNAVAILABLE = 21,        // 서버 점검 등.
        SERVICE_UNAVAILABLE_TEMP = 22,    // 서버 점검 등. 오픈 예정시각 안내 포함    

        // 100 ~ : accounts
        // /** JWT 인증 실패 */
        AUTH_FAILED = 101,
        /** 계정 생성 요청시 : 이미 생성됨 */
        ACCOUNT_EXISTS = 102,
        /** 닉네임 중복되어 생성 불가 */
        NICKNAME_DUPLICATES = 103,
        /** 닉네임 길이 부적절 */
        INVALID_NICK_LENGTH = 104,
        /** 닉네임에 사용 불가능한 문자 포함 */
        INVALID_NICK_CHAR = 105,
        /** 로그인 요청시 : 계정 미생성 상태 */
        ACCOUNT_NOT_EXISTS = 106,
        /** ID 길이 짧음 */
        ID_STRING_TOO_SHORT = 107,
        /** ID 길이 긺 */
        ID_STRING_TOO_LONG = 108,
        /** ID 조건 미달성 */
        ID_STRING_INVALID_ENCODING = 109,
        /** ID 조건 미달성 */
        ID_STRING_INVALID_NUMERIC = 110,
        /** ID 조건 미달성 */
        ID_STRING_MIXED_LANGS = 111,
        /** ID 적절하지 않은 단어 사용 */
        ID_STRING_FILTERED = 112,
        /** dev signup 호출에서 id 중복 */
        ID_DUPLICATES = 113,
        /** 계정 생성 실패 */
        ACCOUNT_CREATE_FAIL = 114,
        /** USER_NO 생성 실패 */
        GENERATE_USER_NO_FAIL = 115,
        /** 계정 SUB 생성 실패 */
        ACCOUNT_SUB_CREATE_FAIL = 116,
        /** 계정 차단됨 */
        ACCOUNT_IS_BANNED = 117,
        /** 계정 삭제됨 */
        ACCOUNT_IS_DELETED = 118,

        ACCOUNT_NOT_TO_BE_BANNED = 119,



        /** CBT 용 화이트리스트 체크 */
        NOT_WHITELIST = 199,

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

        INVALID_REPLACE_POSITION = 211, // 그리드 위치 사용 불가능
        INVALID_REPLACE_BUILDING = 212, // 이동 제한 건물 이동 시도

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
        
        CONDITION_NOT_MET = 404, // 조건 미달성

        NO_REWARD_AVAILABLE = 405, // 받을 수 있는 보상이 없음

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
        /** 여행 탐험지역 미클리어 */
        TRAVEL_ADVENTURE_NOT_CLEAR = 517,
        /** 소원의 성소 달성도가 보상받기에 부족함 */
        EXCHANGE_NOT_ENOUGH_POINT = 518,
        /** 소원의 성소 달성 보상을 이미 받았음 */
        EXCHANGE_ALREADY_RECEIVED = 519,
        /** 잼 파밍 던전 데이터 오류 */
        GEMDUNGEON_DATA_ERROR = 520,
        /** 잼 파밍 던전 층 데이터 오류 */
        GEMDUNGEON_FLOOR_DATA_ERROR = 521,
        /** 해당 층에 슬롯보다 많이 드래곤을 등록하려함. */
        GEMDUNGEON_FLOOR_DRA_COUNT = 522,
        /** 잼 던전에 등록된 드래곤 정보가 없음 */
        GEMDUNGEON_DRA_NO_SEARCH = 523,
        /** 잼 파밍 던전 층 슬롯이 전부 오픈된 상태임 */
        GEMDUNGEON_FLOOR_SLOT_FULL = 524,
        /** 피로도가 꽉 찬 드래곤에 회복 */
        GEMDUNGEON_DRA_FATIGUE_FULL = 525,
        /** 아이템 번호가 피로도 회복 아이템이 아님 */
        GEMDUNGEON_NO_RECOVERY_ITEM = 526,
        /** 아이템 번호가 부스터가 아님 */
        GEMDUNGEON_NO_BOOSTER_ITEM = 527,
        /** Battle상태가 아닌데 부스터를 사용하려함 */
        GEMDUNGEON_NO_BOOSTER_STATE = 528,
        /** 클레임(마그넷 수령 요청) 실패 */
        MINE_CLAIM_FAIL = 529,
        /** 존재하지 않는 아이템 */
        MINE_ITEM_NOT_EXISTS = 530,
        /** 아이템 수량 부족 */
        MINE_LACK_OF_ITEM = 531,
        /** 사용 불가 아이템 */
        MINE_ITEM_CANNOT_BE_USED = 532,
        /** 이미 사용된 아이템 */
        MINE_ALREADY_USED_ITEM = 533,
        /** 보호구역 */
        TRAVEL_INST_PROTECTED_WORLD = 545,



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
        /** 합성 재료로 요청한 카드 미보유 */
        DRA_MERGE_NO_SUCH_CARD = 703,
        /** 합성 재료로 요청한 카드 조합 불가 */
        DRA_MERGE_INVALID_CARDS = 704,
        /** 초월 조건 미달성 */
        DRA_TRANSCENDENCE_UNDER_CONDITION = 705,
        /** 초월 강화 실패 */
        DRA_TRANSCENDENCE_FAIL = 706,

        DRA_NO_TRANSCENDENCE = 707, // 초월하지 않은 드래곤

        DRA_PASSIVE_SLOT_LACK = 708, // 초월 패시브 슬롯 부족

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

        // 900 ~ : part
        /** 올바른 장비가 아님 */
        PART_NOT_EXISTS = 901,
        /** 강화 요청시 대상 레벨이 부적절함*/
        PART_INVALID_LEVEL_TO_REINFORCE = 902,
        /** 합성 요청시 올바른 장비 재료가 아님*/
        PART_INVALID_MATERIAL_TO_MERGE = 903,
        /** 합성 요청시 올바른 장비 등급이 아님*/
        PART_INVALID_GRADE_MATERIAL_TO_MERGE = 904,
        /** 합성 요청시 합성 최소 재료 요구 수량이 아님*/
        PART_INVALID_MATERIAL_COUNT_TO_MERGE = 905,
        /** 분해 요청시 올바른 장비가 아님*/
        PART_INVALID_TAG_TO_DECOMPOUND = 906,
        /** 장비 최대수량 도달 */
        PART_FULL = 907,
        // 착용중인 장비
        PART_EQUIPPED = 908,
        // 락 장비
        PART_LOCKED = 909,

        // 1000 ~ : arena
        ARENA_DRAGON_NOT_EXISTS = 1001,       //방어덱에 드래곤 장착 안됨
        ARENA_TICKET_REFILL_OVERCOUNT = 1002, //티켓 리필 횟수 초과
        ARENA_FIRST_STEP = 1003,              //아레나 첫 입장
        ARENA_INVALID_MATCH_ID = 1004,        // 존재하지 않는 매치 id 전달됨
        ARENA_MATCH_ALREADY_DONE = 1005,      // 이미 진행한 매치 id로 전투 요청
        ARENA_TICKET_SHORT = 1006,            // 티켓 부족
        ARENA_INVAILD_SEASON = 1007,          // 시즌 정보 부적합 or 받을 랭크 보상이 없는데 누르는 경우
        ARENA_NO_PENDED_MATCH = 1008,         // arena/result 호출했으나 진행중인 전투 없음

        // 1100 ~ : pet
        PET_NO_SUCH_PET = 1101,               // 해당 tag 펫 없음
        PET_NO_SUCH_DRAGON = 1102,            // 해당 id 드래곤 없음
        PET_ALREADY_EQUIPPED = 1103,
        PET_NOT_EQUIPPED = 1104,
        PET_TOO_MUCH_YOU_HAVE = 1105,
        PET_NOT_EXIST = 1106,                 //없는 펫 태그
        INVAILD_PET_MATERIALS = 1107,         //펫 경험치 재료 조건 미달성
        PET_FULL = 1108,                      //펫 최대수량 도달
        PET_INVALID_LEVEL_TO_REINFORCE = 1109,//펫 강화 요청시 대상 레벨이 부적절함
        PET_MERGE_INVALID_MATERIALS = 1110,   // 펫 합성 요청시 잘못된 재료

        // 1200 ~ : post
        POST_NO_SEARCH_MAIL = 1201,           // 없는 메일
        POST_USED_MAIL = 1202,                // 사용한(아이템 획득) 메일
        POST_ACCEPTABLE_FAIL = 1203,          // 우편 수령 실패
        POST_NO_DELETE_MAIL = 1204,           // 지운 우편이 없음

        // 1300 ~ : daily
        DAILY_DATA_ERROR = 1301,                // 요일 던전 생성 오류
        DAILY_REFILL_OVERCOUNT = 1302,          // 요일 던전 리필 한도 초과
        DAILY_SWEEP_LOCKED = 1303,              // 요일 던전 소탕 조건 미달
        DAILY_DAY_NOT_MATCHED = 1304,           // 요일 조건 미달
        DAILY_START_FULL = 1305,                // 요일 던전 입장 횟수 초과
        DAILY_NO_SUCH_WORLD = 1306,             // 요일 던전 월드 정보 못 찾음
        DAILY_NO_SUCH_STAGE = 1307,             // 요일 던전 스테이지 정보 못 찾음

        // 1400 ~ : friend
        NOT_FOUND_USER = 1401,                  // 존재하지 않는 유저
        MY_FRIEND_LIST_FULL = 1402,             // 나의 친구 목록이 가득참
        MY_SEND_REQ_LIST_FULL = 1403,           // 내가 보낸 친구 신청 목록이 가득참
        OTHERS_FRIEND_LIST_FULL = 1404,         // 대상의 친구 목록이 가득참
        OTHERS_RECV_REQ_LIST_FULL = 1405,       // 대상이 받은 친구 신청 목록이 가득참
        NOT_A_FRIEND = 1406,                    // 친구가 아님
        ALREADY_FRIEND = 1407,                  // 이미 친구
        ALREADY_RECV_REQUEST = 1408,            // 이미 친구 요청을 받음
        ALREADY_SENT_REQUEST = 1409,            // 이미 친구 요청을 보냄
        ALREADY_BLOCKED = 1410,                 // 이미 차단함
        ALREADY_RECV_GIFT = 1411,               // 이미 선물 받음
        ALREADY_SENT_GIFT = 1412,               // 이미 선물 보냄
        NO_GIFT_RECEIVED = 1413,                // 받은 선물이 없음
        EXCEEDED_DAILY_LIMIT = 1414,            // 선물 받기 일일 한도 초과
        FRIEND_LIST_EMPTY = 1415,               // 친구 목록이 NULL
        RECV_REQ_LIST_EMPTY = 1416,             // 받은 친구 신청 목록이 NULL
        RECOMMEND_LIST_EMPTY = 1417,            // 추천 목록이 NULL
        CANNOT_REFRESH_RECOMMEND_LIST = 1418,   // 아직 추천 친구 목록을 갱신할 수 없음
        MY_BLOCK_LIST_FULL = 1419,              // 아직 추천 친구 목록을 갱신할 수 없음
        FRIENDLY_POIN_FULL = 1420,              // 우정포인트가 가득 참

        PACKAGE_NOT_EXISTS = 1501,              //패키지리소스가 존재하지 않음
        EXPIRED_DATE = 1502,                    //구매기한을 넘김
        OUT_OF_STOCK = 1503,                    //횟수제한을 넘김
        NOT_ENOUGH_FOR_COST = 1504,             //요구가격에 못미침

        // 파라미터 관련
        NO_SUCH_PRODUCT = 1601,          // 존재 않는 상품 id를 전달받음
                                         // 기간 한정
        CURRENTLY_NOT_AVAILABLE = 1602,  // 기간 한정 상품으로 현재 판매 않음
                                         // 구매 횟수 제한
        ALREADY_PURCHASED = 1603,        // 구매 횟수 제한 상품이며 이미 구매함

        // 검증 오류
        PARAM_ERR = 1604,                // 결제 성공 이벤트의 payload를 전달해야 함
        INVALID_RECEIPT = 1605,          // 결제 영수증 검증 실패
        ALREADY_ISSUED = 1606,         // 해당 영수증에 대한 보상이 이미 지급되었음

        // 패스
        NOT_IN_SEASON = 1607,            // 패스 시즌 아님
        REQUIREMENT_NOT_MET = 1608,      // 1단계용 패스 먼저 구매해야 함

        // server err
        SERVER_ERROR = 1609,

        // appending
        PRODUCT_ID_NOT_MATCH = 1610,    // 영수증의 sku와 일치하지 않는 product_id 호출됨
        PURCHASE_NOT_MADE = 1611,       // 구매 완료 상태가 아닌 영수증
        VERIFICATION_FAILED = 1612,     // 영수증 검증 실패

        // 구독형 상품
        NOT_SUBSCRIBING = 1613,          // 구독중이 아님
        TODAY_REWARDED = 1614,


        // 1700 ~ : Raid
        RAID_DATA_ERROR = 1701,         // 레이드 생성 오류
        RAID_START_FULL = 1702,         // 레이드 입장 횟수 초과
        RAID_DAY_NOT_MATCHED = 1703,    // 레이드 월드의 요일 조건이
        RAID_NO_SUCH_WORLD = 1704,      // 레이드 월드 정보 못 찾음
        RAID_NO_SUCH_STAGE = 1705,      // 레이드 스테이지 정보 못 찾음


        // 1800 ~ : guild
        GUILD_DATA_ERROR = 1801,                // 조합 데이터 오류
        GUILD_DUPLICATE_NAME = 1802,            // 조합명 중복
        GUILD_GUILD_NO_GENERATE_FAIL = 1803,    // 조합 번호 생성 실패
        GUILD_ALREADY_REQ_JOIN = 1804,          // 이미 가입 신청한 조합임
        GUILD_REQ_JOIN_FAIL = 1805,             // 가입 신청 실패
        GUILD_NO_REQ_JOIN = 1806,               // 신청한적 없는 조합
        GUILD_REQ_CANCEL_FAIL = 1807,           // 가입 취소 실패
        GUILD_REQ_DENY_FAIL = 1808,             // 가입 취소 실패
        GUILD_NO_AUTH = 1809,                   // 권한이 없음
        GUILD_UNABLE_JOIN = 1810,               // 조합 가입 불가
        GUILD_MEMBER_FULL = 1811,               // 조합이 가득참
        GUILD_ALREADY_BELONG = 1812,            // 이미 소속된 조합이 있음
        GUILD_CANNOT_JOIN_YET = 1813,           // 아직 조합에 가입할 수 없음
        GUILD_NO_GUILD_YOU_CAN_JOIN = 1814,     // 가입할 수 있는 조합이 없음
        GUILD_INVALID_OPEN_CONDITION = 1815,    // 조합 생성 조건이 잘못됨
        GUILD_INVALID_JOIN_CONDITION = 1816,    // 조합 가입 조건이 잘못됨
        GUILD_CHANGE_GUILD_NAME_FAIL = 1817,    // 길드명 변경 실패
        GUILD_CHANGE_EMBLEM_FAIL = 1818,        // 길드명 변경 실패
        GUILD_NO_CHANGE_GRADE = 1819,           // 등급 변경 없음
        GUILD_NOT_CAN_BE_CHANGE = 1820,         // 변경할 대상이 아님
        GUILD_INVALID_CHANGE_MEMBER_TYPE_CONDITION = 1821, // 조합원 등급 변경할 조건이 아님
        GUILD_CANNOT_LEAVE = 1822,              // 탈퇴할 수 없음
        GUILD_CANNOT_EXPEL = 1823,              // 탈퇴할 수 없음
        GUILD_CANNOT_CLOSE_AGAIN = 1824,        // 조합 해체를 아직 할 수 없음
        GUILD_CANNOT_DONATE_YET = 1825,         // 아직 기부할 수 없음
        GUILD_DONATION_COUNT_IS_FULL = 1826,    // 기부를 다 했음
        GUILD_CANNOT_ATTENDENCE_YET = 1827,     // 아직 출석 못함

        // 1900 ~ : tournament
        TOURNAMENT_NO_REGISTERED_SEASON = 1901, // 등록되지 않은 시즌
        TOURNAMENT_NEW_SEASON_CREATE_FAILED = 1902, // 새 시즌 생성 실패
        TOURNAMENT_INCORRECT_ROUND_STEP = 1903, // 잘못된 라운드 또는 단계
        TOURNAMENT_NOT_SELECTED_PARTICIPANT = 1904, // 선정되지 않은 대전자
        TOURNAMENT_NOT_QUALIFIED = 1905, // 참가 자격 미달
        TOURNAMENT_APPLY_FAILED = 1906, // 참가 신청 실패
        TOURNAMENT_CANNOT_TEAM_CHANGE = 1907, // 참가 신청 실패
        TOURNAMENT_SELECT_PARTICIPANT_FAILED = 1908, // 참가 신청 실패
        TOURNAMENT_INVALID_DRAGON_ID = 1909, // 잘못된 드래곤 번호
        TOURNAMENT_NOT_EXISTS_MATCH_INFO = 1910, // 매치 정보가 존재하지 않음
        TOURNAMENT_INVALID_TEAM_INFO = 1911, // 매치 정보가 존재하지 않음
        TOURNAMENT_ALREADY_BET = 1912, // 이미 베팅함
        TOURNAMENT_UNEARNED_WIN = 1913, // 부전승
        TOURNAMENT_INVALIDITY_MATCH = 1914, // 부전승
        TOURNAMENT_NOT_SAME_SEASON_ID = 1915, // 같은 시즌 ID가 아님
        TOURNAMENT_LACK_OF_CHAMP_ORACLE = 1916, // 같은 시즌 ID가 아님
        TOURNAMENT_BET_LIMIT_OVER = 1917, // 한도 베팅액 초과

        //dapp 관련
        CONNECTION_FAIL = 2001,
        WALLET_NOT_CONNECTED = 2002,
        IS_GUEST_ACCOUNT = 2003,
        INVALID_SERVER_CONNECTION = 2004,
        /** API 코드 오류 */
        NOT_IMPLEMENTED_ERROR = 999999,   // TODO 블럭을 완료해야
    }

    public enum eResourcePath
    {
        None,

        PopupPrefabPath,                        //팝업 폴더
        BuildingClonePath,                      //기본 프리팹 클론 모음 폴더
        PrefabClonePath,                        //기본 프리팹 클론 모음 폴더
        DragonClonePath,                        //기본 프리팹 클론 모음 폴더
        UIDragonClonePath,                      //기본 프리팹 클론 모음 폴더
        MonsterClonePath,                       //기본 프리팹 클론 모음 폴더
        EffectPrefabPath,                       //월드 프리팹 모음 폴더
        TownPrefabPath,
        TutorialPrefabPath,                     //튜토리얼 프리팹 모음
        PetClonePath,                           //기본 프리팹 클론 모음 폴더          
        SpecialBGPath,                          //특수(레전더리)드래곤이나 펫 배경 폴더 - spine으로 오면 prefab으로 바꿔야함
        ScriptBGPath,
        ScriptObjectPath,

        ItemIconPath,                           //아이템 아이콘 sprite 모음 폴더
        CharIconPath,                           //캐릭터 또는 적 아이콘 모음 폴더
        ElementIconPath,                        //속성 아이콘 모음 폴더
        ClassIconPath,                          //드래곤 클래스(직업) 아이콘 모음 폴더
        BuildingUIClonePath,                    //건설 건물 UI(스파인) 모음 폴더
        BuildingIconPath,                       //건물 아이콘 sprite 모음 폴더
        BuildingCardIconPath,                   //건물카드 아이콘 sprite 모음 폴더(미사용)
        BuildingRecipeIconPath,                 //건물레시피 아이콘 sprite 모음 폴더(미사용)
        SkillIconPath,                          //스킬 sprite 모음 폴더
        PartsIconPath,                          //장비(파츠) sprite 모음 폴더
        DragonGradeTagIconPath,                 //드래곤 등급 태그(노말, 레어 등등)이미지 폴더
        QuestIconPath,                          //퀘스트 아이콘 sprite 모음 폴더
        BuffIconPath,                           //전투 버프 아이콘 sprite 모음 폴더
        ProfileIconPath,                        //프로필 아이콘 Sprite 모음 폴더
        PortraitFrameIconPath,                  //유저 초상화 아이콘(월드보스 보상용) Sprite 모음 폴더
        PetIconPath,                            //펫 아이콘 Sprite 모음 폴더
        StoreImagePath,                         //상점 디폴트 Sprite 모음 폴더
        ShopMenuIconPath,                       //상점 메뉴 아이콘 모음 폴더
        GachaSpritePath,                        //뽑기 관련 리소스 폴더
        UISpritePath,							//UI 이미지 관련 리소스 폴더

        EffectCustomPath,                       // 이펙트 커스텀 ScriptableObject 폴더

        StaticPrefabPath,                       // 고정 프리팹 Static_Prefabs 폴더
        StaticPrefabUIPath,                     // 고정 프리팹 Static_Prefabs 폴더

        DragonSkeletonDataPath,                     // 스켈레톤 데이터 폴더

        BundleScenePath,                        // 에셋 번들 씬
        ArenaRankPath,                          // 아레나 랭크 아이콘 sprite 모음 폴더
        WorldSelectImgPath,                     // 월드 선택 아이콘 sprtie 모음 폴더

        EventAttendancePath,                    //이벤트 출첵 리소스 폴더

        ProjectileSpritePath,                   //Generic 투사체 Sprite 폴더

        SfxSoundPath,                           // 효과음 AudioClip 폴더
        BgmSoundPath,                           // 배경음 AudioClip 폴더

        GuildResourcePath,                          // 길드마크 Sprite 모음폴더

        Max
    }
    public enum ePartSetNum
    {
        NONE = 0,

        SET_1 = 1,
        SET_2 = 2,
        SET_3 = 3,
        SET_4 = 4,
        SET_5 = 5,
        SET_6 = 6,

        MAX
    }
    public enum eAuthAccount
    {
        NONE = 0,
        GOOGLE,
        APPLE,
        IMX,

        GUEST = 99,
    }

    [System.Flags]
    public enum eLoginUserState
    {
        NONE = 0,
        NORMAL = 1 << 0,     // 정상 유저
        TO_BE_DELETED = 1 << 1,
        DELETED = 1 << 2,
        BANNED = 1 << 3,
    }

    public enum eLoginResult
    {
        NONE = -1,

        OK_NEW_ACCOUNT,
        OK_HAS_ACCOUNT,

        ERROR_UNKNOWN,
        ERROR_INVALID_TOKEN,
        ERROR_BANNED_ACCOUNT,
        ERROR_DELETED_ACCOUNT,
        ERROR_NOT_WHITE_LIST,
        ERROR_TOBE_DELETED_ACCOUNT,
    }

    public enum eSoundDataType
    {
        BGM,
        SFX
    }

    public enum eMapParam
    {
        MetodHeadY = 102,
        CellSizeX = 324,
        CellSizeBothX = 348,
        CellSizeY = 214,
        UnerCellSizeY = 371,
        ElevatorContainerLeft = -203,
        ElevatorContainerRight = 37,
        ElevatorFrameLeft = -120,
        ElevatorFrameRight = 120,
        ElevatorX = 252,
        ElevatorY = 214,
        PulleyTopY = 136,
        PulleyBottomY = -80,
        CellWallPositionY = 55
    }

    public enum eCircleType
    {
        Circle,
        XEllipse,
        YEllipse
    }

    public enum eAIStateMoveStep
    {
        StateStart = 0,
        Check = 1,
        Move = 2,
        StateEnd = 3
    }

    public enum eTableViewType
    {
        Vertical,
        Horizental
    }
    public enum eTableViewAnchor
    {
        FIRST,
        MIDDLE,
        LAST
    }

    public enum eSBScrollType
    {
        Horizontal,
        Vertical
    }

    [Flags]
    public enum eDirectionBit
    {
        None = 0,
        Left = 1,
        Right = 2,
        Up = 4,
        Down = 8
    }

    public enum eBattleState
    {
        None = 0,
        Playing = 1,
        Win = 2,
        Lose = 3,
        TimeOver = 4,
        Abort = 5,
    }

    public enum eDragonElevatorStateType
    {
        None,
        InEscalatorStartMove,
        InEscalatorEndMove,
        InMove,
        InCall,
        In,
        InOrderMove,
        InContainMove,
        Contain,
        ElevatorMove,
        ExitOrderMove,
        ExitContainMove,
        ExitEscalatorStartMove,
        ExitEscalatorEndMove,
        ExitMove,
        Exit,

        RunFast,
        FallState,
        ShakeLeftAndRight
    }

    public enum eDragonTownActionType
    {
        None,
        RunFast,
        FallState,
        ShakeLeftAndRight
    }


    public enum eElevatorType
    {
        None,
        Left,
        Right,
    }

    public enum eElementType
    {
        None = 0,
        START,
        FIRE = START,
        WATER,
        EARTH,
        WIND,
        LIGHT,
        DARK,
        MAX
    }

    public enum eDamageType
    {
        CRITICAL,
        ELEMENT_FIRE,
        ELEMENT_WATER,
        ELEMENT_EARTH,
        ELEMENT_WIND,
        ELEMENT_LIGHT,
        ELEMENT_DARK,
        ELEMENT_NORMAL,
        SKILL,
        MISS,
        RECORVERY
    }

    public enum eDragonGrade
    {
        Normal = 1,
        Uncommon = 2,
        Rare = 3,
        Unique = 4,
        Legend = 5
    }

    public enum ePartGrade
    {
        Common = 1,
        Uncommon = 2,
        Rare = 3,
        Unique = 4,
        Legend = 5,
    }

    public enum eFrameFunctioal
    {
        NONE = 0,
        POPUP,
        TOOLTIP,
        CallBack
    }

    public enum eItemFrameType
    {
        NONE,
        ITEM,
        GOLD,
        GEMSTONE,
        STEMINA,
        ARENA_TICKET,
        ACCOUNT_EXP,
        MAGNET,
        ARENA_POINT,
        FRIEND_POINT,
        MAGNITE,
        MAX
    }

    public enum eItemFrameEventType
    {
        NONE,
        QUANTITY,
        RECEIPE
    }

    public enum eItemKind
    {
        /// <summary>
        /// 리소스 용도로 사용됨
        /// </summary>
        RESOURCE = 1,
        /// <summary>
        /// Unknown(미사용)
        /// </summary>
        EVENT,
        /// <summary>
        /// 뽑기권
        /// </summary>
        GACHA,
        /// <summary>
        /// 건전지
        /// </summary>
        EXP,
        /// <summary>
        /// 1차 완성물(돈으로 만드는 완성물)
        /// </summary>
        RECEIPE,
        /// <summary>
        /// 제작 완성물(재료를 사용해서 만드는 완성물)
        /// </summary>
        PRODUCT,
        /// <summary>
        /// 가속권
        /// </summary>
        ACC_TICKET,
        /// <summary>
        /// 스킬 레벨업 재료
        /// </summary>
        SKILL_UP,
        /// <summary>
        /// 타운 업그레이드 재료
        /// </summary>
        HIGH_RECEIPE,
        /// <summary>
        /// 장비
        /// </summary>
        EQUIP,
        /// <summary>
        /// 장비 업그레이드 재료
        /// </summary>
        EQUIP_UPGRADE,
        /// <summary>
        /// 요일던전 충전티켓(미사용)
        /// </summary>
        RECHARGE,
        /// <summary>
        /// 소탕권(미사용)
        /// </summary>
        SWEEP,
        /// <summary>
        /// 펫 업그레이드 재료
        /// </summary>
        PET_UPGRADE,
        /// <summary>
        /// 진열장
        /// </summary>
        SHOWCASE,
        /// <summary>
        /// 젬 던전 부스터
        /// </summary>
        GEM_BOOSTER,
        /// <summary> 젬 던전 피로 회복제 </summary>
        GEM_FATIGUE_RECOVERY,
        /// <summary> Passive Skill 재료 아이템 </summary>
        SKILL_PASSIVE_MATERIAL,
        /// <summary>
        /// 광산 부스터
        /// </summary>
        MINE_BOOSTER,
    }

    public enum eFriendPopupTab
    {
        FRIEND_LIST,
        FRIEND_RECEIVE_INVITE,
        FRIEND_RECOMMEND
    }

    public enum eGachaGroupMenu
    {
        NONE = 0,
        CLASS = 1,
        DRAGON_PREMIUM,
        DRAGON_FREE,
        PET_PREMIUM,
        PET_FREE,
        LUCKYBOX,
        PICKUP_DRAGON,
    }

    public enum eGachaMenuType
    {
        DRAGON = 1,
        PET,
        ITEM,
        PICKUP = 4,
    }

    public enum eAchieveSystemMessageType
    {
        GET_DRAGON_U,               // UNIQUE 등급 드래곤 획득 시
        GET_DRAGON_L,               // LEGENDARY 등급 드래곤 획득 시
        GET_PET_U,                  // UNIQUE 등급 펫 획득 시
        GET_PET_L,                  // LEGENDARY 등급 펫 획득 시

        COMPOUND_DRAGON_U,          // UNIQUE 등급 드래곤 합성 시
        COMPOUND_DRAGON_L,          // LEGENDARY 등급 드래곤 합성 시
        COMPOUND_PET_U,             // UNIQUE 등급 펫 합성 시
        COMPOUND_PET_L,             // LEGENDARY 등급 펫 합성 시

        PET_MAX_REINFORCE,

        EQUIPMENT_9_REINFORCE,
        EQUIPMENT_12_REINFORCE,
        EQUIPMENT_15_REINFORCE,     // 15강 장비 획득 시
        GET_EQUIPMENT,              // L 등급 장비 획득 시

        ACHIEVE_TOWN_MAX_LEVEL,     // 타운 최대 레벨 달성 시 (12레벨)

        EVENT_LOTTO_WINNER,			//로또 이벤트 당첨자
        EVENT_POCKET_LEVEL10,

        SERVER_MAINTENANCE, //서버점검

        TRANSCENDENCE_STEP1, //초월 1단계
        TRANSCENDENCE_STEP2, //초월 2단계
        TRANSCENDENCE_STEP3, //초월 3단계
    }

    public enum eAccelerationType
    {
        NONE = 0,
        CONSTRUCT = 1,          // 건설
        LEVELUP = 2,            // 레벨업
        JOB = 3,                // 생산작업
        EXCHANGE = 4,           // 소원나무
    }
    // 생산관리 모두채우기 필터에서 사용
    public enum eProduceOptionFilter
    {
        PRODUCT_TIME_SHORT,
        PRODUCT_TIME_LONG,
    }

    public enum eHarvestType
    {
        GET_ONE,            // 해당 생산품 1개만 획득
        GET_BUILDING,       // 해당 빌딩 내 모든 생산품 획득
        GET_BUILDING_TYPE   // 같은 타입 빌딩의 전체 생산품 획득
    }

    [Flags]
    public enum eGradeFilter
    {
        None = 0,
        Common = 1 << 0,
        Uncommon = 1 << 1,
        Rare = 1 << 2,
        Unique = 1 << 3,
        Legendary = 1 << 4,
        ALL = Common | Uncommon | Rare | Unique | Legendary
    }

    [Flags]
    public enum eTypeFilter
    {
        None = 0,
        ATK = 1 << 0,
        DEF = 1 << 1,
        HP = 1 << 2,
        CRI_PROC = 1 << 3,
        CRI_RESIS = 1 << 4,
        CRI_DMG_RESIS = 1 << 5,
        SKILL_DMG_RESIS = 1 << 6,
        PHYS_DMG_RESIS = 1 << 7,
        LIGHT_DMG_RESIS = 1 << 8,
        DARK_DMG_RESIS = 1 << 9,
        WATER_DMG_RESIS = 1 << 10,
        FIRE_DMG_RESIS = 1 << 11,
        WIND_DMG_RESIS = 1 << 12,
        EARTH_DMG_RESIS = 1 << 13,
        DEL_COOLTIME = 1 << 14,
        ALL_ELEMENT_DMG_RESIS = 1 << 15,
        BOSS_DMG = 1 << 16,

        ALL = ATK | DEF | HP | CRI_PROC | CRI_RESIS | CRI_DMG_RESIS | SKILL_DMG_RESIS | PHYS_DMG_RESIS | LIGHT_DMG_RESIS |
        DARK_DMG_RESIS | WATER_DMG_RESIS | FIRE_DMG_RESIS | WIND_DMG_RESIS | EARTH_DMG_RESIS | DEL_COOLTIME | ALL_ELEMENT_DMG_RESIS | BOSS_DMG
    }

    [Flags]
    public enum eJobFilter
    {
        None = 0,

        START,

        TANKER = START,
        WARRIOR = 2,

        ASSASSIN = 4,
        BOMBER = 8,

        SNIPER = 16,
        SUPPORTER = 32,

        ALL = TANKER | WARRIOR | ASSASSIN | BOMBER | SNIPER | SUPPORTER,
    }

    [Flags]
    public enum eReinforceLevelFilter
    {
        None = 0,
        Zero = 1,
        OneToSix = 2,
        SevenToNine = 4,
        TenToTwelve = 8,
        ThirteenToFifteen = 16,
        ALL = Zero | OneToSix | SevenToNine | TenToTwelve | ThirteenToFifteen,
    }

    [Flags]
    public enum eElementFilter
    {
        None = 0,
        Fire = 1,
        Water = 2,
        Earth = 4,
        Wind = 8,
        Light = 16,
        Dark = 32,
        ALL = Fire | Water | Earth | Wind | Light | Dark,
    }

    [Flags]
    public enum eJoinedContentFilter
    {
        None = 0,
        Adventure = 1,
        Arena_Atk = 2,
        Arena_Def = 4,
        Daily_Dungeon = 8,
        World_boss = 16,
        ALL = Adventure | Arena_Def | Arena_Atk | Daily_Dungeon | World_boss,
    }
    [Flags]
    public enum ePetStatFilter
    {
        None = 0,

        RATIO_ATK_DMG_PERCENT = 1 << 0,
        ADD_ATK_DMG_VALUE = 1 << 1,
        CRI_DMG_PERCENT = 1 << 2,
        CRI_DMG_VALUE = 1 << 3,
        LIGHT_DMG_PERCENT = 1 << 4,
        LIGHT_DMG_VALUE = 1 << 5,
        DARK_DMG_PERCENT = 1 << 6,
        DARK_DMG_VALUE = 1 << 7,
        WATER_DMG_PERCENT = 1 << 8,
        WATER_DMG_VALUE = 1 << 9,
        FIRE_DMG_PERCENT = 1 << 10,
        FIRE_DMG_VALUE = 1 << 11,
        WIND_DMG_PERCENT = 1 << 12,
        WIND_DMG_VALUE = 1 << 13,
        EARTH_DMG_PERCENT = 1 << 14,
        EARTH_DMG_VALUE = 1 << 15,
        RATIO_PVP_DMG_PERCENT = 1 << 16,
        ADD_PVP_DMG_VALUE = 1 << 17,
        RATIO_PVP_CRI_DMG_PERCENT = 1 << 18,
        ADD_PVP_CRI_DMG_VALUE = 1 << 19,
        BOSS_DMG_PERCENT = 1 << 20,
        BOSS_DMG_VALUE = 1 << 21,

        ALL = RATIO_ATK_DMG_PERCENT | ADD_ATK_DMG_VALUE | CRI_DMG_PERCENT | CRI_DMG_VALUE | LIGHT_DMG_PERCENT | LIGHT_DMG_VALUE | DARK_DMG_PERCENT | DARK_DMG_VALUE |
            WATER_DMG_PERCENT | WATER_DMG_VALUE | FIRE_DMG_PERCENT | FIRE_DMG_VALUE | WIND_DMG_PERCENT | WIND_DMG_VALUE | EARTH_DMG_PERCENT | EARTH_DMG_VALUE |
            RATIO_PVP_DMG_PERCENT | ADD_PVP_DMG_VALUE | RATIO_PVP_CRI_DMG_PERCENT | ADD_PVP_CRI_DMG_VALUE | BOSS_DMG_PERCENT | BOSS_DMG_VALUE
    }

    [Flags]
    public enum ePetOptionFilter
    {
        None = 0,

        RATIO_ATK_DMG_PERCENT = 1 << 0,
        CRI_DMG_PERCENT = 1 << 1,        
        LIGHT_DMG_PERCENT = 1 << 2,
        DARK_DMG_PERCENT = 1 << 3,
        WATER_DMG_PERCENT = 1 << 4,
        WIND_DMG_PERCENT = 1 << 5,
        FIRE_DMG_PERCENT = 1 << 6,        
        EARTH_DMG_PERCENT = 1 << 7,        
        RATIO_PVP_DMG_PERCENT = 1 << 8,
        RATIO_PVP_CRI_DMG_PERCENT = 1 << 9,
        ADD_BUFF_TIME_PERCENT = 1 << 10,
        DEL_BUFF_TIME_PERCENT = 1 << 11,
        BOSS_DMG_PERCENT = 1 << 12,

        ALL = RATIO_ATK_DMG_PERCENT | CRI_DMG_PERCENT | LIGHT_DMG_PERCENT | DARK_DMG_PERCENT | WATER_DMG_PERCENT | WIND_DMG_PERCENT | FIRE_DMG_PERCENT 
            | EARTH_DMG_PERCENT | RATIO_PVP_DMG_PERCENT | RATIO_PVP_CRI_DMG_PERCENT | ADD_BUFF_TIME_PERCENT | DEL_BUFF_TIME_PERCENT | BOSS_DMG_PERCENT
    }
    public enum eDcomposeCount
    {
        Grade = 5,
        Type = 17,
        Level = 5,
        Element = 6
    }
    public enum eDragonListFilterCount
    {
        Element = 6,
        Job = 6,
        TeamFormation = 10,
        Grade = 5,
        PetStat = 22,
        PetOption = 13,
    }

    public class eType
    {
        public static string String = "String",
        Number = "Number",
        Boolean = "Boolean",
        Undefined = "Undefined",
        Null = "Null",
        Object = "Object",
        Array = "Array",
        RegExp = "RegExp",
        Math = "Math",
        Date = "Date",
        Function = "Function";
    }

    public enum eBuildingState
    {
        NONE = 0,                 // ERROR
        LOCKED = 1,               // 요구조건 미비
        NOT_BUILT = 2,            // 건설 가능
        CONSTRUCTING = 3,         // 건설/레벨업 중
        CONSTRUCT_FINISHED = 4,   // 건설 완료
        NORMAL = 5,               // 배치됨
    }

    public enum eBuildingType
    {
        LANDMARK = 1,
        EXP = 2,
        MATERIAL = 3,
        PRODUCT = 4,
        COSTUME = 5,
        CASH = 6,
    }

    public enum eMailTitleType
    {
        NONE = 0,
        STRING_TABLE = 1,
        TEXT = 2
    }

    public enum eGoodType
    {
        NONE = 0,
        GOLD = 1,
        ENERGY = 2,
        ITEM = 3,
        CHARACTER = 4,
        CARD = 5,
        NOTUSE = 6,
        ITEMGROUP = 7,
        ARENA_TICKET = 8,
        PET = 9,
        EQUIPMENT = 10,
        FRIENDLY_POINT = 11,
        ARENA_POINT = 12,
        PASS_EXP = 13,
        HOLDER_PASS_EXP = 14,

        GUILD_EXP = 15,
        GUILD_POINT = 16, 

        // 번호는 클라이언트에서 구분해서 쓰려고 넣어둠 임시처리
        AD_REMOVE = 8001,
        //

        ADVERTISEMENT = 990,
        CASH = 991,
        MILEAGE = 992,
        GACHA_GROUP = 993,
        DICE_GROUP = 994,
        ACCOUNT_EXP = 995,
        GEMSTONE = 997,
        
        MAGNET = 1001,
        MAGNITE = 1002,
        
        COIN = 999
    }

    public enum eLandmarkType
    {
        UNKNOWN = -1,
        Dozer = 101,
        Travel = 201,
        SUBWAY = 301,
        EXCHANGE = 401,
        GEMDUNGEON = 501,
        MINE = 601,
        GUILD = 701,
    }

    public enum eGemDungeonState
    {
        /// <summary> 초기 상태 </summary>
        NONE = 0,
        /// <summary> 조건 미달성 </summary>
        LOCKED = 1,
        /// <summary> 조건 달성 비활성화 상태 </summary>
        NOT_BUILT = 2,
        /// <summary> 드래곤이 없어 대기중 상태 </summary>
        IDLE = 3,
        /// <summary> 정상 전투중 상태 </summary>
        BATTLE = 4,
        /// <summary> 보상이 꽉차거나 드래곤 피로도가 없어서 종료된 상태 </summary>
        END = 5
    }

    public enum TUTORIAL_EVENT_TYPE
    {
        NONE,
        AUTO_OPEN,
        COMMON_OPEN,
        TOUCH,
        BUTTON,
        WAITING,
    }

    public enum eTutorialGuideArrowDir
    {
        NONE,
        UP,
        DOWN,
        LEFT,
        RIGHT
    }
    public enum eObjectType
    {
        Object,
        Btn,
    }
    public enum eObjectPos
    {
        UICanvas,
        WorldCanvas,
        WorldObject
    }
    [System.Flags]
    public enum eTutorialOption
    {
        None = 0,

        DimmedOff = 1 << 0, // 딤드 끄기

        FocusAction = 1 << 1, // 하이라이트 클릭시 타겟 트랜스폼의 button의 event 실행 
        DimmedAction = 1 << 2,  // 딤드 클릭시 함수로 등록한 event 실행

        FocusNextAction = 1 << 3, // 하이라이트 클릭시 다음 튜토리얼 실행
        DimmedNextAction = 1 << 4, // 딤드 누르면 다음 튜토리얼 실행

        IgnoreBtnEvent = 1 << 5, // 버튼 속성을 가진 오브젝트를 targetTransform 으로 지정했지만 그 오브젝트의 버튼 이벤트를 사용하지 않을 때 
        MsgBoxTargetPos = 1 << 6, // 타겟 기준으로 메세지 박스의 위치 조절 
        MsgBoxTwin = 1 << 7, // 박스 커졌다 작아졌다 효과
        MsgBoxOff = 1 << 8, // 메세지 박스 가리기
        LastInGroup = 1 << 9, // 그룹 내 마지막 튜토리얼
        TutorialRestartBifurcation = 1 << 10, // 현재 그룹의 튜토리얼 재시작 분기점, 이거 넘으면 재시작 불가
    }

    public enum eFocusTarget
    {
        TargetTransform = 1,
        MainQuest = 2,
        Building =3,
        RecordedObj = 4,
    }

    public enum ePopupEventType
    {
        None = 0,
        PopupOpen = 1,
        PopupClose = 2,
        PopupAllClose = 3,
        PopupRefresh = 4
    }

    public enum eQuestState
    {
        UNDER_CONDITION = 0,
        NEW_QUEST = 1,
        PROCEEDING = 2,
        PROCESS_DONE = 3,
        TERMINATE = 4
    }

    public enum eQuestType
    {
        NONE = 0,
        MAIN = 1,
        SUB = 2,
        DAILY = 3,
        WEEKLY = 4,
        EVENT = 5,
        BATTLE_PASS = 6,
        HOLDER_PASS = 7,
        CHAIN = 10,
        TOWN = 99,
        TUTORIAL = 999,
    }

    public enum eQuestGroup
    {
        Normal = 0,
        Guild = 1,
    }

    public enum eQuestStartCondType
    {
        QUEST_CLEAR,
        TUTORIAL_CLEAR,
        //STAGE_CLEAR,
    }

    public enum eQuestCompleteCondType//괄호 안은 SUB_TYPE (default가 NONE)
    {
        NONE = -1,
        CHECK_DRAGON,       //(A,N,R,SR,UR,L)드래곤 특정 마리 수 보유하기
        CHECK_PET,          //(A,N,R,SR,UR,L)펫 특정 마리 수 보유하기
        CHECK_EQUIP,        //(A,N,R,SR,UR,L)장비 특정 수 보유하기

        CLEAR_QUEST,        //퀘스트 완료 체크
        TUTORIAL,           //튜토리얼 완료 체크
        BUILD_START,        //건물 건설 시작 체크

        BUILD,              //(BUILDING_KEY)건물 활성화 체크
        GAIN,               //(ITEM, ALL)특정 생산된 아이템 획득 체크
        GAIN_DOZER,         //골드 도저 받은 보상 횟수 체크
        STAGE_COMPLETE,     //스테이지 성공 체크

        LEVEL_ACCOUNT,      //현재 계정 레벨 체크
        LEVEL_DRAGON,       //드래곤 레벨 체크
        LEVEL_PET,          //펫 레벨 체크
        LEVEL_DRAGON_SKILL, //드래곤 스킬 레벨 체크
        LEVEL_TOWN,         //타운 레벨 체크

        ENHANCE_PET,        //(A,N,R,SR,UR,L)펫 강화 횟수 체크
        ENHANCE_EQUIP,      //(A,N,R,SR,UR,L)장비 강화 횟수 체크

        MERGE_DRAGON,       //드래곤 합성 횟수 체크
        MERGE_PET,          //펫 합성 횟수 체크
        MERGE_EQUIP,        //장비 합성 횟수 체크

        TRAVEL,             //(NONE, OVER - 특정 여행지 이상)여행사 보낸 횟수 체크
        DELIVERY,           //지하철 납품 횟수 체크
        REQUEST,            //의뢰 해결 횟수 체크
        ARENA,              //아레나 참여 횟수 체크
        DAY_DUNGEON,        //요일 던전 이용 횟수 체크
        GACHA_DRAGON,       //드래곤 뽑기 횟수 체크
        GACHA_PET,          //펫 뽑기 횟수 체크
        CONSUME_GOLD,       //골드 소모 수량 체크
        CONSUME_ENERGY,     //태엽 사용 횟수 체크
        EQUIPMENT_EQUIP,    //장비 장착 개수 체크
        EQUIPMENT_PET,      //장착 펫 마리 수 체크
        PRODUCE,            //(ITEM, ALL)특정 아이템 제작 시작 체크
        ADD_FLOOR,          //층 구매 횟수 체크

        EQUIP_DECOM,        //장비 분해 횟수 체크

        STAGE_CLEAR,        //스테이지 현시점부터 클리어 횟수 체크
        START,              //게임 최초 접속 시 퀘스트 출력하는 용도

        CLEAR_TYPE,         //trigger group 퀘스트에 대한 정의(SUB_TYPE -> DAILY, WEEKLY 등등 예정)

        DAILY_ALL_CLEAR_AD, //(DAILY)일일 퀘스트 모두 완료 
        ARENA_WIN_AD,       //아레나 승리 횟수 체크

        DAY_STAGE,          //(ALL)요일던전 현시점부터 클리어 횟수 체크
        GAIN_BATTERY,       //(ALL)생산된 모든 건전지 아이템 획득 체크

        PASS_START,         //배틀 패스 시즌 시작 체크
        HOLDER_START,       //홀더 패스 시즌 시작 체크

        ATTENDANCE,         //출석 체크
        GACHA_LUCKY,        // 럭키박스 가챠 횟수 체크

        GAIN_GEMDUNGEON,    // 젬 블록 던전에서 블럭 아무거나 획득
        BOSS_RAID,          //보스 레이드 참여 횟수 체크

        GUILD_JOIN,         //길드 소속 체크(길드 퀘스트 활성화 용도)
        DAILY_WEEKLY_CLEAR_QUEST,//길드 일일, 주간 퀘스트 완료 체크

        CHECK_TARGET_DRAGON, //특정 드래곤 획득
        LEVEL_TARGET_DRAGON, //특정 드래곤 레벨 체크
        LEVEL_TARGET_DRAGON_SKILL, //특정 드래곤 스킬 레벨 체크
        CHECK_TRANSCENDENCE_TARGET_DRAGON, //특정 드래곤 초월
    }

    public class QuestCondition
    {
        private static string[] stringToQuestCond = {
            "CHECK_DRAGON",
            "CHECK_PET",
            "CHECK_EQUIP",

            "CLEAR_QUEST",
            "TUTORIAL",
            "BUILD_START",

            "BUILD",
            "GAIN",
            "GAIN_DOZER",
            "STAGE_COMPLETE",

            "LEVEL_ACCOUNT",
            "LEVEL_DRAGON",
            "LEVEL_PET",
            "LEVEL_DRAGON_SKILL",
            "LEVEL_TOWN",

            "ENHANCE_PET",
            "ENHANCE_EQUIP",

            "MERGE_DRAGON",
            "MERGE_PET",
            "MERGE_EQUIP",

            "TRAVEL",
            "DELIVERY",
            "REQUEST",
            "ARENA",
            "DAY_DUNGEON",
            "GACHA_DRAGON",
            "GACHA_PET",
            "CONSUME_GOLD",
            "CONSUME_ENERGY",
            "EQUIPMENT_EQUIP",
            "EQUIPMENT_PET",
            "PRODUCE",
            "ADD_FLOOR",

            "EQUIP_DECOM",

            "STAGE_CLEAR",
            "START",

            "CLEAR_TYPE",

            "DAILY_ALL_CLEAR_AD",
            "ARENA_WIN_AD",

            "DAY_STAGE",
            "GAIN_BATTERY",

            "PASS_START",         //배틀 패스 시즌 시작 체크
			"HOLDER_START",       //홀더 패스 시즌 시작 체크

			"ATTENDANCE",			//출석 체크
			"GACHA_LUCKY",        // 럭키박스 가챠 횟수 체크

            "GAIN_GEMDUNGEON",
            "BOSS_RAID",        // 월드보스

            "GUILD_JOIN",
            "DAILY_WEEKLY_CLEAR_QUEST",

            "CHECK_TARGET_DRAGON", //특정 드래곤 획득
            "LEVEL_TARGET_DRAGON", //특정 드래곤 레벨 체크
            "LEVEL_TARGET_DRAGON_SKILL", //특정 드래곤 스킬 레벨 체크
            "CHECK_TRANSCENDENCE_TARGET_DRAGON", //특정 드래곤 초월
        };

        /// <summary>
        /// 광고 타입을 정의
        /// </summary>
        private static string[] stringToQuestAdvertiseType =
        {
            "DAILY_ALL_CLEAR_AD",
            "ARENA_WIN_AD",
        };
        public static List<string> strCondition
        {
            get { return stringToQuestCond.ToList(); }
        }
        static List<string> strAdvertiseTypeCondition
        {
            get { return stringToQuestAdvertiseType.ToList(); }
        }
        public static bool IsAdvertiseQuestType(string _type)
        {
            return strAdvertiseTypeCondition.Contains(_type);
        }
    }
    public enum eCollectionAchievementState//콜렉션 - 업적 통합 상태
    {
        UNDER_CONDITION = 0,
        NEW_COLLECTION = 1,
        PROCEEDING = 2,
        PROCESS_DONE = 3,
        TERMINATE = 4
    }
    public enum eDozerRewardType
    {
        GOLD,
        GEMSTONE,
        ITEM,
        ITEM_GROUP,

        MAX_COUNT
    }

    public enum eArenaRankGrade
    {
        NONE = 0,
        IRON2 = 1,
        IRON1 = 2,
        BRONZE2 = 3,
        BRONZE1 = 4,
        SILVER3 = 5,
        SILVER2 = 6,
        SILVER1 = 7,
        GOLD3 = 8,
        GOLD2 = 9,
        GOLD1 = 10,
        PLATINUM3 = 11,
        PLATINUM2 = 12,
        PLATINUM1 = 13,
        DIA3 = 14,
        DIA2 = 15,
        DIA1 = 16,
        MASTER4 = 17,
        MASTER3 = 18,
        MASTER2 = 19,
        MASTER1 = 20,
        GRAND_MASTER = 21,
        CHALLENGER = 22,
        CHAMPION = 23,
    }
    
    public enum eArenaWinType
    {
        None = 0,

        Open = 1,

        Offense = 2,

        Defense = 3,

        Draw = 4,

        REV_Success = 5,

        REV_Fail = 6,

        Max
    }

    public enum eChampionWinType
    {
        None = 0,

        Open = 1,

        SIDE_A_WIN = 2,

        SIDE_B_WIN = 3,

        INVALIDITY = 11,

        UNEARNED_WIN_A = 12,

        UNEARNED_WIN_B = 13,


        PENDING = 99,

        HIDE = 100,
    }

    public class eCombatLogType
    {
        public const string ATTACK_CAST = "AC",
        ATTACK_DAMAGE = "AD",
        ATTACK_DODGE = "AG",

        SKILL_CAST = "SC",
        SKILL_DAMAGE = "SD",
        SKILL_PERIODIC_DAMAGE = "SP",
        SKILL_DODGE = "SG",

        BUFF_APPLIED = "BA",
        BUFF_UPDATED = "BU",
        BUFF_REMOVED = "BR",
        DEBUFF_DODGE = "BG",
        DEBUFF_TICKDMG = "BT",

        TARGET_UPDATE = "TU";
    }

    public enum eArenaPos
    {
        NONE = 0,
        START = 1,

        TEAM1 = START,
        Offense1Bot = TEAM1,
        Offense1Top,
        Offense2Bot,
        Offense2Top,
        Offense3Bot,
        Offense3Top,
        TEAM1_MAX,

        TEAM2 = TEAM1_MAX,
        Defense1Bot = TEAM2,
        Defense1Top,
        Defense2Bot,
        Defense2Top,
        Defense3Bot,
        Defense3Top,
        TEAM2_MAX,

        MAX = TEAM2_MAX
    }
    public enum eChampionBattlePos
    {
        NONE = 0,
        START = 1,

        TEAM1 = START,
        Offense1Bot = TEAM1,
        Offense1Top,
        Offense2Bot,
        Offense2Top,
        Offense3Bot,
        Offense3Top,
        TEAM1_MAX,

        TEAM2 = TEAM1_MAX,
        Defense1Bot = TEAM2,
        Defense1Top,
        Defense2Bot,
        Defense2Top,
        Defense3Bot,
        Defense3Top,
        TEAM2_MAX,

        MAX = TEAM2_MAX
    }

    public class eWorldBoss
    {
        public const int POS_TOP_LEFT = 0,
        POS_TOP_RIGHT = 1, 
        POS_BOTTOM_LEFT = 2,
        POS_BOTTOM_RIGHT = 3,
        PRIORITY_NONE = 0,
        PRIORITY_BOTTON_LEFT = 1,
        PRIORITY_BOTTON_RIGHT = 2,
        PRIORITY_TOP_LEFT = 4,
        PRIORITY_TOP_RIGHT = 8,
        PRIORITY_LEFT = 16,
        PRIORITY_RIGHT = 32;
    }

    public class eArenaPosition
    {
        public const char Offense1Top = 'A',
        Offense1Bot = 'B',
        Offense2Top = 'C',
        Offense2Bot = 'D',
        Offense3Top = 'E',
        Offense3Bot = 'F',

        Defense1Top = 'a',
        Defense1Bot = 'b',
        Defense2Top = 'c',
        Defense2Bot = 'd',
        Defense3Top = 'e',
        Defense3Bot = 'f';
    }

    public enum eMatchListState
    {
        ERROR = 0,
        OPEN = 1,         // 미사용
        OFFENSE = 2,      // 공격팀 승리로 종료
        DEFENSE = 3,      // 방어팀 승리로 종료
        DRAW = 4,         // (미구현) 무승부
        REV_SUCCESS = 5,  // 공격 승리, 이후 복수 성공
        REV_FAIL = 6,     // 공격 승리, 이후 복수 실패
    }
    public enum eSpineAnimation
    {
        NONE = default,
        ATTACK,
        SKILL,
        WALK,
        IDLE,
        WIN,
        LOSE,
        CASTING,
        DEATH,
        HIT,
        A_CASTING,
        MAX
    }

    public enum eBattleSkillType
    {
        None,
        Normal,
        Skill1,
        Max
    }

    public enum eStageType
    {
        UNKNOWN = 0,
        ADVENTURE = 1,
        DAILY_DUNGEON = 2,
        WORLD_BOSS = 3,
    }
    public enum StageDifficult
    {
        NONE,
        NORMAL,
        HARD,
        HELL,
    }
    public enum eDailyDungeonWorldIndex
    {
        /// <summary>
        /// 월요일
        /// </summary>
        Mon = 101,
        /// <summary>
        /// 화요일
        /// </summary>
        Tue = 102,
        /// <summary>
        /// 수요일
        /// </summary>
        Wed = 103,
        /// <summary>
        /// 목요일
        /// </summary>
        Thu = 104,
        /// <summary>
        /// 금요일
        /// </summary>
        Fri = 105,
        /// <summary>
        /// 토요일
        /// </summary>
        Sat = 106,
        /// <summary>
        /// 일요일
        /// </summary>
        Sun = 107
    }

    public enum eDailyType
    {
        None = 0,
        /// <summary>
        /// 월요일
        /// </summary>
        Mon = 1,
        /// <summary>
        /// 화요일
        /// </summary>
        Tue = 2,
        /// <summary>
        /// 수요일
        /// </summary>
        Wed = 3,
        /// <summary>
        /// 목요일
        /// </summary>
        Thu = 4,
        /// <summary>
        /// 금요일
        /// </summary>
        Fri = 5,
        /// <summary>
        /// 토요일
        /// </summary>
        Sat = 6,
        /// <summary>
        /// 일요일
        /// </summary>
        Sun = 7
    }

    public enum eInvenSlotCheckContentType
    {
        Adventure,
        Travel,
        DailyDungeon,
        SlotTypeMax,
    }

    public enum eSkillEffectType
    {
        NONE = 0,

        /// <summary> 데미지 </summary>
        NORMAL_DMG = 1,
        /// <summary> 데미지 </summary>
        SKILL_DMG,
        /// <summary> 데미지 </summary>
        SKILL_ELEMENT_DMG,
        /// <summary> 데미지 </summary>
        SKILL_CRI_DMG,
        /// <summary> 데미지 </summary>
        DMG,

        /// <summary> '대상 이펙트 리소스 표시'만 사용하기 위한 타입 </summary>
        EFFECT = 10000,

        /// <summary> 버프 </summary>
        BUFF = 10100,
        BUFF_MAIN_ELEMENT,

        /// <summary> 디버프 </summary>
        DEBUFF = 10200,

        /// <summary> 스턴 </summary>
        STUN = 10600,
        /// <summary> 스턴 - 빙결 </summary>
        FROZEN,

        /// <summary> 에어본 </summary>
        AIRBORNE = 10700,

        /// <summary> 무적(타겟 잡힘) </summary>
        IMMUNE_DMG = 10800,
        /// <summary> 무적(타겟도 안잡힘) </summary>
        IMMUNE_HARM,

        /// <summary> 침묵 </summary>
        SILENCE = 10900,

        /// <summary> 틱데미지 </summary>
        TICK_DMG = 11000,
        /// <summary> 독 </summary>
        POISON,
        /// <summary> 도트데미지 </summary>
        DOT,

        /// <summary> 넉백 </summary>
        KNOCK_BACK = 11100,

        /// <summary> 체력회복 </summary>
        HEAL = 11200,

        /// <summary> 방어막 </summary>
        SHIELD = 11300,

        /// <summary> 도발 </summary>
        AGGRO = 11400,
        /// <summary> 캐스터 단일 도발 </summary>
        AGGRO_R,

        /// <summary> 모으기(끌어당기기) </summary>
        PULL = 11500,

        /// <summary> 웨이브동안 유지되는 스텟증가(결정됨) </summary>
        STAT = 11600,

        /// <summary> STAT_TYPE에 해당되는 버프종류들 제거 </summary>
        D_BUFF = 11700,
        /// <summary> 모든 버프 제거 </summary>
        D_ABUFF,

        /// <summary> STAT_TYPE에 해당되는 디버프종류들 제거 </summary>
        D_DEBUFF = 11800,
        /// <summary> 모든 디버프 제거 </summary>
        D_ADEBUFF,

        /// <summary> VALUE에 적혀있는 갯수만큼 도트 제거(먼저 걸린 순서대로) </summary>
        D_DOT = 11900,

        /// <summary> 대상의 STUN 상태를 제거한다 </summary>
        D_SHIELD = 12000,

        /// <summary> 대상의 STUN 상태를 제거한다 </summary>
        D_STUN = 12100,

        /// <summary> 대상의 AGGRO 상태를 제거한다 </summary>
        D_AGGRO = 12200,

        /// <summary> 아무 효과 없이 다음 SUMMON을 발동시키는 타입(안쓰기로 함 들어오면 버그) </summary>
        TRIGGER = 12300,

        /// <summary> 환경에 따라 발생하는 자연 버프 (아레나 버프 등) </summary>
        ENV_BUFF = 12400,

        /// <summary> 스턴 면역  </summary>
        IMN_STUN = 12500,

        /// <summary> 도발 면역 </summary>
        IMN_AGGRO = 12600,

        /// <summary> 에어본 면역 </summary>
        IMN_AIR = 12700,

        /// <summary> 당기기 면역 </summary>
        IMN_PULL = 12800,

        /// <summary> 넉백 면역 </summary>
        IMN_KNOCK = 12900,

        /// <summary> CC기 면역 </summary>
        IMN_CC = 13000,



        EFFECT_MAX,
    }
    [Flags]
    public enum eCharacterImmunity
    {
        NONE = 0,
        STUN = 1 << 0,
        AIRBORNE = 1 << 1,
        KNOCK_BACK = 1 << 2,
        AGGRO = 1 << 3,
        PULL = 1 << 4,
        SILENCE = 1 << 5,
        ALL = STUN | AIRBORNE | KNOCK_BACK | AGGRO | PULL | SILENCE,
        MAX
    }
    public enum eSkillEffectStartType
    {
        None = 0,
        Caster = 1,
        Enemy = 2
    }
    public enum eSkillEffectTarget
    {
        All = 0,
        Team = 1,
        TeamExceptMe = 2,
        Enemy = 3,
        Me = 4,
        EnemyFar = 5,
        AtkHighEnemy = 6
    }
    public enum eSkillTarget
    {
        /// <summary> 타겟 정보 활용안함 </summary>
        NONE = 0,
        /// <summary> 타겟 정보 활용 </summary>
        TARGET,
        /// <summary> 범위 내 타겟의 중심 </summary>
        CENTER,

        MAX
    }
    public enum eSkillTargetType
    {
        NONE = 0,

        /// <summary> 전원 </summary>
        ALL,
        /// <summary> 적 </summary>
        ENEMY,
        /// <summary> 아군(본인제외) </summary>
        ALLY,
        /// <summary> 아군 </summary>
        FRIENDLY,
        /// <summary> 본인 </summary>
        SELF,

        MAX
    }
    public enum eSkillTargetSort
    {
        NONE = 0,

        /// <summary> 가까운 </summary>
        NEARBY,
        /// <summary> 멀리있는 적 </summary>
        FAR,

        /// <summary> 최대 체력 높은 </summary>
        FHP_HIGH,
        /// <summary> 최대 체력 낮은 </summary>
        FHP_LOW,

        /// <summary> 현재 체력 높은 </summary>
        HP_HIGH,
        /// <summary> 현재 체력 낮은 </summary>
        HP_LOW,

        /// <summary> 공격력 높은 </summary>
        ATK_HIGH,
        /// <summary> 공격력 낮은 </summary>
        ATK_LOW,

        /// <summary> 방어력 높은 </summary>
        DEF_HIGH,
        /// <summary> 방어력 낮은 </summary>
        DEF_LOW,

        /// <summary> 빛 속성 대미지 높은 </summary>
        LIGHT_DMG_HIGH,
        /// <summary> 빛 속성 대미지 낮은 </summary>
        LIGHT_DMG_LOW,
        /// <summary> 어둠 속성 대미지 높은 </summary>
        DARK_DMG_HIGH,
        /// <summary> 어둠 속성 대미지 낮은 </summary>
        DARK_DMG_LOW,
        /// <summary> 물 속성 대미지 높은 </summary>
        WATER_DMG_HIGH,
        /// <summary> 물 속성 대미지 낮은 </summary>
        WATER_DMG_LOW,
        /// <summary> 불 속성 대미지 높은 </summary>
        FIRE_DMG_HIGH,
        /// <summary> 불 속성 대미지 낮은 </summary>
        FIRE_DMG_LOW,
        /// <summary> 바람 속성 대미지 높은 </summary>
        WIND_DMG_HIGH,
        /// <summary> 바람 속성 대미지 낮은 </summary>
        WIND_DMG_LOW,
        /// <summary> 땅 속성 대미지 높은 </summary>
        EARTH_DMG_HIGH,
        /// <summary> 땅 속성 대미지 낮은 </summary>
        EARTH_DMG_LOW,

        /// <summary> 크리 확률 높은 </summary>
        CRI_PROC_HIGH,
        /// <summary> 크리 확률 낮은 </summary>
        CRI_PROC_LOW,

        /// <summary> 크리 대미지 높은 </summary>
        CRI_DMG_HIGH,
        /// <summary> 크리 대미지 낮은 </summary>
        CRI_DMG_LOW,

        MAX
    }
    public enum eSkillCharCondition
    {
        NONE = 0,

        /// <summary> 쿨타임 제한 </summary>
        COOL_TIME,
        /// <summary> VALUE 체력 이하 일 때 </summary>
        HP_LOW,
        /// <summary> 아군이 사망했을 때 </summary>
        FRIEND_DIE,

        MAX
    }
    public enum eSkillSummonType
    {
        NONE = 0,

        /// <summary> 즉발시전 </summary>
        IMMEDIATELY,
        /// <summary> 투사체 </summary>
        ARROW,
        /// <summary> 관통형 투사체 </summary>
        PIERCE,
        /// <summary> 장판 </summary>
        LAND,
        /// <summary> 난사 </summary>
        RAPID_R,
        /// <summary> 어쎄씬 텔레포트 </summary>
        BACKSTAB,
        /// <summary> Spawn 데이터 참조하여 소환 </summary>
        SUMMON,
        /// <summary> 일반적인 타입이 아닌 코드 구현 </summary>
        SPECIAL,
        /// <summary> 돌진 </summary>
        CHARGE,

        MAX
    }
    public enum eSkillRangeType
    {
        NONE = 0,

        /// <summary> 중심 원 </summary>
        CIRCLE_C,
        /// <summary> 전방 원 </summary>
        CIRCLE_F,
        /// <summary> 중심 네모 </summary>
        SQUARE_C,
        /// <summary> 전방 네모 </summary>
        SQUARE_F,
        /// <summary> 전방 부체꼴 </summary>
        SECTOR_F,

        MAX
    }
    public enum eSkillTriggerType
    {
        NONE = 0,

        /// <summary> 조건 없이 다음 실행 </summary>
        NEXT,
        /// <summary> 현재 효과로 대상이 사망 </summary>
        DEAD,
        /// <summary> 현재 max_time이 종료된 후에 (사망 포함) (해제 미포함) </summary>
        END,
        /// <summary> 효과가 발생할 경우에 다음 Summon 진행 </summary>
        HIT,


        MAX
    }
    public enum eSkillResourceOrderType
    {
        /// <summary> 프리펩을 따라감 </summary>
        NONE = 0,
        /// <summary> 캐릭터에 직접 위치 </summary>
        CHAR,
        /// <summary> EffectLayer아래에 위치 </summary>
        WORLD,
    }
    public enum eSBObjectDirection
    {
        ALL,
        RIGHT,
        LEFT
    }
    public enum eSkillResourceOrder
    {
        BACK = 0,
        AUTO = 1,
        FRONT = 2,
    }
    public enum eSkillResourceFollow
    {
        NONE = 0,
        FOLLOW = 1
    }
    public enum eSkillResourceLocation
    {
        BOTTOM = 0,
        COLLIDER = 1,
        TOP = 2,
    }
    public enum eSkillCastingType
    {
        ENEMY = 0,
        FRIENDLY,
        FAR_ENEMY,
        ATK_H_ENEMY,
        MAX
    }
    public enum eSkillEffectDirectionType
    {
        All = 0,
        Front = 1,
        Back = 2
    }
    public enum eSkillPassiveGroupType
    {
        /// <summary> DEFAULT </summary>
        NONE = 0,

        /// <summary> 일반 </summary>
        COMMON = 1,
        /// <summary> 고급 </summary>
        UNCOMMON = 2,

        /// <summary> DEFAULT </summary>
        MAX
    }
    public enum eSkillPassiveEffect
    {
        /// <summary> DEFAULT </summary>
        NONE = 0,
        /// <summary> 스텟 증가 </summary>
        STAT,
        /// <summary> 자기 속성 데미지 증가 </summary>
        STAT_MAIN_ELEMENT,
        /// <summary> 페시브 발동 버프 </summary>
        BUFF,
        /// <summary> 자기 속성 버프 증가 </summary>
        BUFF_MAIN_ELEMENT,
        /// <summary> 페시브 발동 디버프 </summary>
        DEBUFF,
        /// <summary> 단발성으로 한순간 스킬 효과를 강화함, 지정된 stat을 강화시키고 효과 </summary>
        HIT,
        /// <summary> 대상이 가진 버프의 지속시간을 전체 시간대비 비율로 즉시 감소 </summary>
        REDUCE_COOLTIME,
        /// <summary> 대상이 가진 버프의 지속시간을 전체 시간대비 비율로 즉시 감소 </summary>
        REDUCE_BUFF,
        /// <summary> 대상이 가진 디버프의 지속시간을 전체 시간대비 비율로 즉시 감소 </summary>
        REDUCE_DEBUFF,
        /// <summary> 사용한 버프 스킬의 value값을 스킬 비율대로 증가시켜서 적용 </summary>
        STRONG_BUFF,
        /// <summary> 사용한 디버프 스킬의 value값을 스킬 비율대로 증가시켜서 적용 </summary>
        STRONG_DEBUFF,
        /// <summary> 스턴, 어그로, 에어본, 넉백 효과를 반사함, 스턴과 어그로는 max_time, 넉백과 에어본은 value값을 스킬 효과의 비율대로 반사 </summary>
        CC_REFLECT,
        /// <summary> 내가 받은 최종 대미지의 일정 비율을 반사함, 대미지의 즉시 반사이기 때문에 방어력이나 속성 등을 따질 필요 없음 </summary>
        DMG_REFLECT,
        /// <summary> 대상을 지정된 시간 동안 스킬 시전 불가하게 만듬, 동시에 시전하는 스킬을 끊어버림, cc기 전체 면역으로 면역 가능 및 해로운 효과 해제로 해제 가능 </summary>
        SILENCE,
        /// <summary> 자신 팀 방향으로 넉백하여 긴급탈출 </summary>
        R_KNOCK_BACK,
        /// <summary> DEFAULT </summary>
        MAX
    }
    public enum eSkillPassiveStartType
    {
        /// <summary> DEFAULT </summary>
        NONE = 0,
        /// <summary> 항상 발동 </summary>
        ALWAYS = 1,
        /// <summary> 피격시 발동 </summary>
        HIT = 2,
        /// <summary> 평타 공격시 발동 </summary>
        NORMAL_ATTACK = 3,
        /// <summary> 크리 공격시 발동 </summary>
        CRITICAL_ATTACK = 4,
        /// <summary> 스킬 공격시 발동 </summary>
        SKILL_ATTACK = 5,
        /// <summary> 상태이상에 걸릴 시 발동 </summary>
        ABNORMAL_STATUS = 6,
        /// <summary> DEFAULT </summary>
        MAX
    }
    public enum eSkillPassiveRateType
    {
        /// <summary> DEFAULT </summary>
        NONE = 0,
        /// <summary> 바로 적용 </summary>
        ALWAYS = 1,
        /// <summary> 시전자 체력이 높을 수록 </summary>
        CASTER_HP_UP = 2,
        /// <summary> 대상자 체력이 낮을 수록 </summary>
        TARGET_HP_DOWN = 3,
        /// <summary> 지정된 확률 </summary>
        PERCENTAGE = 4,
        /// <summary> 캐스터가 유리한 속성 상성일 때 </summary>
        CASTER_POSITIVE_ELEMENT = 5,
        /// <summary> 캐스터가 불리한 속성 상성일 때  </summary>
        CASTER_ADVERSE_ELEMENT = 6,
        /// <summary> 시전자 체력이 낮을 수록 </summary>
        CASTER_HP_DOWN = 7,
        /// <summary> DEFAULT </summary>
        MAX
    }
    public enum eContentType
    {
        /// <summary> DEFAULT </summary>
        NONE = 0,
        /// <summary> 전체 </summary>
        EVERY = 1,
        /// <summary> 아레나 </summary>
        ARENA = 2,
        /// <summary> 아레나 </summary>
        CHAMPION = 4,
        /// <summary> 아레나 </summary>
        WORLDBOSS = 8,
        /// <summary> DEFAULT </summary>
        MAX
    }
    public enum eSceneEffectType
    {
        None = 0,
        CloudAnimation,
        BlackBackground,
        BlockDoor,
    }

    public enum eEffectCustomType
    {
        None = 0,
        CameraShake,
        CameraFocusZoom,
        EffectClear,
        EffectBackground,
        EffectOutlineScale,
        EffectTimeScale
    }

    public enum eEffectCustomFocusObjectType
    {
        None = 0,
        Caster,
        Target,
        Projectile
    }

    [Flags]
    public enum eUIType
    {
        None = 0,

        Town = 1,
        Adventure = 2,
        Arena = 4,
        Gacha = 8,
        Daily = 16,
        TownEdit = 32,
        Battle_Adventure = 64,
        Battle_Arena = 128,
        Battle_Simulator = 256,
        Battle_Daily = 512,
        Battle_WorldBoss = 1024,
        WorldBoss = 2048,
        Battle_ChampionBattle = 4096,
        ChampionBattle = 8192,
    }

    [Flags]
    public enum ePriceDataFlag
    {
        None = 0,
        SubTitleLayer = 1,
        ContentBG = 2,
        CancelBtn = 4,
        CloseBtn = 8,
        Gold = 16,
        GemStone = 32,
        Default = SubTitleLayer | ContentBG | CancelBtn | Gold,
    }

    [Flags]
    public enum eAbnormalState
    {
        None = 0,
        Stun = 1,
        AirBone = 2,


        All = Stun | AirBone
    }

    public enum eElevatorMove
    {
        None,
        Up,
        Down
    }

    [Flags]
    public enum eToolTipDataFlag
    {
        None = 0,
        TITLE = 1,
        DESC = 2,
        Default = TITLE | DESC,
    }

    public class BuffList
    {
        private static string[] buffList = {
            "INCREASE_ATK",
            "INCREASE_ATK_PER",
            "INCREASE_DEF",
            "INCREASE_DEF_PER",
            "INCREASE_CRI_RATE_PER",
            "INCREASE_CRI_DMG_PER",
            "INCREASE_DODGE_PER",
            "INCREASE_HIT_PER",
            "DECREASE_ATK",
            "DECREASE_ATK_PER",
            "DECREASE_DEF",
            "DECREASE_DEF_PER",
            "DECREASE_CRI_RATE_PER",
            "DECREASE_CRI_DMG_PER",
            "DECREASE_DODGE_PER",
            "DECREASE_HIT_PER",
            "BUFF",
            "DEBUFF",
            "STUN",
            "AIRBORNE",
            "INVINCIBILITY",
            "SILENCE",
            "TICK_DMG",
        };

        public static List<string> strBuffList
        {
            get { return buffList.ToList(); }
        }
    }

    public enum eTownMissionType
    {
        Build,
        Gain,
        StageComplete,
        Level,
        Check,
        Enhance,
        Merge,
        Travel,
        Contents,
        Gacha,
        Consume,

        NONE,    //등록 안됨,
    }

    public class TownMissionType
    {
        static string[] townMissionList =
        {
            "build",
            "gain",
            "stage_complete",
            "level",
            "check",
            "enhance",
            "merge",
            "trip",
            "contents",
            "gacha",
            "consume",
        };

        public static List<string> strCondition
        {
            get { return townMissionList.ToList(); }
        }

        public static eTownMissionType GetType(string Type)
        {
            int index = townMissionList.ToList().IndexOf(Type);
            if (index >= 0)
                return (eTownMissionType)index;
            return eTownMissionType.NONE;
        }
    }

    public enum LandmarkSubwayPlantState
    {
        NONE = 0,
        LOCKED = 1,
        CAN_UNLOCK,
        READY,
        DELIVERING,
        DELIVER_COMPLETE
    }
    public enum eTravelState
    {
        None = 0,
        Normal,
        Travel,
        Complete
    }
    public enum eProducesState
    {
        UnKnown = -1,
        None = 0,
        Idle = 1,
        Ing = 2,
        Complete = 3,
    }

    [Flags]
    public enum eDragonState
    {
        Normal = 0,
        Not_Adventure = 1 << 0,
        Travel = 1 << 1,
        Arena = 1 << 2,
        GemDungeon = 1 << 3,
        Guild = 1 << 4,
    }
    public enum eUserPartState
    {
        None = 0,
        UnEquip,
        Equip,
    }
    public enum eUserPetState
    {
        None = 0,
        UnEquip,
        Equip,
    }

    public enum eSkillDimmedType
    {
        None,
        Water
    }

    // 타운맵을 구성하는 오브젝트들의 SortOrder
    public enum eTownMapSortOrder
    {
        Cell_Body_In = -3,
        Cell_Body_Out = 5,

        Wall_Default = -1,
        Wall_Hide = -3,

        Building = -2
    }

    public enum eProjectileOrder
    {
        NONE,
        FRONT,
        BACK
    }
    public enum eProjectileType
    {
        IMMEDIATE = 0,                      //즉시 공격

        PROJECTILE = 100,                   //일반 투사체
                                            //PROJECTILE_MULTI,					//한번에 여러명에게 날아가는 투사체
                                            //PROJECTILE_LOOP,					//한명에게 여러발 나가는 투사체
                                            //PROJECTILE_TIME_CT,					//관통효과가 있는 시간제한 투사체

        PARTICLE = 200,                     //자기 중심으로 뿌려지는 파티클
                                            //PARTICLE_ARRIVE,					//목표 중심으로 뿌려지는 파티클
                                            //PARTICLE_CT,						//목표 대상들의 중심에 뿌려지는 파티클
                                            //PARTICLE_FOLLOW,

        SPINE = 300,                        //자신에게 나오는 스파인
                                            //SPINE_ARRIVE,						//목표 중심에 나오는 스파인
                                            //SPINE_CT,							//목표 대상들의 중심에 나오는 스파인
        SPINE_NONSCALE_EFFECT_CT,           //방향 안바뀌고 Y좌표 중간에 나오는 일반 스파인 뒤에 이펙트가 목표 대상 중심으로 나오는 연출

        TELEPORT = 400,                     //상대 제일 뒤편의 적에게 순간이동하며 데미지

        //ANIM = 9999,						//테이블에만 존재하고 사용하지 않음
        //ARRIVE_ANIM,						//위와 동일
        //NON_TARGET,							//위와 동일
        //SPINE_TILE,							//위와 동일
    }
    public enum eProjectileActionType
    {
        NONE,
        MULTI,
        LOOP,
        TIME,
        RAPID,
        GENERIC,
        WORLD,
        MAX
    }
    public enum eProjectilePositionType
    {
        NONE,
        FOLLOW,
        ARRIVE,
        CENTER,
        CENTER_Y,
        MAX
    }
    public enum eBattleSide
    {
        START = 0,
        OffenseSide_1 = 0,    //공격자 -> 자신 or 우리팀
        DefenseSide_1 = 1,    //방어자 -> 몬스터 or 적팀
        OffenseSide_2 = 2,    //공격자 -> 자신 or 우리팀
        DefenseSide_2 = 3,    //방어자 -> 몬스터 or 적팀
        OffenseSide_3 = 4,    //공격자 -> 자신 or 우리팀
        DefenseSide_3 = 5,    //방어자 -> 몬스터 or 적팀
        OffenseSide_4 = 6,    //공격자 -> 자신 or 우리팀
        DefenseSide_4 = 7,    //방어자 -> 몬스터 or 적팀
        MAX
    }
    public enum eBattleType
    {
        NONE = 0,

        ADVENTURE = 1,
        ARENA = 2,
        DAILY = 3,
        GEM_DUNGEON = 4,
        WORLD_BOSS = 5,
        ChampionBattle = 6,

        MAX
    }
    #region STORE AND SHOP
    public enum eStoreType
    {
        NONE = 0,
        SHOP = 1,
        MILEAGE = 2,
        ARENA_POINT = 3,
        ASSET_STORE = 4,
        MAGNET_STORE = 5,
        TICKET_STORE = 6,
        FRIEND_POINT = 7,
        GUILD_POINT = 8,        
        MAX
    }

    public enum eShopMenuType
    {
        CLASS = 1,              //직업
        SPECIAL,                //스페셜혜택
        SUBSCRIBE,              //구독
        DAILY_BONUS,            //매일보너스
        DAILY_BENEFIT,          //매일 혜택
        WEEKLY_GROWTH,          //주간 성장
        WEEKLY_SKILL,           //주간 스킬
        MONTHLY_BENEFIT,        //월간 혜택
        TIME_DEAL,              //타임딜
        RESOURCE_SHOP,          //재화 상점
        MILEAGE_SHOP,           //마일리지 상점
        ARENA_POINT_SHOP,       //아레나 상점
        MAGNET_SHOP,            //마그넷 상점
        ENTRANCE_TICKET_SHOP,   //입장권 상점
        FRIEND_POINT_SHOP,      //우정 포인트 상점
        EVENT_SHOP,             //이벤트 상점
        GUILD_POINT_SHOP,       //길드 상점
    }

    public enum eShopType
    {
        UNKNOWN = -1,
        REGULAR = 1,
        SUBSCRIBE = 2,
        RANDOM = 3, // 아레나 포인트 상점 같이 랜덤으로 바뀌는 부류
        PRIVATE = 4,  // 개인화 조건형 상품
        LEVEL = 5, // 레벨 달성 방식 - 길드 레벨 달성시 뚫림
    }
    public enum eShopPageType
    {
        NONE = 0,
        MIDEUM = 1,
        SMALL = 2,
        LARGE = 3,
        SUBSCRIBE = 4
    }

    public enum eStoreSubscribeState
    {
        NONE = 0,
        /// <summary>
        /// 구독 안함
        /// </summary>
        NOT_SUB = 1,
        /// <summary>
        /// 구독 보상 수령 가능
        /// </summary>
        REWARD_ABLE = 2,
        /// <summary>
        /// 구독 보상 수령
        /// </summary>
        REWARDED = 3,

    }

    public enum eShopIAPCheckType
    {
        None = 0,
        Buy = 2,
        Refresh = 3,
        SubscribeLookUp = 6,
        SubscribeDailyReward = 7
    }
    public enum ePersonalGoodsConditionType
    {
        NONE = 0, // 0= 사용하지 않음
        TUTORIAL_END = 1, // 1= 튜토리얼 완료
        WORLD_CLEAR = 2, // 2= 월드 클리어
        ARENA_WIN_COUNT = 3, // 3 = 아레나 승리 수
        ARENA_RANK = 4, // 4 = 아레나 랭크
        BUILD_FINISH = 5, // 5 = 건설 완료
        TOWN_LEVEL = 6, // 6 = 타운 레벨
        DRAGON_GET = 7, // 7 = 캐릭터 획득
        GOLD_AMOUNT = 8, // 8 = 가지고 있는 골드량 
        GOLD_USE = 9, // 9 = 사용한 골드량
        FIRST_DRAGON_GET = 10, // 10 = 첫 드래곤 획득
        FIRST_PET_GET = 11, // 11 = 첫 펫 획득
        FIRST_ARENA_REVENGE_WIN = 12, // 12 = 아레나 첫 복수 완료
        GET_REWARD_AFTER_FIRST_PAY = 13, // 13 = 첫 결제 후 보상 수령
        FIRST_PAY = 14, // 14 = 첫 결제 후
        FIRST_PAY_MONTH = 15 // 15 = 월간 첫 결제 후
    }


    public enum BANNER_TYPE
    {
        UNKNOWN = -1,
        FULLSCREEN = 1,
        SMALL = 2,
    }

    public enum eBuyLimitType // 상품에 관한 구매 제한
    {
        UNLIMIT = 0, // 무제한
        ACCOUNT = 1, // 계정당
        DAILY = 2,  // 일일
        WEEK = 3,
        MONTH = 4,
        SEASON = 5,
    }

    public enum eSubscribeItemType
    {
        Default = 0,
        FirstSubscribeGet = 1,
        DailySubsscribeGet = 2

    }

    #endregion

    public enum eDailyRewardRarity  // 출석체크 보상 레어도
    {
        NONE = 0,
        NORMAL = 1,
        UNCOMMON = 2,
        RARE = 3,
    }
    public enum eActionType
    {
        UNKNOWN = -1,
        NONE = 0,
        URL_LINK = 1,
        SHOP_OPEN = 2,
        BANNER_OPEN = 3,
        GACHA_OPEN = 4,
        LEVEL_PASS = 5,
        ANNOUNCE_OPEN = 6,
        EVENT_ATTENDANCE = 7,
        EVENT_DICE = 8,
        EVENT_LUCKY_BAG = 9,
        EVENT_MINE = 10,
        EVENT_HOT_TIME_ADVENTURE = 11,
        EVENT_HOT_TIME_DAILYDUNGEON = 12,
        EVENT_HOT_TIME_WORLDBOSS = 13,
        EVENT_HOT_TIME_GEMDUNGEON = 14,
        OPEN_EVENT = 15,
        UNIONRAID_RANKING = 16,
        CHAMPIONEVENT_RANKING = 17,
        LUNASERVER_OPEN_EVENT = 18,
        RESTRICTED_AREA_EVENT = 19,
    }
    public enum eEventUseFlag
    {
        NOT_USE = 0,
        USE = 1,
        WEB2 = 1 << 1,//2
        WEB3 = 1 << 2,//4
        SERVER_0 = 1 << 3, //8
        SERVER_1 = 1 << 4, //16
        SERVER_2 = 1 << 5, //32
        SERVER_3 = 1 << 6, //64
    }
    public enum eIapState
    {
        PURCHASED = 0,
        CANCLE = 1
    }
    #region Battle Pass
    public enum eBattlePassState
    {
        NONE = 0,
        NOT_BUY = 1,
        BUY = 2,
        HOLDER = 3,
    }

    public enum eBattlePassType
    {
        NONE = 0,
        BATTLE = 1,
        HOLDER = 2,
    }
    public enum ePassUserType
    {
        DEFAULT = 0,
        PASS_BUY = 1,
        HOLDER = 2,
    }

    public enum eBattlePassRewardState
    {
        REWARD_DISABLE = 0, // 보상을 받을 수 없는 상태 - 일반 보상에서 레벨을 달성 못한 경우
        REWARD_ABLE = 1, // 보상을 수령할 수 있는 상태
        REWARDED = 2, // 일반 보상 혹은 스페셜 보상을 수령한 상태
        REWARDED_HOLDER = 3,  // 홀더 보상을 수령한 상태
        LOCK = 4,  // 홀더 아니면서 패스 구매 안해서 잠긴 아이템 
    }

    #endregion

    #region Mining & Miner

    // 채굴 관련 API OP
    public enum eMiningApiOp
    {
        DRILL_UPGRADE = 1,
        DRILL_UPGRADE_ACCELERATE,
        DRILL_INSERT,
        DRILL_EXCHANGE,
        USE_TICKET,
        GET_REWARD_MAGNET,
        GET_REWARD_TICKET,
    }

    // 채굴 상태
    public enum eMiningState
    {
        NONE = -1,  //채굴 시작 버튼 이지만, 회색 버튼
        START,      //시작으로 잡으면 되는지
        MINING,     //채굴 중
        WAIT_CLAIM, //마그넷 획득 가능 상태
        UPGRADE,    //현재 업글 중 <--- 삭제 처리 (건물 자체가 관리)
    }

    // 버프 티켓 종류 (eMinerGiftState와 가급적 동일하게 유지)
    public enum eMiningBuffTicketType
    {
        NONE = 0,

        BONUS = 1,
        SUPER_BONUS,
    }

    // 광부에게서 보상 티켓을 습득가능한지 여부
    public enum eMinerGiftState
    {
        NONE = -1,

        EMPTY,
        BONUS,
        SUPER_BONUS,
        BOTH,
    }

    // 광부의 NFT 타입
    public enum eMinerContractType
    {
        NONE,

        MTDZ,
    }

    // 광부 등급
    public enum eMinerState
    {
        NONE = -1,

        RESERVE,        // 예비
        PROBATION,      // 수습
        EXPERT,         // 숙련
        MASTER,         // 전문

        MAX_STATE_NUM
    }

    // 광부 리스트 필터 타입
    public enum eMinerFilterType
    {
        ALL,            // 모두보기

        RESERVE,        // 예비
        PROBATION,      // 수습
        EXPERT,         // 숙련
        MASTER,         // 전문
    }

    #endregion
    public enum eUIStatisticsEventType
    {
        DamageRecord,
        SkillCount,
        RecieveRecord,
        DeathRecord
    }

    public enum eUIStatisticsContentType
    {
        NONE,
        Arena,
        Adventure,
        DailyDungeon,
        WorldBoss,
        ChampionBattle,
    }

    public enum eGemDungeonSlotState
    {
        NONE,
        Empty,
        DragonExist,
        AddSlot,
        Lock
    }

    public enum eSlotCostInfoType
    {
        NONE = 0,
        Product = 1,
        GemDungeonDragon = 2,
    }

    public enum ePassiveRefreshType
    {
        NONE = 0,
        COMMON = 1,
        UNCOMMON = 2,
    }
    public enum eTutorialType
    {
        Main = 1,
        Sub = 2,
    }
    public enum TutorialDefine
    {
        Construct = 1001,
        Product = 2001,
        ConstructUI = 3001,
        ProductUI = 4001,
        DragonGacha = 5001,
        DragonManage = 6001,
        Battery = 7001,
        Adventure = 8001,
        MainTutorialEnd = Adventure,
        Travel = 9001,
        SubTutorialStart = Travel,
        Exchange = 10001,
        Arena = 11001,
        DailyDungeon = 12001,
        Subway = 13001,
        GemDungeon = 14001,
    }
    public enum ScriptTriggerType
    {
        NONE = -1,
        START = 0,
        STAGE_IN = 1,
        STAGE_CLEAR = 2,
        STAGE_MONSTER_SHOW = 3,
        QUEST_CLEAR = 4,
        CONSTRUCT_START = 5,
        CONSTRUCT_DONE = 6,
        FRIST_INIT = 7,
        ARENA_FIRST_INIT = 8,
        DAILYDUNGEON_FIRST_INIT = 9,
        INTRO = 10,
        TUTORIAL_START = 11,
        QUEST_START = 12,
        SCRIPT_END = 13,
        TUTORIAL_END = 14,
    }
    [Flags]
    public enum eEffectReceiverOption
    {
        NONE = 0,
        BACKGROUND = 1 << 0,
        CAMERA_SHAKE = 1 << 1,
        CAMERA_ZOOM = 1 << 2,
        OUTLINE_SCALE = 1 << 3,
        TIME_SCALE = 1 << 4,
        ALL = BACKGROUND | CAMERA_SHAKE | CAMERA_ZOOM | OUTLINE_SCALE | TIME_SCALE,
    }

    public enum eDamageStatisticsType
    {
        NONE = 0,
        AllDmg,
        SkillDmg,
        NormalDmg,

        SkillUseCnt,
        NormalUseCnt,

        BlockedSkillDmg,
        BlockedNormalDmg,

        BestSkillDmg,
        BestNormalDmg,

        AbsorbedDmg,
        RealRecvDmg,
        AliveTime,
    }

    public enum eArenaStatisticType
    {
        Default = 0,
        AllDmg = 1,
        Count = 2,
        BlockDmg = 3,
        BestDmg = 4,
        Etc = 5,
    }

    public enum eRateBoardType
    {
        None = 0,

        DragonMerge =100,
        DragonGacha = 102,

        PetMerge = 200,
        PetReinforce = 201,
        PetGacha = 202,
        PetMainOption = 203,
        PetSubOption = 204,

        AdventureReward = 300,

        //notused  = 500,

        GachaCoreBlock = 600,
        GachaGoldBox = 601,

        Travel = 900,

        PartMerge = 1000,
        PartReinforce = 1001,

    }

    public enum eGuildJoinType
    {
        None = 0,
        JoinRightNow = 1,
        JoinWait = 2,
        JoinDisable = 3,
    }

    public enum eGuildRankType
    {
        SumRanking = 0,
        WeeklyRanking = 1,
        MonthlyRanking = 2,
        UnifiedRanking = 7,
    }

    public enum eGuildRankRewardGroup
    {
        None = 0,
        GuildRank = 1,
        UserRank = 2,
    }

    public enum eGuildMissionType
    {
        Daily = 0,
        Weekly = 1,
        Left = 2,
    }

    public enum eGuildPosition
    {
        None = 0,
        Normal = 1,
        Operator = 1<<1,
        Leader = 1<<2,
    }

    public enum eGuildDonationAssetType
    {
        None = 0,
        Gold = 1,
        Dia100 = 2,
        Dia1000 = 3,
    }

    public enum eGuildLeaveState
    {
        UNKOWN = -1, //정보없음(길드 가입 전)
        None = 0, // 길드 안나감
        Leave = 1, // 자진해서 길드 나감
        Expel = 1<<1, // 길드 추방 당함
    }

    [Flags]
    public enum eGuildRecommendFilter
    {
        None = 0,
        ImmediateJoin = 1,
        ApplyJoin = 1<<1,
        All = ImmediateJoin | ApplyJoin,
    }

    public enum eGuildManageMode
    {
        Default = 0,
        Manage = 1,
        ChangeAuthority =2
    }

    public enum eGuildRankingPage
    {
        None = 0,
        Daily =1,   // 미사용 - 서버 enum 따라감
        Weekly = 2,   // 미사용 - 서버 enum 따라감
        Monthly =3,   // 미사용 - 서버 enum 따라감
        Cumulative =4,   // 미사용 - 서버 enum 따라감
        Guild = 5,
        Member = 6,
        Unified = 7,
    }

    public enum eGuildJoinLayerType
    {
        RecommendLayer = 0,
        ApplyLayer =1,
        RankingLayer = 2,
    }

    public enum ePortraitEtcType
    {
        NONE,
        RAID,
        CHAMPION,
    }
    [Flags]
    public enum ePortraitEtcUIType
    {
        NONE = 0,
        RAID = 1 << 0,
    }

    public enum eEventDailyRankingType
    {
        NONE = 0,
        DAILY_EVENT_DICE = 1, // 일일 이벤트 (주사위 랭킹)
        DAILY_EVENT_UNION_ARENA_WIN = 2, // 일일 이벤트 (조합 아레나 승리수 합산 랭킹)
        DAILY_EVENT_RAID = 3, // 일일 이벤트 (최대딜 랭킹)
        DAILY_EVENT_EXCHANGE = 4, // 일일 이벤트 (소원의 성소)
        DAILY_EVENT_ARENA_ATTACK = 5, // 일일 이벤트 (아레나 공격 승리)
                                      // const DAILY_EVENT_ARENA_DEFENCE = 5; // 일일 이벤트 (아레나 방어전 승리)
        DAILY_EVENT_ACCOUNT_EXP = 6, // 일일 이벤트 (계정 경험치 최다 획득 랭킹)
                                           // const DAILY_EVENT_BOX_OPEN = 5; // 일일 이벤트 (상자 오픈 랭킹)
                                           // const DAILY_EVENT_PART_MERGE_SUCCESS = 6; // 일일 이벤트 (장비 합성 랭킹)
                                           // const EVENT_DICE = 100;
                                           // const EVENT_BOX_OPEN = 101;
                                           // const EVENT_PVP_RANKING = 102;
    }
}

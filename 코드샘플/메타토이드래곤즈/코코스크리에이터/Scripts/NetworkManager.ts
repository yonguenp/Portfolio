
import { _decorator, Enum, sys } from 'cc';
import { TableManager } from './Data/TableManager';
import { GameManager } from './GameManager';
import { AreaExpansionTable, AreaLevelTable, WorldTripTable } from './Data/AreaTable';
import { BuildingBaseTable, BuildingLevelTable, BuildingOpenTable } from './Data/BuildingTable';
import { InventoryTable, SlotCostTable } from './Data/InventoryTable';
import { DefineResourceTable, ItemBaseTable, ItemGroupTable } from './Data/ItemTable';
import { ProductAutoTable, ProductTable } from './Data/ProductTable';
import { StringTable } from './Data/StringTable';
import { TimeManager } from './Time/TimeManager';
import { User, UserDragon } from './User/User';
import { MapManager } from './Map/MapManager';
import { GetType, ObjectCheck, Type } from './Tools/SandboxTools';
import { Starter } from './Starter';
import { eApiResCode } from './Tools/CommonEnum';
import { SystemPopup } from './UI/SystemPopup';
import { SubwayDeliveryTable, SubwayPlatformTable } from './Data/SubwayTable';
import { IManagerBase } from 'sb';
import { AccountTable } from './Data/AccountTable';
import { CharBaseTable, CharExpTable, CharGradeTable } from './Data/CharTable';
import { StatFactorTable } from './Data/StatTable';
import { ElementRateTable } from './Data/ElementTable';
import { MonsterBaseTable, MonsterSpawnTable } from './Data/MonsterTable';
import { SkillCharTable, SkillEffectTable, SkillProjectileTable } from './Data/SkillTable';
import { StageBaseTable } from './Data/StageTable';
import { WorldBaseTable } from './Data/WorldTable';
import { GachaListTable, GachaShopTable } from './Data/GachaTable';
import { PartTable } from './Data/PartTable';
import { PartOptionTable, PartReinforceTable, PartSetTable } from './Data/PartOptionTable';
import { TutorialTable } from './Data/TutorialTable';
import { QuestTable, QuestTriggerTable } from './Data/QuestTable';
import { QuestManager } from './QuestManager';
import { TutorialManager } from './Tutorial/TutorialManager';
import { PopupManager } from './UI/Common/PopupManager';
import { ToastMessage } from './UI/ToastMessage';

/**
 * Predefined variables
 * Name = NetworkManager
 * DateTime = Mon Jan 10 2022 15:26:23 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = NetworkManager.ts
 * FileBasenameNoExtension = NetworkManager
 * URL = db://assets/Scripts/NetworkManager.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
export class NetworkManager implements IManagerBase {
    public static Name: string = "NetworkManager";
    protected static instance: NetworkManager = null;
    protected sid: string = "";

    public static get Instance() {
        if(NetworkManager.instance == null) {
            return NetworkManager.instance = new NetworkManager();
        }
        return NetworkManager.instance;
    }

    private uiUpdater : () => void = null
    set UIUpdater(func : () => void)
    {
        this. uiUpdater = func
    }


    Init(): void {
        GameManager.Instance.AddManager(this, false);
    }

    GetManagerName(): string {
        return NetworkManager.Name;
    }

    Update(deltaTime: number): void {
    }
    

    public static Send(reqHtml: string, params: {}, callback: Function) {
        params['uno'] = User.Instance.UNO;
        params['sid'] = NetworkManager.Instance.sid;
        console.log(reqHtml, params);
        NetworkManager.Instance.sendXmlHttp(reqHtml, params, callback);
    }

    protected sendXmlHttp(reqHtml: string, params: {}, callback: Function): void
    {
        var xmlhttp = new XMLHttpRequest();
        xmlhttp.onreadystatechange = function()
        {
            if(this.readyState == 4)
            {
                if(this.status == 200)
                {
                    if(this.responseText != "")
                    {
                        const jsonData = JSON.parse(this.responseText);

                        console.log(jsonData);
                        if (NetworkManager.Instance.ErrorCheck(jsonData)) {
                            NetworkManager.Instance.GetTimeRefresh(jsonData);
                            NetworkManager.PushResponse(jsonData);
                            NetworkManager.CheckRsMessage(jsonData)
                            callback(jsonData);
                        }
                    }
                    else
                    {
                        alert("응답 오류, 오류 내용 : " + this.status);
                    }
                }
                else
                {
                    alert("응답 오류, 오류 내용 : " + this.status);
                    console.log(this.statusText);
                    console.log(this.responseText);
                }                       
            }
        }
        xmlhttp.open("POST", `http://sandbox-gs.mynetgear.com:52580/api/${reqHtml}`, true);//"/operation/Coupon/Router?op="+opCode+paramStr, true);

        let data = new FormData();

        const keys = Object.keys(params);
        const keyCount = keys.length;
        for(var i = 0 ; i < keyCount ; i++) {
            const key = keys[i];
            const value = params[key];
            if(Array.isArray(value)) {
                data.append(key, JSON.stringify(value));
            } else {
                data.append(key, value);
            }
        }

        xmlhttp.send(data);
    }

    public static PushResponse(jsonData: any) {
        if (ObjectCheck(jsonData, 'push')) {
            const array: [] = jsonData['push'];

            if(array != null) {
                array.forEach(obj => {
                    if (ObjectCheck(obj, 'api')) {
                        switch(obj['api']) {
                            case 'session_id': {
                                if (ObjectCheck(obj, 'sid')) {
                                    NetworkManager.Instance.sid = obj['sid'];
                                }
                            } break;
                            case 'exp_update':
                            case 'energy_update':
                            case 'gold_update': {
                                User.Instance.UserData.Set(obj);
                                if (ObjectCheck(obj, 'gold') || ObjectCheck(obj, 'energy') || ObjectCheck(obj, 'exp')) {
                                    if(!(NetworkManager.Instance.uiUpdater == undefined || NetworkManager.Instance.uiUpdater == null)) {
                                        NetworkManager.Instance.uiUpdater();
                                    }
                                    //UICanvas.RefershUI();
                                }
                            } break;
                            case 'item_update': {
                                if (ObjectCheck(obj, 'data')) {
                                    const array: [] = obj['data'];
                                    const count = array.length;

                                    for(var i = 0 ; i < count ; i++) {
                                        const itemData = array[i];
                                        const itemNo = itemData[0];
                                        const itemCount = itemData[1];

                                        User.Instance.UpdateItem(itemNo, itemCount);
                                    }
                                }
                            } break;
                            case 'building_update': {
                                if (ObjectCheck(obj, 'tag') && ObjectCheck(obj, 'state') && ObjectCheck(obj, 'level')) {
                                    var time = undefined;
                                    if(ObjectCheck(obj, 'construct_exp')) {
                                        time = obj['construct_exp'];
                                    }
                                    User.Instance.UpdateBuilding(obj['tag'], obj['state'], obj['level'], time);
                                }
                                GameManager.GetManager<MapManager>(MapManager.Name).UpdateBuilding(obj);
                            } break;
                            case 'exterior_update': {
                                User.Instance.UpdateGrid(obj);
                                User.Instance.SetData();
                                GameManager.GetManager<MapManager>(MapManager.Name).UpdateFloor();
                            } break;
                            case 'produce_update': {
                                User.Instance.UpdateProduces(obj);
                            } break;
                            case 'landmark_update': {
                                User.Instance.UpdateLandmark(obj);
                            } break;
                            case 'dragon_update': {
                                console.log("dragon update!");
                                if (ObjectCheck(obj, 'data')) {
                                    const array: [] = obj['data'];
                                    const count = array.length;

                                    for(var i = 0 ; i < count ; i++) {
                                        const dragonData = array[i] as any;
                                        const dragonTag = dragonData.id;
                                        const dragonEXP = dragonData.exp;
                                        const dragonLevel = dragonData.lvl;
                                        const dragonState = dragonData.state;
                                        const dragonParts = dragonData.parts;
                                        const dragonSkillLevel = dragonData.slvl;
                                        const dragonObtainTime = dragonData.obtain;

                                        let getdragonTempData = User.Instance.DragonData.GetDragon(dragonTag);
                                        let userdragonTempData = new UserDragon();
                                        if (getdragonTempData != null) {
                                            userdragonTempData = getdragonTempData;
                                        }

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
                                }
                            }break;
                            case 'dragon_exp_update':{
                                if (ObjectCheck(obj, 'data')) {
                                    const array: [] = obj['data'];
                                    const count = array.length;

                                    for(var i = 0 ; i < count ; i++) {
                                        const dragonData = array[i] as any;
                                        const dragonTag = dragonData.id;
                                        const dragonLevel = dragonData.lvl;
                                        const dragonEXP = dragonData.exp;

                                        let getdragonTempData = User.Instance.DragonData.GetDragon(dragonTag);
                                        let userdragonTempData = new UserDragon();
                                        userdragonTempData = getdragonTempData;
                                        userdragonTempData.Level = dragonLevel;
                                        userdragonTempData.Exp = dragonEXP;

                                        User.Instance.DragonData.AddUserDragon(dragonTag,userdragonTempData);
                                    }
                                }
                            }break;
                            case 'part_update':{
                                if (ObjectCheck(obj, 'data')) {
                                    const array: [] = obj['data'];
                                    const count = array.length;
                                    
                                    array.forEach(element =>
                                    {
                                        if(element)
                                        {
                                            User.Instance.partData.AddUserPart(element);
                                        }
                                        else
                                        {
                                            //업데이트

                                        }
                                    });
                                    //console.log(array);
                                }
                            }break;
                            case 'quest_update':
                            {
                                if(ObjectCheck(obj, 'data'))
                                {
                                    QuestManager.NewQuestCheck(jsonData);
                                    QuestManager.ProgressUpdate(obj);
                                }
                            }break;

                            case 'tutorial_update':
                            {
                                if (ObjectCheck(obj, 'data')) {
                                    if(GetType(obj['data']) == Type.Array)
                                    {
                                        TutorialManager.achiveList = obj['data'];
                                    }
                                }
                            }break;
                            case 'adventure_update' ://웨이브 종료 시 월드 변경 데이터 전체가 들어옴
                            {
                                if (ObjectCheck(obj, 'info')) {
                                    const array: [] = obj['info'];
                                    if(array == null || array.length <= 0){
                                        return;
                                    }
                                    const count = array.length;

                                    for(var i = 0 ; i < count ; i++) {
                                        const data = array[i] as any;
                                        let worldIndex = data.world;
                                        let worldDiff = data.diff;
                                        User.Instance.PrefData.AdventureProgressData.SetWorldInfoData(worldIndex,worldDiff,data);
                                    }
                                }
                            }break;
                        }
                    }
                });
            }
        }
    }

    public static CheckRsMessage(jsonData: any) {
        if (ObjectCheck(jsonData, 'rs') && jsonData.rs != 0 && Enum.getList) 
        {
            let systemMessage = ""

            switch(jsonData['rs'])
            {
                case eApiResCode.GENERIC_SERVER_FAIL: systemMessage=StringTable.GetString(100000634, "알 수 없는 오류 발생"); break;
                case eApiResCode.SQL_ERROR: systemMessage=StringTable.GetString(100000636, "통신 오류 발생") + ` : ${eApiResCode.SQL_ERROR}`; break;                   
                case eApiResCode.NETWORK_ERROR: systemMessage=StringTable.GetString(100000636, "통신 오류 발생") + ` : ${eApiResCode.SQL_ERROR}`; break;               
                case eApiResCode.SCRIPT_INCLUDE_FAIL: systemMessage=StringTable.GetString(100000635, "데이터 오류 발생"); break;         
                case eApiResCode.REDIS_ERROR: systemMessage=StringTable.GetString(100000636, "통신 오류 발생") + ` : ${eApiResCode.SQL_ERROR}`; break;                 
                case eApiResCode.SERVER_BUSY: systemMessage=StringTable.GetString(100000637, "통신 타임아웃 발생"); break;                 
            
                // 10 ~ : common errors
                case eApiResCode.SESSIONID_NOT_MATCH: systemMessage=StringTable.GetString(100000638, "세션 오류 발생"); break;        
                case eApiResCode.PARAM_ERROR: systemMessage=StringTable.GetString(100000639, "잘못된 통신 정보입니다."); break;                
                //case eApiResCode.NO_CHANGE: systemMessage=""; break;
                case eApiResCode.INVENTORY_FULL: systemMessage=StringTable.GetString(100000640, "인벤토리가 가득찼습니다."); break;
                case eApiResCode.COST_SHORT: systemMessage=StringTable.GetString(100000619, "재료 비용 부족"); break;
            
                // 100 ~ : accounts
                // case eApiResCode.AUTH_FAILED: systemMessage=""; break;
                // case eApiResCode.ACCOUNT_EXISTS: systemMessage=""; break;
                // case eApiResCode.NICKNAME_DUPLICATES: systemMessage=""; break;
                // case eApiResCode.INVALID_NICK_LENGTH: systemMessage=""; break;
                // case eApiResCode.INVALID_NICK_CHAR: systemMessage=""; break;
                // case eApiResCode.ACCOUNT_NOT_EXISTS: systemMessage=""; break;
                // case eApiResCode.ID_STRING_TOO_SHORT: systemMessage=""; break;
                // case eApiResCode.ID_STRING_TOO_LONG: systemMessage=""; break;
                // case eApiResCode.ID_STRING_INVALID_ENCODING: systemMessage=""; break;
                // case eApiResCode.ID_STRING_INVALID_NUMERIC: systemMessage=""; break;
                // case eApiResCode.ID_STRING_MIXED_LANGS: systemMessage=""; break;
                // case eApiResCode.ID_STRING_FILTERED: systemMessage=""; break;
                // case eApiResCode.ID_DUPLICATES: systemMessage=""; break;
            
                // 200 ~ : building
                case eApiResCode.ALREADY_BUILT: systemMessage=StringTable.GetString(100000641, "이미 건설한 건물 건설 시도"); break;
                case eApiResCode.YOU_CANT_BUILD_THERE: systemMessage=StringTable.GetString(100000642, "설치 할 수 없는 건물 슬롯"); break;
                case eApiResCode.EXTERIOR_LEVEL_TOO_LOW: systemMessage=StringTable.GetString(100000643, "외형 레벨 조건 미달"); break;
                case eApiResCode.CONSTRUCT_COST_NOT_MET: systemMessage=StringTable.GetString(100000619, "재료 비용 부족"); break;
                case eApiResCode.CONSTRUCTION_ONGOING: systemMessage=StringTable.GetString(100000644, "이미 건설중인 건물입니다."); break;
                case eApiResCode.BUILDING_NOT_EXISTS: systemMessage=StringTable.GetString(100000645, "존재하지 않는 건물 입니다."); break;
            
                // 300 ~ : produce
                case eApiResCode.BUILDING_STATE_NOT_MET: systemMessage=StringTable.GetString(100000646, "건설 조건이 충분하지 않습니다."); break;
                case eApiResCode.INVALID_RECIPE_ID: systemMessage=StringTable.GetString(100000647, "올바르지 않은 생산 레시피"); break;
                case eApiResCode.REQUIRED_LEVEL_NOT_MET: systemMessage=StringTable.GetString(100000646, "건설 조건이 충분하지 않습니다."); break;
                case eApiResCode.PRODUCE_SLOT_FULL: ToastMessage.Set(StringTable.GetString(100000813, "생산 슬롯이 가득찼습니다. 슬롯을 확장해주세요.")); return;
                case eApiResCode.NOTHING_TO_HARVEST: ToastMessage.Set(StringTable.GetString(100000812, "생산 완료된 아이템이 없습니다.")); return;     
            }

            if(systemMessage != "")
            {
                let popup = PopupManager.OpenPopup("SystemPopupOK") as SystemPopup;
                popup.setMessage(StringTable.GetString(100000618, "오류"), systemMessage);
            }            
        }
    }

    public static GetAllData() {
        let hashes = undefined;//임시처리 무조건 전채갱신
        // let hashes = sys.localStorage.getItem('hashes');

        if(hashes == undefined) {
            NetworkManager.Send("data/getall", {}, NetworkManager.GetAllDataResponse);
            return;
        }

        let jsonHashs = JSON.parse(hashes);
        if(jsonHashs != null) {
            NetworkManager.Send("data/filehash", {}, NetworkManager.FileHashResponse);
            return;
        }
    }

    public static GetAllDataResponse(jsonData: any) {
        NetworkManager.Instance.GetAllData(jsonData);
        GameManager.Instance.isNetworkLoaded = true;
    }

    public static FileHashResponse(jsonData: any) {
        if(jsonData['hashes'] != undefined) {
            const checkList = NetworkManager.Instance.CheckHash(jsonData['hashes']);
            const checkCount = checkList.length;
            if(checkCount < 1) {
                if(NetworkManager.Instance.LoadData()) {
                    GameManager.Instance.isNetworkLoaded = true;
                }
                return;
            }

            const param = checkList.join(',');
            NetworkManager.Send("data/getsome", { files: param }, NetworkManager.GetAllDataResponse);
            return;
        }
    }

    protected GetAllData(jsonData: any) {
        this.SetHash(jsonData['hashes']);
        this.SetData(jsonData['data']);
        this.DataLoad(jsonData['hashes'], jsonData['data']);
    }

    protected DataLoad(jsonHashs: any, jsonData: any): boolean {
        if (jsonData == undefined || jsonHashs == undefined) {
            console.log('datas', jsonData, 'hashes', jsonHashs);
            return false;
        }
        const hashKeys = Object.keys(jsonHashs);
        const keys = Object.keys(jsonData);
        const hashesCount = hashKeys.length;
        const keysCount = keys.length;
        if(keysCount < 1) {
            return false;
        }

        // if(hashesCount != keysCount) 
        // {
        //     console.log("hashesCount != keysCount")   
        //     return false;
        // }

        for(var i = 0 ; i < keysCount ; i++) {
            const curKey = keys[i];
            if (jsonData[curKey] != undefined) {
                if(jsonData[curKey] == jsonHashs[curKey] || jsonData[curKey] == "") {
                    return false;
                }
                switch(curKey) {
                    case 'area_expansion': {
                        TableManager.GetTable(AreaExpansionTable.Name).SetTable(jsonData['area_expansion']);
                    } break;
                    case 'area_level': {
                        TableManager.GetTable(AreaLevelTable.Name).SetTable(jsonData['area_level']);
                    } break;

                    case 'building_base': {
                        TableManager.GetTable(BuildingBaseTable.Name).SetTable(jsonData['building_base']);
                    } break;
                    case 'building_level': {
                        TableManager.GetTable(BuildingLevelTable.Name).SetTable(jsonData['building_level']);
                    } break;
                    case 'building_open': {
                        TableManager.GetTable(BuildingOpenTable.Name).SetTable(jsonData['building_open']);
                    } break;
                    case 'building_product': {
                        TableManager.GetTable(ProductTable.Name).SetTable(jsonData['building_product']);
                    } break;
                    case 'building_product_auto': {
                        TableManager.GetTable(ProductAutoTable.Name).SetTable(jsonData['building_product_auto']);
                    } break;
                    
                    case 'define_resource': {
                        TableManager.GetTable(DefineResourceTable.Name).SetTable(jsonData['define_resource']);
                    } break;
                    
                    case 'inventory': {
                        TableManager.GetTable(InventoryTable.Name).SetTable(jsonData['inventory']);
                    } break;
                    case 'item_base': {
                        TableManager.GetTable(ItemBaseTable.Name).SetTable(jsonData['item_base']);
                    } break;
                    case 'item_group': {
                        TableManager.GetTable(ItemGroupTable.Name).SetTable(jsonData['item_group']);
                    } break;
                    
                    case 'slot_cost': {
                        TableManager.GetTable(SlotCostTable.Name).SetTable(jsonData['slot_cost']);
                    } break;
                    case 'string': {
                        TableManager.GetTable(StringTable.Name).SetTable(jsonData['string']);
                    } break;
                    
                    case 'subway_delivery': {
                        TableManager.GetTable(SubwayDeliveryTable.Name).SetTable(jsonData['subway_delivery']);
                    } break;
                    case 'subway_platform': {
                        TableManager.GetTable(SubwayPlatformTable.Name).SetTable(jsonData['subway_platform']);
                    } break;

                    case 'world_trip': {
                        TableManager.GetTable(WorldTripTable.Name).SetTable(jsonData['world_trip']);
                    } break;

                    case 'account_exp': {
                        TableManager.GetTable(AccountTable.Name).SetTable(jsonData['account_exp']);
                    } break;

                    case 'char_base': {
                        TableManager.GetTable(CharBaseTable.Name).SetTable(jsonData['char_base']);
                    } break;
                    case 'char_grade': {
                        TableManager.GetTable(CharGradeTable.Name).SetTable(jsonData['char_grade']);
                    } break;
                    case 'char_exp': {
                        TableManager.GetTable(CharExpTable.Name).SetTable(jsonData['char_exp']);
                    } break;
                    
                    case 'stat_factor': {
                        TableManager.GetTable(StatFactorTable.Name).SetTable(jsonData['stat_factor']);
                    } break;
                    
                    case 'element_rate': {
                        TableManager.GetTable(ElementRateTable.Name).SetTable(jsonData['element_rate']);
                    } break;
                    
                    case 'monster_base': {
                        TableManager.GetTable(MonsterBaseTable.Name).SetTable(jsonData['monster_base']);
                    } break;
                    case 'monster_spawn': {
                        TableManager.GetTable(MonsterSpawnTable.Name).SetTable(jsonData['monster_spawn']);
                    } break;
                    
                    case 'skill_char': {
                        TableManager.GetTable(SkillCharTable.Name).SetTable(jsonData['skill_char']);
                    } break;
                    case 'skill_effect': {
                        TableManager.GetTable(SkillEffectTable.Name).SetTable(jsonData['skill_effect']);
                    } break;
                    case 'skill_projectile': {
                        TableManager.GetTable(SkillProjectileTable.Name).SetTable(jsonData['skill_projectile']);
                    } break;

                    case 'world_base': {
                        TableManager.GetTable(WorldBaseTable.Name).SetTable(jsonData['world_base']);
                    } break;
                    case 'stage_base': {
                        TableManager.GetTable(StageBaseTable.Name).SetTable(jsonData['stage_base']);
                    } break;
                    case 'gacha_shop': {
                        TableManager.GetTable(GachaShopTable.Name).SetTable(jsonData['gacha_shop']);
                    } break;
                    case 'gacha_list': {
                        TableManager.GetTable(GachaListTable.Name).SetTable(jsonData['gacha_list']);
                    } break;
                    case 'part_base': {
                        TableManager.GetTable(PartTable.Name).SetTable(jsonData['part_base']);
                    } break;
                    case 'part_option': {
                        TableManager.GetTable(PartOptionTable.Name).SetTable(jsonData['part_option']);
                    } break;
                    case 'part_set': {
                        TableManager.GetTable(PartSetTable.Name).SetTable(jsonData['part_set']);
                    } break;
                    case 'part_reinforce': {
                        TableManager.GetTable(PartReinforceTable.Name).SetTable(jsonData['part_reinforce']);
                    } break;
                    case 'tutorial_base': {
                        TableManager.GetTable(TutorialTable.Name).SetTable(jsonData['tutorial_base']);
                    } break;
                    case 'quest_base': {
                        TableManager.GetTable(QuestTable.Name).SetTable(jsonData['quest_base']);
                    } break;
                    case 'quest_trigger_group': {
                        TableManager.GetTable(QuestTriggerTable.Name).SetTable(jsonData['quest_trigger_group']);
                    } break;
                }
            }
        }
        return true;
    }

    LoadData(): boolean {
        let hashes = this.GetHash();
        let datas = this.GetData();
        if(!this.DataLoad(hashes, datas)) {
            NetworkManager.Send("data/getall", {}, NetworkManager.GetAllDataResponse);
            return false;
        }
        return true;
    }

    SetData(jsonData: any): void {
        let datas = this.GetData();

        if(jsonData != undefined) {
            const keys = Object.keys(jsonData);
            const keysCount = keys.length;

            for(var i = 0 ; i < keysCount ; i++) {
                const curKey = keys[i];
                if (jsonData[curKey] != undefined) {
                    datas[curKey] = jsonData[curKey];
                }
            }

            sys.localStorage.setItem('datas', JSON.stringify(datas));
        }
    }

    GetData(): Object {
        let hashes: Object = null;
        let stringHashes = sys.localStorage.getItem('datas');
        if(stringHashes == undefined) {
            hashes = {};
        } else {
            hashes = JSON.parse(stringHashes);
        }
        return hashes;
    }

    CheckHash(jsonData: any): string[] {
        let hashes = this.GetHash();
        let refreshHashes: string[] = [];

        if(jsonData != undefined) {
            const keys = Object.keys(jsonData);
            const keysCount = keys.length;

            for(var i = 0 ; i < keysCount ; i++) {
                const curKey = keys[i];
                if (jsonData[curKey] != undefined && hashes[curKey] != undefined) {
                    if (hashes[curKey] != jsonData[curKey]) {
                        refreshHashes.push(curKey);
                    }
                } else if(curKey != undefined && curKey != "") {
                    refreshHashes.push(curKey);
                }
            }
        }

        return refreshHashes;
    }

    SetHash(jsonData: any): void {
        let hashes = this.GetHash();

        if(jsonData != undefined) {
            const keys = Object.keys(jsonData);
            const keysCount = keys.length;

            for(var i = 0 ; i < keysCount ; i++) {
                const curKey = keys[i];
                if (jsonData[curKey] != undefined) {
                    hashes[curKey] = jsonData[curKey];
                }
            }

            sys.localStorage.setItem('hashes', JSON.stringify(hashes));
        }
    }

    GetHash(): Object {
        let hashes: Object = null;
        let stringHashes = sys.localStorage.getItem('hashes');
        if(stringHashes == undefined) {
            hashes = {};
        } else {
            hashes = JSON.parse(stringHashes);
        }
        return hashes;
    }

    protected ErrorCheck(jsonData: any): boolean {
        if (!ObjectCheck(jsonData, 'err') || jsonData['err'] != 0) {
            console.log('<error> ErrorCheck', jsonData);
            return false;
        }

        return true;
    }

    protected GetTimeRefresh(jsonData: any) {
        if (jsonData['ts'] != undefined) {
            TimeManager.TimeRefresh(jsonData.ts);
        } else {
            console.log('net : ts empty');
        }
    }

    protected ErrorResponse(jsonData: any, name: string, okCode: number): boolean {
        if (!ObjectCheck(jsonData, 'rs') || jsonData['rs'] != okCode) {
            console.log(`<error> ${name}`, jsonData);
            return false;
        }
        return true;
    }

    public static SideIn(user_id: string) {
        NetworkManager.Send("auth/signin", {id: user_id}, NetworkManager.SideInResponse);
    }

    public static SideUp(user_id: string) {
        NetworkManager.Send("auth/signup", {id: user_id}, NetworkManager.SideUpResponse);
    }

    public static SideInResponse(jsonData: string) {
        if (NetworkManager.Instance.ErrorResponse(jsonData, 'SideInResponse', 0)) {
            Starter.SigninSuccess();
            User.Instance.Init();
            User.Instance.SetBase(jsonData);
            User.Instance.SetData();
        } else {// 로그인 실패
            Starter.signinFailed();
            // if(ObjectCheck(jsonData, 'rs') && ObjectCheck(jsonData, 'id') && jsonData['rs'] == 106) {
            //     console.log('no_error => sign_up');
            //     NetworkManager.SideUp(jsonData['id']);
            // }
        }
    }

    public static SideUpResponse(jsonData: string) {
        if (NetworkManager.Instance.ErrorResponse(jsonData, 'SideUpResponse', 0)) {
            Starter.SignupSuccess();
            User.Instance.Init();
            User.Instance.SetBase(jsonData);
            User.Instance.SetData();
        }
        else
        {
            Starter.signupFailed();
            //Starter.loginTry()
        }
    }

    // sendXmlHttp(2, params, (jsonObj)=>
    // {
    //     if(jsonObj == null)
    //     {
    //        return; 
    //     }
    //     var jsondata = JSON.parse(jsonObj);

    //     for(i=0; i<jsondata.length; i++)
    //     {
    //         itemsList.push({name:jsondata[i].name_kr, id:jsondata[i].item_id, symbol:"i"});
    //     }
    //     console.log("itemsList : " + itemsList.length);
    // });
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

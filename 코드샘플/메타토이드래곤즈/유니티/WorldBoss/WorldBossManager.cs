using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class WorldBossManager
    {
        private static WorldBossManager instance = null;
        public static WorldBossManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new WorldBossManager();
                    instance.WorldBossProgressData.Init();
                }
                return instance;
            }
        }

        public const int ENTER_DRAGON_LIMIT_COUNT = 4;//드래곤 소유 갯수가 4마리 이상이면 입장 가능

        public WorldBossBattleData Data { get; private set; } = new WorldBossBattleData();
        public WorldBossProgressData WorldBossProgressData { get; private set; } = new WorldBossProgressData();

        public IEnumerator LoadingCoroutine { get; private set; } = null;
        public int WorldBossEnterCount { get {return GameConfigTable.GetConfigIntValue("RAID_CLEAR_COUNT"); } }

        public int UIDeckIndex = -1;//로비에서 팀 편성 화면 넘어갈 때 현재 선택 슬롯 임시 저장용도
        public bool UITeamSettingFlag = false;//팀 편성 화면 씬 이동 임시저장 플래그
        public int UISelectBossKey = -1;//현재 선택한 보스 키 플래그세팅


        //test code must remove
        public int CheckIntFlag = 0;
        //test code end


        public bool IsStartCheck()
        {
            if (Data == null || Data.World <= 0 || Data.Stage <= 0)
                return false;

            if (Data.BattleTag == -1 || Data.State == eBattleState.None)
                return false;

            if (Data.OffensePos == null || Data.DefensePos == null)
                return false;

            if (Data.OffensePos.Count <= 0 || Data.DefensePos.Count <= 0)
                return false;

            return true;
        }

        public bool IsWaveCheck()
        {
            if (Data == null)
                return false;

            if (Data.BattleTag == -1 || Data.State == eBattleState.None || Data.Wave == -1)
                return false;

            if (Data.OffensePos == null || Data.DefensePos == null)
                return false;

            if (Data.OffensePos.Count <= 0 || Data.DefensePos.Count <= 0)
                return false;

            return true;
        }

        public bool IsRewardCheck()
        {
            if (Data == null)
                return false;

            //switch (Data.State)
            //{
            //    case eBattleState.Win:
            //    case eBattleState.Lose:
            //    case eBattleState.TimeOver:
            //        break;
            //    default:
            //        return false;
            //}

            return true;
        }

        public void SetSimulatorStartData(int world, int stage, JObject jsonObject)
        {
            if (Data == null || jsonObject == null)
                return;

            Data.Initialize();
            Data.SetWorld(world, stage);
            Data.SetData(jsonObject);
        }

        public void SetStartData(int world, int stage, JObject jsonObject)
        {
            if (Data == null || jsonObject == null)
                return;

            Data.Initialize();
            Data.SetWorld(world, stage);
            Data.SetData(jsonObject);
            LoadingCoroutine = StartLoadingCO();
        }

        public void SetWaveData(JObject jsonObject)
        {
            if (Data == null || jsonObject == null)
                return;

            Data.InitializeWave();
            Data.SetData(jsonObject);
        }

        public void SetRewardData(JObject jsonObject)
        {
            if (Data == null || jsonObject == null)
                return;

            Data.InitializeReward();
            Data.SetRewardData(jsonObject);
        }
        public void SetWorldBossProgress(JObject _jsonData)//진행 상태
        {
            if (_jsonData == null)
                return;

            WorldBossProgressData.SetData(_jsonData);
        }

        public IEnumerator StartLoadingCO()
        {
            var loadDatas = new Dictionary<eResourcePath, List<string>>();//로딩 리소스 파일 리스트 확보
            //월드 보스는 허수아비기 때문에 실제 사용 리소스 구분하여 로드 필요함
            var stageData = StageBaseData.GetByWorldStage(Data.World, Data.Stage);
            if(stageData != null)
            {
                var spawnDatas = MonsterSpawnData.GetBySpawnGroup(stageData.SPAWN);
                if (spawnDatas != null)
                {
                    for (int i = 0, count = spawnDatas.Count; i < count; ++i)
                    {
                        var spawnData = spawnDatas[i];
                        if (spawnData == null)
                            continue;

                        var baseData = MonsterBaseData.Get(spawnData.MONSTER.ToString());
                        if (baseData == null)
                            continue;

                        SBFunc.AddedResourceKey(loadDatas, eResourcePath.MonsterClonePath, baseData.IMAGE);
                        SBFunc.SkillDataSetLoadingList(loadDatas, baseData.NORMAL_SKILL);
                        SBFunc.SkillDataSetLoadingList(loadDatas, baseData.SKILL1);
                        SBFunc.SkillDataSetLoadingList(loadDatas, baseData.SKILL2);
                    }
                }
            }
            //
            //Dragon PreLoad
            if(Data.Characters != null)
            {
                for(int i = 0, count = Data.Characters.Count; i < count; ++i)
                {
                    if (Data.Characters[i] == null)
                        continue;
                    var curData = Data.Characters[i] as BattleDragonData;
                    if (curData == null || curData.BaseData == null)
                        continue;

                    var baseData = curData.BaseData;

                    SBFunc.AddedResourceKey(loadDatas, eResourcePath.DragonClonePath, baseData.IMAGE);
                    SBFunc.SkillDataSetLoadingList(loadDatas, baseData.NORMAL_SKILL);
                    SBFunc.SkillDataSetLoadingList(loadDatas, baseData.SKILL1);
                    SBFunc.SkillDataSetLoadingList(loadDatas, baseData.SKILL2);

                    SBFunc.PassiveDataSetLoadingList(loadDatas, curData.TranscendenceData);
                }
            }
            //
            yield return ResourceManager.LoadAsyncPaths(loadDatas);

            LoadingCoroutine = null;
            yield break;
        }
        
        /// <summary>
        /// 해당 함수 호출 부 앞단에서 광고체크를 먼저 하려고함.
        /// 서버 쪽에 보내는 필드에 광고 플래그가 필요한지 물어볼 것.
        /// </summary>
        /// <param name="isAd"></param>
        public void RequestBattleStart(bool isAd = false, VoidDelegate success = null, string log = "")
        {
            WorldBossFormationData curData = User.Instance.PrefData.WorldBossFormationData;
            if (!curData.HasFormation())
            {
                ToastManager.On(StringData.GetStringByStrKey("boss_raid_setting_yet"));
                return;
            }

            var selectInfo = instance.WorldBossProgressData.CurSelectBossData;
            if(selectInfo == null)
            {
                ToastManager.On(StringData.GetStringByStrKey("boss_raid_no_world"));
                return;
            }

            int world = selectInfo.World;
            int stage = selectInfo.Stage;
            WWWForm param = new();
            param.AddField("world", world);
            param.AddField("stage", stage);
            param.AddField("is_ad", isAd ? 1 : 0);
            param.AddField("ad_log", log);
            NetworkManager.Send("raid/start", param, (JObject jsonData) =>
            {
                if (jsonData["rs"] != null)
                {
                    switch ((eApiResCode)(int)jsonData["rs"])
                    {
                        case eApiResCode.OK:

#if UNITY_EDITOR
                            var checkTestFlag = WorldBossManager.Instance.CheckIntFlag;
                            if (checkTestFlag > 0)
                                stage = checkTestFlag;
#endif
                            SetStartData(world, stage, jsonData);
                            if (IsStartCheck())
                            {
                                LoadingManager.Instance.EffectiveSceneLoad("WorldBossBattle", eSceneEffectType.BlackBackground, LoadingCoroutine);

                                if (success != null)
                                    success.Invoke();
                            }
                            break;

                        case eApiResCode.RAID_DATA_ERROR:// 레이드 생성 오류
                            break;
                        case eApiResCode.RAID_START_FULL: // 레이드 입장 횟수 초과
                            ToastManager.On(100002103);//일일 최대 입장 횟수를 모두 사용하였습니다.
                            break;
                        case eApiResCode.RAID_DAY_NOT_MATCHED:// 레이드 월드의 요일 조건이
                        case eApiResCode.RAID_NO_SUCH_WORLD:// 레이드 월드 정보 못 찾음
                            

                            break;
                        case eApiResCode.RAID_NO_SUCH_STAGE:// 레이드 스테이지 정보 못 찾음
                            break;
                    }
                }
            }, (string arg) =>
            {

            });
        }

        /// <summary>
        /// 탈주 기능
        /// </summary>
        /// <param name="isAd"></param>
        public void RequestBattleResult(eBattleState _result, NetworkManager.SuccessCallback success = null, NetworkManager.FailCallback fail = null)
        {
            Data.SetState(_result);

            var selectInfo = instance.WorldBossProgressData.CurSelectBossData;
            if (selectInfo == null)
            {
                ToastManager.On(StringData.GetStringByStrKey("boss_raid_no_world"));
                return;
            }

            var form = new WWWForm();
            form.AddField("tag", Data.BattleTag);
            form.AddField("world", Data.World);
            form.AddField("stage", Data.Stage);
            form.AddField("result", (int)_result);

            if(_result != eBattleState.Abort)
            {
                form.AddField("score", Data.BossData.SCORE.ToString());
                form.AddField("level", Data.BossData.Level);
                form.AddField("log", "[]");
                //form.AddField("holder", User.Instance.IS_HOLDER?"1":"0");
                //form.AddField("boss_no", ((WorldBossBattleData)Data).BossData.MonsterKey);
                //form.AddField("alives", lives);
                //form.AddField("deckinf", deckObj.ToString());

#if UNITY_EDITOR
                var myDat = StatisticsMananger.Instance.GetWorldBossDamageInfo(true);
                form.AddBinaryData("my", System.Text.Encoding.Default.GetBytes(myDat));
                form.AddField("my", myDat);
#endif
            }

            NetworkManager.Send("raid/result", form, (jsonData) =>
            { //성공 결과에 따라 처리
                if (SBFunc.IsJTokenType(jsonData["rs"], JTokenType.Integer)
                && jsonData["rs"].Value<int>() == (int)eApiResCode.OK)
                {
                    //결과씬으로 보내는게 맞을 것같음.
                    if (jsonData == null)
                        return;

                    StatisticsMananger.Instance.SetAliveTime();//살아있는 시간 세팅

                    SetRewardData(jsonData);
                    if(_result == eBattleState.Abort)
                        LoadingManager.Instance.EffectiveSceneLoad("WorldBossLobby", eSceneEffectType.BlackBackground);
                    else
                        LoadingManager.Instance.EffectiveSceneLoad("WorldBossResult", eSceneEffectType.BlackBackground);

                    if (success != null)
                        success.Invoke(jsonData);
                    
                    return;
                }
                //네트워크 통신 오류 밖으로 나가게하자
                //
                },
                (error) => //네트워크 연결 오류 밖으로 나가게하자
                {
                    if (fail != null)
                        fail.Invoke(error);
                    else if (Data.State == eBattleState.Abort)
                    {
                        LoadingManager.Instance.EffectiveSceneLoad("WorldBossLobby", eSceneEffectType.BlackBackground);
                    }
                });
        }

        public void RequestRankingData(int page = 0, NetworkManager.SuccessCallback cb = null, NetworkManager.FailCallback fail = null)
        {
            WWWForm form = new WWWForm();
            form.AddField("page", page);

            NetworkManager.Send("raid/ranking", form, (JObject jsonData) =>
            {
                if (SBFunc.IsJTokenCheck(jsonData) && (int)eApiResCode.OK == jsonData["rs"].Value<int>())
                {
                    cb?.Invoke(jsonData);//성공하면 호출한 쪽에서 data 세팅해서 리스트 세팅
                }
            }, (failData) =>
            {
                fail?.Invoke(failData);
            });
        }

        /// <summary>
        /// 보상 관련은 총 4가지 (일단 픽스라고 했음.)인데, 각 레벨별로 분포& 갯수도 다르고, item_group 전체를 뒤져서 세팅하기엔 너무 비효율이라
        /// 그냥 박는다.
        /// </summary>
        public List<Asset> GetWorldBossRewardList()
        {
            var rewardOne = new Asset(eGoodType.ACCOUNT_EXP, 0, 0);//계정경험치
            var rewardTwo = new Asset(eGoodType.ITEM, 150000001, 0);//코어블록
            var rewardThree = new Asset(eGoodType.ITEM, 180000001, 0);//패시브재료
            var rewardFour = new Asset(eGoodType.ITEM, 180000002, 0);//상급패시브재료

            return new List<Asset>() { rewardOne, rewardTwo, rewardThree, rewardFour };
        }

        /// <summary>
        /// (임시) 각 보스 타입 별(요일이든, worldIndex 든)로 입장 컨디션이 테이블 정의가 따로 없어서 하드코딩해둠
        /// </summary>
        /// <returns></returns>
        public bool IsWorldBossEnterCondition()
        {
            var userdragonCount = User.Instance.DragonData.GetAllUserDragons().Count;
            return userdragonCount >= ENTER_DRAGON_LIMIT_COUNT;
        }

        public bool IsAvailEnterCondition()
        {
            var enterWorldIndex = GameConfigTable.GetRaidOpenWorld();

            return StageManager.Instance.AdventureProgressData.isAvailableWorld(enterWorldIndex + 1);
        }
    }
}
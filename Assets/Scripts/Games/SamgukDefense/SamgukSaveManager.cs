using System.Collections.Generic;
using UnityEngine;

namespace SamgukDefense
{
    /// <summary>Persistent Data — 로그라이크 런과 무관하게 영구히 유지되는 값만 담는다
    /// (2026-09-15 데이터 구조 재점검: 아이템 보유/장비는 원래 여기 있었는데, 런 시작 전 로비에
    /// 아이템이 이미 있는 버그의 직접 원인이었다 — 전부 Run Data로 옮겨졌고 다시는 여기 저장하지
    /// 않는다). 전투 데이터(현재 HP·위치·쿨타임·라운드)와 런 데이터(아이템 보유/장비/조합 결과)는
    /// 절대 저장 안 함 — SamgukBattleManager.Items.cs의 ResetRunData() 참고.
    /// JsonUtility는 중첩 배열/딕셔너리를 잘 못 다루므로 전부 병렬 List&lt;string&gt;/List&lt;int&gt;로
    /// 평탄화한다.</summary>
    [System.Serializable]
    public class SamgukSaveData
    {
        public int persistentGold;
        public List<string> ownedCharacterIds = new List<string>();

        /// <summary>런당 출전 가능 인원 — 3(기본)~10(최대). 로비에서 골드로 영구 확장한다
        /// (2026-09-16, SamgukBattleManager.Meta.cs의 TryUpgradeTeamSize 참고 — 처음엔
        /// 1명으로 시작했는데 실제로 해보니 너무 빡빡하다는 사용자 피드백으로 3명으로 조정).</summary>
        public int teamSizeLevel = 3;

        // 강화 레벨 — 캐릭터당 "레벨0,레벨1,...,레벨5" 콤마조인 문자열 하나
        public List<string> upgradeCharIds = new List<string>();
        public List<string> upgradeLevelsCsv = new List<string>();
    }

    public static class SamgukSaveManager
    {
        const string Key = "SamgukDefense_Save";

        public static SamgukSaveData Load()
        {
            string json = PlayerPrefs.GetString(Key, "");
            if (string.IsNullOrEmpty(json))
            {
                return new SamgukSaveData { persistentGold = 0 }; // ssam.md 96.12 — 신규 유저 0골드(예전 100골드 지급 폐지, 라운드/장비 정산으로만 번다)
            }
            var data = JsonUtility.FromJson<SamgukSaveData>(json);
            return data ?? new SamgukSaveData { persistentGold = 0 };
        }

        public static void Save(SamgukSaveData data)
        {
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(Key, json);
            PlayerPrefs.Save();
        }
    }
}

using UnityEngine;

namespace SamgukDefense
{
    [CreateAssetMenu(fileName = "Monster_", menuName = "SamgukDefense/Monster Data")]
    public class SamgukMonsterData : ScriptableObject
    {
        public string id;
        public string displayName;

        public float baseHP = 80f;
        public float baseAttack = 12f;
        public float baseDefense = 3f;
        public float baseAttackSpeed = 1f;
        public float baseMoveSpeed = 2f;
        public float baseAttackRange = 1f;
        public int goldReward = 5;

        /// <summary>죽을 때 각 항목의 dropRate를 독립적으로 굴려 드랍한다(둘 다 뜨는 것도 가능).
        /// 몬스터 자신이 전투 중 이 아이템의 스탯을 실제로 장착·적용하지는 않는다 — Phase 2
        /// MVP 범위를 드랍 테이블로만 좁혔다(§32의 "몬스터도 장착하고 등장" 전투 적용까지는 생략).</summary>
        public SamgukItemData[] possibleDrops;

        public Color tintColor = new Color(0.75f, 0.25f, 0.2f);

        /// <summary>비어있으면 tintColor 사각형으로 대체 렌더링(SamgukCharacterData.icon과 동일한 규칙).</summary>
        public Sprite icon;

        /// <summary>이 라운드부터 일반 스폰 풀에 들어간다(이전 라운드에서도 계속 나오던 몬스터가
        /// 사라지지 않고 누적되는 방식 — 라운드가 올라갈수록 스폰 다양성도 함께 넓어진다).
        /// isBoss인 몬스터에는 이 필드가 아무 의미 없다 — 보스는 SpawnBossWave가 라운드
        /// 번호만으로 등급(bossStar)을 직접 계산해서 찾으므로 minRound를 안 본다.</summary>
        public int minRound = 1;

        /// <summary>true면 일반 스폰 풀에 안 섞이고, 라운드별 보스 웨이브 전용으로만 스폰된다.</summary>
        public bool isBoss;

        /// <summary>isBoss일 때만 의미 있음 — 이 보스가 몇 성급 보스인지(1~6). BeginRound가
        /// "지금 라운드에 맞는 성급의 보스"를 이 값으로 찾는다.</summary>
        public int bossStar = 1;
    }
}

using UnityEngine;

namespace SamgukDefense
{
    [CreateAssetMenu(fileName = "Character_", menuName = "SamgukDefense/Character Data")]
    public class SamgukCharacterData : ScriptableObject
    {
        public string id;
        public string displayName;
        [Range(1, 6)] public int star = 1;

        public float baseHP = 200f;
        public float baseAttack = 20f;
        public float baseDefense = 5f;
        public float baseAttackSpeed = 1f;
        public float baseMoveSpeed = 2.5f;
        public float baseAttackRange = 1.2f;

        public Color tintColor = Color.white;

        /// <summary>실제 캐릭터 스프라이트. 비어있으면 기존처럼 tintColor로 칠한 사각형 +
        /// 이름 첫 글자로 대체 렌더링한다(SamgukUnit.Init 참고) — 리소스가 없어도 게임이
        /// 깨지지 않는다.</summary>
        public Sprite icon;

        /// <summary>성급 = 슬롯 개수(1성 1칸 ~ 6성 6칸) — 사용자가 재확인한 최종 사양
        /// (2026-09-16 재수정). 한때 "고정 3슬롯(무기/방어구/장신구)"으로 바꿨던 적이 있는데
        /// (ssam_info.md의 장수별 아이템 표가 3부위×등급 구조라는 근거였다), 실제로는 성급별
        /// 슬롯 수 차등이 원래 요구사항이었다는 걸 사용자가 다시 명확히 했다. 장비 슬롯은
        /// 원래부터 아이템 타입을 안 가리는 범용 칸이라(SamgukUnit.SetEquipment 참고 — 같은
        /// 슬롯에 무기든 방어구든 아무거나 낄 수 있다), 3칸을 넘는 여분 슬롯도 "그 캐릭터
        /// 전용 4번째 부위" 없이 그냥 인벤토리의 아무 아이템이나 담는 범용 칸으로 자연스럽게
        /// 동작한다 — 540개 아이템 자체(캐릭터 3종×등급6)는 안 건드렸다.</summary>
        public int ItemSlotCount => Mathf.Clamp(star, 1, 6);
    }
}

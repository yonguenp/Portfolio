using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace SamgukDefense
{
    public enum SamgukFaction { Player, Monster }
    public enum SamgukUnitState { Idle, SearchingTarget, Rotating, Moving, Attacking, Stunned, Dead }

    /// <summary>
    /// 전투 유닛 하나(플레이어 캐릭터 또는 몬스터)를 표현한다. PlayerController/CPUController를
    /// 별도 서브클래스로 안 나눈 이유 — ssam.md 65절이 명시한 대로 둘의 차이는 오직 소속 진영뿐이라,
    /// 지금 시점엔 빈 서브클래스 두 개를 만드는 게 과설계다. AI가 실제로 갈라지면(예: 플레이어 진영만
    /// 스킬 발동 조건이 다르다든가) 그때 Faction 분기를 서브클래스로 승격하면 된다.
    ///
    /// 2026-09-16 — 완전 3D 전환. 예전엔 UGUI RectTransform+Image로 그렸는데, "전투 화면은 캔버스와
    /// 독립적이어야 한다"는 요청으로 순수 월드 스페이스 Transform+SpriteRenderer로 바꿨다.
    /// BoardPosition(Vector2)의 "의미"(이동/타겟팅/사거리 계산에 쓰이는 보드 좌표)는 전혀 안 바뀌었다
    /// — SyncTransform()이 그 값을 3D 월드의 (X, 0, Z)로 매핑할 뿐이라, 이 파일의 전투 로직(Tick 등)은
    /// 좌표계 관점에서 거의 그대로다. 시각 요소는 항상 카메라를 향하는 "Billboard" 자식 아래(몸체·
    /// HP바·라벨·공격플래시) + 바닥에 눕는 별도 마커(FacingDot·블롭섀도)로 나뉜다.
    /// </summary>
    public class SamgukUnit : MonoBehaviour
    {
        public static float MinDamage = 1f;
        public static float BoardHalfExtent = 10f;

        /// <summary>매 프레임 Billboard들이 바라볼 카메라 — BattleManager가 씬 진입 시 한 번
        /// 세팅한다(유닛마다 Camera.main을 매번 찾는 비용을 피한다).</summary>
        public static Transform BillboardCamera;

        static Texture2D solidWhiteTexCache;
        /// <summary>진짜 1×1 흰 텍스처를 직접 만들어 쓴다 — Texture2D.whiteTexture를 그대로
        /// 썼다가 캐릭터/몬스터/HP바가 전부 4배 크게 렌더링되는 사고가 났다(2026-09-16). 그
        /// 내장 텍스처는 "1×1"이 아니라 실제로는 4×4 픽셀이고(엔진 내부 폴백용, 문서화된 보장이
        /// 아니다), 아래 Sprite.Create가 rect를 tex.width/height(=4)로, pixelsPerUnit을 1로
        /// 넘기고 있어서 스프라이트의 "스케일 1일 때 원본 크기"가 1×1이 아니라 4×4 월드 유닛이
        /// 돼버렸다 — 그 위에 곱해지는 localScale(visualSize=cellSize*0.82 등)은 그대로였으니
        /// 결과물이 딱 4배로 부풀어 보였다. BlobShadowSprite처럼 실제 픽셀 크기를 직접 아는
        /// 텍스처를 만들어야 이런 종류의 "내장 에셋의 실제 크기를 가정하는" 사고를 원천 차단한다.</summary>
        static Texture2D SolidWhiteTex()
        {
            if (solidWhiteTexCache == null)
            {
                solidWhiteTexCache = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                solidWhiteTexCache.SetPixel(0, 0, Color.white);
                solidWhiteTexCache.Apply();
            }
            return solidWhiteTexCache;
        }

        static Sprite whiteSpriteCache;
        /// <summary>중앙 피벗 흰 스프라이트(Image.Type.Filled에 sprite가 None이면 fillAmount가
        /// 무시된다는 걸 겪은 뒤 확정한 패턴 — 여기서도 SpriteRenderer가 항상 유효한 스프라이트를
        /// 갖도록 공유 캐시로 하나 만들어 쓴다).</summary>
        static Sprite WhiteSprite()
        {
            if (whiteSpriteCache == null)
            {
                var tex = SolidWhiteTex();
                whiteSpriteCache = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 1f);
            }
            return whiteSpriteCache;
        }

        static Sprite leftPivotWhiteSpriteCache;
        /// <summary>왼쪽 피벗 흰 스프라이트 — HP 채움 바 전용. SpriteRenderer는 localScale로
        /// 크기를 줄이면 피벗을 기준으로 줄어들므로, 피벗을 왼쪽에 둬야 Image.FillMethod.Horizontal
        /// (fillOrigin=Left)과 똑같이 "왼쪽부터 채워진 만큼만" 보인다.</summary>
        static Sprite LeftPivotWhiteSprite()
        {
            if (leftPivotWhiteSpriteCache == null)
            {
                var tex = SolidWhiteTex();
                leftPivotWhiteSpriteCache = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0f, 0.5f), 1f);
            }
            return leftPivotWhiteSpriteCache;
        }

        static Sprite blobShadowSpriteCache;
        /// <summary>유닛 발밑 블롭 섀도 — 실시간 그림자 없이(이 프로젝트 확립 원칙, BrickBreaker3D
        /// 깊이감 렌더링 문서 참고) 부드러운 원형 그라데이션을 바닥에 깔아 입체감을 대신한다.</summary>
        static Sprite BlobShadowSprite()
        {
            if (blobShadowSpriteCache == null)
            {
                const int size = 64;
                var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                tex.filterMode = FilterMode.Bilinear;
                var pixels = new Color[size * size];
                float half = size * 0.5f;
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(half, half)) / half;
                        float a = Mathf.Clamp01(1f - d);
                        a = a * a; // 가장자리를 더 빠르게 페이드
                        pixels[y * size + x] = new Color(0f, 0f, 0f, a * 0.55f);
                    }
                }
                tex.SetPixels(pixels);
                tex.Apply();
                blobShadowSpriteCache = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size / 2f);
            }
            return blobShadowSpriteCache;
        }

        public SamgukFaction Faction { get; private set; }
        public string DisplayName { get; private set; }

        /// <summary>SamgukCharacterData.id — 아이템 주인 보너스 판정용. 몬스터는 빈 문자열.</summary>
        public string CharacterId { get; private set; } = "";

        public float MaxHP { get; private set; }
        public float CurrentHP { get; private set; }
        public float Attack { get; private set; }
        public float Defense { get; private set; }
        public float AttackSpeed { get; private set; }
        public float MoveSpeed { get; private set; }
        public float AttackRange { get; private set; }

        public SamgukUnitState State { get; private set; } = SamgukUnitState.Idle;
        public bool IsAlive => State != SamgukUnitState.Dead;

        /// <summary>그리드 셀 단위의 연속 좌표(0~20). 이동 중에는 그리드에 딱 맞물리지 않고
        /// 타겟 방향으로 직선 이동한다 — 스냅은 배치 단계의 드래그&드롭에서만 쓰인다. 3D 전환
        /// 이후에도 이 값의 의미는 그대로다 — SyncTransform()이 (X, 0, Y)로 월드에 매핑한다.</summary>
        public Vector2 BoardPosition;

        /// <summary>배치 단계에서만 의미 있는 정수 셀 좌표(점유 판정용). 전투 중에는 안 갱신된다.</summary>
        public Vector2Int Cell;

        public SamgukUnit Target { get; private set; }

        /// <summary>바라보는 방향(정규화, 보드 XZ 평면 기준). 매 프레임 타겟 벡터로 다시 계산하지
        /// 않는다 — ssam.md 41절 개정판대로, 타겟이 이 방향 허용각(FacingToleranceDeg) 밖에 있을
        /// 때만 이 값 자체가 "회전" 액션으로 갱신되고, 그 틱엔 이동/공격을 안 한다.</summary>
        public Vector2 FacingDirection { get; private set; } = Vector2.up;

        /// <summary>사망 시 정확히 한 번 발생 — 골드 지급 등 외부 보상 훅용.</summary>
        public System.Action<SamgukUnit> OnDied;

        /// <summary>몬스터 처치 보상 골드. 플레이어 유닛은 0으로 둔다(매니저가 스폰 직후 세팅).</summary>
        public int GoldReward;

        /// <summary>스폰 출처 데이터 — 장비 재계산(Character)·드랍 테이블 조회(Monster)에 쓴다.
        /// 어느 한쪽만 채워진다(자기 진영과 무관한 쪽은 null).</summary>
        public SamgukCharacterData SourceData;
        public SamgukMonsterData SourceMonsterData;

        Transform billboard;
        Transform facingDotT;
        Transform hpBarBgT;
        SpriteRenderer bodyImage;
        SpriteRenderer hpFillImage;
        SpriteRenderer hpBarBgImage;
        SpriteRenderer attackFlashImage;
        Color baseBodyColor;
        TextMeshPro label;
        BoxCollider dragCollider;
        float attackCooldown;
        float cellSize;
        float visualSize;

        // ── 아이템 옵션 런타임 상태 (2026-09-16 추가) ───────────────────────
        // 기절/출혈은 "갱신형"이다 — 같은 효과가 다시 걸리면 지속시간을 더 긴 쪽으로만 늘릴
        // 뿐 누적(스택)하지 않는다. 여러 공격자가 동시에 걸어도 무한 CC로 이어지지 않게 하는
        // 가장 단순하고 안전한 규칙.
        float stunTimer;
        float bleedRemaining, bleedTickTimer, bleedDamagePerTickCached, bleedTickIntervalCached;
        float hpRegenPerSecond;
        float lifestealPercent;
        float dashCooldownCached, dashDistanceCached, dashCooldownTimer;

        class WeaponSlot
        {
            public SamgukItemData data;
            public float cooldown;
            public float effDamage;
            public float effScaling;
            public float effRange;
            // 명중 시 발동 옵션 — 주인 전용 무기 보너스(1.2배)는 데미지 계열에만 적용하고
            // 여기(확률/지속시간/이동거리)엔 일부러 안 건다. 능력이 아니라 숫자만 세지는
            // 게 "주인 보너스"의 원래 의미에 더 가깝다는 판단.
            public float knockback;
            public float stunChance, stunDuration;
            public float bleedChance, bleedDamagePerTick, bleedDuration, bleedTickInterval;
        }
        readonly List<SamgukItemData> equipped = new List<SamgukItemData>();
        readonly List<WeaponSlot> weaponSlots = new List<WeaponSlot>();
        public IReadOnlyList<SamgukItemData> Equipped => equipped;

        public void Init(SamgukFaction faction, string displayName, float hp, float attack, float defense,
            float attackSpeed, float moveSpeed, float attackRange, Color color, Transform boardRoot, float cellSizeWorld, string characterId = "", Sprite icon = null)
        {
            Faction = faction;
            DisplayName = displayName;
            CharacterId = characterId ?? "";
            MaxHP = hp;
            CurrentHP = hp;
            Attack = attack;
            Defense = defense;
            AttackSpeed = attackSpeed;
            MoveSpeed = moveSpeed;
            AttackRange = attackRange;
            cellSize = cellSizeWorld;
            State = SamgukUnitState.Idle;
            Target = null;
            attackCooldown = 0f;

            transform.SetParent(boardRoot, false);
            transform.localRotation = Quaternion.identity; // 루트는 항상 회전 없음(위치 전용) — 빌보드는 자식이 담당
            visualSize = cellSize * 0.82f;

            bool hasIcon = icon != null;

            billboard = new GameObject("Billboard").transform;
            billboard.SetParent(transform, false);
            billboard.localPosition = new Vector3(0f, visualSize * 0.5f, 0f);

            var bodyGo = new GameObject("Body");
            bodyGo.transform.SetParent(billboard, false);
            bodyImage = bodyGo.AddComponent<SpriteRenderer>();
            if (hasIcon)
            {
                // 실제 스프라이트가 있으면 원본 색 그대로 쓴다(tintColor를 곱하면 픽셀아트
                // 셰이딩이 탁해진다 — 이 프로젝트 전역에서 이미 확인된 원칙).
                bodyImage.sprite = icon;
                bodyImage.color = Color.white;
                float aspect = icon.rect.width / Mathf.Max(1f, icon.rect.height);
                bodyGo.transform.localScale = aspect >= 1f
                    ? new Vector3(visualSize, visualSize / aspect, 1f)
                    : new Vector3(visualSize * aspect, visualSize, 1f);
            }
            else
            {
                bodyImage.sprite = WhiteSprite();
                bodyImage.color = color;
                bodyGo.transform.localScale = new Vector3(visualSize, visualSize, 1f);
            }
            baseBodyColor = bodyImage.color;
            bodyImage.sortingOrder = 10;

            label = new GameObject("Label").AddComponent<TextMeshPro>();
            label.transform.SetParent(billboard, false);
            // localScale=0.08는 실측해보니 렌더링된 글자 높이가 월드 0.03유닛(visualSize의 3.7%)
            // 밖에 안 나와 화면에서 사실상 안 보였다(2026-09-16, "라벨 폰트 사이즈가 너무 작아서
            // 안보임" 신고 — 3D TextMeshPro의 fontSize는 UGUI와 스케일 관례가 달라, 여기 값
            // (fontSize=visualSize*5f)에 localScale을 곱한 최종 월드 크기를 실측 없이 가정하면
            // 이렇게 쉽게 어긋난다). 0.7로 올려 실측 렌더 높이가 visualSize의 약 40%가 되도록
            // 맞췄다 — 한 글자짜리 배지가 몸통 위에서 읽힐 만큼 크면서 몸통을 안 가릴 정도.
            label.transform.localScale = Vector3.one * 0.7f;
            label.text = string.IsNullOrEmpty(displayName) ? "?" : displayName.Substring(0, 1);
            label.fontSize = visualSize * 5f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            label.sortingOrder = 11;
            // 스프라이트가 있으면 이름 첫 글자 라벨은 필요 없다(가독성만 해친다).
            label.gameObject.SetActive(!hasIcon);

            float barW = visualSize;
            float barH = Mathf.Max(0.05f, cellSize * 0.24f);
            var hpBg = new GameObject("HpBg");
            hpBg.transform.SetParent(billboard, false);
            hpBg.transform.localPosition = new Vector3(0f, visualSize * 0.5f + barH * 0.5f + 0.04f, 0f);
            hpBg.transform.localScale = new Vector3(barW, barH, 1f);
            hpBarBgImage = hpBg.AddComponent<SpriteRenderer>();
            hpBarBgImage.sprite = WhiteSprite();
            hpBarBgImage.color = NormalHpBarBgColor;
            hpBarBgImage.sortingOrder = 12;
            hpBarBgT = hpBg.transform;

            var hpFillGo = new GameObject("HpFill");
            hpFillGo.transform.SetParent(billboard, false);
            hpFillGo.transform.localPosition = hpBg.transform.localPosition + new Vector3(-barW * 0.5f, 0f, 0f);
            hpFillGo.transform.localScale = new Vector3(barW, barH, 1f);
            hpFillImage = hpFillGo.AddComponent<SpriteRenderer>();
            hpFillImage.sprite = LeftPivotWhiteSprite();
            hpFillImage.color = faction == SamgukFaction.Player ? new Color(0.3f, 0.85f, 0.4f) : new Color(0.9f, 0.3f, 0.25f);
            hpFillImage.sortingOrder = 13;

            var flashGo = new GameObject("AttackFlash");
            flashGo.transform.SetParent(billboard, false);
            flashGo.transform.localPosition = Vector3.zero;
            flashGo.transform.localScale = bodyGo.transform.localScale;
            attackFlashImage = flashGo.AddComponent<SpriteRenderer>();
            attackFlashImage.sprite = hasIcon ? icon : WhiteSprite();
            attackFlashImage.color = new Color(1f, 0.95f, 0.55f, 0f);
            attackFlashImage.sortingOrder = 14;

            // 바닥에 눕는 마커(빌보드 아님) — 발밑 그림자 + 바라보는 방향 표시.
            var shadowGo = new GameObject("BlobShadow");
            shadowGo.transform.SetParent(transform, false);
            shadowGo.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            shadowGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            shadowGo.transform.localScale = new Vector3(visualSize * 1.1f, visualSize * 1.1f, 1f);
            var shadowSr = shadowGo.AddComponent<SpriteRenderer>();
            shadowSr.sprite = BlobShadowSprite();
            shadowSr.sortingOrder = 1;

            var dotGo = new GameObject("FacingDot");
            dotGo.transform.SetParent(transform, false);
            dotGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            dotGo.transform.localScale = new Vector3(cellSize * 0.16f, cellSize * 0.16f, 1f);
            var dotSr = dotGo.AddComponent<SpriteRenderer>();
            dotSr.sprite = WhiteSprite();
            dotSr.color = Color.white;
            dotSr.sortingOrder = 2;
            facingDotT = dotGo.transform;

            // 배치 단계 드래그 판정용 콜라이더 — SetDraggable(true)일 때만 켠다(기본 꺼짐).
            dragCollider = gameObject.AddComponent<BoxCollider>();
            dragCollider.center = new Vector3(0f, visualSize * 0.5f, 0f);
            dragCollider.size = new Vector3(visualSize, visualSize * 1.6f, visualSize);
            dragCollider.enabled = false;

            SyncTransform();
            SetFacing(FacingDirection);
        }

        void Update()
        {
            if (billboard != null && BillboardCamera != null) billboard.rotation = BillboardCamera.rotation;
        }

        void SyncTransform()
        {
            if (transform == null) return;
            transform.localPosition = new Vector3(BoardPosition.x * cellSize, 0f, BoardPosition.y * cellSize);
        }

        public void SnapTo(Vector2 boardPos)
        {
            BoardPosition = boardPos;
            SyncTransform();
        }

        public void SnapToCell(Vector2Int cell, SamgukGridBoard grid)
        {
            Cell = cell;
            SnapTo(grid.CellToBoardPos(cell.x, cell.y));
        }

        /// <summary>장비 변경 후(또는 최초 스폰 후) 최종 스탯을 다시 적용한다. hpDelta가 양수면
        /// (장비로 최대HP가 늘면) 현재HP도 같이 늘려준다 — 음수(장비 해제로 줄어드는 경우)는
        /// 현재HP를 안 건드려서 즉사시키지 않는다.</summary>
        public void ApplyStatRecalc(float newMaxHP, float newAttack, float newDefense, float newAttackSpeed, float newMoveSpeed, float newAttackRange)
        {
            float hpDelta = newMaxHP - MaxHP;
            MaxHP = newMaxHP;
            CurrentHP = Mathf.Clamp(CurrentHP + Mathf.Max(0f, hpDelta), 0f, MaxHP);
            Attack = newAttack;
            Defense = newDefense;
            AttackSpeed = newAttackSpeed;
            MoveSpeed = newMoveSpeed;
            AttackRange = newAttackRange;
            UpdateHpBarFill();
        }

        void UpdateHpBarFill()
        {
            if (hpFillImage == null || hpBarBgT == null) return;
            hpFillImage.transform.localScale = new Vector3(hpBarBgT.localScale.x * (MaxHP > 0f ? CurrentHP / MaxHP : 0f), hpBarBgT.localScale.y, 1f);
        }

        /// <summary>현재 장착 아이템 목록을 갱신한다 — 무기 슬롯(쿨타임+주인보너스+명중옵션
        /// 반영된 유효 스탯)을 다시 짜고, 방어구(재생/흡혈)·장신구(돌진) 패시브 총합도 다시
        /// 계산한다. 방어구/장식의 순수 스탯 보너스(HP/공격력 등)는 이미 ApplyStatRecalc로
        /// 반영된 최종값을 받으므로 여기선 새 옵션 필드들만 신경 쓴다.</summary>
        public void SetEquipment(IReadOnlyList<SamgukItemData> items)
        {
            equipped.Clear();
            weaponSlots.Clear();
            hpRegenPerSecond = 0f;
            lifestealPercent = 0f;
            dashCooldownCached = 0f;
            dashDistanceCached = 0f;
            if (items == null) return;
            equipped.AddRange(items);
            foreach (var it in equipped)
            {
                if (it == null) continue;
                if (it.itemType == SamgukItemType.Weapon)
                {
                    bool isOwner = !string.IsNullOrEmpty(it.ownerCharacterId) && it.ownerCharacterId == CharacterId;
                    float mult = isOwner ? SamgukItemData.OwnerBonusMultiplier : 1f;
                    weaponSlots.Add(new WeaponSlot
                    {
                        data = it,
                        cooldown = 0f,
                        effDamage = it.weaponDamage * mult,
                        effScaling = it.weaponDamageAttackScaling * mult,
                        effRange = it.weaponRange,
                        knockback = it.knockbackDistance,
                        stunChance = it.stunChance,
                        stunDuration = it.stunDuration,
                        bleedChance = it.bleedChance,
                        bleedDamagePerTick = it.bleedDamagePerTick,
                        bleedDuration = it.bleedDuration,
                        bleedTickInterval = it.bleedTickInterval
                    });
                }
                else if (it.itemType == SamgukItemType.Armor)
                {
                    hpRegenPerSecond += it.hpRegenPerSecond;
                    lifestealPercent += it.lifestealPercent;
                }
                else if (it.itemType == SamgukItemType.Accessory && it.dashCooldown > 0f && it.dashDistance > 0f)
                {
                    // 돌진 장신구를 여러 개 장착해도 쿨타임끼리 빼는 건 의미가 없으니, 더
                    // 짧은(=더 좋은) 쪽 하나만 채택한다.
                    if (dashCooldownCached <= 0f || it.dashCooldown < dashCooldownCached)
                    {
                        dashCooldownCached = it.dashCooldown;
                        dashDistanceCached = it.dashDistance;
                    }
                }
            }
            lifestealPercent = Mathf.Clamp(lifestealPercent, 0f, MaxLifestealPercent);
        }

        /// <summary>방어구 흡혈 옵션 등, "이 유닛이 입힌 피해의 일부만큼 자신을 회복"하는
        /// 모든 경로(기본 공격·무기 명중·투사체 명중)가 공유하는 진입점.</summary>
        public void Heal(float amount)
        {
            if (!IsAlive || amount <= 0f) return;
            CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);
            UpdateHpBarFill();
        }

        /// <summary>넉백 — sourcePos(공격자 위치) 반대 방향으로 distance만큼 밀려난다.</summary>
        public void ApplyKnockback(Vector2 sourcePos, float distance)
        {
            if (!IsAlive || distance <= 0f) return;
            Vector2 dir = BoardPosition - sourcePos;
            if (dir.sqrMagnitude < 0.0001f) dir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            dir.Normalize();
            BoardPosition += dir * distance;
            ClampToBoard();
            SyncTransform();
        }

        /// <summary>기절 — 갱신형(더 긴 지속시간으로만 늘어남, 누적 안 함). 기절 중엔 이동·
        /// 공격·무기 발동이 전부 멈춘다(Tick() 최상단 게이트 참고).</summary>
        public void ApplyStun(float duration)
        {
            if (!IsAlive || duration <= 0f) return;
            stunTimer = Mathf.Max(stunTimer, duration);
        }

        /// <summary>출혈(DOT) — 이것도 갱신형이라 같은 대상에게 거듭 걸어도 스택되지 않고
        /// 더 긴 지속시간으로만 늘어난다. 새로 걸릴 때(이전 출혈이 이미 끝난 상태)만 다음
        /// 틱까지의 대기시간을 새로 잡는다 — 이미 걸려있는 도중 갱신되면 다음 틱 타이밍은
        /// 그대로 유지해 틱이 밀리거나 몰리지 않게 한다.</summary>
        public void ApplyBleed(float damagePerTick, float duration, float tickInterval)
        {
            if (!IsAlive || duration <= 0f || damagePerTick <= 0f) return;
            bleedDamagePerTickCached = damagePerTick;
            bleedTickIntervalCached = Mathf.Max(0.1f, tickInterval);
            if (bleedRemaining <= 0f) bleedTickTimer = bleedTickIntervalCached;
            bleedRemaining = Mathf.Max(bleedRemaining, duration);
        }

        /// <summary>재생(방어구 옵션)과 출혈(무기 옵션) 둘 다 매 틱 진행 — 기절 중에도 계속
        /// 작동한다(움직이지 못할 뿐 몸은 계속 회복하거나 계속 피를 흘린다는 게 더 자연스러운
        /// 해석). 출혈로 죽을 수 있으므로 호출부는 이후 IsAlive를 다시 확인해야 한다.</summary>
        void ProcessRegenAndBleed(float dt)
        {
            if (hpRegenPerSecond > 0f && CurrentHP < MaxHP) Heal(hpRegenPerSecond * dt);
            if (bleedRemaining > 0f)
            {
                bleedRemaining -= dt;
                bleedTickTimer -= dt;
                if (bleedTickTimer <= 0f)
                {
                    bleedTickTimer += bleedTickIntervalCached;
                    TakeDamage(bleedDamagePerTickCached);
                }
            }
        }

        public void TakeDamage(float dmg)
        {
            if (!IsAlive) return;
            CurrentHP = Mathf.Max(0f, CurrentHP - dmg);
            UpdateHpBarFill();
            PlayHitFlash();
            if (CurrentHP <= 0f) Die();
        }

        /// <summary>기본 공격이나 무기 스킬이 실제로 명중을 발동시키는 그 순간에만 부른다 —
        /// 몸체 오버레이를 잠깐 밝혔다 끄고(색과 무관하게 항상 보임) 살짝 튀는 스케일 펀치를
        /// 더한다. 데미지 판정 자체는 그대로 두고 순수 연출만 얹은 것이라 전투 로직과는 무관.</summary>
        void PlayAttackFlash()
        {
            if (attackFlashImage != null)
            {
                attackFlashImage.DOKill();
                var c = attackFlashImage.color;
                attackFlashImage.color = new Color(c.r, c.g, c.b, 0.65f);
                attackFlashImage.DOFade(0f, 0.18f);
            }
            if (billboard != null)
            {
                billboard.DOKill();
                billboard.localScale = Vector3.one;
                billboard.DOPunchScale(Vector3.one * 0.16f, 0.16f, 4, 0.7f);
            }
        }

        /// <summary>피격 시 몸체를 빨갛게 번쩍였다가 원래 색으로 돌아온다. 체력바 두께를
        /// 키워도(0.24) 최대체력이 큰 유닛은 한 방의 변화가 여전히 작아서, 몸체 자체의 색
        /// 변화로 "맞았다"는 걸 확실히 알려준다 — 체력바에도 작은 펀치를 줘서 시선을 끈다.</summary>
        void PlayHitFlash()
        {
            if (bodyImage != null)
            {
                bodyImage.DOKill();
                bodyImage.color = new Color(1f, 0.15f, 0.15f, baseBodyColor.a);
                bodyImage.DOColor(baseBodyColor, 0.22f);
            }
            if (hpBarBgT != null)
            {
                hpBarBgT.DOKill();
                var baseScale = new Vector3(visualSize, Mathf.Max(0.05f, cellSize * 0.24f), 1f);
                hpBarBgT.localScale = baseScale;
                hpBarBgT.DOPunchScale(baseScale * 0.35f, 0.2f, 3, 0.6f);
            }
        }

        void Die()
        {
            State = SamgukUnitState.Dead;
            Target = null;
            if (bodyImage != null)
            {
                bodyImage.DOKill();
                bodyImage.color = new Color(0.35f, 0.35f, 0.35f, 0.5f);
            }
            if (facingDotT != null) facingDotT.gameObject.SetActive(false);
            if (dragCollider != null) dragCollider.enabled = false;
            // 죽은 시체는 겹쳤을 때 항상 살아있는 유닛 밑으로 깔려야 한다는 피드백 — 3D
            // SpriteRenderer는 UGUI와 달리 하이어라키 sibling 순서가 아니라 sortingOrder(+거리)로
            // 그려지는 순서가 정해지므로, 시체 쪽 스프라이트들의 sortingOrder를 확 낮춘다.
            if (bodyImage != null) bodyImage.sortingOrder -= DeadSortingOffset;
            if (label != null) label.sortingOrder -= DeadSortingOffset;
            if (hpFillImage != null) hpFillImage.sortingOrder -= DeadSortingOffset;
            if (attackFlashImage != null) attackFlashImage.sortingOrder -= DeadSortingOffset;
            if (hpBarBgImage != null)
            {
                hpBarBgImage.sortingOrder -= DeadSortingOffset;
                hpBarBgImage.color = NormalHpBarBgColor; // 기절한 채로 죽었으면 노란 틴트가 남아있을 수 있어 원복
            }
            stunTimer = 0f;
            bleedRemaining = 0f;
            OnDied?.Invoke(this);
        }

        /// <summary>배치 단계에서만 플레이어 유닛의 드래그 판정 콜라이더를 켠다.</summary>
        public void SetDraggable(bool draggable)
        {
            if (dragCollider != null) dragCollider.enabled = draggable;
        }

        const int DeadSortingOffset = 20;
        const float FacingToleranceDeg = 9f;
        const float OverlapRadius = 0.62f;
        const float OverlapPushSpeedMult = 0.5f;
        static readonly Color NormalHpBarBgColor = new Color(0f, 0f, 0f, 0.65f);
        static readonly Color StunHpBarBgColor = new Color(0.95f, 0.85f, 0.15f, 0.85f);
        const float MaxLifestealPercent = 0.8f; // 과도한 눈덩이 방지

        /// <summary>매 프레임 전투 FSM — ssam.md 41절 개정판을 그대로 따른다:
        /// ①겹친 상대가 있으면 그 반대방향으로만 밀려난다(바라보는 방향은 안 바뀜, 이번 틱은 여기서 끝,
        /// 속도는 절반(OverlapPushSpeedMult) — 정상 이동보다 느리게 밀려나야 두 유닛이 서로를
        /// 밀어내며 진동하는 게 덜 튄다)
        /// ②없으면 가장 가까운 적을 찾고, 없으면 대기
        /// ③적이 내가 바라보는 방향(허용각 안)에 없으면 그 방향으로 "회전"만 하고 이번 틱은 끝
        /// ④바라보고 있으면 사거리 안=대기(공격), 사거리 밖=바라보는 방향으로 이동
        ///   — 단, 이번 프레임 이동으로 타겟 Controller와 겹칠 만큼 가까워질 예정이면 그 이동
        ///   자체를 생략한다(반응형 밀어내기 ①이 나중에 처리하게 두지 않고, 겹치기 전에 미리 멈춘다).
        /// allUnits는 겹침 판정용(진영 무관), enemies는 타겟 탐색용(반대 진영만) — 둘 다
        /// BattleManager가 매 프레임 미리 걸러서 넘겨준다.</summary>
        public void Tick(float dt, IReadOnlyList<SamgukUnit> allUnits, IReadOnlyList<SamgukUnit> enemies)
        {
            if (!IsAlive) return;

            // 재생/출혈은 기절 중에도 계속 진행된다 — 몸이 못 움직일 뿐 회복이나 피흘림 자체가
            // 멎을 이유는 없다는 해석. 출혈로 죽을 수 있어 곧바로 IsAlive를 한 번 더 확인한다.
            ProcessRegenAndBleed(dt);
            if (!IsAlive) return;

            dashCooldownTimer -= dt;

            bool stunned = stunTimer > 0f;
            if (hpBarBgImage != null) hpBarBgImage.color = stunned ? StunHpBarBgColor : NormalHpBarBgColor;
            if (stunned)
            {
                stunTimer -= dt;
                State = SamgukUnitState.Stunned;
                return; // 겹침 판정·타겟팅·이동·공격·무기 전부 이번 틱은 완전히 멈춘다
            }

            attackCooldown -= dt;
            TickWeapons(dt, enemies);

            var overlapping = FindOverlap(allUnits);
            if (overlapping != null)
            {
                State = SamgukUnitState.Moving;
                Vector2 away = BoardPosition - overlapping.BoardPosition;
                if (away.sqrMagnitude < 0.0001f) away = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                away.Normalize();
                BoardPosition += away * MoveSpeed * OverlapPushSpeedMult * dt;
                ClampToBoard();
                SyncTransform();
                return;
            }

            if (Target == null || !Target.IsAlive)
            {
                Target = FindNearest(enemies);
            }

            if (Target == null)
            {
                State = SamgukUnitState.SearchingTarget;
                return;
            }

            Vector2 toTarget = Target.BoardPosition - BoardPosition;
            float dist = toTarget.magnitude;
            bool facingTarget = dist > 0.0001f && Vector2.Angle(FacingDirection, toTarget) <= FacingToleranceDeg;

            if (!facingTarget)
            {
                State = SamgukUnitState.Rotating;
                if (dist > 0.0001f) SetFacing(toTarget.normalized);
                return;
            }

            if (dist <= AttackRange)
            {
                State = SamgukUnitState.Attacking;
                if (attackCooldown <= 0f)
                {
                    float dmg = Mathf.Max(Attack - Target.Defense, MinDamage);
                    Target.TakeDamage(dmg);
                    if (lifestealPercent > 0f) Heal(dmg * lifestealPercent);
                    PlayAttackFlash();
                    attackCooldown = 1f / Mathf.Max(0.01f, AttackSpeed);
                }
            }
            else
            {
                State = SamgukUnitState.Moving;
                // 장신구 돌진 옵션 — 쿨타임이 다 됐고 아직 꽤 멀리 있을 때만 발동한다(사거리
                // 바로 밖 정도의 자잘한 거리에 쓰면 순간이동이 눈에 띄게 뻔해 보인다).
                if (dashCooldownCached > 0f && dashCooldownTimer <= 0f && dist > AttackRange * 1.25f)
                {
                    float dashStep = Mathf.Min(dashDistanceCached, dist - AttackRange * 0.5f);
                    if (dashStep > 0f)
                    {
                        BoardPosition += FacingDirection * dashStep;
                        ClampToBoard();
                        SyncTransform();
                        dashCooldownTimer = dashCooldownCached;
                        PlayAttackFlash(); // 별도 연출을 새로 안 만들고 기존 펀치+플래시를 재사용
                        return;
                    }
                }
                Vector2 step = FacingDirection * MoveSpeed * dt;
                // 예외 처리: 이번 이동으로 타겟 Controller와 실제로 겹칠 만큼(OverlapRadius 이내)
                // 가까워질 예정이면, 겹치는 그 순간까지 가지 않고 이번 틱은 그냥 제자리에 머문다.
                float predictedDistToTarget = ((Target.BoardPosition) - (BoardPosition + step)).magnitude;
                if (predictedDistToTarget >= OverlapRadius)
                {
                    BoardPosition += step;
                    ClampToBoard();
                    SyncTransform();
                }
            }
        }

        void TickWeapons(float dt, IReadOnlyList<SamgukUnit> enemies)
        {
            for (int i = 0; i < weaponSlots.Count; i++)
            {
                var w = weaponSlots[i];
                w.cooldown -= dt;
                if (w.cooldown > 0f) continue;
                var wt = PickWeaponTarget(w, enemies);
                if (wt == null) continue;
                w.cooldown = Mathf.Max(0.05f, w.data.weaponCooldown);
                float dmg = Mathf.Max(w.effDamage + Attack * w.effScaling - wt.Defense, MinDamage);
                FireWeapon(w, wt, dmg, enemies);
                PlayAttackFlash();
            }
        }

        SamgukUnit PickWeaponTarget(WeaponSlot w, IReadOnlyList<SamgukUnit> enemies)
        {
            float range2 = w.effRange * w.effRange;
            SamgukUnit best = null;
            switch (w.data.targetType)
            {
                case SamgukTargetType.Farthest:
                    {
                        float bestD = -1f;
                        for (int i = 0; i < enemies.Count; i++)
                        {
                            var e = enemies[i];
                            if (e == null || !e.IsAlive) continue;
                            float d = (e.BoardPosition - BoardPosition).sqrMagnitude;
                            if (d <= range2 && d > bestD) { bestD = d; best = e; }
                        }
                        break;
                    }
                case SamgukTargetType.LowestHP:
                    {
                        float bestHp = float.MaxValue;
                        for (int i = 0; i < enemies.Count; i++)
                        {
                            var e = enemies[i];
                            if (e == null || !e.IsAlive) continue;
                            if ((e.BoardPosition - BoardPosition).sqrMagnitude > range2) continue;
                            if (e.CurrentHP < bestHp) { bestHp = e.CurrentHP; best = e; }
                        }
                        break;
                    }
                case SamgukTargetType.HighestHP:
                    {
                        float bestHp = -1f;
                        for (int i = 0; i < enemies.Count; i++)
                        {
                            var e = enemies[i];
                            if (e == null || !e.IsAlive) continue;
                            if ((e.BoardPosition - BoardPosition).sqrMagnitude > range2) continue;
                            if (e.CurrentHP > bestHp) { bestHp = e.CurrentHP; best = e; }
                        }
                        break;
                    }
                case SamgukTargetType.Random:
                    {
                        var inRange = new List<SamgukUnit>();
                        for (int i = 0; i < enemies.Count; i++)
                        {
                            var e = enemies[i];
                            if (e == null || !e.IsAlive) continue;
                            if ((e.BoardPosition - BoardPosition).sqrMagnitude <= range2) inRange.Add(e);
                        }
                        if (inRange.Count > 0) best = inRange[Random.Range(0, inRange.Count)];
                        break;
                    }
                default: // Nearest
                    {
                        float bestD = float.MaxValue;
                        for (int i = 0; i < enemies.Count; i++)
                        {
                            var e = enemies[i];
                            if (e == null || !e.IsAlive) continue;
                            float d = (e.BoardPosition - BoardPosition).sqrMagnitude;
                            if (d <= range2 && d < bestD) { bestD = d; best = e; }
                        }
                        break;
                    }
            }
            return best;
        }

        void FireWeapon(WeaponSlot w, SamgukUnit target, float dmg, IReadOnlyList<SamgukUnit> enemies)
        {
            switch (w.data.attackType)
            {
                case SamgukAttackType.Splash:
                    ApplySplash(w, target.BoardPosition, w.data.splashRadius, dmg, enemies);
                    break;
                case SamgukAttackType.Projectile:
                    // 투사체는 도착 시점에야 실제로 명중하므로, 넉백/기절/출혈/흡혈 전부 발사
                    // 순간이 아니라 SamgukProjectile 자신이 착탄할 때 적용해야 한다 — 그래서
                    // 발동에 필요한 값 전부와 "치유 대상(나 자신)"을 같이 넘긴다.
                    SamgukProjectile.Spawn(transform.parent, cellSize, BoardPosition, target, w.data.projectileSpeed, dmg, w.data.tintColor,
                        this, w.knockback, w.stunChance, w.stunDuration, w.bleedChance, w.bleedDamagePerTick, w.bleedDuration, w.bleedTickInterval, lifestealPercent);
                    break;
                default: // SingleHit
                    ResolveWeaponHit(w, target, dmg);
                    break;
            }
        }

        void ApplySplash(WeaponSlot w, Vector2 center, float radius, float dmg, IReadOnlyList<SamgukUnit> enemies)
        {
            float r2 = radius * radius;
            for (int i = 0; i < enemies.Count; i++)
            {
                var e = enemies[i];
                if (e == null || !e.IsAlive) continue;
                if ((e.BoardPosition - center).sqrMagnitude <= r2) ResolveWeaponHit(w, e, dmg);
            }
        }

        /// <summary>무기 한 발이 실제로 명중했을 때 공통으로 처리되는 후속 효과 — 피해 자체는
        /// 이미 호출부가 계산해 건네준 dmg를 그대로 적용하고, 그 위에 이 무기의 명중 옵션
        /// (넉백/기절/출혈)과 장착자의 흡혈을 얹는다. SingleHit·Splash(각 피격 대상마다)·
        /// 투사체 착탄(SamgukProjectile) 세 경로가 전부 이 로직을 공유한다.</summary>
        void ResolveWeaponHit(WeaponSlot w, SamgukUnit target, float dmg)
        {
            target.TakeDamage(dmg);
            if (lifestealPercent > 0f) Heal(dmg * lifestealPercent);
            if (w.knockback > 0f) target.ApplyKnockback(BoardPosition, w.knockback);
            if (w.stunChance > 0f && Random.value <= w.stunChance) target.ApplyStun(w.stunDuration);
            if (w.bleedChance > 0f && Random.value <= w.bleedChance) target.ApplyBleed(w.bleedDamagePerTick, w.bleedDuration, w.bleedTickInterval);
        }

        void ClampToBoard()
        {
            BoardPosition.x = Mathf.Clamp(BoardPosition.x, -BoardHalfExtent, BoardHalfExtent);
            BoardPosition.y = Mathf.Clamp(BoardPosition.y, -BoardHalfExtent, BoardHalfExtent);
        }

        void SetFacing(Vector2 dir)
        {
            FacingDirection = dir;

            // 방향에 따라 캐릭터 스프라이트를 좌우 반전(2026-09-20, 사용자 요청). 보드 X축이
            // 월드 좌/우로 그대로 매핑되므로(SyncTransform 참고) dir.x 부호로 판단한다.
            // localScale 부호를 뒤집지 않고 SpriteRenderer.flipX를 쓴 이유 — bodyGo.transform.
            // localScale은 이미 아이콘 원본 비율(aspect)로 X/Y를 각각 계산해 둔 값이라(Init 참고),
            // 부호만 뒤집으면 같은 계산을 다시 하지 않고도 안전하게 좌우만 뒤집힌다. dir.x가
            // 거의 0(위/아래를 바라볼 때)이면 이전 방향을 그대로 유지해 미세한 좌우 깜빡임을 막는다.
            if (bodyImage != null && Mathf.Abs(dir.x) > 0.05f)
                bodyImage.flipX = dir.x < 0f;

            if (facingDotT == null) return;
            float radius = cellSize * 0.42f;
            // 바닥에 눕혀진(X=90) 마커라 로컬 X/Y가 월드 X/Z에 대응한다.
            facingDotT.localPosition = new Vector3(dir.x * radius, 0.02f, dir.y * radius);
        }

        SamgukUnit FindNearest(IReadOnlyList<SamgukUnit> pool)
        {
            SamgukUnit best = null;
            float bestDist = float.MaxValue;
            for (int i = 0; i < pool.Count; i++)
            {
                var c = pool[i];
                if (c == null || !c.IsAlive) continue;
                float d = (c.BoardPosition - BoardPosition).sqrMagnitude;
                if (d < bestDist) { bestDist = d; best = c; }
            }
            return best;
        }

        SamgukUnit FindOverlap(IReadOnlyList<SamgukUnit> pool)
        {
            SamgukUnit best = null;
            float bestDist = float.MaxValue;
            float r2 = OverlapRadius * OverlapRadius;
            for (int i = 0; i < pool.Count; i++)
            {
                var o = pool[i];
                if (o == null || o == this || !o.IsAlive) continue;
                float d = (o.BoardPosition - BoardPosition).sqrMagnitude;
                if (d < r2 && d < bestDist) { bestDist = d; best = o; }
            }
            return best;
        }
    }
}

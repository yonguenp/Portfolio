using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SamgukDefense
{
    /// <summary>인벤토리/장비/조합 패널 화면 구성 전담. 로직은 SamgukBattleManager.Items.cs.</summary>
    public partial class SamgukBattleManager
    {
        RectTransform inventoryButton;
        RectTransform inventoryPanel;
        RectTransform equipListArea;
        RectTransform equipListContent;
        RectTransform equipSlotTemplate;
        RectTransform itemListArea;
        RectTransform itemListContent;
        RectTransform itemRowTemplate;
        TMP_Text inventoryLogText;
        SamgukItemData selectedInventoryItem;

        void BuildInventoryPanel()
        {
            inventoryButton = MakeRect(canvasRoot.Find("BottomBar") as RectTransform ?? canvasRoot, "InventoryBtn",
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-40f, 0f), new Vector2(260f, 76f));
            var invBtnImg = AddImage(inventoryButton, new Color(0.32f, 0.28f, 0.12f), true);
            var invBtn = GetOrAddButton(inventoryButton, invBtnImg);
            invBtn.onClick.AddListener(() => { RefreshInventoryPanel(); inventoryPanel.gameObject.SetActive(true); });
            AddLabel(inventoryButton, "인벤토리", 28f, Color.white);

            inventoryPanel = MakeRect(canvasRoot, "InventoryPanel", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            AddImage(inventoryPanel, ColorDim, true);

            var panel = MakeRect(inventoryPanel, "Panel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1700f, 900f));
            AddImage(panel, ColorPanel, true);

            var title = MakeRect(panel, "Title", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -36f), new Vector2(1500f, 50f));
            AddLabel(title, "장비 · 인벤토리 · 조합", 32f, ColorAccent);

            var closeRt = MakeRect(panel, "Close", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-30f, -30f), new Vector2(90f, 60f));
            var closeImg = AddImage(closeRt, ColorButtonDisabled, true);
            var closeBtn = GetOrAddButton(closeRt, closeImg);
            closeBtn.onClick.AddListener(() => inventoryPanel.gameObject.SetActive(false));
            AddLabel(closeRt, "닫기", 24f, Color.white);

            // 일괄조합 — 인벤토리에서 가능한 조합을 전부(연쇄 포함) 한 번에 처리한다(CraftAll,
            // SamgukBattleManager.Items.cs 참고). Close 버튼 바로 왼쪽에 둔다.
            var craftAllRt = MakeRect(panel, "CraftAllBtn", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-130f, -30f), new Vector2(160f, 60f));
            var craftAllImg = AddImage(craftAllRt, ColorButton, true);
            var craftAllBtn = GetOrAddButton(craftAllRt, craftAllImg);
            craftAllBtn.onClick.AddListener(() => { CraftAll(); RefreshInventoryPanel(); });
            AddLabel(craftAllRt, "일괄조합", 22f, Color.white);

            // 일괄장착 — 배치된 전 캐릭터의 슬롯을 인벤토리에서 구할 수 있는 최선의 장비(등급
            // 우선, 동일 등급이면 전용장비 우선)로 채운다(EquipAllBest, SamgukBattleManager.Items.cs
            // 참고). 일괄조합 바로 왼쪽에 10px 간격으로 둔다.
            var equipAllRt = MakeRect(panel, "EquipAllBtn", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-300f, -30f), new Vector2(160f, 60f));
            var equipAllImg = AddImage(equipAllRt, ColorButton, true);
            var equipAllBtn = GetOrAddButton(equipAllRt, equipAllImg);
            equipAllBtn.onClick.AddListener(() => { EquipAllBest(); RefreshInventoryPanel(); });
            AddLabel(equipAllRt, "일괄장착", 22f, Color.white);

            // 장비/아이템 둘 다 데이터 개수가 화면을 넘길 수 있는 목록이라 진짜 ScrollRect로
            // 만든다(마스크로 클리핑 + 스크롤로 나머지 확인) — 예전엔 마스크도 스크롤도 없어서
            // 아이템이 많아지면 패널 밖으로 그냥 넘쳐흘렀다.
            equipListArea = BuildVerticalScrollArea(panel, "EquipList", new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(10f, -10f), new Vector2(800f, 760f), out equipListContent);
            itemListArea = BuildVerticalScrollArea(panel, "ItemList", new Vector2(0.5f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-10f, -10f), new Vector2(830f, 760f), out itemListContent);

            equipSlotTemplate = BuildEquipSlotTemplate(equipListContent, 90f);
            itemRowTemplate = BuildItemRowTemplate(itemListContent);

            var logRt = MakeRect(panel, "Log", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 20f), new Vector2(1500f, 36f));
            inventoryLogText = AddLabel(logRt, "", 22f, ColorAccent);

            inventoryPanel.gameObject.SetActive(false);
            inventoryButton.gameObject.SetActive(false);
        }

        /// <summary>장비 슬롯 하나의 디자인 원본 — 런 중 인벤토리 패널 전용(로비는 장비를 안
        /// 다룬다, Run Data이므로). 에디터에서 이 템플릿(EquipList/Viewport/Content/SlotTemplate)
        /// 만 고치면 슬롯 전체가 바뀐다.</summary>
        static RectTransform BuildEquipSlotTemplate(RectTransform parent, float slotSize)
        {
            var slot = MakeRect(parent, "SlotTemplate", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), Vector2.zero, new Vector2(slotSize, slotSize));
            var img = AddImage(slot, new Color(0.16f, 0.16f, 0.16f), true);
            GetOrAddButton(slot, img);
            // 아이콘이 있으면 이걸로 채우고(RebuildEquipList가 activeSelf/sprite를 매번 갱신),
            // 없으면 기존처럼 "빈 슬롯"/아이템 이름 텍스트만 보여준다.
            var iconRt = MakeRect(slot, "Icon", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(slotSize - 16f, slotSize - 16f));
            var iconImg = AddImage(iconRt, Color.white);
            iconImg.preserveAspect = true;
            iconRt.gameObject.SetActive(false);
            AddLabel(slot, "빈 슬롯", 16f, Color.white, TextAlignmentOptions.Center, autoSize: true);

            // 등급 배지(좌상단) — 슬롯이 비었으면 RebuildEquipList가 꺼둔다.
            var gradeBadgeRt = MakeRect(slot, "GradeBadge", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(2f, -2f), new Vector2(24f, 24f));
            AddImage(gradeBadgeRt, Color.gray, false);
            AddLabel(gradeBadgeRt, "1", 15f, Color.white);
            gradeBadgeRt.gameObject.SetActive(false);

            // 주인 캐릭터 일치 배지(우상단) — 지금 이 슬롯의 주인 캐릭터가 그 아이템의
            // ownerCharacterId와 실제로 일치할 때(=+20% 보너스가 적용 중일 때)만 켠다.
            var ownerBadgeRt = MakeRect(slot, "OwnerBadge", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-2f, -2f), new Vector2(24f, 24f));
            AddImage(ownerBadgeRt, ColorAccent, false);
            AddLabel(ownerBadgeRt, "★", 16f, Color.white);
            ownerBadgeRt.gameObject.SetActive(false);

            slot.gameObject.SetActive(false);
            return slot;
        }

        /// <summary>아이템 목록 행 하나의 디자인 원본 — 런 중 인벤토리 패널 전용. 이름 한 줄뿐이던
        /// 예전 66px 행을, 등급 배지 + 이름(등급색) + 효과 요약 + 주인 캐릭터까지 3줄로 보여주도록
        /// 108px로 키웠다("아이템 등급/효과/주인 캐릭터 정보 표시" 요청, 2026-09-15).</summary>
        static RectTransform BuildItemRowTemplate(RectTransform parent)
        {
            const float rowH = 108f;
            var row = MakeRect(parent, "RowTemplate", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, rowH));
            var rowImg = AddImage(row, new Color(0.18f, 0.17f, 0.14f), true);
            GetOrAddButton(row, rowImg);

            // 아이콘(있으면) — 왼쪽 위. 없는 아이템은 숨긴다.
            var iconRt = MakeRect(row, "Icon", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(6f, -6f), new Vector2(54f, 54f));
            var iconImg = AddImage(iconRt, Color.white);
            iconImg.preserveAspect = true;
            iconRt.gameObject.SetActive(false);

            // 등급 배지 — 아이콘 유무와 무관하게 항상 왼쪽 위 구석에(아이콘 없는 아이템도
            // 등급은 보여야 하므로 아이콘과 별개의 형제 오브젝트로 둔다).
            var gradeBadgeRt = MakeRect(row, "GradeBadge", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(4f, -4f), new Vector2(24f, 24f));
            AddImage(gradeBadgeRt, Color.gray, false);
            AddLabel(gradeBadgeRt, "1", 15f, Color.white);

            // 이름 × 개수 — 등급색으로 칠해 목록에서 한눈에 등급을 구분한다.
            var titleArea = MakeRect(row, "TitleArea", new Vector2(0f, 1f), new Vector2(0.78f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -4f), new Vector2(0f, 28f));
            titleArea.offsetMin = new Vector2(66f, titleArea.offsetMin.y);
            AddLabel(titleArea, "아이템 이름 ×1", 22f, ColorTextMain, TextAlignmentOptions.Left, autoSize: true);

            // 효과 요약(피해/사거리/쿨타임 또는 보너스 스탯)
            var effectArea = MakeRect(row, "EffectArea", new Vector2(0f, 1f), new Vector2(0.78f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -32f), new Vector2(0f, 24f));
            effectArea.offsetMin = new Vector2(66f, effectArea.offsetMin.y);
            AddLabel(effectArea, "", 17f, ColorTextSub, TextAlignmentOptions.Left, autoSize: true);

            // 주인 캐릭터(있을 때만 채워짐 — RebuildItemList가 빈 문자열이면 자리만 비워둔다)
            var ownerArea = MakeRect(row, "OwnerArea", new Vector2(0f, 1f), new Vector2(0.78f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -56f), new Vector2(0f, 24f));
            ownerArea.offsetMin = new Vector2(66f, ownerArea.offsetMin.y);
            AddLabel(ownerArea, "", 17f, ColorAccent, TextAlignmentOptions.Left, autoSize: true);

            var craftRt = MakeRect(row, "Craft", new Vector2(0.78f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0f, 54f));
            var craftImg = AddImage(craftRt, ColorButton, true);
            GetOrAddButton(craftRt, craftImg);
            AddLabel(craftRt, "조합", 20f, Color.white);

            row.gameObject.SetActive(false);
            return row;
        }

        void AppendInventoryLog(string msg)
        {
            if (inventoryLogText != null) inventoryLogText.text = msg;
        }

        void RefreshInventoryPanel()
        {
            selectedInventoryItem = null;
            RebuildEquipList();
            RebuildItemList();
        }

        void RebuildEquipList()
        {
            foreach (RectTransform child in equipListContent)
                if (child != equipSlotTemplate) SafeDestroy(child.gameObject);

            float y = 0f;
            const float rowH = 130f;
            float slotSize = equipSlotTemplate.sizeDelta.x, gap = 10f;
            foreach (var unit in playerUnits)
            {
                if (unit == null || unit.SourceData == null) continue;
                // 이름을 캐릭터별로 다르게 줘야 한다 — MakeRect는 이름으로 재사용 여부를 판단하므로,
                // 전부 "Name"으로 고정하면 두 번째 캐릭터부터는 첫 번째 것을 그대로 돌려받는다
                // (그러면 텍스트도 위치도 갱신이 안 된다 — 실제로 겪은 버그).
                var rowTitle = MakeRect(equipListContent, $"Name_{unit.SourceData.id}", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(0f, 30f));
                rowTitle.anchoredPosition = new Vector2(0f, y);
                AddLabel(rowTitle, $"{unit.SourceData.displayName} (HP {unit.CurrentHP:0}/{unit.MaxHP:0})", 24f, ColorTextMain, TextAlignmentOptions.Left, autoSize: true);

                var slots = GetOrCreateEquipSlots(unit.SourceData);
                for (int i = 0; i < slots.Length; i++)
                {
                    var slotRt = Instantiate(equipSlotTemplate, equipListContent);
                    slotRt.name = $"Slot{i}";
                    slotRt.anchoredPosition = new Vector2(i * (slotSize + gap), y - 40f);
                    slotRt.gameObject.SetActive(true);

                    var item = slots[i];
                    slotRt.GetComponent<Image>().color = item != null ? new Color(0.24f, 0.30f, 0.16f) : new Color(0.16f, 0.16f, 0.16f);
                    bool slotHasIcon = item != null && item.icon != null;
                    var slotIconRt = slotRt.Find("Icon");
                    slotIconRt.gameObject.SetActive(slotHasIcon);
                    if (slotHasIcon) slotIconRt.GetComponent<Image>().sprite = item.icon;
                    var slotLabel = slotRt.Find("Label").GetComponent<TMP_Text>();
                    slotLabel.text = item != null ? item.displayName : "빈 슬롯";
                    slotLabel.gameObject.SetActive(!slotHasIcon);

                    // 등급 배지 — 채워진 슬롯에만. 주인 배지 — 이 캐릭터가 실제로 그 아이템의
                    // ownerCharacterId와 일치해 +20% 보너스를 받고 있을 때만(위 별점).
                    var gradeBadgeRt = slotRt.Find("GradeBadge");
                    gradeBadgeRt.gameObject.SetActive(item != null);
                    if (item != null)
                    {
                        gradeBadgeRt.GetComponent<Image>().color = GradeColor(item.grade);
                        gradeBadgeRt.Find("Label").GetComponent<TMP_Text>().text = item.grade.ToString();
                    }
                    bool ownerMatch = item != null && !string.IsNullOrEmpty(item.ownerCharacterId) && item.ownerCharacterId == unit.SourceData.id;
                    slotRt.Find("OwnerBadge").gameObject.SetActive(ownerMatch);

                    var slotBtn = slotRt.GetComponent<Button>();
                    slotBtn.onClick.RemoveAllListeners();
                    var capturedData = unit.SourceData;
                    int capturedIndex = i;
                    slotBtn.onClick.AddListener(() => OnSlotClicked(capturedData, capturedIndex));
                }
                y -= rowH;
            }
            SetVerticalContentExtent(equipListContent, equipListArea.sizeDelta.y, -y);
        }

        void OnSlotClicked(SamgukCharacterData data, int slotIndex)
        {
            var slots = GetOrCreateEquipSlots(data);
            var current = slots[slotIndex];
            if (selectedInventoryItem != null)
            {
                EquipItem(data, slotIndex, selectedInventoryItem);
                selectedInventoryItem = null;
            }
            else if (current != null)
            {
                EquipItem(data, slotIndex, null);
            }
            RefreshInventoryPanel();
        }

        void RebuildItemList()
        {
            foreach (RectTransform child in itemListContent)
                if (child != itemRowTemplate) SafeDestroy(child.gameObject);
            if (itemRoster == null) return;

            float y = 0f;
            float rowH = itemRowTemplate.sizeDelta.y + 8f;
            foreach (var item in itemRoster)
            {
                if (item == null) continue;
                inventory.TryGetValue(item, out int count);
                if (count <= 0) continue;

                var rowRt = Instantiate(itemRowTemplate, itemListContent);
                rowRt.name = $"Row_{item.id}";
                rowRt.anchoredPosition = new Vector2(0f, y);
                rowRt.gameObject.SetActive(true);

                bool isSelected = selectedInventoryItem == item;
                rowRt.GetComponent<Image>().color = isSelected ? new Color(ColorAccent.r, ColorAccent.g, ColorAccent.b, 0.5f) : new Color(0.18f, 0.17f, 0.14f);
                var rowIconRt = rowRt.Find("Icon");
                bool rowHasIcon = item.icon != null;
                rowIconRt.gameObject.SetActive(rowHasIcon);
                if (rowHasIcon) rowIconRt.GetComponent<Image>().sprite = item.icon;

                rowRt.Find("GradeBadge").GetComponent<Image>().color = GradeColor(item.grade);
                rowRt.Find("GradeBadge/Label").GetComponent<TMP_Text>().text = item.grade.ToString();

                var capturedItem = item;
                var rowBtn = rowRt.GetComponent<Button>();
                rowBtn.onClick.RemoveAllListeners();
                rowBtn.onClick.AddListener(() =>
                {
                    selectedInventoryItem = selectedInventoryItem == capturedItem ? null : capturedItem;
                    RebuildItemList();
                });

                var titleLabel = rowRt.Find("TitleArea/Label").GetComponent<TMP_Text>();
                titleLabel.text = $"[{ItemTypeLabel(item.itemType)}·{GradeLabel(item.grade)}] {item.displayName} ×{count}";
                titleLabel.color = GradeColor(item.grade);
                rowRt.Find("EffectArea/Label").GetComponent<TMP_Text>().text = BuildEffectSummary(item);
                rowRt.Find("OwnerArea/Label").GetComponent<TMP_Text>().text = OwnerSummary(item) ?? "";

                var craftRt = rowRt.Find("Craft");
                bool craftable = item.upgradesTo != null && count >= 3;
                craftRt.gameObject.SetActive(craftable);
                if (craftable)
                {
                    var craftBtn = craftRt.GetComponent<Button>();
                    craftBtn.onClick.RemoveAllListeners();
                    craftBtn.onClick.AddListener(() => { TryCraft(capturedItem); RefreshInventoryPanel(); });
                }

                y -= rowH;
            }
            SetVerticalContentExtent(itemListContent, itemListArea.sizeDelta.y, -y);
        }

        static string ItemTypeLabel(SamgukItemType t) => t switch
        {
            SamgukItemType.Weapon => "무기",
            SamgukItemType.Armor => "방어구",
            SamgukItemType.Accessory => "장식",
            _ => "?"
        };

        static string GradeLabel(int grade) => grade switch
        {
            1 => "일반",
            2 => "고급",
            3 => "희귀",
            4 => "영웅",
            5 => "전설",
            6 => "신화",
            _ => "?"
        };

        /// <summary>등급색 — 일반(회백)/고급(초록)/희귀(파랑)/영웅(보라)/전설(금)/신화(적주홍)
        /// 순으로 점점 화려해지는 통상적인 등급색 배열. 아이템 이름 텍스트와 좌상단 배지 양쪽에
        /// 쓰인다. 전투 로직과는 무관한 순수 표시용 색. (2026-09-16 3→6등급 확장.)</summary>
        static Color GradeColor(int grade) => grade switch
        {
            1 => new Color(0.78f, 0.78f, 0.78f),
            2 => new Color(0.42f, 0.82f, 0.38f),
            3 => new Color(0.40f, 0.62f, 0.98f),
            4 => new Color(0.68f, 0.42f, 0.94f),
            5 => new Color(0.97f, 0.68f, 0.16f),
            6 => new Color(0.95f, 0.28f, 0.24f),
            _ => Color.white
        };

        static string AttackTypeLabel(SamgukAttackType t) => t switch
        {
            SamgukAttackType.SingleHit => "단일 타격",
            SamgukAttackType.Projectile => "투사체",
            SamgukAttackType.Splash => "범위 피해",
            _ => "?"
        };

        static string TargetTypeLabel(SamgukTargetType t) => t switch
        {
            SamgukTargetType.Nearest => "가장 가까운 적",
            SamgukTargetType.Farthest => "가장 먼 적",
            SamgukTargetType.LowestHP => "체력 낮은 적",
            SamgukTargetType.HighestHP => "체력 높은 적",
            SamgukTargetType.Random => "무작위 적",
            _ => "?"
        };

        /// <summary>아이템의 실제 수치 효과를 한 줄로 요약한다 — 무기면 피해/사거리/쿨타임/
        /// 공격방식/타겟팅까지, 그 외엔 0이 아닌 보너스 스탯만 " · "로 이어붙인다.</summary>
        static string BuildEffectSummary(SamgukItemData item)
        {
            var parts = new List<string>();
            if (item.itemType == SamgukItemType.Weapon)
            {
                parts.Add(item.weaponDamageAttackScaling > 0f
                    ? $"피해 {item.weaponDamage:0}+공격력×{item.weaponDamageAttackScaling:0.0#}"
                    : $"피해 {item.weaponDamage:0}");
                parts.Add($"사거리 {item.weaponRange:0.#}");
                parts.Add($"쿨타임 {item.weaponCooldown:0.#}초");
                parts.Add(AttackTypeLabel(item.attackType));
                parts.Add(TargetTypeLabel(item.targetType));
            }
            if (item.bonusHP != 0f) parts.Add($"체력 +{item.bonusHP:0}");
            if (item.bonusAttack != 0f) parts.Add($"공격력 +{item.bonusAttack:0}");
            if (item.bonusDefense != 0f) parts.Add($"방어력 +{item.bonusDefense:0}");
            if (item.bonusAttackSpeed != 0f) parts.Add($"공속 +{item.bonusAttackSpeed:0.00}");
            if (item.bonusMoveSpeed != 0f) parts.Add($"이속 +{item.bonusMoveSpeed:0.0}");
            if (item.bonusAttackRange != 0f) parts.Add($"사거리 +{item.bonusAttackRange:0.0}");
            return string.Join(" · ", parts);
        }

        /// <summary>주인 캐릭터가 지정된 아이템이면 "OO 전용 (+20%)" 문구를, 없으면 null을
        /// 돌려준다(호출부가 빈 문자열로 대체해 그 줄을 비워둔다).</summary>
        string OwnerSummary(SamgukItemData item)
        {
            if (string.IsNullOrEmpty(item.ownerCharacterId)) return null;
            var c = FindCharacterData(item.ownerCharacterId);
            string name = c != null ? c.displayName : item.ownerCharacterId;
            int bonusPct = Mathf.RoundToInt((SamgukItemData.OwnerBonusMultiplier - 1f) * 100f);
            return $"{name} 전용 (+{bonusPct}%)";
        }
    }
}

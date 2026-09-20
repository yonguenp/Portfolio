using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SamgukDefense
{
    /// <summary>로비 화면(뽑기·보유 장수·영구 강화) 구성 전담. 로직은 SamgukBattleManager.Meta.cs.
    /// 절차적 아트 없음 — 다른 화면들과 동일하게 단색 Image + TMP 텍스트만 쓴다.
    /// 아이템/장비는 Run Data라 로비에는 절대 안 보여준다(예전엔 여기 아이템 목록·장비 슬롯이
    /// 있었는데, 그게 "로비에 아이템이 이미 있다" 버그의 UI 쪽 절반이었다 — 2026-09-15 제거.
    /// 장비 관리는 런 중 인벤토리 패널(SamgukBattleManager.ItemsUI.cs)에서만 한다).</summary>
    public partial class SamgukBattleManager
    {
        RectTransform lobbyPanel;
        TMP_Text lobbyGoldText;
        TMP_Text lobbyGachaResultText;
        RectTransform lobbyOwnedListArea;
        RectTransform lobbyOwnedListContent;
        RectTransform lobbyCardTemplate;
        RectTransform lobbyDetailArea;
        RectTransform lobbyUpgradeRowTemplate;
        RectTransform lobbyStartBtn;
        RectTransform lobbyTeamSizeBtn;
        RectTransform lobbyCombineModeBtn;
        SamgukCharacterData selectedLobbyCharacter;

        /// <summary>조합(승급) 재료로 고른 캐릭터 목록 — 최대 3개, 같은 캐릭터를 여러 번 골라도
        /// 그만큼 중복으로 들어간다(2026-09-16 개정: "같은 장수 3마리"만 되던 걸 "등급만 같으면
        /// 아무 3마리"로 넓히며 신설). combineMode가 꺼지면(로비 재진입 포함) 항상 비운다.</summary>
        bool combineMode;
        readonly List<SamgukCharacterData> combineSelection = new List<SamgukCharacterData>();

        void BuildLobbyPanel()
        {
            lobbyPanel = MakeRect(canvasRoot, "LobbyPanel", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            AddImage(lobbyPanel, ColorDim, true);

            var panel = MakeRect(lobbyPanel, "Panel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1820f, 980f));
            AddImage(panel, ColorPanel, true);

            var title = MakeRect(panel, "Title", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -40f), new Vector2(300f, 50f));
            AddLabel(title, "로비", 36f, ColorAccent, TextAlignmentOptions.Left);

            var goldRt = MakeRect(panel, "Gold", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-40f, -40f), new Vector2(300f, 50f));
            lobbyGoldText = AddLabel(goldRt, "보유 골드: 0", 30f, ColorTextMain, TextAlignmentOptions.Right, autoSize: true);

            var gachaRt = MakeRect(panel, "GachaBtn", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-40f, -110f), new Vector2(260f, 68f));
            var gachaImg = AddImage(gachaRt, ColorButton, true);
            var gachaBtn = GetOrAddButton(gachaRt, gachaImg);
            gachaBtn.onClick.AddListener(OnGachaClicked);
            AddLabel(gachaRt, "장수 뽑기 (30G)", 24f, Color.white);

            var gachaResultRt = MakeRect(panel, "GachaResult", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-40f, -198f), new Vector2(420f, 40f));
            lobbyGachaResultText = AddLabel(gachaResultRt, "", 22f, ColorAccent, TextAlignmentOptions.Right, autoSize: true);

            // 출전 인원 확장 — 처음엔 1명뿐이고, 골드로 최대 10명까지 영구 확장한다
            // (2026-09-16 사용자 확정). Title 바로 아래, 뽑기 버튼과 겹치지 않는 왼쪽에 둔다.
            lobbyTeamSizeBtn = MakeRect(panel, "TeamSizeBtn", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -110f), new Vector2(420f, 68f));
            var teamSizeImg = AddImage(lobbyTeamSizeBtn, ColorButton, true);
            var teamSizeBtn = GetOrAddButton(lobbyTeamSizeBtn, teamSizeImg);
            teamSizeBtn.onClick.AddListener(() => { if (TryUpgradeTeamSize()) RefreshLobbyUI(); });
            AddLabel(lobbyTeamSizeBtn, "", 20f, Color.white, TextAlignmentOptions.Center, autoSize: true);

            // 장수 조합 모드 토글 — 켜져 있는 동안은 카드를 클릭할 때마다 조합 재료로 고르고,
            // 아래 상세 영역이 스탯/강화 대신 "선택 N/3 + 조합하기" 패널로 바뀐다
            // (2026-09-16, TeamSizeBtn 바로 아래에 같은 폭으로 둔다).
            lobbyCombineModeBtn = MakeRect(panel, "CombineModeBtn", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -184f), new Vector2(420f, 60f));
            var combineModeImg = AddImage(lobbyCombineModeBtn, ColorButton, true);
            var combineModeBtn = GetOrAddButton(lobbyCombineModeBtn, combineModeImg);
            combineModeBtn.onClick.AddListener(() => { combineMode = !combineMode; combineSelection.Clear(); RefreshLobbyUI(); });
            AddLabel(lobbyCombineModeBtn, "", 20f, Color.white, TextAlignmentOptions.Center, autoSize: true);

            // 뽑기로 보유 캐릭터가 늘어나면 한 줄에 다 안 들어갈 수 있으므로 가로 ScrollRect로.
            lobbyOwnedListArea = BuildHorizontalScrollArea(panel, "OwnedList", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -260f), new Vector2(1740f, 240f), out lobbyOwnedListContent);
            lobbyCardTemplate = BuildLobbyCardTemplate(lobbyOwnedListContent, 150f, 220f);

            // 아이템/장비 슬롯 섹션(예전엔 여기 있었다)을 없애면서 남는 폭을 이 패널이 전부 쓴다 —
            // 로비 상세는 이제 "선택한 장수의 기본+강화 스탯 확인 + 영구 강화 구매"만 한다.
            lobbyDetailArea = MakeRect(panel, "Detail", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -520f), new Vector2(1740f, 340f));
            lobbyUpgradeRowTemplate = BuildUpgradeRowTemplate(lobbyDetailArea);

            lobbyStartBtn = MakeRect(panel, "StartRun", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 20f), new Vector2(340f, 80f));
            var startImg = AddImage(lobbyStartBtn, ColorButton, true);
            var startBtn = GetOrAddButton(lobbyStartBtn, startImg);
            startBtn.onClick.AddListener(() => { HideLobbyUI(); ShowCharacterSelect(); });
            AddLabel(lobbyStartBtn, "전투 시작", 30f, Color.white);

            lobbyPanel.gameObject.SetActive(false);
        }

        /// <summary>보유 장수 카드 하나의 디자인 원본 — 캐릭터 선택 화면의 카드
        /// (SamgukBattleManager.UI.cs의 CardTemplate)와 별개다. 여기는 스탯 없이 작게 보여준다.</summary>
        static RectTransform BuildLobbyCardTemplate(RectTransform parent, float cardW, float cardH)
        {
            var card = MakeRect(parent, "CardTemplate", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(cardW, cardH));
            var cardImg = AddImage(card, new Color(0.18f, 0.17f, 0.14f), true);
            GetOrAddButton(card, cardImg);
            var swatch = MakeRect(card, "Swatch", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -14f), new Vector2(cardW - 30f, cardW - 30f));
            AddImage(swatch, Color.gray);
            var nameRt = MakeRect(card, "Name", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 46f), new Vector2(cardW - 10f, 30f));
            AddLabel(nameRt, "이름", 20f, ColorTextMain, TextAlignmentOptions.Center, autoSize: true);
            var starRt = MakeRect(card, "Star", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 16f), new Vector2(cardW - 10f, 24f));
            AddLabel(starRt, "★★★", 16f, ColorAccent);

            // 보유 마릿수 뱃지(우상단) — 가챠 중복이 캐릭터를 그냥 버리지 않고 쌓이게 되면서
            // (2026-09-16) 몇 마리인지 한눈에 보여야 조합(승급) 재료가 모였는지 알 수 있다.
            var countRt = MakeRect(card, "Count", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-4f, -4f), new Vector2(36f, 28f));
            AddImage(countRt, new Color(0f, 0f, 0f, 0.75f), false);
            AddLabel(countRt, "×1", 16f, Color.white);

            card.gameObject.SetActive(false);
            return card;
        }

        /// <summary>강화 한 줄(스탯명+레벨 / 강화 버튼)의 디자인 원본.</summary>
        static RectTransform BuildUpgradeRowTemplate(RectTransform parent)
        {
            var row = MakeRect(parent, "UpRowTemplate", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, 28f));

            var labelRt = MakeRect(row, "Label", new Vector2(0f, 0.5f), new Vector2(0.58f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            AddLabel(labelRt, "스탯 Lv.0", 19f, ColorTextMain, TextAlignmentOptions.Left);

            var btnRt = MakeRect(row, "Btn", new Vector2(0.6f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0f, 28f));
            var btnImg = AddImage(btnRt, ColorButton, true);
            GetOrAddButton(btnRt, btnImg);
            AddLabel(btnRt, "강화 (0G)", 15f, Color.white);

            row.gameObject.SetActive(false);
            return row;
        }

        void ShowLobbyUI()
        {
            gameOverPanel.gameObject.SetActive(false);
            if (characterSelectPanel != null) characterSelectPanel.gameObject.SetActive(false);
            selectedInventoryItem = null;
            combineMode = false;
            combineSelection.Clear();
            if (selectedLobbyCharacter == null || !IsOwned(selectedLobbyCharacter.id))
                selectedLobbyCharacter = FirstOwnedCharacter();
            RefreshLobbyUI();
            lobbyPanel.gameObject.SetActive(true);
        }

        void HideLobbyUI() => lobbyPanel.gameObject.SetActive(false);

        SamgukCharacterData FirstOwnedCharacter()
        {
            if (characterRoster == null) return null;
            foreach (var c in characterRoster) if (c != null && IsOwned(c.id)) return c;
            return null;
        }

        void OnGachaClicked()
        {
            if (balance == null || persistentGold < balance.gachaCost)
            {
                if (lobbyGachaResultText != null) lobbyGachaResultText.text = "골드가 부족합니다";
                return;
            }
            if (TryGacha(out var result))
            {
                lobbyGachaResultText.text = $"{new string('★', Mathf.Clamp(result.star, 1, 6))} {result.displayName} 획득!";
                selectedLobbyCharacter = result;
                RefreshLobbyUI();
            }
        }

        void RefreshLobbyUI()
        {
            lobbyGoldText.text = $"보유 골드: {persistentGold}";
            SanitizeCombineSelection();
            RefreshTeamSizeButton();
            RefreshCombineModeButton();
            RebuildLobbyOwnedList();
            if (combineMode) RebuildCombinePanel();
            else RebuildLobbyDetail();

            bool canStart = ownedCharacterCounts.Count > 0;
            var img = lobbyStartBtn.GetComponent<Image>();
            var btn = lobbyStartBtn.GetComponent<Button>();
            btn.interactable = canStart;
            img.color = canStart ? ColorButton : ColorButtonDisabled;
        }

        void RefreshTeamSizeButton()
        {
            var img = lobbyTeamSizeBtn.GetComponent<Image>();
            var btn = lobbyTeamSizeBtn.GetComponent<Button>();
            bool canUpgrade = CanUpgradeTeamSize(out int cost);
            bool maxed = TeamSizeLevel >= AbsoluteMaxTeamSize;
            btn.interactable = canUpgrade;
            img.color = canUpgrade ? ColorButton : ColorButtonDisabled;
            lobbyTeamSizeBtn.Find("Label").GetComponent<TMP_Text>().text =
                maxed ? $"출전 인원 {TeamSizeLevel}/{AbsoluteMaxTeamSize} (MAX)" : $"출전 인원 확장 {TeamSizeLevel}→{TeamSizeLevel + 1} ({cost}G)";
        }

        void RefreshCombineModeButton()
        {
            var img = lobbyCombineModeBtn.GetComponent<Image>();
            img.color = combineMode ? new Color(ColorAccent.r, ColorAccent.g, ColorAccent.b, 0.9f) : ColorButton;
            lobbyCombineModeBtn.Find("Label").GetComponent<TMP_Text>().text =
                combineMode ? $"조합 모드 — 선택 {combineSelection.Count}/3 (끄기)" : "장수 조합";
        }

        /// <summary>조합 도중 화면 밖(강화/뽑기 등)에서 보유 마릿수가 바뀌어 선택이 더 이상
        /// 성립하지 않게 되는 경우(예: 다른 조합으로 재료가 먼저 소모됨)를 방어한다 — 매
        /// RefreshLobbyUI마다 값싸게 돌려도 되는 순수 정리 작업이라 항상 호출한다.</summary>
        void SanitizeCombineSelection()
        {
            if (combineSelection.Count == 0) return;
            var counts = new Dictionary<string, int>();
            for (int i = combineSelection.Count - 1; i >= 0; i--)
            {
                var id = combineSelection[i].id;
                counts.TryGetValue(id, out int c);
                c++;
                if (c > GetOwnedCount(id)) combineSelection.RemoveAt(i);
                else counts[id] = c;
            }
        }

        /// <summary>카드 클릭으로 재료를 하나 추가한다 — 이미 3장이 찼거나, 이 캐릭터를 선택
        /// 목록에서 이미 보유 마릿수만큼 다 골랐으면 무시한다(더 고르려면 먼저 "선택 초기화").</summary>
        void AddCombinePick(SamgukCharacterData data)
        {
            if (data == null || combineSelection.Count >= 3) return;
            int pickedSoFar = 0;
            foreach (var d in combineSelection) if (d == data) pickedSoFar++;
            if (pickedSoFar >= GetOwnedCount(data.id)) return;
            combineSelection.Add(data);
        }

        void RebuildLobbyOwnedList()
        {
            foreach (RectTransform child in lobbyOwnedListContent)
                if (child != lobbyCardTemplate) SafeDestroy(child.gameObject);
            if (characterRoster == null) return;

            var owned = new List<SamgukCharacterData>();
            foreach (var c in characterRoster) if (c != null && IsOwned(c.id)) owned.Add(c);
            // 조합 모드에서는 등급이 섞여 있으면 다른 등급 카드를 실수로 같이 고르기 쉽다
            // (실제로 재현됨 — 로스터 원래 순서엔 등급이 뒤섞여 있어서, 겉보기엔 비슷해
            // 보이는 카드 3장을 골랐는데 그중 하나만 등급이 달라 조합 버튼이 안 켜지는 걸
            // "선택 하나 더 눌러야 켜진다"로 오인하기 쉬웠다). 같은 등급끼리 뭉쳐 보이도록
            // 정렬해서 이 혼동 자체를 없앤다.
            if (combineMode) owned.Sort((a, b) => a.star != b.star ? a.star.CompareTo(b.star) : 0);

            if (owned.Count == 0)
            {
                AddLabel(lobbyOwnedListContent, "보유한 장수가 없습니다. 뽑기를 진행하세요.", 22f, ColorTextSub);
                SetHorizontalContentExtent(lobbyOwnedListContent, lobbyOwnedListArea.sizeDelta.x, 0f);
                return;
            }

            float cardW = lobbyCardTemplate.sizeDelta.x, gap = 14f;
            float total = owned.Count * cardW + Mathf.Max(0, owned.Count - 1) * gap;
            for (int i = 0; i < owned.Count; i++)
            {
                var data = owned[i];
                float x = -total * 0.5f + cardW * 0.5f + i * (cardW + gap);

                var card = Instantiate(lobbyCardTemplate, lobbyOwnedListContent);
                card.name = $"Owned_{data.id}";
                card.anchoredPosition = new Vector2(x, 0f);
                card.gameObject.SetActive(true);

                int pickedCount = 0;
                if (combineMode) foreach (var d in combineSelection) if (d == data) pickedCount++;
                bool selected = combineMode ? pickedCount > 0 : selectedLobbyCharacter == data;
                card.GetComponent<Image>().color = selected ? new Color(ColorAccent.r, ColorAccent.g, ColorAccent.b, 0.85f) : new Color(0.18f, 0.17f, 0.14f);
                ApplyPortraitSwatch(card.Find("Swatch").GetComponent<Image>(), data.icon, data.tintColor);
                card.Find("Name/Label").GetComponent<TMP_Text>().text = combineMode && pickedCount > 0 ? $"{data.displayName} (선택{pickedCount})" : data.displayName;
                card.Find("Star/Label").GetComponent<TMP_Text>().text = new string('★', Mathf.Clamp(data.star, 1, 6));
                card.Find("Count/Label").GetComponent<TMP_Text>().text = $"×{GetOwnedCount(data.id)}";

                var btn = card.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                var captured = data;
                btn.onClick.AddListener(() =>
                {
                    if (combineMode) AddCombinePick(captured);
                    else selectedLobbyCharacter = captured;
                    RefreshLobbyUI();
                });
            }
            SetHorizontalContentExtent(lobbyOwnedListContent, lobbyOwnedListArea.sizeDelta.x, total);
        }

        /// <summary>조합 모드일 때 상세 영역을 대신 채운다 — 등급이 같은 아무 3마리(위 보유
        /// 목록에서 카드를 최대 3번 클릭해 고른 것)를 조합해 상위 등급 캐릭터 1명을 랜덤으로
        /// 얻는다. 같은 캐릭터를 3번 고르면 예전 방식과 동일하게 동작한다(2026-09-16 개정 —
        /// "같은 장수 3마리"만 되던 걸 "등급만 같으면 아무 3마리"로 넓혔다).</summary>
        void RebuildCombinePanel()
        {
            foreach (RectTransform child in lobbyDetailArea)
                if (child != lobbyUpgradeRowTemplate) SafeDestroy(child.gameObject);

            var header = MakeRect(lobbyDetailArea, "Header", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, 28f));
            AddLabel(header, "장수 조합 — 위 목록에서 등급이 같은 장수 3마리를 클릭해 고르세요 (같은 장수를 여러 번 골라도 됩니다)", 21f, ColorAccent, TextAlignmentOptions.Left, autoSize: true);

            var listRt = MakeRect(lobbyDetailArea, "PickList", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -36f), new Vector2(0f, 30f));
            string picksText = combineSelection.Count == 0
                ? "선택된 장수 없음"
                : string.Join("  +  ", combineSelection.ConvertAll(d => $"{d.displayName}({new string('★', Mathf.Clamp(d.star, 1, 6))})"));
            AddLabel(listRt, $"선택 {combineSelection.Count}/3 : {picksText}", 19f, ColorTextSub, TextAlignmentOptions.Left);

            bool canCombine = CanCombineCharacters(combineSelection, out int cost, out string reason);

            var actionRt = MakeRect(lobbyDetailArea, "CombineAction", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -78f), new Vector2(300f, 50f));
            var actionImg = AddImage(actionRt, canCombine ? ColorButton : ColorButtonDisabled, true);
            var actionBtn = GetOrAddButton(actionRt, actionImg);
            actionBtn.interactable = canCombine;
            actionBtn.onClick.RemoveAllListeners();
            actionBtn.onClick.AddListener(() =>
            {
                var picks = new List<SamgukCharacterData>(combineSelection);
                if (TryCombineCharacters(picks, out var result))
                {
                    lobbyGachaResultText.text = $"조합 완료 → {new string('★', Mathf.Clamp(result.star, 1, 6))} {result.displayName}!";
                    combineSelection.Clear();
                    selectedLobbyCharacter = result;
                }
                RefreshLobbyUI();
            });
            AddLabel(actionRt, combineSelection.Count == 3 ? $"조합하기 ({cost}G)" : "조합하기", 20f, Color.white);

            var clearRt = MakeRect(lobbyDetailArea, "CombineClear", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(320f, -78f), new Vector2(170f, 50f));
            var clearImg = AddImage(clearRt, new Color(0.3f, 0.28f, 0.24f), true);
            var clearBtn = GetOrAddButton(clearRt, clearImg);
            clearBtn.onClick.RemoveAllListeners();
            clearBtn.onClick.AddListener(() => { combineSelection.Clear(); RefreshLobbyUI(); });
            AddLabel(clearRt, "선택 초기화", 18f, Color.white);

            if (!canCombine && combineSelection.Count > 0)
            {
                var reasonRt = MakeRect(lobbyDetailArea, "CombineReason", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -142f), new Vector2(0f, 26f));
                AddLabel(reasonRt, reason, 17f, new Color(0.9f, 0.4f, 0.35f), TextAlignmentOptions.Left);
            }
        }

        void RebuildLobbyDetail()
        {
            foreach (RectTransform child in lobbyDetailArea)
                if (child != lobbyUpgradeRowTemplate) SafeDestroy(child.gameObject);
            var data = selectedLobbyCharacter;
            if (data == null)
            {
                AddLabel(lobbyDetailArea, "장수를 선택하세요", 22f, ColorTextSub);
                return;
            }

            // 로비는 장비를 절대 안 다룬다(Run Data라 여기 존재하면 안 됨) — 기본+영구강화 스탯만.
            var eff = ComputeEffectiveStats(data, null);

            var header = MakeRect(lobbyDetailArea, "Header", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, 28f));
            AddLabel(header, $"{data.displayName}  {new string('★', Mathf.Clamp(data.star, 1, 6))}", 26f, ColorAccent, TextAlignmentOptions.Left, autoSize: true);

            var statsRt = MakeRect(lobbyDetailArea, "Stats", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(0f, 24f));
            AddLabel(statsRt, $"HP {eff.hp:0}  공격 {eff.atk:0}  방어 {eff.def:0}  공속 {eff.aspd:0.00}  이속 {eff.mspd:0.00}  사거리 {eff.range:0.0}", 17f, ColorTextSub, TextAlignmentOptions.Left);

            // 조합(승급)은 이제 별도 "조합 모드"(RebuildCombinePanel)에서 여러 캐릭터를 골라
            // 진행한다 — 같은 캐릭터 3마리로 제한하던 예전 인라인 버튼은 폐기(2026-09-16 개정).

            var lv = GetUpgradeLevels(data.id);
            float rowH = lobbyUpgradeRowTemplate.sizeDelta.y + 4f;
            float y = -58f;
            for (int i = 0; i < 6; i++)
            {
                int cost = UpgradeStatCost(data.id, i);

                var rowRt = Instantiate(lobbyUpgradeRowTemplate, lobbyDetailArea);
                rowRt.name = $"UpRow{i}";
                rowRt.anchoredPosition = new Vector2(0f, y);
                rowRt.gameObject.SetActive(true);

                rowRt.Find("Label/Label").GetComponent<TMP_Text>().text = $"{UpgradeStatNames[i]} Lv.{lv[i]}";

                var btnRt = rowRt.Find("Btn");
                bool affordable = balance != null && persistentGold >= cost;
                btnRt.GetComponent<Image>().color = affordable ? ColorButton : ColorButtonDisabled;
                var btn = btnRt.GetComponent<Button>();
                btn.interactable = affordable;
                btn.onClick.RemoveAllListeners();
                var capturedData = data;
                int capturedIndex = i;
                btn.onClick.AddListener(() => { if (TryUpgradeStat(capturedData.id, capturedIndex)) RefreshLobbyUI(); });
                btnRt.Find("Label").GetComponent<TMP_Text>().text = $"강화 ({cost}G)";

                y -= rowH;
            }
        }
    }
}

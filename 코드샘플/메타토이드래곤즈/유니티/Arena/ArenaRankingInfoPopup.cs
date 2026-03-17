using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SandboxNetwork;
using Newtonsoft.Json.Linq;
using DG.Tweening;

public class ArenaRankingInfoPopup : Popup<PopupData>
{
    const int MASTER_SEASON_GRADE = 17;//마스터 그레이드 등급 조건 값

    [Header("Ranking Info")]
    [SerializeField]
    private Text userRankingName = null;
    [SerializeField]
    private RectTransform RankingNameBoxRect = null;
    [SerializeField]
    private Image userRankingIcon = null;
    [SerializeField]
    private Image userNextRankIcon = null;
    [SerializeField]
    private Slider userArenaPointProgressBar = null;
    [SerializeField]
    private Text userArenaPointLabel = null;
    [SerializeField]
    private GameObject rankRewardBubbleObj = null;
    [SerializeField]
    private Text rankRewardCntText = null;
    [SerializeField]
    private Image rankRewardImg = null;

    [SerializeField]
    private GameObject[] rankEffectObj = null;

    [SerializeField] private GameObject arenaPreseason = null;
    [SerializeField] private GameObject arenaRegularseason = null;





    [SerializeField]
    private GameObject rankRewardBtn  = null;

    // Start is called before the first frame update


    // Update is called once per frame

    public void Init()
    {
        SetRankRewardBtn();
        RefreshData();
    }

    public void RefreshData()
    {
        int currentUserPoint = ArenaManager.Instance.UserArenaData.season_point;
        int seasonHighPoint = ArenaManager.Instance.UserArenaData.season_high_point;
        int currentUserGrade = (int)ArenaManager.Instance.UserArenaData.SeasonGrade;
        
        ArenaRankData currentRankData = ArenaRankData.GetFirstInGroup(currentUserGrade);
        if (currentRankData == null) 
            return;

        int nextNeedPoint = -1;
        ArenaRankData nextRankData = ArenaRankData.GetFirstInGroup(currentUserGrade + 1);
        if (nextRankData != null)            
            nextNeedPoint = nextRankData.NEED_POINT;

        bool isNextValue = nextNeedPoint > 0;
        string iconName = currentRankData.ICON;
        string timerNameIndex = currentRankData._NAME;
        int currentNeedPoint = currentRankData.NEED_POINT;


        if (nextNeedPoint <= seasonHighPoint)
            rankRewardBubbleObj.SetActive(false);
        else
        {
            int groupKey = /*User.Instance.IS_HOLDER ? ArenaRankData.GetCurrentRankData(nextNeedPoint + 1).HOLDER_FIRST_REWARD_GROUP :*/ ArenaRankData.GetCurrentRankData(nextNeedPoint + 1).FIRST_REWARD_GROUP;
            var items = ItemGroupData.Get(groupKey);
            if (items == null)
                rankRewardBubbleObj.SetActive(false);
            else
            {
                rankRewardBubbleObj.SetActive(true);
                foreach (var item in items)
                {
                    rankRewardImg.sprite = item.Reward.ICON;
                    rankRewardCntText.text = item.Reward.Amount.ToString();
                    break;
                }
            }
        }


        if (userRankingIcon !=null)
        {
            userRankingIcon.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ArenaRankPath, iconName);
            if (userRankingIcon.sprite == null)
                userRankingIcon.gameObject.SetActive(false);

            foreach ( var rankEffect in rankEffectObj)
                rankEffect.SetActive(false);

            switch (iconName)  // 완전 임시 코드 - 기획도 없음, 색상에 맞춰서 그냥 넣어라고 전달 받음
            {
                case "icon_rank_1":  // 아이언
                    break;
                case "icon_rank_2":   // 브론즈
                case "icon_rank_3":   // 실버
                    rankEffectObj[0].SetActive(true);
                    break;
                case "icon_rank_4":   // 골드
                    rankEffectObj[1].SetActive(true);
                    break;
                case "icon_rank_5":   // 플레
                case "icon_rank_6":   // 다이아
                    rankEffectObj[2].SetActive(true);
                    break;
                case "icon_rank_7":   // 마스터
                    rankEffectObj[3].SetActive(true);
                    break;
                default:  // 그마, 챌린저, 챔피온
                    break;
            }
        }
        if (userNextRankIcon != null)
        {
            if (!isNextValue)
            {
                userNextRankIcon.gameObject.SetActive(false);
                rankRewardBubbleObj.gameObject.SetActive(false);
            }
            else
            {
                userNextRankIcon.gameObject.SetActive(true);
                userNextRankIcon.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ArenaRankPath, ArenaRankData.GetCurrentRankData(nextNeedPoint).ICON);
                if (userNextRankIcon.sprite == null)
                    userNextRankIcon.gameObject.SetActive(false);
            }
        }
        
        if(userArenaPointProgressBar != null && userArenaPointLabel != null)
        {
            if(currentUserGrade >= MASTER_SEASON_GRADE)//마스터 이후부터 게이지 풀로 채움
            {
                userArenaPointProgressBar.maxValue = 1;
                userArenaPointProgressBar.value = 1;

                userArenaPointLabel.text = currentUserPoint.ToString();
            }
            else
            {
                userArenaPointProgressBar.maxValue = isNextValue ? nextNeedPoint - currentNeedPoint : 1;
                userArenaPointProgressBar.value = isNextValue ? currentUserPoint - currentNeedPoint : 1;

                userArenaPointLabel.text = isNextValue ? string.Format("{0} / {1}", currentUserPoint, nextNeedPoint) : currentUserPoint.ToString();
            }
        }

        if (userRankingName != null)
            userRankingName.text = StringData.GetStringByStrKey(timerNameIndex);

        LayoutRebuilder.MarkLayoutForRebuild(userRankingName.GetComponent<RectTransform>());
        LayoutRebuilder.MarkLayoutForRebuild(RankingNameBoxRect);

        if (arenaPreseason != null)
        {
            arenaPreseason.SetActive(ArenaManager.Instance.UserArenaData.season_type == ArenaBaseData.SeasonType.PreSeason);
        }
        if (arenaRegularseason != null)
        {
            arenaRegularseason.SetActive(ArenaManager.Instance.UserArenaData.season_type == ArenaBaseData.SeasonType.RegularSeason);
        }
    }

    public override void InitUI()
    {

    }

    void SetRankRewardBtn()
    {
        string currentRankKey = ArenaManager.Instance.UserArenaData?.first_reward.ToString();

        if (string.IsNullOrEmpty(currentRankKey))
        {
            rankRewardBtn.SetActive(false);
            return;
        }

        var arenaTableData = ArenaRankData.Get(currentRankKey);
        var currentRankingTableData = ArenaRankData.GetCurrentRankData(ArenaManager.Instance.UserArenaData.season_high_point);

        if(arenaTableData == null || currentRankingTableData == null)
        {
            rankRewardBtn.SetActive(false);
            return;
        }

        if (arenaTableData.GROUP < currentRankingTableData.GROUP)
        {
            rankRewardBtn.SetActive(true);
            rankRewardBtn.transform.DOScale(Vector3.one * 1.2f, 0.5f).SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            rankRewardBtn.SetActive(false);
        }
    }

    public void OnClickRankRewardBtn()
    {
        int seasonHigh = ArenaManager.Instance.UserArenaData.season_high_point;
        int groupKey = User.Instance.IS_HOLDER ? ArenaRankData.GetCurrentRankData(seasonHigh).HOLDER_FIRST_REWARD_GROUP : ArenaRankData.GetCurrentRankData(seasonHigh).FIRST_REWARD_GROUP;
        List<Asset> AllItems = new List<Asset>();
        foreach(var dat in ItemGroupData.Get(groupKey))
        {
            AllItems.Add(dat.Reward);
        }
        if (User.Instance.CheckInventoryGetItem(AllItems))
        {
            IsFullBagAlert();
            return;
        }
        NetworkManager.Send("arena/firstreward", null, (JObject jsonData) =>
        {
            if (SBFunc.IsJTokenCheck(jsonData["rs"]) == false)
                return;
            var rs = jsonData["rs"].ToObject<eApiResCode>();
            if (rs == eApiResCode.OK)
            {
                var assetList = new List<Asset>();
                var rewardDatas = (JArray)jsonData["reward"];
                for(int i = 0, count= rewardDatas.Count; i < count; ++i)
                {
                    if(SBFunc.IsJArray(rewardDatas[i]))
                    {
                        assetList.AddRange(SBFunc.ConvertSystemRewardDataList((JArray)rewardDatas[i]));
                    }
                }
                SystemRewardPopup.OpenPopup(assetList).SetText(StringData.GetStringByIndex(100001194));
            }
            else if (rs == eApiResCode.ARENA_INVAILD_SEASON)
            {
                Debug.Log("이미 보상을 받았는데 또 받으려고 접근함");
            }


            
        });
        rankRewardBtn.SetActive(false);
        //SystemRewardPopup.OpenPopup(SBFunc.ConvertSystemRewardDataList(  )).SetText(StringData.GetStringByIndex(100001194));



    }

    void IsFullBagAlert()
    {
        SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077), StringData.GetStringByIndex(100000414), "",
            () => {
                //메인팝업 열기
                PopupManager.OpenPopup<InventoryPopup>();
                PopupManager.ClosePopup<PostListPopup>();
            }, () => { }, () => { });
    }
}

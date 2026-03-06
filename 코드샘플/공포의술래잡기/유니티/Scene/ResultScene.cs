using UnityEngine;
using SBSocketSharedLib;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Linq;
using Spine.Unity;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class ResultScene : BaseScene
{
    [SerializeField] Text curGameType;

    [SerializeField] ResultScoreListHolder scoreHolder;
    [SerializeField] PlayerListHolder playerHolder;

    [SerializeField] Text curRankPointText;
    [SerializeField] Image userRankIcon;

    [SerializeField] SelectedCharacter selectedCharacter;
    //[SerializeField] ResultCharacter selectedCharacter;

    [SerializeField] ResultScoreListItem totalScoreItem;
    [SerializeField] ResultScoreListItem totalGoldItem;
    [SerializeField] ResultScoreListItem totalPassPointItem;
    [SerializeField] UIBundleItem rewardItemSample;
    //[SerializeField] ResultScoreListItem totalExpItem;

    [SerializeField] GameObject winUI;
    [SerializeField] SkeletonGraphic skeletonGraphicWIn;

    [SerializeField] GameObject loseUI;
    [SerializeField] SkeletonGraphic skeletonGraphicLose;

    [SerializeField] GameObject escapeUI;
    [SerializeField] SkeletonGraphic skeletonGraphicEscape;

    [SerializeField] GameObject animationSkip;
    [SerializeField] Image bgTile;

    [SerializeField] Text getUpPoint;
    [SerializeField] Text getDownPoint;
    [SerializeField] Text rankText;

    [SerializeField] GameObject detailPopup;

    [SerializeField] GameObject gameRank_Warning;

    [SerializeField] Button ad_Btn;
    [SerializeField] Text ad_text;
    int mySelectedCharacter = 0;
    CharacterLevelGameData preLevelData = null;
    bool animationDone = false;


    int totalPoint = 0;
    int passPoint = 0;
    int gain_gold = 0;
    float gold_rate = 1.0f;
    float clan_gold_rate = 0.0f;
    float buff_gold_rate = 0.0f;
    float buff_item_rate = 0.0f;

    List<SBWeb.ResponseReward> gain_reward = new List<SBWeb.ResponseReward>();
    JObject resultData = null;
    JObject myResultInfos = null;

    Dictionary<long, int> win_UserList = new Dictionary<long, int>();
    public override void Clear()
    {
    }

    private void Start()
    {
        if (resultData == null)
            InitUI();
    }

    public void SetResultData(JObject data)
    {
        resultData = data;
        InitUI();
        OnResultData(resultData);
        InitResultUI();
    }

    public override void StartBackgroundMusic(bool clearPopup = true)
    {
        BGM = Managers.Resource.LoadAssetsBundle<AudioClip>("AssetsBundle/Sounds/bgm/BGM_LOBBY");
        base.StartBackgroundMusic(clearPopup);
    }
    private void InitUI()
    {
        curGameType.text = "";
        // 로드된 이펙트 삭제
        Managers.Effect.OnGameOver();

        if (resultData != null)
        {
            Dictionary<string, int> points = new Dictionary<string, int>();
            foreach (JObject roomPlayer in resultData["result"])
            {
                points[roomPlayer["user_no"].Value<string>()] = roomPlayer["point"].Value<int>();
            }

            foreach (JObject roomPlayer in resultData["users"])
            {
                if (roomPlayer["user_no"].Value<long>() == Managers.UserData.MyUserID)
                {
                    mySelectedCharacter = roomPlayer["char_id"].Value<int>();
                    UserEquipData equip = null;
                    var myChar = Managers.UserData.GetMyCharacterInfo(mySelectedCharacter);
                    if (myChar != null)
                    {
                        equip = myChar.curEquip;
                    }
                    selectedCharacter.SetCharacter(mySelectedCharacter, equip);
                    selectedCharacter.SetResultAnimation(roomPlayer["win"].Value<int>() > 0);
                    playerHolder.AddItem(roomPlayer, points[roomPlayer["user_no"].Value<string>()]);
                }
                else
                {
                    playerHolder.AddItem(roomPlayer, points[roomPlayer["user_no"].Value<string>()]);
                }
            }
        }
        else if (Managers.PlayData.RoomPlayers != null && Managers.PlayData.RoomPlayers.Count > 0)
        {
            foreach (RoomPlayerInfo roomPlayer in Managers.PlayData.RoomPlayers)
            {
                if (roomPlayer.UserId == Managers.UserData.MyUserID)
                {
                    mySelectedCharacter = roomPlayer.SelectedCharacter.CharacterType;
                    UserEquipData equip = null;
                    var myChar = Managers.UserData.GetMyCharacterInfo(mySelectedCharacter);
                    if (myChar != null)
                    {
                        equip = myChar.curEquip;
                    }
                    selectedCharacter.SetCharacter(mySelectedCharacter, equip);
                    selectedCharacter.SetResultAnimation(Managers.PlayData.GameResult == PlayDataManager.GAME_RESULT.WIN);
                    playerHolder.AddItem(roomPlayer);
                }
                else
                {
                    playerHolder.AddItem(roomPlayer);
                }
            }
        }

        var charInfo = Managers.UserData.GetMyCharacterInfo(mySelectedCharacter);
        curRankType = GetRankType(Managers.UserData.MyPoint);

        preLevelData = charInfo.characterData.levelData[charInfo.lv];

        userRankIcon.sprite = Managers.UserData.MyRank.rank_resource;
        getUpPoint.text = "";
        getUpPoint.gameObject.SetActive(false);
        getDownPoint.text = "";
        getDownPoint.gameObject.SetActive(false);
        rankText.text = Managers.UserData.MyRank.GetName();

        SetRankPointText(Managers.UserData.MyPoint);

        winUI.SetActive(false);
        loseUI.SetActive(false);
        escapeUI.SetActive(false);

        totalScoreItem.gameObject.SetActive(false);
        totalGoldItem.gameObject.SetActive(false);
        totalPassPointItem.gameObject.SetActive(false);
        //totalExpItem.gameObject.SetActive(false);
        rewardItemSample.gameObject.SetActive(false);
        foreach (Transform child in rewardItemSample.transform.parent)
        {
            if (child == rewardItemSample.transform)
                continue;

            Destroy(child.gameObject);
        }
        scoreHolder.Clear();
        OnShowDetailPopup(false);
        ShowResultEffect();

        gameRank_Warning.SetActive(Managers.PlayData.GameRank_Warning);
    }

    public void OnShowDetailPopup(bool isShow)
    {
        detailPopup.SetActive(isShow);
    }

    public void OnShowGameInfo()
    {
        scoreHolder.Clear();

        if (myResultInfos != null)
        {
            IList<string> keys = myResultInfos.Properties().Select(p => p.Name).ToList();
            for (int j = 0; j < keys.Count; j++)
            {
                string k = keys[j];
                var v = myResultInfos[k];

                var key = int.Parse(k);
                var value = v.Value<int>();

                var data = Managers.Data.GetData(GameDataManager.DATA_TYPE.reward_point, key) as RewardGameData;
                scoreHolder.AddItem(StringManager.Instance.GetDesc(GameDataManager.DATA_TYPE.reward_point, key), value, data.Point);
            }
        }

        OnShowDetailPopup(true);
    }

    public void OnShowGameReward()
    {
        scoreHolder.Clear();

        float hottime = (gold_rate - GameConfig.Instance.REWARD_GOLD_RATIO) * 10.0f;
        int default_gold = (int)(totalPoint * GameConfig.Instance.REWARD_GOLD_RATIO);
        int plusGold = gain_gold - default_gold;

        int hottime_gold = (int)(default_gold * hottime);

        int bonus_gold = plusGold - hottime_gold;

        float total_rate = (clan_gold_rate + buff_gold_rate);
        float clan_rate = clan_gold_rate / total_rate;
        float buff_rate = buff_gold_rate / total_rate;

        int clan_gold = (int)(bonus_gold * clan_rate);
        int buff_gold = (int)(bonus_gold * buff_rate);

        if (bonus_gold != (clan_gold + buff_gold))
        {
            buff_gold += bonus_gold - (clan_gold + buff_gold);
        }

        scoreHolder.AddItem(StringManager.GetString("결과_기본획득골드"), 1, default_gold);
        if (hottime > 0.0f)
            scoreHolder.AddItem(StringManager.GetString("결과_핫타임획득골드"), 1, hottime_gold);

        if (clan_gold_rate > 0.0f)
            scoreHolder.AddItem(StringManager.GetString("결과_클랜획득골드"), 1, clan_gold);
        if (buff_gold_rate > 0.0f)
            scoreHolder.AddItem(StringManager.GetString("결과_버프획득골드"), 1, buff_gold);

        OnShowDetailPopup(true);
    }

    void ShowResultEffect()
    {
        GameObject effectTarget = SetResultUI();

        if (effectTarget != null)
        {
            effectTarget.transform.localScale = Vector3.one;// * 3.0f;
            effectTarget.transform.DOScale(Vector3.one, 0f).SetEase(Ease.InOutBack);
        }

        InitResultUI();
    }

    GameObject SetResultUI()
    {
        GameObject effectTarget = null;
        SkeletonGraphic graphic = null;

        if (bgTile != null)
            bgTile.color = Color.white;

        if (resultData != null)
        {
            foreach (JObject roomPlayer in resultData["users"])
            {
                if (roomPlayer["user_no"].Value<long>() == Managers.UserData.MyUserID)
                {
                    if (roomPlayer["win"].Value<int>() > 0)
                    {
                        effectTarget = winUI;
                        ColorUtility.TryParseHtmlString("#EEA0FF", out Color color);
                        if (bgTile != null)
                            bgTile.color = color;
                        graphic = skeletonGraphicWIn;
                    }
                    else
                    {
                        effectTarget = loseUI;
                        graphic = skeletonGraphicLose;
                    }
                }
            }
        }
        else
        {
            switch (Managers.PlayData.GameResult)
            {
                case PlayDataManager.GAME_RESULT.UNKNOWN:
                    effectTarget = loseUI;
                    graphic = skeletonGraphicLose;
                    break;
                case PlayDataManager.GAME_RESULT.WIN:
                    effectTarget = winUI;
                    ColorUtility.TryParseHtmlString("#EEA0FF", out Color color);
                    if (bgTile != null)
                        bgTile.color = color;
                    graphic = skeletonGraphicWIn;
                    break;
                case PlayDataManager.GAME_RESULT.LOSE:
                    effectTarget = loseUI;
                    graphic = skeletonGraphicLose;
                    break;
                case PlayDataManager.GAME_RESULT.ESCAPED:
                    //effectTarget = escapeUI;
                    //graphic = skeletonGraphicLose;
                    effectTarget = loseUI;
                    graphic = skeletonGraphicLose;
                    break;
            }
        }

        if (effectTarget != null)
        {
            effectTarget.SetActive(true);
            SetUIGraphic(graphic);
            return effectTarget;
        }

        return null;
    }



    RankType GetRankType(int point)
    {
        return RankType.GetRankFromPoint(point);
    }

    //rank data;
    float targetValue = 0;
    ////
    float curValue = 0;

    float playValue = 0;

    RankType curRankType = null;

    private void InitResultUI()
    {
        bool isChaser = Managers.PlayData.AmIChaser();
        UserEquipData equip = null;
        var myChar = Managers.UserData.GetMyCharacterInfo(mySelectedCharacter);
        if (myChar != null)
        {
            equip = myChar.curEquip;
        }
        selectedCharacter.SetCharacter(mySelectedCharacter, equip);
        selectedCharacter.SetResultAnimation(Managers.PlayData.GameResult == PlayDataManager.GAME_RESULT.WIN || Managers.PlayData.GameResult == PlayDataManager.GAME_RESULT.ESCAPED);

        scoreHolder.Clear();

        totalScoreItem.gameObject.SetActive(true);
        totalGoldItem.gameObject.SetActive(true);
        totalPassPointItem.gameObject.SetActive(true);
        //totalExpItem.gameObject.SetActive(true);

        totalScoreItem.Initialize(StringManager.GetString("ui_get_point"), 1, totalPoint);
        totalGoldItem.Initialize(StringManager.GetString("ui_get_gold"), 1, gain_gold);
        totalPassPointItem.Initialize(StringManager.GetString("ui_get_passpoint"), 1, passPoint);
        //totalExpItem.Initialize("획득 경험치", 1, exp);

        foreach (Transform child in rewardItemSample.transform.parent)
        {
            if (child == rewardItemSample.transform)
                continue;

            Destroy(child.gameObject);
        }

        curValue = Managers.UserData.MyPoint;
        if (curValue < 0) curValue = 0;
        curRankType = GetRankType((int)curValue);

        if (resultData == null)
        {
            SBWeb.GetGameResult(Managers.PlayData.GameRoomInfo.RoomInfo.RoomId, (gameResults) =>
            {
                if (gameResults.Count == 0) return;

                var gameResultData = JObject.Parse(gameResults[0]["data"].Value<string>());
                SBDebug.Log(gameResultData);

                OnResultData(gameResultData);
            });
        }
        else
        {
            OnResultData(resultData);
        }


        //결과창 광보 버튼 UI
        ad_Btn.gameObject.SetActive(GameConfig.Instance.AD_REWARD_USE == 1);
        ad_text.text = StringManager.GetString("ui_result_ad_btn", GameConfig.Instance.AD_REWARD_MULTIPLE + 1);
    }

    public void OnResultData(JObject gameResultData)
    {
        bool bGameDone = resultData == null;

        if (gameResultData.ContainsKey("game_type"))
        {
            switch (gameResultData["game_type"].Value<int>())
            {
                case 1:
                    curGameType.text = StringManager.GetString("ui_rank_match");
                    if (bGameDone)
                    {
                        int playTime = Managers.UserData.RankPlayCount + 1;
                        Managers.UserData.RankPlayCount = playTime;

                        com.adjust.sdk.AdjustEvent adjustEvent = null;
                        switch (playTime)
                        {
                            case 1:
                                if (Managers.UserData.CustomPlayCount == 0)
                                {
                                    com.adjust.sdk.AdjustEvent ae = new com.adjust.sdk.AdjustEvent("e6j4a9");
                                    com.adjust.sdk.Adjust.trackEvent(ae);
                                }

                                adjustEvent = new com.adjust.sdk.AdjustEvent("topvqh");
                                break;
                            case 10:
                                adjustEvent = new com.adjust.sdk.AdjustEvent("hzsr7v"); break;
                            case 50:
                                adjustEvent = new com.adjust.sdk.AdjustEvent("cvc9s5"); break;
                            case 70:
                                adjustEvent = new com.adjust.sdk.AdjustEvent("10u5n4"); break;
                            case 100:
                                adjustEvent = new com.adjust.sdk.AdjustEvent("8aw59u"); break;
                        }

                        if (adjustEvent != null)
                        {
                            com.adjust.sdk.Adjust.trackEvent(adjustEvent);
                        }
                    }
                    break;
                case 2:
                    curGameType.text = StringManager.GetString("ui_normal_match");
                    if (bGameDone)
                    {
                        int playTime = Managers.UserData.CustomPlayCount + 1;
                        Managers.UserData.CustomPlayCount = playTime;
                    }
                    break;
                case 3:
                    curGameType.text = StringManager.GetString("ui_train_match");
                    break;
            }
        }

        var users = gameResultData["users"];
        SBDebug.Log(users);

        bool pointShown = false;
        var playerList = playerHolder.GetItems();
        JArray userArray = (JArray)gameResultData["users"];
        win_UserList.Clear();
        if (userArray.Count != 0)
        {
            for (int i = 0; i < userArray.Count; ++i)
            {
                var user = userArray[i];
                SBDebug.Log($"user {user}");

                long user_id = user["user_no"].Value<long>();
                var dodge = user["dodge"].Value<int>();

                var point = user["point"].Value<int>();
                int passpoint = 0;
                if (user["pass_point"] != null)
                    passpoint = user["pass_point"].Value<int>();

                var gold = user["gold"].Value<int>();
                var chaser = user["chaser"].Value<int>();
                var win = user["win"].Value<int>();

                if (win == 1 && !win_UserList.ContainsKey(user_id))
                    win_UserList.Add(user_id, point);

                SBDebug.Log($"id {user_id}, dodge {dodge}, point {point}, chaser {chaser}, win {win}");

                JObject resultInfos = null;
                if (user["resultInfos"].Type == JTokenType.Object)
                    resultInfos = (JObject)user["resultInfos"];

                var playerItem = playerHolder.GetItem(user_id.ToString());

                if (user_id == Managers.UserData.MyUserID)
                {
                    if (dodge > 0)
                    {
                        //SBWeb.CheckDodge(() => {
                        //    PopupCanvas.Instance.ShowMessagePopup("닷지안내");
                        //});
                    }
                    else
                    {
                        totalPoint = point;
                        gain_gold = gold;
                        passPoint = passpoint;

                        try
                        {
                            gold_rate = user["gold_rate"].Value<float>();
                            clan_gold_rate = user["clan_gold_rate"].Value<float>();
                            buff_gold_rate = user["buff_gold_rate"].Value<float>();
                            buff_item_rate = user["buff_item_rate"].Value<float>();
                        }
                        catch
                        {
                            gold_rate = GameConfig.Instance.REWARD_GOLD_RATIO;
                            clan_gold_rate = 0.0f;
                            buff_gold_rate = 0.0f;
                            buff_item_rate = 0.0f;
                        }

                        gain_reward.Clear();

                        if (user["items"] != null)
                        {
                            foreach (JToken reward in user["items"])
                            {
                                if (reward.Type == JTokenType.Array)
                                {
                                    foreach (JToken rewardData in reward)
                                    {
                                        bool newReward = true;
                                        foreach (SBWeb.ResponseReward gainReward in gain_reward)
                                        {
                                            if ((int)gainReward.Type == rewardData[0].Value<int>() && (int)gainReward.Id == rewardData[1].Value<int>())
                                            {
                                                gainReward.AddAmount(rewardData[2].Value<int>());
                                                newReward = false;
                                            }
                                        }
                                        if (newReward)
                                            gain_reward.Add(new SBWeb.ResponseReward(rewardData[0].Value<int>(), rewardData[1].Value<int>(), rewardData[2].Value<int>()));
                                    }
                                }
                                else
                                {
                                    bool newReward = true;
                                    foreach (SBWeb.ResponseReward gainReward in gain_reward)
                                    {
                                        if ((int)gainReward.Type == reward[0].Value<int>() && (int)gainReward.Id == reward[1].Value<int>())
                                        {
                                            gainReward.AddAmount(reward[2].Value<int>());
                                            newReward = false;
                                        }
                                    }
                                    if (newReward)
                                        gain_reward.Add(new SBWeb.ResponseReward(reward[0].Value<int>(), reward[1].Value<int>(), reward[2].Value<int>()));
                                }
                            }
                        }
                    }

                    if (resultInfos != null)
                    {
                        IList<string> keys = resultInfos.Properties().Select(p => p.Name).ToList();
                        for (int j = 0; j < keys.Count; j++)
                        {
                            string k = keys[j];
                            var v = resultInfos[k];

                            var key = int.Parse(k);
                            var value = v.Value<int>();

                            var data = Managers.Data.GetData(GameDataManager.DATA_TYPE.reward_point, key) as RewardGameData;
                            scoreHolder.AddItem(StringManager.Instance.GetDesc(GameDataManager.DATA_TYPE.reward_point, key), value, data.Point);
                        }

                        if (playerItem != null)
                            playerItem.SetPointText(point.ToString());
                        playerHolder.SetPoint(user_id, point);

                        myResultInfos = resultInfos;
                    }
                }

                if (playerItem == null)
                {
                    continue;
                }
                else
                {
                    playerItem.OnExitUser(false);
                    playerItem.OnDuoUser(false);
                    if (gameResultData.ContainsKey("result") && gameResultData["result"].Type == JTokenType.Array)
                    {
                        foreach (JObject us in gameResultData["result"])
                        {
                            if (us["user_no"].Value<long>() == user_id)
                            {
                                playerItem.SetRankPoint(us["point"].Value<int>());


                                if (user_id == Managers.UserData.MyUserID)
                                {
                                    pointShown = true;
                                    StartCoroutine(PlaySliderBar(us["point"].Value<int>() - us["win_point"].Value<int>(), us["point"].Value<int>(), 1, 1));
                                }
                            }
                        }
                    }
                }

                if (dodge > 0)
                {
                    playerItem.SetPointText(StringManager.GetString("ui_game_leave"));
                    playerHolder.SetPoint(user_id, -1);
                }
                else
                {
                    playerItem.SetPointText(point.ToString());
                    playerHolder.SetPoint(user_id, point);

                    if (resultInfos != null)
                    {
                        IList<string> keys = resultInfos.Properties().Select(p => p.Name).ToList();
                        if (resultInfos.ContainsKey("17"))
                            playerItem.OnExitUser(resultInfos["17"].Value<int>() > 0);
                    }
                }

                JObject userData = (JObject)user;
                int duoType = 0;
                if (userData != null && userData.ContainsKey("duotype"))
                {
                    duoType = userData["duotype"].Value<int>();
                }

                playerItem.OnDuoUser(duoType > 0);
            }
        }


        ////mvp 처리

        if (gameResultData.ContainsKey("mvp"))
        {
            long mvp_user = gameResultData["mvp"].Value<long>();
            var playerItem = playerHolder.GetItem(mvp_user.ToString());
            if (playerItem != null)
                playerItem.PlayMvpParticle();
        }

        //if (win_UserList.Count > 0)
        //{
        //    int maxPoint = 0;
        //    long user_id = 0;
        //    foreach (var item in win_UserList)
        //    {
        //        if (maxPoint < item.Value)
        //        {
        //            user_id = item.Key;
        //            maxPoint = item.Value;
        //        }
        //    }
        //    if (maxPoint != 0)
        //        playerHolder.GetItem(user_id.ToString()).PlayMvpParticle();
        //}


        playerHolder.SortWithPoint();
        int curCharExp = Managers.UserData.GetMyCharacterInfo(mySelectedCharacter).exp;
        int curCharLevel = Managers.UserData.GetMyCharacterInfo(mySelectedCharacter).lv;

        targetValue = Managers.UserData.MyPoint;
        if (targetValue < 0)
            targetValue = 0;

        if (!pointShown)
        {
            StartCoroutine(PlaySliderBar((int)curValue, (int)targetValue, 1, 1));
        }
        rewardItemSample.gameObject.SetActive(true);
        foreach (SBWeb.ResponseReward reward in gain_reward)
        {
            GameObject multiItemRow = Instantiate(rewardItemSample.gameObject);
            multiItemRow.transform.SetParent(rewardItemSample.transform.parent);
            multiItemRow.transform.localPosition = Vector3.zero;
            multiItemRow.transform.localScale = Vector3.one;

            UIBundleItem item = multiItemRow.GetComponent<UIBundleItem>();
            item.SetReward(reward);
        }
        rewardItemSample.gameObject.SetActive(false);

        OnExpAnimationDone();
    }

    string GetMaxValue(int value)
    {
        if ((int)value >= 1000000)
            return "-";
        return value.ToString();
    }

    void SetRankPointText(int point)
    {
        curRankPointText.text = new StringBuilder().AppendFormat("{0}", point).ToString();
    }

    void SetMyRankSprite(int point)
    {
        var rt = GetRankType(point);
        userRankIcon.sprite = rt.rank_resource;
        rankText.text = rt.GetName();
    }

    IEnumerator PlaySliderBar(int start, int target, int end, float time)
    {
        float addValue = (target - start);
        float playValue = start;

        Text warning_text = null;
        if (Managers.PlayData.GameRank_Warning)
        {
            ColorUtility.TryParseHtmlString("#77FF3B", out Color color);
            getUpPoint.color = color;
            warning_text = gameRank_Warning.GetComponentInChildren<Text>();
        }
        else
        {
            ColorUtility.TryParseHtmlString("#00FFEF", out Color color);
            getUpPoint.color = color;
        }

        if (addValue < 0)
        {
            if (warning_text)
            {
                warning_text.text = StringManager.GetString("ui_point_discount");
            }

            getDownPoint.gameObject.SetActive(true);
            (getDownPoint.transform as RectTransform).DOAnchorPosY((getDownPoint.transform as RectTransform).anchoredPosition.y + 60f, 1f).OnComplete(() =>
            {
                (getDownPoint.transform as RectTransform).DORewind();
            });
            getDownPoint.text = addValue.ToString();

            yield return new WaitForSeconds(0.5f);
        }
        else if (addValue > 0)
        {
            if (warning_text)
            {
                warning_text.text = StringManager.GetString("ui_bonus_point");
            }

            getUpPoint.gameObject.SetActive(true);
            (getUpPoint.transform as RectTransform).DOAnchorPosY((getUpPoint.transform as RectTransform).anchoredPosition.y + 60f, 1f).OnComplete(() =>
            {
                (getUpPoint.transform as RectTransform).DORewind();
            });
            getUpPoint.text = new StringBuilder().AppendFormat("+{0}", addValue).ToString();

            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            if (warning_text)
            {
                warning_text.text = StringManager.GetString("ui_point_discount");
            }

            getUpPoint.text = new StringBuilder().AppendFormat("+{0}", addValue).ToString();
        }

        curRankPointText.text = start.ToString();
        SetRankPointText(start);

        while (true)
        {
            if (addValue > 0)
            {
                if (playValue >= target)
                    break;
            }
            else
            {
                if (playValue <= target)
                    break;
            }

            playValue += addValue * Time.deltaTime;
            SetRankPointText((int)playValue);
            SetMyRankSprite((int)playValue);
            yield return null;
        }

        SetRankPointText((int)target);
        SetMyRankSprite((int)target);

        if (Managers.UserData.IsRankChanged())
        {
            var value1 = CacheUserData.GetInt("saved_rank", Managers.UserData.MyRank.GetID());
            var value2 = Managers.UserData.MyRank.GetID();
            PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.ADVANCEMENT_POPUP);
            bool check = value1 < value2;
            var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.ADVANCEMENT_POPUP) as AdvancementPopup;
            popup.Init(check);

        }
    }

    public void OnExpAnimationDone()
    {
        if (Managers.UserData.GameResult == null)
            return;
        UserEquipData equip = null;
        var myChar = Managers.UserData.GetMyCharacterInfo(mySelectedCharacter);
        if (myChar != null)
        {
            equip = myChar.curEquip;
        }
        selectedCharacter.SetCharacter(mySelectedCharacter, equip);
        selectedCharacter.SetResultAnimation(Managers.PlayData.GameResult == PlayDataManager.GAME_RESULT.WIN);

        totalScoreItem.gameObject.SetActive(true);
        totalGoldItem.gameObject.SetActive(true);
        totalPassPointItem.gameObject.SetActive(true);
        //totalExpItem.gameObject.SetActive(true);

        totalScoreItem.Initialize(StringManager.GetString("ui_get_point"), 1, totalPoint);
        totalGoldItem.Initialize(StringManager.GetString("ui_get_gold"), 1, gain_gold);
        totalPassPointItem.Initialize(StringManager.GetString("ui_get_passpoint"), 1, passPoint);

        //SetResultUI();

        int curCharLevel = Managers.UserData.GetMyCharacterInfo(mySelectedCharacter).lv;
        //DOTween.Kill(prevPlayerExpSlider);

        var expPoint = Managers.UserData.MyPoint - Managers.UserData.MyRank.start_point;
        var expMaxValue = Managers.UserData.MyRank.end_point - Managers.UserData.MyRank.start_point;

        var charInfo = Managers.UserData.GetMyCharacterInfo(mySelectedCharacter);
        int prevLevelExp = 0;
        if (charInfo.lv > 2 && charInfo.characterData.levelData[charInfo.lv - 1] != null)
        {
            prevLevelExp = charInfo.characterData.levelData[charInfo.lv - 1].need_exp;
        }

        animationDone = true;

        animationSkip.SetActive(false);
    }

    public void OnExitResult()
    {
        if (!animationDone && Managers.UserData.GameResult != null)
        {
            OnExpAnimationDone();
            return;
        }

        MoveLobby();
    }

    void MoveLobby()
    {
        Managers.Sound.Clear();
        Managers.Scene.LoadScene(SceneType.Lobby);
    }

    public void OnEnterGame()
    {
        if (!animationDone && Managers.UserData.GameResult != null)
        {
            OnExpAnimationDone();
            return;
        }

        Managers.PlayData.SetAutoEnterGame();
        OnExitResult();

    }

    public void OnCharacterAction(TweenCallback cb)
    {
        if (!Managers.PlayData.AmIChaser())
        {
            cb.Invoke();
            return;
        }

        selectedCharacter.transform.SetAsLastSibling();
        selectedCharacter.transform.parent.SetAsLastSibling();
        selectedCharacter.transform.parent.parent.SetAsLastSibling();

        selectedCharacter.SetAnimation("f_run_0");

        selectedCharacter.transform.DOScale(Vector3.one * 10.0f, 2.0f);
        selectedCharacter.transform.DOLocalMoveY(-1000.0f, 1.5f).OnComplete(cb);
        selectedCharacter.GetSkeletonGraphic().DOColor(new Color(1f, 1f, 1f, 0), 0.5f).SetDelay(1.0f);
    }

    public void SetUIGraphic(SkeletonGraphic skeleton)
    {
        print(skeleton.AnimationState.Data.SkeletonData.Animations.ToArray());

        skeleton.AnimationState.SetAnimation(0, skeleton.AnimationState.Data.SkeletonData.Animations.ToArray()[0].ToString(), loop: false);
        skeleton.AnimationState.AddAnimation(0, skeleton.AnimationState.Data.SkeletonData.Animations.ToArray()[1].ToString(), loop: true, 0f);

    }

    public void TestBTN()
    {
        PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.ADVANCEMENT_POPUP);
        bool check = CacheUserData.GetInt("saved_rank", Managers.UserData.MyRank.GetID()) < Managers.UserData.MyRank.GetID();
        var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.ADVANCEMENT_POPUP) as AdvancementPopup;
        popup.Init(true);
    }

    public override void Update()
    {
        if (!PopupCanvas.Instance.IsOpeningPopup())
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnExitResult();
            }
        }
    }

    public void TryADResultBtn()
    {
        if (!Managers.ADS.IsAdvertiseReady())
        {
            PopupCanvas.Instance.ShowFadeText("광고로드실패");
            return;
        }

        PopupCanvas.Instance.ShowConfirmPopup(StringManager.GetString("ui_result_ad_popup", GameConfig.Instance.AD_REWARD_MULTIPLE + 1), StringManager.GetString("ui_acc"), StringManager.GetString("ui_refusal"), () =>
         {
             Managers.ADS.TryADWithCallback(() =>
             {
                 SBWeb.GetAddResultReward(Managers.PlayData.GameRoomInfo.RoomInfo.RoomId, (res) =>
                 {
                    //SBDebug.Log(res);
                    ad_Btn.gameObject.SetActive(false);
                 });
             }, () =>
             {
                 PopupCanvas.Instance.ShowFadeText("광고로드실패");
             });
         },
        () =>
        {
        });



    }
}

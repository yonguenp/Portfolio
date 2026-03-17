using DG.Tweening;
using Newtonsoft.Json.Linq;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class RankRewardPopup : Popup
{
    //플레이 데이터 카드 목록
    [SerializeField]
    private enum PlayInfoDes
    {
        NONE = 0,

        user_play_winning,
        user_play_best_rpoint,
        user_play_mvp,
        user_play_max_point,

        user_play_maxkill,
        user_play_averkill,

        user_play_max_battery,
        user_play_averbattery,

        user_play_get_character,

        MAX_INFO
    }
    [SerializeField]
    RankRewardItem sample;

    List<bool> rewardState = new List<bool>();
    JObject response = null;
    public bool IsNew { get; private set; } = false;

    List<RankType> unPaidList = new List<RankType>();

    [SerializeField] Slider slider;
    [SerializeField] RectTransform sliderRect;
    [SerializeField] Text myPointText;
    [SerializeField] RectTransform currentPointBox;
    [SerializeField] ScrollRect rankScrollRect;
    [SerializeField] GameObject LeftArrow;
    [SerializeField] GameObject RightArrow;

    [Header("[내정보]")]
    [SerializeField] Text rankInfoText;
    [SerializeField] Text rankNameText;
    [SerializeField] Image rankImage;

    [Header("[즐겨하는 캐릭터]")]
    [SerializeField] UIPortraitCharacter surCharacter;
    [SerializeField] SkeletonGraphic surCharacterSpine;
    [SerializeField] Text surCharacterName;
    [SerializeField] UIGrade surGrade;
    [SerializeField] Text surCharacterLv;
    [SerializeField] UIEnchant surEnchant;

    [SerializeField] UIPortraitCharacter chaserCharacter;
    [SerializeField] SkeletonGraphic chaserCharacterSpine;
    [SerializeField] Text chaserCharacterName;
    [SerializeField] UIGrade chaserGrade;
    [SerializeField] Text chaserCharacterLv;
    [SerializeField] UIEnchant chaserEnchant;

    [Header("[내 플레이 정보]")]
    [SerializeField] List<GameObject> playDataInfoList = new List<GameObject>();
    [SerializeField] Transform playData_content;
    [SerializeField] GameObject infoCard;



    private void OnEnable()
    {
        Clear();
        SetRankUI();
    }

    void Clear()
    {
        foreach (Transform child in sample.transform.parent)
        {
            if (child == sample.transform)
                continue;

            Destroy(child.gameObject);
        }

        foreach (Transform item in infoCard.transform.parent)
        {
            if (item == infoCard.transform)
                continue;
            Destroy(item.gameObject);
        }

        infoCard.SetActive(false);
        sample.SetActive(false);

        foreach(Transform point in slider.fillRect.transform)
        {
            Destroy(point.gameObject);
        }
    }

    public void SyncData()
    {
        SBWeb.GetRankReward(null, SetData);
    }

    public void RefreshRewardEnableFlag()
    {
        SetNewFlag(CheckNewFlag());
    }
    public void AddStartPoint(Text pointText)
    {
        var clone = GameObject.Instantiate(pointText.gameObject, slider.fillRect.transform);
        clone.transform.position = pointText.transform.position;
        Color color = clone.GetComponent<Text>().color;
        color.a = 1.0f;
        clone.GetComponent<Text>().DOColor(color, 0.5f);

        Destroy(pointText.gameObject);
    }

    bool CheckNewFlag()
    {
        if (response == null || !response.ContainsKey("rank_reward"))
            return false;

        List<GameData> rankDataList = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.rank_grade);
        if (rankDataList == null)
            return false;

        JObject res = (JObject)response["rank_reward"];
        foreach (RankType rankData in rankDataList)
        {
            JToken value;
            if (res.TryGetValue(rankData.GetID().ToString(), out value))
            {
                if (Managers.UserData.MyRank.GetID() >= rankData.GetID() && value.Value<int>() == 0)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void SetData(JToken res)
    {
        response = (JObject)res;

        if (gameObject.activeInHierarchy)
        {
            SetRankUI();
        }
        else
        {
            RefreshRewardEnableFlag();
        }
    }

    void SetRankUI()
    {
        if (response == null || !response.ContainsKey("rank_reward"))
            return;

        List<GameData> rankDataList = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.rank_grade);
        if (rankDataList == null)
            return;

        sample.SetActive(true);
        unPaidList.Clear();
        JObject res = (JObject)response["rank_reward"];
        foreach (RankType rankData in rankDataList)
        {
            JToken value;

            if (res.TryGetValue(rankData.GetID().ToString(), out value))
            {
                GameObject rankItem = Instantiate(sample.gameObject);
                rankItem.transform.SetParent(sample.transform.parent);
                rankItem.transform.localPosition = Vector3.zero;
                rankItem.transform.localScale = Vector3.one;

                rankItem.GetComponent<RankRewardItem>().SetData(rankData, value.Value<int>() > 0, this);

                if (value.Value<int>() == 0 && Managers.UserData.MyRank.GetID() >= rankData.GetID())
                {
                    unPaidList.Add(rankData);
                }
            }
        }

        sample.SetActive(false);
        SetNewFlag(false);

        LayoutRebuilder.ForceRebuildLayoutImmediate(sample.transform.parent.GetComponent<RectTransform>());
        RefreshUI();
    }

    void SetNewFlag(bool isnew)
    {
        if (IsNew != isnew)
        {
            IsNew = isnew;
            NotifyEvent.Trigger(NotifyEvent.NotifyEventMessage.ON_RANK_REWARD);
        }
    }
    public override void Close()
    {
        base.Close();
    }

    public override void Open(CloseCallback cb = null)
    {
        closeCallback = cb;
        gameObject.SetActive(true);
        transform.SetAsLastSibling();

        foreach (Transform item in playData_content)
        {
            playDataInfoList.Add(item.gameObject);
        }

    }
    public override void RefreshUI()
    {
        List<GameData> rankDataList = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.rank_grade);
        int RankCount = rankDataList.Count;
        int maxPoint = 0;
        foreach (RankType rank in rankDataList)
        {
            if (rank.start_point > maxPoint)
                maxPoint = rank.start_point;
        }

        slider.GetComponent<RectTransform>().sizeDelta = new Vector2((RankCount * sample.GetComponent<RectTransform>().rect.width) - sample.GetComponent<RectTransform>().rect.width, slider.GetComponent<RectTransform>().rect.height);

        slider.minValue = 1;
        slider.maxValue = RankCount;

        int pivotPoint = Managers.UserData.MyPoint;
        if (pivotPoint > maxPoint)
        {
            slider.value = slider.maxValue;
        }
        else
        {
            float ratio = pivotPoint - Managers.UserData.MyRank.start_point;
            float range = Managers.UserData.MyRank.end_point - Managers.UserData.MyRank.start_point;

            slider.value = Managers.UserData.MyRank.GetID() + (ratio / range);
        }

        currentPointBox.localPosition = new Vector2((currentPointBox.parent as RectTransform).sizeDelta.x * ((slider.value - slider.minValue) / (slider.maxValue - slider.minValue)), currentPointBox.localPosition.y);

        myPointText.text = Managers.UserData.MyPoint.ToString();



        //랭크 정보
        rankInfoText.text = StringManager.GetString("ui_user_info", Managers.UserData.MyName);
        rankNameText.text = Managers.UserData.MyRank.GetName();
        rankImage.sprite = Managers.UserData.MyRank.rank_resource;

        //즐겨하는 캐릭터 
        List<PlayData> survivor_datas = new List<PlayData>();
        List<PlayData> chaser_datas = new List<PlayData>();

        foreach (var item in Managers.UserData.MyCharacters.Values)
        {
            if (item.characterData.IsChaserCharacter())
                chaser_datas.Add(item.playData);
            else
                survivor_datas.Add(item.playData);
        }

        var survivor = survivor_datas.OrderByDescending(x => x.play).ThenByDescending(x => x.win).ThenByDescending(x => Managers.UserData.GetMyCharacterInfo(x.charid).enchant)
            .ThenByDescending(x => Managers.UserData.GetMyCharacterInfo(x.charid).lv).ThenByDescending(x => (Managers.UserData.MyDefaultSurvivorCharacter == x.charid) ? 1 : 0).ToList().FirstOrDefault();

        var chaser = chaser_datas.OrderByDescending(x => x.play).ThenByDescending(x => x.win).ThenByDescending(x => Managers.UserData.GetMyCharacterInfo(x.charid).enchant)
            .ThenByDescending(x => Managers.UserData.GetMyCharacterInfo(x.charid).lv).ThenByDescending(x => (Managers.UserData.MyDefaultChaserCharacter == x.charid) ? 1 : 0).ToList().FirstOrDefault();

        if (survivor != null)
        {
            SetCharacterSpine(surCharacterSpine, survivor.charid);
            //surCharacter.SetPortrait(survivor.charid);
            surCharacterName.text = CharacterGameData.GetCharacterData(survivor.charid).GetName();
            surGrade.SetGrade(CharacterGameData.GetCharacterData(survivor.charid).char_grade);
            surCharacterLv.text = "LV." + Managers.UserData.GetMyCharacterInfo(survivor.charid).lv.ToString();
            surEnchant.SetEnchant(Managers.UserData.GetMyCharacterInfo(survivor.charid).enchant);
        }
        if (chaser != null)
        {
            SetCharacterSpine(chaserCharacterSpine, chaser.charid);
            //chaserCharacter.SetPortrait(chaser.charid);
            chaserCharacterName.text = CharacterGameData.GetCharacterData(chaser.charid).GetName();
            chaserGrade.SetGrade(CharacterGameData.GetCharacterData(chaser.charid).char_grade);
            chaserCharacterLv.text = "LV." + Managers.UserData.GetMyCharacterInfo(chaser.charid).lv.ToString();
            chaserEnchant.SetEnchant(Managers.UserData.GetMyCharacterInfo(chaser.charid).enchant);
        }


        //플레이어 정보
        var playerDatas = survivor_datas.Concat(chaser_datas).ToList();
        infoCard.SetActive(true);
        for (int i = 1; i < (int)PlayInfoDes.MAX_INFO; i++)
        {
            var obj = GameObject.Instantiate(infoCard, infoCard.transform.parent);
            var winText = obj.transform.Find("Win_Text").GetComponent<Text>();
            var valueText = obj.transform.Find("Value_Text").GetComponent<Text>();

            winText.text = StringManager.GetString(((PlayInfoDes)i).ToString());
            obj.name = winText.text;

            valueText.text = string.Empty;

            float sum = 0;
            switch ((PlayInfoDes)i)
            {
                case PlayInfoDes.user_play_winning:     //승률
                    float win = 0;
                    foreach (var data in playerDatas)
                    {
                        sum += data.play;
                        win += data.win;
                    }

                    if (sum == 0)
                    {
                        valueText.text = StringManager.GetString("ui_percent", 0);
                        break;
                    }
                    valueText.text = StringManager.GetString("ui_percent", ((win / sum) * 100).ToString("F2"));
                    break;
                case PlayInfoDes.user_play_best_rpoint: //최고 랭크 포인트
                    valueText.text = Managers.UserData.MyHightPoint.ToString();
                    break;
                case PlayInfoDes.user_play_mvp:     //mvp 달성 한 수 
                    foreach (var data in playerDatas)
                    {
                        sum += data.mvp;
                    }
                    valueText.text = StringManager.GetString("ui_number", sum);
                    break;
                case PlayInfoDes.user_play_max_point:      //최대 게임 포인트
                    float maxScore = float.MinValue;
                    foreach (var data in Managers.UserData.MyCharacters.Values)
                    {
                        if (maxScore < data.playData.highscore)
                            maxScore = data.playData.highscore;
                    }
                    valueText.text = maxScore <= 0 ? "0" : maxScore.ToString();
                    break;
                case PlayInfoDes.user_play_maxkill:     //최대 킬
                    float maxKill = float.MinValue;
                    foreach (var data in chaser_datas)
                    {
                        if (maxKill < data.best_kill)
                            maxKill = data.best_kill;
                    }
                    valueText.text = maxKill <= 0 ? StringManager.GetString("ui_people", 0) : StringManager.GetString("ui_people", maxKill);
                    break;
                case PlayInfoDes.user_play_averkill: // 평균 킬
                    float kill = 0;
                    foreach (var data in chaser_datas)
                    {
                        sum += data.play;
                        kill += data.kill;
                    }
                    if (sum == 0)
                    {
                        valueText.text = StringManager.GetString("ui_people", 0);
                        break;
                    }
                    valueText.text = StringManager.GetString("ui_people", ((kill / sum)).ToString("F1"));
                    break;
                case PlayInfoDes.user_play_max_battery:     //최대 배터리
                    float maxcharge = float.MinValue;
                    foreach (var data in survivor_datas)
                    {
                        if (maxcharge < data.best_charge)
                            maxcharge = data.best_charge;
                    }
                    valueText.text = maxcharge <= 0 ? StringManager.GetString("ui_count", 0) : StringManager.GetString("ui_count", maxcharge);
                    break;
                case PlayInfoDes.user_play_averbattery: // 배터리 평균
                    float charge = 0;
                    foreach (var data in survivor_datas)
                    {
                        sum += data.play;
                        charge += data.charge;
                    }
                    if (sum == 0)
                    {
                        valueText.text = StringManager.GetString("ui_count", 0);
                        break;
                    }
                    valueText.text = StringManager.GetString("ui_count", ((charge / sum)).ToString("F1"));
                    break;
                case PlayInfoDes.user_play_get_character:       //보유 캐릭터 수 
                    valueText.text = StringManager.GetString("ui_people", Managers.UserData.MyCharacters.Count);
                    break;
                default:
                    SBDebug.Log("새로운 정보가 나왔다!");
                    break;
            }
        }
        infoCard.SetActive(false);

        Invoke("OnCurRankPosition", 0.1f);
    }

    public void OnTryReward(RankType type)
    {
        Clear();

        List<int> types = new List<int>();
        types.Add(type.GetID());

        SBWeb.GetRankReward(types, (res) =>
        {
            SetData(res);
        });
    }
    public void OnTryGetAll()
    {
        if (unPaidList.Count == 0)
        {
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("ui_no_reward"));
            return;
        }

        Clear();

        List<int> types = new List<int>();
        foreach (RankType type in unPaidList)
        {
            types.Add(type.GetID());
        }

        SBWeb.GetRankReward(types, (res) =>
        {
            SetData(res);
        });
    }

    public void Update()
    {
        sliderRect.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(sample.transform.parent.GetComponent<RectTransform>().anchoredPosition.x, sliderRect.anchoredPosition.y);

        Vector3 pos = currentPointBox.localPosition;
        pos.x = slider.fillRect.rect.width;

        Rect worldBound = PopupCanvas.Instance.GetWorldRect();
        if (worldBound.x >= currentPointBox.transform.position.x)//좌측으로 넘어감
        {
            currentPointBox.gameObject.SetActive(false);
            LeftArrow.SetActive(true);
            RightArrow.SetActive(false);
        }
        else if (worldBound.x + worldBound.width <= currentPointBox.transform.position.x)//우측측으로 넘어감
        {
            currentPointBox.gameObject.SetActive(false);
            LeftArrow.SetActive(false);
            RightArrow.SetActive(true);
        }
        else
        {
            currentPointBox.gameObject.SetActive(true);
            LeftArrow.SetActive(false);
            RightArrow.SetActive(false);
        }

        //if (slider.value >= slider.maxValue)
        //    slider.transform.parent.Find("R_Image").gameObject.SetActive(true);
        //else
        //    slider.transform.parent.Find("R_Image").gameObject.SetActive(false);

        //currentArrow.SetActive(onArrow);
    }

    public void OnCurRankPosition()
    {
        CancelInvoke("OnCurRankPosition");
        float value = (slider.value - slider.minValue) / (slider.maxValue - slider.minValue);
        if (value < 0.0f)
            value = 0.0f;
        rankScrollRect.horizontalNormalizedPosition = value;
    }
    public void SetCharacterSpine(SkeletonGraphic characterObj, int char_id, bool check = false)
    {
        CharacterGameData data = CharacterGameData.GetCharacterData(char_id);
        if (characterObj != null && data != null)
        {
            characterObj.gameObject.SetActive(true);
            characterObj.skeletonDataAsset = data.spine_resource;
            characterObj.startingAnimation = "f_idle_0";
            characterObj.startingLoop = true;


            characterObj.Initialize(true);
        }
    }
}

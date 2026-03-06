using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using Spine.Unity;

public class RankRewardItem : UIBundleItem
{
    [SerializeField] 
    SkeletonGraphic characterSpine;
    [SerializeField]
    Button rewardButton;
    [SerializeField]
    Image image;
    [SerializeField]
    Text rankStartPointText;

    RankType data = null;
    RankRewardPopup parent = null;
    bool rewarded = true;

    protected override void Init()
    {
        base.Init();

        characterSpine.gameObject.SetActive(false);
    }
    public void SetActive(bool enable)
    {
        gameObject.SetActive(enable);
    }

    public void SetData(RankType d, bool rewardedflag, RankRewardPopup p)
    {
        data = d;
        parent = p;
        rewarded = rewardedflag;

        SetActive(true);
        //checkImage.gameObject.SetActive(false);
        image.sprite = data.rank_resource;

        if (data != null && data.rank_reward.Count > 0)
        {
            SetRewards(data.rank_reward);
        }

        rankStartPointText.text = data.start_point.ToString();

        if (Managers.UserData.MyRank.GetID() == data.GetID())
        {
            float scale = 1.5f;
            if (data.rank_reward.Count == 1)
            {
                if (data.rank_reward[0].targetCharacter != null)
                    scale = 1.2f;
            }

            SetScale(scale);
        }
        else
        {
            if (Managers.UserData.MyRank.GetID() < data.GetID())
            {
                //SetDisableColor();
            }
        }

        if (rewarded)
        {
            if(Checker != null)
                Checker.SetActive(true);

            //SetDisableColor();
        }

        Color color = rankStartPointText.color;
        color.a = 0.0f;
        rankStartPointText.color = color;

        Invoke("CloneStartPoint", 1.0f);
    }

    void CloneStartPoint()
    {
        CancelInvoke("CloneStartPoint");

        parent.AddStartPoint(rankStartPointText);
    }

    void SetDisableColor()
    {

        foreach (MaskableGraphic child in GetComponentsInChildren<MaskableGraphic>())
        {
            if (Checker.transform == child.transform)
                continue;

            if (multiItemDetailPopupButton.transform == child.transform)
                continue;

            child.color = new Color(0.61514798f, 0.61514798f, 0.61514798f, 1.0f);
        }
    }

    public override void SetCharacterInfo(int char_id, bool check = false)
    {
        base.SetCharacterInfo(char_id, check);

        CharacterGameData data = CharacterGameData.GetCharacterData(char_id);
        if (characterSpine != null && data != null)
        {
            characterSpine.gameObject.SetActive(true);
            characterSpine.skeletonDataAsset = data.spine_resource;
            characterSpine.startingAnimation = "f_idle_0";
            characterSpine.startingLoop = true;

            if (Managers.UserData.GetMyCharacterInfo(char_id) != null)
            {
                characterSpine.startingAnimation = "f_victory_0";
            }

            characterSpine.Initialize(true);
        }
    }
    public override void SetScale(float scale)
    {
        base.SetScale(scale);

        characterSpine.transform.localScale = Vector3.one * scale;
    }
    protected override void OnButton()
    {
        if(rewarded)
        {
            PopupCanvas.Instance.ShowFadeText("msg_already_getitem");
            return;
        }
        
        if (Managers.UserData.MyRank.GetID() < data.GetID())
        {
            PopupCanvas.Instance.ShowFadeText("msg_not_getitem");
            return;
        }

        if (data != null && data.rank_reward.Count > 0)
        {
            parent.OnTryReward(data);
        }
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
    }
}

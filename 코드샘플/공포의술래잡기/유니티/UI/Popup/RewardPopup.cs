using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static SBWeb;
using DG.Tweening;

public class RewardPopup : Popup
{
    public enum RewardType
    {
        REWARD,
        BUY
    }

    [SerializeField] Text title;
    [SerializeField] GameObject sampleItem;
    [SerializeField] Text touchText;
    [SerializeField] InventoryEchantResult equipResult;
    public override void Open(CloseCallback cb = null)
    {
        base.Open(cb);
        equipResult.gameObject.SetActive(false);
        touchText.DOKill();
        ColorUtility.TryParseHtmlString("#FFF5B3", out Color color);
        touchText.color = color;
        touchText.transform.localScale = Vector3.one;
        //touchText.DOColor(Color.white, 0.5f);
        touchText.transform.DOScale(1.15f, 1.0f).SetLoops(-1,LoopType.Yoyo);
        Managers.Sound.Play("effect/GET_RESULT", Sound.Effect);
    }
    public override void RefreshUI()
    {
    }
    public override void Close()
    {
        Clear();
        base.Close();
    }

    public void SetData(RewardType type, List<SBWeb.ResponseReward> rewards)
    {
        switch(type)
        {
            case RewardType.REWARD:
                title.text = StringManager.GetString("msg_item_get");
                break;
            case RewardType.BUY:
                title.text = StringManager.GetString("구매결과");
                break;
            default:
                title.text = "";
                break;
        }

        SetItemLIstUI(rewards);
    }
    void SetItemLIstUI(List<ResponseReward> rewards)
    {
        Clear();

        sampleItem.SetActive(true);

        List<ResponseReward> result = new List<ResponseReward>();
        foreach (var reward in rewards)
        {
            bool newR = true;
            foreach (var prev in result)
            {
                if(prev.Type == reward.Type && prev.Id == reward.Id)
                {
                    prev.AddAmount(reward.Amount);
                    newR = false;
                    break;
                }
            }

            if(newR)
                result.Add(reward);
        }

        foreach (var reward in result)
        {
            var obj = GameObject.Instantiate(sampleItem, sampleItem.transform.parent);
            
            obj.GetComponent<RewardItemUI>().SetRewardInfo(reward);
            obj.transform.DOScale(0.0f, 0.1f).From();               
        }


        RefreshUI();
        sampleItem.SetActive(false);
    }

    public void Clear()
    {
        foreach (Transform item in sampleItem.transform.parent)
        {
            if (item == sampleItem.transform)
                continue;
            Destroy(item.gameObject);
        }
    }

    public void ResultEquipEnchant(UserEquipData prior, UserEquipData cur)
    {
        equipResult.gameObject.SetActive(true);
        equipResult.Init(prior, cur);
    }
}

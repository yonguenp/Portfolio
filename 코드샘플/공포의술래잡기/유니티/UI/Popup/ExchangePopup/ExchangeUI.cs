using Coffee.UIExtensions;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExchangeUI : MonoBehaviour
{
    [SerializeField] ExchangeSlot[] exchangeSlots;
    [SerializeField] Text totalTradeCount;

    [SerializeField] GameObject soulStone;
    [SerializeField] Transform image;

    [SerializeField] Button convertBtn;
    [Header("[uiparticl]")]
    [SerializeField] UIParticle[] fx_soulstone04s;
    [SerializeField] UIParticle[] fx_effects;

    public int slot_idx;
    public int totalAmount = 0;

    public Dictionary<int, int> idList = new Dictionary<int, int>();
    public void Init()
    {
        int idx = 0;
        foreach (var item in exchangeSlots)
        {
            item.Init(idx, this);
            idx++;
        }
        totalAmount = 0;
        OnSoulStoneAnim();
    }

    public void Fx_Soulstone04sInit()
    {
        int idx = 0;
        foreach (var item in fx_soulstone04s)
        {
            if (exchangeSlots[idx].card_d == null)
            {
                idx++;
                continue;
            }

            item.transform.position = exchangeSlots[idx].UIBundleItem.transform.position;

            item.Play();
            item.transform.DOMove(image.transform.position, 0.7f).SetEase(Ease.OutExpo).OnComplete(() =>
            {
                item.Stop();
                foreach (var effect in fx_effects)
                {
                    effect.Play();
                }
            });
            idx++;
        }
    }

    public void OnSoulStoneAnim()
    {
        var rect = soulStone.transform as RectTransform;
        rect.DOAnchorPosY(40f, 2f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.OutQuad);

        var effect = Array.Find(fx_effects, _ => _.name == "fx_soulstone01");

        if (effect != null)
            effect.GetComponent<UIParticle>().Play();

    }

    public void RefreshSoulstone()
    {
        totalAmount = 0;
        foreach (var item in exchangeSlots)
        {
            if (item.card_d == null)
                continue;

            totalAmount += item.changeSoulAmount;
        }

        totalTradeCount.text = totalAmount.ToString();
    }

    public void RefreshTradeGold()
    {
        exchangeSlots[0].UndoEquipBtn();

    }

    public void ConvertToSoulstone()
    {
        convertBtn.interactable = false;
        idList.Clear();
        foreach (var item in exchangeSlots)
        {
            if (item.card_d == null || item.itemUseCount < 1)
                continue;
            idList.Add(item.card_d.goods_param, (int)item.itemUseCount);
        }

        if (idList == null || idList.Count == 0)
        {
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("msg_nocheck_soulcard"));
            convertBtn.interactable = true;
            return;
        }
        SBWeb.TradeSoulStone(idList, (rewards) =>
        {
            Sequence sq = DOTween.Sequence();

            sq.AppendCallback(() => { Fx_Soulstone04sInit(); });
            sq.AppendInterval(1.2f);
            sq.AppendCallback(() =>
            {
                SlotAllClear();
                RefreshSoulstone();
            });
            sq.AppendCallback(() =>
            {
                PopupCanvas.Instance.ShowRewardResult(rewards);
                convertBtn.interactable = true;
            });

        });
    }
    public void RefreshGold(string text)
    {
        totalTradeCount.text = text;
    }
    public void ConvertToGold()
    {
        convertBtn.interactable = false;

        var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.EXCHANGE_POPUP) as ExchangePopup;

        if (popup.exchange_equipList == null || popup.exchange_equipList.Count == 0)
        {
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("msg_nocheck_equipgold"));
            convertBtn.interactable = true;
            return;
        }
        List<int> equip_uid = new List<int>();
        equip_uid.Clear();
        foreach (UIbundleEquip item in popup.exchange_equipList)
        {
            equip_uid.Add(item.info.id);
        }
        SBWeb.TradeEquipToItem(equip_uid, (rewards) =>
        {
            Sequence sq = DOTween.Sequence();

            //sq.AppendCallback(() => { Fx_Soulstone04sInit(); });
            //sq.AppendInterval(1.2f);
            //sq.AppendCallback(() =>
            //{
            //    SlotAllClear();
            //    RefreshSoulstone();
            //});
            RefreshTradeGold();
            sq.AppendCallback(() =>
            {
                PopupCanvas.Instance.ShowRewardResult(rewards);
                convertBtn.interactable = true;
            });
        });
    }

    public void SetItemId(BundleInfo item_id)
    {
        foreach (var item in exchangeSlots)
        {
            if (item.card_d != null && item.card_d.goods_param == item_id.goods_param)
            {
                PopupCanvas.Instance.ShowFadeText(StringManager.GetString("msg_already_check_soulcard"));
                item_id = null;
                break;
            }
        }

        exchangeSlots[slot_idx].setCardInfo(item_id);
    }

    public ExchangeSlot[] Getlot()
    {
        return exchangeSlots;
    }

    public void SlotAllClear()
    {
        foreach (var item in exchangeSlots)
        {
            item.CancelBtn();
        }
    }

}

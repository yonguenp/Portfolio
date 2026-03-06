using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GachaTableGroupClone : GachaTableSubClone
{
    [SerializeField] ScrollRect scrollRect = null;
    [SerializeField] GameObject gachaTableSubClone = null;
    List<GachaTableSubClone> child = new List<GachaTableSubClone>();
    int subtotal = 0;
    bool subOpen = true;
    GachaTablePopup parent = null;
    public void InitGroupClone(ItemGroupData rateData, GachaTablePopup parentPopup)
    {
        parent = parentPopup;
        currentItemGroupData = rateData;
        nameText.text = StringData.GetStringByStrKey("gacha_rate:" + rateData.KEY.ToString());

        foreach (ItemGroupData data in currentItemGroupData.Child)
        {
            GameObject newSubClone = Instantiate(gachaTableSubClone, scrollRect.content);
            var sub = newSubClone.GetComponent<GachaTableSubClone>();
            sub.InitSubClone(data);
            child.Add(sub);

            subtotal += data.ITEM_RATE;
        }
    }
    public override void UpdateMaxRate(float curRate, int subTotal)
    {
        if (probabilityText != null)
        {
            if (currentItemGroupData != null)
            {
                ItemGroupListData data = ItemGroupListData.Get(currentItemGroupData.GROUP);
                switch(data.DICE_TYPE)
                {
                    case 3:
                    case 4:
                        var rate = ((float)currentItemGroupData.ITEM_RATE / data.MAX_RATE) * curRate;
                        probabilityText.text = SBFunc.StrBuilder((rate * 100f - 0.00005f).ToString("F4"), "%");

                        foreach(var c in child)
                        {
                            c.UpdateMaxRate(rate, subtotal);
                        }
                        break;
                }

                //var rate = ((float)currentItemGroupData.ITEM_RATE / subTotal) * curRate;
                //probabilityText.text = SBFunc.StrBuilder((rate * 100f - 0.00005f).ToString("F4"), "%");
            }
        }
    }
    public void OnToggleTable()
    {
        SetSubTable(!subOpen);
    }

    public void SetSubTable(bool enable)
    {
        subOpen = enable;

        foreach (var sub in child)
        {
            sub.SetActive(enable);
        }

        parent.CancelInvoke("CheckScrollSize");
        parent.Invoke("CheckScrollSize", 0.1f);
    }

    public override void SetActive(bool enable)
    {
        base.SetActive(enable);

        foreach (var sub in child)
        {
            sub.SetActive(enable && subOpen);
        }
    }
}

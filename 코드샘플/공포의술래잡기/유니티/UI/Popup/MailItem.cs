using Newtonsoft.Json.Linq;
using SBCommonLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MailItem : MonoBehaviour
{
    enum MailType
    {
        NONE = -1,
        MAIL_DEFAULT = 0,
        MAIL_RECEIVE,
        MAIL_DELETE,
    }

    private MailType curType = MailType.NONE;
    [SerializeField] Text title;
    [SerializeField] Text subTitle;
    [SerializeField] Text postTime;
    [SerializeField] Text receiveTime;
    [SerializeField] Text limiteTime;

    [SerializeField] List<UIBundleItem> uIBundleItems;
    [SerializeField] GameObject Dim;
    [SerializeField] Button getBtn;

    MailData data;
    public void Init(MailData d)
    {
        data = d;
        curType = (MailType)d.state;

        title.text = StringManager.GetString(data.mail_title);
        subTitle.text = data.mail_subtitle;
        subTitle.text = StringManager.GetString(data.mail_subtitle);

        postTime.text = StringManager.GetString("발송 날짜") + ": " + data.post_date.ToString();

        if (d.limit_date < DateTime.MaxValue)
        {
            var limitdate = (d.limit_date - SBUtil.KoreanTime);
            if (limitdate.Days >= 1.0f)
                limiteTime.text = StringManager.GetString("ui_day", limitdate.Days.ToString());
            else
                limiteTime.text = StringManager.GetString("ui_hour", limitdate.Hours.ToString());
        }
        if (d.limit_date == DateTime.MaxValue)
            limiteTime.text = StringManager.GetString("ui_limitless");

        List<BundleInfo> infos = new List<BundleInfo>();
        infos.Clear();

        ASSET_TYPE type = (ASSET_TYPE)d.type;
        int param = d.param;
        int amount = d.amount;

        if (d.type != 0)
        {
            BundleInfo info = new BundleInfo(type, param, amount);
            infos.Add(info);
        }


        foreach (var item in uIBundleItems)
        {
            item.gameObject.SetActive(false);
        }

        if (infos.Count > 3)
        {
            uIBundleItems[0].gameObject.SetActive(true);
            uIBundleItems[0].SetIcon(Managers.Resource.LoadAssetsBundle<Sprite>("AssetsBundle/Texture/Icon/dummy_box"));
            uIBundleItems[0].SetBundleInfos(infos, StringManager.GetString("우편보상"));
        }
        else
        {
            for (int i = 0; i < infos.Count; i++)
            {
                uIBundleItems[i].gameObject.SetActive(true);
                uIBundleItems[i].SetReward(infos[i]);
            }
        }

        if (curType == MailType.MAIL_DEFAULT)
        {
            Dim.SetActive(false);
            getBtn.interactable = true;
        }
        else
        {
            Dim.SetActive(true);
            getBtn.interactable = false;
        }
        if (infos.Count == 0)
        {
            getBtn.GetComponent<Image>().sprite = Resources.Load<Sprite>("Texture/UI/Lobby/btn_sub_cherry_01");
            if (getBtn.GetComponentInChildren<Text>() != null)
                getBtn.GetComponentInChildren<Text>().text = StringManager.GetString("button_mail_del");
        }

    }

    public void ReceiveMail()
    {
        List<int> temp = new List<int>();
        temp.Add(data.id);

        (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.MAIL_POPUP) as MailPopup).TryReward(temp);
    }
}

using Newtonsoft.Json.Linq;
using SBCommonLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MailPopup : Popup
{
    [SerializeField] MailItem sampleMailObject;
    [SerializeField] Transform content;
    [SerializeField] Button getAllBtn;
    [SerializeField] GameObject emptyMail;


    public bool isNew { get; private set; } = false;

    List<MailData> mailLists = new List<MailData>();

    List<int> ableRewardList = new List<int>();
    public override void Close()
    {
        Clear();
        base.Close();
    }
    public override void Open(CloseCallback cb = null)
    {
        SBWeb.GetUserMailDB(GetMailData);

        base.Open(cb);
    }
    public override void RefreshUI()
    {
        Clear();

        sampleMailObject.gameObject.SetActive(true);
        foreach (var item in mailLists)
        {
            MailItem obj = Instantiate(sampleMailObject, content);
            obj.Init(item);
        }
        sampleMailObject.gameObject.SetActive(false);

        emptyMail.SetActive(!Convert.ToBoolean(mailLists.Count));
    }
    public void Clear()
    {
        foreach (Transform item in content)
        {
            if (item == sampleMailObject.transform)
                continue;
            Destroy(item.gameObject);
        }
    }

    public void GetMailData(JToken datas)
    {
        mailLists.Clear();
        ableRewardList.Clear();

        foreach (JToken data in datas)
        {
            MailData mailData = new MailData();
            mailData.SetData(data);

            if (mailData.state == 0 && mailData.limit_date > SBUtil.KoreanTime)
            {
                if (mailData.AbleRewardMail())
                    ableRewardList.Add(mailData.id);

                mailLists.Add(mailData);
            }
            else if(mailData.limit_date == DateTime.MaxValue && mailData.state == 0)
                ableRewardList.Add(mailData.id);
        }

        RefreshUI();
        SetNewFlag(CheckNewFlag());
    }

    public bool CheckNewFlag()
    {
        foreach (var item in mailLists)
        {
            if (item.state == 0)
                return true;
        }
        return false;
    }
    public void SetNewFlag(bool isnew)
    {
        isNew = isnew;
        NotifyEvent.Trigger(NotifyEvent.NotifyEventMessage.ON_MAIL_INFO);
    }

    public void TryGetAllReward()
    {
        if (ableRewardList.Count == 0)
        {
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("ui_no_reward"));
            return;
        }

        TryReward(ableRewardList);
    }
    public void TryReward(List<int> id = null)
    {
        SBWeb.GetUserMailReward(id, (res) =>
        {
            GetMailData(res);
        });
    }
}

public class MailData
{
    public int id;
    public DateTime post_date;
    public DateTime receive_date;
    public DateTime limit_date;

    public int type;
    public int param;
    public int amount;
    public string mail_title;
    public string mail_subtitle;
    public int state;
    public string mail_id;
    public void SetData(JToken d)
    {
        id = d["id"].Value<int>();
        post_date = Convert.ToDateTime(d["post_date"].Value<string>());

        if (d["receive_date"].Value<string>() == null || d["receive_date"].Value<string>() == string.Empty)
            receive_date = DateTime.MinValue;
        else
            receive_date = Convert.ToDateTime(d["receive_date"].Value<string>());

        if (d["limit_date"].Value<string>() != null)
            limit_date = Convert.ToDateTime(d["limit_date"].Value<string>());
        else
            limit_date = DateTime.MaxValue;

        type = d["type"].Value<int>();
        param = d["param"].Value<int>();
        amount = d["amount"].Value<int>();
        mail_title = d["mail_title"].Value<string>();
        mail_subtitle = d["mail_subtitle"].Value<string>();
        state = d["state"].Value<int>();
        mail_id = d["mail_id"].Value<string>();
    }

    public bool AbleRewardMail()
    {
        if (receive_date == DateTime.MinValue && state == 0 && type != 0)
            return true;
        else
            return false;
    }
}

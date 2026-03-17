using Newtonsoft.Json.Linq;
using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuildDonationClone : MonoBehaviour
{
    [SerializeField]
    Text titleText;

    [SerializeField]
    Image assetIconImg;

    [SerializeField]
    Text assetAmountText;
    [SerializeField]
    Image assetIconSmallImg;
    [SerializeField]
    Button DonateBtn;

    [SerializeField]
    Text account_exp_amount;
    [SerializeField]
    Text guild_exp_amount;
    [SerializeField]
    Text guild_point_amount;

    GuildDonationData data = null;
    bool isDonateAssetPossible = false;

    /// <summary>
    /// 서버로 전송하기 위한 키값
    /// </summary>
    int DonationKey = 0;
    public void Init(GuildDonationData donationData)
    {
        if (donationData != null)
            data = donationData;
        else
            return;

        RefreshButtonState();
    }

    void SetInfo()
    {
        int price = data.NEED_NUM;
        DonationKey = int.Parse(data.KEY);

        switch (data.NEED_TYPE)
        {
            case "GOLD":
                assetIconImg.sprite = assetIconSmallImg.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "gold");
                assetAmountText.text = price.ToString();
                isDonateAssetPossible = User.Instance.GOLD >= price;                     
                break;
            case "GEMSTONE":
                if (price > 300)
                {
                    //assetIconImg.sprite = assetIconSmallImg.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "gemstone");
                    assetIconSmallImg.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "gemstone");
                }
                else
                {
                    assetIconImg.sprite = assetIconSmallImg.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "gemstone");
                }
                assetAmountText.text = price.ToString();
                isDonateAssetPossible = User.Instance.GEMSTONE >= price;
                break;
        }

        titleText.text = StringData.GetStringByStrKey("guild_donetion:" + DonationKey);
        assetAmountText.color = isDonateAssetPossible ? Color.white : Color.red;
        account_exp_amount.text = SBFunc.CommaFromNumber(data.REWARD_ACCOUNT_EXP);
        guild_exp_amount.text = SBFunc.CommaFromNumber(data.REWARD_GUILD_EXP);
        guild_point_amount.text = SBFunc.CommaFromNumber(data.REWARD_GUILD_POINT);
    }

    public void RefreshButtonState()
    {
        SetInfo();

        DonateBtn.SetButtonSpriteState(isDonateAssetPossible && GuildManager.Instance.GuildRemainDonationCount > 0);
    }

    public void SetButtonListener(VoidDelegate _serverCallback)
    {
        if(DonateBtn != null)
        {
            DonateBtn.onClick.RemoveAllListeners();
            DonateBtn.onClick.AddListener(() => { OnClickDonation(_serverCallback); });
        }
    }

    public void OnClickDonation(VoidDelegate _cb = null)
    {
        if(GuildManager.Instance.GuildRemainDonationCount <= 0)
        {
            ToastManager.On(StringData.GetStringByStrKey("guild_desc:112"));
            return;
        }
        if (isDonateAssetPossible ==false)
        {
            ToastManager.On(StringData.GetStringByStrKey("dragon_info_text_07"));
            return;
        }

        WWWForm form = new WWWForm();
        form.AddField("donate_type", data.KEY);
        form.AddField("gno", GuildManager.Instance.GuildID);
        GuildManager.Instance.NetworkSend("guild/donate", form, (JObject jsonData) =>
        {
            if (jsonData.ContainsKey("rewards"))
            {
                ChatManager.Instance.SendGuildDonationSystemMessage(new GuildSystemMessage(eGuildSystemMsgType.Donation, data.KEY));

                var rewards = jsonData["rewards"];
                List<Asset> assets = new();
                if (SBFunc.IsJTokenCheck(rewards["acc_exp"]))
                {
                    assets.Add(new Asset(eGoodType.ACCOUNT_EXP, 0, rewards["acc_exp"].ToObject<int>()));
                }
                if (SBFunc.IsJTokenCheck(rewards["guild_point"]))
                {
                    assets.Add(new Asset(eGoodType.GUILD_POINT, 0, rewards["guild_point"].ToObject<int>()));
                }
                if (SBFunc.IsJTokenCheck(rewards["guild_exp"]))
                {
                    assets.Add(new Asset(eGoodType.GUILD_EXP, 0, rewards["guild_exp"].ToObject<int>()));
                }

                SystemRewardPopup.OpenPopup(assets, null, true);
            }

            _cb?.Invoke();
        });
    }
}

using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChampionTableUserSlot : MonoBehaviour
{
    [SerializeField]
    GuildBaseInfoObject guildBaseInfo;
    [SerializeField]
    Text UserName;
    [SerializeField]
    Image Panel;

    [SerializeField]
    Sprite AngleSprite;
    [SerializeField]
    Sprite WonderSprite;
    [SerializeField]
    Sprite LunaSprite;

    [SerializeField]
    Sprite DisableSprite;
    [SerializeField]
    Color MyColor;
    [SerializeField]
    Color OtherColor;
    [SerializeField]
    GameObject Dim;

    public Sprite GetEnableSprite(ParticipantData user)
    {
        switch(user.SERVER)
        {
            case 1:
                return AngleSprite;
            case 2:
                return WonderSprite;
            case 3:
                return LunaSprite;
        }

        return null;
    }
    public void SetActive(bool enable)
    {
        gameObject.SetActive(enable);
    }
    public void SetUser(ParticipantData user, bool isAfter, bool isWin)
    {
        Panel.sprite = isAfter ? (user != null && isWin ? GetEnableSprite(user) : DisableSprite) : (user == null ? DisableSprite : GetEnableSprite(user));
        Dim.SetActive(isAfter ? (user == null || !isWin) : false);

        if (user == null)
        {
            guildBaseInfo.gameObject.SetActive(false);
            UserName.gameObject.SetActive(false);
            return;
        }

        bool enableGuild = GuildManager.Instance.GuildWorkAble && user.HasGuild;
        guildBaseInfo.gameObject.SetActive(enableGuild);

        if (enableGuild)
        {
            guildBaseInfo.Init(user.GUILD_NAME, user.GUILD_MARK, user.GUILD_EMBLEM, user.GUILD_NO);
        }

        UserName.gameObject.SetActive(true);
        UserName.text = user.NICK;

        UserName.color = user.USER_NO == User.Instance.UserAccountData.UserNumber ? MyColor : OtherColor;
    }
}

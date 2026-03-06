using Coffee.UIExtensions;
using DG.Tweening;
using Newtonsoft.Json.Linq;
using SBSocketSharedLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListItem : MonoBehaviour
{
    [SerializeField] Image background;
    [SerializeField] Image portraitImage;
    [SerializeField] Text txtUserName;
    [SerializeField] Text txtClanName;

    [SerializeField] Image textPointIcon;
    [SerializeField] Text txtPointName;
    [SerializeField] Text txtPoint;

    [SerializeField] UIPortraitCharacter imageCharacter;

    [SerializeField] Image rankImage;
    [SerializeField] Button addFriendButton;
    [SerializeField] UIEnchant enchantUI;
    [SerializeField] Text levelText;

    [SerializeField] UIParticle mvp_grid_cir_sur;
    [SerializeField] UIParticle mvp_grid_cir_chase;
    [SerializeField] UIParticle mine_grid_rect;

    [SerializeField] GameObject mvpImage;
    [SerializeField] GameObject exitImage;
    [SerializeField] GameObject duoImage;

    public string UserId { get; set; }
    bool isChaser = false;
    public void Initialize(RoomPlayerInfo roomPlayer)
    {
        if (roomPlayer.UserId == Managers.UserData.MyUserID)
        {
            background.sprite = Managers.Resource.Load<Sprite>("Texture/UI/Lobby/result_frame_06");
            portraitImage.sprite = Managers.Resource.Load<Sprite>("Texture/UI/Lobby/result_frame_05");
            textPointIcon.sprite = Managers.Resource.Load<Sprite>("Texture/UI/InGame/kill_icon");

            if (CharacterGameData.IsChaserCharacter(roomPlayer.SelectedCharacter.CharacterType))
                txtPoint.text = Game.Instance.GameRoom.GetKillCount(roomPlayer.UserId).ToString();
            else
                txtPoint.text = Game.Instance.GameRoom.GetChargeCount(roomPlayer.UserId).ToString();

            addFriendButton.gameObject.SetActive(false);
            txtUserName.color = Color.white;
        }
        else if (CharacterGameData.IsChaserCharacter(roomPlayer.SelectedCharacter.CharacterType))
        {

            background.sprite = Managers.Resource.Load<Sprite>("Texture/UI/Lobby/result_frame_03");
            portraitImage.sprite = Managers.Resource.Load<Sprite>("Texture/UI/Lobby/result_frame_01");
            textPointIcon.sprite = Managers.Resource.Load<Sprite>("Texture/UI/InGame/kill_icon");
            txtPoint.text = Game.Instance.GameRoom.GetKillCount(roomPlayer.UserId).ToString();
        }
        else
        {
            background.sprite = Managers.Resource.Load<Sprite>("Texture/UI/Lobby/result_frame_04");
            portraitImage.sprite = Managers.Resource.Load<Sprite>("Texture/UI/Lobby/result_frame_02");
            textPointIcon.sprite = Managers.Resource.Load<Sprite>("Texture/UI/InGame/battery_effect");
            txtPoint.text = Game.Instance.GameRoom.GetChargeCount(roomPlayer.UserId).ToString();
        }

        isChaser = CharacterGameData.IsChaserCharacter(roomPlayer.SelectedCharacter.CharacterType);
        txtPointName.text = StringManager.GetString("ui_get_point");
        txtUserName.text = roomPlayer.UserName;
        imageCharacter.SetPortrait(roomPlayer.SelectedCharacter.CharacterType);
        rankImage.sprite = RankType.GetRankFromPoint(roomPlayer.RankPoint).rank_resource;
        mvp_grid_cir_sur.Stop();
        mvp_grid_cir_chase.Stop();
        mvpImage.GetComponent<Image>().sprite = Managers.Resource.LoadAssetsBundle<Sprite>("AssetsBundle/Texture/Icon/quest_mvp");
        mvpImage.SetActive(false);
        mine_grid_rect.Stop();
        if (isChaser)
            OnExitUser(false);

        if (enchantUI != null)
        {
            enchantUI.SetEnchant(roomPlayer.SelectedCharacter.Enhance_Step);
        }

        if (levelText != null)
        {
            levelText.text = "Lv." + roomPlayer.SelectedCharacter.Level;
        }


        addFriendButton.interactable = false;

        UserId = roomPlayer.UserId.ToString();

        if (long.TryParse(UserId, out long uno))
        {
            foreach (UserProfile friend in Managers.FriendData.GetFriendList())
            {
                if (friend.uno == uno)
                    return;
            }

            foreach (UserProfile friend in Managers.FriendData.GetSentList())
            {
                if (friend.uno == uno)
                    return;
            }

            addFriendButton.interactable = true;
        }

        if(string.IsNullOrEmpty(roomPlayer.ClanName))
        {
            txtClanName.gameObject.SetActive(false);
        }
        else
        {
            txtClanName.gameObject.SetActive(true);
            txtClanName.text = roomPlayer.ClanName;
        }

        OnDuoUser(false);
    }

    public void Initialize(JObject roomPlayer, int rankPoint)
    {
        if (roomPlayer["user_no"].Value<long>() == Managers.UserData.MyUserID)
        {
            background.sprite = Managers.Resource.Load<Sprite>("Texture/UI/Lobby/result_frame_06");
            portraitImage.sprite = Managers.Resource.Load<Sprite>("Texture/UI/Lobby/result_frame_05");
            textPointIcon.sprite = Managers.Resource.Load<Sprite>("Texture/UI/InGame/kill_icon");

            txtPoint.text = roomPlayer["point"].Value<int>().ToString();

            addFriendButton.gameObject.SetActive(false);
            txtUserName.color = Color.white;
        }
        else if (CharacterGameData.IsChaserCharacter(roomPlayer["char_id"].Value<int>()))
        {
            background.sprite = Managers.Resource.Load<Sprite>("Texture/UI/Lobby/result_frame_03");
            portraitImage.sprite = Managers.Resource.Load<Sprite>("Texture/UI/Lobby/result_frame_01");
            textPointIcon.sprite = Managers.Resource.Load<Sprite>("Texture/UI/InGame/kill_icon");

            txtPoint.text = roomPlayer["point"].Value<int>().ToString();
        }
        else
        {
            background.sprite = Managers.Resource.Load<Sprite>("Texture/UI/Lobby/result_frame_04");
            portraitImage.sprite = Managers.Resource.Load<Sprite>("Texture/UI/Lobby/result_frame_02");
            textPointIcon.sprite = Managers.Resource.Load<Sprite>("Texture/UI/InGame/battery_effect");

            txtPoint.text = roomPlayer["point"].Value<int>().ToString();
        }
        isChaser = CharacterGameData.IsChaserCharacter(roomPlayer["char_id"].Value<int>());

        txtPointName.text = StringManager.GetString("ui_get_point");
        txtUserName.text = roomPlayer["user_nick"].Value<string>();
        imageCharacter.SetPortrait(roomPlayer["char_id"].Value<int>());

        rankImage.sprite = RankType.GetRankFromPoint(rankPoint).rank_resource;
        mvp_grid_cir_sur.Stop();
        mvp_grid_cir_chase.Stop();
        mvpImage.GetComponent<Image>().sprite = Managers.Resource.LoadAssetsBundle<Sprite>("AssetsBundle/Texture/Icon/quest_mvp");
        mvpImage.SetActive(false);
        mine_grid_rect.Stop();

        if (isChaser)
            OnExitUser(false);

        if (enchantUI != null)
        {
            enchantUI.SetEnchant(roomPlayer["char_en"].Value<int>());
        }

        if (levelText != null)
        {
            levelText.text = "Lv." + roomPlayer["char_lv"].Value<int>();
        }

        addFriendButton.interactable = false;

        UserId = roomPlayer["user_no"].ToString();

        if (long.TryParse(UserId, out long uno))
        {
            foreach (UserProfile friend in Managers.FriendData.GetFriendList())
            {
                if (friend.uno == uno)
                    return;
            }

            foreach (UserProfile friend in Managers.FriendData.GetSentList())
            {
                if (friend.uno == uno)
                    return;
            }

            addFriendButton.interactable = true;
        }

        string clanName = "";
        if (roomPlayer.ContainsKey("clan_name"))
        {
            clanName = roomPlayer["clan_name"].ToString();
        }

        if (string.IsNullOrEmpty(clanName))
        {
            txtClanName.gameObject.SetActive(false);
        }
        else
        {
            txtClanName.gameObject.SetActive(true);
            txtClanName.text = clanName;
        }

        int duoType = 0;
        if (roomPlayer.ContainsKey("duotype"))
        {
            duoType = roomPlayer["duotype"].Value<int>();
        }

        OnDuoUser(duoType > 0);
    }

    public void SetRankPoint(int rankPoint)
    {
        Sprite sprite = RankType.GetRankFromPoint(rankPoint).rank_resource;
        if(sprite != rankImage.sprite)
        {
            rankImage.transform.DOScale(1.1f, 0.3f).From().OnComplete(()=> {
                rankImage.sprite = sprite;
            });
            return;
        }

        rankImage.sprite = sprite;
    }
    public void OnAddFriend()
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "friend");
        data.AddField("op", 5);
        data.AddField("uno", UserId);

        SBWeb.SendPost("friend/friend", data, (response) =>
        {
            JObject root = (JObject)response;
            JToken resultCode = root["rs"];
            if (resultCode != null && resultCode.Type == JTokenType.Integer)
            {
                int rs = resultCode.Value<int>();
                if (rs == 0)
                {
                    PopupCanvas.Instance.ShowFadeText(StringManager.GetString("msg_fr_request_clear"));
                }
                else
                {
                    (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.FRIEND_POPUP) as FriendPopup).ShowErrorMessage(rs);
                }
            }
        });

        addFriendButton.interactable = false;
    }

    public void SetPointText(string point)
    {
        int ret = 0;
        if (!int.TryParse(point, out ret) || ret < 0)
        {
            txtPoint.color = Color.red;
        }

        txtPoint.text = point;
    }

    public void PlayMvpParticle()
    {
        if (isChaser)
            mvp_grid_cir_chase.Play();
        else
            mvp_grid_cir_sur.Play();

        mine_grid_rect.Play();
        mvpImage.SetActive(true);
    }

    public void OnExitUser(bool active)
    {
        exitImage.SetActive(active);
    }

    public void OnDuoUser(bool active)
    {
        duoImage.SetActive(active);
    }
}

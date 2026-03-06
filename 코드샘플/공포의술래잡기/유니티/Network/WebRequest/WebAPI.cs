//여기 코드들은 나중에 web API로 추출될 예정입니다.

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class SBWeb
{
    public class ResponseReward
    {
        public enum RandomRewardType { GOLD = 1, DIA = 2, ITEM = 3, CHARACTER = 4, EQUIP = 13 };
        public RandomRewardType Type { get; private set; }
        public int Id { get; private set; }
        public int Amount { get; private set; }
        public ResponseReward originReward = null;
        public ResponseReward(int type, int id, int amount, ResponseReward origin = null)
        {
            Type = (RandomRewardType)type;
            Id = id;
            Amount = amount;
            originReward = origin;
        }

        public void AddAmount(int count)
        {
            Amount += count;
        }
    };

    public delegate void RewardCallback(List<ResponseReward> rewards);
    public delegate void GameResultCallback(JArray gameResults);
    public static JToken GetResultData(JToken response)
    {
        JObject res = (JObject)response;
        if (res.ContainsKey("rs"))
        {
            int rs = res["rs"].Value<int>();
            if (rs == 0)
            {
                return res["result"];
            }
            else
            {
                switch (rs)
                {
                    case 1:
                        PopupCanvas.Instance.ShowFadeText("GENERIC_SERVER_FAIL");
                        break;
                    case 2:
                        PopupCanvas.Instance.ShowFadeText("SQL_ERROR");
                        break;
                    case 3:
                        PopupCanvas.Instance.ShowFadeText("NETWORK_ERROR");
                        break;
                    case 4:
                        PopupCanvas.Instance.ShowFadeText("SCRIPT_INCLUDE_FAIL");
                        break;
                    case 5:
                        PopupCanvas.Instance.ShowFadeText("REDIS_ERROR");
                        break;
                    case 6:
                        PopupCanvas.Instance.ShowFadeText("SERVER_BUSY");
                        break;

                    case 11:
                        PopupCanvas.Instance.ShowFadeText("PARAM_ERROR");
                        break;
                    case 12:
                        PopupCanvas.Instance.ShowFadeText("TARGET_NOT_EXISTS");
                        break;
                    case 13:
                        PopupCanvas.Instance.ShowFadeText("LESS_THAN_VERSION");
                        break;
                    case 14:
                        PopupCanvas.Instance.ShowFadeText("EXPIRED_DATE");
                        break;

                    case 99:
                        PopupCanvas.Instance.ShowFadeText("NOT_IMPLEMENTED");
                        break;

                    case 101:
                        PopupCanvas.Instance.ShowMessagePopup("SESSIONID_NOT_MATCH", () =>
                        {
                            UnityEngine.SceneManagement.SceneManager.LoadScene("Start");
                        });
                        break;
                    case 102:
                        PopupCanvas.Instance.ShowFadeText("REMOVED_ACCOUNT");
                        break;
                    case 103:
                        PopupCanvas.Instance.ShowFadeText("USER_BANNED");
                        break;

                    case 301:
                        PopupCanvas.Instance.ShowFadeText("NOT_ENOUGH_FOR_COST");
                        break;

                    case 302:
                        PopupCanvas.Instance.ShowFadeText("OUT_OF_STOCK");
                        break;

                    case 1001:
                        PopupCanvas.Instance.ShowFadeText("DATA_TABLE_NOT_EXISTS");
                        break;
                }
            }
        }
        return null;
    }

    public static bool IsResultOK(JToken response)
    {
        return (GetResultData(response) != null);
    }
    public static void OnResponseCheck(JToken response, Action cb)
    {
        if (IsResultOK(response))
            cb?.Invoke();
    }

    public static void GetLobbyData(Action cb = null)
    {
        SendPost("user/lobbydata", null, (response) =>
        {
            try
            {
                JObject in_app_sku = JObject.Parse(response["in_app_sku"].ToString());
                if (in_app_sku != null)
                {
                    if (IsResultOK(in_app_sku))
                    {
                        JToken res = GetResultData(in_app_sku);
                        ShopPackageGameData.SetIAPConstants(res);
                    }
                }
            }
            catch
            {
                Debug.LogError("====== Response user/lobbydata Error : in_app_sku");
            }

            try
            {
                JObject playdata_info = JObject.Parse(response["playdata/playdata_info"].ToString());
                if (playdata_info != null)
                {
                    if (IsResultOK(playdata_info))
                    {
                        JToken res = GetResultData(playdata_info);
                        Managers.UserData.PlayDataJToken(res);
                    }
                }
            }
            catch
            {
                Debug.LogError("====== Response user/lobbydata Error : playdata/playdata_info");
            }
            try
            {
                JObject friend_info = JObject.Parse(response["friend/friend"].ToString());
                if (friend_info != null)
                {
                    Managers.FriendData.SetRequestFriendInfo((JObject)friend_info);
                }
            }
            catch
            {
                Debug.LogError("====== Response user/lobbydata Error : friend/friend");
            }

            try
            {
                JObject rank_info = JObject.Parse(response["point/rank"].ToString());
                if (rank_info != null)
                {
                    OnResponseCheck(rank_info, () =>
                    {
                        var res = GetResultData(rank_info);
                        var mydata = res["my"];
                        Managers.UserData.UpdatePoint(mydata["point"]);
                        Managers.UserData.UpdateHightPoint(mydata["best_point"].Value<int>());
                    });
                }
            }
            catch
            {
                Debug.LogError("====== Response user/lobbydata Error : point/rank");
            }
            try
            {
                JObject rankreward_info = JObject.Parse(response["user/rank_reward"].ToString());
                if (rankreward_info != null)
                {
                    if (IsResultOK(rankreward_info))
                    {
                        JObject res = (JObject)GetResultData(rankreward_info);

                        (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.RANKREWARD_POPUP) as RankRewardPopup).SetData(res);
                    }
                }
            }
            catch
            {
                Debug.LogError("====== Response user/lobbydata Error : user/rank_reward");
            }
            try
            {
                JObject advertisement_info = JObject.Parse(response["user/advertisement"].ToString());
                if (advertisement_info != null)
                {
                    if (IsResultOK(advertisement_info))
                    {
                        JToken res = GetResultData(advertisement_info);
                        Managers.UserData.AdvertisementInfo(res);
                    }
                }
            }
            catch
            {
                Debug.LogError("====== Response user/lobbydata Error : user/advertisement");
            }
            try
            {
                JObject shop_history = JObject.Parse(response["shop/shop_history"].ToString());
                if (shop_history != null)
                {
                    if (IsResultOK(shop_history))
                    {
                        JToken res = GetResultData(shop_history);
                        Managers.UserData.SetMyShopInfo((JObject)res);
                    }
                }
            }
            catch
            {
                Debug.LogError("====== Response user/lobbydata Error : shop/shop_history");
            }

            var LobbyScene = Managers.Scene.CurrentScene as LobbyScene;
            if (LobbyScene != null)
            {
                try
                {
                    JObject quest_info = JObject.Parse(response["user/quest_info"].ToString());
                    if (quest_info != null)
                    {
                        if (IsResultOK(quest_info))
                        {
                            JToken res = GetResultData(quest_info);
                            LobbyScene.CheckQuestRedDot(res);
                        }
                    }
                }
                catch
                {
                    Debug.LogError("====== Response user/lobbydata Error : user/quest_info");
                }

                try
                {
                    JObject battle_pass = JObject.Parse(response["shop/battlepass"].ToString());
                    if (battle_pass != null)
                    {
                        if (IsResultOK(battle_pass))
                        {
                            JToken passInfo = GetResultData(battle_pass);
                            Managers.UserData.SetPassData(passInfo);

                            if (Managers.UserData.seasonData.CheckEnableReward())
                            {
                                var curScene = Managers.Scene.CurrentScene as LobbyScene;
                                if (curScene != null)
                                    curScene.OnRedDot(curScene.lobbyBtns[6].transform, true);
                            }
                            else
                            {
                                var curScene = Managers.Scene.CurrentScene as LobbyScene;
                                if (curScene != null)
                                    curScene.OnRedDot(curScene.lobbyBtns[6].transform, false);
                            }

                        }
                    }
                }
                catch
                {
                    Debug.LogError("====== Response user/lobbydata Error : shop/battlepass");
                }


                LobbyScene.RefreshBattlePassIcon();
            }
            try
            {
                JObject usermail_info = JObject.Parse(response["user/user_mail"].ToString());
                if (usermail_info != null)
                {
                    if (IsResultOK(usermail_info))
                    {
                        JToken res = GetResultData(usermail_info);
                        (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.MAIL_POPUP) as MailPopup).GetMailData(res);
                    }
                }
            }
            catch
            {
                Debug.LogError("====== Response user/lobbydata Error : user/user_mail");
            }

            try
            {
                JObject buffinfo = JObject.Parse(response["buffinfo"].ToString());
                if (buffinfo != null)
                {
                    if (IsResultOK(buffinfo))
                    {
                        JToken res = GetResultData(buffinfo);
                        Managers.UserData.UpdateBuff(res);
                    }
                }
            }
            catch
            {
                Debug.LogError("====== Response user/lobbydata Error : buffinfo");
            }
            cb.Invoke();
        });
    }

    public static void GetUserInfo(Action cb = null)
    {
        SendPost("user/myinfo", null, (response) =>
        {
            OnUserInfo(response);
            cb?.Invoke();
        });
    }

    public static void OnUserInfo(JToken response)
    {
        Managers.UserData.SetMyUserInfo(GetResultData(response));
    }

    public static void GetCharactersInfo(Action cb = null)
    {
        SendPost("character/infoall", null, (response) =>
        {
            Managers.UserData.SetMyCharacterInfo(GetResultData(response));
            cb?.Invoke();
        });
    }

    public static void GetCharacterInfo(int charID, Action cb = null)
    {
        WWWForm param = new WWWForm();

        param.AddField("character_uid", charID);

        SendPost("character/info", param, (response) =>
        {
            Managers.UserData.UpdateMyCharacter(GetResultData(response));
            cb?.Invoke();
        });
    }

    public static void GetItemsInfo(Action cb = null)
    {
        SendPost("user/iteminfoall", null, (response) =>
        {
            Managers.UserData.SetMyItemInfo(response["items"]);
            cb?.Invoke();
        });
    }

    public static void GetItemInfo(int itemID, Action cb = null)
    {
        WWWForm param = new WWWForm();

        param.AddField("item_uid", itemID);

        SendPost("user/iteminfo", param, (response) =>
        {
            Managers.UserData.UpdateMyItem(GetResultData(response));
            cb?.Invoke();
        });
    }

    public static void SetDefaultChaserCharacter(int char_id, Action cb = null)
    {
        WWWForm param = new WWWForm();

        param.AddField("character_uid", char_id);

        SendPost("user/setchaser", param, (response) =>
        {
            if (IsResultOK(response))
            {
                OnUserInfo(response);
                PopupCanvas.Instance.ShowFadeText(StringManager.GetString("캐릭터변경알림"));
                cb?.Invoke();
            }
        });
    }

    public static void SetDefaultSuvivorCharacter(int char_id, Action cb = null)
    {
        WWWForm param = new WWWForm();

        param.AddField("character_uid", char_id);

        SendPost("user/setsurvivor", param, (response) =>
        {
            if (IsResultOK(response))
            {
                OnUserInfo(response);
                PopupCanvas.Instance.ShowFadeText(StringManager.GetString("캐릭터변경알림"));
                cb?.Invoke();
            }
        });
    }

    public static void CharacterExpUp(int char_id, int smallPostion, int mediumPostion, int largePostion, Action cb = null, Action failcb = null)
    {
        if (smallPostion + mediumPostion + largePostion <= 0)
        {
            SBDebug.Log("아이템 아무것도 선택안했음");
            return;
        }

        if (smallPostion > 0)
        {
            int itemNO = 1;
            if (Managers.UserData.GetMyItemCount(itemNO) < smallPostion)
            {
                PopupCanvas.Instance.ShowFadeText(StringManager.GetString("아이템부족"), ItemGameData.GetItemData(itemNO) != null ? ItemGameData.GetItemData(itemNO).GetName() : "");
                //SBDebug.LogError($"아이템 부족. item_no : {itemNO}");
                return;
            }
        }
        if (mediumPostion > 0)
        {
            int itemNO = 2;
            if (Managers.UserData.GetMyItemCount(itemNO) < mediumPostion)
            {
                PopupCanvas.Instance.ShowFadeText(StringManager.GetString("아이템부족"), ItemGameData.GetItemData(itemNO) != null ? ItemGameData.GetItemData(itemNO).GetName() : "");
                //SBDebug.LogError($"아이템 부족. item_no : {itemNO}");
                return;
            }
        }
        if (largePostion > 0)
        {
            int itemNO = 3;
            if (Managers.UserData.GetMyItemCount(itemNO) < largePostion)
            {
                PopupCanvas.Instance.ShowFadeText(StringManager.GetString("아이템부족"), ItemGameData.GetItemData(itemNO) != null ? ItemGameData.GetItemData(itemNO).GetName() : "");
                //SBDebug.LogError($"아이템 부족. item_no : {itemNO}");
                return;
            }
        }

        WWWForm param = new WWWForm();

        param.AddField("character_uid", char_id);
        param.AddField("small", smallPostion);
        param.AddField("medium", mediumPostion);
        param.AddField("large", largePostion);

        SendPost("character/use_expitem", param, (response) =>
        {
            if (IsResultOK(response))
            {
                JToken res = GetResultData(response);
                Managers.UserData.UpdateMyCharacter(res["character"]);
                Managers.UserData.SetMyItemInfo(res["items"]);
                Managers.UserData.SetMyUserInfo(res["user"]);
                cb?.Invoke();
            }
            else
                failcb.Invoke();
        });
    }

    public static void CharacterSkillUp(int char_id, Action cb = null)
    {
        WWWForm param = new WWWForm();

        param.AddField("character_uid", char_id);

        SendPost("character/levelup_skill", param, (response) =>
        {
            if (IsResultOK(response))
            {
                JToken res = GetResultData(response);
                Managers.UserData.UpdateMyCharacter(res["character"]);
                Managers.UserData.SetMyItemInfo(res["items"]);
                Managers.UserData.SetMyUserInfo(res["user"]);
                cb?.Invoke();
            }
        });
    }
    public static void TryCharacterTalent(int char_id, int slot, SuccessCallback cb = null, bool special = false)
    {
        WWWForm param = new WWWForm();
        param.AddField("character_uid", char_id);
        param.AddField("talent_index", slot);
        param.AddField("special", special ? 1 : 0);

        SendPost("character/talent", param, (response) =>
        {
            JToken res = GetResultData(response);
            if (IsResultOK(response))
            {
                Managers.UserData.UpdateMyCharacter(res["character"]);
                Managers.UserData.SetMyItemInfo(res["items"]);
                Managers.UserData.SetMyUserInfo(res["user"]);
                cb?.Invoke(res);
            }
        });
    }

    public static void ApplyCharacterTalent(int char_id, int slot, int pick, SuccessCallback cb = null)
    {
        WWWForm param = new WWWForm();
        param.AddField("character_uid", char_id);
        param.AddField("talent_index", slot);
        param.AddField("talent_pick", pick);

        SendPost("character/talent", param, (response) =>
        {
            JToken res = GetResultData(response);
            if (IsResultOK(response))
            {
                Managers.UserData.UpdateMyCharacter(res["character"]);
                cb?.Invoke(res);
            }
        });
    }

    public static void UpgradeEnchantCharacter(int char_id, Action cb = null)
    {
        WWWForm param = new WWWForm();

        param.AddField("character_uid", char_id);
        SendPost("character/enchant_char", param, (response) =>
        {
            if (IsResultOK(response))
            {
                JToken res = GetResultData(response);
                Managers.UserData.UpdateMyCharacter(res["character"]);
                Managers.UserData.SetMyItemInfo(res["items"]);
                Managers.UserData.SetMyUserInfo(res["user"]);
                cb?.Invoke();

            }
        });
    }

#if UNITY_EDITOR || SB_TEST
    public static void OnObtainCharacter(int charID, int enchan, int skill, Action cb = null)
    {
        WWWForm param = new WWWForm();

        string query = string.Format("INSERT INTO characters_{0} (user_no, character_uid, exp, level, enhance_step, skill) VALUES ({1}, {2}, {3}, {4}, {5}, {6}) ON DUPLICATE KEY UPDATE enhance_step={5}, skill={6}", Managers.UserData.MyUserID.ToString().Last(), Managers.UserData.MyUserID, charID, 0, 1, enchan, skill);

        param.AddField("query", query);
        param.AddField("type", "row");

        SendPost("dev/chore", param, (response) =>
        {
            OnResponseCheck(response,
                () => { GetCharactersInfo(cb); });
        });
    }
    public static void OnLvCheat(int charID, int lv, Action cb = null)
    {
        WWWForm param = new WWWForm();

        string query = string.Format("INSERT INTO characters_{0} (user_no, character_uid, level) VALUES ({1}, {2}, {3}) ON DUPLICATE KEY UPDATE level={3}", Managers.UserData.MyUserID.ToString().Last(), Managers.UserData.MyUserID, charID, lv);

        param.AddField("query", query);
        param.AddField("type", "row");

        SendPost("dev/chore", param, (response) =>
        {
            OnResponseCheck(response,
                () => { GetCharactersInfo(cb); });
        });
    }

    public static void OnGetAllMaxEquips(int[] itemNos, Action cb = null)
    {
        WWWForm param = new WWWForm();

        string query = string.Format("INSERT INTO equipment_{0} (user_no, item_no, level, exp) VALUES ", Managers.UserData.MyUserID.ToString().Last());
        foreach (int itemNo in itemNos)
        {
            query += string.Format("({0}, {1}, {2}, {3})", Managers.UserData.MyUserID, itemNo, 40, 78260);
            if (itemNo != itemNos[itemNos.Length - 1])
            {
                query += ",";
            }
        }

        query += ";";

        param.AddField("query", query);
        param.AddField("type", "query");

        SendPost("dev/chore", param, (response) =>
        {
            OnResponseCheck(response,
                () => { GetItemsInfo(cb); });
        });
    }
#endif//SB_TEST 

    public static void GetRank(SuccessCallback cb = null)
    {
        WWWForm param = new WWWForm();

        SendPost("point/rank", null, (response) =>
        {
            OnResponseCheck(response, () =>
            {
                if (IsResultOK(response))
                {
                    Managers.UserData.SetDayRank(GetResultData(response));
                    cb?.Invoke(GetResultData(response));
                }
            });
        });
    }

    public static void GetClanRank(SuccessCallback cb = null)
    {
        WWWForm param = new WWWForm();

        SendPost("clan/clan_rank", null, (response) =>
        {
            OnResponseCheck(response, () =>
            {
                if (IsResultOK(response))
                {
                    Managers.UserData.SetClanRank(GetResultData(response));
                    cb?.Invoke(GetResultData(response));
                }
            });
        });
    }


    public static void OnBuy(int goodsID, int count, RewardCallback cb = null)
    {
        WWWForm param = new WWWForm();

        param.AddField("goods_id", goodsID);
        param.AddField("count", count);

        SendPost("shop/buy", param, (response) =>
        {
            if (IsResultOK(response))
            {
                JToken res = GetResultData(response);

                List<ResponseReward> rewards = new List<ResponseReward>();
                foreach (JToken reward in (JArray)res["rewards"])
                {
                    JArray rewardData = (JArray)reward;
                    if (!rewardData.HasValues)
                        continue;
                    rewards.Add(new ResponseReward(rewardData[0].Value<int>(), rewardData[1].Value<int>(), rewardData[2].Value<int>()));
                }

                Managers.UserData.UpdateMyCharacter(res["character"]);
                Managers.UserData.SetMyItemInfo(res["items"]);
                Managers.UserData.SetMyUserInfo(res["user"]);
                Managers.UserData.AdvertisementInfo(res);
                cb?.Invoke(rewards);
            }
        });
    }

    public static void OnBuyRandomItem(int goodsID, RewardCallback cb = null)
    {
        WWWForm param = new WWWForm();

        param.AddField("goods_id", goodsID);

        SendPost("shop/random_buy", param, (response) =>
        {
            if (IsResultOK(response))
            {
                JToken res = GetResultData(response);

                List<ResponseReward> rewards = new List<ResponseReward>();
                foreach (JToken reward in (JArray)res["rewards"])
                {
                    JArray rewardData = (JArray)reward;
                    rewards.Add(new ResponseReward(rewardData[0].Value<int>(), rewardData[1].Value<int>(), rewardData[2].Value<int>()));
                }

                Managers.UserData.UpdateMyCharacter(res["character"]);
                Managers.UserData.SetMyItemInfo(res["items"]);
                Managers.UserData.SetMyUserInfo(res["user"]);
                cb?.Invoke(rewards);
            }
        });
    }


    public static void OnRandomBox(int id, int type, RewardCallback cb = null)
    {
        WWWForm param = new WWWForm();

        param.AddField("gacha_base", id);
        param.AddField("gacha_type", type);

        SendPost("gacha/exec", param, (response) =>
        {
            if (IsResultOK(response))
            {
                JToken res = GetResultData(response);

                List<ResponseReward> rewards = new List<ResponseReward>();
                JArray rewardArray = (JArray)res["rewards"];
                JArray pickArray = (JArray)res["picks"];
                for (int i = 0; i < rewardArray.Count; i++)
                {
                    JArray rewardData = (JArray)rewardArray[i];
                    JArray pickData = (JArray)pickArray[i];

                    ResponseReward originReward = new ResponseReward(pickData[0].Value<int>(), pickData[1].Value<int>(), pickData[2].Value<int>());
                    rewards.Add(new ResponseReward(rewardData[0].Value<int>(), rewardData[1].Value<int>(), rewardData[2].Value<int>(), originReward));
                }

                Managers.UserData.UpdateMyCharacter(res["character"]);
                Managers.UserData.SetMyItemInfo(res["items"]);
                Managers.UserData.SetMyUserInfo(res["user"]);
                Managers.UserData.SetLimitedIAP(res["limit"]);
                Managers.UserData.AdvertisementInfo(res);

                cb?.Invoke(rewards);
            }
        });
    }

    public static void GetGameResult(string game_uid, GameResultCallback cb = null)
    {
        WWWForm param = new WWWForm();

        param.AddField("game_uid", game_uid);
        param.AddField("count", 1);

        SendPost("point/getresult", param, (response) =>
        {
            if (IsResultOK(response))
            {
                JToken res = GetResultData(response);

                Managers.UserData.UpdateMyCharacter(res["character"]);
                Managers.UserData.SetMyUserInfo(res["user"]);
                Managers.UserData.SetMyItemInfo(res["items"]);

                if (res["game_result"] != null && res["game_result"].Type == JTokenType.Array)
                    cb?.Invoke((JArray)res["game_result"]);
            }
        });
    }


    public static void GetGameLogDB(SuccessCallback cb = null)
    {
        WWWForm param = new WWWForm();

        param.AddField("count", 10);

        SendPost("point/getresult", param, (response) =>
        {
            if (IsResultOK(response))
            {
                JToken res = GetResultData(response);

                Managers.UserData.UpdateMyCharacter(res["character"]);
                Managers.UserData.SetMyUserInfo(res["user"]);

                cb?.Invoke(res["game_result"]);
            }
        });
    }


    public static void CheckDodge(Action dodgeCB, Action cb)
    {
        SendPost("user/checkdodge", new WWWForm(), (response) =>
        {
            if (IsResultOK(response))
            {
                JObject res = (JObject)GetResultData(response);

                if (res != null && res.ContainsKey("dodge"))
                {
                    if (res["dodge"].Value<int>() > 0)
                        dodgeCB.Invoke();
                }
            }

            cb.Invoke();
        });
    }
    public static void TradeSoulStone(Dictionary<int, int> id, RewardCallback cb = null)
    {
        WWWForm param = new WWWForm();
        if (id != null && id.Count > 0)
        {
            param.AddField("card_id_dic", string.Join("#", id));
        }
        SendPost("shop/exchange_soulstone", param, (response) =>
        {
            if (IsResultOK(response))
            {
                if (IsResultOK(response))
                {
                    JObject res = (JObject)GetResultData(response);

                    JArray rew = (JArray)res["rewards"];
                    List<ResponseReward> rewards = new List<ResponseReward>();
                    if (rew != null && rew.Count != 0)
                    {
                        JArray arr = (JArray)rew;
                        foreach (JToken reward_array in arr)
                        {
                            foreach (JToken reward in reward_array)
                            {
                                JArray rewardData = (JArray)reward;

                                bool newReward = true;
                                foreach (ResponseReward gainReward in rewards)
                                {
                                    if ((int)gainReward.Type == rewardData[0].Value<int>() && (int)gainReward.Id == rewardData[1].Value<int>())
                                    {
                                        gainReward.AddAmount(rewardData[2].Value<int>());
                                        newReward = false;
                                    }
                                }
                                if (newReward)
                                    rewards.Add(new ResponseReward(rewardData[0].Value<int>(), rewardData[1].Value<int>(), rewardData[2].Value<int>()));
                            }
                        }

                        string itemText = "";
                        foreach (var reward in rewards)
                        {
                            switch (reward.Type)
                            {
                                case SBWeb.ResponseReward.RandomRewardType.GOLD:
                                    itemText += "골드 " + reward.Amount + "개";
                                    break;
                                case SBWeb.ResponseReward.RandomRewardType.DIA:
                                    itemText += "다이아 " + reward.Amount + "개";
                                    break;
                                case SBWeb.ResponseReward.RandomRewardType.ITEM:
                                    itemText += ItemGameData.GetItemData(reward.Id).GetName() + " " + reward.Amount + "개";
                                    break;
                                case SBWeb.ResponseReward.RandomRewardType.CHARACTER:
                                    itemText += CharacterGameData.GetCharacterData(reward.Id).GetName();
                                    break;
                            }

                            itemText += "\n";
                        }

                        //PopupCanvas.Instance.ShowRewardResult(rewards);
                    }
                    Managers.UserData.SetMyItemInfo(res["items"]);
                    cb?.Invoke(rewards);
                }
            }
        });
    }

    public static void TradeEquipToItem(List<int> id, RewardCallback cb = null)
    {
        WWWForm param = new WWWForm();
        if (id != null && id.Count > 0)
        {
            param.AddField("equip_id_list", string.Join(",", id));
        }
        SendPost("shop/exchange_equipgold", param, (response) =>
        {
            if (IsResultOK(response))
            {
                if (IsResultOK(response))
                {
                    JObject res = (JObject)GetResultData(response);

                    JArray rew = (JArray)res["rewards"];
                    List<ResponseReward> rewards = new List<ResponseReward>();
                    if (rew != null && rew.Count != 0)
                    {
                        JArray arr = (JArray)rew;
                        foreach (JToken reward_array in arr)
                        {
                            foreach (JToken reward in reward_array)
                            {
                                JArray rewardData = (JArray)reward;

                                bool newReward = true;
                                foreach (ResponseReward gainReward in rewards)
                                {
                                    if ((int)gainReward.Type == rewardData[0].Value<int>() && (int)gainReward.Id == rewardData[1].Value<int>())
                                    {
                                        gainReward.AddAmount(rewardData[2].Value<int>());
                                        newReward = false;
                                    }
                                }
                                if (newReward)
                                    rewards.Add(new ResponseReward(rewardData[0].Value<int>(), rewardData[1].Value<int>(), rewardData[2].Value<int>()));
                            }
                        }

                        string itemText = "";
                        foreach (var reward in rewards)
                        {
                            switch (reward.Type)
                            {
                                case SBWeb.ResponseReward.RandomRewardType.GOLD:
                                    itemText += "골드 " + reward.Amount + "개";
                                    break;
                                case SBWeb.ResponseReward.RandomRewardType.DIA:
                                    itemText += "다이아 " + reward.Amount + "개";
                                    break;
                                case SBWeb.ResponseReward.RandomRewardType.ITEM:
                                    itemText += ItemGameData.GetItemData(reward.Id).GetName() + " " + reward.Amount + "개";
                                    break;
                                case SBWeb.ResponseReward.RandomRewardType.CHARACTER:
                                    itemText += CharacterGameData.GetCharacterData(reward.Id).GetName();
                                    break;
                            }

                            itemText += "\n";
                        }

                        //PopupCanvas.Instance.ShowRewardResult(rewards);
                    }
                    Managers.UserData.SetMyUserInfo(res["user"]);
                    Managers.UserData.SetMyEquipItems(res["equips"]);
                    cb?.Invoke(rewards);
                }
            }
        });

    }
    public static void GetRankReward(List<int> target, SuccessCallback cb = null)
    {
        WWWForm param = new WWWForm();
        if (target != null && target.Count > 0)
        {
            param.AddField("reward_rank", string.Join(",", target));
        }

        SendPost("user/rank_reward", param, (response) =>
        {
            if (IsResultOK(response))
            {
                JObject res = (JObject)GetResultData(response);

                JArray rew = (JArray)res["rewards"];
                if (rew != null && rew.Count != 0)
                {
                    JArray arr = rew;
                    List<ResponseReward> rewards = new List<ResponseReward>();
                    foreach (JToken reward_array in arr)
                    {
                        foreach (JToken reward in reward_array)
                        {
                            JArray rewardData = (JArray)reward;

                            bool newReward = true;
                            foreach (ResponseReward gainReward in rewards)
                            {
                                if ((int)gainReward.Type == rewardData[0].Value<int>() && (int)gainReward.Id == rewardData[1].Value<int>())
                                {
                                    gainReward.AddAmount(rewardData[2].Value<int>());
                                    newReward = false;
                                }
                            }
                            if (newReward)
                                rewards.Add(new ResponseReward(rewardData[0].Value<int>(), rewardData[1].Value<int>(), rewardData[2].Value<int>()));
                        }
                    }

                    string itemText = "";
                    foreach (var reward in rewards)
                    {
                        switch (reward.Type)
                        {
                            case SBWeb.ResponseReward.RandomRewardType.GOLD:
                                itemText += "골드 " + reward.Amount + "개";
                                break;
                            case SBWeb.ResponseReward.RandomRewardType.DIA:
                                itemText += "다이아 " + reward.Amount + "개";
                                break;
                            case SBWeb.ResponseReward.RandomRewardType.ITEM:
                                itemText += ItemGameData.GetItemData(reward.Id).GetName() + " " + reward.Amount + "개";
                                break;
                            case SBWeb.ResponseReward.RandomRewardType.CHARACTER:
                                itemText += CharacterGameData.GetCharacterData(reward.Id).GetName();
                                break;
                        }

                        itemText += "\n";
                    }

                    PopupCanvas.Instance.ShowRewardResult(rewards);
                }

                Managers.UserData.SetMyUserInfo(res["user"]);
                Managers.UserData.UpdateMyCharacter(res["character"]);
                Managers.UserData.SetMyItemInfo(res["items"]);
                cb.Invoke(res);
            }
        });
    }


    public static void GetGachaRate(int gacha_base, SuccessCallback cb = null)
    {
        WWWForm param = new WWWForm();
        param.AddField("gacha_base", gacha_base);

        SendPost("gacha/rate", param, (response) =>
        {
            JToken res = GetResultData(response);

            if (IsResultOK(response))
            {
                cb?.Invoke(res["gacha_rate"]);
            }
        });
    }

    public static void GetRandomShopItems(int menuID, SuccessCallback cb = null)
    {
        WWWForm data = new WWWForm();
        data.AddField("menu_id", menuID);

        SendPost("shop/random_list", data, (response) =>
        {
            JToken res = GetResultData(response);

            if (IsResultOK(response))
            {
                cb?.Invoke(res);
            }
        });
    }

    public static void RefreshRandomShopItems(int menuID, SuccessCallback cb = null)
    {
        WWWForm data = new WWWForm();
        data.AddField("menu_id", menuID);
        data.AddField("use_money", 1);

        SendPost("shop/random_list", data, (response) =>
        {
            JToken res = GetResultData(response);

            if (IsResultOK(response))
            {
                PopupCanvas.Instance.ShowFadeText("랜덤상품갱신");
                if (((JObject)res).ContainsKey("user"))
                    Managers.UserData.SetMyUserInfo(res["user"]);
                cb?.Invoke(res);
            }
        });
    }

    public void OnResponseFriend(int op, JObject jObject)
    {
        switch (op)
        {
            case 1:
                {
                    List<UserProfile> friends = Managers.FriendData.GetFriendList();
                    Managers.FriendData.DeleteUserProfiles(friends);
                    JArray users = (JArray)jObject["list"];
                    foreach (JToken user in users)
                    {
                        JObject userObject = (JObject)user;
                        UserProfile newUser = Managers.FriendData.UpdateUserProfile(userObject, UserProfile.FriendType.FRIEND);
                        foreach (UserProfile u in friends)
                        {
                            if (u.uno == newUser.uno)
                            {
                                if (newUser.last_update < u.last_update)
                                    newUser.last_update = u.last_update;
                                newUser.lastMessage = u.lastMessage;
                            }
                        }
                    }
                }
                break;
            case 2:
                {
                    List<UserProfile> sents = Managers.FriendData.GetSentList();
                    Managers.FriendData.DeleteUserProfiles(sents);
                    JArray users = (JArray)jObject["list"];
                    foreach (JToken user in users)
                    {
                        JObject userObject = (JObject)user;
                        Managers.FriendData.UpdateUserProfile(userObject, UserProfile.FriendType.SENT);
                    }
                }
                break;
            case 3:
                {
                    List<UserProfile> takens = Managers.FriendData.GetTakenList();
                    Managers.FriendData.DeleteUserProfiles(takens);
                    JArray users = (JArray)jObject["list"];
                    foreach (JToken user in users)
                    {
                        JObject userObject = (JObject)user;
                        Managers.FriendData.UpdateUserProfile(userObject, UserProfile.FriendType.TAKEN);
                    }
                }
                break;
            case 12: //FRIEND_LIST_UPDATE 
                {
                    uint nf = 0;
                    uint rv = 0;
                    JToken friend = jObject["friend"];
                    if (friend != null)
                    {
                        if (friend.Value<uint>() > 0)
                        {
                            nf = friend.Value<uint>();
                        }
                    }
                    JToken rcv = jObject["rcv"];
                    if (rcv != null)
                    {
                        if (rcv.Value<uint>() > 0)
                        {
                            rv = rcv.Value<uint>();
                        }
                    }

                    Managers.FriendData.SetNewFriendCount(nf);
                    Managers.FriendData.SetNewRecivedCount(rv);
                }
                break;
            case 13:
                {
                    List<UserProfile> recommands = Managers.FriendData.GetRecommandList();
                    Managers.FriendData.DeleteUserProfiles(recommands);
                    JArray users = (JArray)jObject["list"];
                    foreach (JToken user in users)
                    {
                        JObject userObject = (JObject)user;
                        Managers.FriendData.UpdateUserProfile(userObject, UserProfile.FriendType.RECOMMAND);
                    }
                }
                break;
        }
    }


    public static void GetPlayDataInfo(SuccessCallback cb = null)
    {
        SendPost("playdata/playdata_info", null, (response) =>
        {
            if (IsResultOK(response))
            {
                JToken res = GetResultData(response);
                Managers.UserData.PlayDataJToken(res);
            }
        });
    }
    public static void GetMyQuestDB(SuccessCallback cb = null)
    {
        SendPost("user/quest_info", null, (response) =>
        {
            JToken res = GetResultData(response);
            if (IsResultOK(response))
            {
                cb?.Invoke(res);

            }
        });
    }

    public static void GetQuestReward(List<int> target, SuccessCallback cb = null)
    {
        WWWForm param = new WWWForm();
        if (target != null && target.Count > 0)
        {
            param.AddField("reward_quest", string.Join(",", target));
        }

        SendPost("user/quest_reward", param, (response) =>
        {
            JObject temp = (JObject)response;
            if (temp.ContainsKey("quest_clear"))
            {
                cb?.Invoke(GetResultData(response));
                return;
            }

            if (IsResultOK(response))
            {
                JObject res = (JObject)GetResultData(response);

                JArray rew = (JArray)res["rewards"];
                if (rew != null && rew.Count != 0)
                {
                    JArray arr = (JArray)rew;
                    List<ResponseReward> rewards = new List<ResponseReward>();
                    foreach (JToken reward_array in arr)
                    {
                        foreach (JToken reward in reward_array)
                        {
                            JArray rewardData = (JArray)reward;

                            bool newReward = true;
                            foreach (ResponseReward gainReward in rewards)
                            {
                                if ((int)gainReward.Type == rewardData[0].Value<int>() && (int)gainReward.Id == rewardData[1].Value<int>())
                                {
                                    gainReward.AddAmount(rewardData[2].Value<int>());
                                    newReward = false;
                                }
                            }
                            if (newReward)
                                rewards.Add(new ResponseReward(rewardData[0].Value<int>(), rewardData[1].Value<int>(), rewardData[2].Value<int>()));
                        }
                    }

                    PopupCanvas.Instance.ShowRewardResult(rewards);
                }

                Managers.UserData.UpdateMyCharacter(res["character"]);
                Managers.UserData.SetMyItemInfo(res["items"]);
                Managers.UserData.SetMyUserInfo(res["user"]);

                cb.Invoke(res);

            }
        });
    }
    public static void GetUserMailDB(SuccessCallback cb = null)
    {
        SendPost("user/user_mail", null, (response) =>
        {
            JToken res = GetResultData(response);
            if (IsResultOK(response))
            {
                cb?.Invoke(res);
            }
        });
    }
    public static void GetUserMailReward(List<int> target, SuccessCallback cb = null)
    {
        WWWForm param = new WWWForm();
        if (target != null && target.Count > 0)
        {
            param.AddField("reward_mail", string.Join(",", target));
        }

        SendPost("user/user_mailReward", param, (response) =>
        {
            JToken res = GetResultData(response);
            if (IsResultOK(response))
            {
                JArray rew = (JArray)res["rewards"];
                if (rew != null && rew.Count != 0)
                {
                    JArray arr = (JArray)rew;
                    List<ResponseReward> rewards = new List<ResponseReward>();
                    foreach (JToken reward_array in arr)
                    {
                        foreach (JToken reward in reward_array)
                        {
                            JArray rewardData = (JArray)reward;

                            bool newReward = true;
                            foreach (ResponseReward gainReward in rewards)
                            {
                                if ((int)gainReward.Type == rewardData[0].Value<int>() && (int)gainReward.Id == rewardData[1].Value<int>())
                                {
                                    gainReward.AddAmount(rewardData[2].Value<int>());
                                    newReward = false;
                                }
                            }
                            if (newReward)
                                rewards.Add(new ResponseReward(rewardData[0].Value<int>(), rewardData[1].Value<int>(), rewardData[2].Value<int>()));
                        }
                    }

                    string itemText = "";
                    foreach (var reward in rewards)
                    {
                        switch (reward.Type)
                        {
                            case SBWeb.ResponseReward.RandomRewardType.GOLD:
                                itemText += "골드 " + reward.Amount + "개";
                                break;
                            case SBWeb.ResponseReward.RandomRewardType.DIA:
                                itemText += "다이아 " + reward.Amount + "개";
                                break;
                            case SBWeb.ResponseReward.RandomRewardType.ITEM:
                                itemText += ItemGameData.GetItemData(reward.Id).GetName() + " " + reward.Amount + "개";
                                break;
                            case SBWeb.ResponseReward.RandomRewardType.CHARACTER:
                                itemText += CharacterGameData.GetCharacterData(reward.Id).GetName();
                                break;
                        }

                        itemText += "\n";
                    }

                    PopupCanvas.Instance.ShowRewardResult(rewards);
                }

                Managers.UserData.UpdateMyCharacter(res["character"]);
                Managers.UserData.SetMyItemInfo(res["items"]);
                Managers.UserData.SetMyUserInfo(res["user"]);
                cb?.Invoke(res["mailList"]);
            }
        });
    }

    public static void GetAttendanceInfo(int attendance_id, SuccessCallback cb = null)
    {
        WWWForm param = new WWWForm();
        param.AddField("type", attendance_id);

        SendPost("user/attendance", param, (response) =>
        {
            JToken res = GetResultData(response);
            if (IsResultOK(response))
            {
                Managers.UserData.UpdateMyCharacter(res["character"]);
                Managers.UserData.SetMyItemInfo(res["items"]);
                Managers.UserData.SetMyUserInfo(res["user"]);
                cb?.Invoke(res);
            }
        });
    }

    public static void GetAttendanceInfoChristmas22(SuccessCallback cb = null)
    {
        WWWForm param = new WWWForm();

        SendPost("user/attendance_xmas", param, (response) =>
        {
            JToken res = GetResultData(response);
            if (IsResultOK(response))
            {
                Managers.UserData.UpdateMyCharacter(res["character"]);
                Managers.UserData.SetMyItemInfo(res["items"]);
                Managers.UserData.SetMyUserInfo(res["user"]);
                cb?.Invoke(res);
            }
        });
    }
    public static void GetAttendanceMonthInfo(SuccessCallback cb = null)
    {
        WWWForm param = new WWWForm();

        SendPost("user/attendance_month", param, (response) =>
        {
            JToken res = GetResultData(response);
            if (IsResultOK(response))
            {
                Managers.UserData.UpdateMyCharacter(res["character"]);
                Managers.UserData.SetMyItemInfo(res["items"]);
                Managers.UserData.SetMyUserInfo(res["user"]);
                cb?.Invoke(res);
            }
        });
    }

    public static void GetPassData(SuccessCallback cb = null)
    {
        GetPassReward(0, 0, cb);
    }

    public static void GetPassReward(int level, int vip, SuccessCallback cb = null)
    {
        WWWForm param = new WWWForm();
        param.AddField("level", level);
        param.AddField("vip", vip);
        param.AddField("all", 0);
        SendPost("shop/battlepass", param, (response) =>
        {
            JToken res = GetResultData(response);
            if (IsResultOK(response))
            {
                Managers.UserData.UpdateMyCharacter(res["character"]);
                Managers.UserData.SetMyItemInfo(res["items"]);
                Managers.UserData.SetMyUserInfo(res["user"]);

                cb?.Invoke(res);

                if (Managers.UserData.seasonData.CheckEnableReward())
                {
                    var curScene = Managers.Scene.CurrentScene as LobbyScene;
                    if (curScene != null)
                        curScene.OnRedDot(curScene.lobbyBtns[6].transform, true);
                }
                else
                {
                    var curScene = Managers.Scene.CurrentScene as LobbyScene;
                    if (curScene != null)
                        curScene.OnRedDot(curScene.lobbyBtns[6].transform, false);
                }
            }
        });
    }

    public static void GetPassRewards(SuccessCallback cb = null)
    {
        WWWForm param = new WWWForm();
        param.AddField("all", 1);

        SendPost("shop/battlepass", param, (response) =>
        {
            JToken res = GetResultData(response);
            if (IsResultOK(response))
            {
                Managers.UserData.UpdateMyCharacter(res["character"]);
                Managers.UserData.SetMyItemInfo(res["items"]);
                Managers.UserData.SetMyUserInfo(res["user"]);

                cb?.Invoke(res);

                if (Managers.UserData.seasonData.CheckEnableReward())
                {
                    var curScene = Managers.Scene.CurrentScene as LobbyScene;
                    if (curScene != null)
                        curScene.OnRedDot(curScene.lobbyBtns[6].transform, true);
                }
                else
                {
                    var curScene = Managers.Scene.CurrentScene as LobbyScene;
                    if (curScene != null)
                        curScene.OnRedDot(curScene.lobbyBtns[6].transform, false);
                }
            }
        });
    }

    public static void UseRandomBox(int itemID, int count, SuccessCallback cb = null)
    {
        WWWForm param = new WWWForm();
        param.AddField("item_no", itemID);
        param.AddField("cnt", count);

        SendPost("user/item_use", param, (response) =>
        {
            JToken res = GetResultData(response);
            if (IsResultOK(response))
            {
                Managers.UserData.UpdateMyCharacter(res["character"]);
                Managers.UserData.SetMyItemInfo(res["items"]);
                Managers.UserData.SetMyUserInfo(res["user"]);
                Managers.UserData.SetLimitedIAP(res["limit"]);
                cb?.Invoke(res);
            }
        });
    }

    public static void UseChoiceBox(int itemID, string selected, SuccessCallback cb = null)
    {
        WWWForm param = new WWWForm();
        param.AddField("item_no", itemID);
        param.AddField("selected", selected);

        SendPost("user/item_choice", param, (response) =>
        {
            JToken res = GetResultData(response);
            if (IsResultOK(response))
            {
                Managers.UserData.UpdateMyCharacter(res["character"]);
                Managers.UserData.SetMyItemInfo(res["items"]);
                Managers.UserData.SetMyUserInfo(res["user"]);

                cb?.Invoke(res);
            }
        });
    }

    public static void RequestSubscribeReward(int pid, SuccessCallback cb = null)
    {
        WWWForm param = new WWWForm();
        param.AddField("prod", pid);
        param.AddField("op", 7);

        SendPost("shop/iap", param, (response) =>
        {
            JToken res = GetResultData(response);
            if (IsResultOK(response))
            {
                cb?.Invoke(res);
            }
        });
    }

    public static void TryBingo(int try_bingo, SuccessCallback cb = null)
    {
        WWWForm param = new WWWForm();
        param.AddField("try_bingo", try_bingo);

        SendPost("event/halloween22", param, (response) =>
        {
            JToken res = GetResultData(response);
            if (response["rewards"] != null)
            {
                if (IsResultOK(response))
                {
                    JObject j_obj = (JObject)response;

                    JArray rew = (JArray)j_obj["rewards"];
                    if (rew != null && rew.Count != 0)
                    {
                        JArray arr = (JArray)rew;
                        List<ResponseReward> rewards = new List<ResponseReward>();
                        foreach (JToken reward_array in arr)
                        {
                            foreach (JToken reward in reward_array)
                            {
                                JArray rewardData = (JArray)reward;

                                bool newReward = true;
                                foreach (ResponseReward gainReward in rewards)
                                {
                                    if ((int)gainReward.Type == rewardData[0].Value<int>() && (int)gainReward.Id == rewardData[1].Value<int>())
                                    {
                                        gainReward.AddAmount(rewardData[2].Value<int>());
                                        newReward = false;
                                    }
                                }
                                if (newReward)
                                    rewards.Add(new ResponseReward(rewardData[0].Value<int>(), rewardData[1].Value<int>(), rewardData[2].Value<int>()));
                            }
                        }
                        var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.HALLOWEENEVENT_POPUP) as HalloweenEventPopup;
                        if (popup.IsOpening())
                        {

                            popup.PlayAnim(rewards, popup.CheckLine(res["line_clear"].Value<int>()), popup.CheckStage(res["cur_stage"].Value<int>()), response["cur_pick"][0].Value<int>(), response["cur_pick"][1].Value<int>());
                        }
                    }
                }
                else
                {
                    PopupCanvas.Instance.ShowFadeText(StringManager.GetString("msg_candy_lack"));
                    return;
                }
                Managers.UserData.UpdateMyCharacter(response["character"]);
                Managers.UserData.SetMyItemInfo(response["items"]);
                Managers.UserData.SetMyUserInfo(response["user"]);
            }
            cb?.Invoke(res);
        });
    }
    public static void TryXmasBox(int try_open, int try_count, int advertisement, SuccessCallback cb = null)
    {
        WWWForm param = new WWWForm();
        param.AddField("try_open", try_open);
        param.AddField("try_count", try_count);
        param.AddField("advertisement", advertisement);

        SendPost("event/xmas22", param, (response) =>
        {
            JToken res = GetResultData(response);
            if (response["result"] != null)
            {
                Managers.UserData.UpdateMyCharacter(response["character"]);
                Managers.UserData.SetMyItemInfo(response["items"]);
                Managers.UserData.SetMyUserInfo(response["user"]);
            }

            JArray rew = (JArray)response["rewards"];
            if (rew != null && rew.Count != 0)
            {
                List<ResponseReward> rewards = new List<SBWeb.ResponseReward>();
                foreach (JToken rewardData in rew)
                {
                    rewards.Add(new ResponseReward(int.Parse(rewardData[0].ToString()), int.Parse(rewardData[1].ToString()), int.Parse(rewardData[2].ToString())));
                }

                var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.CHRISTMAS_POPUP) as ChristmasPopup;
                if (popup.IsOpening())
                {
                    if (popup.isBoxEvent)
                    {
                        PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.BOXGACHA_POPUP, () =>
                        {
                            PopupCanvas.Instance.ShowRewardResult(rewards);
                        });
                    }
                    else
                    {
                        PopupCanvas.Instance.ShowRewardResult(rewards);
                    }
                }
                popup.isBoxEvent = true;
            }

            //SBDebug.Log(response);
            cb?.Invoke(response);
        });
    }

    public static void TryNewYearPouch(int item_no = 0, int levelup = 0, SuccessCallback cb = null)
    {
        WWWForm param = new WWWForm();
        param.AddField("item_no", item_no);
        param.AddField("levelup", levelup);

        SendPost("event/newyear23", param, (response) =>
        {
            JToken res = GetResultData(response);
            if (response["result"] != null)
            {
                Managers.UserData.UpdateMyCharacter(response["character"]);
                Managers.UserData.SetMyItemInfo(response["items"]);
                Managers.UserData.SetMyUserInfo(response["user"]);
            }

            var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.KOREANEWYEAR_POPUP) as KoreaNewYearPopup;

            JArray rew = (JArray)response["rewards"];
            if (rew != null && rew.Count != 0)
            {
                List<ResponseReward> rewards = new List<SBWeb.ResponseReward>();
                foreach (JToken rewardData in rew)
                {
                    rewards.Add(new ResponseReward(int.Parse(rewardData[0].ToString()), int.Parse(rewardData[1].ToString()), int.Parse(rewardData[2].ToString())));
                }

                if (rewards.Count >= 10)
                {
                    popup.EndResultAnim(rewards);
                    popup.isForce = true;
                }
                else
                {
                    PopupCanvas.Instance.ShowRewardResult(rewards);
                }
            }

            popup.rewardedList.Clear();
            JObject myItems = (JObject)response["result"];
            foreach (var item in myItems)
            {
                if (item.Key.Contains("step"))
                {
                    popup.rewardedList.Add(item.Value);
                }

                if (item.Key.Contains("level"))
                    popup.level = int.Parse(item.Value.ToString());
            }
            cb?.Invoke(response);

        });
    }

    public static void GetAddResultReward(string game_uid, SuccessCallback cb = null)
    {
        WWWForm param = new WWWForm();
        param.AddField("game_uid", game_uid);

        SendPost("point/getresult_addreward", param, (response) =>
        {
            JToken res = GetResultData(response);

            if (response["result"] != null)
            {
                Managers.UserData.SetMyUserInfo(response["user"]);
            }

            JArray rew = (JArray)response["rewards"];
            if (rew != null && rew.Count != 0)
            {
                List<ResponseReward> rewards = new List<SBWeb.ResponseReward>();
                foreach (JToken rewardData in rew)
                {
                    rewards.Add(new ResponseReward(int.Parse(rewardData[0].ToString()), int.Parse(rewardData[1].ToString()), int.Parse(rewardData[2].ToString())));
                }

                PopupCanvas.Instance.ShowRewardResult(rewards);
            }
            cb?.Invoke(response);
        });
    }
    //public static void GetRewardPackage(int package_id, SuccessCallback cb = null)
    //{
    //    WWWForm param = new WWWForm();
    //    param.AddField("package_id", package_id);

    //    SendPost("user/reward_package", param, (response) =>
    //    {
    //        JToken res = GetResultData(response);
    //        if (IsResultOK(response))
    //        {
    //            cb?.Invoke(res);
    //        }
    //    });
    //}
}

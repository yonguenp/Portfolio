using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameLogItem : MonoBehaviour
{
    [SerializeField] Text dateTime;
    [SerializeField] Text gameResult;
    [SerializeField] Text gameName;

    [SerializeField] UICharacterListingItem[] user_Character;
    DateTime gameTime;

    List<GameResultLog.User> chaser = new List<GameResultLog.User>();
    List<GameResultLog.User> sur = new List<GameResultLog.User>();
    int isChaser;
    JToken curData = null;
    public void Init(JToken data, int index)
    {
        curData = data;

        gameTime = DateTime.Parse(data["time"].Value<string>());

        string strDate = "";
        var diff = (SBCommonLib.SBUtil.KoreanTime - gameTime);
        if(diff.TotalDays >= 1.0f)
        {
            strDate += StringManager.GetString("ui_day", (int)diff.TotalDays);
            diff = diff - new TimeSpan((int)diff.TotalDays, 0, 0, 0);
        }
        if (diff.TotalHours >= 1.0f)
        {
            strDate += StringManager.GetString("ui_hour", (int)diff.TotalHours);
            diff = diff - new TimeSpan((int)diff.TotalHours, 0, 0);
        }

        strDate += StringManager.GetString("ui_min", (int)diff.TotalMinutes);

        dateTime.text = StringManager.GetString("time_befor", strDate);

        if (index < 2)
            SetUI();
        else
            Invoke("SetUI", 0.1f * index);
    }

    void SetUI()
    {
        CancelInvoke("SetUI");

        var userDatas = JToken.Parse(curData["data"].Value<string>());
        GameResultLog log = new GameResultLog();
        log.SetData(userDatas);
        if (log.game_type == 1)
            gameName.text = StringManager.GetString("ui_rank_match");
        else if (log.game_type == 2)
            gameName.text = StringManager.GetString("ui_train_match");

        for (int i = 0; i < log.user_count; i++)
        {
            if (log.users[i].chaser == 1)
                chaser.Add(log.users[i]);
            else
                sur.Add(log.users[i]);
        }
        if (log.user_count < 6)
        {
            Destroy(this.gameObject);
            return;
        }

        RefreshItem(chaser);
        RefreshItem(sur, 2);
    }
    public void RefreshItem(List<GameResultLog.User> data, int value = 0)
    {
        bool recordReult = false;
        for (int i = 0; i < data.Count; i++)
        {
            if (user_Character.Length <= i+ value)
                continue;
            var charItem = user_Character[i + value];
            CharacterGameData user_character = null;
            if (data[i].char_id > 0)
                user_character = CharacterGameData.GetCharacterData(data[i].char_id);

            if (data[i].dodge == true)
            {
                charItem.characterFace.gameObject.SetActive(false);
                charItem.runCharacter.gameObject.SetActive(true);

                charItem.characterName.text = null;
                charItem.uIGradeBG.SetGrade(1);
                charItem.gridText.text = data[i].user_nick;
                charItem.playerRankImage.sprite = RankType.GetRankFromPoint(data[i].rank_point).rank_resource;                
            }
            else
            {
                charItem.characterFace.gameObject.SetActive(true);
                charItem.runCharacter.gameObject.SetActive(false);

                charItem.characterFace.sprite = user_character.sprite_ui_resource;
                charItem.characterName.text = user_character.GetName();
                charItem.gridText.text = data[i].user_nick;
                charItem.uIGradeBG.SetGrade(user_character.char_grade);
                charItem.playerRankImage.sprite = RankType.GetRankFromPoint(data[i].rank_point).rank_resource;
            }

            if (charItem.enchantUI != null)
                charItem.enchantUI.SetEnchant(data[i].char_enchant);
            if (charItem.lvText != null)
                charItem.lvText.text = "Lv." + (data[i].char_lv).ToString();

            if (!recordReult && data[i].user_id == Managers.UserData.MyUserID)
            {
                recordReult = true;
                gameResult.text = data[i].win ? "<color=blue>WIN</color>" : "<color=red>LOSE</color>";
                isChaser = data[i].chaser;
                charItem.gridText.color = charItem.color;
                continue;
            }
            charItem.gridText.color = Color.white;
        }

    }

    public void OnSelectedItem()
    {
#if UNITY_EDITOR
        Managers.Scene.LoadSceneAsync(SceneType.Result, (scene, loadSceneMode)=> {
            ResultScene result_scene = (Managers.Scene.CurrentScene as ResultScene);
            if(result_scene)
                result_scene.SetResultData(JObject.Parse(curData["data"].Value<string>()));
        });
#endif
    }
}

public class GameResultLog
{
    public struct User
    {
        public string user_nick;
        public long user_id;
        public int char_id;
        
        public int char_lv;
        public int char_enchant;

        public int chaser;
        public int point;
        public bool win;
        public bool dodge;

        public int rank_point;
    }
    public string game_id { get; private set; }
    public int game_type { get; private set; }
    public int user_count { get; private set; }
    public List<User> users { get; private set; }

    public void SetData(JToken datas)
    {
        game_id = datas["game_id"].Value<string>();
        game_type = datas["game_type"].Value<int>();
        users = new List<User>();
        foreach (var item in datas["users"])
        {
            User user = new User();
            if (item["user_no"] != null)
                user.user_id = item["user_no"].Value<long>();
            user.user_nick = "";
            if (item["user_nick"] != null)
                user.user_nick = item["user_nick"].Value<string>();
            user.char_id = item["char_id"].Value<int>();

            if(item["char_lv"] != null)
            {
                user.char_lv = item["char_lv"].Value<int>();
            }
            else
            {
                user.char_lv = 1;
            }

            if (item["char_en"] != null)
            {
                user.char_enchant = item["char_en"].Value<int>();
            }
            else
            {
                user.char_enchant = 1;
            }

            user.chaser = item["chaser"].Value<int>();
            user.point = item["point"].Value<int>();
            user.win = Convert.ToBoolean(item["win"].Value<int>());
            user.dodge = Convert.ToBoolean(item["dodge"].Value<int>());
            user.rank_point = 0;
            foreach (var result in datas["result"])
            {
                if (result["user_no"].Value<long>() == item["user_no"].Value<long>())
                {
                    user.rank_point = result["point"].Value<int>();
                }
            }

            users.Add(user);
        }


        user_count = users.Count;

        users = users.OrderByDescending(_ => _.chaser).ToList();
    }
}


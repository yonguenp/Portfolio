using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayDataItem : MonoBehaviour
{
    [SerializeField] Image characterUI;
    [SerializeField] List<Text> titleText = new List<Text>();
    [SerializeField] List<Text> countText = new List<Text>();

    public void Init(PlayData data)
    {
        characterUI.sprite = (Managers.Data.GetData(GameDataManager.DATA_TYPE.character, data.charid) as CharacterGameData).sprite_ui_resource;
        if (characterUI.sprite == null)
            characterUI.sprite = Managers.Resource.LoadAssetsBundle<Sprite>("AssetsBundle/Texture/Portrait/dummy");

        for (int i = 0; i < titleText.Count; i++)
        {
            switch (i)
            {
                case 0:
                    titleText[i].text = "총 플레이 수";
                    countText[i].text = data.play.ToString();
                    break;
                case 1:
                    titleText[i].text = StringManager.GetString("ui_win_round");
                    countText[i].text = data.win.ToString();
                    break;
                case 2:
                    titleText[i].text = StringManager.GetString("ui_lose_round");
                    countText[i].text = data.lose.ToString();
                    break;
                case 3:
                    titleText[i].text = StringManager.GetString("ui_kill_survivor_count");
                    countText[i].text = data.kill.ToString();
                    break;
                case 4:
                    titleText[i].text = StringManager.GetString("ui_bettery_charge_count");
                    countText[i].text = data.charge.ToString();
                    break;
                default:
                    SBDebug.Log("아직 처리되지 않은 데이터입니다.");
                    break;
            }
        }
    }

}

public class PlayData
{
    public int charid = 0;
    public int kill = 0;
    public int charge = 0;
    public int play = 0;
    public int win = 0;
    public int lose = 0;
    public int get = 0;
    public int hit = 0;
    public int highscore = 0;
    public int totalscore = 0;
    public int mvp = 0;
    public int best_kill = 0;
    public int best_charge = 0;

    public PlayData()
    {

    }

    public PlayData(JToken data)
    {
        charid = data.Value<int>("char_id");
        kill = data.Value<int>("kill");
        charge = data.Value<int>("charge");
        play = data.Value<int>("play");
        win = data.Value<int>("win");
        lose = data.Value<int>("lose");
        get = data.Value<int>("get");
        hit = data.Value<int>("hit");
        highscore = data.Value<int>("highscore");
        totalscore = data.Value<int>("totalscore");
        mvp = data.Value<int>("mvp");
        best_kill = data.Value<int>("best_kill");
        best_charge = data.Value<int>("best_charge");
    }
    public static PlayData operator +(PlayData a, PlayData b)
    {
        if (a.charid != b.charid)
        {
            SBDebug.LogError("뭔가 잘못되었어..");
            return a;
        }

        a.kill += b.kill;
        a.charge += b.charge;
        a.play += b.play;
        a.win += b.win;
        a.lose += b.lose;
        a.get += b.get;
        a.hit += b.hit;
        a.totalscore += b.totalscore;
        a.mvp += b.mvp;

        //최대 점수 갱싱
        a.best_charge = a.best_charge >= b.best_charge ? a.best_charge : b.best_charge;
        a.best_kill = a.best_kill >= b.best_kill ? a.best_kill : b.best_kill;
        a.highscore = a.highscore >= b.highscore ? a.highscore : b.highscore;
        return a;
    }

    public void Clear()
    {
        kill = 0;
        charge = 0;
        play = 0;
        win = 0;
        lose = 0;
        get = 0;
        hit = 0;
        highscore = 0;
        totalscore = 0;
        mvp = 0;
        best_kill = 0;
        best_charge = 0;
    }
}



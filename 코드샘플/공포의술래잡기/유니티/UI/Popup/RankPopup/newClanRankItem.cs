using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class newClanRankItem : MonoBehaviour
{
    [SerializeField] Image BgImage;
    [SerializeField] Image rankIcon;
    [SerializeField] Text rankIndex;
    [SerializeField] UIClanEmblem clanIcon;
    [SerializeField] Text clanLevel;
    [SerializeField] Text clanName;
    [SerializeField] Text clanPeopleCount;
    [SerializeField] Text clanPoint;

    [SerializeField] Color[] colors;

    public void SetData(string name, int _rank, int icon, int level, int cnt, int point)
    {
        List<RankingRewardData> rankTable = (Managers.Data.GetData(GameDataManager.DATA_TYPE.ranking, Managers.UserData.seasonData.seasonID) as RankingGameData).rewards;

        if (_rank < 4 && _rank > 0)
        {
            rankIcon.color = Color.white;
            if (_rank == 1)
                rankIcon.sprite = Managers.Resource.LoadAssetsBundle<Sprite>("AssetsBundle/Texture/Icon/grade_01");
            else if (_rank == 2)
                rankIcon.sprite = Managers.Resource.LoadAssetsBundle<Sprite>("AssetsBundle/Texture/Icon/grade_02");
            else if (_rank == 3)
                rankIcon.sprite = Managers.Resource.LoadAssetsBundle<Sprite>("AssetsBundle/Texture/Icon/grade_03");

            BgImage.color = colors[0];
            rankIndex.text = string.Empty;
        }
        else if (_rank <= 0)
        {
            BgImage.color = colors[2];
            rankIcon.sprite = null;
            rankIcon.color = Color.clear;
            rankIndex.text = StringManager.GetString("순위밖");
        }
        else
        {
            BgImage.color = colors[1];
            rankIcon.sprite = null;
            rankIcon.color = Color.clear;
            rankIndex.text = _rank.ToString();
        }
        clanName.text = name;
        clanLevel.text = "LV." + level.ToString();
        clanPeopleCount.text = cnt.ToString() + $"/{GameConfig.Instance.DEFAULT_CLAN_HEADCOUNT}";
        clanPoint.text = point.ToString();
        clanIcon.Init(icon);

        if (name == Managers.UserData.MyClanName)
            BgImage.color = colors[2];

        clanPoint.text = point.ToString();

    }
}

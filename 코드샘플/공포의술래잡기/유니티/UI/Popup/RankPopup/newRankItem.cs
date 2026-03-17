using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class newRankItem : MonoBehaviour
{
    [SerializeField] Image BgImage;
    [SerializeField] Image rankIcon;
    [SerializeField] Text rankIndex;
    [SerializeField] Image rankGrade;
    [SerializeField] Text playerName_text;
    [SerializeField] Text battlePoint_text;
    [SerializeField] UIBundleItem reward;

    [SerializeField] Color[] colors;

    public void SetData(string _nick, int _rank, int _battlePoint, int _gradePoint, bool reward_shown = true)
    {
        List<RankingRewardData> rankTable = null;
        RankingGameData curSeasonData = (Managers.Data.GetData(GameDataManager.DATA_TYPE.ranking, Managers.UserData.seasonData.seasonID)) as RankingGameData;
        if(curSeasonData != null)
            rankTable = curSeasonData.rewards;

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
        else if(_rank <= 0)
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
        playerName_text.text = _nick;
        if (_nick == Managers.UserData.MyName)
            BgImage.color = colors[2];

        battlePoint_text.text = _battlePoint.ToString();
        rankGrade.sprite = RankType.GetRankFromPoint(_gradePoint).rank_resource;

        if (reward_shown)
        {
            //reward.transform.parent.gameObject.SetActive(true);

            foreach (RankingRewardData item in rankTable)
            {
                if (item.ranking_max == _rank && item.ranking_min == _rank)
                {
                    var reward_id = ShopPackageGameData.GetRewardDataList(item.reward);
                    reward.SetRewards(reward_id);
                    break;
                }
                else if (item.ranking_max <= _rank && item.ranking_min >= _rank)
                {
                    var reward_id = ShopPackageGameData.GetRewardDataList(item.reward);
                    reward.SetRewards(reward_id);
                    break;
                }
                else
                {
                    var reward_id = ShopPackageGameData.GetRewardDataList(item.reward);
                    reward.SetRewards(reward_id);
                    break;
                }
            }
        }
        else
        {
            reward.transform.parent.gameObject.SetActive(false);
        }
    }

    public void SetData(int _rank, ChristmasRank data)
    {
        string _nick = data.name;
        int _score = data.score;
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
        playerName_text.text = _nick;
        if (_nick == Managers.UserData.MyName)
            BgImage.color = colors[2];

        battlePoint_text.text = _score.ToString();
    }
}

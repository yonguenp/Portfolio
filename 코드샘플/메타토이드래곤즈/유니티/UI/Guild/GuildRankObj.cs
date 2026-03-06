using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork {
    public class GuildRankObj : MonoBehaviour
    {
        [SerializeField]
        Image back;
        [SerializeField]
        Text rankText;
        [SerializeField]
        Image pickImg;
        [SerializeField]
        GuildMarkObject markObj;
        [SerializeField]
        Text lvText;
        [SerializeField]
        Text expText;
        [SerializeField]
        Text nameText;
        [SerializeField]
        Text leaderNameText;
        [SerializeField]
        Text guildUserAmountText;
        [SerializeField]
        Sprite[] ServerFlag;

        public void Init(GuildRankData _rankingData,eGuildRankType rankType)
        {
            if(rankText != null)
            {
                int curRank = rankType switch
                {
                    eGuildRankType.SumRanking => _rankingData.Rank,
                    eGuildRankType.WeeklyRanking => _rankingData.WeeklyRank,
                    eGuildRankType.MonthlyRanking => _rankingData.MonthlyRank,
                    eGuildRankType.UnifiedRanking => _rankingData.Rank,
                    _ =>0,
                };
                if (curRank > 0)
                {
                    rankText.text = curRank.ToString();
                }
                else
                {
                    rankText.text = "-";
                }

                if(rankType == eGuildRankType.UnifiedRanking)
                {
                    pickImg.gameObject.SetActive(false);
                    pickImg.color = new Color(1f, 1f, 1f, 0f);

                    switch (_rankingData.Server)
                    {
                        case 1:
                            pickImg.gameObject.SetActive(true);
                            pickImg.sprite = ServerFlag[0];
                            pickImg.color = new Color(1f, 1f, 1f, 1f);
                            break;
                        case 2:
                            pickImg.gameObject.SetActive(true);
                            pickImg.sprite = ServerFlag[1];
                            pickImg.color = new Color(1f, 1f, 1f, 1f);
                            break;
                        case 3:
                            pickImg.gameObject.SetActive(true);
                            pickImg.sprite = ServerFlag[2];
                            pickImg.color = new Color(1f, 1f, 1f, 1f);
                            break;
                        default:                            
                            break;
                    }
                    
                }
                else if(_rankingData.Rank > 0)
                {
                    GuildResourceData data = null;
                    var guildRankData = GuildRankRewardData.GetByRankGroup(_rankingData.Rank, eGuildRankRewardGroup.GuildRank);
                    if (guildRankData != null)
                    {
                        var pickSpriteIdx = guildRankData.ACCUMULATE_REWARD;
                        data = GuildResourceData.Get(pickSpriteIdx);
                        pickImg.gameObject.SetActive(true);
                    }
                    
                    if (data != null)
                    {
                        pickImg.color = new Color(1f, 1f, 1f, 1f);
                        pickImg.sprite = data.RESOURCE;
                    }
                    else
                    {
                        pickImg.color = new Color(1f, 1f, 1f, 0f);
                    }
                }
                else
                {
                    //pickImg.sprite = GuildResourceData.DEFAULT_GUILD_PICK;
                    pickImg.color = new Color(1f, 1f, 1f, 0f);
                }
                    
            }
            markObj.SetGuildMark(_rankingData.EmblemNo, _rankingData.MarkNo);
            if(lvText != null)
                lvText.text = StringData.GetStringFormatByStrKey("user_info_lv_02", _rankingData.GetGuildLevel());
            if(expText != null)
            {
                expText.text = SBFunc.CommaFromNumber(rankType switch
                {
                    eGuildRankType.SumRanking => _rankingData.TotalPt,
                    eGuildRankType.WeeklyRanking => _rankingData.WeeklyPt,
                    eGuildRankType.MonthlyRanking => _rankingData.MonthlyPt,
                    eGuildRankType.UnifiedRanking => _rankingData.TotalPt,
                    _ =>0
                });
            }

            if (nameText != null) 
                nameText.text = _rankingData.GuildName;
            if(leaderNameText != null)
                leaderNameText.text = _rankingData.LeaderNick;
            if(guildUserAmountText != null)
                guildUserAmountText.text = string.Format("{0}/{1}", _rankingData.CurUserCnt, _rankingData.MaxUserCnt);

            if (_rankingData.GuildNo == GuildManager.Instance.GuildID)
                back.color = new Color(1.0f, 0.9803921568627451f, 0.9098039215686275f);
            else
                back.color = Color.white;
        }

        

    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class DiceEventRankingSlot : MonoBehaviour
    {
        const string BILLION_SYMBOL = "B";
        const string MILLION_SYMBOL = "M";

        [SerializeField] UserPortraitFrame portraitFrame = null;

        [SerializeField]
        Text rankText = null;
        [SerializeField]
        GameObject rankerImgObj = null;
        [SerializeField]
        Text nickText = null;
        [SerializeField]
        Text pointText = null;
        [SerializeField]
        Color[] rankColor = null;
        [SerializeField]
        GuildBaseInfoObject guildBaseInfoObject = null;
        [SerializeField]
        GuildMarkObject MarkObj = null;
        public void Init(OpenEventRankingData rankData, bool guild)
        {
            rankText.rectTransform.localPosition = new Vector2(rankText.rectTransform.localPosition.x, -9);
            if (rankData.rank > 0 && rankData.rank - 1 < rankColor.Length)
            {
                rankText.color = rankColor[rankData.rank - 1];
                rankerImgObj.SetActive(true);
                rankText.text = SBFunc.GetRankText(rankData.rank);
            }
            else
            {
                rankText.color = Color.white;
                rankerImgObj.SetActive(false);
                rankText.text = rankData.rank.ToString();
            }

            if (!guild)
            {
                if (portraitFrame != null)
                {
                    portraitFrame.gameObject.SetActive(true);
                    portraitFrame.SetUserPortraitFrame(rankData.icon, 0, true, rankData.PortraitData);
                }

                if (MarkObj != null)
                    MarkObj.gameObject.SetActive(false);

                if (!string.IsNullOrEmpty(rankData.nick) && nickText != null)
                    nickText.text = rankData.nick.ToString();

                bool enableGuild = GuildManager.Instance.GuildWorkAble && rankData.GuildNo > 0;
                guildBaseInfoObject.gameObject.SetActive(enableGuild);
                if (enableGuild)
                {
                    int markNo = rankData.GuildMarkNo;
                    int emblemNo = rankData.GuildEmblemNo;
                    string guildName = rankData.GuildName;
                    int guildNo = rankData.GuildNo;
                    guildBaseInfoObject.gameObject.SetActive(guildNo > 0);
                    guildBaseInfoObject.Init(guildName, markNo, emblemNo, guildNo);
                }
            }
            else
            {
                if (portraitFrame != null)
                    portraitFrame.gameObject.SetActive(false);

                if (MarkObj != null)
                {
                    MarkObj.gameObject.SetActive(true);
                    MarkObj.SetGuildMark(rankData.GuildEmblemNo, rankData.GuildMarkNo);
                }

                if (!string.IsNullOrEmpty(rankData.nick) && nickText != null)
                    nickText.text = rankData.GuildName.ToString();
            }

            pointText.text = ConvertBillionSymbolicWordByNumber(rankData.point);
        }

        string ConvertBillionSymbolicWordByNumber(long point, bool _setComma = true)
        {
            if (point < SBDefine.BILLION)
                return _setComma ? SBFunc.CommaFromNumber(point) : point.ToString();

            var billionModuler = System.Convert.ToInt32((long)point / SBDefine.BILLION);
            var remain = point - billionModuler * SBDefine.BILLION;

            var millionModuler = System.Convert.ToInt32((long)remain / SBDefine.MILLION);

            return string.Format("{0}" + BILLION_SYMBOL + " {1}" + MILLION_SYMBOL, billionModuler, millionModuler);
        }

        public void InitMyRanking(bool guild)
        {
            if(!guild)
            {
                if (portraitFrame != null)                
                    portraitFrame.gameObject.SetActive(true);                

                if (MarkObj != null)
                    MarkObj.gameObject.SetActive(false);

                
                if (nickText != null)
                    nickText.text = User.Instance.UserData.UserNick;


                if (portraitFrame != null)
                    portraitFrame.InitPortrait();
            }
            else
            {
                if (portraitFrame != null)               
                    portraitFrame.gameObject.SetActive(false);                

                if (MarkObj != null)
                    MarkObj.gameObject.SetActive(true);


                if (nickText != null)
                    nickText.text = GuildManager.Instance.MyGuildInfo != null ? GuildManager.Instance.MyGuildInfo.GetGuildName() : "-";
            }
            
            if (rankText != null)
                rankText.text = "-";
            if (pointText != null)
                pointText.text = "-";

            rankText.color = Color.white;
            rankText.rectTransform.localPosition = new Vector2(rankText.rectTransform.localPosition.x, -9);
            rankerImgObj.SetActive(false);
        }
    }
}

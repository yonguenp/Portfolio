using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class OpenEventRankingInfoSlot : MonoBehaviour
    {
        const string BILLION_SYMBOL = "B";
        const string MILLION_SYMBOL = "M";


        [Header("portrait")]
        [SerializeField] private UserPortraitFrame portrait = null;

        [SerializeField] private Text userNameLabel = null;
        [SerializeField] private Text battlePointLabel = null;
        [SerializeField] private Text worldBossPointLabel = null;
        [SerializeField] private Sprite[] rankIconSpriteArr = null;// 랭킹 아이콘 (1~3등)
        [SerializeField] private Image rankIcon = null;
        [SerializeField] private Text rankLabel = null;

        [SerializeField] private ButtonEventHandler detailPointButtonHandeler = null;

        [Header("guild")]
        [SerializeField] private GuildBaseInfoObject guildBaseInfoObject = null;

        private OpenEventRankingData rankData = null;


        public void Init(OpenEventRankingData info)
        {
            if (info == null) return;

            rankData = info;
            SetRankingInfoSlotData();
        }

        void SetRankingInfoSlotData()
        {
            if (!string.IsNullOrEmpty(rankData.nick) && userNameLabel != null)
                userNameLabel.text = rankData.nick;

            if (portrait != null)
                portrait.SetUserPortraitFrame(rankData.icon, 0, true, rankData.PortraitData);

            userNameLabel.text = rankData.nick;
            battlePointLabel.text = ConvertBillionSymbolicWordByNumber(rankData.point);

            int rank = rankData.rank;
            rankLabel.text = rank <= 0 ? "-" : SBFunc.GetRankText(rank);
            rankLabel.color = Color.white;
            if (rank > 0 && rank <= 3)
            {
                rankIcon.gameObject.SetActive(true);
                rankLabel.gameObject.SetActive(true);                
                (rankLabel.transform as RectTransform).localPosition = new Vector2(0, 10);
                rankIcon.sprite = rankIconSpriteArr[rank - 1];
            }
            else
            {
                rankIcon.gameObject.SetActive(false);
                rankLabel.gameObject.SetActive(true);
                (rankLabel.transform as RectTransform).localPosition = Vector2.zero;                
            }

            SetGuildData(rankData.GuildNo, rankData.GuildName, rankData.GuildMarkNo, rankData.GuildEmblemNo);
        }
        void SetGuildData(int guildNo, string _guildName, int _guildMark, int _guildEmblem)
        {
            if (guildBaseInfoObject == null)
                return;

            if (string.IsNullOrEmpty(_guildName))
            {
                guildBaseInfoObject.gameObject.SetActive(false);
                return;
            }

            bool enableGuild = GuildManager.Instance.GuildWorkAble && guildNo > 0;
            guildBaseInfoObject.gameObject.SetActive(enableGuild);

            if (enableGuild)
            {
                guildBaseInfoObject.Init(_guildName, _guildMark, _guildEmblem, guildNo);
            }
        }


        /// <summary>
        /// 대체 표기 변환 - 다른곳에서도 통합 사용 되면 SBFunc로 옮길것.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="_setComma"></param>
        /// <returns></returns>
        string ConvertBillionSymbolicWordByNumber(long point , bool _setComma = true)
        {
            if(point < SBDefine.BILLION)
                return _setComma ? SBFunc.CommaFromNumber(point) : point.ToString();

            var billionModuler = Convert.ToInt32((long)point / SBDefine.BILLION);
            var remain = point - billionModuler * SBDefine.BILLION;

            var millionModuler = Convert.ToInt32((long)remain / SBDefine.MILLION);

            return string.Format("{0}" + BILLION_SYMBOL + " {1}" + MILLION_SYMBOL , billionModuler , millionModuler);
        }

        string ConvertBillionSymbolicWordByNumber(int point, bool _setComma = true)
        {
            if (point < SBDefine.BILLION)
                return _setComma ? SBFunc.CommaFromNumber(point) : point.ToString();

            var billionModuler = point / SBDefine.BILLION;
            var remain = point - billionModuler * SBDefine.BILLION;

            var millionModuler = Convert.ToInt32((long)remain / SBDefine.MILLION);

            return string.Format("{0}" + BILLION_SYMBOL + " {1}" + MILLION_SYMBOL, billionModuler, millionModuler);
        }


        public void InitMyRanking()
        {
            if (rankLabel != null)
                rankLabel.text = "-";
            if (userNameLabel != null)
                userNameLabel.text = User.Instance.UserData.UserNick;
            if (battlePointLabel != null)
                battlePointLabel.text = "-";
            if (worldBossPointLabel != null)
                worldBossPointLabel.text = "-";

            rankLabel.color = Color.white;
            rankLabel.gameObject.SetActive(true);
            rankLabel.rectTransform.localPosition = new Vector2(rankLabel.rectTransform.localPosition.x, -9);
            rankIcon.gameObject.SetActive(false);

            if (portrait != null)
                portrait.InitPortrait(true);

            var isNotGuild = GuildManager.Instance.IsNoneGuild;
            var guildName = isNotGuild ? "" : GuildManager.Instance.GuildName;
            var guildMark = isNotGuild ? 0 : GuildManager.Instance.MyGuildInfo.GetGuildMark();
            var guildEmblem = isNotGuild ? 0 : GuildManager.Instance.MyGuildInfo.GetGuildEmblem();
            SetGuildData(GuildManager.Instance.GuildID, guildName, guildMark, guildEmblem);
        }

        public void OnTooltip()
        {
            CancelInvoke("OnTooltip");

            var stringData = SBFunc.CommaFromNumber(rankData.point);
            SimpleToolTip.OpenPopup(stringData, detailPointButtonHandeler.gameObject);
        }

        public void ClearTooltip()
        {
            CancelInvoke("OnTooltip");

            PopupManager.ClosePopup<SimpleToolTip>();
        }

        public void OnClickDetailPoint()
        {
            bool isOverBillion = rankData.point > Convert.ToInt64(SBDefine.BILLION) ;

            if (!isOverBillion)
                return;

            var stringData = SBFunc.CommaFromNumber(rankData.point);
            SimpleToolTip.OpenPopup(stringData, detailPointButtonHandeler.gameObject);
        }
    }
}

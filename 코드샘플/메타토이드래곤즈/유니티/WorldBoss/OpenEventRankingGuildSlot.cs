using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class OpenEventRankingGuildSlot : MonoBehaviour
    {
        [SerializeField] private Text GuildNameLabel = null;
        [SerializeField] private Text PointLabel = null;

        [SerializeField] private Sprite[] rankIconSpriteArr = null;// 랭킹 아이콘 (1~3등)
        [SerializeField] private GuildMarkObject guildMark = null;
        [SerializeField] private Image rankIcon = null;
        [SerializeField] private Text rankLabel = null;

        public void Init()
        {
            GuildNameLabel.text = "-";
            PointLabel.text = "-";
            rankIcon.gameObject.SetActive(false);
            rankLabel.text = "-";

            guildMark.SetGuildMark(0, 0);
        }
        public void Init(OpenEventRankingData data)
        {
            GuildNameLabel.text = data.GuildName;
            PointLabel.text = SBFunc.CommaFromNumber(data.point);
            int rank = data.rank;
            rankLabel.color = Color.white;
            rankLabel.text = rank <= 0 ? "-" : SBFunc.GetRankText(rank);
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

            guildMark.SetGuildMark(data.GuildEmblemNo, data.GuildMarkNo);
        }
    }
}

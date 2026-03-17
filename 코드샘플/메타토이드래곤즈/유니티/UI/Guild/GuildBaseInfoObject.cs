using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork { 
    public class GuildBaseInfoObject : MonoBehaviour
    {
        [SerializeField]
        GuildMarkObject guildMarkObject;
        [SerializeField]
        Text GuildNameText;
        [SerializeField]
        Image RankIcon;

        [SerializeField]
        Sprite[] rankSprite;

        private void OnEnable()
        {
            Invoke("RefreshLayout", 0.1f);
        }

        private void OnDisable()
        {
            CancelInvoke("RefreshLayout");
        }

        public void RefreshLayout()
        {
            CancelInvoke("RefreshLayout");
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }

        public void Init(GuildBaseData guildBaseData)
        {
            if (guildBaseData == null || guildBaseData.GuildID <= 0)
            {
                RankIcon.gameObject.SetActive(false);
                guildMarkObject.gameObject.SetActive(false);
                GuildNameText.text = string.Empty;
                return;
            }
            int emblemNo =guildBaseData.GuildEmblem;
            int markNo = guildBaseData.GuildMark;
            guildMarkObject.gameObject.SetActive(emblemNo>0 &&markNo>0);
            guildMarkObject.SetGuildMark(emblemNo, markNo);

            GuildNameText.text = guildBaseData.GuildName;

            RankIcon.gameObject.SetActive(false);
            if (guildBaseData.GuildID > 0)
            {
                int rank = GuildManager.Instance.GetGuildRanking(guildBaseData.GuildID);
                if (rank > 0 && rank < 50)
                {
                    RankIcon.gameObject.SetActive(true);
                    if (rank == 1)
                    {
                        RankIcon.sprite = rankSprite[0];
                    }
                    else if (rank == 2)
                    {
                        RankIcon.sprite = rankSprite[1];
                    }
                    else if (rank == 3)
                    {
                        RankIcon.sprite = rankSprite[2];
                    }
                    else if (rank <= 5)
                    {
                        RankIcon.sprite = rankSprite[3];
                    }
                    else if (rank <= 10)
                    {
                        RankIcon.sprite = rankSprite[4];
                    }
                    else if (rank <= 20)
                    {
                        RankIcon.sprite = rankSprite[5];
                    }
                    else if (rank <= 49)
                    {
                        RankIcon.sprite = rankSprite[6];
                    }
                    else
                    {
                        RankIcon.gameObject.SetActive(false);
                    }
                }
            }

            Invoke("ResetLayout", 0.1f);
        }
        public void Init(string guildName, int mark =0, int emblem = 0, int guild_no = 0)
        {
            if (guildName == "")
            {
                RankIcon.gameObject.SetActive(false);
                guildMarkObject.gameObject.SetActive(false);
                GuildNameText.text = string.Empty;
                return;
            }

            guildMarkObject.gameObject.SetActive(mark > 0 && emblem > 0);
            guildMarkObject.SetGuildMark(emblem, mark);
            GuildNameText.text = guildName;

            RankIcon.gameObject.SetActive(false);
            if (guild_no > 0)
            {
                int rank = GuildManager.Instance.GetGuildRanking(guild_no);
                if(rank > 0 && rank < 50)
                {
                    RankIcon.gameObject.SetActive(true);
                    if (rank == 1)
                    {
                        RankIcon.sprite = rankSprite[0];
                    }
                    else if(rank == 2)
                    {
                        RankIcon.sprite = rankSprite[1];
                    }
                    else if(rank == 3)
                    {
                        RankIcon.sprite = rankSprite[2];
                    }
                    else if(rank <= 5)
                    {
                        RankIcon.sprite = rankSprite[3];
                    }
                    else if(rank <=10)
                    {
                        RankIcon.sprite = rankSprite[4];
                    }
                    else if(rank <= 20)
                    {
                        RankIcon.sprite = rankSprite[5];
                    }
                    else if(rank <= 49)
                    {
                        RankIcon.sprite = rankSprite[6];
                    }
                    else
                    {
                        RankIcon.gameObject.SetActive(false);
                    }
                }
            }

            Invoke("ResetLayout", 0.1f);
        }

        void ResetLayout()
        {
            CancelInvoke("ResetLayout");
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }
    }
}
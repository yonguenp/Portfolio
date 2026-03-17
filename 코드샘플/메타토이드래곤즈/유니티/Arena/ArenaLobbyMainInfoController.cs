using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Newtonsoft.Json.Linq;
using TMPro;
namespace SandboxNetwork
{
    public class ArenaLobbyMainInfoController : MonoBehaviour
    {
        [SerializeField]
        ArenaRankingInfoPopup ArenaRankingInfo = null;
        [SerializeField]
        ArenaTeamInfoPopup ArenaTeamInfo = null;
        [Header("Main Info")]
        [SerializeField]
        Text mainTitleLabel = null;
        [SerializeField]
        Text seasonExpireLabel = null;
        [SerializeField]
        RectTransform seasonExpireParentRect = null;
        [SerializeField]
        TimeObject seasonExpireTime = null;


        public void Init()
        {

            SetSeasonInfo();
            SetTeamInfoPopup();
            SetRankingInfoPopup();
            RefreshMainInfoLayer();
            RefreshSeasonTime();
        }
        public void RefreshMainInfoLayer()
        {
            RefreshTeamInfoPopup();
            RefreshRankingInfoPopup();

        }
        void SetSeasonInfo()  // 현재 시즌 아이디만 받아 오고 그 이상 정보 없음 
        {
            mainTitleLabel.text = string.Format(StringData.GetStringByIndex(100001141), ArenaManager.Instance.UserArenaData.season_id.ToString());
        }
        void SetTeamInfoPopup()
        {
            if (ArenaTeamInfo != null)
            {
                ArenaTeamInfo.init();
            }
        }

        void SetRankingInfoPopup()
        {
            if (ArenaRankingInfo != null)
            {
                ArenaRankingInfo.Init();
            }
        }
        public void RefreshTeamInfoPopup()
        {
            if (ArenaTeamInfo != null)
            {
                ArenaTeamInfo.RefreshData();
            }
        }
        public void RefreshRankingInfoPopup()
        {
            if (ArenaRankingInfo != null)
            {
                ArenaRankingInfo.RefreshData();
            }
        }


        public void RefreshSeasonTime()
        {
            
            if (seasonExpireLabel == null || seasonExpireTime == null) return;
            if (ArenaManager.Instance.UserArenaData.season_remain_time > 0)
            {
                if (seasonExpireTime.Refresh == null)
                {
                    ArenaManager.Instance.SetTimeObject(seasonExpireTime);
                    seasonExpireTime.Refresh = () =>
                    {
                        float remain = TimeManager.GetTimeCompare(ArenaManager.Instance.UserArenaData.season_remain_time);
                        seasonExpireLabel.text = SBFunc.TimeString((int)remain, "아레나기간");
                        if (0 >= remain)
                        {
                            seasonExpireTime.Refresh = null;
                            ArenaManager.Instance.DeleteTimeObject(seasonExpireTime);
                        }
                    };
                }
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(seasonExpireParentRect);
        }

        public void OnClickSeasonElementBtn()
        {
            switch (ArenaManager.Instance.UserArenaData.season_type)
            {
                case ArenaBaseData.SeasonType.PreSeason:                    
                case ArenaBaseData.SeasonType.RegularSeason:
                    break;
                default:
#if DEBUG
                    Debug.LogError("여기 들어오면 안됨");
#endif
                    ToastManager.On(StringData.GetStringByStrKey("다음시즌정보없음"));
                    return;
            }

            PopupManager.OpenPopup<ElemBuffInfoPopup>();
        }
        public void OnClickPointShop()
        {

            PopupManager.OpenPopup<ArenaPointShopPopup>();
            // ToastManager.On(100000326);
            // ToastManager.On(StringData.GetStringFormatByStrKey("system_message_update_01"));
        }
    }
}
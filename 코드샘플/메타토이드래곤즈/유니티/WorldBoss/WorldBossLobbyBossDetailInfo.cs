using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 보스 상세 설명
/// </summary>
namespace SandboxNetwork
{
    public class WorldBossLobbyBossDetailInfo : MonoBehaviour
    {
        [SerializeField] GameObject spineParent = null;
        [SerializeField] Text descText = null;
        [SerializeField] List<ItemFrame> rewardList = new List<ItemFrame>();
        [SerializeField] GameObject arrowNode = null;
        [SerializeField] Image elementIcon = null;
        [SerializeField] Text bossName = null;
        [SerializeField] GameObject bossNameNode = null;
        [SerializeField] Text maxLevelText = null;
        [SerializeField] Text maxPointText = null;

        [SerializeField] Sprite sundayElementIcon = null;//일요일 일때만 '무속성' 아이콘으로 박아달라고 요청. (WJ - 2024.02.22)

        WorldBossStageDataInfo detailInfoData = null;
        DailyStageData dailyStageData = null;
        public void SetData(WorldBossStageDataInfo _infoData)
        {
            if (_infoData == null)
                return;
            else
            {
                if (detailInfoData != null && _infoData.RaidBossKey == detailInfoData.RaidBossKey)
                    return;
            }

            detailInfoData = _infoData;
            dailyStageData = DailyStageData.GetByWorldAndDay(detailInfoData.World, detailInfoData.Stage);

            SetMonsterSpine();
            SetRewardData();
            SetMonsterDesc();
            SetArrowNode();
            SetNameTag();
            SetLogDesc();
        }

        void SetArrowNode()
        {
            var todayBossData = WorldBossManager.Instance.WorldBossProgressData.TodayBossStageDataInfo;
            if(arrowNode != null)
                arrowNode.SetActive(todayBossData != null ? todayBossData.Count > 1 : false);
        }

        /// <summary>
        /// spine으로 할지 초상화로 할지 물어볼것.
        /// </summary>
        void SetMonsterSpine()
        {
            if (detailInfoData == null)
                return;

            if (dailyStageData == null)
                return;

            if (spineParent != null)
            {
                var spinePrefab = dailyStageData.GetDailySpinePrefab();  // data[index].GetDailySpinePrefab();
                if (spinePrefab != null)
                {
                    SBFunc.RemoveAllChildrens(spineParent.transform);
                    Instantiate(spinePrefab, spineParent.transform);
                }
            }
        }

        void SetRewardData()
        {
            if (rewardList == null || rewardList.Count <= 0)
                return;

            var rewardItemList = WorldBossManager.Instance.GetWorldBossRewardList();

            for(int i = 0; i< rewardList.Count; i++)
            {
                var item = rewardList[i];
                if (item == null)
                    continue;

                item.gameObject.SetActive(false);
            }

            for(int k = 0; k < rewardItemList.Count; k++)
            {
                if(k >= rewardList.Count)
                {
                    var comp = Instantiate(rewardList[0], rewardList[0].transform.parent);
                    rewardList.Add(comp);
                }
                var assetData = rewardItemList[k];
                rewardList[k].SetFrameItem(assetData);
                
                //switch(k)
                //{
                //    case 1:
                //        rewardList[k].SetCustomText(StringData.GetStringByStrKey("보스레이드_보상_코어블록"));
                //        break;
                //    case 2:
                //        rewardList[k].SetCustomText(StringData.GetStringByStrKey("보스레이드_보상_일반큐브"));
                //        break;
                //    case 3:
                //        rewardList[k].SetCustomText(StringData.GetStringByStrKey("보스레이드_보상_고급큐브"));
                //        break;
                //}
                //rewardList[k].SetTextAlignment(TextAnchor.MiddleCenter);
                rewardList[k].gameObject.SetActive(true);
            }
        }
        /// <summary>
        /// 보스 설명 부분
        /// </summary>
        void SetMonsterDesc()
        {
            if (descText == null)
                return;

            if (detailInfoData == null)
                return;

            var monsterData = detailInfoData.RaidBossData;
            if (monsterData == null)
                return;

            var descData = StringData.GetStringByStrKey(monsterData._DESC);
            descText.text = descData;
        }

        //최대 도달 레벨, 최고 점수
        void SetLogDesc()
        {
            if (maxLevelText == null || maxPointText == null)
                return;

            if (detailInfoData == null)
                return;

            var worldIndex = detailInfoData.World;
            var logData = WorldBossManager.Instance.WorldBossProgressData.TodayBossLogDataInfo;

            if (logData == null || logData.Count <= 0)
            {
                InitLogUI();
                return;
            }

            if(!logData.ContainsKey(worldIndex))
            {
                InitLogUI();
                return;
            }

            var curLogData = logData[worldIndex];

            var hasLog = curLogData.High_Level > 0;
            maxLevelText.text = hasLog ? StringData.GetStringFormatByStrKey("user_info_lv_02" , curLogData.High_Level) : StringData.GetStringByStrKey("기록없음");

            if (int.TryParse(curLogData.High_Score, out int result))
                maxPointText.text = result > 0 ? SBFunc.CommaFromNumber(result) : StringData.GetStringByStrKey("기록없음");
            else
                maxPointText.text = curLogData.High_Score;
        }

        void InitLogUI()
        {
            maxLevelText.text = "-";
            maxPointText.text = "-";
        }
        void SetNameTag()
        {
            if (elementIcon == null)
                return;

            if (bossName == null)
                return;

            if (detailInfoData == null)
                return;

            var monsterData = detailInfoData.RaidBossData;
            if (monsterData == null)
                return;

            var isSunday = DailyManager.Instance.GetDaily() == eDailyType.Sun;
            var elementSprite = ResourceManager.GetResource<Sprite>(eResourcePath.ElementIconPath, string.Format("icon_property_{0}_160", SBDefine.ConvertToElementString(monsterData.ELEMENT)));

            elementIcon.sprite = isSunday ? sundayElementIcon : elementSprite;
            bossName.text = StringData.GetStringByStrKey(monsterData._NAME);

            if(bossNameNode != null)
                RefreshContentFitter(bossNameNode.GetComponent<RectTransform>());
        }

        private void RefreshContentFitter(RectTransform transform)
        {
            if (transform == null || !transform.gameObject.activeSelf)
                return;

            if (transform.gameObject.GetComponent<ParticleSystem>() != null)
                return;

            foreach (RectTransform child in transform)
            {
                RefreshContentFitter(child);
            }

            var layoutGroup = transform.GetComponent<LayoutGroup>();
            var contentSizeFitter = transform.GetComponent<ContentSizeFitter>();
            if (layoutGroup != null)
            {
                layoutGroup.SetLayoutHorizontal();
                layoutGroup.SetLayoutVertical();
            }

            if (contentSizeFitter != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
            }
        }
    }
}


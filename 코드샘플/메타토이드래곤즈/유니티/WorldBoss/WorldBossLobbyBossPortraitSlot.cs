using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 월드 보스 세팅하는 부분
/// </summary>
namespace SandboxNetwork
{
    public class WorldBossLobbyBossPortraitSlot : MonoBehaviour
    {
        [SerializeField] EnemyFrame frame = null;
        [SerializeField] Button button = null;
        [SerializeField] Image elementIcon = null;
        [SerializeField] Text bossName = null;
        [SerializeField] GameObject spineParent = null;

        WorldBossStageDataInfo detailInfoData = null;
        DailyStageData dailyStageData = null;
        MonsterBaseData raidBossData = null;

        public void SetData(MonsterBaseData _data, IntDelegate _delegate)
        {
            if (_data == null || frame == null)
                return;

            frame.SetEnemyFrame(_data.KEY, _data.ELEMENT, true);

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => { _delegate.Invoke(_data.KEY); });
            }
        }

        public void SetData()
        {
            var curSelectData = WorldBossManager.Instance.WorldBossProgressData.GetStageDataByMonsterKey(WorldBossManager.Instance.UISelectBossKey);
            if (curSelectData == null)
                return;

            detailInfoData = curSelectData;
            raidBossData = detailInfoData.RaidBossData;
            dailyStageData = DailyStageData.GetByWorldAndDay(detailInfoData.World, detailInfoData.Stage);

            SetMonsterSpine();
            SetNameTag();
        }

        void SetNameTag()
        {
            if (elementIcon == null)
                return;

            if (bossName == null)
                return;

            if (raidBossData == null)
                return;

            elementIcon.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ElementIconPath, string.Format("icon_property_{0}_160", SBDefine.ConvertToElementString(raidBossData.ELEMENT)));
            bossName.text = StringData.GetStringByStrKey(raidBossData._NAME);
        }
        void SetMonsterSpine()
        {
            if (detailInfoData == null)
                return;

            if (dailyStageData == null)
                return;

            if (spineParent != null)
            {
                var childCount = spineParent.gameObject.transform.childCount;
                if (childCount > 0)
                    return;

                var spinePrefab = dailyStageData.GetDailySpinePrefab();  // data[index].GetDailySpinePrefab();
                if (spinePrefab != null)
                {
                    SBFunc.RemoveAllChildrens(spineParent.transform);
                    Instantiate(spinePrefab, spineParent.transform);
                }
            }
        }
    }
}



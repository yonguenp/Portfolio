using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 보스 초상화가 세팅되는 스크롤뷰 - 미사용.
/// </summary>
namespace SandboxNetwork
{
    public class WorldBossLobbyBossPortraitScrollView : MonoBehaviour
    {
        [SerializeField] List<WorldBossLobbyBossPortraitSlot> bossList = new List<WorldBossLobbyBossPortraitSlot>();
        [SerializeField] ScrollRect scroll = null;
        public void SetData(List<MonsterBaseData> _monsterList, IntDelegate _slotClickDelegate)
        {
            //worldBossManager 를 통해서 전체(오늘의 보스)데이터 세팅해줌.

            if (bossList == null || bossList.Count <= 0)
                return;

            foreach (var bossSlot in bossList)
                if (bossSlot != null)
                    bossSlot.gameObject.SetActive(false);

            if (_monsterList == null || _monsterList.Count <= 0)
                return;

            for (int i = 0; i< _monsterList.Count; i++)
            {
                if(i >= bossList.Count)
                {
                    var slotComp = Instantiate(bossList[0], bossList[0].transform.parent);
                    bossList.Add(slotComp);
                }

                if (_monsterList[i] == null)
                    continue;

                bossList[i].SetData(_monsterList[i], _slotClickDelegate);//터치 하면 가장 바깥의 worldbossLobby로 넘기기
                bossList[i].gameObject.SetActive(true);
            }

            if (scroll != null)
                scroll.verticalNormalizedPosition = 0;

            RefreshContentFitter(scroll.content.GetComponent<RectTransform>());
        }

        private void RefreshContentFitter(RectTransform transform)
        {
            if (transform == null || !transform.gameObject.activeSelf)
            {
                return;
            }

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


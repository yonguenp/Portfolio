using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class WorldTravelWorldSelectLayer : MonoBehaviour
    {
        public ScrollRect worldInfoScollRect = null;

        public GameObject worldInfoPrefab = null;

        List<GameObject> worldInfoList = new List<GameObject>();    // 생성한 월드 인포 노드 리스트

        int curClearWorld = 0;      // 현재까지 클리어한 월드 
        int curSelectedWorld = 0;   // 현재 선택한 월드

        public void Init(int curWorld)
        {
            curSelectedWorld = curWorld;
            SetWorldSelectLayer();
        }

        void SetWorldSelectLayer()
        {
            curClearWorld = StageManager.Instance.AdventureProgressData.GetLatestWorld();

            ClearLayer();
            worldInfoList.Clear();

            foreach (TravelData worldData in TravelData.GetAll())
            {
                GameObject newWorldInfo = Instantiate(worldInfoPrefab, worldInfoScollRect.content.transform);
                newWorldInfo.GetComponent<TravelWorldInfo>().Init(worldData, this);

                worldInfoList.Add(newWorldInfo);
            }

            RefreshContentFitter(worldInfoScollRect.GetComponent<RectTransform>());
            PlaySelectScrollTween(curSelectedWorld);
        }

        public void RefreshWorldInfoButtonState(GameObject travelInfo)
        {
            foreach (GameObject worldInfo in worldInfoList)
            {
                var equalObject = worldInfo == travelInfo;
                if (equalObject)
                    curSelectedWorld = worldInfo.GetComponent<TravelWorldInfo>().GetTravelData().WORLD;
                worldInfo.GetComponent<TravelWorldInfo>()?.SwitchCurrentSelectedFrame(equalObject);
            }

            RefreshContentFitter(worldInfoScollRect.GetComponent<RectTransform>());
            PlaySelectScrollTween(curSelectedWorld);
        }

        public int GetCurrentSelectedWorld()
        {
            return curSelectedWorld;
        }

        public void OnClickCloseButton()
        {
            gameObject.SetActive(false);
        }

        void ClearLayer()
        {
            SBFunc.RemoveAllChildrens(worldInfoScollRect.content.transform);
        }

        void MoveSelectScrollCenter(int _worldIndex)
        {
            var modifyIndex = _worldIndex - 1;
            if (worldInfoList == null || worldInfoList.Count <= 0 || worldInfoList.Count <= modifyIndex)
                return;

            worldInfoScollRect.FocusOnItem(worldInfoList[modifyIndex].GetComponent<RectTransform>(), 0.2f);
        }

        void PlaySelectScrollTween(int _worldIndex)
        {
            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(0.1f);
            seq.AppendCallback(() => {
                MoveSelectScrollCenter(_worldIndex);
            });

            seq.Play();
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

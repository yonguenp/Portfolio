using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class TravelWorldSelect : MonoBehaviour
    {
        [SerializeField] ScrollRect worldInfoScollRect = null;
        [SerializeField] List<TravelWorldSelectSlot> worldSelectSlotList = new List<TravelWorldSelectSlot>();// 생성한 월드 인포 노드 리스트
        [SerializeField] List<Sprite> worldSelectBgList = new List<Sprite>();
        [SerializeField] Image targetBg = null;

        [SerializeField]GameObject worldSelectSlotPrefab = null;

        int curSelectedWorld = 0;   // 현재 선택한 월드

        public void Init(int curWorld)
        {
            curSelectedWorld = curWorld;
            SetWorldSelectLayer();
        }

        void SetWorldSelectLayer()
        {
            var worldTravelDataList = TravelData.GetAll();
            if(worldTravelDataList.Count < worldSelectSlotList.Count)
            {
                Debug.LogError("월드 데이터 크기 오류");
                return;
            }

            for(int i = 0; i< worldTravelDataList.Count; i++)
            {
                var worldTravelData = worldTravelDataList[i];
                if (worldTravelData == null)
                    continue;

                if(worldSelectSlotList.Count <= i)
                {
                    if (worldSelectSlotPrefab == null)
                        continue;

                    var worldSlotPrefab = Instantiate(worldSelectSlotPrefab, worldInfoScollRect.content);
                    var worldslotComp = worldSlotPrefab.GetComponent<TravelWorldSelectSlot>();
                    if(worldslotComp == null)
                    {
                        Destroy(worldSlotPrefab);
                        continue;
                    }
                    worldSelectSlotList.Add(worldslotComp);
                }    

                worldSelectSlotList[i].Init(worldTravelData, curSelectedWorld, RefreshWorldInfoButtonState);
            }

            playSelectScrollTween(curSelectedWorld);
            RefreshWorldSelectBG(curSelectedWorld);
        }

        public void RefreshWorldInfoButtonState(int _selectWorldIndex)
        {
            if (worldSelectSlotList == null || worldSelectSlotList.Count <= 0)
                return;

            foreach(var slot in worldSelectSlotList)
            {
                var isEqual = slot.IsEqualWorldIndex(_selectWorldIndex);
                slot.SwitchCurrentSelectedFrame(isEqual);
            }
            curSelectedWorld = _selectWorldIndex;
            playSelectScrollTween(curSelectedWorld);
            RefreshWorldSelectBG(curSelectedWorld);
        }

        public int GetCurrentSelectedWorld()
        {
            return curSelectedWorld;
        }

        public void OnClickCloseButton()
        {
            gameObject.SetActive(false);
        }

        void MoveSelectScrollCenter(int _worldIndex)
        {
            var modifyIndex = _worldIndex - 1;
            if (worldSelectSlotList == null || worldSelectSlotList.Count <= 0 || worldSelectSlotList.Count <= modifyIndex)
                return;

            worldInfoScollRect.FocusOnItem(worldSelectSlotList[modifyIndex].GetComponent<RectTransform>(), 0.2f);
        }

        void playSelectScrollTween(int _worldIndex)
        {
            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(0.1f);
            seq.AppendCallback(() => {
                MoveSelectScrollCenter(_worldIndex);
            });

            seq.Play();
        }

        void RefreshWorldSelectBG(int _worldIndex)
        {
            var modifyIndex = _worldIndex - 1;
            if(targetBg != null && worldSelectBgList != null && worldSelectBgList.Count > 0 && worldSelectBgList.Count > modifyIndex)
            {
                targetBg.sprite = worldSelectBgList[modifyIndex];
            }
        }
    }
}

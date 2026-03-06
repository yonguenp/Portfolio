using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class WorldBossDeckInfoSlot : MonoBehaviour
    {
        [SerializeField] List<DragonPortraitFrame> dragonList = new List<DragonPortraitFrame>();
        [SerializeField] Text battlePointText = null;
        [SerializeField] Text emptyBattleLineText = null;//빈 덱일 때 표시 라벨

        int slotIndex = 0;
        List<int> dragonIDList = new List<int>();//현재 덱의 드래곤ID 값 리스트


        public void InitDeckSlot(int _slotIndex)
        {
            slotIndex = _slotIndex;

            if (dragonIDList == null)
                dragonIDList = new List<int>();
            dragonIDList.Clear();

            foreach (var dragonPortrait in dragonList)
                if (dragonPortrait != null)
                    dragonPortrait.gameObject.SetActive(false);

            if (emptyBattleLineText != null)
                emptyBattleLineText.gameObject.SetActive(true);
        }

        public void SetDataSlot(int _slotIndex, List<int> _dragonIdList)
        {
            slotIndex = _slotIndex;
            battlePointText.text = "-";
            SetIdList(_dragonIdList);
            SetDragonPortrait(_dragonIdList);
        }

        void SetIdList(List<int> _dragonIdList)
        {
            if (dragonIDList == null)
                dragonIDList = new List<int>();

            dragonIDList.Clear();
            dragonIDList = _dragonIdList?.ToList();
        }

        void SetDragonPortrait(List<int> _dragonIdList)
        {
            if (dragonList == null || dragonList.Count <= 0)
                return;

            if(_dragonIdList == null || _dragonIdList.Count <= 0)
            {
                foreach (var dragonPortrait in dragonList)
                    if (dragonPortrait != null)
                        dragonPortrait.gameObject.SetActive(false);

                if (emptyBattleLineText != null)
                    emptyBattleLineText.gameObject.SetActive(true);
                return;
            }

            var isEmptyCheckCount = 0;
            foreach (var id in dragonIDList)
                if (id <= 0)
                    isEmptyCheckCount++;

            if (emptyBattleLineText != null)
                emptyBattleLineText.gameObject.SetActive(isEmptyCheckCount == dragonIDList.Count);

            var tempINF = 0;
            var _dragonIDCount = _dragonIdList.Count;
            for(int i = 0; i < dragonList.Count; i++)
            {
                if(i < _dragonIDCount)
                {
                    var dragonID = _dragonIdList[i];
                    var dragonData = User.Instance.DragonData.GetDragon(dragonID);
                    if(dragonData == null)
                    {
                        dragonList[i].gameObject.SetActive(false);
                        continue;
                    }
                    dragonList[i].SetDragonPortraitFrame(dragonData, false, true, false);                    
                    dragonList[i].setCallback(OnClickDragonPortrait);
                    dragonList[i].gameObject.SetActive(true);
                    tempINF += dragonData.GetTotalINF();
                }
                else
                    dragonList[i].gameObject.SetActive(false);
            }

            SetBattlePoint(tempINF);
        }

        void SetBattlePoint(int _point)
        {
            if (battlePointText != null)
                battlePointText.text = _point <= 0 ? "-" : SBFunc.CommaFromNumber(_point);
        }

        /// <summary>
        /// 전체 덱을 보여줄지, 현재 덱만 보여줄지 논의 - 빼는 것으로 결정.
        /// </summary>
        /// <param name="_dragonID"></param>
        void OnClickDragonPortrait(string _dragonID)
        {
            return;

            if(int.TryParse(_dragonID , out int result))
            {
                var popup = DragonManagePopup.OpenPopup(0, 1);
                popup.SetExitCallback(() => {
                    DragonChangedEvent.Refresh();
                });
                popup.CurDragonTag = result;
                popup.ClearDragonInfoList();

                popup.DragonInfoList.AddRange(User.Instance.PrefData.GetSerializeTotalWorldBossFormation().ToList());
                popup.ForceUpdate();
            }
        }
        /// <summary>
        /// 덱 편집 버튼
        /// </summary>
        public void OnClickEditDeck()
        {
            WorldBossManager.Instance.UIDeckIndex = slotIndex;//현재 선택한 슬롯 임시 저장
            LoadingManager.Instance.EffectiveSceneLoad("WorldBossTeamSetting", eSceneEffectType.BlackBackground);


        }
    }
}


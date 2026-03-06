using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class HotTimeEventDescPopup : Popup<PopupData>
    {
        [SerializeField] List<HotTimeDescSlot> descList = new List<HotTimeDescSlot>();

        public override void InitUI()
        {
            RefreshSlot();
        }

        public void RefreshSlot()
        {
            foreach(var slot in descList)
            {
                if (slot == null)
                    continue;

                var type = slot.GetDescType();
                var index = GetIndexByType(type);

                if (index < 0 || index >= descList.Count)
                    continue;

                var isActive = GetActiveByType(type);
                descList[index].SetActiveSlot(isActive);
            }
        }
        int GetIndexByType(HotTimeDescType _type)
        {
            return (int)_type - 1;
        }

        bool GetActiveByType(HotTimeDescType _type)
        {
            switch(_type)
            {
                case HotTimeDescType.ADVENTURE:
                    return GameConfigTable.IsAdventureHotTime;
                case HotTimeDescType.WORLDBOSS:
                    return GameConfigTable.IsRaidHotTime;
                case HotTimeDescType.DAILYDUNGEON:
                    return GameConfigTable.IsDailyDungeonHotTime;
                case HotTimeDescType.GEMDUNGEON:
                    return GameConfigTable.IsGemDungeonHotTime;
            }
            return false;
        }
    }
}
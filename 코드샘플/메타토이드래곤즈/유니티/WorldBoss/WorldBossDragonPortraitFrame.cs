using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class WorldBossDragonPortraitFrame : DragonPortraitFrame
    {
        /// <summary>
        /// 덱정보 표시 오브젝트
        /// </summary>
        [SerializeField] List<GameObject> iconListObject = new List<GameObject>();
        public override void SetDragonPortraitFrame(UserDragon _dragonData, bool isSelectCheck = false, bool clickEnable = true, bool isSpineOn = true)
        {
            base.SetDragonPortraitFrame(_dragonData, isSelectCheck, true, false);
        }
        protected override void SetSelectCheckNode(bool _isVisible)
        {
            base.SetSelectCheckNode(_isVisible);

            //팀표시 세팅
            if(_isVisible)
            {
                InitIconList();

                var curIndex = WorldBossManager.Instance.UIDeckIndex;
                var checkDeckIndex = GetTeamIndexByTag(dragonData.Tag);

                if (checkDeckIndex == curIndex)
                    iconListObject[4].SetActive(true);
                else if(checkDeckIndex >= 0)
                    iconListObject[checkDeckIndex].SetActive(true);
                else
                    iconListObject[4].SetActive(true);
            }
        }

        void InitIconList()
        {
            if (iconListObject == null || iconListObject.Count <= 0)
                return;

            foreach(var obj in iconListObject)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
        }

        int GetTeamIndexByTag(int _dragonTag)
        {
            var tempStorageDeck = User.Instance.PrefData.WorldBossFormationData.TeamTemporarystorage;

            for (int i = 0; i < tempStorageDeck.Count; i++)
            {
                var list = tempStorageDeck[i];
                if (list == null || list.Count <= 0)
                    continue;

                var isContain = list.Contains(_dragonTag);
                if (isContain)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}


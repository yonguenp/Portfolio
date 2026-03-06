using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class UserArenaPortraitFrame : UserPortraitFrame
    {
        [SerializeField] private Image userRankIcon = null;

        public override void SetUserPortraitFrame(ThumbnailUserData _arenaInfoData, bool _justLevel = true)
        {
            if (_arenaInfoData == null) 
                return;

            base.SetUserPortraitFrame(_arenaInfoData, _justLevel);
            if (_arenaInfoData is ArenaUserData arenaInfoData && !string.IsNullOrEmpty(arenaInfoData.RankIconName))
                SetRankIcon(arenaInfoData.RankIconName);
        }
        public override void SetUserPortraitFrame(string dragonID, int _level = 0, bool _justLevel = true, PortraitEtcInfoData _portraitData = null)
        {
            base.SetUserPortraitFrame(dragonID, _level, _justLevel, _portraitData);
            userRankIcon.gameObject.SetActive(false);
        }

        void SetRankIcon(string _iconName)
        {
            userRankIcon.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ArenaRankPath, _iconName);
            if (userRankIcon.sprite == null)
                userRankIcon.gameObject.SetActive(false);
            else
                userRankIcon.gameObject.SetActive(true);
        }
    }
}


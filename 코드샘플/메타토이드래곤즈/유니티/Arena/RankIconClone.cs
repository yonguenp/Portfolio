using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class RankIconClone : MonoBehaviour
    {
        [SerializeField]
        private Image iconImg = null;
        [SerializeField]
        private Text iconText = null;

        public void Init(int key)
        {
            var rankDat = ArenaRankData.Get(key.ToString());
            iconImg.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ArenaRankPath, rankDat.ICON);
            iconText.text = StringData.GetStringByStrKey(rankDat._NAME);
        }
        public void Init(ArenaRankData data)
        {
            iconImg.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ArenaRankPath, data.ICON);
            iconText.text = StringData.GetStringByStrKey(data._NAME);
        }
    }
}


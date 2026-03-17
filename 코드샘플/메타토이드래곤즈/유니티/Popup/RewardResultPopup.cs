using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class RewardResultPopup : Popup<RewardPopupData>
    {
        [SerializeField]
        GameObject targetParent = null;
        [SerializeField]
        GameObject effectPrefab = null;
        List<GameObject> particles = null;

        public VoidDelegate closeCallback = null;
        public VoidDelegate CloseCallback { set { if(value != null)closeCallback = value; } }

        bool buttonLock = false;

        public override void InitUI()
        {
            SetResultItemSlot(Data);
            if (particles == null)
                particles = new List<GameObject>();

            CancelInvoke("ButtonUnlock");
            buttonLock = true;
            Invoke("ButtonUnlock", 0.5f);
        }

        void SetResultItemSlot(RewardPopupData rewardData)
        {
            if (targetParent == null)
            {
                return;
            }
            if (particles == null)
                particles = new List<GameObject>();

            SBFunc.RemoveAllChildrens(targetParent.transform);

            foreach (var reward in rewardData.Rewards)
            {
                var clone = Instantiate(ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "itemPrefab"), targetParent.transform);
                var itemframe = clone.GetComponent<ItemFrame>();
                itemframe.SetFrameItemInfo(reward.ItemNo, reward.Amount);//0으로 초기화

                if (effectPrefab != null)
                {
                    var effect = Instantiate(effectPrefab, targetParent.transform.parent);
                    if (effect != null)
                    {
                        effect.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 36);
                        particles.Add(effect);
                    }
                }
            }
        }
        void ButtonUnlock()
        {
            CancelInvoke("ButtonUnlock");
            buttonLock = false;
        }
        public override void ClosePopup()
        {
            if (buttonLock)
                return;

            if (particles != null)
            {
                for(int i = 0, count = particles.Count; i < count; ++i)
                {
                    if (particles[i] == null)
                        continue;

                    Destroy(particles[i]);
                }
                particles.Clear();
            }
            base.ClosePopup();

            if (closeCallback != null)
                closeCallback();
        }
    }
}

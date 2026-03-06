using UnityEngine;

namespace SandboxNetwork
{
    public class DecomposeResultPopup : Popup<RewardPopupData>
    {
        [SerializeField]
        GameObject targetParent = null;

        public override void InitUI() 
        {
            SetResultItemSlot(Data);
        }

        void SetResultItemSlot(RewardPopupData rewardData)
        {
            if (targetParent == null)
            {
                return;
            }

            SBFunc.RemoveAllChildrens(targetParent.transform);

            foreach (var reward in rewardData.Rewards)
            {
                var clone = Instantiate(ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "item"), targetParent.transform);
                ItemFrame itemframe = clone.GetComponent<ItemFrame>();
                itemframe.SetFrameItemInfo(reward.ItemNo, reward.Amount);//0으로 초기화
            }
        }
    }
}

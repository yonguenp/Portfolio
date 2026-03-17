using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class PartDestroyPopup : Popup<PartPopupData>
    {
        [SerializeField]
        GameObject itemParentNode = null;

        public override void InitUI() //파괴된 아이템 인덱스 및 갯수 넘어옴
        {
            this.SetData();
        }

        void SetData()
        {
            if (this.itemParentNode != null)
            {
                SBFunc.RemoveAllChildrens(itemParentNode.transform);
                var itemPrefab = ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "itemPrefab");
                if (Data.Rewards != null)
                {
                    for(int i = 0, count = Data.Rewards.Count; i < count; ++i)
                    {
                        if (Data.Rewards[i] == null || Data.Rewards[i].GoodType != eGoodType.ITEM)
                            continue;

                        if (Data.Rewards[i].ItemNo < 1 || Data.Rewards[i].Amount < 1)
                            continue;

                        var itemClone = Instantiate(itemPrefab, itemParentNode.transform);
                        itemClone.GetComponent<ItemFrame>().SetFrameItemInfo(Data.Rewards[i].ItemNo, Data.Rewards[i].Amount);
                    }
                }
            }
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class UIDragonIconObject : UIObject
    {
        [SerializeField] GameObject reddot = null;
        public override void InitUI(eUIType targetType)
        {
            base.InitUI(targetType);

            CheckButtonStates();
        }

        public override bool RefreshUI(eUIType targetType) //타입 갱신부는 아래에서 상속으로 구현
        {
            if (base.RefreshUI(targetType))
            {
                CheckButtonStates();
                return true;
            }
            return false;
        }

        public void CheckButtonStates()
        {
            if(reddot != null)
            {
                var isShow = CollectionAchievementManager.Instance.IsShowCollectionReddot();
                reddot.SetActive(isShow);
            }
        }
    }
}
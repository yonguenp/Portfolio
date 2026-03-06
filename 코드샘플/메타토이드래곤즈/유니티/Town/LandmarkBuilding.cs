

using DG.Tweening;
using UnityEngine;

namespace SandboxNetwork
{
    public class LandmarkBuilding : Building
    {
        [SerializeField]
        protected GameObject locked = null;

        protected eBuildingState curBuildingState = eBuildingState.NONE;

        protected virtual void OnDisable()
        {
            if (locked != null)
                locked.transform.DOKill();
        }

        protected override void SetLockIcon(eBuildingState state)
        {
            if (curBuildingState == state)
                return;
            
            curBuildingState = state;

            bool bLocked = state == eBuildingState.LOCKED || state == eBuildingState.NOT_BUILT;

            if (locked != null)
            {
                if (bLocked && !locked.activeInHierarchy)
                {
                    locked.transform.DOKill();
                    locked.SetActive(true);
                    locked.transform.localScale = Vector3.one * 0.8f;
                    if (state == eBuildingState.NOT_BUILT)
                    {
                        locked.transform.DOScale(1.25f, 1.15f).SetEase(Ease.InCubic).SetLoops(-1, LoopType.Yoyo);
                    }
                }
                else if (!bLocked)
                {
                    locked.transform.DOKill();
                    locked.SetActive(false);
                }
            }
        }
    }
}
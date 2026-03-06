using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class UIObject : MonoBehaviour, IUIBase
    {
        protected eUIType curSceneType = eUIType.None;
        [SerializeField]
        protected eUIType curUIType = eUIType.None;
        [SerializeField]
        protected List<UIObject> uiChildrens = null;

        public virtual void Init()
        {
            if (uiChildrens == null || uiChildrens.Count < 1)
                return;

            var count = uiChildrens.Count;
            for (var i = 0; i < count; ++i)
            {
                if (uiChildrens[i] == null || uiChildrens[i] == this)
                    continue;

                uiChildrens[i].Init();
            }
        }
        public virtual void InitUI(eUIType targetType)
        {
            if (curSceneType != targetType)
                curSceneType = targetType;

            if (curSceneType > eUIType.None && curUIType.HasFlag(curSceneType))
				ReuseAnim();               
            else
                UnuseAnim();

            if (uiChildrens == null || uiChildrens.Count < 1)
                return;

            var count = uiChildrens.Count;
            for (var i = 0; i < count; ++i)
            {
                if (uiChildrens[i] == null)
                    continue;

                uiChildrens[i].InitUI(curSceneType);
            }
        }
        public virtual void RefreshUI() { } //전체 갱신부는 아래에서 상속으로 구현
        public virtual bool RefreshUI(eUIType targetType) //타입 갱신부는 아래에서 상속으로 구현
        {
            if (uiChildrens == null || uiChildrens.Count < 1)
                return curSceneType != targetType;

            var count = uiChildrens.Count;
            for (var i = 0; i < count; ++i)
            {
                if (uiChildrens[i] == null)
                    continue;

                uiChildrens[i].RefreshUI(targetType);
            }

            return curSceneType != targetType;
        } 

        public virtual void ShowEvent()
        {
            gameObject.SetActive(true);
        }
        public virtual void HideEvent()
        {
            gameObject.SetActive(false);
        }
        public virtual void ReuseAnim() { ShowEvent(); } //연출 필요한 경우 아래에서 상속으로 구현.
        public virtual void UnuseAnim() { HideEvent(); } //연출 필요한 경우 아래에서 상속으로 구현.

        public bool IsCurSceneType(eUIType type)
        {
            return curSceneType > eUIType.None && curSceneType.HasFlag(type);
        }
    }
}
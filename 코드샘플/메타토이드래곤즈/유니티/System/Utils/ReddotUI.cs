using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class ReddotUI: MonoBehaviour, EventListener<ReddotEvent>
    {
        public enum Anchor { LEFT, RIGHT }
        [SerializeField] eReddotEvent curUI = eReddotEvent.NONE;
        [SerializeField] eReddotEvent[] subUI;
        public static ReddotUI AddReddot(RectTransform parent, eReddotEvent type, Anchor anchor = Anchor.LEFT)
        {
            GameObject obj = Instantiate(ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "ReddotObject"), parent);
            switch(anchor)
            {
                case Anchor.LEFT:
                    (obj.transform as RectTransform).anchorMin = Vector2.up;
                    (obj.transform as RectTransform).anchorMax = Vector2.up;
                    obj.transform.localPosition = new Vector2((parent.sizeDelta.x * -0.5f) + 10, (parent.sizeDelta.x * 0.5f) + - 10);
                    break;
                case Anchor.RIGHT:
                    (obj.transform as RectTransform).anchorMin = Vector2.one;
                    (obj.transform as RectTransform).anchorMax = Vector2.one;
                    obj.transform.localPosition = (parent.sizeDelta * 0.5f) + new Vector2(-10, -10);
                    break;
            }

            var ret = obj.GetComponent<ReddotUI>();
            ret.Set(type);

            return ret;
        }

        public void Set(eReddotEvent type)
        {
            curUI = type;
        }
        private void OnEnable()
        {
            EventManager.AddListener(this);
            Refresh();
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener(this);
        }

        public void OnEvent(ReddotEvent eventType)
        {
            if (eventType.type == curUI)
            {
                Refresh();
            }
            else
            {
                foreach (var type in subUI)
                {
                    if (eventType.type == type)
                    {
                        Refresh();
                    }
                }
            }
        }

        void Refresh()
        {
            bool reddot = ReddotManager.IsOn(curUI);
            if(!reddot)
            {
                foreach (var type in subUI)
                {
                    reddot = ReddotManager.IsOn(type);
                    if (reddot)
                        break;
                }
            }

            gameObject.SetActive(reddot);
        }
    }
}

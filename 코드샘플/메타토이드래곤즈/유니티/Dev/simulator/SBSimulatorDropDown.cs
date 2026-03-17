using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace SandboxNetwork
{
    public class SBSimulatorDropDown : TMP_Dropdown
    {
        public delegate void func();

        private func destroyBlock = null;
        public func DestroyBlock { get { return destroyBlock; } set { destroyBlock = value; } }

        public override void OnCancel(BaseEventData eventData)
        {
            base.OnCancel(eventData);

            Debug.Log("cancel");
        }

        protected override void DestroyBlocker(GameObject blocker)//드롭다운 하위 메뉴가 꺼질 때마다 호출됨 (select 이벤트 이후 호출).
        {
            base.DestroyBlocker(blocker);

            if(destroyBlock != null)
            {
                destroyBlock();
            }
        }
        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            if(s_SelectableCount < 0)
            {
                s_SelectableCount = 0;
            }

            base.OnDisable();
        }
    }
}

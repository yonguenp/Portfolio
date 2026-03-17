using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//버튼 호버 기능이나 타켓 이미지, 타겟 라벨 색상이나 이미지 교체 할 때 쓰는 용도, 일단은 마우스 오버 상태만 추가
namespace SandboxNetwork
{
    public class ButtonEventHandler : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler
    {
        [SerializeField]
        Button targetButton = null;
        [SerializeField]
        Text targetLabel = null;

        [Space(10)]
        [Header("Change Setting")]
        [SerializeField]
        private Color normalColor = new Color();
        [SerializeField]
        private Color hoverColor = new Color();
        [SerializeField]
        private Color disableColor = new Color();

        bool isSetCustomEvent = false;
        protected Action enterAction = null;
        protected Action exitAction = null;

        public void SetAction(Action _enterAction , Action _exitAction)
        {
            enterAction = _enterAction;
            exitAction = _exitAction;
            isSetCustomEvent = true;
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            var pointerTarget = eventData.pointerEnter;

            if (pointerTarget == null)
            {
                return;
            }

            if(!isSetCustomEvent)
            {
                if(targetLabel != null)
                    targetLabel.color = GetColorByState(true, pointerTarget);
            }
            else
            {
                if (enterAction != null)
                    enterAction();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            var pointerTarget = eventData.pointerEnter;

            if(pointerTarget == null)
            {
                return;
            }
            if (!isSetCustomEvent)
            {
                if(targetLabel != null)
                    targetLabel.color = GetColorByState(false, pointerTarget);
            }
            else
            {
                if (exitAction != null)
                    exitAction();
            }
        }

        bool isInteractableButton(Button btn)
        {
            if(btn == null)
            {
                return false;
            }

            return btn.interactable;
        }
        public void SetStateByInteractable(Button btn)
        {
            if(btn != targetButton)
            {
                return;
            }

            if(targetLabel != null)
            {
                targetLabel.color = GetColorByInteract();
            }
        }


        Color GetColorByState(bool isEnter, GameObject enterObject)
        {
            if(targetLabel == null)
            {
                return Color.white;
            }

            Color color = Color.white;

            if (enterObject.GetComponent<Button>())//버튼 타겟과 동일하면
            {
                if (isInteractableButton(targetButton))//상호작용 가능 상태
                {
                    color = isEnter ? hoverColor : normalColor;
                }
                else
                {
                    color = disableColor;
                }
            }

            return color;
        }

        Color GetColorByInteract()
        {
            Color color = new Color();
            if (isInteractableButton(targetButton))//상호작용 가능 상태
            {
                color = normalColor;
            }
            else
            {
                color = disableColor;
            }
            return color;
        }
    }
}

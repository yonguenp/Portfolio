using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ButtonChangeChildColor : MonoBehaviour
    {
        [SerializeField]
        public Color activateColor = new Color();

        [SerializeField]
        public Color deActivateColor = new Color();

        [SerializeField]
        Text targetLabel = null;

        [SerializeField]
        Image targetSprite = null;


        public void refreshColor()
        {
            var currentButton = this.GetComponent<Button>();
            if (currentButton != null)
            {
                var isInteract = currentButton.interactable;
                if (this.targetLabel != null)
                {
                    if (isInteract)
                    {
                        this.targetLabel.color = this.activateColor;
                    }
                    else
                    {
                        this.targetLabel.color = this.deActivateColor;
                    }
                }
                if (this.targetSprite != null)
                {
                    if (isInteract)
                    {
                        this.targetSprite.color = this.activateColor;
                    }
                    else
                    {
                        this.targetSprite.color = this.deActivateColor;
                    }
                }
            }
        }
    }
}

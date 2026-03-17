using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
public static class ButtonExtensions
{
    public enum eChangeFlag
    {
        NONE,
        JUSTCOLOR,
        JUSTSPRITE,
    }

    public static Dictionary<MaskableGraphic, Color> originColors = new Dictionary<MaskableGraphic, Color>();

    public static void SetInteractable(this Button _targetButton, bool interactable)
    {
        var normalColor = _targetButton.colors.normalColor;
        var disabledColor = _targetButton.colors.disabledColor;

        var changeColor = interactable ? normalColor : disabledColor;
        _targetButton.interactable = interactable;

        var childrenComp = _targetButton.GetComponentsInChildren<MaskableGraphic>();
        if (childrenComp == null || childrenComp.Length <= 0)
        {
            return;
        }

        for (int i = 0; i < childrenComp.Length; i++)
        {
            var targetGraphic = childrenComp[i];
            if (targetGraphic.gameObject == _targetButton.gameObject)
                continue;

            var currentColor = targetGraphic.color;
            if (originColors.ContainsKey(targetGraphic))
            {
                currentColor = originColors[targetGraphic];
            }
            else
            {
                originColors[targetGraphic] = currentColor;
                targetGraphic.gameObject.AddComponent<ButtonColorChangeComponent>();
            }

            targetGraphic.color = currentColor * changeColor;
        }
    }

    public static void SetButtonSpriteState(this Button _targetButton, bool _isNormal)
    {
        _targetButton.GetComponent<Image>().sprite = _isNormal ? _targetButton.spriteState.highlightedSprite : _targetButton.spriteState.disabledSprite;
    }
}

public class ButtonColorChangeComponent : MonoBehaviour
{
    private void OnDestroy()
    {
        var graphic = GetComponent<MaskableGraphic>();
        if (graphic != null)
        {
            if (ButtonExtensions.originColors.ContainsKey(graphic))
            {
                ButtonExtensions.originColors.Remove(graphic);
            }
        }
    }
}

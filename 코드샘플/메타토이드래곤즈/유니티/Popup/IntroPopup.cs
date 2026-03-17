using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class IntroPopup : Popup<PopupData>
{
    [SerializeField] Text text;
    public override void InitUI()
    {
        Color color = text.color;
        color.a = 0.0f;
        text.color = color;
        color.a = 1.0f;

        text.DOColor(color, 1.0f).OnComplete(MoveText);        
    }

    void MoveText()
    {
        RectTransform textRt = text.transform as RectTransform;
        textRt.DOLocalMoveY(textRt.sizeDelta.y, 30.0f).OnComplete(OnSkip);
    }

    public void OnSkip()
    {
        ClosePopup();
    }
}

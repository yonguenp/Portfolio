using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class ServerWaitPopup : MonoBehaviour
{
    public Image BackGround;
    public Image WaitImage;

    private void OnEnable()
    {
        BackGround.DOKill();
        BackGround.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);

        BackGround.DOColor(new Color(0.0f, 0.0f, 0.0f, 0.3f), 1.0f).SetDelay(1.0f);

        WaitImage.DOKill();
        WaitImage.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);


        WaitImage.DOColor(new Color(1.0f, 1.0f, 1.0f, 1.0f), 1.0f).SetDelay(2.0f).OnComplete(() => {
            WaitImage.DOColor(new Color(1.0f, 1.0f, 1.0f, 0.7f), 1.0f).SetLoops(-1, LoopType.Yoyo);
        });
    }
}

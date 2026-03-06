using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BatteryNotifyMessage : GameLogNotifyMessage
{
    [SerializeField] Text batteryCountText = null;
    [SerializeField] Text survivorText = null;
    [SerializeField] UIPortraitCharacter portrait = null;
    public void Initialize(int cnt, string name, int charType)
    {
        batteryCountText.text = cnt.ToString();
        survivorText.text = name;
        portrait.SetPortrait(charType);

        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        gameObject.SetActive(true);

        StartCoroutine(FadeOutCoroutine());
    }
}

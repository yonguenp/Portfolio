using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class KillNotifyMessage : GameLogNotifyMessage
{
    [SerializeField] Text killerText = null;
    [SerializeField] Text victimText = null;
    [SerializeField] UIPortraitCharacter killer_portrait = null;
    [SerializeField] UIPortraitCharacter victim_portrait = null;

    [SerializeField] Image icon = null;

    public void Initialize(CharacterObject killer, CharacterObject victim)
    {
        killerText.text = killer.UserName;
        victimText.text = victim.UserName;
        if(icon != null)
            icon.transform.DOShakeRotation(0.3f);

        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        gameObject.SetActive(true);

        if (killer_portrait != null)
            killer_portrait.SetPortrait(killer.CharacterType);

        if (victim_portrait != null)
        {
            victim_portrait.SetPortrait(victim.CharacterType);
            victim_portrait.SetColor(new Color(1.0f, 0.2f, 0.2f));
        }

        StartCoroutine(FadeOutCoroutine());
    }
}

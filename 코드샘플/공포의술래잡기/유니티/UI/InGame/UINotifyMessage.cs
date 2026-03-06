using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UINotifyMessage : MonoBehaviour
{
    [SerializeField] BatteryNotifyMessage battery = null;
    [SerializeField] KillNotifyMessage kill = null;

    private void Awake()
    {
        battery.gameObject.SetActive(false);
        kill.gameObject.SetActive(false);
    }

    public void CreateKillMessage(CharacterObject killer, CharacterObject victim)
    {
        var message = Instantiate(kill);
        message.SetTime();
        message.transform.SetParent(transform);
        message.Initialize(killer, victim);
    }

    public void CreateBatteryMessage(int cnt, string name, int charType)
    {
        var message = Instantiate(battery);
        message.SetTime();
        message.transform.SetParent(transform);
        message.Initialize(cnt, name, charType);

        //Managers.Sound.Play("effect/EF_IN_BATTERY");
    }
}

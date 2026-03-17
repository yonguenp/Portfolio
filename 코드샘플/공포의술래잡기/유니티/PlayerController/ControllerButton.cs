using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControllerButton : MonoBehaviour
{
    [SerializeField]
    Button targetButton;
    [SerializeField]
    Image coolTimer;
    bool interactabled = false;

    Coroutine buttonCooltime = null;

    private void Start()
    {
        coolTimer.fillAmount = 0.0f;
    }
    public void SetInteractable(bool enable)
    {
        interactabled = enable;

        if (buttonCooltime == null)
        {
            targetButton.interactable = enable;
            //if (targetButton.interactable)
            coolTimer.fillAmount = 0.0f;
        }
    }

    public void OnButtonEventCooltime()
    {
        buttonCooltime = StartCoroutine(ButtonCoolTimeCoroutine());
    }

    IEnumerator ButtonCoolTimeCoroutine()
    {
        targetButton.interactable = false;

        float waitTime = 0.5f;
        float time = 0.0f;
        coolTimer.fillAmount = 1.0f;
        while (coolTimer.fillAmount > 0)
        {
            time += Time.deltaTime;
            coolTimer.fillAmount = Mathf.Max((1.0f - time / waitTime), 0f);
            yield return new WaitForEndOfFrame();
        }

        if (Game.Instance.PlayerController.Character.IsVehicle)
            targetButton.interactable = true;
        else if (Game.Instance.PlayerController.Character.State == SBSocketSharedLib.CreatureStatus.Hiding)
            targetButton.interactable = true;
        else
            targetButton.interactable = interactabled;

        coolTimer.fillAmount = 0.0f;
        buttonCooltime = null;
    }
}

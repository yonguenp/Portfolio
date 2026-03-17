using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkillCancelDetector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    bool needToCancel = false;
    List<ControllerJoystick> joysticks = new List<ControllerJoystick>();
    public bool NeedToCancel { get { return needToCancel; } }

    public void AddJoystick(ControllerJoystick joystick)
    {
        joysticks.Add(joystick);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //SBDebug.Log("SkillCancelDetector Enter");
        needToCancel = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //SBDebug.Log("SkillCancelDetector Exit");
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
        foreach(var touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Ended)
                return;
        }
#endif
        needToCancel = false;
    }

    private void OnEnable()
    {
        needToCancel = false;
    }

    private void OnDisable()
    {
        needToCancel = false;
    }

    private void Update()
    {
        foreach(ControllerJoystick joystick in joysticks)
        {
            if (joystick.IsTouch)
                return;
        }

        SetActive(false);
    }

    public void SetActive(bool enable)
    {
        gameObject.SetActive(enable);
    }

    public bool IsActive()
    {
        return gameObject.activeInHierarchy;
    }
}

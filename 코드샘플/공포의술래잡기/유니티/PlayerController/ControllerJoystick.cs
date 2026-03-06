using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum JoystickInputType
{
    Move = 0,
    Normal = 1,
    ActiveSkill = 2,
    Emotion = 3,
}

public class ControllerJoystick : VariableJoystick
{
    [SerializeField] private JoystickInputType type;
    [SerializeField] SkillCancelDetector cancelDetector = null;
    public Image CooltimeImage;
    public JoystickInputType InputType { get { return type; } }

    IControllerListener controller;
    public bool IsTouch { get; private set; } = false;
    private Vector2 initAnchoredPos;
    
    protected override void Start()
    {
        base.Start();
        initAnchoredPos = background.anchoredPosition;
        ChangeJoystickType(GameConfig.Instance.OPTION_JOYSTICK);
        IsTouch = false;
        cancelDetector?.AddJoystick(this);
        if (CooltimeImage != null) CooltimeImage.fillAmount = 0f;
    }

    public void ChangeJoystickType(JoystickType type)
    {
        SetMode(type);
        background.gameObject.SetActive(true);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        IsTouch = true;
        if (TouchAvailable())
            controller.OnPadEvent((int)type, TouchPhase.Began, Vector2.zero, 0);

        if (cancelDetector != null)
        {
            PlayerController playerController = controller as PlayerController;
            if (playerController != null)
            {
                if (playerController.IsEnableSkill(type == JoystickInputType.Normal ? PlayerController.SKILL_TYPE.NORMAL_ATK : PlayerController.SKILL_TYPE.ACTIVE_SKILL))
                {
                    cancelDetector.SetActive(true);
                }
            }
        }
    }

    public void SetController(IControllerListener listener)
    {
        controller = listener;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        // SBDebug.Log("ControllerJoystick OnPointerUp");
        // if (eventData.pointerId != id) return;
        if (TouchAvailable())
        {
            if (type == JoystickInputType.Move)
            {
                controller.OnPadEvent((int)type, TouchPhase.Ended, Vector2.zero, 0);
            }
            else
            {
                if (!cancelDetector.NeedToCancel)
                {
                    var vec = new Vector2(Horizontal, Vertical);
                    controller.OnPadEvent((int)type, TouchPhase.Ended, vec, vec.magnitude);
                }
                else
                {
                    Game.Instance.PlayerController.RemoveSkillRangeGuideUI();
                    Game.Instance.UIGame.HideSkillGuide();
                }
            }
        }

        base.OnPointerUp(eventData);
        background.gameObject.SetActive(true);
        background.anchoredPosition = initAnchoredPos;
        IsTouch = false;
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (IsTouch && TouchAvailable())
        {
            base.OnDrag(eventData);
        }
    }

    private void Update()
    {
        if (IsTouch && TouchAvailable())
        {
            Vector2 vec = new Vector2(Horizontal, Vertical);
            controller.OnPadEvent((int)type, TouchPhase.Moved, vec, vec.magnitude);

            if(cancelDetector != null && !cancelDetector.IsActive())
            {
                PlayerController playerController = controller as PlayerController;
                if (playerController != null)
                {
                    if (playerController.IsEnableSkill(type == JoystickInputType.Normal ? PlayerController.SKILL_TYPE.NORMAL_ATK : PlayerController.SKILL_TYPE.ACTIVE_SKILL))
                    {
                        cancelDetector.SetActive(true);
                    }
                }
            }
        }
        else
        {
            if (type == JoystickInputType.Move)
            {
                if (IsTouch && !TouchAvailable())
                {
                    controller.OnPadEvent((int)type, TouchPhase.Moved, Vector2.zero, 0);
                }
            }
        }
    }

    public void SetPadIconSprite(Sprite sprite)
    {
        if (sprite == null) 
            return;
        
        handle.GetComponent<Image>().sprite = sprite;
    }

    private bool TouchAvailable()
    {
        if (Game.Instance.PlayerController.ObserverMode)
        {
            return type == JoystickInputType.Normal;
        }

        // 스킬 사용 불가 상태 (이동 불가 포함)
        if (!Game.Instance.PlayerController.Character.IsSkillAvailable)
        {
            return false;
        }

        // 그로기 상태일때
        if (Game.Instance.PlayerController.Character.State == SBSocketSharedLib.CreatureStatus.Groggy)
        {
            return false;
        }

        if(InputType == JoystickInputType.ActiveSkill)
        {
            if (!Game.Instance.PlayerController.Character.IsActiveSkillAvailable)
                return false;
        }

        return true;
    }
}
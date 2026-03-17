using UnityEngine;

public class ControllerKeyboard : MonoBehaviour, IController
{
    protected IControllerListener target = null;

    Vector2 prevMoveKeyDir = Vector2.zero;
    Vector2 curMoveKeyDir = Vector2.zero;
    Vector2 curTargetDir = Vector2.down;
    bool[] EventAction = new bool[2];

    private void Start()
    {
        for (int i = 0; i < 2; ++i)
            EventAction[i] = false;
    }

    private void Update()
    {
        var controller = Game.Instance.PlayerController;
        if (controller == null) return;

        if (controller.ObserverMode)//옵저버모드일때는
        {
            UpdateActionInput();
            return;
        }

        if (controller.Character.State == SBSocketSharedLib.CreatureStatus.Groggy)
            return;

        UpdateMove();
        UpdateDebuging();
    }

    void UpdateDebuging()
    {
#if UNITY_EDITOR

#endif
    }
    void UpdateMove()
    {
        if (target == null) return;

        UpdateControllInput();

        if (!Game.Instance.PlayerController.Character.IsMovable)
        {
            prevMoveKeyDir = Vector2.zero;
            return;
        }

        UpdateMoveInput();

        if (prevMoveKeyDir != curMoveKeyDir)
        {
            prevMoveKeyDir = curMoveKeyDir;
            target.OnMove(curMoveKeyDir);
        }
    }

    void UpdateMoveInput()
    {
        Vector2 dir = Vector2.zero;

        if (Input.GetKey(KeyCode.W))
            dir += Vector2.up;
        if (Input.GetKey(KeyCode.S))
            dir += Vector2.down;
        if (Input.GetKey(KeyCode.A))
            dir += Vector2.left;
        if (Input.GetKey(KeyCode.D))
            dir += Vector2.right;

        if (Game.Instance.PlayerController.Character.IsConfused)
        {
            dir = -dir;
        }

        curMoveKeyDir = dir.normalized;

        if (dir != Vector2.zero)
        {
            curTargetDir = prevMoveKeyDir;
        }
    }

    void UpdateControllInput()
    {
        UpdateActionInput();

        if (Input.GetKeyUp(KeyCode.K) || Input.GetKeyUp(KeyCode.LeftShift))
        {
            if (Game.Instance.PlayerController.Character.IsActiveSkillAvailable)
            {
                target.OnPadEvent(2, TouchPhase.Ended, curTargetDir, 1);
            }
            
        }
        else if (Input.GetKey(KeyCode.K) || Input.GetKey(KeyCode.LeftShift))
        {
            target.OnPadEvent(2, TouchPhase.Began, curTargetDir, 1);
        }

        if (Input.GetKeyUp(KeyCode.L) || Input.GetKeyUp(KeyCode.LeftControl))
        {
            Game.Instance.PlayerController.ControllerPad.OnNearestObjectButton();
        }

#if UNITY_EDITOR
        if (Input.GetKeyUp(KeyCode.Insert))
        {
            Game.Instance.PlayerController.ChangeSurvivorObserverMode();
        }
        if (Input.GetKeyUp(KeyCode.Delete))
        {
            Game.Instance.UIGame.ShowCenterMessage(1, Managers.UserData.MyUserID.ToString());
        }
        if (Input.GetKeyDown(KeyCode.Home))
        {
            Game.Instance.UIGame.CreateBatteryMessage(5, "TEST", 10001);
        }
        else if (Input.GetKeyDown(KeyCode.End))
        {
            //Game.Instance.UIGame.CreateKillMessage("TESTKILLER", "TESTVICTIM");
        }
#endif
    }



    void UpdateActionInput()
    {
        if (Input.GetKeyUp(KeyCode.J) || Input.GetKeyUp(KeyCode.Space))
        {
            target.OnPadEvent(1, TouchPhase.Ended, curTargetDir, 1);
        }
        else if (Input.GetKey(KeyCode.J) || Input.GetKey(KeyCode.Space))
        {
            target.OnPadEvent(1, TouchPhase.Began, curTargetDir, 1);
        }
    }

    public void InitController(IControllerListener listener)
    {
        target = listener;
    }
}

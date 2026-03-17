using Coffee.UIExtensions;
using SBSocketSharedLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControllerPad : MonoBehaviour, IController
{
    public List<ControllerJoystick> Joysticks;
    public ControllerButton objectButton;
    public Image objectButtonIcon;
    [SerializeField] GameObject[] keyGuideForWindows;

    bool isMobile = false;
    IControllerListener _listener = null;
    PropController curNearestObjectPropController = null;

    public RectTransform[] dynamicControllPad;

    public Button emotionButton;
    public Image emotionFillImage;

    public UIParticle tutorialEffect;
    private void SetKeyGuideVisible()
    {
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
        isMobile = true;
#endif


#if !UNITY_EDITOR && UNITY_ANDROID
        var unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var currentActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
        var packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager");
        var isPC = packageManager.Call<bool>("hasSystemFeature", "com.google.android.play.feature.HPE_EXPERIENCE");
        if (isPC)
        {
            isMobile = false;
        }
#endif

        foreach (GameObject guideUI in keyGuideForWindows)
        {
            guideUI.SetActive(!isMobile);
            //guideUI.SetActive(false);
        }
    }

    public void InitController(IControllerListener listener)
    {
        _listener = listener;

        RoomPlayerInfo info = Managers.PlayData.GetMyRoomPlayerInfo();
        UserCharacterData data = null;
        if (info != null)
        {
            data = Managers.UserData.GetMyCharacterInfo(info.SelectedCharacter.CharacterType);
        }

        foreach (var joystick in Joysticks)
        {
            joystick.SetController(_listener);
            if (joystick.InputType == JoystickInputType.ActiveSkill)
            {
                if (data != null)
                {
                    var skillData = data.GetSkillData();
                    if (skillData != null)
                    {
                        Sprite sprite = skillData.GetIcon();
                        if (sprite != null)
                            joystick.SetPadIconSprite(sprite);
                    }
                }
            }

            if (joystick.InputType == JoystickInputType.Normal)
            {
                if (data != null)
                {
                    var skillData = data.GetAtkSkillData();
                    if (skillData != null)
                    {
                        Sprite sprite = skillData.GetIcon();
                        if (sprite != null)
                            joystick.SetPadIconSprite(sprite);
                    }
                }
            }
        }

        tutorialEffect.Stop();

        SetNearestObject(null);
        SetKeyGuideVisible();
        ControllerPadSizeSet();
    }

    public void ChangeSkillIcon(PlayerController.SKILL_TYPE type, SkillGameData skillData)
    {
        ControllerJoystick candidate = null;
        foreach (var joystick in Joysticks)
        {
            if (joystick.InputType == JoystickInputType.Normal
                && type == PlayerController.SKILL_TYPE.NORMAL_ATK)
            {
                candidate = joystick;
            }
            else if (joystick.InputType == JoystickInputType.ActiveSkill
                && type == PlayerController.SKILL_TYPE.ACTIVE_SKILL)
            {
                candidate = joystick;
            }
        }

        if (candidate != null && skillData != null)
        {
            candidate.SetPadIconSprite(skillData.GetIcon());
        }
    }

    public void SetNearestObject(PropController propCont)
    {
        objectButtonIcon.gameObject.SetActive(false);
        curNearestObjectPropController = propCont;
        if (propCont == null)
        {
            objectButton.SetInteractable(false);
            return;
        }

        var objType = propCont.MapObjectType;
        bool interaction = false;

        if (objType == ObjectGameData.MapObjectType.Vehicle)
        {
            interaction = true;
        }

        if (objType == ObjectGameData.MapObjectType.Hide)
        {
            interaction = true;
        }
        if (objType == ObjectGameData.MapObjectType.Vent)
        {
            interaction = true;
        }

        if (interaction == false)
        {
            curNearestObjectPropController = null;
            objectButton.SetInteractable(false);
            return;
        }

        objectButtonIcon.gameObject.SetActive(true);
        objectButton.SetInteractable(true);
    }

    public void OnNearestObjectButton()
    {
        bool buttonAction = false;
        if (Game.Instance.PlayerController.Character.IsVehicle)
        {
            buttonAction = Game.Instance.PlayerController.RequestGetOffVehicle();
        }
        else if (curNearestObjectPropController != null)
        {
            buttonAction = Game.Instance.PlayerController.OnUseMapObject(curNearestObjectPropController);
        }
        else if (Game.Instance.PlayerController.Character.State == CreatureStatus.Hiding)
        {
            buttonAction = Game.Instance.PlayerController.RequestGetOffHide();
        }

        if (buttonAction)
            objectButton.OnButtonEventCooltime();
    }

    public void SetObserverMode(bool bObserver, bool escaped)
    {
        for (int i = 0; i < Joysticks.Count; i++)
        {
            if (escaped && i == 1)  //옵저버 모드일때 액션 키로 시점변환 시켜줘야하기 때문에
                continue;

            if (Joysticks[i] != null)
            {
                Joysticks[i].gameObject.SetActive(!bObserver);
            }
        }

        objectButton.gameObject.SetActive(!bObserver);
    }

    public void ChangeJoystickType(JoystickType type)
    {
        foreach (var joystick in Joysticks)
        {
            joystick.ChangeJoystickType(type);
        }
    }

    public void ControllerPadSizeSet()
    {
        if (Screen.width <= 1920 && Screen.height <= 1080)
            return;
        if (dynamicControllPad == null)
            return;

        var ratioX = (float)Screen.width / 1920;
        var ratioY = (float)Screen.height / 1080;

        var ratio = Mathf.Min(ratioX, ratioY);

        foreach (var item in dynamicControllPad)
        {
            item.sizeDelta = new Vector2(item.sizeDelta.x * ratio, item.sizeDelta.y * ratio);
            // item.transform.localScale = new Vector3(ratio, ratio, 1f);
        }
    }

    public void EmoticonButton(int id)
    {
        if (!Game.Instance.UIGame.CoolCheck())
            return;

        int itemNo = 0;
        switch (id)
        {
            case 1:
                itemNo = CacheUserData.GetInt("Emotion1", 100);
                break;
            case 2:
                itemNo = CacheUserData.GetInt("Emotion2", 101);
                break;
            case 3:
                itemNo = CacheUserData.GetInt("Emotion3", 102);
                break;
            case 4:
                itemNo = CacheUserData.GetInt("Emotion4", 103);
                break;
            default:
                return;
        }

        Managers.GameServer.SendEmotion((ushort)itemNo);

        Game.Instance.UIGame.EmotionCoolTime();
    }
}

using SandboxPlatform.SAMANDA;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class BaseScene : MonoBehaviour
{
    public SceneType SceneType { get; protected set; } = SceneType.Unknown;
    public AudioClip BGM;
    SoundController uiSoundController;
    [SerializeField]
    Camera uiCamera = null;

    protected void Awake()
    {
        Init();
    }

    protected virtual void Init()
    {
        StartBackgroundMusic();

        if (uiSoundController == null)
        {
            uiSoundController = gameObject.AddComponent<SoundController>();
            uiSoundController.Init(0, false, SoundController.PlayType.OnlyMe);
        }

        if (uiCamera == null)
        {
            SBDebug.LogError("uiCamera is Null");
        }

        PopupCanvas.Instance.SetUICamera(uiCamera);


        //if (UICanvas != null)
        //{
        //    foreach (var canvas in UICanvas)
        //    {
        //        var ratio = Screen.safeArea.size.x / Screen.safeArea.size.y;
        //        // 가로가 더 길다.
        //        if (ratio >= 16f / 9f)
        //        {
        //            // 가로가 더 길면 height에 맞춘다
        //            canvas.matchWidthOrHeight = 1f;
        //        }
        //        else
        //        {
        //            // 가로가 더 길면 width에 맞춘다
        //            canvas.matchWidthOrHeight = 0f;
        //        }
        //    }
        //}
    }

    public virtual void StartBackgroundMusic(bool clearPopup = true)
    {
        Object obj = GameObject.FindObjectOfType(typeof(EventSystem));
        if (obj == null)
            Managers.Resource.Instantiate("UI/EventSystem").name = "@EventSystem";

        if (clearPopup)
            PopupCanvas.Instance.SceneChanged();

        if (BGM != null)
        {
            Managers.Sound.Play(BGM, Sound.Bgm);
        }
    }

    public virtual void PlayUISound(string path)
    {
        uiSoundController?.Play(path, SoundController.PlayType.UI);
    }

    public abstract void Clear();

    public virtual Camera GetUICamera()
    {
        return uiCamera;
    }

    public virtual void Update()
    {
        if (!PopupCanvas.Instance.IsOpeningPopup())
        {
            if (Input.GetKeyDown(KeyCode.Escape) && !SAMANDA.Instance.UI.gameObject.activeInHierarchy)
            {
                PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.QUIT_POPUP);
            }
        }
    }
}

using UnityEngine;
using SBCommonLib;

public class RoomScene : BaseScene
{
    [SerializeField]
    LoadingUI LoadingUI;

    public bool isSurvivorList = false;
    enum eRoomState
    {
        NONE,
        LOADING,    //전체 레디후 로딩 시작
    };

    eRoomState curRoomState = eRoomState.NONE;
    private int curSurvivor = 0;
    private int curChaser = 0;
    private Coroutine loadCoroutine = null;
    void Start()
    {
        if (curSurvivor == 0)
            curSurvivor = Managers.UserData.MyDefaultSurvivorCharacter;
        if (curChaser == 0)
            curChaser = Managers.UserData.MyDefaultChaserCharacter;

        OnRoomLoading();
    }

    public override void Clear() { }

    public override void StartBackgroundMusic(bool clearPopup = true)
    {
        BGM = Managers.Resource.LoadAssetsBundle<AudioClip>("AssetsBundle/Sounds/bgm/BGM_MATCHING_ROOM");
        base.StartBackgroundMusic(clearPopup);
    }

    public void OnAllReady(long serverTimestamp)
    {
        GameRoom gr = Managers.PlayData.CreateGameRoom();
        gr.GameTime.SetBaseTime(serverTimestamp, SBUtil.GetCurrentMilliSecTimestamp());

        PopupCanvas.Instance.ClearAll();
    }

    public void OnRoomLoading()
    {
        SetUIState(eRoomState.LOADING);

        PopupCanvas.Instance.ClearAll();

        LoadingUI.Init(this);
        if (loadCoroutine != null)
            StopCoroutine(loadCoroutine);
        loadCoroutine = StartCoroutine(LoadingUI.GameLoad());
    }

    void SetUIState(eRoomState state)
    {
        curRoomState = state;

        switch (curRoomState)
        {
            case eRoomState.LOADING:
                LoadingUI.SetActive(true);
                break;
            default:    //여기로 오면안됩니다.
                LoadingUI.SetActive(false);
                break;
        }
    }

    public void ClearLoading()
    {
        if (loadCoroutine != null)
            StopCoroutine(loadCoroutine);
    }

    public override void Update()
    {

    }
}

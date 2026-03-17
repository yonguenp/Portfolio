using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Spine.Unity;

public class LoadingUI : MonoBehaviour
{
    static private LoadingUI curLoadingUI = null;
    static public void ClearLoadingUI()
    {
        if (curLoadingUI != null)
        {
            if (curLoadingUI.op == null || curLoadingUI.op.allowSceneActivation == false)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Start");
                return;
            }

            curLoadingUI.Clear();
            Destroy(curLoadingUI.gameObject);
            curLoadingUI = null;

            Game.Instance.GameRoom.ReadyComplete();
        }
    }

    static public bool IsLoading { get { return curLoadingUI != null; } }


    [SerializeField]
    RoomScene scene;

    [SerializeField]
    SkeletonGraphic background;
    [SerializeField]
    PlayerSlot[] loadingSurvivorSlot;
    [SerializeField]
    PlayerSlot[] loadingChaserSlot;
    [SerializeField]
    GameObject SurvivorPanel;
    [SerializeField]
    GameObject ChaserPanel;

    [SerializeField]
    Image LoadProgress;
    [SerializeField]
    Text LoadProgressText;
    [SerializeField]
    GameObject SurvivorNotice;
    [SerializeField]
    GameObject ChaserNotice;
    [SerializeField]
    Text loadPlayMapNameInfo;
    [SerializeField]
    GameObject rank_warning;
    [SerializeField]
    GameObject wait_friends;
    AsyncOperation op = null;
    bool playChaser = false;
    RoomScene roomScene = null;
    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
        Clear();

        curLoadingUI = this;
        DontDestroyOnLoad(gameObject);
    }

    void Clear()
    {
        SurvivorNotice.SetActive(false);
        ChaserNotice.SetActive(false);

        if (roomScene != null)
            roomScene.ClearLoading();
    }

    public void Init(RoomScene room)
    {
        if (room != null)
            roomScene = room;

        Clear();

        foreach (PlayerSlot slot in loadingChaserSlot)
        {
            slot.ClearUI();
        }
        foreach (PlayerSlot slot in loadingSurvivorSlot)
        {
            slot.ClearUI();
        }

        GameRoomInfo info = Managers.PlayData.GameRoomInfo;

        int chaserIndex = 0;
        int survivorIndex = 0;

        float chaserTotalRP = 0;
        float surTotalRP = 0;

        foreach (var chaserInfo in info.RoomInfo.ChaserList)
        {
            long id = long.Parse(chaserInfo.Key);
            SBSocketSharedLib.RoomPlayerInfo p = chaserInfo.Value;
            if (id == p.UserId)
            {
                if (id == Managers.UserData.MyUserID)
                {
                    SBDebug.Log($"chaserInfo {chaserInfo.Key}");
                    playChaser = true;
                    //p.SelectedCharacter.CharacterType = scene.GetSelectedChaser();
                }

                if (loadingChaserSlot.Length > chaserIndex)
                {
                    loadingChaserSlot[chaserIndex].SetSlotPlayerInLoding(p, true, chaserIndex);
                    chaserIndex++;
                }
            }

            if (p.DuoKey == 0)
                chaserTotalRP += p.RankPoint;
            else
                chaserTotalRP += Mathf.Max(info.RoomInfo.ChaserList[0].Value.RankPoint, info.RoomInfo.ChaserList[1].Value.RankPoint);
        }

        foreach (var survivorInfo in info.RoomInfo.SurvivorList)
        {
            long id = long.Parse(survivorInfo.Key);
            SBSocketSharedLib.RoomPlayerInfo p = survivorInfo.Value;
            if (id == p.UserId)
            {
                if (id == Managers.UserData.MyUserID)
                {
                    SBDebug.Log($"survivorInfo {survivorInfo.Key}");
                    playChaser = false;
                    //p.SelectedCharacter.CharacterType = scene.GetSelectedSurvivor();
                }

                if (loadingSurvivorSlot.Length > survivorIndex)
                {
                    loadingSurvivorSlot[survivorIndex].SetSlotPlayerInLoding(p, false, survivorIndex);
                    survivorIndex++;
                }
            }

            if (p.DuoKey == 0)
                surTotalRP += p.RankPoint;
            else
            {
                foreach (var item in info.RoomInfo.SurvivorList)
                {
                    if (item.Value.DuoKey == p.DuoKey && item.Value.UserId != p.UserId)
                    {
                        surTotalRP += Mathf.Max(item.Value.RankPoint, p.RankPoint);
                        break;
                    }
                }
            }
        }
        surTotalRP /= info.RoomInfo.SurvivorList.Count;
        chaserTotalRP /= info.RoomInfo.ChaserList.Count;

        Managers.PlayData.GameRank_Warning = false;
        bool checkRankWarning = false;
        MatchInfoPopup matchInfo = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.MATCH_INFO_POPUP) as MatchInfoPopup;
        if (matchInfo)
        {
            checkRankWarning = matchInfo.IsRankMode;
        }

        if (checkRankWarning)
        {
            if (surTotalRP > chaserTotalRP + 1000)
            {
                foreach (var item in info.RoomInfo.ChaserList)
                {
                    if (item.Value.UserId == Managers.UserData.MyUserID)
                        Managers.PlayData.GameRank_Warning = true;
                }
            }
            else if (surTotalRP + 1000 < chaserTotalRP)
            {
                foreach (var item in info.RoomInfo.SurvivorList)
                {
                    if (item.Value.UserId == Managers.UserData.MyUserID)
                        Managers.PlayData.GameRank_Warning = true;
                }

            }
            else
                Managers.PlayData.GameRank_Warning = false;
        }

        rank_warning.SetActive(Managers.PlayData.GameRank_Warning);
        loadPlayMapNameInfo.text = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.map_type, Managers.PlayData.GameRoomInfo.RoomInfo.MapNo).GetName();
        LoadProgress.fillAmount = 0.0f;
    }

    public IEnumerator GameLoad()
    {
        const float moveChaserX = 1280;
        const float moveSurvivorX = 960 * 0.5f;

        op = Managers.Scene.LoadSceneAsync(SceneType.Game, LoadSceneEnd);
        op.allowSceneActivation = false;

        background.AnimationState.ClearTracks();
        background.Initialize(true);
        var track = background.AnimationState.SetAnimation(0, "loading", false);

        RefreshProgress(0);

        yield return new WaitForEndOfFrame();

        // 데이터 로딩하는 부분 시작
        Managers.PlayData.ClearPlayerSkillData();
        foreach (var playerInfo in Managers.PlayData.RoomPlayers)
        {
            Managers.PlayData.AddPlayerActiveSkillData(playerInfo.UserId, playerInfo.SelectedCharacter.CharacterType);
            yield return new WaitForEndOfFrame();
        }
        Managers.Effect.Initialize();
        // 데이터 로딩하는 부분 종료

        RefreshProgress(4);

        yield return new WaitForSpineAnimationComplete(track);

        if (playChaser)
            track = background.AnimationState.SetAnimation(0, "loading_chaser", false);
        else
            track = background.AnimationState.SetAnimation(0, "loading_survivor", false);

        //GameObject NoticeTarget = SurvivorNotice;
        //if (playChaser)
        //{
        //    NoticeTarget = ChaserNotice;
        //}

        //NoticeTarget.SetActive(true);
        //Text TextNotice = NoticeTarget.transform.Find("Text").GetComponent<Text>();
        //Color TextColor = TextNotice.color;
        //TextColor.a = 0.0f;
        //TextNotice.color = TextColor;

        //Color from = playChaser ? Color.white : Color.black;
        //Color to = playChaser ? Color.black : Color.white;
        //NoticeTarget.GetComponent<Image>().color = from;
        //NoticeTarget.GetComponent<Image>().DOColor(to, 0.5f).OnComplete(() =>
        //{
        //    TextColor.a = 1.0f;
        //    TextNotice.DOColor(TextColor, 0.5f);
        //});

        RefreshProgress(5);
        yield return new WaitForSpineAnimationComplete(track);

        if (playChaser)
        {
            foreach (PlayerSlot slot in loadingSurvivorSlot)
            {
                slot.gameObject.SetActive(false);
            }
        }
        else
        {
            foreach (PlayerSlot slot in loadingChaserSlot)
            {
                slot.gameObject.SetActive(false);
            }
        }

        op.allowSceneActivation = true;

        //TextColor.a = 0.0f;
        //TextNotice.DOColor(TextColor, 0.5f);
        //NoticeTarget.GetComponent<Image>().color = to;
        //to.a = 0.0f;
        //NoticeTarget.GetComponent<Image>().DOColor(to, 0.5f);

        //NoticeTarget.SetActive(false);
        
        while (true)
        {
            RefreshProgress(6);
            yield return new WaitForEndOfFrame();
        }
    }

    public void EnterGame()
    {
        Managers.GameServer.SendGameServerEnterGame();

        if (wait_friends != null)
        {
            wait_friends.SetActive(true);
            Text text = wait_friends.GetComponent<Text>();
            if(text != null)
            {
                Color color = Color.white;
                text.color = color;
                color.a = 0.3f;

                text.DOKill();
                text.DOColor(color, 0.6f).SetLoops(-1, LoopType.Yoyo);
            }
        }
    }

    public void LoadSceneEnd(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode)
    {
        if (scene.name == "Game")
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= LoadSceneEnd;

            if (!Managers.Network.IsAlive())
                return;
            Game.Instance.InitGame();

            EnterGame();
        }
    }

    void RefreshProgress(int step)
    {
        float gauge = op.progress;

        if (step >= 5)
        {
            if (gauge >= 0.9f)
            {
                gauge = 1.0f;
                LoadProgressText.text = StringManager.GetString("로딩완료플레이어대기");
            }
            else
            {
                LoadProgressText.text = StringManager.GetString("로딩대기");
            }
        }
        else
        {
            LoadProgressText.text = StringManager.GetString("로딩중", (gauge * 100.0f).ToString("n0"));
        }

        LoadProgress.DOKill();
        LoadProgress.DOFillAmount(gauge, 1.0f);
    }
}

using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SandboxNetwork
{
    public class Game : SBPersistentSingleton<Game>
    {
        string ClientIP = string.Empty;
        // Start is called before the first frame update
        protected override void Awake()
        {
            base.Awake();

            DataBase.SetDefaultDB();
            SBGameManager.Instance.Init();
        }

        protected virtual void Start()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;

            UIManager.Instance.InitUI(eUIType.None);

            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
        private void OnDestroy()
        {
            StopAllCoroutines();
            DataBase.Destroy();
            Debug.LogWarning("destroyed Game Instance object");
        }
        // Update is called once per frame
        void Update()
        {
            SBGameManager.Instance.Update(Time.deltaTime);
            //#if (UNITY_EDITOR || UNITY_ANDROID) && DEBUG  //임시
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                //				if (SceneManager.GetActiveScene().name == "Town")
                //				{
                //					SystemPopup popup = PopupManager.OpenPopup<SystemPopup>();
                //					popup.setMessage("NOTICE", "Are you sure you want to quit the game?", "EXIT", "CANCEL");
                //					popup.SetCallBack(
                //#if UNITY_ANDROID
                //						() => Application.Quit(),
                //#elif UNITY_EDITOR
                //						() => EditorApplication.ExitPlaymode(),
                //#endif
                //						() => PopupManager.ClosePopup<SystemPopup>());
                //				}


                // 튜토리얼중인지 체크
                //if (TutorialManager.Instance == null || TutorialManager.Instance.IsPlayingTutorial)
                //{
                //	return;
                //}

                if (SceneManager.GetActiveScene().name.Equals("Splash"))
                {
                    return;
                }

                if (SceneManager.GetActiveScene().name.Equals("Start"))
                {
                    var popup = SystemLoadingPopup.Instance;

                    popup.InitInstance();
                    popup.SetMessage(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002420), StringData.GetStringByIndex(100002421), StringData.GetStringByIndex(100000200));
                    popup.SetCallBack(
                        SBFunc.Quit,
                        popup.ClosePopup
                    );
                    return;
                }

                if (TutorialManager.tutorialManagement != null && TutorialManager.tutorialManagement.IsPlayingTutorial)
                {

#if UNITY_EDITOR
                    TutorialManager.tutorialManagement.EndTutorialEvent();
                    if (PopupManager.IsExistPopup(typeof(ProductTutorialPopup)))
                        PopupManager.GetPopup<ProductTutorialPopup>().ClosePopup();
                    if (PopupManager.IsExistPopup(typeof(ProductManageTutorialPopup)))
                        PopupManager.GetPopup<ProductManageTutorialPopup>().ClosePopup();
                    if (PopupManager.IsExistPopup(typeof(AccelerationTutorialPopup)))
                        PopupManager.GetPopup<AccelerationTutorialPopup>().ClosePopup();
#endif
                    return;
                }

                if (PopupManager.OpenPopupCount > 0)
                {
                    IPopup popup = PopupManager.GetFirstPopup();
                    if (popup != null)
                    {
                        popup.BackButton();
                    }
                }
                else
                {
                    // 탐험씬 체크
                    if (AdventureManager.Instance.IsStartCheck() ||
                        SceneManager.GetActiveScene().name.Equals("AdventureReward") ||
                        // 아레나 씬 체크
                        SceneManager.GetActiveScene().name.Equals("ArenaColosseum") ||
                        SceneManager.GetActiveScene().name.Equals("ArenaResult") ||
                        // 요일던전 씬 체크
                        SceneManager.GetActiveScene().name.Equals("DailyBattle") ||
                        SceneManager.GetActiveScene().name.Equals("DailyReward") ||
                        // 월드보스 씬 체크
                        SceneManager.GetActiveScene().name.Equals("WorldBossBattle") ||
                        SceneManager.GetActiveScene().name.Equals("WorldBossResult") ||

                        // 로딩 끝났다는 정보와 씬 이동중, 네트워크 통신중 정보를 구분해 타운으로 돌아가는 기능
                        false == LoadingManager.Instance.IsLoadingEnd ||
                        NetworkManager.Instance.IsWait)
                    {
                        return;
                    }

                    bool isTownScene = SceneManager.GetActiveScene().name.Equals("Town");
                    if (!isTownScene)
                    {
                        LoadingManager.Instance.LoadLastestScene(eSceneEffectType.CloudAnimation);
                        return;
                    }

                    SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002420), StringData.GetStringByIndex(100002421), StringData.GetStringByIndex(100000200),
                        SBFunc.Quit,
                        () => PopupManager.ClosePopup<SystemPopup>()
                    );
                }
            }

            string nowIP = CChatServer.ClientIP;
            if (ClientIP != nowIP)
            {
                if (ClientIP == string.Empty)//최초
                {
                    ClientIP = nowIP;
                }
                else
                {
                    if (!string.IsNullOrEmpty(nowIP))
                    {
                        ClientIP = nowIP;

                        bool prevEnableP2E = User.Instance.ENABLE_P2E;
                        NetworkManager.Send("auth/check", null, (response) =>
                        {
                            // 유저의 cypto 관련 상태 정보
                            if (SBFunc.IsJTokenCheck(response["user_state"]))
                            {
                                User.Instance.UserData.UpdateUserState(response["user_state"].Value<int>());
                            }

                            if (prevEnableP2E != User.Instance.ENABLE_P2E)
                            {
                                SBGameManager.Instance.BackToLoginScene(false);
                            }
                        });
                    }
                }
            }

        }

        private void OnApplicationPause(bool pause)
        {
            if (!pause)
            {
                if (User.Instance.UserAccountData.UserNumber > 0 && User.Instance.UserAccountData.SessionToken != "")
                    NetworkManager.Send("ping", null, (jsonData) => TimeManager.Instance.RefreshObject());
            }
        }
    }
}

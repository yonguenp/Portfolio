using Newtonsoft.Json.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public class PausePopup : Popup<PopupData>
    {
        [SerializeField]
        private GameObject yesBtn;
        [SerializeField]
        private GameObject noBtn;
        private VoidDelegate okCallback = null;
        private VoidDelegate cancelCallback = null;

        #region OpenPopup
        private static PausePopup OpenPopup()
        {
            return PopupManager.OpenPopup<PausePopup>();
        }
        public static PausePopup OpenDailyPopup()
        {
            var popup = OpenPopup();
            if (popup != null)
            {
                EffectReceiverClearEvent.Send();
                Time.timeScale = 0f;
                popup.SetCallBack(DailyPauseOk, DailyPauseCancle);
            }
            return popup;
        }
        public static PausePopup OpenAdventurePopup()
        {
            var popup = OpenPopup();
            if (popup != null)
            {
                EffectReceiverClearEvent.Send();
                Time.timeScale = 0f;
                popup.SetCallBack(AdventurePauseOk, AdventurePauseCancle);
            }
            return popup;
        }
        public static PausePopup OpenArenaPopup()
        {
            var popup = OpenPopup();
            if (popup != null)
            {
                EffectReceiverClearEvent.Send();
                Time.timeScale = 0f;
                popup.SetCallBack(ArenaPauseOk, ArenaPauseCancle);
            }
            return popup;
        }
        public static PausePopup OpenWorldBossPopup()
        {
            var popup = OpenPopup();
            if (popup != null)
            {
                EffectReceiverClearEvent.Send();
                Time.timeScale = 0f;
                popup.SetCallBack(WorldBossPauseOk, WorldBossPauseCancel);
            }
            return popup;
        }
        #region Funcion
        private static void ArenaPauseOk()
        {
            Time.timeScale = ArenaManager.Instance.ColosseumData.Speed;
        }
        private static void ArenaPauseCancle()
        {
            Time.timeScale = ArenaManager.Instance.ColosseumData.Speed;
            UIDamageClearEvent.Send();
            if (ArenaManager.Instance.ColosseumData.IsMock)
            {
                //LoadingManager.ImmediatelySceneLoad("Town");
                LoadingManager.Instance.EffectiveSceneLoad("ArenaResult", eSceneEffectType.CloudAnimation);
            }
            else //탈주우
            {
                WWWForm param = new WWWForm();
                param.AddField("abort", 1);

                NetworkManager.Send("arena/result", param, (JObject jsonData) =>
                {
                    if (jsonData == null)
                        return;
                    ArenaManager.Instance.ColosseumData.SetResultData(jsonData);
                    LoadingManager.Instance.EffectiveSceneLoad("ArenaResult", eSceneEffectType.CloudAnimation);
                });
            }
        }
        private static void AdventurePauseOk()
        {
            Time.timeScale = AdventureManager.Instance.Data.Speed;
        }
        private static void AdventurePauseCancle()
        {
            EffectReceiverClearEvent.Send();
            UIDamageClearEvent.Send();
            Time.timeScale = 1f;
            WWWForm param = new WWWForm();
            NetworkManager.Send("adventure/abort", param, (JObject jsonData) =>
            {
                if (jsonData == null)
                    return;
                ArenaManager.Instance.ColosseumData.Initialize();
                ArenaManager.Instance.ColosseumData.SetResultData(jsonData);
                LoadingManager.Instance.EffectiveSceneLoad("AdventureReward", eSceneEffectType.CloudAnimation);
            });
        }
        private static void DailyPauseOk()
        {
            Time.timeScale = DailyManager.Instance.Data.Speed;
        }
        private static void DailyPauseCancle()
        {
            EffectReceiverClearEvent.Send();
            UIDamageClearEvent.Send();
            Time.timeScale = 1f;
            WWWForm param = new WWWForm();
            NetworkManager.Send("daily/dailyabort", param, (JObject jsonData) =>
            {
                if (jsonData == null)
                    return;

                DailyManager.Instance.Data.InitializeReward();
                DailyManager.Instance.SetRewardData(jsonData);
                LoadingManager.Instance.EffectiveSceneLoad("DailyReward", eSceneEffectType.CloudAnimation);
            });
        }
        private static void WorldBossPauseOk()
        {
            Time.timeScale = WorldBossManager.Instance.Data.Speed;
        }
        private static void WorldBossPauseCancel()
        {
           var popup = ReviewPopup.OnReviewPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("boss_raid_exit"), StringData.GetStringByIndex(100000199), StringData.GetStringByIndex(100000200),
                () => {
                    EffectReceiverClearEvent.Send();
                    UIDamageClearEvent.Send();
                    Time.timeScale = 1f;
                    WorldBossManager.Instance.RequestBattleResult(eBattleState.Abort);
                },
                () => {
                    WorldBossPauseOk();
                }, ()=> {
                    WorldBossPauseOk();
                },false);
            
        }
        #endregion
        #endregion
        public void SetCallBack(VoidDelegate okCallback = null, VoidDelegate cancelCallback = null) //callback 에 null 넣으면 ok 아닌 버튼들은 사라집니다
        {
            if (yesBtn != null)
            {
                this.okCallback = okCallback;
                yesBtn.SetActive(true);
            }
            if (noBtn != null)
            {
                this.cancelCallback = cancelCallback;
                noBtn.SetActive(true);
            }
            PopupTopUIRefreshEvent.Hide();
        }
        public void OnClickOK()
        {

            if (okCallback == null)
            {
                ClosePopup();
                return;
            }
            okCallback();
            ClosePopup();
        }
        public void OnClickCancle()
        {
            if (cancelCallback == null)
            {
                ClosePopup();
                return;
            }
            cancelCallback();
            ClosePopup();
        }

        public override void InitUI() { }

        public override void BackButton()
        {
            OnClickCancle();
        }
		// Start is called before the first frame update
	}
}

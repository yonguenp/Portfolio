using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if DEBUG
namespace SandboxNetwork
{
    public class UIBattleSimulatorObject : UIBattleObject
    {
        void Start()
        {
            EventManager.AddListener<AdventureStateEvent>(this);
        }
        void OnEnable()
        {
            EventManager.AddListener<UIBattleObjectStartEvent>(this);
            EventManager.AddListener<UIBattleBossStartEvent>(this);
            EventManager.AddListener<UIBattleStateEndEvent>(this);
        }
        void OnDisable()
        {
            Time.timeScale = 1.0f;
            EventManager.RemoveListener<UIBattleObjectStartEvent>(this);
            EventManager.RemoveListener<UIBattleBossStartEvent>(this);
            EventManager.RemoveListener<UIBattleStateEndEvent>(this);
        }
        void OnDestroy()
        {
            EventManager.RemoveListener<AdventureStateEvent>(this);
        }
        public override void Init()
        {
            base.Init();
        }

        public override void InitUI(eUIType targetType)
        {
            base.InitUI(targetType);
            advAutoObj.SetActive(false);
            if (targetType.HasFlag(eUIType.Battle_Simulator))
            {
                Time.timeScale = AdventureManager.Instance.Data.Speed;
                StopCoroutine("UpdateAdventure");
                StartCoroutine("UpdateAdventure");
                setAutoAdvObj();

                if (noticeObject != null)//속성 팝업 열려있으면 끄기
                    noticeObject.SetActive(false);
            }
            else
            {
                StopAllCoroutines();
            }
        }

        public override void OnClickPause()
        {
            if (curSceneType.HasFlag(eUIType.Battle_Simulator))
            {
                if (isAutoAdvMode)
                {
                    Time.timeScale = 0f;
                    SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000780), StringData.GetStringByIndex(100000778), StringData.GetStringByIndex(100000779),
                        () => {
                            Time.timeScale = AdventureManager.Instance.Data.Speed;
                        },
                        () => {
                            Time.timeScale = AdventureManager.Instance.Data.Speed;
                            OnClickAutoAdventure();
                        },
                        () => {
                            Time.timeScale = AdventureManager.Instance.Data.Speed;
                        }
                    );
                    return;
                }

                SimulatorPauseProcess();
            }
        }
        void SimulatorPauseProcess()
        {
            var popup = PopupManager.OpenPopup<SimulatorPausePopup>();
            if (popup != null)
            {
                Time.timeScale = 0f;
                popup.SetCallBack(SimulatorPauseOk, SimulatorPauseCancel, SimulatorPauseExit);
            }
        }

        void SimulatorPauseOk()
        {
            Time.timeScale = AdventureManager.Instance.Data.Speed;
        }
        void SimulatorPauseCancel()
        {
            Time.timeScale = AdventureManager.Instance.Data.Speed;
            if (SimulatorLoger.BattleInfo != null)
            {
                var worldIndex = SimulatorLoger.World;
                var stageIndex = SimulatorLoger.Stage;

                SimulatorLoger.ClearLog();

                AdventureManager.Instance.SetSimulatorStartData(worldIndex, stageIndex, SimulatorLoger.BattleInfo);
                LoadingManager.Instance.EffectiveSceneLoad("AdventureBattle", eSceneEffectType.CloudAnimation);
            }
            else
            {
                Debug.LogError("재시작 데이터가 없음 오류");
                return;
            }
        }
        void SimulatorPauseExit()
        {
            SimulatorLoger.ClearLog();
            Time.timeScale = AdventureManager.Instance.Data.Speed;
            UIManager.Instance.InitUI(eUIType.None);
            LoadingManager.Instance.EffectiveSceneLoad("pve_simulator", eSceneEffectType.CloudAnimation);
        }
        public override void OnClickSpeed()
        {
            if (curSceneType.HasFlag(eUIType.Battle_Simulator))
            {
                var cur = PlayerPrefs.GetInt("AdventureSpeedIndex", 0);
                cur++;

                int max = User.Instance.BATTLE_SPPED_BOOST ? 5 : 4;
                cur = cur % max;

                var speed = 1.0f;
                switch (cur)
                {
                    case 1:
                        speed = 1.2f;
                        break;
                    case 2:
                        speed = 1.5f;
                        break;
                    case 3:
                        speed = 2f;
                        break;
                    case 4:
                        speed = 2.5f;
                        break;
                    case 0:
                        speed = 1.0f;
                        break;
                }
                PlayerPrefs.SetInt("AdventureSpeedIndex", cur);
                PlayerPrefs.SetFloat("AdventureSpeed", speed);
                AdventureManager.Instance.Data.Speed = speed;
                Time.timeScale = speed;

                if (speedText != null)
                {
                    speedText.text = SBFunc.StrBuilder("x", speed.ToString("F1"));
                }
            }
        }

        public override void OnEvent(UIBattleObjectStartEvent eventType)
        {
            if (curSceneType.HasFlag(eUIType.Battle_Simulator))
            {
                if (advAnim != null)
                {
                    advAnim.gameObject.SetActive(true);
                    advAnim.Play("AdventureStart");
                }
            }
        }
        public override void OnEvent(UIBattleBossStartEvent eventType)
        {
            if (curSceneType.HasFlag(eUIType.Battle_Simulator))
            {
                if (advBossAnim != null)
                {
                    StartCoroutine(bossAlert());
                    UIBattleBossEnemyAnim ui = advBossAnim.GetComponent<UIBattleBossEnemyAnim>();
                    if (ui != null)
                    {
                        if (stageUI != null)
                            stageUI.gameObject.SetActive(false);

                        ui.SetCallBack(() => {
                            UIBattleBossEndEvent.Send();
                            advBossHpObj.SetTargetBoss(eventType.targetBoss);
                        });
                    }
                }
            }
        }
        public override void OnEvent(UIBattleStateEndEvent eventType)
        {
            if (curSceneType.HasFlag(eUIType.Battle_Simulator))
            {
                if (pauseBtn != null)
                    pauseBtn.SetActive(false);
            }
        }
        protected override IEnumerator bossAlert()
        {
            yield return SBDefine.GetWaitForSeconds(bossAlertAnimWaitTime / AdventureManager.Instance.Data.Speed);
            advBossAnim.gameObject.SetActive(true);
            advBossAnim.speed = bossAlertAnimSpeed * AdventureManager.Instance.Data.Speed;
            advBossAnim.Play("AdventureBoss");
        }

        public override void StartAnimEnd()
        {
            if (curSceneType.HasFlag(eUIType.Battle_Simulator))
            {
                if (advAnim != null)
                {
                    advAnim.gameObject.SetActive(false);
                    UIBattleObjectEndEvent.Send();
                }
            }
        }

        public override void BossAnimEnd()
        {
            if (curSceneType.HasFlag(eUIType.Battle_Simulator))
            {
                if (advBossAnim != null)
                {
                    advBossAnim.gameObject.SetActive(false);
                    UIBattleObjectEndEvent.Send();
                }
            }
        }

        public override void OnEvent(AdventureStateEvent eventType)
        {
            Pause = eventType.Pause;
        }
        public override void OnClickAuto()
        {
            if (curSceneType.HasFlag(eUIType.Battle_Simulator))
            {
                var cur = PlayerPrefs.GetInt("AdventureAuto", 1);
#if UNITY_EDITOR
                cur++;
                cur = cur % 2;
#else
                cur = 1;
#endif
                PlayerPrefs.SetInt("AdventureAuto", cur);
                AdventureManager.Instance.Data.IsAuto = cur == 1;

                if (autoAni != null)
                {
                    autoAni.Play(AdventureManager.Instance.Data.IsAuto ? "auto_a" : "auto_off");
                }
            }
        }
    }
}
#endif
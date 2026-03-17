using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class UIGame : MonoBehaviour
{
    const float OriginalCamSize = 6.5f;
    const float ZoomCamSize = 4.0f;
    public Camera UICamera;
    public int currentTutoType = 1;

    [SerializeField] Transform sight_block = null;

    [SerializeField] GameObject timer = null;

    [SerializeField] UIProgressBar escapeProgressBar = null;
    [SerializeField] UIEscapeKeyScore escapeScore = null;
    [SerializeField] UIBatteryGeneratorProgress batteryProgress = null;

    [SerializeField] GameObject EscapeCountPanel = null;

    // [SerializeField] Image vehicleRemainBar = null;
    [SerializeField] UIScoreEarn uiScoreEarn = null;
    [SerializeField] UINotifyMessage uiNotifyMessage = null;
    [SerializeField] UICenterMessage uiCenterMessage = null;
    [Header("[Text 및 Button, Image]")]
    [SerializeField] Text txtGameReadyCount = null;

    [SerializeField] Transform readyCount = null;
    [SerializeField] Transform gameResult = null;
    [SerializeField] Transform win = null;
    [SerializeField] Transform lose = null;
    [SerializeField] Transform escaped = null;

    [SerializeField] Text txtTimer = null;
    [SerializeField] Image timerGauge = null;
    [SerializeField] Text txtEscape = null;
    [SerializeField] Text txtEscapeCount = null;
    [SerializeField] Image exitlabel = null;
    [SerializeField] Text exitText;

    [SerializeField] Transform chaserQuest = null;
    [SerializeField] Transform survivorQuest = null;

    [SerializeField] Transform map = null;
    [SerializeField] Transform cctv = null;
    [SerializeField] Text respawnNoticeUI = null;


    [SerializeField] Button btnLeave = null;
    [SerializeField] Text guideText = null;

    [SerializeField] GameObject networkAlert = null;
    [SerializeField] GameObject rankpoint_warning = null;

    [Header("[사용자 인터페이스]")]
    [SerializeField] GameObject InputLayer = null;
    [SerializeField] GameObject SkillGuideUI = null;
    [SerializeField] Toggle IsJoystickDynamic = null;
    [SerializeField] GameObject emotionPanel = null;
    [SerializeField] Image[] emotionItem = null;

    [Header("[튜토리얼 오브젝트]")]
    [SerializeField] GameObject[] sur_time_tuto;
    [SerializeField] GameObject[] chaser_time_tuto;
    [SerializeField] GameObject sur_playinfo;
    [SerializeField] GameObject chaser_playinfo;
    [SerializeField] Image chaser_attack_icon;


    float vehicleBarWidth = 0.0f;

    Game _game = null;
    CharacterObject killerCharacter = null;

    Coroutine resultButtonCoroutine = null;
    Coroutine respawnUICoroutine = null;
    HudVehicleBar vehicleBar = null;

    Tweener escapeTimerTween;
    bool sightBlock = false;
    void Start()
    {
        _game = Game.Instance;

        HideResult();
        ShowReadyCount(false);
        InitTutorial();
        ShowScore(false);
        ShowResult(false);
        //StaminaRatio(1.0f);
        //SetEscapeScore(0);
        ShowEscapeScore(false);
        ShowEscapeIcon(false);
        ShowEscapeProgress(false);
        ShowBatteryGeneratorProgress(false);
        SetBatteryProgress(0);
        txtEscape.gameObject.SetActive(false);
        CloseEmotionPanel();
        HideMap();
        //OnShowStamina(false);
        ShowPlayerRespawn(-1);
        // vehicleBarWidth = vehicleRemainBar.rectTransform.sizeDelta.x;
        vehicleBar = Game.Instance.HudNode.CreateVehicleBar(Game.Instance.PlayerController.Character);
        OnVehicleRemainBar(0.0f);
        respawnNoticeUI.gameObject.SetActive(false);
        sight_block.gameObject.SetActive(false);

        var sightBlockHeight = sight_block.GetComponent<RectTransform>().rect.height;
        if (sightBlockHeight < 1080) sightBlockHeight = 1080;
        sight_block.GetComponent<RectTransform>().sizeDelta = new Vector2(sightBlockHeight * 3, 0);

        IsJoystickDynamic.isOn = false;

        EscapeCountPanel.SetActive(false);
        for (int i = 1; i <= 3; i++)
        {
            Transform portraitTransform = EscapeCountPanel.transform.Find(i.ToString());
            if (portraitTransform != null)
            {
                portraitTransform.gameObject.SetActive(false);
            }
        }
#if UNITY_EDITOR || SB_TEST
        btnLeave.gameObject.SetActive(true);
#else
        btnLeave.gameObject.SetActive(false);
#endif

        timer.SetActive(false);
        exitText.gameObject.SetActive(false);
        chaserQuest.gameObject.SetActive(false);
        survivorQuest.gameObject.SetActive(false);
        exitlabel.gameObject.SetActive(false);
        timerGauge.fillAmount = 1f;
        rankpoint_warning.SetActive(Managers.PlayData.GameRank_Warning);

        SetMyEmotionItem();

        ShowNetworkAlert(false);
    }

    private void OnDestroy()
    {
        if (_game != null)
            _game.ClearUIGame();
    }

    [ContextMenu("ShowNetworkAlert")]
    public void ShowNetworkAlert()
    {
        ShowNetworkAlert(true);
    }

    public void ShowNetworkAlert(bool show)
    {
        if (networkAlert)
        {
            networkAlert.SetActive(show);
            if (show)
            {
                Image image = networkAlert.GetComponent<Image>();
                if (image)
                {
                    image.DOKill();
                    image.color = new Color(0.8207547f, 0.0f, 0.0f, 1.0f);
                    image.DOColor(new Color(0.8207547f, 0.0f, 0.0f, 0.0f), 1.0f).SetEase(Ease.InQuad).SetLoops(-1, LoopType.Yoyo);
                }
            }
        }
    }

    public void SetBatteryRemainCount(int v)
    {
        batteryProgress.SetRemainCount(v);
    }

    public void RefreshTime(int time)
    {
        if (time < 0)
            time = 0;

        int min = time / 60;
        int sec = time % 60;

        var sb = new StringBuilder().AppendFormat("{0}:{1}", min.ToString("00"), sec.ToString("00"));
        if (txtTimer != null)
            txtTimer.text = sb.ToString();

        if (timerGauge != null)
        {
            var escapeTime = Managers.PlayData.GameRoomInfo.EscapeTime / 1000f;
            if (escapeTimerTween != null)
            {
                escapeTimerTween.Kill();
                escapeTimerTween = null;
            }
            escapeTimerTween = timerGauge.DOFillAmount(time / escapeTime, 1f);
        }
    }

    public void SetRound(int round)
    {

    }

    public void RefreshBatteryGeneratorTime(int remainSeconds, int curRound, bool isCoolTime)
    {
        batteryProgress.UpdateTimer(remainSeconds, curRound, isCoolTime);
    }

    public void ClearTime()
    {
        txtTimer.text = string.Empty;
    }

    public void StartBatteryGeneratorTime()
    {
        batteryProgress.StartTimer();
    }

    public void StopBatteryGeneratorTime()
    {
        batteryProgress.StopTimer();
    }

    public void SetOpenEscapeDoor()
    {
        timer.SetActive(true);
        currentTutoType = 3;
        if (Managers.UserData.MyPoint < 50)
            Game.Instance.UIGame.ShowTutorial();
        //txtTimer.color = Color.green;
        txtEscape.gameObject.SetActive(true);
        RefreshTime(Managers.PlayData.GameRoomInfo.EscapeTime / 1000);
        exitlabel.gameObject.SetActive(true);
    }

    public void ShowScore(bool isShow)
    {
        //txtTimer.gameObject.SetActive(isShow);
        //lifeText.gameObject.SetActive(isShow);
    }

    public void OnReadyCount(int count)
    {
        if (Managers.UserData.MyPoint < 50)
            ShowTutorial(0);

        if (count <= 5 && count > 0)
            Managers.Scene.CurrentScene.PlayUISound("effect/EF_INGAME_5S");

        txtGameReadyCount.text = count.ToString();

        txtGameReadyCount.transform.DOKill();
        txtGameReadyCount.transform.localScale = Vector3.one;
        txtGameReadyCount.transform.DOScale(0.5f, 1.0f);
        if (count == 1)
        {
            rankpoint_warning.SetActive(false);
            ClearTutorial();

            if (Managers.UserData.MyPoint < 50)
                ShowTutorial(1);
        }


    }

    public void ShowReadyCount(bool isShow, bool isChaser = false)
    {
        readyCount.gameObject.SetActive(isShow);
    }

    public void ShowResult(bool isShow)
    {
        gameResult.gameObject.SetActive(isShow);
    }

    public void SetResult()
    {
        ShowResult(true);

        win.gameObject.SetActive(false);
        lose.gameObject.SetActive(false);
        escaped.gameObject.SetActive(false);

        Transform targetUI = null;
        switch (Managers.PlayData.GameResult)
        {
            case PlayDataManager.GAME_RESULT.UNKNOWN:
                break;
            case PlayDataManager.GAME_RESULT.ESCAPED:
                //Managers.Sound.Play("Sounds/effect/EF_WIN", Sound.Effect);
                //targetUI = escaped;
                Managers.Sound.Play("Sounds/effect/EF_LOSE", Sound.Effect);
                targetUI = lose;
                break;
            case PlayDataManager.GAME_RESULT.WIN:
                Managers.Sound.Play("Sounds/effect/EF_WIN", Sound.Effect);
                targetUI = win;
                break;
            case PlayDataManager.GAME_RESULT.LOSE:
                Managers.Sound.Play("Sounds/effect/EF_LOSE", Sound.Effect);
                targetUI = lose;
                break;
        }

        if (targetUI == null)
            return;

        targetUI.gameObject.SetActive(true);

        targetUI.DOScale(Vector3.one * 1.2f, 0.3f).OnComplete(() =>
        {
            targetUI.DOScale(Vector3.one, 0.1f);
        });

        var bt = targetUI.Find("Button");
        if (bt != null) bt.gameObject.SetActive(false);

        var txtTouch = targetUI.Find("txtTouch");
        if (txtTouch != null) txtTouch.gameObject.SetActive(false);

        if (resultButtonCoroutine != null)
            StopCoroutine(resultButtonCoroutine);

        resultButtonCoroutine = StartCoroutine(ShowResultButton(targetUI));
    }

    public void HideResult()
    {
        ShowResult(false);
        //win.gameObject.SetActive(false);
        //lose.gameObject.SetActive(false);
    }

    public void OnExitGame()
    {
        if (_game.GameRoom.State == GameRoom.eState.GameOver)
        {
            Managers.GameServer.Disconnect();
            DOTween.KillAll();

            _game.GameRoom.SetStateWithCallback(GameRoom.eState.GameOver);
            Managers.GameServer.SendExitGame();
            Managers.Scene.LoadScene(SceneType.Result);
            return;
        }

        PopupCanvas.Instance.ShowConfirmPopup("룸_강제이탈경고", () =>
        {
            Managers.GameServer.Disconnect();
            DOTween.KillAll();

            _game.GameRoom.SetStateWithCallback(GameRoom.eState.GameOver);
            Managers.GameServer.SendExitGame();
            Managers.Scene.LoadScene(SceneType.Result);
        });
    }


    IEnumerator ShowResultButton(Transform target)
    {
        yield return new WaitForSeconds(3.0f);

        if (target != null)
        {
            if (target != null)
            {
                var bt = target.transform.Find("Button");
                if (bt != null) bt.gameObject.SetActive(true);

                var txtTouch = target.transform.Find("txtTouch");
                if (txtTouch != null) txtTouch.gameObject.SetActive(true);
            }
        }

        yield return new WaitForSeconds(10.0f);

        OnExitGame();
    }

    public void ShowEscapeProgress(bool isShow)
    {
        if (escapeProgressBar != null)
        {
            escapeProgressBar.SetValue(0);
            escapeProgressBar.gameObject.SetActive(isShow);
        }
    }

    public void SetEscapeProgressBar(float ratio)
    {
        if (escapeProgressBar != null)
            escapeProgressBar.SetValue(ratio);
    }

    public void SetBatteryProgress(float value)
    {
        if (batteryProgress != null)
            batteryProgress.SetProgressValue(value);

        if (value >= 1f)
        {
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("탈출가능메시지"));
        }
    }

    public void ShowEscapeIcon()
    {
        PopupCanvas.Instance.ShowFadeText(StringManager.GetString("탈출구열림알림"));
        if (escapeScore != null)
        {
            escapeScore.AllScoreSetVisible(false);
            escapeScore.EscapeSetVisible(true);
        }

        SetOpenEscapeDoor();
    }

    public void ShowEscapeScore(bool isShow)
    {
        if (escapeScore != null)
            escapeScore.AllScoreSetVisible(isShow);
    }

    public void ShowEscapeIcon(bool isShow)
    {
        if (escapeScore != null)
            escapeScore.EscapeSetVisible(isShow);
    }

    public void ShowBatteryGeneratorProgress(bool isShow)
    {
        if (batteryProgress != null)
            batteryProgress.SetVisible(isShow);
    }

    public void ShowMap()
    {
        map.gameObject.SetActive(true);
    }

    public void HideMap()
    {
        map.gameObject.SetActive(false);
    }

    public void ShowCCTV(bool isShow)
    {
        cctv.gameObject.SetActive(isShow);
    }

    public void OnVehicleRemainBar(float remain)
    {
        vehicleBar.ShowVehicleBar(remain);
        // //StopCoroutine("StaminaRatio");
        // if (remain <= 0)
        // {
        //     vehicleRemainBar.transform.parent.gameObject.SetActive(false);
        //     return;
        // }

        // StartCoroutine(VehicleRemainBarAction(remain));
    }

    // IEnumerator VehicleRemainBarAction(float remain)
    // {
    //     vehicleRemainBar.transform.parent.gameObject.SetActive(true);

    //     float curTime = 0.0f;
    //     while (remain - curTime > 0.0f)
    //     {
    //         curTime += Time.deltaTime;

    //         var ratio = (remain - curTime) / remain;
    //         vehicleRemainBar.rectTransform.sizeDelta = new Vector2(vehicleBarWidth * ratio, vehicleRemainBar.rectTransform.sizeDelta.y);

    //         byte g = (byte)(255 * ratio);
    //         vehicleRemainBar.color = new Color(1, ratio, 0);

    //         yield return new WaitForEndOfFrame();
    //     }

    //     vehicleRemainBar.transform.parent.gameObject.SetActive(false);
    // }

    public void AddScoreEffect(int score)
    {
        if (score == 0) { return; }

        if (uiScoreEarn != null)
            uiScoreEarn.AddScoreEffect(score);
    }

    public void ShowPlayerRespawn(int ms)
    {
        if (respawnUICoroutine != null)
        {
            StopCoroutine(respawnUICoroutine);
            respawnUICoroutine = null;
        }

        bool enable = ms > 0;
        respawnNoticeUI.gameObject.SetActive(enable);
        if (enable == false)
        {
            ResetCamSize();
            return;
        }

        respawnUICoroutine = StartCoroutine(RefreshRespawnTime(ms));
    }

    IEnumerator RefreshRespawnTime(int time)
    {
        float timeSeconds = time * 0.001f;
        float lastTime = 0;

        float camSize = Camera.main.orthographicSize;
        //일단 더미로
        bool observer = false;
        while (timeSeconds - lastTime > 0)
        {
            respawnNoticeUI.gameObject.SetActive(true);
            respawnNoticeUI.text = StringManager.GetString("respawnNoticeUI", (int)(timeSeconds - lastTime) + 1);

            lastTime += Time.deltaTime;

            yield return new WaitForEndOfFrame();

            if (!observer && lastTime > 2.0f && killerCharacter != null)//2초뒤 공격자 판단
            {
                observer = true;

                Game.Instance.PlayerController.SetObserverObject(killerCharacter);
                killerCharacter = null;
            }

            if (camSize > ZoomCamSize)
                camSize -= Time.deltaTime;
            else
                camSize = ZoomCamSize;

            Camera.main.orthographicSize = camSize;
        }

        //respawnNoticeUI.gameObject.SetActive(false);
        //Game.Instance.PlayerController?.OffObserverMode();

        while (camSize < OriginalCamSize)
        {
            camSize += Time.deltaTime;
            Camera.main.orthographicSize = camSize;
            yield return new WaitForEndOfFrame();
        }

        ResetCamSize();
    }

    public void ResetCamSize()
    {
        Camera.main.orthographicSize = 6.5f;
    }

    public void SetSightBlockStatus(bool value)
    {
        sightBlock = value;
    }

    public void SetEscapeUI(int count, CharacterPortrait portrait = null)
    {
        EscapeCountPanel.SetActive(true);

        if (portrait != null)
        {
            //Transform portraitTransform = EscapeCountPanel.transform.Find(count.ToString());
            //if (portraitTransform != null)
            //{
            //    portraitTransform.gameObject.SetActive(true);
            //    portraitTransform.GetComponent<CharacterPortrait>().SetData(portrait, count);
            //}
            portrait.SetData(portrait, count);

            Transform waitText = EscapeCountPanel.transform.Find("Wait" + count.ToString() + "Text");
            if (waitText != null)
                waitText.gameObject.SetActive(false);
        }

        if (count == 0)
        {
            txtEscapeCount.text = StringManager.GetString("배터리게이지꽉참");
            exitText.text = Managers.PlayData.AmIChaser() ? StringManager.GetString("탈출안내:추적자") : StringManager.GetString("탈출안내:생존자");

            Managers.Sound.Play("effect/EF_ESCAPE_TIME", Sound.Effect);
            exitText.gameObject.SetActive(true);

            Color color = exitText.color;
            color.a = 1.0f;
            exitText.color = color;
            color.a = 0.0f;
            exitText.DOColor(color, 1.0f).OnComplete(() =>
            {
                exitText.gameObject.SetActive(false);
            });

            foreach (MaskableGraphic graphic in exitText.GetComponentsInChildren<MaskableGraphic>())
            {
                if (graphic == exitText)
                    continue;

                color = graphic.color;
                color.a = 1.0f;
                graphic.color = color;
                color.a = 0.0f;
                graphic.DOColor(color, 0.5f).SetDelay(0.5f);
                graphic.transform.DOShakeRotation(0.5f);
            }
        }
        else
            txtEscapeCount.text = StringManager.GetString("탈출인원수", count, Managers.PlayData.GameRoomInfo.TargetEscapeCount);
    }

    public void CreateKillMessage(CharacterObject killer, CharacterObject victim)
    {
        uiNotifyMessage.CreateKillMessage(killer, victim);
    }

    public void CreateBatteryMessage(int cnt, string name, int charType)
    {
        if (cnt == 0)
            return;

        uiNotifyMessage.CreateBatteryMessage(cnt, name, charType);
    }

    public void SetKiller(CharacterObject attacker)
    {
        killerCharacter = attacker;
    }

    public void ShowCenterMessage(int uid, string id)
    {
        uiCenterMessage.ShowDummyMessage(uid, id);
    }

    public void SetGuideText(string text)
    {
        guideText.text = text;
    }

    public void SetVisibleInput(bool visible)
    {
        InputLayer.SetActive(visible);
    }

    public void InitSkillGuide(string name, string desc)
    {
        SkillGuideUI.SetActive(false);

        SkillGuideUI.transform.Find("name").GetComponent<Text>().text = name;
        SkillGuideUI.transform.Find("desc").GetComponent<Text>().text = desc;
    }
    public void ShowSkillGuide()
    {
        SkillGuideUI.SetActive(true);
    }

    public void HideSkillGuide()
    {
        SkillGuideUI.SetActive(false);
    }

    public void OnTutorial()
    {
        PopupCanvas.Instance.ShowHelpPopup(TutorialPopup.HelpTapType.RULE);
    }

    public void ShowQuestGuide()
    {
        chaserQuest.gameObject.SetActive(Managers.PlayData.AmIChaser());
        survivorQuest.gameObject.SetActive(!Managers.PlayData.AmIChaser());

        Invoke("HideQuestGuide", 3.0f);
    }

    void HideQuestGuide()
    {
        CancelInvoke("HideQuestGuide");

        chaserQuest.gameObject.SetActive(false);
        survivorQuest.gameObject.SetActive(false);
    }

    public void ToggleJoystickMode(bool value)
    {
        if (value)
        {
            Game.Instance.PlayerController.ControllerPad.ChangeJoystickType(JoystickType.Dynamic);
        }
        else
        {
            Game.Instance.PlayerController.ControllerPad.ChangeJoystickType(JoystickType.Floating);
        }
    }



    //Emotion
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public void OpenEmoticonPanel()
    {
        if (!emotionPanel.activeSelf)
        {
            emotionPanel.SetActive(true);
        }
        else
            emotionPanel.SetActive(false);
    }
    public void CloseEmotionPanel()
    {
        emotionPanel.SetActive(false);
    }

    public void EmotionCoolTime()
    {
        var fillImage = InputLayer.GetComponentInChildren<ControllerPad>().emotionFillImage;
        var button = InputLayer.GetComponentInChildren<ControllerPad>().emotionButton;

        button.transform.DOKill();

        fillImage.DOFillAmount(1, 0f);

        Sequence sequence = DOTween.Sequence();
        //foreach (var item in emotionItem)
        //{
        //    if (item.GetComponentInParent<Button>() != null)
        //        item.GetComponentInParent<Button>().interactable = false;
        //}
        fillImage.DOFillAmount(0, 2f);
        sequence.AppendInterval(2f);
        sequence.AppendCallback(() =>
        {
            //foreach (var item in emotionItem)
            //{
            //    if (item.GetComponentInParent<Button>() != null)
            //        item.GetComponentInParent<Button>().interactable = true;
            //}
        });
    }
    public bool CoolCheck()
    {
        var fillImage = InputLayer.GetComponentInChildren<ControllerPad>().emotionFillImage;

        if (fillImage.fillAmount > 0)
            return false;
        else
            return true;
    }

    public void SetMyEmotionItem()
    {
        //일단 유져 계정에 적용된 아이템이 없기 때문에 강제로 더미 적용
        int[] items = { 
                CacheUserData.GetInt("Emotion1", 100),
                CacheUserData.GetInt("Emotion2", 101),
                CacheUserData.GetInt("Emotion3", 102),
                CacheUserData.GetInt("Emotion4", 103)
        };

        for (int i = 0; i < emotionItem.Length; i++)
        {
            emotionItem[i].sprite = ItemGameData.GetItemData(items[i]).sprite;
        }
    }
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////


    /*************************************************************************************************************************
    인게임 튜토리얼 작업
    *************************************************************************************************************************/

    public void InitTutorial()
    {
        foreach (Transform tr in sur_playinfo.transform.parent)
        {
            tr.gameObject.SetActive(false);
        }
        currentTutoType = 1;

        var character = Managers.Object.FindCharacterById(Managers.UserData.MyUserID);
        if (character.IsChaser)
        {
            CharacterGameData skillData = Managers.Data.GetData(GameDataManager.DATA_TYPE.character, character.CharacterType) as CharacterGameData;
            chaser_attack_icon.sprite = skillData.GetAtkSkillData().GetIcon();
        }

    }

    /// <param name="type">
    /// 0 -> 플레이 정보
    /// 1 -> 배터리 수집
    /// 2 -> 배터리 충전
    /// 3 -> 탈출 시간
    /// </param>
    public void ShowTutorial(int type)
    {
        var character = Managers.Object.FindCharacterById(Managers.UserData.MyUserID);

        ClearTutorial();
        if (type == 0)
        {
            if (character.IsChaser)
                chaser_playinfo.SetActive(true);
            else
                sur_playinfo.SetActive(true);
        }
        else
        {
            if (character.IsChaser)
                chaser_time_tuto[type].SetActive(true);
            else
                sur_time_tuto[type].SetActive(true);
        }
    }

    public void ShowTutorial()
    {
        var character = Managers.Object.FindCharacterById(Managers.UserData.MyUserID);

        ClearTutorial();
        if (currentTutoType == 0)
        {
            if (character.IsChaser)
                chaser_playinfo.SetActive(true);
            else
                sur_playinfo.SetActive(true);
        }
        else
        {
            if (character.IsChaser)
                chaser_time_tuto[currentTutoType].SetActive(true);
            else
                sur_time_tuto[currentTutoType].SetActive(true);
        }

    }

    public void ClearTutorial()
    {
        foreach (Transform tr in sur_playinfo.transform.parent)
        {
            tr.gameObject.SetActive(false);
        }
    }

    public void PlayerInfoTutorialBtn()
    {
        if (chaser_playinfo.activeSelf || sur_playinfo.activeSelf)
        {
            ClearTutorial();
            return;
        }

        ShowTutorial(0);
    }
    public void HelpTutorialBtn()
    {
        var character = Managers.Object.FindCharacterById(Managers.UserData.MyUserID);
        if (character.IsChaser)
        {
            foreach (var item in chaser_time_tuto)
            {
                if (item == null)
                    continue;
                if (item.activeSelf)
                {
                    ClearTutorial();
                    return;
                }
            }
        }
        else
        {
            foreach (var item in sur_time_tuto)
            {
                if (item == null)
                    continue;
                if (item.activeSelf)
                {
                    ClearTutorial();
                    return;
                }
            }
        }

        ShowTutorial();
    }

    private void Update()
    {
        bool sightBlcokOn = sightBlock && (!Game.Instance.PlayerController.ObserverMode);
        if(sight_block.gameObject.activeSelf != sightBlcokOn)
            sight_block.gameObject.SetActive(sightBlcokOn);
    }
    /************************************************************************************************************************/

}

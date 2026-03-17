using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using SBSocketSharedLib;
using System;

public class UIBatteryGeneratorProgress : MonoBehaviour
{
    [SerializeField] UIProgressBar progressBar;
    [SerializeField] UIProgressBar[] playerProgressBar;
    [SerializeField] Slider batteryChargeableRemainBar;
    [SerializeField] Text chargeableText;
    [SerializeField] Text chargeableSubText;

    [SerializeField] Text remainTimeText;
    [SerializeField] Text remainChargeCountText;
    [SerializeField] Text nextStepText;
    [SerializeField] Animator batteryAnimation;
    [SerializeField] GameObject chargeWaitIcon;

    [SerializeField] Image[] RoundIcons;
    [SerializeField] Image ChargeableEffect;

    private float previousValue;
    Dictionary<string, int> indexPlayersID = new Dictionary<string, int>();
    bool timerPlay { get { return enabled; } set { enabled = value; } }
    Tweener timeTweener = null;
    HudChargeGuide chargeGuideUI = null;
    //int curRound = 0;

    Color grayColor = new Color(0.4039216f, 0.3803922f, 0.4431373f);
    Color greenColor = new Color(0.08627451f, 0.7607844f, 0.0f);

    private void Awake()
    {
        nextStepText.text = string.Empty;
        progressBar.gameObject.SetActive(true);
        nextStepText.gameObject.SetActive(false);
        previousValue = 0f;

        remainTimeText.text = "";
        remainTimeText.gameObject.SetActive(false);

        foreach (UIProgressBar bar in playerProgressBar)
        {
            bar.SetValue(0.0f);
        }

        int key = 0;
        foreach (RoomPlayerInfo roomPlayer in Managers.PlayData.RoomPlayers)
        {
            if (Managers.PlayData.IsChaserPlayer(roomPlayer.UserId))
                continue;

            indexPlayersID.Add(roomPlayer.UserId.ToString(), key);
            ++key;
        }

        batteryChargeableRemainBar.value = 0.0f;
        chargeableText.gameObject.SetActive(false);

        for (int i = 0; i < RoundIcons.Length; i++)
        {
            RoundIcons[i].gameObject.SetActive(false);
        }

        chargeWaitIcon.SetActive(false);
        ChargeableEffect.gameObject.SetActive(false);
    }

    public void SetVisible(bool value)
    {
        progressBar.gameObject.SetActive(value);
        if (!value)
            nextStepText.gameObject.SetActive(value);

        if (value == false)
        {
            for (int i = 0; i < RoundIcons.Length; i++)
                RoundIcons[i].gameObject.SetActive(false);

            if (chargeGuideUI != null)
            {
                Destroy(chargeGuideUI.gameObject);
                chargeGuideUI = null;
            }
        }
    }

    public void SetProgressValue(float value)
    {
        if (value < 0f) value = 0f;
        else if (value > 1f) value = 1f;

        if (value > 0.0f)
        {
            var batteryScore = Game.Instance.GameRoom.GetBatteryScore();
            var totalBatteryCount = 0;
            Dictionary<string, int> ratedScore = new Dictionary<string, int>();
            foreach (var pair in batteryScore)
            {
                ratedScore[pair.Key] = pair.Value + totalBatteryCount;
                totalBatteryCount += pair.Value;
            }

            int rank = indexPlayersID.Count - 1;
            foreach (var pair in ratedScore)
            {
                var index = indexPlayersID[pair.Key];
                var bar = playerProgressBar[index];

                bar.SetValue(value * pair.Value / totalBatteryCount);
                bar.transform.SetSiblingIndex(rank);
                rank--;
            }
        }

        progressBar.SetValue(value);
        if (previousValue < value)
        {
            previousValue = value;

            ChargeableEffect.DOKill();
            ChargeableEffect.color = Color.green;
            Color guideColor = Color.yellow;
            guideColor.a = 0.4705882352941176f;//120
            ChargeableEffect.DOColor(guideColor, 1.0f).OnComplete(() =>
            {
                ChargeableEffect.color = guideColor;
                guideColor.a = 0.6666666666666667f;//150
                ChargeableEffect.DOColor(guideColor, 1.25f).SetLoops(-1, LoopType.Yoyo);
            });

            batteryAnimation.Play("ui_battery", -1, 0f);
        }
        if (value == 1f)
        {
            progressBar.BlinkBar(() =>
            {
                progressBar.gameObject.SetActive(false);
                //nextStepText.gameObject.SetActive(true);
            });
        }
    }

    public void SetRemainCount(int v)
    {
        remainChargeCountText.text = v.ToString();
    }

    public void StartTimer()
    {
        timerPlay = true;
    }

    public void StopTimer()
    {
        timerPlay = false;
        batteryChargeableRemainBar.value = 0f;
        remainTimeText.text = string.Empty;
    }

    public void UpdateTimer(int seconds, int curRound, bool isCoolTime)
    {
        if (!timerPlay)
            return;

        var activeSeconds = (int)(Managers.PlayData.GameRoomInfo.BatteryGeneratorActiveTime * 0.001f);
        var cooltimeSeconds = (int)(Managers.PlayData.GameRoomInfo.BatteryGeneratorCoolTime * 0.001f);
        RoundIcons[curRound - 1].gameObject.SetActive(true);

        if (isCoolTime)
        {
            chargeWaitIcon.SetActive(true);
            chargeableText.gameObject.SetActive(false);

            if (timeTweener != null)
            {
                timeTweener.Kill();
                timeTweener = null;
            }

            batteryChargeableRemainBar.fillRect.transform.GetComponent<Image>().color = Color.gray;
            batteryChargeableRemainBar.value = 1.0f - (float)seconds / cooltimeSeconds;
            timeTweener = batteryChargeableRemainBar.DOValue(1.0f - (seconds - 1.0f) / cooltimeSeconds, 1f).SetEase(Ease.Linear);

            if (cooltimeSeconds == seconds)
            {
                var co = Managers.Object.FindCharacterById(Managers.UserData.MyUserID);
                co.OnChangeRound();

                ChargeableEffect.DOKill();
                ChargeableEffect.transform.DOKill();
                ChargeableEffect.gameObject.SetActive(false);
            }

            if (seconds <= 5)
            {
                remainTimeText.gameObject.SetActive(true);
                remainTimeText.text = StringManager.GetString("충전시작알림", seconds);
                remainTimeText.color = Color.yellow;

                if (seconds <= 1)
                {
                    Game.Instance.UIGame.currentTutoType = 2;
                    if (Managers.UserData.MyPoint < 50)
                        Game.Instance.UIGame.ShowTutorial();
                }
            }
            else
            {
                remainTimeText.gameObject.SetActive(false);
            }

            if (chargeGuideUI != null)
            {
                Destroy(chargeGuideUI.gameObject);
                chargeGuideUI = null;
            }
        }
        else
        {
            chargeWaitIcon.SetActive(false);

            if (seconds == activeSeconds)
            {
                chargeableText.gameObject.SetActive(true);
                if (chargeableSubText != null)
                    chargeableSubText.gameObject.SetActive(!Managers.PlayData.AmIChaser());

                Managers.Scene.CurrentScene.PlayUISound("effect/EF_CHARGE_TIME");

                Color color = chargeableText.color;
                color.a = 1.0f;
                chargeableText.color = color;
                color.a = 0.0f;
                chargeableText.DOColor(color, 1.0f).OnComplete(() =>
                {
                    chargeableText.gameObject.SetActive(false);
                });

                foreach (MaskableGraphic graphic in chargeableText.GetComponentsInChildren<MaskableGraphic>())
                {
                    if (graphic == chargeableText)
                        continue;

                    color = graphic.color;
                    color.a = 1.0f;
                    graphic.color = color;
                    color.a = 0.0f;
                    graphic.DOColor(color, 0.5f).SetDelay(0.5f);
                    graphic.transform.DOShakeRotation(0.5f);
                }

                ChargeableEffect.gameObject.SetActive(true);
                ChargeableEffect.DOKill();
                ChargeableEffect.transform.DOKill();

                ChargeableEffect.transform.localScale = Vector3.one;
                ChargeableEffect.transform.DOScale(1.01f, 1.0f).SetLoops(-1, LoopType.Yoyo);

                Color guideColor = Color.yellow;
                guideColor.a = 0.4705882352941176f;//120
                ChargeableEffect.color = guideColor;
                guideColor.a = 0.6666666666666667f;//150
                ChargeableEffect.DOColor(guideColor, 1.25f).SetLoops(-1, LoopType.Yoyo);
            }

            //remainTimeText.text = string.Empty;
            if (timeTweener != null)
            {
                timeTweener.Kill();
                timeTweener = null;
            }

            batteryChargeableRemainBar.fillRect.transform.GetComponent<Image>().color = Color.yellow;
            batteryChargeableRemainBar.value = (float)seconds / activeSeconds;
            timeTweener = batteryChargeableRemainBar.DOValue((seconds - 1f) / activeSeconds, 1f).SetEase(Ease.Linear);

            if (seconds <= 5)
            {
                remainTimeText.gameObject.SetActive(true);
                remainTimeText.text = StringManager.GetString("충전종료알림", seconds);
                Game.Instance.UIGame.currentTutoType = 1;
                if (Managers.UserData.MyPoint < 50)
                    Game.Instance.UIGame.ShowTutorial();
                remainTimeText.color = Color.red;
                if (seconds == 1)
                    Managers.Sound.Play("effect/EF_CHARGE_END", Sound.Effect);
                else
                    Managers.Sound.Play("effect/EF_MROOM_5S", Sound.Effect);

            }
            else
            {
                remainTimeText.gameObject.SetActive(false);
            }


            if (chargeGuideUI == null && !Managers.PlayData.AmIChaser())
            {
                chargeGuideUI = Game.Instance.HudNode.CreateHudPosCharge(Managers.Object.GetBatteryGenerator().gameObject, Managers.Object.GetBatteryGenerator().transform.position) as HudChargeGuide;
            }
        }
    }
}

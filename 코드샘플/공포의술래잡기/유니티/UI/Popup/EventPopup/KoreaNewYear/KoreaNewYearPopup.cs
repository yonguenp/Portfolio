using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using static SBWeb;
using Spine.Unity;
using Coffee.UIExtensions;
using System;

public class KoreaNewYearPopup : Popup
{
    [Header("[ UI ]")]
    [SerializeField] GameObject rewardInfo;
    [SerializeField] GameObject ruleInfo;

    [Header("[ IMAGE ]")]
    [SerializeField] Image mainPouch;
    [SerializeField] Image event_assets;

    [Header("[ ITEM ]")]
    [SerializeField] GameObject rewardItem;
    [SerializeField] GameObject getItem;
    [SerializeField] UIBundleItem rewardBundle;

    [Header("[ TEXT ]")]
    [SerializeField] Text assetText;
    [SerializeField] Text pouchText;
    [SerializeField] Text enchantPerText;

    [Header("[ BUTTON ]")]
    [SerializeField] Button stopBtn;
    [SerializeField] Button enchantBtn;
    [SerializeField] Button isSkipBtn;
    //[SerializeField] Toggle dev_toggle;

    [Header("[ SPINE && EFFECT ]")]
    [SerializeField] SkeletonGraphic skeletonGraphic;
    [SerializeField] List<UIParticle> particleList = new List<UIParticle>();
    [SerializeField] Animator animator_pouch;

    public List<JToken> rewardedList = new List<JToken>();

    List<GameObject> so = new List<GameObject>();

    public int level = 0;
    public bool isForce = false;
    private bool isAnim = false;

    private object sequence_id = null;
    public override void Close()
    {
        if (isAnim)
            return;

        base.Close();
    }
    public override void Open(CloseCallback cb = null)
    {
        TryCall();
//        dev_toggle.gameObject.SetActive(false);
//        dev_toggle.isOn = false;
//#if UNITY_EDITOR || SB_TEST
//        dev_toggle.gameObject.SetActive(true);
//        dev_toggle.isOn = true;
//#endif

        if (skeletonGraphic.skeletonDataAsset == null)
        {
            skeletonGraphic.skeletonDataAsset = Managers.Resource.LoadAssetsBundle<SkeletonDataAsset>("AssetsBundle/Spine/etc_spine/event_success_fail_eff_SkeletonData");
            skeletonGraphic.Initialize(true);
        }
        skeletonGraphic.gameObject.SetActive(false);
        isSkipBtn.gameObject.SetActive(false);

        base.Open(cb);
    }
    public override void RefreshUI()
    {
        base.RefreshUI();

        Clear();

        if (so.Count > 0)
        {
            foreach (var item in so)
            {
                Destroy(item);
            }
            so.Clear();
        }

        assetText.text = Managers.UserData.GetMyItemCount(37).ToString();
        event_assets.sprite = ItemGameData.GetItemData(37).sprite;
        mainPouch.sprite = GetResourcePouch(level);

        if (mainPouch.sprite == null)
            mainPouch.sprite = GetResourcePouch(0);

        var tb = Managers.Data.GetData(GameDataManager.DATA_TYPE.event_newyear);
        EventNewYear eData = null;
        foreach (EventNewYear item in tb)
        {
            if (item.item_no == level + 37)
            {
                eData = item;
                break;
            }
        }
        pouchText.text = eData.GetDesc();
        enchantPerText.text = eData.rates.ToString() + "%";

        //현재 파우치 보상
        var curPouchItemList = EventBoxData.GetItemGroupList(eData.item_no);

        int totalRate = 0;
        foreach (var item in curPouchItemList)
        {
            totalRate += item.rates;
        }

        rewardItem.SetActive(true);
        foreach (EventBoxData item in curPouchItemList)
        {
            var reward = GameObject.Instantiate(rewardItem, rewardItem.transform.parent);
            reward.GetComponent<NewYear_Item>().Init(item, totalRate);
        }
        rewardItem.SetActive(false);

        getItem.SetActive(true);
        foreach (JToken item in rewardedList)
        {
            int id = int.Parse(item.ToString());
            if (id == 0)
                continue;
            var data = Managers.Data.GetData(GameDataManager.DATA_TYPE.event_box, id) as EventBoxData;

            var reward = GameObject.Instantiate(getItem, getItem.transform.parent);
            reward.GetComponent<NewYear_Item>().SetItem(data);
        }
        getItem.SetActive(false);


        stopBtn.interactable = level == 0 ? false : true;
    }

    public Sprite GetResourcePouch(int level)
    {
        string pathKey = "AssetsBundle/Texture/Icon/icon_event_pouch_";

        int key = level + 1;

        pathKey += key.ToString("D2");
        return Managers.Resource.LoadAssetsBundle<Sprite>(pathKey);
    }

    public void RuleSetActive(bool isActive)
    {
        ruleInfo.SetActive(isActive);
    }

    public void RateOpen()
    {
        Application.OpenURL(GameConfig.Instance.NEWYEAR_RATE_PAGE);
    }

    public void Clear()
    {
        StopAllCoroutines();

        foreach (Transform item in rewardItem.transform.parent)
        {
            if (item == rewardItem.transform)
                continue;

            Destroy(item.gameObject);
        }

        foreach (Transform item in getItem.transform.parent)
        {
            if (item == getItem.transform)
                continue;

            Destroy(item.gameObject);
        }

        ruleInfo.gameObject.SetActive(false);

        isAnim = false;
        stopBtn.interactable = level == 0 ? false : true;
        enchantBtn.interactable = true;
        isSkipBtn.gameObject.SetActive(false);
    }

    public void ClearGetItem()
    {
        foreach (Transform item in getItem.transform.parent)
        {
            if (item == getItem.transform)
                continue;

            Destroy(item.gameObject);
        }
    }

    public void ShopOpen()
    {
        PopupCanvas.Instance.ShowShopPopup();
    }


    public void GetReward()
    {
        if (isAnim)
            return;

        var item_no = 37;
        TryCall(item_no, 0);
    }
    public void EnchantPouchBtn()
    {
        if (isAnim)
            return;

        if (level > 5) //&& !dev_toggle.isOn
        {
            PopupCanvas.Instance.ShowConfirmPopup("ui_newyear_enchant_check", "ui_newyear_go", StringManager.GetString("ui_newyear_get"),
                //강화 선택 버튼
                () =>
                {
                    var item_no = 37;
                    TryCall(item_no, 1);
                },
                //보상받기 버튼
                () =>
                {
                },
                level + 1);
        }
        else
        {
            var item_no = 37;
            TryCall(item_no, 1);
        }
    }

    public void StartEvent(int type = 0)
    {
        isSkipBtn.gameObject.SetActive(true);
        StartCoroutine(EventAnim(type));
    }
    IEnumerator EventAnim(int type)
    {
        isAnim = true;
        stopBtn.interactable = !isAnim;
        enchantBtn.interactable = !isAnim;

        if (type == 0)
        {
            //PopupCanvas.Instance.ShowFadeText("보상받기연출");
        }
        else
        {
            if (rewardedList.Count > 0 && rewardedList[0].Value<int>() > 0)
            {
                animator_pouch.Rebind();
                animator_pouch.enabled = true;
                //복주머니 흔드는 연출
                //Sequence sequence = DOTween.Sequence();
                //sequence_id = sequence.intId;
                //sequence.Append(mainPouch.transform.DOShakePosition(1f, 10f, fadeOut: false));
                //sequence.Append(mainPouch.transform.DOScale(0.5f, 0.3f));
                //sequence.Append(mainPouch.transform.DOScale(1f, 0.2f));
                yield return new WaitForSeconds(0.7f);
                animator_pouch.enabled = false;

                var soundClip = SoundResourceData.GetAudioClip("EF_NEWYEAR_SUCCESS");
                Managers.Sound.Play(soundClip, Sound.Effect);
                particleList.Find(_ => _.name == "fx_pouch_win").Play();
                OnAnimationPlay("event_success_txt_eff1");
                yield return new WaitForSeconds(skeletonGraphic.SkeletonData.FindAnimation("event_success_txt_eff1").Duration);
                //PopupCanvas.Instance.ShowFadeText("강화성공연출");
            }
            else if(isForce)
            {
                var soundClip = SoundResourceData.GetAudioClip("EF_NEWYEAR_SUCCESS");
                Managers.Sound.Play(soundClip, Sound.Effect);
                particleList.Find(_ => _.name == "fx_pouch_win").Play();
                OnAnimationPlay("event_success_txt_eff1");
                yield return new WaitForSeconds(skeletonGraphic.SkeletonData.FindAnimation("event_success_txt_eff1").Duration);
                isForce = false;
                //PopupCanvas.Instance.ShowFadeText("강화성공연출");
            }
            else
            {
                var soundClip = SoundResourceData.GetAudioClip("EF_NEWYEAR_FAIL");
                Managers.Sound.Play(soundClip, Sound.Effect);
                particleList.Find(_ => _.name == "fx_pouch_lose").Play();
                OnAnimationPlay("event_fail_txt_eff1");
                yield return new WaitForSeconds(skeletonGraphic.SkeletonData.FindAnimation("event_fail_txt_eff1").Duration);
                //PopupCanvas.Instance.ShowFadeText("강화실패연출");
            }
        }
        //yield return new WaitForSeconds(0.5f);

        RefreshUI();

        yield return new WaitForSeconds(0.5f);

        isAnim = false;
        stopBtn.interactable = level == 0 ? false : true;
        enchantBtn.interactable = !isAnim;
        isSkipBtn.gameObject.SetActive(false);
    }

    public void IsSkipBtn()
    {
        if (GameConfig.Instance.NEWYEAR_SKIP_FLAG == 1)
        {
            StopAllCoroutines();
            if (sequence_id != null)
            {
                DOTween.Rewind(sequence_id);
                DOTween.Kill(sequence_id);
            }

            isAnim = false;
            stopBtn.interactable = level == 0 ? false : true;
            enchantBtn.interactable = !isAnim;
            isSkipBtn.gameObject.SetActive(false);
            RefreshUI();
        }
    }

    public void OnAnimationPlay(string stateName)
    {
        skeletonGraphic.AnimationState.Start += delegate
        {
            skeletonGraphic.gameObject.SetActive(true);
        };
        skeletonGraphic.AnimationState.Complete += delegate
        {
            particleList.Find(_ => _.name == "fx_pouch_win").Stop();
            particleList.Find(_ => _.name == "fx_pouch_lose").Stop();

            particleList.Find(_ => _.name == "fx_pouch_win").Clear();
            particleList.Find(_ => _.name == "fx_pouch_lose").Clear();

            skeletonGraphic.gameObject.SetActive(false);
        };
        skeletonGraphic.AnimationState.SetAnimation(0, stateName, false);
    }

    public void TryCall(int item_no = 0, int isGo = 0)
    {
        //37 중단하지 못하도록 예외처리
        if (item_no > 0 && level < 1 && isGo == 0)
            return;

        if (item_no == 37 && level < 1 && Managers.UserData.GetMyItemCount(item_no) <= 0)
        {
            PopupCanvas.Instance.ShowFadeText("복주머니부족");
            return;
        }

        //SBDebug.Log($"@ITEM_NO ::{item_no}");
        SBWeb.TryNewYearPouch(item_no, isGo, (res) =>
        {
            //SBDebug.Log($"@ITEM_NO ::{item_no} " + res);
            if (item_no > 0)
            {
                if (level >= 0)
                    StartEvent(isGo);
                else
                    RefreshUI();
            }
            else
            {
                RefreshUI();
            }
        });
    }

    public void EndResultAnim(List<ResponseReward> rewards)
    {
        StartCoroutine(coEndResultAnim(rewards));
    }
    IEnumerator coEndResultAnim(List<ResponseReward> rewards)
    {
        ClearGetItem();

        isAnim = true;
        stopBtn.interactable = !isAnim;
        enchantBtn.interactable = !isAnim;

        for (int i = 0; i < rewards.Count; i++)
        {
            var obj = GameObject.Instantiate(rewardBundle, rewardBundle.transform.parent);
            so.Add(obj.gameObject);
            obj.SetReward(rewards[i]);

            float r = 350;
            var rad = (i + 1) * (360 / rewards.Count) * Mathf.Deg2Rad;
            obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(r * Mathf.Sin(rad), r * Mathf.Cos(rad));
            obj.transform.DOScale(1.3f, 0.1f).SetEase(Ease.InOutBack);
            obj.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.05f);
        }

        int idx = 0;
        foreach (var item in so)
        {
            item.GetComponent<RectTransform>().DOAnchorPos(Vector2.zero, 0.15f);
            idx++;

            if (idx == so.Count)
            {
                so[so.Count - 1].transform.DOScale(2.5f, 0.2f).SetEase(Ease.InOutBack).SetDelay(0.15f);
                Invoke("PlayParticle", 0.1f);
            }
        }

        //연출 종료
        yield return new WaitForSeconds(0.8f);


        RefreshUI();
        PopupCanvas.Instance.ShowRewardResult(rewards, () =>
        {
            isAnim = false;
            stopBtn.interactable = !isAnim;
            enchantBtn.interactable = !isAnim;
        });
    }

    void PlayParticle()
    {
        particleList.Find(_ => _.name == "fx_respawn").Play();
    }
}

public class EventNewYear : GameData
{
    public int uid { get; private set; }
    public int item_no { get; private set; }
    public float rates { get; private set; }
    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        uid = Int(data["uid"]);
        item_no = Int(data["item_no"]);
        rates = float.Parse(data["rates"]);
    }

}

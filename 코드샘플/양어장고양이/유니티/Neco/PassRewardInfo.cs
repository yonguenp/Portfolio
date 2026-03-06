using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PassRewardInfo : MonoBehaviour
{
    [Header("[Pass Reward Layer]")]
    public GameObject receivedLayer;
    public GameObject checkIconObject;
    public GameObject lockedLayer;
    public Image rewardImage;
    public Text rewardNameText;
    public Text rewardCountText;

    neco_pass_reward_forever curPassRewardData;
    PassListInfo rootUI;

    uint curIndexStep;

    public void InitPassRewardInfo(uint index, neco_pass_reward_forever rewardData, string type, uint id, uint count, PassListInfo root)
    {
        curPassRewardData = rewardData;
        curIndexStep = index;

        SetPassRewardInfo(type, id, count);

        receivedLayer.SetActive(curPassRewardData.IsRecivedReward(index));
        lockedLayer.SetActive(index > neco_data.Instance.GetPassData().GetCurPassStep());

        rootUI = root;

        rewardImage.color = Color.white;
        rewardImage.DOKill();
        rewardImage.transform.DOKill();

        if (!receivedLayer.activeSelf && !lockedLayer.activeSelf && neco_data.Instance.GetPassData().GetCurLevel() >= rewardData.GetNecoPassRewardLevel())
        {
            if (!curPassRewardData.IsRecivedReward(index))
            {
                rewardImage.transform.DOScale(0.9f, 0.6f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutCirc);
                rewardImage.DOColor(new Color(1.0f, 1.0f, 1.0f, 0.5f), 1.0f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutCirc);
            }
        }

        if (!curPassRewardData.IsRecivedReward(index))
        {
            Button button = gameObject.GetComponent<Button>();
            if (button == null)
            {
                button = gameObject.AddComponent<Button>();
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                if (index > neco_data.Instance.GetPassData().GetCurPassStep())
                {
                    if (neco_data.PrologueSeq.배틀패스강조및대사 == neco_data.GetPrologueSeq())
                    {
                        NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("패틀패스강조"));
                        return;
                    }

                    ConfirmPopupData param = new ConfirmPopupData();

                    param.titleText = LocalizeData.GetText("LOCALIZE_324");
                    param.titleMessageText = LocalizeData.GetText("LOCALIZE_325") + (neco_data.Instance.GetPassData().GetCurPassStep() + 1).ToString() + LocalizeData.GetText("LOCALIZE_326");
                    param.messageText_1 = LocalizeData.GetText("LOCALIZE_327");

                    neco_shop curShopData = null;
                    if (neco_data.Instance.GetPassData().GetCurPassStep() == 1)
                    {
                        curShopData = neco_shop.GetNecoShopData(22);
                    }
                    else if (neco_data.Instance.GetPassData().GetCurPassStep() == 2)
                    {
                        curShopData = neco_shop.GetNecoShopData(23);
                    }
                    param.amountText = string.Format("\\ {0}", curShopData?.GetNecoShopPrice().ToString("n0"));

                    NecoCanvas.GetPopupCanvas().OnSystemConfirmPopupShow(param, CONFIRM_POPUP_TYPE.COMMON, () =>
                    {
                        uint pid = 0;
                        string packName = "";
                        switch (neco_data.Instance.GetPassData().GetCurPassStep())
                        {
                            case 1:
                                pid = 22;
                                packName = "hahaha_pass_1";
                                break;
                            case 2:
                                pid = 23;
                                packName = "hahaha_pass_2";
                                break;
                            case 3:
                            default:
                                return;
                        }

                        IAPManager.GetInstance().TryPurchase(pid, packName, (responseArr) =>
                        {
                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_328"), LocalizeData.GetText("LOCALIZE_329"),
                                ()=> {
                                    rootUI.RefreshRootPanel();
                                });
                        }, (responseArr) =>
                        {
                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_316"), LocalizeData.GetText("SERVER_ERROR"));
                        });
                    });
                }
                else if (neco_data.Instance.GetPassData().GetCurLevel() < rewardData.GetNecoPassRewardLevel())
                {
                    NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_330"), LocalizeData.GetText("LOCALIZE_331"));
                }
                else if (curPassRewardData.IsRecivedReward(index))
                {
                    NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_332"), LocalizeData.GetText("LOCALIZE_333"));
                }
                else
                {
                    rootUI.OnClickGetButton((int)index);
                }
            });
        }
    }

    void SetPassRewardInfo(string type, uint id, uint count)
    {
        switch (type)
        {
            case "gold":
                rewardImage.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_coin");
                rewardNameText.text = LocalizeData.GetText("LOCALIZE_334");
                rewardCountText.text = count.ToString("n0");
                break;
            case "point":
                rewardImage.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_point");
                rewardNameText.text = LocalizeData.GetText("LOCALIZE_335");
                rewardCountText.text = count.ToString("n0");
                break;
            case "item":
                items itemData = items.GetItem(id);
                rewardImage.sprite = itemData.GetItemIcon();
                rewardNameText.text = itemData.GetItemName();
                
                if (count > 1)
                    rewardCountText.text = count.ToString();
                else
                    rewardCountText.text = "";
                
                break;
            case "memory":
                // todo bt - 현재 임시 데이터 적용되어있으므로 추후 연동 처리 필요
                neco_cat_memory memoryData = neco_cat_memory.GetNecoMemory(id);
                if (memoryData != null)
                {
                    rewardImage.sprite = Resources.Load<Sprite>(memoryData.GetNecoMemoryThumbnail());
                    //rewardNameText.text = memoryData.GetNecoMemoryTitle();
                }
                
                if (count > 1)
                    rewardCountText.text = count.ToString();
                else
                    rewardCountText.text = "";

                break;
        }
    }

    public void PlayRecieveTween()
    {
        receivedLayer.SetActive(curPassRewardData.IsRecivedReward(curIndexStep));

        if (checkIconObject != null)
        {
            // 체크 아이콘 트윈 애니
            checkIconObject.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 1.0f, 5).OnComplete(() => {
                rootUI.RemoveAvailRecieveData(this);
            });
        }
    }

    public void PlayRecieveAllTween()
    {
        receivedLayer.SetActive(curPassRewardData.IsRecivedReward(curIndexStep));

        if (checkIconObject != null)
        {
            // 체크 아이콘 트윈 애니
            checkIconObject.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 1.0f, 5);
        }

        // 받기 가능한 tween은 off
        rewardImage.DOKill();
        rewardImage.transform.DOKill();
    }

    public bool CheckIsGetReward()
    {
        if (curPassRewardData == null) { return false; }

        return curPassRewardData.IsRecivedReward(curIndexStep);
    }

    private void OnDisable()
    {
        checkIconObject.transform.DORewind();
        checkIconObject.transform.DOKill();

        rewardImage.transform.DORewind();
        rewardImage.transform.DOKill();
    }

    public PassListInfo GetRootInfo()
    {
        return rootUI;
    }
}

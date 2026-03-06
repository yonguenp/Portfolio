using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageToastConfirmPopup : MonoBehaviour
{
    public delegate void Callback();

    const float TWEEN_TIME = 0.2f;

    [Header("[Toast Info Layer]")]
    public Image toastBgImage;
    public Image titleBgImage;
    public Text titleMsgText;
    public Text contentsMsgText;

    [Header("[Item Info Layer]")]
    public GameObject itemObject;
    public Image itemBgImage;
    public Image itemImage;
    public Text itemNameText;
    public Text itemCountText;

    [Header("[Layout List]")]
    public RectTransform layoutRect;

    Callback closeCallback = null;
    Coroutine showToastCoroutine = null;

    RewardData curRewardData = null;

    private void OnEnable()
    {
        //showToastCoroutine = StartCoroutine(ShowToastPopup(notifyTime));

        StartDOTweenAnim();
    }

    public void OnClickCloseButton()
    {
        EndDOTweenAnim();

        MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
        if (mapController != null)
        {
            neco_data.PrologueSeq seq = neco_data.GetPrologueSeq();
            switch(seq)
            {
                case neco_data.PrologueSeq.통발UI닫힘:
                    mapController.SendMessage("통발수급완료대사", SendMessageOptions.DontRequireReceiver);
                    break;
                case neco_data.PrologueSeq.조리대UI닫힘:
                    mapController.SendMessage("첫요리완료대사", SendMessageOptions.DontRequireReceiver);
                    break;
                case neco_data.PrologueSeq.첫밥주기완료:
                    mapController.SendMessage("첫밥주기대사", SendMessageOptions.DontRequireReceiver);
                    break;
                case neco_data.PrologueSeq.고양이10번터치가이드퀘스트완료:
                    mapController.SendMessage("고양이만지기보상완료", SendMessageOptions.DontRequireReceiver);
                    break;
                case neco_data.PrologueSeq.철판제작가이드퀘스트완료:
                    mapController.SendMessage("철판제작가이드퀘스트완료대사", SendMessageOptions.DontRequireReceiver);
                    break;
                case neco_data.PrologueSeq.조리대레벨업완료:
                    mapController.SendMessage("조리대레벨업완료후대사", SendMessageOptions.DontRequireReceiver);
                    break;
                case neco_data.PrologueSeq.상점배스구매완료:
                    mapController.SendMessage("상점배스구매완료대사", SendMessageOptions.DontRequireReceiver);
                    break;
                case neco_data.PrologueSeq.낚시장난감만들기완료:
                    mapController.SendMessage("길막이낚시장난감배치", SendMessageOptions.DontRequireReceiver);
                    break;
                case neco_data.PrologueSeq.길막이낚시돌발완료:
                    mapController.SendMessage("첫돌발체험성공", SendMessageOptions.DontRequireReceiver);
                    break;
                case neco_data.PrologueSeq.첫보은받기성공:
                    mapController.SendMessage("첫보은받기퀘스트성공", SendMessageOptions.DontRequireReceiver);
                    break;                
                default:
                    break;
            }            
        }
    }

    public void SetImageToastPopup(string titleMsg, string contentsMsg, RewardData reward, Callback _closeCallback = null)
    {
        if (reward == null) { return; }

        curRewardData = reward;
        closeCallback = _closeCallback;

        titleMsgText.text = titleMsg;
        contentsMsgText.text = contentsMsg;

        itemImage.sprite = curRewardData.gold > 0 ? Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_coin") : curRewardData.itemData.GetItemIcon();
        itemNameText.text = curRewardData.gold > 0 ? LocalizeData.GetText("LOCALIZE_229") : curRewardData.itemData.GetItemName();
        itemCountText.text = curRewardData.gold > 0 ? string.Format("{0}", curRewardData.gold) : string.Format("{0}", curRewardData.count);

        itemCountText.gameObject.SetActive(curRewardData.count > 0 || curRewardData.gold > 0);

        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(RefreshLayout());
        }

        if (neco_data.GetPrologueSeq() < neco_data.PrologueSeq.프리플레이)
        {
            NecoCanvas.GetPopupCanvas().OnTopUIInfoLayer(TOP_UI_PANEL_TYPE.GUIDE_QUEST);
        }
    }

    //IEnumerator ShowToastPopup(float time)
    //{
    //    yield return new WaitForSeconds(time - TWEEN_TIME);

    //    EndDOTweenAnim();
    //}

    void StartDOTweenAnim()
    {
        // Toast info Layer Tween Set
        Sequence openToastTween = DOTween.Sequence();
        openToastTween.Append(toastBgImage.DOFade(0.8f, TWEEN_TIME));
        openToastTween.Join(titleBgImage.DOFade(1.0f, TWEEN_TIME));
        openToastTween.Join(titleMsgText.DOFade(1.0f, TWEEN_TIME));
        openToastTween.Join(contentsMsgText.DOFade(1.0f, TWEEN_TIME));

        // Icon info Layer Tween Set
        Sequence openIconLayerTween = DOTween.Sequence();
        openIconLayerTween.Append(itemBgImage.DOFade(1.0f, TWEEN_TIME));
        openIconLayerTween.Join(itemImage.DOFade(1.0f, TWEEN_TIME));
        openIconLayerTween.Join(itemNameText.DOFade(1.0f, TWEEN_TIME));
        openIconLayerTween.Join(itemCountText.DOFade(1.0f, TWEEN_TIME));

        openToastTween.Restart();
        openIconLayerTween.Restart();
    }

    void EndDOTweenAnim()
    {
        // Toast info Layer Tween Set
        Sequence closeToastTween = DOTween.Sequence();
        closeToastTween.Append(toastBgImage.DOFade(0.0f, TWEEN_TIME));
        closeToastTween.Join(titleBgImage.DOFade(0.0f, TWEEN_TIME));
        closeToastTween.Join(titleMsgText.DOFade(0.0f, TWEEN_TIME));
        closeToastTween.Join(contentsMsgText.DOFade(0.0f, TWEEN_TIME));

        // Icon info Layer Tween Set
        Sequence closeIconLayerTween = DOTween.Sequence();
        closeIconLayerTween.Append(itemBgImage.DOFade(0.0f, TWEEN_TIME));
        closeIconLayerTween.Join(itemImage.DOFade(0.0f, TWEEN_TIME));
        closeIconLayerTween.Join(itemNameText.DOFade(0.0f, TWEEN_TIME));
        closeIconLayerTween.Join(itemCountText.DOFade(0.0f, TWEEN_TIME)).OnComplete(CloseToastPopup);

        closeToastTween.Restart();
        closeIconLayerTween.Restart();
    }

    void CloseToastPopup()
    {
        //if (showToastCoroutine != null)
        //{
        //    StopCoroutine(showToastCoroutine);
        //}

        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.IMAGE_TOAST_POPUP);
        closeCallback?.Invoke();

        // 재화 레이어 갱신
        NecoCanvas.GetPopupCanvas().RefreshTopUILayer(TOP_UI_PANEL_TYPE.RESOURCE);

        // 재료 관련 UI 갱신
        if (curRewardData != null && curRewardData.itemData != null)
        {
            string rewardItemType = curRewardData.itemData.GetItemType();
            if (rewardItemType == "M_MATERIAL" || rewardItemType == "T_MATERIAL" || rewardItemType == "S_MATERIAL" || rewardItemType == "TOY")
            {
                NecoCanvas.GetPopupCanvas().RefreshPopupData(NecoPopupCanvas.POPUP_REFRESH_TYPE.CAT_CRAFT);
            }
            else if (rewardItemType == "F_MATERIAL" || rewardItemType == "FOOD")
            {
                NecoCanvas.GetPopupCanvas().RefreshPopupData(NecoPopupCanvas.POPUP_REFRESH_TYPE.CAT_FOOD);
            }
        }

        //MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
        //if (mapController != null)
        //{
        //    mapController.SendMessage("CheckGuideQuestCleared", SendMessageOptions.DontRequireReceiver);
        //}
    }

    IEnumerator RefreshLayout()
    {
        // 원인 불명.. 2프레임에 걸쳐 최소 2회 갱신해야 정상 작동함

        yield return new WaitForEndOfFrame();

        if (layoutRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRect);
        }

        yield return new WaitForEndOfFrame();

        if (layoutRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRect);
        }
    }
}

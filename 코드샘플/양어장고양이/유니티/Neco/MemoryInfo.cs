using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MemoryInfo : MonoBehaviour
{
    public CatDetailInfo catdetailInfo;

    [Header("[Have Memory]")]
    public GameObject haveObject;

    public Image ThumbnailIcon;
    public Text MemoryText;

    [Header("[UnKnown Memory]")]
    public GameObject unknownObject;
    public GameObject unknownPhotoImage;
    public GameObject unknownAniPhotoImage;

    [Header("[Common]")]
    public GameObject redDotIcon;

    neco_cat curCatData = null;
    neco_cat_memory curMemoryData = null;

    bool hasMemory = false;

    string newMemoryKey = "";

    public void OnClickMemoriesButton()
    {
        if (curMemoryData == null) { return; }

        if (hasMemory)
        {
            //NecoCanvas.GetPopupCanvas().OnPopupClose();

            if (curMemoryData.GetNecoMemoryType() == "photo")
            {
                NecoCanvas.GetPopupCanvas().OnShowCatPhotoViewPopup(curMemoryData);
            }
            else if (curMemoryData.GetNecoMemoryType() == "ani")
            {
                NecoCanvas.GetPopupCanvas().OnShowCatPhotoViewPopup(curMemoryData);
            }
            else if (curMemoryData.GetNecoMemoryType() == "movie")
            {
                NecoCanvas.GetPopupCanvas().OnPopupClose();
                NecoCanvas.GetVideoCanvas().OnVideoPlayWithDisplayControl(curMemoryData.GetNecoMemoryID(), () => {
                    NecoCanvas.GetPopupCanvas().OnCatDetailPopupShow(curCatData);
                });
            }

            PlayerPrefs.SetInt(newMemoryKey, 1);
            redDotIcon.SetActive(PlayerPrefs.GetInt(newMemoryKey, 0) == 0);

            catdetailInfo?.UpdateRedDotState();
        }
        else
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_230"));
        }
    }

    public void SetMemoryInfo(neco_cat_memory memoryData, neco_cat curData, float rotateTime)
    {
        ClearData();

        curCatData = curData;
        curMemoryData = memoryData;

        if (curCatData == null) { return; }
        if (curMemoryData == null) { return; }

        haveObject.SetActive(false);
        unknownObject.SetActive(false);

        hasMemory = false;

        gameObject.transform.localEulerAngles = Vector3.zero;
        gameObject.transform.DOKill();
        gameObject.transform.DOLocalRotate(new Vector3(0, -90, 0), rotateTime * 0.5f, RotateMode.FastBeyond360).SetRelative().OnComplete(() =>
        {             
            unknownObject.SetActive(true);
            unknownPhotoImage.SetActive(curMemoryData.GetNecoMemoryType() != "ani");
            unknownAniPhotoImage.SetActive(curMemoryData.GetNecoMemoryType() == "ani");

            // 2021.7.21 - 추억획득 팝업에서 상세보기 진입하는 루트가 생겨서 미획득 고양이더라도 추억 정보를 오픈
            //bool isCatInfoOpen = curCatData.GetCatState() >= 3;

            //if (curCatData.IsGainCat() == false || isCatInfoOpen == false) // 캣 인포에서 고양이 오픈 Ani를 보지 않을 경우 정보표시X
            //{
            //    gameObject.transform.DOLocalRotate(new Vector3(0, 90, 0), 0, RotateMode.FastBeyond360).SetRelative();
            //    return;
            //}

            neco_user_cat userCatInfo = neco_user_cat.GetUserCatInfo(curCatData.GetCatID());
            //if (userCatInfo == null)
            //{
            //    gameObject.transform.DOLocalRotate(new Vector3(0, 90, 0), rotateTime * 0.5f, RotateMode.FastBeyond360).SetRelative();
            //    return;
            //}

            haveObject.SetActive(true);

            ThumbnailIcon.sprite = Resources.Load<Sprite>(curMemoryData.GetNecoMemoryThumbnail());

            if (userCatInfo != null)
            {
                List<uint> memoryList = userCatInfo.GetMemories();
                hasMemory = memoryList.Exists(x => x == curMemoryData.GetNecoMemoryID());
            }

            // 실제로 획득한 추억의 경우에만 물음표 레이어 off
            if (hasMemory)
            {
                unknownObject.SetActive(false);

                newMemoryKey = string.Format("{0}_{1}", SamandaLauncher.GetAccountNo(), curMemoryData.GetNecoMemoryID());
                redDotIcon.SetActive(PlayerPrefs.GetInt(newMemoryKey, 0) == 0);
            }
            else
            {
                unknownObject.transform.DOScale(1.1f, 0.25f).SetDelay(rotateTime * 0.5f).OnComplete(()=> { unknownObject.transform.DOScale(1.0f, 0.25f); });
            }


            gameObject.transform.DOLocalRotate(new Vector3(0, 90, 0), rotateTime * 0.5f, RotateMode.FastBeyond360).SetRelative();
        });
    }

    public void SetFavoritObjectAction(bool action)
    {
        if (action)
            ThumbnailIcon.transform.DOScale(0.9f, 0.5f).SetLoops(-1, LoopType.Yoyo);
    }

    void ClearData()
    {
        haveObject.SetActive(false);
        unknownObject.SetActive(false);

        unknownPhotoImage.SetActive(false);
        unknownAniPhotoImage.SetActive(false);

        gameObject.transform.DORewind();
        gameObject.transform.DOKill();
    }
}

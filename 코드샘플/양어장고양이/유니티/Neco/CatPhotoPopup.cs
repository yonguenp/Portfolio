using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class CatPhotoPopup : MonoBehaviour
{
    enum PHOTO_STATE { 
        INTRODUCE,
        ANIMATION_EFFECT,
        RESULT,
    };

    PHOTO_STATE curState;
    neco_cat curCat;
    neco_cat_memory curMemory;
    bool isNew = false;

    public GameObject[] PHOTO_STATE_OBJECT;
    public GameObject[] CatList;

    public AudioSource catSoundAudioSource;
    public AudioClip[] catSoundList;

    public Font HoonminFont;
    public Font JapFont;

    public RawImage PhotoImage;
    public Image PhotoIcon;
    public Text PhotoTitle;
    public Text PhotoCountText;
    public Text PhotoTargetName;
    public Text ResultTargetName;
    public VideoPlayer videoPlayer;

    public GameObject catPhotoButtonImage;

    public delegate void Callback();
    public Callback callback;
    public NecoCatDetailPanel catDetailPanel;

    public void OnShowPhotoPoup(neco_cat cat, neco_cat_memory memory, bool newMemory, Callback cb = null)
    {
        callback = cb;
        isNew = newMemory;

        if (memory == null || cat == null)
        {
            OnClosePhotoPopup();
            return;
        }
                
        InitCardData(cat, memory);
        SetPhotoPopupState(PHOTO_STATE.INTRODUCE);

        // 고양이 사진 버튼 강조 연출
        if (catPhotoButtonImage != null)
        {
            //Sequence mySequence = DOTween.Sequence();

            //mySequence.Append(catPhotoButtonImage.transform.DOLocalRotate(new Vector3(0, 0, -10), 0.5f, RotateMode.Fast));
            //mySequence.Append(catPhotoButtonImage.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.5f, RotateMode.Fast));
            //mySequence.Append(catPhotoButtonImage.transform.DOLocalRotate(new Vector3(0, 0, 10), 0.5f, RotateMode.Fast));
            //mySequence.Append(catPhotoButtonImage.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.5f, RotateMode.Fast));

            //mySequence.SetLoops(-1);

            catPhotoButtonImage.transform.DOScale(1.2f, 0.5f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
        }
    }

    public void OnShowPhotoBoxPoup(neco_cat cat, neco_cat_memory memory, Callback cb = null)
    {
        callback = cb;

        if (memory == null || cat == null)
        {
            OnClosePhotoPopup();
            return;
        }

        InitCardData(cat, memory);
        SetPhotoPopupState(PHOTO_STATE.ANIMATION_EFFECT);

        // 고양이 사진 버튼 강조 연출
        if (catPhotoButtonImage != null)
        {
            //Sequence mySequence = DOTween.Sequence();

            //mySequence.Append(catPhotoButtonImage.transform.DOLocalRotate(new Vector3(0, 0, -10), 0.5f, RotateMode.Fast));
            //mySequence.Append(catPhotoButtonImage.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.5f, RotateMode.Fast));
            //mySequence.Append(catPhotoButtonImage.transform.DOLocalRotate(new Vector3(0, 0, 10), 0.5f, RotateMode.Fast));
            //mySequence.Append(catPhotoButtonImage.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.5f, RotateMode.Fast));

            //mySequence.SetLoops(-1);

            catPhotoButtonImage.transform.DOScale(1.2f, 0.5f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
        }
    }

    public void OnClosePhotoPopup()
    {
        if(neco_data.GetPrologueSeq() == neco_data.PrologueSeq.사진찍기돌발대사)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("사진찍기프로필가이드"));
            return;
        }

        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CAT_PHOTO_GET_POPUP);

        // 고양이 사진 버튼 강조 연출 Off
        if (catPhotoButtonImage != null)
        {
            catPhotoButtonImage.transform.DORewind();
            catPhotoButtonImage.transform.DOKill();
        }

        callback?.Invoke();
        callback = null;
    }

    private void InitCardData(neco_cat cat, neco_cat_memory memory)
    {
        curCat = cat;
        curMemory = memory;

        foreach (GameObject catObject in CatList)
        {
            if(catObject != null)
                catObject.SetActive(false);
        }

        videoPlayer.Stop();
        VideoClip preClip = videoPlayer.clip;
        Resources.UnloadAsset(preClip);

        RenderTexture target = videoPlayer.targetTexture;
        if (target)
        {
            RenderTexture rt = RenderTexture.active;
            RenderTexture.active = target;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = rt;
        }

        Invoke("SetUI", 0.1f);
    }

    void SetUI()
    {
        GameObject targetCatObject = CatList[curCat.GetCatID()];
        if (targetCatObject != null)
        {
            targetCatObject.SetActive(true);
            Canvas.ForceUpdateCanvases();

            foreach (Transform child in targetCatObject.GetComponentsInChildren<Transform>(true))
            {
                child.gameObject.SetActive(true);                
            }
        }

        // 사운드 플레이
        PlayCatSound(curCat.GetCatID());

        string path = curMemory.GetNecoMemorySource();
        if (curMemory.GetNecoMemoryType() == "photo")
        {
            PhotoImage.texture = Resources.Load<Sprite>(path).texture;
        }
        else if (curMemory.GetNecoMemoryType() == "ani")
        {
            VideoClip clip = Resources.Load<VideoClip>(path);
            if (clip)
            {
                videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
                videoPlayer.clip = clip;
                videoPlayer.isLooping = true;
                PhotoImage.texture = videoPlayer.targetTexture;
                PhotoImage.uvRect = new Rect(Vector2.zero, Vector2.one);
                videoPlayer.enabled = true;

                videoPlayer.Prepare();
            }
        }
        else
        {
            OnClosePhotoPopup();
            return;
        }

        // 폰트 설정
        PhotoTitle.font = LocalizeData.instance.CurLanguage() == SystemLanguage.Japanese ? JapFont : HoonminFont;

        PhotoIcon.sprite = Resources.Load<Sprite>(curMemory.GetNecoMemoryThumbnail());
        PhotoTitle.text = curMemory.GetNecoMemoryTitle();
        PhotoTargetName.text = curCat.GetCatName();
        ResultTargetName.text = curCat.GetCatName();

        neco_user_cat userCatInfo = neco_user_cat.GetUserCatInfo(curCat.GetCatID());
        uint curCount = userCatInfo == null ? 0 : userCatInfo.GetPhotoMemoryCount();      // 해당팝업은 사진관련 획득 연출만 노출됨.
        int maxCount = curMemory.GetTotalNecoMemroyCountByType(curMemory.GetNecoMemoryType());

        PhotoCountText.text = string.Format("({0}/{1})", curCount, maxCount);

        
        GameObject memories = PHOTO_STATE_OBJECT[(int)PHOTO_STATE.RESULT];
        if (memories != null)
        {
            Transform CatInfoButton = memories.transform.Find("CatInfoButton");
            if (CatInfoButton != null)
            {
                if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.사진찍기돌발대사)
                    CatInfoButton.DOScale(1.2f, 0.5f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
                else
                {
                    CatInfoButton.DORewind();
                    CatInfoButton.DOKill();
                }
            }
        }
    }


    private void SetPhotoPopupState(PHOTO_STATE state)
    {
        curState = state;
        foreach (GameObject stateObject in PHOTO_STATE_OBJECT)
        {
            stateObject.SetActive(false);
        }

        PHOTO_STATE_OBJECT[(int)curState].SetActive(true);

        if(curState == PHOTO_STATE.ANIMATION_EFFECT)
        {
            videoPlayer.Play();
        }
    }

    public void OnTouchPanel()
    {
        switch(curState)
        {
            case PHOTO_STATE.INTRODUCE:
                SetPhotoPopupState(PHOTO_STATE.ANIMATION_EFFECT);
                break;
            case PHOTO_STATE.ANIMATION_EFFECT:
                if(isNew)
                    SetPhotoPopupState(PHOTO_STATE.RESULT);
                else
                    OnClosePhotoPopup();
                break;
            case PHOTO_STATE.RESULT:
                OnClosePhotoPopup();
                break;
        }
    }

    public void OnCatDetail()
    {
        List<neco_cat> dummy = new List<neco_cat>();
        dummy.Add(curCat);

        //if (curCat.IsGainCat() == false)
        //{
        //    NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_188"));
        //}

        NecoCanvas.GetPopupCanvas().OnPopupClose();
        catDetailPanel.SetCatDetailInfo(curCat, dummy, false);
    }

    void PlayCatSound(uint curCatID)
    {
        if (catSoundAudioSource != null)
        {
            if (catSoundList != null && catSoundList.Length > curCatID && catSoundList[curCatID] != null)
            {
                catSoundAudioSource.clip = catSoundList[curCatID];
                catSoundAudioSource.Play();
            }   
            else
            {
                Debug.LogError("catSoundList.Length <= curCatID");
            }
        }
    }
}

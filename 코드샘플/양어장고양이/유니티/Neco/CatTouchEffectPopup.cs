using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class CatTouchEffectPopup : MonoBehaviour
{
    public delegate void Callback();
    Callback callback;

    public GameObject[] Cat;

    public AudioSource catSoundAudioSource;
    public AudioClip[] catSoundList;

    public void OnShowCatTouchEffect(neco_cat.CAT_SUDDEN_STATE type, neco_cat cat, Callback cb)
    {
        callback = cb;
        foreach(GameObject go in Cat)
        {
            if(go != null)
                go.SetActive(false);
        }

        uint id = cat.GetCatID();
        if (Cat[id] != null)
        {
            Cat[id].SetActive(true);
            Cat[id].GetComponent<Button>().interactable = false;

            Cat[id].transform.localScale = Vector3.zero;
            Cat[id].transform.DOScale(1.2f, 0.5f).SetEase(Ease.OutBounce).OnComplete(() => {
                Cat[id].GetComponent<Button>().interactable = true;
            });

            Text text = Cat[id].GetComponentInChildren<Text>(true);
            if (text != null)
            {
                text.text = "";
                if (type == neco_cat.CAT_SUDDEN_STATE.WATCH)
                    text.text = LocalizeData.GetText("LOCALIZE_495");
                if (type == neco_cat.CAT_SUDDEN_STATE.TOUCH)
                    text.text = LocalizeData.GetText("LOCALIZE_496");

                text.gameObject.SetActive(true);
            }
        }

        // 사운드 재생
        PlayCatSound(id);
    }

    //public void OnNewCatAlarm(uint id)
    //{
    //    foreach (GameObject go in Cat)
    //    {
    //        if (go != null)
    //            go.SetActive(false);
    //    }

    //    if (Cat[id] != null)
    //    {
    //        Cat[id].SetActive(true);
    //    }

    //    Invoke("CloseAlarm", 5.0f);
    //}

    //public void CloseAlarm()
    //{
    //    NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.NEW_CAT_ALARM);
    //}

    public void OnTouchCat()
    {
        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.TOUCH_CAT_EFFECT_POPUP);
        callback?.Invoke();
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

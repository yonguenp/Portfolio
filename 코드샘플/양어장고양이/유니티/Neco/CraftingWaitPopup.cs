using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class CraftingWaitPopup : MonoBehaviour
{
    public delegate void Callback();

    public RenderTexture videoRenderTexture;
    public Text CraftingText;
    public Image CraftingProgress;

    [Header("[Video Objects]")]
    public GameObject videoPlayObject;
    public GameObject craftMatAniObject;

    private Callback CraftingWaitDone = null;

    public void SetCraftingInfo(recipe recipe, Callback cb)
    {
        object obj;
        string path = "";
        if(recipe.data.TryGetValue("making_clip", out obj))
        {
            path = (string)obj;
        }

        //if (string.IsNullOrEmpty(path))
        //    return;

        VideoClip clip = Resources.Load<VideoClip>(path);
        if (clip != null)
        {
            videoPlayObject.SetActive(true);
            craftMatAniObject.SetActive(false);
            gameObject.SetActive(true);

            CraftingProgress.DOKill();
            CraftingProgress.fillAmount = 0.0f;
            
            RenderTexture rt = RenderTexture.active;
            RenderTexture.active = videoRenderTexture;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = rt;

            VideoManager.GetInstance().PlayVideo(clip, videoRenderTexture, false, true, () =>
            {
                CraftingWaitDone = cb;

                CraftingProgress.fillAmount = 0.0f;

                CraftingProgress.DOFillAmount(1.0f, (float)clip.length).OnComplete(() =>
                {
                    CraftingDone();
                });

                CraftingText.text = LocalizeData.GetText("LOCALIZE_207");
                if (recipe.data.TryGetValue("recipe_type", out obj))
                {
                    if ((string)obj == "FOOD")
                    {
                        CraftingText.text = LocalizeData.GetText("LOCALIZE_208");
                    }
                }
            });
        }
        else
        {
            videoPlayObject.SetActive(false);
            craftMatAniObject.SetActive(true);

            CraftingWaitDone = cb;

            CraftingProgress.fillAmount = 0.0f;
            CraftingProgress.DORewind();

            CraftingProgress.DOFillAmount(1.0f, 3.0f).OnComplete(() =>
            {
                CraftingDone();
            });

            gameObject.SetActive(true);

            CraftingText.text = LocalizeData.GetText("LOCALIZE_207");
            if (recipe.data.TryGetValue("recipe_type", out obj))
            {
                if ((string)obj == "FOOD")
                {
                    CraftingText.text = LocalizeData.GetText("LOCALIZE_208");
                }
            }
        }
    }


    public void CraftingDone()
    {
        VideoManager.GetInstance().StopVideo(true);

        if (gameObject.activeSelf)
            CraftingWaitDone?.Invoke();

        gameObject.SetActive(false);
    }

    public void OnCancel()
    {
        // 프롤로그 체크
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.철판제작가이드퀘스트)
        {
            return;
        }

        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.낚시장난감만들기)
        {
            return;
        }

        VideoManager.GetInstance().StopVideo(true);
        gameObject.SetActive(false);
    }

    private void Update()
    {
        // Android Back Button
        #if UNITY_ANDROID
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 프롤로그 체크
            if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.철판제작가이드퀘스트)
            {
                return;
            }

            if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.낚시장난감만들기)
            {
                return;
            }

            VideoManager.GetInstance().StopVideo(true);
            gameObject.SetActive(false);
        }
        #endif
    }
}

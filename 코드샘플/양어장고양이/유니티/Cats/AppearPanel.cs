using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class AppearPanel : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public RawImage rawImage;
    public VideoPlayer GameCanvasVideoPlayer;

    public GameObject TitlePanel;
    public Text TitleText;
    public Text ScriptText;

    public IEnumerator PlayAppearMovie(cat_def catData)
    {
        gameObject.SetActive(true);

        RenderTexture target = videoPlayer.targetTexture;
        if (target)
        {
            RenderTexture rt = RenderTexture.active;
            RenderTexture.active = target;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = rt;
        }

        clip_event eventData = catData.GetAppearClip();

        TitleText.text = "";
        ScriptText.text = "";
        ScriptText.gameObject.SetActive(false);

        if (!string.IsNullOrEmpty(eventData.GetClipTitle()))
        {
            TitlePanel.SetActive(true);
            TitleText.text = eventData.GetClipTitle();
            Image titleBackgroundImage = TitlePanel.GetComponent<Image>();
            Color bgColor = titleBackgroundImage.color;
            bgColor.a = 0.0f;
            titleBackgroundImage.color = bgColor;

            Color color = TitleText.color;
            color.a = 0.0f;
            TitleText.color = color;

            while (color.a < 1.0f)
            {
                color.a += 0.1f;                
                TitleText.color = color;

                if(bgColor.a < 1.0f)
                    bgColor.a += 0.2f;
                titleBackgroundImage.color = bgColor;
                yield return new WaitForSeconds(0.1f);
            }

            yield return new WaitForSeconds(0.5f);

            while (color.a > 0.0f)
            {
                color.a -= 0.2f;
                TitleText.color = color;

                yield return new WaitForSeconds(0.1f);
            }
        }

        TitlePanel.SetActive(false);


        ResourceRequest req = Resources.LoadAsync<VideoClip>(eventData.GetClipPath());
        while (!req.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        VideoClip clip = (VideoClip)req.asset;
        if (clip == null)
        {
            yield return null;
        }

        videoPlayer.playOnAwake = false;

        videoPlayer.clip = clip;
        videoPlayer.isLooping = false;
        videoPlayer.time = 0;

        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared)
        {
            yield return new WaitForEndOfFrame();
        }

        if (GameCanvasVideoPlayer.gameObject.activeSelf)
        {
            if (GameCanvasVideoPlayer.clip != null)
                GameCanvasVideoPlayer.Stop();

            RenderTexture gct = GameCanvasVideoPlayer.targetTexture;
            if (gct)
            {
                RenderTexture rt = RenderTexture.active;
                RenderTexture.active = gct;
                GL.Clear(true, true, Color.clear);
                RenderTexture.active = rt;
            }
        }


        videoPlayer.Play();
        
        ScriptText.text = "";
        List<ClipSciptInfo> scripts = eventData.GetScriptList();
        if (scripts != null)
        {
            ScriptText.gameObject.SetActive(true);
            Sequence tseq = DOTween.Sequence();

            foreach (ClipSciptInfo info in scripts)
            {
                Sequence seq = DOTween.Sequence();
                seq.AppendInterval(info.startTime);
                seq.Append(ScriptText.DOText(info.scriptText, 0.0f));
                seq.AppendInterval(info.expireTime);
                seq.Append(ScriptText.DOText("", 0.0f));
                tseq.Join(seq);
            }

            tseq.Restart();
        }

        yield return new WaitForSecondsRealtime((float)videoPlayer.length);

        if (videoPlayer.isPlaying)
            yield return new WaitForEndOfFrame();

        if (GameCanvasVideoPlayer.gameObject.activeSelf)
        {
            if (GameCanvasVideoPlayer.clip != null)
                GameCanvasVideoPlayer.Play();
        }

        gameObject.SetActive(false);
    }
}

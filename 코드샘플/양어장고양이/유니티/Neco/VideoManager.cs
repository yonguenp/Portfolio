using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
    public delegate void Callback();

    public AudioSource videoAudioSource;
    static private VideoManager instance = null;
    private VideoPlayer videoPlayer;
    
    private VideoClip defaultClip = null;
    private RenderTexture defaultTargetTexture = null;
    private Callback LoadedCallback = null;
    private Coroutine videoLoadCoroutine = null;
    private void Awake()
    {
        instance = this;
        videoPlayer = GetComponent<VideoPlayer>();
    }

    private void OnDestroy()
    {
        instance = null;
        videoPlayer = null;
    }

    static public VideoManager GetInstance()
    {
        return instance;
    }

    public void PlayBackgroundVideo(VideoClip clip, RenderTexture targetTexture)
    {
        defaultClip = clip;
        defaultTargetTexture = targetTexture;

        PlayBackgroundVideo();
    }

    public void PlayBackgroundVideo()
    {
        if (defaultClip != null && defaultTargetTexture != null)
        {
            NecoCanvas.GetGameCanvas().OnUICurtain(true);

            PlayVideo(defaultClip, defaultTargetTexture, true, false, () =>
            {
                NecoCanvas.GetGameCanvas().OnUICurtain(false);
            });
        }
    }

    public void PlayVideo(VideoClip clip, RenderTexture targetTexture, bool loop = false, bool useSound = true, Callback loadedCallback = null)
    {
        if (videoLoadCoroutine != null)
        {
            StopCoroutine(videoLoadCoroutine);
            LoadedCallback?.Invoke();            
        }

        LoadedCallback = null;

        videoLoadCoroutine = StartCoroutine(VideoLoadCoroutine(clip, targetTexture, loop, useSound, loadedCallback));
    }

    public IEnumerator SetVideoCoroutine(VideoClip clip, RenderTexture targetTexture)
    {
        if (videoLoadCoroutine != null)
        {
            StopCoroutine(videoLoadCoroutine);
            LoadedCallback?.Invoke();
        }

        LoadedCallback = null;
        videoLoadCoroutine = StartCoroutine(VideoLoadCoroutine(clip, targetTexture));
        yield return videoLoadCoroutine;
    }
    private IEnumerator VideoLoadCoroutine(VideoClip clip, RenderTexture targetTexture, bool loop = false, bool useSound = true, Callback loadedCallback = null)
    {
        LoadedCallback = loadedCallback;

        videoPlayer.Stop();
        
        useSound = PlayerPrefs.GetInt("Setting_SFX", 1) == 1;

        RenderTexture target = videoPlayer.targetTexture;
        if (target)
        {
            RenderTexture rt = RenderTexture.active;
            RenderTexture.active = target;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = rt;
        }

        videoPlayer.clip = clip;
        videoPlayer.targetTexture = targetTexture;

        videoPlayer.isLooping = loop;

        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared)
        {
            yield return new WaitForEndOfFrame();
        }

        videoPlayer.SetDirectAudioMute(0, !useSound);
        videoPlayer.Play();

        if (useSound)
        {
            videoPlayer.SetTargetAudioSource(0, videoAudioSource);
            videoAudioSource.Play();
            AudioManager.GetInstance().PauseBackgroundAudio();
        }

        LoadedCallback?.Invoke();
        LoadedCallback = null;
    }

    public void StopVideo(bool returnDefault = true)
    {
        videoPlayer.Stop();

        if (returnDefault)
        {
            PlayBackgroundVideo();
            AudioManager.GetInstance().ResumeBackgroundAudio();
        }
    }

    public void PauseVideo()
    {
        videoPlayer.Pause();
    }

    public void ResumeVideo()
    {
        videoPlayer.Play();
    }
}

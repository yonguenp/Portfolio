using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager
{
    AudioSource bgmAudioSource = null;

    GameObject effectGameObject = null;
    List<AudioSource> effectAudioSources = null;

    Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();

    // MP3 Player   -> AudioSource
    // MP3 음원     -> AudioClip
    // 관객(귀)     -> AudioListener

    GameObject _root = null;
    public void Init()
    {
        _root = GameObject.Find("@SoundManager");
        if (_root != null)
        {
            Object.DestroyImmediate(_root);
            _root = null;
        }

        _root = new GameObject { name = "@SoundManager" };
        Object.DontDestroyOnLoad(_root);

        GameObject bgmGameObject = new GameObject { name = Sound.Bgm.ToString() };
        bgmGameObject.transform.SetParent(_root.transform);
        bgmAudioSource = bgmGameObject.AddComponent<AudioSource>();
        bgmAudioSource.loop = true;

        effectGameObject = new GameObject { name = Sound.Effect.ToString() };
        effectGameObject.transform.SetParent(_root.transform);
        effectAudioSources = new List<AudioSource>();

        RefreshConfig();
    }

    public void Clear()
    {
        bgmAudioSource.clip = null;
        bgmAudioSource.Stop();

        foreach (AudioSource audioSource in effectAudioSources)
        {
            if (audioSource == null)
                continue;
            audioSource.clip = null;
            audioSource.Stop();
        }

        _audioClips.Clear();
    }

    public AudioSource Play(string path, Sound type = Sound.Effect, float volume = 1.0f, float pitch = 1.0f, bool loop = false)
    {
        AudioClip audioClip = GetOrAddAudioClip(path, type);
        return Play(audioClip, type, volume, pitch, loop);
    }

    public AudioSource Play(AudioClip audioClip, Sound type = Sound.Effect, float volume = 1.0f, float pitch = 1.0f, bool loop = false)
    {
        if (audioClip == null)
            return null;

        if (type == Sound.Bgm)
        {
            bool sameClip = bgmAudioSource.clip == audioClip;
            if (!sameClip)
            {
                if (bgmAudioSource.isPlaying)
                    bgmAudioSource.Stop();
            }
            else
            {
                if (!bgmAudioSource.isPlaying)
                    bgmAudioSource.Play();
            }

            bgmAudioSource.pitch = pitch;
            bgmAudioSource.clip = audioClip;
            bgmAudioSource.volume = volume;

            if (!sameClip)
            {
                bgmAudioSource.Play();
            }


            if (!GameConfig.Instance.OPTION_BGM)
                bgmAudioSource.Stop();

            return bgmAudioSource;
        }
        else if (type == Sound.Effect && GameConfig.Instance.OPTION_SFX)
        {
            AudioSource targetSouce = null;
            for (int i = effectAudioSources.Count - 1; i >= 0; i--)
            {
                AudioSource audioSource = effectAudioSources[i];
                if (audioSource == null)
                {
                    continue;
                }

                if (audioSource.clip == null || audioSource.isPlaying == false)
                {
                    if (targetSouce == null)
                        targetSouce = audioSource;
                    else
                    {
                        Object.Destroy(effectAudioSources[i].gameObject);
                        effectAudioSources.RemoveAt(i);
                    }
                }

                if (audioSource.clip == audioClip)
                {
                    if (audioSource.isPlaying && audioSource.time < 0.5f)
                        return null;
                }
            }

            if (targetSouce == null)
            {
                GameObject tmp = new GameObject { name = Sound.Effect.ToString() };
                tmp.transform.SetParent(effectGameObject.transform);

                targetSouce = tmp.AddComponent<AudioSource>();
                targetSouce.loop = false;
                effectAudioSources.Add(targetSouce);
            }

            targetSouce.volume = volume;
            targetSouce.pitch = pitch;
            targetSouce.clip = audioClip;
            targetSouce.loop = loop;
            targetSouce.Play();

            return targetSouce;
        }

        return null;
    }

    public void Stop(AudioSource target)
    {
        for (int i = effectAudioSources.Count - 1; i >= 0; i--)
        {
            if (effectAudioSources[i] == target)
            {
                Object.Destroy(effectAudioSources[i].gameObject);
                effectAudioSources.RemoveAt(i);
                return;
            }
        }
    }

    
    AudioClip GetOrAddAudioClip(string path, Sound type = Sound.Effect)
    {
        if (path.Contains("AssetsBundle/Sounds/") == false)
            path = $"AssetsBundle/Sounds/{path}";

        AudioClip audioClip = null;

        if (type == Sound.Bgm)
        {
            audioClip = Managers.Resource.LoadAssetsBundle<AudioClip>(path);
        }
        else
        {
            if (_audioClips.TryGetValue(path, out audioClip) == false)
            {
                audioClip = Managers.Resource.LoadAssetsBundle<AudioClip>(path);
                _audioClips.Add(path, audioClip);
            }
        }

        if (audioClip == null)
            SBDebug.Log($"AudioClip Missing ! {path}");

        return audioClip;
    }

    public void BGMStop()
    {
        if(bgmAudioSource.isPlaying)
            bgmAudioSource.Stop();
    }

    public void BGMPlay()
    {
        if(!bgmAudioSource.isPlaying)
            bgmAudioSource.Play();
    }

    public void SFXStop()
    {
        foreach(AudioSource sfxAudio in effectAudioSources)
        {
            Object.Destroy(sfxAudio);
        }
        effectAudioSources.Clear();
    }

    public void SFXPlay()
    {
        
    }

    public void Reset()
    {

    }

    public void Mute()
    {
        AudioListener.volume = 0f;
    }

    public void UnMute()
    {
        AudioListener.volume = 1f;
    }

    public void RefreshConfig()
    {
        if (!GameConfig.Instance.OPTION_SFX)
            SFXPlay();
        else
            SFXStop();

        if (GameConfig.Instance.OPTION_BGM)
            BGMPlay();
        else
            BGMStop();
    }
}

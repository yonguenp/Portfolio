
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class SoundManager : IManagerBase
    {
        private static SoundManager instance = null;
        public static SoundManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SoundManager();
                }
                return instance;
            }
        }
        
        public Game Game
        {
            get
            {   
                return Game.Instance;
            }
        }

        const float FADE_VALUE = 1f;

        public float MasterVolume { get; protected set; } = 1f;
        public float BgmVolume { get; protected set; } = 1f;
        public float EffectVolume { get; protected set; } = 1f;
        public float BGMVolume { get { return MasterVolume * BgmVolume; } }
        public float SFXVolume { get { return MasterVolume * EffectVolume; } }

        //폴더 클립
        protected Dictionary<string, AudioClip> _clips = null;
        protected List<AudioSource> _playSoundList = null;
        protected List<AudioSource> _loopingSounds = null;
        protected Stack<BGMData> _playBGMStack = null;
        protected SBListPool<AudioSource> _sourcePool = null;
        private int poolCount = 0;

        List<SoundResourceData> soundDataList = null;

        public void Initialize()
        {
            StopBGM();

            if (_clips == null)
                _clips = new Dictionary<string, AudioClip>();
            else
                _clips.Clear();

            if (_playSoundList == null)
                _playSoundList = new List<AudioSource>();
            else
            {
                foreach (var s in _playSoundList)
                {
                    Object.Destroy(s.gameObject);
                }
                _playSoundList.Clear();
            }

            if (_loopingSounds == null)
                _loopingSounds = new List<AudioSource>();
            else
            {
                foreach (var s in _loopingSounds)
                {
                    Object.Destroy(s.gameObject);
                }
                _loopingSounds.Clear();
            }

            if (_playBGMStack == null)
                _playBGMStack = new Stack<BGMData>();
            else
                _playBGMStack.Clear();

            if (_sourcePool == null)
                _sourcePool = new SBListPool<AudioSource>(ReuseSource, UnuseSource);

            if (soundDataList == null)
                soundDataList = new List<SoundResourceData>();

            //ClipsLoad();

            MasterVolume = PlayerPrefs.GetFloat("masterVolume", 1f);
            BgmVolume = PlayerPrefs.GetFloat("bgmVolume", 1f);
            EffectVolume = PlayerPrefs.GetFloat("effectVolume", 1f);

            SpawnPool(5);
        }

        public void Update(float dt) {}

        #region SFX
        public AudioClip GetClip(string soundKey)
        {
            if (_clips == null)
                return null;
            
            if (_clips.ContainsKey(soundKey) && _clips[soundKey] != null)
            {
                return _clips[soundKey];
            }
            else
            {
                string soundName = SoundResourceData.GetSoundNameBySoundKey(soundKey);
                if (soundName != string.Empty)
                {
                    AudioClip resource= ResourceManager.GetResource<AudioClip>(eResourcePath.SfxSoundPath, soundName);
                    if (resource != null)
                    {
                        _clips[soundKey] = resource;
                        return _clips[soundKey];
                    }
                }
                else
                {
                    AudioClip resource = ResourceManager.GetResource<AudioClip>(eResourcePath.SfxSoundPath, soundKey);
                    if(resource != null)
                    {
                        _clips[soundKey] = resource;
                        return _clips[soundKey];
                    }
                        
                }
            }

            return null;
        }
        public AudioSource PlaySFX(string soundKey, bool loop = false, float _delay = 0f, Vector3 _location = default)
        {
            if (string.IsNullOrWhiteSpace(soundKey))
                return null;
            if (_clips == null)
                return null;


            if (_clips.ContainsKey(soundKey) && _clips[soundKey] != null)
            {
                if (_delay <= 0f)
                    return PlaySound(_clips[soundKey], _location, loop);
                else
                    Game.StartCoroutine(PlayDelaySound(_clips[soundKey], _delay, _location, loop));
            }
            else
            {
                // FX는 사운드 키와 사운드 이름이 일치하거나 일치하지 않을 수 있음
                string soundName = SoundResourceData.GetSoundNameBySoundKey(soundKey);
                if (soundName != string.Empty)
                {
                    AudioClip resource = ResourceManager.GetResource<AudioClip>(eResourcePath.SfxSoundPath, soundName);
                    if(resource != null)
                    {
                        _clips[soundKey] = resource;
                        if (_delay <= 0f)
                            return PlaySound(_clips[soundKey], _location, loop);
                        else
                            Game.StartCoroutine(PlayDelaySound(_clips[soundKey], _delay, _location, loop));
                    }
                    else
                        Debug.LogWarningFormat("Not Loaded Sound => {0}", soundKey);
                }
                else
                {
                    AudioClip resource = ResourceManager.GetResource<AudioClip>(eResourcePath.SfxSoundPath, soundKey);
                    if (resource != null)
                    {
                        _clips[soundKey] = resource;
                        if (_delay <= 0f)
                            return PlaySound(_clips[soundKey], _location, loop);
                        else
                            Game.StartCoroutine(PlayDelaySound(_clips[soundKey], _delay, _location, loop));
                    }
                    else
                        Debug.LogWarningFormat("Not Loaded Sound => {0}", soundKey);
                }
                
            }

            return null;
        }
        public void PlaySFX(AudioClip _clip, bool loop = false, float _delay = 0f, Vector3 _location = default)
        {
            if (_clip == null)
                return;

            if (_delay <= 0.0f)
                PlaySound(_clip, _location, loop);
            else
                Game.StartCoroutine(PlayDelaySound(_clip, _delay, _location, loop));
        }
        public void StopSFX(string _soundName)
        {
            var SfxIT = _playSoundList.GetEnumerator();
            _soundName = SBFunc.StrBuilder("Temp_", _soundName);
            var isStop = false;

            while (SfxIT.MoveNext())
            {
                if (SfxIT.Current == null)
                    continue;

                if (SfxIT.Current.name == _soundName)
                {
                    SfxIT.Current.Stop();
                    isStop = true;
                    break;
                }
            }

            if (isStop)
                return;

            var LoopIT = _loopingSounds.GetEnumerator();

            while (LoopIT.MoveNext())
            {
                if (LoopIT.Current == null)
                    continue;

                if (LoopIT.Current.name == _soundName)
                {
                    StopLoopingSound(LoopIT.Current);
                    break;
                }
            }
        }
        public void StopLoopingSound(AudioSource source)
        {
            if (source != null)
            {
                source.Stop();
                _loopingSounds.Remove(source);
                _sourcePool.Put(source);
            }
        }
        protected IEnumerator PlayDelaySound(AudioClip _clip, float _delay, Vector3 _location, bool _loop)
        {
            yield return SBDefine.GetWaitForSeconds(_delay);

            PlaySound(_clip, _location, _loop);
        }
        protected IEnumerator TrackingSoundCO(AudioSource source, float delay, bool isDelete)
        {
            if (source == null)
                yield break;

            _playSoundList.Add(source);

            yield return SBDefine.GetWaitForSeconds(delay);

            _playSoundList.Remove(source);
            if (isDelete && source != null)
            {
                _sourcePool.Put(source);
            }
        }
        public virtual AudioSource PlaySound(AudioClip sfx, Vector3 location, bool loop = false, float power = 1f)
        {
            if (!loop && (sfx == null || SFXVolume <= 0f))
                return null;

            if (!loop)
            {
                foreach (var source in _sourcePool.datas)
                {
                    if (source.clip == null)
                        continue;

                    if (source.clip == sfx)
                    {
                        if(source.isPlaying)
                            return null;
                    }
                }
            }
            // we add an audio source to that host
            AudioSource audioSource = GetSource();
            // we set that audio source clip to the one in paramaters
            audioSource.clip = sfx;
            // we set the audio source volume to the one in parameters
            audioSource.volume = SFXVolume;
            // we set our loop setting
            audioSource.loop = loop;
            // we start playing the sound
            audioSource.Play();

            if (!loop)
                Game.StartCoroutine(TrackingSoundCO(audioSource, sfx.length, true));
            else
                _loopingSounds.Add(audioSource);

            // we return the audiosource reference
            return audioSource;
        }

        public virtual AudioSource PlaySound(AudioClip sfx, Vector3 location, float pitch, float pan, float spatialBlend = 0.0f, float volumeMultiplier = 1.0f, bool loop = false,
            AudioSource reuseSource = null, UnityEngine.Audio.AudioMixerGroup audioGroup = null)
        {
            if (!loop && (sfx == null || SFXVolume <= 0f))
                return null;

            if (!loop)
            {
                foreach (var source in _sourcePool.datas)
                {
                    if (source.clip == null)
                        continue;

                    if (source.clip == sfx)
                    {
                        if (source.isPlaying)
                            return null;
                    }
                }
            }

            if (reuseSource == null)
            {
                // we add an audio source to that host
                var newAudioSource = GetSource();
                reuseSource = newAudioSource;
                reuseSource.transform.parent = Game.transform;
                reuseSource.transform.position = location;
            }
            // we set the temp audio's position

            reuseSource.time = 0.0f; // Reset time in case it's a reusable one.

            // we set that audio source clip to the one in paramaters
            reuseSource.clip = sfx;

            reuseSource.pitch = pitch;
            reuseSource.spatialBlend = spatialBlend;
            reuseSource.panStereo = pan;

            // we set the audio source volume to the one in parameters
            reuseSource.volume = SFXVolume * volumeMultiplier;
            // we set our loop setting
            reuseSource.loop = loop;
            // Assign an audio mixer group.
            if (audioGroup != null)
                reuseSource.outputAudioMixerGroup = audioGroup;


            // we start playing the sound
            reuseSource.Play();

            if (!loop)
                Game.StartCoroutine(TrackingSoundCO(reuseSource, sfx.length, reuseSource == null));
            else
                _loopingSounds.Add(reuseSource);

            // we return the audiosource reference
            return reuseSource;
        }
        #endregion
        #region BGM
        public virtual BGMData PlayBGM()
        {
            if (_playBGMStack == null || _playBGMStack.Count < 1)
                return null;

            var target = _playBGMStack.Peek();
            if(target != null)
            {
                target.BGMAudioSource.volume = BGMVolume;
                target.BGMAudioSource.Play();
            }

            return target;
        }
        public virtual BGMData StopBGM()
        {
            if (_playBGMStack == null || _playBGMStack.Count < 1)
                return null;

            var target = _playBGMStack.Peek();
            if (target != null)
            {
                target.BGMAudioSource.volume = BGMVolume;
                target.BGMAudioSource.Stop();
            }

            return target;
        }
        public virtual void PushBGM(BGMData Music, bool isLoop = true)
        {
            if (_playBGMStack == null || Music == null)
                return;

            // BGM 데이터 구성
            BGMData currentBGMData = new BGMData();
            currentBGMData.isSmoothChange = Music.isSmoothChange;
            currentBGMData.BGMAudioSource = GetSource(Music.BGMAudioSource);

            BGMData prev = null;
            if (_playBGMStack != null && _playBGMStack.Count > 0)
            {
                prev = _playBGMStack.Pop();
            }

            currentBGMData.BGMAudioSource.volume = 0f;
            currentBGMData.BGMAudioSource.loop = isLoop;

            _playBGMStack.Push(currentBGMData);

            BGMSoundChange(prev, currentBGMData);
        }

        public virtual void PushBGM(string soundKey, bool isSmoothChange, bool isLoop = true)
        {
            if (string.IsNullOrWhiteSpace(soundKey))
                return;
            if (_playBGMStack == null)
                return;

            // 복제본 생성

            AudioSource currentAudio = GetSource();
            if (_clips.ContainsKey(soundKey))
            {
                currentAudio.clip = _clips[soundKey];
            }
            else
            {
                // bgm 은 사운드 키와 사운드 이름이 일치하지 않음
                string soundName = SoundResourceData.GetSoundNameBySoundKey(soundKey);
                if (soundName != string.Empty)
                {
                    AudioClip resource = ResourceManager.GetResource<AudioClip>(eResourcePath.BgmSoundPath, soundName);
                    if (resource != null)
                    {
                        currentAudio.clip = _clips[soundKey] = resource;
                    }
                }
            }
            currentAudio.playOnAwake = false;

            // BGM 데이터 구성
            BGMData currentBGMData = new BGMData();
            currentBGMData.isSmoothChange = isSmoothChange;
            currentBGMData.BGMAudioSource = currentAudio;

            BGMData prev = null;
            if (_playBGMStack != null && _playBGMStack.Count > 0)
            {
                prev = _playBGMStack.Pop();
            }

            currentBGMData.BGMAudioSource.volume = 0f;
            currentBGMData.BGMAudioSource.loop = isLoop;

            _playBGMStack.Push(currentBGMData);

            BGMSoundChange(prev, currentBGMData);
        }

        public virtual void PopBGM()
        {
            BGMData prev = null;

            if (_playBGMStack.Count > 0)
                prev = _playBGMStack.Pop();

            if (_playBGMStack == null || _playBGMStack.Count == 0)
            {
                BGMSoundChange(prev, null);
                return;
            }

            // we set the background music clip
            var target = _playBGMStack.Peek();
            if (target == null)
            {
                BGMSoundChange(prev, null);
                return;
            }

            BGMSoundChange(prev, target);
        }
        #endregion
        #region Volume
        protected IEnumerator curSoundInCO = null;
        protected BGMData nextSource = null;
        void BGMSoundChange(BGMData prev, BGMData next)
        {
            if (prev != null)
            {
                Game.StartCoroutine(SoundOut(prev));
            }

            if (next != null)
            {
                if (curSoundInCO != null)
                {
                    Game.StopCoroutine(curSoundInCO);
                    if (nextSource != null && nextSource.BGMAudioSource != null && nextSource.BGMAudioSource.volume > 0f)
                    {
                        Game.StartCoroutine(SoundOut(nextSource));
                        nextSource = null;
                    }
                    curSoundInCO = null;
                }

                curSoundInCO = SoundIn(next);
                Game.StartCoroutine(curSoundInCO);
            }
        }
        IEnumerator SoundIn(BGMData inSound)
        {
            if (inSound != null)
            {
                nextSource = inSound;
                inSound.BGMAudioSource.volume = inSound.isSmoothChange ? 0f : BGMVolume;
                inSound.BGMAudioSource.Play();
                while (inSound.BGMAudioSource.volume < BGMVolume)
                {
                    yield return null;
                    inSound.BGMAudioSource.volume += Time.deltaTime * FADE_VALUE;

                    if (inSound == null || inSound.BGMAudioSource == null)
                        yield break;
                }
                inSound.BGMAudioSource.volume = BGMVolume;
            }
            curSoundInCO = null;
        }
        IEnumerator SoundOut(BGMData outSound)
        {
            if (outSound != null)
            {
                outSound.BGMAudioSource.volume = outSound.isSmoothChange ? outSound.BGMAudioSource.volume : 0f;
                while (outSound.BGMAudioSource.volume > 0f)
                {
                    yield return null;

                    if (outSound == null || outSound.BGMAudioSource == null)
                        yield break;

                    outSound.BGMAudioSource.volume -= Time.deltaTime * FADE_VALUE;
                }
                outSound.BGMAudioSource.volume = 0f;
                outSound.BGMAudioSource.Stop();

                _sourcePool.Put(outSound.BGMAudioSource);
            }
        }
        public void SetMasterVolume(float _volume)
        {
            MasterVolume = _volume;
            PlayerPrefs.SetFloat("masterVolume", MasterVolume);
            BackgroundVolumeControl();
            EffectVolumeControl();
        }
        public void SetBgmVolume(float _volume)
        {
            BgmVolume = _volume;
            PlayerPrefs.SetFloat("bgmVolume", BgmVolume);
            BackgroundVolumeControl();
        }
        public void SetEffectVolume(float _volume)
        {
            EffectVolume = _volume;
            PlayerPrefs.SetFloat("effectVolume", EffectVolume);
            EffectVolumeControl();
        }
        public void VolumeControl(float mVolume, float bVolume, float eVolume)
        {
            MasterVolume = mVolume;
            BgmVolume = bVolume;
            EffectVolume = eVolume;
            BackgroundVolumeControl();
            EffectVolumeControl();
        }

        public void BackgroundVolumeControl()
        {
            if(_playBGMStack != null && _playBGMStack.Count > 0)
            {
                var bgm = _playBGMStack.Peek();
                if (bgm != null && bgm.BGMAudioSource != null)
                    bgm.BGMAudioSource.volume = BGMVolume;
            }
        }

        public void EffectVolumeControl()
        {
            if (_loopingSounds != null && _loopingSounds.Count > 0)
            {
                for (int i = 0; i < _loopingSounds.Count; i++)
                {
                    if (_loopingSounds[i] == null)
                        continue;

                    _loopingSounds[i].volume = SFXVolume;
                }
            }

            if (_playSoundList != null && _playSoundList.Count > 0)
            {
                for (int i = 0; i < _playSoundList.Count; i++)
                {
                    if (_playSoundList[i] == null)
                        continue;

                    _playSoundList[i].volume = SFXVolume;
                }
            }
        }
        #endregion
        #region SourcePool
        private AudioSource GetSource()
        {
            if (_sourcePool.Count < 1)
                SpawnPool(1);

            return _sourcePool.Get();
        }
        private AudioSource GetSource(AudioSource data)
        {
            if (data == null)
                return null;

            var source = GetSource();
            source.clip = data.clip;
            source.time = 0.0f;
            source.volume = data.volume;
            source.playOnAwake = data.playOnAwake;
            source.loop = data.loop;
            source.pitch = data.pitch;
            source.spatialBlend = data.spatialBlend;
            source.panStereo = data.panStereo;
            source.outputAudioMixerGroup = data.outputAudioMixerGroup;

            return source;
        }
        private void SpawnPool(int count)
        {
            if (_sourcePool == null)
                return;

            while (_sourcePool.Count < count)
            {
                GameObject soundObject = new GameObject(SBFunc.StrBuilder("Sound_", poolCount));
                soundObject.transform.parent = Game.transform;
                soundObject.transform.position = default;
                _sourcePool.Put(soundObject.AddComponent<AudioSource>());
                poolCount++;
            }
        }
        private void ReuseSource(AudioSource source)
        {
            if (source == null)
                return;

            source.Stop();
            source.clip = null;
            source.time = 0.0f;
            source.volume = 0f;
            source.playOnAwake = false;
            source.loop = false;
            source.pitch = 1f;
            source.spatialBlend = 0f;
            source.panStereo = 0f;
            source.outputAudioMixerGroup = null;

            source.gameObject.SetActive(true);            
        }
        private void UnuseSource(AudioSource source)
        {
            if (source == null)
                return;

            source.Stop();
            source.clip = null;
            source.time = 0.0f;
            source.volume = 0f;
            source.playOnAwake = false;
            source.loop = false;
            source.pitch = 1f;
            source.spatialBlend = 0f;
            source.panStereo = 0f;
            source.outputAudioMixerGroup = null;

            source.gameObject.SetActive(false);
        }
        #endregion
        public void AllStopSound()
        {
            if (_playSoundList != null)
            {
                while (_playSoundList.Count > 0)
                {
                    if (_playSoundList[0] == null)
                    {
                        _playSoundList.RemoveAt(0);
                        continue;
                    }

                    _playSoundList[0].Stop();
                    _playSoundList.RemoveAt(0);
                }
            }

            if (_loopingSounds != null)
            {
                while (_loopingSounds.Count > 0)
                {
                    if (_loopingSounds[0] == null)
                    {
                        _loopingSounds.RemoveAt(0);
                        continue;
                    }

                    StopLoopingSound(_loopingSounds[0]);
                }
            }

            if (_playBGMStack != null)
            {
                if (_playBGMStack.Count > 0)
                    _playBGMStack.Peek().BGMAudioSource.Stop();

                _playBGMStack.Clear();
            }
        }
    }
}
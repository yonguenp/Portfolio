using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundController : MonoBehaviour
{
    public enum PlayType
    {
        Invalid = 0,
        OnlyMe = 1,
        Friend = 2,
        Broadcast = 3,
        UI = 4,
        Nobody,
    }

    [SerializeField] string[] audioClips_key;
    [SerializeField] int maxDistance = 15;
    [SerializeField] bool needDistanceCheck = true;
    [SerializeField] AudioClip[] audioClips;
    [SerializeField] PlayType playType = PlayType.Broadcast;
    [SerializeField] bool loop = false;

    CharacterObject rootCharcter = null;
    Button targetButton;
    bool effectSoundPlayed = false;
    AudioSource audioSource = null;

    public void Init(int distance, bool distanceCheck, PlayType type)
    {
        maxDistance = distance;
        needDistanceCheck = distanceCheck;
        playType = type;
    }

    void OnEnable()
    {
        rootCharcter = GetComponent<CharacterObject>();
    }

    private void OnDisable()
    {
        if (loop)
        {
            if (audioSource != null)
            {
                foreach (AudioClip clip in audioClips)
                {
                    if (audioSource.clip == clip)
                    {
                        audioSource.Stop();
                        audioSource = null;
                        break;
                    }
                }
            }
        }
    }

    private void Start()
    {
        if (playType != PlayType.UI)
        {
            if (!effectSoundPlayed)
                PlaySound();
        }
        else
        {
            if (targetButton == null)
            {
                targetButton = GetComponent<Button>();
                if (targetButton != null)
                    targetButton.onClick.AddListener(PlaySound);
                else
                    PlaySound();    // 버튼이 아닌 애들은 바로 소리 재생
            }
        }
    }

    bool IsSoundPlayable(PlayType type)
    {
        if (!GameConfig.Instance.OPTION_SFX)
            return false;

        if (type == PlayType.Nobody)
            return false;

        // 버튼은 무조건 재생
        if (type == PlayType.UI)
            return true;

        if (Managers.Scene.CurrentScene.name != "GameScene")
            return false;

        if (needDistanceCheck)
        {
            var distance = Vector2.Distance(Camera.main.transform.position, this.transform.position);
            if (distance > maxDistance)
            {
                return false;
            }
        }

        if (rootCharcter == null)
            rootCharcter = gameObject.GetComponentInParent<CharacterObject>();

        if (rootCharcter != null)
        {
            switch (type)
            {
                case PlayType.OnlyMe:
                    if (!rootCharcter.IsMe)
                        return false;
                    break;

                case PlayType.Friend:
                    if (!rootCharcter.IsFriend)
                        return false;
                    break;

                default:
                    break;
            }
        }

        return true;
    }

    public void PlaySound()
    {
        if (audioClips_key != null && audioClips_key.Length == 0)
        {
            string[] temp = new string[1];
            temp[0] = this.gameObject.name.ToUpper().Replace("(CLONE)", "");
            audioClips_key = temp;
        }
        if (audioClips_key != null && audioClips_key.Length > 0)
        {
            List<AudioClip> temp = new List<AudioClip>();
            foreach (string keys in audioClips_key)
            {
                AudioClip audio = SoundResourceData.GetAudioClip(keys);
                if (audio == null)
                    return;
                temp.Add(audio);
            }
            audioClips = temp.ToArray();
            temp.Clear();
        }

        if (audioClips == null || audioClips.Length == 0)
            return;

        AudioClip audioClip = audioClips[Random.Range(0, audioClips.Length)];
        if (audioClip == null)
            return;

        if (IsSoundPlayable(playType))
        {
            audioSource = Managers.Sound.Play(audioClip, loop: loop);
            effectSoundPlayed = true;
        }
    }

    public void SetPlayType(PlayType type)
    {
        playType = type;
    }

    public AudioSource Play(string path, PlayType overrideType = PlayType.Invalid)
    {
        if (overrideType == PlayType.Invalid)
        {
            overrideType = playType;
        }

        if (IsSoundPlayable(overrideType))
        {
            audioSource = Managers.Sound.Play(path, Sound.Effect);
            return audioSource;
        }

        else return null;
    }
}

public class SoundResourceData : GameData
{
    public int uid { get; private set; }
    public string sound_key { get; private set; }
    public string resource_path { get; private set; }

    public AudioClip audioclip { get; private set; }

    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        uid = Int(data["uid"]);
        sound_key = data["sound_key"];
        resource_path = data["resource_path"];
        audioclip = Managers.Resource.LoadAssetsBundle<AudioClip>(resource_path);
    }

    static public AudioClip GetAudioClip(string key)
    {
        foreach (SoundResourceData data in GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.sound_resource, true))
        {
            if (data.sound_key == key)
            {
                return data.audioclip;
            }
        }
        return null;
    }
}

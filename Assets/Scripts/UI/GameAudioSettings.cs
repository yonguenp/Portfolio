using UnityEngine;

/// <summary>
/// 전 게임 공용 볼륨 설정. PlayerPrefs에 저장한다.
///
/// 지금 소리가 있는 건 BrickBreaker3D뿐이지만(BrickBreakerAudio), 이후 게임에
/// 소리가 붙어도 전부 여기 하나만 보게 해서 볼륨 조절 화면이 게임마다
/// 따로 생기지 않게 한다. 값은 매 프레임/매 재생 시점에 새로 읽으므로
/// 슬라이더를 드래그하면 재생 중인 배경음에도 바로 반영된다 — 별도 이벤트가
/// 필요 없다.
/// </summary>
public static class GameAudioSettings
{
    const string BGM_KEY = "Vol_Bgm";
    const string SFX_KEY = "Vol_Sfx";

    static float bgm = -1f, sfx = -1f;

    public static float Bgm
    {
        get { if (bgm < 0f) bgm = PlayerPrefs.GetFloat(BGM_KEY, 0.8f); return bgm; }
        set
        {
            bgm = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(BGM_KEY, bgm);
            PlayerPrefs.Save();
        }
    }

    public static float Sfx
    {
        get { if (sfx < 0f) sfx = PlayerPrefs.GetFloat(SFX_KEY, 0.8f); return sfx; }
        set
        {
            sfx = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(SFX_KEY, sfx);
            PlayerPrefs.Save();
        }
    }
}

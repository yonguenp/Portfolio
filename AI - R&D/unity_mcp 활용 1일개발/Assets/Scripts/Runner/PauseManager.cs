using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;
    public bool IsPaused { get; private set; }

    void Awake() { Instance = this; }

    public void Pause()
    {
        if (RunnerGameManager.Instance?.isPlaying != true) return;
        IsPaused         = true;
        Time.timeScale   = 0f;
        AudioManager.Instance?.PlayButton();
        RunnerGameManager.Instance?.uiManager?.ShowPause();
    }

    public void Resume()
    {
        IsPaused       = false;
        Time.timeScale = 1f;
        AudioManager.Instance?.PlayButton();
        RunnerGameManager.Instance?.uiManager?.ShowHUD();
    }

    public void QuitToMenu()
    {
        IsPaused       = false;
        Time.timeScale = 1f;
        RunnerGameManager.Instance?.RestartGame();
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [SerializeField] Button continueButton;
    [SerializeField] GameObject optionsPanel;

    static readonly string[] GameScenes = { "GameScene", "Game1to50Scene", "Game2048Scene", "Game1010Scene", "GameBrickBreakerScene", "GoStopScene", "GoStop3PScene" };

    GoStopModeChoiceUI goStopChoice;

    void Start()
    {
        if (continueButton)
            continueButton.gameObject.SetActive(SaveManager.HasSave());

        // 설정(볼륨/라이선스) 팝업은 씬에 미리 만들어두지 않고 여기서 붙인다 —
        // 언어 버튼처럼 씬마다 반복해서 만들 필요 없이 타이틀에서 한 번만 뜬다.
        var canvas = GetComponentInParent<Canvas>();
        var safe   = GameObject.Find("SafeArea");
        var lang   = GameObject.Find("LangBtn");
        if (canvas != null && safe != null && lang != null)
            TitleOptionsUI.Create(canvas.GetComponent<RectTransform>(),
                                  safe.GetComponent<RectTransform>(),
                                  lang.GetComponent<RectTransform>());

        // 고스톱 카드는 곧바로 씬을 열지 않고 2인/3인 선택 팝업을 먼저 띄운다
        // (2026-08-16, 3인 고스톱 추가). 같은 이유로 여기서 한 번만 만든다.
        if (canvas != null)
            goStopChoice = GoStopModeChoiceUI.Create(canvas.GetComponent<RectTransform>());
    }

    public void OnContinue()   => SceneManager.LoadScene(SaveManager.GetSaveScene());
    public void OnRandom()     => SceneManager.LoadScene(GameScenes[Random.Range(0, GameScenes.Length)]);
    public void OnColorSort()  => SceneManager.LoadScene("GameScene");
    public void On1to50()      => SceneManager.LoadScene("Game1to50Scene");
    public void On2048()       => SceneManager.LoadScene("Game2048Scene");
    public void On1010()       => SceneManager.LoadScene("Game1010Scene");
    public void OnBrickBreaker() => SceneManager.LoadScene("GameBrickBreakerScene");
    public void OnGoStop()       => goStopChoice?.Open();
    public void OnOptions()    { if (optionsPanel) optionsPanel.SetActive(true); }

    public void OnExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

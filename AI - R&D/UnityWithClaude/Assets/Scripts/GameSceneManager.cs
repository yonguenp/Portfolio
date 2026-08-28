using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    void Start()
    {
        SaveManager.WriteSave("GameScene");

        var btn = GameObject.Find("ReturnButton");
        btn?.GetComponent<Button>()?.onClick.AddListener(() => SceneManager.LoadScene("TitleScene"));
    }
}

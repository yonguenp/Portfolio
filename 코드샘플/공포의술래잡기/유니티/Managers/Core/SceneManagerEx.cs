using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SceneManagerEx
{
    public BaseScene CurrentScene { get { return GameObject.FindObjectOfType<BaseScene>(); } }

    public void LoadScene(SceneType type)
    {
        Managers.Clear();

        SceneManager.LoadScene(GetSceneName(type));
    }

    public AsyncOperation LoadSceneAsync(SceneType type, UnityAction<UnityEngine.SceneManagement.Scene, LoadSceneMode> LoadSceneEnd = null)
    {
        Managers.Clear();

        if (LoadSceneEnd != null)
            SceneManager.sceneLoaded += LoadSceneEnd;

        return SceneManager.LoadSceneAsync(GetSceneName(type));
    }

    string GetSceneName(SceneType type)
    {
        string name = System.Enum.GetName(typeof(SceneType), type);
        return name;
    }

    public void Clear()
    {
        CurrentScene.Clear();
    }
}

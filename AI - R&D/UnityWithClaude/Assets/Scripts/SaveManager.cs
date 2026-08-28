using UnityEngine;

public static class SaveManager
{
    const string HasSaveKey = "HasSave";
    const string SaveSceneKey = "SaveScene";

    public static bool HasSave() => PlayerPrefs.GetInt(HasSaveKey, 0) == 1;
    public static string GetSaveScene() => PlayerPrefs.GetString(SaveSceneKey, "GameScene");

    public static void WriteSave(string scene = "GameScene")
    {
        PlayerPrefs.SetInt(HasSaveKey, 1);
        PlayerPrefs.SetString(SaveSceneKey, scene);
        PlayerPrefs.Save();
    }

    public static void EraseSave()
    {
        PlayerPrefs.DeleteKey(HasSaveKey);
        PlayerPrefs.DeleteKey(SaveSceneKey);
        PlayerPrefs.Save();
    }
}

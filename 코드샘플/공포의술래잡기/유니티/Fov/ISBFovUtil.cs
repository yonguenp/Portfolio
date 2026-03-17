public interface ISBFovUtil
{
    void OnEnter();
    void OnExit();
    void UtilUpdate(SBFieldOfView view, float time);
}

[System.Serializable]
public abstract class SBFovUtil : UnityEngine.MonoBehaviour, ISBFovUtil
{
    public abstract void OnEnter();
    public abstract void OnExit();
    public abstract void UtilUpdate(SBFieldOfView view, float time);
}

using UnityEngine;

public interface IController
{
    void InitController(IControllerListener listener);
}

public interface IControllerListener
{
    void OnMove(Vector2 dir);
    void OnPadEvent(int eventType, TouchPhase touchPhase, Vector2 vec, float level);
    void ListenerLateUpdate();
}

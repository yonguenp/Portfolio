using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CanvasControl : MonoBehaviour
{
    public GameMain GameManager = null;

    public abstract void SetCanvasState(GameMain.HahahaState state);
}

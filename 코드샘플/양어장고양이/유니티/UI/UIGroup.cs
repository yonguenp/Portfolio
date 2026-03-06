using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIGroup : MonoBehaviour
{
    public FarmUIPanel FarmUIPanel;
    public GameObject[] UI;
    public abstract void SetUI(bool enable);
    public abstract void Refresh();
}

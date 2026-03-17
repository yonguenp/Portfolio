using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UseLocalDataScript : MonoBehaviour
{
    Color originColor;
    private void Awake()
    {
        originColor = GetComponent<Image>().color;
        Init();
    }

    private void Init()
    {
        GetComponent<Image>().color = (GameDataManager.Instance.UseLocalData()) ? Color.red : originColor;
    }

    public void OnUseLocalData()
    {
        GameDataManager.Instance.SetUseLocalData(!GameDataManager.Instance.UseLocalData());

        GetComponent<Image>().color = (GameDataManager.Instance.UseLocalData()) ? Color.red : originColor;
    }

}

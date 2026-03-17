using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIEscapeKeyScore : MonoBehaviour
{
    [SerializeField] Image[] key = null;
    [SerializeField] Image escape = null;

    public void AllScoreSetVisible(bool isVisible)
    {
        int count = key.Length;
        for (int i = 0; i < count; ++i)
        {
            key[i].gameObject.SetActive(isVisible);
        }

    }

    public void EscapeSetVisible(bool isVisible)
    {
        escape.gameObject.SetActive(isVisible);
    }

    void AllGray()
    {
        int count = key.Length;
        for (int i = 0; i < count; ++i)
        {
            key[i].color = Color.white;
        }
    }

    public void SetScore(int score)
    {
        AllGray();
        for (int i = 0; i < score; ++i)
        {
            key[i].color = Color.yellow;
        }
    }


}

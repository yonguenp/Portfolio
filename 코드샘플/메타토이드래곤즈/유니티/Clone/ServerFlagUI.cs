using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerFlagUI : MonoBehaviour
{
    [SerializeField]
    Image image;
    [SerializeField]
    Sprite Angel;
    [SerializeField]
    Sprite Wonder;
    [SerializeField]
    Sprite Luna;

    public void SetFlag(int tag)
    {
        if (tag <= 0)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        switch(tag)
        {
            case 1:
                image.sprite = Angel;
                break;
            case 2:
                image.sprite = Wonder;
                break;
            case 3:
                image.sprite = Luna;
                break;
            default:
                gameObject.SetActive(false);
                break;
        }
    }
}

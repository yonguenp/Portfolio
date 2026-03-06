using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer color;

    int cell = -1;
    int floor = -1;
    private void OnEnable()
    {
        //EventManager.AddListener(this);

        if (color != null)
        {
            color.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
            color.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        //EventManager.RemoveListener(this);
    }

    public void Init(int x, int y)
    {
        cell = x;
        floor = y;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextBackground : MonoBehaviour
{
    // Start is called before the first frame update
    private Text target;

    private void Awake()
    {
        target = GetComponentInChildren<Text>();
    }
    private void Update()
    {
        if (target != null)
        {
            Vector2 size = (transform as RectTransform).sizeDelta;
            size.x = (target.transform as RectTransform).sizeDelta.x + 20;

            (transform as RectTransform).sizeDelta = size;
        }        
    }
}

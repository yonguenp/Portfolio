using Coffee.UIEffects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldBossUISpine : MonoBehaviour
{
    [SerializeField]
    UIGradient gradient;
    [SerializeField]
    RectTransform rectTransform;

    const float time = 3.0f;
    float goal = 1.0f;
    private void Start()
    {
        SyncSize();
        Invoke("SyncSize", 0.1f);
    }

    void SyncSize()
    {
        var canvas = rectTransform.GetComponentInParent<Canvas>();
        if (canvas != null)
            rectTransform.sizeDelta = (canvas.transform as RectTransform).sizeDelta;
    }
    private void Update()
    {
        gradient.offset += (goal / time) * Time.deltaTime;
        if (goal > 0)
        {
            if (gradient.offset >= goal)
            {
                goal = -1;
                SyncSize();
            }
        }
        else
        {
            if (gradient.offset <= goal)
            {
                goal = 1;
                SyncSize();
            }
        }

    }
}

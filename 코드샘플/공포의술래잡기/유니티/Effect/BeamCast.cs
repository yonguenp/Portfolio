using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamCast : MonoBehaviour
{
    [SerializeField] float castTime = 1.0f;
    [SerializeField] float maxWidth = 1.0f;
    [SerializeField] LineRenderer[] lrArr = null;

    float offsetX = 0;
    float playTime = 0.0f;
    
    // Start is called before the first frame update
    void Start()
    {
        foreach (var lr in lrArr)
        {
            if (lr != null)
            {
                lr.startWidth = 0.0f;
                lr.endWidth = 0.0f;
            }
        }

        offsetX = 0;
        playTime = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        float delta = Time.deltaTime;
        foreach (var lr in lrArr)
        {
            if (lr != null)
            {
                lr.material.mainTextureOffset = new Vector2(offsetX, 0);

                if (playTime < castTime)
                {
                    lr.startWidth = maxWidth * (playTime / castTime);
                    lr.endWidth = maxWidth * (playTime / castTime);
                }
                else
                {
                    lr.startWidth = maxWidth;
                    lr.endWidth = maxWidth;
                }
            }
        }

        playTime += delta;
        offsetX = playTime * -1.0f;
    }
}

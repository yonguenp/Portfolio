using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamOffsetTrigger : MonoBehaviour
{
    [SerializeField] float castTime = 0.5f;
    [SerializeField] LineRenderer[] lrArr = null;

    float offsetX = -1.0f;
    float playTime = 0.0f;

    List<Vector2> lineWidths = new List<Vector2>();
    List<Gradient> lineGradients = new List<Gradient>();

    // Start is called before the first frame update
    void Start()
    {
        foreach (var lr in lrArr)
        {
            if (lr != null)
            {
                Vector2 oriWidth = new Vector2(lr.startWidth, lr.endWidth);
                Gradient oriColor = lr.colorGradient;

                lineWidths.Add(oriWidth);
            }
        }

        offsetX = -1.0f;
        playTime = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        float delta = Time.deltaTime;
        foreach(var lr in lrArr)
        {
            if (lr != null)
            {
                lr.material.mainTextureOffset = new Vector2(offsetX, 0);
            }

            if (playTime < castTime)
            {
                if (lr.colorGradient.alphaKeys.Length > 1)
                {
                    lr.colorGradient.alphaKeys[lr.colorGradient.alphaKeys.Length - 1].alpha = (playTime / castTime);
                }
            }
            else
            {
                if (lr.startWidth > 0.0f)
                    lr.startWidth -= delta;
                else
                    lr.startWidth = 0.0f;

                if (lr.endWidth > 0.0f)
                    lr.endWidth -= delta;
                else
                    lr.endWidth = 0.0f;
            }
        }

        playTime += delta;
        offsetX = -1.0f - (playTime);
    }
}

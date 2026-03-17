using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeText : Popup
{
    public float LimitTime { get; set; } = 0;
    public Vector2 MoveDir { get; set; } = Vector2.zero; //초당 몇움직일지 
    public bool IsFade { get; set; } = false;
    public float Delay { get; set; } = 0;       //움직임 페이드 딜레이

    bool _isPlay = false;
    float _playTime = 0;

    [SerializeField]
    Text Text;

    public void SetText(string text)
    {
        Text.text = text;

        MoveDir = new Vector2(0, 0.5f);
        LimitTime = 2;
        IsFade = true;
        Delay = 0.5f;
        Text.color = Color.white;

        Play();
    }

    public void SetText(string text, Color color)
    {
        SetText(text);
        Text.color = color;
    }

    public void Play()
    {
        this.gameObject.SetActive(true);
        _playTime = 0;
        _isPlay = true;
        transform.localPosition = Vector3.zero;

        foreach (MaskableGraphic graphic in GetComponentsInChildren<MaskableGraphic>())
        {
            var graphicColor = graphic.color;
            graphicColor.a = 1.0f;
            graphic.color = graphicColor;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_isPlay == false) return;
        if (MoveDir == Vector2.zero) return;

        if (LimitTime < _playTime)
        {
            //Destroy(this.gameObject);
            this.gameObject.SetActive(false);
            Close();
            return;
        }

        float delta = Time.deltaTime;

        float actionTime = _playTime - Delay;
        if (actionTime > 0)
        {
            var nextPos = new Vector3(MoveDir.x * delta, MoveDir.y * delta) + transform.position;
            transform.position = nextPos;
            if (IsFade)
            {
                float LimitFadeTime = LimitTime - Delay;
                float alpha = 1.0f - actionTime / LimitFadeTime;
                foreach (MaskableGraphic graphic in GetComponentsInChildren<MaskableGraphic>())
                {
                    var graphicColor = graphic.color;                    
                    graphicColor.a = alpha;
                    graphic.color = graphicColor;
                }
            }
        }

        _playTime += delta;
    }
}

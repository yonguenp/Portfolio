using Coffee.UIEffects;
using DG.Tweening;
using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DragonEmotion : MonoBehaviour
{
    public enum Emotion
    { 
        None = -1,

        RANDOM_MIN = 0,

        HEART = 0,
        STAR = 1,
        WATERDROP = 2,
        LIGHT = 3,
        QUESTION = 4,
        SLEEP = 5,
        GOOD = 6,
        BAD = 7,
        HAPPY = 8,
        SAD = 9,
        SURPRISE = 10,
        OK = 11,

        RANDOM_MAX,

        RANDOM
    }

    public enum EmotionColor
    {
        WHITE = 0,

        RANDOM_MIN = 1,

        //rainbow
        RED = 1,
        ORANGE,
        YELLOW,
        GREEN,
        BLUE,
        NAVY,
        PURPLE,

        //additional
        PINK,

        RANDOM_MAX,

        RANDOM
    }

    [SerializeField]
    Sprite[] emotionIcons;
    [SerializeField]
    SpriteRenderer box;
    [SerializeField]
    SpriteRenderer icon;
    [SerializeField]
    SpriteRenderer[] outlines;
    [SerializeField]
    Transform icons;

    Tween tween = null;
    Emotion cur = Emotion.None;
    
    public void SetOrder(int order)
    {
        box.sortingOrder = order;
        icon.sortingOrder = order;
        foreach (var outline in outlines)
        {
            outline.sortingOrder = order;
        }
    }

    private void OnDisable()
    {
        Clear();
    }

    public void Clear()
    {
        cur = Emotion.None;

        if (tween != null)
            tween.Kill();
        tween = null;

        SetColor(EmotionColor.WHITE);

        gameObject.SetActive(false);

        icon.color = Color.white;
        icon.transform.localScale = Vector3.one;
        foreach (var outline in outlines)
        {
            outline.color = Color.black;
            outline.transform.localScale = Vector3.one;
        }
    }

    public void SetEmotion(Emotion emotion, EmotionColor color)
    {
        Clear();

        SetColor(color);

        if (PopupManager.IsPopupOpening())
            return;

        if (emotion == Emotion.None)
        {
            return;
        }
        
        if (emotion == Emotion.RANDOM)
        {
            emotion = (Emotion)SBFunc.Random((int)Emotion.RANDOM_MIN, (int)Emotion.RANDOM_MAX);
        }

        icon.sprite = emotionIcons[(int)emotion];
        foreach (var outline in outlines)
        {
            outline.sprite = emotionIcons[(int)emotion];
        }

        if (icon.sprite != null)
        {
            cur = emotion;

            gameObject.SetActive(true);

            var startPos = Vector3.up * 0.4f;
            var startScale = Vector3.zero;
            transform.localPosition = startPos;
            transform.localScale = startScale;

            Sequence seq = DOTween.Sequence();
            seq.Append(transform.DOLocalMoveY(0.6f, 0.25f));
            seq.Join(transform.DOScale(1.0f, 0.15f));

            switch (Random.Range(0, 3))
            {
                case 0:
                    Color clr = icon.color;
                    clr.a = 0.0f;
                    icon.color = clr;
                    foreach (var outline in outlines)
                    {
                        outline.color = clr;
                    }

                    clr.a = 1.0f;
                    seq.Append(icon.DOColor(clr, 0.2f));
                    foreach (var outline in outlines)
                    {
                        seq.Join(outline.DOColor(Color.black, 0.2f));
                    }
                    break;
                case 1:
                    icon.transform.localScale = new Vector3(0f, 1f, 1f);
                    foreach (var outline in outlines)
                    {
                        outline.transform.localScale = new Vector3(0f, 1f, 1f);
                    }
                    seq.Append(icon.transform.DOScaleX(1.0f, 0.2f));
                    foreach (var outline in outlines)
                    {
                        seq.Join(outline.transform.DOScaleX(1.0f, 0.2f));
                    }
                    break;
                default:
                    icon.transform.localScale = new Vector3(1f, 0f, 1f);
                    foreach (var outline in outlines)
                    {
                        outline.transform.localScale = new Vector3(1f, 0f, 1f);
                    }
                    seq.Append(icon.transform.DOScaleY(1.0f, 0.2f));
                    foreach (var outline in outlines)
                    {
                        seq.Join(outline.transform.DOScaleY(1.0f, 0.2f));
                    }
                    break;
            }

            seq.AppendInterval(1.5f);

            seq.Append(transform.DOLocalMoveY(startPos.y, 0.25f));
            seq.Join(transform.DOScale(startScale, 0.15f));

            seq.AppendCallback(Clear);

            tween = seq;
        }
        else
        {
            Clear();
        }
    }

    public void SetColor(EmotionColor color)
    {
        Color c = Color.white;
        Color o = Color.black;

        switch(color)
        {
            case EmotionColor.WHITE:
                c = Color.white;
                o = Color.black;
                break;
            case EmotionColor.RED:
                ColorUtility.TryParseHtmlString("#f60000", out c);
                ColorUtility.TryParseHtmlString("#530000", out o);
                break;                                         
            case EmotionColor.ORANGE:                          
                ColorUtility.TryParseHtmlString("#fe7800", out c);
                ColorUtility.TryParseHtmlString("#5d2c00", out o);
                break;                                         
            case EmotionColor.YELLOW:                          
                ColorUtility.TryParseHtmlString("#ffcf25", out c);
                ColorUtility.TryParseHtmlString("#9c5800", out o);
                break;                                         
            case EmotionColor.GREEN:                           
                ColorUtility.TryParseHtmlString("#abff11", out c);
                ColorUtility.TryParseHtmlString("#286300", out o);
                break;                                         
            case EmotionColor.BLUE:                            
                ColorUtility.TryParseHtmlString("#0bffe8", out c);
                ColorUtility.TryParseHtmlString("#006083", out o);
                break;                                         
            case EmotionColor.NAVY:                            
                ColorUtility.TryParseHtmlString("#0b93ff", out c);
                ColorUtility.TryParseHtmlString("#00309e", out o);
                break;                                         
            case EmotionColor.PURPLE:                          
                ColorUtility.TryParseHtmlString("#c100ff", out c);
                ColorUtility.TryParseHtmlString("#4e0067", out o);
                break;                                         
            case EmotionColor.PINK:                            
                ColorUtility.TryParseHtmlString("#ff61ca", out c);
                ColorUtility.TryParseHtmlString("#7f0078", out o);
                break;
            case EmotionColor.RANDOM:
                SetColor((EmotionColor)SBFunc.Random((int)EmotionColor.RANDOM_MIN, (int)EmotionColor.RANDOM_MAX));
                return;
        }
        icon.color = c;
    }

    private void Update()
    {
        if (!gameObject.activeInHierarchy)
            return;

        icons.transform.localScale = transform.parent.localScale;
    }
}

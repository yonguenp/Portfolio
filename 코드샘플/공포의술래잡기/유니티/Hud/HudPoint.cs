using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class HudPoint : MonoBehaviour
{
    [SerializeField] Text pointText;
    
    CharacterObject character = null;
    RectTransform canvas = null;

    public void InitPoint(CharacterObject character, RectTransform canvas, CharacterHud parent, int point)
    {
        this.character = character;
        this.canvas = canvas;

        parent.SetChild(character, transform);
        transform.localScale = Vector3.one;

        pointText.text = "+" + point.ToString() + "P";

        pointText.transform.DOLocalMoveY(150f, 1.0f).OnComplete(()=> {
            Destroy(gameObject);
        }).SetEase(Ease.InCirc);
        Color toColor = pointText.color;
        toColor.a = 0.0f;
        pointText.DOColor(toColor, 1.0f);
    }

    private void Update()
    {
        if (character != null)
        {
            SetPosition();
        }
    }

    private void SetPosition()
    {
        Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(character.transform.position);
        Vector2 WorldObject_ScreenPosition = new Vector2((ViewportPosition.x - 0.5f) * canvas.rect.width, (ViewportPosition.y - 0.5f) * canvas.rect.height);

        transform.localPosition = WorldObject_ScreenPosition + new Vector2(0, 220 * (6.5f / Camera.main.orthographicSize));
    }
}

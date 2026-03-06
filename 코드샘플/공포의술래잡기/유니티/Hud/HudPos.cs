using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HudPos : MonoBehaviour
{
    const float HIDE_ANIM_TIME = 0.5f;
    protected GameObject character = null;    
    protected RectTransform canvas = null;
    Camera renderCamera = null;
    public float AddY { get; private set; } = 0;
    public virtual void Init(GameObject character, Camera rc, RectTransform canvas, Transform parent, float playTime = 0)
    {
        this.character = character;
        renderCamera = rc;
        this.canvas = canvas;

        this.transform.SetParent(parent);
        transform.localPosition = new Vector3(0, 0, 0);
        transform.localScale = new Vector3(1, 1, 1);

        if (playTime != 0)
        {
            Destroy(this.gameObject, playTime);

            if (playTime > 1.0f)
                Invoke("Hide", playTime - HIDE_ANIM_TIME);
            else
                Hide();
        }
    }

    public void SetAddPosY(float offset)
    {
        AddY = offset;
    }

    public void Hide()
    {
        CancelInvoke("Hide");

        foreach (MaskableGraphic graphic in GetComponentsInChildren<MaskableGraphic>())
        {
            Color color = graphic.color;
            color.a = 0;
            graphic.DOColor(color, HIDE_ANIM_TIME).SetEase(Ease.InCirc);
        }
    }

    private void LateUpdate()
    {
        Refresh();
    }

    protected virtual void Refresh()
    {
        if (character != null)
        {
            var pos = character.transform.position + new Vector3(0, AddY, 0);

            Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(pos);
            if (ViewportPosition.x < 0.1f)
                ViewportPosition.x = 0.1f;
            if (ViewportPosition.x >= 0.9f)
                ViewportPosition.x = 0.9f;
            if (ViewportPosition.y < 0.1f)
                ViewportPosition.y = 0.1f;
            if (ViewportPosition.y >= 0.9f)
                ViewportPosition.y = 0.9f;

            Vector2 WorldObject_ScreenPosition = new Vector2((ViewportPosition.x - 0.5f) * canvas.rect.width, (ViewportPosition.y - 0.5f) * canvas.rect.height) -
            new Vector2(canvas.rect.width, canvas.rect.height) / 2;

            transform.localPosition = WorldObject_ScreenPosition;

            SBDebug.Log($"hudPos pos{pos}, ViewportPosition {ViewportPosition}, WorldObject_ScreenPosition {WorldObject_ScreenPosition}");
        }
    }
}

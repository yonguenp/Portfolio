using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HudDetect : HudPos
{
    [SerializeField]
    RectTransform pivot;
    [SerializeField]
    RectTransform cursor;

    float screenOffsetMin = 0.1f;
    float screenOffsetMax = 0.9f;
    CharacterObject characterObject = null;
    public void Init(CharacterObject cobj, Camera rc, RectTransform canvas, Transform parent, float playTime = 0)
    {
        Init(cobj.gameObject, rc, canvas, parent, playTime);
        characterObject = cobj;
    }
    public override void Init(GameObject character, Camera rc, RectTransform canvas, Transform parent, float playTime = 0)
    {
        base.Init(character, rc, canvas, parent, playTime);
        pivot.localPosition = new Vector2(canvas.rect.width, canvas.rect.height) / 2;

        screenOffsetMin = 100.0f / canvas.rect.width;
        screenOffsetMax = 1.0f - screenOffsetMin;
    }

    protected override void Refresh()
    {
        if (character != null)
        {
            if(characterObject != null)
            {
                if(!characterObject.IsPlaying)
                {
                    Destroy(this.gameObject);
                }
            }

            var pos = character.transform.position + new Vector3(0, AddY, 0);

            Vector3 angle = Vector3.zero;
            Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(pos);
            if (ViewportPosition.x < screenOffsetMin)
            {
                ViewportPosition.x = screenOffsetMin;                

                if (ViewportPosition.y < screenOffsetMin)
                {
                    ViewportPosition.y = screenOffsetMin;
                    angle = new Vector3(0.0f, 0.0f, -45.0f);
                }
                else if (ViewportPosition.y >= screenOffsetMax)
                {
                    ViewportPosition.y = screenOffsetMax;
                    angle = new Vector3(0.0f, 0.0f, -135.0f);
                }
                else
                {
                    angle = new Vector3(0.0f, 0.0f, -90.0f);
                }
            }
            else if (ViewportPosition.x >= screenOffsetMax)
            {
                ViewportPosition.x = screenOffsetMax;
                
                if (ViewportPosition.y < screenOffsetMin)
                {
                    ViewportPosition.y = screenOffsetMin;
                    angle = new Vector3(0.0f, 0.0f, 45.0f);
                }
                else if (ViewportPosition.y >= screenOffsetMax)
                {
                    ViewportPosition.y = screenOffsetMax;
                    angle = new Vector3(0.0f, 0.0f, 135.0f);
                }
                else
                {
                    angle = new Vector3(0.0f, 0.0f, 90.0f);
                }
            }
            else
            {
                if (ViewportPosition.y < screenOffsetMin)
                {
                    ViewportPosition.y = screenOffsetMin;
                    angle = new Vector3(0.0f, 0.0f, 0.0f);
                }
                else if (ViewportPosition.y >= screenOffsetMax)
                {
                    ViewportPosition.y = screenOffsetMax;
                    angle = new Vector3(0.0f, 0.0f, 180.0f);
                }
            }

            cursor.eulerAngles = angle;

            Vector2 WorldObject_ScreenPosition = new Vector2((ViewportPosition.x - 0.5f) * canvas.rect.width, (ViewportPosition.y - 0.5f) * canvas.rect.height) -
            new Vector2(canvas.rect.width, canvas.rect.height) / 2;
            
            transform.localPosition = WorldObject_ScreenPosition;
            
            SBDebug.Log($"hudPos pos{pos}, ViewportPosition {ViewportPosition}, WorldObject_ScreenPosition {WorldObject_ScreenPosition}");
        }
    }
}

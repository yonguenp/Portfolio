using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HudVehicleBar : MonoBehaviour
{
    [SerializeField] GameObject bar;
    [SerializeField] Image barImage;

    CharacterObject character = null;
    Camera renderCamera = null;
    RectTransform canvas = null;
    float currentRemainTime;
    float totalTime;
    Color fullColor = new Color(251f / 255f, 235f / 255f, 68f / 255f);
    Color halfColor = new Color(247f / 255f, 131f / 255f, 2f / 255f);
    Color zeroColor = new Color(215f / 255f, 11f / 255f, 44f / 255f);

    private void SetVisible(bool value)
    {
        if (bar == null) return;
        if (bar.activeInHierarchy == value) return;
        bar.SetActive(value);
    }

    public void Init(CharacterObject character, Camera rc, RectTransform canvas, CharacterHud parent)
    {
        this.character = character;
        renderCamera = rc;
        this.canvas = canvas;

        parent.SetChild(character, transform);
        this.transform.localScale = Vector3.one;

        SetVisible(false);
    }

    public void ShowVehicleBar(float time)
    {
        if (time <= 0f)
            SetVisible(false);
        else
        {
            totalTime = currentRemainTime = time;
            barImage.color = fullColor;
            barImage.DOColor(halfColor, time / 2f).OnComplete(() =>
                {
                    barImage.DOColor(zeroColor, time / 2f);
                });
            SetVisible(true);
        }
    }

    private void SetPosition()
    {
        Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(character.transform.position);
        Vector2 WorldObject_ScreenPosition = new Vector2((ViewportPosition.x - 0.5f) * canvas.rect.width, (ViewportPosition.y - 0.5f) * canvas.rect.height);

        transform.localPosition = WorldObject_ScreenPosition + new Vector2(0, 255);
    }

    private void Update()
    {
        if (currentRemainTime > 0)
        {
            currentRemainTime -= Time.deltaTime;
            var ratio = currentRemainTime / totalTime;
            barImage.fillAmount = ratio;
            SetPosition();
        }
        else
        {
            currentRemainTime = 0f;
            SetVisible(false);
        }
    }
}
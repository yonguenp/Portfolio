using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HudHp : MonoBehaviour
{
    [SerializeField] GameObject bar;
    [SerializeField] Image barImage;
    int curHp = 0;
    int maxHp = 0;

    CharacterObject character = null;
    Camera renderCamera = null;
    RectTransform canvas = null;
    Tweener fillTweener;

    public void InitHp(CharacterObject character, Camera rc, RectTransform canvas, CharacterHud parent, int maxHp)
    {
        this.character = character;
        renderCamera = rc;
        this.canvas = canvas;

        parent.SetChild(character, transform);
        transform.localScale = Vector3.one;

        this.maxHp = maxHp;

        curHp = maxHp;
        barImage.fillAmount = 1f;

        gameObject.SetActive(true);
        SetPosition();
    }

    public void SetHp(ushort hp)
    {
        curHp = hp;
        if (fillTweener != null)
        {
            fillTweener.Kill();
            fillTweener = null;
        }

        var fillAmountValue = Mathf.Max((float)curHp / maxHp, 0.01f);
        
        if (barImage.gameObject.activeInHierarchy)
            fillTweener = barImage.DOFillAmount(fillAmountValue, 0.1f);
        else
            barImage.fillAmount = fillAmountValue;
    }

    private void OnEnable()
    {
        try { SetPosition(); }
        catch { }
    }

    private void SetPosition()
    {
        Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(character.transform.position);
        Vector2 WorldObject_ScreenPosition = new Vector2((ViewportPosition.x - 0.5f) * canvas.rect.width, (ViewportPosition.y - 0.5f) * canvas.rect.height);

        transform.localPosition = WorldObject_ScreenPosition + new Vector2(0, -25 * (6.5f / Camera.main.orthographicSize));
    }

    private void Update()
    {
        if (Game.Instance.PlayerController.Character.IsSightBlocked)
        {
            gameObject.SetActive(false);
            return;
        }
        if (character != null)
        {
            // SetVisible(true);
            SetPosition();
        }
    }
}

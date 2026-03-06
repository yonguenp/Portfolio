using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HudEmotion : MonoBehaviour
{
    [SerializeField] Image icon;

    CharacterObject character = null;
    Camera renderCamera = null;
    RectTransform canvas = null;


    public void InitEmotion(CharacterObject character, Camera rc, RectTransform canvas, CharacterHud parent)
    {
        this.character = character;
        renderCamera = rc;
        this.canvas = canvas;

        parent.SetChild(character, transform);
        transform.localScale = Vector3.zero;

        gameObject.SetActive(false);
        SetPosition();
    }
    private void OnEnable()
    {
        try
        {
            SetPosition();
        }
        catch { }
    }

    public void OnAnimation()
    {
        this.DOKill();

        Sequence sequence = DOTween.Sequence();

        //sequence.Append(this.gameObject.transform.DOScale(Vector3.zero, 0));
        sequence.Append(this.gameObject.transform.DOScale(new Vector3(1.5f, 1.5f, 1.5f), 0.01f).SetEase(Ease.OutQuad));
        sequence.Append(this.gameObject.transform.DOScale(Vector3.one, 0.005f));

        int angle = 30;
        for (int i = 0; i < 3; i++)
        {
            sequence.Append(this.gameObject.transform.DORotate(new Vector3(0, 0, angle), 0.1f)).SetEase(Ease.OutQuad);
            sequence.Append(this.gameObject.transform.DORotate(new Vector3(0, 0, 0), 0.1f)).SetEase(Ease.InQuad);
            sequence.Append(this.gameObject.transform.DORotate(new Vector3(0, 0, -1 * angle), 0.1f)).SetEase(Ease.OutQuad);
            sequence.Append(this.gameObject.transform.DORotate(new Vector3(0, 0, 0), 0.1f)).SetEase(Ease.InQuad);

            angle -= 10;
        }
    }
    private void SetPosition()
    {
        //Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(character.transform.position);
        //Vector2 WorldObject_ScreenPosition = new Vector2((ViewportPosition.x - 0.5f) * canvas.rect.width, (ViewportPosition.y - 0.5f) * canvas.rect.height);

        //transform.localPosition = WorldObject_ScreenPosition + new Vector2(0, 250 * (6.5f / Camera.main.orthographicSize));

        Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(character.transform.position);
        Vector2 WorldObject_ScreenPosition = new Vector2((ViewportPosition.x - 0.5f) * canvas.rect.width, (ViewportPosition.y - 0.5f) * canvas.rect.height);

        var resolutionRatio = 1f;
        var ratio = (float)Screen.width / Screen.height;
        if (ratio < (16f / 9f))
            resolutionRatio = (16f / 9f) / ratio;
        transform.localPosition = WorldObject_ScreenPosition + new Vector2(0, 250 * (6.5f / Camera.main.orthographicSize) * resolutionRatio);

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

    public void ShowEmotion(ushort type)
    {
        EmoticonItemData emoticonData = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.emoticon, (int)type) as EmoticonItemData;
        if (emoticonData == null)
            return;

        icon.sprite = emoticonData.sprite;

        gameObject.SetActive(true);
        OnAnimation();
        StartCoroutine(StartEmotion(2));
    }
    IEnumerator StartEmotion(int time)
    {
        float timeSeconds = time * 1f;
        float lastTime = 0;

        while (timeSeconds - lastTime > 0)
        {
            lastTime += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }
        this.gameObject.transform.DOScale(Vector3.zero, 0);
        this.gameObject.SetActive(false);
    }

    public string GetName()
    {
        return icon.sprite.name;
    }
    public void SetIcon(Sprite sprite)
    {
        icon.sprite = sprite;
    }
}

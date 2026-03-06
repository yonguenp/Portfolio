using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class HudBattery : MonoBehaviour
{
    [SerializeField] GameObject bar;
    [SerializeField] Image barImage;
    [SerializeField] Text batteryCntText;
    [SerializeField] HorizontalLayoutGroup horizontalLayout;
    [SerializeField] GameObject fillRectSample;
    int curBattery = 0;
    int maxBattery = 0;

    CharacterObject character = null;
    Camera renderCamera = null;
    RectTransform canvas = null;
    Tweener fillTweener;

    List<GameObject> fillrectLists = new List<GameObject>();
    public void InitBattery(CharacterObject character, Camera rc, RectTransform canvas, CharacterHud parent, int maxBattery)
    {
        this.character = character;
        renderCamera = rc;
        this.canvas = canvas;

        parent.SetChild(character, transform);
        transform.localScale = Vector3.one;

        this.maxBattery = maxBattery;
        batteryCntText.text = string.Empty;

        curBattery = 0;
        barImage.fillAmount = 0;

        CreateFillRect();

        gameObject.SetActive(true);

        SetBattery(0);
        SetPosition();
    }
    public void CreateFillRect()
    {
        if (fillRectSample == null)
        {
            SBDebug.LogError("프리팹 빠져있음");
            return;
        }

        foreach (var item in fillrectLists)
        {
            Destroy(item);
        }
        fillrectLists.Clear();

        horizontalLayout.childControlWidth = true;
        horizontalLayout.childForceExpandWidth = true;

        for (int i = 0; i < this.maxBattery; i++)
        {
            var obj = GameObject.Instantiate(fillRectSample, horizontalLayout.transform);
            obj.SetActive(true);
            fillrectLists.Add(obj);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(horizontalLayout.GetComponent<RectTransform>());
    }

    public void SetBattery(int cnt)
    {
        horizontalLayout.childControlWidth = false;
        horizontalLayout.childForceExpandWidth = false;

        for (int i = 0; i < fillrectLists.Count; i++)
        {
            fillrectLists[i].SetActive(false);
        }

        curBattery = cnt;
        //if (fillTweener != null)
        //{
        //    fillTweener.Kill();
        //    fillTweener = null;
        //}

        //var fillAmountValue = (float)curBattery / maxBattery;
        //if (barImage.gameObject.activeInHierarchy)
        //    fillTweener = barImage.DOFillAmount(fillAmountValue, 0.1f);
        //else
        //    barImage.fillAmount = fillAmountValue;

        for (int i = 0; i < cnt; i++)
        {
            fillrectLists[i].SetActive(true);
        }
    }

    public int GetMaxBattery()
    {
        return maxBattery;
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

        transform.localPosition = WorldObject_ScreenPosition + new Vector2(0, -52 * (6.5f / Camera.main.orthographicSize));
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

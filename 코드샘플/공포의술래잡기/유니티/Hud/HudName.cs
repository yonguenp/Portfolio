using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HudName : MonoBehaviour
{
    [SerializeField] Image backPanel;
    [SerializeField] Text nameText;
    
    CharacterObject character = null;
    Camera renderCamera = null;
    RectTransform canvas = null;
    public void InitName(CharacterObject character, Camera rc, RectTransform canvas, CharacterHud parent, string name, bool chaser, int index)
    {
        this.character = character;
        renderCamera = rc;
        this.canvas = canvas;

        parent.SetChild(character, transform);
        transform.localScale = Vector3.one;

        nameText.text = name;
        if (chaser)
        {
            nameText.color = SHelper.TextColor[(int)SHelper.TEXT_TYPE.CHASER_1 + index];
        }
        else
        {
            nameText.color = SHelper.TextColor[(int)SHelper.TEXT_TYPE.SURVIVOR_1 + index];
        }

        if(backPanel != null)
        {
            backPanel.gameObject.SetActive(false);
            //backPanel.gameObject.SetActive(true);
            //(backPanel.transform as RectTransform).sizeDelta = new Vector2(nameText.preferredWidth + 30.0f, (backPanel.transform as RectTransform).sizeDelta.y);
            //Color color = Color.white - nameText.color;
            //color.a = 1.0f;
            //backPanel.color = color;
        }

        gameObject.SetActive(true);
        SetPosition();
    }

    public void SetName(string name)
    {
        nameText.text = name;
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

        var resolutionRatio = 1f;
        var ratio = (float)Screen.width / Screen.height;
        if (ratio < (16f / 9f))
            resolutionRatio = (16f / 9f) / ratio;
        transform.localPosition = WorldObject_ScreenPosition + new Vector2(0, 180 * (6.5f / Camera.main.orthographicSize) * resolutionRatio);
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

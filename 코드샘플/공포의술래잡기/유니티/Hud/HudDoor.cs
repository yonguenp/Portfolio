using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HudDoor : MonoBehaviour
{
    [SerializeField]
    GameObject _targetObject = null;
    [SerializeField]
    Camera _renderCamera = null;
    [SerializeField]
    RectTransform _canvas = null;

    Image _guide = null;
    Image _icon = null;

    public GameObject Create(Vector2 pos)
    {
        var go = transform.Find("guide");
        _guide = go.GetComponent<Image>();

        go = transform.Find("icon");
        _icon = go.GetComponent<Image>();

        go.transform.localScale = new Vector3(1, 1, 1);

        return go.gameObject;
    }

    public void Init(GameObject targetObject, Camera rc, RectTransform canvas, Transform parent, Vector2 pos)
    {
        this._targetObject = targetObject;
        _renderCamera = rc;
        this._canvas = canvas;

        this.transform.parent = parent;
        transform.localPosition = new Vector3(0, 0, 0);
        transform.localScale = new Vector3(1, 1, 1);
        Create(pos);
    }

    private void LateUpdate()
    {


        if (_targetObject != null)
        {
            Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(_targetObject.transform.position);

            if (ViewportPosition.x < 0)
                ViewportPosition.x = 0.1f;
            if (ViewportPosition.x >= 1)
                ViewportPosition.x = 0.9f;
            if (ViewportPosition.y < 0)
                ViewportPosition.y = 0.1f;
            if (ViewportPosition.y >= 1)
                ViewportPosition.y = 0.9f;

            Vector2 WorldObject_ScreenPosition = new Vector2((ViewportPosition.x - 0.5f) * _canvas.rect.width, (ViewportPosition.y - 0.5f) * _canvas.rect.height);

            transform.localPosition = WorldObject_ScreenPosition;
            var subPos = transform.localPosition - _targetObject.transform.position;
            var angle = Mathf.Atan2(subPos.y, subPos.x) * Mathf.Rad2Deg;
            transform.eulerAngles = new Vector3(0, 0, angle);
        }
    }

}

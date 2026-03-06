using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class EffectButtonSFX : MonoBehaviour
{
    [SerializeField] Image Image;

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        this.gameObject.AddComponent<Mask>();
        var item = CreateObject();
        Image = item;

        StartCoroutine(Animation());
    }

    IEnumerator Animation()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            this.transform.DOPunchScale(Vector3.one * 0.2f, 0.5f, 1).OnComplete(() => { this.transform.DORewind(); });
            Image.DOColor(new Vector4(1f, 1f, 1f, 0.7f), 0.25f).OnComplete(() => { Image.DOColor(new Vector4(1f, 1f, 1f, 0f), 0.25f); });

        }
    }
    public Image CreateObject()
    {
        GameObject obj = new GameObject("reFlextImage");
        obj.transform.SetParent(this.transform);
        obj.AddComponent<RectTransform>();

        obj.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        obj.GetComponent<RectTransform>().anchorMax = Vector2.one;

        obj.transform.localPosition = Vector2.zero;
        obj.transform.localScale = Vector2.one;


        obj.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        obj.GetComponent<RectTransform>().offsetMax= Vector2.zero;


        obj.AddComponent<Image>();
        obj.GetComponent<Image>().DOColor(new Vector4(1f, 1f, 1f, 0f), 0f);

        return obj.GetComponent<Image>();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System;


public class EffectSFX : MonoBehaviour
{
    [SerializeField] int maxCount = 5;
    [SerializeField] GameObject sampleObject;
    [SerializeField] Transform targetPosition;

    Action action = null;
    List<GameObject> list = new List<GameObject>();
    int assetCount;

    [SerializeField] RectTransform[] posAry = new RectTransform[] { null, };

    public void Init(ShopPackageGameData reward, Action cb = null)
    {
        if (targetPosition == null)
            return;

        sampleObject.GetComponent<Image>().sprite = reward.GetIcon();
        assetCount = reward.goods_amount > maxCount ? maxCount : reward.goods_amount;
        action = cb;

        if (gameObject.activeSelf)
            StartCoroutine(CreateSample());
    }

    IEnumerator CreateSample()
    {
        if (sampleObject != null)
            sampleObject.SetActive(false);
        var radious = this.GetComponent<RectTransform>().rect.width;
        int randomAngle = 0;

        var pos = Vector2.zero;

        for (int i = 0; i < assetCount; i++)
        {
            GameObject obj = GameObject.Instantiate(sampleObject, this.transform);
            obj.transform.localScale = Vector3.one;

            //randomAngle = 30 * UnityEngine.Random.Range(0, 7);  // 60~300

            //pos.x = radious * Mathf.Cos(-randomAngle * Mathf.Deg2Rad);
            //pos.y = radious * Mathf.Sin(-randomAngle * Mathf.Deg2Rad);

            obj.GetComponent<RectTransform>().DOAnchorPos(posAry[i].anchoredPosition, 0f);
            yield return null;

            obj.SetActive(true);
            obj.GetComponent<RectTransform>().DOAnchorPos(new Vector2(posAry[i].anchoredPosition.x, posAry[i].anchoredPosition.y + 20f), 0.2f).SetLoops(-1, LoopType.Yoyo);
            list.Add(obj);
            yield return new WaitForSeconds(0.3f);
        }

        foreach (var item in list)
        {
            item.transform.DOJump(targetPosition.position, 1f, 1, 0.5f).OnComplete(() =>
            {
                action?.Invoke();
                action = null;

                item.transform.DOKill();

                Clear();
            });

        }

        yield return null;
    }

    public void Clear()
    {
        Destroy(this.gameObject);
    }
}



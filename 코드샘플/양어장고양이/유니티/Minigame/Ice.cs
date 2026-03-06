using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Ice : MonoBehaviour
{
    public int x;
    public int y;
    Minigame minigame;
    bool bLive = false;

    public void Init(int _x, int _y, Minigame parent)
    {
        minigame = parent;
        x = _x;
        y = _y;
        bLive = true;

        Button btn = GetComponent<Button>();
        if (btn == null)
            btn = gameObject.AddComponent<Button>();

        btn.onClick.AddListener(OnTouched);
    }

    public void OnTouched()
    {
        if (!bLive)
            return;

        minigame.OnTouched(x, y, Random.Range(0.1f, 0.3f));
    }

    public void OnPowered(float power)
    {        
        Color target = GetComponent<Image>().color;
        target.a -= power;
        if (GetComponent<Image>().color.a < 0.3f)
        {
            GetComponent<Image>().color = new Color(0, 0, 0, 0);
            bLive = false;
        }
        else
        {
            GetComponent<Image>().color = target;
        }

        transform.DOScale(Vector3.one * (1.0f - power), 0.1f).OnComplete(()=> { transform.localScale = Vector3.one; });
    }

    public bool IsLive()
    {
        return bLive;
    }
}

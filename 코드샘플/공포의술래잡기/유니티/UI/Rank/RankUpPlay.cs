using Coffee.UIExtensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankUpPlay : MonoBehaviour
{
    [Range(0, 1)]
    public float white = 0;
    public Image curRank = null;
    
    //private
    Material matRank = null;

    public Sprite nextRankSprite = null;
    public UIParticle rankup;

    // Start is called before the first frame update
    void Start()
    {
        if(curRank == null)
            SBDebug.Log("curRank is null");

        curRank.color = Color.white;   
        matRank = curRank.material;

        white = 0;
        SetWhite(white);
    }

    public void SetRankSprite(Sprite sprite)
    {
        curRank.sprite = sprite;
    }

    // Update is called once per frame
    void SetWhite(float value)
    {
        matRank.SetFloat("_White", value);
    }

    public void PlayChangeWhite()
    {
        SBDebug.Log("PlayChangeWhite");
        StartCoroutine(PlayChangeWhiteCO());
    }
    
    IEnumerator PlayChangeWhiteCO()
    {
        var playValue = 0.0f;
        while(true)
        {
            playValue += Time.deltaTime * 1.0f / 0.15f;
            if(playValue >= 1)
                break;

            SetWhite(playValue);
            yield return null;
        }

        SetWhite(1);
    }

    public void PlayChangeNormal()
    {
        curRank.sprite = nextRankSprite;
        SBDebug.Log("PlayChangeNormal");
        StartCoroutine(PlayChangeNormalCO());
    }
    
    IEnumerator PlayChangeNormalCO()
    {
        var playValue = 0.0f;
        while(true)
        {
            playValue += Time.deltaTime * 1.0f / 0.15f;
            if(playValue >= 1)
                break;

            SetWhite(1 - playValue);
            yield return null;
        }

        SetWhite(0);
    }
}

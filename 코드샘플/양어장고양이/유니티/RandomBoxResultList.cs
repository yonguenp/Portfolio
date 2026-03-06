using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RandomBoxResultList : MonoBehaviour
{
    public GameObject[] resultList;
    public CardShopPopup shopCanvas;
    public GameObject ExitButton;
    public GameObject skipButton;
    public GameObject retryButton;

    private int insertIndex = 0;
    private List<uint> point = new List<uint>();
    private List<user_card> resultCardData = new List<user_card>();
    private List<float> interpolationRatio = new List<float>();
    public void Clear()
    {
        insertIndex = 0;
        ExitButton.SetActive(false);
        skipButton.SetActive(true);
        retryButton.SetActive(false);
        resultCardData.Clear();
        point.Clear();
        interpolationRatio.Clear();

        foreach(GameObject resultObject in resultList)
        {
            Button btn = resultObject.GetComponent<Button>();
            if(btn != null) 
                btn.interactable = true;
            Transform Perfect_Icon = resultObject.transform.Find("Perfect_Icon");
            Perfect_Icon?.gameObject.SetActive(false);
            Transform Movie_Icon = resultObject.transform.Find("Movie_Icon");
            Movie_Icon?.gameObject.SetActive(false);
            Transform Point_Icon = resultObject.transform.Find("Point_Icon");
            Point_Icon?.gameObject.SetActive(false);
        }
    }

    public void AddResult(user_card card, uint _point)
    {
        int curIndex = insertIndex;
        point.Add(_point);

        ImageBlur img = resultList[curIndex].GetComponentInChildren<ImageBlur>();
        img.enabled = true;
        Sprite icon = card.GetIcon();
        
        img.UseNormalTextureBlur(icon);
        Rect uv = card.GetUVRect();
        if (uv.size != Vector2.one)
        {
            Rect rect = card.GetRect();
            float diff = rect.width - rect.height;
            rect.x += diff * 0.5f;
            rect.width -= diff;

            Vector2 size = card.CardSpriteSize;
            uv.x = rect.x / size.x;
            uv.width = rect.width / size.x;
            uv.y = rect.y / size.y;
            uv.height = rect.height / size.y;

            img.target.uvRect = uv;
        }
        else
        {
            uv.position = Vector2.one;
            uv.size = Vector2.zero;
            foreach(Vector2 u in icon.uv)
            {
                if (u.x > uv.width)
                    uv.width = u.x;
                if (u.x < uv.x)
                    uv.x = u.x;
                if (u.y > uv.height)
                    uv.height = u.y;
                if (u.y < uv.y)
                    uv.y = u.y;
            }
            uv.size = uv.size - uv.position;
        }

        img.target.uvRect = uv;
        img.iterations = 7;
        img.interpolation = 1;
        img.downsample = 1;
        img.RefreshBlur();

        img.transform.parent.Find("Card_open_eff")?.gameObject.SetActive(false);
        img.transform.parent.Find("Card_open_perfect")?.gameObject.SetActive(false);

        Button btn = resultList[curIndex].GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {
            ImageBlur imgblur = resultList[curIndex].GetComponentInChildren<ImageBlur>();
            if (imgblur.enabled)
            {
                shopCanvas.ShowResult(curIndex);
            }
            else
            {
                shopCanvas.ShowCurCardDetailWithIndex(curIndex);
            }
        }));
        resultCardData.Add(card);
        interpolationRatio.Add(Random.value * 0.01f);
        insertIndex++;
    }

    private void Update()
    {
        for (int i = 0; i < resultCardData.Count; i++)
        {
            ImageBlur img = resultList[i].GetComponentInChildren<ImageBlur>();

            if (img != null && img.enabled)
            {
                float next = img.interpolation + interpolationRatio[i];
                if (next > 1.0f)
                {
                    next = 1.0f;
                    interpolationRatio[i] = Random.value * -0.01f;
                }
                if (next < 0.7f)
                {
                    next = 0.7f;
                    interpolationRatio[i] = Random.value * 0.01f;
                }

                img.interpolation = next;

                img.RefreshBlur();
            }
        }
    }

    public void OffEffect(int index)
    {
        for (int i = 0; i < resultCardData.Count; i++)
        {
            if (resultCardData[index] == resultCardData[i])
            {
                if(resultCardData[i].GetCardType() == user_card.CARD_TYPE.PERFECT)
                    resultList[i].transform.Find("Card_open_perfect")?.gameObject.SetActive(true);
                else
                    resultList[i].transform.Find("Card_open_eff")?.gameObject.SetActive(true);
            }
        }
    }

    public bool OffBlur(int index)
    {
        bool bAllClear = true;
        for (int i = 0; i < resultCardData.Count; i++)
        {
            ImageBlur img = resultList[i].GetComponentInChildren<ImageBlur>();
            RawImage rawImg = resultList[i].GetComponentInChildren<RawImage>();
            if (resultCardData[index] == resultCardData[i])
            {
                img.iterations = 1;
                img.interpolation = 0.0f;
                img.downsample = 0;
                img.RefreshBlur();

                img.enabled = false;

                if (rawImg)
                    rawImg.texture = img.texture;

                user_card.CARD_TYPE type = resultCardData[i].GetCardType();
                Transform Perfect_Icon = resultList[i].transform.Find("Perfect_Icon");
                Perfect_Icon?.gameObject.SetActive(type == user_card.CARD_TYPE.PERFECT);
                Transform Movie_Icon = resultList[i].transform.Find("Movie_Icon");
                Movie_Icon?.gameObject.SetActive(type == user_card.CARD_TYPE.MOVIE);
                Transform Point_Icon = resultList[i].transform.Find("Point_Icon");
                Point_Icon?.gameObject.SetActive(point[i] > 0);
            }
    
            if (img != null && img.enabled)
               bAllClear = false;            
        }

        return bAllClear && resultCardData.Count > 1;        
    }

    public void OnEffectFinished()
    {
        ExitButton.SetActive(true);
        skipButton.SetActive(false);
        SetRetryButton();
    }
    public void ShowAll()
    {
        StartCoroutine(ShowAllEffect());
    }

    public IEnumerator ShowAllEffect()
    {
        foreach (GameObject ro in resultList)
        {
            Button btn = ro.GetComponent<Button>();
            if (btn != null)
                btn.interactable = false;
        }

        skipButton.SetActive(false);

        for (int i = 0; i < resultCardData.Count; i++)
        {
            ImageBlur img = resultList[i].GetComponentInChildren<ImageBlur>();
            RawImage rawImg = resultList[i].GetComponentInChildren<RawImage>();

            if (img.enabled)
            {
                img.iterations = 1;
                img.interpolation = 0.0f;
                img.downsample = 0;
                img.RefreshBlur();

                img.enabled = false;

                if (rawImg)
                    rawImg.texture = img.texture;

                user_card.CARD_TYPE type = resultCardData[i].GetCardType();
                Transform effect = resultList[i].transform.Find("Card_open_eff");

                if (type == user_card.CARD_TYPE.PERFECT)
                {
                    effect = resultList[i].transform.Find("Card_open_perfect");
                }

                effect?.gameObject.SetActive(true);

                Transform Perfect_Icon = resultList[i].transform.Find("Perfect_Icon");
                Perfect_Icon?.gameObject.SetActive(type == user_card.CARD_TYPE.PERFECT);
                Transform Movie_Icon = resultList[i].transform.Find("Movie_Icon");
                Movie_Icon?.gameObject.SetActive(type == user_card.CARD_TYPE.MOVIE);
                Transform Point_Icon = resultList[i].transform.Find("Point_Icon");
                Point_Icon?.gameObject.SetActive(point[i] > 0);

                yield return new WaitForSeconds(0.2f);
            }
        }

        ExitButton.SetActive(true);
        SetRetryButton();

        yield return new WaitForSeconds(1.0f);

        foreach (GameObject ro in resultList)
        {
            Button btn = ro.GetComponent<Button>();
            if (btn != null)
                btn.interactable = true;
        }
    }
    public int GetInsertCount()
    {
        return insertIndex;
    }

    private void OnEnable()
    {
        
    }

    void SetRetryButton()
    {
        uint total = 0;
        foreach(uint p in point)
        {
            total += p;
        }

        point.Clear();
        if(total > 0)
        {
            RewardData reward = new RewardData();
            reward.point = total;
            NecoCanvas.GetPopupCanvas().OnSingleRewardPopup(LocalizeData.GetText("LOCALIZE_242"), LocalizeData.GetText("LOCALIZE_243"), reward);
        }
        retryButton.SetActive(true);
    }
}

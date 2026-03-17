using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UICountdown : MonoBehaviour
{
    Text countText;
    int result;
    int value;
    const int initDividor = 1000000;
    int currDividor;
    const float animTime = 0.05f;

    private void Awake()
    {
        countText = GetComponent<Text>();
        if (!countText) gameObject.SetActive(false);
    }

    public void Init(int cnt)
    {
        result = cnt;
        value = 0;
        countText.text = value.ToString("N0");
        currDividor = initDividor;
        StartCoroutine(SlotmachineCoroutine());
    }

    private IEnumerator SlotmachineCoroutine()
    {
        var digit = Mathf.FloorToInt(Mathf.Log10(result));
        currDividor = (int)Mathf.Pow(10, digit);

        int initFrame = 120;

        while (initFrame > 0)
        {
            countText.text = (value + UnityEngine.Random.Range(currDividor, currDividor * 10)).ToString("N0");
            yield return null;
            --initFrame;
        }

        int frame = 20;
        int currFrame = 0;

        while (currDividor > 0)
        {
            countText.text = (value + UnityEngine.Random.Range(currDividor, currDividor * 10)).ToString("N0");
            yield return null;
            ++currFrame;
            if (currFrame >= frame)
            {
                value += (result - value) / currDividor * currDividor;
                currDividor /= 10;
                currFrame = 0;
                countText.text = value.ToString("N0");
            }
        }
    }

    private IEnumerator CountdownCoroutine()
    {
        while (currDividor > 0)
        {
            if (result - value >= currDividor)
            {
                value += currDividor;
                countText.text = value.ToString("N0");
                yield return new WaitForSecondsRealtime(animTime);

            }
            else
            {
                currDividor /= 10;
            }
        }
    }
}

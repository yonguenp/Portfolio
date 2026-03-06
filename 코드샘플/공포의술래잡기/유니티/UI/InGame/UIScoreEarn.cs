using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScoreEarn : MonoBehaviour
{
    [SerializeField] Text txtScore;

    readonly float minIntervalSeconds = 0.05f;
    Queue<int> scoreQueue = new Queue<int>();
    float elapsedTime = 0f;

    public void AddScoreEffect(int score)
    {
        scoreQueue.Enqueue(score);
        //StartCoroutine(AddScoreEffectCO(score));
    }

    private void ShowScoreEffect()
    {
        var score = scoreQueue.Dequeue();
        var go = GameObject.Instantiate(txtScore);
        go.gameObject.SetActive(true);
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;

        (go as Text).text = $"+ {score}p";

        go.transform.DOLocalMove(new Vector3(0, 30, 0), 0.5f)
            .OnComplete(() => { Destroy(go); });
    }

    private void Update()
    {
        if (scoreQueue.Count > 0)
        {
            if (elapsedTime < minIntervalSeconds)
            {
                elapsedTime += Time.deltaTime;
            }
            else
            {
                ShowScoreEffect();
                elapsedTime = 0f;
            }
        }
    }
}

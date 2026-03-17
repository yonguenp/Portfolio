using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WebIndecator : MonoBehaviour
{
    [SerializeField] 
    Image background;
    [SerializeField]
    GameObject retryButton;
    [SerializeField]
    GameObject exitButton;
    [SerializeField]
    Image iconImage;
    [SerializeField]
    Text connectingText;

    Coroutine waitCoroutine = null;
    int retryCount = 0;
    Sequence seq = null;
    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    public void OnIndecator(SandboxNetwork.NetworkManager.NetworkQueue data)
    {
        if (data.useIndecator)
        {
            SetActive(true);
        }   
    }

    public void OffIndecator(SandboxNetwork.NetworkManager.NetworkQueue data)
    {
        if (data.useIndecator)
        {
            SetActive(false);
        }        
    }

    private void OnEnable()
    {
        Clear();

        waitCoroutine = StartCoroutine(WebWaitCoroutine());
    }

    private void OnDisable()
    {
        Clear();
    }

    private void Clear()
    {
        retryCount = 0;

        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }

        Color originColor = background.color;
        originColor.a = 0.0f;
        background.color = originColor;

        retryButton.SetActive(false);
        exitButton.SetActive(false);

        if (seq != null)
            seq.Kill();

        seq = null;

        iconImage.gameObject.SetActive(false);
        connectingText.gameObject.SetActive(false);
    }

    IEnumerator WebWaitCoroutine()
    {
        const float maxAlpha = 0.65f;
        const float noshowDelay = 0.5f;
        const float waitTime = 0.5f;
        const float perSecAlpha = maxAlpha / waitTime;

        if (seq != null)
            seq.Kill();

        iconImage.gameObject.SetActive(true);
        connectingText.gameObject.SetActive(false);
        retryButton.SetActive(false);
        exitButton.SetActive(false);


        Color iconColor = iconImage.color;
        iconImage.color = new Color(0.345098f, 0.9803922f, 3333333f, 0.0f);

        yield return new WaitForSeconds(0.5f);

        while (iconColor.a < 1.0f)
        {
            iconColor.a += Time.deltaTime * 2.0f;
            yield return new WaitForEndOfFrame();
        }

        Color bgColor = background.color;        
        if (bgColor.a <= 0.0f)
        {   
            yield return SandboxNetwork.SBDefine.GetWaitForSeconds(noshowDelay);
        }

        seq = DOTween.Sequence();
        seq.Join(iconImage.transform.DORotate(new Vector3(0, 0, 360), 0.75f * 4, RotateMode.FastBeyond360).SetEase(Ease.Linear));

        var coloring = DOTween.Sequence();
        coloring.Append(iconImage.DOColor(new Color(0f, 1.0f, 0.9411765f), 0.75f));
        coloring.Append(iconImage.DOColor(new Color(0.3254902f, 0.7764706f, 0.9882353f), 0.75f));
        coloring.Append(iconImage.DOColor(new Color(07843138f, 1.0f, 0.6666667f), 0.75f));
        coloring.Append(iconImage.DOColor(new Color(0.345098f, 0.9803922f, 3333333f), 0.75f));        
        seq.Join(coloring);

        seq.SetLoops(-1);


        while (bgColor.a < maxAlpha)
        {
            bgColor.a += perSecAlpha * Time.deltaTime;
            background.color = bgColor;
            yield return SandboxNetwork.SBDefine.GetWaitForEndOfFrame();
        }

        yield return new WaitForSeconds(0.5f);

        connectingText.gameObject.SetActive(true);

        bgColor.a = maxAlpha;
        background.color = bgColor;
    }

    public void OnNetworkError(SandboxNetwork.NetworkManager.NetworkQueue network)
    {
        if (retryCount < 3)
        {
            connectingText.gameObject.SetActive(false);
            retryButton.SetActive(true);
            exitButton.SetActive(false);

            Button button = retryButton.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => {
                retryCount++;

                retryButton.SetActive(false);

                SandboxNetwork.NetworkManager.Retry(network);
            });
        }
        else
        {
            connectingText.gameObject.SetActive(false);
            retryButton.SetActive(false);
            exitButton.SetActive(true);

            Button button = exitButton.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => {
                SceneManager.LoadScene("Start");
            });
        }
    }

    public Coroutine SendCorutine(SandboxNetwork.NetworkManager.NetworkQueue data, IEnumerator routine)
    {
        OnIndecator(data);

        return SandboxNetwork.UICanvas.Instance.StartCoroutine(routine);
    }
}

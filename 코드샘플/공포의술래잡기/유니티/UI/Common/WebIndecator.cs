using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WebIndecator : MonoBehaviour
{
    [SerializeField]
    Image background;
    [SerializeField]
    GameObject indecateGameObject;
    [SerializeField]
    GameObject retryButton;

    [SerializeField]
    GameObject errorObject;
    [SerializeField]
    GameObject errorButton;

    Coroutine waitCoroutine = null;
    int retryCount = 0;

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
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
        indecateGameObject.SetActive(false);
        errorObject.SetActive(false);
        errorButton.SetActive(false);
    }

    IEnumerator WebWaitCoroutine()
    {
        const float maxAlpha = 0.65f;
        const float noshowDelay = 0.5f;
        const float waitTime = 0.5f;
        const float perSecAlpha = maxAlpha / waitTime;

        retryButton.SetActive(false);
        indecateGameObject.SetActive(false);
        Color bgColor = background.color;

        if(bgColor.a <= 0.0f)
        {
            yield return new WaitForSeconds(noshowDelay);
        }

        
        while (bgColor.a < maxAlpha)
        {
            bgColor.a += perSecAlpha * Time.deltaTime;
            background.color = bgColor;
            yield return new WaitForEndOfFrame();
        }

        bgColor.a = maxAlpha;
        background.color = bgColor;
    }

    public void OnNetworkError(string uri,
        WWWForm param,
        SBWeb.SuccessCallback onSuccess)
    {
        if (retryCount < 3)
        {
            retryButton.SetActive(true);
            indecateGameObject.SetActive(true);
            errorObject.SetActive(false);
            errorButton.SetActive(false);

            Button button = retryButton.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => {
                retryCount++;

                retryButton.SetActive(false);
                indecateGameObject.SetActive(false);

                SBWeb.SendPost(uri, param, onSuccess);
            });
        }
        else
        {
            retryButton.SetActive(false);
            indecateGameObject.SetActive(false);
            errorObject.SetActive(true);
            errorButton.SetActive(true);

            Button button = errorButton.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => {
                SetActive(false);
                if (Managers.Network.IsAlive())
                    Managers.Network.Disconnect();
                else
                    Managers.Network.OnDisconnected();
            });
        }
    }
}

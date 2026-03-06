using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CookListUI : MonoBehaviour
{
    public bool isShow = false;
    public GameMain GameMain;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnEnable()
    {
        GameMain.FarmCanvas.FarmUIPanel.SetUI(FarmUIPanel.UI_STATE.UI_POPUP_OPENING);
    }

    public void OnDisable()
    {
        GameMain.FarmCanvas.FarmUIPanel.SetUI(FarmUIPanel.UI_STATE.UI_NORMAL);
    }

    public void ShowCookList()
    {
        isShow = true;
        gameObject.SetActive(true);
        gameObject.GetComponent<DOTweenAnimation>().DOPlayForward();

        CancelInvoke("OnCompleteTweenAnimation");
    }

    public void CloseCookList()
    {
        isShow = false;
        gameObject.SetActive(true);
        gameObject.GetComponent<DOTweenAnimation>().DOPlayBackwards();
        Invoke("OnCompleteTweenAnimation", 0.5f);
    }

    public void OnCompleteTweenAnimation()
    {
        gameObject.SetActive(isShow);

        if (!isShow)
        {
            ((WorldCanvas)GameMain.WorldCanvas).SetWorldState(WorldCanvas.STATE_WORLD.WORLD_MAP);
        }
    }
}

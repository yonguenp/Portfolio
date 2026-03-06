using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class TouchListUI : MonoBehaviour
{
    public bool isShow = false;
    public GameObject ListItemPrefab;
    public GameObject ScrollContainer;
    public Text TouchCountInfo;

    private GameCanvas GameCanvas = null;
    public GameCanvas GetGameCanvas() { return GameCanvas; }
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
        foreach (Transform child in ScrollContainer.transform)
        {
            Destroy(child.gameObject);
        }

        List<game_data> data_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.INTER_TOUCH);
        int enableCount = 0;
        if (data_list != null)
        {
            foreach (game_data data in data_list)
            {
                GameObject listItem = Instantiate(ListItemPrefab);
                listItem.transform.SetParent(ScrollContainer.transform);
                RectTransform rt = listItem.GetComponent<RectTransform>();
                rt.localScale = Vector3.one;
                rt.localPosition = Vector3.zero;
                List_item item = listItem.GetComponent<List_item>();
                if (item.SetTouchListUI((inter_touch)data, this))
                    enableCount += 1;
            }

            TouchCountInfo.text = enableCount.ToString() + "/" + data_list.Count.ToString();
        }
        else
            TouchCountInfo.text = "-/-";
    }

    public void OnDisable()
    {
        
    }

    public void OnTouchButton()
    {
        CloseTouchList();
    }

    public void ShowTouchList(GameCanvas canvas)
    {
        GameCanvas = canvas;

        isShow = true;
        gameObject.SetActive(true);
        gameObject.GetComponent<DOTweenAnimation>().DOPlayForward();
        CancelInvoke("OnCompleteTweenAnimation");
    }

    public void CloseTouchList()
    {
        isShow = false;
        gameObject.SetActive(true);
        gameObject.GetComponent<DOTweenAnimation>().DOPlayBackwards();
        Invoke("OnCompleteTweenAnimation", 0.5f);
    }

    public void OnCompleteTweenAnimation()
    {
        gameObject.SetActive(isShow);
    }
}

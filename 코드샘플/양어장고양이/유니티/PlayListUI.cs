using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class PlayListUI : MonoBehaviour
{
    public bool isShow = false;
    public GameObject ToyListContainer;
    public GameObject PlayListContainer;

    public GameObject ToyItemPrefab;
    public GameObject PlayItemPrefab;

    public Text PlayCountInfo;
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
        LoadUI();
    }

    public void OnDisable()
    {
        
    }

    public void OnPlayButton()
    {
        ClosePlayList();
    }

    public void ShowPlayList(GameCanvas canvas)
    {
        GameCanvas = canvas;

        isShow = true;
        gameObject.SetActive(true);
        gameObject.GetComponent<DOTweenAnimation>().DOPlayForward();
        CancelInvoke("OnCompleteTweenAnimation");
    }

    public void ClosePlayList()
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

    public void LoadUI()
    {
        foreach (Transform child in ToyListContainer.transform)
        {
            Destroy(child.gameObject);
        }

        List<game_data> toy_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_ITEMS);

        if (toy_list != null)
        {
            foreach (game_data data in toy_list)
            {
                GameObject listItem = Instantiate(ToyItemPrefab);
                listItem.transform.SetParent(ToyListContainer.transform);
                RectTransform rt = listItem.GetComponent<RectTransform>();
                rt.localScale = Vector3.one;
                rt.localPosition = Vector3.zero;
                ToyList_item item = listItem.GetComponent<ToyList_item>();
                item.SetToyListUI((user_items)data);
            }
        }

        foreach (Transform child in PlayListContainer.transform)
        {
            Destroy(child.gameObject);
        }

        int enableCount = 0;
        List<game_data> data_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.INTER_PLAY);

        if (data_list != null)
        {
            foreach (game_data data in data_list)
            {
                GameObject listItem = Instantiate(PlayItemPrefab);
                listItem.transform.SetParent(PlayListContainer.transform);
                RectTransform rt = listItem.GetComponent<RectTransform>();
                rt.localScale = Vector3.one;
                rt.localPosition = Vector3.zero;
                List_item item = listItem.GetComponent<List_item>();
                if(item.SetPlayListUI((inter_play)data, this))
                    enableCount += 1;
            }

            PlayCountInfo.text = enableCount.ToString() + "/" + data_list.Count.ToString();
        }
        else
            PlayCountInfo.text = "-/-";

    }
}

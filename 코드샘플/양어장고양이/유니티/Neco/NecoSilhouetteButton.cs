using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Newtonsoft.Json.Linq;

public class NecoSilhouetteButton : MonoBehaviour
{
    public GameObject busy;
    public GameObject boring;
    public GameObject gift;
    public GameObject visit;
    public GameObject exit;

    enum STATE{
        STATE_NONE,
        STATE_SOUND,
        STATE_BUSY,
        STATE_BORING,
        
        STATE_GIFT,
        STATE_ESCAPE,
        STATE_VISIT,
        STATE_EXIT,
    };

    STATE state = STATE.STATE_NONE;

    Image image;
    Button button;

    GameObject subUI = null;

    int curCatID = 0;
    protected void Awake()
    {
        button = gameObject.AddComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(OnSilhouetteButton);

        image = GetComponent<Image>();

        MapObjectController controller = GetComponentInParent<MapObjectController>();
        if (controller != null)
        {
            for (int i = 0; i < controller.Silhouette.Length; i++)
            {
                if (controller.Silhouette[i] == gameObject)
                {
                    curCatID = i;

                    int heart_gen = PlayerPrefs.GetInt("HEART_GEN_" + curCatID, 0);
                    if (heart_gen == 0)
                    {
                        heart_gen = (int)NecoCanvas.GetCurTime() + Random.Range(30, 60);
                        PlayerPrefs.SetInt("HEART_GEN_" + curCatID, heart_gen);

                        return;
                    }

                }
            }
        }
    }

    public void OnSilhouetteButton()
    {
        if (image.color.a < 1.0f)
            return;

        if (enabled == false)
        {
            if(neco_data.GetPrologueSeq() == neco_data.PrologueSeq.뒷길막이선물생성)
            {
                GetGift();
            }
            return;
        }

        switch(state)
        {
            case STATE.STATE_GIFT:
                GetGift();
                break;

            case STATE.STATE_BORING:
            case STATE.STATE_BUSY:
            case STATE.STATE_NONE:
                //SetRandomState();
                break;
        }
    }

    void OnEnable()
    {
        Invoke("SetGiftState", 1.0f);
    }

    private void OnDisable()
    {
        SetState(STATE.STATE_NONE);
    }

    private void Update()
    {
        if (image.color.a < 1.0f)
        {
            if(state != STATE.STATE_NONE && state != STATE.STATE_EXIT && state != STATE.STATE_VISIT)
                SetState(STATE.STATE_NONE);
        }
    }

    public void SetGiftState()
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (enabled == false)
            return;
        
        if (image.color.a < 1.0f)
        {
            Invoke("SetGiftState", 1.0f);
            return;
        }

        if(state == STATE.STATE_NONE)
        {
            if(curCatID == 0)
            {
                Invoke("SetGiftState", 1.0f);
                return;
            }

            int heart_gen = PlayerPrefs.GetInt("HEART_GEN_" + curCatID);
            if(NecoCanvas.GetCurTime() > heart_gen)
            {
                SetState(STATE.STATE_GIFT);
            }
            else
            {
                int diff = heart_gen - (int)NecoCanvas.GetCurTime();
                Invoke("SetGiftState", diff);
            }
        }
    }

    public void SetRandomState()
    {
        if(state != STATE.STATE_NONE && state != STATE.STATE_BORING && state != STATE.STATE_BUSY)
        {
            return;
        }

        CancelInvoke("SetGiftState");

        List<STATE> randomState = new List<STATE>();
        randomState.Add(STATE.STATE_SOUND);
        randomState.Add(STATE.STATE_BUSY);
        randomState.Add(STATE.STATE_BORING);
        //randomState.Add(STATE.STATE_ESCAPE);
        
        SetState(randomState[Random.Range(0, randomState.Count)]);
    }


    public void SetState()
    {
        SetState(STATE.STATE_NONE);
    }

    public void SetBoringStateForPrologue()
    {
        if (STATE.STATE_BORING == state)
            return;

        SetState(STATE.STATE_BORING, true);
    }

    public void SetGiftStateForPrologue()
    {
        if(STATE.STATE_GIFT != state)
            SetState(STATE.STATE_GIFT, true);
    }

    public void SetVisitObject()
    {
        CancelInvoke("SetState");
        CancelInvoke("SetGiftState");

        if (state == STATE.STATE_VISIT)
            return;

        if (!gameObject.activeInHierarchy)
            return;

        state = STATE.STATE_VISIT;
        if (subUI != null)
        {
            Destroy(subUI);
        }

        MapObjectController map = NecoCanvas.GetGameCanvas().GetCurMapController();
        if (map == null)
            return;

        neco_map mapData = map.GetMapData();
        if (mapData == null)
            return;

        bool bVisitCurMap = false;
        foreach (neco_spot spot in mapData.GetSpots())
        {
            if(spot == neco_cat.GetNecoCat((uint)curCatID).GetSpot())
            {
                bVisitCurMap = true;
            }
        }

        if (bVisitCurMap)
        {
            if (visit == null)
            {
                return;
            }

            subUI = Instantiate(visit);
            subUI.transform.SetParent(transform.parent);
            subUI.transform.localPosition = transform.localPosition;
            subUI.transform.localScale = transform.localScale;

            foreach (MaskableGraphic graphic in subUI.GetComponentsInChildren<MaskableGraphic>())
            {
                graphic.color = new Color(1, 1, 1, 0);
                Sequence seq = DOTween.Sequence();

                seq.Append(graphic.DOColor(Color.white, 0.1f));
                seq.AppendInterval(0.5f);
                seq.Append(graphic.DOColor(new Color(1, 1, 1, 0), 0.3f).OnComplete(()=> {
                    Color color = gameObject.GetComponent<Image>().color;
                    if(color.a > 0)
                    {
                        color.a = 0;
                        gameObject.GetComponent<Image>().DOColor(color, 0.5f);
                    }
                }));

                seq.Restart();
            }
        }
        else
        {
            if (exit == null)
            {
                return;
            }

            subUI = Instantiate(exit);
            subUI.transform.SetParent(transform.parent);
            subUI.transform.localPosition = transform.localPosition;
            subUI.transform.localScale = transform.localScale;

            foreach (MaskableGraphic graphic in subUI.GetComponentsInChildren<MaskableGraphic>())
            {
                graphic.color = new Color(1, 1, 1, 0);
                Sequence seq = DOTween.Sequence();

                seq.Append(graphic.DOColor(Color.white, 0.1f));
                seq.AppendInterval(0.2f);
                seq.Append(graphic.DOColor(new Color(1, 1, 1, 0), 0.3f).OnComplete(() => {
                    Color color = gameObject.GetComponent<Image>().color;
                    if (color.a > 0)
                    {
                        color.a = 0;
                        gameObject.GetComponent<Image>().DOColor(color, 0.5f);
                    }
                }));

                seq.Restart();
            }
        }

        Invoke("ClearSubUI", 1.0f);
    }

    public void SetExitMap()
    {
        CancelInvoke("SetState");
        CancelInvoke("SetGiftState");

        if (state == STATE.STATE_EXIT)
            return;
        
        if (!gameObject.activeInHierarchy)
            return;

        state = STATE.STATE_EXIT;
        if (subUI != null)
        {
            Destroy(subUI);
        }

        if (exit == null)
        {
            return;
        }

        subUI = Instantiate(exit);
        subUI.transform.SetParent(transform.parent);
        subUI.transform.localPosition = transform.localPosition;
        subUI.transform.localScale = transform.localScale;

        foreach(MaskableGraphic graphic in subUI.GetComponentsInChildren<MaskableGraphic>())
        {
            graphic.color = new Color(1, 1, 1, 0);
            Sequence seq = DOTween.Sequence();
            
            seq.Append(graphic.DOColor(Color.white, 0.1f));
            seq.AppendInterval(0.2f);
            seq.Append(graphic.DOColor(new Color(1, 1, 1, 0), 0.3f));

            seq.Restart();
        }

        Invoke("ClearSubUI", 1.0f);        
    }

    public void ClearSubUI()
    {
        Destroy(subUI);
        subUI = null;
    }

    void SetState(STATE s, bool force = false)
    {
        CancelInvoke("SetGiftState");

        state = s;
        
        if (subUI != null)
        {
            Destroy(subUI);
        }

        if (!gameObject.activeInHierarchy || !enabled)
        {
            if(!force)
                state = STATE.STATE_NONE;
        }

        List<Button> childButton = null;
        Transform MovableFoodTruck = transform.parent.Find("MovableFoodTruck");

        switch (state)
        {
            case STATE.STATE_NONE:
                if(gameObject.activeInHierarchy && enabled)
                    Invoke("SetGiftState", 5.0f);                
                break;
            case STATE.STATE_SOUND:
                AudioManager.GetInstance().PlayNyangSound(0.5f);
                Invoke("SetState", 3.0f);
                break;
            case STATE.STATE_BUSY:
                subUI = Instantiate(busy);
                subUI.transform.SetParent(transform.parent);
                subUI.transform.localPosition = transform.localPosition;
                subUI.transform.localScale = transform.localScale;
                childButton = new List<Button>(subUI.GetComponentsInChildren<Button>());

                if(MovableFoodTruck)
                {
                    subUI.transform.SetSiblingIndex(MovableFoodTruck.GetSiblingIndex());
                }
                Invoke("SetState", 3.0f);
                break;
            case STATE.STATE_BORING:
                subUI = Instantiate(boring);
                subUI.transform.SetParent(transform.parent);
                subUI.transform.localPosition = transform.localPosition;
                subUI.transform.localScale = transform.localScale;
                childButton = new List<Button>(subUI.GetComponentsInChildren<Button>());

                if (MovableFoodTruck)
                {
                    subUI.transform.SetSiblingIndex(MovableFoodTruck.GetSiblingIndex());
                }
                Invoke("SetState", 3.0f);
                break;
            case STATE.STATE_GIFT:
                subUI = Instantiate(gift);
                subUI.transform.SetParent(transform.parent);
                subUI.transform.localPosition = transform.localPosition;
                subUI.transform.localScale = transform.localScale;
                childButton = new List<Button>(subUI.GetComponentsInChildren<Button>());

                if (MovableFoodTruck)
                {
                    subUI.transform.SetSiblingIndex(MovableFoodTruck.GetSiblingIndex());
                }
                break;
            case STATE.STATE_ESCAPE:
                bool moved = false;
                MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
                if(mapController != null)
                {
                    for(int i = 0; i < mapController.Silhouette.Length; i++)
                    {
                        if(mapController.Silhouette[i] == this.gameObject)
                        {
                            moved = NecoCanvas.GetGameCanvas().catUpdater.ForceMoveCat((uint)i);
                        }
                    }                    
                }

                if(!moved)
                {
                    Invoke("SetState", 3.0f);
                }

                break;
        }

        if (childButton != null)
        {
            foreach (Button btn in childButton)
            {
                btn.onClick.AddListener(OnSilhouetteButton);
            }
        }
    }

    public void GetGift()
    {
        if (state != STATE.STATE_GIFT || curCatID == 0)
            return;

        PlayerPrefs.SetInt("HEART_GEN_" + curCatID, (int)NecoCanvas.GetCurTime() + Random.Range(180, 300));

        button.interactable = false;
        Button childButton = subUI.GetComponentInChildren<Button>();
        if(childButton != null)
            childButton.interactable = false;
        int type = Random.Range(0, 2);
        
        WWWForm data = new WWWForm();
        data.AddField("api", "chore");
        data.AddField("op", 4);

        GameObject bubbleTextObject = null;
        GameObject getAniObject = null;
        GameObject goldObject = null;
        GameObject materialObject = null;
        if (subUI != null)
        {
            Transform Back_Bubble = subUI.transform.Find("Back_Bubble");
            if (Back_Bubble != null)
            {
                bubbleTextObject = Back_Bubble.Find("Bubble_text").gameObject;
                getAniObject = Back_Bubble.Find("Get_ani").gameObject;
                if (getAniObject != null)
                {
                    goldObject = getAniObject.transform.Find("Gold_ani").gameObject;
                    materialObject = getAniObject.transform.Find("Material_ani").gameObject;
                }
            }
        }

        System.Action cb = null;
        switch (type)
        {
            case 0:
                {
                    goldObject.SetActive(true);
                    materialObject.SetActive(false);
                    float level = (float)System.Convert.ToInt32(neco_data.Instance.GetFishfarmLevel());
                    List<neco_food> foods = new List<neco_food>();
                    List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_MAP);
                    if (necoData != null)
                    {
                        foreach (neco_map map in necoData)
                        {
                            if (map != null)
                            {
                                if (map.IsOpened())
                                {
                                    neco_spot foodspot = map.GetFoodSpot();
                                    if (foodspot != null)
                                    {
                                        int foodRemain = (int)foodspot.GetItemRemain();
                                        if (foodRemain > 0 && foodspot.GetCurItem() != null)
                                        {
                                            foods.Add(neco_food.GetFoodInfo(foodspot.GetCurItem().GetItemID()));
                                        }   
                                    }
                                }
                            }
                        }
                    }
                    float foodRate = 0.0f;
                    foreach(neco_food food in foods)
                    {
                        foodRate += food.GetWeight();
                    }

                    foodRate = foodRate * 0.9f;

                    int cnt = (int)Random.Range(level, (level * 1.5f) + foods.Count + foodRate);
                    data.AddField("gold", cnt);

                }
                break;           
            case 1:
                {
                    goldObject.SetActive(false);
                    materialObject.SetActive(true);

                    type = 56 + Random.Range(0, 8);

                    if (type >= 61)
                        type = 61;

                    data.AddField("item", type);
                    int level = System.Convert.ToInt32(neco_data.Instance.GetGiftBasketLevel());
                    int cnt = Random.Range(level, (level * 5) + 1);
                    data.AddField("cnt", cnt);
                }
                break;
        }

        NetworkManager.GetInstance().SendPostSimple("chore", 4, data, (response) =>
        {
            if (subUI != null)
            {
                if (bubbleTextObject != null)
                {
                    bubbleTextObject.SetActive(false);

                    if (getAniObject != null)
                    {
                        getAniObject.SetActive(true);
                    }
                }
            }

            JObject root = JObject.Parse(response);
            JToken apiToken = root["api"];
            if (null == apiToken || apiToken.Type != JTokenType.Array
                || !apiToken.HasValues)
            {
                return;
            }

            JArray apiArr = (JArray)apiToken;
            foreach (JObject row in apiArr)
            {
                string uri = row.GetValue("uri").ToString();
                if (uri == "chore")
                {
                    JToken resultCode = row["rs"];
                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            JObject income = (JObject)row["rew"];
                            if (income.ContainsKey("gold"))
                            {
                                if (income["gold"].Value<uint>() > 0)
                                {
                                    Image goldImage = goldObject.transform.Find("Image").GetComponent<Image>();
                                    Text amountText = goldObject.transform.Find("Text").GetComponent<Text>();
                                    goldImage.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_coin");
                                    amountText.text = "+" + income["gold"].Value<uint>();
                                }
                            }

                            if (income.ContainsKey("item"))
                            {
                                JArray item = (JArray)income["item"];
                                foreach (JObject rw in item)
                                {
                                    Image materialImage = materialObject.transform.Find("Material_get").GetComponent<Image>();
                                    Text amountText = materialObject.transform.Find("Text").GetComponent<Text>();
                                    materialImage.sprite = items.GetItem(rw["id"].Value<uint>()).GetItemIcon();
                                    amountText.text = "+" + rw["amount"].Value<uint>();
                                }
                            }
                        }
                    }
                }

                if (uri == "user")
                {
                    NetworkManager.GetInstance().OnResponseUser(row);
                }
                if (uri == "item")
                {
                    NetworkManager.GetInstance().OnResponseItem(row);
                }

                cb?.Invoke();
            }

            Invoke("GiftEffectDone", 2.0f);
        });
    }

    public void GiftEffectDone()
    {
        button.interactable = true;

        Invoke("SetState", 0.5f);

        NecoCanvas.GetUICanvas().RefreshTopUILayer(TOP_UI_PANEL_TYPE.RESOURCE);

        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.말풍선아이템받기)
        {
            MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
            if (mapController != null)
            {
                mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.말풍선아이템받기, SendMessageOptions.DontRequireReceiver);
            }
        }
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.뒷길막이선물생성)
        {
            MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
            if (mapController != null)
            {
                mapController.SendMessage("뒷길막이선물획득대사", SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}

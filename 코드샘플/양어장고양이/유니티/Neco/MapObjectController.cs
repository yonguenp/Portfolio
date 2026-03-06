using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapObjectController : UIBehaviour
{
    public RectTransform MapBackgroundImage;
    public RectTransform MapSpotContainer;
    public MapObjectSpot[] MapObjectSpots;

    public Vector2 MaxSize;
    public Vector2 MinSize;

    public GameObject[] Silhouette = new GameObject[14];
    public GameObject MovableFoodTruck;

    protected neco_map curMapData = null;
    bool firstUpdate = true;
    protected void ClearSpotObject()
    {
        foreach(MapObjectSpot child in MapObjectSpots)
        {
            child.gameObject.SetActive(false);
        }
    }

    protected override void OnEnable()
    {
        ResetBackgroundSize();
    }

    protected override void OnRectTransformDimensionsChange()
    {
        ResetBackgroundSize();
    }

    public void ResetBackgroundSize()
    {
        NecoGameCanvas gameCanvas = NecoCanvas.GetGameCanvas();
        if (gameCanvas == null)
            return;

        RectTransform canvasRectTransform = (gameCanvas.transform as RectTransform);
        RectTransform backgroundRectTransform = (MapBackgroundImage.transform as RectTransform);

        Vector2 canvasSize = canvasRectTransform.sizeDelta;

        float widthRatio = MaxSize.x / canvasSize.x;
        float heightRatio = MaxSize.y / canvasSize.y;        
        float maxRatio = Mathf.Max(widthRatio, heightRatio);

        widthRatio = MinSize.x / canvasSize.x;
        heightRatio = MinSize.y / canvasSize.y;
        float minRatio = Mathf.Max(widthRatio, heightRatio);

        float raito = Mathf.Min(maxRatio, minRatio);

        backgroundRectTransform.sizeDelta = new Vector2(MapBackgroundImage.sizeDelta.x, MapBackgroundImage.sizeDelta.y);
        backgroundRectTransform.localScale = Vector3.one / raito;

        if ((transform as RectTransform).sizeDelta != canvasSize)
        {
            (transform as RectTransform).sizeDelta = canvasSize;

            Vector2 viewSize = new Vector2(MapBackgroundImage.sizeDelta.x, MapBackgroundImage.sizeDelta.y) * (Vector3.one / raito);
            viewSize.x = Mathf.Min(viewSize.x, canvasRectTransform.sizeDelta.x);
            viewSize.y = Mathf.Min(viewSize.y, canvasRectTransform.sizeDelta.y);

            NecoCanvas.GetGameCanvas()?.ResetBackgroundSize(viewSize);
            NecoCanvas.GetUICanvas()?.ResetBackgroundSize(viewSize);
            NecoCanvas.GetPopupCanvas()?.ResetBackgroundSize(viewSize);
        }
    }

    public virtual void OnInitMap(neco_map mapData)
    {
        curMapData = mapData;

        ClearSpotObject();
        
        InitSpots();

        RefreshCatSilhouettes(true);
        RefreshFoodTruck();
    }

    protected void InitSpots()
    {
        foreach (MapObjectSpot mapSpot in MapObjectSpots)
        {
            mapSpot.gameObject.SetActive(true);
            mapSpot.OnInitSpot();
        }
    }

    public void RefreshCatSilhouettes(bool bFirstInit = false)
    {
        for (int i = 0; i < Silhouette.Length; i++)
        {
            if (Silhouette.Length > i && Silhouette[i] != null)
            {
                GameObject silhouette = Silhouette[i];
                neco_cat catData = neco_cat.GetNecoCat((uint)i);
                
                if (catData != null && catData.IsReadyCat())
                {
                    uint firstAppearMapID = neco_data.Instance.FirstAppearCatMapID((uint)i);
                    if (firstAppearMapID == curMapData.GetMapID())
                    {
                        silhouette.SetActive(true);
                        silhouette.GetComponent<Image>().DOColor(Color.white, 1.0f).OnComplete(() =>
                        {
                            foreach (Transform child in silhouette.transform)
                            {
                                child.gameObject.SetActive(true);
                            }
                        });

                        silhouette.GetComponent<NecoSilhouetteButton>().enabled = false;                        
                    }
                    else
                    {
                        silhouette.SetActive(false);
                        foreach (Transform child in silhouette.transform)
                        {
                            child.gameObject.SetActive(false);
                        }
                    }
                }
                else
                {
                    if(bFirstInit)
                    {
                        silhouette.SetActive(false);
                        foreach (Transform child in silhouette.transform)
                        {
                            child.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }
    }

    public void RefreshCat(neco_cat cat)
    {
        if (Silhouette.Length <= cat.GetCatID())
            return;

        GameObject catObject = Silhouette[cat.GetCatID()];
        if(catObject != null)
        {
            Image image = catObject.GetComponent<Image>();
            
            if (cat.IsOnSpot())
            {
                if (image.color == Color.white)
                {
                    catObject.GetComponent<NecoSilhouetteButton>().SetVisitObject();
                }

                Sequence seq = DOTween.Sequence();

                seq.AppendInterval(0.5f);
                seq.Append(image.DOColor(new Color(1, 1, 1, 0), 0.5f));

                seq.Restart();
            }
            else
            {
                bool nowActive = false;
                bool OnFood = false;
                neco_spot food_spot = curMapData.GetFoodSpot();
                if (food_spot != null)
                {
                    uint foodRemain = food_spot.GetItemRemain();
                    if (foodRemain > 0)
                    {
                        Invoke("RefreshCatSilhouettes", foodRemain);
                        OnFood = true;
                    }
                }

                nowActive = cat.IsGainCat() && OnFood && !cat.IsOnSpot() && NecoCanvas.GetGameCanvas().catUpdater.IsCatOnMap(curMapData, cat);

                if (!catObject.activeInHierarchy)
                {   
                    image.color = new Color(1, 1, 1, 0);                    
                }

                catObject.SetActive(nowActive);
                
                if (nowActive)
                {
                    if (image.color != Color.white)
                        catObject.GetComponent<NecoSilhouetteButton>().SetState();

                    image.DOColor(new Color(1, 1, 1, 1), 1.0f).OnComplete(OnCatSilhouetteIn);
                }
            }

            foreach (Transform child in catObject.transform)
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    public void RefreshCat(List<neco_cat> cats)
    {
        for(int i = 0; i < Silhouette.Length; i++)
        {
            neco_cat cat = neco_cat.GetNecoCat((uint)i);
            GameObject catObject = Silhouette[i];
            if(catObject != null && cat != null)
            {
                if (cat.IsReadyCat())
                    continue;

                Color goalColor = new Color(1, 1, 1, 0);
                Image image = catObject.GetComponent<Image>();
                
                bool nowActive = false;
                
                if (!cat.IsOnSpot())                
                {                    
                    nowActive = cat.IsGainCat() && !cat.IsOnSpot() && cats.Contains(cat) && NecoCanvas.GetGameCanvas().catUpdater.IsCatOnMap(curMapData, cat);

                    if (!catObject.activeInHierarchy)
                    {
                        image.color = new Color(1, 1, 1, 0);
                    }

                    catObject.SetActive(nowActive);

                    if (nowActive)
                    {
                        goalColor = new Color(1, 1, 1, 1);                        
                    }
                }
                else
                {
                    image.DOColor(goalColor, 0.0f);
                }

                if (catObject.activeInHierarchy)
                {                    
                    if (firstUpdate)
                        image.color = goalColor;
                    else
                    {
                        if (!cats.Contains(cat))//맵에없음
                        {
                            catObject.GetComponent<NecoSilhouetteButton>().SetExitMap();
                        }

                        image.DOColor(goalColor, 1.0f);
                    }
                }
            }
        }

        firstUpdate = false;
    }

    public bool IsMovableMap()
    {
        
        return true;
    }

    public virtual void OnCatSilhouetteIn()
    {

    }

    public virtual void OnCatSilhouetteOut()
    {
        //??
    }

    public neco_map GetMapData()
    {
        return curMapData;
    }

    public void RefreshFoodTruck()
    {
        CancelInvoke("RefreshFoodTruck");

        if (MovableFoodTruck == null)
            return;

        MovableFoodTruck.SetActive(false);

        bool isTruckOpened = user_items.GetUserItemAmount(129) > 0;
        bool isTruckEnable = PlayerPrefs.GetInt("AUTO_DISPENSER", 0) > 0;
        if (isTruckOpened && isTruckEnable)
        {
            bool hasFood = false;
            List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_FOOD);
            if (necoData != null)
            {
                foreach (neco_food data in necoData)
                {
                    if (data != null)
                    {
                        if(user_items.GetUserItemAmount(data.GetFoodID()) > 0)
                        {
                            hasFood = true;
                            break;
                        }
                    }
                }
            }

            if (hasFood)
            {
                List<uint> mapList = new List<uint>();
                if (neco_map.GetNecoMap(8).IsOpened())
                    mapList.Add(8);
                if (neco_map.GetNecoMap(9).IsOpened())
                    mapList.Add(9);
                if (neco_map.GetNecoMap(6).IsOpened())
                    mapList.Add(6);
                if (neco_map.GetNecoMap(10).IsOpened())
                    mapList.Add(10);                
                if (neco_map.GetNecoMap(1).IsOpened())
                    mapList.Add(1);
                if (neco_map.GetNecoMap(2).IsOpened())
                    mapList.Add(2);
                if (neco_map.GetNecoMap(3).IsOpened())
                    mapList.Add(3);
                if (neco_map.GetNecoMap(4).IsOpened())
                    mapList.Add(4);
                if (neco_map.GetNecoMap(5).IsOpened())
                    mapList.Add(5);
                if (neco_map.GetNecoMap(7).IsOpened())
                    mapList.Add(7);

                if (mapList.Count == 0)
                {
                    Invoke("CheckAutoFoodDispenser", 1.0f);
                    return;
                }

                float moveTime = 15.0f;
                float cycleTime = mapList.Count * moveTime; //75
                int curTime = (int)NecoCanvas.GetCurTime();
                float curCycleRate = (curTime % (int)cycleTime);

                int curIndex = (int)(curCycleRate / moveTime);
                float curPosRate = (float)(curCycleRate % moveTime) / moveTime;

                if (mapList[curIndex] == curMapData.GetMapID())
                {
                    MovableFoodTruck.SetActive(true);
                    float mapWidth = MapBackgroundImage.sizeDelta.x + 300;

                    Vector3 truckPos = MovableFoodTruck.transform.localPosition;
                    truckPos.x = (mapWidth * -0.5f) + (mapWidth * curPosRate);
                    MovableFoodTruck.transform.localPosition = truckPos;
                    MovableFoodTruck.transform.DOLocalMoveX(mapWidth * 0.5f, moveTime - (moveTime * curPosRate)).OnComplete(() =>
                    {
                        MovableFoodTruck.SetActive(false);
                    });
                }

                Invoke("RefreshFoodTruck", (moveTime - (moveTime * curPosRate)));
            }

            Invoke("CheckAutoFoodDispenser", 1.0f);
        }
    }

    public void CheckAutoFoodDispenser()
    {
        CancelInvoke("CheckAutoFoodDispenser");

        items item = null;
        uint itemCount = 0;
        int itemDuration = 0;
        List<game_data> f = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_FOOD);
        if (f != null)
        {
            foreach (neco_food data in f)
            {
                if (data != null)
                {
                    if (user_items.GetUserItemAmount(data.GetFoodID()) > 0)
                    {
                        item = items.GetItem(data.GetFoodID());
                        itemCount = user_items.GetUserItemAmount(data.GetFoodID());
                        itemDuration = (int)data.GetFoodDuration();
                        break;
                    }
                }
            }
        }

        
        bool enableAutoDispenser = itemCount > 0;
        
        if (enableAutoDispenser)
        {
            int nextCheckTime = int.MaxValue;
            List<neco_spot> targets = new List<neco_spot>();
            List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_MAP);
            if (necoData != null)
            {
                if (PlayerPrefs.GetInt("AUTO_DISPENSER", 0) > 0)
                {
                    foreach (neco_map map in necoData)
                    {
                        if (map != null)
                        {
                            if(map.IsOpened())
                            {
                                neco_spot foodspot = map.GetFoodSpot();
                                if (itemCount > 0)
                                {
                                    if (foodspot != null)
                                    {
                                        int foodRemain = (int)foodspot.GetItemRemain();
                                        if (foodRemain <= 0)
                                        {
                                            targets.Add(foodspot);
                                            nextCheckTime = Mathf.Min(itemDuration, nextCheckTime);
                                            itemCount--;
                                        }
                                        else
                                        {
                                            nextCheckTime = Mathf.Min(foodRemain, nextCheckTime);
                                        }
                                    }
                                }
                                else
                                {
                                    if(targets.Count > 0)
                                    {
                                        NecoCanvas.GetGameCanvas().OnSetFoods(targets, item, RefreshFoodTruck);
                                    }
                                    return;
                                }
                            }
                        }
                    }
                }
            }

            if (targets.Count > 0)
            {
                NecoCanvas.GetGameCanvas().OnSetFoods(targets, item);
            }

            if (nextCheckTime != int.MaxValue)
                Invoke("CheckAutoFoodDispenser", nextCheckTime);
            else
                Invoke("CheckAutoFoodDispenser", 1);
        }
        else
        {
            Invoke("CheckAutoFoodDispenser", 1);
        }
    }
}

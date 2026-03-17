using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldMapUI : MonoBehaviour
{
    public List<WorldMapItem> setupObject;
    public DOTweenAnimation tween;
    public WorldCanvas worldCanvas;
    public FoodListUI foodListUI;
    public CookListUI cookListUI;
    public Text worldDesc;
    public uint curSelected = 0;

    public GameObject backgroundUI;
    public GameObject setupUI;
    public AreaUI areaUI;
    
    public DOTweenAnimation[] ToggleTweens;

    private void OnEnable()
    {
        OnAreaUIClose();
        InitUI();
    }

    public void InitUI()
    {
        tween.gameObject.SetActive(false);
        setupUI.SetActive(true);
        areaUI.gameObject.SetActive(false);

        if (ContentLocker.GetCurContentSeq() < ContentLocker.ContentGuideDoneSeq)
        {
            curSelected = 0;
        }

        AudioSource audio = worldCanvas.GameManager.GetComponent<AudioSource>();
        if (audio.mute != false)
        {
            audio.DOFade(0.0f, 0.3f).OnComplete(() => {
                audio.clip = Resources.Load<AudioClip>("Sound/bgm_map");
                audio.Play();

                audio.DOFade((float)PlayerPrefs.GetInt("Config_BV", 9) / 9, 0.3f);
            });
        }

        foreach (WorldMapItem item in setupObject)
        {
            item.gameObject.SetActive(true);
            item.SetParentWorldMapPanel(this);
        }

        //object obj;
        //List<game_data> data_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.WALK_AREA);
        //foreach (game_data data in data_list)
        //{
        //    if (data.data.TryGetValue("walk_id", out obj))
        //    {
        //        uint id = (uint)obj;
        //        foreach (WorldMapItem item in setupObject)
        //        {
        //            if (item.world_type == id)
        //            {
        //                item.gameObject.SetActive(true);
        //                item.SetParentWorldMapPanel(this);
        //            }
        //        }
        //    }
        //}

        //foreach (WorldMapItem item in setupObject)
        //{
        //    if (item.world_type == 99 || item.world_type == 100 || item.world_type == 101)
        //    {
        //        item.gameObject.SetActive(true);
        //        item.SetParentWorldMapPanel(this);
        //    }
        //}

        //foreach (WorldMapItem item in areaObjects)
        //{
        //    item.SetCursor(false);
        //}

        //tween.gameObject.SetActive(false);

        if (curSelected == 1)
            curSelected = 0;

        OnSelect(curSelected);
    }

    private void OnDisable()
    {
        AudioSource audio = worldCanvas.GameManager.GetComponent<AudioSource>();
        if (audio.mute != false)
        {
            audio.DOFade(0.0f, 0.3f).OnComplete(() => {
                audio.clip = Resources.Load<AudioClip>("Sound/bgm_intro");
                audio.Play();

                audio.DOFade((float)PlayerPrefs.GetInt("Config_BV", 9) / 9, 0.3f);
            });
        }
    }

    public void OnSelect(uint index)
    {
        if (index != 0)
        {
            if (!tween.gameObject.activeSelf)
            {
                tween.gameObject.SetActive(true);
                tween.DORewind();
                tween.DOPlayForward();
            }
        }
        else
        {
            tween.gameObject.SetActive(false);
        }

        curSelected = index;
        if (curSelected == 1)
        {
            OnAreaUIOpen();
            return;
        }

        foreach (WorldMapItem item in setupObject)
        {
            item.SetCursor(curSelected == item.world_type);
        }

        string key = "";
        switch (curSelected)
        {
            case 2:
                key = "25";
                break;
            case 1:
                key = "19";
                break;
            case 99:
                key = "23";
                break;
            case 100:
                key = "21";
                break;
            case 101:
                key = "28";
                break;
            default:
                worldDesc.text = "";
                return;
        }

        worldDesc.text = LocalizeData.GetText(key);
    }

    public void OnStartWorld()
    {
        switch (curSelected)
        {
            case 99:
                worldCanvas.SetWorldState(WorldCanvas.STATE_WORLD.WORLD_COOK);//for tutorial
                cookListUI.ShowCookList();
                break;
            case 100:
                worldCanvas.SetWorldState(WorldCanvas.STATE_WORLD.WORLD_FEED);//for tutorial
                foodListUI.ShowFoodList();
                break;
            case 101:
                worldCanvas.GameManager.PopupControl.OnPopupMessage(LocalizeData.GetText("Product_Ready"));
                break;

            default:
                worldCanvas.OnWorldMapSelect(curSelected);
                break;
        }
    }

    public void OnClose()
    {
        if (setupUI.activeSelf)
            worldCanvas.GameManager.OnGameButton();
        else
            OnAreaUIClose();
    }

    public void OnAreaUIClose()
    {
        InitUI();

        OnBackgroundOut();
        worldCanvas.curWorldUI = WorldCanvas.STATE_WORLD.WORLD_MAP;

        areaUI.mapScroller.horizontalNormalizedPosition = 0.5f;
        areaUI.mapScroller.verticalNormalizedPosition = 0.0f;

        foreach (WorldMapItem item in setupObject)
        {
            item.gameObject.SetActive(true);
        }
    }

    public void OnAreaUIOpen()
    {
        tween.gameObject.SetActive(false);
        setupUI.SetActive(false);
        areaUI.gameObject.SetActive(true);

        OnBackgroundIn();
        worldCanvas.curWorldUI = WorldCanvas.STATE_WORLD.WORLD_STAGE;

        areaUI.mapScroller.horizontalNormalizedPosition = 0.5f;
        areaUI.mapScroller.verticalNormalizedPosition = 0.0f;

        foreach (WorldMapItem item in setupObject)
        {
            item.gameObject.SetActive(false);
        }
    }

    public void OnBackgroundOut()
    {
        backgroundUI.transform.DOScale(1.0f, 0.5f);
        OnUIShow();
    }

    public void OnBackgroundIn()
    {
        backgroundUI.transform.DOScale(1.2f, 0.5f);
        OnUIHide();
    }

    public void OnUIHide()
    {
        foreach(DOTweenAnimation anim in ToggleTweens)
        {
            if(anim.gameObject.activeInHierarchy)
                anim.DOPlayBackwards();
        }
    }

    public void OnUIShow()
    {
        foreach (DOTweenAnimation anim in ToggleTweens)
        {
            if (anim.gameObject.activeInHierarchy)
                anim.DOPlayForward();
        }
    }

    public void OnUIRefresh(bool show)
    {
        if (show)
            OnUIShow();
        else
            OnUIHide();
    }
}

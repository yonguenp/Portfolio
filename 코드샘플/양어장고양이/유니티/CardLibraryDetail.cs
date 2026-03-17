using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class CardLibraryDetail : MonoBehaviour
{
    public CardListPopup cardCanvas;
    public CardDetailPanel detailPanel;
    public ImageBlur fullShot;
    public RawImage fullShotImage;
    public RawImage curShot;
    //public GameObject cloneTarget;
    //public GameObject LibraryDetailListContainer;
    public VideoPlayer videoPlayer;

    card_define curData;
    user_card cursor;

    Coroutine videoPlayChecker = null;
    private void OnDisable()
    {
        CancelInvoke("InitCusor"); 
    }
    public void ShowLibraryDetail(card_define data)
    {
        cardCanvas.OnManageStateChange(4);

        SetLibraryDetailUI(data);
    }

    public void SetLibraryDetailUI(card_define _data = null)
    {
        if(videoPlayChecker != null)
        {
            StopCoroutine(videoPlayChecker);
            videoPlayChecker = null;
        }
        //foreach (Transform listItem in LibraryDetailListContainer.transform)
        //{
        //    if (cloneTarget.transform != listItem)
        //    {
        //        Destroy(listItem.gameObject);
        //    }
        //}

        //cloneTarget.SetActive(true);

        if (_data != null)
            curData = _data;
        List<game_data> userData_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_CARD);

        object obj;
        uint curCardID = 0;
        if (curData.data.TryGetValue("card_id", out obj))
        {
            curCardID = (uint)obj;
        }
        
        cursor = null;
        foreach (game_data data in userData_list)
        {
            if (data.data.TryGetValue("card_id", out obj))
            {
                if (curCardID == (uint)obj)
                {
                    //GameObject listItem = Instantiate(cloneTarget);
                    //listItem.transform.SetParent(LibraryDetailListContainer.transform);
                    //RectTransform rt = listItem.GetComponent<RectTransform>();
                    //rt.localPosition = Vector3.zero;
                    //rt.localScale = Vector3.one;

                    //CardLibraryItem component = listItem.GetComponent<CardLibraryItem>();
                    //if (component)
                    //{
                    //    component.SetLibraryDetailItem(curData, (user_card)data, this);
                    //    if (cursor == null)
                    //    {
                    //        cursor = (user_card)data;
                    //    }
                    //}

                    if (cursor == null)
                    {
                        cursor = (user_card)data;
                    }
                }
            }
        }

        //cloneTarget.SetActive(false);

        videoPlayer.Stop();
        VideoClip preClip = videoPlayer.clip;
        Resources.UnloadAsset(preClip);

        RenderTexture target = videoPlayer.targetTexture;
        if (target)
        {
            RenderTexture rt = RenderTexture.active;
            RenderTexture.active = target;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = rt;
        }

        string path = curData.GetResourcePath();
        uint type = curData.GetResourceType();

        switch (type)
        {
            case 1:
                VideoClip clip = Resources.Load<VideoClip>(path);
                if (clip)
                {
                    videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
                    videoPlayer.clip = clip;
                    videoPlayer.isLooping = true;
                    fullShot.texture = videoPlayer.targetTexture;
                    curShot.texture = videoPlayer.targetTexture;                    
                    videoPlayer.enabled = true;

                    videoPlayChecker = cardCanvas.StartCoroutine(LoadCheckPlayVideo());
                }
                break;
            case 0:
            default:
                videoPlayer.Stop();
                videoPlayer.enabled = false;
                Sprite sprite = Resources.Load<Sprite>(path);
                fullShot.UseNormalTextureBlur(sprite);
                curShot.texture = sprite.texture;                
                break;
        }

        InitCusor();
    }

    public IEnumerator LoadCheckPlayVideo()
    {
        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared)
        {
            yield return new WaitForEndOfFrame();
        }

        videoPlayer.Play();
    }

    public void InitCusor()
    {
        CancelInvoke("InitCusor");
        OnCardLibraryDetail(cursor);
    }

    public void SetCurosr(user_card target)
    {
        OnCardLibraryDetail(target);
    }

    public user_card GetCursor()
    {
        return cursor;
    }

    public void OnCardLibraryDetail(user_card target)
    {
        cursor = target;

        //foreach (Transform listItem in LibraryDetailListContainer.transform)
        //{
        //    CardLibraryItem component = listItem.GetComponent<CardLibraryItem>();
        //    if (component)
        //    {
        //        component.RefreshCursor(target);
        //    }
        //}

        curShot.gameObject.SetActive(cursor != null);

        if(cursor == null)
        {
            if (curData.GetResourceType() == 1)
            {
                curShot.texture = videoPlayer.texture;
                curShot.gameObject.SetActive(true);
            }
            else
            {
                object obj;
                uint cardID = 0;
                if (curData.data.TryGetValue("card_id", out obj))
                {
                    cardID = (uint)obj;
                }
                List<game_data> album = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.ALBUM);
                foreach (game_data uc in album)
                {
                    if (uc.data.TryGetValue("card_id", out obj))
                    {
                        if (cardID == (uint)obj)
                        {
                            if (uc.data.TryGetValue("flag", out obj))
                            {
                                int state = int.Parse(((uint)obj).ToString());
                                if ((state & 2) != 0)
                                {
                                    curShot.gameObject.SetActive(true);
                                    Rect u = new Rect(Vector2.zero, Vector2.one);
                                    curShot.uvRect = u;
                                    RectTransform r = curShot.transform as RectTransform;
                                    Vector2 s = (fullShot.transform as RectTransform).rect.size;
                                    Vector2 d = s;
                                    d.x = s.x * u.width;
                                    d.y = s.y * u.height;
                                    r.sizeDelta = d;

                                    r.localPosition = new Vector2(s.x * u.x - (s.x * 0.5f), (s.y * -1.0f) + (s.y * (u.y + u.height)));
                                }
                            }
                        }
                    }
                }
            }
            return;
        }
        else if (curData.GetResourceType() == 1)
        {
            curShot.texture = videoPlayer.targetTexture;
        }
        
        Rect uv = target.GetUVRect();
        
        curShot.uvRect = uv;
        RectTransform rt = curShot.transform as RectTransform;
        Vector2 size = (fullShot.transform as RectTransform).rect.size;
        Vector2 delta = size;
        delta.x = size.x * uv.width;
        delta.y = size.y * uv.height;
        rt.sizeDelta = delta;

        rt.localPosition = new Vector2(size.x * uv.x - (size.x * 0.5f), (size.y * -1.0f) + (size.y * (uv.y + uv.height)));
    }

    public void OnShowCardDetail()
    {
        if (curData == null)
            return;

        List<game_data> userCards = new List<game_data>();

        uint curCardID = curData.GetCardID();
        List<game_data> userData_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_CARD);
        foreach (user_card data in userData_list)
        {
            if (data.GetCardID() == curCardID)
                userCards.Add(data);
        }

        detailPanel.OnShow(cursor, userCards, false);
    }

    public void SetPanelOnData()
    {
        curShot.gameObject.SetActive(true);
    }

    public void SetPanelOffData()
    {
        cursor = null;

        fullShotImage.texture = null;
        curShot.texture = null;
        curShot.uvRect = Rect.zero;
        curShot.gameObject.SetActive(false);
    }
}

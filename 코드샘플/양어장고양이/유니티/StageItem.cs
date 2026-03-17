using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageItem : MonoBehaviour
{
    public delegate void Callback(stage stageData);

    private stage curStageData;
    private Callback callback;

    public Image ThumnailImage;

    public GameObject[] StarPenal;
    public GameObject[] StarIcon;

    public GameObject cursor;
    public GameObject GiftBox;
    public GameObject CoverObject;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public StageItem CloneItem(Transform parent, stage data)
    {
        gameObject.SetActive(true);

        GameObject listItem = Instantiate(gameObject);
        listItem.transform.SetParent(parent);
        RectTransform rt = listItem.GetComponent<RectTransform>();
        rt.localPosition = Vector3.zero;
        rt.localScale = Vector3.one;

        gameObject.SetActive(false);

        StageItem ret = listItem.GetComponent<StageItem>();
        ret.SetUI(data);

        return ret;
    }

    public void SetUI(stage data)
    {
        curStageData = data;

        int curState = curStageData.GetCurStageState();
        if (curState < 0)
        {
            CoverObject.SetActive(true);
            GiftBox.SetActive(false);
            ThumnailImage.gameObject.SetActive(false);

            for (int i = 0; i < StarPenal.Length; i++)
            {
                StarPenal[i].SetActive(false);
            }
        }
        else
        {
            uint curStar = curStageData.GetCurStar();
            ThumnailImage.sprite = Resources.Load<Sprite>(curStageData.GetThumbnailPath());
            uint maxStar = curStageData.GetMaxStar();
            for (int i = 0; i < StarPenal.Length; i++)
            {
                StarPenal[i].SetActive(i < maxStar);
            }

            for (int i = 0; i < StarIcon.Length; i++)
            {
                StarIcon[i].SetActive(i < curStar);
            }

            CoverObject.SetActive(false);

            GiftBox.SetActive(maxStar == curState);
        }

        SetCursor(false);        
    }

    public stage GetData() { return curStageData; }

    public void OnSelectItem()
    {
        if(CoverObject.activeSelf)
        {
            WorldMapUI worldUI = transform.GetComponentInParent<WorldMapUI>();
            if (worldUI != null)
            {
                worldUI.worldCanvas.GameManager.PopupControl.OnPopupToast("고양이를 찾아보세요.");
            }
            return;
        }
        else if(GiftBox.activeSelf)
        {
            WWWForm data = new WWWForm();
            data.AddField("api", "adventure");
            data.AddField("op", 7);
            data.AddField("stage", curStageData.GetStageID().ToString());

            NetworkManager.GetInstance().SendApiRequest("adventure", 7, data, (res)=> {
                WorldCanvas canvas = GetComponentInParent<WorldCanvas>();
                canvas.ResponseEventData(res);
                
                GiftBox.SetActive(false);
            });

            return;
        }

        callback?.Invoke(curStageData);
    }

    public void SetButtonCallback(Callback cb)
    {
        callback = cb;
    }

    public void SetCursor(bool bCursor)
    {
        cursor.SetActive(bCursor);
    }

    public bool IsCursor()
    {
        return cursor.activeSelf;
    }
}

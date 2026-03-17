using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SamandaButton : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public GameObject ShortcutGroup;
    public GameObject[] ShortcutBtn;
    public Button BtnSelf;

    Color iconColor = new Color(0.0627450980392157f, 0.1490196078431373f, 0.2235294117647059f);
    bool tweenRunning = false;
    float fDragedWaitTime = 0.0f;
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    public void SamandaDestroy()
    {
        Destroy(this.gameObject);
    }

    void OnEnable()
    {
        Vector3 pos = new Vector3(100, 100, 0);
        transform.position = pos;

        //transform.position = new Vector3(pos.x, pos.y - 150, pos.z);
        //iTween.MoveTo(gameObject, iTween.Hash("position", pos, "easetype", iTween.EaseType.easeOutBack, "time", 0.5f));

        UnityEngine.UI.Button ButtonCoponunt = this.gameObject.GetComponent<UnityEngine.UI.Button>();
        ButtonCoponunt.onClick.AddListener(() => onShortCutMenu());

        ShortCutHideActionDone();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
        //gameObject.transform.Find("icon").GetComponent<RectTransform>().localScale = Vector3.one;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        gameObject.GetComponent<RectTransform>().localScale = Vector3.one * 0.5f;
        //gameObject.transform.Find("icon").GetComponent<RectTransform>().localScale = Vector3.one * 0.5f;

        if (fDragedWaitTime > 0)
            fDragedWaitTime = 0.5f;
    }

    public void OnDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (BtnSelf.interactable == false)
            return;

        if(Mathf.Abs(Vector2.Distance(eventData.position, transform.position)) < 10.0f)
        {
            return;
        }

        fDragedWaitTime = 0.5f;
        transform.position = eventData.position;
    }

    void onShortCutMenu()
    {
        if (tweenRunning)
            return;

        if(fDragedWaitTime > 0.0f)
        {            
            return;
        }

        BtnSelf.interactable = false;
        ShortcutGroup.SetActive(true);
        gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
        gameObject.transform.Find("icon").GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

        float ratio = gameObject.transform.localPosition.x > 0 ? -1.0f : 1.0f;
        int count = 0;
        foreach (GameObject obj in ShortcutBtn)
        {
            obj.GetComponent<RectTransform>().sizeDelta = new Vector3(0.0f, 0.0f, 0.0f);
            obj.transform.Find("icon").GetComponent<Image>().color = iconColor * 0.0f;

            obj.GetComponent<RectTransform>().localPosition = new Vector2(120.0f * count * ratio, 0.0f);
            //obj.GetComponent<RectTransform>().localPosition = new Vector2(0.0f, 0.0f);
            //iTween.Stop(obj);
            //iTween.MoveTo(obj, iTween.Hash("x", 120.0f * count * ratio, "delay", 0.5f, "time", 0.5f * ((float)count/ ShortcutBtn.Length), "islocal", true, "easetype", iTween.EaseType.easeInQuad));            
            count++;
        }

        //iTween.Stop(gameObject);
        //iTween.ValueTo(gameObject, iTween.Hash("from", 0.0f, "to", 2.0f, "easetype", iTween.EaseType.easeInQuad, "time", 0.5f, "onupdate", "updateSizeBtn", "onupdatetarget", this.gameObject, "oncomplete", "ShortCutShowActionDone", "oncompletetarget", this.gameObject));

        ShortCutShowActionDone();
        tweenRunning = true;
    }

    public void updateSizeBtn(float val)
    {
        if (val < 1.0f)
        {
            gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(100 * (1 - val), 100 * (1 - val));
            gameObject.transform.Find("icon").GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, (1.0f - val));
            return;
        }

        gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
        gameObject.transform.Find("icon").GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 0.0f);

        val -= 1.0f;
        int count = 0;
        foreach (GameObject obj in ShortcutBtn)
        {
            obj.transform.Find("icon").GetComponent<Image>().color = (count == 0 ? new Color(1.0f, 1.0f, 1.0f, 1.0f) : iconColor) * val;
            obj.transform.Find("icon").GetComponent<RectTransform>().localScale = Vector3.one * val;
            obj.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100) * val;
            count++;
        }
    }

    public void ShortCutShowActionDone()
    {
        if (tweenRunning)
            return;

        tweenRunning = false;
        gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
        gameObject.transform.Find("icon").GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 0.0f);

        float ratio = gameObject.transform.localPosition.x > 0 ? -1.0f : 1.0f;
        int count = 0;
        foreach(GameObject obj in ShortcutBtn)
        {
            obj.transform.Find("icon").GetComponent<Image>().color = count == 0 ? new Color(1.0f, 1.0f, 1.0f, 1.0f) : iconColor;
            obj.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
            obj.GetComponent<RectTransform>().localPosition = new Vector2(120 * count * ratio, 0.0f);            
            count++;
        }
        tweenRunning = true;
    }

    void ResetButton()
    {
        BtnSelf.interactable = false;
        ShortcutGroup.SetActive(true);
        gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
        gameObject.transform.Find("icon").GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 0.0f);

        float ratio = gameObject.transform.localPosition.x > 0 ? -1.0f : 1.0f;
        int count = 0;
        foreach (GameObject obj in ShortcutBtn)
        {
            obj.transform.Find("icon").GetComponent<Image>().color = count == 0 ? new Color(1.0f, 1.0f, 1.0f, 1.0f) : iconColor;
            obj.GetComponent<RectTransform>().sizeDelta = new Vector3(100.0f, 100.0f, 0.0f);            
            obj.GetComponent<RectTransform>().localPosition = new Vector2(0, 0.0f);

            //obj.GetComponent<RectTransform>().localPosition = new Vector2(120 * count * ratio, 0.0f);
            //iTween.Stop(obj);
            //iTween.MoveTo(obj, iTween.Hash("x", 0, "time", 0.5f * ((float)count / ShortcutBtn.Length), "islocal", true, "easetype", iTween.EaseType.easeInQuad));
            count++;
        }

        //iTween.Stop(gameObject);
        //iTween.ValueTo(gameObject, iTween.Hash("from", 2.0f, "to", 0.0f, "easetype", iTween.EaseType.easeInQuad, "time", 0.5f, "onupdate", "updateSizeBtn", "onupdatetarget", this.gameObject, "oncomplete", "ShortCutHideActionDone", "oncompletetarget", this.gameObject));

        ShortCutHideActionDone();
    }

    public void ShortCutHideActionDone()
    {
        tweenRunning = false;
        BtnSelf.interactable = true;
        ShortcutGroup.SetActive(false);
        gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
        gameObject.transform.Find("icon").GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

        foreach (GameObject obj in ShortcutBtn)
        {
            obj.GetComponent<RectTransform>().sizeDelta = new Vector3(0.0f, 0.0f, 0.0f);
            obj.GetComponent<RectTransform>().localPosition = new Vector2(0.0f, 0.0f);
            obj.transform.Find("icon").GetComponent<Image>().color = iconColor * 0.0f;
        }
    }


    public void OnShortCutBtn(int type)
    {
        if (type < 0)
        {
            ResetButton();
            return;
        }
                
        SamandaLauncher.OnSamandaShortcut((SamandaLauncher.Samanda_Shorcut)type);
    }
        
    void Update()
    {
        if (fDragedWaitTime > 0.0f)
            fDragedWaitTime -= Time.deltaTime;
    }
}

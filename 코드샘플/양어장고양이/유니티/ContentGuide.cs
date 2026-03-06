using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Button;

public class ContentGuide : MonoBehaviour
{
    public static ContentGuide curGuideUI = null;

    public CutoutMaskUI MaskBackground;
    public Image HighlightCursor;
    public Button CloseButton;

    public GameObject GuideTextBox;
    public Text GuideText;
    public int ContentGuideID = 0;    
    public RectTransform TargetRectTransform;

    bool bAnimationDone = false;
    ButtonClickedEvent buttonClickEvent = null;

    private int curIndex = 0;
    private string[] GuideMsg;
    private float bgAlphaValue = 0;
    private float bgAlphaMaxValue = 0.0f;
    private Coroutine coroutineTextBoxAnimation = null;
    private Sprite targetSprite = null;
    private string[] guideMsg;
    private RectTransform targetRectTransform = null;
    private Button targetBtn = null;
    public void SetGuide(int contentGuideID, string[] _GuideMsg, RectTransform targetRectTransfrom, Sprite TargetSprite = null, Button btn = null, float delay = 0.0f)
    {
        if(curGuideUI != null)
        {
            curGuideUI.ForceGuideDestroy();
            Destroy(curGuideUI.gameObject);
        }

        curGuideUI = this;
        ContentGuideID = contentGuideID;
        targetSprite = TargetSprite;
        guideMsg = _GuideMsg;
        targetRectTransform = targetRectTransfrom;
        targetBtn = btn;

        HighlightCursor.gameObject.SetActive(false);
        Invoke("RunGuide", delay);
    }

    void RunGuide()
    {
        if (ContentGuideID == ContentLocker.ContentGuideDoneSeq - 1)
        {
            GameObject[] roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject root in roots)
            {
                if (root.GetComponent<Canvas>() != null)
                {
                    if (root.name == "Game_Canvas")
                    {
                        GameCanvas gameCanvas = root.GetComponent<GameCanvas>();
                        if (gameCanvas != null)
                        {
                            gameCanvas.OnHideUI();
                            ContentIntroducer introducer = gameCanvas.EmptyUI.GetComponent<ContentIntroducer>();
                            introducer.ContentGuideEffectAudioPlay();
                        }
                    }
                }
            }
        }
        
        curIndex = 0;

        HighlightCursor.sprite = targetSprite;

        Canvas canvas = GetComponent<Canvas>();

        Camera[] cams = Camera.allCameras;
        foreach (Camera cam in cams)
        {
            if (cam.transform.name == "UICamera")
                canvas.worldCamera = cam;
        }
        GuideMsg = guideMsg;
        if (GuideMsg.Length > 0)
            GuideText.text = GuideMsg[0].Replace("\\n", "\n");
        GuideTextBox.gameObject.SetActive(false);
        TargetRectTransform = targetRectTransform;

        GuideUpdate();

        bAnimationDone = false;

        if (targetBtn != null)
        {
            SetButtonClickEvent(targetBtn.onClick);
        }


        if (targetSprite != null && targetRectTransform != null)
        {
            Image image = targetRectTransform.GetComponent<Image>();
            if (image != null)
            {
                HighlightCursor.type = image.type;
                HighlightCursor.fillAmount = image.fillAmount;
                HighlightCursor.pixelsPerUnitMultiplier = image.pixelsPerUnitMultiplier;
            }
        }


        Invoke("AnimationDone", 0.5f);

        bgAlphaValue = 0;
        bgAlphaMaxValue = targetSprite == null && targetRectTransform == null ? 0.0f : 0.8235294117647059f;
        MaskBackground.color = new Color(0, 0, 0, bgAlphaValue);

        StartCoroutine(BackgroundAlpha(true));
    }
    public void SetButtonClickEvent(ButtonClickedEvent evt)
    {
        buttonClickEvent = evt;        
    }

    public void Update()
    {
        GuideUpdate();
    }

    public void AnimationDone()
    {
        HighlightCursor.gameObject.SetActive(true);

        CloseButton.interactable = false;

        if (targetRectTransform != null)
        {
            DOTweenAnimation ani = targetRectTransform.GetComponent<DOTweenAnimation>();
            if (ani)
            {
                foreach (Tween tween in ani.GetTweens())
                {
                    if(!tween.IsComplete())
                    {
                        Invoke("AnimationDone", 0.1f);
                        return;
                    }
                }
            }
            
            DOTweenAnimation[] anims = targetRectTransform.GetComponentsInParent<DOTweenAnimation>();            
            foreach (DOTweenAnimation anim in anims)
            {
                foreach (Tween tween in anim.GetTweens())
                {
                    if (!tween.IsComplete())
                    {
                        Invoke("AnimationDone", 0.1f);
                        return;
                    }
                }
            }
        }

        bAnimationDone = true;
        if (GuideMsg.Length > 0)
            GuideTextBox.gameObject.SetActive(true);

        RectTransform highlightTransform = HighlightCursor.GetComponent<RectTransform>();
        RectTransform guideBoxTransform = GuideTextBox.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(guideBoxTransform);

        if (TargetRectTransform != null)
        {
            Vector2 targetPos = highlightTransform.localPosition;
            Vector2 normalPos = highlightTransform.localPosition.normalized;
            
            Vector2 pivot = highlightTransform.pivot;
            pivot.x = pivot.x - 0.5f;
            targetPos.x += (highlightTransform.sizeDelta.x * pivot.x);

            if (normalPos.y <= 0)
            {
                pivot.y = 1.0f - pivot.y;
                targetPos.y += 50;
                targetPos.y += (highlightTransform.sizeDelta.y * pivot.y);                
                targetPos.y += (guideBoxTransform.sizeDelta.y * 0.5f);
            }
            else
            {
                pivot.y = pivot.y * -1.0f;
                targetPos.y -= 50;
                targetPos.y += (highlightTransform.sizeDelta.y * pivot.y);
                targetPos.y += (guideBoxTransform.sizeDelta.y * -0.5f);
            }

            RectTransform curRT = GetComponent<RectTransform>();
            if (targetPos.x - (guideBoxTransform.sizeDelta.x * 0.5f) - 10 < curRT.sizeDelta.x * -0.5f)
            {
                targetPos.x = (curRT.sizeDelta.x * -0.5f) + (guideBoxTransform.sizeDelta.x * 0.5f) + 10;
            }
            if (targetPos.x + (guideBoxTransform.sizeDelta.x * 0.5f) + 10 > curRT.sizeDelta.x * 0.5f)
            {
                targetPos.x = (curRT.sizeDelta.x * 0.5f) - (guideBoxTransform.sizeDelta.x * 0.5f) - 10;
            }

            guideBoxTransform.localPosition = targetPos;
        }
        else
        {
            guideBoxTransform.localPosition = new Vector2(0, 720/5);
        }


        if (coroutineTextBoxAnimation != null)
            StopCoroutine(coroutineTextBoxAnimation);

        coroutineTextBoxAnimation = StartCoroutine(TextBoxAnimation());
    }

    IEnumerator TextBoxAnimation()
    {
        RectTransform textRectTransform = GuideTextBox.GetComponent<RectTransform>();
        Vector3 goal = textRectTransform.localPosition;
        //textRectTransform.localPosition = HighlightCursor.transform.localPosition;
        //textRectTransform.localPosition += (goal - HighlightCursor.transform.localPosition).normalized * 30.0f;

        Text txt = GuideText.GetComponent<Text>();
        Image target = GuideTextBox.GetComponent<Image>();

        Color boxColor= target.color;
        boxColor.a = 0;
        target.color = boxColor;

        Color txtColor = txt.color;
        txtColor.a = 0;
        txt.color = txtColor;
        
        float time = 0.0f;

        textRectTransform.localScale = Vector3.one * 0.5f;
        Vector3 diff = goal - textRectTransform.localPosition;
        while(time <= 0.5f)
        {
            float curDelta = Time.deltaTime;
            time += curDelta;
            textRectTransform.localPosition += diff * curDelta * 2.0f;
            textRectTransform.localScale += Vector3.one * 0.5f * curDelta * 2.0f;
            
            boxColor = target.color;
            boxColor.a = boxColor.a + curDelta * 2.0f;
            target.color = boxColor;

            if (time > 0.25f)
            {
                txtColor = txt.color;
                txtColor.a = txtColor.a + curDelta * 4.0f;
                txt.color = txtColor;
            }

            yield return new WaitForEndOfFrame();
        }

        boxColor.a = 1.0f;
        target.color = boxColor;
        txtColor.a = 1.0f;
        txt.color = txtColor;
        textRectTransform.localPosition = goal;


        CloseButton.interactable = true;
    }

    void GuideUpdate()
    {
        RectTransform curRT = GetComponent<RectTransform>();
        RectTransform hightlightTransform = HighlightCursor.GetComponent<RectTransform>();
        RectTransform backgroundRT = MaskBackground.GetComponent<RectTransform>();

        if (TargetRectTransform != null)
        {
            hightlightTransform.sizeDelta = TargetRectTransform.rect.size;
            hightlightTransform.pivot = TargetRectTransform.pivot;
            hightlightTransform.position = TargetRectTransform.position;
        }
        else
        {
            hightlightTransform.position = Vector3.zero;
        }


        backgroundRT.sizeDelta = curRT.sizeDelta;
        backgroundRT.position = curRT.position;        
    }

    public void OnGuideTouch()
    {
        if (!bAnimationDone)
            return;

        curIndex++;
        if(curIndex < GuideMsg.Length)
        {
            GuideText.text = GuideMsg[curIndex].Replace("\\n", "\n");

            RectTransform curRT = GetComponent<RectTransform>();
            RectTransform guideBoxTransform = GuideTextBox.GetComponent<RectTransform>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(guideBoxTransform);
            Vector2 targetPos = guideBoxTransform.localPosition;
            if (targetPos.x - (guideBoxTransform.sizeDelta.x * 0.5f) - 10 < curRT.sizeDelta.x * -0.5f)
            {
                targetPos.x = (curRT.sizeDelta.x * -0.5f) + (guideBoxTransform.sizeDelta.x * 0.5f) + 10;
                guideBoxTransform.localPosition = targetPos;
            }
            if (targetPos.x + (guideBoxTransform.sizeDelta.x * 0.5f) + 10 > curRT.sizeDelta.x * 0.5f)
            {
                targetPos.x = (curRT.sizeDelta.x * 0.5f) - (guideBoxTransform.sizeDelta.x * 0.5f) - 10;
                guideBoxTransform.localPosition = targetPos;
            }
            if (coroutineTextBoxAnimation != null)
                StopCoroutine(coroutineTextBoxAnimation);

            coroutineTextBoxAnimation = StartCoroutine(TextBoxAnimation());
            return;
        }

        GuideTextBox.gameObject.SetActive(false);
        bAnimationDone = false;
        //PlayerPrefs.SetInt("UnlockEffect", ContentGuideID);


        Color color = HighlightCursor.color;
        color.a = 0.0f;
        HighlightCursor.color = color;

        StartCoroutine(BackgroundAlpha(false));

        Canvas canvas = transform.GetComponentInParent<Canvas>();
        Destroy(canvas.gameObject, 1.0f);
        
        Invoke("RunRegistedCallback", 0.99f);
    }

    IEnumerator BackgroundAlpha(bool fadeIn)
    {
        if (fadeIn)
        {
            while (MaskBackground.color.a < bgAlphaMaxValue)
            {
                bgAlphaValue += Time.deltaTime;
                if (bgAlphaValue > bgAlphaMaxValue)
                    bgAlphaValue = bgAlphaMaxValue;

                Color color = MaskBackground.color;
                color.a = bgAlphaValue;
                MaskBackground.color = color;

                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            while (MaskBackground.color.a > 0)
            {
                bgAlphaValue -= Time.deltaTime;
                if (bgAlphaValue < 0)
                    bgAlphaValue = 0;

                Color color = MaskBackground.color;
                color.a = bgAlphaValue;
                MaskBackground.color = color;

                yield return new WaitForEndOfFrame();
            }
        }
    }

    public void RunRegistedCallback()
    {
        if (buttonClickEvent != null)
        {
            buttonClickEvent.Invoke();
        }

        ContentLocker.SendContentSeq(ContentGuideID + 1);
    }

    void OnDestroy()
    {
        if (curGuideUI == this)
            curGuideUI = null;

        if (ContentGuideID >= (ContentLocker.ContentGuideDoneSeq - 1))
        {
            GameDataManager.Instance.GetUserData().data["contents"] = ContentLocker.ContentGuideDoneSeq;
            GameObject[] roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject root in roots)
            {
                if (root.GetComponent<Canvas>() != null)
                {
                    if (root.name == "Game_Canvas")
                    {
                        GameCanvas gameCanvas = root.GetComponent<GameCanvas>();
                        if (gameCanvas != null)
                        {
                            gameCanvas.OnShowUI();
                        }
                    }
                }
            }
        }
    }

    public void ForceGuideDestroy()
    {
        Debug.Log("ForceGuideDestroy! ShowFlag Clear");

        GameObject[] roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject root in roots)
        {
            if (root.GetComponent<Canvas>() != null)
            {
                ContentLocker[] contentLockers = root.GetComponentsInChildren<ContentLocker>(true);
                foreach (ContentLocker contentLocker in contentLockers)
                {
                    if(contentLocker.ContentID == ContentGuideID)
                    {
                        contentLocker.ClearForce();
                    }
                }
            }
        }
    }
}

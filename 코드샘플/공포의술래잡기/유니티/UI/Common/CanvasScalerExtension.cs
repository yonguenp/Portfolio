using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CanvasScalerExtension : UIBehaviour
{
    [HideInInspector] [SerializeField] CanvasScaler canvas;

    [Header("UI 노치디자인")]
    [SerializeField] bool safeAreaOption = false;

    public RectTransform panel;
    public Rect lastSafeArea = new Rect(0, 0, 0, 0);

    [Header("노치가 적용되야 하는 목록")]
    public RectTransform[] UI;

    [Header("[노치 제외 항목들]")]
    public RectTransform[] ExceptionUI;

    public RectTransform bg_Container;
    //#if !(UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN)
    protected override void OnRectTransformDimensionsChange()
    {
        DynamicUI();
    }
    //#endif

    protected void Update()
    {
        if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft || Input.deviceOrientation == DeviceOrientation.LandscapeRight)
            DynamicUI();
    }

    protected override void Awake()
    {
        canvas = this.GetComponent<CanvasScaler>();

        Init();
        if (UI != null && safeAreaOption)
        {
            for (int i = 0; i < UI.Length; i++)
            {
                panel = UI[i].GetComponent<RectTransform>();
                Refresh();
            }
        }
    }

    public void DynamicUI()
    {
        Init();

        if (UI != null)
        {
            for (int i = 0; i < UI.Length; i++)
            {
                panel = UI[i].GetComponent<RectTransform>();
                Refresh();
            }
        }
        if (bg_Container != null)
        {
            if (bg_Container.childCount == 1)
                bg_Container.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(bg_Container.parent.GetComponent<RectTransform>().rect.width, bg_Container.parent.GetComponent<RectTransform>().rect.height);
        }
    }

    public void Init()
    {
        if (canvas != null)
        {
            var ratio = Screen.safeArea.size.x / Screen.safeArea.size.y;
            // 가로가 더 길다.
            if (ratio >= 16f / 9f)
            {
                // 가로가 더 길면 height에 맞춘다
                canvas.matchWidthOrHeight = 1f;
            }
            else
            {
                // 가로가 더 길면 width에 맞춘다
                canvas.matchWidthOrHeight = 0f;
            }

        }
    }


    public void Refresh()
    {
        Rect safeArea = GetSafeArea();
        ApplySafeArea(safeArea);
        GetSafeAreatoScreen();
    }
    public Rect GetSafeArea()
    {
        Rect safeArea = Screen.safeArea;

        //SBDebug.Log($"OriginArea : {Screen.width}, {Screen.height} /////\nScreen.safeArea : {Screen.safeArea}");

        return safeArea;
    }

    public void ApplySafeArea(Rect r)
    {
        lastSafeArea = r;

        Vector2 anchorMin = r.position;
        Vector2 anchorMax = r.position + r.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;
        panel.anchorMin = anchorMin;
        panel.anchorMax = anchorMax;
    }

    public void GetSafeAreatoScreen()
    {
        if (ExceptionUI.Length <= 0)
            return;

        var v = new Vector2(0.5f, 0.5f);
        foreach (var item in ExceptionUI)
        {
            if (item == null)
                continue;

            item.anchorMin = v;
            item.anchorMax = v;

            item.sizeDelta = new Vector2(Screen.width, Screen.height);
        }
    }
}

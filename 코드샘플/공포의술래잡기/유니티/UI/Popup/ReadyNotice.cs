using UnityEngine;
using UnityEngine.UI;

public class ReadyNotice : Popup
{
    public float LimitTime { get; set; } = 0;
    public Vector2 MoveDir { get; set; } = Vector2.zero; //초당 몇움직일지 
    public bool IsFade { get; set; } = false;
    public float Delay { get; set; } = 0;       //움직임 페이드 딜레이

    bool _isPlay = false;
    float _playTime = 0;

    [SerializeField]
    public Text Text;
    [SerializeField]
    public Image BG;

    public override void Open(CloseCallback cb = null)
    {
        base.Open(cb);
        Init();
    }
    public void Init()
    {
        Text.text = StringManager.GetString("ui_get_ready");
        MoveDir = new Vector2(0, 0);
        LimitTime = 2;
        IsFade = true;
        Delay = 0.5f;
        transform.localPosition = new Vector3(0, 0);
        Text.color = Color.white;
        BG.color = Color.white;

        Play();
    }

    public void Play()
    {
        this.gameObject.SetActive(true);
        _playTime = 0;
        _isPlay = true;

        var textColor = Text.color;
        var bgColor = BG.color;

        textColor.a = 1;
        Text.color = textColor;

        bgColor.a = 1;
        BG.color = bgColor;
    }

    // Update is called once per frame
    void Update()
    {
        if (_isPlay == false) return;

        if (LimitTime < _playTime)
        {
            PopupCanvas.Instance.ClosePopup(PopupCanvas.POPUP_TYPE.DEVLOPING_NOTICE_POPUP);
            return;
        }

        float delta = Time.deltaTime;

        float actionTime = _playTime - Delay;
        if (actionTime > 0)
        {
            var nextPos = new Vector3(MoveDir.x * delta, MoveDir.y * delta) + transform.position;
            transform.position = nextPos;
            if (IsFade)
            {
                var textColor = Text.color;
                var bgColor = BG.color;

                float LimitFadeTime = LimitTime - Delay;

                float alpha = LimitFadeTime - (actionTime * LimitFadeTime);
                textColor.a = alpha;
                Text.color = textColor;

                bgColor.a = alpha;
                BG.color = bgColor;
            }
        }

        _playTime += delta;
    }
}

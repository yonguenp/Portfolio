using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

public class FishingGame : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public RectTransform SuccessGuageBack;
    public RectTransform SuccessGuage;

    public RectTransform FishingGuageBack;
    public RectTransform FishingGuage;
    public RectTransform UserGuage;
    
    public RectTransform RightHand;
    public RectTransform Line;
    public RectTransform[] Fish;

    public RectTransform[] GameUI;

    private bool touching = false;
    private float deltaPos = 0.0f;
    private float moveRate = 0.0f;

    private float maxY;
    private float minY;
    private float successGuageMax = 0.0f;

    enum STATE { 
        START,
        PLAY,
        OVER,
        CLEAR,
    };
    STATE curState = STATE.START;
    private void Update()
    {
        if (curState != STATE.PLAY)
            return;

        if(touching)
        {
            deltaPos += Time.deltaTime * moveRate;
        }
        else
        {
            deltaPos -= Time.deltaTime * moveRate;
        }

        Vector2 pos = UserGuage.localPosition;
        pos.y = pos.y + deltaPos;
        if(pos.y > maxY)
        {
            pos.y = maxY;
            deltaPos = 0;
        }
        else if(pos.y < minY)
        {
            pos.y = minY;
            deltaPos = 0;
        }

        UserGuage.localPosition = pos;

        Vector2 size = SuccessGuage.sizeDelta;

        if (FishingGuage.localPosition.y < UserGuage.localPosition.y + UserGuage.sizeDelta.y
            && FishingGuage.localPosition.y > UserGuage.localPosition.y - UserGuage.sizeDelta.y)
        {            
            size.x += 1.0f;
            if (size.x > successGuageMax)
            {
                size.x = successGuageMax;
                SetUI(STATE.CLEAR);
            }
        }
        else
        {
            size.x -= 1.0f;
            if (size.x < 0)
            {
                size.x = 0;
                SetUI(STATE.OVER);
            }
        }
        SuccessGuage.sizeDelta = size;
    }

    private void Start()
    {
        RectTransform curRT = transform as RectTransform;
        float halfWidth = curRT.sizeDelta.x * 0.5f;
        float halfHeight = curRT.sizeDelta.y * 0.5f;

        foreach (RectTransform fish in Fish)
        {
            fish.localPosition = new Vector2(Random.Range(halfWidth * -1.0f, halfWidth), Random.Range(halfHeight * -1.0f, halfHeight));
            fish.DOLocalRotate(new Vector3(0, 0, Random.Range(0.0f, 360.0f)), Random.Range(5.0f, 10.0f));
            fish.DOLocalMove(new Vector3(Random.Range(halfWidth * -1.0f, halfWidth), Random.Range(halfHeight * -1.0f, halfHeight)), Random.Range(5.0f, 10.0f)).OnComplete(FishMoveDone);
        }

        SetUI(STATE.START);
    }
    void FishMoveDone()
    {
        RectTransform curRT = transform as RectTransform;
        float halfWidth = curRT.sizeDelta.x * 0.5f;
        float halfHeight = curRT.sizeDelta.y * 0.5f;

        foreach (RectTransform f in Fish)
        {
            if (!DOTween.IsTweening(f))
            {
                f.DOLocalRotate(new Vector3(0, 0, Random.Range(0.0f, 360.0f)), Random.Range(5.0f, 10.0f));
                f.DOLocalMove(new Vector3(Random.Range(halfWidth * -1.0f, halfWidth), Random.Range(halfHeight * -1.0f, halfHeight)), Random.Range(5.0f, 10.0f)).OnComplete(FishMoveDone);
            }
        }
    }
    public void OnUIButton(int index)
    {
        switch(index)
        {
            case 0://start
                SetUI(STATE.PLAY);
                GameStart();
                break;
            case 2:
            case 3:
                SetUI(STATE.START);
                break;
        }
    }
    void SetUI(STATE state)
    {
        curState = state;

        foreach (RectTransform ui in GameUI)
        {
            ui.gameObject.SetActive(false);
        }

        GameUI[(int)state].gameObject.SetActive(true);

        FishingGuage.DOKill();
        RightHand.DOKill();       
        Line.DOKill();

        Vector2 pos = RightHand.localPosition;
        pos.y = -40;
        RightHand.localPosition = pos;
    }

    void GameStart()
    {
        successGuageMax = SuccessGuageBack.sizeDelta.x;
        Vector2 size = SuccessGuage.sizeDelta;
        size.x = successGuageMax * 0.3f;
        SuccessGuage.sizeDelta = size;
        moveRate = 10.0f;

        maxY = (FishingGuageBack.sizeDelta.y * 0.5f) - (FishingGuage.sizeDelta.y * 0.5f);
        minY = (FishingGuageBack.sizeDelta.y * -0.5f) + (FishingGuage.sizeDelta.y * 0.5f);

        touching = false;
        deltaPos = 0.0f;

        MoveAnimation();
    }



    private void MoveAnimation()
    {
        if (curState != STATE.PLAY)
            return;

        float val = Random.Range(minY, maxY);
        float time = 1.0f + (Random.value * 5.0f);
        FishingGuage.DOLocalMoveY(val, time).OnComplete(MoveAnimation);
        Vector2 size = Line.sizeDelta;
        size.y = 450 + val;
        Line.DOSizeDelta(size, time);
    }

    public void OnPointerDown(PointerEventData data)
    {
        touching = true;

        RightHand.DOLocalMoveY(-30, 0.25f).SetLoops(-1, LoopType.Yoyo);
    }

    public void OnPointerUp(PointerEventData data)
    {
        touching = false;

        RightHand.DOKill();
        Vector2 pos = RightHand.localPosition;
        pos.y = -40;
        RightHand.localPosition = pos;
    }
}

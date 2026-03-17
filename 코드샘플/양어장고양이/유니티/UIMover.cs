using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMover : MonoBehaviour
{
    Vector3 MoveDir = Vector3.zero;
    float MoveSpeed = 0.0f;
    Rect LocalPositionBoundary = Rect.zero;
    bool isPlay = false;
    Vector3 OriginalPosition = Vector3.zero;

    Transform originalParent = null;
    int originalSibling = 0;

    Coroutine AnimationCoroutine = null;
    void Start()
    {
        
    }

    void ResetMover()
    {
        MoveDir.x = Random.value - 0.5f;
        MoveDir.y = Random.value - 0.5f;

        MoveDir = MoveDir.normalized;
        MoveSpeed = 0.1f + (Random.value * 1.0f);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    public IEnumerator PlayFreeMoveAnimation()
    {
        bool slicedImage = false;
        Image image = GetComponent<Image>();
        if(image != null)
        {
            slicedImage = image.type == Image.Type.Sliced;            
        }

        while (true)
        { 
            if(slicedImage)
            {
                Vector2 scale = GetComponent<RectTransform>().localScale;
                if(scale.x > 0.1f)
                {
                    scale.x *= 0.9f * Random.value;
                }
                if (scale.y > 0.1f)
                {
                    scale.y *= 0.9f * Random.value;
                }

                if (scale.x < 0.1f && scale.y < 0.1f)
                    slicedImage = false;
            }

            transform.localPosition = (MoveDir * MoveSpeed) + transform.localPosition;
            if (!LocalPositionBoundary.Contains(transform.localPosition))
            {
                ResetMover();
                transform.localPosition = new Vector2(Mathf.Min(LocalPositionBoundary.width, Mathf.Max(LocalPositionBoundary.x, transform.localPosition.x)), Mathf.Min(LocalPositionBoundary.height, Mathf.Max(LocalPositionBoundary.y, transform.localPosition.y)));
            }

            yield return new WaitForEndOfFrame();
        }
    }

    public IEnumerator PlayOrignPosMoveAnimation()
    {
        float dist = Vector3.Distance(transform.localPosition, OriginalPosition);
        Vector3 dir = (OriginalPosition - transform.localPosition).normalized;
        float MoveSpeed = 0.3f;

        while (dist <= 1.0f)
        {
            transform.localPosition += dir * MoveSpeed;

            yield return new WaitForEndOfFrame();
        }

        transform.parent = originalParent;
        transform.SetSiblingIndex(originalSibling);

        transform.localPosition = OriginalPosition;
    }

    [ContextMenu("UIMOVE TOGGLE")]
    public void UIMoveToggle()
    {
        GameObject[] roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject root in roots)
        {
            if (root.GetComponent<Canvas>() != null)
            {
                UIMover[] movers = root.GetComponentsInChildren<UIMover>(true);
                foreach (UIMover mover in movers)
                {
                    if (mover.gameObject.activeInHierarchy)
                    {
                        if (mover.isPlay)
                            mover.OnStopUIMove();
                        else
                            mover.OnPlayUIMove();
                    }
                }
            }
        }
    }

    public void OnPlayUIMove()
    {
        if (isPlay)
            return;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            return;

        originalParent = gameObject.transform.parent;
        originalSibling = gameObject.transform.GetSiblingIndex();

        LocalPositionBoundary.size = canvas.GetComponent<RectTransform>().sizeDelta * 0.5f;
        LocalPositionBoundary.position = canvas.GetComponent<RectTransform>().sizeDelta * -0.5f;

        ResetMover();

        LayoutGroup layOutGroup = transform.parent.GetComponent<LayoutGroup>();
        if (layOutGroup)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layOutGroup.GetComponent<RectTransform>());
            layOutGroup.enabled = false;
        }

        isPlay = true;

        OriginalPosition = transform.localPosition;

        if (AnimationCoroutine != null)
            StopCoroutine(AnimationCoroutine);

        AnimationCoroutine = StartCoroutine(PlayFreeMoveAnimation());

        transform.parent = canvas.transform;
    }

    public void OnStopUIMove()
    {
        if (!isPlay)
            return;
        
        LayoutGroup layOutGroup = transform.parent.GetComponent<LayoutGroup>();
        if (layOutGroup)
        {
            layOutGroup.enabled = true;
        }

        isPlay = false;

        if (AnimationCoroutine != null)
            StopCoroutine(AnimationCoroutine);

        AnimationCoroutine = StartCoroutine(PlayOrignPosMoveAnimation());
    }
}

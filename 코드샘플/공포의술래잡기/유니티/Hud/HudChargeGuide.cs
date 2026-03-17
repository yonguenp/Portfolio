using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HudChargeGuide : HudPos
{
    [SerializeField] GameObject Arrow;
    [SerializeField] float redius;

    float characterIdleTime = 0;
    Vector3 prevCharacterPos = Vector3.zero;
    Tweener arrowTween = null;
    public override void Init(GameObject target, Camera rc, RectTransform canvas, Transform parent, float playTime = 0)
    {
        transform.localPosition = Vector3.zero;

        SetAddPosY(250.0f);
        Arrow.SetActive(true);
        
        Vector3 localPos = Vector3.zero;
        localPos.y = AddY;
        Arrow.transform.localPosition = localPos;

        base.Init(target, rc, canvas, parent, playTime);
    }
    //character => 충전기 좌표
    protected override void Refresh()
    {
        if (character != null)
        {
            if (prevCharacterPos != Camera.main.transform.position)
            {
                if(arrowTween != null)
                {
                    arrowTween.Kill();
                    arrowTween = null;

                    Vector3 localPos = Vector3.zero;
                    localPos.y = AddY;
                    Arrow.transform.localPosition = localPos;
                }

                Vector3 camPos = Camera.main.transform.position;
                camPos.z = 0.0f;

                if (Vector3.Distance(character.transform.position, camPos) > 5.0f)
                {
                    Arrow.SetActive(true);

                    Vector3 v3 = character.transform.position - camPos;
                    transform.rotation = Quaternion.Euler(0.0f, 0.0f, ((Mathf.Atan2(v3.y, v3.x) * Mathf.Rad2Deg) - 90.0f));
                }
                else
                {
                    Arrow.SetActive(false);
                }

                prevCharacterPos = Camera.main.transform.position;
                characterIdleTime = 0;
            }
            else
            {
                if (characterIdleTime < 2.0f)
                {
                    if (arrowTween != null)
                    {
                        arrowTween.Kill();
                        arrowTween = null;

                        Vector3 localPos = Vector3.zero;
                        localPos.y = AddY;
                        Arrow.transform.localPosition = localPos;
                    }

                    characterIdleTime += Time.deltaTime;
                }
                else if(arrowTween == null)
                {   
                    arrowTween = Arrow.transform.DOLocalMoveY(AddY * 0.5f, 0.34f).SetLoops(-1, LoopType.Yoyo);
                }
            }
        }

    }
}

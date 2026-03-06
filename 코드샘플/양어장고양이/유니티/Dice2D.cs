using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Dice2D : MonoBehaviour
{
    public List<Transform> face;

    public void Update()
    {
        face.Sort((a, b) => { return a.transform.position.z.CompareTo(b.transform.position.z); });
        for (int i = 0; i < face.Count; i++)
        {
            face[i].SetSiblingIndex(i);
        }
    }

    public void OnDice(int targetnum, TweenCallback callback)
    {
        transform.parent.gameObject.SetActive(true);

        Sequence seq = DOTween.Sequence();

        seq.Append(transform.DOLocalRotate(new Vector3(180.0f + Random.value * 180.0f, 180.0f + Random.value * 180.0f, 180.0f + Random.value * 180.0f), 0.3f));
        seq.Append(transform.DOLocalRotate(new Vector3(Random.value * 360.0f, Random.value * 360.0f, Random.value * 360.0f), 0.1f));
        seq.Append(transform.DOLocalRotate(new Vector3(Random.value * 360.0f, Random.value * 360.0f, Random.value * 360.0f), 0.1f));

        seq.Append(transform.DOLocalRotate(new Vector3(Random.value * 360.0f, Random.value * 360.0f, Random.value * 360.0f), 0.1f));
        seq.Append(transform.DOLocalRotate(new Vector3(Random.value * 360.0f, Random.value * 360.0f, Random.value * 360.0f), 0.1f));
        seq.Append(transform.DOLocalRotate(new Vector3(Random.value * 360.0f, Random.value * 360.0f, Random.value * 360.0f), 0.1f));
        seq.Append(transform.DOLocalRotate(new Vector3(Random.value * 360.0f, Random.value * 360.0f, Random.value * 360.0f), 0.1f));
        seq.Append(transform.DOLocalRotate(new Vector3(Random.value * 360.0f, Random.value * 360.0f, Random.value * 360.0f), 0.1f));
        seq.Restart();

        Sequence diceSeq = DOTween.Sequence();
        transform.localScale = Vector3.one * 0.8f;
        diceSeq.Append(transform.DOScale(Vector3.one * 1.2f, 0.3f));        
        diceSeq.Append(transform.DOScale(Vector3.one * 0.8f, 0.5f).SetEase(Ease.OutBounce).OnComplete(
        ()=> {
            transform.DOKill();
            List<Vector3> pr = new List<Vector3>();

            switch (targetnum)
            {
                case 1:
                    //1
                    pr.Add(new Vector3(0, 180, 0));
                    pr.Add(new Vector3(0, 180, 90));
                    pr.Add(new Vector3(0, 180, 180));
                    pr.Add(new Vector3(0, 180, 270));
                    break;
                case 2:
                    //2
                    pr.Add(new Vector3(0, 90, 180));
                    pr.Add(new Vector3(0, 270, 0));
                    pr.Add(new Vector3(270, 180, 90));
                    pr.Add(new Vector3(180, 270, 0));
                    break;
                case 3:
                    //3
                    pr.Add(new Vector3(0, 90, 90));
                    pr.Add(new Vector3(0, 270, 270));
                    pr.Add(new Vector3(90, 0, 0));
                    pr.Add(new Vector3(180, 270, 270));
                    break;
                case 4:
                    //4
                    pr.Add(new Vector3(0, 90, 270));
                    pr.Add(new Vector3(0, 270, 90));
                    pr.Add(new Vector3(180, 270, 90));
                    pr.Add(new Vector3(90, 90, 270));
                    break;
                case 5:
                    //5
                    pr.Add(new Vector3(0, 90, 0));
                    pr.Add(new Vector3(0, 270, 180));
                    pr.Add(new Vector3(90, 90, 0));
                    pr.Add(new Vector3(90, 180, 90));
                    break;
                case 6:
                    //6
                    pr.Add(new Vector3(0, 0, 0));
                    pr.Add(new Vector3(0, 0, 90));
                    pr.Add(new Vector3(0, 0, 180));
                    pr.Add(new Vector3(0, 0, 270));
                    break;
            }


            pr.Sort((a, b) => { return Vector3.Angle(a, transform.localEulerAngles).CompareTo(Vector3.Angle(b, transform.localEulerAngles)); });
            
            Sequence sq = DOTween.Sequence();

            sq.Append(transform.DOLocalRotate(pr[0], 0.1f));
            sq.Append(transform.DOLocalRotate(pr[Random.Range(0, pr.Count)], 0.1f));

            sq.Append(transform.DOScale(Vector3.one * 0.8f, 0.25f));

            sq.Append(transform.DOScale(Vector3.one * 1.1f, 0.25f));
            sq.Append(transform.DOScale(Vector3.one * 1.0f, 0.25f));
            sq.Append(transform.DOScale(Vector3.one * 1.1f, 0.25f));
            sq.Append(transform.DOScale(Vector3.one * 1.0f, 0.25f).OnComplete(() => { Clear(); callback?.Invoke(); }));
            sq.Restart();
        }));
    }

    public void Clear()
    {
        transform.parent.gameObject.SetActive(false);
    }
}

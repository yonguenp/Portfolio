using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SBFieldOfView : MonoBehaviour
{
    public enum SBFOVDirection
    {
        None,
        Up,
        Down,
        Left,
        Right,
        Down_Right,
        Down_Left,
        Up_Right,
        Up_Left,
    }

    [SerializeField]
    protected Vector3 viewPosition = new Vector3();
    [SerializeField]
    protected float viewRadius = 1f;    
    [Range(1, 360)]
    [SerializeField]
    protected float viewAngle = 1f;    
    [SerializeField]
    protected LayerMask targetMask;
    [SerializeField]
    protected LayerMask obstacleMask;
    [SerializeField]
    protected List<SBFovUtil> utilList = null;

    [HideInInspector]
    public List<Transform> visibleTargets = new List<Transform>();

    public float ViewAngle { get { return viewAngle; } }
    public Vector3 ViewPosition { get { return viewPosition + addViewPosision; } }
    public float ViewRadius { get { return viewRadius; } }
    public float CurRadius { get; set; }

    protected Dictionary<Type, SBFovUtil> utilDic = null;

    Vector3 addViewPosision = Vector3.zero;

    public Vector3 AddViewPosision
    {
        set { addViewPosision = value; }
    }



    protected virtual void Start()
    {
        CurRadius = viewRadius;

        utilDic = new Dictionary<Type, SBFovUtil>();

        if (utilList != null)
        {
            for (int i = 0; i < utilList.Count; i++)
            {
                if (utilList[i] == null)
                    continue;
                utilDic.Add(utilList[i].GetType(), utilList[i]);
            }
        }
    }

    protected virtual void Update()
    {
        for (int i = 0; i < utilList.Count; i++)
        {
            if (utilList[i] == null)
                continue;
            utilList[i].UtilUpdate(this, Time.deltaTime);
        }
    }

    public void OnUtilEnter(Type utilType)
    {
        if (utilDic == null)
            return;
        if (utilDic.ContainsKey(utilType))
        {
            utilDic[utilType]?.OnEnter();
        }
    }

    public void OnUtilExit(Type utilType)
    {
        if (utilDic == null)
            return;
        if (utilDic.ContainsKey(utilType))
        {
            addViewPosision = Vector3.zero;
            utilDic[utilType]?.OnExit();
        }
    }

    public virtual void FindVisibleTargets()
    {
        visibleTargets.Clear();
        //Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, curRadius, targetMask);
        Collider2D[] targetsInViewRadius = Physics2D.OverlapCircleAll(transform.position + viewPosition, CurRadius, targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            var target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            if (Vector3.Angle(dirVector3, dirToTarget) < viewAngle * .5f)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.position);

                var check = Physics2D.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask);
                if (!check)
                {
                    visibleTargets.Add(target);
                }
            }
        }
    }

    protected SBFOVDirection Direction = SBFOVDirection.None;
    protected Vector3 dirVector3 = Vector3.up;
    protected Vector2 dirVector2 = Vector2.up;
    protected float dirAngle = 0f;
    public virtual void DirectionCal(SBFOVDirection dir)
    {
        if (Direction != dir)
        {
            switch (dir)
            {
                case SBFOVDirection.Left:
                    dirVector3 = Vector3.left;
                    dirVector2 = Vector2.left;
                    dirAngle = -90f;
                    break;
                case SBFOVDirection.Right:
                    dirVector3 = Vector3.right;
                    dirVector2 = Vector2.right;
                    dirAngle = 90f;
                    break;
                case SBFOVDirection.Down:
                    dirVector3 = Vector3.down;
                    dirVector2 = Vector2.down;
                    dirAngle = 180f;
                    break;
                case SBFOVDirection.Down_Left:
                    dirVector3 = new Vector3(-1, -1, 0);
                    dirVector2 = new Vector2(-1, -1);
                    dirAngle = 225f;
                    break;
                case SBFOVDirection.Down_Right:
                    dirVector3 = new Vector3(1, -1, 0);
                    dirVector2 = new Vector2(1, -1);
                    dirAngle = 135f;
                    break;
                case SBFOVDirection.Up_Left:
                    dirVector3 = new Vector3(-1, 1, 0);
                    dirVector2 = new Vector2(-1, 1);
                    dirAngle = 315f;
                    break;
                case SBFOVDirection.Up_Right:
                    dirVector3 = new Vector3(1, 1, 0);
                    dirVector2 = new Vector2(1, 1);
                    dirAngle = 45f;
                    break;
                case SBFOVDirection.Up:
                default:
                    dirVector3 = Vector3.up;
                    dirVector2 = Vector2.up;
                    dirAngle = 0f;
                    break;
            }
            Direction = dir;
        }
    }

    List<Transform> prevtargets = new List<Transform>();
    public virtual void TargetCheckEvent()//걸러진 타겟의 이벤트 실행
    {
        for (int i = 0; i < prevtargets.Count; i++)
        {
            if (visibleTargets.Contains(prevtargets[i]))//처음 타겟이 됨.
                continue;

            TargetOff(prevtargets[i]);
            prevtargets.RemoveAt(i);
            i--;
        }


        for (int i = 0; i < visibleTargets.Count; i++)
        {
            if (!prevtargets.Contains(visibleTargets[i]))//처음 타겟이 됨.
            {
                prevtargets.Add(visibleTargets[i]);
                TargetOn(visibleTargets[i]);
            }
        }
    }

    public virtual void AllEventOff()
    {
        for (int i = 0; i < visibleTargets.Count; i++)
        {
            TargetOff(visibleTargets[i]);
        }
        visibleTargets.Clear();
        prevtargets.Clear();
    }

    public virtual Vector3 DirFromAngle(float angleIndegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleIndegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin((angleIndegrees + dirAngle) * Mathf.Deg2Rad), Mathf.Cos((angleIndegrees + dirAngle) * Mathf.Deg2Rad), 0);
    }
    #region 하위 클래스 구현부
    protected virtual void TargetOn(Transform target)
    {

    }
    protected virtual void TargetOff(Transform target)
    {

    }
    public virtual GameObject GetFirstObject()
    {
        return null;
    }
    #endregion
}

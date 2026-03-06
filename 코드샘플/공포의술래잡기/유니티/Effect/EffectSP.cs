using SBSocketSharedLib;
using Spine.Unity;
using UnityEngine;

public class EffectSP : BaseObject
{
    [SerializeField]
    protected GameObject _model = null;
    public SkeletonAnimation SkeletonAnimation { get; protected set; }

    protected Spine.AnimationState _animState = null;


    public bool LoopEffect = true;

    bool AlwaysShow {get;set;} = false;

    public virtual void Init(EffectResourceGameData effectData, Transform target = null, MoveDir dir = MoveDir.Down, bool isParent = false)
    {
        if (target == null) return;

        if (_model == null)
        {
            SBDebug.LogError("_model is NULL");
            return;
        }

        SkeletonAnimation = _model.GetComponent<SkeletonAnimation>();
        if (SkeletonAnimation == null)
        {
            SBDebug.LogError("_skeletonAnimation is NULL");
            return;
        }

        _animState = SkeletonAnimation.AnimationState;
        if (_animState == null)
        {
            SBDebug.LogError("_animState is NULL");
            return;
        }

        if (isParent)
        {
            transform.SetParent(target);
            transform.localPosition = Vector3.zero;
            _model.transform.localPosition = new Vector3(effectData.OffsetX, effectData.OffsetY, 0);
        }
        else
        {
            ApplyPos(target.position.x, target.position.y);
        }
        _model.transform.localScale = new Vector3(effectData.ScaleX, effectData.ScaleY, 1);
        _model.transform.eulerAngles = new Vector3(0, 0, effectData.Rotation);

        if (!string.IsNullOrEmpty(effectData.AnimState))
        {
            string stateName = effectData.AnimState;
            switch (dir)
            {
                case MoveDir.Left:
                    stateName = stateName.Replace("f_", "r_");
                    SetFlip(true);
                    break;

                case MoveDir.Right:
                    stateName = stateName.Replace("f_", "r_");
                    break;

                case MoveDir.Up:
                    stateName = stateName.Replace("f_", "b_");
                    break;

                default:
                    break;
            }
            var animData = SkeletonAnimation.skeletonDataAsset.GetSkeletonData(false);
            if (animData != null)
            {
                var animationObject = animData.FindAnimation(stateName);
                if (animationObject != null)
                {
                    _animState.SetAnimation(0, stateName, LoopEffect);
                }
            }
        }
    }

    public void SetFlip(bool flip)
    {
        SkeletonAnimation.skeleton.ScaleX = flip ? -1 : 1;
        if (flip)
            _model.transform.localPosition = new Vector3(-_model.transform.localPosition.x, _model.transform.localPosition.y, 0);
    }

    public override void ShowRenderer(bool isShow)
    {
        if(AlwaysShow == false)
            base.ShowRenderer(isShow);
        else
        {
            base.ShowRenderer(true);
        }
    }

}

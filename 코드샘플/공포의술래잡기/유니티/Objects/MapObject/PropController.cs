using SBSocketSharedLib;
using Spine.Unity;
using UnityEngine.Rendering;

public class PropController : MapObjectBase
{
    protected SkeletonAnimation skeletonAnimation = null;
    protected Spine.AnimationState animState = null;

    public bool IsBroken { get; protected set; }

    protected SoundController soundController = null;
    public ObjectGameData ObjData { get; private set; }

    public override void Init()
    {
        base.Init();

        GameObjectType = GameObjectType.MapObject;
        if (model)
        {
            skeletonAnimation = model.GetComponent<SkeletonAnimation>();
            if (skeletonAnimation == null) return;
            animState = skeletonAnimation.AnimationState;
        }

        IsBroken = false;
        soundController = GetComponent<SoundController>();
        ObjData = Managers.Data.GetData(GameDataManager.DATA_TYPE.@object, ObjectIndex) as ObjectGameData;
        if (ObjData == null) SBDebug.LogWarning($"ERROR INDEX : {ObjectIndex}");

        model.transform.localScale = ObjData.LocalScale;
        model.transform.localPosition = ObjData.LocalPosition;
    }

    protected void SetRenderOrder(int order)
    {
        var sg = model.GetComponent<SortingGroup>();
        if (sg == null)
            model.AddComponent<SortingGroup>();

        sg.sortingOrder = order;
    }

    //void OnEvent(TrackEntry trackEntry, Spine.Event e) { }

    public void PlayAnim(string animName, bool isLoop = false)
    {
        animState.SetAnimation(0, animName, isLoop);
    }

    public void AddAnimQueue(string animName, bool isLoop = false, float delay = 0f)
    {
        animState.AddAnimation(0, animName, isLoop, delay);
    }

    public virtual void OnDespawn()
    {
        gameObject.SetActive(false);
    }

    public virtual void OnRespawn()
    {
        IsBroken = false;
        gameObject.SetActive(true);
    }

    public virtual void SetRespawnTimeOnReconnect(long regenTime) { }
}

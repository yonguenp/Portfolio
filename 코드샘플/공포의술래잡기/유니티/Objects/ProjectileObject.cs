using SBSocketSharedLib;
using DG.Tweening;
using UnityEngine;
using Spine;
using Spine.Unity;
using System.Collections;
using System.Text;

public class ProjectileObject : BaseObject
{
    public enum eShowType
    {
        None,
        All,
        Me,
        Friend,
        Conditional, // 조건부 타입
    }


    float _angle = 0;

    TrailRenderer tp = null;
    Renderer sk = null;
    CharacterObject co = null;
    string ownerID = null;
    public SkillSummonGameData summonData { get; private set; } = null;

    Transform animGo = null;
    Transform modelGo = null;

    public float AddPosY { get; set; } = 0;

    public eShowType ShowType { get; set; } = eShowType.All;

    public SkillSummonGameData.SummonType SummonType
    {
        get
        {
            if (summonData == null) return SkillSummonGameData.SummonType.None;
            return summonData.Type;
        }
    }

    void Start()
    {

    }

    public override void Init()
    {
        GameObjectType = GameObjectType.Projectile;

        SetState(CreatureStatus.Moving);

        animGo = transform.Find("anim");
        if (animGo != null)
            tp = animGo.GetComponent<TrailRenderer>();

        modelGo = transform.Find("model");
        if (modelGo != null)
            sk = modelGo.GetComponent<Renderer>();

        // 분신 스킬에서만 non-null이 될 것이다
        co = GetComponent<CharacterObject>();

        base.Init();

        ShowRenderer(Game.Instance.CheckVisibleObject(gameObject));

        var eff = transform.Find("eff_remove");
        if (eff && eff.gameObject.activeSelf)
            eff.gameObject.SetActive(false);

        var projInfo = GetComponent<ProjectileInfo>();
        if (projInfo == null)
            return;

        switch (projInfo.directionType)
        {
            case ProjectileInfo.eDirectionType.Rotate:
                _angle = Mathf.Atan2(PosInfo.MoveDir.Y, PosInfo.MoveDir.X) * Mathf.Rad2Deg;
                transform.eulerAngles = new Vector3(0, 0, _angle);
                break;

            case ProjectileInfo.eDirectionType.HasAnim:
                var dir = CharacterAnimationController.GetDirForAnimation(PosInfo.MoveDir.X, PosInfo.MoveDir.Y);
                SetDirAnim(dir);
                break;
        }
    }

    public void SetDirAnim(MoveDir dir)
    {
        string strDir = "";
        bool isLeft = false;
        switch (dir)
        {
            case MoveDir.Up:
                strDir = "b"; break;
            case MoveDir.Down:
                strDir = "f"; break;
            case MoveDir.Left:
                strDir = "r";
                isLeft = true;
                break;
            case MoveDir.Right:
                strDir = "r";
                break;
        }

        var sb = new StringBuilder().AppendFormat("{0}_play_0", strDir);
        var scaleX = isLeft ? -1 : 1;

        var _skeletonAnimation = modelGo.GetComponent<SkeletonAnimation>();
        var AnimState = _skeletonAnimation.AnimationState;

        _skeletonAnimation.skeleton.ScaleX = scaleX;
        AnimState.SetAnimation(0, sb.ToString(), true);
    }

    public void SetOwnerID(string owner)
    {
        ownerID = owner;
    }

    public override void ShowRenderer(bool isShow)
    {
        if (ShowType == eShowType.Me)
        {
            if (ownerID == Managers.UserData.MyUserID.ToString())
                isShow = true;
        }
        else if (ShowType == eShowType.Friend)
        {
            var obj = Managers.Object.FindById(ownerID);
            if (obj != null)
            {
                var characterObject = obj.GetComponent<CharacterObject>();
                if (characterObject == null)
                    return;
                isShow = characterObject.IsChaser == Game.Instance.PlayerController.Character.IsChaser;
            }
        }
        else if (ShowType == eShowType.Conditional)
        {
            if (Game.Instance.PlayerController.Character.IsDetectTrap)
                isShow = true;
            else
            {
                var obj = Managers.Object.FindById(ownerID);
                if (obj != null)
                {
                    var characterObject = obj.GetComponent<CharacterObject>();
                    if (characterObject == null)
                        return;
                    isShow = characterObject.IsChaser == Game.Instance.PlayerController.Character.IsChaser;
                }
            }
        }
        else
        {
            isShow = true;
        }

        base.ShowRenderer(isShow);

        if (sk) sk.enabled = isShow;
        if (tp) tp.enabled = isShow;
        if (co) co.ShowRenderer(isShow);
    }

    public void SetSummonData(SkillSummonGameData data)
    {
        summonData = data;
    }

    protected override void SetPosition(Vector3 pos)
    {
        if (summonData != null)
        {
            if (summonData.Type == SkillSummonGameData.SummonType.Projectile)
            {
                pos.y += AddPosY;
            }
        }

        base.SetPosition(pos);
    }

    public bool ReadyRemove { get; set; } = false;

    void OnCompleteEvent(TrackEntry trackEntry)
    {
        if (ReadyRemove)
        {
            Managers.Object.Remove(Id, 5.0f);
            ReadyRemove = false;
            var _skeletonAnimation = modelGo.GetComponent<SkeletonAnimation>();
            var AnimState = _skeletonAnimation.AnimationState;
            AnimState.Complete -= OnCompleteEvent;
        }
    }

    public void SetReadyRemove()
    {
        if (modelGo == null)
        {
            modelGo = transform.Find("model");
            if (modelGo == null)
            {
                SBDebug.LogError("model is Null");
                return;
            }
        }

        var _skeletonAnimation = modelGo.GetComponent<SkeletonAnimation>();
        if (_skeletonAnimation != null)
        {
            var animData = _skeletonAnimation.skeletonDataAsset.GetSkeletonData(false);
            if (animData != null)
            {
                var animationObject = animData.FindAnimation("f_broken_0");
                if (animationObject != null)
                {
                    var AnimState = _skeletonAnimation.AnimationState;
                    AnimState.Complete += OnCompleteEvent;
                    AnimState.SetAnimation(0, "f_broken_0", false);
                    if (summonData.summon_explosion_sound_id != null)
                    {
                        var datas = Managers.Data.GetData(GameDataManager.DATA_TYPE.sound_resource);
                        foreach (SoundResourceData item in datas)
                        {
                            if (item.sound_key == summonData.summon_explosion_sound_id)
                            {
                                var audio = Managers.Resource.LoadAssetsBundle<AudioClip>(item.resource_path);
                                Managers.Sound.Play(audio, Sound.Effect);
                                break;
                            }
                        }
                    }
                    ReadyRemove = true;
                }
            }
        }

        var eff = transform.Find("eff_remove");
        if (eff && eff.gameObject.activeSelf == false)
            eff.gameObject.SetActive(true);

        if (ReadyRemove)
        {
            ShowType = eShowType.All;
            ShowRenderer(true);
        }
        else
        {
            Managers.Object.Remove(Id, 5.0f);
        }
    }

    public void SetAnimation(int trackId, string animName, bool isLoop)
    {
        var _skeletonAnimation = modelGo.GetComponent<SkeletonAnimation>();
        var AnimState = _skeletonAnimation.AnimationState;
        AnimState.SetAnimation(trackId, animName, isLoop);
    }

    public void SetRemove(Vec2Float despawnPos)
    {
        StartCoroutine(PlayRemoveAnimWithSyncPosCO(despawnPos));

        var eff = transform.Find("eff_remove");
        if (eff && eff.gameObject.activeSelf == false) eff.gameObject.SetActive(true);
    }
    public void SetRemove(Vec2Float despawnPos, ProjectileObject p_obj)
    {
        StartCoroutine(PlayRemoveAnimWithSyncPosCO(despawnPos));

        var eff = transform.Find("eff_remove");
        if (eff && eff.gameObject.activeSelf == false) eff.gameObject.SetActive(true);

        if (p_obj.summonData != null && p_obj.summonData.explosion_effect_id > 0)
            Managers.Effect.PlayEffect(p_obj.summonData.explosion_effect_id, p_obj.transform);
    }

    public void SetRemove()
    {
        StartCoroutine(PlayRemoveAnimCO());

        var eff = transform.Find("eff_remove");
        if (eff && eff.gameObject.activeSelf == false) eff.gameObject.SetActive(true);
    }

    IEnumerator PlayRemoveAnimWithSyncPosCO(Vec2Float despawnPos)
    {
        if (Speed > 0)
        {
            Vector2 diffPos = new Vector2(despawnPos.X, despawnPos.Y) - new Vector2(PosInfo.Pos.X, PosInfo.Pos.Y);

            //    while (true)
            //    {
            //        if (diffPos.x > 0 && PosInfo.Pos.X >= despawnPos.X)
            //        {
            //            break;
            //        }
            //        if (diffPos.x < 0 && PosInfo.Pos.X <= despawnPos.X)
            //        {
            //            break;
            //        }

            //        if (diffPos.y > 0 && PosInfo.Pos.Y >= despawnPos.X)
            //        {
            //            break;
            //        }
            //        if (diffPos.y < 0 && PosInfo.Pos.Y <= despawnPos.X)
            //        {
            //            break;
            //        }

            //        yield return new WaitForEndOfFrame();
            //    }

            PosInfo.Pos = new Vec2Float(despawnPos.X, despawnPos.Y);
        }

        yield return PlayRemoveAnimCO();
    }
    IEnumerator PlayRemoveAnimCO()
    {
        PosInfo.Status = (int)CreatureStatus.Idle;

        var _skeletonAnimation = modelGo.GetComponent<SkeletonAnimation>();
        var _sprite = modelGo.GetComponent<SpriteRenderer>();

        float animTime = 0.3f;
        float playTime = 0.0f;
        Color skeletonColor = Color.white;
        Color spriteColor = Color.white;
        if (_skeletonAnimation != null)
        {
            skeletonColor = _skeletonAnimation.skeleton.GetColor();

            var animData = _skeletonAnimation.skeletonDataAsset.GetSkeletonData(false);
            if (animData != null)
            {
                var animationObject = animData.FindAnimation("f_broken_0");
                if (animationObject != null)
                {
                    _skeletonAnimation.loop = false;
                    _skeletonAnimation.ClearState();
                    _skeletonAnimation.AnimationName = "f_broken_0";
                    _skeletonAnimation = null;
                }
            }
        }

        if (_sprite != null)
            spriteColor = _sprite.color;

        float originSpineAlpha = skeletonColor.a;
        float originSpriteAlpha = spriteColor.a;

        if (tp != null)
        {
            tp.DOResize(0.0f, 0.0f, animTime * 0.5f);
        }

        while (playTime < animTime)
        {
            playTime += Time.deltaTime;
            float ratio = (playTime / animTime);

            skeletonColor.a = originSpineAlpha - (originSpineAlpha * ratio);
            spriteColor.a = originSpriteAlpha - (originSpriteAlpha * ratio);

            if (skeletonColor.a < 0.0f)
                skeletonColor.a = 0.0f;
            if (spriteColor.a < 0.0f)
                spriteColor.a = 0.0f;

            if (_skeletonAnimation != null)
                _skeletonAnimation.skeleton.SetColor(skeletonColor);
            if (_sprite != null)
                _sprite.color = spriteColor;

            yield return new WaitForEndOfFrame();
        }


        if (modelGo != null)
            modelGo.gameObject.SetActive(false);
        if (animGo != null)
            animGo.gameObject.SetActive(false);
    }

    public void SetHp(ushort hp, ushort maxhp)
    {
        Stat.Hp = hp;
    }

    public void OnDamage(string attackerID, ushort targetHp, byte targetShieldCnt, DamageType damageType, int damagePoint)
    {
        Stat.Hp = targetHp;
        SetHp(Stat.Hp, Stat.MaxHp);
        Managers.Effect.PlayEffect(10001, this.transform);
    }
}

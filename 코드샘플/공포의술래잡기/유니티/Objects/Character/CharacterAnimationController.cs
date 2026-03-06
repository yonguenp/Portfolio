using Spine.Unity;
using UnityEngine;
using Spine;
using SBSocketSharedLib;
using System.Text;
using UnityEngine.Rendering;

public class CharacterAnimationController : MonoBehaviour
{
    CharacterObject _character = null;
    GameObject _spineObject = null;
    SkeletonAnimation _skeletonAnimation = null;
    Renderer _renderer = null;
    ParticleSystem moveDust = null;

    ParticleSystem deathEffect = null;
    ParticleSystem respawnEffect = null;

    Material normalMaterial = null;
    [SerializeField]
    Transform backGround;
    [SerializeField]
    Transform frontGround;

    static public MoveDir GetDirForAnimation(float moveDirX, float moveDirY)
    {
        var angle = Mathf.Atan2(moveDirY, moveDirX) * Mathf.Rad2Deg;
        MoveDir dir = MoveDir.None;
        if (angle > 45 && angle < 135) dir = MoveDir.Up;
        else if (angle >= 135 || angle <= -135) dir = MoveDir.Left;
        else if (angle < -45 && angle > -135) dir = MoveDir.Down;
        else dir = MoveDir.Right;
        return dir;
    }

    public SkeletonAnimation SkeletonAnimation { get { return _skeletonAnimation; } }
    public Spine.AnimationState AnimState { get; private set; } = null;
    private bool canChangeAnimation = true;
    bool _playingAttack = false;
    bool _playingVent = false;
    bool _playingInteract = false;
    Vector2 direction = new Vector2(0, -1);

    SkeletonRenderTextureLight skeletonRenderTextureLight = null;

    private void Start()
    {
        _character = GetComponent<CharacterObject>();
        Transform anim = transform.Find("anim");
        if (anim != null)
            _spineObject = anim.gameObject;

        if (_spineObject)
        {
            _skeletonAnimation = _spineObject.GetComponent<SkeletonAnimation>();
            AnimState = _skeletonAnimation.AnimationState;

            AnimState.Complete += OnCompleteEvent;

            AnimState.SetAnimation(0, "f_idle_0", true);
            _renderer = _spineObject.GetComponent<Renderer>();

            var sg = _spineObject.GetComponent<SortingGroup>();
            if (sg == null)
                _spineObject.AddComponent<SortingGroup>();
        }

        if (moveDust == null)
        {
            var data = CharacterGameData.GetCharacterData(GetComponent<CharacterObject>().CharacterType);
            var moveDustObj = Managers.Resource.Instantiate(data.cha_dust_reource);
            moveDustObj.transform.parent = transform;
            moveDustObj.transform.localPosition = Vector3.zero;

            moveDust = moveDustObj.GetComponent<ParticleSystem>();
        }

        if (deathEffect == null)
        {
            var deathObj = Managers.Resource.Instantiate("Effect/fx_death");
            deathObj.transform.parent = transform;
            deathObj.transform.localPosition = Vector3.zero;

            deathEffect = deathObj.GetComponent<ParticleSystem>();

            deathEffect.gameObject.SetActive(false);
        }

        if (respawnEffect == null)
        {
            var respawnObj = Managers.Resource.Instantiate("Effect/fx_respawn");
            respawnObj.transform.parent = transform;
            respawnObj.transform.localPosition = Vector3.zero;

            respawnEffect = respawnObj.GetComponent<ParticleSystem>();

            ClearRespawnEffect();
        }
        PlayMoveDust();

        skeletonRenderTextureLight = _spineObject.GetComponent<SkeletonRenderTextureLight>();

        SetColor(Color.white);
    }

    public void SetEquip(SkeletonAnimation skeletonAnimation, int equip = 0)
    {
        foreach (Transform child in backGround.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in frontGround.transform)
        {
            Destroy(child.gameObject);
        }

        if (equip > 0)
        {
            EquipInfo info = EquipInfo.GetEquipData(equip);
            if (info != null)
            {
                if (!string.IsNullOrEmpty(info.sp_effect_resource))
                {
                    var auraObj = Managers.Resource.InstantiateFromBundle(info.sp_effect_resource);
                    if (auraObj != null)
                    {
                        auraObj.transform.parent = backGround;
                        auraObj.transform.localPosition = Vector3.zero;
                        auraObj.transform.localScale = Vector3.one;
                    }
                }

                if (!string.IsNullOrEmpty(info.sp_effect_resource_front))
                {
                    var auraObj = Managers.Resource.InstantiateFromBundle(info.sp_effect_resource_front);
                    if (auraObj != null)
                    {
                        auraObj.transform.parent = frontGround;
                        auraObj.transform.localPosition = Vector3.zero;
                        auraObj.transform.localScale = Vector3.one;
                    }
                }
            }
        }
    }


    public void OnRespawn()
    {
        SetCanChangeAnimation(true);
        SetAnimation(.0f, .0f, CreatureStatus.Idle, MoveStatus.None, true);

        if (respawnEffect != null)
        {
            respawnEffect.gameObject.SetActive(true);
            Invoke("ClearRespawnEffect", 3.0f);
        }
    }

    void ClearRespawnEffect()
    {
        CancelInvoke("ClearRespawnEffect");

        if (respawnEffect != null)
        {
            respawnEffect.gameObject.SetActive(false);
        }
    }

    public void ClearTrack(int index)
    {
        AnimState.ClearTrack(index);
    }

    public void ResetAttackAnim()
    {
        canChangeAnimation = true;
        _playingAttack = false;
        AnimState.ClearTrack(1);
        _character.UpdateAnimation();
    }

    void OnCompleteEvent(TrackEntry trackEntry)
    {
        if (trackEntry.Loop)
            return;

        if (_playingAttack && trackEntry.TrackIndex == 1)
        {
            ResetAttackAnim();
        }

        if (_playingVent && trackEntry.TrackIndex == 0)
        {
            canChangeAnimation = true;
            _character.UpdateAnimation();
            _playingVent = false;
        }

        if (_playingInteract && trackEntry.TrackIndex == 1)
        {
            canChangeAnimation = true;
            _character.UpdateAnimation();
            AnimState.ClearTrack(1);
            _playingInteract = false;
        }
    }

    public void SetAddAnimation(float dirX, float dirY)
    {
        //if (canChangeAnimation == false) return;
        if (dirX == 0 && dirY == 0)
        {
            dirX = direction.x;
            dirY = direction.y;
        }
        else
        {
            direction.x = dirX;
            direction.y = dirY;
        }

        MoveDir dir = GetDirForAnimation(dirX, dirY);

        string strStats = "add";

        string strDir = "";
        switch (dir)
        {
            case MoveDir.Up:
                strDir = "b"; break;
            case MoveDir.Down:
                strDir = "f"; break;
            case MoveDir.Left:
            case MoveDir.Right:
                strDir = "r";
                break;
        }

        ClearTrack(2);
        if (strStats == "victory")
            strDir = "f";
        var sb = new StringBuilder().AppendFormat("{0}_{1}_0", strDir, strStats);
        AnimState.SetAnimation(2, sb.ToString(), true);
    }


    public void SetAnimationVehicle(int vehicleIndex, float dirX, float dirY, CreatureStatus stats, MoveStatus moveState, bool isLoop)
    {
        if (canChangeAnimation == false) return;

        if (dirX == 0 && dirY == 0)
        {
            dirX = direction.x;
            dirY = direction.y;
        }
        else
        {
            direction.x = dirX;
            direction.y = dirY;
        }
        MoveDir dir = GetDirForAnimation(dirX, dirY);

        ObjectVehicleGameData vehicleData = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.object_vehicle, vehicleIndex) as ObjectVehicleGameData;
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

        var sb = new StringBuilder().AppendFormat("{0}_{1}_0", strDir, vehicleData.anim_name);
        var scaleX = isLeft ? -1 : 1;
        if (AnimState.ToString().Equals(sb.ToString()) == true && _skeletonAnimation.skeleton.ScaleX == scaleX) return;
        _skeletonAnimation.skeleton.ScaleX = scaleX;

        AnimState.SetAnimation(0, sb.ToString(), isLoop);
        canChangeAnimation = true;

        //SBDebug.Log($"setanimation : {sb.ToString()}, Flip : {isRight.ToString()}");
    }

    public void SetAnimation(float dirX, float dirY, CreatureStatus stats, MoveStatus moveState, bool isLoop, bool isMetamorphosis = false)
    {
        if (canChangeAnimation == false) return;
        if (dirX == 0 && dirY == 0)
        {
            dirX = direction.x;
            dirY = direction.y;
        }
        else
        {
            direction.x = dirX;
            direction.y = dirY;
        }

        MoveDir dir = GetDirForAnimation(dirX, dirY);
        bool needResetPose = true;
        string strStats = "";
        switch (stats)
        {
            case CreatureStatus.Idle:
            case CreatureStatus.Hiding:
                {
                    if (moveState == MoveStatus.Jump || moveState == MoveStatus.Knockback)
                    {
                        strStats = "run";
                    }
                    else
                    {
                        strStats = "idle";
                    }
                    break;
                }
            case CreatureStatus.Moving:
                {
                    if (moveState == MoveStatus.Run || moveState == MoveStatus.Jump || moveState == MoveStatus.Walk || moveState == MoveStatus.Knockback)
                    {
                        strStats = "run";
                    }
                    else if (moveState == MoveStatus.Teleport)
                    {
                        strStats = "idle";
                    }
                    else
                    {
                        strStats = "run";
                    }
                    break;
                }
            case CreatureStatus.Groggy:
                {
                    strStats = "stun";
                    break;
                }
            default:
                return;
        }

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

        var sb = new StringBuilder().AppendFormat("{0}_{1}_0", strDir, strStats);
        var scaleX = isLeft ? -1 : 1;
        if (AnimState.ToString().Equals(sb.ToString()) == true && _skeletonAnimation.skeleton.ScaleX == scaleX)
        {
            if (needResetPose)
            {
                _skeletonAnimation.skeleton.SetToSetupPose();
            }
            return;
        }

        _skeletonAnimation.skeleton.ScaleX = scaleX;
        AnimState.SetAnimation(0, sb.ToString(), isLoop);
        if (needResetPose)
        {
            _skeletonAnimation.skeleton.SetToSetupPose();
        }
        canChangeAnimation = true;

        if (isMetamorphosis)
        {
            ClearTrack(2);
            sb = new StringBuilder().AppendFormat("{0}_add_0", strDir, strStats);
            AnimState.SetAnimation(2, sb.ToString(), isLoop);
        }
        //SBDebug.Log($"setanimation : {sb.ToString()}, Flip : {isRight.ToString()}");

        if (moveState == MoveStatus.None)
            StopMoveDust();
        else
            PlayMoveDust();

        if (deathEffect != null && deathEffect.gameObject.activeSelf)
        {
            deathEffect.gameObject.SetActive(false);
        }
    }

    public void PlaySkillAnimation(float dirX, float dirY, int type)
    {
        if (_playingVent)
        {
            _playingVent = false;
        }

        MoveDir dir = GetDirForAnimation(dirX, dirY);
        string strDir = "";
        switch (dir)
        {
            case MoveDir.Up:
                strDir = "b"; break;
            case MoveDir.Down:
                strDir = "f"; break;
            case MoveDir.Left:
            case MoveDir.Right:
                strDir = "r";
                break;
        }

        var sb = new StringBuilder().AppendFormat("{0}_action_{1}", strDir, type);

        try
        {
            AnimState.ClearTracks();
            _skeletonAnimation.skeleton.SetToSetupPose();
            AnimState.SetAnimation(1, sb.ToString(), false);

            canChangeAnimation = false;
            _playingAttack = true;
        }
        catch
        {
            SBDebug.LogWarning("No Animation : " + sb.ToString());
        }
    }

    public void PlayInteractAnimation(float dirX, float dirY)
    {
        MoveDir dir = GetDirForAnimation(dirX, dirY);

        string strDir = "";
        switch (dir)
        {
            case MoveDir.Up:
                strDir = "b"; break;
            case MoveDir.Down:
                strDir = "f"; break;
            case MoveDir.Left:
            case MoveDir.Right:
                if (dirY == 0 && dirX == 0)//기본값이 우측으로 잡혀있지만 인터렉트할때는 뒷쪽을 보는경우가 많다
                    strDir = "b";
                else
                    strDir = "r";
                break;
        }

        string aniName = "interecting_0";

        var sb = new StringBuilder().AppendFormat("{0}_{1}", strDir, aniName);
        try
        {
            AnimState.ClearTracks();
            _skeletonAnimation.skeleton.SetToSetupPose();
            AnimState.SetAnimation(1, sb.ToString(), false);

            canChangeAnimation = false;
            _playingInteract = true;
        }
        catch
        {
            SBDebug.LogWarning("No Animation : " + sb.ToString());
        }
    }

    public void PlayCastingAnimation(float dirX, float dirY)
    {

        MoveDir dir = GetDirForAnimation(dirX, dirY);
        string strDir = "";
        switch (dir)
        {
            case MoveDir.Up:
                strDir = "b"; break;
            case MoveDir.Down:
                strDir = "f"; break;
            case MoveDir.Left:
            case MoveDir.Right:
                strDir = "r";
                break;
        }

        string aniName = "casting_0";

        var sb = new StringBuilder().AppendFormat("{0}_{1}", strDir, aniName);
        try
        {
            AnimState.ClearTracks();
            _skeletonAnimation.skeleton.SetToSetupPose();
            AnimState.SetAnimation(1, sb.ToString(), false);

            canChangeAnimation = true;
            _playingAttack = false;
        }
        catch
        {
            SBDebug.LogWarning("No Animation : " + sb.ToString());
        }
    }

    public void PlayStunAnimation(float dirX, float dirY, bool isChaser)
    {
        AnimState.ClearTrack(1);

        MoveDir dir = GetDirForAnimation(dirX, dirY);
        bool isLeft = false;
        switch (dir)
        {
            case MoveDir.Left:
                isLeft = true;
                break;
        }

        string aniName = "r_stun_0";
        var scaleX = isLeft == isChaser ? -1 : 1;
        canChangeAnimation = true;
        _playingAttack = false;

        if (AnimState.ToString().Equals(aniName.ToString()) == true && _skeletonAnimation.skeleton.ScaleX == scaleX) return;

        _skeletonAnimation.skeleton.ScaleX = scaleX;
        AnimState.SetAnimation(0, aniName.ToString(), false);
    }

    public void PlayGroggyAnimation(float dirX, float dirY)
    {
        AnimState.ClearTrack(1);

        MoveDir dir = GetDirForAnimation(dirX, dirY);
        bool isLeft = dirX < 0;
        // switch (dir)
        // {
        //     case MoveDir.Left:
        //     case MoveDir.Up:
        //         isLeft = true;
        //         break;

        //     default:
        //         isLeft = false;
        //         break;
        // }

        string aniName = "groggy_0";
        var scaleX = isLeft ? -1 : 1;

        if (AnimState.ToString().Equals(aniName.ToString())) return;
        //if (_animState.ToString().Equals(aniName.ToString()) == true && _skeletonAnimation.skeleton.ScaleX == scaleX) return;
        _skeletonAnimation.skeleton.ScaleX = scaleX;
        AnimState.SetAnimation(0, aniName.ToString(), true);
        canChangeAnimation = false;

        StopMoveDust();

        if (deathEffect != null)
            deathEffect.gameObject.SetActive(true);
    }

    public void PlayEscapedAnimation()
    {
        AnimState.ClearTrack(1);

        string aniName = "f_victory_0";
        var anim = _skeletonAnimation.skeletonDataAsset.GetSkeletonData(false).FindAnimation(aniName);
        if (anim == null)
            aniName = "f_idle_0";

        if (AnimState.ToString().Equals(aniName.ToString())) return;
        _skeletonAnimation.skeleton.ScaleX = 1.0f;
        AnimState.SetAnimation(0, aniName.ToString(), true);
        canChangeAnimation = false;
        StopMoveDust();
    }

    public void PlayVentUpAnimation(float dirX, float dirY)
    {
        if (_playingVent)
        {
            AnimState.ClearTrack(1);
        }

        MoveDir dir = GetDirForAnimation(dirX, dirY);
        bool isLeft = false;
        switch (dir)
        {
            case MoveDir.Left:
                isLeft = true;
                break;
        }

        string aniName = "vent_0";
        var scaleX = isLeft ? -1 : 1;

        if (AnimState.ToString().Equals(aniName.ToString()) == true && _skeletonAnimation.skeleton.ScaleX == scaleX) return;
        _skeletonAnimation.skeleton.ScaleX = scaleX;
        AnimState.SetAnimation(0, aniName.ToString(), false);

        _playingVent = true;
        //canChangeAnimation = false;
    }

    public void SetColor(Color color)
    {
        if(skeletonRenderTextureLight != null)
            skeletonRenderTextureLight.color = color;
        //_skeletonAnimation.skeleton.SetColor(color);
        if (color.a != 1.0f) StopMoveDust();
        else PlayMoveDust();
    }

    public void Show(bool isShow)
    {
        //if (_renderer != null) _renderer.enabled = isShow;
        if (skeletonRenderTextureLight != null) skeletonRenderTextureLight.Show(isShow);
        if (!isShow) StopMoveDust();
        else PlayMoveDust();



        if (frontGround != null)
            frontGround.gameObject.SetActive(isShow);
        if (backGround != null)
            backGround.gameObject.SetActive(isShow);
    }

    public void SetCanChangeAnimation(bool value)
    {
        canChangeAnimation = value;
    }

    public void StopMoveDust()
    {
        if (moveDust != null && moveDust.isPlaying)
        {
            moveDust.Stop();
        }
    }

    public void PlayMoveDust()
    {
        if (_character != null && _character.IsInvisible)
            return;
        if (moveDust != null && !moveDust.isPlaying)
        {
            moveDust.Play();
        }
    }
}

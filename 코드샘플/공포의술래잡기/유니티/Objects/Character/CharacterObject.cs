using UnityEngine;
using System.Collections;
using SBSocketSharedLib;
using UnityEngine.Events;
using System.Collections.Generic;

public class CharacterEventState : UnityEvent<CreatureStatus, bool> { }

struct CharacterVehicleData
{
    public int Index;
    public float Speed;
    public float time;
}

public partial class CharacterObject : BaseObject
{
    protected HudHp _hudHp = null;
    protected HudPosText _hudPos = null;
    protected HudBattery _hudBattery = null;
    protected HudName userNameObject = null;
    protected HudEmotion userEmotionObject = null;

    CharacterVehicleData characterVehicleData;
    CharacterAnimationController _characterRenderer = null;
    float _normalSpeed = 0;
    CharacterEventState characterEventState = new CharacterEventState();
    bool isHit = false;
    UnityEvent moveAfterEvent = new UnityEvent();
    Coroutine hitCoroutine = null;
    BuffController _buff = null;

    GameObject _shadow = null;
    Color currentColor = Color.white;
    private bool metamorphosis;
    private Vector2 goalPosition = Vector2.zero;
    private AudioSource ventAudio = null;

    public int CharacterType { get; private set; } = 0;
    public bool IsChaser { get; private set; } = false;
    public string UserName { get; private set; } = null;
    public CharacterPortrait PortraitUI { get; private set; }
    public bool IsVehicle { get; protected set; }
    public int BatteryCount { get; private set; }
    public bool Escaped { get { return goalPosition != Vector2.zero; } }
    public bool RecivedEscape { get; private set; }
    public Transform RootEffect { get; private set; }
    public bool IsMe { get { return Managers.UserData.MyUserID.ToString() == Id; } }
    public bool IsFriend { get { return Managers.PlayData.AmIChaser() == IsChaser; } }
    public bool IsNeverGroggy { get; private set; }
    public bool IsGroggyOnRound { get; private set; }
    public SoundController soundController { get; private set; }

    public bool IsDespawned { get; private set; } = false;
    private void Start()
    {
        GameObjectType = GameObjectType.Player;
        _characterRenderer = GetComponent<CharacterAnimationController>();
        soundController = GetComponent<SoundController>();

        characterVehicleData = new CharacterVehicleData();
        characterVehicleData.Index = 0;
        characterVehicleData.Speed = 0;
        characterVehicleData.time = 0;
        IsVehicle = false;
        _buff = new BuffController(this);
        BatteryCount = 0;
        IsNeverGroggy = true;

        CreateRootEffect();
        //StartCoroutine(PlayWalkSound());
        CreateBotCircle();
    }

    void CreateBotCircle()
    {
        GameObject botCircle = null;

        if (Game.Instance != null && Game.Instance.PlayerController != null)
        {
            if (Game.Instance.PlayerController.Character.IsChaser == IsChaser)
            {
                botCircle = Managers.Resource.Instantiate("Object/Guide/playerCircle");
            }
            else
            {
                botCircle = Managers.Resource.Instantiate("Object/Guide/enemyCircle");
            }
        }

        if (botCircle != null)
        {
            botCircle.transform.parent = RootEffect.transform;
            botCircle.transform.localPosition = Vector3.zero;
        }
    }

    private void OnDisable()
    {
        try
        {
            ShowAllHuds(false);
        }
        catch { }
    }

    private void OnDestroy()
    {
        //SBDebug.Log("CharacterObject : OnDestroy // 가끔 갑자기 파괴되는 경우가 있어서 디버깅 차 출력");
        RemoveAllHuds();
    }

    public override void SetBaseData(string id, PositionInfo pos = null, StatInfo stat = null)
    {
        CreateRootEffect();

        base.SetBaseData(id, pos, stat);
        if (!string.IsNullOrEmpty(id))
        {
            UserName = Managers.PlayData.GetRoomPlayer(id).UserName;
            gameObject.name = UserName;
        }
    }

    public void SetCharacterType(int type)
    {
        CharacterType = type;
        IsChaser = CharacterGameData.IsChaserCharacter(type);
    }

    public void SetPortrait(CharacterPortrait po)
    {
        PortraitUI = po;
    }

    public void AddCharacterEventState(UnityAction<CreatureStatus, bool> action)
    {
        characterEventState.AddListener(action);
    }

    public void RemoveCharacterEventState(UnityAction<CreatureStatus, bool> action)
    {
        characterEventState.RemoveListener(action);
    }

    public void AddMoveAfterEvent(UnityAction action)
    {
        moveAfterEvent.AddListener(action);
    }

    public void RemoveMoveAfterEvent(UnityAction action)
    {
        moveAfterEvent.RemoveListener(action);
    }

    public void SetSpeed(float norSpeed)
    {
        _normalSpeed = norSpeed;
    }

    public void SetHp(ushort hp, ushort maxhp)
    {
        Stat.Hp = hp;
        PortraitUI?.SetHp(hp, maxhp);
        _hudHp?.SetHp(hp);
    }

    public void SetBattery(int cnt, bool fromDropped = false, bool isResume = false)
    {
        // 배터리를 획득한 경우
        if (BatteryCount < cnt && !isResume)
        {
            if (fromDropped)
            {
                soundController.Play("effect/EF_GET_AT_BATTERY");
            }
            else
            {
                soundController.Play("effect/EF_GET_BATTERY");
            }
        }

        if (IsMe && _hudBattery)
            VibrateManager.OnVibrate(0.5f, (int)((float)Mathf.Abs(cnt - BatteryCount) / _hudBattery.GetMaxBattery() * 255.0f));

        BatteryCount = cnt;
        _hudBattery?.SetBattery(BatteryCount);
    }

    public void DropBattery(IList<DropInfo> drops)
    {
        var cnt = DropBatteryObject(drops);

        if (BatteryCount < cnt)
        {
            SBDebug.LogError("배터리 개수가 드랍 개수보다 적음, 어딘가 문제있음");
            return;
        }

        soundController.Play("effect/EF_ATTACK_BATTERY");
        SetBattery(BatteryCount - cnt);
    }

    private int DropBatteryObject(IList<DropInfo> drops)
    {
        int value = 0;
        foreach (var drop in drops)
        {
            value += drop.Cnt;

            var go = Managers.Resource.Instantiate("Object/battery");
            if (go != null)
            {
                var bo = go.GetComponent<Battery>();
                if (bo != null)
                {
                    bo.SetBaseData(drop.GameObjectId);
                    bo.Init(drop.Cnt);
                    bo.ApplyPos(PosInfo.Pos.X, PosInfo.Pos.Y);

                    Managers.Object.SpawnBattery(drop.GameObjectId, bo);

                    var subVec = new Vector2(drop.Pos.X + 0.5f - PosInfo.Pos.X, drop.Pos.Y + 0.5f - PosInfo.Pos.Y);
                    bo.PlayDrop(0.2f, subVec.normalized, subVec.magnitude);
                }
            }
        }

        return value;
    }

    protected override void UpdateController()
    {
        if (Game.Instance.IsPlay == false) return;
        base.UpdateController();
        if (IsVehicle)
        {
            if (characterVehicleData.time <= 0)
                ClearVehicle();

            characterVehicleData.time -= Time.deltaTime;
        }
    }

    public override void UpdateAnimation()
    {
        if (_characterRenderer == null) return;

        if (IsVehicle == true)
            _characterRenderer.SetAnimationVehicle(characterVehicleData.Index, PosInfo.MoveDir.X, PosInfo.MoveDir.Y, State, (MoveStatus)PosInfo.MoveStatus, true);
        else
            _characterRenderer.SetAnimation(PosInfo.MoveDir.X, PosInfo.MoveDir.Y, State, (MoveStatus)PosInfo.MoveStatus, true);
    }

    IEnumerator HitCoroutine()
    {
        yield return new WaitForSeconds(0.2f);
        SetColor(Color.white);
        isHit = false;
    }

    public override void UpdateState()
    {
        switch (State)
        {
            case CreatureStatus.Idle:
                if (!isHit)
                    SetColor(Color.white);
                break;

            case CreatureStatus.Hiding:
                SetColor(new Color(1f, 1f, 1f, 0f));
                break;
        }
    }

    public void SetColor(Color color)
    {
        if (color != Color.white && isHit && hitCoroutine != null)  // 색상변경을 해야하는데 피격 이펙트를 표시하는 중이라면 덮어씌워버린다
        {
            isHit = false;

            if (hitCoroutine != null)
                StopCoroutine(hitCoroutine);

            hitCoroutine = null;
        }

        var value = color.a != 0f;
        // 캐릭터 자체가 보이지 않으면 HUD도 숨긴다
        if (!Game.Instance.CheckVisibleObject(gameObject))
        {
            value = false;
        }

        if (_characterRenderer != null)
            _characterRenderer.SetColor(color);

        // 숨어있는 캐릭터의 HUD UI는 추적자는 못보고, 생존자는 볼 수 있다
        if (State == CreatureStatus.Hiding)
            value = !Managers.PlayData.AmIChaser();

        // 알파값이 조금이라도 들어가면 그림자를 없앤다
        if (_shadow != null) _shadow.gameObject.SetActive(color.a == 1f);

        currentColor = color;

        ShowAllHuds(value);
    }

    public void OnDamage(string attackerID, ushort targetHp, byte targetShieldCnt, DamageType damageType, int damagePoint)
    {
        if (IsChaser) return;

        if (Stat.Shield_Cnt > 0 && damageType != DamageType.Heal)
        {
            Stat.Shield_Cnt = targetShieldCnt;
            if (Stat.Shield_Cnt == 0) _buff.DeleteShield();
        }
        else
        {
            Stat.Hp = targetHp;
            SetHp(Stat.Hp, Stat.MaxHp);
            if (targetHp == 0)
            {
                var attacker = Managers.Object.FindCharacterById(attackerID);
                if (attacker != null)
                {
                    Game.Instance.UIGame.CreateKillMessage(attacker, this);

                    if (IsMe)
                    {
                        Game.Instance.UIGame.SetKiller(attacker);
                    }
                }
            }
        }

        switch (damageType)
        {
            case DamageType.Attack:
                Managers.Effect.PlayEffect(10001, RootEffect);
                if (currentColor == Color.white) // 캐릭터가 색상의 영향을 받지 않을 때만 동작한다
                {
                    SetColor(Color.red);
                    isHit = true;
                }

                if (IsMe)
                    VibrateManager.OnVibrate(0.5f, 125);
                break;
            case DamageType.Heal:
                Managers.Effect.PlayEffect(12, RootEffect);

                if (IsMe)
                    VibrateManager.OnVibrate(0.5f, 50);
                break;
            case DamageType.Critical:
                Managers.Effect.PlayEffect(10008, RootEffect);
                if (currentColor == Color.white) // 캐릭터가 색상의 영향을 받지 않을 때만 동작한다
                {
                    SetColor(Color.red);
                    isHit = true;
                }
                if (IsMe)
                    VibrateManager.OnVibrate(0.5f, 50);
                break;
            case DamageType.Miss:
                Managers.Effect.PlayEffect(10009, RootEffect);
                if (IsMe)
                    VibrateManager.OnVibrate(0.5f, 50);
                break;
            case DamageType.DotAttack:
                Managers.Effect.PlayEffect(10013, RootEffect);
                if (currentColor == Color.white) // 캐릭터가 색상의 영향을 받지 않을 때만 동작한다
                {
                    SetColor(Color.green);
                    isHit = true;
                }

                if (IsMe)
                    VibrateManager.OnVibrate(0.5f, 125);
                break;
        }

        if (isHit)
        {
            if (hitCoroutine != null)
            {
                StopCoroutine(hitCoroutine);
                hitCoroutine = null;
            }
            hitCoroutine = StartCoroutine(HitCoroutine());
        }
    }

    public void OnHide(bool isHide)
    {
        if (State != CreatureStatus.Hiding)
        {
            SetState(CreatureStatus.Hiding);
            _diffDis = 0;
            _diffPos = Vector3.zero;
            characterEventState.Invoke(CreatureStatus.Hiding, false);
            SetImmovable();

            soundController.Play("effect/EF_IN_BOX", SoundController.PlayType.Broadcast);
        }
        else
        {
            SetState(CreatureStatus.Idle);
            characterEventState.Invoke(CreatureStatus.Hiding, true);
            SetMovable();

            soundController.Play("effect/EF_BOX_BREAK", SoundController.PlayType.Broadcast);
        }
    }

    public void OnChangeRound()
    {
        IsGroggyOnRound = false;
    }

    public void OnGroggy()
    {
        _characterRenderer.PlayGroggyAnimation(PosInfo.MoveDir.X, PosInfo.MoveDir.Y);
        if (PortraitUI != null)
            PortraitUI.SetState(CharacterPortrait.PORTRAIT_STATE.GROGGY);

        IsNeverGroggy = false;
        IsGroggyOnRound = true;
        SetImmovable();

        // 진행중인 작업이 있을 때 진행바 표시 해제
        Game.Instance.PlayEscapeKey(false, Id);
        Game.Instance.PlayOpenDoor(false, Id);

        if (_hudBattery != null)
            _hudBattery.gameObject.SetActive(false);
        if (_hudHp != null)
            _hudHp.gameObject.SetActive(false);

        // 스킬 레인지를 표시하고 있을 때 꺼주고, 스킬 가이드를 표시 해제하고, 락걸린 쿨타임 다 풀어준다
        if (IsMe)
        {
            VibrateManager.OnVibrate(1.0f, 255);

            Game.Instance.PlayerController.RemoveSkillRangeGuideUI();
            Game.Instance.UIGame.HideSkillGuide();
            Game.Instance.PlayerController.UnlockAllCoolTime();
        }
    }

    public void OnEscape()
    {
        SetImmovable();
        if (PortraitUI != null) PortraitUI.SetState(CharacterPortrait.PORTRAIT_STATE.ESCAPE);
        StartCoroutine(EscapeCoroutine());
        if (IsMe)
        {
            Game.Instance.PlayerController.ShowGuide(false);
            Sprite icon_exit = Managers.Resource.LoadAssetsBundle<Sprite>("AssetsBundle/Texture/Icon/icon_observer");
            if (icon_exit != null)
            {
                Game.Instance.PlayerController.ControllerPad.Joysticks[(int)JoystickInputType.Normal].SetPadIconSprite(icon_exit);
            }
        }
    }

    IEnumerator EscapeCoroutine()
    {
        yield return FadeCoroutine(2.0f);
        if (IsMe)
        {
            Game.Instance.PlayerController.RemoveSkillRangeGuideUI();
        }

        PositionInfo posInfo = new PositionInfo();
        posInfo.MoveDir = new Vec2Float(0.0f, -1.0f);
        posInfo.Pos = new Vec2Float(goalPosition.x, goalPosition.y);//일단 y값 하드코딩
        posInfo.Status = (byte)CreatureStatus.Escape;
        SetPosInfo(posInfo);

        _characterRenderer.PlayEscapedAnimation();
        _characterRenderer.Show(true);
        if (_shadow != null) _shadow.gameObject.SetActive(true);

        float curTime = 1.0f;
        while (true)
        {
            float value = 1.0f - (curTime / 1.0f);
            if (_characterRenderer != null)
                _characterRenderer.SetColor(new Color(1, 1, 1, value));

            curTime -= Time.deltaTime;
            if (curTime <= 0) break;

            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator FadeCoroutine(float playTime)
    {
        ShowAllHuds(false);

        float curTime = playTime;
        while (curTime > 0)
        {
            float value = curTime / playTime;

            if (_characterRenderer != null)
                _characterRenderer.SetColor(new Color(1,1,1,value));

            curTime -= Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        SetEscaped(transform.position);

        ShowRenderer(false);
    }

    public void OnDespawnClone(float playTime, SkillSummonGameData data)
    {
        IsDespawned = true;

        StartCoroutine(FadeCoroutine(playTime));

        if (data != null && data.explosion_effect_id > 0)
            Managers.Effect.PlayEffect(data.explosion_effect_id, RootEffect);
    }

    public void SetVehicle(ObjectVehicleGameData vehicle)
    {
        if (vehicle == null) return;

        ventAudio = null;
        IsVehicle = true;
        characterVehicleData.Index = vehicle.GetID();
        characterVehicleData.time = vehicle.duration * 0.001f;
        characterVehicleData.Speed = vehicle.speed;

        UpdateAnimation();

        if (IsMe)
            Game.Instance.UIGame.OnVehicleRemainBar(vehicle.duration * 0.001f);

        AudioSource audio = null;
        switch (characterVehicleData.Index)
        {
            case 50001:
                audio = soundController.Play("effect/EF_RIDE_IV", SoundController.PlayType.Invalid);
                break;
            case 50002:
                audio = soundController.Play("effect/EF_RIDE_BOTTLE", SoundController.PlayType.Invalid);
                break;
            case 50003:
                audio = soundController.Play("effect/EF_RIDE_CART", SoundController.PlayType.Invalid);
                break;
        }
        ventAudio = audio;
        if (audio != null)
            StartCoroutine(VehicleSoundCoroutine(audio, vehicle.duration * 0.001f));
    }

    IEnumerator VehicleSoundCoroutine(AudioSource audio, float time)
    {
        yield return new WaitForSeconds(time);

        Managers.Sound.Stop(audio);
    }

    public void ClearVehicle()
    {
        if (ventAudio != null)
            Managers.Sound.Stop(ventAudio);

        IsVehicle = false;
        characterVehicleData.Index = 0;
        characterVehicleData.time = 0;
        characterVehicleData.Speed = 0;

        UpdateAnimation();

        if (IsMe)
        {
            //Game.Instance.PlayerController.OnMove(Vector2.zero);
            Game.Instance.UIGame.OnVehicleRemainBar(0.0f);
        }
    }

    public override void ShowRenderer(bool isShow)
    {
        if (Escaped)
        {
            string aniName = "f_victory_0";

            isShow = _characterRenderer.AnimState.ToString().Equals(aniName.ToString());//킹받게 계속표시
        }
        else//도망친 상태가 아니면...
        {
            // 이 캐릭터가 보이지 않는데 적이면 아무것도 안보여야지 암 그렇고말고
            if (!IsFriend)
            {
                if (IsInvisible || currentColor.a == 0)
                {
                    isShow = false;
                }
            }

            if(IsDespawned)
                isShow = false;
        }

        base.ShowRenderer(isShow);
        if (_characterRenderer == null) return;

        _characterRenderer.Show(isShow);
        if (_shadow != null) _shadow.gameObject.SetActive(isShow && currentColor.a == 1f);
        // if (State == CreatureStatus.Hiding && Managers.PlayData.AmIChaser())
        //     isShow = false;

        ShowAllHuds(isShow);
    }

    public void OnRespawn(RespawnObjectInfo info)
    {
        isHit = false;
        SetState(CreatureStatus.Idle);
        PosInfo.MoveStatus = (byte)MoveStatus.None;
        ApplyPos(info.Position.X, info.Position.Y);
        SetHp(Stat.MaxHp, Stat.MaxHp);
        SetBattery(0);
        SetEscaped(Vector2.zero);

        if (PortraitUI != null)
            PortraitUI.SetState(CharacterPortrait.PORTRAIT_STATE.NORMAL);

        _characterRenderer.OnRespawn();

        SetMovable();

        if (IsMe)
        {
            Game.Instance.UIGame.ShowPlayerRespawn(-1);
            Game.Instance.PlayerController.OffObserverMode();
        }
    }

    public void SetShadow(GameObject shadowObj)
    {
        shadowObj.transform.parent = transform;
        shadowObj.transform.localPosition = Vector3.zero;
        shadowObj.transform.localScale = new Vector3(0.4f, 0.4f);
        _shadow = shadowObj;
    }

    public void InitBatteryChargeCount(int count)
    {
        PortraitUI.SetBattery(count);
    }

    public void AddBatteryChargeCount(int count)
    {
        soundController.Play("effect/EF_IN_BATTERY");
        PortraitUI.AddBatteryChargeCount(count);
    }
    
    void CreateRootEffect()
    {
        if (RootEffect == null)
        {
            var rootEffectObject = new GameObject();

            RootEffect = rootEffectObject.transform;
            RootEffect.name = "@Effect";
            RootEffect.parent = gameObject.transform;
            RootEffect.localPosition = Vector3.zero;
        }
    }

    public void SetClone(CharacterObject source)
    {
        soundController = GetComponent<SoundController>();
        _characterRenderer = GetComponent<CharacterAnimationController>();
        soundController.SetPlayType(SoundController.PlayType.Nobody);

        UserName = source.UserName;
        CharacterType = source.CharacterType;
        // 원본과 똑같은 UI를 배정해준다 (체력, 배터리, 닉네임 등)
        SetHudHp(Game.Instance.HudNode.CreateHudHP(this, source.Stat.MaxHp));
        SetHudBattery(Game.Instance.HudNode.CreateHudBattery(this, source.Stat.Max_Battery));
        SetBattery(source.BatteryCount);
        SetHp(source.Stat.Hp, source.Stat.MaxHp);

        RoomPlayerInfo curPlayerInfo = Managers.PlayData.GetRoomPlayer(source.Id);
        if (curPlayerInfo != null)
        {
            SetUserName(Game.Instance.HudNode.CreateHudUserName(this, Managers.PlayData.GetSlotIndex(source.Id)));
        }
    }

    IEnumerator PlayWalkSound()
    {
        string[] walkSoundPath = { "effect/EF_WALK_1", "effect/EF_WALK_2", "effect/EF_WALK_3", "effect/EF_WALK_4" };

        while (true)
        {
            if (State == CreatureStatus.Moving && !IsVehicle)
            {
                var path = walkSoundPath[UnityEngine.Random.Range(0, walkSoundPath.Length)];
                soundController.Play(path);
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    public bool CheckVisibleCharacter()
    {
        if (IsMe) return true;
        if (currentColor.a == 0) return false;
        else return IsShow;
    }

    public void OnInteract()
    {
        Managers.Effect.PlayEffect(11, RootEffect);
        _characterRenderer.PlayInteractAnimation(PosInfo.MoveDir.X, PosInfo.MoveDir.Y);
    }

    public void SetEscaped(Vector2 pos)
    {
        goalPosition = pos;
    }

    public void SetEmotion(ushort type)
    {
        userEmotionObject.ShowEmotion(type);
    }
}

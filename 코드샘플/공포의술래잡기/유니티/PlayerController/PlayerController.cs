//using LOS;
using SBCommonLib;
using SBSocketSharedLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using DG.Tweening;

public partial class PlayerController : MonoBehaviour, IControllerListener
{
    GameObject _dirGuide = null;
    Game _game = null;
    UIGame _uiGame = null;
    Camera _camera = null;

    //FOV
    SBFieldOfRender fov = null;
    //LOSManager LOSManager = null;

    public Vector2 _inputDir { get; protected set; } = Vector2.down;
    protected long _inputTime = 0;

    Vector2 halfViewboxSize = Vector2.zero;

    public ControllerPad ControllerPad { get; private set; }
    public CharacterObject Character { get; private set; }
    public bool ObserverMode { get; private set; } = false;
    CharacterObject ObserverModeCharacterContainer = null;

    Stack<CSMove> MovePackets = new Stack<CSMove>();
    float MovePacketDelay = 0.0f;
    //나중 게임헬퍼로 빼자
    protected struct AngleInfo
    {
        public float rMin;
        public float rMax;
        public float angle;
        public float x;
        public float y;
    }

    AngleInfo[] angleInfo;
    float halfOneAngle;

    protected AngleInfo GetAngleInfo(float angle)
    {
        if (angle > 360 - halfOneAngle)
        {
            angle -= 360;
        }

        int count = angleInfo.Length;
        for (int i = 0; i < count; ++i)
        {
            AngleInfo ai = angleInfo[i];

            if (ai.angle + halfOneAngle >= angle && ai.angle - halfOneAngle < angle)
                return ai;
        }

        return default(AngleInfo);
    }

    public void Init(CharacterObject charController)
    {
        int count = GameConfig.Instance.MOVE_DIRECTION_COUNT;
        var oneAngle = 360f / count;
        halfOneAngle = oneAngle / 2;

        angleInfo = new AngleInfo[count];
        float r = Mathf.Deg2Rad;
        float prev = r * halfOneAngle;
        float startAngle = halfOneAngle;

        angleInfo[0].rMin = r * (360 - halfOneAngle);
        angleInfo[0].rMax = r * halfOneAngle;
        angleInfo[0].angle = 0;
        angleInfo[0].x = Mathf.Cos(r * angleInfo[0].angle);
        angleInfo[0].y = Mathf.Sin(r * angleInfo[0].angle);

        for (int i = 0; i < count - 1; ++i)
        {
            if (i == 0) continue;
            startAngle += oneAngle;

            angleInfo[i].rMin = angleInfo[i - 1].rMax;
            angleInfo[i].rMax = r * startAngle;
            angleInfo[i].angle = i * oneAngle;
            angleInfo[i].x = Mathf.Cos(r * angleInfo[i].angle);
            angleInfo[i].y = Mathf.Sin(r * angleInfo[i].angle);
        }

        angleInfo[count - 1].rMin = angleInfo[count - 2].rMax;
        angleInfo[count - 1].rMax = angleInfo[0].rMin;
        angleInfo[count - 1].angle = (count - 1) * oneAngle;
        angleInfo[count - 1].x = Mathf.Cos(r * angleInfo[count - 1].angle);
        angleInfo[count - 1].y = Mathf.Sin(r * angleInfo[count - 1].angle);

        Character = charController;
        if (Character == null)
        {
            SBDebug.LogError("character is null");
            return;
        }
        ObserverModeCharacterContainer = Character;

        Character.AddCharacterEventState(CharacterEventStateCallback);
        Character.AddMoveAfterEvent(CharacterMoveAfterEvent);

        //skill setting
        InitializeSkill();

        _dirGuide = Managers.Resource.Instantiate("Object/Guide/GuideDirection");
        if (_dirGuide)
        {
            //캐릭터는 기본적으로 아래를 보고 있음으로 아래쪽에 위치
            _dirGuide.transform.position = Character.transform.position + new Vector3(0, 0.5f) + (Vector3.down * 0.5f);
            var angle = Mathf.Atan2(-1, 0) * Mathf.Rad2Deg;
            _dirGuide.transform.eulerAngles = new Vector3(0, 0, angle);
            _dirGuide.name = "Guide";
            _dirGuide.SetActive(true);
        }

        _game = Game.Instance;
        _uiGame = _game.UIGame;

        SetStanmina();

        _camera = Camera.main;

        //fov = _camera.GetComponent<SBFieldOfRender>(); // 없어도 동작하던데 과거 잔재인듯하여 일단 비활성화

        //LOSManager = GameObject.FindObjectOfType<LOSManager>();

        var targetPos = new Vector3(Character.transform.position.x, Character.transform.position.y, -10);
        _camera.transform.position = targetPos;

        OffObserverMode();
    }

    internal bool IsEnableSkill(object p)
    {
        throw new NotImplementedException();
    }

    public void ListenerLateUpdate()
    {
        SetCameraFocus();
    }

    public void SetCameraFocus()
    {
        var targetPos = new Vector3(Character.transform.position.x, Character.transform.position.y, -10);
        _camera.transform.position = Vector3.Lerp(_camera.transform.position, targetPos, Time.deltaTime * 8.0f);

        if (_dirGuide && !ObserverMode)
        {
            var moveDir = Character.PosInfo.MoveDir;
            if (moveDir.X != 0 || moveDir.Y != 0)
            {
                _dirGuide.transform.position = Character.transform.position + (new Vector3(moveDir.X, moveDir.Y) * 0.5f);
                var angle = Mathf.Atan2(moveDir.Y, moveDir.X) * Mathf.Rad2Deg;
                _dirGuide.transform.eulerAngles = new Vector3(0, 0, angle);
            }
        }
    }

    public virtual void OnMove(Vector2 dir)
    {
        if (!Game.Instance.IsPlay) return;
        if (!Character.IsMovable) return;

        //임시 처리
        //SBDebug.Log($"Character OnMove[{dir}]");
        var creatureState = CreatureStatus.Idle;
        var moveState = MoveStatus.None;
        dir = dir.normalized;
        if (dir != Vector2.zero)
        {
            if (Character.CanMove(dir.x, dir.y) == false)
            {
                return;
            }

            creatureState = CreatureStatus.Moving;

            //걷기가 사라짐
            //if (_inputRun) moveState = isUseStamina ? MoveStatus.Run : MoveStatus.Walk;
            //else moveState = MoveStatus.Walk;
            if (Character.MoveStatus == MoveStatus.Riding)
                moveState = MoveStatus.Riding;
            else
                moveState = MoveStatus.Run;

            _inputDir = dir;
        }

        MovePackets.Push(
             new CSMove
             {
                 MoveDir = new Vec2Float(dir.x, dir.y),
                 Position = new Vec2Float(Character.PosInfo.Pos.X, Character.PosInfo.Pos.Y),
                 Status = (byte)creatureState,
                 MoveStatus = (byte)moveState,
             }
        );
    }

    void CharacterMoveAfterEvent()
    {
        //character.UpdateMoveing() after call function
        RefreshObjectVisible();
    }

    void CharacterEventStateCallback(CreatureStatus state, bool isActive)
    {
        switch (state)
        {
            case CreatureStatus.Hiding:
                {
                    if (isActive == false)
                    {
                        var prop = Target as PropController;
                        if (prop == null) return;

                        if (fov)
                        {
                            fov.AddViewPosision = prop.transform.position - new Vector3(_camera.transform.position.x, _camera.transform.position.y);
                            fov.OnUtilEnter(typeof(SBFovHide));
                        }
                    }
                    else
                    {
                        if (fov)
                        {
                            fov.AddViewPosision = Vector3.zero;
                            fov.OnUtilExit(typeof(SBFovHide));
                        }
                    }
                }
                break;
        }
    }

    void UpdateSkill()
    {
        _characterSkill.Update();
    }

    void Update()
    {
        UpdateSkill();
        UpdateLogic();
        CheckCharacterVisible();
        CheckObjectVisible<ProjectileObject>();
        CheckWallShadow();

        if (Game.Instance.IsPlay)
            ListenerLateUpdate();

        ProcessMove();
    }

    void ProcessMove()
    {
        MovePacketDelay += Time.deltaTime;

        if (MovePacketDelay < GameConfig.Instance.MOVE_PACKET_DELAY)
            return;

        MovePacketDelay = 0.0f;

        if (MovePackets.Count > 0)
        {
            Managers.GameServer.SendMove(MovePackets.Pop());
            Debug.Log($"-=-=-=날아간 이동패킷 갯수 : {MovePackets.Count}");
            MovePackets.Clear();
        }
    }

    public bool IsVisibleObject(GameObject targetObject)
    {
        if (Character.gameObject == targetObject) return true;

        float cullingDistance = Mathf.Max(halfViewboxSize.x, halfViewboxSize.y);

        var subPos = targetObject.transform.position - Character.transform.position;

        if (subPos.magnitude > cullingDistance)
        {
            return false;
        }

        bool isShow = true;
        RaycastHit2D[] hitArr = Physics2D.RaycastAll(Character.transform.position, subPos.normalized, cullingDistance);
        foreach (var hit in hitArr)
        {
            if (hit.collider.gameObject == Character.gameObject) continue;
            if (hit.collider.gameObject.CompareTag("ShadowCasterObject"))
            {
                isShow = false;
                continue;
            }

            if (hit.collider.gameObject == targetObject)
            {
                return isShow;
            }
            else
            {
                if (isShow) continue;
                return false;
            }
        }

        return false;
    }

    void CheckObjectVisible<T>() where T : BaseObject
    {
        var objects = Managers.Object.Objects;
        var iter = objects.GetEnumerator();
        while (iter.MoveNext())
        {
            var bo = iter.Current.Value;
            if (bo == null) continue;
            var comp = bo.GetComponent<T>();
            if (comp == null) continue;
            comp.ShowRenderer(IsVisibleObject(bo.gameObject));
        }
    }

    void CheckCharacterVisible()
    {
        if (Character == null)
            return;

        List<CharacterObject> characters = Managers.Object.GetCharacters();
        float cullingDistance = Mathf.Max(halfViewboxSize.x, halfViewboxSize.y);

        foreach (var c in characters)
        {
            var subPos = c.transform.position - Character.transform.position;

            if (subPos.magnitude > cullingDistance)
            {
                c.ShowRenderer(false);
            }
            else
            {
                bool isShow = true;
                RaycastHit2D[] hitArr = Physics2D.RaycastAll(Character.transform.position, subPos.normalized, cullingDistance);
                foreach (var hit in hitArr)
                {
                    if (hit.collider.gameObject == Character.gameObject) continue;
                    if (hit.collider.gameObject.CompareTag("ShadowCasterObject"))
                    {
                        isShow = false;
                        continue;
                    }

                    if (hit.collider.gameObject == c.gameObject)
                    {
                        c.ShowRenderer(isShow);
                        break;
                    }
                    else
                    {
                        if (isShow) continue;
                        c.ShowRenderer(false);
                        break;
                    }
                }
            }
        }
    }


    void CheckWallShadow()
    {
        //카메라 사이즈가 매프레임 변경 될 수도 있어 매프레임 계산하도록 함.
        //카메라 사이즈 변경시 update flag를 통해 최적화 가능
        var screenSize = SHelper.GetScreenSizeInWorld(Camera.main);
        halfViewboxSize = screenSize / 2 * 1.01f;
        //Vector2 upperRight = new Vector2(halfViewboxSize.x, halfViewboxSize.y) + SMath.Vec3ToVec2(_character.transform.position);
        //Vector2 upperLeft = new Vector2(-halfViewboxSize.x, halfViewboxSize.y) + SMath.Vec3ToVec2(_character.transform.position);
        //Vector2 lowerLeft = new Vector2(-halfViewboxSize.x, -halfViewboxSize.y) + SMath.Vec3ToVec2(_character.transform.position);
        //Vector2 lowerRight = new Vector2(halfViewboxSize.x, -halfViewboxSize.y) + SMath.Vec3ToVec2(_character.transform.position);

        Rect rect = new Rect();
        rect.x = Character.transform.position.x - halfViewboxSize.x;
        rect.y = Character.transform.position.y - halfViewboxSize.y;
        rect.width = screenSize.x;
        rect.height = screenSize.y;

        var sco = GameObject.FindGameObjectsWithTag("ShadowCasterObject");
        foreach (var s in sco)
        {
            var sc = s.GetComponent<ShadowCaster2D>();
            if (sc == null) continue;
            var box2d = s.GetComponent<BoxCollider2D>();
            if (box2d == null) continue;

            float hw = box2d.size.x / 2;
            float hh = box2d.size.y / 2;

            var targetRect = new Rect();
            targetRect.x = s.gameObject.transform.position.x - hw;
            targetRect.y = s.gameObject.transform.position.y - hh;
            targetRect.width = box2d.size.x;
            targetRect.height = box2d.size.y;

            if (rect.Overlaps(targetRect))
            {
                const float bufferX = 0.5f;
                const float bufferY = 1.0f;

                bool selfShadow = targetRect.y <= Character.transform.position.y && (Character.transform.position.x < targetRect.x - bufferX || Character.transform.position.x > targetRect.x + targetRect.width + bufferX);
                if (!selfShadow)
                {
                    selfShadow = targetRect.y <= Character.transform.position.y - bufferY;
                }

                sc.castsShadows = true;
                sc.selfShadows = selfShadow;
            }
            else
            {
                sc.castsShadows = false;
            }

        }
    }

    public void Release()
    {
        Character.RemoveCharacterEventState(CharacterEventStateCallback);
        Character.RemoveMoveAfterEvent(CharacterMoveAfterEvent);
    }

    public void SetControllPad(ControllerPad pad)
    {
        ControllerPad = pad;
    }

    //#if UNITY_EDITOR || UNITY_STANDALONE
    public ControllerKeyboard ControllerKeyboard { get; private set; }
    public void SetControllerKeyboard(ControllerKeyboard keyboard)
    {
        ControllerKeyboard = keyboard;
    }
    //#endif

    public void SetObserverObject(CharacterObject obj)
    {
        Character = obj;
        Character.AddMoveAfterEvent(CharacterMoveAfterEvent);
        obj.ShowRenderer(true);
        ObserverMode = ObserverModeCharacterContainer != Character;
        ShowGuide(false);

        ControllerPad.SetObserverMode(ObserverMode, ObserverModeCharacterContainer.Escaped);
    }

    public void ChangeSurvivorObserverMode()
    {
        List<CharacterObject> charlist = new List<CharacterObject>(Managers.Object.GetCharacters());
        charlist.Add(ObserverModeCharacterContainer);

        CharacterObject target = null;
        bool choice = false;
        bool chaserPlaying = ObserverModeCharacterContainer.IsChaser;
        foreach (CharacterObject obj in charlist)
        {
            if ((chaserPlaying || !obj.IsChaser) && choice)
            {
                target = obj;
                break;
            }

            if (obj == Character)
            {
                choice = true;
            }
        }

        if (target == null)
        {
            target = charlist[0];
        }

        SetObserverObject(target);
    }

    public void OffObserverMode()
    {
        Character.RemoveMoveAfterEvent(CharacterMoveAfterEvent);
        SetObserverObject(ObserverModeCharacterContainer);

        Vector3 playerPos = new Vector3(Character.transform.position.x, Character.transform.position.y, -10);
        Camera.main.transform.position = playerPos;

        Camera.main.DOKill();
        Camera.main.orthographicSize = 7.0f;
        Camera.main.DOOrthoSize(6.5f, 1.0f);

        if (_dirGuide)
        {
            ShowGuide(true);
            //캐릭터는 기본적으로 아래를 보고 있음으로 아래쪽에 위치
            _dirGuide.transform.position = Character.transform.position + new Vector3(0, 0.5f) + (Vector3.down * 0.5f);
            var angle = Mathf.Atan2(-1, 0) * Mathf.Rad2Deg;
            _dirGuide.transform.eulerAngles = new Vector3(0, 0, angle);
        }

        RemoveSkillRangeGuideUI();

        Game.Instance.UIGame.SetSightBlockStatus(false);
    }

    public void ShowGuide(bool isShow)
    {
        _dirGuide?.SetActive(isShow);
    }
}

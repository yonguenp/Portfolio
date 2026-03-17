using SBSocketSharedLib;
using System.Collections;
using UnityEngine;

public partial class PlayerController
{
    //GameObject icon = null;
    //PropController targetProp = null;
    //public PropController TargetProp { get { return targetProp; } }

    public BaseObject Target { get; private set; }
    GameObject targetIcon = null;
    Coroutine progressCoroutine = null;
    //스테미너는 차후 MyPlayer정보로 빼야됨.
    protected float staminaMax = 100;
    protected float stamina = 100;
    protected float staminaRestoreSecondValue = 0;//100.0f / 10.0f;
    protected float staminaUseValue = 0;//100.0f / 5.0f; //100/5(초)

    protected float staminaUseSecondTime = 7f;
    protected float staminaRestoreTotalTime = 10f;

    protected float stMax = 0;
    protected float stValue = 0;

    public bool IsUseStamina { get; private set; }

    BaseObject GetNearestTarget()
    {
        Vector2 retVec = new Vector2(1000, 1000);
        float retVecMag = retVec.magnitude;
        var basePos = Character.PosInfo.Pos;

        bool isVehicle = Character.IsVehicle;
        bool isChaser = Character.IsChaser;
        bool isHide = Character.State == CreatureStatus.Hiding;

        PropController baseController = null;
        var iter = Managers.Object.Objects.GetEnumerator();
        // if (isVehicle || isHide) return null;
        if (isVehicle) return null;

        while (iter.MoveNext())
        {
            var go = iter.Current.Value;
            if (go == null || go.activeSelf == false) continue;
            if (go == Character.gameObject) continue;

            var goController = go.GetComponent<BaseObject>();
            if (goController == null) continue;
            if (goController.enabled == false) continue;
            if (goController.GameObjectType != GameObjectType.MapObject) continue;

            var propController = goController as PropController;
            if (propController != null && propController.ObjData != null)
            {
                //propController.ObjData.enableUserType => 1 = chaser, 2 = survivor, 3 = all
                if (propController.ObjData.enableUserType == 0)
                    continue;

                if (isChaser && propController.ObjData.enableUserType == 2) continue;
                else if (!isChaser && propController.ObjData.enableUserType == 1) continue;

                //23.02.15 준형 기존 생존자, 추격자 직업으로 처리하던 로직 테이블 칼럼으로 조절 할 수 있도록 변경
                //if (isChaser && propController.ObjData.ObjectType == ObjectGameData.MapObjectType.Hide) continue;
                //if (!isChaser && propController.ObjData.ObjectType == ObjectGameData.MapObjectType.Vent) continue;
            }

            var vecDis = new Vector2(Mathf.Abs(goController.transform.position.x - basePos.X), Mathf.Abs(goController.transform.position.y - basePos.Y));
            float mag = vecDis.magnitude;
            if (mag < retVecMag && propController.MapObjectType != ObjectGameData.MapObjectType.None)
            {
                retVec = vecDis;
                retVecMag = mag;
                baseController = propController;
            }
        }

        if (baseController != null && baseController.ObjData != null)
        {
            if (baseController.ObjData.interaction_x >= retVec.x && baseController.ObjData.interaction_y >= retVec.y)
            {
                return baseController;
            }
        }

        return null;
    }

    public void UpdateLogic()
    {
        var baseController = GetNearestTarget();
        if (baseController != null && baseController != Target)
        {
            //SBDebug.Log($"target is {baseController.name}");
            Target = baseController;
            ShowTargetIcon();
        }
        else if (baseController == null && Target != null)
        {
            HideTargetIcon();

            if (Character.IsVehicle)
            {
                Game.Instance.PlayerController.ControllerPad.SetNearestObject((PropController)Target);
            }
            else
            {
                Target = null;
            }
        }
    }

    public void ResetTarget()
    {
        Target = null;
        HideTargetIcon();
    }

    public void UpdateTargetIcon()
    {
        ShowTargetIcon();
    }

    void ShowTargetIcon()
    {
        if (Target == null)
            return;
        var objectType = Target.GameObjectType;

        switch (objectType)
        {
            case GameObjectType.Player:
                break;
            case GameObjectType.MapObject:
                {
                    var propCont = Target as PropController;

                    if (propCont != null && !propCont.IsBroken)
                    {
                        var mapObjectType = propCont.MapObjectType;
                        var keyObjectType = propCont.ObjectKeyType;

                        var icon = CreateIcon(mapObjectType, keyObjectType);
                        if (icon)
                        {
                            if (targetIcon != null)
                            {
                                Destroy(targetIcon);
                                targetIcon = null;
                            }

                            Game.Instance.PlayerController.ControllerPad.SetNearestObject(propCont);

                            targetIcon = icon;
                            targetIcon.transform.position = propCont.transform.position + new Vector3(0, 2, 0);
                        }
                    }
                }
                break;
        }
    }

    GameObject CreateIcon(ObjectGameData.MapObjectType objType, ObjectKeyGameData.ObjectKeyType keyType)
    {
        // 공통
        if (objType == ObjectGameData.MapObjectType.Vehicle)
        {
            return Managers.Resource.Instantiate("Object/Icon/ride_icon");
        }

        // 생존자
        if (!Character.IsChaser)
        {
            if (objType == ObjectGameData.MapObjectType.Hide)
                return Managers.Resource.Instantiate("Object/Icon/hiding_icon");
            else if (objType == ObjectGameData.MapObjectType.Key)
            {
                if (keyType == ObjectKeyGameData.ObjectKeyType.ElectricBox)
                {
                    return Managers.Resource.Instantiate("Object/Icon/escapekey_icon");
                }
                // if (keyType == ObjectKeyGameData.ObjectKeyType.EscapeDoor)
                // {
                //     if (_game.GameRoom.CanOpenDoor() == false)
                //         return Managers.Resource.Instantiate("Object/Icon/x_icon");
                //     return Managers.Resource.Instantiate("Object/Icon/open_door_icon");
                // }
            }
            else if (objType == ObjectGameData.MapObjectType.BatteryCreater)
            {
                var prop = Target.GetComponent<BatteryCreator>();
                if (prop != null)
                    if (prop.IsAvailable)
                    {
                        if (Managers.UserData.MyPoint < 50)
                            Game.Instance.PlayerController.ControllerPad.tutorialEffect.Play();
                        return Managers.Resource.Instantiate("Object/Icon/escapekey_icon");
                    }
                    else
                        return Managers.Resource.Instantiate("Object/Icon/x_icon");

            }
            else if (objType == ObjectGameData.MapObjectType.BatteryGenerater)
            {
                var prop = Target.GetComponent<BatteryGenerator>();
                if (prop != null)
                    if (!prop.IsFullyCharged && prop.IsActived)
                    {
                        if (Managers.UserData.MyPoint < 50)
                            Game.Instance.PlayerController.ControllerPad.tutorialEffect.Play();

                        return Managers.Resource.Instantiate("Object/Icon/escapekey_icon");
                    }
                    else
                        return Managers.Resource.Instantiate("Object/Icon/x_icon");
            }
            else if (objType == ObjectGameData.MapObjectType.Vent)
                return Managers.Resource.Instantiate("Object/Icon/vent_icon");

        }
        // 추격자
        else
        {
            if (objType == ObjectGameData.MapObjectType.Vent)
                return Managers.Resource.Instantiate("Object/Icon/vent_icon");
            //else if (objType == ObjectGameData.MapObjectType.Hide)
            //    return Managers.Resource.Instantiate("Object/Icon/hiding_icon");

        }

        return null;
    }

    void HideTargetIcon()
    {
        Game.Instance.PlayerController.ControllerPad.SetNearestObject(null);
        Game.Instance.PlayerController.ControllerPad.tutorialEffect.Stop();

        if (targetIcon == null)
            return;

        Destroy(targetIcon);
        targetIcon = null;
    }

    void ReqBatteryCreator(PropController target)
    {
        if (target == null) return;
        var bc = target.GetComponent<BatteryCreator>();
        if (bc != null)
            if (bc.IsAvailable)
                Managers.GameServer.SendGetBattery(target.Id);
    }

    void ReqBatteryGenerator(PropController target)
    {
        if (target == null) return;
        if (Character.BatteryCount <= 0) return;
        var genObject = target.GetComponent<BatteryGenerator>();
        if (genObject != null)
            if (!genObject.IsFullyCharged && genObject.IsActived)
                Managers.GameServer.SendPutBattery(target.Id);
    }

    void ReqStartEscapeKey()
    {
        if (Target == null) return;

        Managers.GameServer.SendStartActivateEscapeKey(Target.Id);
    }

    public void PlayEscapeKey(bool isPlay)
    {
        var elBox = Target as ElectricBox;
        if (elBox != null)
        {
            ShowEscapeProgress(isPlay, elBox.Gauge);
            if (isPlay)
            {
                var keyData = Managers.Data.GetData(GameDataManager.DATA_TYPE.object_key, elBox.ObjData.sub_obj_uid) as ObjectKeyGameData;
                var time = keyData.activation_time * 0.001f;

                if (progressCoroutine != null) StopCoroutine(progressCoroutine);
                progressCoroutine = StartCoroutine(PlayEscapeKeyCO(elBox.Gauge / time, 1, time));
            }
        }
        if (!isPlay)
        {
            if (progressCoroutine != null) StopCoroutine(progressCoroutine);
            progressCoroutine = null;
        }
    }
    //void ReqEndEscapeKey()
    //{
    //    return;
    //    if (target == null) return;
    //    var packet = new CSKeyEnd();
    //    packet.KeyId = target.Id;

    //    Managers.Network.SendMessage(PacketId.CSKeyEnd, packet);
    //}

    void ReqStartOpenDoor()
    {
        if (Target == null) return;

        if (!Game.Instance.GameRoom.CanOpenDoor())
        {
            PopupCanvas.Instance.ShowFadeText("문_조건경고1");
            return;
        }
        if (Game.Instance.GameRoom.IsAnyDoorOpened())
        {
            PopupCanvas.Instance.ShowFadeText("문_조건경고2");
            return;
        }

        Managers.GameServer.SendStartOpenEscapeDoor(Target.Id);
    }

    public void PlayOpenDoor(bool isPlay)
    {
        var escapeKey = Target as EscapeKey;
        if (escapeKey != null)
        {
            ShowEscapeProgress(isPlay, escapeKey.Gauge);

            if (isPlay)
            {
                var keyData = Managers.Data.GetData(GameDataManager.DATA_TYPE.object_key, escapeKey.ObjData.sub_obj_uid) as ObjectKeyGameData;
                var time = keyData.activation_time * 0.001f;

                progressCoroutine = StartCoroutine(PlayOpenDoorCO(escapeKey.Gauge / time, 1, time));
            }
        }
        if (!isPlay)
        {
            if (progressCoroutine != null) StopCoroutine(progressCoroutine);
            progressCoroutine = null;
        }
    }

    public void ShowEscapeProgress(bool isShow, float startValue = 0)
    {
        if (_uiGame != null)
        {
            _uiGame.ShowEscapeProgress(isShow);
            _uiGame.SetEscapeProgressBar(startValue);
        }
    }

    IEnumerator PlayEscapeKeyCO(float startValue, float targetValue, float time)
    {
        _uiGame.SetEscapeProgressBar(startValue);

        float playTime = startValue * time;
        while (true)
        {
            if (playTime > time)
                playTime = time;

            var value = playTime;
            _uiGame.SetEscapeProgressBar(value == 0 ? 0 : value / time);

            if (playTime == time)
            {
                ShowEscapeProgress(false);
                yield break;
            }

            playTime += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator PlayOpenDoorCO(float startValue, float targetValue, float time)
    {
        _uiGame.SetEscapeProgressBar(startValue);

        float playTime = startValue * time;
        while (true)
        {
            if (_game.GameRoom.IsAnyDoorOpened() == true)
            {
                PopupCanvas.Instance.ShowFadeText("문_조건경고2");
                ShowEscapeProgress(false);
                yield break;
            }

            if (playTime > time)
                playTime = time;

            var value = playTime;
            _uiGame.SetEscapeProgressBar(value == 0 ? 0 : value / time);

            if (playTime == time)
            {
                SendOpenDoor();
                ShowEscapeProgress(false);
                yield break;
            }

            playTime += Time.deltaTime;
            yield return null;
        }
    }

    public void SendOpenDoor()
    {
        if (Target == null) return;

        var prop = Target as PropController;
        if (prop)
        {
            Managers.GameServer.SendCompleteOpenEscapeDoor(prop.Id);
        }
    }

    public EscapeKey GetNearEscapeDoor()
    {
        if (Target == null) return null;

        var escapeKey = Target.GetComponent<EscapeKey>();
        return escapeKey;
    }

    void SetStanmina()
    {
        if (Character.IsChaser)
        {

            staminaUseSecondTime = 10f;
            staminaRestoreTotalTime = 8f;

            staminaRestoreSecondValue = 100.0f / staminaUseSecondTime;
            staminaUseValue = 100.0f / staminaRestoreTotalTime;

        }

        stMax = staminaUseSecondTime * 1000f;
        stValue = stMax;

        staminaRestoreSecondValue = stMax / staminaRestoreTotalTime;
        staminaUseValue = stMax / staminaUseSecondTime;
    }

    void RequestRideFromTarget()
    {
        var propCont = Target as PropController;
        if (propCont == null) return;

        SBDebug.Log("RequestRideFromTarget()");
        Managers.GameServer.SendUseVehicle(Character.Id, Target.Id);
    }

    public bool RequestGetOffVehicle()
    {
        if (Character.IsVehicle)
        {
            Managers.GameServer.SendGetOffVehicle(Character.Id);
            return true;
        }

        return false;
    }
    public bool RequestGetOffHide()
    {
        if (Character.State == CreatureStatus.Hiding)
        {
            Managers.GameServer.SendHide(Character.Id);
            return true;
        }
        return false;
    }

    void RequestUserFromVent()
    {
        var propCont = Target as PropController;
        if (propCont == null) return;

        SBDebug.Log("RequestUserFromVent()");
        Managers.GameServer.SendUseVent(Character.Id, Target.Id);
    }

    public void RefreshObjectVisible()
    {
        var sqrtbaseDis = 20.0f * 20.0f;

        var objs = Managers.Object.Objects;
        foreach (var obj in objs)
        {
            var go = obj.Value;
            if (go == null) continue;
            var baseCont = obj.Value.GetComponent<BaseObject>();
            var vecDis = new Vector2(Mathf.Abs(go.transform.position.x - Character.transform.position.x), Mathf.Abs(go.transform.position.y - Character.transform.position.y));
            baseCont.ShowRenderer(sqrtbaseDis > vecDis.sqrMagnitude);
        }
    }
}

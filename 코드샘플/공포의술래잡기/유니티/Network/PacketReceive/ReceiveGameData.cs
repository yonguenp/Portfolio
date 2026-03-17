using SBSocketPacketLib;
using SBSocketSharedLib;
using UnityEngine;

public partial class PacketManager
{
    private void ReceiveSCBcDamaged(TcpServerSession serverSession_, SBPacket packet_)
    {
        SCBcDamaged resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }

        var targetCharacter = Managers.Object.FindCharacterById(resPacket.TargetID);
        if (targetCharacter)
        {
            targetCharacter.OnDamage(resPacket.AttackerID, resPacket.TargetHp, resPacket.TargetShieldCnt, (DamageType)resPacket.DamageType, resPacket.DamagePoint);
            if (resPacket.TargetHp == 0)
            {
                Game.Instance.GameRoom.IncreaseKillCount(resPacket.AttackerID);
                if (targetCharacter.IsVehicle)
                    Game.Instance.GameRoom.IncreaseVehicleKillCount(resPacket.AttackerID);
            }
        }
        else
        {
            var summonedObj = Managers.Object.FindBaseObjectById(resPacket.TargetID) as ProjectileObject;
            if (summonedObj != null)
            {
                summonedObj.OnDamage(resPacket.AttackerID, resPacket.TargetHp, resPacket.TargetShieldCnt, (DamageType)resPacket.DamageType, resPacket.DamagePoint);
            }
            else
            {
                SBDebug.Log($"==============ReceiveSCBcDamaged CharacterController is null {resPacket.TargetID}");
            }
        }
    }

    private void ReceiveSCBcBroken(TcpServerSession serverSession_, SBPacket packet_)
    {
        SCBcBroken resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        if (resPacket.ErrorCode != (int)ErrorCode.Success)
        {
            SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)}");
            //SBLog.PrintError($"{Error.GetMessage(resPacket.ErrorCode)}");
            return;
        }

        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }
        //SBDebug.Log($"ProcessSCBcUnlockKey [{resPacket.Position.X}/[{resPacket.Position.Y}], [{resPacket.UnlockStatus}], [{resPacket.UnlockedObjectCount}]");
        //PlayerManager.Instance.ObjectDamage(resPacket.AttackerInfo, resPacket.TargetInfo);

        Game.Instance.ObjectDamage(resPacket.TargetInfo.MapObjectId);

    }

    private void ReceiveSCBcCreateBuffs(TcpServerSession serverSession_, SBPacket packet_)
    {
        SCBcCreateBuffs resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }

        var co = Managers.Object.FindCharacterById(resPacket.PlayerId);
        if (co == null)
        {
            SBDebug.Log($"CharacterObject is null.");
            //SBLog.PrintError($"TSCPacket packet is null.");
            return;
        }
        co.AddBuffs(resPacket.EffectIds);
    }

    private void ReceiveSCBcDeleteBuffs(TcpServerSession serverSession_, SBPacket packet_)
    {
        SCBcDeleteBuffs resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }

        var co = Managers.Object.FindCharacterById(resPacket.PlayerId);
        if (co == null)
        {
            SBDebug.Log($"CharacterObject is null.");
            //SBLog.PrintError($"TSCPacket packet is null.");
            return;
        }
        co.DeleteBuffs(resPacket.EffectIds);
    }

    private void ReceiveSCBcDespawn(TcpServerSession serverSession_, SBPacket packet_)
    {
        // var gs = UnityEngine.GameObject.FindObjectOfType<GameScript>();
        SCBcDespawn resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        if (resPacket.ErrorCode != (sbyte)ErrorCode.Success)
        {
            SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)}");
            return;
        }

        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }

        Game.Instance.Despawn(resPacket);
    }

    private void ReceiveSCBcEscapeEnd(TcpServerSession serverSession_, SBPacket packet_)
    {
        SCBcEscapeEnd resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        if (resPacket.ErrorCode != (int)ErrorCode.Success)
        {
            SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)}");
            return;
        }
        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }

        var game = Game.Instance;
        if (game)
        {
            game.SetEscapeOpenDoor(resPacket.EscapeId, resPacket.InteractionTime);
            game.PlayOpenDoor(false, resPacket.PlayerId);
        }
    }

    private void ReceiveSCBcEscapeNotify(TcpServerSession serverSession_, SBPacket packet_)
    {
        SCBcEscapeNotify resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }

        int count = resPacket.EscapeInfos.Count;
        SBDebug.Log($"count : [{count}]");
        foreach (var obj in resPacket.EscapeInfos)
        {
            SBDebug.Log($"[{obj.Key}][{obj.Value}]");
            Game.Instance.NotifyEscapeDoor(obj.Key, obj.Value);
        }
    }

    private void ReceiveSCBcEscapeRoom(TcpServerSession serverSession_, SBPacket packet_)
    {
        SCBcEscapeRoom resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        if (resPacket.ErrorCode != (int)ErrorCode.Success)
        {
            SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)}");
            if (resPacket.ObjectId == Managers.UserData.MyUserID.ToString())
            {
                if (EscapeEvent.Instance)
                    EscapeEvent.Instance.Show(true);

                Game.Instance.PlayerController.Character.SetEscaped(Vector2.zero);
            }
            return;
        }

        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.PlayerController.Character.SetEscaped(Vector2.zero);
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }

        Game.Instance.EscapeRoom(resPacket.ObjectId);
    }

    private void ReceiveSCBcEscapeStart(TcpServerSession serverSession_, SBPacket packet_)
    {
        SCBcEscapeStart resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        if (resPacket.ErrorCode != (int)ErrorCode.Success)
        {
            SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)}");
            //SBLog.PrintError($"{Error.GetMessage(resPacket.ErrorCode)}");
            return;
        }

        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }

        var game = Game.Instance;
        if (game) game.PlayOpenDoor(true, resPacket.PlayerId);
    }

    private void ReceiveSCBcGameResult(TcpServerSession serverSession_, SBPacket packet_)
    {
        SCBcGameResult resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        if (resPacket.ErrorCode != (int)ErrorCode.Success)
        {
            SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)}");
            //SBLog.PrintError($"{Error.GetMessage(resPacket.ErrorCode)}");
            return;
        }

        //if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        //{
        //    Game.Instance.AddTempPacket(serverSession_, packet_);
        //    return;
        //}

        Game.Instance.GameResult(resPacket);
    }

    private void ReceiveSCBcKeyEnd(TcpServerSession serverSession_, SBPacket packet_)
    {
        SCBcKeyEnd resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        if (resPacket.ErrorCode != (int)ErrorCode.Success)
        {
            SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)}");
            //SBLog.PrintError($"{Error.GetMessage(resPacket.ErrorCode)}");
            return;
        }

        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }

        var game = Game.Instance;
        if (game)
        {
            game.SetEscapeKey(resPacket.KeyId, resPacket.InteractionTime);
            game.PlayEscapeKey(false, resPacket.PlayerId);
        }
    }

    private void ReceiveBcSCKeyStart(TcpServerSession serverSession_, SBPacket packet_)
    {
        SCBcKeyStart resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        if (resPacket.ErrorCode != (int)ErrorCode.Success)
        {
            SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)}");
            //SBLog.PrintError($"{Error.GetMessage(resPacket.ErrorCode)}");
            return;
        }

        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }

        var game = Game.Instance;
        if (game) game.PlayEscapeKey(true, resPacket.PlayerId);
    }

    private void ReceiveSCBcKeyNotify(TcpServerSession serverSession_, SBPacket packet_)
    {
        SCBcKeyNotify resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        //if (resPacket.ErrorCode != (int)ErrorCode.Success)
        //{
        //    SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)}");
        //    //SBLog.PrintError($"{Error.GetMessage(resPacket.ErrorCode)}");
        //    return;
        //}
        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }

        int count = resPacket.KeyInfos.Count;
        SBDebug.Log($"count : [{count}]");
        foreach (var obj in resPacket.KeyInfos)
        {
            SBDebug.Log($"[{obj.Key}][{obj.Value}]");
            Game.Instance.NotifyElectricBox(obj.Key, obj.Value);
        }
    }

    private void ReceiveSCBcLeaveGame(TcpServerSession serverSession_, SBPacket packet_)
    {
        SCBcLeaveGame resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        if (resPacket.ErrorCode != (sbyte)ErrorCode.Success)
        {
            SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)}");
            //SBLog.PrintError($"{Error.GetMessage(resPacket.ErrorCode)}");
            return;
        }

        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }

        SBDebug.Log($"recv packet SCBroadcastLeaveGame");

        CharacterObject user = Managers.Object.FindCharacterById(resPacket.GameObjectId);
        if (user != null)
        {
            user.gameObject.SetActive(false);
        }
    }

    private void ReceiveSCBcMoveUpdate(TcpServerSession serverSession_, SBPacket packet_)
    {
        SCBcMoveUpdate resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        if (resPacket.ErrorCode != (int)ErrorCode.Success)
        {
            SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)}");
            //SBLog.PrintError($"{Error.GetMessage(resPacket.ErrorCode)}");
            return;
        }

        //SBDebug.Log($"MoveUpdate ID:{resPacket.ObjectId}, Dir:{resPacket.Direction}, Pos:{resPacket.Position}, Time:{resPacket.LastMoveTime}");
        //PlayerManager.Instance.MoveUpdate(resPacket.ObjectId, resPacket.Direction, resPacket.Position, resPacket.LastMoveTime);
        //PlayerManager.Instance.Move(resPacket);
    }

    private void ReceiveSCBcProjectileDespawn(TcpServerSession serverSession_, SBPacket packet_)
    {
        SCBcProjectileDespawn resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            SBDebug.Log("AddTempPacket:::패킷 추가 합니다");
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }

        var game = Game.Instance;

        foreach (var item in resPacket.DelObjects)
        {
            game.ProjectileDespawn(item.ObjectId, item.Position);
        }
        //game.ProjectileDespawn(resPacket.ObjectId, resPacket.Position);
    }

    private void ReceiveSCBcProjectileSpawn(TcpServerSession serverSession_, SBPacket packet_)
    {
        SCBcProjectileSpawn resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }

        //if (resPacket.ErrorCode != (int)ErrorCode.Success)
        //{
        //    SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)}");
        //    //SBLog.PrintError($"{Error.GetMessage(resPacket.ErrorCode)}");
        //    return;
        //}

        var game = Game.Instance;
        if (game != null)
            game.ProjectileSpawn(resPacket);
    }

    private void ReceiveSCBcSkillCasting(TcpServerSession serverSession_, SBPacket packet_)
    {
        Deserialize(serverSession_, packet_, out SCBcSkillCasting resPacket);
        if (resPacket == null)
            return;

        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }

        SBDebug.Log($"SCBcSkillCasting Received : {SBCommonLib.Json.SBJson.ToString(resPacket)}");

        var character = Managers.Object.FindCharacterById(resPacket.PlayerId);
        if (character)
        {
            character.OnSkillCasting(resPacket.SkillId, resPacket.SkillDir, character);
        }
    }


    private void ReceiveSCBcSkill(TcpServerSession serverSession_, SBPacket packet_)
    {
        Deserialize(serverSession_, packet_, out SCBcSkill resPacket);
        if (resPacket == null)
            return;

        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }

        var character = Managers.Object.FindCharacterById(resPacket.PlayerId);
        if (character)
        {
            character.OnSkill(resPacket.SkillId, resPacket.SkillDir);
        }
    }

    private void ReceiveSCBcSpawn(TcpServerSession serverSession_, SBPacket packet_)
    {
        // var gs = UnityEngine.GameObject.FindObjectOfType<GameScript>();
        SCBcSpawn resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        if (resPacket.ErrorCode != (sbyte)ErrorCode.Success)
        {
            SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)}");
            //SBLog.PrintError($"{Error.GetMessage(resPacket.ErrorCode)}");
            return;
        }

        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }

        foreach (RespawnObjectInfo info in resPacket.RespawnObjectList)
        {
            Game.Instance.ObjectSpawn(info);
        }
    }

    private void ReceiveSCBcStatusSync(TcpServerSession serverSession_, SBPacket packet_)
    {
        SCBcStatusSync resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }

        var bo = Managers.Object.FindBaseObjectById(resPacket.PlayerId);
        if (bo == null)
        {
            SBDebug.Log($"BaseObject is null.");
            //SBLog.PrintError($"TSCPacket packet is null.");
            return;
        }
        bo.SetStatsInfo(resPacket.StatInfo);

        var co = bo as CharacterObject;
        if (co != null && co.IsMe)
        {
            Game.Instance.PlayerController.ChangeReducedCoolTime(resPacket.StatInfo.ReduceAttackTime, resPacket.StatInfo.ReduceSkillTime);
        }
    }

    private void ReceiveSCBcUnlockEscape(TcpServerSession serverSession_, SBPacket packet_)
    {
        SCBcUnlockEscape resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        if (resPacket.ErrorCode != (int)ErrorCode.Success)
        {
            SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)}");
            //SBLog.PrintError($"{Error.GetMessage(resPacket.ErrorCode)}");
            return;
        }

        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }

        Game.Instance.SetEscapeOpenDoor(resPacket.EscapeId, resPacket.EscapeDoorOpenTime);
    }

    private void ReceiveSCBcUnlockKey(TcpServerSession serverSession_, SBPacket packet_)
    {
        SCBcUnlockKey resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        if (resPacket.ErrorCode != (int)ErrorCode.Success)
        {
            SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)}");
            //SBLog.PrintError($"{Error.GetMessage(resPacket.ErrorCode)}");
            return;
        }

        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }

        //PlayerManager.Instance.SetEscapeKey(resPacket.UnlockedObjectCount, resPacket.UnlockStatus, resPacket.Position.X, resPacket.Position.Y);
        //SBDebug.Log($"ProcessSCBcUnlockKey [{resPacket.Position.X}/[{resPacket.Position.Y}], [{resPacket.UnlockStatus}], [{resPacket.UnlockedObjectCount}]");

        Game.Instance.SetEscapeKey(resPacket.UnlockedObjectCount, resPacket.UnlockStatus, resPacket.KeyId);
    }

    private void ReceiveSCBcUseVehicle(TcpServerSession serverSession_, SBPacket packet_)
    {
        SCBcUseVehicle resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        if (resPacket.ErrorCode != (int)ErrorCode.Success)
        {
            SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)}");
            //SBLog.PrintError($"{Error.GetMessage(resPacket.ErrorCode)}");
            return;
        }

        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }

        //SBDebug.Log($"ProccessSCBcUseVehicle pID {resPacket.PlayerId}, vehicleId {resPacket.VehicleId}");

        //var go = Managers.Object.FindById(resPacket.PlayerId);
        //if (go == null) return;
        var character = Managers.Object.FindCharacterById(resPacket.PlayerId);
        if (character == null) return;
        if (resPacket.VehicleId.Equals("NULL"))
            character.ClearVehicle();
        else
        {
            var obj = Managers.Object.GetVehicle(resPacket.VehicleId);
            obj.gameObject.SetActive(false);
            ObjectVehicleGameData vehicleData = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.object_vehicle, obj.ObjData.sub_obj_uid) as ObjectVehicleGameData;
            character.SetVehicle(vehicleData);
        }
    }

    private void ReceiveSCBcGetOffVehicle(TcpServerSession serverSession_, SBPacket packet_)
    {
        SCBcGetOffVehicle resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        if (resPacket.ErrorCode != (int)ErrorCode.Success)
        {
            SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)}");
            //SBLog.PrintError($"{Error.GetMessage(resPacket.ErrorCode)}");
            return;
        }

        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }


        //SBDebug.Log($"ProccessSCBcUseVehicle pID {resPacket.PlayerId}, vehicleId {resPacket.VehicleId}");

        var character = Managers.Object.FindCharacterById(resPacket.PlayerId);
        if (character == null) return;

        character.ClearVehicle();
    }

    private void ReceiveSCBcMoveStart(TcpServerSession serverSession_, SBPacket packet_)
    {
        SCBcMove resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        if (resPacket.ErrorCode != (sbyte)ErrorCode.Success)
        {
            SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)}");
            //SBLog.PrintError($"{Error.GetMessage(resPacket.ErrorCode)}");
            return;
        }

        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }

        //var go = Managers.Object.FindById(resPacket.ObjectId);
        //if (go == null)
        //    return;

        var state = resPacket.Status;
        var moveState = resPacket.MoveStatus;
        var moveDirX = resPacket.MoveDir.X;
        var moveDirY = resPacket.MoveDir.Y;
        var posX = resPacket.Position.X;
        var posY = resPacket.Position.Y;
        var isForceMove = resPacket.ForcePos;

        //서버 위치 확인용 코드 현재 삭제
        //#if UNITY_EDITOR
        //        var mark = Managers.Resource.Instantiate("Object/Icon/x_icon");
        //        mark.transform.position = new Vector2(posX, posY);        
        //        GameObject.Destroy(mark.gameObject, 10.0f);
        //#endif
        var character = Managers.Object.FindCharacterById(resPacket.ObjectId);
        if (character == null) return;
        if (state == (byte)CreatureStatus.Idle)
            character.MoveEnd((CreatureStatus)state, (MoveStatus)moveState, moveDirX, moveDirY, posX, posY, isForceMove);
        else
            character.MoveStart((CreatureStatus)state, (MoveStatus)moveState, moveDirX, moveDirY, posX, posY, isForceMove);

        //차후 charactercontroller를 상속받는 인터페이스에서 처리할 수 있도록 처리
        if (moveState == (byte)MoveStatus.Vent)
        {
            var prop = Managers.Object.GetPropinArea(new UnityEngine.Vector3(posX, posY, 0));
            if (prop != null)
            {
                if (Managers.UserData.MyUserID.Equals(resPacket.ObjectId))
                {
                    Game.Instance.PlayerController.ResetTarget();
                }

                if (prop.MapObjectType == ObjectGameData.MapObjectType.Vent)
                {
                    prop.PlayAnim("out");
                }
            }
        }
    }

    private void ReceiveSCBcMoveEnd(TcpServerSession serverSession_, SBPacket packet_)
    {
        SCBcMoveEnd resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        if (resPacket.ErrorCode != (int)ErrorCode.Success)
        {
            SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)}");
            //SBLog.PrintError($"{Error.GetMessage(resPacket.ErrorCode)}");
            return;
        }

        //SBDebug.Log($"ProcessSCMoveStart[{resPacket.MoveEndTime}]");
        //PlayerManager.Instance.MoveEnd(resPacket.ObjectId,resPacket.Position, resPacket.MoveEndTime);

        //var sceneType = GameManager.Instance.SceneType;
        //if (sceneType == eSceneType.Lobby)
        //    LobbyPlayerManager.Instance.MoveEnd(resPacket.ObjectId, resPacket.Position, resPacket.MoveEndTime);
        //else if (sceneType == eSceneType.Game)
        //    PlayerManager.Instance.MoveEnd(resPacket.ObjectId, resPacket.Position, resPacket.MoveEndTime);
    }

    private void ReceiveSCMoveStart(TcpServerSession serverSession_, SBPacket packet_)
    {
        SCBcMoveStart resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        if (resPacket.ErrorCode != (int)ErrorCode.Success)
        {
            SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)}");
            //SBLog.PrintError($"{Error.GetMessage(resPacket.ErrorCode)}");
            return;
        }

        //SBDebug.Log($"ProcessSCMoveStart[{resPacket.MoveStartTime}]");
        //SBDebug.Log($"MoveStart[{resPacket.Position}], MoveDir[{resPacket.Direction}]");
        //PlayerManager.Instance.MoveStart(resPacket.ObjectId, resPacket.Direction, resPacket.Position, (CreatureStatus)resPacket.Status, resPacket.MoveStartTime);

        //var sceneType = GameManager.Instance.SceneType;
        //if (sceneType == eSceneType.Lobby)
        //    LobbyPlayerManager.Instance.MoveStart(resPacket.ObjectId, resPacket.Direction, resPacket.Position, (CreatureStatus)resPacket.Status, resPacket.MoveStartTime);
        //else if (sceneType == eSceneType.Game)
        //    PlayerManager.Instance.MoveStart(resPacket.ObjectId, resPacket.Direction, resPacket.Position, (CreatureStatus)resPacket.Status, resPacket.MoveStartTime);
    }

    private void ReceiveSCObject(TcpServerSession serverSession_, SBPacket packet_)
    {
        SCObjectInfo resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        if (resPacket.ErrorCode != (sbyte)ErrorCode.Success)
        {
            SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)}");
            //SBLog.PrintError($"{Error.GetMessage(resPacket.ErrorCode)}");
            return;
        }

        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }

        SBDebug.Log($"recv packet SCPlayer");
    }

    private void ReceiveSCObjectList(TcpServerSession serverSession_, SBPacket packet_)
    {
        SCUserList resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        if (resPacket.ErrorCode != (sbyte)ErrorCode.Success)
        {
            SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)}");
            //SBLog.PrintError($"{Error.GetMessage(resPacket.ErrorCode)}");
            return;
        }

        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }

        SBDebug.Log($"recv packet SCPlayerList");
    }

    private void ReceiveSCSkillCasting(TcpServerSession serverSession_, SBPacket packet_)
    {
        Deserialize(serverSession_, packet_, out SCSkillCasting resPacket);
        if (resPacket == null)
            return;

        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }

        switch ((ErrorCode)resPacket.ErrorCode)
        {
            case ErrorCode.Success:
                SBDebug.Log($"SCSkillCasting Success");
                return;
            case ErrorCode.SystemError:
                SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)}");
                Game.Instance.PlayerController.Character.SetState(CreatureStatus.Idle);
                Game.Instance.PlayerController.Character.ClearSkillCastring();
                return;
            default:
                Game.Instance.PlayerController.Character.ClearSkillCastring();
                SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)}");
                return;
        }
    }

    private void ReceiveSCSkill(TcpServerSession serverSession_, SBPacket packet_)
    {
        Deserialize(serverSession_, packet_, out SCSkill resPacket);
        if (resPacket == null)
            return;

        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }

        switch ((ErrorCode)resPacket.ErrorCode)
        {
            case ErrorCode.Success:
                Game.Instance.PlayerController.OnResetSkillCoolTime(resPacket.SkillId);
                return;
            case ErrorCode.SystemError:
                {
                    SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)}");
                    Game.Instance.PlayerController.Character.SetState((CreatureStatus)resPacket.CurrState);
                }
                return;
            default:
                SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)}");
                return;
        }
    }

    private void ReceiveSCBcStateChanged(TcpServerSession serverSession_, SBPacket packet_)
    {
        SCBcStateChanged resPacket = null;
        Deserialize(serverSession_, packet_, out resPacket);
        if (resPacket == null)
            return;

        if (resPacket.ErrorCode != (int)ErrorCode.Success)
        {
            SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)}");
            return;
        }

        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }

        Game.Instance.OnStateChanged(resPacket);
    }

    private void ReceiveSCBatteryCreater(TcpServerSession serverSession_, SBPacket packet_)
    {
        Deserialize(serverSession_, packet_, out SCBatteryCreater resPacket);
        if (resPacket == null)
            return;

        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }

        switch ((ErrorCode)resPacket.ErrorCode)
        {
            case ErrorCode.Success:
                break;
            default:
                SBDebug.Log($"{Error.GetMessage(resPacket.ErrorCode)} / {(FailReason)resPacket.FailReason}");
                break;
        }

        Game.Instance.CheckBatteryStatus();
    }

    private void ReceiveSCBcBatteryCreate(TcpServerSession serverSession_, SBPacket packet_)
    {
        Deserialize(serverSession_, packet_, out SCBcBatteryCreate resPacket);
        if (resPacket == null)
            return;

        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }

        var co = Managers.Object.FindCharacterById(resPacket.PlayerId);
        if (co != null)
        {
            co.SetBattery(resPacket.BatteryCnt);
            co.OnInteract();
        }

        var bo = Managers.Object.FindBaseObjectById(resPacket.GameObjectId);
        if (bo != null)
        {
            var bc = bo.GetComponent<BatteryCreator>();
            if (bc != null)
            {
                bc.OnGetBattery();
            }
        }

        Game.Instance.CheckBatteryStatus();
    }

    private void ReceiveSCBcBatteryDrop(TcpServerSession serverSession_, SBPacket packet_)
    {
        Deserialize(serverSession_, packet_, out SCBcBatteryDrop resPacket);
        if (resPacket == null)
            return;

        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }

        var co = Managers.Object.FindCharacterById(resPacket.PlayerId);
        if (co != null)
        {
            co.DropBattery(resPacket.DropGameObjects);
        }

        Game.Instance.CheckBatteryStatus();
    }

    private void ReceiveSCBcBatteryPickUp(TcpServerSession serverSession_, SBPacket packet_)
    {
        Deserialize(serverSession_, packet_, out SCBcBatteryPickUp resPacket);
        if (resPacket == null)
            return;

        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }

        var co = Managers.Object.FindCharacterById(resPacket.PlayerId);
        if (co != null)
        {
            co.SetBattery(resPacket.BatteryCnt, true);
        }

        foreach (var objectId in resPacket.GameObjectIds)
        {
            Managers.Object.Remove(objectId);
        }

        Game.Instance.CheckBatteryStatus();
    }

    private void ReceiveSCBcBatteryGenerater(TcpServerSession serverSession_, SBPacket packet_)
    {
        Deserialize(serverSession_, packet_, out SCBcBatteryGenerater resPacket);
        if (resPacket == null)
            return;

        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }

        var amount = resPacket.ChargeCnt;

        Game.Instance.GameRoom.SetChargeBatteryCount(resPacket.PlayerId, amount);

        var co = Managers.Object.FindCharacterById(resPacket.PlayerId);
        if (co != null)
        {
            co.AddBatteryChargeCount(amount);
            co.SetBattery(0);   // 배터리를 다 넣었으므로 배터리 바를 초기화해준다
            co.OnInteract();
            Game.Instance.UIGame.CreateBatteryMessage(amount, co.UserName, co.CharacterType);
        }

        var bo = Managers.Object.FindBaseObjectById(resPacket.GameObjectId);
        if (bo != null)
        {
            var bg = bo.GetComponent<BatteryGenerator>();
            if (bg != null)
            {
                bg.OnSetBattery(resPacket.CollectCnt, co.IsMe);
            }
        }

        Game.Instance.CheckBatteryStatus();
    }

    private void ReceiveScBcDeleteDropObjects(TcpServerSession serverSession_, SBPacket packet_)
    {
        Deserialize(serverSession_, packet_, out SCBcDeleteDropObjects resPacket);
        if (resPacket == null)
            return;

        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }

        foreach (var objectId in resPacket.GameObjects)
        {
            Managers.Object.Remove(objectId);
        }
    }

    private void ReceiveScBcGamePoint(TcpServerSession serverSession_, SBPacket packet_)
    {
        Deserialize(serverSession_, packet_, out SCBcGamePoint resPacket);
        if (resPacket == null)
            return;
        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }

        SBDebug.Log("SCBcGamePoint");
        Game.Instance.ProcessGamePoint(resPacket.PlayerId, resPacket.RewardInfos);
    }

    private void ReceiveSCBcEmoticon(TcpServerSession serverSession_, SBPacket packet_)
    {
        Deserialize(serverSession_, packet_, out SCBcEmoticon resPacket);
        if (resPacket == null)
            return;

        if (!Game.Instance.IsPlay && Managers.PlayData.IsResume && Managers.PlayData.ResumeGameDataPacket != null)
        {
            Game.Instance.AddTempPacket(serverSession_, packet_);
            return;
        }

        SBDebug.Log("SCBcGameEmotion");

        var co = Managers.Object.FindCharacterById(resPacket.PlayerId);
        if (co)
            co.SetEmotion(resPacket.EmoticonId);
    }

    private void ReceiveSCAlivePing(TcpServerSession serverSession_, SBPacket packet_)
    {
        //Deserialize(serverSession_, packet_, out SCAlivePing resPacket);
        //if (resPacket == null)
        //    return;

        //SBDebug.Log("SCAlivePing");
        //var time = resPacket.Time;
        //Managers.Network.SendPingPong(time);
    }

}

using SBCommonLib;
using SBSocketPacketLib;
using SBSocketSharedLib;

public partial class GameServerManager
{
    public bool SendPing(ref long time)
    {
        time = SBUtil.GetCurrentMilliSecTimestamp();
        SendMessage(PacketId.CSPing,
            new CSPing
            {
                Time = time,
            });

        return true;
    }

    public void GameServerEnter()
    {
        SendMessage(PacketId.CGGameServerEnter,
        new CGGameServerEnter
        {
            UserNo = Managers.UserData.MyUserID,
            SessionToken = Managers.UserData.MyWebSessionID,
        });

        Managers.Instance.Invoke("FailGameServerConnect", 3.0f);
    }

    public void ReqGamePlayerInfo()
    {
        //SendMessage(PacketId.CGGamePlayerInfo,
        //new CGGamePlayerInfo
        //{

        //});
    }
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// 인게임
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public bool SendStartActivateEscapeKey(string id)
    {
        SendMessage(PacketId.CSKeyStart,
           new CSKeyStart
           {
               KeyId = id,
           });

        return true;
    }

    public bool SendStartOpenEscapeDoor(string id)
    {
        SendMessage(PacketId.CSEscapeStart,
           new CSEscapeStart
           {
               EscapeId = id,
           });

        return true;
    }

    // 배터리를 가져온다
    public bool SendGetBattery(string id)
    {
        SendMessage(PacketId.CSBatteryCreater,
            new CSBatteryCreater
            {
                GameObjectId = id,
            });

        return true;
    }

    // 들고 있는 배터리를 전부 집어넣는다
    public bool SendPutBattery(string id)
    {
        SendMessage(PacketId.CSBatteryGenerater,
            new CSBatteryGenerater
            {
                GameObjectId = id,
            });

        return true;
    }

    public bool SendCompleteOpenEscapeDoor(string id)
    {
        SendMessage(PacketId.CSUnlockEscape, new CSUnlockEscape
        {
            EscapeId = id,
            InteractionTime = 0
        });

        return true;
    }

    //public bool SendEscapeRoom(string id)
    //{
    //    SendMessage(PacketId.CSEscapeRoom,
    //       new CSEscapeRoom
    //       {
    //           ObjectId = id
    //       });

    //    return true;
    //}

    public bool SendHide(string id)
    {
        SendMessage(PacketId.CSHiding,
            new CSHiding
            {
                HidingId = id,
            });

        return true;
    }

    public bool SendUseVehicle(string playerID, string vehicleID)
    {
        SendMessage(PacketId.CSUseVehicle, new CSUseVehicle
        {
            PlayerId = playerID,
            VehicleId = vehicleID
        });

        return true;
    }

    public bool SendGetOffVehicle(string playerID)
    {
        SendMessage(PacketId.CSGetOffVehicle, new CSGetOffVehicle
        {
            PlayerId = playerID
        });

        return true;
    }

    public bool SendUseVent(string playerID, string ventID)
    {
        SendMessage(PacketId.CSUseVent, new CSUseVent
        {
            PlayerId = playerID,
            VentId = ventID
        });

        return true;
    }

    public bool SendUseSkillCasting(int skillID, Vec2Float dir, Vec2Float pos)
    {
        SBDebug.Log("Send CSSkillCasting");
        // 방향이 없으면 기본적으로는 0도 방향, 즉 오른쪽을 가리킨다
        if (dir.X == 0 && dir.Y == 0)
        {
            dir.X = 1;
        }

        SendMessage(PacketId.CSSkillCasting,
        new CSSkillCasting()
        {
            SkillId = skillID,
            SkillDir = dir,
            Position = pos
        });

        return true;

    }

    public bool SendUseSkill(int skillID, Vec2Float dir, Vec2Float pos)
    {
        // 방향이 없으면 기본적으로는 0도 방향, 즉 오른쪽을 가리킨다
        if (dir.X == 0 && dir.Y == 0)
        {
            dir.X = 1;
        }

        SendMessage(PacketId.CSSkill,
        new CSSkill()
        {
            SkillId = skillID,
            SkillDir = dir,
            Position = pos
        });

        return true;
    }

    public bool SendMove(byte state, byte moveState, float dirX, float dirY, float x, float y)
    {
        if (state == (byte)CreatureStatus.Idle)
            SBDebug.Log($"====SendMove {state}, moveSt {moveState} dx({dirX}), dy({dirY}) x({x}), y({y})");

        SendMessage(PacketId.CSMove,
        new CSMove
        {
            MoveDir = new Vec2Float(dirX, dirY),
            Position = new Vec2Float(x, y),
            Status = state,
            MoveStatus = moveState,
        });

        return true;
    }

    public bool SendMove(CSMove move)
    {
        SendMessage(PacketId.CSMove, move);

        return true;
    }

    public bool SendExitGame()
    {
#if UNITY_EDITOR
        if (GameConfig.Instance.USE_DUMMY)
        {
            SendMessage(PacketId.CSLeaveGameWithDummy, new CSLeaveGame());
        }
        else
#endif
        {
            SendMessage(PacketId.CSLeaveGame, new CSLeaveGame());
        }

        return true;
    }

    public bool SendResume(bool resume)
    {
        SendMessage(PacketId.CSReconnectRoom, new CSReconnectRoom
        {
            UserId = Managers.UserData.MyUserID,
            SessionToken = Managers.UserData.MyWebSessionID,
            Result = resume
        });
        return true;
    }
    public bool SendEmotion(ushort emotionType)
    {
        SendMessage(PacketId.CSEmoticon, new CSEmoticon
        {
            EmoticonId = emotionType
        });

        return true;
    }

    public bool SendGameServerEnterGame()
    {
#if UNITY_EDITOR
        if (GameConfig.Instance.USE_DUMMY && !Managers.PlayData.IsResume)
        {
            SendMessage(PacketId.CSEnterGameWithDummy, new CSEnterGame());
        }
        else
#endif
        {
            SendMessage(PacketId.CSEnterGame, new CSEnterGame());
        }
        return true;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class neco_admin : game_data
{
    public enum ADMIN_TYPE
    {
        NONE = 0,
        ADMIN,
        MODERATOR,
        HELPER,
        BEST_USER
    }

    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.NECO_ADMIN; }

    static public ADMIN_TYPE GetAdmin(uint uid)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_ADMIN);
        if (necoData == null)
            return ADMIN_TYPE.NONE;

        foreach(neco_admin info in necoData)
        {
            if(info.GetAdminID() == uid)
            {
                return info.GetAdminType();
            }
        }

        return ADMIN_TYPE.NONE;
    }

    [NonSerialized]
    private uint admin_id = 0;
    public uint GetAdminID()
    {
        if (admin_id == 0)
        {
            object obj;
            if (data.TryGetValue("user_id", out obj))
            {
                admin_id = (uint)obj;
            }
        }

        return admin_id;
    }

    [NonSerialized]
    private ADMIN_TYPE admin_type = ADMIN_TYPE.NONE;
    public ADMIN_TYPE GetAdminType()
    {
        if (admin_type == ADMIN_TYPE.NONE)
        {
            object obj;
            if (data.TryGetValue("admin_type", out obj))
            {
                admin_type = (ADMIN_TYPE)((int)(uint)obj);
            }
        }

        return admin_type;
    }
}
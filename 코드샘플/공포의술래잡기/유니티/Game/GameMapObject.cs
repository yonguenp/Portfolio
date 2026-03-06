using SBCommonLib;
using SBSocketSharedLib;
using UnityEngine;
using System.Collections.Generic;

public partial class Game
{
    public void ObjectSpawn(RespawnObjectInfo info)
    {
        PropController obj = Managers.Object.GetPropObject(info.ObjectId);

        if (obj == null)
        {
            if(Managers.PlayData.GetRoomPlayer(info.ObjectId) != null)
            {
                SpawnPlayer(info);
            }
            return;
        }

        float x = info.Position.X;
        float y = info.Position.Y;

        obj.ApplyPos(x, y);

        obj.OnRespawn();
    }

    public void Despawn(SCBcDespawn info)
    {
        PropController obj = Managers.Object.GetPropObject(info.ObjectId);

        if (obj == null)
        {
            var co = Managers.Object.FindCharacterById(info.ObjectId);
            var localTime = GameRoom.GameTime.GetClientTimestamp();
            var effectTimeMiliseconds = (int)(GameRoom.GameTime.GetClientTimestamp(info.RespawnTime) - localTime);
            if (co != null)
            {
                bool flip = false;
                if (co.PosInfo.MoveDir.X < 0)
                {
                    flip = true;
                }
                // 그로기 이펙트 표시
                Managers.Effect.PlayEffect(7, co.RootEffect, effectTimeMiliseconds * 0.001f, flip);
            }
            if (Managers.UserData.MyUserID.ToString() == info.ObjectId)
            {
                UIGame.ShowPlayerRespawn(effectTimeMiliseconds);
            }
            return;
        }

        obj.OnDespawn();

        float x = info.Position.X;
        float y = info.Position.Y;
        obj.ApplyPos(x, y);
    }

    void CreateObject(SBSocketSharedLib.MapObjectInfo objData)
    {
        ObjectGameData objTableData = (ObjectGameData)Managers.Data.GetData(GameDataManager.DATA_TYPE.@object, objData.MapObjectIndex);

        if (objTableData.ObjectType == ObjectGameData.MapObjectType.Vehicle)
        {
            CreateVehicleObject(objData.MapObjectIndex, objData.MapObjectId, new Vector2(objData.Position.X, objData.Position.Y), objTableData.obj_resource);
        }
        else if (objTableData.ObjectType == ObjectGameData.MapObjectType.Vent)
        {
            var ventObj = Managers.Object.CreateVentObject(objTableData.obj_resource);
            ventObj.SetMapObjectBaseData(ObjectGameData.MapObjectType.Vent, objData.MapObjectId, objData.MapObjectIndex, objData.Position.X, objData.Position.Y);

            //나중에 삭제
            ventObj.SetObjectID(objData.MapObjectIndex);

            Managers.Object.AddPropObject(ventObj);
        }
        else if (objTableData.ObjectType == ObjectGameData.MapObjectType.Hide)
        {
            var hideObj = Managers.Object.CreateHideObject(objTableData.obj_resource);
            hideObj.SetMapObjectBaseData(ObjectGameData.MapObjectType.Hide, objData.MapObjectId, objData.MapObjectIndex, objData.Position.X, objData.Position.Y);

            Managers.Object.AddPropObject(hideObj);
        }
        else if (objTableData.ObjectType == ObjectGameData.MapObjectType.Key
            && (objTableData.TypeData as ObjectKeyGameData).KeyType == ObjectKeyGameData.ObjectKeyType.EscapeDoor)   //문
        {
            var keyObj = Managers.Object.CreateEscapeDoor(objTableData.obj_resource);
            keyObj.SetMapObjectBaseData(ObjectGameData.MapObjectType.Key, objData.MapObjectId, objData.MapObjectIndex, objData.Position.X, objData.Position.Y);
            Managers.Object.AddEscapeKey(keyObj);

        }
        else if (objTableData.ObjectType == ObjectGameData.MapObjectType.Key
            && (objTableData.TypeData as ObjectKeyGameData).KeyType == ObjectKeyGameData.ObjectKeyType.ElectricBox)  //배전함
        {
            var keyObj = Managers.Object.CreateEscapeObject(objTableData.obj_resource);
            keyObj.SetMapObjectBaseData(ObjectGameData.MapObjectType.Key, objData.MapObjectId, objData.MapObjectIndex, objData.Position.X, objData.Position.Y);

            Managers.Object.AddPropObject(keyObj);
            //keyObj.SetScale(1.3f, 1.3f);
        }
        else if (objTableData.ObjectType == ObjectGameData.MapObjectType.BatteryCreater)
        {
            var obj = Managers.Object.CreateBatteryCreator(objTableData.obj_resource);
            obj.SetMapObjectBaseData(ObjectGameData.MapObjectType.BatteryCreater, objData.MapObjectId, objData.MapObjectIndex, objData.Position.X, objData.Position.Y);

            Managers.Object.AddPropObject(obj);
        }
        else if (objTableData.ObjectType == ObjectGameData.MapObjectType.BatteryGenerater)
        {
            var obj = Managers.Object.CreateBatteryGenerator(objTableData.obj_resource);
            obj.SetMapObjectBaseData(ObjectGameData.MapObjectType.BatteryGenerater, objData.MapObjectId, objData.MapObjectIndex, objData.Position.X, objData.Position.Y);

            Managers.Object.AddPropObject(obj);
        }

        //SBDebug.Log($"obj map type = {objTableData.type}");
    }

    void CreateVehicleObject(int index, string id, Vector2 pos, string resourcePath)
    {
        var vehicleObj = Managers.Object.CreateVehicleObject(index, resourcePath);
        vehicleObj.SetMapObjectBaseData(ObjectGameData.MapObjectType.Vehicle, id, index, pos.x, pos.y);

        //나중에 삭제
        vehicleObj.SetObjectID(index);
        Managers.Object.AddPropObject(vehicleObj);
    }

    public void CheckBatteryStatus()
    {
        
    }
}

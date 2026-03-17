using SBSocketSharedLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager
{
    List<EscapeDoor> _escapeDoor = new List<EscapeDoor>();
    // 캐싱용
    List<CharacterObject> currentCharacters = new List<CharacterObject>();
    List<PropController> currentHideObjects = new List<PropController>();
    BatteryGenerator batteryGenerator = null;

    public ConcurrentDictionary<string, GameObject> Objects { get; private set; }

    public ObjectManager()
    {
        Objects = new ConcurrentDictionary<string, GameObject>();
    }

    public static GameObjectType GetObjectTypeById(int id)
    {
        int type = (id >> 24) & 0x7F;
        return (GameObjectType)type;
    }

    public void AddPropObject(PropController propInfo)
    {
        if (Objects.ContainsKey(propInfo.Id))
            return;
        Objects.TryAdd(propInfo.Id, propInfo.gameObject);
        propInfo.Init();
    }

    public PropController GetPropObject(string id)
    {
        if (Objects.ContainsKey(id) == false) return null;
        var prop = Objects[id].GetComponent<PropController>();
        return prop;
    }

    //base object 기준으로 재작업 해야될거같다.ㅠㅠ
    public void AddProjectile(ProjectileObject info)
    {
        if (Objects.ContainsKey(info.Id))
            return;

        Objects.TryAdd(info.Id, info.gameObject);
    }

    public void AddEscapeKey(EscapeKey escapeKey)
    {
        if (Objects.ContainsKey(escapeKey.Id))
            return;

        Objects.TryAdd(escapeKey.Id, escapeKey.gameObject);
        escapeKey.Init();
    }

    public void AddEscapeDoor(EscapeDoor escapeDoor)
    {
        _escapeDoor.Add(escapeDoor);
    }

    public void AddVehicle(PropController vehicleController)
    {
        if (Objects.ContainsKey(vehicleController.Id))
            return;

        Objects.TryAdd(vehicleController.Id, vehicleController.gameObject);
    }

    public void SpawnBattery(string objectId, Battery obj)
    {
        if (Objects.ContainsKey(objectId))
            return;

        Objects.TryAdd(objectId, obj.gameObject);
    }

    public PropController GetVehicle(string id)
    {
        if (Objects.ContainsKey(id) == false) return null;
        var vehicle = Objects[id].GetComponent<PropController>();
        return vehicle;
    }

    public CharacterObject AddPlayer(PlayerObjectInfo info)
    {
        var game = Game.Instance;
        CharacterObject character = null;
        bool isMyPlayer = false;

        if (Objects.ContainsKey(info.ObjectId))
        {
            character = FindCharacterById(info.ObjectId);
            if (character != null)
            {
                character.SetBaseData(info.ObjectId, info.PosInfo, info.StatInfo);
            }
            return character;
        }

        if (Managers.UserData.MyUserID.ToString() == info.ObjectId)
        {
            isMyPlayer = true;
        }

        SBDebug.Log($"add character {info.ObjectId} hp {info.StatInfo.MaxHp}");

        RoomPlayerInfo curPlayerInfo = Managers.PlayData.GetRoomPlayer(info.ObjectId);
        if (curPlayerInfo == null)
            return null;

        bool isChaser = Managers.PlayData.IsChaserPlayer(curPlayerInfo.UserId);

        //GameObjectType objectType = GetObjectTypeById(info.ObjectId);
        GameObjectType objectType = GameObjectType.Player;
        if (objectType == GameObjectType.Player)
        {
            GameObject go = null;

            var data = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.character, curPlayerInfo.SelectedCharacter.CharacterType) as CharacterGameData;
            if (data != null)
            {
                int equip = 0;
                if (curPlayerInfo.SelectedCharacter.ItemNos != null && curPlayerInfo.SelectedCharacter.ItemNos.Count > 0)
                    equip = curPlayerInfo.SelectedCharacter.ItemNos[0];

                go = data.LoadCharacterObject(equip);
            }

            if (go == null)
                return null;

            Objects.TryAdd(info.ObjectId, go);
            character = go.GetComponent<CharacterObject>();
            character.SetBaseData(info.ObjectId, info.PosInfo, info.StatInfo);
            character.SetCharacterType(curPlayerInfo.SelectedCharacter.CharacterType);

            if (isMyPlayer)
            {
                var characterController = new GameObject();
                characterController.name = "CharacterController";
                var compCharacterController = characterController.AddComponent<PlayerController>();

                //controller device에 따라 분기 필요
                var controllerKeyboard = new GameObject();
                controllerKeyboard.name = "ControllerKeyboard";
                var compControllerKeyboard = controllerKeyboard.AddComponent<ControllerKeyboard>();
                compControllerKeyboard.InitController(compCharacterController);
                compCharacterController.SetControllerKeyboard(compControllerKeyboard);

                var controllerPad = GameObject.FindObjectOfType<ControllerPad>();
                if (controllerPad != null)
                {
                    controllerPad.InitController(compCharacterController);
                    compCharacterController.SetControllPad(controllerPad);
                }

                compCharacterController.Init(character);

                game.SetPlayerController(compCharacterController);
            }
            else
            {
                currentCharacters.Add(character);
            }
        }

        character.SetUserName(Game.Instance.HudNode.CreateHudUserName(character, Managers.PlayData.GetSlotIndex(info.ObjectId)));
        character.SetUserEmotion(Game.Instance.HudNode.CreateHudEmotion(character));

        if (isChaser == false)
        {
            CharacterPortrait cp = Game.Instance.UIPortrait.CreatePortrait(character.CharacterType, info, character.IsChaser);
            character.SetPortrait(cp);
            character.SetHudBattery(Game.Instance.HudNode.CreateHudBattery(character, character.Stat.Max_Battery));
            character.SetHudHp(Game.Instance.HudNode.CreateHudHP(character, character.Stat.MaxHp));
        }

        var shadowObj = Managers.Resource.Instantiate("Shadow/shadow");
        if (shadowObj != null)
        {
            character.SetShadow(shadowObj);
        }

        return character;
    }

    public void Remove(string id, float delay = 0)
    {
        if (Game.Instance.PlayerController != null && Managers.UserData.MyUserID.ToString() == id)

            //  return;
            if (Objects.ContainsKey(id) == false)
                return;

        GameObject go = FindById(id);
        if (go == null)
            return;

        Objects.TryRemove(id, out GameObject temp);
        Managers.Resource.Destroy(go, delay);
    }

    public GameObject FindById(string id)
    {
        GameObject go = null;
        Objects.TryGetValue(id, out go);
        return go;
    }

    public BaseObject FindBaseObjectById(string id)
    {
        var go = FindById(id);

        if (go == null)
            return null;

        var bo = go.GetComponent<BaseObject>();
        return bo;
    }

    public CharacterObject FindCharacterById(string id)
    {
        var go = FindById(id);

        if (go == null)
            return null;

        var co = go.GetComponent<CharacterObject>();
        return co;
    }
    public CharacterObject FindCharacterById(int id)
    {
        return FindCharacterById(id.ToString());
    }

    public CharacterObject FindCharacterById(long id)
    {
        return FindCharacterById(id.ToString());
    }

    public GameObject FindCreature(Vector2 cellPos)
    {
        foreach (GameObject obj in Objects.Values)
        {
            CreatureObject cc = obj.GetComponent<CreatureObject>();
            if (cc == null)
                continue;

            if (cc.CellPos == cellPos)
                return obj;
        }

        return null;
    }

    public GameObject Find(Func<GameObject, bool> condition)
    {
        foreach (GameObject obj in Objects.Values)
        {
            if (condition.Invoke(obj))
                return obj;
        }

        return null;
    }

    public void Clear()
    {
        foreach (GameObject obj in Objects.Values)
            Managers.Resource.Destroy(obj);
        Objects.Clear();
        _escapeDoor.Clear();

        currentCharacters.Clear();
        currentHideObjects.Clear();
        BlockObjects.Clear();
    }

    public PropController CreateHideObject(string resourcePath)
    {
        var hideObject = Managers.Resource.Instantiate(resourcePath);

        var propInfo = hideObject.AddComponent<HideObjectController>();
        if (propInfo != null)
        {
            currentHideObjects.Add(propInfo);
        }

        return propInfo;
    }

    public PropController CreateEscapeObject(string resourcePath)
    {
        GameObject escapeObj = Managers.Resource.Instantiate(resourcePath);

        var propInfo = escapeObj.AddComponent<ElectricBox>();
        if (propInfo != null)
        {
            propInfo.ObjectKeyType = ObjectKeyGameData.ObjectKeyType.ElectricBox;
        }

        return propInfo;
    }

    public EscapeKey CreateEscapeDoor(string resourcePath)
    {
        var keyObject = Managers.Resource.Instantiate(resourcePath);

        var propInfo = keyObject.AddComponent<EscapeKey>();
        if (propInfo != null)
        {
            propInfo.ObjectKeyType = ObjectKeyGameData.ObjectKeyType.EscapeDoor;
        }

        return propInfo;
    }

    public GameObject CreateEscapeOnObject()
    {
        GameObject escapeObj = Managers.Resource.Instantiate("object/escape01_on");
        return escapeObj;
    }

    public BatteryCreator CreateBatteryCreator(string resourcePath)
    {
        var keyObject = Managers.Resource.Instantiate(resourcePath);

        var propInfo = keyObject.AddComponent<BatteryCreator>();

        return propInfo;
    }

    public BatteryGenerator CreateBatteryGenerator(string resourcePath)
    {
        var keyObject = Managers.Resource.Instantiate(resourcePath);

        var propInfo = keyObject.AddComponent<BatteryGenerator>();
        batteryGenerator = propInfo;

        return propInfo;
    }

    public BatteryGenerator GetBatteryGenerator()
    {
        return batteryGenerator;
    }

    public PropController CreateVehicleObject(int index, string resourcePath)
    {
        GameObject go = Managers.Resource.Instantiate(resourcePath);

        if (go != null)
        {
            var vehicleController = go.AddComponent<VehicleObjectController>();
            // vehicleController.SetVehicleData(index);
            return vehicleController;
        }

        return null;
    }

    public PropController CreateVentObject(string resourcePath)
    {
        GameObject go = Managers.Resource.Instantiate(resourcePath);

        if (go != null)
        {
            var ventController = go.AddComponent<VentObjectController>();
            return ventController;
        }

        return null;
    }

    public PropController GetPropinArea(Vector3 basePos, float dis = 1)
    {
        Vector2 retVec = new Vector2(1000, 1000);
        float retVecMag = retVec.magnitude;

        PropController retProp = null;
        var iter = Objects.GetEnumerator();
        while (iter.MoveNext())
        {
            var go = iter.Current.Value;
            if (go == null) continue;
            var prop = go.GetComponent<PropController>();
            if (prop == null) continue;
            var vecDis = new Vector2(Mathf.Abs(prop.transform.position.x - basePos.x), Mathf.Abs(prop.transform.position.y - basePos.y));
            float mag = vecDis.magnitude;
            if (mag < retVecMag)
            {
                retVec = vecDis;
                retVecMag = mag;
                retProp = prop;
            }
        }

        if (Mathf.Abs(retVecMag) <= dis)
        {
            return retProp;
        }

        return null;
    }

    public bool CanGoObject(float x, float y)
    {
        int count = _escapeDoor.Count;
        for (int i = 0; i < count; ++i)
        {
            var item = _escapeDoor[i];
            if (item.IsOpen) continue;
            if ((int)item.transform.position.x == (int)x && (int)item.transform.position.y == (int)y)
                return false;
        }

        return true;
    }

    public PropController GetPropinArea(ObjectGameData.MapObjectType objectType, Vector3 basePos, float dis = 1)
    {
        Vector2 retVec = new Vector2(1000, 1000);
        float retVecMag = retVec.magnitude;

        PropController retProp = null;
        var iter = Objects.GetEnumerator();
        while (iter.MoveNext())
        {
            var go = iter.Current.Value;
            var prop = go.GetComponent<PropController>();
            if (prop == null) continue;
            if (prop.MapObjectType == objectType)
            {
                var vecDis = new Vector2(Mathf.Abs(prop.transform.position.x - basePos.x), Mathf.Abs(prop.transform.position.y - basePos.y));
                float mag = vecDis.magnitude;
                if (mag < retVecMag)
                {
                    retVec = vecDis;
                    retVecMag = mag;
                    retProp = prop;
                }
            }
        }

        if (Mathf.Abs(retVecMag) <= dis)
        {
            return retProp;
        }

        return null;
    }

    public List<CharacterObject> GetCharacters()
    {
        return currentCharacters;
    }

    public List<PropController> GetHideObjects()
    {
        return currentHideObjects;
    }

    public HideObjectController GetHideObjectByPos(Vec2Float pos)
    {
        foreach (PropController obj in GetHideObjects())
        {
            if (obj.ObjData != null)
            {
                if (obj.transform.localPosition.x == pos.X && obj.transform.localPosition.y == pos.Y)
                {
                    return obj as HideObjectController;
                }
            }
        }

        return null;
    }

    public void ShowEscapeDoor(int id, bool isAll = false)
    {
        var doors = _escapeDoor;
        var iter = doors.GetEnumerator();
        while (iter.MoveNext())
        {
            var door = iter.Current;
            var escapeDoor = door.GetComponent<EscapeDoor>();
            if (escapeDoor == null) continue;
            if (isAll)
                escapeDoor.ShowRenderer(false);
            else
            {
                if (escapeDoor.EscapeDoorId == id)
                    escapeDoor.ShowRenderer(false);
            }
        }
    }


    //block object work
    public List<PropController> BlockObjects = new List<PropController>();

    public void AddBlockObject(PropController p)
    {
        BlockObjects.Add(p);
    }

    public void InitBlockObjects()
    {
        bool isChaser = Game.Instance.PlayerController.Character.IsChaser;
        foreach (var bo in BlockObjects)
        {
            bo.ShowRenderer(isChaser);
        }
    }
}

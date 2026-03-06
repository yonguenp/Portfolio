using System.Collections.Generic;

public class ObjectGameData : GameData
{
    public enum MapObjectType : byte
    {
        None = 0,
        Hide = 1,
        Key = 2,
        Door = 3,
        Obstruct = 4,
        Vehicle = 5,
        Vent = 6,
        BatteryCreater = 7,
        BatteryGenerater = 8,
    }

    protected int obj_type { get; private set; }
    public int destruction { get; private set; }
    public int enableUserType { get; private set; }
    public int sub_obj_uid { get; private set; }
    public int interaction_x { get; private set; }
    public int interaction_y { get; private set; }
    public string obj_resource { get; private set; }

    public UnityEngine.Vector3 LocalScale { get; private set; }
    public UnityEngine.Vector3 LocalPosition { get; private set; }


    public MapObjectType ObjectType { get { return (MapObjectType)obj_type; } }
    public ObjectTypeData TypeData { get; private set; }


    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        obj_type = Int(data["obj_type"]);
        destruction = Int(data["destruction"]);
        enableUserType = Int(data["user"]);
        sub_obj_uid = Int(data["sub_obj_uid"]);
        interaction_x = Int(data["interaction_x"]);
        interaction_y = Int(data["interaction_y"]);
        obj_resource = data["obj_resource"];
        LocalScale = new UnityEngine.Vector3(Int(data["scale_x"]) * 0.001f, Int(data["scale_y"]) * 0.001f, 1f);
        LocalPosition = new UnityEngine.Vector3(Int(data["offset_x"]) * 0.001f, Int(data["offset_y"]) * 0.001f, 0f);
    }

    public void SetTypeData(ObjectTypeData type)
    {
        TypeData = type;
    }
}

public class ObjectTypeData : GameData
{
    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        List<GameData> objectData = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.@object);
        foreach (ObjectGameData obj in objectData)
        {
            if (obj.sub_obj_uid == GetID())
            {
                obj.SetTypeData(this);
            }
        }
    }
}

public class ObjectHideGameData : ObjectTypeData
{
    public int hideObjectType { get; private set; }
    public int DestroyEffectId { get; private set; }
    public string DestroySoundPath { get; private set; }

    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        hideObjectType = Int(data["hide_object_type"]);
        DestroyEffectId = Int(data["destroy_effect"]);
        DestroySoundPath = data["destroy_sound"];
    }
}

public class ObjectKeyGameData : ObjectTypeData
{
    public enum ObjectKeyType
    {
        None = 0,
        ElectricBox = 1,
        EscapeDoor = 2,
    }

    private int alarm1;
    private int alarm2;

    public ObjectKeyType KeyType { get { return (ObjectKeyType)type; } }
    public int type { get; private set; }
    public int activation_time { get; private set; }
    public string alarm1_resource { get; private set; }
    public string alarm2_resource { get; private set; }

    public float Alarm1 { get { return alarm1 * 0.001f; } }
    public float Alarm2 { get { return alarm2 * 0.001f; } }
    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        type = Int(data["type"]);
        activation_time = Int(data["activation_time"]);
        alarm1 = Int(data["alarm1"]);
        alarm2 = Int(data["alarm2"]);
        alarm1_resource = data["alarm1_resource"];
        alarm2_resource = data["alarm2_resource"];
    }
}

public class ObjectDoorGameData : ObjectTypeData
{
    public enum ObjectDoorType
    {
        None = 0,
        Normal = 1,
        NeedKey = 2,
    }

    public ObjectDoorType DoorType { get { return (ObjectDoorType)door_type; } }

    public int door_type { get; private set; }
    public int door_size_x { get; private set; }
    public int door_size_y { get; private set; }
    public int use_key { get; private set; }

    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        door_type = Int(data["door_type"]);
        door_size_x = Int(data["door_size_x"]);
        door_size_y = Int(data["door_size_y"]);
        use_key = Int(data["use_key"]);
    }
}

public class ObjectObsturctGameData : ObjectTypeData
{
    public enum ObjectObstructType
    {
        None = 0,
        Sight = 1,
        BlockSight = 2,
    }

    public ObjectObstructType ObstructType { get { return (ObjectObstructType)obstacle_sight_type; } }

    public int hp { get; private set; }
    public int activation_time { get; private set; }
    public int obstacle_sight_type { get; private set; }
    public int obstacle_active_size_x { get; private set; }
    public int obstacle_active_size_y { get; private set; }

    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        hp = Int(data["hp"]);
        obstacle_sight_type = Int(data["obstacle_sight_type"]);
        obstacle_active_size_x = Int(data["obstacle_active_size_x"]);
        obstacle_active_size_y = Int(data["obstacle_active_size_y"]);
    }
}

public class ObjectVehicleGameData : ObjectTypeData
{
    public int vehicle_type { get; private set; }
    public int speed { get; private set; }
    public float respawnTime { get; private set; }
    public int duration { get; private set; }
    public string anim_name { get; private set; }


    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        vehicle_type = Int(data["vehicle_type"]);
        speed = Int(data["move_speed"]);
        duration = Int(data["duration_time"]);
        anim_name = data["anim_name"];
        respawnTime = Int(data["respawn"]) * 0.001f;
    }
}

public class ObjectVentGameData : ObjectTypeData
{
    public int move_time { get; private set; }
    public int cool_time { get; private set; }
    public int production_time { get; private set; }

    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        move_time = Int(data["move_time"]);
        cool_time = Int(data["cool_time"]);
        production_time = Int(data["production_time"]);
    }
}

public class ObjectBatteryCreatorGameData : ObjectTypeData
{
    public int create_battery_max { get; private set; }
    public float create_cool_time { get; private set; }
    public float create_starting_cool_time { get; private set; }
    public int resource_id { get; private set; }

    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        create_battery_max = Int(data["create_battery_max"]);
        create_cool_time = Int(data["create_cool_time"]) * 0.001f;
        create_starting_cool_time = Int(data["create_starting_cool_time"]) * 0.001f;
        resource_id = Int(data["resource_id"]);
    }
}

public class ObjectBatteryGeneratorGameData : ObjectTypeData
{
    public int input_battery_max { get; private set; }
    public float active_time { get; private set; }
    public float cooltime_time { get; private set; }
    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        input_battery_max = Int(data["input_battery_max"]);
        active_time = Int(data["active_time"]) * 0.001f;
        cooltime_time = Int(data["cooltime_time"]) * 0.001f;
    }
}

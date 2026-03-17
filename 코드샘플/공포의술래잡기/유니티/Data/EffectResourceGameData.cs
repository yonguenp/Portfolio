using System.Collections.Generic;

public class EffectResourceGameData : GameData
{
    public string Effectname { get; private set; }
    public float ScaleX { get; private set; }
    public float ScaleY { get; private set; }
    public float Rotation { get; private set; }
    public float OffsetX { get; private set; }
    public float OffsetY { get; private set; }
    public float PlayTime { get; private set; }
    public bool IsParentDirection { get; private set; }
    public bool IsParentTarget { get; private set; }
    public string ResourcePath { get; private set; }
    public string AnimState { get; private set; }
    public bool AlwaysShow {get;private set; }
    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        Effectname = data["effect_name"];
        ScaleX = Int(data["scale_x"]) * 0.001f;
        ScaleY = Int(data["scale_y"]) * 0.001f;
        Rotation = Int(data["rotation"]);
        OffsetX = Int(data["offset_x"]) * 0.001f;
        OffsetY = Int(data["offset_y"]) * 0.001f;
        PlayTime = Int(data["play_time"]) * 0.001f;
        ResourcePath = data["resource_path"];
        AnimState = data["anim_state"];
        IsParentTarget = Int(data["is_parent_target"]) == 1;
        IsParentDirection = Int(data["is_parent_direction"]) == 1;
        AlwaysShow = Int(data["always_show"]) == 1;
    }
}

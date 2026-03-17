using SBSocketSharedLib;

public class ElectricBox : PropController
{
    public float Gauge { get; set; }

    public override void Init()
    {
        GameObjectType = GameObjectType.MapObject;
        base.Init();
    }
}

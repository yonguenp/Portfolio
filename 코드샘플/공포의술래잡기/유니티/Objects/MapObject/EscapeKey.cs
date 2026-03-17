using System.Collections.Generic;
using UnityEngine;

public class EscapeKey : PropController
{
    //List<EscapeDoor> _escapeDoors = new List<EscapeDoor>();
    public float Gauge { get; set; } = 0;

    public override void Init()
    {
        GameObjectType = SBSocketSharedLib.GameObjectType.MapObject;
        model = transform.Find("model").gameObject;
        Gauge = 0;

        if (Game.Instance.IsOpenDoor)
            OpenDoor();
    }

    public void OpenDoor()
    {
        var id = GetPosId();
        Managers.Object.ShowEscapeDoor(id, true);
    }
}

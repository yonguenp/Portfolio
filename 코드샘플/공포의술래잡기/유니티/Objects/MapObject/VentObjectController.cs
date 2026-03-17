using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VentObjectController : PropController
{
     public override void Init()
    {
        base.Init();
        SetRenderOrder(-9);
    }
}

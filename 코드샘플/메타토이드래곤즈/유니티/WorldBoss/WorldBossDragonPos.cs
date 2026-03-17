using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldBossDragonPos : MonoBehaviour
{
    [SerializeField]
    Transform[] DragonPos = null;

    public Transform GetDragonPos(int index)
    {
        return DragonPos[index];
    }
}

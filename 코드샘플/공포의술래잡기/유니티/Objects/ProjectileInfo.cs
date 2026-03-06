using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileInfo : MonoBehaviour
{
    public enum eDirectionType
    {
        None,
        Rotate,
        HasAnim,
    }

    public eDirectionType directionType = eDirectionType.None;
}

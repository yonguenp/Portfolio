using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    [SerializeField] Transform CenterPoint;
    [SerializeField] Transform EscapePoint;
    [SerializeField] Transform ChargePoint;

    public Vector3 CenterPosition { get { return CenterPoint != null ? CenterPoint.position : Vector3.zero; } }
    public Vector3 EscapePosition { get { return EscapePoint != null ? EscapePoint.position : Vector3.zero; } }
    public Vector3 ChargePosition { get { return ChargePoint != null ? ChargePoint.position : Vector3.zero; } }

    public MapData MapData{get;set;}
}

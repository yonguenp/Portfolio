using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTileInfo : MonoBehaviour
{
    public byte doorId = 255;
    public Vector2Int[] posInfos = null;
    [SerializeField]
    Color _color = Color.red;

    private void OnDrawGizmos() {
        if(posInfos == null) return;
        Gizmos.color = _color;
        for(int i = 0 ; i < posInfos.Length ; ++ i)
        {
            var vec = new Vector3(posInfos[i].x + 0.5f, posInfos[i].y + 0.5f, 0);
            Gizmos.DrawCube(vec, Vector3.one);
        }
    }
}

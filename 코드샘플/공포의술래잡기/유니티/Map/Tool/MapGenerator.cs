using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    [SerializeField]
    Grid TargetGrid;

    [ContextMenu("Generator Out Object")]
    public void GenerateOutObjet()
    {
        string outObjectName = "Out_sample";
        Transform outTranform = transform.Find(outObjectName);
        if (outTranform != null)
        {
            Debug.LogError("이미 out이 있슴");
            return;
        }

        if (TargetGrid == null)
        {
            Debug.LogError("타겟이 미지정됨.");
            return;
        }

        bool isSetBound = false;
        BoundsInt bound = new BoundsInt();
        Vector3 NearLeftBottom = new Vector3();
        Vector3 FarRightTop = new Vector3();

        foreach (Tilemap map in TargetGrid.GetComponentsInChildren<Tilemap>())
        {
            map.CompressBounds();
            if (!isSetBound)
            {
                isSetBound = true;
                bound.xMin = map.cellBounds.xMin;
                bound.xMax = map.cellBounds.xMax;
                bound.yMin = map.cellBounds.yMin;
                bound.yMax = map.cellBounds.yMax;
            }
            else
            {
                bound.xMin = bound.xMin > map.cellBounds.xMin ? map.cellBounds.xMin : bound.xMin;
                bound.xMax = bound.xMax < map.cellBounds.xMax ? map.cellBounds.xMax : bound.xMax;
                bound.yMin = bound.yMin > map.cellBounds.yMin ? map.cellBounds.yMin : bound.yMin;
                bound.yMax = bound.yMax < map.cellBounds.yMax ? map.cellBounds.yMax : bound.yMax;
            }

            Vector3 minPivot = Vector3.zero;
            Vector3 NearLeftTile = minPivot;
            for (int x = bound.xMin; x < bound.xMin + bound.xMax; x++)
            {
                for (int y = bound.yMin; y < bound.yMin + bound.yMax; y++)
                {
                    if (map.GetSprite(new Vector3Int(x, y, 0)) != null)
                    {
                        NearLeftTile = new Vector3(x, y, 0);
                        break;
                    }
                }

                if (NearLeftTile != minPivot)
                    break;
            }

            if (NearLeftTile != minPivot)
            {
                Vector3 minPos = new Vector3(bound.xMin, bound.yMin, 0);
                Vector3 curNearTilePos = NearLeftTile;

                if (Vector3.Distance(minPos, curNearTilePos) < Vector3.Distance(minPos, NearLeftBottom))
                {
                    NearLeftBottom = curNearTilePos;
                }
            }

            Vector3 maxPivot = new Vector3(float.MaxValue, float.MaxValue);
            Vector3 FarRightTile = maxPivot;
            for (int y = bound.yMin + bound.yMax; y >= bound.yMin; y--)
            {
                for (int x = bound.xMin + bound.xMax; x >= bound.xMin; x--)
                {
                    if (map.GetSprite(new Vector3Int(x, y, 0)) != null)
                    {
                        FarRightTile = new Vector3(x, y, 0);
                        break;
                    }
                }

                if (FarRightTile != maxPivot)
                    break;
            }

            if (FarRightTile != maxPivot)
            {
                Vector3 maxPos = new Vector3(bound.xMin + bound.xMax, bound.yMin + bound.yMax, 0);
                Vector3 curFarTilePos = FarRightTile;

                if (Vector3.Distance(maxPos, curFarTilePos) > Vector3.Distance(maxPos, FarRightTop))
                {
                    FarRightTop = curFarTilePos;
                }
            }
        }

        bool[,] tiles = new bool[bound.xMax, bound.yMax];

        foreach (Tilemap map in TargetGrid.GetComponentsInChildren<Tilemap>())
        {
            for (int x = bound.xMin; x < bound.xMin + bound.xMax; x++)
            {
                for (int y = bound.yMin; y < bound.yMin + bound.yMax; y++)
                {
                    Sprite tileSprite = map.GetSprite(new Vector3Int(x, y, 0));
                    if (tileSprite != null)
                    {
                        tiles[x, y] = true;
                    }
                }
            }
        }

        ////test
        //List<Vector3> outline = new List<Vector3>();
        //for(int x = 0; x < bound.xMax; x++)
        //{
        //    for(int y = 0; y < bound.yMax; y++)
        //    {
        //        if(tiles[x, y])
        //        {
        //            outline.Add(new Vector3(x, y, 0));
        //            outline.Add(new Vector3(x + 1, y, 0));
        //            outline.Add(new Vector3(x + 1, y + 1, 0));
        //            outline.Add(new Vector3(x, y + 1, 0));
        //            outline.Add(new Vector3(x, y, 0));
        //        }
        //    }
        //}

        List<Vector3> outline = GetOutline(tiles, NearLeftBottom);

        GameObject outObject = new GameObject();
        outObject.name = outObjectName;
        outTranform = outObject.transform;
        outTranform.SetParent(transform);
        outTranform.position = Vector3.zero;
        outTranform.localScale = Vector3.one;
        outTranform.rotation = Quaternion.identity;

        ShadowCaster2D outShadowCaster = outObject.AddComponent<ShadowCaster2D>();
        outShadowCaster.castsShadows = true;
        outShadowCaster.selfShadows = true;

        BindingFlags accessFlagsPrivate = BindingFlags.NonPublic | BindingFlags.Instance;
        FieldInfo meshField = typeof(ShadowCaster2D).GetField("m_Mesh", accessFlagsPrivate);
        FieldInfo shapePathField = typeof(ShadowCaster2D).GetField("m_ShapePath", accessFlagsPrivate);
        MethodInfo onEnableMethod = typeof(ShadowCaster2D).GetMethod("OnEnable", accessFlagsPrivate);

        List<Vector3> testPath = new List<Vector3>();
        testPath.Add(bound.position);
        testPath.Add(bound.position + new Vector3(bound.size.x, 0, 0));
        testPath.Add(bound.position + bound.size);
        testPath.Add(bound.position + new Vector3(0, bound.size.y, 0));
        testPath.Add(bound.position);
        testPath.AddRange(outline);

        shapePathField.SetValue(outShadowCaster, testPath.ToArray());
        meshField.SetValue(outShadowCaster, null);
        onEnableMethod.Invoke(outShadowCaster, new object[0]);
    }

    List<Vector3> GetOutline(bool[,] tiles, Vector3 startPos)
    {
        List<Vector3> outline = new List<Vector3>();
        outline.Add(startPos);

        Vector3[] rightTraveler = {
            Vector3.right,
            Vector3.up,
            Vector3.down,
            Vector3.left
        };

        //do
        //{
        //    Vector3 curPos = outline[outline.Count - 1];

        //    for (int i = 0; i < rightTraveler.Length; i++)
        //    {
        //        Vector3Int checkPos = Vector3Int.FloorToInt(curPos + rightTraveler[i]);
        //        if (checkPos.x < 0 || checkPos.x >= tiles.GetLength(0) ||
        //            checkPos.y < 0 || checkPos.y >= tiles.GetLength(1))
        //            continue;

        //        if (tiles[checkPos.x, checkPos.y])
        //        {
        //            outline.Add(checkPos);
        //        }
        //    }

        //} while (outline[outline.Count - 1] != startPos);
        //코드 추가예정
        return outline;
    }
}

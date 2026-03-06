using UnityEngine;
using System;

public class MapObjectBase : BaseObject
{
    protected GameObject model = null;

    private Renderer[] renderers = null;

    public ObjectGameData.MapObjectType MapObjectType { get; private set; }
    public ObjectKeyGameData.ObjectKeyType ObjectKeyType { get; set; }
    public int ObjectIndex { get; private set; }

    public int ObjectId { get; private set; }

    public override void Init()
    {
        base.Init();

        model = transform.Find("model").gameObject;
        renderers = GetComponentsInChildren<Renderer>();
    }

    public void SetMapObjectBaseData(ObjectGameData.MapObjectType type, string id, int mapObjectIndex, float x, float y)
    {
        SetBaseData(id);
        SetType(type);
        ObjectIndex = mapObjectIndex;
        ApplyPos(x, y);
    }

    public void SetType(ObjectGameData.MapObjectType type)
    {
        MapObjectType = type;
    }

    public void SetObjectID(int objectId)
    {
        ObjectId = objectId;
    }

    public void SetScale(float x, float y)
    {
        if (model)
        {
            model.transform.localScale = new Vector3(1.3f, 1.3f, 0);
        }
    }

    public override void ShowRenderer(bool isShow)
    {
        if (renderers != null)
        {
            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = isShow;
            }
        }
    }

    public int GetPosId()
    {
        float offx = 0.5f;
        float offy = 0.5f;

        //차후 테이블에서 데이터 가져다 사용
        // offx += bc.offset.x / bc.size.x;
        // offy += bc.offset.y / bc.size.y;

        var width = (int)1 * 100;
        var height = (int)1 * 100;

        float round_x = (float)Math.Round(PosInfo.Pos.X, 1);
        float round_y = (float)Math.Round(PosInfo.Pos.Y, 1);

        var x = (int)(round_x * 100) - (int)(offx * width);
        var y = (int)(round_y * 100) - (int)(offy * height);

        var id = x * 100000 + y;

        return id;
    }
}

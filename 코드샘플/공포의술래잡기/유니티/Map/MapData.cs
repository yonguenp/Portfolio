using System;
using System.Collections.Generic;
[Serializable]

public enum eMapObjectDir
{
    Right,
    Down,
    Left,
    Up,
}

public enum VehicleType
{
    Ringer = 10000,
    Barrel = 20000,
    Cart = 30000,
}



[Serializable]
public struct MapRect
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public int MinX { get; set; }
    public int MinY { get; set; }
    public int MaxX { get; set; }
    public int MaxY { get; set; }

    public float OffsetX { get; set; }
    public float OffsetY { get; set; }

    public MapRect(int x_, int y_, int width_, int height_, float offsetX_, float offsetY_)
    {
        X = x_;
        Y = y_;
        Width = width_;
        Height = height_;
        OffsetX = offsetX_;
        OffsetY = offsetY_;

        MinX = MinY = MaxX = MaxY = 0;
    }

    public void Init()
    {

    }
}

[Serializable]
public struct Pos
{
    public Pos(int x_, int y_) { X = x_; Y = y_; }

    public int X { get; set; }
    public int Y { get; set; }
}


[Serializable]
public class ObjectData
{
    public MapRect Rect { get; set; }
    public ObjectGameData.MapObjectType Type { get; set; }
    public int ObjectId { get; set; }
    public byte Dir { get; set; }
}

[Serializable]
public struct VentData
{
    public Pos LinkPos;
}

[Serializable]
public struct DoorTileInfoData
{
    public Pos[] pos;
}

[Serializable]
public class MapData
{
    public string MapName { get; set; }
    public int MapNo { get; set; }
    public List<ObjectData> Objects { get; set; } = new List<ObjectData>();
    public Dictionary<int, Pos> SurvivorSpawns { get; set; } = new Dictionary<int, Pos>();
    public Dictionary<int, Pos> ChaserSpawns { get; set; } = new Dictionary<int, Pos>();
    public Dictionary<int, VentData> Vents { get; set; } = new Dictionary<int, VentData>();
    public Dictionary<int, DoorTileInfoData> DoorTiles { get; set; } = new Dictionary<int, DoorTileInfoData>();

    public byte[,] TileInfo;

    public int MaxX = 0;
    public int MaxY = 0;

    public MapData(string name, int index)
    {
        MapName = name;
        MapNo = index;
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

using MessagePack;
//using ZeroFormatter;

namespace SBSocketSharedLib
{
    public struct Pos
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Pos(int x_, int y_) { X = x_; Y = y_; }
    }

    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public struct Vec2Float
    {
        [Key(0)]
        public float X { get; set; }
        [Key(1)]
        public float Y { get; set; }

        public Vec2Float(float x_, float y_) { X = x_; Y = y_; }

        [IgnoreMember]
        public Vector2 Get { get => new Vector2(X, Y); }

        //public void Set(Vector2 value_)
        //{
        //    X = value_.X;
        //    Y = value_.Y;
        //}
    }

    //[ZeroFormattable]
    [MessagePackObject]
    [Serializable]
    public struct Vec2Int
    {
        [Key(0)]
        public int X { get; set; }
        [Key(1)]
        public int Y { get; set; }

        public Vec2Int(int value_) { X = value_; Y = value_; }
        public Vec2Int(int x_, int y_) { X = x_; Y = y_; }

        public static Vec2Int Up { get => new Vec2Int(0, 1); }
        public static Vec2Int Down { get => new Vec2Int(0, -1); }
        public static Vec2Int Left { get => new Vec2Int(-1, 0); }
        public static Vec2Int Right { get => new Vec2Int(1, 0); }
        public static Vec2Int UpLeft { get => new Vec2Int(-1, 1); }
        public static Vec2Int UpRight { get => new Vec2Int(1, 1); }
        public static Vec2Int DownLeft { get => new Vec2Int(-1, -1); }
        public static Vec2Int DownRight { get => new Vec2Int(1, -1); }

        public static Vec2Int operator +(Vec2Int a_, Vec2Int b_)
        {
            return new Vec2Int(a_.X + b_.X, a_.Y + b_.Y);
        }

        public static Vec2Int operator -(Vec2Int a_, Vec2Int b_)
        {
            return new Vec2Int(a_.X - b_.X, a_.Y - b_.Y);
        }

        public static Vec2Int operator *(Vec2Int a_, int b_)
        {
            return new Vec2Int(a_.X * b_, a_.Y * b_);
        }

        public static Vec2Int operator /(Vec2Int a_, int b_)
        {
            return new Vec2Int(a_.X / b_, a_.Y / b_);
        }

        [IgnoreMember]
        public float Magnitude { get { return (float)Math.Sqrt(SqrMagnitude); } }
        [IgnoreMember]
        public int SqrMagnitude { get { return (X * X + Y * Y); } }
        [IgnoreMember]
        public int CellDistFromZero { get { return Math.Abs(X) + Math.Abs(Y); } }
    }

    //[ZeroFormattable]
    //[MessagePackObject]
    //[Serializable]
    //public struct Vec2Float
    //{
    //    [Key(0)]
    //    public float X { get; set; }
    //    [Key(1)]
    //    public float Y { get; set; }

    //    public Vec2Float(float x_, float y_) { X = x_; Y = y_; }

    //    public static Vec2Float up { get { return new Vec2Float(0f, 1f); } }
    //    public static Vec2Float down { get { return new Vec2Float(0f, -1f); } }
    //    public static Vec2Float left { get { return new Vec2Float(-1f, 0f); } }
    //    public static Vec2Float right { get { return new Vec2Float(1f, 0f); } }

    //    public static Vec2Float operator +(Vec2Float a_, Vec2Float b_)
    //    {
    //        return new Vec2Float(a_.X + b_.X, a_.Y + b_.Y);
    //    }

    //    public static Vec2Float operator -(Vec2Float a_, Vec2Float b_)
    //    {
    //        return new Vec2Float(a_.X - b_.X, a_.Y - b_.Y);
    //    }

    //    public void Normalize()
    //    {
    //        X /= Magnitude;
    //        Y /= Magnitude;
    //    }

    //    //[IgnoreMember]
    //    //public Vec2Float Normalize { get => new Vec2Float(X / Magnitude, Y / Magnitude); }
    //    [IgnoreMember]
    //    public float Magnitude { get { return (float)Math.Sqrt(SqrMagnitude); } }
    //    [IgnoreMember]
    //    public float SqrMagnitude { get { return (X * X + Y * Y); } }
    //    [IgnoreMember]
    //    public float CellDistFromZero { get { return Math.Abs(X) + Math.Abs(Y); } }
    //}
}

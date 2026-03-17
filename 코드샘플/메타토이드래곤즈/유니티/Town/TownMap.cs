
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SandboxNetwork.SBDefine;

namespace SandboxNetwork
{
    public static class TownMap
    {
#if DEBUG && HYEON_TEST
        public static Vector2Int Dummy = Vector2Int.zero;
#endif
        private static Vector4 mapSize = Vector4.zero;
        private static Vector2 openSize = Vector2.zero;
        public static int X { get { return Mathf.RoundToInt(mapSize.x); } }
        public static int Y { get { return Mathf.RoundToInt(mapSize.y); } }
        public static int UndergroundY { get { return Y; } }

        public static int Width { 
            get 
            {
#if DEBUG && HYEON_TEST
                return Mathf.RoundToInt(mapSize.z) + Dummy.x;
#else
                return Mathf.RoundToInt(mapSize.z);
#endif
            }
        }
        public static int Height 
        { 
            get 
            {
#if DEBUG && HYEON_TEST
                return Mathf.RoundToInt(mapSize.w) + Dummy.y + 1;
#else
                return Mathf.RoundToInt(mapSize.w) + 1;
#endif
            }
        }
        public static int OpenHeight
        {
            get
            {
                return Mathf.RoundToInt(openSize.y) + 1;
            }
        }
        public static void SetMap(Vector4 size)
        {
            mapSize.x = size.x;
            mapSize.y = size.y;
            mapSize.z = size.z;
            mapSize.w = size.w;
        }
        public static void SetOpenMap(Vector2 size)
        {
            openSize.x = size.x;
            openSize.y = size.y;
        }
        public static Vector3Int GetRandomBatchCell()
        {
            var result = new Vector3Int(-1000, -1000, DefaultOrder);

            result.y = SBFunc.Random(Y, OpenHeight);
            
            if(UndergroundY == result.y)
            {
                var subway = User.Instance.GetUserBuildingInfoByTag((int)eLandmarkType.SUBWAY);
                if (subway == null || subway.State != eBuildingState.NORMAL)
                {
                    result.y = SBFunc.Random(0, OpenHeight);
                }
            }

            if (result.y == Y)
            {
                var isFront = SBFunc.Random(0, 2) == 0;
                if (isFront)
                {
                    result.z = UnderFrontOrder;
                    result.x = SBFunc.Random(X + 1, Width - 1);
                    return result;
                }
                else
                {
                    result.z = UnderBackOrder;
                }
            }
            result.x = SBFunc.Random(X, Width);

            return result;
        }
        public static Vector3Int GetRandomCell(Vector2Int except)
        {
            var result = new Vector3Int(-1000, -1000, DefaultOrder);
            var subway = User.Instance.GetUserBuildingInfoByTag((int)eLandmarkType.SUBWAY);

            if (Y + 1 < OpenHeight)
            {
                while (result.y < -999)
                {
                    var ry = SBFunc.Random(UndergroundY, OpenHeight);
                    if (ry == except.y)
                        continue;

                    if (UndergroundY == ry)
                    {
                        if (subway == null || subway.State != eBuildingState.NORMAL)
                        {
                            continue;
                        }
                    }

                    result.y = ry;
                }
            }

            if (result.y == UndergroundY)
            {
                var isFront = SBFunc.Random(0, 2) == 0;
                if (isFront)
                {
                    result.z = UnderFrontOrder;
                    result.x = SBFunc.Random(X + 1, Width - 1);
                    return result;
                }
                else
                {
                    result.z = UnderBackOrder;
                }
            }
            result.x = SBFunc.Random(X, Width);

            return result;
        }
        public static Vector3Int GetRandomCellByFloor(Vector3Int curCell)
        {
            var result = new Vector3Int(SBFunc.Random(X, Width), curCell.y, curCell.z);
            if(curCell.y == Y && curCell.z == UnderFrontOrder)
                result.x = SBFunc.Random(X + 1, Width - 1);            
            
            return result;
        }
        public static Vector3Int GetRandomFloorCell(Vector3Int curCell)
        {
            var subway = User.Instance.GetUserBuildingInfoByTag((int)eLandmarkType.SUBWAY);
            var minY = UndergroundY;

            if (subway == null || subway.State != eBuildingState.NORMAL)
            {
                minY = UndergroundY + 1;
            }

            var result = new Vector3Int(SBFunc.Random(X, Width), SBFunc.Random(minY, OpenHeight), DefaultOrder);
            
            while (result.y == curCell.y)
            {
                result.y = SBFunc.Random(minY, OpenHeight);
            }

            if (result.y == UndergroundY)
            {
                var isFront = SBFunc.Random(0, 2) == 0;
                if (isFront)
                {
                    result.z = UnderFrontOrder;
                    result.x = SBFunc.Random(X + 1, Width - 1);
                    return result;
                }
                else
                {
                    result.z = UnderBackOrder;
                }
            }

            return result;
        }
        public static float GetCellPosX(int x)
        {
            var result = StartOffsetX();
            if (x == X)
            {
                result += CellBothSpancing * 0.5f;
            }
            else if (x == (Width - 1))
            {
                result += CellBothSpancing + (x - 1) * CellSpancing + CellBothSpancing * 0.5f;
            }
            else
            {
                result += CellBothSpancing + (x - 1) * CellSpancing + CellSpancing * 0.5f;
            }
            return result;
        }
        public static float GetBuildingPosX(int x)
        {
            var result = GetCellPosX(x);
            if (x == X)
            {
                result += BuildingBothSpine;
            }
            else if (x == (Width - 1))
            {
                result -= BuildingBothSpine;
            }
            return result;
        }
        public static float GetCellPosY(int y)
        {
            if (y < 0)
            {
                return GetUnderFloorSpacing(y);
                //return y * UnderFloorSpancing;
            }
            else
                return y * FloorSpancing;
        }
        public static Vector2 GetCellPos(Vector2Int pos)
        {
            return GetCellPos(pos.x, pos.y);
        }
        public static Vector2 GetGuildTopPos(int x, int y)
        {
            float xPos = x switch
            {
                0 => -CellSpancing,
                1 => 0f,
                2 => CellSpancing,
                _ => 0
            };
            
            return new Vector2(xPos, GetCellPosY(y)+ GuildTopSpancing);
        }

        public static Vector2 GetCellPos(int x, int y)
        {
            return new Vector2(GetCellPosX(x), GetCellPosY(y));
        }
        public static Vector2 GetGuildCellPos (int x, int y)
        {
            float xPos = x switch
            {
                0 => -CellSpancing,
                1 => 0f,
                2 => CellSpancing,
                _ => 0
            };
            return new Vector2(xPos, GetCellPosY(y)+ GuildCellSpancing);
        }
        public static float GetRandomCellPosX(int x)
        {
            return GetCellPosX(x) + SBFunc.Random(0, CellSpancing) - CellSpancing * 0.5f;
        }
        public static Vector2 GetRandomCellPos(int x, int y)
        {
            return new Vector2(GetRandomCellPosX(x), GetCellPosY(y));
        }
        public static Vector2 GetRightCellPos(int x, int y)
        {
            return new Vector2(GetCellPosX(x) + SBFunc.Random(CellSpancing *.2f, CellSpancing * .5f), GetCellPosY(y));
        }
        public static Vector2 GetLeftCellPos(int x, int y)
        {
            return new Vector2(GetCellPosX(x) - SBFunc.Random(CellSpancing * .2f, CellSpancing * .5f), GetCellPosY(y));
        }
        public static Vector2 GetBuildingPos(int x, int y)
        {
            return new Vector2(GetBuildingPosX(x), GetCellPosY(y));
        }
        public static Vector2 GetWallPos(int x, int y)
        {
            bool isX = x > X;
            return new Vector2(StartOffsetX() + (isX ? CellBothSpancing : 0) + (isX ? (x - 1) * CellSpancing : 0), y * FloorSpancing + WallY);
        }
        public static Vector2 GetGuildWallPos(int x, int y)
        {
            //bool isX = x > X;
            return new Vector2((x - 1.5f) * CellSpancing, y * FloorSpancing + WallY + GuildCellSpancing);
        }
        public static Vector2 GetHeadPos(int y)
        {
            return new Vector2(0f, HeadY + y * FloorSpancing);
        }
        public static Vector2 GetHeadPosForGuild(int y)
        {
            return new Vector2(0f, HeadY + y * FloorSpancing + GuildTopSpancing);
        }
        public static Vector2 GetSliceHeadPos(int x)
        {
            return new Vector2(GetCellPosX(x) - 15f, 3.775f);
        }
        public static Vector2 GetElevatorPos(int y, bool isLeft)
        {
            return new Vector2(isLeft ? -GetElevatorPosX() : GetElevatorPosX(), GetElevatorPosY(y));
        }

        public static Vector2 GetTopFlagPos(int y, bool isLeft)
        {
            return new Vector2(isLeft ? -GetElevatorPosX() : GetElevatorPosX(), GetElevatorPosY(y) + WatchTowerSpancing);
        }

        public static float GetElevatorPosX()
        {
            return CellBothSpancing + (Width - 2) * CellSpancing * 0.5f + ElevatorCellSpancing;
        }
        public static float StartOffsetX()
        {
            return -(CellBothSpancing + (Width - 2) * CellSpancing * 0.5f);
        }
        public static float GetElevatorContainerPosY(int y)
        {
            return y * ElevatorFloorSpancing;
        }
        public static float GetElevatorPosY(int y)
        {
            return y < 0 ? y * UnderElevatorFloorSpancing : y * ElevatorFloorSpancing;
        }
        public static float StartOffsetY()
        {
            return GetUnderFloorSpacing(Y) - 1.5f;
        }
        public static float MiddleOffsetY()
        {
            return EndOffsetY() - StartOffsetY();
        }
        public static float EndOffsetY()
        {
            return FloorSpancing * (Height - 1) + 16f;
        }
        public static Vector4 MapCameraSize()
        {
            const float cameraHeadSpancing = 14f;
            const float cameraLeftRightSpancing = 6f;
            const float guildSpancing = 4.5f;
            const float cameraUnder = -4.7f;
            float cameraTop = Height * FloorSpancing + cameraHeadSpancing + (GuildManager.Instance.GuildBuildingShowAble ? guildSpancing: 0);
            float cameraLeftRight = (Width - 2) * CellSpancing * 0.5f + CellBothSpancing + cameraLeftRightSpancing;
            float cameraBotton = cameraUnder + UnderFloorSpancing + GetUnderFloorSpacing(Y);
            var gemdungeonData = LandmarkGemDungeon.Get();
            if (gemdungeonData.FloorDatas.Count>0)
                cameraBotton -= GetUnderFloorSpacing(gemdungeonData.FloorDatas.Count);
            else if(BuildingOpenData.GetByInstallTag((int)eLandmarkType.GEMDUNGEON) != null)
                cameraBotton -= UnderFloorSpancing;

            return new Vector4(-cameraLeftRight, cameraBotton, cameraLeftRight, cameraTop);
        }
        public static Vector2Int GetCellPosByInputPosition(Vector2 inputWorldPos)
        {
            int cell = Mathf.FloorToInt((inputWorldPos.x + Width * CellSpancing * 0.5f) / CellSpancing);
            int floor = 0;
            if(inputWorldPos.y > 0f)
            {
                floor = Mathf.FloorToInt(inputWorldPos.y / FloorSpancing);
            }
            else
            {
                floor = Mathf.FloorToInt(inputWorldPos.y / UnderFloorSpancing);
            }

            return new Vector2Int(floor, cell);
        }

        public static Vector3Int GetCellPosByWorldPos(Vector3 position)
        {
            int cell = Mathf.FloorToInt((position.x + Width * CellSpancing * 0.5f) / CellSpancing);
            int floor = 0;
            if (position.y >= 0f)
            {
                floor = Mathf.FloorToInt(position.y / FloorSpancing);
            }
            else
            {
                floor = Mathf.FloorToInt(position.y / UnderFloorSpancing);
            }

            return new Vector3Int(cell, floor, (int)position.z);
        }
    }
}
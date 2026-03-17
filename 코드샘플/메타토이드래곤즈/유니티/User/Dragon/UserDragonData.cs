using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public class UserDragonData
    {
        private Dictionary<int, UserDragon> dragons = new Dictionary<int, UserDragon>();
        
        public UserDragon AddUserDragon(int tag, UserDragon dragon)
        {
            dragons[tag] = dragon;
            return dragons[tag];
        }

        public bool DeleteUserDragon(int tag)
        {
            if (dragons.ContainsKey(tag))
            {
                dragons.Remove(tag);
                return true;
            }
            return false;
        }

        public UserDragon GetDragon(int tag)
        {
            if (dragons.ContainsKey(tag))
            {
                return dragons[tag];
            }

            return null;
        }
        public bool IsContainsDragon(int tag)
        {
            if (dragons == null)
                return false;

            return dragons.ContainsKey(tag);
        }
        /// <summary>
        /// 유저가 보유중인 드래곤인가
        /// </summary>
        /// <param name="_tag"></param>
        /// <returns></returns>
        public bool IsUserDragon(int _tag)
        {
            return dragons.ContainsKey(_tag);
        }

        public List<UserDragon> GetRandomDragon(int count, Dictionary<int, UserDragon> pickDragons = null)
        {
            List<int> keys = new List<int>(dragons.Keys);
            if (keys.Count < count)
            {
                return null;
            }

            List<UserDragon> returnDragons = new List<UserDragon>();

            var keysCount = keys.Count;
            while (returnDragons.Count < count)
            {
                var random = SBFunc.Random(0, keysCount);
                var tag = keys[random];

                if (!dragons.ContainsKey(tag))
                    continue;

                UserDragon pick = dragons[tag];
                if (pick == null)
                {
                    continue;
                }

                if (returnDragons.Contains(pick))
                {
                    continue;
                }

                returnDragons.Add(dragons[tag]);
            }

            return returnDragons;
        }

        public List<UserDragon> GetAllUserDragons()
        {
            return dragons.Values.ToList();
        }
        public int GetAllUserDragonCount()
        {
            return GetAllUserDragons().Count;
        }
        public void ClearData()
        {
            dragons.Clear();
        }

        /// <summary>
        /// 글로벌 버프가 들어온(업데이트) 시점에서 모든 드래곤의 스탯을 다시 계산해준다.
        /// </summary>
        public void RefreshALLDragonStat()
        {
            if (dragons == null || dragons.Count <= 0)
                return;

            var list = GetAllUserDragons();
            foreach(var dragon in list)
            {
                if (dragon == null)
                    continue;

                dragon.RefreshALLStatus();
            }
        }

        Dictionary<int, bool> favorites = new Dictionary<int, bool>();
        public bool IsFavorite(int tag)
        {
            if (favorites.ContainsKey(tag))
                return favorites[tag];

            if (IsUserDragon(tag))
            {
                bool bF = CacheUserData.GetBoolean("dragonfavorite_" + tag, false);
                favorites.Add(tag, bF);

                return bF;
            }

            return false;
        }

        public void SetFavorite(int tag, bool favorite)
        {
            if (IsUserDragon(tag))
            {
                CacheUserData.SetBoolean("dragonfavorite_" + tag, favorite);

                if (favorites.ContainsKey(tag))
                    favorites[tag] = favorite;
                else
                    favorites.Add(tag, favorite);

                if (favorite)
                    ToastManager.On(StringData.GetStringByStrKey("드래곤즐겨찾기"));
                else
                    ToastManager.On(StringData.GetStringByStrKey("드래곤즐겨찾기취소"));
            }
        }
    }
}
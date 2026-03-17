using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public abstract class BattleLine
    {
        /// <summary> 자리 최대 배열  </summary>
        public virtual int MaxCount { get; private set; } = -1;
        /// <summary> 덱에서 드래곤 최대 배치 숫자 </summary>
        protected abstract int MaxDeckCount { get; }
        /// <summary> 덱에서 드래곤 숨김 가능 숫자 </summary>
        protected abstract int HiddenCount { get; }
        /// <summary> Line 수 </summary>
        protected abstract int XSize { get; }
        /// <summary> Line의 Count </summary>
        protected abstract int YSize { get; }
        /// <summary> 현재 배치된 드래곤 숫자 </summary>
        public virtual int DeckCount { get; private set; } = 0;
        /// <summary> 드래곤 배열 </summary>
        protected virtual int[] Dragons { get; set; } = null;
        public BattleLine()
        {
            DeckCount = 0;
            MaxCount = XSize * YSize;
            Dragons = new int[MaxCount];
            Clear();
        }
        /// <summary> 해당 Index 가 진짜 유효한가 </summary>
        public bool IsAvailable(int idx)
        {
            if (Dragons == null)
                return false;

            if (idx < 0 || Dragons.Length <= idx)
                return false;

            return true;
        }
        /// <summary> 좌표를 이용해 Index 가져오기 </summary>
        protected int GetIndex(int x, int y)
        {
            return x * YSize + y;
        }
        /// <summary> 해당 Index의 드래곤 번호 가져오기 </summary>
        public int GetDragon(int idx)
        {
            if (false == IsAvailable(idx))
                return 0;

            return Dragons[idx];
        }
        /// <summary> 해당 좌표의 드래곤 번호 가져오기 </summary>
        public int GetDragon(int x, int y)
        {
            return GetDragon(GetIndex(x, y));
        }
        /// <summary> 어떤 BattleLine을 사용하는가는 하위 구현 </summary>
        public abstract bool LoadBattleLine(int index = 0);
        /// <summary> BattleLine 통째로 입력 </summary>
        public virtual bool SetLine(List<int> formation)
        {
            Clear();
            if (formation == null || formation.Count <= 0)
                return false;

            for (int i = 0; i < formation.Count; ++i)
            {
                SetDragon(i, formation[i]);
            }
            return true;
        }
        /// <summary> 해당 Index에 태그 변경 </summary>
        protected void SetDragon(int idx, int tag)
        {
            if (false == IsAvailable(idx))
                return;

            if (Dragons[idx] == 0 && tag != 0)
                DeckCount++;
            else if (Dragons[idx] != 0 && tag == 0)
                DeckCount--;

            Dragons[idx] = tag;
        }
        public void AddDragonPosition(int x, int y, int tag)
        {
            SetDragon(GetIndex(x, y), tag);
        }
        public void DeleteDragon(int tag)
        {
            for (int i = 0; i < MaxCount; ++i)
            {
                if (Dragons[i] == tag)
                {
                    SetDragon(i, 0);
                    break;
                }
            }
        }
        public void DeleteDragonByIndex(int idx)
        {
            SetDragon(idx, 0);
        }
        public void ChangeDragon(int currentTag, int changeTag)
        {
            for (int i = 0; i < MaxCount; ++i)
            {
                if (Dragons[i] == currentTag)
                {
                    SetDragon(i, changeTag);
                    break;
                }
            }
        }
        public void SwapSlot(int a, int b)
        {
            if (false == IsAvailable(a) || false == IsAvailable(b))
                return;

            var temp = Dragons[a];
            Dragons[a] = Dragons[b];
            Dragons[b] = temp;
        }
        public virtual void Clear()
        {
            for (int i = 0; i < MaxCount; ++i)
            {
                Dragons[i] = 0;
            }
            DeckCount = 0;
        }
        public int[] GetArray()
        {
            return Dragons;
        }
        public List<int> GetList()
        {
            return Dragons.ToList();
        }
        public virtual string GetJsonString()
        {
            var colArray = new JArray();

            for (int i = 0; i < XSize; ++i)
            {
                var rowArray = new JArray();
                for (int j = YSize - 1; j >= 0; --j)
                {
                    var dragon_no = GetDragon(i, j);
                    if (dragon_no == 0)
                        continue;

                    rowArray.Add(dragon_no);
                }
                colArray.Add(rowArray);
            }

            return colArray.ToString();
        }
        public string GetJsonStringINF()
        {
            var colArray = new JObject();

            for (int i = 0, count = Dragons.Length; i < count; i++)
            {
                var id = Dragons[i];
                if (id < 1)
                    continue;

                var dragon = User.Instance.DragonData.GetDragon(id);
                if (dragon == null)
                    continue;

                colArray.Add(id.ToString(), dragon.GetTotalINF());
            }

            return colArray.ToString();
        }

        public virtual bool IsDeckFull()
        {
            return DeckCount >= MaxDeckCount;
        }
        /// <summary> 현재 드래곤 데이터가 0보다 크고(이미 드래곤이 있다) 덱 풀 상태에서의 교체 로직 종속 조건 </summary>
        public bool IsChangePossibleCondition(int index)
        {
            return GetDragon(index) > 0;
        }

        public bool IsDeckEmpty()
        {
            return DeckCount == 0;
        }

        public bool IsContainDragon(int tag)
        {
            if (Dragons == null)
                return false;

            return Dragons.Contains(tag);
        }
    }
}
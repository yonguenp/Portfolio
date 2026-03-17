using System;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class ToolTipData
    {
        public string TitleStr { get; private set; }
        public string ContentStr { get; private set; } //DESC
        public bool IsReverse { get; private set; } //정방향 false

        public bool IsUpSideDown { get; private set; } // 위아래 위치 // 위 - false/ 아래 - true
        public eToolTipDataFlag Flag { get; private set; }

        private GameObject parentObj;

        public GameObject Parent
        {
            get { return parentObj; }
            set { parentObj = value; }
        }


        public ToolTipData(string titleStr, string contentStr, GameObject parent, bool reverse = false, bool lower =false, eToolTipDataFlag flag = eToolTipDataFlag.Default)
        {
            TitleStr = titleStr;
            ContentStr = contentStr;
            Flag = flag;
            parentObj = parent;
            IsReverse = reverse;
            IsUpSideDown = lower;
        }
    }
    public class ItemToolTipData : ToolTipData
    {
        public ItemFrame Frame { get; private set; } = null;
        public ItemToolTipData(string titleStr, string contentStr, GameObject parent, bool dir, bool lower, eToolTipDataFlag flag, ItemFrame frame)
            : base(titleStr, contentStr, parent, dir, lower, flag)
        {
            Frame = frame;
        }
    }

    public class CharLevelExpData
    {
        public int FinalLevel { get; set; }
        public int ReduceExp { get; set; }
    }
}
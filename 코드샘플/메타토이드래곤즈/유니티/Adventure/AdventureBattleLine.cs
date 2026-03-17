using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SandboxNetwork {
    public class AdventureBattleLine : BattleLine
    {
        public AdventureBattleLine() : base() { }
        protected override int MaxDeckCount => 5;
        protected override int HiddenCount => 0;
        protected override int XSize => 3;
        protected override int YSize => 2;

        public override bool LoadBattleLine(int index = 0)
        {
            var adventureData = User.Instance.PrefData.AdventureFormationData;
            if (null == adventureData) 
                return false;
            
            return SetLine(adventureData.GetFormation(index));
        }
    }

    public class RestrictedBattleLine : BattleLine
    {
        public RestrictedBattleLine() : base() { }
        protected override int MaxDeckCount => 5;
        protected override int HiddenCount => 0;
        protected override int XSize => 3;
        protected override int YSize => 2;

        int world = -1;
        int diff = -1;
        public override bool LoadBattleLine(int index = 0)
        {
            world = index;
            List<int> line = new List<int>();

            for(int i = 0; i < 6; i++)
            {
                int no = CacheUserData.GetInt("restricted_deck_" + diff + "_" + index + "_" + i, -1);
                if (no < 0)
                    no = 0;

                line.Add(no);
            }         
            
            return SetLine(line);
        }
        public bool LoadBattleLine(int index, StageDifficult d)
        {
            world = index;
            diff = (int)d;

            List<int> line = new List<int>();

            for (int i = 0; i < 6; i++)
            {
                int no = CacheUserData.GetInt("restricted_deck_" + diff + "_" + index + "_" + i, -1);
                if (no < 0)
                    no = 0;

                line.Add(no);
            }

            return SetLine(line);
        }
        public void Save()
        {
            var cur = GetList();
        
            for (int i = 0; i < 6; i++)
            {
                if(cur[i] > 0)
                    CacheUserData.SetInt("restricted_deck_" + diff + "_" + world + "_" + i, cur[i]);
                else
                    CacheUserData.SetInt("restricted_deck_" + diff + "_" + world + "_" + i, 0);
            }
        }
    }
}
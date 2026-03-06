using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public struct BattleCharacter
    {
        public BattleCharacter(int bTag, int dTag)
        {
            BattleTag = bTag;
            DragonTag = dTag;
            Type = -1;
            Group = -1;
            SpawnID = -1;
            IsEnemy = false;
        }
        public BattleCharacter(int bTag, int type, int group, int spawnID)
        {
            BattleTag = bTag;
            DragonTag = -1;
            Type = type;
            Group = group;
            SpawnID = spawnID;
            IsEnemy = true;
        }
        public int BattleTag { get; private set; }
        public int DragonTag { get; private set; }
        public int Type { get; private set; }
        public int Group { get; private set; }
        public int SpawnID { get; private set; }
        public bool IsEnemy { get; private set; }
    }
}
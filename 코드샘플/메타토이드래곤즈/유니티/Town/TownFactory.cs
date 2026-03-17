using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    //Order 지정 미사용(프리팹 적용)
    //public enum eTownSpriteOrder
    //{
    //    UnderFrontEscalator                 = 3,
    //    UnderBackEscalator                  = 1,
    //    UnderWireBack                       = 0,
    //    UnderWireFront                      = 3,


    //    UnderBackDragon                     = -1,
    //    UnderFrontDragon                    = 3,
    //    Dragon                              = 0,

    //    CellBack                            = -1,
    //    CellFront                           = 5,

    //    ElevatorDecoTower                   = 5,
    //    ElevatorDecoFront                   = 4,
    //    ElevatorDecoBack                    = -4,
    //    ElevatorDecoRail                    = -3,
    //    ElevatorDecoPulley                  = -2,
    //    ElevatorBack                        = -1,
    //    ElevatorFront                       = 4,
    //}
    public class TownFactory : MonoBehaviour
    {
        //Factory 두개 생성 방지
        private static TownFactory instance = null;
        public static TownFactory Instance
        {
            get { return instance; }
        }
        //

        [SerializeField]
        private List<GameObject> townPrefabs = null;
        private Dictionary<string, GameObject> dicPrefabs = null;

        void Start()
        {
            if (instance == null)
            {
                instance = this;
                dicPrefabs = new Dictionary<string, GameObject>();

                if (townPrefabs != null)
                {
                    for (int i = 0, count = townPrefabs.Count; i < count; ++i)
                    {
                        if (townPrefabs[i] == null)
                            continue;

                        dicPrefabs.Add(townPrefabs[i].name, townPrefabs[i]);
                    }
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }
        private void OnDestroy()
        {
            if (this == instance)
            {
                instance = null;
            }
        }
        public static GameObject GetCell(int x)
        {
            if(x == 0)
            {
                return Instance.dicPrefabs["cell_L"];
            }
            else if(x == (TownMap.Width - 1))
            {
                return Instance.dicPrefabs["cell_R"];
            }
            else
            {
                return Instance.dicPrefabs["cell_M"];
            }
        }
        
        public static GameObject GetGuildCell(int x)
        {
            if (x == 0)
            {
                return Instance.dicPrefabs["cell_guild_L"];
            }
            else if (x == 2)
            {
                return Instance.dicPrefabs["cell_guild_R"];
            }
            else
            {
                return Instance.dicPrefabs["cell_guild_M"];
            }
        }
        public static GameObject GetUnderCell(int x)
        {
            if (x == 0)
            {
                return Instance.dicPrefabs["under_L"];
            }
            else if (x == (TownMap.Width - 1))
            {
                return Instance.dicPrefabs["under_R"];
            }
            else
            {
                return Instance.dicPrefabs["under_M"];
            }
        }

        public static GameObject GetGemUnderCell(int x)
        {
            if (x == 0)
            {
                return Instance.dicPrefabs["gem_under_L"];
            }
            else if (x == (TownMap.Width - 1))
            {
                return Instance.dicPrefabs["gem_under_R"];
            }
            else
            {
                return Instance.dicPrefabs["gem_under_M"];
            }
        }

        public static GameObject GetTopCell(int x)
        {
            if (x == 0)
            {
                return Instance.dicPrefabs["top_L"];
            }
            else if (x == (TownMap.Width - 1))
            {
                return Instance.dicPrefabs["top_R"];
            }
            else
            {
                return Instance.dicPrefabs["top_M"];
            }
        }
        public static GameObject GetGuildTopCell(int x)
        {
            if (x == 0)
            {
                return Instance.dicPrefabs["top_guild_L"];
            }
            else if (x == 2)
            {
                return Instance.dicPrefabs["top_guild_R"];
            }
            else
            {
                return Instance.dicPrefabs["top_guild_M"];
            }
        }
        public static GameObject GetWall()
        {
            return Instance.dicPrefabs["wall"];
        }
        public static GameObject GetHead()
        {
            return Instance.dicPrefabs["top_head"];
        }
        public static GameObject GetElevatorBG(int floor)
        {
            if ((TownMap.Height - 1) == floor)
            {
                return Instance.dicPrefabs["tower_T"];
            }
            else if (floor < 0)
            {
                return Instance.dicPrefabs["tower_U"];
            }
            else
            {
                return Instance.dicPrefabs["tower_F"];
            }
        }
        public static GameObject GetElevatorDeco()
        {
            return Instance.dicPrefabs["tower_D"];
        }
        public static GameObject GetElevator()
        {
            return Instance.dicPrefabs["elevator"];
        }
        public static GameObject GetEscalatorLeft()
        {
            return Instance.dicPrefabs["escalator_L"];
        }
        public static GameObject GetEscalatorRight()
        {
            return Instance.dicPrefabs["escalator_R"];
        }
        public static GameObject GetBuilding(string name)
        {
            if(Instance.dicPrefabs.ContainsKey(name))
                return Instance.dicPrefabs[name];
            return null;
        }
        public static GameObject GetLocked()
        {
            return Instance.dicPrefabs["LockObject"];
        }

        public static GameObject GetGuildFlagTop()
        {
            return Instance.dicPrefabs["top_guild_Flag"];
        }
    }
}
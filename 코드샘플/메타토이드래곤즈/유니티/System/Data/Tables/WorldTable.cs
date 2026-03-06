using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SandboxNetwork
{
    public class WorldTable : TableBase<WorldData, DBWorld_base>
    {
        public override void Preload()
        {
            base.Preload();
            LoadAll();
        }
        public WorldData GetByWorldNumber(int worldNumber)
        {
            WorldData wData = null;
            foreach (KeyValuePair<string, WorldData> element in datas)
            {
                if (element.Value.NUM == worldNumber)
                {
                    wData = element.Value;
                    break;
                }
            }
            return wData;
        }
    }
}
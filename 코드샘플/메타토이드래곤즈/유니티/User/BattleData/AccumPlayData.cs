using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace SandboxNetwork
{
    public class AccumPlayData
    {
        public Dictionary<int, Dictionary<int, Asset>> Rewards { get; private set; } = new Dictionary<int, Dictionary<int, Asset>>();
        public int Count { get; private set; } = -1;
        public int TotalCount { get; private set; } = -1;

        public void StageCompleteAccum()
        {
            Count--;
        }
        public void StageStartAccum()
        {
            Count = 0;
        }

        public void SetAccumData(int count, int total)
        {
            Count = count;
            TotalCount = total;
        }
    }
}
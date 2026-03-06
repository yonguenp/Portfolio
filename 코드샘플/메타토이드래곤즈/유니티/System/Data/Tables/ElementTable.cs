using Newtonsoft.Json.Linq;

namespace SandboxNetwork
{
    public class ElementTable : TableBase<ElementRateData, DBElement_rate>
    {
        public ElementRateData Get(eElementType type)
        {
            return Get((int)type);
        }
    }
}
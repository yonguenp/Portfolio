using Newtonsoft.Json.Linq;

namespace SandboxNetwork
{
	public class GameConfigData: TableData<DBGame_config>
	{
		static private GameConfigTable table = null;
		static public GameConfigData Get(string key)
		{
			if (table == null)
				table = TableManager.GetTable<GameConfigTable>();

			return table.Get(key);
		}
		public string KEY => UNIQUE_KEY;
		public string VALUE => Data.VALUE;
		public string TYPE => Data.TYPE;
    }
}
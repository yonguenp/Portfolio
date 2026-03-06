using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SandboxNetwork
{
    public class SoundResourceData : TableData<DBSound_resource>
    {
        static private SoundResourceTable table = null;
        static public SoundResourceData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<SoundResourceTable>();

            return table.Get(key);
        }
        static public List<SoundResourceData> GetAll()
        {
            if (table == null)
                table = TableManager.GetTable<SoundResourceTable>();

            return table.GetAllList();
        }
        static public string GetSoundNameBySoundKey(string soundKey)
        {
            if (table == null)
                table = TableManager.GetTable<SoundResourceTable>();

            return table.GetSoundNameBySoundKey(soundKey);
        }

        public string SOUND_KEY => Data.SOUND_KEY;
        public int TYPE => Data.TYPE;
        public string SOUND_FILE_NAME => Data.SOUND_FILE_NAME;

    }
}
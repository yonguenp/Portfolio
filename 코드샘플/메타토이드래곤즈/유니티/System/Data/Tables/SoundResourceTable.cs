using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SandboxNetwork
{
    public class SoundResourceTable : TableBase<SoundResourceData, DBSound_resource>
    {
        private static Dictionary<string, string> _SoundKeyToNameDic = new Dictionary<string, string>();
        public override void Init()
        {
            base.Init();
        }


        public override void Preload()
        {
            base.Preload();
            LoadAll();

            foreach(var data in datas.Values)
            {
                _SoundKeyToNameDic[data.SOUND_KEY] = data.SOUND_FILE_NAME;
            }
        }

        

        public string GetSoundFileName(string soundKey)
        {
            string resultFileName = "";
            foreach (KeyValuePair<string, SoundResourceData> element in datas)
            {
                if (element.Value.SOUND_KEY == soundKey)
                {
                    resultFileName = element.Value.SOUND_FILE_NAME;
                    break;
                }
            }

            return resultFileName;
        }

        public string GetSoundNameBySoundKey(string soundKey)
        {
            if (_SoundKeyToNameDic.ContainsKey(soundKey))
                return _SoundKeyToNameDic[soundKey];
            return string.Empty;
        }
    }
}

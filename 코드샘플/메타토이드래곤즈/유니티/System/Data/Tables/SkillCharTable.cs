using Newtonsoft.Json.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public class SkillCharTable : TableBase<SkillCharData, DBSkill_char>
    {
        public static Sprite DefaultSkillIcon { get; private set; } = null;
        public override void Init()
        {
            base.Init();
        }
        public override void Preload()
        {
            base.Preload();
            DefaultSkillIcon = ResourceManager.GetResource<Sprite>(eResourcePath.SkillIconPath, "Skill-Icon_hammershield_dark");
        }
        public SkillCharData GetKey(long key)
        {
            return Get(key.ToString());
        }
    }
}
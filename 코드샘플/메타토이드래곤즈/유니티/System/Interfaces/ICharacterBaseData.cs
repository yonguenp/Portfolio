namespace SandboxNetwork
{
    public interface ICharacterBaseData
    {
        public int KEY { get; }
        public int FACTOR { get; }
        public int GRADE { get; }
        public int ELEMENT { get; }
        public eElementType ELEMENT_TYPE { get; }
        public string IMAGE { get; }
        public string THUMBNAIL { get; }
        public string SKIN { get; }
        public string _NAME { get; }
        public string _DESC { get; }
        public int ATK { get; }
        public int DEF { get; }
        public int HP { get; }
        public float CRI_PROC { get; }
        public int CRI_DMG { get; }
        public int LIGHT_DMG { get; }
        public int DARK_DMG { get; }
        public int WATER_DMG { get; }
        public int FIRE_DMG { get; }
        public int WIND_DMG { get; }
        public int EARTH_DMG { get; }
        public int ADD_PVP_DMG { get; }
        public float RATIO_PVP_DMG { get; }
        public int ADD_PVP_CRI_DMG { get; }
        public float RATIO_PVP_CRI_DMG { get; }
        public float ADD_ATKSPEED { get; }
        public float MOVE_SPEED { get; }
        public bool IsMonster { get; }
        public SkillCharData NORMAL_SKILL { get; }
        public SkillCharData SKILL1 { get; }
        public SkillCharData SKILL2 { get; }
        public string SCRIPT_OBJECT_KEY { get; }
        public float SIZE { get; }
        public UnityEngine.Sprite GetThumbnail();
        public eCharacterImmunity Immunity { get; }
    }
}
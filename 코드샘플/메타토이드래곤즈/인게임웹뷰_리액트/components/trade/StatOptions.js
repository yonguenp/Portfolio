
export const STAT_OPTIONS = [
  {
    key: "ATK",
    label: "ATK",
    type: 1,
    no: [1001, 1021, 1026, 1044],
  },
  {
    key: "DEF",
    label: "DEF",
    type: 1,
    no: [1002, 1022, 1027, 1045],
  },
  {
    key: "HP",
    label: "HP",
    type: 1,
    no: [1003, 1023, 1028, 1046],
  },
  {
    key: "CRI_PROC",
    label: "CRIT%",
    type: 1,
    no: [1004, 1024, 1029, 1047],
  },
  // {
  //   key: "CRI_RESIS",
  //   label: "CRIT Resist",
  //   no: [1005],
  // },
  {
    key: "CRI_DMG_RESIS",
    label: "CRIT DMG Resist",
    type: 1,
    no: [1006, 1030],
  },
  // {
  //   key: "SKILL_DMG_RESIS",
  //   label: "Skill DMG Resist",
  //   no: [1007],
  // },
  {
    key: "ATK_DMG_RESIS",
    label: "Base Resist",
    type: 1,
    no: [1008, 1019, 1031, 1041, 1043],
  },
  {
    key: "BOSS_DMG",
    label: "Boss Damage",
    type: 1,
    no: [1020, 1025, 1042, 1048],
  },  
  {
    key: "DEL_COOLTIME",
    label: "Cooldown",
    type: 1,
    no: [1015, 1049],
  },
  {
    key: "ADD_BUFF_TIME",
    label: "Buff Duration",
    type: 1,
    no: [1016, 1038],
  },
  {
    key: "DEL_BUFF_TIME",
    label: "Debuff Duration",
    type: 1,
    no: [1017, 1039],
  },
  {
    key: "ADD_ATKSPEED",
    label: "ATK Speed",
    type: 1,
    no: [1018, 1040],
  },
  {
    key: "LIGHT_DMG_RESIS",
    label: "Light Resist",
    no: [1009, 1032],
    type: 1,
    useless : true,
  },
  {
    key: "DARK_DMG_RESIS",
    label: "Dark Resist",
    no: [1010, 1033],
    type: 1,
    useless : true,
  },
  {
    key: "WATER_DMG_RESIS",
    label: "Water Resist",
    no: [1011, 1034],
    type: 1,
    useless : true,
  },
  {
    key: "FIRE_DMG_RESIS",
    label: "Fire Resist",
    no: [1012, 1035],
    type: 1,
    useless : true,
  },
  {
    key: "WIND_DMG_RESIS",
    label: "Wind Resist",
    no: [1013, 1036],
    type: 1,
    useless : true,
  },
  {
    key: "EARTH_DMG_RESIS",
    label: "Earth Resist",
    no: [1014, 1037],
    type: 1,
  },
  {
    key: "ALL_ELEMENT_DMG_RESIS",
    label: "Attributes Resist",
    no: [],            // 실제 번호 있으면 나중에 채우면 됨
    type: 1,
    useless: true,     // 기존 속성저항들과 동일 취급
  },
  {
    key: "PHYS_DMG_RESIS",
    label: "Physical Resist",
    type: 1,
    no: [],           
  },
  //fusion
  {
    key: "DEL_START_COOLTIME",
    label: "Initial Cooldown",
    type: 1,
    no: [],           
  },
  {
    key: "ALL_ELEMENT_DMG_PIERCE",
    label: "Elemental Res Pierce",
    type: 1,
    no: [],           
  },
  {
    key: "PHYS_DMG_PIERCE",
    label: "Phys Res Pierce",
    type: 1,
    no: [],           
  },
  {
    key: "CRI_DMG_PIERCE",
    label: "Critical Res Pierce",
    type: 1,
    no: [],           
  },
  {
    key: "RATIO_PASSIVE_EFFECT",
    label: "Passive Effect Amp",
    type: 1,
    no: [],           
  },  
  {
    key: "RATIO_PASSIVE_RATE",
    label: "Passive Proc Amp",
    type: 1,
    no: [],           
  },  
  {
    key: "DEL_KNOCKBACK_DISTANCE",
    label: "Knockback Reduced",
    type: 1,
    no: [],           
  },
  {
    key: "CRI_DMG",
    label: "Critical Damage",
    type: 1,
    no: [],           
  },  
  {
    key: "ADD_ATK_DMG",
    label: "Base Damage",
    type: 1,
    no: [],           
  },  
  {
    key: "RATIO_BOSS_DMG",
    label: "Boss Damage Amp",
    type: 1,
    no: [],           
  },
  {
    key: "ADD_MAIN_ELEMENT_DMG",
    label: "Main Elemental DMG",
    type: 1,
    no: [],           
  },
  
];

export const GRADE_OPTIONS = [
  // { key: "Common" },
  { key: "Uncommon", label: "Uncommon" },
  { key: "Rare", label: "Rare" },
  { key: "Unique", label: "Unique" },
  { key: "Unique_FUSION", label: "Unique FUSION" },
  { key: "Legendary", label: "Legendary" },
  { key: "Legendary_FUSION", label: "Legendary FUSION" },
];

export const TYPE_OPTIONS = [
  { key: "Advanced"  },
  { key: "Standard" },
];

export const CATEGORY_OPTIONS = [
  { key: "Berserker" },
  { key: "Dragon-Scale" },
  { key: "Heartbeat" },
  { key: "Awakening" },
  { key: "Patience" },
  { key: "Determination" },
  { key: "Invincible" },
  { key: "Guardian" },
  { key: "Shining" },
  { key: "Shadow" },
  { key: "Water-Drop" },
  { key: "Flame-Shard" },
  { key: "Wind-Flower" },
  { key: "Loess-Stone" },
  { key: "Quickness" },
  { key: "Meteor" },
];
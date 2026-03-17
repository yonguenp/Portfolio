using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public delegate void VoidDelegate();

    public delegate void JsonDelegate(JObject jsonData);

    public delegate void IntDelegate(int index);

    public delegate void DownloadState(int val = 0, int maxVal = 0);
    public static class SBDefine
    {
        public static readonly int Day = 86400;
        public static readonly int Hour = 3600;
        public static readonly int Minute = 60;
        public static readonly int DayHour = 24;
        public static readonly int CONVERT_INT = 1;
        public static readonly int DIV_ARENA_SEASON = 100;
        public static readonly int BUILDING_TAG_MULT = 100;
        public static readonly float BASE_FLOAT = 100f;
        public static readonly float CONVERT_FLOAT = .01f;
        public static readonly float CONVERT_MILLION = 0.000001f;
        public static readonly float CONVERT_ELEMENT = 0.000001f;
        public static readonly float BattleTileX = 0.4f;
        public static readonly float BattleTileY = 0.4f;
        public const int THOUSAND = 1000;
        public static readonly int MILLION = 1000000;
        public static readonly int BILLION = 1000000000;
        public static readonly float CONVERT_MILLION_PERC = .0001f;
        public static readonly float CONVERT_THOUSAND = .001f;

        public static readonly int DEFAULT_ATTENDANCE_REWARD_GROUP = -1;
        #region TYPE
        public static readonly Type TYPE_SHORT = typeof(short);
        public static readonly Type TYPE_INT = typeof(int);
        public static readonly Type TYPE_LONG = typeof(long);
        public static readonly Type TYPE_DECIMAL = typeof(decimal);
        public static readonly Type TYPE_FLOAT = typeof(float);
        public static readonly Type TYPE_DOUBLE = typeof(double);
        public static readonly Type TYPE_STRING = typeof(string);
        public static readonly Type TYPE_SPINEASSET = typeof(Spine.Unity.SkeletonDataAsset);
        public static readonly Type TYPE_AUDIOCLIP = typeof(AudioClip);
        public static readonly Type TYPE_SPRITE = typeof(Sprite);
        public static readonly Type TYPE_KNOCKBACK = typeof(KnockbackEffect);
        public static readonly Type TYPE_DOZER = typeof(LandmarkDozer);
        public static readonly Type TYPE_TRAVEL = typeof(LandmarkTravel);
        public static readonly Type TYPE_SUBWAY = typeof(LandmarkSubway);
        public static readonly Type TYPE_EXCHANGE = typeof(LandmarkExchange);
        public static readonly Type TYPE_GEMDUNGEON = typeof(LandmarkGemDungeon);
        public static readonly Type TYPE_MINE = typeof(LandmarkMine);
        #endregion
        #region Item
        public static readonly int ITEM_GOLD = 10000001;
        public static readonly int ITEM_ENERGY = 10000002;
        public static readonly int ITEM_ACC_EXP = 10000003;
        public static readonly int ITEM_GEMSTONE = 10000005;
        public static readonly int ITEM_TICKET_PVP = 10000007;
        public static readonly int ITEM_MAGNET = 10000009;
        #endregion
        #region Town
        public static float ElevatorFloorSpancing { get { return 2.14f; } }
        public static float UnderElevatorFloorSpancing { get { return 2.52f; } }
        public static float ElevatorCellSpancing { get { return 1.2f; } }
        public static float WatchTowerSpancing { get { return 2.8f; } }
        public static float FloorSpancing { get { return 2.14f; } }
        public static float UnderFloorSpancing { get { return 3.71f; } }
        public static float UnderFloorGemDungeonSpancing { get { return 0.15f; } }

        public static float GetUnderFloorSpacing(int y)
        {
            return UnderFloorGemDungeonSpancing * (y + 1) + UnderFloorSpancing * y;
        }
        public static float CellBothSpancing { get { return 3.48f; } }
        public static float CellSpancing { get { return 3.24f; } }
        public static float GuildCellSpancing { get { return 1f; } }
        public static float BuildingBothSpine { get { return 0.12f; } }
        public static float WallY { get { return 0.55f; } }
        public static float HeadY { get { return 0.95f; } }
        public static float DragonY { get { return 0.5f; } }
        public static float UnderFrontDragonY { get { return 0.8f; } }
        public static float UnderBackDragonY { get { return 2.02f; } }
        public static float ElevatorInOutX { get { return 1.75f; } }
        public static float ElevatorOrderX { get { return 1.2f; } }
        public static float ElevatorRandX { get { return 0.3f; } }

        public static float GuildTopSpancing { get { return 2f; } }
        public static float GuildYStartSpancing { get { return 0.5f; } }
        public static int GuildCellWidth { get { return 3; } }

        public const float GuildBuildingSizeX = 2.0f;
        public const float GuildBuildingSizeFormX = 1.5f * GuildBuildingSizeX;

        public static int GemDungeonDefaultFloor = -2;
        #endregion
        #region Dragon
        public static int TownDefaultSpeed { get { return 100; } }
        public static int TownDefaultMoveSpeed { get { return 100; } }

        public static float AdventureLobbyScrollTime { get { return 0.2f; } }
        public static float AdventureSpeedRatio { get { return 200f; } }
        public static int UnderFrontOrder { get { return 3; } }
        public static int UnderBackOrder { get { return -1; } }
        public static int DefaultOrder { get { return 0; } }
        public static float GlobalHIT { get { return 1000f; } }
        public static float GlobalDODGE { get { return 80f; } }
        public static float GlobalAnimSpeedRate { get { return 0.0006667f; } }
        public static float GlobalAfterAnimSpeedRate { get { return 0.001f; } }
        //private static Dictionary<float, WaitForSeconds> waitForSeconds = new();
        private static Dictionary<float, WaitForSecondsRealtime> waitForSecondsRealtime = new();
        //private static WaitForEndOfFrame waitForEndOfFrame = new();
        public static float DeathX1 { get => 2.4f; }
        public static float DeathX2 { get => 4f; }
        public static float DeathY1 { get => 2f; }
        public static float DeathY2 { get => 1.8f; }
        public static int DefaultFatigue { get => 10800; }
        public static string GetTranscendEffectName(int step = 0)
        {
            switch (step)
            {
                case 1:
                    return "fx_transcend_1";
                case 2:
                    return "fx_transcend_2";
                case 3:
                    return "fx_transcend_3";
            }
            return "fx_transcend_4";
        }
        public static int DefaultDragonID => 11000;
        #endregion
        #region Stat
        /// <summary> 방어력 감소 계수 </summary>
        public static readonly float DEF_WEIGHT = 100f;
        public static readonly float STAT_INF_ATK = 1.1f;              // 공격력
        public static readonly float STAT_INF_DEF = 1.7f;              // 방어력
        public static readonly float STAT_INF_HP = 0.17f;              // 체력
        public static readonly float STAT_INF_ADD_ATK_DMG = 10f;        // 추가 기본 대미지
        public static readonly float STAT_INF_RATIO_ATK_DMG = 20f;      // 기본 데미지 증가
        public static readonly float STAT_INF_ADD_CRI_DMG = 4.5f;       // 치명타 데미지
        public static readonly float STAT_INF_RATIO_CRI_DMG = 18f;      // 치명타 데미지
        public static readonly float STAT_INF_ELEMENT = 10.8f;          // 속성 대미지 계수
        public static readonly float STAT_INF_CRI_DMG_RESIS = 19.8f;    // 치명타 데미지 저항
        public static readonly float STAT_INF_ATK_DMG_RESIS = 22f;      // 기본 데미지 저항
        public static readonly float STAT_INF_ADD_ATKSPEED = 26f;       // 공격속도 증가
        public static readonly float STAT_INF_SKILL_LEVEL = 33f;        // 스킬레벨
        public static readonly float STAT_INF_ELEMENT_RESIS = 3.9f;          // 속성 대미지 저항
        #endregion
        #region AdvertisementKey
        public static readonly int AD_DAILY_KEY = 777007;
        public static readonly int AD_RAID_BOSS_KEY = 777013;//월드 보스 광고 플래그 키값
        #endregion
        #region AssetBundle
        public static readonly string BUNDLE_EXTENSION = ".unity3d";
        public static string GetBundleExtension(string names)
        {
            return SBFunc.StrBuilder(names, BUNDLE_EXTENSION);
        }
        #endregion
        #region Portrait
        public const string RAID_FRONT_SPRITE_PREFIX = "f_";
        public const string RAID_BOTTOM_SPRITE_PREFIX = "b_";
        public const string RAID_TOP_SPRITE_PREFIX = "t_";
        #endregion
        #region Chat
        public const int CHAT_TIME_UI_DELAY = 5;
        public const int USER_ACCESS_DAY_MAX = 30;
        #endregion
        public static IEnumerator GetWaitForSeconds(float time)
        {
            while (time > 0)
            {
                time -= SBGameManager.Instance.DTime;
                yield return GetWaitForEndOfFrame();
            }
        }
        public static WaitForSecondsRealtime GetWaitForSecondsRealtime(float time)
        {
            if (waitForSecondsRealtime == null)
                waitForSecondsRealtime = new();

            if (!waitForSecondsRealtime.TryGetValue(time, out WaitForSecondsRealtime wait))
                waitForSecondsRealtime.Add(time, wait = new WaitForSecondsRealtime(time));

            return wait;
        }
        public static IEnumerator GetWaitForEndOfFrame()
        {
            if (SBGameManager.Instance.IsFixedDelta)
            {
                yield return new WaitForFixedUpdate();
            }
            else
            {
                yield return new WaitForEndOfFrame();
            }
        }
        public static string GetDragonAnimTypeToName(eSpineAnimation eAnim, int ani_type = 1)
        {
            return eAnim switch
            {
                eSpineAnimation.A_CASTING => SBFunc.StrBuilder("acasting_ani", ani_type),
                eSpineAnimation.ATTACK => SBFunc.StrBuilder("atk_ani", ani_type),
                eSpineAnimation.CASTING => SBFunc.StrBuilder("scasting_ani", ani_type),
                eSpineAnimation.SKILL => SBFunc.StrBuilder("skill_ani", ani_type),
                eSpineAnimation.WALK => SBFunc.StrBuilder("move_ani", ani_type),
                eSpineAnimation.IDLE => SBFunc.StrBuilder("idle_ani", ani_type),
                eSpineAnimation.WIN => SBFunc.StrBuilder("win_ani", ani_type),
                eSpineAnimation.LOSE => SBFunc.StrBuilder("lose_ani", ani_type),
                eSpineAnimation.DEATH => SBFunc.StrBuilder("death_ani", ani_type),
                _ => ""
            };
        }
        public static eSpineAnimation GetDragonAnimNameToType(string strAnim)
        {
            return strAnim switch
            {
                "acasting_ani1" => eSpineAnimation.A_CASTING,
                "atk_ani1" => eSpineAnimation.ATTACK,
                "atk_ani2" => eSpineAnimation.ATTACK,
                "atk_ani3" => eSpineAnimation.ATTACK,
                "scasting_ani1" => eSpineAnimation.CASTING,
                "scasting_ani2" => eSpineAnimation.CASTING,
                "scasting_ani3" => eSpineAnimation.CASTING,
                "scasting_ani4" => eSpineAnimation.CASTING,
                "skill_ani1" => eSpineAnimation.SKILL,
                "skill_ani2" => eSpineAnimation.SKILL,
                "skill_ani3" => eSpineAnimation.SKILL,
                "skill_ani4" => eSpineAnimation.SKILL,
                "move_ani1" => eSpineAnimation.WALK,
                "idle_ani1" => eSpineAnimation.IDLE,
                "win_ani1" => eSpineAnimation.WIN,
                "lose_ani1" => eSpineAnimation.LOSE,
                "death_ani1" => eSpineAnimation.DEATH,
                _ => eSpineAnimation.NONE
            };
        }
        public static string GetMonsterAnimTypeToName(eSpineAnimation eAnim)
        {
            return eAnim switch
            {
                eSpineAnimation.A_CASTING => "monster_attack",
                eSpineAnimation.ATTACK => "monster_attack",
                eSpineAnimation.SKILL => "monster_skill1",
                eSpineAnimation.WALK => "monster_walk",
                eSpineAnimation.IDLE => "monster_idle",
                eSpineAnimation.WIN => "monster_win",
                eSpineAnimation.LOSE => "monster_lose",
                eSpineAnimation.CASTING => "monster_casting",
                eSpineAnimation.DEATH => "monster_death",
                eSpineAnimation.HIT => "monster_hit",
                _ => ""
            };
        }
        public static eSpineAnimation GetMonsterAnimNameToType(string strAnim)
        {
            return strAnim switch
            {
                "monster_attack" => eSpineAnimation.ATTACK,
                "monster_skill1" => eSpineAnimation.SKILL,
                "monster_walk" => eSpineAnimation.WALK,
                "monster_idle" => eSpineAnimation.IDLE,
                "monster_win" => eSpineAnimation.WIN,
                "monster_lose" => eSpineAnimation.LOSE,
                "monster_casting" => eSpineAnimation.CASTING,
                "monster_death" => eSpineAnimation.DEATH,
                "monster_hit" => eSpineAnimation.HIT,
                _ => eSpineAnimation.NONE
            };
        }
        public static string ResourceFolder(eResourcePath type)
        {
            switch (type)
            {
                case eResourcePath.StaticPrefabPath:
                    return "Static_Prefabs";
                case eResourcePath.StaticPrefabUIPath:
                    return "Static_Prefabs/UI_Prefab";

                case eResourcePath.PopupPrefabPath:
                    return "Popup";
                case eResourcePath.BuildingClonePath:
                    return "Prefabs/Building";
                case eResourcePath.BuildingUIClonePath:
                    return "Prefabs/BuildingUISpineObject";
                case eResourcePath.PrefabClonePath:
                    return "Prefabs/UIClone";
                case eResourcePath.DragonClonePath:
                    return "Prefabs/Dragon";
                case eResourcePath.UIDragonClonePath:
                    return "Prefabs/Dragon/UI";
                case eResourcePath.MonsterClonePath:
                    return "Prefabs/Monster";
                case eResourcePath.EffectPrefabPath:
                    return "Effect/Prefab";
                case eResourcePath.TownPrefabPath:
                    return "Prefabs/Town";
                case eResourcePath.TutorialPrefabPath:
                    return "Prefabs/Tutorial";
                case eResourcePath.PetClonePath:
                    return "Prefabs/Pet";
                case eResourcePath.SpecialBGPath:
                    return "Prefabs/special_bg_large";
                case eResourcePath.ScriptBGPath:
                    return "Prefabs/ScriptBG";
                case eResourcePath.ScriptObjectPath:
                    return "Prefabs/ScriptObject";

                case eResourcePath.ItemIconPath:
                    return "Item";
                case eResourcePath.CharIconPath:
                    return "Character";
                case eResourcePath.ElementIconPath:
                    return "UI/ElementIcon";
                case eResourcePath.ClassIconPath:
                    return "UI/ClassIcon";
                case eResourcePath.BuildingIconPath:
                    return "Images/building";
                case eResourcePath.BuildingCardIconPath:
                    return "Images/building";
                case eResourcePath.BuildingRecipeIconPath:
                    return "Images/building";
                case eResourcePath.SkillIconPath:
                    return "Images/skill";
                case eResourcePath.PartsIconPath:
                    return "Part";
                case eResourcePath.DragonGradeTagIconPath:
                    return "Images/character_grade_tag";
                case eResourcePath.QuestIconPath:
                    return "UI/Quest";
                case eResourcePath.BuffIconPath:
                    return "UI/BuffIcon";
                case eResourcePath.ProfileIconPath:
                    return "Images/profile_icon";
                case eResourcePath.PortraitFrameIconPath:
                    return "Images/portrait_frame";
                case eResourcePath.PetIconPath:
                    return "Images/pet";
                case eResourcePath.ArenaRankPath:
                    return "Images/rank";
                case eResourcePath.WorldSelectImgPath:
                    return "Images/Stage";
                case eResourcePath.StoreImagePath:
                    return "Images/store";
                case eResourcePath.ShopMenuIconPath:
                    return "Images/shop_menu_icon";
                case eResourcePath.GachaSpritePath:
                    return "Images/gacha";
                case eResourcePath.UISpritePath:
                    return "UI/CommonUI";

                case eResourcePath.EffectCustomPath:
                    return "ScriptableObjects/EffectCustom";

                case eResourcePath.DragonSkeletonDataPath:
                    return "Spine/Charactor/Dragon";

                case eResourcePath.BundleScenePath:
                    return "BundleScenes";

                case eResourcePath.EventAttendancePath:
                    return "Images/event_attendance";

                case eResourcePath.ProjectileSpritePath:
                    return "Projectile";
                case eResourcePath.SfxSoundPath:
                    return "SoundFx";
                case eResourcePath.BgmSoundPath:
                    return "SoundBGM";
                case eResourcePath.GuildResourcePath:
                    return "Images/Guild";

                default:
                    return "";
            }
        }
        public static string ResourcePath(eResourcePath type, string target)
        {
            var str = ResourceFolder(type);
            if (str == "")
                return "";

            return System.IO.Path.Combine(str, ResourceName(type, target));
        }
        public static string ResourceName(eResourcePath type, string target)
        {
            switch (type)
            {
                /** UIDragonClonePath는 UI드래곤 블러올 때 이름 앞에 ui가 붙어야되서 더하기만함 */
                case eResourcePath.UIDragonClonePath:
                    return SBFunc.StrBuilder("ui", target);
                default:
                    return target;
            }
        }
        public static string GetTypeExtensionStr(Type type)
        {
            if (type == TYPE_SPINEASSET)
                return ".asset";
            else if (type == TYPE_AUDIOCLIP)
                return "";
            else if (type == TYPE_SPRITE)
                return ".png";
            else
                return ".prefab";
        }
        public static string GetBundleName(this eResourcePath type)
        {
            return GetBundleExtension(type.GetBundleKey());
        }
        public static string GetBundleKey(this eResourcePath type)
        {
            return type switch
            {
                eResourcePath.EffectPrefabPath => "effect",
                eResourcePath.ProjectileSpritePath => "projectile",
                eResourcePath.CharIconPath => "character",
                eResourcePath.PartsIconPath => "part",
                eResourcePath.ItemIconPath => "item",
                eResourcePath.DragonSkeletonDataPath => "spine",
                eResourcePath.BundleScenePath => "bundlescenes",
                eResourcePath.EffectCustomPath => "scriptableobjects",
                eResourcePath.SfxSoundPath => "soundfx",
                _ => ""
            };
        }
        public static string ConvertToElementString(int e_type)
        {
            return e_type switch
            {
                1 => "fire",
                2 => "water",
                3 => "soil",
                4 => "wind",
                5 => "light",
                6 => "dark",
                _ => ""
            };
        }
        public static string GetRateBoardTypeToURL(eRateBoardType type)
        {
            string url = RateTableUrlData.Get(type);
            if (string.IsNullOrEmpty(url))
                return url;

            return NetworkManager.WEB_SERVER + url + "&lang=" + SBGameManager.Instance.ConvertStringLangBySystemLang(true);
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class TableManager : IManagerBase
    {
        protected static TableManager instance = null;
        public static TableManager Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new TableManager();
                    instance.InitTables();
                }
                return instance;
            }
        }
        private Dictionary<Type, ITableBase> tables = null;
        public void Initialize()
        {
            if(tables == null)
                return;

            var it = tables.GetEnumerator();
            while(it.MoveNext())
            {
                it.Current.Value.Init();
            }
        }
        public void Clear()
        {
            if (tables == null)
                return;

            var it = tables.GetEnumerator();
            while (it.MoveNext())
            {
                it.Current.Value.DataClear();
            }
        }
        public void Preload()
        {
            if (tables == null)
                return;

            var it = tables.GetEnumerator();
            while (it.MoveNext())
            {
                it.Current.Value.Preload();
            }
        }
        public bool AddTable(Type type, ITableBase target)
        {
            if (tables == null)
            {
                return false;
            }

            if (tables.ContainsKey(type)) //매니저 중복
            {
                return false;
            }

            tables.Add(type, target);
            return true;
        }
        public static T GetTable<T>() where T : class, ITableBase
        {
            if(Instance.tables == null)
            {
                return null;
            }

            var type = typeof(T);
            if(Instance.tables.ContainsKey(type) && Instance.tables[type] is T)
            {
                return Instance.tables[type] as T;
            }
            return null;
        }

        public void InitTables()
        {
            tables = new Dictionary<Type, ITableBase>();
#region 테이블 세팅
            AddTable(typeof(ServerOptionTable), new ServerOptionTable());
            AddTable(typeof(AccountTable), new AccountTable());
            AddTable(typeof(AreaExpansionTable), new AreaExpansionTable());
            AddTable(typeof(AreaLevelTable), new AreaLevelTable());
            AddTable(typeof(ProductTable), new ProductTable());
            AddTable(typeof(ProductAutoTable), new ProductAutoTable());
            AddTable(typeof(BuildingBaseTable), new BuildingBaseTable());
            AddTable(typeof(BuildingLevelTable), new BuildingLevelTable());
            AddTable(typeof(BuildingOpenTable), new BuildingOpenTable());
            AddTable(typeof(ItemBaseTable), new ItemBaseTable());
            AddTable(typeof(ItemGroupTable), new ItemGroupTable());
            AddTable(typeof(ItemGroupListTable), new ItemGroupListTable());
            AddTable(typeof(DefineResourceTable), new DefineResourceTable());
            AddTable(typeof(InventoryTable), new InventoryTable());
            AddTable(typeof(SlotCostTable), new SlotCostTable());
            AddTable(typeof(TravelTable), new TravelTable());
            AddTable(typeof(SubwayPlatformTable), new SubwayPlatformTable());
            AddTable(typeof(SubwayDeliveryTable), new SubwayDeliveryTable());
            AddTable(typeof(CharBaseTable), new CharBaseTable());
            AddTable(typeof(CharGradeTable), new CharGradeTable());
            AddTable(typeof(CharExpTable), new CharExpTable());
            AddTable(typeof(StatTable), new StatTable());
            AddTable(typeof(ElementTable), new ElementTable());
            AddTable(typeof(MonsterBaseTable), new MonsterBaseTable());
            AddTable(typeof(MonsterSpawnTable), new MonsterSpawnTable());
            AddTable(typeof(WorldTable), new WorldTable());
            AddTable(typeof(StageTable), new StageTable());
            AddTable(typeof(GachaShopTable), new GachaShopTable());
            AddTable(typeof(GachaListTable), new GachaListTable());
            AddTable(typeof(PartTable), new PartTable());
            AddTable(typeof(SubOptionTable), new SubOptionTable());
            AddTable(typeof(PartSetTable), new PartSetTable());
            AddTable(typeof(PartReinforceTable), new PartReinforceTable());
            AddTable(typeof(QuestTable), new QuestTable());
            AddTable(typeof(QuestTriggerTable), new QuestTriggerTable());
            AddTable(typeof(PartMergeBaseTable), new PartMergeBaseTable());
            AddTable(typeof(PartMergeReinforceBonusTable), new PartMergeReinforceBonusTable());
            AddTable(typeof(PartMergeEquipAmountBonusTable), new PartMergeEquipAmountBonusTable());
            AddTable(typeof(PartDecomposeTable), new PartDecomposeTable());
            AddTable(typeof(PartFusionTable), new PartFusionTable());
            AddTable(typeof(GameConfigTable), new GameConfigTable());
            AddTable(typeof(ArenaRankTable), new ArenaRankTable());
            AddTable(typeof(ArenaRankSeasonRewardTable), new ArenaRankSeasonRewardTable());
            AddTable(typeof(ArenaSeasonTable), new ArenaSeasonTable());
            AddTable(typeof(PetTable), new PetTable());
            AddTable(typeof(PetGradeTable), new PetGradeTable());
            AddTable(typeof(PetSkillNormalTable), new PetSkillNormalTable());
            AddTable(typeof(PetExpTable), new PetExpTable());
            AddTable(typeof(PetReinforceTable), new PetReinforceTable());
            AddTable(typeof(PetElementTable), new PetElementTable());
            AddTable(typeof(PetMergeBaseTable), new PetMergeBaseTable());
            AddTable(typeof(CharMergeBaseTable), new CharMergeBaseTable());
            AddTable(typeof(CharMergeListTable), new CharMergeListTable());
            AddTable(typeof(AreaLevelMissionTable), new AreaLevelMissionTable());
            AddTable(typeof(ExchangeTable), new ExchangeTable());
            AddTable(typeof(ExchangeGroupTable), new ExchangeGroupTable());
            AddTable(typeof(PetStatTable), new PetStatTable());
            AddTable(typeof(StatTypeTable), new StatTypeTable());
            AddTable(typeof(SoundResourceTable), new SoundResourceTable());
            AddTable(typeof(PetDecomposeTable), new PetDecomposeTable());
            AddTable(typeof(DailyStageTable), new DailyStageTable());
            AddTable(typeof(GachaGroup), new GachaGroup());
            AddTable(typeof(GachaMenu), new GachaMenu());
            AddTable(typeof(GachaType), new GachaType());
            AddTable(typeof(GachaRate), new GachaRate());
            AddTable(typeof(RateTableUrlTable), new RateTableUrlTable());
            AddTable(typeof(ShopMenuTable), new ShopMenuTable());
            AddTable(typeof(ShopGoodsTable), new ShopGoodsTable());
            AddTable(typeof(ShopSubscriptionTable), new ShopSubscriptionTable());
            AddTable(typeof(PersonalGoodsTable), new PersonalGoodsTable());
            AddTable(typeof(ShopBannerTable), new ShopBannerTable());
            AddTable(typeof(PostRewardTable), new PostRewardTable());
            AddTable(typeof(ShopSKUTable), new ShopSKUTable());
            AddTable(typeof(DailyRewardTable), new DailyRewardTable());
            AddTable(typeof(AdvertisementTable), new AdvertisementTable());
            AddTable(typeof(AchievementBaseTable), new AchievementBaseTable());
            AddTable(typeof(CollectionGroupTable), new CollectionGroupTable());
            AddTable(typeof(CollectionTable), new CollectionTable());
            AddTable(typeof(ShopRandomTable), new ShopRandomTable());
            AddTable(typeof(ScriptTriggerTable), new ScriptTriggerTable());
            AddTable(typeof(ScriptGroupTable), new ScriptGroupTable());
            AddTable(typeof(ScriptObjectTable), new ScriptObjectTable());
            AddTable(typeof(SkillCharTable), new SkillCharTable());
            AddTable(typeof(SkillSummonTable), new SkillSummonTable());
            AddTable(typeof(SkillEffectTable), new SkillEffectTable());
            AddTable(typeof(SkillLevelTable), new SkillLevelTable());
            AddTable(typeof(SkillLevelGroupTable), new SkillLevelGroupTable());
            AddTable(typeof(SkillResourceTable), new SkillResourceTable());
            AddTable(typeof(EventBannerTable), new EventBannerTable());
            AddTable(typeof(PassInfoTable), new PassInfoTable());
            AddTable(typeof(PassItemTable), new PassItemTable());
            AddTable(typeof(MagicShowcaseTable), new MagicShowcaseTable());
            AddTable(typeof(RecipeBaseTable), new RecipeBaseTable());
            AddTable(typeof(RecipeMaterialTable), new RecipeMaterialTable());
            AddTable(typeof(ReservMailTable), new ReservMailTable());
            AddTable(typeof(EventScheduleTable), new EventScheduleTable());
            AddTable(typeof(EventRewardTable), new EventRewardTable());
            AddTable(typeof(EventRankRewardTable), new EventRankRewardTable());
            AddTable(typeof(EventAttendanceResourceTable), new EventAttendanceResourceTable());
            AddTable(typeof(DiceBoardTable), new DiceBoardTable());
            AddTable(typeof(LanguageTable), new LanguageTable());
            AddTable(typeof(CharTranscendenceTable), new CharTranscendenceTable());
            AddTable(typeof(SkillPassiveTable), new SkillPassiveTable());
            AddTable(typeof(SkillPassiveGroupTable), new SkillPassiveGroupTable());
            AddTable(typeof(SkillPassiveRateTable), new SkillPassiveRateTable());
            AddTable(typeof(WorldBossLevelTable), new WorldBossLevelTable());
            AddTable(typeof(WorldBossRankRewardTable), new WorldBossRankRewardTable());
            AddTable(typeof(WorldBossPartTable), new WorldBossPartTable());
            AddTable(typeof(TutorialScriptTable), new TutorialScriptTable());
            AddTable(typeof(TutorialTriggerTable), new TutorialTriggerTable());
            AddTable(typeof(MineBoosterTable), new MineBoosterTable());
            AddTable(typeof(MineDrillTable), new MineDrillTable());
            AddTable(typeof(GuildExpTable), new GuildExpTable());
            AddTable(typeof(GuildDonationTable), new GuildDonationTable());
            AddTable(typeof(GuildResourceTable), new GuildResourceTable());
            AddTable(typeof(GuildRankRewardTable), new GuildRankRewardTable());
            AddTable(typeof(RestrictedAreaTable), new RestrictedAreaTable());
            //나중에 테이블 생성용 메모
            //AddTable(typeof(StringTable), new MailStringTable());
            //AddTable(typeof(MailStringTable), new MailStringTable());
            //AddTable(typeof(ScriptStringTable), new ScriptStringTable());
            //
            #endregion
        }
        public void Update(float dt) {}
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public partial class PopupManager
    {
        static Dictionary<Type, string> PopupPathInfo = new Dictionary<Type, string>()
        {
            { typeof(AccelerationMainPopup), "AccelerationMainPopup" },
            { typeof(AdventureReadyPopup), "AdventureReadyPopup" },
            { typeof(AdvRemoveBuyGuidePopup), "AdvRemoveBuyGuidePopup" },
            { typeof(AnnouncePopup), "AnnouncementPopup" },
            { typeof(ArenaInfoPopup), "ArenaInfoPopup" },
            { typeof(ArenaMatchListRefreshPopup), "ArenaMatchListRefreshPopup" },
            { typeof(ArenaPointShopPopup), "ArenaPointShopPopup" },
            { typeof(ArenaRankChangePopup), "ArenaRankChangePopup" },
            { typeof(ArenaSeasonOpeningPopup), "ArenaSeasonOpeningPopup" },
            { typeof(ArenaTicketRechargePopup), "ArenaTicketRechargePopup" },
            { typeof(AttendancePopup), "AttendancePopup" },
            { typeof(AutoAdventurePopup), "AutoAdventurePopup" },
            { typeof(AutoAdventureWithTicketPopup), "AutoAdventureWithTicketPopup" },
            { typeof(ArenaDamageStatisticPopup), "battleDamageaStatisticPopup" },
            { typeof(BattlePassPopup), "BattlePassPopup" },
            { typeof(BatteryBuildingUpgradePopup), "BuildingBatteryUpgradePopup" },
            { typeof(BuildingCompletePopup), "BuildingCompletePopup" },
            { typeof(BuildingConstructListPopup), "BuildingConstructListPopup" },
            { typeof(BuildingConstructPopup), "BuildingConstructPopup" },
            { typeof(BuildingTypeDozer), "BuildingDozerUpgradePopup" },
            { typeof(BuildingUpgradePopup), "BuildingNormalUpgradePopup" },
            { typeof(BuildingMineUpgradePopup), "BuildingMineUpgradePopup" },
            { typeof(ChampionBattleDragonSelectPopup), "ChampionBattleDragonSelectPopup" },
            { typeof(ChampionWinnerPopup_old), "ChampionWinnerPopup_old" },
            { typeof(ChampionWinnerPopup), "ChampionWinnerPopup" },
            { typeof(ChampionDragonDetailPopup), "ChampionDragonDetailPopup" },
            { typeof(CoinBetPopup), "CoinBetPopup" },
            
            { typeof(ChampionBetResultPopup), "ChampionBetResultPopup" },
            { typeof(MatchInfoPopup), "MatchInfoPopup" },
            { typeof(ChattingBlockAlarmPopup), "ChattingBlockPopup" },
            { typeof(ChattingMacroRegistPopup), "ChattingMacroRegistPopup" },
            { typeof(ChattingPopup), "ChattingPopup" },
            { typeof(ChattingProfilePopup), "ChattingProfilePopup" },
            { typeof(CollectionAchievementPopup), "CollectionAchievementPopup" },
            { typeof(ConditionalBuyPopup), "ConditionalBuyPopup" },
            { typeof(DailyReadyPopup), "DailyReadyPopup" },
            { typeof(DecomposeResultPopup), "DecomposeResultPopup" },
            { typeof(DragonCompoundConstraintPopup), "DragonCompoundConstraintPopup" },
            { typeof(DragonCompoundResultPopup), "DragonCompoundResultPopup" },
            { typeof(PartFilterPopup), "DragonDecomposeFilterPopup" },
            { typeof(DragonLevelUpPopup), "DragonLevelUpPopup" },
            { typeof(DragonListFilterPopup), "DragonListFilterPopup" },
            { typeof(DragonManagePopup), "DragonManagePopup" },
            { typeof(DragonPartAutoCompoundPopup), "DragonPartAutoCompoundPopup" },
            { typeof(DragonTeamFilterPopup), "DragonTeamFilterPopup" },
            { typeof(DungeonSelectPopup), "DungeonSelectPopup" },
            { typeof(ElemBuffInfoPopup), "ElemBuffInfoPopup" },
            { typeof(EventAttendancePopup), "EventAttendancePopup" },
            { typeof(FriendPointShopPopup), "FriendPointShopPopup" },
            { typeof(GachaMileageShopPopup), "GachaMileageShopPopup" },
            { typeof(GachaTablePopup), "GachaTablePopup" },
            { typeof(HolderPassPopup), "HolderPassPopup" },
            { typeof(IntroPopup), "IntroPopup" },
            { typeof(InventoryLevelUpPopup), "InventoryLevelUpPopup" },
            { typeof(InventoryPopup), "InventoryPopup" },
            { typeof(ItemInfoPopup), "ItemInfoPopup" },
            { typeof(ItemToolTip), "ItemToolTip" },
            { typeof(SimpleToolTip), "SimpleToolTip" },
            { typeof(LandMarkPopup), "LandMarkPopup" },
            { typeof(MagicShowcaseBlockConstructPopup), "MagicShowcaseBlockConstructPopup" },
            { typeof(MagicShowcaseBlockInputPopup), "MagicShowcaseBlockInputPopup" },
            { typeof(MagicShowcaseEnterPopup), "MagicShowcaseEnterPopup" },
            { typeof(MagicShowcasePopup), "MagicShowcasePopup" },
            { typeof(MiningDrillRepairInsufficiencyPopup), "MiningDrillRepairInsufficiencyPopup" },
            { typeof(MiningDrillRepairPopup), "MiningDrillRepairPopup" },
            { typeof(MiningUseBoosterItemPopup), "MiningUseBoosterItemPopup" },
            { typeof(MiningMainPopup), "MiningMainPopup" },
            { typeof(MissionPopup), "MissionPopup" },
            { typeof(NameTagToolTip), "NameTagToolTip" },
            { typeof(OptionSelectPopup), "OptionSelectPopup" },
            { typeof(PartCompoundResultPopup), "PartCompoundResultPopup" },
            { typeof(PartDestroyPopup), "PartDestroyPopup" },
            { typeof(PausePopup), "PausePopup" },
            { typeof(PetCompoundResultPopup), "PetCompoundResultPopup" },
            { typeof(PetDetailInfoPopup), "PetDetailInfoPopup" },
            { typeof(PetLevelUpConstraintPopup), "PetLevelUpConstraintPopup" },
            { typeof(PetLevelUpPopup), "PetLevelUpPopup" },
            { typeof(PetListFilterPopup), "PetListFilterPopup" },
            { typeof(PortraitPopup), "PortraitBoxPopup" },
            { typeof(PostListPopup), "PostListPopup" },
            { typeof(PricePopup), "PricePopup" },
            { typeof(ProductManagePopup), "ProductManagePopup" },
            { typeof(ProductManageProduceOptionPopup), "ProductManageProduceOptionPopup" },
            { typeof(ProductPopup), "ProductPopup" },
            { typeof(QuestPopup), "QuestInfoPopup" },
            { typeof(ReviewPopup), "ReviewPopup" },
            { typeof(RewardListShowPopup), "RewardListShowPopup" },
            { typeof(RewardResultPopup), "DecomposeResultPopup" },
            { typeof(SettingPopup), "SettingPopup" },
            { typeof(ShopBannerPopup), "ShopBannerPopup" },
            { typeof(ShopBuyPopup), "ShopBuyPopup" },
            { typeof(ShopPopup), "ShopPopup" },
            { typeof(StageInfoPopup), "StageInfoPopup" },
            { typeof(SystemPopup), "SystemPopup" },
            { typeof(SystemRewardPopup), "SystemRewardPopup" },
            { typeof(ToolTip), "ToolTip" },
            { typeof(TownManagePopup), "TownManagePopup" },
            { typeof(TownUpgradePopup), "TownUpgradePopup" },
            { typeof(StatisticInfoPopup),"StatisticsInfoPopup" },
            { typeof(AdventureStatisticsPopup),"AdventureStatisticsPopup" },
            { typeof(ArtBlockPopup), "ArtBlockPopup" },
            { typeof(GemDungeonPopup), "GemDungeonPopup" },
            { typeof(GemDungeonBoostPopup), "GemDungeonBoostPopup" },
            { typeof(GemDungeonHealUsePopup),"GemDungeonHealUsePopup" },
            { typeof(GemDungeonTeamRecommendPopup), "GemDungeonTeamRecommendPopup" },
            { typeof(ProductsBuyNowPopup), "ProductsBuyNowPopup" },
            { typeof(LevelPassPopup), "LevelPassPopup" },
            { typeof(ItemMakePerfectPopup), "ItemMakePerfectPopup" },
            { typeof(ItemMakePopup), "ItemMakePopup" },
            { typeof(PassiveSkillPopup), "PassiveSkillPopup" },
            { typeof(PassiveSkillResultPopup), "PassiveSkillResultPopup" },
            { typeof(PassiveTablePopup), "PassiveTablePopup" },
            { typeof(TranscendenceResultPopup),"TranscendenceResultPopup" },
            { typeof(WorldBossRankingPopup),"WorldBossRankingPopup" },
            { typeof(DAppWebBrowserPopup),"DAppWebBrowserPopup" },
            { typeof(ArenaStatisticPopup),"ArenaStatisticsPopup" },
            { typeof(ChampionBattleStatisticPopup),"ChampionBattleStatisticsPopup" },
            { typeof(WebViewPopup), "WebViewPopup" },
            { typeof(WorldBossInfoPopup), "WorldBossInfoPopup" },
            { typeof(HelpDescriptionPopup), "HelpDescriptionPopup" },
            { typeof(CAPTCHAPopup), "CAPTCHAPopup" },
            { typeof(RestrictedAreaReadyPopup), "RestrictedAreaReadyPopup" },
            { typeof(AccelerationImmediatelyPopup), "AccelerationImmediatelyPopup" },
            { typeof(ChampionSurpportPopup), "ChampionSurpportPopup" },
            { typeof(ChampionSurpportServerPopup), "ChampionSurpportServerPopup" },
            { typeof(ChampionSurpportCheckPopup), "ChampionSurpportCheckPopup" },
            { typeof(ShopGroupBanner), "ShopGroupBanner" },
            { typeof(PetSubOptionRerollPopup), "PetSubOptionRerollPopup" },
            { typeof(PetOptionRerollConstraintPopup), "PetOptionRerollConstraintPopup" },
            //Guild
            { typeof(GuildSelectPopup), "GuildSelectPopup" },
            { typeof(GuildStartPopup), "GuildStartPopup" },
            { typeof(GuildRankingRewardPopup), "GuildRankingRewardPopup" },
            { typeof(GuildInfoPopup), "GuildInfoPopup" },
            { typeof(GuildLvRewardInfoPopup), "GuildLvRewardInfoPopup" },
            { typeof(GuildDonatePopup), "GuildDonatePopup" },
            { typeof(GuildManagePopup), "GuildManagePopup" },
            { typeof(GuildOperatorPermissionPopup), "GuildOperatorPermissionPopup" },
            { typeof(GuildNameChangePopup), "GuildNameChangePopup" },
            { typeof(GuildEmblemChangePopup), "GuildEmblemChangePopup" },
            { typeof(GuildMarketingChangePopup), "GuildMarketingChangePopup" },
            { typeof(GuildApplyListPopup), "GuildApplyListPopup" },
            { typeof(GuildDestroyPopup), "GuildDestroyPopup" },
            //{ typeof(GuildUserRankingRewardPopup), "GuildUserRankingRewardPopup" }, //GuildRankingRewardPopup 에 유저랭크도 통합
            { typeof(GuildDestroyInfoPopup), "GuildDestroyInfoPopup" },
            { typeof(GuildShopPopup), "GuildShopPopup" },
            { typeof(GuildRecommendFilterPopup), "GuildRecommendFilterPopup" },
            { typeof(GuildMakePopup),"GuildMakePopup" },
            { typeof(GuildJoinPopup),"GuildJoinPopup" },
            { typeof(GuildWidthdrawPopup), "GuildWidthdrawPopup" },
            { typeof(GuildWidthdrawCheckPopup), "GuildWidthdrawCheckPopup" },
            //Guild End

            //TutorialPopup
            { typeof(ProductTutorialPopup),"ProductTutorialPopup" },
            { typeof(ProductManageTutorialPopup),"ProductManageTutorialPopup" },
            { typeof(AccelerationTutorialPopup), "AccelerationTutorialPopup" },
            //Tutorial End

            //Event
            { typeof(OpenEventRankingPopup),"OpenEventRankingPopup" },
            { typeof(UnionRaidEventPopup),"UnionRaidEventPopup" },
            { typeof(ChampionEventRankingPopup), "ChampionEventRankingPopup" },
            { typeof(LunaServerEventPopup), "LunaServerEventPopup" },
            { typeof(RestrictedAreaEventPopup),"RestrictedAreaEventPopup" },

            //{ typeof(DiceEventPopup), "DiceEventPopup" },               //구 크리스마스 이벤트
            //{ typeof(DiceEventHelpPopup),"DiceEventHelpPopup" },        //이벤트 도움말 팝업
            //{ typeof(DiceEventGiftOpenPopup),"DiceEventGiftOpenPopup" },//이벤트 상자오픈 팝업

            { typeof(DiceEventPopup), "FallDiceEventPopup" },               //현 다이스 이벤트
            { typeof(DiceEventHelpPopup),"FallDiceEventHelpPopup" },        //이벤트 도움말 팝업
            { typeof(DiceEventGiftOpenPopup),"FallDiceEventGiftOpenPopup" },//이벤트 상자오픈 팝업

            { typeof(LuckyBagEventPopup),"LuckyBagEventPopup" },          //구 구정이벤트 / 현 복주머니이벤트
            { typeof(LuckyBagEventHelpPopup),"LuckyBagEventHelpPopup" },
            
            { typeof(HotTimeEventDescPopup),"HotTimeEventDescPopup" },          //핫타임 설명 팝업
            //EventEnd


            //tool
            { typeof(SpineToolEditRangePopup), "SpineToolEditRangePopup" },
            //toolEnd

            #if UNITY_EDITOR
            { typeof(SimulatorDragonEditPopup), "SimulatorDragonEditPopup" },
            { typeof(SimulatorPausePopup), "SimulatorPausePopup" },
            { typeof(SimulatorPreset), "SimulatorPreset" },
            { typeof(SimulatorPresetSavePopup), "SimulatorPresetSave" },
            #endif//for Simulator in unity
        };
    }
}

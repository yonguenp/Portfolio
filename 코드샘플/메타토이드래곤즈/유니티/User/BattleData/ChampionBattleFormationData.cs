using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SandboxNetwork
{
    public class ChampionBattleFormationData
    {
        string ATK_KEY { get { return ((int)ParticipantData.eTournamentTeamType.ATTACK).ToString(); } }
        string DEF_KEY { get { return ((int)ParticipantData.eTournamentTeamType.DEFFENCE).ToString(); } }
        string HID_KEY { get { return ((int)ParticipantData.eTournamentTeamType.HIDDEN).ToString(); } }
        public Dictionary<ParticipantData.eTournamentTeamType, ChampionBattleLine> TeamFormation { get; private set; } = new Dictionary<ParticipantData.eTournamentTeamType, ChampionBattleLine>();

        public ChampionBattleFormationData()
        {
            AllClear();
        }

        public void SetJsonData(JToken jsonToken)//공격 / 방어 세팅 서버에서 어떻게 줄지 물어보기
        {
            AllClear();
            if (jsonToken == null || jsonToken.Type != JTokenType.Object)
                return;

            JObject jsonData = (JObject)jsonToken;

            if(jsonData != null && jsonData.ContainsKey(ATK_KEY) && jsonData[ATK_KEY] != null && jsonData[ATK_KEY].Type == JTokenType.Array)
            {
                JArray jsonDeck = (JArray)jsonData[ATK_KEY];
                TeamFormation[ParticipantData.eTournamentTeamType.ATTACK] = new ChampionBattleLine(ParticipantData.eTournamentTeamType.ATTACK, jsonDeck);
            }

            if (jsonData != null && jsonData.ContainsKey(DEF_KEY) && jsonData[DEF_KEY] != null && jsonData[ATK_KEY].Type == JTokenType.Array)
            {
                JArray jsonDeck = (JArray)jsonData[DEF_KEY];
                TeamFormation[ParticipantData.eTournamentTeamType.DEFFENCE] = new ChampionBattleLine(ParticipantData.eTournamentTeamType.DEFFENCE, jsonDeck);
            }
            
            if (jsonData != null && jsonData.ContainsKey(HID_KEY) && jsonData[HID_KEY] != null && jsonData[HID_KEY].Type == JTokenType.Array)
            {
                JArray jsonDeck = (JArray)jsonData[HID_KEY];
                TeamFormation[ParticipantData.eTournamentTeamType.HIDDEN] = new ChampionBattleLine(ParticipantData.eTournamentTeamType.HIDDEN, jsonDeck);
            }
        }

        public void AllClear()
        {
            TeamFormation.Clear();
            TeamFormation.Add(ParticipantData.eTournamentTeamType.ATTACK, new ChampionBattleLine(ParticipantData.eTournamentTeamType.ATTACK));
            TeamFormation.Add(ParticipantData.eTournamentTeamType.DEFFENCE, new ChampionBattleLine(ParticipantData.eTournamentTeamType.DEFFENCE));
            TeamFormation.Add(ParticipantData.eTournamentTeamType.HIDDEN, new ChampionBattleLine(ParticipantData.eTournamentTeamType.HIDDEN));
        }

        public ChampionBattleLine GetChampionBattleFormation(ParticipantData.eTournamentTeamType type)
        {
            if (TeamFormation == null || !TeamFormation.ContainsKey(type))
                return null;

            return TeamFormation[type];
        }

        public ParticipantData.eTournamentTeamType GetForamtionType(int tag)
        {
            foreach(var formation in TeamFormation)
            {
                if(formation.Value.IsContainDragon(tag))
                {
                    return formation.Key;
                }
            }

            return ParticipantData.eTournamentTeamType.NONE;
        }
    }
}
using Newtonsoft.Json.Linq;
using SandboxNetwork;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
#if DEBUG
    public class SimulatorLoger
    {
        protected static SimulatorLoger instance = null;
        public static SimulatorLoger Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SimulatorLoger();
                }
                return instance;
            }
        }

        public enum LogType
        {
            DAMAGE,
            TAKEN_DAMAGE,
            TAKEN_ORIGIN_DAMAGE
        }

        private Dictionary<int, Dictionary<LogType, float>> dragonDamageLog = new Dictionary<int, Dictionary<LogType, float>>();
        private int[] dragons = new int[6];
        private List<PresetDragon> presetDragons = new List<PresetDragon>();
        int totalRound = -1;
        int curRound = -1;
        int world = -1;
        int stage = -1;
        int wave = -1;
        bool worldSelect = false;
        bool autoPlay = false;
        JObject battleInfo = null;
        bool isFirstPveSceneinit = true;

        static public bool FirstInit { get { return instance.isFirstPveSceneinit; } set { instance.isFirstPveSceneinit = value; } }
        static public int World { get { return instance.world; } set { instance.world = value; } }
        static public int Stage { get { return instance.stage; } set { instance.stage = value; } }
        static public int Wave { get { return instance.wave; } set { instance.wave = value; } }
        static public bool WorldSelect { get { return instance.worldSelect; } set { instance.worldSelect = value; } }
        static public int[] Dragons { get { return instance.dragons; } set { instance.dragons = value; } }
        static public List<PresetDragon> PresetDragons { get { return instance.presetDragons; } set { instance.presetDragons = value; } }
        static public bool AutoPlay { get { return instance.autoPlay; } set { instance.autoPlay = value; } }
        static public JObject BattleInfo { get { return instance.battleInfo; } set { instance.battleInfo = value; } }
        static public int TotalRound { get { return instance.totalRound; } set { instance.totalRound = value; } }
        static public int CurRound { get { return instance.curRound; } set { instance.curRound = value; } }

        static public void MakeInstance()
        {
            if (instance == null)
            {
                instance = new SimulatorLoger();
            }
        }

        static public void ClearLog()
        {
            Instance.dragonDamageLog.Clear();
        }
        static public void UpdateLog(int tag, LogType type, float value)
        {
            if (!Instance.dragonDamageLog.ContainsKey(tag))
                Instance.dragonDamageLog[tag] = new Dictionary<LogType, float>();

            if (Instance.dragonDamageLog[tag].ContainsKey(type))
                Instance.dragonDamageLog[tag][type] += value;
            else
                Instance.dragonDamageLog[tag][type] = value;
        }

        static public float GetLogValue(int tag, LogType type)
        {
            if (!Instance.dragonDamageLog.ContainsKey(tag))
                return 0.0f;
            if (!Instance.dragonDamageLog[tag].ContainsKey(type))
                return 0.0f;

            return Instance.dragonDamageLog[tag][type];
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void InitPlayMode()
        {
            instance = null;
        }
    }
#endif
}

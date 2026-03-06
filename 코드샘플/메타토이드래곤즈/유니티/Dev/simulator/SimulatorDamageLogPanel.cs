using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if DEBUG

namespace SandboxNetwork
{
    public class SimulatorDamageLogPanel : MonoBehaviour
    {
        [SerializeField]
        List<GameObject> dragonTextList = new List<GameObject>();

        [SerializeField] GameObject autoModeNode = null;
        public void SetDamageLog()
        {
            initLog();
            RefreshDamageLog();
        }

        void RefreshDamageLog()
        {
            for (int i = 0; i < dragonTextList.Count; i++)
            {
                var go = dragonTextList[i];
                if (go == null)
                    continue;

                var dragonTag = SimulatorLoger.Dragons[i];
                if (dragonTag > 0)
                {
                    var dmg = SBFunc.GetChildrensByName(go.transform, "dmg").GetComponent<Text>();
                    dmg.text = SimulatorLoger.GetLogValue(dragonTag, SimulatorLoger.LogType.DAMAGE).ToString();

                    var takendmg = SBFunc.GetChildrensByName(go.transform, "takendmg").GetComponent<Text>();
                    takendmg.text = SimulatorLoger.GetLogValue(dragonTag, SimulatorLoger.LogType.TAKEN_DAMAGE).ToString();

                    var takenrealdmg = SBFunc.GetChildrensByName(go.transform, "takenrealdmg").GetComponent<Text>();
                    takenrealdmg.text = SimulatorLoger.GetLogValue(dragonTag, SimulatorLoger.LogType.TAKEN_ORIGIN_DAMAGE).ToString();
                }
            }
        }

        void initLog()
        {
            if (dragonTextList == null || dragonTextList.Count <= 0)
                return;

            bool isSimulatorAuto = SimulatorLoger.AutoPlay;
            autoModeNode.SetActive(isSimulatorAuto);
            if (isSimulatorAuto)
                SetCurrentAutoLabel();

            for (int i = 0; i < dragonTextList.Count; i++)
            {
                var go = dragonTextList[i];
                if (go == null)
                    continue;

                go.SetActive(!(SimulatorLoger.Dragons[i] == 0));

                var dmg = SBFunc.GetChildrensByName(go.transform, "dmg").GetComponent<Text>();
                dmg.text = "-";

                var takendmg = SBFunc.GetChildrensByName(go.transform, "takendmg").GetComponent<Text>();
                takendmg.text = "-";

                var takenrealdmg = SBFunc.GetChildrensByName(go.transform, "takenrealdmg").GetComponent<Text>();
                takenrealdmg.text = "-";
            }
        }

        void SetCurrentAutoLabel()
        {
            var dmg = SBFunc.GetChildrensByName(autoModeNode.transform, "dmg").GetComponent<Text>();
            if (SimulatorLoger.TotalRound > 0)
            {
                dmg.text = SimulatorLoger.TotalRound.ToString();
            }
            else
            {
                dmg.text = "-";
            }

            var takenrealdmg = SBFunc.GetChildrensByName(autoModeNode.transform, "takenrealdmg").GetComponent<Text>();
            if (SimulatorLoger.CurRound > 0)
            {
                takenrealdmg.text = SimulatorLoger.CurRound.ToString();
            }
            else
            {
                takenrealdmg.text = "-";
            }
        }
    }
}
#endif

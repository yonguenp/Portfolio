using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


#if DEBUG
namespace SandboxNetwork
{
    public class SimulatorBattleStartScene : MonoBehaviour
    {
        [SerializeField] Button pvpButton = null;
        [SerializeField] Button pveButton = null;

        private void Start()
        {
            StartCoroutine(DataLoad());

            init();
        }

        IEnumerator DataLoad()
        {
            yield return SBGameManager.Instance.GameDataSyncAndLoad(null);
            
            PopupTopUIRefreshEvent.Hide();
            UIManager.Instance.InitUI(eUIType.None);
            UIManager.Instance.RefreshUI(eUIType.None);
            
            yield return new WaitForSeconds(1.0f);
        }
        public void onClickChangePveScene()
        {
            LoadingManager.Instance.EffectiveSceneLoad("pve_simulator", eSceneEffectType.CloudAnimation);
        }

        void init()
        {
            if (pvpButton != null)
                SetButtonInteraction(pvpButton, true);
            if(pveButton != null)
                SetButtonInteraction(pveButton, true);
        }

        void SetButtonInteraction(Button targetButton, bool isInteract)
        {
            if (targetButton != null)
                targetButton.interactable = isInteract;
        }
    }
}
#endif

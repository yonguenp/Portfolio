using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if DEBUG

namespace SandboxNetwork
{
    public class SimulatorPausePopup : Popup<PopupData>
    {
        [SerializeField]
        private GameObject resumeBtn;
        [SerializeField]
        private GameObject restartBtn;
        [SerializeField]
        private GameObject exitBtn;

        private VoidDelegate resumeCallback = null;
        private VoidDelegate restartCallback = null;
        private VoidDelegate exitCallback = null;

        public void SetCallBack(VoidDelegate resumeCallback = null, VoidDelegate restartCallback = null, VoidDelegate exitCallback = null) //callback 에 null 넣으면 ok 아닌 버튼들은 사라집니다
        {
            if (resumeBtn != null)
            {
                this.resumeCallback = resumeCallback;
                resumeBtn.SetActive(true);
            }
            if (restartBtn != null)
            {
                this.restartCallback = restartCallback;
                restartBtn.SetActive(true);
            }
            if (exitBtn != null)
            {
                this.exitCallback = exitCallback;
                exitBtn.SetActive(true);
            }
        }
        public void OnClickResume()
        {
            if (resumeCallback == null)
            {
                ClosePopup();
                return;
            }
            resumeCallback();
            ClosePopup();
        }
        public void OnClickRestart()
        {
            if (restartCallback == null)
            {
                ClosePopup();
                return;
            }
            restartCallback();
            ClosePopup();
        }

        public void OnClickExit()
        {
            if (exitCallback == null)
            {
                ClosePopup();
                return;
            }
            exitCallback();
            ClosePopup();
        }

        public override void InitUI() { }

    }
}
#endif

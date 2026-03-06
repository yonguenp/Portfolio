using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    [System.Serializable]
    public struct SubLayerInfoData
    {
        public SubLayer targetLayer;
        public Button targetButton;
        public bool isShowInfoIcon;
        public bool isShowDescText;
    }

    public class SubLayer : MonoBehaviour, ISBLayer
    {
        public virtual void ForceUpdate() { }

        public virtual void Init() { }
        public virtual bool backBtnCall() { return false; } //백 버튼 콜백이 없으면 false 를 출력
    }
}

using Newtonsoft.Json.Linq;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    /// <summary>
    /// 블록 제작 팝업 결과 상태
    /// </summary>
    public class MagicShowcaseBlockConstructResultUI : MonoBehaviour
    {
        [Header("success")]
        [SerializeField] GameObject successNode = null;
        [SerializeField] ItemFrame successItem = null;
        [SerializeField] SkeletonGraphic successSpine = null;

        [Space(10)]
        [Header("fail")]
        [SerializeField] GameObject failNode = null;
        [SerializeField] ItemFrame failItem = null;
        [SerializeField] SkeletonGraphic failSpine = null;

        public void SetVisible(bool _isVisible)
        {
            gameObject.SetActive(_isVisible);
        }

        public void InitUI(JObject _serverData)//서버요청 데이터
        {
            if(_serverData.ContainsKey("result") && SBFunc.IsJTokenType(_serverData["result"], JTokenType.Object))
            {
                var resultData = (JObject)_serverData["result"];

                if (resultData.ContainsKey("success"))
                {
                    var arr = resultData["success"];
                    SetItem(true, arr);
                }

                if (resultData.ContainsKey("fail"))
                {
                    var arr = resultData["fail"];
                    SetItem(false, arr);
                }
            }

            var popup = PopupManager.GetPopup<MagicShowcasePopup>();
            if (popup != null)
                popup.ForceUpdate();//일단 임시
        }

        void SetItem(bool _isSuccess, JToken _data)//무조건 하나만 들어온다고 픽스
        {
            var hasValue = SBFunc.IsJTokenType(_data , JTokenType.Array);
            SetVisibleNode(_isSuccess, hasValue);

            if (!hasValue)
                return;

            var valueData = (JArray)_data[0];
            if (valueData.Count != 3)
                return;

            var itemNo = valueData[1].Value<int>();
            var itemCount = valueData[2].Value<int>();

            if (_isSuccess)
            {
                successItem.SetFrameItemInfo(itemNo, itemCount);
                successSpine.AnimationState.SetAnimation(0,"success",false);
            }
            else
            {
                failItem.SetFrameItemInfo(itemNo, itemCount);
                failSpine.AnimationState.SetAnimation(0, "failed", false);
            }
        }

        void SetVisibleNode(bool _isSuccess , bool _isVisible)
        {
            if(_isSuccess)
                successNode.SetActive(_isVisible);
            else
                failNode.SetActive(_isVisible);
        }

        public void OnClickCompleteButton()//일단은 초기 팝업UI로 보냄
        {
            MagicShowcaseBlockConstructEvent.InitUI();
        }
    }
}

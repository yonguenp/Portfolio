using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    /// <summary>
    /// 이름 하나 정도 들어가는 간단한 말풍선 - ex)콜렉션에서 드래곤 초상화 누르면 나오는 이름표
    /// </summary>
    public class SimpleToolTip : ToolTip
    {
        #region OpenPopup
        public static SimpleToolTip OpenPopup(string _title, GameObject _targetParent)
        {
            return OpenPopup(new TooltipPopupData(new ToolTipData(_title, "", _targetParent)));
        }
        public static SimpleToolTip OpenPopup(TooltipPopupData data)
        {
            if (data == null)
                return null;

            return PopupManager.OpenPopup<SimpleToolTip>(data);
        }
        #endregion

        protected override void SetPosition()
        {
            if (parentObject == null) { return; }
            if (nodeBody == null) { return; }

            var parentPos = parentObject.transform.position;
            var parentScale = parentObject.transform.parent.localScale;//오브젝트에 걸려있는 부모의 스케일
            var parentSizeY = parentObject.GetComponent<RectTransform>().sizeDelta.y;//높이
            var isReverse = Data.TipData.IsReverse;
            var isUpDown = Data.TipData.IsUpSideDown;

            nodeBody.transform.position = parentPos;//일단 가운대로 세팅

            var bgNode = SBFunc.GetChildrensByName(nodeBody.transform, new string[] { "bg" }).gameObject;//기존 (0,0)으로 잡혀있는 좌표 수정
            if (bgNode != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(bgNode.GetComponent<RectTransform>());//contentSize Fitter로 인한 sizeDelta 0 버그 해소용 코드
                var bgNodeHeight = bgNode.GetComponent<RectTransform>().sizeDelta.y;
                var scaleOneVec = new Vector2(0, bgNodeHeight * 0.5f + parentSizeY * 0.5f * parentScale.y);
                bgNode.GetComponent<RectTransform>().anchoredPosition = new Vector2(scaleOneVec.x, isUpDown ? -scaleOneVec.y : scaleOneVec.y);
            }
        }
    }
}

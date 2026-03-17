using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//타겟 오브젝트위치에서 desc 및 위치 데이터만 세팅해주고, 표시 및 설명 만 하는 역할.

namespace SandboxNetwork
{
    public class ObjectTargetIndicator : MonoBehaviour
    {
        [SerializeField] List<GameObject> arrowList = new List<GameObject>();
        [SerializeField] Text desc = null;
        [SerializeField] RectTransform targetRect = null;

        GameObject curTarget = null;
        public void SetData(GameObject targetObj, eObjectAnchor _anchor , string _desc)
        {
            curTarget = targetObj;

            InitArrowList();
            SetActiveArrow(_anchor);
            SetDesc(_desc);
            RefreshContentFitter(targetRect);
            SetPosition(_anchor);
        }

        void SetPosition(eObjectAnchor _anchor)
        {
            var size = targetRect.sizeDelta;//말풍선 사이즈
            var targetSize = curTarget ? curTarget.GetComponent<RectTransform>().sizeDelta : Vector2.zero;
            var targetPos = curTarget ? curTarget.GetComponent<RectTransform>().anchoredPosition3D : Vector3.zero;
            var scaleFactor = curTarget ? curTarget.GetComponent<RectTransform>().localScale : Vector3.one;
            var diffFactor = new Vector3(50, 50, 0);//화살표

            if(curTarget != null)
            {
                gameObject.GetComponent<RectTransform>().anchorMin = curTarget.GetComponent<RectTransform>().anchorMin;
                gameObject.GetComponent<RectTransform>().anchorMax = curTarget.GetComponent<RectTransform>().anchorMax;
            }

            //pivot 보정값 계산하기
            if(curTarget.GetComponent<RectTransform>().pivot.x == 1)
            {
                targetPos.x -= (targetSize.x * 0.5f * scaleFactor.x);
            }
            else if(curTarget.GetComponent<RectTransform>().pivot.x == 0)
            {
                targetPos.x += (targetSize.x * 0.5f * scaleFactor.x);
            }

            if(curTarget.GetComponent<RectTransform>().pivot.y == 1)
            {
                targetPos.y -= (targetSize.y * 0.5f * scaleFactor.y);
            }
            else if (curTarget.GetComponent<RectTransform>().pivot.y == 0)
            {
                targetPos.y += (targetSize.y * 0.5f * scaleFactor.y);
            }


            switch (_anchor)
            {
                case eObjectAnchor.LEFT_TOP:
                    gameObject.GetComponent<RectTransform>().anchoredPosition3D = new Vector3((float)(targetPos.x + targetSize.x * 0.5 + diffFactor.x + size.x * 0.5),
                        (float)(targetPos.y - targetSize.y * 0.5 - diffFactor.y - size.y * 0.5));
                    break;
                case eObjectAnchor.LEFT_BOTTOM:
                    gameObject.GetComponent<RectTransform>().anchoredPosition3D = new Vector3((float)(targetPos.x + targetSize.x * 0.5 + diffFactor.x + size.x * 0.5),
                        (float)(targetPos.y + targetSize.y * 0.5 + diffFactor.y + size.y * 0.5));
                    break;
                case eObjectAnchor.LEFT_CENTER:
                    gameObject.GetComponent<RectTransform>().anchoredPosition3D = new Vector3((float)(targetPos.x + targetSize.x * 0.5 + diffFactor.x * 2 + size.x * 0.5), targetPos.y);
                    break;
                case eObjectAnchor.RIGHT_TOP:
                    gameObject.GetComponent<RectTransform>().anchoredPosition3D = new Vector3((float)(targetPos.x - targetSize.x * 0.5 - diffFactor.x - size.x * 0.5),
                        (float)(targetPos.y - targetSize.y * 0.5 - diffFactor.y - size.y * 0.5));
                    break;
                case eObjectAnchor.RIGHT_BOTTOM:
                    gameObject.GetComponent<RectTransform>().anchoredPosition3D = new Vector3((float)(targetPos.x - targetSize.x * 0.5 - diffFactor.x - size.x * 0.5),
                        (float)(targetPos.y + targetSize.y * 0.5 + diffFactor.y + size.y * 0.5));
                    break;
                case eObjectAnchor.RIGHT_CENTER:
                    gameObject.GetComponent<RectTransform>().anchoredPosition3D = new Vector3((float)(targetPos.x - targetSize.x * 0.5 - diffFactor.x * 2 - size.x * 0.5),targetPos.y);
                    break;
                case eObjectAnchor.CENTER_TOP:
                    gameObject.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(targetPos.x,(float)(targetPos.y - targetSize.y * 0.5 - diffFactor.y * 2 - size.y * 0.5));
                    break;
                case eObjectAnchor.CENTER_BOTTOM:
                    gameObject.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(targetPos.x,(float)(targetPos.y + targetSize.y * 0.5 + diffFactor.y * 2 + size.y * 0.5));
                    break;
            }
        }

        void SetDesc(string _desc)
        {
            if(desc != null)
            {
                desc.text = _desc;
            }
        }

        void SetActiveArrow(eObjectAnchor _anchor)
        {
            if (arrowList == null || arrowList.Count <= 0)
                return;

            var modifyIndex = (int)_anchor -1;
            if (modifyIndex < 0 || modifyIndex >= arrowList.Count)
                return;

            arrowList[modifyIndex].SetActive(true);
        }

        void InitArrowList()
        {
            if (arrowList == null || arrowList.Count <= 0)
                return;

            foreach(var arrow in arrowList)
            {
                if (arrow == null)
                    continue;
                arrow.SetActive(false);
            }
        }

        protected void RefreshContentFitter(RectTransform transform)
        {
            if (transform == null || !transform.gameObject.activeSelf)
            {
                return;
            }

            foreach (RectTransform child in transform)
            {
                RefreshContentFitter(child);
            }

            var layoutGroup = transform.GetComponent<LayoutGroup>();
            var contentSizeFitter = transform.GetComponent<ContentSizeFitter>();
            if (layoutGroup != null)
            {
                layoutGroup.SetLayoutHorizontal();
                layoutGroup.SetLayoutVertical();
            }

            if (contentSizeFitter != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
            }
        }
    }
}

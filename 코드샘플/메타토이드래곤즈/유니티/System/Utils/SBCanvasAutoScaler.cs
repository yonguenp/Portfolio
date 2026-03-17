using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SandboxNetwork
{
    public class SBCanvasAutoScaler : UIBehaviour
    {
        [SerializeField]
        CanvasScaler scaler = null;

        [Header("exclusive option only 0f,0.5f,1f")]
        [SerializeField]
        bool isRatioMode = false;

        private float standardWidth = 16f;
        private float standardHeight = 9f;

        //float CheckDelay = 0.5f; // How long to wait until we check again.
        //bool isAlive = true;

        //Coroutine Co = null;

        protected override void Start()
        {
            if (scaler == null)
            {
                scaler = GetComponent<CanvasScaler>();
            }

            if (scaler != null)
            {
                AutoScaler();
            }

            //Co = StartCoroutine(CheckForChange());
        }

        void AutoScaler()
        {
            if(scaler == null)
            {
                return;
            }

            if (isEqualStandardResolution())//해상도 같으면 계산 할 필요 없음
            {
                scaler.matchWidthOrHeight = 0.5f;
                return;
            }

            float matchValue = isRatioMode ? CalcResolution() : GetExclusiveMathResolution();//켜져있으면 비례식으로 조절 , 아니면 3개의 값으로만 판단

            scaler.matchWidthOrHeight = matchValue;
        }

        bool isEqualStandardResolution()
        {
            var resolutionValue = standardWidth / standardHeight;

            var screenWidth = Screen.width;
            var screenHeight = Screen.height;

            float inputValue = (float)screenWidth / (float)screenHeight;


            return inputValue == resolutionValue;
        }

        bool isOverResolution()//16:9 기준으로 값이 큰가 작은가
        {
            var screenWidth = Screen.width;
            var screenHeight = Screen.height;

            var resolutionValue = standardWidth / standardHeight;
            float inputValue = (float)screenWidth / (float)screenHeight;


            return resolutionValue < inputValue;
        }

        float GetExclusiveMathResolution()
        {
            var isOver = isOverResolution();

            var matchValue = isOver ? 1f : 0f;

            return matchValue;
        }

        float CalcResolution()
        {
            var screenWidth = Screen.width;
            var screenHeight = Screen.height;

            //기본 해상도 16:9라 가정하고 변경해줄 해상도 계산
            float standardValue = standardWidth / standardHeight;
            float standardMatchValue = 0.5f;

            float inputValue = (float)screenWidth / (float)screenHeight;

            float inputMatchValue = inputValue * standardMatchValue / standardValue;

            return inputMatchValue;
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();

            if (scaler != null)
            {
                AutoScaler();
            }
        }

        //IEnumerator CheckForChange()
        //{
        //    while (isAlive)
        //    {
        //        if (scaler != null)
        //        {
        //            AutoScaler();
        //        }

        //        yield return SBDefine.GetWaitForSeconds(CheckDelay);
        //    }
        //}

        //protected override void OnDestroy()
        //{
        //    isAlive = false;

        //    if(Co != null)
        //    {
        //        StopCoroutine(Co);
        //        Co = null;
        //    }
        //}
#if UNITY_EDITOR

        [ContextMenu("AutoDoubleSize")]
        public void AutoDoubleSize()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();

            SetDoubleSize(transform);

            foreach (Text text in transform.GetComponentsInChildren<Text>(true))
            {
                text.fontSize *= 2;
                text.resizeTextMinSize *= 2;
                text.resizeTextMaxSize *= 2;
            }

            foreach (Image image in transform.GetComponentsInChildren<Image>(true))
            {
                if (image.type == Image.Type.Sliced)
                {
                    image.pixelsPerUnitMultiplier /= 2;
                }
            }

            foreach (LayoutGroup group in transform.GetComponentsInChildren<LayoutGroup>(true))
            {
                group.padding.left *= 2;
                group.padding.right *= 2;
                group.padding.top *= 2;
                group.padding.bottom *= 2;
                if (group as HorizontalLayoutGroup != null)
                    ((HorizontalLayoutGroup)group).spacing *= 2.0f;
                if (group as VerticalLayoutGroup != null)
                    ((VerticalLayoutGroup)group).spacing *= 2.0f;
            }

            foreach (var gridLayout in transform.GetComponentsInChildren<GridLayoutGroup>(true))
            {
                gridLayout.padding.left *= 2;
                gridLayout.padding.right *= 2;
                gridLayout.padding.top *= 2;
                gridLayout.padding.bottom *= 2;
                gridLayout.spacing *= 2f;
                gridLayout.cellSize *= 2f;
            }

            foreach (LayoutElement el in transform.GetComponentsInChildren<LayoutElement>(true))
            {
                el.minWidth *= 2;
                el.minHeight *= 2;

                el.preferredHeight *= 2;
                el.preferredWidth *= 2;

                el.flexibleWidth *= 2;
                el.flexibleHeight *= 2;
            }

            foreach (var tableview in transform.GetComponentsInChildren<TableView>(true))
            {
                tableview.SetPaddingMulti(2f);
                tableview.SetSpaceingMulti(2f);
            }

            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        }


        public void SetDoubleSize(Transform trans)
        {
            Debug.Log("SetDoubleSize : " + trans.gameObject.name);

            foreach (RectTransform rt in trans)
            {
                Vector2 min = rt.anchorMin;
                Vector2 max = rt.anchorMax;
                Vector2 sizeDelta = rt.sizeDelta;
                Vector2 offsetMin = rt.offsetMin;
                Vector2 offsetMax = rt.offsetMax;

                if (min.x == 0 && max.x == 1)
                {
                    //strech width
                    offsetMin.x = offsetMin.x * 2;
                    offsetMax.x = offsetMax.x * 2;

                    //rt.offsetMin = offsetMin;
                    //rt.offsetMax = offsetMax;
                }
                else
                {
                    sizeDelta.x = sizeDelta.x * 2;
                }

                if (min.y == 0 && max.y == 1)
                {
                    //strech height
                    offsetMin.y = offsetMin.y * 2;
                    offsetMax.y = offsetMax.y * 2;

                    //rt.offsetMin = offsetMin;
                    //rt.offsetMax = offsetMax;
                }
                else
                {
                    sizeDelta.y = sizeDelta.y * 2;
                }

                //rt.localPosition = rt.localPosition * 2.0f;
                rt.sizeDelta = sizeDelta;
                rt.anchoredPosition = rt.anchoredPosition * 2;

                SetDoubleSize(rt);
            }
        }
#endif
    }
}


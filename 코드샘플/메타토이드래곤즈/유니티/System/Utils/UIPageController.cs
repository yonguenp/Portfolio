using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class UIPageController : MonoBehaviour
    {
        public UIPageViewController parentController = null;
        public Toggle toggleBase = null;                // 복사 원본 페이지 인디케이터

        List<Toggle> toggleList = new List<Toggle>();   // 페이지 인디케이터를 저장

        public int PageCount { get { return toggleList.Count; }  }

        private void Awake()
        {
            // 복사 원본 페이지 인디케이터는 비활성화
            toggleBase.gameObject.SetActive(false);
        }

        // 페이지 수를 설정
        public void SetNumberOfPages(int number)
        {
            if (toggleList.Count < number)
            {
                // 페이지 인디케이터 수가 지정된 페이지 수보다 적을 경우
                // 복사 원본 페이지 인디케이터로부터 새로운 페이지 인디케이터 작성
                for (int i = toggleList.Count; i < number; ++i)
                {
                    Toggle indicator = Instantiate(toggleBase);
                    indicator.gameObject.SetActive(true);
                    indicator.transform.SetParent(toggleBase.transform.parent);
                    indicator.transform.localScale = toggleBase.transform.localScale;
                    indicator.isOn = false;

                    // 버튼 값 설정
                    Button childButton = indicator.GetComponentInChildren<Button>();
                    if (childButton != null)
                    {
                        int toggleIndex = i;        // closure problem
                        childButton.onClick.AddListener(() => { 
                            if (parentController != null)
                            {
                                parentController.MovePageByNumer(toggleIndex);
                            }
                        });
                    }

                    toggleList.Add(indicator);
                }
            }
            else if (toggleList.Count > number)
            {
                // 페이지 인디케이터 수가 지정된 페이지 수보다 많을 경우 삭제
                for (int i = toggleList.Count - 1; i >= number; --i)
                {
                    Destroy(toggleList[i].gameObject);
                    toggleList.RemoveAt(i);
                }
            }
        }

        // 현재 페이지를 설정
        public void SetCurrentPage(int index)
        {
            if (index >= 0 && index <= toggleList.Count - 1)
            {
                toggleList[index].isOn = true;
            }
        }
    }
}
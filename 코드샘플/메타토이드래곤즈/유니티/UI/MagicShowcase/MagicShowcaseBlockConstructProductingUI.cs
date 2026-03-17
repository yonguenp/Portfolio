using DG.Tweening;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    /// <summary>
    /// 블록 제작 팝업 제작중 상태
    /// </summary>
    public class MagicShowcaseBlockConstructProductingUI : MonoBehaviour
    {
        [SerializeField] float constructCoolTime = 3f;
        [SerializeField] Slider slider = null;
        [SerializeField] GameObject tweenObj = null;
        
        Sequence coolTimeSeq = null;
        int recipeKey = 0;//recipe_core table의 key값
        int recipeAmount = 0;//요구 수량

        private void OnDisable()
        {
            if (coolTimeSeq != null)
                coolTimeSeq.Kill();

            coolTimeSeq = null;
        }

        public void SetVisible(bool _isVisible)
        {
            gameObject.SetActive(_isVisible);
        }

        public void InitUI(int _recipeKey, int _recipeAmount)
        {
            recipeKey = _recipeKey;
            recipeAmount = _recipeAmount;

            PlaySequence();
        }

        void SetSlider(float value, float maxValue = 0)
        {
            if(slider != null)
            {
                if(maxValue > 0)
                    slider.maxValue = maxValue;

                slider.value = value;
            }    
        }

        void PlaySequence()
        {
            if (coolTimeSeq != null)
                coolTimeSeq.Kill();

            if (tweenObj != null)
                tweenObj.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            SetSlider(0, constructCoolTime);

            tweenObj.GetComponent<RectTransform>().DOAnchorPosY(constructCoolTime, constructCoolTime).SetEase(Ease.Linear).OnUpdate(()=> {
                SetSlider(tweenObj.GetComponent<RectTransform>().anchoredPosition.y);
            });

            coolTimeSeq = DOTween.Sequence();
            coolTimeSeq.AppendInterval(constructCoolTime).AppendCallback(() => {
                CompleteCoolTime();
            }).Play();
        }

        public void OnClickCancel()//초기값으로 돌아감
        {
            MagicShowcaseBlockConstructEvent.InitUI();
        }

        void CompleteCoolTime()
        {

            WWWForm data = new WWWForm();
            data.AddField("recipeid", recipeKey);
            data.AddField("itemamount", recipeAmount);

            NetworkManager.Send("magicshowcase/blockconstruct", data, (jsonData) =>
            {
                if (jsonData.ContainsKey("err"))
                {
                    var errorFlag = jsonData["err"].Value<int>();
                    if (errorFlag != 0)
                    {
                        ShowInitPopup();
                        return;
                    }
                }
                if (jsonData.ContainsKey("rs"))
                {
                    JToken resultResponse = jsonData["rs"];
                    if (resultResponse != null && resultResponse.Type == JTokenType.Integer)
                    {
                        int rs = resultResponse.Value<int>();
                        if((eApiResCode)rs == eApiResCode.OK)
                            MagicShowcaseBlockConstructEvent.ShowResult(jsonData);
                        else
                        {
                            ShowInitPopup();
                        }
                    }
                }
            },(jsonData)=> {
                ShowInitPopup();
            });
        }

        void ShowInitPopup()
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000636),
                                            () =>
                                            {
                                                MagicShowcaseBlockConstructEvent.InitUI();
                                            },
                                            null,
                                            () =>
                                            {
                                                MagicShowcaseBlockConstructEvent.InitUI();
                                            },
                                        true, false, false);
        }
    }
}

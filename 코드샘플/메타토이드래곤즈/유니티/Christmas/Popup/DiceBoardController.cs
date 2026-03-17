using DG.Tweening;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// DiceBoardSlot 에 대한 객체 컨트롤을 여기서 하고 서버데이터(주사위 onDice api) 처리도 여기서.
/// 주사위 던지기 연출 (diceController)
/// 주사위 슬롯(glow) 움직임 제어
/// </summary>
namespace SandboxNetwork
{
    public class DiceBoardController : MonoBehaviour
    {
        [SerializeField] List<DiceBoardSlot> diceList = new List<DiceBoardSlot>();
        [SerializeField] Text diceCountText = null;
        [SerializeField] SBDiceController diceController = null;
        [SerializeField] Button diceRollButton = null;
        [SerializeField] Button diceRollButton10 = null;
        [SerializeField] Button diceRollButton100 = null;
        [SerializeField] Text diceRollButtonText = null;

        [SerializeField] GameObject descNode = null;
        [SerializeField] CanvasGroup descAlpha = null;

        [SerializeField] GameObject eaterNode = null;
        [SerializeField] Button eaterButton = null;

        [SerializeField] string sfx_beef_name = null;

        EventDiceBaseData holidayData = null;
        int boardIndex = 0;//현재 유저의 말 위치인덱스

        Sequence slotProductionTween = null;
        Sequence boardProductionTween = null;
        bool isRollingDice = false;
        bool isSkipped = false;

        Coroutine multiThrowCo = null;

        private void OnDisable()
        {
            RefreshBoard();
        }

        public void SetBoardSlotData()//테이블 데이터 기준으로 깔기 - 한번만 실행.
        {
            isRollingDice = false;
            isSkipped = false;

            if (diceList == null || diceList.Count <= 0)
                return;

            holidayData = DiceEventPopup.GetHolidayData();
            if(holidayData == null)
            {
                Debug.LogError("HOLIDAY Data is null");
                return;
            }

            var eventKey = holidayData.GetScheduleDataKey();
            if (eventKey > 0)//dice board 그리기
            {
                var diceDataList = DiceBoardData.GetBoards(eventKey).ToList();
                if (diceDataList == null || diceDataList.Count <= 0)
                    return;

                diceDataList.Sort((x,y) => x.BOARD_ID - y.BOARD_ID);//board_id 오름차순 정렬

                if(diceDataList.Count != diceList.Count)
                {
                    Debug.LogError("주사위 보드 사이즈와 데이터 테이블 사이즈가 다름");
                    return;
                }

                boardIndex = holidayData.BoardIndex;

                for(int i = 0; i< diceList.Count; i++)
                {
                    var data = diceDataList[i];
                    if (data == null)
                        continue;

                    var diceUI = diceList[i];
                    if (diceUI == null)
                        continue;

                    diceUI.SetSlotData(boardIndex, data);
                }

                RefreshDiceCount();
            }
            InitDiceController();
            InitProductionTween();
        }

        public void RefreshBoard()
        {
            isRollingDice = false;
            isSkipped = false;
            InitProductionTween();
            InitDiceController();
            RefreshAllSlot();
            RefreshDiceCount();
            RefreshDescAlpha();
        }

        public void InitDiceController()
        {
            if (diceController != null)
            {
                diceController.SetVisibleOffAllDice();
                diceController.SetDiceLayer();
            }
        }

        public void InitProductionTween()
        {
            if (slotProductionTween != null)
                slotProductionTween.Kill();

            slotProductionTween = null;

            if (boardProductionTween != null)
                boardProductionTween.Kill();

            boardProductionTween = null;

            if (multiThrowCo != null)
                StopCoroutine(multiThrowCo);

            multiThrowCo = null;
        }

        public void RefreshDescAlpha()
        {
            if (descAlpha != null)
                descAlpha.alpha = 1;
        }

        public void RefreshAllSlot()
        {
            if (diceList == null || diceList.Count <= 0)
                return;

            if (holidayData == null)
                return;

            var curSlotIndex = holidayData.BoardIndex;

            foreach(var dice in diceList)
            {
                if (dice == null)
                    continue;

                dice.SetGlowVisible(curSlotIndex == dice.SlotIndex);
                dice.SetEffectNodeVisible(false);
            }

            boardIndex = curSlotIndex;
        }

        void SetVisibleTargetSlot(int _targetSlotIndex)//연출 시작전 초기화 용도
        {
            if (diceList == null || diceList.Count <= 0)
                return;

            foreach (var dice in diceList)
            {
                if (dice == null)
                    continue;

                dice.SetGlowVisible(_targetSlotIndex == dice.SlotIndex);
            }
        }

        public void OnClickDice(int count)
        {
            if (holidayData == null)
            {
                isRollingDice = false;
                return;
            }

            var curItem = holidayData.GetDiceItem();
            if (curItem == null)
            {
                isRollingDice = false;
                return;
            }

            //WJ - 2023.12.15 주사위 굴리기 UI_END_TIME 기준까지 굴릴 수 있게 해달라고 변경.
            var eventPeriod = holidayData.IsEventPeriod(true);
            if (!eventPeriod)
            {
                ToastManager.On(StringData.GetStringByStrKey("이벤트종료안내"));
                isRollingDice = false;
                RefreshDiceButton(0);
                return;
            }

            if (curItem.Amount < count)
            {
                //주사위 부족 토스트 || 버튼 회색 처리 논의
                ToastManager.On(StringData.GetStringByStrKey("이벤트주사위부족알림"));
                isRollingDice = false;
                return;
            }

            if (isRollingDice)
                return;

            var prevIndex = boardIndex;

            isRollingDice = true;

            holidayData.OnDice(count, (jsonData)=> 
            {
                List<Asset> rewardList = new List<Asset>();
                rewardList.Clear();
                if (jsonData.ContainsKey("reward"))//보상
                    rewardList = SBFunc.ConvertSystemRewardDataList((JArray)jsonData["reward"]);

                if (jsonData.ContainsKey("info"))//슬롯 데이터 갱신하고 이전 -> 현재 슬롯 간격 만큼 연출
                {
                    RefreshDiceCount();//주사위 갯수 먼저 갱신

                    var infoData = (JObject)jsonData["info"];
                    if (infoData != null)
                        boardIndex = infoData["board_index"].Value<int>();//일단 성공하면 대입
                }
                else
                {
                    isRollingDice = false;
                    ToastManager.On(StringData.GetStringByStrKey("서버요청실패"));
                    return;
                }

                var isMailSended = holidayData.IsMailSended(jsonData);//보상이 메일로 보내졌다는 플래그있어도 연출 붙어야함.
                if(jsonData.ContainsKey("isMulti") && jsonData["isMulti"].Value<bool>() == true)//여러번 던짐
                {
                    var throwCount = jsonData["use_count"].Value<int>();//주사위 던진 총 횟수

                    if(jsonData.ContainsKey("move"))
                    {
                        var moveObject = (JObject)jsonData["move"];
                        var moveIndexList = JArray.FromObject(moveObject["moves"]);
                        var moveIntList = moveIndexList.ToObject<List<int>>();//int list 로 치환

                        SetMultiDiceProduction(prevIndex, moveIntList, throwCount, rewardList, isMailSended);
                    }
                }
                else
                    SetBoardSlotBeep(prevIndex, boardIndex, rewardList, 0.5f, isMailSended);

            },(failData) => {
                isRollingDice = false;
                ToastManager.On(StringData.GetStringByStrKey("서버요청실패"));
            });
        }

        void RefreshDiceCount()
        {
            if (holidayData == null)
            {
                RefreshDiceButton(0);
                return;
            }

            var curItem = holidayData.GetDiceItem();
            if (curItem == null)
            {
                RefreshDiceButton(0);
                return;
            }

            if (diceCountText != null)
                diceCountText.text = string.Format("x {0}", curItem.Amount);

            var isUpperZero = curItem.Amount > 0;

            var maxDiceCount = Convert.ToInt32(GameConfigData.Get("2023_HOLIDAY_DICE_USE_MAX").VALUE);
            var availableCount = maxDiceCount > curItem.Amount ? curItem.Amount : maxDiceCount;

            if (!isUpperZero)
                availableCount = 0;

            if (diceRollButtonText != null)
            {
                if (availableCount > 1)
                    diceRollButtonText.text = StringData.GetStringFormatByStrKey("시작특수문자", availableCount);
                else
                    diceRollButtonText.text = StringData.GetStringByIndex(100001737);
                
            }

            //WJ - 2023.12.15 주사위 굴리기 UI_END_TIME 기준까지 굴릴 수 있게 해달라고 변경.
            if(holidayData.IsEventPeriod(true))
                RefreshDiceButton(curItem.Amount);
            else
                RefreshDiceButton(0);
            DiceUIEvent.RefreshTabReddot();//탭 레드닷 갱신
        }

        void RefreshDiceButton(int amount)
        {
            if (diceRollButton != null)
                diceRollButton.SetButtonSpriteState(amount > 0);
            if (diceRollButton10 != null)
            {
                diceRollButton10.SetButtonSpriteState(amount >= 10);
                diceRollButton10.gameObject.SetActive(amount >= 10);
            }
            if (diceRollButton100 != null)
            {
                diceRollButton100.SetButtonSpriteState(amount >= 100);
                diceRollButton100.gameObject.SetActive(amount >= 100);
            }
        }

        void SetBoardSlotBeep(int _startIndex, int _goalIndex, List<Asset> _rewardList, float glowDelay, bool _isMailSended = false)
        {
            if (diceController != null)
            {
                descAlpha.alpha = 0;
                SetVisibleTargetSlot(_startIndex);//시작 슬롯만 켜기
                PlayDiceTween(_startIndex, _goalIndex, _rewardList, glowDelay, _isMailSended);//연출 시작

                SetVisibleEaterNode(true);

                if (eaterButton != null)
                {
                    eaterButton.onClick.RemoveAllListeners();
                    eaterButton.onClick.AddListener(delegate {
                        OnClickEaterNode(_rewardList, _isMailSended);
                    });
                }
            }
        }

        void SetMultiDiceProduction(int _prevIndex , List<int> _moveIndexList , int _throwCount , List<Asset> _rewardList, bool _isMailSended = false)
        {
            if (diceController != null)
            {
                descAlpha.alpha = 0;

                SetVisibleEaterNode(true);

                if (eaterButton != null)
                {
                    eaterButton.onClick.RemoveAllListeners();
                    eaterButton.onClick.AddListener(delegate {
                        OnClickEaterNode(_rewardList, _isMailSended);
                    });
                }

                if (multiThrowCo != null)
                    StopCoroutine(multiThrowCo);

                multiThrowCo = StartCoroutine(MultipleThorwDice(_prevIndex, _moveIndexList, _throwCount , _rewardList, _isMailSended));
            }
        }

        //다중 주사위 연출
        IEnumerator MultipleThorwDice(int _prevIndex, List<int> _moveIndexList, int _throwCount, List<Asset> _rewardList, bool _isMailSended = false)
        {
            InitProductionTween();

            int curCount = 0;
            while(curCount < _throwCount)
            {
                //주사위 연출 추가
                var destList = GetIndexByList(_prevIndex , _moveIndexList, curCount);

                PlayDiceTween(destList[0], destList[1], 0.5f);

                yield return SBDefine.GetWaitForSeconds(1.05f);

                yield return slotProductionTween.WaitForCompletion();
                curCount++;
            }

            //토탈 주사위 보상 연출(스킵 기능)
            if (isSkipped)
            {
                SetSkippedTweenDice();
                yield break;
            }

            SetVisibleEaterNode(false);

            if (_isMailSended)
                ToastManager.On(StringData.GetStringByStrKey("보상아이템우편발송"));

            if (_rewardList != null && _rewardList.Count > 0 && isRollingDice)//완료 보상연출 추가
                SystemRewardPopup.OpenPopup(_rewardList, null, true);

            diceController.SetVisibleOffAllDice();

            if (descAlpha != null)
                descAlpha.DOFade(1, 0.2f);

            isRollingDice = false;
        }

        List<int> GetIndexByList(int _prevIndex, List<int> _moveIndexList, int _throwCount)
        {
            List<int> tempList = new List<int>();//0번이 시작 , 1번이 종료
            if (_throwCount == 0)
            {
                tempList.Add(_prevIndex);
                tempList.Add(_moveIndexList[0]);
            }
            else
            {
                tempList.Add(_moveIndexList[_throwCount - 1]);
                tempList.Add(_moveIndexList[_throwCount]);
            }

            return tempList;
        }

        void PlayDiceTween(int _startIndex, int _goalIndex, List<Asset> _rewardList, float glowDelay, bool _isMailSended = false)
        {
            var productionSlotList = GetBeepIndex(_startIndex, _goalIndex);//시작 인덱스도 포함해서 주기 때문에 자기 자신은 빼야함.

            InitProductionTween();

            boardProductionTween = DOTween.Sequence();//슬롯 이동 연출
            boardProductionTween.AppendCallback(() => { diceController.OnDiceAnimator(productionSlotList.Count - 1); });
            boardProductionTween.AppendInterval(1f);//glowDelay * productionSlotList.Count
            boardProductionTween.AppendCallback(() =>
            {
                if (isSkipped)
                {
                    SetSkippedTweenDice();
                    return;
                }

                var totalDelay = glowDelay * (productionSlotList.Count - 1) * 0.5f;//빨리 움직이다가, 나중에 느리게 움직이는 연출 만들기
                var weight = 1.5f;//가중치 2배 -> 1.5배로 수정
                var timeFactor = 0f;
                var totalSize = productionSlotList.Count - 1;
                var totalTimeFactor = 0f;
                for (int i = 0; i < totalSize; i++)
                {
                    var powFactor = (int)Mathf.Pow(weight, totalSize - i - 1);
                    totalTimeFactor += (float)1 / powFactor;
                }

                timeFactor = totalDelay / totalTimeFactor;

                slotProductionTween = DOTween.Sequence();//슬롯 이동 연출

                for (int i = 0; i < productionSlotList.Count; i++)
                {
                    var slotIndex = productionSlotList[i];
                    if (i < productionSlotList.Count - 1)//처음
                    {
                        slotProductionTween.AppendCallback(() => {
                            if (isSkipped)
                            {
                                SetSkippedTweenDice();
                                return;
                            }

                            if (isRollingDice)
                            {
                                diceList[slotIndex].SetGlowVisible(true);
                                PlaySfxSound();
                            }
                        });

                        var delayFactor = ((float)1 / Mathf.Pow(weight, totalSize - i - 1)) * timeFactor;

                        slotProductionTween.AppendInterval(delayFactor);
                        slotProductionTween.AppendCallback(() => {
                            if (isSkipped)
                            {
                                SetSkippedTweenDice();
                                return;
                            }

                            if (isRollingDice)
                                diceList[slotIndex].SetGlowVisible(false);
                        });
                    }
                    else//마지막 슬롯은 그냥 켬
                    {
                        slotProductionTween.AppendCallback(() => {
                            if (isSkipped)
                            {
                                SetSkippedTweenDice();
                                return;
                            }

                            if (isRollingDice)
                            {
                                diceList[slotIndex].SetGlowVisible(true);
                                diceList[slotIndex].SetEffectNodeVisible(true);
                                PlaySfxSound();
                            }
                        });

                        slotProductionTween.AppendInterval(1f);//보상 연출 호출 전 약간의 딜레이
                        diceList[slotIndex].SetEffectNodeVisible(false);//이펙트 끄고
                    }
                }

                slotProductionTween.OnComplete(() => {
                    if (isSkipped)
                    {
                        SetSkippedTweenDice();
                        return;
                    }

                    SetVisibleEaterNode(false);

                    if (_rewardList != null && _rewardList.Count > 0 && isRollingDice)//완료 보상연출 추가
                        SystemRewardPopup.OpenPopup(_rewardList,null, true);

                    if(_isMailSended)
                        ToastManager.On(StringData.GetStringByStrKey("보상아이템우편발송"));

                    diceController.SetVisibleOffAllDice();

                    if (descAlpha != null)
                        descAlpha.DOFade(1, 0.2f);

                    isRollingDice = false;
                });
                slotProductionTween.Play();
            });
        }
        void PlayDiceTween(int _startIndex, int _goalIndex, float glowDelay)
        {
            var productionSlotList = GetBeepIndex(_startIndex, _goalIndex);//시작 인덱스도 포함해서 주기 때문에 자기 자신은 빼야함.

            boardProductionTween = DOTween.Sequence();//슬롯 이동 연출
            boardProductionTween.AppendCallback(() => { diceController.OnDiceAnimator(productionSlotList.Count - 1); });
            boardProductionTween.AppendInterval(1f);//glowDelay * productionSlotList.Count
            boardProductionTween.AppendCallback(() =>
            {
                if (isSkipped)
                {
                    SetSkippedTweenDice();
                    return;
                }

                var totalDelay = glowDelay * (productionSlotList.Count - 1) * 0.5f;//빨리 움직이다가, 나중에 느리게 움직이는 연출 만들기
                var weight = 2;//가중치는 일단 2배
                var timeFactor = 0f;
                var totalSize = productionSlotList.Count - 1;
                var totalTimeFactor = 0f;
                for (int i = 0; i < totalSize; i++)
                {
                    var powFactor = (int)Mathf.Pow(weight, totalSize - i - 1);
                    totalTimeFactor += (float)1 / powFactor;
                }

                timeFactor = totalDelay / totalTimeFactor;

                slotProductionTween = DOTween.Sequence();//슬롯 이동 연출

                for (int i = 0; i < productionSlotList.Count; i++)
                {
                    var slotIndex = productionSlotList[i];
                    if (i < productionSlotList.Count - 1)//처음
                    {
                        slotProductionTween.AppendCallback(() => {
                            if (isSkipped)
                            {
                                SetSkippedTweenDice();
                                return;
                            }

                            if (isRollingDice)
                            {
                                diceList[slotIndex].SetGlowVisible(true);
                                PlaySfxSound();
                            }
                        });

                        var delayFactor = ((float)1 / Mathf.Pow(weight, totalSize - i - 1)) * timeFactor;

                        slotProductionTween.AppendInterval(delayFactor);
                        slotProductionTween.AppendCallback(() => {
                            if (isSkipped)
                            {
                                SetSkippedTweenDice();
                                return;
                            }

                            if (isRollingDice)
                                diceList[slotIndex].SetGlowVisible(false);
                        });
                    }
                    else//마지막 슬롯은 그냥 켬
                    {
                        slotProductionTween.AppendCallback(() => {
                            if (isSkipped)
                            {
                                SetSkippedTweenDice();
                                return;
                            }

                            if (isRollingDice)
                            {
                                diceList[slotIndex].SetGlowVisible(true);
                                diceList[slotIndex].SetEffectNodeVisible(true);
                                PlaySfxSound();
                            }
                        });

                        slotProductionTween.AppendInterval(1f);//보상 연출 호출 전 약간의 딜레이
                        diceList[slotIndex].SetEffectNodeVisible(false);//이펙트 끄고
                    }
                }
                slotProductionTween.Play();
            });
        }

        List<int> GetBeepIndex(int _start , int _dest)//시작과 끝으로 연출 해야될 인덱스 리스트 구하기
        {
            List<int> tempList = new List<int>();
            if(_start < _dest)//일반상태
            {
                for(int i = _start; i <= _dest; i++)
                    tempList.Add(i);
            }
            else
            {
                for(int i = _start; i < diceList.Count; i++)
                    tempList.Add(i);

                for (int k = 0; k <= _dest; k++)
                    tempList.Add(k);
            }

            return tempList;
        }
        void SetVisibleEaterNode(bool _isVisible)
        {
            if (eaterNode != null)
                eaterNode.SetActive(_isVisible);
        }

        void SetSkippedTweenDice()
        {
            InitProductionTween();
            diceController.SetVisibleOffAllDice();
        }
        public void OnClickEaterNode(List<Asset> _rewardList, bool _isMailSended = false)
        {
            StopSfxSound();

            isSkipped = true;

            InitProductionTween();

            RefreshAllSlot();

            SetVisibleEaterNode(false);

            diceController.SetVisibleOffAllDice();

            if (descAlpha != null)
                descAlpha.DOFade(1, 0.2f);

            if(_isMailSended)
                ToastManager.On(StringData.GetStringByStrKey("보상아이템우편발송"));

            if (_rewardList != null && _rewardList.Count > 0 && isRollingDice)//완료 보상연출 추가
                SystemRewardPopup.OpenPopup(_rewardList,()=> {
                    isRollingDice = false;
                    isSkipped = false;
                },true);
            else
            {
                isRollingDice = false;
                isSkipped = false;
            }
        }

        void PlaySfxSound()
        {
            if (string.IsNullOrEmpty(sfx_beef_name))
                return;

            SoundManager.Instance?.PlaySFX(sfx_beef_name);
        }

        void StopSfxSound()
        {
            if (string.IsNullOrEmpty(sfx_beef_name))
                return;

            SoundManager.Instance?.StopSFX(sfx_beef_name);
        }
    }
}


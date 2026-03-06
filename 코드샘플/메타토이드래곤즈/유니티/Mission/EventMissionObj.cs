using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    /// <summary>
    /// 이벤트 전용 일일 미션 오브젝트 - 퀘스트 데이터 과거 싱크 하는 부분이 시간이 서로 달라서 (기존 일일퀘는 새벽 4시 // 이벤트는 0시로 예정)
    /// 대응용으로 찢음.
    /// 구조 자체는 완전히 같아서, 현재 이벤트 일일 퀘스트가 과거일 때만 체크 하는 코드 수정
    /// </summary>
    public class EventMissionObj : MissionUIObject
    {
        [SerializeField]
        protected Button btnGetADReward = null;
        public override void OnClickGetReward()
        {
            if (currentQuest.IsAlreadyGetRewards())
            {
                ToastManager.On(StringData.GetStringByStrKey("일일보상오류"));
                return;
            }

            QuestManager.Instance.RequestAcceptableRewardQuest(currentQuest, () =>
            {
                QuestManager.Instance.RequestQuestComplete(currentQuest.ID, () => {
                    if (getRewardDelegate != null)
                        getRewardDelegate();
                });
            }
            , () =>
            {
                if (getRewardDelegate != null)//UI 갱신
                    getRewardDelegate();
            });
        }

        public void OnClickAdReward()//광고 보고 보상 받기
        {
            if (currentQuest.IsAlreadyGetRewards())
            {
                ToastManager.On(StringData.GetStringByStrKey("일일보상오류"));
                return;
            }

            QuestManager.Instance.RequestAcceptableRewardQuest(currentQuest, () =>
            {
                AdvertiseManager.Instance.TryADWithPopup((log) => {
                    //광고 시청 이후
                    QuestManager.Instance.RequestQuestComplete(currentQuest.ID, () => {
                        if (getRewardDelegate != null)
                            getRewardDelegate();
                    }, log);
                }, () => { ToastManager.On(100007692); });//더이상 광고를 불러올 수 없습니다.
            }
            , () =>
            {
                if (getRewardDelegate != null)//UI 갱신
                    getRewardDelegate();
            });
        }
    }
}

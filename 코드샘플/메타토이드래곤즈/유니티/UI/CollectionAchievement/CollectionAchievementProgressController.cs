using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class CollectionAchievementProgressController : MonoBehaviour
    {
        [SerializeField] List<CollectionAchievementProgressObject> progressList = new List<CollectionAchievementProgressObject>();

        public void InitUI(eCollectionAchievementType _tabType)//탭에 따라서 그냥 세팅 해버림
        {
            switch(_tabType)
            {
                case eCollectionAchievementType.COLLECTION:
                    SetColletionProgress();
                    break;
                case eCollectionAchievementType.ACHIEVEMENT:
                    SetAchievementProgress();
                    break;
            }

        }
        /// <summary>
        /// 0번은 드래곤 수집률 , 1번은 콜렉션 완성률
        /// </summary>
        void SetColletionProgress()
        {
            SetUserDragonCollect();
            SetUserCollection();
        }

        void SetUserDragonCollect()//유저 드래곤 
        {
            var userDragonCount = User.Instance.DragonData.GetAllUserDragons().Count;//유저 보유 드래곤 총 수량
            var dragonData = CharBaseData.GetTotalDragonCount();//드래곤 총 수량
            progressList[0].SetData(userDragonCount,dragonData);
        }
        void SetUserCollection()//콜렉션 완성률
        {
            var userCompleteCount = CollectionAchievementManager.Instance.GetCompleteDataByType(eCollectionAchievementType.COLLECTION).Count;//콜렉션 완성 갯수
            var collectionTableCount = CollectionData.GetCollectionTotalCount();
            progressList[1].SetData(userCompleteCount, collectionTableCount);
        }

        /// <summary>
        /// 0번은 업적 완성률
        /// </summary>
        void SetAchievementProgress()
        {
            var userCompleteCount = CollectionAchievementManager.Instance.GetCompleteDataByType(eCollectionAchievementType.ACHIEVEMENT).Count;//업적 완성 갯수
            var achievementTableCount = AchievementBaseData.GetAchievementTotalCount();
            progressList[0].SetData(userCompleteCount, achievementTableCount);
        }
    }
}


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public enum HotTimeDescType
    {
        NONE,
        ADVENTURE,
        DAILYDUNGEON,
        WORLDBOSS,
        GEMDUNGEON,

        DUNGEON_LIST
    }


    public class HotTimeDescSlot : MonoBehaviour
    {
        [SerializeField] HotTimeDescType type = HotTimeDescType.NONE;

        [SerializeField] Text remainLabel = null;
        [SerializeField] Text rateLabel = null;
        public HotTimeDescType GetDescType()
        {
            return type;
        }

        public void SetActiveSlot(bool _isActive)
        {
            gameObject.SetActive(_isActive);

            if(_isActive)
            {
                DateTime time = GameConfigTable.GetHotTimeEndTime(type);
                float rate = GameConfigTable.GetHotTimeRate(type);

                var timeInterval = TimeManager.GetTimeCompare(time);
                if (timeInterval > 0 && rate > 1.0f)
                {
                    var resultString = StringData.GetStringFormatByStrKey("남음", SBFunc.TimeCustomString(timeInterval, 1, true));
                    remainLabel.text = resultString;
                    rateLabel.text = "+ " + (Math.Round(rate - 1.0f, 2) * 100) + "%";
                }
                else
                {
                    remainLabel.text = "";
                    rateLabel.text = "";
                    gameObject.SetActive(false);
                }
            }
        }

        public void OnClickDirectMove()
        {
            if (type == HotTimeDescType.NONE)
                return;

            if (User.Instance.DragonData.GetAllUserDragons().Count <= 0)
            {
                ToastManager.On(100000623);
                return;
            }

            if(type == HotTimeDescType.GEMDUNGEON)
            {
                var gemDungeon = LandmarkGemDungeon.Get();
                if (gemDungeon == null)
                    ToastManager.On(StringData.GetStringByIndex(100000791));//건설이 필요합니다.
                else
                {
                    switch (gemDungeon.State)
                    {
                        case eBuildingState.NORMAL:
                            PopupManager.ClosePopup<HotTimeEventDescPopup>();
                            PopupManager.OpenPopup<GemDungeonPopup>();
                            break;
                        default:
                            ToastManager.On(StringData.GetStringByIndex(100000791));//건설이 필요합니다.
                            break;
                    }
                }
            }
            else
            {
                NetworkManager.Send("user/dungeonstate", null, (jsonData) => {
                    WorldBossManager.Instance.SetWorldBossProgress(jsonData);
                    PopupManager.AllClosePopup();
                    LoadingManager.Instance.EffectiveSceneLoad("WorldBossLobby", eSceneEffectType.CloudAnimation);

                    switch (type)
                    {
                        case HotTimeDescType.WORLDBOSS:
                        {
                            if (!WorldBossManager.Instance.IsWorldBossEnterCondition())
                            {
                                ToastManager.On("입장조건이 충분하지않습니다.\n" + StringData.GetStringFormatByStrKey("quest_group_CHECK_DRAGON_A", "", "",
                                    WorldBossManager.ENTER_DRAGON_LIMIT_COUNT, User.Instance.DragonData.GetAllUserDragons().Count));
                                return;
                            }

                            WorldBossManager.Instance.SetWorldBossProgress(jsonData);
                            LoadingManager.Instance.EffectiveSceneLoad("WorldBossLobby", eSceneEffectType.CloudAnimation);
                            PopupManager.ClosePopup<HotTimeEventDescPopup>();
                        }
                        break;
                        case HotTimeDescType.DAILYDUNGEON:
                        {
                            LoadingManager.Instance.EffectiveSceneLoad("DailyDungeonLobby", eSceneEffectType.CloudAnimation);
                            PopupManager.ClosePopup<HotTimeEventDescPopup>();
                        }
                        break;
                        case HotTimeDescType.ADVENTURE:
                        {
                            LoadingManager.Instance.EffectiveSceneLoad("AdventureStageSelect", eSceneEffectType.CloudAnimation);
                            PopupManager.ClosePopup<HotTimeEventDescPopup>();
                        }
                        break;
                    }
                });
            }

            //switch (type)
            //{
            //    case HotTimeDescType.ADVENTURE:
            //    case HotTimeDescType.WORLDBOSS:
            //    case HotTimeDescType.DAILYDUNGEON:
            //    {
            //        PopupManager.ClosePopup<HotTimeEventDescPopup>();
            //        PopupManager.OpenPopup<DungeonSelectPopup>();
            //    }
            //    break;
            //    case HotTimeDescType.GEMDUNGEON:
            //    {
            //        var gemDungeon = LandmarkGemDungeon.Get();
            //        if (gemDungeon == null)
            //            ToastManager.On(StringData.GetStringByIndex(100000791));//건설이 필요합니다.
            //        else
            //        {
            //            switch (gemDungeon.State)
            //            {
            //                case eBuildingState.NORMAL:
            //                    PopupManager.ClosePopup<HotTimeEventDescPopup>();
            //                    PopupManager.OpenPopup<GemDungeonPopup>();
            //                    break;
            //                default:
            //                    ToastManager.On(StringData.GetStringByIndex(100000791));//건설이 필요합니다.
            //                    break;
            //            }
            //        }
            //    }
            //    break;
            //}
        }
    }
}

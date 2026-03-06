using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class MiningDrillRepairInsufficiencyPopup : Popup<PopupData>
    {
        [SerializeField] Button worldbossDirectButton = null;

        public override void InitUI()
        {
            RefreshBossEnterButton();
        }

        public void RefreshBossEnterButton()
        {
            if (worldbossDirectButton != null)
                worldbossDirectButton.SetButtonSpriteState(WorldBossManager.Instance.IsWorldBossEnterCondition());
        }

        public void OnClickGachaShop()
        {
            SBFunc.MoveGachaScene(eGachaGroupMenu.LUCKYBOX);
        }

        public void OnClickWorldBoss()
        {
            if(!WorldBossManager.Instance.IsWorldBossEnterCondition())
            {
                ToastManager.On("입장조건이 충분하지않습니다.\n" +StringData.GetStringFormatByStrKey("quest_group_CHECK_DRAGON_A","","",
                    WorldBossManager.ENTER_DRAGON_LIMIT_COUNT, User.Instance.DragonData.GetAllUserDragons().Count));
                return;
            }

            //월드 보스 데이터 갱신 호출하는 api가 따로 없어서, 월드 보스쪽에대한 입장데이터 세팅
            //해당 api는 전투컨텐츠UI 호출시 호출하는 api (DungeonSelectPopup 팝업에 있는 내용)
            NetworkManager.Send("user/dungeonstate", null, (jsonData) => {
                WorldBossManager.Instance.SetWorldBossProgress(jsonData);
                PopupManager.AllClosePopup();
                LoadingManager.Instance.EffectiveSceneLoad("WorldBossLobby", eSceneEffectType.CloudAnimation);
            });
        }
    }
}


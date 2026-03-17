
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace SandboxNetwork
{
    public class GemDungeonBoostPopup : Popup<GemDungeonBoosterPopupData>
    {

        [SerializeField] GameObject boostPrefab;
        [SerializeField] Transform boostParentTr;
        public override void InitUI()
        {
            boostPrefab.SetActive(false);
            SBFunc.RemoveAllChildrens(boostParentTr);
            var list = ItemBaseData.GetItemListByKind(eItemKind.GEM_BOOSTER);
            
            foreach (var item in list)
            {
                int boastCnt = User.Instance.GetItemCount(item.KEY);
                if(boastCnt > 0)
                {
                    var boastObj = Instantiate(boostPrefab, boostParentTr);
                    boastObj.SetActive(true);
                    boastObj.GetComponent<GemDungeonBoostItemClone>().Init(item, boastCnt, OnClickBoast);
                }
            }
        }

        void OnClickBoast(int boastItemNo)
        {
            WWWForm param = new WWWForm();
            param.AddField("floor", Data.Floor);
            param.AddField("item_no", boastItemNo);
            // 네트워크 매니저 부스트 프로세스
            NetworkManager.Send("gemdungeon/booster", param, (JObject json) =>
            {
                ClosePopup();
                PopupManager.GetPopup<GemDungeonPopup>().Refresh();
            });
            
        }

        public override void OnClickDimd()
        {
            base.OnClickDimd();
            ClosePopup();
        }
    }
}


using Newtonsoft.Json.Linq;
using System.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public class ConstructionClone : MonoBehaviour
    {
        int buildTag = 0;
        int buildFloor = 0;
        int buildCell = 0;

        public delegate void Func();
        public Func cancelCallback = null;

        bool isNetworkState = false;
        bool IsTutorial = false;
        public void InitClone(int tag, int floor, int cell, Func _cancelCallback = null, bool isTutorial =false)
        {
            buildTag = tag;
            buildFloor = floor;
            buildCell = cell;
            cancelCallback = _cancelCallback;
            isNetworkState = false;
            IsTutorial = isTutorial;
            if (IsTutorial)
            {
                if(TutorialManager.tutorialManagement.GetCurTutoPrivateKey() == tag)
                    TutorialManager.tutorialManagement.NextTutorialStart();
            }
                
        }

        public void OnClickOkButton()
        {
            WWWForm paramData = new WWWForm();
            paramData.AddField("tag", buildTag);
            paramData.AddField("x", buildCell);
            paramData.AddField("y", buildFloor);
            if (isNetworkState)
            {
                return;
            }
            isNetworkState = true;
            NetworkManager.Send("building/construct", paramData, (NetworkManager.SuccessCallback)((jsonData) =>
            {
                isNetworkState = false;
                if (SBFunc.IsJTokenCheck(jsonData["rs"]))
                {
                    switch (jsonData["rs"].Value<int>())
                    {
                        case (int)eApiResCode.OK:
                            
                            SoundManager.Instance.PlaySFX("sfx_build_set");
                            Town.Instance.SetConstructModeState((bool)false);
                            UIManager.Instance.RefreshCurrentUI();
                            if (TutorialManager.tutorialManagement.IsPlayingTutorialByGroup(TutorialDefine.Construct) || TutorialManager.tutorialManagement.IsPlayingTutorialByGroup(TutorialDefine.ConstructUI))
                            {
                                WWWForm form = new WWWForm();
                                form.AddField("type", (int)eAccelerationType.CONSTRUCT);
                                form.AddField("tag", buildTag);
                                var ticketItemDataList = ItemBaseData.GetItemListByKind(eItemKind.ACC_TICKET);
                                ticketItemDataList = ticketItemDataList.OrderBy(elemet => elemet.VALUE).ToList();
                                if (ticketItemDataList.Count == 0)
                                    return;
                                form.AddField("item", ticketItemDataList[0].KEY);

                                NetworkManager.Send("building/haste", form, (jsonData) =>
                                {
                                    if (SBFunc.IsJTokenCheck(jsonData["rs"]))
                                    {
                                        switch (jsonData["rs"].Value<int>())
                                        {
                                            case (int)eApiResCode.OK:
                                                Town.Instance.RefreshMap();
                                                BuildCompleteEvent.Send(Town.Instance.GetBuilding(buildTag), eBuildingState.CONSTRUCT_FINISHED);
                                                break;
                                        }
                                    }
                                });
                            }
                            else
                            {
                                Town.Instance.RefreshMap();
                            }   
                            break;
                    }
                }
            }),(string arg) =>
            {
                isNetworkState= false;
            });
        }

        public void OnClickCancelButton()
        {
            Town.Instance.SetConstructModeState(false);
            if (cancelCallback != null)
                cancelCallback();
        }
    }
}
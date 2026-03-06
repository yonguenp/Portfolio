using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if DEBUG
namespace SandboxNetwork
{
    public class SimulatorPetPresetPanel : MonoBehaviour
    {
        [SerializeField] GameObject presetPanel = null;
        [SerializeField] TableView tableView = null;
        [SerializeField] Text invenCheckLabel = null;
        [SerializeField] GameObject petSlotContent = null;
        private List<JsonPreset> presetPets = null;
        private bool isTableInit = false;
        private bool viewDirty = true;

        PresetPet currentClickPreset = null;
        SimulatorDragonEditPetController petController = null;
        public void onClickClose()
        {
            SetVisiblePanel(false);
        }

        void SetVisiblePanel(bool _isVisible)
        {
            if (presetPanel != null)
            {
                presetPanel.SetActive(_isVisible);
            }
        }

        void initData()
        {
            SimulatorPreset.ReadMyDocument();
            presetPets = SimulatorPreset.GetPresetPets();
        }

        void init()
        {
            if (tableView != null && !isTableInit)
            {
                tableView.OnStart();
                isTableInit = true;
            }

            initData();
        }

        public void onShowPanel(SimulatorDragonEditPetController _controller)
        {
            if (_controller != null && petController == null)
            {
                petController = _controller;
            }

            init();

            currentClickPreset = null;//선택된 펫 슬롯 초기화

            SetVisiblePanel(true);

            viewDirty = true;
            DrawScrollView();
        }

        public void DrawScrollView()
        {
            if (!viewDirty || tableView == null || presetPets == null)
            {
                return;
            }

            if (presetPets.Count <= 0)
            {
                invenCheckLabel.gameObject.SetActive(true);
                invenCheckLabel.text = "등록한 프리셋이 없습니다.";//기본 인벤에 프리셋 없을 때 처리
            }
            else
            {
                invenCheckLabel.gameObject.SetActive(false);
            }
            List<ITableData> tableViewItemList = new List<ITableData>();
            tableViewItemList.Clear();
            if (presetPets != null && presetPets.Count > 0)
            {
                for (var i = 0; i < presetPets.Count; i++)
                {
                    var data = presetPets[i];
                    if (data == null)
                    {
                        continue;
                    }

                    tableViewItemList.Add((ITableData)data);
                }
            }

            tableView.SetDelegate(new TableViewDelegate(tableViewItemList, (GameObject node, ITableData item) => {
                if (node == null)
                {
                    return;
                }
                var frame = node.GetComponent<PresetPetSlot>();
                if (frame == null)
                {
                    return;
                }

                var presetData = (PresetPet)item;
                frame.SetVisibleClickNode(currentClickPreset == presetData);
                frame.SetData(presetData);
                frame.setCallback((param) => {
                    initPartslotClickNode();
                    onClickData(param);
                    frame.SetVisibleClickNode(true);
                });
            }));

            tableView.ReLoad();
            viewDirty = false;
        }

        void initPartslotClickNode()
        {
            var children = SBFunc.GetChildren(petSlotContent);
            if (children == null || children.Length <= 0)
            {
                return;
            }

            for (var i = 0; i < children.Length; i++)
            {
                var node = children[i];
                if (node == null)
                {
                    continue;
                }

                var partComp = node.GetComponent<PresetPartSlot>();
                if (partComp == null)
                {
                    continue;
                }
                partComp.SetVisibleClickNode(false);
            }
        }
        void onClickData(PresetPet _clickData)
        {
            currentClickPreset = _clickData;

        }

        public void onClickApplyPreset()
        {
            if (currentClickPreset == null)
            {
                ToastManager.On("적용할 프리셋을 먼저 선택해주세요");
                return;
            }

            SystemPopup.OnSystemPopup("적용하기", "현재 선택한 프리셋을 적용할까요?",
                () => {
                    //ok
                    //현재 클릭한 프리셋 파츠 데이터와 슬롯 인덱스 넘기기
                    if (petController != null)
                    {
                        petController.CustomPresetLoad(currentClickPreset);
                    }

                    ToastManager.On("프리셋 적용완료");
                    onClickClose();
                }, 
                () => {
                    //cancel
                },
                () => {
                    //x
                }
            );
        }
    }
}
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if DEBUG

namespace SandboxNetwork
{
    public class SimulatorPartPresetPanel : MonoBehaviour
    {
        [SerializeField] private TMPro.TMP_Dropdown partSlotDrop = null;
        [SerializeField] GameObject presetPanel = null;
        [SerializeField] TableViewGrid tableViewGrid = null;
        [SerializeField] Text invenCheckLabel = null;
        [SerializeField] GameObject partSlotContent = null;
        private List<JsonPreset> presetParts = null;
        private bool isTableInit = false;
        private bool viewDirty = true;

        PresetPart currentClickPreset = null;
        SimulatorDragonEditPartController partController = null;
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
            presetParts = SimulatorPreset.GetPresetParts();
        }

        void init()
        {
            if (tableViewGrid != null && !isTableInit)
            {
                tableViewGrid.OnStart();
                isTableInit = true;
            }

            initData();
        }

        public void onShowPanel(SimulatorDragonEditPartController _controller)
        {
            if(_controller != null && partController == null)
            {
                partController = _controller;
            }

            init();

            currentClickPreset = null;//선택된 파츠 슬롯 초기화

            SetVisiblePanel(true);

            viewDirty = true;
            DrawScrollView();

            SetDropData();//드롭다운 데이터 세팅
        }

        public void DrawScrollView()
        {
            if (!viewDirty || tableViewGrid == null || presetParts == null)
            {
                return;
            }

            if (presetParts.Count <= 0)
            {
                invenCheckLabel.gameObject.SetActive(true);
                invenCheckLabel.text = "등록한 프리셋이 없습니다.";//기본 인벤에 장비가 없을 때 처리
            }
            else
            {
                invenCheckLabel.gameObject.SetActive(false);
            }
            List<ITableData> tableViewItemList = new List<ITableData>();
            tableViewItemList.Clear();
            if (presetParts != null && presetParts.Count > 0)
            {
                for (var i = 0; i < presetParts.Count; i++)
                {
                    var data = presetParts[i];
                    if (data == null)
                    {
                        continue;
                    }

                    tableViewItemList.Add((ITableData)data);
                }
            }

            tableViewGrid.SetDelegate(new TableViewDelegate(tableViewItemList, (GameObject node, ITableData item) => {
                if (node == null)
                {
                    return;
                }
                var frame = node.GetComponent<PresetPartSlot>();
                if (frame == null)
                {
                    return;
                }

                var presetData = (PresetPart)item;
                frame.SetVisibleClickNode(currentClickPreset == presetData);
                frame.SetData(presetData);
                frame.setCallback((param)=> {
                    initPartslotClickNode();
                    onClickData(param);
                    frame.SetVisibleClickNode(true);
                });
            }));

            tableViewGrid.ReLoad();
            viewDirty = false;
        }

        void initPartslotClickNode()
        {
            var children = SBFunc.GetChildren(partSlotContent);
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
        void onClickData(PresetPart _clickData)
        {
            currentClickPreset = _clickData;

        }

        void SetDropData()
        {
            if(partSlotDrop == null)
            {
                return;
            }

            partSlotDrop.ClearOptions();

            List<string> stringList = new List<string>();

            for(var i = 0; i < 6; i ++)//최대 슬롯 6개라 하드코딩
            {
                stringList.Add((i + 1).ToString());
            }

            partSlotDrop.AddOptions(stringList);
        }
        public void onClickSlotDrop()//현재 클릭한 슬롯 인덱스
        {



        }

        public void onClickApplyPreset()
        {
            if(currentClickPreset == null)
            {
                ToastManager.On("적용할 프리셋을 먼저 선택해주세요");
                return;
            }

            SystemPopup.OnSystemPopup("적용하기", "현재 선택한 프리셋을 슬롯 " + partSlotDrop.captionText.text + "번에 적용할까요?",
                () => {
                    //ok
                    //현재 클릭한 프리셋 파츠 데이터와 슬롯 인덱스 넘기기
                    if(partController != null)
                    {
                        partController.CustomPresetLoad(currentClickPreset, int.Parse(partSlotDrop.captionText.text));
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
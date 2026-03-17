using Newtonsoft.Json.Linq;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

#if DEBUG
namespace SandboxNetwork
{
    public class PresetDragonSlot : MonoBehaviour, EventListener<SBSimulatorEvent>
	{
		[SerializeField]
		private Text presetNameField;
		[SerializeField]
		private int battleLineIndex;

		private PresetDragon preset = null;
		public PresetDragon Preset { get { return preset; } }
		private int did;

		private SimulatorBattleLine battleLine = new SimulatorBattleLine();
		private void Start()
		{
			EventManager.AddListener(this);
		}
		private void OnDisable()
		{
			EventManager.RemoveListener(this);
		}
		public void SetBattleLine(SimulatorBattleLine _battleLine)
        {
			battleLine = _battleLine;
        }

		bool isBattleLineFull()
        {
			if(battleLine == null)
            {
				return false;
            }

			return battleLine.IsDeckFull();
        }

		bool isExistDragon(int tag)
        {
			if (battleLine == null)
			{
				return false;
			}

			return battleLine.GetDragon(tag) > 0;
		}

		public void OnClickLoadDragonPreset(int tag)
		{
            if (isBattleLineFull() && !isExistDragon(tag))//덱이 다 찼는데, 현재 있는 드래곤을 교체하는 거면 상관없음
			{
				ToastManager.On(100002518);
				return;
			}

			PopupManager.OpenPopup<SimulatorPresetPopup>(new SimulatorDragonPopupData(tag.ToString()));
		}

		public void OnClickSaveDragonPreset()
		{
			PopupManager.OpenPopup<SimulatorPresetSavePopup>(new SimulatorDragonPopupData(tag.ToString()));
		}

		public void RefreshPresetData(PresetDragon _preset, int _dragonTag)
        {
			preset = _preset;
			did = _dragonTag;

			if (presetNameField != null)
			{
				if (preset != null)
				{
					presetNameField.text = preset.PresetName;
				}
				else
				{
					presetNameField.text = "";
				}
			}
		}

		public void OnEvent(SBSimulatorEvent eventType)
		{
			if (battleLineIndex != eventType.battleLineIndex)
			{
				return;
			}

			switch (eventType.Event)
			{
				case SBSimulatorEvent.eSimulatorEventEnum.deleteDragonUI:
					if(presetNameField != null)
						presetNameField.text = "";
					did = 0;
					break;
				case SBSimulatorEvent.eSimulatorEventEnum.drawDragonUI:
					RefreshPresetData(eventType.preset , eventType.dragonData.Tag);
					break;
				case SBSimulatorEvent.eSimulatorEventEnum.refreshDragonData:
					break;
				case SBSimulatorEvent.eSimulatorEventEnum.refreshPartData:
					break;
				case SBSimulatorEvent.eSimulatorEventEnum.refreshPetData:
					break;
				default:
					break;
			}
		}
	}
}

#endif
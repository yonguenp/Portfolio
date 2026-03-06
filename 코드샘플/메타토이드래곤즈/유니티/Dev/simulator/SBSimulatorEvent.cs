using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if DEBUG

//드래곤 그리기, 드래곤 스텟 및 장비,펫 데이터 변경점(user Data 변조용), 모두 이벤트로 제어 해보려함
namespace SandboxNetwork
{
	public struct SBSimulatorEvent
	{
		public enum eSimulatorEventEnum
		{
			deleteDragonUI,//드래곤 배치 빈칸 만들기
			drawDragonUI,//드래곤 새로 그리기

			refreshDragonData,//드래곤 데이터 변경
			refreshPartData,//장비 데이터 변경
			refreshPetData,//펫 데이터 변경 

			dragonDataLoadStart,
			dragonDataLoadEnd,
		}

		public eSimulatorEventEnum Event;
		static SBSimulatorEvent e;

		// 기타 추가 정보가 있다면
		/*
		 * 하위 깡통 데이터는 던지기 전에 미리 만들고
		 * 결과 데이터를 SBPveSimulator쪽에서 DragonData, PartData, PetData에 집어 넣는 형태로 생각 중 
		 */

		public int battleLineIndex;//배치 세팅할 위치인덱스
		public int dragonTag;//단순 지우기 용도

		public UserDragon dragonData;
		public UserPart partData;
		public UserPet petData;
		public PresetDragon preset;

		public SBSimulatorEvent(eSimulatorEventEnum _Event,int _dragonTag, int _battleLineIndex, UserDragon _dragonData, UserPart _partData, UserPet _petData, PresetDragon _preset = null)
		{
			Event = _Event;
			dragonTag = _dragonTag;
			battleLineIndex = _battleLineIndex;
			dragonData = _dragonData;
			partData = _partData;
			petData = _petData;
			preset = _preset;
		}

		public static void deleteDragonUI(int _index)
		{
			e.battleLineIndex = _index;
			e.Event = eSimulatorEventEnum.deleteDragonUI;
			EventManager.TriggerEvent(e);
		}

		public static void drawDragonUI(UserDragon _dragonData, int _index, PresetDragon _preset = null)
		{
			e.dragonData = _dragonData;
			e.battleLineIndex = _index;
			e.Event = eSimulatorEventEnum.drawDragonUI;
			e.preset = _preset;
			EventManager.TriggerEvent(e);
		}

		public static void refreshDragonData()
		{
			e.Event = eSimulatorEventEnum.refreshDragonData;
			EventManager.TriggerEvent(e);
		}
		public static void DragonDataLoadStart()
		{
			e.Event = eSimulatorEventEnum.dragonDataLoadStart;
			EventManager.TriggerEvent(e);
		}
		public static void DragonDataLoadEnd()
		{
			e.Event = eSimulatorEventEnum.dragonDataLoadEnd;
			EventManager.TriggerEvent(e);
		}
	}
}

#endif
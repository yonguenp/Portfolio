using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if DEBUG

namespace SandboxNetwork
{
    public class SimulatorPresetPopup: Popup<SimulatorDragonPopupData>
	{
		readonly int PRESETCLONE_WIDTH = 220;
		readonly int PRESETCLONE_HEIGHT = 40;

		[SerializeField] Text path;
		[SerializeField] ScrollRect scroll;
		[SerializeField] GameObject presetClone;
		[SerializeField] SimulatorPresetDetailPanel detailPanel = null;
		List<JsonPreset> dragons;
		int selected;

		public override void InitUI()
		{
			SBFunc.RemoveAllChildrens(scroll.content);

			selected = -1;
			SimulatorPreset.ReadMyDocument();
			dragons = SimulatorPreset.GetPresetDragons();

			path.text = SimulatorPreset.DIRECTORY_MTD_PATH.Split(":")[1] + "\\" + SimulatorPreset.FILE_PRESET;
			scroll.content.sizeDelta = new Vector2(PRESETCLONE_WIDTH, PRESETCLONE_HEIGHT * dragons.Count);

			for (int i = 0; i < dragons.Count; i++)
			{
				GameObject go = Instantiate(presetClone, scroll.content);
				int pid = i;
				go.GetComponentInChildren<Text>().text = dragons[i].PresetName;
				go.GetComponentInChildren<Button>().onClick.AddListener(delegate {LoadDragonPreset(pid);});
				go.SetActive(true);
			}

			detailPanel.onHidePanel();
		}

		public void OnClickLoad()
		{
			if(selected < 0)
			{
				return;
			}			

			int index = int.Parse(Data.SimulatorDragonTag);			
			UserDragon ndragon = new UserDragon();
			PresetDragon pdragon = (PresetDragon)dragons[selected];

			var prevDragonCheck = User.Instance.DragonData.GetDragon(pdragon.DragonID);
			if (prevDragonCheck != null)
			{
				ToastManager.On("덱에 중복 드래곤은 불가능해요");
				return;
			}

			var baseData = CharBaseData.Get(pdragon.DragonID);

			ndragon.SetBaseData(pdragon.DragonID, eDragonState.Normal, CharExpData.GetCurrentAccumulateGradeAndLevelExp(baseData.GRADE, ndragon.Level),
				CharExpData.GetLevelAndExpByTotalExp(baseData.GRADE, pdragon.Exp).FinalLevel, pdragon.SkillLevel, -1);
			ndragon.SetPetTag(0);

			//pet, part 생성 및 링크 연결
			PresetPart[] parts = pdragon.PartsArray;

			for (int i = 0; i < 6; i++)
			{
				int tag = 6 * index + i + 1;
				ndragon.Parts[i] = 0;

				if (parts.Length > i)
				{
					PresetPart partPreset = parts[i];
					if(partPreset.ToString() == "{}")//빈값 체크
                    {
						continue;
                    }

					JObject partObj = partPreset.ToJObject();
					if (User.Instance.PartData.GetPart(tag) != null)
					{
						User.Instance.PartData.DeleteUserPart(tag);//등록한 파츠 지우기
					}

					if(partObj.ContainsKey("subs"))
                    {
						JArray partArr = (JArray)partObj["subs"];
						JArray newSubsArr = new JArray();
						if(partArr.Count > 0)
                        {
							for(var k = 0; k < partArr.Count; k++)
                            {
								JArray keyValueArr = new JArray();
								var key = partArr[k].Value<int>();
								var value = TableManager.GetTable<SubOptionTable>().Get(key).VALUE_MAX;

								keyValueArr.Add(key);
								keyValueArr.Add(value);

								newSubsArr.Add(keyValueArr);
							}
						}

						partObj.Remove("subs");//이전 부옵데이터 삭제
						partObj.Add("subs", newSubsArr);//밸류 값 넣어서 새로 넣기
                    }

					partObj.Add("equip_tag", tag);
					User.Instance.PartData.AddUserPart(partObj);
					User.Instance.PartData.SetPartLink(tag, ndragon.Tag);
					ndragon.Parts[i] = tag;
				}
			}

			ndragon.SetPartSetEffectOption();//부옵 계산

			int petTag = pdragon.DragonID;

			if (User.Instance.PetData.GetPet(petTag) != null)
			{
				User.Instance.PetData.DeleteUserPet(petTag);
			}

			PresetPet presetPet = pdragon.Pet;

			if (presetPet.PetID > 0)
			{
				var petBaseData = PetBaseData.Get(presetPet.PetID);
				if (petBaseData != null)
				{
					var level = 1;
					var levelAndExpData = TableManager.GetTable<PetExpTable>().GetLevelAndExpByTotalExp(presetPet.Exp, petBaseData.GRADE);
					if (levelAndExpData != null && levelAndExpData.ContainsKey("finallevel"))
						level = levelAndExpData["finallevel"];

					UserPet userPetTempData = new UserPet(petTag, presetPet.PetID, level, presetPet.Exp, 0, -1);

					userPetTempData.SetUniqueSkillID(-1);
					userPetTempData.SetSkillsID(presetPet.Passives.ToArray());
					User.Instance.PetData.AddPet(userPetTempData);
					ndragon.SetPetTag(petTag);
					User.Instance.PetData.SetPetLink(petTag, ndragon.Tag);
				}
			}			
			
			User.Instance.DragonData.AddUserDragon(pdragon.DragonID, ndragon);
			SBSimulatorEvent.DragonDataLoadStart();
			SBSimulatorEvent.drawDragonUI(ndragon, index, pdragon);
			ClosePopup();
			SBSimulatorEvent.DragonDataLoadEnd();
		}

		public void LoadDragonPreset(int pid)
		{
			//Debug.Log(pid);
			selected = pid;

			RefreshButtonInteract();

			if(dragons != null && dragons.Count > 0 && dragons.Count > pid)
				detailPanel.onShowPanel((PresetDragon)dragons[pid]);
		}

		public void RefreshButtonInteract()
        {
			if(dragons == null || dragons.Count <= 0)
            {
				return;
            }

			var scrollChild = SBFunc.GetChildren(scroll.content);
			if(scrollChild == null || scrollChild.Length <= 0)
            {
				return;
            }

			List<GameObject> contentChild = new List<GameObject>();
			contentChild.Clear();

			for(var i = 0; i < scrollChild.Length; i++)
            {
				contentChild.Add(scrollChild[i].gameObject);
            }

			for(var i = 0; i < dragons.Count; i++)
            {
				var preset = dragons[i];
				if(preset == null)
                {
					continue;
                }

				var button = contentChild[i].GetComponent<Button>();
				if (button != null)
					button.interactable = (selected != i);

			}
        }
	}
}
#endif
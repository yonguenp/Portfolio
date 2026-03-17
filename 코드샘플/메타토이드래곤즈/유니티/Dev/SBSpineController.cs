using DG.Tweening;
using Newtonsoft.Json.Linq;
using Spine;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
	public class SBSpineController : MonoBehaviour
	{
		private UIDragonSpine spine = null;
		private BattleMonsterSpine mspine = null;

		[SerializeField] private GameObject[] uispineList = null;
		[SerializeField] private GameObject[] spineList = null;
		[SerializeField] private GameObject[] effectSpineList = null;
		[SerializeField] private bool toggleUiPref;

		private List<string> spineNameList = null;

		#region Controller Objects
		[SerializeField] private GameObject spineParent = null;
		[SerializeField] private GameObject effectSpineParent = null;
		[SerializeField] private GameObject monsterSpineParent = null;
		[SerializeField] private GameObject targetSpine = null;
		[SerializeField] private GameObject layout = null;
		[SerializeField] private GameObject normalSearchDropNode = null;
		[SerializeField] private TMPro.TMP_Dropdown SpineDrops;
		[SerializeField] private TMPro.TMP_Dropdown SkinDrops;
		[SerializeField] private TMPro.TMP_Dropdown AniDrops;
		[SerializeField] private TMPro.TMP_Dropdown EffectDrops;
		[SerializeField] private TMPro.TMP_Dropdown MonsterDrops;
		[SerializeField] private TMPro.TMP_Dropdown MonsterAniDrops;
		[SerializeField] private TMPro.TMP_Dropdown normalGradeDrops;
		[SerializeField] private TMPro.TMP_Dropdown normalNumberDrops;

		[SerializeField] private Toggle toggleSpine;
		[SerializeField] private Toggle toggleShadow;
		[SerializeField] private TMPro.TextMeshProUGUI playTimeLabel;
		[SerializeField] private TMPro.TextMeshProUGUI playSpeedLabel;
		[SerializeField] private Slider frameSlider = null;
		[SerializeField] private Slider mFrameSlider = null;
		[SerializeField] private TMPro.TextMeshProUGUI sliderLabel = null;
		[SerializeField] private TMPro.TextMeshProUGUI mSliderLabel = null;

		[SerializeField] private Slider rangeXSlider = null;
		[SerializeField] private Slider rangeYSlider = null;

		[SerializeField] private LineRenderer skill_line = null;
		[SerializeField] private LineRenderer normal_line = null;
		[SerializeField] private LineRenderer effect_skill_line = null;
		[SerializeField] private LineRenderer effect_normal_line = null;

		[SerializeField] private LineRenderer monster_skill_line = null;
		[SerializeField] private LineRenderer monster_normal_line = null;

		[SerializeField] private SpriteRenderer effect_normal_sprite = null;
		[SerializeField] private SpriteRenderer effect_skill_sprite = null;

		[SerializeField] private Sprite[] circleFrame = null;

		[SerializeField] private TMPro.TextMeshProUGUI skill_rangeXLabel;
		[SerializeField] private TMPro.TextMeshProUGUI skill_rangeYLabel;
		[SerializeField] private TMPro.TextMeshProUGUI normal_rangeXLabel;
		[SerializeField] private TMPro.TextMeshProUGUI normal_rangeYLabel;

		[SerializeField] private TMPro.TextMeshProUGUI EffectLabel;
		[SerializeField] Button rangeResetButton = null;

		[SerializeField] private Toggle toggleSkillRange = null;
		[SerializeField] private Toggle toggleNormalRange = null;
		[SerializeField] private Toggle toggleEffectSkillRange = null;
		[SerializeField] private Toggle toggleEffectNormalRange = null;


		private Vector3[] _ellipsePoints = new Vector3[0];
		public Vector3[] GetEllipsePoints { get { return _ellipsePoints; } }

		Dictionary<string, Spine.Skin> skinDictionary;
		Dictionary<string, Spine.Animation> animDictionary;
		Dictionary<string, Spine.Animation> mAnimDictionary;
		SkeletonData skData = null;
		bool btoggleSpine = true;

		private GameObject[] monsterList = null;
		private GameObject targetMonster = null;
		private Vector2 skillRange = Vector2.zero;

		private float playAnimSpeed = 1.0f;

		Dictionary<string, List<string>> searchNormalDataDic = new Dictionary<string, List<string>>();//등급 , 순서
		#endregion

		void initTool()
		{
			UIManager.Instance.InitUI(eUIType.None);
			ClearOptions();
			skinDictionary = new Dictionary<string, Skin>();
			animDictionary = new Dictionary<string, Spine.Animation>();
			mAnimDictionary = new Dictionary<string, Spine.Animation>();
			spineNameList = new List<string>();

			SetVisibleSearchNode(false);//노말 검색 노드 일단 끄기

			InitSpineDrops();
			InitEffectDrops();
			InitMonsterDrops();
			//onChangeSpineOption();
			//onChangeSkinOption();
		}

		public void onClickSpineOptionButton()
		{
			if (targetSpine != null)
			{
				Destroy(targetSpine);
			}

			if (SpineDrops.captionText.text == "none")
			{
				initAllDragonCharacter();
				return;
			}

			if (toggleUiPref)
			{
				targetSpine = Instantiate(uispineList[spineNameList.IndexOf(SpineDrops.captionText.text) - 1]);
				targetSpine.transform.SetParent(spineParent.transform);
				targetSpine.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
			}
			else
			{
				targetSpine = Instantiate(spineList[spineNameList.IndexOf(SpineDrops.captionText.text) - 1]);
				targetSpine.transform.SetParent(spineParent.transform);
			}

			spine = targetSpine.GetComponentInChildren<UIDragonSpine>();
			skData = spine.GetSkeletonData();

			//onChangeSkinOption();
			InitSkinDrops();
			InitAniDrops();
			onChangeAniOption();

			if (SpineDrops.captionText.text == "normal")
			{
				SetVisibleSearchNode(true);//검색 노드 켜기
				SetNormalGradeDropsData();//등급 드롭다운 켜기
			}
			else
			{
				SetVisibleSearchNode(false);
			}
		}

		void initAllDragonCharacter()
		{
			if (targetSpine != null)
			{
				Destroy(targetSpine);
			}

			spine = null;

			effect_normal_line.positionCount = 0;
			effect_skill_line.positionCount = 0;
			normal_line.positionCount = 0;
			skill_line.positionCount = 0;

			effect_normal_sprite.gameObject.SetActive(false);
			effect_skill_sprite.gameObject.SetActive(false);

			//스킬 범위 초기화
			skillRange = Vector2.zero;
			if (rangeXSlider != null)
			{
				rangeXSlider.value = 0;
			}
			if (rangeYSlider != null)
			{
				rangeYSlider.value = 0;
			}

			if (normal_rangeXLabel != null)
				normal_rangeXLabel.text = "0";
			if (normal_rangeYLabel != null)
				normal_rangeYLabel.text = "0";

			SetVisibleSearchNode(false);
		}

		public void onClickEffectSpineButton()
		{
			if (effectSpineParent != null)
			{
				var currentValue = EffectDrops.value;
				if (effectSpineList == null || effectSpineList.Length <= 0)
				{
					return;
				}

				if (effectSpineList.Length <= currentValue)
				{
					return;
				}

				Instantiate(effectSpineList[currentValue], effectSpineParent.transform);
			}
		}

		public void onChangeSkinOption()
		{
			if (spine == null)
			{
				ToastManager.On(100002517);
				return;
			}

			spine.SetSkin(SkinDrops.captionText.text);
			DrawSkillBoundary();
		}

		public void onChangeAniOption()
		{
			if (spine == null)
			{
				ToastManager.On(100002517);
				return;
			}
			spine.SetAnimation(AniDrops.captionText.text);

			RefreshPlayTime(AniDrops.captionText.text);
		}

		void InitSpineDrops()
		{
			SpineDrops.ClearOptions();
			spineNameList.Add("none");
			if (toggleUiPref)
			{
				for (int i = 0; i < uispineList.Length; i++)
				{
					spineNameList.Add(uispineList[i].name);
				}
			}
			else
			{
				for (int i = 0; i < spineList.Length; i++)
				{
					spineNameList.Add(spineList[i].name);
				}
			}


			SpineDrops.AddOptions(spineNameList);
		}

		void InitEffectDrops()
		{
			EffectDrops.ClearOptions();

			if (effectSpineList == null || effectSpineList.Length <= 0)
			{
				return;
			}

			List<string> effectList = new List<string>();
			for (var i = 0; i < effectSpineList.Length; i++)
			{
				var data = effectSpineList[i];
				if (data == null)
				{
					continue;
				}

				effectList.Add(data.name);
			}

			EffectDrops.AddOptions(effectList);
		}

		void InitSkinDrops()
		{
			SkinDrops.ClearOptions();

			List<Skin> skines = skData.Skins.ToList();
			IEnumerator<Skin> e = skines.GetEnumerator();
			List<string> strSkins = new List<string>();

			while (e.MoveNext())
			{
				strSkins.Add(e.Current.Name);
			}

			SkinDrops.AddOptions(strSkins);
		}

		void InitAniDrops()
		{
			AniDrops.ClearOptions();

			List<Spine.Animation> anims = skData.Animations.ToList();
			IEnumerator<Spine.Animation> e = anims.GetEnumerator();
			animDictionary.Clear();

			while (e.MoveNext())
			{
				animDictionary.Add(e.Current.Name, e.Current);
			}

			AniDrops.AddOptions(animDictionary.Keys.ToList());
		}

		void InitMonsterAniDrops()
		{
			MonsterAniDrops.ClearOptions();

			if (targetMonster == null)
			{
				return;
			}

			var skeletonAni = targetMonster.GetComponentInChildren<SkeletonAnimation>();
			if (skeletonAni == null)
			{
				return;
			}

			List<Spine.Animation> anims = skeletonAni.SkeletonDataAsset.GetSkeletonData(true).Animations.ToList();
			IEnumerator<Spine.Animation> e = anims.GetEnumerator();
			mAnimDictionary.Clear();

			while (e.MoveNext())
			{
				mAnimDictionary.Add(e.Current.Name, e.Current);
			}

			MonsterAniDrops.AddOptions(mAnimDictionary.Keys.ToList());
		}

		void InitMonsterDrops()
		{
			SBFunc.RemoveAllChildrens(monsterSpineParent.transform);

			MonsterDrops.ClearOptions();

			monsterList = ResourceManager.GetResources<GameObject>(SBDefine.ResourceFolder(eResourcePath.MonsterClonePath));
			if (monsterList == null || monsterList.Length <= 0)
			{
				return;
			}

			List<string> strList = new List<string>();
			strList.Add("none");

			for (var i = 0; i < monsterList.Length; i++)
			{
				strList.Add(monsterList[i].name);
			}

			MonsterDrops.AddOptions(strList);
		}

		void ClearOptions()
		{
			SpineDrops.ClearOptions();
			AniDrops.ClearOptions();
		}

		public void ToggleDrops()
		{
			btoggleSpine = !btoggleSpine;
			toggleSpine.isOn = btoggleSpine;
			layout.SetActive(btoggleSpine);
		}

		public void ToggleShadow()
		{
			btoggleSpine = !btoggleSpine;
			toggleSpine.isOn = btoggleSpine;
			layout.SetActive(btoggleSpine);
		}

		bool isContainAnimation(string _animName)
		{
			if (animDictionary == null || animDictionary.Count <= 0 || spine == null)
				return false;

			return animDictionary.ContainsKey(_animName) && spine != null;
		}
		
		bool isContainMonsterAnimation(string _animName)
		{
			if (mAnimDictionary == null || mAnimDictionary.Count <= 0 || mspine == null)
				return false;

			return mAnimDictionary.ContainsKey(_animName) && mspine != null;
		}

		public void onClickReplay()
		{
			//onChangeAniOption();
			var _animName = AniDrops.captionText.text;
			string _mAniName = MonsterAniDrops.captionText.text;

			if (isContainAnimation(_animName))
			{
				spine.SetAnimation(0, _animName, true).TimeScale = playAnimSpeed;
                isValueClickCheck = false;
				frameSlider.value = 0;
			}
			
			if (isContainMonsterAnimation(_mAniName))
			{
				mspine.SetAnimation(0, _mAniName, true).TimeScale = playAnimSpeed;
				isValueClickCheck = false;
				mFrameSlider.value = 0;
			}
		}

		public void onClickStop()
		{
			var _animName = AniDrops.captionText.text;
			string _mAniName = MonsterAniDrops.captionText.text;

			if (isContainAnimation(_animName))
			{
				spine.SetAnimation(0, _animName, true).TimeScale = 0f;
				isValueClickCheck = false;
				frameSlider.value = 0;
			}
			
			if (isContainMonsterAnimation(_mAniName) && mspine != null)
			{
				mspine.SetAnimation(0, _mAniName, true).TimeScale = 0f;
				isValueClickCheck = false;
				mFrameSlider.value = 0;
			}
		}

		void RefreshPlayTime(string _animName)
		{
			if (isContainAnimation(_animName) && playTimeLabel != null)
			{
				var animData = animDictionary[_animName];
				playTimeLabel.text = SBFunc.StrBuilder(animData.Duration, "f");
				sliderLabel.text = (frameSlider.value * animData.Duration).ToString();
			}
		}

		bool isValueClickCheck = false;

		public void onClickMonsterSliderValueChanged()
		{
			if (targetMonster == null)
			{
				return;
			}

			if (!isValueClickCheck)
			{
				mspine.SetSpineAnimFrame(0);
				isValueClickCheck = true;

				var _animName = MonsterAniDrops.captionText.text;
				if (isContainMonsterAnimation(_animName))
				{
					mspine.SetAnimation(0, MonsterAniDrops.captionText.text, true).TimeScale = 1f;
				}
			}

			var currentValue = mFrameSlider.value;
			SetMonsterAnimationTrackTime(currentValue);
		}
		
		public void onClickSliderValueChanged()
		{
			if (spine == null)
			{
				return;
			}

            if (!isValueClickCheck)
			{
				spine.SkeletonAni.Update(0);
				isValueClickCheck = true;

				var _animName = AniDrops.captionText.text;
				if (isContainAnimation(_animName))
				{
					spine.SetAnimation(0, AniDrops.captionText.text, true).TimeScale = 1f;
				}
			}

			var currentValue = frameSlider.value;
			SetAnimationTrackTime(currentValue);
		}

		private float lastFrame = 0f;
		void SetAnimationTrackTime(float _currentValue)
		{
			var _animName = AniDrops.captionText.text;
			if (isContainAnimation(_animName))
			{
				var animData = animDictionary[_animName];
				if (spine != null)
				{
					var curFrame = 0f;
					if (lastFrame == 0f)
					{
						lastFrame = _currentValue * animData.Duration;
						curFrame = lastFrame;
					}
					else
					{
						curFrame = lastFrame;
						lastFrame = _currentValue * animData.Duration;
						curFrame = lastFrame - curFrame;
					}
					spine.SkeletonAni.Update(curFrame);
					sliderLabel.text = (_currentValue * animData.Duration).ToString();
				}
			}
		}
		
		void SetMonsterAnimationTrackTime(float _currentValue)
		{
			var _animName = MonsterAniDrops.captionText.text;
			if (isContainMonsterAnimation(_animName))
			{
				var animData = mAnimDictionary[_animName];
				if (mspine != null)
				{
					var curFrame = 0f;
					if (lastFrame == 0f)
					{
						lastFrame = _currentValue * animData.Duration;
						curFrame = lastFrame;
					}
					else
					{
						curFrame = lastFrame;
						lastFrame = _currentValue * animData.Duration;
						curFrame = lastFrame - curFrame;
					}
					mspine.SetSpineAnimFrame(curFrame);
					mSliderLabel.text = (_currentValue * animData.Duration).ToString();
				}
			}
		}

		Spine.Animation GetAnimationData(string _animationName)
		{
			if (animDictionary.ContainsKey(_animationName))
			{
				return animDictionary[_animationName];
			}
			return null;
		}

		// 0.6
		// 0.5
		
		// 0.4
		// 0.35

		const float BattleTileX = 0.6f;
		const float BattleTileY = 0.5f;

		void DrawSkillBoundary()
		{
			if (SkinDrops == null) { return; }

			var charData = CharBaseData.GetDataBySkin(SkinDrops.captionText.text);
			if (charData == null)
			{
				return;
			}

			var skillData = charData.SKILL1;
			var normalSkillData = charData.NORMAL_SKILL;

			drawSkill(skillData, false, true, skill_line, skill_rangeXLabel, skill_rangeYLabel, new Vector2(0, -1.5f));//스킬 범위 그리기
			drawSkill(normalSkillData, false, false, normal_line, normal_rangeXLabel, normal_rangeYLabel, new Vector2(0, -1.5f));//평타 범위 그리기

            var effectList = GetTotalSkillEffectData(skillData);

            bool isRelativePos = false;//start_position 값이 1이면 자신 위치 기준, 2이면 피격 대상 위치 기준
			Vector2 skillEffectPos = Vector2.zero;
			if (effectList != null && effectList.Count > 0)
			{
				for (var i = 0; i < effectList.Count; i++)
				{
					var value = effectList[i];
					if (value == null)
						continue;
					//var startPos = value.START_POSITION;
					//if (startPos == eSkillEffectStartType.Enemy)//스킬이 피격 대상 기준이면
					//{
					//	isRelativePos = true;
					//	break;
					//}
				}
			}

			skillEffectPos = isRelativePos ? new Vector2(3.75f, -1.5f) : new Vector2(0, -1.5f);

			//drawSkill(SkillEffectData.GetEffectData(skillData), true, effect_skill_line, skillEffectPos, effect_skill_sprite);//이펙트 스킬 라인 그리기
			//drawSkill(SkillEffectData.GetEffectData(normalSkillData), false, effect_normal_line, new Vector2(3.75f, -1.5f), effect_normal_sprite);//이펙트 평타 라인 그리기
		}

		List<SkillEffectData> GetTotalSkillEffectData(SkillCharData skillData)
		{
			List<SkillEffectData> tempList = new List<SkillEffectData>();
			
			//if (skillData != null)
			//	return SkillEffectData.GetDataByGroup(skillData.EFFECT_GROUP);
			return tempList;
		}

		SkillEffectData GetSkillEffectData(SkillCharData skillData)
		{
			//if (skillData != null)
			//{
			//	var effectList = SkillEffectData.GetDataByGroup(skillData.EFFECT_GROUP);
			//	if (effectList == null || effectList.Count <= 0)
			//		return null;
			//	return effectList[0];
			//}
			return null;
		}

		Vector2 GetRangeBySkillData(SkillCharData data)
		{
			Vector2 tempVec = Vector2.zero;
			if (data != null)
			{
				tempVec.x = data.RANGE_X;
				tempVec.y = data.RANGE_Y;
				return tempVec;
			}
			return tempVec;
		}

		void drawSkill(SkillCharData skillData, bool isMonster, bool isSkill1, LineRenderer targetLine, TMPro.TextMeshProUGUI targetXLabel, TMPro.TextMeshProUGUI targetYLabel, Vector2 modifyPos)
		{
			if (skillData != null)
			{
				var RangeX = skillData.RANGE_X;
				var RangeY = skillData.RANGE_Y;

				if (targetXLabel != null)
					targetXLabel.text = RangeX.ToString();
				if (targetYLabel != null)
					targetYLabel.text = RangeY.ToString();

				drawEllipse(targetLine, RangeX, RangeY, modifyPos);

				if (isSkill1 && !isMonster)
				{
					skillRange.x = RangeX;
					skillRange.y = RangeY;
					var projectileKey = skillData.SUMMON_KEY;
					var projectileData = SkillSummonData.Get(projectileKey.ToString());
					EffectLabel.text = projectileData != null ? projectileData.SKILL_EFFECT_RSC_KEY.ToString() : "none";
					RefreshSliderValue();

					if (rangeResetButton != null)
					{
						rangeResetButton.onClick.AddListener(() => onClickRewind(RangeX, RangeY));
					}
				}
			}
			else
			{
				if (isSkill1 && !isMonster)
				{
					EffectLabel.text = "none";
					skillRange = Vector2.zero;
					RefreshSliderValue();
				}

				if (targetXLabel != null)
					targetXLabel.text = "0";
				if (targetYLabel != null)
					targetYLabel.text = "0";
				if (targetLine != null)
					targetLine.positionCount = 0;
			}
		}

		void drawSkill(SkillEffectData skillData, bool isSkill, LineRenderer targetLine, Vector2 modifyPos, SpriteRenderer targetSprite)
		{
			if (skillData != null)
			{
				if (isSkill)
				{
					//switch ((int)skillData.DIRECTION)
					//{
					//	case 0:
					//		drawEllipse(targetLine, skillData.RANGE_X, skillData.RANGE_Y, modifyPos);
					//		break;
					//	case 1:
					//		drawEllipse(targetLine, skillData.RANGE_X, skillData.RANGE_Y, modifyPos, 180);
					//		break;
					//	case 2:
					//		drawEllipse(targetLine, skillData.RANGE_X, skillData.RANGE_Y, modifyPos, -180);
					//		break;
					//}
				}
				else
				{
					drawEllipse(targetLine, skillData.EXPLOSION_X, skillData.EXPLOSION_Y, modifyPos);
				}

				//SetEffectCircleSprite(isSkill, targetSprite, skillData.RANGE_X, skillData.RANGE_Y, modifyPos, (int)skillData.DIRECTION);
			}
			else
			{
				if (targetSprite != null)
					targetSprite.gameObject.SetActive(false);
				targetLine.positionCount = 0;
			}
		}

		void drawEllipse(LineRenderer targetLine, float scaleX, float scaleY, Vector2 modifyPos, int angle = 360)
		{
			var _ellipsePoints = new Vector3[21];

			for (int i = 0; i <= 20; i++)
			{
				float _angle = ((float)i / (float)20) * angle * Mathf.Deg2Rad;
				float x = Mathf.Sin(_angle) * scaleX;
				float y = Mathf.Cos(_angle) * scaleY;

				_ellipsePoints[i] = new Vector3((x + modifyPos.x) * BattleTileX, (y + modifyPos.y) * BattleTileY, -1f);
			}
			targetLine.positionCount = 0;
			targetLine.positionCount = 22;

			for (int i = 0; i < _ellipsePoints.Length; i++)
			{
				targetLine.SetPosition(i, _ellipsePoints[i]);
			}
			targetLine.SetPosition(21, targetLine.GetPosition(0));
		}

		public void onClickChangeMonster()
		{
			SBFunc.RemoveAllChildrens(monsterSpineParent.transform);
			var selectedText = MonsterDrops.captionText.text;
			if (selectedText == "none")
			{
				monster_skill_line.positionCount = 0;
				monster_normal_line.positionCount = 0;

				MonsterAniDrops.ClearOptions();
				targetMonster = null;
				return;
			}
			else
			{
				var currentValue = MonsterDrops.value;//"none"
				var modifyIndex = currentValue - 1;

				if (monsterList != null && monsterList.Length > modifyIndex)
				{
					var prefab = monsterList[modifyIndex];
					targetMonster = Instantiate(prefab, monsterSpineParent.transform);
					mspine = targetMonster.GetComponent<BattleMonsterSpine>();

					if (mspine == null )
					{
						mspine = targetMonster.AddComponent<BattleMonsterSpine>();
					}

					var monsterSkinName = MonsterDrops.captionText.text;
					var currentMonsterData = MonsterBaseData.GetMonsterDataByImageName(monsterSkinName);
					BattleMonsterData mData = new BattleMonsterData(eStageType.UNKNOWN);
					mData.BaseData = currentMonsterData;
					mspine.SetData(mData);


					if (currentMonsterData != null)
					{
						var normalSkill = currentMonsterData.NORMAL_SKILL;
						var skill1 = currentMonsterData.SKILL1;

						drawSkill(skill1, true, true, monster_skill_line, null, null, new Vector2(3.75f, -1.5f));
						drawSkill(normalSkill, true, false, monster_normal_line, null, null, new Vector2(3.75f, -1.5f));

						InitMonsterAniDrops();
					}
				}
			}
		}
		
		public void onClickMonsterAnimation()
		{
			if (targetMonster == null)
			{
				return;
			}

			var skeletonAni = targetMonster.GetComponentInChildren<SkeletonAnimation>();
			if (skeletonAni == null)
			{
				return;
			}

			skeletonAni.AnimationState.SetAnimation(0, MonsterAniDrops.captionText.text, true);
		}

		void RefreshSliderValue()
		{
			if (rangeXSlider != null)
			{
				rangeXSlider.value = skillRange.x;
			}
			if (rangeYSlider != null)
			{
				rangeYSlider.value = skillRange.y;
			}
		}

		public void onClickChangeSliderX()
		{
			if (rangeXSlider != null)
			{
				var currentValue = rangeXSlider.value;
				var floatValue = (float)Math.Round(currentValue, 2);

				skillRange.x = floatValue;
				skill_rangeXLabel.text = skillRange.x.ToString();
				drawEllipse(skill_line, skillRange.x, skillRange.y, new Vector2(0, -1.5f));
			}
		}

		public void onClickChangeSliderY()
		{
			if (rangeYSlider != null)
			{
				var currentValue = rangeYSlider.value;
				var floatValue = (float)Math.Round(currentValue, 2);

				skillRange.y = floatValue;
				skill_rangeYLabel.text = skillRange.y.ToString();
				drawEllipse(skill_line, skillRange.x, skillRange.y, new Vector2(0, -1.5f));
			}
		}

		public void onClickRewind(float rangeX, float rangeY)
		{
			//Debug.Log("rangeX :" + rangeX);
			//Debug.Log("rangeY :" + rangeY);

			if (rangeXSlider != null)
			{
				rangeXSlider.value = rangeX;
			}
			if (rangeYSlider != null)
			{
				rangeYSlider.value = rangeY;
			}
		}

		public void onClickShowRangePopup(bool _isRangeX)//true 면 X좌표, false면 Y 좌표
		{
			var popup = PopupManager.OpenPopup<SpineToolEditRangePopup>();
			popup.SetController(this, _isRangeX);
		}

		public void SetModifyRange(bool _isRangeX, float _value)
		{
			if (_value < 0)
				_value = 0f;
			if (_value > 10)
				_value = 10f;

			if (_isRangeX)
			{
				if (rangeXSlider != null)
				{
					rangeXSlider.value = _value;
				}
			}
			else
			{
				if (rangeYSlider != null)
				{
					rangeYSlider.value = _value;
				}
			}
		}

		public void onClickToggleSkillRangeLine()
		{
			var check = toggleSkillRange.isOn;
			if (skill_line != null)
				skill_line.gameObject.SetActive(check);
			if (monster_skill_line != null)
				monster_skill_line.gameObject.SetActive(check);
		}
		public void onClickToggleNormalRangeLine()
		{
			var check = toggleNormalRange.isOn;
			if (normal_line != null)
				normal_line.gameObject.SetActive(check);
			if (monster_normal_line != null)
				monster_normal_line.gameObject.SetActive(check);
		}
		public void onClickToggleEffectSkillRangeLine()
		{
			var check = toggleEffectSkillRange.isOn;
			if (effect_skill_line != null)
				effect_skill_line.gameObject.SetActive(check);
			if (effect_skill_sprite != null)
				effect_skill_sprite.gameObject.SetActive(check);
		}
		public void onClickToggleEffectNormalRangeLine()
		{
			var check = toggleEffectNormalRange.isOn;
			if (effect_normal_line != null)
				effect_normal_line.gameObject.SetActive(check);
			if (effect_normal_sprite != null)
				effect_normal_sprite.gameObject.SetActive(check);
		}

		/*
		 * All = 0,
		 * Front = 1,
		 * Back = 2
		 */
		void SetEffectCircleSprite(bool isSKill, SpriteRenderer targetSprite, float scaleX, float scaleY, Vector2 pos, int dir)
		{
			if (targetSprite == null) { return; }

			targetSprite.gameObject.SetActive(true);

			if (isSKill)
			{
				switch (dir)
				{
					case 0:
						targetSprite.sprite = circleFrame[0];
						targetSprite.flipX = false;
						break;
					case 1:
						targetSprite.sprite = circleFrame[1];
						targetSprite.flipX = false;
						break;
					case 2:
						targetSprite.sprite = circleFrame[2];
						targetSprite.flipX = true;
						break;
				}
			}

			Vector2 modifyPos = (pos == new Vector2(3.75f, -1.5f)) ? new Vector2(2.25f, -0.75f) : new Vector2(0f, -0.75f);

			targetSprite.gameObject.transform.position = modifyPos;
			targetSprite.size = new Vector2(scaleX * 1.2f, scaleY );
		}

		//노말 스킨은 너무 많아서 검색기능 만듦
		void SetVisibleSearchNode(bool _isVisible)
        {
			if (normalSearchDropNode != null)
				normalSearchDropNode.SetActive(_isVisible);
			SkinDrops.gameObject.SetActive(!_isVisible);
        }

		
		void SetNormalGradeDropsData()
        {
			if (SkinDrops == null)
				return;

			if (normalGradeDrops == null)
				return;

			normalGradeDrops.ClearOptions();

			var options = SkinDrops.options;
			if (options == null || options.Count <= 0)
				return;

			List<string> gradeString = new List<string>();

			for(int i = 0; i < options.Count; i++)
            {
				var textOption = options[i].text;
				var substrText = textOption.Substring(textOption.IndexOf('/') + 1);

				if (substrText == "default")
					continue;

				var splitData = substrText.Split('_');
				var grade = splitData[0];
				var indexStr = splitData[1];
				gradeString.Add(splitData[0]);

				if(!searchNormalDataDic.ContainsKey(grade))
                {
					searchNormalDataDic[grade] = new List<string>();
				}
				searchNormalDataDic[grade].Add(indexStr);
			}

			gradeString = gradeString.Distinct().ToList();//중복 제거
			normalGradeDrops.AddOptions(gradeString);

			SetDataNormalStringIndexDrops();//인덱스 갱신
		}

		public void onClickChangeNormalGradeValue()//등급 선택 값 바뀌면 인덱스 갱신
        {
			//var context = normalGradeDrops.captionText.text;
			//Debug.Log("value Change :" + context);
			SetDataNormalStringIndexDrops();
		}

		void SetDataNormalStringIndexDrops()
        {
			if (normalNumberDrops == null)
				return;

			var currentGrade = normalGradeDrops.captionText.text;
			var isContain = searchNormalDataDic.ContainsKey(currentGrade);

			normalNumberDrops.ClearOptions();

			if (!isContain)
				return;

			var optionList = searchNormalDataDic[currentGrade];
			normalNumberDrops.AddOptions(optionList);
		}

		public void onClickNormalSearchNode()
        {
			if (SkinDrops == null)
				return;

			var skinOptions = SkinDrops.options;
			if (skinOptions == null || skinOptions.Count <= 0)
				return;

			var gradeText = normalGradeDrops.captionText.text;
			var strIndexText = normalNumberDrops.captionText.text;
			var resultStr = SBFunc.StrBuilder("dragon/", gradeText, "_", strIndexText);

			var checkIndex = -1;
			for (int i = 0; i < skinOptions.Count; i++)
			{
				var optionText = skinOptions[i].text;
				if (optionText == resultStr)
                {
					checkIndex = i;
					break;
                }
            }

			if (checkIndex < 0)
				return;

			SkinDrops.value = checkIndex;
		}

		public void onClickSpeedArrow(bool _isLeft)
        {
			if (_isLeft)
				playAnimSpeed -= 0.1f;
			else
				playAnimSpeed += 0.1f;

			if (playAnimSpeed <= 0)
				playAnimSpeed = 0.1f;

			if (playAnimSpeed > 5.0f)
				playAnimSpeed = 5.0f;

			if(playSpeedLabel != null)
            {
				playSpeedLabel.text = SBFunc.StrBuilder("x", Math.Round(playAnimSpeed, 2).ToString());
            }
        }

		public void onClickSpeedReset()
        {
			playAnimSpeed = 1.0f;
			if (playSpeedLabel != null)
			{
				playSpeedLabel.text = SBFunc.StrBuilder("x", Math.Round(playAnimSpeed, 2).ToString());
			}
		}
	}
}

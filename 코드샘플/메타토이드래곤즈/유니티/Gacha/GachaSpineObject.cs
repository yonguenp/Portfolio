using Coffee.UIEffects;
using Coffee.UIExtensions;
using DG.Tweening;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
	public class GachaSpineObject : MonoBehaviour
	{
		[SerializeField] private GameObject bottomObject = null;
		[SerializeField] private Text nametext;
		[SerializeField] private SkeletonGraphic spine;
		[SerializeField] private GameObject[] effects;
		[SerializeField] private GameObject[] result_effects;
		[SerializeField] private GameObject[] grade;

		[SerializeField] private SkeletonDataAsset petsAsset;

		[SerializeField] private GameObject newIcon = null;

		[SerializeField] private Animator spineAnim = null;

		[SerializeField] GameObject cover;
		[SerializeField] UIGradient gradient;
		[SerializeField] UIDissolve dissolve;

		Coroutine coverAction = null;
		int curGrade = -1;
		bool needAnimation = true;
		bool useDissolve = true;
		Sequence spineSeq = null;
		List<Tweener> stopContainer = new();

		private int capsuleUIIndex = 0;//캡슐 스파인에 세팅된 UI 인덱스를 세팅
        private void OnDisable()
        {
			SkipCoverAnimation();

			StopSpineSequence();

			if (stopContainer == null)
				stopContainer = new();
			else
            {
				for(int i = 0, count = stopContainer.Count; i < count; ++i)
                {
					if (stopContainer[i] == null)
						continue;

					stopContainer[i].Kill();
				}
			}
			stopContainer.Clear();
		}
		private void OnEnable()
		{
			if (stopContainer == null)
				stopContainer = new();

			if (curGrade > 0 && needAnimation)
				OnShow();

		}
        public void Init(int id, string _name, int g, eGachaType _type, string _skin, bool dissolveAnim = true, bool isNew = false, int _capsuleUIIndex = 0)
		{
			dissolve.effectFactor = 1.0f;
			cover.SetActive(true);

			string idleName = "";
			curGrade = g;

			nametext.text = _name;

			switch (_type)
			{
				case eGachaType.DRAGON:
				{
					var baseData = CharBaseData.Get(id);
					if (baseData != null)
						spine.skeletonDataAsset = baseData.GetSkeletonDataAsset();

					idleName = "idle_ani1";

					if (newIcon != null)
						newIcon.SetActive(isNew);
				}
				break;
				case eGachaType.PET:
					spine.skeletonDataAsset = petsAsset;
					idleName = "stop";

					Vector3 pos = spine.transform.localPosition;
					pos.y = -20.0f;
					spine.transform.localPosition = pos;
					stopContainer.Add(spine.transform.DOLocalMoveY(0.0f, 1.0f + (SBFunc.RandomValue * 0.5f)).SetLoops(-1, LoopType.Yoyo));
					
					if (newIcon != null)
						newIcon.SetActive(false);
					break;
			}
			bottomObject.GetComponent<RectTransform>().anchoredPosition = _type == eGachaType.DRAGON ? new Vector2(0, -195f) : new Vector2(0, -105f);
			bottomObject.gameObject.SetActive(false);
			spine.transform.localScale = new Vector3(2,2,2);

			var data = spine.skeletonDataAsset.GetSkeletonData(true).FindSkin(_skin);
			if(data != null)
            {
				spine.initialSkinName = _skin;
				spine.gameObject.SetActive(true);
				spine.Initialize(true);
				spine.Skeleton.SetSkin(_skin);
				spine.AnimationState.SetAnimation(0, idleName, true);
			}
			
			useDissolve = dissolveAnim;
			spineAnim.SetInteger("isShow", 0);
			capsuleUIIndex = _capsuleUIIndex;
		}

		public void OnShow()
        {
			coverAction = StartCoroutine(CoverAction());
			needAnimation = false;
		}

		public void SkipCoverAnimation(bool force = false, bool isSkipped = false)
		{
			if (coverAction != null)
			{
				StopCoroutine(coverAction);
			}
			coverAction = null;
			cover.SetActive(false);
			nametext.gameObject.SetActive(true);
			if (curGrade > 0)
				grade[curGrade - 1].SetActive(true);

			foreach (var effect in effects)
			{
				effect.gameObject.SetActive(false);
			}
			foreach (var effect in result_effects)
			{
				effect.gameObject.SetActive(false);
			}

			GameObject particleObj = null;
			switch (curGrade)
			{
				case 1:
					particleObj = effects[0];
					break;
				case 2:
					particleObj = result_effects[0];
					break;
				case 3:
					particleObj = result_effects[1];
					break;
				case 4:
					particleObj = result_effects[2];
					break;
				case 5:
					particleObj = result_effects[3];
					break;
			}

			if (particleObj != null)
			{
				particleObj.SetActive(true);
				particleObj.GetComponent<UIParticle>().Clear();
				particleObj.GetComponent<UIParticle>().Play();
			}

			//skip 일 때 분기 처리 해야하는지 확인 할 것
			if(isSkipped)
				spineAnim.SetInteger("isShow", 0);//idle
			else
			{
				spineAnim.SetInteger("isShow", curGrade);//드래곤 anim 출력
			}
			
			if(force)
            {
				needAnimation = false;
			}
		}

		public void StopSpineSequence()
        {
			if (spineSeq != null)
				spineSeq.Kill();

			spineSeq = null;
		}

		IEnumerator CoverAction()
		{
			foreach (var effect in effects)
			{
				effect.gameObject.SetActive(false);
			}
			foreach (var effect in result_effects)
			{
				effect.gameObject.SetActive(false);
			}
			
			spine.transform.localScale = Vector3.zero;
			grade[curGrade - 1].SetActive(false);
			nametext.gameObject.SetActive(false);
			cover.SetActive(useDissolve);
			dissolve.effectFactor = 0.0f;
			float showDelay = 0.5f;
			float delay = 0.0f;
			float discoverDelay = 0.5f;
			Color color = Color.white;
			switch (curGrade)
			{
				case 1:
					ColorUtility.TryParseHtmlString("#c8c5c2", out color);
					gradient.color1 = color;
					gradient.color2 = color;
					dissolve.color = color;
					delay = 0.0f;
					discoverDelay = 0.1f;
					showDelay = 0.1f;
					break;
				case 2:
					ColorUtility.TryParseHtmlString("#40be2a", out color);
					gradient.color1 = color;
					gradient.color2 = color;
					dissolve.color = color;
					delay = 0.0f;
					discoverDelay = 0.1f;
					showDelay = 0.2f;
					break;
				case 3:
					ColorUtility.TryParseHtmlString("#40be2a", out color);
					gradient.color1 = color;
					ColorUtility.TryParseHtmlString("#1296f2", out color);
					gradient.color2 = color;
					ColorUtility.TryParseHtmlString("#1296f2", out color);
					dissolve.color = color;
					delay = 1.0f;
					discoverDelay = 0.8f;
					showDelay = 0.5f;
					break;
				case 4:
					ColorUtility.TryParseHtmlString("#1296f2", out color);
					gradient.color1 = color;
					ColorUtility.TryParseHtmlString("#a53efe", out color);
					gradient.color2 = color;
					ColorUtility.TryParseHtmlString("#a53efe", out color);
					dissolve.color = color;
					delay = 1.5f;
					discoverDelay = 1.5f;
					showDelay = 0.5f;
					break;
				case 5:
					ColorUtility.TryParseHtmlString("#a53efe", out color);
					gradient.color1 = color;
					ColorUtility.TryParseHtmlString("#ff9600", out color);
					gradient.color2 = color;
					ColorUtility.TryParseHtmlString("#ff9600", out color);
					dissolve.color = color;
					delay = 2.0f;
					discoverDelay = 2.0f;
					showDelay = 0.5f;
					break;
			}

			dissolve.effectFactor = 0.0f;
			
			yield return spine.transform.DOScale(1.5f, showDelay).SetEase(Ease.OutBounce).WaitForCompletion();

			bool down = false;
			while (delay > 0.0f && useDissolve)
			{
				float delta = Time.deltaTime;
				delay -= delta;
				if (down)
				{
					gradient.offset -= delta;
					if (gradient.offset < -0.3f)
						down = false;
				}
				else
				{
					gradient.offset += delta;
					if (gradient.offset > 0.3f)
						down = true;
				}

				yield return new WaitForEndOfFrame();
			}

			float perDiscover = 1 / discoverDelay;
			bool boom = false;
			while (discoverDelay > 0.0f && useDissolve)
			{
				discoverDelay -= Time.deltaTime;
				dissolve.effectFactor += Time.deltaTime * perDiscover;

				if(!boom && discoverDelay < 0.5f)
                {
					boom = true;
				}
				yield return new WaitForEndOfFrame();
			}

			GameObject particleObj = null;
			switch (curGrade)
			{
				case 1:
					particleObj = effects[0];
					break;
				case 2:
					particleObj = effects[0];
					break;
				case 3:
					particleObj = effects[1];
					break;
				case 4:
					particleObj = effects[2];
					break;
				case 5:
					particleObj = effects[3];
					break;
			}

			if (particleObj != null)
			{
				particleObj.SetActive(true);
				particleObj.GetComponent<UIParticle>().Clear();
				particleObj.GetComponent<UIParticle>().Play();
			}

			cover.SetActive(false);
			nametext.gameObject.SetActive(true);
			grade[curGrade - 1].SetActive(true);
		}

		public void SendShowEffect()
        {
			if(curGrade > 2)//등급이 레어 이상일 때만 쏘는 걸로.
				GachaEvent.CapsuleAnimationShowEffect(capsuleUIIndex);
        }

		public void PlaySfxSound(string _sfxName)
        {
			SoundManager.Instance.PlaySFX(_sfxName);
        }
	}
}
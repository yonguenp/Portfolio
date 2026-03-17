using Coffee.UIExtensions;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
	public class GachaPortraitObject: MonoBehaviour
	{
		[SerializeField] private Text nametext;
		[SerializeField] private Image thumbnail;
		[SerializeField] private Image element;
		[SerializeField] private Image bg;
		[SerializeField] private SlotFrameController frame;
		[SerializeField] private GameObject[] grade;
		[SerializeField] private GameObject portrait;
		[SerializeField] private GameObject[] effects;

		bool needAnimation = true;
		Coroutine coverAction = null;
		int curGrade = -1;

		private void OnDisable()
		{
			SkipCoverAnimation();
		}

		private void OnEnable()
		{
			if (curGrade > 0 && needAnimation)
				OnShow();
		}

		public void Init(string _name, Sprite _thumbnail, Sprite _bg, int _grade, Sprite _element)
		{
			curGrade = _grade;
			nametext.text = _name;
			thumbnail.sprite = _thumbnail;
			grade[_grade - 1].SetActive(true);
			element.sprite = _element;
			bg.sprite = _bg;

			if (frame != null)
				frame.SetColor(_grade);

			portrait.transform.localScale = Vector3.zero;
		}

		public void SkipCoverAnimation(bool force = false)
		{
			foreach (var effect in effects)
			{
				effect.gameObject.SetActive(false);
			}

			if (coverAction != null)
			{
				StopCoroutine(coverAction);
			}
			coverAction = null;			
			nametext.gameObject.SetActive(true);
			if (curGrade > 0)
				grade[curGrade - 1].SetActive(true);

			portrait.transform.localScale = Vector3.one;

			if (force)
			{
				needAnimation = false;
			}
		}

		public void OnShow()
		{
			coverAction = StartCoroutine(CoverAction());
			needAnimation = false;
		}

		IEnumerator CoverAction()
		{
			foreach (var effect in effects)
			{
				effect.gameObject.SetActive(false);
			}

			portrait.transform.localScale = Vector3.zero;
			if(curGrade > 0)
				grade[curGrade - 1].SetActive(false);
			nametext.gameObject.SetActive(false);

			float showDelay = 0.5f;
			switch (curGrade)
			{
				case 1:
					showDelay = 0.1f;
					break;
				case 2:
					showDelay = 0.2f;
					break;
				case 3:
					showDelay = 0.5f;
					break;
				case 4:
					showDelay = 0.5f;
					break;
				case 5:
					showDelay = 0.5f;
					break;
			}

			yield return portrait.transform.DOScale(1.0f, showDelay).SetEase(Ease.OutBounce).WaitForCompletion();

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

			float delay = 0.5f;
			while (delay > 0.0f)
			{
				delay -= Time.deltaTime;

				yield return new WaitForEndOfFrame();
			}
			nametext.gameObject.SetActive(true);
			grade[curGrade - 1].SetActive(true);
		}
	}
}

using Coffee.UIEffects;
using Coffee.UIExtensions;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class PartCompoundResultPopup : Popup<PartPopupData>
    {
        [SerializeField]
        PartSlotFrame partSlot = null;

        [SerializeField]
        Text itemNameLabel = null;

        [SerializeField]
        Text itemOptionTypeLabel = null;

        [SerializeField]
        Text itemOptionValueLabel = null;

        [SerializeField] private GameObject[] prev_effects;
        [SerializeField] private GameObject[] effects;
        [SerializeField] private GameObject[] result_effects;

		int curGrade = -1;
		Coroutine co = null;

		bool buttonLock = false;
		public override void InitUI() 
        {
            this.SetData();
        }

        private void OnDisable()
        {
			if (co != null)
			{
				StopCoroutine(co);
			}
			co = null;
		}

        void SetData()
        {
            if (Data.Rewards == null || Data.Rewards.Count < 1)
                return;

            int partTag = 0;
            for (int i = 0, count = Data.Rewards.Count; i < count; ++i)
            {
                if (Data.Rewards[i] == null)
                    continue;

                partTag = Data.Rewards[i].ItemNo;
                break;
            }

            partSlot.SetPartSlotFrame(partTag, 0);
            RefreshUI(partTag);

			CancelInvoke("ButtonUnlock");
			buttonLock = true;
			Invoke("ButtonUnlock", 0.5f);
		}
		void ButtonUnlock()
		{
			CancelInvoke("ButtonUnlock");
			buttonLock = false;
		}
		void RefreshUI(int partTag)
        {
            var partData = User.Instance.PartData.GetPart(partTag);
            if (partData == null)
                return;

            var partDesignData = partData.GetItemDesignData();
            if (partDesignData == null)
                return;

            var partBaseData = partData.GetPartDesignData();
            if (partBaseData == null)
                return;

            var partName = partDesignData.NAME;
            var stat_type = partBaseData.STAT_TYPE;
            var value_type = partBaseData.VALUE_TYPE;
            var value = partBaseData.VALUE;

            if (itemNameLabel != null)
            {
                itemNameLabel.text = partName;
            }

            if (itemOptionTypeLabel != null)
            {
                itemOptionTypeLabel.text = SBFunc.StrBuilder("[", StatTypeData.GetDescStringByStatType(stat_type),"]");
            }

            if (itemOptionValueLabel != null)
            {
                var str = SBFunc.StrBuilder("+", value);

                if (value_type.ToUpper() == "PERCENT")
                {
                    str += "%";
                }

                itemOptionValueLabel.text = str;
            }

			curGrade = partBaseData.GRADE;

			co = StartCoroutine(effectAction());
		}
		IEnumerator effectAction()
		{
			foreach (var effect in prev_effects)
			{
				effect.gameObject.SetActive(false);
			}
			foreach (var effect in effects)
			{
				effect.gameObject.SetActive(false);
			}
			foreach (var effect in result_effects)
			{
				effect.gameObject.SetActive(false);
			}

			Color color = Color.white;

			//float discoverDelay = 0.05f;
			//if (prev_effects[0] != null)
			//{
			//	prev_effects[0].SetActive(true);
			//	prev_effects[0].GetComponent<UIParticle>().Clear();
			//	prev_effects[0].GetComponent<UIParticle>().Play();
			//}

			//bool boom = false;
			//while (discoverDelay > 0.0f)
			//{
			//	discoverDelay -= Time.deltaTime;
			//	if (!boom && discoverDelay < 0.5f)
			//	{
			//		if (prev_effects[0] != null)
			//		{
			//			prev_effects[0].GetComponent<UIParticle>().Stop();
			//		}

			//		if (prev_effects[1] != null)
			//		{
			//			prev_effects[1].SetActive(true);
			//			prev_effects[1].GetComponent<UIParticle>().Clear();
			//			prev_effects[1].GetComponent<UIParticle>().Play();
			//		}

			//		boom = true;
			//	}
			//	yield return new WaitForEndOfFrame();
			//}
			yield return new WaitForEndOfFrame();
			if (prev_effects[1] != null)
			{
				prev_effects[1].SetActive(true);
				prev_effects[1].GetComponent<UIParticle>().Clear();
				prev_effects[1].GetComponent<UIParticle>().Play();
			}

			GameObject particleObj = null;
			switch (curGrade)
			{
				case 1:
					particleObj = null;
					break;
				case 2:
					particleObj = result_effects[0];
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
		}

		public void OnClosePartCompoundResultPopup()
		{
			if (buttonLock)
				return;

			ClosePopup();
		}
	}
}

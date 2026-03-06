using DG.Tweening;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class DragonCompoundResultSlot : MonoBehaviour
    {
        [SerializeField]
        private GameObject nodeNew = null;
        [SerializeField]
        private GameObject[] gradeNodes = null;
        [SerializeField]
        private Text labelName = null;
        [SerializeField]
        private GameObject dataContainer = null;
        [SerializeField]
        private GameObject cardEffectPrefab = null;
        [SerializeField]
        private GameObject dragonEffectPrefab = null;

        [SerializeField]
        GameObject effectNode = null;

        [SerializeField]
        UIDragonSpine spine = null;
        [SerializeField]
        DragonCardFrame cardFrame = null;

        private bool isNew = false;
        private bool isSuccess = false;
        private CharBaseData baseData = null;

        public void SetData(DragonCompoundInfoData _data)
        {
            isNew = _data.isNew;
            isSuccess = _data.isSuccess;
            baseData = _data.DragonCharBaseData;

            if(baseData == null)
            {
                Debug.LogWarning("기본 캐릭터 정보 누락 : dragonID " + _data.dragonID);
                return;
            }

            if (nodeNew != null)
            {
                nodeNew.SetActive(isNew);
            }

            if (gradeNodes != null)
            {
                var count = gradeNodes.Length;
                for (var i = 0; i < count; ++i)
                {
                    var curNode = gradeNodes[i];
                    if (curNode == null)
                    {
                        continue;
                    }

                    curNode.SetActive((i + 1) == baseData.GRADE);
                }
            }

            if (labelName != null)
            {
                labelName.text = StringData.GetStringByStrKey(baseData._NAME);
            }

            if (spine != null)
                spine.gameObject.SetActive(false);

            if (cardFrame != null)
                cardFrame.gameObject.SetActive(false);

            InitSpineAndCard();

            if (isNew)
            {
                MakeDragonSpine();

                ToastManager.On(100000418);
            }
            else
            {
                var dragonEffect = MakeDragonEffect();
                var dragonNode = MakeDragonSpine();

                var mat = spine.GetSpineMaterial();
                var tempSeq = DOTween.Sequence();
                tempSeq.OnStart(() =>
                {
                    if (mat != null)
                        mat.SetFloat("_FillPhase", 0f);
                }).AppendCallback(() => {
                    var mat = spine.GetSpineMaterial();
                    if (mat != null)
                        mat.DOFloat(1f, "_FillPhase", 1f);
                }).AppendInterval(1f).AppendInterval(0.3f)
                .Append(dragonNode.GetComponent<RectTransform>().DOScale(new Vector3(1.6f, 1.6f, 1.6f), 0.3f))
                .Append(dragonNode.GetComponent<RectTransform>().DOScale(new Vector3(0f, 0f, 0f), 0.2f)).AppendCallback(() =>
                {
                    if (mat != null)
                        mat.SetFloat("_FillPhase", 0f);
                    dragonNode.SetActive(false);
                }).AppendCallback(() => {
                    var cardNode = MakeDragonCard();
                    cardNode.GetComponent<RectTransform>().localScale = new Vector3(0, 0, 0);

                    var tempSeq2 = DOTween.Sequence();
                    tempSeq2.Append(cardNode.GetComponent<RectTransform>().DOScale(new Vector3(1, 1, 1), 0.3f)).AppendCallback(() =>
                    {
                        if (cardNode != null)
                            MakeDragonCardEffect(cardNode.transform);
                    }).Play();
                }).Play();
            }
        }
        GameObject MakeDragonSpine()
        {
            if (spine != null)
            {
                spine.SetData(baseData);
                spine.gameObject.SetActive(true);
            }

            return spine.gameObject;
        }

        void InitSpineAndCard()
        {
            if (spine != null)
            {
                spine.gameObject.SetActive(false);
                spine.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
            }
                
            if (cardFrame != null)
            {
                cardFrame.gameObject.SetActive(false);
                cardFrame.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
            }
        }

        GameObject MakeDragonCard()
        {
            var card = new UserDragonCard();
            var strArray = new int[] { 0, int.Parse(baseData.KEY.ToString()) };
            var parseJarr = JArray.FromObject(strArray);

            card.Set(parseJarr);
            cardFrame.InitCardFrame(card, false);
            cardFrame.gameObject.SetActive(true);

            return cardFrame.gameObject;
        }

        GameObject MakeDragonEffect(Transform target = null)
        {
            SBFunc.RemoveAllChildrens(effectNode.transform);

            var node = Instantiate(dragonEffectPrefab, effectNode.transform);
            if (node != null)
            {
                if (target != null)
                    node.transform.position = target.position;
                else
                    node.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 30f, 0);
            }

            return node;
        }

        GameObject MakeDragonCardEffect(Transform target)
        {
            SBFunc.RemoveAllChildrens(effectNode.transform);

            var node = Instantiate(cardEffectPrefab, effectNode.transform);
            if (node != null)
            {
                if (target != null)
                    node.transform.position = target.position;
                else
                    node.transform.position = Vector3.zero;
            }

            return node;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class DragonPartCompoundSlot : MonoBehaviour
    {
        [SerializeField]
        GameObject emptyNode = null;

        [SerializeField]
        List<Sprite> gradeSprite = null;

        [SerializeField]
        Image gradeBG = null;

        [SerializeField]
        Image partIconSprite = null;

        [SerializeField]
        Text levelLabel = null;

        public void SetMaterialButtonUI(int tag, GameObject targetNode, bool isEmpty)
        {
            emptyNode.SetActive(isEmpty);

            if (isEmpty)
                return;

            Sprite image = null;
            var partData = User.Instance.PartData.GetPart(tag);
            var partLevel = partData.Reinforce;
            if (partData != null)
            {
                var designData = partData.GetItemDesignData();
                if (designData != null)
                {
                    image = designData.ICON_SPRITE;
                }
            }

            partIconSprite.sprite = image;

            if (partLevel > 0)
                levelLabel.text = ('+' + partLevel.ToString());//파츠 강화 수치 세팅
            else
                levelLabel.gameObject.SetActive(false);

            if (gradeBG != null)
                gradeBG.sprite = gradeSprite[partData.Grade() - 1];
        }
    }
}

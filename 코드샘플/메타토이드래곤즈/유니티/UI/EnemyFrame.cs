using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class EnemyFrame : MonoBehaviour
    {
        [SerializeField] Image sprIcon = null;
        [SerializeField] Image elementIcon = null;
        [SerializeField] Image Frame = null;

        [SerializeField] Color normalFrameColor = Color.white;
        [SerializeField] Color bossFrameColor = Color.white;

        [SerializeField] Text enemyNameText = null;
        [SerializeField] GameObject bossPanel = null;

        int monsterIndex = 0;


        public void SetEnemyFrame(int index = 0, int element = -1, bool isBoss = false)
        {
            var enemyInfo = MonsterBaseData.Get(index.ToString());

            var icon = ResourceManager.GetResource<Sprite>(eResourcePath.CharIconPath, enemyInfo.THUMBNAIL);

            if (icon == null)
                icon = ResourceManager.GetResource<Sprite>(eResourcePath.CharIconPath, "enemy_0");

            if (element > 0 && elementIcon != null)
            {
                elementIcon.gameObject.SetActive(true);
                elementIcon.sprite = this.GetElementIconSpriteByIndex(element);
            }
            else
                elementIcon.gameObject.SetActive(false);

            sprIcon.sprite = icon;
            monsterIndex = index;
            if(Frame != null)
                Frame.color = isBoss ? bossFrameColor : normalFrameColor;

            if(bossPanel != null)
                bossPanel.SetActive(isBoss);

            if (enemyNameText != null)
                enemyNameText.text = StringData.GetStringByStrKey(enemyInfo._NAME);
        }

        private Sprite GetElementIconSpriteByIndex(int e_type)
        {
            string elementStr = "";
            switch (e_type)
            {
                case 1:
                    elementStr = "fire";
                    break;
                case 2:
                    elementStr = "water";
                    break;
                case 3:
                    elementStr = "soil";
                    break;
                case 4:
                    elementStr = "wind";
                    break;
                case 5:
                    elementStr = "light";
                    break;
                case 6:
                    elementStr = "dark";
                    break;
                default:
                    break;
            }

            return ResourceManager.GetResource<Sprite>(eResourcePath.ElementIconPath, string.Format("type_{0}", elementStr));
        }

        public void onClickFrame()
        {
            return;

            // 용준님 요청 - 몬스터 설명은 세계관, 배경 등등 정립된 뒤에 설명을 다시 보기 위해 지금은 제한
            // 이 후 사용할지 안할지는 미지수

            //툴팁 팝업 띄우기 
            var enemyInfo = MonsterBaseData.Get(monsterIndex.ToString());
            var eneymyName = StringData.GetStringByStrKey(enemyInfo._NAME);
            var eneymyDesc = StringData.GetStringByStrKey(enemyInfo._DESC);


            Vector2 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(UICanvas.Instance.GetComponent<Canvas>().GetComponent<RectTransform>(), screenPos, Camera.main, out localPos);


            var btnWorldpos = transform.localPosition;
            var parent = PopupManager.Instance.Beacon;
            var beaconScale = parent.transform.localScale;
            bool reverseFlag = !(localPos.x < 400 * beaconScale.x);

            var popupData = new TooltipPopupData(new ToolTipData(eneymyName, eneymyDesc, gameObject, reverseFlag));
            PopupManager.OpenPopup<ToolTip>(popupData);
        }
    }
}
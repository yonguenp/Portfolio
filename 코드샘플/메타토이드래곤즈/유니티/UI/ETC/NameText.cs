using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class NameText : MonoBehaviour
    {
        [SerializeField]
        Sprite[] userRankSprite;

        [SerializeField]
        private Image UserRankIcon = null;

        [SerializeField]
        private Text textObject = null;
        private GuildUserData UserData { get; set; } = null;
        private Transform Target { get; set; } = null;

        public void SetData(GuildUserData data, Transform target)
        {
            UserData = data;
            Target = target;

            if (textObject != null)
            {
                textObject.text = UserData.Nick;

                if (UserData.UID == User.Instance.UserAccountData.UserNumber)
                {
                    textObject.color = Color.yellow;
                }
                else
                {
                    textObject.color = Color.white;
                }
            }

            int userrank = UserData.Rank;
            if (userrank > 0 && userrank < 50)
            {
                UserRankIcon.gameObject.SetActive(true);
                if (userrank == 1)
                {
                    UserRankIcon.sprite = userRankSprite[0];
                }
                else if (userrank == 2)
                {
                    UserRankIcon.sprite = userRankSprite[1];
                }
                else if (userrank == 3)
                {
                    UserRankIcon.sprite = userRankSprite[2];
                }
                else if (userrank <= 5)
                {
                    UserRankIcon.sprite = userRankSprite[3];
                }
                else if (userrank <= 10)
                {
                    UserRankIcon.sprite = userRankSprite[4];
                }
                else if (userrank <= 20)
                {
                    UserRankIcon.sprite = userRankSprite[5];
                }
                else if (userrank <= 49)
                {
                    UserRankIcon.sprite = userRankSprite[6];
                }
                else
                {
                    UserRankIcon.gameObject.SetActive(false);
                }
            }
            else
            {
                UserRankIcon.gameObject.SetActive(false);
            }


            transform.localPosition = GetTargetPosition();
        }

        private void LateUpdate()
        {
            if (UserData == null)
            {
                gameObject.SetActive(false);
                return;
            }

            if(UserRankIcon.IsActive())
                (transform as RectTransform).sizeDelta = (textObject.transform as RectTransform).sizeDelta + new Vector2((UserRankIcon.transform as RectTransform).sizeDelta.x + 20, 0);
            else
                (transform as RectTransform).sizeDelta = (textObject.transform as RectTransform).sizeDelta + new Vector2(15, 0);

            transform.localPosition = GetTargetPosition();
        }

        public Vector3 GetTargetPosition()
        {
            if (Target == null)
                return Vector3.zero;

            return new Vector2(Target.position.x * 72, Target.localPosition.y * 72 - 75.0f + (30.0f * Target.transform.lossyScale.y));
        }
    }
}
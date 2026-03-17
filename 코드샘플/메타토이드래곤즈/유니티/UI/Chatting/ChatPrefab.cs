using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ChatPrefab : MonoBehaviour
    {
        [SerializeField] protected bool usingAutoSpace = true;

        [SerializeField] protected GameObject CheckHeightNode = null;
        [SerializeField] protected float defaultSize = 250f;
        [SerializeField] protected LayoutElement layout;
        [SerializeField] protected int limitCharacterCount;
        [SerializeField] protected Text commentLabel = null;
        [SerializeField] protected Text timeLabel = null;
        [SerializeField] protected float diffY = 20f;
        [SerializeField] protected Text userNameText;

        protected eChatUIType chatUIType = eChatUIType.None;
        protected ChatUIItem chatData;

        public virtual void Init(ChatUIItem _chatTotalData)
        {
            chatUIType = _chatTotalData.chatUIType;
            chatData = _chatTotalData;

            if (userNameText != null)
                userNameText.text = chatData.chatInfo.SendNickname;
            SetChatPrefabHardcode();

            RefreshText();
            RebuildLayout();
        }

        protected virtual void SetTimeLabel(bool _isVisible)
        {
            if (timeLabel != null)
                timeLabel.gameObject.SetActive(_isVisible);
        }

        /*
         * 기준 해상도 1280 x 720 - 1.77777 (250 , 20)
         * 2152x1536 (1.4010)---- 1680x720 (2.3333)
         * 2208x1768 (1.2488) ---- 2260 x 816 (2.7696)
         * 2208 x 1768 (1.2488)---- 2268 x 832 (2.725)
         * 2176 x 1812 (1.200)---- 2316 x 904(2.5619)
         */
        protected void SetChatPrefabHardcode()//폴드 해상도 하드코드
        {
            var screenWidth = Screen.width;
            var screenHeight = Screen.height;
            var screenResolution = (float)screenWidth / (float)screenHeight;//현재 디바이스 종횡비

            if (screenResolution > 2.33333f)//접음 - 세로방향 길어짐
            {
                limitCharacterCount = 25;
                defaultSize = 450f;
            }
            else if (screenResolution < 1.44444f)//펼침 - 세로방향 짧아짐
            {
                limitCharacterCount = 10;
                defaultSize = 200f;
            }
            else
            {
                return;
            }
        }

        void SetCalcProportionalExpression()//종횡비 기준으로 변경하기에 오류 상황(글자 갯수제한)나와서 일단 패스
        {
            var defaultResolution = (float)1280 / (float)720;//기준 종횡비

            var screenWidth = Screen.width;//현재 화면 해상도
            var screenHeight = Screen.height;
            var screenResolution = (float)screenWidth / (float)screenHeight;//현재 디바이스 종횡비

            var scaleFactor = screenResolution / defaultResolution;//변화량

            var changePreferSize = (int)Mathf.Round(scaleFactor * defaultSize);
            var changeCharacterCount = (int)Mathf.Round(scaleFactor * limitCharacterCount);

            limitCharacterCount = changeCharacterCount;
            defaultSize = changePreferSize;
        }

        public virtual void Init(eChatUIType _type, string _comment)
        {

        }

        public void RefreshText()
        {
            var comment = chatData.chatInfo.Comment;
            var commentLength = chatData.chatInfo.Comment.Length;

            var isCharacterLimit = commentLength < limitCharacterCount;
            layout.enabled = !isCharacterLimit;

            if (!isCharacterLimit)
                layout.preferredWidth = defaultSize;

            SetCommentLabel(comment);

            var timeStamp = chatData.time;
            SetTimeLabel(timeStamp);
        }

        public virtual void SetCommentLabel(string _comment)
        {
            if (usingAutoSpace)
                _comment = _comment.Replace(' ', '\u00A0');//스페이스 -> 'non-breaking space' 로 변경 (개행이 아닌 스페이스가 단락 분리되는 이슈 막기)

            if (commentLabel != null)
                commentLabel.text = _comment;
        }

        public void SetTimeLabel(long _timeStamp)
        {
            var dateTime = SBFunc.TimeStampToDateTime(_timeStamp);

            if (timeLabel != null)
                timeLabel.text = dateTime.ToString("tt h:mm");
        }

        public virtual void RebuildLayout()
        {
            RefreshContentFitters();

            var sizeX = CheckHeightNode.GetComponent<RectTransform>().sizeDelta.x;
            var sizeY = CheckHeightNode.GetComponent<RectTransform>().sizeDelta.y;
            GetComponent<RectTransform>().sizeDelta = new Vector2(sizeX, sizeY + diffY);
        }

        public void RefreshContentFitters()
        {
            var rectTransform = (RectTransform)transform;
            RefreshContentFitter(rectTransform);
        }

        private void RefreshContentFitter(RectTransform transform)
        {
            if (transform == null || !transform.gameObject.activeInHierarchy)
            {
                return;
            }

            foreach (RectTransform child in transform)
            {
                RefreshContentFitter(child);
            }

            var layoutGroup = transform.GetComponent<LayoutGroup>();
            var contentSizeFitter = transform.GetComponent<ContentSizeFitter>();
            if (layoutGroup != null)
            {
                layoutGroup.SetLayoutHorizontal();
                layoutGroup.SetLayoutVertical();
            }

            if (contentSizeFitter != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
            }
        }
    }
}

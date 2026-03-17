using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public enum eHelpCommentType
    {
        NONE,
        SIMPLE,
        MULTIPLE,
    }

    public class HelpPopupData : PopupData
    {
        public int TabIndex { get; private set; } = -1;
        public int SubIndex { get; private set; } = -1;
        public string Comment { get; private set; } = "";
        public string Title { get; private set; } = "";
        public List<string> CommentList { get; private set; } = new List<string>();

        public eHelpCommentType commentType = eHelpCommentType.NONE;

        public HelpPopupData(int tab, int sub, string _comment = "", string _title = "")
        {
            TabIndex = tab;
            SubIndex = sub;
            Comment = _comment;
            Title = _title;
            commentType = eHelpCommentType.SIMPLE;

        }
        public HelpPopupData(string _title ,string _comment)
        {
            TabIndex = -1;
            SubIndex = -1;
            Comment = _comment;
            Title = _title;
            commentType = eHelpCommentType.SIMPLE;
        }

        public HelpPopupData (string _title, List<string> _commentStringKeyList)
        {
            TabIndex = -1;
            SubIndex = -1;
            CommentList = _commentStringKeyList;
            Title = _title;
            commentType = eHelpCommentType.MULTIPLE;
        }
    }
    /// <summary>
    /// 기본적인 도움말 같은 텍스트로만 이뤄진 형태로 단순하게 사용할 목적의 팝업
    /// </summary>
    public class HelpDescriptionPopup : Popup<HelpPopupData>
    {
        [SerializeField]
        protected Text titleText = null;
        [SerializeField]
        protected Text contentText = null;
        [SerializeField]
        protected RectTransform contentRect = null;

        public override void InitUI()
        {
            if (titleText != null)
                titleText.text = Data.Title;

            if (contentText != null)
            {
                if (Data.commentType != eHelpCommentType.MULTIPLE)
                    contentText.text = Data.Comment;
                else
                    SetMultipleComment();

            }

            if(contentRect != null)
                RefreshContentFitter(contentRect);
        }

        /// <summary>
        /// string Key 값을 가지고, key 값에 title 이 포함 되어있으면 제목 처리, desc 가 들어있으면 본문 처리
        /// </summary>
        void SetMultipleComment()
        {
            if (Data.CommentList == null || Data.CommentList.Count <= 0)
                return;

            var SetString = "";
            var titleRichText = "<size=50><b>{0}</b></size>";
            var commentRichText = "<size=40>{0}</size>";

            for(int i = 0; i< Data.CommentList.Count; i++)
            {
                var stringKey = Data.CommentList[i];
                if (stringKey.Contains("title"))
                    SetString += string.Format(titleRichText, StringData.GetStringByStrKey(stringKey));

                if (stringKey.Contains("desc"))
                    SetString += ("\n" + string.Format(commentRichText, StringData.GetStringByStrKey(stringKey)) + (i != Data.CommentList.Count - 1 ? "\n\n" : ""));
            }
            
            contentText.text = SetString;
        }

        protected void RefreshContentFitter(RectTransform transform)
        {
            if (transform == null || !transform.gameObject.activeSelf)
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



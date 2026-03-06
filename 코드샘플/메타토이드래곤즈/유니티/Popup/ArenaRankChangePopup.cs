using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace SandboxNetwork
{
    public class ArenaRankChangePopup : Popup<ArenaRankChangePopupData>
    {


        [Header("rank icon and effect")]
        [SerializeField] Image[] rankImgs = null;
        [SerializeField] RectTransform[] rankRects = null;
        [SerializeField] RectTransform rankParent = null;
        [SerializeField] Mask rankMaskObj = null; // 연출 끝나고 이 마스크를 켜서 보여주자 다른 랭크 아이콘 없애자
        [SerializeField] Sprite invisibleSprite = null; // 다다음 승급이나 전 전 강등의 아이콘이 없는 등 아이콘 없을때 처리하기 위한 용도

        [Space(20)]
        [Header("text and effect")]
        [SerializeField] GameObject resultObj = null;
        [SerializeField] Text rankText = null;
        [SerializeField] Text rankAlert = null;
        [SerializeField] GameObject fireworkParentObj = null;

        Sequence sequence = null;
        public override void InitUI()
        {
        }

        public void Init()
        {
            if (sequence != null)
            {
                sequence.Kill();
            }

            int prevGrade = Data.prevGrade;//경기 이전
            int currentGrade = Data.currentGrade;//현재

            var afterData = ArenaRankData.GetFirstInGroup(currentGrade);
            var beforeData = ArenaRankData.GetFirstInGroup(prevGrade);

            string afterIconStr = afterData.ICON;
            string beforeIconStr = beforeData.ICON;
            string rankStr = StringData.GetStringByStrKey(afterData._NAME);
            sequence = DOTween.Sequence();
            rankParent.anchoredPosition = Vector3.zero;
            SetDefaultRankSize();
            rankText.text= rankStr;
            if (prevGrade < currentGrade) // 랭크 상승 - 팝업 켜는 시점에서도 진짜 랭크 변동인지 체크하자
            {
                fireworkParentObj.SetActive(true);
                rankAlert.text = StringData.GetStringFormatByStrKey("랭크승급", rankStr);
                if (afterData != null)
                {
                    resultObj.SetActive(false);
                    InitRankImg();
                    rankImgs[3].sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ArenaRankPath, afterIconStr);
                    rankImgs[2].sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ArenaRankPath, beforeIconStr);

                    var downRankData = ArenaRankData.GetFirstInGroup(beforeData.GROUP - 1);
                    if (downRankData == null)
                    {
                        rankImgs[1].sprite = invisibleSprite;
                    }
                    else
                    {
                        rankImgs[1].sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ArenaRankPath, downRankData.ICON);
                    }
                    var afterAfterRankData = ArenaRankData.GetFirstInGroup(afterData.GROUP + 1);
                    if (afterAfterRankData == null)
                    {
                        rankImgs[4].sprite = invisibleSprite;
                    }
                    else
                    {
                        rankImgs[4].sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ArenaRankPath, afterAfterRankData.ICON);
                    }
                    sequence.SetDelay(0.5f);
                    sequence.Append(rankParent.DOLocalMoveX(-1100, 0.7f).SetEase(Ease.InQuad));
                    sequence.Append(rankRects[3].DOSizeDelta(new Vector2(800, 800), 0.5f));
                    sequence.Join(rankRects[2].DOSizeDelta(new Vector2(600, 600), 0.5f));
                    sequence.AppendCallback(() => ShowInfo(3));

                }
                else  // 이미 최강인가
                {
                    ClosePopup();
                    // 연출없이
                    //rankImgs[2].sprite = ResourceManager.GetResource<Sprite>(SBDefine.ResourcePath(eResourcePath.ArenaRankPath, beforeIconStr));
                    //ShowInfo(2);
                }
            }
            else  // 랭크 하락
            {
                fireworkParentObj.SetActive(false);
                rankAlert.text = StringData.GetStringFormatByStrKey("랭크하락", rankStr);
                if (currentGrade < prevGrade) // 진짜 진짜 랭크 하락
                {
                    if (afterData != null)// 강등인가
                    {
                        resultObj.SetActive(false);
                        InitRankImg();
                        rankImgs[1].sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ArenaRankPath, afterIconStr);
                        rankImgs[2].sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ArenaRankPath, beforeIconStr);

                        var upRankData = ArenaRankData.GetFirstInGroup(beforeData.GROUP + 1);
                        if (upRankData == null)
                        {
                            rankImgs[3].sprite = invisibleSprite;
                        }
                        else
                        {
                            rankImgs[3].sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ArenaRankPath, upRankData.ICON);
                        }

                        var downDownRankData = ArenaRankData.GetFirstInGroup(afterData.GROUP - 1);
                        if (downDownRankData == null)
                        {
                            rankImgs[0].sprite = invisibleSprite;
                        }
                        else
                        {
                            rankImgs[0].sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ArenaRankPath, downDownRankData.ICON);
                        }
                        sequence.SetDelay(0.5f);
                        sequence.Append(rankParent.DOLocalMoveX(1100, 0.7f).SetEase(Ease.InQuad));
                        sequence.Append(rankRects[1].DOSizeDelta(new Vector2(800, 800), 0.5f));
                        sequence.Join(rankRects[2].DOSizeDelta(new Vector2(600, 600), 0.5f));
                        sequence.AppendCallback(()=>ShowInfo(1));

                    }
                    else  // 이미 가장 나락인가
                    {
                        // 연출없이
                        ClosePopup();
                        //rankImgs[2].sprite = ResourceManager.GetResource<Sprite>(SBDefine.ResourcePath(eResourcePath.ArenaRankPath, beforeIconStr));
                        //ShowInfo(2);
                    }
                }
                else
                {
                    ClosePopup();
                }

            }
        }


        public override void ClosePopup()
        {
           
            base.ClosePopup();
        }


        void SetDefaultRankSize()
        {
            Vector2 defaultMiniSize = new Vector2(600, 600);
            rankRects[0].sizeDelta = rankRects[1].sizeDelta = rankRects[3].sizeDelta = rankRects[4].sizeDelta = defaultMiniSize;
            rankRects[2].sizeDelta = new Vector2(800, 800);
        }

        void InitRankImg()
        {
            foreach (var item in rankRects)
            {
                item.gameObject.SetActive(true);
            }
        }

        void ShowInfo(int index)
        {
            resultObj.SetActive(true);
            foreach (var item in rankRects)
            {
                item.gameObject.SetActive(false);
            }
            rankRects[index].gameObject.SetActive(true);
        }
    }
}



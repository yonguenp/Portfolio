using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIRankItem : MonoBehaviour
{
    [Header("기본")]
    [SerializeField] Text rankText;
    [SerializeField] Text pointText;
    [SerializeField] Text nickNameText;
    
    int rank;
    RankType rankType;

    [Header("랭크 아이템")]
    [SerializeField] Image rankImage;

    [Header("플레이어 리스트")]
    [SerializeField] Image bgFrame;
    [SerializeField] Image gradeFrame;
    [SerializeField] Image gradeIcon;
    [SerializeField] bool user;

    public void MyRankData()
    {
        rankText.text = Managers.UserData.MyUserID.ToString();
        pointText.text = "0";
        nickNameText.text = Managers.UserData.MyName;
        rankImage.DOColor(Color.clear, 0f);

        rankType = Managers.UserData.MyRank;

        if (rankType == null)
        {
            gradeIcon.sprite = RankType.DUMMY_RANK_ICON;
        }
        else
        {
            gradeIcon.sprite = rankType.rank_resource;
        }
    }

    public void Init(int _rank, int _point, string _name)
    {
        rank = _rank;
        rankText.text = _rank <= 0 ? "-" : _rank.ToString();
        pointText.text = _point.ToString();
        nickNameText.text = _name;

        rankType = RankType.GetRankFromPoint(_point);
    }

    public void SetRankListUI()
    {
        if (rank < 4)
        {
            rankImage.DOColor(Color.white, 0f);
            rankImage.sprite = Managers.Resource.LoadAssetsBundle<Sprite>("AssetsBundle/Texture/Icon/grade_0" + rank.ToString());
            rankText.text = string.Empty;
        }
        else
            rankImage.DOColor(Color.clear, 0f);

        if (rankType == null)
        {
            gradeIcon.sprite = RankType.DUMMY_RANK_ICON;
        }
        else
        {
            gradeIcon.sprite = rankType.rank_resource;
        }
    }

    public void SetPlayerListUI(Color bgframe, bool user = false)
    {
        this.user = user;

        if (bgFrame != null)
            bgFrame.color = bgframe;

        if (gradeIcon != null && gradeFrame != null)
        {
            // 유져 없는경우 
            if (!user)
            {
                gradeFrame.color = new Color(255f, 255f, 255f, 255f);
                gradeFrame.sprite = Resources.Load<Sprite>("Texture/UI/Lobby/img_circle_02");
                gradeIcon.sprite = Resources.Load<Sprite>("Texture/UI/Lobby/img_circle_01");
                gradeIcon.fillAmount = 0.2f;
                OnAnim();
            }
            //유져 있는경우
            else
            {
                gradeIcon.transform.DOKill();
                gradeIcon.transform.rotation = Quaternion.identity;
                gradeFrame.color = new Color(255f, 255f, 255f, 0f);

                if(rankType == null)
                {
                    gradeIcon.sprite = RankType.DUMMY_RANK_ICON;
                }
                else
                {
                    gradeIcon.sprite = rankType.rank_resource;
                }
                
                gradeIcon.fillAmount = 1f;
            }
        }
    }

    public void OnAnim()
    {
        gradeIcon.transform.DOKill();
        gradeIcon.transform.rotation = Quaternion.identity;
        gradeIcon.transform.DORotate(new Vector3(0, 0, -180), 1f).SetLoops(-1, loopType: LoopType.Incremental);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SeasonPassBannerPanel : MonoBehaviour
{
    [Header("[Title Info Layer]")]
    //public Image seasonPassBgImage;
    public Text seasonPassTitleText;
    public Text seasonPassSubTitleText;

    //public Image seasonPassSubReward_lvl2;
    //public Image seasonPassSubReward_lvl3;

    neco_pass curPassData;

    public void SetSeasonPassBannerData(neco_pass passData)
    {
        curPassData = passData;

        // 타이틀 세팅
        seasonPassTitleText.text = curPassData.GetNecoPassMainTitle();
        seasonPassSubTitleText.text = curPassData.GetNecoPassSubTitle();

        //if(seasonPassSubReward_lvl2 != null && seasonPassSubReward_lvl2.sprite == null)
        //{
        //    //현재 시즌패스번호 얻어오기
        //    //(100+ 시즌패스 번호) * 10 + 1 = 메모리 이미지 썸네일 번호
        //    uint rewardNumber = (100 + passData.GetNecoPassSeason()) * 10 + 1;

        //    neco_package packageInfo = neco_package.GetNecoPackageByID(rewardNumber);

        //    seasonPassSubReward_lvl2.sprite = Resources.Load<Sprite>(string.Format("beta_cards/thm_{0}", packageInfo.GetNecoPackageItemID()));
        //}

        //if (seasonPassSubReward_lvl3 != null && seasonPassSubReward_lvl3.sprite == null)
        //{
        //    //현재 시즌패스번호 얻어오기
        //    //(100+ 시즌패스 번호) * 10 + 2  = 메모리 이미지 썸네일 번호
        //    uint rewardNumber = (100 + passData.GetNecoPassSeason()) * 10 + 2;

        //    neco_package packageInfo = neco_package.GetNecoPackageByID(rewardNumber);

        //    seasonPassSubReward_lvl3.sprite = Resources.Load<Sprite>(string.Format("beta_cards/thm_{0}", packageInfo.GetNecoPackageItemID()));
        //}
    }
}


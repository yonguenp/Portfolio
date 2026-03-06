using Coffee.UIEffects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

 namespace SandboxNetwork
{
    public class PortraitBox : MonoBehaviour
    {
        [SerializeField] private Image portraitImg;
        [SerializeField] private Image backgroundImg;
        [SerializeField] private Image frame;
        [SerializeField] private UIGradient CheckBoxObj;
        [SerializeField] private Color selectedColor;
        [SerializeField] private Sprite[] bgList;

        public delegate void CallBack(string dragonID, PortraitBox box);
        CallBack btnCallback;

        CharBaseData currentDragonData = null;

        private readonly string defaultPortraitName = "mtdz_cap";
        private int gradient_offset_rate = 0;
        public void Init(CharBaseData portraitDragonData, bool isCheck = false, CallBack cb = null)
        {
            btnCallback = cb;
            currentDragonData = portraitDragonData;

            if (portraitDragonData !=null && portraitDragonData.KEY > 0) 
            {
                var dragonImg = portraitDragonData.GetThumbnail();
                if (dragonImg == null)
                {
                    SetDefaultImg();
                    SetCheck(isCheck);
                    return;
                }
                portraitImg.sprite = dragonImg;
                backgroundImg.sprite = GetCustomGradeBG(currentDragonData.GRADE);
                SetFrame();
            }
            else
            {
                SetDefaultImg();
            }

            SetCheck(isCheck);
        }
        Sprite GetCustomGradeBG(int grade)
        {
            if (grade > 0 && grade - 1 < bgList.Length)
            {
                return bgList[grade - 1];
            }
            else
                return ResourceManager.GetResource<Sprite>(eResourcePath.DragonGradeTagIconPath, "default_infobg");
        }

        void SetFrame()
        {
            if (currentDragonData == null)
            {
                frame.color = Color.white;
                return;
            }

            if (User.Instance.DragonData.IsFavorite(currentDragonData.KEY))
            {
                frame.color = Color.green;
            }
            else
            {
                switch (currentDragonData.GRADE)
                {
                    case 0:
                    case 1:
                        frame.color = new Color(0.5686274509803922f, 0.5686274509803922f, 0.5686274509803922f);
                        break;
                    case 2:
                        frame.color = new Color(0.1098039215686275f, 0.603921568627451f, 0.0f);
                        break;
                    case 3:
                        frame.color = new Color(0.0313725490196078f, 0.5647058823529412f, 0.8823529411764706f);
                        break;
                    case 4:
                        frame.color = new Color(0.4941176470588235f, 0.0784313725490196f, 0.9098039215686275f);
                        break;
                    case 5:
                        frame.color = new Color(1.0f, 0.5490196078431373f, 0.0f);
                        break;
                }
            }
        }


        void SetDefaultImg()
        {
            portraitImg.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.CharIconPath, defaultPortraitName);
            backgroundImg.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.DragonGradeTagIconPath, "default_infobg");
        }

        public void SetCheck(bool isCheck)
        {
            gradient_offset_rate = 0;
            CheckBoxObj.gameObject.SetActive(isCheck);
            CheckBoxObj.offset = gradient_offset_rate;
            if (isCheck)
            {
                gradient_offset_rate = -1;
            }
        }

        public void onClickPortrait()
        {
            btnCallback?.Invoke(currentDragonData != null ? currentDragonData.KEY.ToString() : defaultPortraitName, this);
            SetCheck(true);
        }
      
        private string GetGradeConvertString(int grade)
        {
            switch (grade)
            {
                case 2:
                    return "r";
                case 3:
                    return "sr";
                case 4:
                    return "ur";
                case 1:
                default:
                    return "n";
            }
        }

        private void Update()
        {
            if (gradient_offset_rate == 0)
                return;

            CheckBoxObj.offset += gradient_offset_rate * Time.deltaTime;
            if (gradient_offset_rate > 0 && CheckBoxObj.offset >= gradient_offset_rate)
                gradient_offset_rate = -1;
            if (gradient_offset_rate < 0 && CheckBoxObj.offset <= gradient_offset_rate)
                gradient_offset_rate = 1;
        }
    }
}

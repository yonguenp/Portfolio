using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class GuildEmblemSelectObj : MonoBehaviour
    {
        [SerializeField] 
        Image IconImg = null;
        [SerializeField]
        GameObject checkObj = null;
        [SerializeField]
        Image outlineImg = null;

        GuildResourceData IconData = null;
        IntDelegate ClickCallBack = null;
        public void SetIconImg(int iconNo, bool isEmblem, bool isCheck)
        {
            IconData = GuildResourceData.Get(iconNo);
            if (IconData == null)
            {
                if (isEmblem)
                    IconImg.sprite = GuildResourceData.DEFAULT_EMBLEM;
                else
                    IconImg.sprite = GuildResourceData.DEFAULT_MARK;
            }
            else
            {
                IconImg.sprite = IconData.RESOURCE;
            }
            checkObj.SetActive(false);
            outlineImg.gameObject.SetActive(isCheck);
            //SetCheckState(isCheck, isEmblem);
        }

        void SetCheckState(bool isCheck, bool isEmblem)
        {
            checkObj.SetActive(isCheck);
            outlineImg.gameObject.SetActive(isCheck);
        }
        
        public void SetClickCallBack(IntDelegate clickCallBack)
        {
            if(clickCallBack != null)
            {
                ClickCallBack = clickCallBack;
            }
        }

        public void OnClick()
        {
            if(ClickCallBack != null)
            {
                ClickCallBack(IconData == null ? 0 : IconData.KEY);
            }
        }
        
    }
}


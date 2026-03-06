using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{

    public class AnnounceMenuObj : MonoBehaviour
    {
        [SerializeField]
        private Text menuText;

        [SerializeField]
        private Image menuImg;

        [SerializeField]
        private Sprite selectImg;

        [SerializeField]
        private Sprite noneSelectImg;



        public int MenuID { get; private set; }


        public delegate void CallBack(int id);

        CallBack btnCallBack = null;

        public void Init(int menuId, string menuString, CallBack cb)
        {
            MenuID = menuId;
            menuText.text = menuString;
            btnCallBack = cb;
        }

        public void OnClickMenu()
        {
            btnCallBack?.Invoke(MenuID);
        }

        public bool SetMenuSelectSprite(int id)
        {
            if(id == MenuID)
            {
                menuImg.sprite =selectImg;
                transform.localScale = Vector3.one *1.05f;
            }
            else
            {
                menuImg.sprite = noneSelectImg;
                transform.localScale = Vector3.one;
            }

            return id == MenuID;


        }


    }
}

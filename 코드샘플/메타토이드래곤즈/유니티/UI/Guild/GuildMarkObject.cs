using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class GuildMarkObject : MonoBehaviour
    {
        [SerializeField]
        Image Emblem = null;
        [SerializeField]
        Image MarkImg = null;
        public void SetGuildMark(Sprite emblem=null, Sprite markSprite= null)
        {
            if(emblem == null)
                Emblem.gameObject.SetActive(false);
            else
            {
                Emblem.gameObject.SetActive(true);
                Emblem.sprite = emblem;
            }

            if(markSprite == null)
                MarkImg.gameObject.SetActive(false);
            else
            {
                MarkImg.gameObject.SetActive(true);
                MarkImg.sprite = markSprite;
            }
        }

        public void SetGuildMark(int emblemNo, int markNo)
        {
            Sprite emblemSprite = null;
            var emblem = GuildResourceData.Get(emblemNo);
            if (emblem != null)
                emblemSprite = emblem.RESOURCE;
            
            Sprite markSprite = null;
            var mark = GuildResourceData.Get(markNo);
            if (mark != null)
                markSprite = mark.RESOURCE;

            if (emblemSprite != null)
            {
                Emblem.gameObject.SetActive(true);
                Emblem.sprite = emblemSprite;
            }
            else
            {
                Emblem.gameObject.SetActive(false);
            }

            if(markSprite != null)
            {
                MarkImg.gameObject.SetActive(true);
                MarkImg.sprite = markSprite;
            }
            else
            {
                MarkImg.gameObject.SetActive(false);
            }
        }
    }
}


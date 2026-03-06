using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork { 
    public class FlagCell : Cell, EventListener<GuildEvent>
    {
        [SerializeField]
        GameObject matObj;
        [SerializeField]
        GameObject stickObj;

        private Texture2D FlagTexture;
        private readonly float scaleValue = 0.2f;

        private void OnEnable()
        {
            this.EventStart();
            Refresh();
        }
        private void OnDisable()
        {
            this.EventStop();
        }
        public void SetGuildState(bool isOn)
        {
            matObj.SetActive(isOn);
            stickObj.SetActive(isOn);
        }
        public void SetFlagImage(Sprite flagImg, Sprite markImg,Sprite emblemImg)
        {
            matObj.SetActive(true);
            stickObj.SetActive(true);
            /// Left와 Right 깃발이 다른 배경을 가질 수 있다고 생각하여 
            /// 각각 개별 텍스쳐로 진행하도록 함
            /// 마찬가지로 복사할 필요가 없음
            FlagTexture = TextureFromSprite(flagImg);
            if (FlagTexture == null)
                return;

            SetTextureBySprite(emblemImg, Vector2Int.up * 20);
            SetTextureBySprite(markImg, Vector2Int.up * 19);

            /// 마지막 한번만 하도록 수정
            FlagTexture.Apply();
            matObj.GetComponent<SkinnedMeshRenderer>().materials[0].mainTexture = FlagTexture;
        }

        void SetTextureBySprite(Sprite sprite, Vector2Int offset)
        {
            /// 복사까지는 안해도 되는것으로 확인되어 DuplicateTexture 제거
            var curTexture = TextureFromSprite(sprite);
            if (curTexture == null)
                return;
            
            int halfX = curTexture.width / 2;
            int halfY = curTexture.height / 2;
            Vector2Int totalSize = new(FlagTexture.width, FlagTexture.height);
            Vector2Int centerPos = totalSize / 2 + offset;
            int startX = centerPos.x - halfX;
            int endX = centerPos.x + halfX;
            int startY = centerPos.y - halfY;
            int endY = centerPos.y + halfY;
            for (int y = 0; y < totalSize.y; ++y)
            {
                for (int x = 0; x < totalSize.x; ++x)
                {
                    if (startX <= x && endX > x && startY <= y && endY > y )
                    {
                        Color color = curTexture.GetPixel(x - startX, y - startY);
                        if(color.a != 0f)
                        {
                            if(color.a == 1f)
                                FlagTexture.SetPixel(x, y, color);
                            else
                            {
                                var pixelColor = FlagTexture.GetPixel(x, y);
                                pixelColor += new Color(color.r - pixelColor.r , color.g - pixelColor.g, color.b - pixelColor.b, 0f) * color.a;
                                FlagTexture.SetPixel(x, y, pixelColor);
                            }
                        }
                    }
                }
            }
        }

        Texture2D TextureFromSprite(Sprite sprite)
        {
            if(sprite.texture.isReadable ==false)
                return null;
            Texture2D newText = new Texture2D((int)sprite.textureRect.width, (int)sprite.textureRect.height);
            Color[] newColors = sprite.texture.GetPixels((int)sprite.textureRect.x,
                                                            (int)sprite.textureRect.y,
                                                            (int)sprite.textureRect.width,
                                                            (int)sprite.textureRect.height);
            newText.SetPixels(newColors);
            return newText;
        }
        
        public void OnEvent(GuildEvent eventType)
        {
            switch (eventType.Event)
            {
                case GuildEvent.eGuildEventType.LostGuild:
                    SetGuildState(false);
                    break;
                case GuildEvent.eGuildEventType.GuildRefresh:
                    Refresh();
                    break;
            }
        }

        public void Refresh()
        {
            //int emblemNo = GuildManager.Instance.MyGuildInfo.GetGuildEmblem();
            int markNo = GuildManager.Instance.MyGuildInfo.GetGuildMark();
            int emblemNo = GuildManager.Instance.MyGuildInfo.GetGuildEmblem();
            var flagSprite = GuildResourceData.DEFAULT_FLAG;

            Sprite markSprite = GuildResourceData.DEFAULT_MARK;
            var mark = GuildResourceData.Get(markNo);
            if (mark != null)
                markSprite = mark.RESOURCE;

            Sprite emblemSprite = GuildResourceData.DEFAULT_EMBLEM;
            var emblem = GuildResourceData.Get(emblemNo);
            if (emblem != null)
                emblemSprite = emblem.RESOURCE;

            SetFlagImage(flagSprite, markSprite, emblemSprite);
        }
    }
}
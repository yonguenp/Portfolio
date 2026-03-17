using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class user_card : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.USER_CARD; }

    public static List<user_card> GetUserCardList(uint cardId)
    {
        List<game_data> cardData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_CARD);

        List<user_card> resultCardList = new List<user_card>();

        foreach (user_card data in cardData)
        {
            if (data != null && data.GetCardID() == cardId)
            {
                resultCardList.Add(data);
            }
        }

        return resultCardList;
    }

    public static user_card GetUserCard(uint cardUID, uint cardID)
    {
        List<game_data> cardData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_CARD);

        foreach (user_card data in cardData)
        {
            if (data != null && data.GetCardUniqueID() == cardUID)
            {
                if (data.GetCardID() == cardID)
                {
                    return data;
                }
            }
        }

        return null;
    }

    [NonSerialized]
    private uint UID = 0;

    public uint GetCardUniqueID()
    {
        if (UID == 0)
        {
            object obj;
            if (data.TryGetValue("card_uid", out obj))
            {
                UID = (uint)obj;
            }
        }

        return UID;

    }
    [NonSerialized]
    private uint cardID = 0;

    public uint GetCardID()
    {
        if (cardID == 0)
        {
            object obj;
            if (data.TryGetValue("card_id", out obj))
            {
                cardID = (uint)obj;
            }
        }

        return cardID;
    }

    [NonSerialized]
    private card_define define_data = null;

    public card_define GetCardData()
    {
        if (define_data == null)
        {
            uint curID = GetCardID();
            object obj;
            List<game_data> data_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.CARD_DEFINE);
            if (data_list != null)
            {
                foreach (game_data data in data_list)
                {
                    if (data.data.TryGetValue("card_id", out obj))
                    {
                        if (curID == (uint)obj)
                            define_data = (card_define)data;
                    }
                }
            }
        }

        return define_data;
    }

    [NonSerialized]
    private Rect rect = Rect.zero;
    public Rect GetRect()
    {
        if (rect == Rect.zero)
        {
            card_define card = GetCardData();
            if (card != null)
            {
                if (card.GetResourceType() == 0)
                {
                    object obj;
                    if (data.TryGetValue("rect", out obj))
                    {
                        string strRect = (string)obj;
                        if (strRect == "FULL")
                        {
                            rect.position = Vector2.zero;
                            rect.size = CardSpriteSize;
                        }
                        else
                        {
                            string[] strInfo = strRect.Split(',');
                            if (strInfo.Length >= 4)
                            {
                                rect.x = float.Parse(strInfo[0]);
                                rect.y = float.Parse(strInfo[1]);
                                rect.width = float.Parse(strInfo[2]);
                                rect.height = float.Parse(strInfo[3]);
                            }
                            else
                            {
                                Debug.LogError("user_card GetRect Err!!!");
                            }
                        }
                    }
                }
                else if (card.GetResourceType() == 1)
                {
                    rect.position = Vector2.zero;
                    rect.size = VideoSpriteSize;
                }
            }
        }

        return rect;
    }

    [NonSerialized]
    private Rect UVrect = Rect.zero;
    public Rect GetUVRect()
    {
        if (UVrect == Rect.zero)
        {
            Rect rect = GetRect();
            if(rect != Rect.zero)
            {
                Vector2 size = GetCardData().GetResourceType() == 0 ? CardSpriteSize : VideoSpriteSize;
                UVrect.x = rect.x / size.x;
                UVrect.width = rect.width / size.x;
                UVrect.y = rect.y / size.y;
                UVrect.height = rect.height / size.y;
            }
        }

        return UVrect;
    }

    [NonSerialized]
    private Sprite origin_sprite = null;

    public bool IsOriginLoaded() { return origin_sprite != null; }
    public void SetOriginSprite(Sprite origin)
    {
        origin_sprite = origin;
    }

    private Sprite GetOriginSprite()
    {
        if (origin_sprite == null)
        {
            card_define card = GetCardData();
            if (card != null)
            {
                origin_sprite = Resources.Load<Sprite>(card.GetResourcePath());
            }
        }

        return origin_sprite;
    }

    [NonSerialized]
    private Sprite sprite = null;
    public Sprite GetSprite()
    {
        if (sprite == null)
        {
            card_define card = GetCardData();
            if (card != null)
            {
                sprite = Sprite.Create(GetOriginSprite().texture, GetRect(), Vector2.one * 0.5f, 16f, 0, SpriteMeshType.FullRect);                
            }
        }

        return sprite;
    }

    [NonSerialized]
    private Sprite icon = null;
    public Sprite GetIcon()
    {
        if (icon == null)
        {
            card_define card = GetCardData();
            if (card != null)
            {
                Vector2 fitSize = CardSpriteSize;
                if (card.GetResourceType() == 1)
                {
                    fitSize = VideoSpriteSize;
                }

                if (fitSize == GetRect().size)
                {
                    object obj;
                    if (card.data.TryGetValue("cover_img", out obj))
                    {
                        icon = Resources.Load<Sprite>((string)obj);
                    }
                    if (icon == null)
                    {
                        icon = Resources.Load<Sprite>("Sprites/icon/card_default");
                    }
                }
                else
                {
                    Texture2D texture = GetSprite().texture;
                    Rect rect = GetRect();
                    float diff = rect.width - rect.height;
                    rect.x += diff * 0.5f;
                    rect.width -= diff;
                    icon = Sprite.Create(texture, rect, Vector2.one * 0.5f, 16f, 0, SpriteMeshType.FullRect);
                }
            }
        }

        return icon;
    }

    public IEnumerator SetIconSpriteAsync(Image image, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (icon == null)
        {
            card_define card = GetCardData();
            if (card != null)
            {
                Vector2 fitSize = CardSpriteSize;
                if (card.GetResourceType() == 1)
                {
                    fitSize = VideoSpriteSize;
                }

                if (fitSize == GetRect().size)
                {
                    object obj;
                    if (card.data.TryGetValue("cover_img", out obj))
                    {
                        icon = Resources.Load<Sprite>((string)obj);

                        ResourceRequest req = Resources.LoadAsync<Sprite>((string)obj);
                        while (!req.isDone)
                        {
                            yield return new WaitForEndOfFrame();
                        }
                        icon = (Sprite)req.asset;
                    }

                    if (icon == null)
                    {
                        icon = Resources.Load<Sprite>("Sprites/icon/card_default");
                    }
                }
                else
                {
                    if(sprite == null)
                    {
                        if(origin_sprite == null)
                        {
                            ResourceRequest req = Resources.LoadAsync<Sprite>(card.GetResourcePath());
                            while (!req.isDone)
                            {
                                yield return new WaitForEndOfFrame();
                            }
                            origin_sprite = (Sprite)req.asset;
                        }
                        
                        sprite = Sprite.Create(origin_sprite.texture, GetRect(), Vector2.one * 0.5f, 16f, 0, SpriteMeshType.FullRect);
                    }

                    Texture2D texture = sprite.texture;
                    Rect rect = GetRect();
                    float diff = rect.width - rect.height;
                    rect.x += diff * 0.5f;
                    rect.width -= diff;
                    icon = Sprite.Create(texture, rect, Vector2.one * 0.5f, 16f, 0, SpriteMeshType.FullRect);
                }
            }
        }

        if(!image.IsDestroyed())
            image.sprite = icon;
    }

    public string GetCardMemo()
    {
        object obj;
        if (data.TryGetValue("memo", out obj))
        {
            return (string)obj;
        }

        return "";
    }

    public enum CARD_TYPE
    { 
        UNKNOWN,
        PIECE,
        PERFECT,
        MOVIE
    };

    [NonSerialized]
    private CARD_TYPE type = CARD_TYPE.UNKNOWN;
    public CARD_TYPE GetCardType()
    {
        if (type == CARD_TYPE.UNKNOWN)
        {
            card_define card = GetCardData();
            if (card != null)
            {
                if (card.GetResourceType() == 1)
                {
                    type = CARD_TYPE.MOVIE;
                }
                else
                {
                    if (GetRect().size == CardSpriteSize)
                    {
                        type = CARD_TYPE.PERFECT;
                    }
                    else
                    {
                        type = CARD_TYPE.PIECE;
                    }
                }
            }
        }
        return type;
    }

    public Vector2 VideoSpriteSize { get { return new Vector2(1200, 900); } }
    public Vector2 CardSpriteSize { get { return new Vector2(2048, 1536); } }
}


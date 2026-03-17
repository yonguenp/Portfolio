using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

[Serializable]
public class card_define_sub : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.CARD_DEFINE_SUB; }

    public static List<card_define_sub> GetCardDefineSubList(uint cardID)
    {
        List<game_data> cardSubData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.CARD_DEFINE_SUB);
        if (cardSubData == null)
        {
            return null;
        }

        List<card_define_sub> cardSubList = new List<card_define_sub>();

        foreach (card_define_sub data in cardSubData)
        {
            if (data != null && data.GetParentCardID() == cardID)
            {
                cardSubList.Add(data);
            }
        }

        return cardSubList;
    }

    [NonSerialized]
    private uint cardID = 0;
    public uint GetCardID()
    {
        if (cardID == 0)
        {
            object obj;
            if (data.TryGetValue("id", out obj))
            {
                cardID = (uint)obj;
            }
        }

        return cardID;
    }

    [NonSerialized]
    private uint parentCardID = 0;
    public uint GetParentCardID()
    {
        if (parentCardID == 0)
        {
            object obj;
            if (data.TryGetValue("parent_id", out obj))
            {
                parentCardID = (uint)obj;
            }
        }

        return parentCardID;
    }

    [NonSerialized]
    private uint isPefectValue = 0;
    public uint GetIsPefectValue()
    {
        if (isPefectValue == 0)
        {
            object obj;
            if (data.TryGetValue("is_perfect", out obj))
            {
                isPefectValue = (uint)obj;
            }
        }

        return isPefectValue;
    }

    [NonSerialized]
    private card_define define_data = null;

    public card_define GetCardData()
    {
        if (define_data == null)
        {
            uint curID = GetParentCardID();
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
                    if (data.TryGetValue("rect_value", out obj))
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
                    if (sprite == null)
                    {
                        if (origin_sprite == null)
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

        if (!image.IsDestroyed())
            image.sprite = icon;
    }

    public Vector2 VideoSpriteSize { get { return new Vector2(1200, 900); } }
    public Vector2 CardSpriteSize { get { return new Vector2(2048, 1536); } }
}

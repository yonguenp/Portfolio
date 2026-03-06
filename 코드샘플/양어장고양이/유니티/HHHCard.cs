using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class HHHCard
{
    public Sprite sprite = null;
    public List<string> clipName;

    public uint ID = 0;
    public uint Grade = 0;
    public string CoverImage = "";
    public string FrameImage = "";
    public string CardName = "";
    public string CardDesc = "";


    public uint Level = 0;
    public uint EXP = 0;
    public uint CardCount = 0;

    //public int StarCount = 0;    
    //public int EnchantCount = 0;
    
    public HHHCard()
    {

    }

    public void SetData(card_define data)
    {
        object obj;
        if (data.data.TryGetValue("card_id", out obj))
            ID = (uint)obj;
        if (data.data.TryGetValue("card_grade", out obj))
            Grade = (uint)obj;
        if (data.data.TryGetValue("cover_img", out obj))
            CoverImage = (string)obj;
        if (data.data.TryGetValue("frame_img", out obj))
            FrameImage = (string)obj;
        if (data.data.TryGetValue("card_title_kr", out obj))
            CardName = (string)obj;
        if (data.data.TryGetValue("card_desc_kr", out obj))
            CardDesc = (string)obj;

        sprite = Resources.Load<Sprite>(CoverImage);
        if (sprite == null)
        {
            Debug.Log("error : " + CoverImage);
        }
    }

    public void SetUserData(user_card data)
    {
        object obj;
        if (data.data.TryGetValue("card_lv", out obj))
            Level = (uint)obj;
        if (data.data.TryGetValue("card_exp", out obj))
            EXP = (uint)obj;
        if (data.data.TryGetValue("card_amount", out obj))
            CardCount = (uint)obj;
    }

    public void SetTestData()
    {
        clipName = new List<string>();
        for (int i = 0; i < 5; i++)
        {
            clipName.Add(Random.Range(0, 43).ToString());
        }

        string[] sample = {
            "mar1",
            "mar1_sm",
            "mar1_01",
            "non_sm",
            "sam1",
            "sam1_sm",
            "sam2",
            "sam2_sm",
            "sam3",
            "sam3_sm",
            "sam_01",
            "sam_02",
            "sam_03",
        };

        while (sprite == null)
        {
            string randomSpriteName = sample[Random.Range(1, sample.Length - 1)];
            CardName = randomSpriteName;
            sprite = Resources.Load<Sprite>("Card_Clips/" + randomSpriteName);
            if (sprite == null)
            {
                Debug.Log("error : " + randomSpriteName);
            }
        }

        //StarCount = Random.Range(1, 5);
        //EnchantCount = Random.Range(0, 4);
        Level = (uint)Random.Range(1, 99);
        CardCount = (uint)Random.Range(1, 99);
        //CardName = randomSpriteName;
    }

    public VideoClip GetVideoClip(ref int index)
    {
        if(index < -1)
        {
            index = clipName.Count - 1;
        }

        if(clipName.Count <= index || index < 0)
        {
            return null;
        }

        VideoClip clip = Resources.Load<VideoClip>("CardClips/" + clipName[index]);
        return clip;
    }
    
}

public class HHHCardManager
{
    private List<HHHCard> Cards = new List<HHHCard>();
    private static HHHCardManager instance = null;

    public static HHHCardManager Instance
    {
        get
        {
            if (null == instance)
            {
                instance = new HHHCardManager();
            }
            return instance;
        }
    }

    public HHHCardManager()
    {
        
    }

    public void Init()
    {
        Cards.Clear();

        List<game_data> card_data = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.CARD_DEFINE);
        if (card_data == null)
            return;

        List<game_data> user_data = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_CARD);

        foreach (game_data cd in card_data)
        {
            HHHCard card = new HHHCard();
            card.SetData((card_define)cd);
            Cards.Add(card);

            if (user_data != null)
            {
                foreach (game_data ud in user_data)
                {
                    object id;
                    if (ud.data.TryGetValue("card_id", out id))
                    {
                        if ((uint)id == card.ID)
                        {
                            card.SetUserData((user_card)ud);
                        }
                    }
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardListItem : MonoBehaviour
{
    public Image IconImage;
    public Text Desc;
    public Text Count;

    public GameObject PerfectIcon;
    public GameObject MovieIcon;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetRewarditem(string iconImage, string desc, int count = 1)
    {
        if (PerfectIcon != null && MovieIcon != null)
        {
            PerfectIcon.SetActive(false);
            MovieIcon.SetActive(false);
        }

        if (Count != null)
        {
            Count.gameObject.SetActive(count > 1);
            if (count > 1)
            {
                Count.text = count.ToString();
            }
        }

        string[] iconSplit = iconImage.Split(':');
        if (iconSplit.Length > 1)
        {
            if (iconSplit[0] == "card")
            {
                uint uid = uint.Parse(iconSplit[1]);
                List<game_data> item_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_CARD);
                foreach(game_data uc in item_list)
                {
                    user_card userCard = ((user_card)uc);
                    if (userCard.GetCardUniqueID() == uid)
                    {
                        IconImage.sprite = ((user_card)uc).GetIcon();

                        switch (userCard.GetCardType())
                        {
                            case user_card.CARD_TYPE.PERFECT:
                                PerfectIcon.SetActive(true);
                                break;
                            case user_card.CARD_TYPE.MOVIE:
                                MovieIcon.SetActive(true);
                                break;
                            case user_card.CARD_TYPE.PIECE:
                            default:
                                break;
                        }
                    }
                }
            }
        }
        else
        {
            Sprite sprite = Resources.Load<Sprite>(iconImage);
            if (sprite != null)
                IconImage.sprite = sprite;
        }

        if(Desc != null)
            Desc.text = desc;
    }
}

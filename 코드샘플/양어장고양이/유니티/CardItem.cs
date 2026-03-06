using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardItem : MonoBehaviour
{
    public HHHCard cardInfo = null;

    int cardListIndex = -1;
    Image image;

    private void Awake()
    {
        if(cardInfo == null)
            cardInfo = new HHHCard();

        image = gameObject.GetComponent<Image>();
        image.sprite = cardInfo.sprite;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetIndex(int index)
    {
        cardListIndex = index;
    }

    public int GetIndex()
    {
        return cardListIndex;
    }

    public HHHCard GetCardInfo()
    {
        return cardInfo;
    }

    public void RefreshCard()
    {
        image.sprite = cardInfo.sprite;
    }
}

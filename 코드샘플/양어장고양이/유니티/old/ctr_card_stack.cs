using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ctr_card_stack : MonoBehaviour
{
    // Start is called before the first frame update
    public card myCard;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(myCard != null) 
            this.CNT.text = myCard.count.ToString();
    }
    public void click_this()
    {
        state_manager._pnl_card_list.GetComponent<mng_card_list>().change_deck_card(myCard);
    }
    
    public GameObject THIS;
    public GameObject[] STARS;
    
    public Text CNT;
    public void initial_card(string img_name, int star, int count)
    {
        THIS.GetComponent<Image>().sprite = Resources.Load<Sprite>("Card_Clips/" + img_name);
        
    }
    public void initial_card(card c)        
    {
        myCard = c;
        THIS.GetComponent<Image>().sprite = Resources.Load<Sprite>("Card_Clips/" + c.img_name+"_sm");
        for (int i = 0; i < 5; i++) STARS[i].SetActive(false);
        for (int i = 0; i < c.star; i++) STARS[i].SetActive(true);        
    }
    
}

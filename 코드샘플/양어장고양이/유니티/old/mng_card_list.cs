using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class mng_card_list : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject THIS;
    public GameObject FILTER;
    public GameObject[] ROWS;
    public Button[] ACTS;
    public int current_card_count = 0;
    public card currentDeck;
    public GameObject DECK;
    public Text POINT;
    public Text NAME;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void clk_filter(int idx)
    {

    }

    private void OnEnable()
    {
        
    }
    public void add_Card(card c)
    {
        
        GameObject prefab = Resources.Load("Prefabs/card_stack") as GameObject;
        GameObject new_card = MonoBehaviour.Instantiate(prefab) as GameObject;

        new_card.GetComponent<ctr_card_stack>().initial_card(c);
        new_card.transform.SetParent(ROWS[current_card_count / 3].transform);
        new_card.transform.localScale = Vector3.one;
        new_card.transform.SetAsLastSibling();
        current_card_count++;

    }
    public void change_deck_card(card c)
    {
        currentDeck = c;
        DECK.GetComponent<ctr_card_stack>().initial_card(currentDeck);
        for (int i = 0; i < 5; i++) ACTS[i].interactable = false;
        for (int i = 0; i < c.star; i++) ACTS[i].interactable = true;
        POINT.text = (c.star * 500).ToString();
        NAME.text = c.cat_name;
    }
    public void meetCat()
    {
        state_manager._this.Current_Card = currentDeck;
    }
}

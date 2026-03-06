using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class pnl_new_card_control : MonoBehaviour
{
    // Start is called before the first frame update
    public float DestroyTime = 0.1f;
    public float ActivateTime = 0.4f;
    public bool onDisable = false;
    public bool onEnable = false;

    public GameObject THIS;
    public Image card_img;
    List<Image> objs = new List<Image>();
    public GameObject[] STARTS;
    public Text card_text;

    void Start()
    {        
        
    }

    public void initial_card(string img_name, int stars, string catname)
    {
        card_img.sprite = Resources.Load<Sprite>("Card_Clips/" + img_name);
        foreach(GameObject o in STARTS) objs.Add(o.GetComponent<Image>());
        for (int i = 0; i < stars; i++) STARTS[i].SetActive(true);
        card_text.text = "새로운 " + catname + " 카드를\n획득하셨습니다!";
    }
    public void initial_card(card card)
    {
        card_img.sprite = Resources.Load<Sprite>("Card_Clips/" + card.img_name);
        foreach (GameObject o in STARTS) objs.Add(o.GetComponent<Image>());
        for (int i = 0; i < card.star; i++) STARTS[i].SetActive(true);
        card_text.text = "새로운 " + card.cat_name + " 카드를\n획득하셨습니다!";
    }

    // Update is called once per frame
    float t = 0;
    void Update()
    {      

        if (onEnable)
        {
            t += Time.deltaTime;
            Color color = card_img.color;
            color.a = t / ActivateTime;
            card_img.color = color;
            foreach (Image img in objs)
            {
                img.color = color;
            }
            color = card_text.color;
            color.a = t / ActivateTime;
            card_text.color = color;
            
            if (t > ActivateTime)
            {                
                t = 0;
                onEnable = false;
            }
        }
        
        
        
    }
    private void OnEnable()
    {
        onEnable = true;
    }    

    public void touch_to_disable()
    {
        state_manager.update_point();
        Destroy(THIS);
    }
    
    
}

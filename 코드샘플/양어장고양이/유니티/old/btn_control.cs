using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class btn_control : MonoBehaviour
{
    Button my_btn;
    Image panel;
    Image icon;

    // Start is called before the first frame update
    void Start()
    {
        my_btn = (this.gameObject).GetComponent< Button> ();
        GameObject.Find("cnv_main").GetComponent<btn_manager>().add_btn(this.gameObject.name,this.gameObject);

        Transform trIcon = this.transform.Find("icon");
        if(trIcon)
            icon = trIcon.gameObject.GetComponent<Image>();
        panel = gameObject.GetComponent<Image>();
        if(icon)
            icon.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
        if(panel)
            panel.color = new Color(1.0f, 1.0f, 1.0f, 0.3f);

        BtnFadeInColor();
    }
    private void OnDestroy()
    {
        
    }


    // Update is called once per frame
    void Update()
    {
        
    }
    void OnEnable()
    {
             
    }
    public void updateColor(float val)
    {
        icon.color = new Color(1.0f, 1.0f, 1.0f, val);
        panel.color = new Color(1.0f, 1.0f, 1.0f, (val * 0.5f));
    }

    void BtnFadeOutColor()
    {
        //if(icon)
        //    iTween.ValueTo(icon.gameObject, iTween.Hash("from", 1.0f, "to", 0.3f, "easetype", iTween.EaseType.easeOutQuad, "time", 2.0f, "onupdate", "updateColor", "onupdatetarget", this.gameObject, "oncomplete", "BtnFadeInColor", "oncompletetarget", this.gameObject));
    }

    void BtnFadeInColor()
    {
        //if(icon)
        //    iTween.ValueTo(icon.gameObject, iTween.Hash("from", 0.3f, "to", 1.0f, "easetype", iTween.EaseType.easeOutQuad, "time", 2.0f, "onupdate", "updateColor", "onupdatetarget", this.gameObject, "oncomplete", "BtnFadeOutColor", "oncompletetarget", this.gameObject));
    }
}

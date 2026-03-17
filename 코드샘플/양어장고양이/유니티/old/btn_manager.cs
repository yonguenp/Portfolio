using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class btn_manager : MonoBehaviour
{

    public static GameObject _btn_appr;
    public static GameObject _btn_hand;
    public static GameObject _btn_feed;
    public static Dictionary<string, GameObject> btn_pool = new Dictionary<string, GameObject>();
    public void add_btn(string _name, GameObject _btn)
    {
        if (btn_pool.ContainsKey(_name))
            btn_pool.Remove(_name);

        btn_pool.Add(_name, _btn);
    }
    // Start is called before the first frame update
    void Start(){
        Debug.Log("btn_manager");
    }
    
    // Update is called once per frame
    void Update(){}

    public void Off_All()
    {
        foreach(GameObject _btn in btn_pool.Values)
        {
            if(_btn.name != "btn_idle" && _btn.name != "btn_find" && _btn.name != "btn_card") 
                _btn.SetActive(false);
        }
    }
    public void ON_All()
    {
        foreach (GameObject _btn in btn_pool.Values)
        {
            _btn.SetActive(true);
        }
    }
    public void ON(string _name, float x, float y)
    {
        if (btn_pool.ContainsKey(_name))
        {
            btn_pool[_name].SetActive(true);
            if (_name != "btn_idle")
            {
                Vector3 _pos = new Vector3(x, y, 0);
                btn_pool[_name].transform.localPosition = _pos;
            }
        }
        
    }
    public void OFF(string _name)
    {
        if (btn_pool.ContainsKey(_name)) btn_pool[_name].SetActive(false);
    }
    public GameObject get_btn_obj(string _name)
    {
        if (btn_pool.ContainsKey(_name)) return btn_pool[_name];
        else return null;
    }
    public Button get_btn_com(string _name)
    {
        if (btn_pool.ContainsKey(_name)) return btn_pool[_name].GetComponent<Button>();
        else return null;
    }

    public void OnSkipBtn()
    {
        if (btn_pool.ContainsKey("btn_skip"))
        {
            btn_pool["btn_skip"].SetActive(true);

            btn_pool["btn_skip"].transform.position = new Vector3(Screen.width - (Screen.width / 15), 70, 0);
            btn_pool["btn_idle"].transform.localPosition = new Vector3(btn_pool["btn_skip"].transform.localPosition.x, btn_pool["btn_skip"].transform.localPosition.y + btn_pool["btn_idle"].GetComponent<RectTransform>().rect.height, 0);
        }
    }
}

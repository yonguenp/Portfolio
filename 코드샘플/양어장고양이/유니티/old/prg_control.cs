using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class prg_control : MonoBehaviour
{
    // Start is called before the first frame update
    GameObject _this;
    Image _bar;    
    float _time_limit = 0;
    
    void Start()
    {
        _this = this.gameObject;
        _bar = transform.Find("img_guage").GetComponent<Image>();
        Debug.Log("prg_control");
    }
    
    // Update is called once per frame
    void Update()
    {
        if (_bar.fillAmount > 0.0f)
        {
            _bar.fillAmount -= (Time.deltaTime / _time_limit);
        }
        else _this.SetActive(false);
    }

    public void run_progress(float _time)
    {
        _time_limit = _time - 2.5f;
        _bar.fillAmount = 1.0f;
        _this.SetActive(true);
    }
    public void off_progress() { _this.SetActive(false);  }

}


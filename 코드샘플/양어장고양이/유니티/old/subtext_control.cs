using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class subtext_control : MonoBehaviour
{
    // Start is called before the first frame update
    GameObject _this;
    Text _text;
    
    Vector3 originPosition;    
    float max_shake = 5.0f;
    private float update = 0.0f;

    void Start()
    {
        _this = this.gameObject;
        _text = _this.GetComponent<Text>();
    }

    public void on_sub_Text(string _sub, bool center = false)
    {
        _text.text = _sub;
        _this.SetActive(true);

        //originPosition = new Vector3(0, center ? 0 : 251, 0);
        //transform.localPosition = originPosition;
    }

    public void off()
    {
        _this.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //update += Time.deltaTime;
        //if (update > 0.3f)
        //{
        //    update = 0.0f;

        //    Vector3 pos = transform.localPosition - new Vector3(Random.Range(-max_shake, max_shake), Random.Range(-max_shake, max_shake), 0);
        //    pos.x = originPosition.x + max_shake < pos.x ? originPosition.x + max_shake : pos.x;
        //    pos.x = originPosition.x - max_shake > pos.x ? originPosition.x - max_shake : pos.x;
        //    pos.y = originPosition.y + max_shake < pos.y ? originPosition.y + max_shake : pos.y;
        //    pos.y = originPosition.y - max_shake > pos.y ? originPosition.y - max_shake : pos.y;

        //    transform.localPosition = pos;
        //}
    }
}

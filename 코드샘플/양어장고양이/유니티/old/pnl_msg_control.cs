using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pnl_msg_control : MonoBehaviour
{
    // Start is called before the first frame update
    public float DestroyTime = 3.0f;
    void Start()
    {
       
    }



    // Update is called once per frame
    float t = 0;
    void Update()
    {
        t += Time.deltaTime;
        if(t > DestroyTime)
        {
            GameObject pnl_msg = GameObject.Find("pnl_msg");
            pnl_msg.SetActive(false);
            t = 0;
            
        }
    }
    private void OnDisable()
    {
        state_manager.update_point();
        
    }
    
}

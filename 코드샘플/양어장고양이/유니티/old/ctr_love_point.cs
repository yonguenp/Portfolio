using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class ctr_love_point : MonoBehaviour
{
    public Text _point;
    public Text _maxP;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Awake()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        int mp = 500;
        if (state_manager._this.Current_Card != null) 
            mp = state_manager._this.Current_Card.star * 500;
        _maxP.text = "/" + mp.ToString();
        _point.text = state_manager.JT_point.ToString();
    }
    private void OnEnable()
    {
        
    }
}

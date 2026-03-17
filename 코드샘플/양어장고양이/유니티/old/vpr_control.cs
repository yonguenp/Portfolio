using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
public class vpr_control : MonoBehaviour
{

    public VideoPlayer _vp;
    // Start is called before the first frame update
    public void stop_vr(){ _vp.Stop(); }
    public void play_vr() { _vp.Play(); }

    public void set_clip(VideoClip _clip) { _vp.clip = _clip; }

    void Start()
    {
        _vp = this.gameObject.GetComponent<VideoPlayer>();
        GameObject.Find("cnv_main").GetComponent<vpr_manager>().SetVideoPlayer(_vp);
        Debug.Log("vpr_control");
    }
    
    // Update is called once per frame
    void Update(){}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public enum action_type { Approach=1, Handling=2, Feeding=3}
public class vpr_manager : MonoBehaviour
{
    // Start is called before the first frame update
    public static VideoPlayer videoPlayer = null;
    public static int current_player = 1;
    public prg_control ctr_prg;
    public static subtext_control ctr_sub;
    public static state_manager mng_state;
    public static btn_manager mgr_btn;

    public touch_event pnl_happy;

    bool isPause = false;
    public void SetVideoPlayer(VideoPlayer _vpr) {
        videoPlayer = _vpr;
    }
    public state_manager st_mgr;
    public static GameObject _pnl_blank;
    public static Image _pnl_blank_image;
    public static bool changing_clip = false;
    bool use_interactionTool = false;
    public void UseInteractionTool() { use_interactionTool = true; }

    Coroutine clipPlayCoroutine = null;
    Coroutine clipInteractionCorutine = null;

    void Start()
    {
        _pnl_blank = GameObject.Find("pnl_blank");
        if (_pnl_blank == null)
            return;
        
        _pnl_blank_image = _pnl_blank.GetComponent<Image>();
        _pnl_blank.SetActive(false);

        current_player = 1;
        mng_state = GameObject.Find("cnv_main").GetComponent<state_manager>();
        ctr_prg = GameObject.Find("prg_time").GetComponent<prg_control>();
        ctr_sub = GameObject.Find("txt_subtext").GetComponent<subtext_control>();
        mgr_btn = this.gameObject.GetComponent<btn_manager>();
        
        pnl_happy = GameObject.Find("pnl_happy").GetComponent<touch_event>();
        pnl_happy.gameObject.SetActive(false);
        pnl_happy.SetVideoPlayerManager(this);
    }
    public IEnumerator ClipInteraction(ha_clip _vcp)
    {
        float t = 0.0f;
        while (true)
        {
            if (isPause == false)
                t += Time.deltaTime;

            pnl_happy.SetInteraction(true, new Vector2(0,200), new Vector2(800, 900), touch_event.INTERACTION_TYPE.TAP);           

            yield return null;
        }
    }

    public IEnumerator ClipPlay(ha_clip _vcp, bool is_finding) // 알파값 0에서 1로 전환
    {
        pnl_happy.gameObject.SetActive(false);

        //ctr_prg = GameObject.Find("prg_time").GetComponent<prg_control>();
        //ctr_sub = GameObject.Find("txt_subtext").GetComponent<subtext_control>();
        ctr_sub.off();
        float t = 1.0f;
        while (t > 0.0f)
        {
            t -= Time.deltaTime / 0.5f;
            yield return null;
        }
        
        
        _pnl_blank.SetActive(true);
        _pnl_blank_image.color = new Color(_pnl_blank_image.color.r, _pnl_blank_image.color.g, _pnl_blank_image.color.b, 0);
       
        while (_pnl_blank_image.color.a < 1.0f)
        {
            _pnl_blank_image.color = new Color(_pnl_blank_image.color.r, _pnl_blank_image.color.g, _pnl_blank_image.color.b, _pnl_blank_image.color.a + (Time.deltaTime / 0.5f));
            yield return null;
        }

        videoPlayer.Stop();                
        videoPlayer.clip = null;
        
        VideoClip clip = Resources.Load<VideoClip>("Game_Clips/"+_vcp._clip);
        videoPlayer.clip = clip;
        videoPlayer.Play();

        videoPlayer.isLooping = clip && clip.length + 1 < _vcp._time;

        while (true)
        {
            if (videoPlayer.clip == null || videoPlayer.isPlaying) break;
            yield return null;
        }
        t = 1.0f;
        while (t > 0.0f)
        {
            t -= Time.deltaTime / 0.3f;
            yield return null;
        }
        while (_pnl_blank_image.color.a > 0.0f)
        {
            _pnl_blank_image.color = new Color(_pnl_blank_image.color.r, _pnl_blank_image.color.g, _pnl_blank_image.color.b, _pnl_blank_image.color.a - (Time.deltaTime / 0.7f));
            yield return null;
        }
        _pnl_blank.SetActive(false);
        t = 1.0f;
        while (t > 0.0f)
        {
            t -= Time.deltaTime / 0.5f;
            yield return null;
        }
        
        //pnl_happy.gameObject.SetActive(state_manager.CURRENT_STATE == GAME_STATE.TOUCH || state_manager.CURRENT_STATE == GAME_STATE.TOUCH2 || state_manager.CURRENT_STATE == GAME_STATE.TOUCH3);
        ctr_sub.on_sub_Text(_vcp._sub, videoPlayer.clip == null);

        for (int i = 0; i < _vcp._btns.Count; i++)
        {
            mgr_btn.ON(_vcp._btns[i], _vcp._btn_pos[i].x, _vcp._btn_pos[i].y);
        }
        if (state_manager.CURRENT_STATE == GAME_STATE.IDLE || state_manager.CURRENT_STATE == GAME_STATE.GO_MAIN)
        {
            mng_state.pnl_love_point.SetActive(true);
            state_manager.CURRENT_STATE = GAME_STATE.IDLE;
        }
        //mgr_btn.OnSkipBtn();

        if (_vcp.is_guage)
            ctr_prg.run_progress(_vcp._time);
        else
            ctr_prg.off_progress();

        if (_vcp.is_pause)
            videoPlayer.Pause();
        else
        {
           
            mng_state.Time_Advance();
        }
        
    }

    public void Play_Next_Clip(ha_clip _vcp, bool is_finding)
    {
        if (clipPlayCoroutine != null)
            StopCoroutine(clipPlayCoroutine);

        clipPlayCoroutine = StartCoroutine(ClipPlay(_vcp, is_finding));

        if (clipInteractionCorutine != null)
            StopCoroutine(clipInteractionCorutine);

       if(state_manager.CURRENT_STATE == GAME_STATE.TOUCH 
            || state_manager.CURRENT_STATE == GAME_STATE.TOUCH2 
            || state_manager.CURRENT_STATE == GAME_STATE.TOUCH3
            || state_manager.CURRENT_STATE == GAME_STATE.PLAY
            || state_manager.CURRENT_STATE == GAME_STATE.BORING            
        )
            clipInteractionCorutine = StartCoroutine(ClipInteraction(_vcp));

        isPause = false;
    }
   
    // Update is called once per frame
    void Update(){
        /*if (isPause)
        {
            if(videoPlayer.playbackSpeed > 0.0f)
            {
                videoPlayer.playbackSpeed -= Time.deltaTime;
                state_manager.PAUSE_TIME += Time.deltaTime * videoPlayer.playbackSpeed;
            }
            else
            {
                videoPlayer.Pause();
                state_manager.PAUSE_TIME += Time.deltaTime;
            }
        }
        else
        {
            videoPlayer.playbackSpeed = 1.0f;
        }*/
    }

    public void pause_current_clip()
    {
        isPause = true;
    }

    public void pause_current_clip_immediately()
    {
        isPause = true;
        videoPlayer.playbackSpeed = 0.0f;
        videoPlayer.Pause();
    }

    public void resume_current_clip()
    {
        isPause = false;
        videoPlayer.playbackSpeed = 1.0f;
        videoPlayer.Play();
    }

    public void stop_current_clip()
    {
        videoPlayer.Pause();
    }

    public void play_current_clip()
    {
        videoPlayer.Play();
    }
}



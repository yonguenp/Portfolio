#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
//#define INTERACTION_TOOL
#endif

using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


/*public enum GAME_STATE {     
    IDLE,
    COOK,
    WALK,
    WALK_MEET,
    SHOW,
    WAIT,
    EAT,
    TOUCH,    
    BYE,
    INTERACTION,
}*/
public enum GAME_STATE
{
    START,    
    FIND,
    IDLE,
    ANGRY,
    TOUCH,
    TOUCH2,
    TOUCH3,
    EAT,
    PLAY,
    BORING,    
    FINISH,
    GET_CARD,
    GO_MAIN,
    GO_CARD,
    GO_FIND
}
public enum user_action { start_find=0, approach=1, touch=2, feed=3, happy_touch=4 }

public class card
{
    public int c_idx;
    public string cat_name;
    public string img_name;
    public int star;
    public int[] attr = { 1, 1, 0, 0, 0 };    
    public int count = 0;
}

public class state_manager : MonoBehaviour
{
    public string scenestate = "Main";
    public bool ToolMode;

    public static int USER_POINT;

    public static GAME_STATE CURRENT_STATE;
    public static double CURRENT_SIGMA;
    public static double CURRENT_START_AT;
    public static double ELAPSED_TIME;
    public static double PAUSE_TIME;
    public static GameObject _pnl_card;
    public static GameObject _pnl_card_list;
    public static state_manager _this;
    public static GameObject _cnv_main;
    public btn_manager mgr_btn;
    public vpr_manager mgr_vpr;
    public clip_manager mgr_clip;
    public prg_control ctr_prg;
    public subtext_control ctr_sub;
    public GameObject pnl_msg;
    public GameObject txt_point_msg;
    public GameObject txt_point_total;
    public GameObject pnl_new_card;
    public GameObject pnl_card_list;
    public GameObject pnl_love_point;
    public Image img_love_gauge;
    public InteractionManager interactionManager = null;
    //int cnt_happytime = 0;
    public int _target_cat = 0;
    public int _target_food = 1;
    int debug_food = 0;
    // Start is called before the first frame update
    void Start()
    {
        _pnl_card = this.pnl_new_card;
        _pnl_card_list = this.pnl_card_list;
        _cnv_main = this.gameObject;
        _this = this;
        StartCoroutine(loading_scene());
    }

    // Update is called once per frame
    void Update(){}
    public static float getNow()
    {
        return DateTime.Now.Hour * 60 * 60 + DateTime.Now.Minute * 60 + DateTime.Now.Second + (DateTime.Now.Millisecond * 0.001f);
    }
    public void initial_state()
    {
        mgr_btn.Off_All();
        Next_State(GAME_STATE.IDLE);


            
    }
    public void run_search()
    { 
        CURRENT_START_AT = getNow();
        mgr_vpr.play_current_clip();
        StartCoroutine(time_advance());
    }
    public static void update_point()
    {
        int maxP = 500;
        if (state_manager._this.Current_Card != null) maxP = 500 * state_manager._this.Current_Card.star;
        if (JT_point - maxP >= 0)
        {
            JT_point -= maxP;
            state_manager._this.get_new_card();
            //_pnl_card.SetActive(true);
        }

        _this.img_love_gauge.fillAmount = (float)JT_point / (float)maxP;       
        
    }




    string[] card_names = { "sam1", "mar1", "sam2", "sam3"  };
    int[] card_stars = { 2, 3, 4, 5 };
    string[] card_cats = { "삼색이", "marlyn", "삼색이", "삼색이" };
    int[] card_idx = { 0, 1, 2, 3 };
    public card Current_Card = null;
    public Dictionary<string, List<card>> user_card = new Dictionary<string, List<card>>();
    public Dictionary<int, card> idx_card = new Dictionary<int, card>();
    card add_card(string c_name, int star,  string img_name, int cidx)
    {
        if (idx_card.ContainsKey(cidx)) { idx_card[cidx].count++; return idx_card[cidx]; }
        else
        {
            card temp = new card();
            temp.cat_name = c_name;
            temp.img_name = img_name;
            temp.star = star;
            for (int i = 0; i < star; i++) temp.attr[i] = 1;
            temp.c_idx = cidx;
            temp.count = 1;
            if (user_card.ContainsKey(c_name) == false) user_card[c_name] = new List<card>();
            user_card[c_name].Add(temp);
            idx_card[temp.c_idx] = temp;
            this.pnl_card_list.GetComponent<mng_card_list>().add_Card(temp);
            return idx_card[temp.c_idx];
        }
    }
    void init_card_list_dev()
    {
        add_card(card_names[0], card_stars[0], card_cats[0], card_idx[0]);
    }
    
    public void get_new_card()
    {
        GameObject prefab = Resources.Load("Prefabs/pnl_new_card") as GameObject;        
        GameObject new_card = MonoBehaviour.Instantiate(prefab) as GameObject;
        int cc_star = 0;
        if (Current_Card != null) cc_star = Current_Card.star;
        
        int card_idx = r.Next(0, cc_star); if (card_idx > 3) card_idx = 3;
        new_card.GetComponent<pnl_new_card_control>().initial_card(add_card(card_cats[card_idx],card_stars[card_idx],card_names[card_idx], this.card_idx[card_idx]));
        new_card.transform.SetParent(_cnv_main.transform);
        new_card.transform.SetAsLastSibling();
        new_card.transform.localScale = Vector3.one;
        //new_card.transform.GetComponent<RectTransform>().pivot = Vector2.zero;
        //new_card.transform.position = new Vector3(0, 0, 0);
        new_card.GetComponent<RectTransform>().offsetMin = new Vector2(0,0);
        new_card.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);

    }



    public void check_point(GAME_STATE _nxst)
    {
        if (_nxst == GAME_STATE.IDLE || _nxst == GAME_STATE.FINISH)
        {
            if (J_point > 0)
            {   
                Text t = txt_point_msg.GetComponent<Text>();
                t.text = "삼색이에게 정을\n" + J_point.ToString() + "만큼 주었습니다.";
                JT_point += J_point;
                J_point = 0;
                pnl_msg.SetActive(true);                
            }
        }
    }
    public void external_event(int state)
    {
        mgr_btn.Off_All();
        if (state == 13)
        {
            this.pnl_card_list.SetActive(true);
        }
        else if(state == 4)
        {
            Next_State((GAME_STATE)(state + r.Next(0, 2)));
        }
        else
        {
            this.pnl_card_list.SetActive(false);
            Next_State((GAME_STATE)state);
        }
    }
    int idle_count = 0;
    public void timeout_event()
    {
        mgr_btn.Off_All();
        switch (CURRENT_STATE)
        {
            case GAME_STATE.START:
                Next_State(GAME_STATE.FIND);
                break;
            case GAME_STATE.FIND:
                scenestate = "Main";
                Next_State(GAME_STATE.IDLE);
                break;
            case GAME_STATE.IDLE:
                if (scenestate == "Main")
                {
                    Next_State(GAME_STATE.IDLE);
                }
                else
                {
                    if (idle_count < 5)
                    {
                        idle_count++;
                        Next_State(GAME_STATE.IDLE);
                    }
                    else
                    {
                        idle_count = 0;
                        Next_State(GAME_STATE.FINISH);
                    }
                }
                break;
            case GAME_STATE.ANGRY:
                Next_State(GAME_STATE.IDLE);
                break;
            case GAME_STATE.TOUCH:
                if (scenestate == "Main"){
                    int i = r.Next(0, 4);
                    if(i == 0) Next_State(GAME_STATE.TOUCH2);
                    else if (i == 1) Next_State(GAME_STATE.TOUCH3);                    
                    else if (i == 2) Next_State(GAME_STATE.BORING);
                    else Next_State(GAME_STATE.IDLE);
                }
                else Next_State(GAME_STATE.IDLE);
                break;
            case GAME_STATE.TOUCH2:
                if (scenestate == "Main")
                {
                    int i = r.Next(1, 4);
                    
                    if (i == 1) Next_State(GAME_STATE.TOUCH3);                    
                    else if (i == 2) Next_State(GAME_STATE.BORING);
                    else Next_State(GAME_STATE.IDLE);
                }
                else Next_State(GAME_STATE.IDLE);                
                break;
            case GAME_STATE.TOUCH3:
                if (scenestate == "Main")
                {
                    int i = r.Next(3, 5);
                    
                    
                    if (i == 3) Next_State(GAME_STATE.BORING);
                    else Next_State(GAME_STATE.IDLE);
                }
                else Next_State(GAME_STATE.IDLE);
                break;
            case GAME_STATE.EAT:
                if (scenestate == "Main")
                {
                    Next_State(GAME_STATE.TOUCH);
                }
                else
                {
                    Next_State(GAME_STATE.TOUCH);
                }
                break;
            case GAME_STATE.PLAY:
                Next_State(GAME_STATE.IDLE);
                break;
            case GAME_STATE.BORING:
                Next_State(GAME_STATE.IDLE);
                break;
            case GAME_STATE.FINISH:
                scenestate = "Main";
                Next_State(GAME_STATE.IDLE);
                break;
            case GAME_STATE.GET_CARD:
                scenestate = "GetCard";
                break;
            case GAME_STATE.GO_MAIN:
                scenestate = "Main";
                Next_State(GAME_STATE.IDLE);
                break;
            case GAME_STATE.GO_CARD:
                scenestate = "CardList";
                

                break;
            case GAME_STATE.GO_FIND:
                scenestate = "Find";
                Next_State(GAME_STATE.START);
                break;
            default:
                Debug.LogError("매칭되는게 없네요");
                Next_State(GAME_STATE.IDLE);
                break;
        }

        check_point(CURRENT_STATE);
    }
    public ha_clip current_clip;
    System.Random r = new System.Random();
    public static int J_point = 0;
    public static int JT_point = 0;
    void Next_State(GAME_STATE _nxst)
    {

        ctr_prg.off_progress();
        if (_nxst == GAME_STATE.TOUCH)
        {
            int i = r.Next(0, 6);
            if (i == 0) Next_State(GAME_STATE.TOUCH2);
            else if (i == 1) Next_State(GAME_STATE.TOUCH3);
            else if (i == 2) Next_State(GAME_STATE.PLAY);
            else if (i == 3) Next_State(GAME_STATE.BORING);
            else Next_State(GAME_STATE.TOUCH);
        }

        _nxst = CheckBeforeEvent(_nxst);
       this.pnl_love_point.SetActive(false);
        
        

        if (_nxst == GAME_STATE.TOUCH)
        {
            if (r.Next(0, 100) > 75) _nxst = GAME_STATE.ANGRY;
        }

        current_clip = mgr_clip.get_clip(_nxst, 0, _target_food);

        float sigma = 5;
        if (current_clip == null)
        {
            if (GAME_STATE.EAT == _nxst || GAME_STATE.START == _nxst)
            {
                Next_State(GAME_STATE.FINISH);
                return;
            }

            ctr_sub.on_sub_Text("영상클립이 없습니다. 현재 상태는 " + _nxst.ToString() + "입니다.", true);
            mgr_btn.ON_All();
        }
        else
        {
            sigma = current_clip._time;
        }
        
        
        CheckAfterEvent(_nxst);
        
        PAUSE_TIME = 0;
        CURRENT_STATE = _nxst;
        CURRENT_SIGMA = sigma;
        CURRENT_START_AT = getNow();


        
        mgr_vpr.Play_Next_Clip(current_clip, false);
        

    }

    GAME_STATE CheckBeforeEvent(GAME_STATE state)
    {
        /*switch (state)
        {
            case GAME_STATE.IDLE:
                if(CURRENT_STATE != GAME_STATE.IDLE && CURRENT_STATE != GAME_STATE.COOK)
                {
                    _target_food = 1;
                }
                return GAME_STATE.IDLE;
            case GAME_STATE.TOUCH:                
                if (mgr_clip.IsLastClip(GAME_STATE.TOUCH, _target_cat, _target_food))
                {
                    mgr_clip.ResetClip(GAME_STATE.TOUCH, _target_cat, _target_food);
                    return GAME_STATE.BYE;
                }
                break;
            case GAME_STATE.COOK:
                // _target_food = UnityEngine.Random.Range(2, 6); //2~5
                debug_food += 1;
                _target_food = _target_food + debug_food;
                if (_target_food > 5)
                {
                    _target_food = 2;
                    debug_food = 0;
                }
                return GAME_STATE.COOK;
            case GAME_STATE.WALK:
                if (UnityEngine.Random.Range(0f, 10f) <= 5.0f)
                {
                    //_target_cat = UnityEngine.Random.Range(1, 4); //1~3
                    _target_cat = _target_cat + 1;
                    if (_target_cat > 3)
                        _target_cat = 1;

                    mgr_clip.ResetClip(GAME_STATE.TOUCH, _target_cat, _target_food);
                    mgr_clip.ResetClip(GAME_STATE.EAT, _target_cat, _target_food);

                    return GAME_STATE.WALK_MEET;
                }
                else
                {
                    return GAME_STATE.WALK;
                }                
            case GAME_STATE.EAT:
                if (mgr_clip.IsLastClip(GAME_STATE.EAT, _target_cat, _target_food))
                {
                    mgr_clip.ResetClip(GAME_STATE.EAT, _target_cat, _target_food);
                    return GAME_STATE.BYE;
                }
                break;
            case GAME_STATE.INTERACTION:
                if (mgr_clip.IsLastClip(GAME_STATE.INTERACTION, _target_cat, _target_food))
                {
                    mgr_clip.ResetClip(GAME_STATE.INTERACTION, _target_cat, _target_food);
                    return GAME_STATE.BYE;
                }
                break;
        }*/

        return state;
    }

    void CheckAfterEvent(GAME_STATE state)
    {
        switch (state)
        {
            case GAME_STATE.IDLE:                
                break;
        }
    }

    public void Time_Advance()
    {
        StartCoroutine(time_advance());
    }

    public IEnumerator time_advance()
    {
        while (getNow() - (CURRENT_START_AT + PAUSE_TIME) < CURRENT_SIGMA) {   
            
            yield return null;
        }       
        timeout_event();        
    }
    public void Off_UI_All(){}

    public IEnumerator loading_scene()
    {
        while (true)
        {
            if (mgr_btn != null && mgr_vpr != null && mgr_clip != null) 
                break;

            mgr_btn = this.gameObject.GetComponent<btn_manager>();
            mgr_vpr = this.gameObject.GetComponent<vpr_manager>();
            mgr_clip = this.gameObject.GetComponent<clip_manager>();
            ctr_prg = GameObject.Find("prg_time").GetComponent<prg_control>();
            ctr_sub = GameObject.Find("txt_subtext").GetComponent<subtext_control>();

            mgr_vpr.UseInteractionTool();

#if INTERACTION_TOOL
            Transform InteractionManagerTransform = this.gameObject.transform.Find("InteractionManager");
            if (InteractionManagerTransform)
            {
                interactionManager = InteractionManagerTransform.GetComponent<InteractionManager>();

                if (interactionManager)
                {
                    interactionManager.gameObject.SetActive(true);
                    interactionManager.UseEditMode(false);
                }
            }
#endif

            yield return null;
        }

        initial_state();
#if INTERACTION_TOOL
        mgr_vpr.pause_current_clip_immediately();
#endif
    }


#if INTERACTION_TOOL
    void OnGUI()
    {
        // Make a background box
        GUI.Box(new Rect(10, 10, 150, 110), "Edit Tool");

        if (GUI.Button(new Rect(20, 30, 130, 20), "Edit Mode Control"))
        {
            ToolMode = true;            
        }

        if (GUI.Button(new Rect(20, 60, 130, 20), "Restart Clip"))
        {
            interactionManager.Restart();            
        }
        
        if (GUI.Button(new Rect(20, 90, 130, 20), "Pause/Resume"))
        {
            if (mgr_vpr != null && vpr_manager.videoPlayer != null)
            {
                if(vpr_manager.videoPlayer.isPlaying)
                    mgr_vpr.pause_current_clip_immediately(); 
                else
                    mgr_vpr.resume_current_clip();
            }
        }

        if (ToolMode)
        {
            GUI.Box(new Rect(10, 120, 150, 70), "Edit Menu");
            if (GUI.Button(new Rect(20, 140, 20, 20), "<"))
            {
                interactionManager.PrevData();
            }

            if (GUI.Button(new Rect(130, 140, 20, 20), ">"))
            {
                interactionManager.NextData();
            }

            if (GUI.Button(new Rect(45, 140, 40, 20), "삭제"))
            {
                interactionManager.RemoveCurData();
            }

            if (GUI.Button(new Rect(85, 140, 40, 20), "추가"))
            {
                interactionManager.UseEditMode(true);
            }

            if (GUI.Button(new Rect(20, 165, 60, 20), "저장"))
            {
                interactionManager.UseEditMode(true);
                interactionManager.SaveData();
                ToolMode = false;
                interactionManager.UseEditMode(false);
            }

            if (GUI.Button(new Rect(85, 165, 60, 20), "취소"))
            {
                ToolMode = false;
                interactionManager.UseEditMode(false);
            }
        }
    }
#endif
}




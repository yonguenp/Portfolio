using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Text.RegularExpressions;

public class clip_manager : MonoBehaviour
{    
    public static Dictionary<string, ha_clipList> dic_clips = new Dictionary<string, ha_clipList>();
    
    // Start is called before the first frame update
    void Start()
    {
        List<Dictionary<string, string>> data = CSVReader.Read("Data/data");

        for(int i = 0; i < data.Count; i++)
        {
            string st = data[i]["영상 길이"].ToString();
            float ft = 0;

            if(!String.IsNullOrEmpty(st))
            {
                ft = (float)Convert.ToDouble(data[i]["영상 길이"].ToString());
            }
            
            add_clip(data[i]["상태"].ToString(), data[i]["영상 아이디"].ToString(), ft, data[i]["버튼"].ToString(), data[i]["대사"].ToString(), data[i]["정지 여부"].ToString() == "TRUE", data[i]["게이지 여부"].ToString() == "TRUE");
        }

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /*public void ResetClip(GAME_STATE _state, int _cat = -1, int _food = -1)
    {
       
        string clip_strid = "";
        int type = 0;
        switch (_state)
        {
            case GAME_STATE.IDLE:
                clip_strid += "IDLE";
                type = 2;
                break;
            case GAME_STATE.COOK:
                clip_strid += "COOK";
                type = 3;
                break;
            case GAME_STATE.WALK:
                clip_strid += "WALK";
                type = 2;
                break;
            case GAME_STATE.WALK_MEET:
                clip_strid += "WALK_MEET";
                type = 1;
                break;
            case GAME_STATE.SHOW:
                clip_strid += "SHOW";
                type = 1;
                break;
            case GAME_STATE.WAIT:
                clip_strid += "WAIT";
                type = 1;
                break;
            case GAME_STATE.EAT:
                clip_strid += "EAT";
                type = 4;
                break;
            case GAME_STATE.TOUCH:
                clip_strid += "TOUCH";
                type = 1;
                break;
            case GAME_STATE.BYE:
                clip_strid += "BYE";
                type = 1;
                break;
            default:
                Debug.LogError("[error] get_clip : 매칭되는게 없네요");
                break;
        }

        if (type == 0)
            return;

        string clip_info = "";
        switch (type)
        {
            case 1:
                clip_info = _cat.ToString("00");
                break;
            case 2:
                break;
            case 3:
                clip_info = _food.ToString("00");
                break;
            case 4:
                clip_info = _food.ToString("00") + "_" + _cat.ToString("00");
                break;
        }

        if (dic_clips.ContainsKey(clip_strid))
        {
            if (!String.IsNullOrEmpty(clip_info))
                dic_clips[clip_strid].ResetClip(clip_info);
            else
                dic_clips[clip_strid].ResetClip();
        }
    }*/

   /* public bool IsLastClip(GAME_STATE _state, int _cat = -1, int _food = -1)
    {
        string clip_strid = "";
        int type = 0;
        switch (_state)
        {
            case GAME_STATE.IDLE:
                clip_strid += "IDLE";
                type = 2;
                break;
            case GAME_STATE.COOK:
                clip_strid += "COOK";
                type = 3;
                break;
            case GAME_STATE.WALK:
                clip_strid += "WALK";
                type = 2;
                break;
            case GAME_STATE.WALK_MEET:
                clip_strid += "WALK_MEET";
                type = 1;
                break;
            case GAME_STATE.SHOW:
                clip_strid += "SHOW";
                type = 1;
                break;
            case GAME_STATE.WAIT:
                clip_strid += "WAIT";
                type = 1;
                break;
            case GAME_STATE.EAT:
                clip_strid += "EAT";
                type = 4;
                break;
            case GAME_STATE.TOUCH:
                clip_strid += "TOUCH";
                type = 1;
                break;
            case GAME_STATE.BYE:
                clip_strid += "BYE";
                type = 1;
                break;
            case GAME_STATE.INTERACTION:
                clip_strid += "INTERACTION";
                type = -1;
                break;
            default:
                Debug.LogError("[error] get_clip : 매칭되는게 없네요");
                break;
        }

        if (type == 0)
            return false;

        string clip_info = "";
        switch (type)
        {
            case -1:
                break;
            case 1:
                clip_info = _cat.ToString("00");
                break;
            case 2:
                break;
            case 3:
                clip_info = _food.ToString("00");
                break;
            case 4:
                clip_info = _food.ToString("00") + "_" + _cat.ToString("00");
                break;
        }

        if (dic_clips.ContainsKey(clip_strid))
        {
            if (!String.IsNullOrEmpty(clip_info))
                return dic_clips[clip_strid].IsLastClip(clip_info);
            else
                return dic_clips[clip_strid].IsLastClip();
        }

        return false;
    }*/

    public ha_clip get_clip(GAME_STATE _state, int _cat = -1, int _food = -1)
    {
        string clip_strid = "";
        int type = 0;
        switch (_state)
        {
            case GAME_STATE.START: clip_strid += "START"; break;
            case GAME_STATE.FIND: clip_strid += "FIND"; break;
            case GAME_STATE.IDLE: clip_strid += "IDLE"; break;
            case GAME_STATE.ANGRY: clip_strid += "ANGRY"; break;
            case GAME_STATE.TOUCH: clip_strid += "TOUCH"; type = 0; break;
            case GAME_STATE.TOUCH2: clip_strid += "TOUCH"; type = 1; break;
            case GAME_STATE.TOUCH3: clip_strid += "TOUCH"; type = 2; break;
            case GAME_STATE.EAT: clip_strid += "EAT"; break;
            case GAME_STATE.PLAY: clip_strid += "PLAY"; break;
            case GAME_STATE.BORING: clip_strid += "BORING"; break;
            case GAME_STATE.FINISH: clip_strid += "FINISH"; break;
            case GAME_STATE.GET_CARD:break;
            case GAME_STATE.GO_MAIN: clip_strid += "IDLE"; break;
            case GAME_STATE.GO_CARD:break;
            case GAME_STATE.GO_FIND: clip_strid += "START"; break;
        }

        string clip_info = _cat.ToString("00") + "_" + type.ToString("00");

        if (dic_clips.ContainsKey(clip_strid))
        {
            if (!String.IsNullOrEmpty(clip_info))
                return dic_clips[clip_strid].GetClip(clip_info);
            else
                return dic_clips[clip_strid].GetClip();
        }

        return null;
    }

    public ha_clip get_randomClip()
    {
        int i = UnityEngine.Random.Range(0, dic_clips.Count);
        string name = dic_clips.Keys.ToList()[i];
        return dic_clips[name].GetClip();
    }

    public void add_clip(string fileName, string _idx, float _t, 
        string _btns, string _sub, bool _pause, bool _guage)
    {
        ha_clip temp = new ha_clip();
        temp._idx = _idx;
        temp._list_name = fileName;
        temp._time = _t + 1.5f;
        if (_btns != "")
        {
            string[] t_btns = _btns.Split('|');
            if (t_btns.Length > 0)
            {
                foreach (string p in t_btns)
                {
                    string[] t = p.Split(',');
                    temp._btns.Add(t[0]);
                    temp._btn_pos.Add(new Vector2(System.Convert.ToInt32(t[1]), System.Convert.ToInt32(t[2])));
                }
            }
        }
        temp._clip = fileName + "/" +  _idx.ToString();

        temp._sub = _sub;
        temp.is_pause = _pause;
        temp.is_guage = _guage;

        if (!dic_clips.ContainsKey(fileName))
            dic_clips.Add(fileName, new ha_clipList());
        (dic_clips[fileName]).AddClip(temp);
    }
}

public class ha_clip
{
    public string _idx;
    public string _list_name;
    public float _time;
    public List<string> _btns = new List<string>();
    public List<Vector2> _btn_pos = new List<Vector2>();
    public string _clip;
    public string _sub;
    public bool is_pause;
    public bool is_guage;
}

public class ha_clipList
{ 
    List<ha_clip> clips = new List<ha_clip>();
    public class groupClip
    {
        public List<ha_clip> clips = new List<ha_clip>();
        public int play_clip_index = 0;
    }

    Dictionary<string, groupClip> hc = new Dictionary<string, groupClip>();

    static string pt = "_";

    int play_clip_index = 0;
    public void AddClip(ha_clip clip)
    {
        clips.Add(clip);
        
        var values = Regex.Split(clip._idx, pt);
        /*if (values.Length == 2)
        {
            if (!hc.ContainsKey(values[0]))
                hc.Add(values[0], new groupClip());

            hc[values[0]].clips.Add(clip);
        }
        if (values.Length == 3)*/
        //{
            string key = values[0] + "_" + values[1];
            if (!hc.ContainsKey(key))
                hc.Add(key, new groupClip());

            hc[key].clips.Add(clip);
        //}
    }

    public ha_clip GetClip()
    {
        if (IsEmpty())
            return null;

        if (IsLastClip())
            play_clip_index = 0;

        ha_clip ret = clips[play_clip_index++];

        return ret;
    }

    public ha_clip GetClip(string clip_info)
    {
        if (IsEmpty())
            return null;

        string key = "";
        var values = Regex.Split(clip_info, pt);
        if (values.Length == 1)
        {
            key = values[0];
        }
        if (values.Length == 2)
        {
            key = values[0] + "_" + values[1];
        }

        if (!hc.ContainsKey(key))
            return null;

        if (IsLastClip(key))
            hc[key].play_clip_index = 0;

        return hc[key].clips[hc[key].play_clip_index++];
    }

    public bool IsLastClip(string key)
    {
        if (!hc.ContainsKey(key))
            return true;

        return hc[key].play_clip_index >= hc[key].clips.Count;
    }

    public bool IsLastClip()
    {
        return play_clip_index >= clips.Count;
    }

    public void ResetClip(string clip_info)
    {
        string key = "";
        var values = Regex.Split(clip_info, pt);
        if (values.Length == 1)
        {
            key = values[0];
        }
        if (values.Length == 2)
        {
            key = values[0] + "_" + values[1];
        }

        if (!hc.ContainsKey(key))
            return;

        hc[key].play_clip_index = 0;
    }

    public void ResetClip()
    {
        play_clip_index = 0;
    }

    public bool IsEmpty()
    {
        return clips.Count <= 0;
    }
}

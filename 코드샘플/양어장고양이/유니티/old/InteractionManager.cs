using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InteractionManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    class InteractionData
    {
        public class TouchData
        {
            public float time;
            public Vector2 pos;

            public TouchData(float t, Vector2 p) { time = t; pos = p; }
        };

        public string clip_name;
        public float start_time;
        public float end_time;

        public List<TouchData> touchData = new List<TouchData>();        
    };

    private List<InteractionData> interactionData = new List<InteractionData>();
    private InteractionData curEditData = null;
    private InteractionData curInteractionData = null;
    private InteractionData.TouchData curInteractionTouchData = null;

    public GameObject RecodingIcon;
    public GameObject TouchGuide;
    public vpr_manager VideoPlayerManager;

    public GameObject TouchEffectPrefab;
    private GameObject CurTouchEffect = null;

    private string FilePath = "";
    bool isEditMode = false;

    float ratio = 1.0f;
    float gap = 0.0f;
    void Start()
    {
        FilePath = Application.dataPath + "/Resources/Data/InteractionData.txt";
        string str = File.ReadAllText(FilePath);
        StringReader sr = new StringReader(str);
        string line = sr.ReadLine();
        while (line != null)
        {
            InteractionData data = new InteractionData();
            string[] values = line.Split(',');
            foreach (string value in values)
            {                
                string[] val = value.Split(':');
                switch (val[0])
                {
                    case "clip":
                        data.clip_name = val[1];
                        break;
                    case "start":
                        data.start_time = float.Parse(val[1]);
                        break;
                    case "end":
                        data.end_time = float.Parse(val[1]);
                        break;
                    case "touchs":
                        {
                            string[] ts = val[1].Split('/');
                            foreach (string t in ts)
                            {
                                string[] f = t.Split('_');
                                string[] xy = f[1].Split('|');
                                InteractionData.TouchData td = new InteractionData.TouchData(float.Parse(f[0]), new Vector2(float.Parse(xy[0]), float.Parse(xy[1])));
                                data.touchData.Add(td);
                            }
                        }
                        break;
                }
            }
            interactionData.Add(data);
            line = sr.ReadLine();
        }
    }

    public void OnEnable()
    {
        GameObject canvas = GameObject.Find("cnv_main");
        if (canvas)
        {
            Vector2 refSize = canvas.GetComponent<CanvasScaler>().referenceResolution;
            ratio = (Screen.height / refSize.y);
            float width = Screen.width / ratio;
            float refWidth = refSize.x;
            gap = width - refWidth;
            Vector2 curSize = new Vector2(width, refSize.y);
            this.gameObject.GetComponent<RectTransform>().sizeDelta = curSize;
        }
    }

    void Update()
    {
        if (isEditMode)
        {
            curInteractionData = null;
            curInteractionTouchData = null;
            return;
        }

        if (vpr_manager.videoPlayer == null || vpr_manager.videoPlayer.clip == null)
            return;

        float time = (float)vpr_manager.videoPlayer.time;

        if (curInteractionData == null)
        {            
            foreach (InteractionData data in interactionData)
            {
                if (vpr_manager.videoPlayer.clip.name == data.clip_name)
                {
                    if (curInteractionData == null && data.start_time <= time && data.end_time >= time)
                    {
                        for (int i = 0; i < data.touchData.Count; i++)
                        {
                            if (data.touchData[i].time < time)
                            {
                                if (data.touchData.Count == i + 1 || data.touchData[i + 1].time > time)
                                {
                                    curInteractionData = data;
                                    curInteractionTouchData = data.touchData[i];
                                    Vector2 dataPos = curInteractionTouchData.pos;                                    
                                    dataPos.x += (gap * 0.5f);
                                    dataPos *= ratio;
                                    Vector3 pos = Camera.main.transform.InverseTransformPoint(Camera.main.ScreenToWorldPoint(dataPos));
                                    pos.z = Camera.main.transform.position.z * -1;
                                    TouchGuide.transform.localScale = Vector3.one;
                                    TouchGuide.transform.localPosition = pos;                                    
                                    VideoPlayerManager.pause_current_clip_immediately();
                                    break;
                                }
                            }
                        }
                    }
                }

                if (curInteractionData != null)
                    break;
            }
        }
        else
        {
            if(curInteractionData.end_time <= time)
            {
                curInteractionData = null;
                curInteractionTouchData = null;
            }
            else if (curInteractionData.start_time <= time)
            {
                for (int i = 0; i < curInteractionData.touchData.Count; i++)
                {
                    if (curInteractionData.touchData[i].time < time)
                    {
                        if (curInteractionData.touchData.Count == i + 1 || curInteractionData.touchData[i + 1].time > time)
                        {
                            if (curInteractionTouchData == curInteractionData.touchData[i])
                                break;

                            curInteractionTouchData = curInteractionData.touchData[i];
                            Vector2 dataPos = curInteractionTouchData.pos;
                            dataPos.x += (gap * 0.5f);
                            dataPos *= ratio;
                            Vector3 pos = Camera.main.transform.InverseTransformPoint(Camera.main.ScreenToWorldPoint(dataPos));
                            pos.z = Camera.main.transform.position.z * -1;
                            //TouchGuide.transform.localScale = Vector3.one;
                            TouchGuide.transform.localPosition = pos;
                            VideoPlayerManager.pause_current_clip_immediately();
                            break;
                        }
                    }
                }
            }
        }

        TouchGuide.SetActive(curInteractionData != null);
    }

    public void UseEditMode(bool bEdit)
    {
        isEditMode = bEdit;

        RecodingIcon.SetActive(isEditMode);
    }

    public void StartInteractionRecoding(float time, Vector2 position)
    {
        if (curEditData != null)
            return;

        curEditData = new InteractionData();
        curEditData.clip_name = vpr_manager.videoPlayer.clip.name;
        curEditData.start_time = time;
        curEditData.touchData.Add(new InteractionData.TouchData(time, position));
        interactionData.Add(curEditData);
    }

    public void AddInteractionRecoding(float time, Vector2 position)
    {
        if (curEditData == null)
            return;

        if (curEditData.touchData.Count == 0 || curEditData.touchData[curEditData.touchData.Count - 1].time <= time - 0.1f) 
            curEditData.touchData.Add(new InteractionData.TouchData(time, position));
    }

    public void EndInteractionRecoding(float time, Vector2 position)
    {
        if (curEditData == null)
            return;

        curEditData.end_time = time;
        curEditData.touchData.Add(new InteractionData.TouchData(time, position));

        curEditData = null;
    }
    public void OnPointerDown(PointerEventData data)
    {
        if (CurTouchEffect != null)
        {
            Destroy(CurTouchEffect);
            CurTouchEffect = null;
        }

        if (isEditMode)
        {
            float time = (float)(state_manager.getNow() - (state_manager.CURRENT_START_AT + state_manager.PAUSE_TIME));
            StartInteractionRecoding(time, data.position);
        }
        else
        {
            if (curInteractionTouchData != null)
            {
                Vector2 size = Vector2.one * 50 * ratio;
                Vector2 dataPos = curInteractionTouchData.pos;
                dataPos.x += (gap * 0.5f);
                dataPos *= ratio;
                Rect touchRect = new Rect(dataPos - (size / 2), size);
                if (touchRect.Contains(data.position))
                {
                    VideoPlayerManager.resume_current_clip();

                    CurTouchEffect = Instantiate(TouchEffectPrefab);
                    CurTouchEffect.transform.SetParent(Camera.main.transform);
                    CurTouchEffect.transform.localScale = Vector3.one;
                    Vector3 localpos = Camera.main.transform.InverseTransformPoint(Camera.main.ScreenToWorldPoint(data.position));
                    localpos.z = Camera.main.transform.position.z * -1;
                    CurTouchEffect.transform.localPosition = localpos;
                }
            }
        }
    }
    public void OnDrag(PointerEventData data)
    {
        if (isEditMode)
        {
            float time = (float)(state_manager.getNow() - (state_manager.CURRENT_START_AT + state_manager.PAUSE_TIME));
            AddInteractionRecoding(time, data.position);
        }
        else
        {
            if (curInteractionTouchData != null)
            {
                Vector2 size = Vector2.one * 50 * ratio;
                Vector2 dataPos = curInteractionTouchData.pos;
                dataPos.x += (gap * 0.5f);
                dataPos *= ratio;
                Rect touchRect = new Rect(dataPos - (size / 2), size);
                if (touchRect.Contains(data.position))
                {
                    VideoPlayerManager.resume_current_clip();
                    if(CurTouchEffect != null)
                    {
                        Vector3 localpos = Camera.main.transform.InverseTransformPoint(Camera.main.ScreenToWorldPoint(data.position));
                        localpos.z = Camera.main.transform.position.z * -1;
                        CurTouchEffect.transform.localPosition = localpos;
                    }
                    else
                    {
                        CurTouchEffect = Instantiate(TouchEffectPrefab);
                        CurTouchEffect.transform.SetParent(Camera.main.transform);
                        CurTouchEffect.transform.localScale = Vector3.one;
                        Vector3 localpos = Camera.main.transform.InverseTransformPoint(Camera.main.ScreenToWorldPoint(data.position));
                        localpos.z = Camera.main.transform.position.z * -1;
                        CurTouchEffect.transform.localPosition = localpos;
                    }
                }
            }
        }
    }

    public void OnPointerUp(PointerEventData data)
    {
        if (CurTouchEffect != null)
        {
            var particle = CurTouchEffect.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particle)
            {
                var em = ps.emission;
                em.enabled = false;
            }
            Destroy(CurTouchEffect, 3.0f);
            CurTouchEffect = null;
        }

        if (isEditMode)
        {
            float time = (float)(state_manager.getNow() - (state_manager.CURRENT_START_AT + state_manager.PAUSE_TIME));
            EndInteractionRecoding(time, data.position);
        }
        else
        {
            if (curInteractionTouchData != null)
            {
                Vector2 size = Vector2.one * 50 * ratio;
                Vector2 dataPos = curInteractionTouchData.pos;
                dataPos.x += (gap * 0.5f);
                dataPos *= ratio;
                Rect touchRect = new Rect(dataPos - (size / 2), size);
                if (touchRect.Contains(data.position))
                {
                    VideoPlayerManager.resume_current_clip();                    
                }
            }
        }
    }

    public void SaveData()
    {
        if (isEditMode)
        {            
            FileStream fs = new FileStream(FilePath, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);

            foreach (InteractionData data in interactionData)
            {
                List<string> strTouchData = new List<string>();

                foreach (InteractionData.TouchData td in data.touchData)
                {
                    strTouchData.Add(td.time.ToString() + "_" + td.pos.x.ToString() + "|" + td.pos.y.ToString());
                }

                sw.WriteLine("clip:" + data.clip_name + ",start:" + data.start_time.ToString() + ",end:" + data.end_time.ToString() + ",touchs:" + string.Join("/", strTouchData));
            }

            sw.Flush();
            sw.Close();
            fs.Close();
        }
    }

    public void NextData()
    {
        if (vpr_manager.videoPlayer == null || vpr_manager.videoPlayer.clip == null)
            return;

        if (curInteractionData != null)
        {
            for(int i = 0; i < interactionData.Count; i++)
            {
                if(interactionData[i] == curInteractionData)
                {
                    if(i + 1 < interactionData.Count)
                    {
                        curInteractionData = interactionData[i + 1];
                        break;
                    }
                }
            }
        }
        else
        {
            if (interactionData.Count > 0)
                curInteractionData = interactionData[0];            
        }

        if(curInteractionData != null)
        {
            vpr_manager.videoPlayer.time = curInteractionData.start_time;
            state_manager.CURRENT_START_AT = state_manager.getNow() - curInteractionData.start_time;
            state_manager.CURRENT_SIGMA = vpr_manager.videoPlayer.clip.length;
            state_manager.PAUSE_TIME = 0;
        }
    }

    public void PrevData()
    {
        if (vpr_manager.videoPlayer == null || vpr_manager.videoPlayer.clip == null)
            return;

        if (curInteractionData != null)
        {
            for (int i = 0; i < interactionData.Count; i++)
            {
                if (interactionData[i] == curInteractionData)
                {
                    if (i - 1 >= 0)
                    {
                        curInteractionData = interactionData[i - 1];
                        break;
                    }
                }
            }
        }
        else
        {
            if (interactionData.Count > 0)
                curInteractionData = interactionData[interactionData.Count - 1];
        }

        if (curInteractionData != null)
        {
            vpr_manager.videoPlayer.time = curInteractionData.start_time;
            state_manager.CURRENT_START_AT = state_manager.getNow() - curInteractionData.start_time;
            state_manager.CURRENT_SIGMA = vpr_manager.videoPlayer.clip.length;
            state_manager.PAUSE_TIME = 0;
        }
    }

    public void RemoveCurData()
    {
        if (vpr_manager.videoPlayer == null || vpr_manager.videoPlayer.clip == null)
            return;

        if (curInteractionData != null)
        {
            interactionData.Remove(curInteractionData);
            curInteractionData = null;
            curInteractionTouchData = null;
        }
    }

    public void Restart()
    {
        curInteractionData = null;
        curInteractionTouchData = null;

        vpr_manager.videoPlayer.time = 0;
        state_manager.CURRENT_START_AT = state_manager.getNow();
        state_manager.CURRENT_SIGMA = vpr_manager.videoPlayer.clip.length;
        state_manager.PAUSE_TIME = 0;

        VideoPlayerManager.resume_current_clip();
    }
}

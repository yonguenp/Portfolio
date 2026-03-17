using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Spine.Unity;
using UnityEngine.Video;

public class touch_event : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public enum INTERACTION_TYPE { 
        NONE = 0,
        TAP = (1 << 0),
        STROKE = (1 << 1),
        RUB = (1 << 2),
        GRIP = (1 << 3),
        ALL = (1 << 4) - 1,
    };

    public GameObject TAP_EFFECT;
    public GameObject STROKE_EFFECT;
    public GameObject RUB_EFFECT;
    public GameObject GRIP_EFFECT;
    
    public SkeletonGraphic GUIDE_UI;
    public SkeletonDataAsset[] GUIDE_ANI;
    public state_manager sm = null;
    public vpr_manager videoPlayerManager = null;
    INTERACTION_TYPE CUR_ENABLE_EFFECT = INTERACTION_TYPE.NONE;
    GameObject CUR_EFFECT = null;

    public List<GameObject> particles = new List<GameObject>();
    public List<PointerEventData> pointerData = new List<PointerEventData>();

    public void SetVideoPlayerManager(vpr_manager vp)
    {
        videoPlayerManager = vp;
    }

    public void SetInteraction(bool active, Vector2 pos, Vector2 size, INTERACTION_TYPE type)
    {
        bool needGuide = false;
        if (this.gameObject.activeSelf == false && active == true)
        {
            videoPlayerManager.pause_current_clip();
            needGuide = true;
        }
        else if (this.gameObject.activeSelf == true && active == false)
        {
            videoPlayerManager.resume_current_clip();
        }
        
        this.gameObject.SetActive(active);
        RectTransform rtTrsf = this.gameObject.GetComponent<RectTransform>();
        rtTrsf.localPosition = pos;
        rtTrsf.sizeDelta = size;


        CUR_ENABLE_EFFECT = type;

        if(needGuide)
        {
            int index = -1;
            switch(type)
            {
                case INTERACTION_TYPE.TAP:
                    index = 0;
                    break;
                case INTERACTION_TYPE.RUB:
                case INTERACTION_TYPE.STROKE:
                    index = 1;
                    break;
            }

            if (index >= 0)
            {
                GUIDE_UI.gameObject.SetActive(true);
                GUIDE_UI.skeletonDataAsset = GUIDE_ANI[index];
                GUIDE_UI.Initialize(true);
            }
            else
            {
                GUIDE_UI.gameObject.SetActive(false);
            }
        }
    }

    private void Start()
    {

    }

    void Update()
    {
        //GUIDE_UI.gameObject.SetActive(CUR_EFFECT == null);
    }

    void OnEnable()
    {
        foreach(GameObject obj in particles)
        {
            Destroy(obj);
        }

        particles.Clear();
    }

    void OnDisable()
    {
        CUR_ENABLE_EFFECT = INTERACTION_TYPE.NONE;

        foreach (GameObject obj in particles)
        {
            Destroy(obj);
        }

        particles.Clear();
    }

    public void TapEnd()
    {
        if(this.gameObject.activeSelf)
            videoPlayerManager.pause_current_clip();
    }
    public void OnPointerDown(PointerEventData data)
    {
        pointerData.Add(data);

        if ((CUR_ENABLE_EFFECT & INTERACTION_TYPE.TAP) != INTERACTION_TYPE.NONE)
        {
            GameObject go = Instantiate(TAP_EFFECT);
            go.transform.SetParent(Camera.main.transform);
            go.transform.localScale = Vector3.one;
            Vector3 localpos = Camera.main.transform.InverseTransformPoint(Camera.main.ScreenToWorldPoint(data.position));
            localpos.z = Camera.main.transform.position.z * -1;
            go.transform.localPosition = localpos;
            Destroy(go, 3.0f);

            videoPlayerManager.resume_current_clip();
            CancelInvoke("TapEnd");
            Invoke("TapEnd", 1.0f);
            state_manager.J_point += 100;
        }

        if ((CUR_ENABLE_EFFECT & INTERACTION_TYPE.STROKE) != INTERACTION_TYPE.NONE)
        {
            if (CUR_EFFECT)
            {
                Destroy(CUR_EFFECT);
                CUR_EFFECT = null;
            }

            CUR_EFFECT = Instantiate(STROKE_EFFECT);
            CUR_EFFECT.transform.SetParent(Camera.main.transform);
            CUR_EFFECT.transform.localScale = Vector3.one;
            Vector3 localpos = Camera.main.transform.InverseTransformPoint(Camera.main.ScreenToWorldPoint(data.position));
            localpos.z = Camera.main.transform.position.z * -1;
            CUR_EFFECT.transform.localPosition = localpos;

            particles.Add(CUR_EFFECT);
            videoPlayerManager.resume_current_clip();
        }
        else if ((CUR_ENABLE_EFFECT & INTERACTION_TYPE.GRIP) != INTERACTION_TYPE.NONE)
        {
            if (CUR_EFFECT)
            {
                Destroy(CUR_EFFECT);
                CUR_EFFECT = null;
            }

            CUR_EFFECT = Instantiate(GRIP_EFFECT);
            CUR_EFFECT.transform.SetParent(Camera.main.transform);
            CUR_EFFECT.transform.localScale = Vector3.one;
            Vector3 localpos = Camera.main.transform.InverseTransformPoint(Camera.main.ScreenToWorldPoint(data.position));
            localpos.z = Camera.main.transform.position.z * -1;
            CUR_EFFECT.transform.localPosition = localpos;

            particles.Add(CUR_EFFECT);
            videoPlayerManager.resume_current_clip();
        }
        else if ((CUR_ENABLE_EFFECT & INTERACTION_TYPE.RUB) != INTERACTION_TYPE.NONE)
        {
            if (CUR_EFFECT)
            {
                Destroy(CUR_EFFECT);
                CUR_EFFECT = null;
            }

            CUR_EFFECT = Instantiate(RUB_EFFECT);
            CUR_EFFECT.transform.SetParent(Camera.main.transform);
            CUR_EFFECT.transform.localScale = Vector3.one;
            Vector3 localpos = Camera.main.transform.InverseTransformPoint(Camera.main.ScreenToWorldPoint(data.position)); ;
            localpos.z = Camera.main.transform.position.z * -1;
            CUR_EFFECT.transform.localPosition = localpos;

            particles.Add(CUR_EFFECT);
            videoPlayerManager.resume_current_clip();
        }
    }
    public void OnDrag(PointerEventData data)
    {
        if (CUR_EFFECT)
        {
            if ((CUR_ENABLE_EFFECT & INTERACTION_TYPE.STROKE) != INTERACTION_TYPE.NONE || (CUR_ENABLE_EFFECT & INTERACTION_TYPE.RUB) != INTERACTION_TYPE.NONE)
            {
                Vector3 localpos = Camera.main.transform.InverseTransformPoint(Camera.main.ScreenToWorldPoint(data.position));
                localpos.z = Camera.main.transform.position.z * -1;
                CUR_EFFECT.transform.localPosition = localpos;
            }
        }
    }

    public void OnPointerUp(PointerEventData data)
    {
        if (pointerData.Count >= 2)
        {
            if ((CUR_ENABLE_EFFECT & INTERACTION_TYPE.GRIP) != INTERACTION_TYPE.NONE)
            {
                float distance = Math.Abs(Vector2.Distance(pointerData[0].pressPosition, pointerData[1].pressPosition));
                if(distance > Math.Abs(Vector2.Distance(pointerData[0].position, pointerData[1].position)))
                {
                    Debug.Log("PINCHED!");
                }
            }
        }

        pointerData.Remove(data);

        if(CUR_EFFECT)
        {
            particles.Remove(CUR_EFFECT);
            var particle = CUR_EFFECT.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particle)
            {
                var em = ps.emission;
                em.enabled = false;
            }
            Destroy(CUR_EFFECT, 3.0f);
            CUR_EFFECT = null;            
        }

        if ((CUR_ENABLE_EFFECT & INTERACTION_TYPE.TAP) != INTERACTION_TYPE.NONE)
        {

        }
        else
        {
            if(this.gameObject.activeSelf)
                videoPlayerManager.pause_current_clip();
        }
    }
}
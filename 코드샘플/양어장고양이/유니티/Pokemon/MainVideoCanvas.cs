using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class MainVideoCanvas : MonoBehaviour
{
    private NecoVideoCanvas videoCanvas { get { return NecoCanvas.GetVideoCanvas(); } }

    public Animation SeqAnimation;
    public Text SeqText;
    public RenderTexture videoRenderTexture;
    public Button Search_Btn;
    private bool was_cleared = false;
    public Button[] badge;
    public string[] SeqScript;
    public AnimationClip[] SeqAnimationClip;
    public VideoClip[] SeqVideoClip;
    public uint[] SeqStartClipID;
    public Sprite[] ButtonType;
    
    private uint curSeqID = 0;
    private uint nextSeqID = 0;
    private void Awake()
    {
        SetSeqAnimation(curSeqID);
        VideoManager.GetInstance().PlayBackgroundVideo(SeqVideoClip[curSeqID], videoRenderTexture);        
    }
        
    public void OnClipPlay()
    {        
        gameObject.SetActive(false);
        videoCanvas.gameObject.SetActive(true);
        videoCanvas.OnVideoPlay(SeqStartClipID[curSeqID], OnLoadNextMap);
    }

    public void OnLoadNextMap()
    {
        NecoCanvas.GetGameCanvas().LoadForegroundMap();
    }
    public void OnTargetPlay(int target)
    {
        if (was_cleared
            ||(target == 2006 && curSeqID >= 3)
            || (target == 1006 && curSeqID >= 6)
            || (target == 3006 && curSeqID >= 9))
        {
            curSeqID--;
            gameObject.SetActive(false);
            videoCanvas.gameObject.SetActive(true);
            videoCanvas.OnVideoPlay((uint)(int)target);
        }        
    }
    public void SetSeqAnimation(uint Seq)
    {
        curSeqID = Seq;

        //SeqVideoPlayer.clip = SeqVideoClip[Seq];
        
        SeqText.text = SeqScript[Seq];

        SeqAnimation.clip = SeqAnimationClip[Seq];
        Search_Btn.image.sprite = ButtonType[Seq];    
        SeqAnimation.Play();
    }
    
    public void get_badge(uint sq)
    {
        if(sq == 4)//길막이 뱃지 오픈
        {
            badge[0].transform.GetChild(0).gameObject.SetActive(false);
            badge[0].transform.GetChild(1).gameObject.SetActive(true);
            badge[0].transform.GetChild(1).gameObject.GetComponent<Animation>().Play();
        }

        if(sq == 8)//어미냥 맷지 오픈
        {
            badge[1].transform.GetChild(0).gameObject.SetActive(false);
            badge[1].transform.GetChild(1).gameObject.SetActive(true);
        }

        if(sq == 12)//삼색이 뱃지 오픈

        {
            badge[2].transform.GetChild(0).gameObject.SetActive(false);
            badge[2].transform.GetChild(1).gameObject.SetActive(true);
        }
    }
}

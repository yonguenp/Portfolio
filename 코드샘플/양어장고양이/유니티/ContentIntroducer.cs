using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContentIntroducer : MonoBehaviour
{
    public GameObject guidePrefab;
    public uint ContentID;
    public float delay = 0.0f;
    public AudioSource audio;
    public AudioClip audioClip;

    private bool bGuideShown = false;
    private void OnEnable()
    {
        if (ContentLocker.GetCurContentSeq() == ContentID && bGuideShown == false)
        {
            GameObject guide = Instantiate(guidePrefab);
            ContentGuide contentGuide = guide.GetComponent<ContentGuide>();

            List<string> GuideText = new List<string>();
            string key = "";
            string value = "";
            int index = 0;
            do
            {
                key = "CS" + ContentID.ToString("00") + "-" + index.ToString("00");
                value = LocalizeData.GetText(key);

                if (key != value)
                {
                    GuideText.Add(value);
                }
                index++;
            } while (key != value);

            contentGuide.SetGuide((int)ContentID, GuideText.ToArray(), null, null, GetComponent<Button>(), delay);
            bGuideShown = true;

            Invoke("ContentGuideEffectAudioPlay", 1.0f);
        }
    }

    public void ContentGuideEffectAudioPlay()
    {
        if (audio)
        {
            audio.clip = audioClip;
            audio.PlayDelayed(1.0f);
        }
    }
}

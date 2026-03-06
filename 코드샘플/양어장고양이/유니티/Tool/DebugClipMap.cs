using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugClipMap : MonoBehaviour
{
    public GameObject VideoSampleUI;
    public InputField input;

    public void OnCommand(string text)
    {
        uint index = uint.Parse(text);
        object obj;

        clip_event targetEvent = null;
        List<game_data> datas = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.CLIP_EVENT);
        foreach(game_data data in datas)
        {
            if(data.data.TryGetValue("event_id", out obj))
            {
                if(index == (uint)obj)
                {
                    targetEvent = (clip_event)data;
                }
            }
        }


    }
}

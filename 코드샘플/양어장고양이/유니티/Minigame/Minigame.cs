using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Minigame : MonoBehaviour
{
    enum GAME_STATE { 
        GAME_INIT,
        GAME_PLAY,
        GAME_OVER,
        GAME_CLEAR,
    };

    List<List<Ice>> ice_group = null;
    public GameObject Cat;
    public GameObject fish;
    private GAME_STATE state = GAME_STATE.GAME_INIT;

    public GameObject[] UI;
    public GameObject Hint;

    [ContextMenu("SetMap")]
    public void SetMap()
    {
        ice_group = new List<List<Ice>>();

        int x = 0;
        int y = 0;
        foreach (Transform child in transform)
        {
            if (fish.transform == child || Cat.transform == child)
                continue;

            child.GetComponent<Image>().color = (x + y) % 2 == 0 ? new Color(0.2314881f, 0.5607843f, 0.8901961f, 0.9f) : new Color(0.652f, 0.9607843f, 0.8901961f, 0.9f);
            
            Ice ice = child.GetComponent<Ice>();
            if (ice == null)
                ice = child.gameObject.AddComponent<Ice>();

            ice.Init(x, y, this);
            
            if(ice_group.Count <= y)
            {
                ice_group.Add(new List<Ice>());
            }

            ice_group[y].Add(ice);

            x++;
            if (x % 10 == 0)
            {
                x = 0;
                y++;
            }
        }

        Cat.transform.SetParent(transform);
        fish.transform.SetParent(transform);
        float half = 720.0f * 0.5f;
        Cat.transform.localPosition = new Vector3(Random.Range(half * -1.0f, half), Random.Range(half * -1.0f, half), 0.0f);        
        fish.transform.localPosition = new Vector3(Random.Range(half * -1.0f, half), Random.Range(half * -1.0f, half), 0.0f);
        fish.transform.SetAsFirstSibling();

        state = GAME_STATE.GAME_PLAY;

        Cat.transform.localScale = Vector3.one;
        fish.transform.localScale = Vector3.one;
    }

    public void Awake()
    {
        SetState(GAME_STATE.GAME_INIT);
    }

    public void SetState(int i)
    {
        SetState((GAME_STATE)i);
    }

    private void SetState(GAME_STATE s)
    {
        state = s;

        foreach(GameObject ui in UI)
        {
            ui.SetActive(false);
        }

        UI[(int)state].SetActive(true);

        switch (state)
        {
            case GAME_STATE.GAME_PLAY:
                Hint.SetActive(false);
                SetMap();
                break;            
        }
    }

    public void OnTouched(int _x, int _y, float power)
    {
        if (state != GAME_STATE.GAME_PLAY)
            return;

        Vector2 p = new Vector2(_x, _y);
        float MaxDistance = Mathf.Abs(Vector2.Distance(Vector2.zero, new Vector2(ice_group[ice_group.Count - 1].Count, ice_group.Count)));
        for(int y = 0; y < ice_group.Count; y++)
        {
            for(int x = 0; x < ice_group[y].Count; x++)
            {
                float powerRate = ((MaxDistance - Mathf.Abs(Vector2.Distance(p, new Vector2(x, y)))) / MaxDistance);
                if(powerRate < 1.0f)
                {
                    powerRate *= Random.value * 0.1f;
                }
                ice_group[y][x].OnPowered(power * powerRate);
            }
        }
        bool needRefresh = true;
        while (needRefresh)
        {
            needRefresh = false;
            for (int y = 0; y < ice_group.Count; y++)
            {
                for (int x = 0; x < ice_group[y].Count; x++)
                {
                    if (ice_group[y][x].IsLive())
                    {
                        bool bHope = false;

                        try
                        {
                            if (ice_group[y + 1][x].IsLive())
                                bHope = true;
                        }
                        catch { }
                        try
                        {
                            if (ice_group[y - 1][x].IsLive())
                                bHope = true;
                        }
                        catch { }
                        try
                        {
                            if (ice_group[y][x - 1].IsLive())
                                bHope = true;
                        }
                        catch { }
                        try
                        {
                            if (ice_group[y][x + 1].IsLive())
                                bHope = true;
                        }
                        catch { }

                        if (!bHope)
                        {
                            ice_group[y][x].OnPowered(1.0f);
                            needRefresh = true;
                        }
                    }
                }
            }
        }

        Vector3 pos = Cat.transform.localPosition;
        
        int ix = (int)(((pos.x + (720 / 2)) / 72));
        if (ix < 0)
            ix = 0;
        if (ix > 9)
            ix = 9;
        int iy = (int)((10 - ((pos.y + (720 / 2)) / 72)));
        if (iy < 0)
            iy = 0;
        if (iy > 9)
            iy = 9;
        float force = Random.value * power * 72.0f;        
        pos.x += force * (_x == ix ? -0.5f + Random.value : (_x - ix) * 0.5f);
        force = Random.value * power * 72.0f;
        pos.y += force * (_y == iy ? -0.5f + Random.value : (iy - _y) * 0.5f);
        Cat.transform.localPosition = pos;

        if(!ice_group[iy][ix].IsLive())
        {
            Cat.transform.localScale = (Vector3.one * 0.5f);
            SetState(GAME_STATE.GAME_OVER);
        }

        pos = fish.transform.localPosition;
        ix = (int)(((pos.x + (720 / 2)) / 72));
        if (ix < 0)
            ix = 0;
        if (ix > 9)
            ix = 9;
        iy = (int)((10 - ((pos.y + (720 / 2)) / 72)));
        if (iy < 0)
            iy = 0;
        if (iy > 9)
            iy = 9;
        force = Random.value * power * 72.0f;
        pos.x += force * (_x == ix ? -0.5f + Random.value : (_x - ix) * 0.5f);
        force = Random.value * power * 72.0f;
        pos.y += force * (_y == iy ? -0.5f + Random.value : (iy - _y) * 0.5f);
        fish.transform.localPosition = pos;

        if (!ice_group[iy][ix].IsLive() && Mathf.Abs(Vector2.Distance(Cat.transform.localPosition, fish.transform.localPosition)) < 72)
        {
            Cat.transform.localScale = (Vector3.one * 2.0f);
            fish.transform.localScale = (Vector3.one * 0.5f);
            SetState(GAME_STATE.GAME_CLEAR);
        }
    }

    public void OnHint()
    {
        Hint.SetActive(!Hint.activeSelf);
    }
}

using SBSocketSharedLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPortrait : MonoBehaviour
{
    [SerializeField]
    GameObject PortraitUIPrefab;

    Dictionary<string, CharacterPortrait> portraits = new Dictionary<string, CharacterPortrait>();
    Dictionary<string, Sprite> charSprites = new Dictionary<string, Sprite>();

    // Start is called before the first frame update
    void Start()
    {
        LoadChracterSprites();
    }

    void LoadChracterSprites()
    {
        var datas = Resources.LoadAll<Sprite>("Texture/portrait");
        int len = datas.Length;
        for (int i = 0; i < len; ++i)
        {
            charSprites.Add(datas[i].name, datas[i]);
        }
    }

    void AddPortrait(string id, CharacterPortrait p)
    {
        if (portraits.ContainsKey(id) == false)
        {
            p.transform.SetParent(gameObject.transform);
            p.transform.localPosition = Vector3.zero;
            p.transform.localScale = Vector3.one;

            portraits.Add(id, p);
            return;
        }

        SBDebug.LogError("에러");
        return;
    }

    public Sprite GetSprite(int type)
    {
        string name = CharacterGameData.GetUIResourceName(type);
        Sprite sprite = null;
        if (!string.IsNullOrEmpty(name))
            sprite = charSprites[name];
        return sprite;
    }

    public CharacterPortrait CreatePortrait(int type, PlayerObjectInfo info, bool isChaser)
    {
        var portrait = Instantiate(PortraitUIPrefab);
        if (portrait == null) return null;

        var ps = portrait.GetComponent<CharacterPortrait>();
        if (ps == null) return null;
        ps.SetData(type, info, isChaser);

        AddPortrait(info.ObjectId, ps);
        return ps;
    }
}

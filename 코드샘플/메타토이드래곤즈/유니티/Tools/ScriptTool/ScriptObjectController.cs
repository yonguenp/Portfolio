using DG.Tweening;
using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScriptObjectController : MonoBehaviour
{
    [SerializeField] Text Name = null;
    [SerializeField] Image NamePanel = null;
    [SerializeField] UIDragonSpine uiDragonSpine = null;
    [SerializeField] UIMonsterSpine uiMonsterSpine = null;
    [SerializeField] Image item = null;
    [SerializeField] Transform group = null;
    public void Clear()
    {
        gameObject.SetActive(false);
        uiDragonSpine.gameObject.SetActive(false);
        uiMonsterSpine.gameObject.SetActive(false);
        item.gameObject.SetActive(false);
        NamePanel.gameObject.SetActive(false);
        group.gameObject.SetActive(false);
        foreach (Transform child in group)
        {
            Destroy(child.gameObject);
        }
    }

    public void SetData(ScriptObjectData data, ScriptGroupData.OBJECT_STATE state)
    {
        Clear();

        if(data == null)
        {
            return;
        }

        gameObject.SetActive(true);

        Name.text = data.NAME;
        NamePanel.color = data.COLOR;

        switch (data.TYPE)
        {
            case ScriptObjectData.OBJECT_TYPE.DRAGON:
            {
                uiDragonSpine.SetData(CharBaseData.Get(data.RESOURCE_ID));
                if(string.IsNullOrEmpty(data.PARAM))
                    uiDragonSpine.SetAnimation(eSpineAnimation.IDLE);
                else
                    uiDragonSpine.SetAnimation(data.PARAM);
                uiDragonSpine.gameObject.SetActive(true);
                uiDragonSpine.SetScale(data.SCALE);
                uiDragonSpine.transform.localPosition = data.POSITION * data.SCALE;
            }
            break;
            case ScriptObjectData.OBJECT_TYPE.MONSTER:
            {
                uiMonsterSpine.SetData(MonsterBaseData.Get(data.RESOURCE_ID.ToString()), data.PARAM);                
                uiMonsterSpine.gameObject.SetActive(true);
                uiMonsterSpine.SetScale(data.SCALE);
                uiMonsterSpine.transform.localPosition = data.POSITION * data.SCALE;
            }
            break;
            case ScriptObjectData.OBJECT_TYPE.ITEM:
            {
                const float defaultScale = 6.0f;
                item.sprite = ItemBaseData.Get(data.RESOURCE_ID).ICON_SPRITE;
                item.gameObject.SetActive(true);
                item.transform.localScale = new Vector3(data.SCALE.x, data.SCALE.y, 1.0f) * defaultScale;
                item.transform.localPosition = data.POSITION * data.SCALE;

                item.transform.DOKill();
                item.transform.DOLocalMoveY(data.POSITION.y * data.SCALE.y + 30f, 1.2f).SetLoops(-1, LoopType.Yoyo);
            }
            break;
            case ScriptObjectData.OBJECT_TYPE.GROUP:
            {
                group.gameObject.SetActive(true);
                group.transform.localScale = new Vector3(data.SCALE.x, data.SCALE.y, 1.0f);
                group.transform.localPosition = data.POSITION * data.SCALE;

                string[] param_array = data.PARAM.Split(',');
                foreach(var param in param_array)
                {
                    ScriptObjectData child = ScriptObjectData.Get(int.Parse(param));
                    switch (child.TYPE)
                    {
                        case ScriptObjectData.OBJECT_TYPE.DRAGON:
                        {
                            UIDragonSpine target = Instantiate(uiDragonSpine, group);
                            target.SetData(CharBaseData.Get(child.RESOURCE_ID));
                            if (string.IsNullOrEmpty(child.PARAM))
                                target.SetAnimation(eSpineAnimation.IDLE);
                            else
                                target.SetAnimation(child.PARAM);
                            target.gameObject.SetActive(true);
                            target.SetScale(child.SCALE);
                            target.transform.localPosition = child.POSITION * child.SCALE;
                        }
                        break;
                        case ScriptObjectData.OBJECT_TYPE.MONSTER:
                        {
                            UIMonsterSpine target = Instantiate(uiMonsterSpine, group);
                            target.SetData(MonsterBaseData.Get(child.RESOURCE_ID.ToString()), child.PARAM);
                            target.gameObject.SetActive(true);
                            target.SetScale(child.SCALE);
                            target.transform.localPosition = child.POSITION * child.SCALE;
                        }
                        break;
                        case ScriptObjectData.OBJECT_TYPE.ITEM:
                        {
                            Image target = Instantiate(item, group);
                            const float defaultScale = 6.0f;
                            target.sprite = ItemBaseData.Get(child.RESOURCE_ID).ICON_SPRITE;
                            target.gameObject.SetActive(true);
                            target.transform.localScale = new Vector3(child.SCALE.x, child.SCALE.y, 1.0f) * defaultScale;
                            target.transform.localPosition = child.POSITION * child.SCALE;
                        }
                        break;
                    }
                }
            }
            break;
            case ScriptObjectData.OBJECT_TYPE.ETC:
            {
                group.gameObject.SetActive(true);
                group.transform.localScale = new Vector3(data.SCALE.x, data.SCALE.y, 1.0f);
                group.transform.localPosition = data.POSITION * data.SCALE;

                var etcObj = Instantiate(ResourceManager.GetResource<GameObject>(eResourcePath.ScriptObjectPath, data.PARAM), group);
                etcObj.transform.localScale = Vector3.one;
                etcObj.transform.localPosition = Vector3.zero;
            }
            break;
            default:
            {

            }
            break;
        }

        switch(state)
        {
            case ScriptGroupData.OBJECT_STATE.SPEAK:
                uiDragonSpine.SetColor(Color.white);
                uiMonsterSpine.SetColor(Color.white);
                item.color = Color.white;
                NamePanel.transform.localScale = Vector3.one * 1.1f;
                NamePanel.gameObject.SetActive(!string.IsNullOrEmpty(data.NAME));
                break;            
            case ScriptGroupData.OBJECT_STATE.DIM:
                uiDragonSpine.SetColor(Color.gray);
                uiMonsterSpine.SetColor(Color.gray);
                item.color = Color.gray;
                NamePanel.transform.localScale = Vector3.one * 0.9f;
                NamePanel.gameObject.SetActive(false);
                break;
            case ScriptGroupData.OBJECT_STATE.NORMAL:
                NamePanel.gameObject.SetActive(false);
                break;
            default:                
                Clear();
                break;
        }
    }

    private void Update()
    {
        var size =(Name.transform as RectTransform).sizeDelta;
        if (size.x < 320.0f)
            size.x = 320.0f;
        size += new Vector2(40f, 20f);

        (NamePanel.transform as RectTransform).sizeDelta = size;
    }
}

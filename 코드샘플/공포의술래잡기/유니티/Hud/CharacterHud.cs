using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterHud : MonoBehaviour
{
    Dictionary<CharacterObject, Transform> childs = new Dictionary<CharacterObject, Transform>(); 
    public void SetChild(CharacterObject character, Transform child)
    {
        if(!childs.ContainsKey(character))
        {
            var container = new GameObject();
            container.name = character.UserName;
            container.transform.SetParent(transform);

            RectTransform rt = container.AddComponent<RectTransform>();
            rt.transform.localPosition = Vector3.zero;
            rt.transform.localScale = Vector3.one;
            rt.transform.localRotation = Quaternion.identity;

            childs.Add(character, container.transform);
        }

        child.SetParent(childs[character]);
    }


    public void OnRemoveCharacterHud(CharacterObject charObj)
    {
        if (childs.ContainsKey(charObj))
        {
            if (childs[charObj] != null)
            {
                Destroy(childs[charObj].gameObject);
            }

            childs.Remove(charObj);
        }
    }
    
    public void LateUpdate()
    {
        var list = childs.Keys.ToList();

        if (Camera.main.orthographicSize <= 7.0f)
        {
            list.Sort((a, b) =>
            {
                return b.transform.position.y.CompareTo(a.transform.position.y);
            });

            for (int i = 0; i < list.Count; i++)
            {
                childs[list[i]].gameObject.SetActive(true);
                childs[list[i]].SetSiblingIndex(i);                
            }
        }
        else
        {
            for (int i = 0; i < list.Count; i++)
            {
                childs[list[i]].gameObject.SetActive(false);
            }
        }
    }
}

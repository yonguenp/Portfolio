using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultCharacter : MonoBehaviour
{
    [SerializeField]
    Image target;

    [Serializable]
    public class CharacterInfo
    {
        public string resourceName;
        public Sprite win;
        public Sprite lose;
    }

    [SerializeField]
    CharacterInfo[] CharacterList;

    public void ClearUI()
    {
        target.gameObject.SetActive(false);
        target.sprite = null;
    }
    public void SetCharacter(int type, bool win)
    {
        string charName = CharacterGameData.GetUIResourceName(type);
        
        foreach (CharacterInfo info in CharacterList)
        {
            if (info.resourceName.Equals(charName))
            {
                target.sprite = win ? info.win : info.lose;
                target.SetNativeSize();
                target.gameObject.SetActive(true);
                return;
            }
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

public class SkillPopup : MonoBehaviour
{
    [SerializeField]
    Text text = null;
    
    public void SetText(string text)
    {
        this.text.text = text;
    }
}

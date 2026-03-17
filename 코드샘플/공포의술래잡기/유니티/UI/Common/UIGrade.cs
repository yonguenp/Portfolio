using UnityEngine;
using UnityEngine.UI;

public class UIGrade : MonoBehaviour
{
    [SerializeField]
    Sprite[] iconImages;
    [SerializeField]
    Image icon;


    public void SetGrade(int grade)
    {
        if (icon != null)
            icon.gameObject.SetActive(true);
        switch (grade)
        {
            case 1:
            case 2:
            case 3:
            case 4:
                if (icon != null)
                    icon.sprite = iconImages[grade];
                break;
            default:
                if (icon != null)
                    icon.gameObject.SetActive(false);
                break;
        }
    }
}

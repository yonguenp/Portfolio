using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotFrameController : MonoBehaviour
{
    [SerializeField] Image sprite_frame;
    [SerializeField] Color[] color_frame;

    public void SetColor(int grade)
    {
        if (sprite_frame == null)
            return;
        int colorGrade = grade;
        if (grade >= color_frame.Length)
            colorGrade = color_frame.Length - 1;
        if (grade < 0)
            colorGrade = 0;
        sprite_frame.color = color_frame[colorGrade];
    }

    public void SetColor(Color color)
    {
        sprite_frame.color = color;
    }
}

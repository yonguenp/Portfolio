using UnityEngine;

public class SkillRangeIndicator : MonoBehaviour
{
    public SpriteRenderer RangeImage;

    int materialDirectionPropertyID = -1;
    int materialAnglePropertyID = -1;

    public void SetColor(Color color)
    {
        RangeImage.color = color;
    }

    public void SetPosition(Vector3 targetPos)
    {
        transform.position = targetPos;
    }

    public void SetAngle(Vector2 dir, float angle)
    {
        var directionDegree = Vector2.SignedAngle(Vector2.right, dir);
        if (directionDegree < 0) directionDegree += 360;

        if (materialDirectionPropertyID == -1)
        {
            materialDirectionPropertyID = Shader.PropertyToID("_Angle");
        }
        if (materialAnglePropertyID == -1)
        {
            materialAnglePropertyID = Shader.PropertyToID("_Arc");
        }

        RangeImage.material.SetFloat(materialDirectionPropertyID, directionDegree);
        RangeImage.material.SetFloat(materialAnglePropertyID, angle);
    }

    // 직사각형
    public void SetRange(Vector2 dir, Vector2 size, Color color, Vector2 targetPos, float startPos)
    {
        SetPosition(targetPos + startPos * dir);
        var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.eulerAngles = new Vector3(0, 0, angle);
        transform.localScale = size;
        SetColor(color);
    }

    // 부채꼴형
    public void SetRange(Vector2 dir, float range, int degree, Color color, Vector2 targetPos, float startPos)
    {
        SetPosition(targetPos + startPos * dir);
        transform.localScale = new Vector3(range * 2, range * 2, range * 2);
        SetColor(color);
        SetAngle(dir, degree);
    }
}

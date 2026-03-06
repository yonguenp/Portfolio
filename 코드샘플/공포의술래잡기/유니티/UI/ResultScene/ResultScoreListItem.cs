using UnityEngine;
using UnityEngine.UI;

public class ResultScoreListItem : MonoBehaviour
{
    [SerializeField] Text txtScoreType;
    [SerializeField] Text txtScore;

    public void Initialize(string desc, int count, int unitScore)
    {
        if (count > 1)
            txtScoreType.text = $"{desc} x{count}";
        else
            txtScoreType.text = $"{desc}";

        txtScore.text = (unitScore * count).ToString("N0");
    }
}

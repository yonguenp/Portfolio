using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChampionLeagueLine : MonoBehaviour
{
    [SerializeField]
    GameObject A_WIN;
    [SerializeField]
    GameObject B_WIN;
        
    public void SetLine(eChampionWinType wintype)
    {
        A_WIN.SetActive(wintype == eChampionWinType.SIDE_A_WIN);
        B_WIN.SetActive(wintype == eChampionWinType.SIDE_B_WIN);
    }
}

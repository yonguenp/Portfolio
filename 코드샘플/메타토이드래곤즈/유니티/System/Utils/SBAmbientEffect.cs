using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SBAmbientEffect : MonoBehaviour
{
    [SerializeField]
    Color bgColor = Color.white;
    [SerializeField]
    float time = 0.0f;

    private void OnEnable()
    {
        BattleMapAmbientEvent.Send(bgColor, time);
    }
}

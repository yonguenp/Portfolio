using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerSelectUI : MonoBehaviour
{
    [SerializeField]
    Text VersionText;
    [SerializeField]
    Text BranchText;

    private void Start()
    {
        VersionText.text = GameConfig.Instance.VERSION;
        BranchText.text = GameConfig.Instance.BUILD_BRANCH;
    }
}

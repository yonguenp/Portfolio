using Coffee.UIExtensions;
using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if DEBUG
public class SBUIParticleSetScale : MonoBehaviour
{
    public int scaleFactor = 200;
    public List<GameObject> effectPrefabList = new List<GameObject>();

    public string effectAssetPath = "";

    public List<GameObject> prefabList = new List<GameObject>();

    public void SetEffectDirectory()
    {
        effectAssetPath = "Assets/Resources/AssetBundle/" + SBDefine.ResourceFolder(eResourcePath.EffectPrefabPath);

        prefabList = Resources.LoadAll<GameObject>("AssetBundle/" + SBDefine.ResourceFolder(eResourcePath.EffectPrefabPath)).ToList();
    }
}
#endif
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugFinder : MonoBehaviour
{
    private void Awake()
    {
#if !UNITY_EDITOR
        enabled = false;
        Destroy(gameObject);
#endif
    }

    // Start is called before the first frame update
    void Start()
    {
        Scene scene = SceneManager.GetActiveScene();
        List<GameObject> roots = scene.GetRootGameObjects().ToList();
        List<AudioSource> objs = new List<AudioSource>();
        int sourceCount = 0;

        for(int i = 0; i < roots.Count; i++)
        {
            List<AudioSource> child = roots[i].GetComponentsInChildren<AudioSource>(true).ToList();
        
            objs.AddRange(child);
        }
        
        string debs = "";
        for(int i = 0; i < objs.Count; i++)
        {
            if(objs[i].outputAudioMixerGroup == null)
            {
                if (sourceCount != 0)
                {
                    debs += "\n";
                }
                debs += objs[i].gameObject.name;
                sourceCount++;
            }            
        }

        if(sourceCount > 0)
        {
            Debug.LogError("총 오디오 수 : " + sourceCount);
            Debug.Log(debs);
        }
    }
}

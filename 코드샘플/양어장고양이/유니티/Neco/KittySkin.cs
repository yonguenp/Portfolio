using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class KittySkin : MonoBehaviour
{
    public List<GameObject> disableTargets = null;
    public List<string> skinTargets = null;
    public List<MonoBehaviour> disableScriptTargets = null;

    private void OnEnable()
    {
        if(disableTargets != null && disableTargets.Count > 0)
        {
            foreach(GameObject obj in disableTargets)
            {
                obj.SetActive(false);
            }
        }

        if(skinTargets != null && skinTargets.Count > 0)
        {
            SkeletonGraphic spine = GetComponent<SkeletonGraphic>();

            spine.Skeleton.SetSkin(skinTargets[Random.Range(0, skinTargets.Count - 1)]);
            spine.Skeleton.SetSlotsToSetupPose();
        }

        if(disableScriptTargets != null && disableScriptTargets.Count > 0)
        {
            foreach (MonoBehaviour script in disableScriptTargets)
            {
                script.enabled = false;
            }
        }
    }

    private void OnDisable()
    {
        if (disableTargets != null)
        {
            foreach (GameObject obj in disableTargets)
            {
                obj.SetActive(true);
            }
        }

        if (disableScriptTargets != null)
        {
            foreach (MonoBehaviour script in disableScriptTargets)
            {
                script.enabled = true;
            }
        }
    }
}

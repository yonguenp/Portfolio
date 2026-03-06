using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class DestroyEffect : MonoBehaviour
    {
        [SerializeField] private GameObject[] targetPrefab = null;

        private void OnDestroy()
        {
            if (targetPrefab == null)
                return;

            for(int i = 0, count = targetPrefab.Length; i < count; ++i)
            {
                if (targetPrefab[i] == null)
                    continue;

                var effect = Instantiate(targetPrefab[i], transform.parent);
                if (effect != null)
                    effect.transform.position = transform.position;
            }
        }
    }
}
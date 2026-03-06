using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class SBParticle : MonoBehaviour
    {
        private ParticleSystem particle;
        [SerializeField]
        GameObject[] FreeOrderObjects = null;

        Vector3[] originFreeOrderPos = null;
        void Start()
        {
            particle = GetComponent<ParticleSystem>();
            if (particle != null)
            {
                if (FreeOrderObjects != null)
                {
                    originFreeOrderPos = new Vector3[FreeOrderObjects.Length];
                    for(int i = 0; i < FreeOrderObjects.Length; i++)
                    {
                        if (FreeOrderObjects[i] != null)
                        {
                            FreeOrderObjects[i].transform.parent = transform.parent;
                            originFreeOrderPos[i] = FreeOrderObjects[i].transform.position - transform.position;
                        }
                    }
                }
            }
        }

        void FixedUpdate()
        {
            if (particle != null)
            {
                for (int i = 0; i < FreeOrderObjects.Length; i++)
                {
                    if (FreeOrderObjects[i] != null)
                        FreeOrderObjects[i].transform.position = transform.position + originFreeOrderPos[i];
                }

                if (!particle.IsAlive(false))
                {
                    Destroy(gameObject);
                }
            }
        }

        private void OnDestroy()
        {
            if (particle != null)
            {
                foreach (var obj in FreeOrderObjects)
                {
                    Destroy(obj);
                }
            }
        }
    }
}

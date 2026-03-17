using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    [System.Serializable]
    public class OutlineMaterial
    {
        public Material targetMaterial = null;
        public Material changeMaterial = null;
    }
    public class SpineOutlineRenderer : OutlineRenderer
    {
        [SerializeField]
        protected MeshRenderer outlineRenderer = null;
        [SerializeField]
        protected MeshFilter outlineMeshFilter = null;
        [SerializeField]
        protected MeshRenderer spineRenderer = null;
        [SerializeField]
        protected MeshFilter spineMeshFilter = null;
        [SerializeField]
        protected List<OutlineMaterial> materials = null;
        protected static Dictionary<Material, Material> matDic = null;

        public override void SetOutline(bool isActive)
        {
            gameObject.SetActive(isActive);
            if (isActive)
                Sync();
        }

        public override void Sync()
        {
            if (outlineRenderer == null || spineRenderer == null)
                return;

            if (outlineMeshFilter == null || spineMeshFilter == null)
                return;

            CheckDic();

            if (spineRenderer.sharedMaterials != null)
            {
                if(spineRenderer.sharedMaterials.Length > 0)
                {
                    for (int i = 0, count = spineRenderer.sharedMaterials.Length; i < count; ++i)
                    {
                        if(matDic.TryGetValue(spineRenderer.sharedMaterials[i], out Material mat))
                        {
                            outlineRenderer.material = mat;
                            break;
                        }
                    }
                }
            }
            outlineMeshFilter.sharedMesh = spineMeshFilter.sharedMesh;
        }
        private void CheckDic()
        {
            if (materials == null)
                return;

            if(matDic == null)
            {
                matDic = new();
                for(int i = 0, count = materials.Count; i < count; ++i)
                {
                    if (materials[i] == null)
                        continue;

                    if (!matDic.TryAdd(materials[i].targetMaterial, materials[i].changeMaterial))
                        matDic[materials[i].targetMaterial] = materials[i].changeMaterial;
                }
            }
        }
        public override void LateUpdateOutline(Transform target)
        {
            if (target == null)
                return;

            transform.localScale = target.transform.localScale;
            transform.localPosition = target.transform.localPosition;
        }
    }
}

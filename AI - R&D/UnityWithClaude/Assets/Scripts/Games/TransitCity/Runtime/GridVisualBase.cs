using System.Collections;
using UnityEngine;

namespace TransitCity
{
    /// <summary>
    /// 격자 위 오브젝트(도로·건물)의 공통 비주얼 베이스. 상속을 적극 활용하는
    /// 컨벤션(§18)에 따라 RoadVisual/HouseVisual이 여기서 파생된다.
    /// MaterialPropertyBlock으로 색을 바꿔 머티리얼 인스턴스 생성을 피한다.
    /// </summary>
    public abstract class GridVisualBase : MonoBehaviour
    {
        [SerializeField] protected MeshRenderer meshRenderer;

        static readonly int ColorId = Shader.PropertyToID("_Color");
        MaterialPropertyBlock mpb;
        Coroutine punchRoutine;

        protected virtual void Awake()
        {
            mpb = new MaterialPropertyBlock();
        }

        protected void SetColor(Color color)
        {
            if (meshRenderer == null) return;
            meshRenderer.GetPropertyBlock(mpb);
            mpb.SetColor(ColorId, color);
            meshRenderer.SetPropertyBlock(mpb);
        }

        /// <summary>작은 "짠!" 연출 — 건설 완공·주거지 발생 같은 순간에 쓴다.</summary>
        protected void PlayPunch(Vector3 baseScale, float punchAmount = 1.3f, float duration = 0.22f)
        {
            if (punchRoutine != null) StopCoroutine(punchRoutine);
            punchRoutine = StartCoroutine(PunchRoutine(baseScale, punchAmount, duration));
        }

        IEnumerator PunchRoutine(Vector3 baseScale, float punchAmount, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / duration);
                // 0->1에서 punchAmount까지 올라갔다가 1로 튕겨 돌아오는 곡선.
                float s = 1f + (punchAmount - 1f) * (1f - p) * Mathf.Sin(p * Mathf.PI);
                transform.localScale = Vector3.Scale(baseScale, new Vector3(s, s, s));
                yield return null;
            }
            transform.localScale = baseScale;
        }
    }
}

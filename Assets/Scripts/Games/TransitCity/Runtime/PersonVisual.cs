using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TransitCity
{
    /// <summary>
    /// 순수 장식용 보행자(§6/§11, 미니메트로/미니모터웨이식 "작은 원") — 시뮬레이션에
    /// 영향을 주지 않는다. 집에서 나와 가장 가까운 도로까지 걸어가는 "라스트마일"
    /// 구간만 표현한다 — 도로에 올라선 뒤는 차량(CarVisual, 네모)이 이어받는다는
    /// 설정. 나중에 버스/지하철/공항철도가 추가되면 이 보행자가 그 정류장/역까지
    /// 걸어가는 구간으로 자연스럽게 확장될 지점.
    /// </summary>
    public class PersonVisual : MonoBehaviour
    {
        public event Action<PersonVisual> OnJourneyComplete;

        Coroutine walkRoutine;

        public void Walk(List<Vector3> path, float speed)
        {
            if (walkRoutine != null) StopCoroutine(walkRoutine);
            walkRoutine = StartCoroutine(WalkRoutine(path, speed));
        }

        IEnumerator WalkRoutine(List<Vector3> path, float speed)
        {
            if (path == null || path.Count < 2)
            {
                OnJourneyComplete?.Invoke(this);
                yield break;
            }

            transform.position = path[0];
            for (int i = 1; i < path.Count; i++)
            {
                Vector3 from = path[i - 1];
                Vector3 to = path[i];
                float dist = Vector3.Distance(from, to);
                float duration = dist / Mathf.Max(0.01f, speed);
                float t = 0f;
                while (t < duration)
                {
                    t += Time.deltaTime;
                    transform.position = Vector3.Lerp(from, to, Mathf.Clamp01(t / duration));
                    yield return null;
                }
            }

            OnJourneyComplete?.Invoke(this);
        }
    }
}

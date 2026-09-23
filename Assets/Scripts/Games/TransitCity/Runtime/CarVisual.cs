using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TransitCity
{
    /// <summary>
    /// 순수 장식용 차량(§6/§11) — 시뮬레이션 수치에 전혀 영향을 주지 않는다.
    /// 경로를 받아 그 위를 달리다 끝에 도달하면 콜백으로 스스로 반납을 요청한다.
    /// 주민 출퇴근 차량과 경찰·소방 순찰차 양쪽에 재사용한다 — SetColor로
    /// 구분 색을 입힐 수 있다(순찰차는 자기 시설 색과 맞춤).
    ///
    /// 혼잡도를 눈으로 보여주기 위해 구간별 속도 배율(segmentSpeedScale)과
    /// 칸별 정차 시간(waypointPauseSeconds)을 선택적으로 받는다 — 막힌 구간은
    /// 느려지고, 미설치 교차로가 과부하면 잠깐 완전히 멈췄다 간다("전원 정지
    /// 후 순서대로" 연출).
    ///
    /// 진행 방향으로 회전하고(차 모델의 긴 축이 로컬 Z), 우측통행을 흉내 내기
    /// 위해 각 구간에서 진행 방향 기준 오른쪽으로 laneOffset만큼 옆으로 비켜
    /// 달린다.
    ///
    /// 그것만으로는 차들이 서로를 모르고 그냥 겹쳐서 지나가 버려 "정체"가
    /// 안 느껴진다는 지적이 있었다 — 그래서 TransitCityManager가 들고 있는
    /// 칸 단위 점유 등록(TryReserveTile/ReleaseTile)에 각 차가 직접 참여한다.
    /// 다음 칸으로 넘어가기 전에 그 칸을 예약해야 하고, 이미 다른 차가 있으면
    /// (최대 대기 시간까지) 제자리에서 기다린다 — 이게 진짜로 "줄이 서는"
    /// 정체를 만든다. 영구 교착을 막기 위해 대기 시간이 너무 길어지면 그냥
    /// 밀고 들어간다.
    /// </summary>
    public class CarVisual : MonoBehaviour
    {
        public event Action<CarVisual> OnJourneyComplete;

        static readonly int ColorId = Shader.PropertyToID("_Color");
        MeshRenderer meshRenderer;
        MaterialPropertyBlock mpb;
        Coroutine driveRoutine;

        void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            mpb = new MaterialPropertyBlock();
        }

        /// <summary>null을 넘기면 프리팹 원래 색(장식용 출퇴근 차량 기본색)으로 되돌아간다.</summary>
        public void SetColor(Color? color)
        {
            if (meshRenderer == null) return;
            meshRenderer.GetPropertyBlock(mpb);
            if (color.HasValue) mpb.SetColor(ColorId, color.Value);
            else mpb.Clear();
            meshRenderer.SetPropertyBlock(mpb);
        }

        /// <summary>
        /// path는 세계좌표 웨이포인트(첫 칸은 출발 건물), roadCoords는 path[1..]에
        /// 대응하는 도로 격자좌표(칸 점유 예약용, 건물이 아닌 실제 도로 구간만) —
        /// 길이가 path.Count-1보다 짧아도 된다(예: 순찰차가 마지막에 건물로
        /// 복귀하는 구간처럼 예약이 필요 없는 꼬리 구간).
        /// </summary>
        public void Drive(List<Vector3> path, List<GridCoord> roadCoords, float speed, float laneOffset = 0f,
            float maxQueueWaitSeconds = 4f, List<float> segmentSpeedScale = null, List<float> waypointPauseSeconds = null)
        {
            if (driveRoutine != null) StopCoroutine(driveRoutine);
            driveRoutine = StartCoroutine(DriveRoutine(path, roadCoords, speed, laneOffset, maxQueueWaitSeconds, segmentSpeedScale, waypointPauseSeconds));
        }

        IEnumerator DriveRoutine(List<Vector3> path, List<GridCoord> roadCoords, float speed, float laneOffset,
            float maxQueueWaitSeconds, List<float> segmentSpeedScale, List<float> waypointPauseSeconds)
        {
            if (path == null || path.Count < 2)
            {
                OnJourneyComplete?.Invoke(this);
                yield break;
            }

            transform.position = path[0];
            GridCoord? reservedCoord = null;
            var manager = TransitCityManager.Instance;

            for (int i = 1; i < path.Count; i++)
            {
                // path[i]가 교차로/신호등이라 대기가 필요하면, 그 칸으로 들어서기
                // 전인 지금(아직 path[i-1]에 서 있는 상태)에 미리 멈춰서 기다린다 —
                // 교차로 한복판이 아니라 진입 전에 서야 자연스럽다.
                if (waypointPauseSeconds != null && waypointPauseSeconds.Count > i && waypointPauseSeconds[i] > 0f)
                    yield return new WaitForSeconds(waypointPauseSeconds[i]);

                GridCoord? targetCoord = roadCoords != null && roadCoords.Count >= i ? roadCoords[i - 1] : (GridCoord?)null;

                if (targetCoord.HasValue && manager != null)
                {
                    float waited = 0f;
                    while (!manager.TryReserveTile(targetCoord.Value, this) && waited < maxQueueWaitSeconds)
                    {
                        waited += Time.deltaTime;
                        yield return null;
                    }
                    // maxQueueWaitSeconds를 넘기면 예약 실패 상태 그대로 밀고
                    // 들어간다 — 영구 정체(교착) 방지.
                }

                if (reservedCoord.HasValue) manager?.ReleaseTile(reservedCoord.Value, this);
                reservedCoord = targetCoord;

                Vector3 rawFrom = path[i - 1];
                Vector3 rawTo = path[i];
                Vector3 forward = rawTo - rawFrom;
                float dist = forward.magnitude;

                Vector3 from = rawFrom;
                Vector3 to = rawTo;
                if (dist > 0.0001f)
                {
                    forward /= dist;
                    transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
                    // 진행 방향 기준 "오른쪽"으로 비켜서 우측통행처럼 보이게 한다.
                    Vector3 right = Quaternion.LookRotation(forward, Vector3.up) * Vector3.right;
                    Vector3 offset = right * laneOffset;
                    from += offset;
                    to += offset;
                }

                float scale = segmentSpeedScale != null && segmentSpeedScale.Count >= i ? Mathf.Max(0.15f, segmentSpeedScale[i - 1]) : 1f;
                float duration = dist / Mathf.Max(0.01f, speed * scale);
                float t = 0f;
                while (t < duration)
                {
                    t += Time.deltaTime;
                    transform.position = Vector3.Lerp(from, to, Mathf.Clamp01(t / duration));
                    yield return null;
                }
            }

            if (reservedCoord.HasValue) manager?.ReleaseTile(reservedCoord.Value, this);
            OnJourneyComplete?.Invoke(this);
        }
    }
}

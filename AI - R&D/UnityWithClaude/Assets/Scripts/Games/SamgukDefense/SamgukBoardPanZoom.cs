using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace SamgukDefense
{
    /// <summary>
    /// 2026-09-16 완전 3D 전환 — 전투 화면이 더 이상 UGUI가 아니라서(캔버스와 독립) 예전처럼
    /// IScrollHandler/IDragHandler 같은 UGUI 이벤트를 못 받는다. 마우스/터치를 직접 폴링해서
    /// (1) 카메라 줌·팬, (2) 배치 단계 유닛 드래그(Physics 레이캐스트로 유닛 콜라이더를 먼저
    /// 확인 — 맞으면 유닛을 옮기고, 안 맞으면 카메라를 팬) 둘 다 이 한 컴포넌트가 처리한다.
    /// 예전엔 SamgukBoardPanZoom(카메라)과 SamgukUnitDragHandler(유닛)가 UGUI 이벤트 버블링
    /// ("더 가까운 조상이 먼저 가로챈다")으로 우선순위가 자연히 갈렸는데, 레이캐스트 기반에서는
    /// 그 우선순위를 여기서 직접 코드로 정한다(유닛 히트 우선, 없으면 팬).
    ///
    /// 카메라 시점 규칙(사용자 확정 사양): 줌아웃을 최대로 하면 완전 탑다운(카메라가 정확히
    /// 위에서 수직으로 내려다봄), 줌인할수록 카메라의 X축 회전(피치)만 줄어들어 지형과 점점
    /// 수평에 가까워진다. Y·Z축 회전은 절대 없음(요(헤딩)는 항상 고정).
    /// </summary>
    public class SamgukBoardPanZoom : MonoBehaviour
    {
        Camera cam;
        SamgukGridBoard grid;

        public float minPitchDeg = 34f;
        public float maxPitchDeg = 90f;
        public float minDistance = 7f;
        public float maxDistance = 26f;

        /// <summary>줌아웃(탑다운)일 때 화면에 보드 전체가 넓게 들어오도록 FOV를 살짝 키우고,
        /// 줌인할수록 좁혀서 망원 느낌으로 압축감을 준다("Field of view도 같이 부드럽게 조정되면
        /// 연출이 좋을것 같음" 요청, 2026-09-16).</summary>
        public float minFov = 42f;
        public float maxFov = 58f;

        /// <summary>0=완전 줌아웃(탑다운, 보드 전체가 한눈에), 1=완전 줌인(지형과 거의 수평).</summary>
        float zoomT = 0.28f;
        Vector3 focus = Vector3.zero;

        /// <summary>Apply()가 계산한 "목표" 위치/피치/FOV — 실제 카메라는 매 프레임 여기로
        /// Lerp/Slerp만 하고 순간이동하지 않는다("드래그로 카메라를 옮길 때 화면이 깜빡이며
        /// 부드럽지 못하다" 신고, 2026-09-16 — 예전엔 Apply()가 cam.transform을 매 프레임
        /// 목표값으로 즉시 스냅했다). 첫 Setup() 호출 때만 예외적으로 즉시 스냅한다 — 그때는
        /// "부드럽게 이동할 이전 상태"가 아예 없어서, 보간을 걸면 카메라가 원점(0,0,0)에서부터
        /// 눈에 띄게 날아오는 어색한 등장 연출이 된다.</summary>
        Vector3 targetPos;
        Quaternion targetRot;
        float targetFov;
        const float PosLerpSpeed = 10f;
        const float RotLerpSpeed = 10f;
        const float FovLerpSpeed = 8f;

        bool pointerActive;
        bool draggingUnit;
        SamgukUnit draggedUnit;
        Vector2Int draggedStartCell;
        Vector3 dragStartGroundHit;
        Vector3 dragStartFocus;

        float pinchStartDist = -1f;
        float pinchStartZoomT;

        public void Setup(Camera camera, SamgukGridBoard boardGrid)
        {
            cam = camera;
            grid = boardGrid;
            focus = Vector3.zero;
            Apply();
            // 최초 배치는 보간 없이 즉시 스냅 — 안 그러면 카메라가 원점에서부터 날아오는
            // 어색한 등장 연출이 된다(위 targetPos 필드 설명 참고).
            cam.transform.position = targetPos;
            cam.transform.rotation = targetRot;
            cam.fieldOfView = targetFov;
        }

        void Update()
        {
            if (cam == null || grid == null) return;
            HandlePointer();
            HandleScroll();
            HandlePinch();

            float dt = Time.unscaledDeltaTime; // 배속 버튼(Time.timeScale)과 무관하게 카메라 조작감은 항상 일정해야 한다
            cam.transform.position = Vector3.Lerp(cam.transform.position, targetPos, 1f - Mathf.Exp(-PosLerpSpeed * dt));
            cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, targetRot, 1f - Mathf.Exp(-RotLerpSpeed * dt));
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFov, 1f - Mathf.Exp(-FovLerpSpeed * dt));
        }

        // ── 포인터(마우스 1개 또는 터치 1개) — 드래그로 유닛 이동 또는 카메라 팬 ──

        bool TryGetPointer(out Vector2 pos, out bool down, out bool held, out bool up)
        {
            pos = default; down = held = up = false;
            var mouse = Mouse.current;
            if (mouse != null)
            {
                pos = mouse.position.ReadValue();
                down = mouse.leftButton.wasPressedThisFrame;
                held = mouse.leftButton.isPressed;
                up = mouse.leftButton.wasReleasedThisFrame;
                if (down || held || up) return true;
            }
            var ts = Touchscreen.current;
            if (ts != null)
            {
                foreach (var t in ts.touches)
                {
                    if (!t.press.isPressed && !t.press.wasReleasedThisFrame) continue;
                    pos = t.position.ReadValue();
                    down = t.press.wasPressedThisFrame;
                    held = t.press.isPressed;
                    up = t.press.wasReleasedThisFrame;
                    return true;
                }
            }
            return false;
        }

        void HandlePointer()
        {
            // 터치가 2개 이상이면(핀치 중) 팬/드래그는 건너뛴다 — HandlePinch가 전담.
            int touchCount = Touchscreen.current != null ? ActiveTouchCount() : 0;
            if (touchCount >= 2) { pointerActive = false; draggingUnit = false; return; }

            if (!TryGetPointer(out var pos, out var down, out var held, out var up))
            {
                pointerActive = false;
                return;
            }

            if (down)
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    pointerActive = false; // UGUI 버튼/패널 위에서 시작한 클릭은 무시
                    return;
                }
                pointerActive = true;
                var ray = cam.ScreenPointToRay(pos);
                var hitUnit = RaycastUnit(ray);
                if (hitUnit != null)
                {
                    draggingUnit = true;
                    draggedUnit = hitUnit;
                    draggedStartCell = hitUnit.Cell;
                }
                else
                {
                    draggingUnit = false;
                    if (RayGroundHit(ray, out var hit))
                    {
                        dragStartGroundHit = hit;
                        dragStartFocus = focus;
                    }
                }
                return;
            }

            if (!pointerActive) return;

            if (held)
            {
                var ray = cam.ScreenPointToRay(pos);
                if (draggingUnit)
                {
                    if (draggedUnit != null && RayGroundHit(ray, out var hit))
                        draggedUnit.SnapTo(new Vector2(hit.x / grid.CellSize, hit.z / grid.CellSize));
                }
                else if (RayGroundHit(ray, out var hit2))
                {
                    focus = dragStartFocus + (dragStartGroundHit - hit2);
                    ClampFocus();
                    Apply();
                }
                return;
            }

            if (up)
            {
                if (draggingUnit && draggedUnit != null)
                {
                    var dropCell = grid.BoardPosToCell(draggedUnit.BoardPosition);
                    if (dropCell != draggedStartCell && grid.IsOccupied(dropCell))
                    {
                        draggedUnit.SnapToCell(draggedStartCell, grid);
                    }
                    else
                    {
                        grid.ClearOccupied(draggedStartCell);
                        grid.MarkOccupied(dropCell);
                        draggedUnit.SnapToCell(dropCell, grid);
                    }
                }
                pointerActive = false;
                draggingUnit = false;
                draggedUnit = null;
            }
        }

        int ActiveTouchCount()
        {
            int n = 0;
            foreach (var t in Touchscreen.current.touches) if (t.press.isPressed) n++;
            return n;
        }

        SamgukUnit RaycastUnit(Ray ray)
        {
            if (Physics.Raycast(ray, out var hitInfo, 500f))
                return hitInfo.collider.GetComponentInParent<SamgukUnit>();
            return null;
        }

        static bool RayGroundHit(Ray ray, out Vector3 hit)
        {
            if (Mathf.Abs(ray.direction.y) > 1e-5f)
            {
                float t = -ray.origin.y / ray.direction.y;
                if (t >= 0f)
                {
                    hit = ray.origin + ray.direction * t;
                    return true;
                }
            }
            hit = Vector3.zero;
            return false;
        }

        // ── 휠(마우스) 줌 ──

        void HandleScroll()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;
            float delta = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(delta) < 0.01f) return;
            zoomT = Mathf.Clamp01(zoomT + delta * 0.0006f);
            Apply();
        }

        // ── 2손가락 핀치 줌 ──

        void HandlePinch()
        {
            var ts = Touchscreen.current;
            if (ts == null) { pinchStartDist = -1f; return; }

            int active = 0;
            Vector2 p0 = default, p1 = default;
            foreach (var t in ts.touches)
            {
                if (!t.press.isPressed) continue;
                if (active == 0) p0 = t.position.ReadValue();
                else if (active == 1) p1 = t.position.ReadValue();
                active++;
                if (active > 2) break;
            }

            if (active < 2) { pinchStartDist = -1f; return; }

            float dist = Vector2.Distance(p0, p1);
            if (pinchStartDist < 0f)
            {
                pinchStartDist = dist;
                pinchStartZoomT = zoomT;
                return;
            }
            if (pinchStartDist > 1f)
            {
                zoomT = Mathf.Clamp01(pinchStartZoomT + (dist - pinchStartDist) / 420f);
                Apply();
            }
        }

        void ClampFocus()
        {
            float half = grid.BoardHalfExtent * grid.CellSize;
            focus.x = Mathf.Clamp(focus.x, -half, half);
            focus.z = Mathf.Clamp(focus.z, -half, half);
            focus.y = 0f;
        }

        /// <summary>목표 위치/회전/FOV만 다시 계산한다 — 실제 카메라를 옮기는 건 Update()의
        /// 매프레임 보간이 전담한다(위 targetPos 필드 설명 참고).</summary>
        void Apply()
        {
            if (cam == null) return;
            float pitchDeg = Mathf.Lerp(maxPitchDeg, minPitchDeg, zoomT);
            float distance = Mathf.Lerp(maxDistance, minDistance, zoomT);
            float pitchRad = pitchDeg * Mathf.Deg2Rad;
            var forward = new Vector3(0f, -Mathf.Sin(pitchRad), Mathf.Cos(pitchRad));
            targetPos = focus - forward * distance;
            targetRot = Quaternion.Euler(pitchDeg, 0f, 0f);
            targetFov = Mathf.Lerp(maxFov, minFov, zoomT);
            if (SamgukUnit.BillboardCamera == null) SamgukUnit.BillboardCamera = cam.transform;
        }
    }
}

using UnityEngine;

namespace TransitCity
{
    /// <summary>
    /// 도로 한 칸의 비주얼. 공사 중엔 낮게 깔려있다가 완공되면 솟아오르며
    /// 펀치 스케일로 "짠!"한다. 혼잡은 더 이상 도로 색으로 표현하지 않는다 —
    /// 이제 차량이 실제로 느려지고 멈추는 게 눈에 보이므로 색까지 바뀔 필요가
    /// 없다는 판단(중복 정보).
    /// </summary>
    public class RoadVisual : GridVisualBase
    {
        RoadCell cell;
        RoadTypeData data;
        Vector3 fullScale;
        bool completedPunchPlayed;

        public void Bind(RoadCell cell, RoadTypeData data, Vector3 fullScale)
        {
            this.cell = cell;
            this.data = data;
            this.fullScale = fullScale;
            completedPunchPlayed = false;
            SetScaleGrounded(new Vector3(fullScale.x, fullScale.y * 0.08f, fullScale.z));
            SetColor(new Color(data.baseColor.r, data.baseColor.g, data.baseColor.b, 0.5f));
        }

        /// <summary>피벗이 중심인 큐브가 바닥(y=0)에서 위로 자라는 것처럼 보이게 보정.</summary>
        void SetScaleGrounded(Vector3 scale)
        {
            transform.localScale = scale;
            var p = transform.localPosition;
            p.y = scale.y * 0.5f;
            transform.localPosition = p;
        }

        /// <summary>매 틱(또는 완공 이벤트) 후 호출 — 모델 상태를 읽어 비주얼을 갱신한다.</summary>
        public void Refresh()
        {
            if (cell == null) return;

            if (cell.State is BuildingState building)
            {
                float p = building.Progress01;
                SetScaleGrounded(new Vector3(fullScale.x, Mathf.Lerp(fullScale.y * 0.08f, fullScale.y, p), fullScale.z));
                return;
            }

            if (!completedPunchPlayed)
            {
                completedPunchPlayed = true;
                SetScaleGrounded(fullScale);
                PlayPunch(fullScale);
                SetColor(data.baseColor);
            }
        }
    }
}

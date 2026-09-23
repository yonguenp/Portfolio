using UnityEngine;

namespace TransitCity
{
    /// <summary>
    /// 건물 한 채의 비주얼 — 이제 1칸짜리 고정 박스가 아니라 모델이 실제로
    /// 점유한 발자국(1x1/1x2/2x2, CityTile.FootprintExtensions)만큼 진짜로
    /// 늘어난다. 인구가 늘수록 박스가 커지고, 재산 수준(상대 백분위, §5.5.1)에
    /// 따라 색이 바뀌며, 발생 순간엔 펀치 스케일로 눈에 띄게 "짠" 하고
    /// 나타난다(§11 단순 3D 박스). 같은 컴포넌트를 공공시설(§5.8 단순화)
    /// 비주얼로도 재사용한다 — BindUtility로 바인딩하면 tile이 null로 남아
    /// Refresh()의 인구 성장 로직이 자동으로 꺼진다.
    ///
    /// 위치(칸 중심으로부터의 오프셋)는 이 컴포넌트가 아니라 발자국 전체의
    /// 중심을 아는 TransitCityManager가 매 틱 다시 맞춰준다 — 여기선 크기만
    /// 책임진다.
    /// </summary>
    public class HouseVisual : GridVisualBase
    {
        static readonly Color PoorColor = new Color(0.78f, 0.68f, 0.5f);
        static readonly Color RichColor = new Color(0.42f, 0.5f, 0.85f);

        CityTile tile;
        Vector3 cellFootprint;
        float cellSize;
        float minHeight;
        float maxHeight;
        bool spawnPunchPlayed;

        public void Bind(CityTile tile, Vector3 cellFootprint, float cellSize, float minHeight, float maxHeight)
        {
            this.tile = tile;
            this.cellFootprint = cellFootprint;
            this.cellSize = cellSize;
            this.minHeight = minHeight;
            this.maxHeight = maxHeight;
            spawnPunchPlayed = false;
            SetColor(PoorColor);
            SetScaleGrounded(new Vector3(cellFootprint.x, minHeight * 0.3f, cellFootprint.z));
        }

        /// <summary>
        /// 공공시설(§5.8)용 — 인구 성장 로직 없이 고정된 색으로 배치하되, 티어가
        /// 오를수록(소형→중형→대형) 몸집이 커진다. 최초 배치와 티어 승급 갱신
        /// 양쪽에서 재사용한다(둘 다 "지금 티어에 맞는 모습으로 맞춘다"는 점에서 동일).
        /// 바닥 면적은 이제 spanCells(실제 점유 칸 수, 1x1/1x2/2x2)를 그대로
        /// 반영한다 — 예전엔 칸 밖으로 못 나가게 살짝만 부풀리는 "가짜 범프"였는데,
        /// 이제 모델이 실제로 그만큼 칸을 점유하므로 비주얼도 그만큼 커져야 맞다.
        /// 실제 "공간이 없으면 승급 못 한다/이주한다" 판단은 CityGridModel이
        /// 미리 해주므로, 여기 비주얼은 그 결과(허용된 tier·발자국)만 그대로 그린다.
        /// </summary>
        public void BindUtility(Vector3 baseFootprint, float cellSize, float baseHeight, Color color, int tier, Vector2Int spanCells)
        {
            tile = null;
            this.cellFootprint = baseFootprint;
            this.cellSize = cellSize;
            SetColor(color);
            float heightScale = 1f + 0.5f * (tier - 1);
            var target = new Vector3(SpanWidth(spanCells.x), baseHeight * heightScale, SpanWidth(spanCells.y));
            SetScaleGrounded(target);
            PlayPunch(target);
        }

        /// <summary>1칸일 때의 여백(cellSize - cellFootprint)을 그대로 유지한 채, 칸 수만큼 늘어난 폭을 계산한다.</summary>
        float SpanWidth(int spanCells) => cellSize * spanCells - (cellSize - cellFootprint.x);

        public void Refresh(Vector2Int footprintSpanCells)
        {
            if (tile == null) return;

            float sizeTier = Mathf.Clamp(tile.SizeTier, 0.8f, 1.2f);
            float ratio = tile.Capacity <= 0 ? 0f : Mathf.Clamp01(tile.Population / (float)tile.Capacity);

            // 밀도(인구/정원 비율)가 낮을수록 바닥 면적도 좁게 — 저밀도는 작고 낮은
            // 건물, 고밀도는 크고 높은 건물로 보이게 한다("저밀도는 낮게, 고밀도는
            // 높고 커지게" 요청). 발자국 자체(칸 수)는 이미 모델이 정한 실제
            // 점유 범위이므로, 여기서는 그 범위 "안에서" 얼마나 꽉 채우는지만 조절한다.
            float densityFootprintScale = Mathf.Lerp(0.55f, 1f, ratio);
            float tierHeightScale = 1f + 0.6f * (tile.Tier - 1);

            var footprint = new Vector3(
                SpanWidth(footprintSpanCells.x) * sizeTier * densityFootprintScale,
                cellFootprint.y,
                SpanWidth(footprintSpanCells.y) * sizeTier * densityFootprintScale);

            float height = Mathf.Lerp(minHeight, maxHeight, ratio) * sizeTier * tierHeightScale;
            var target = new Vector3(footprint.x, height, footprint.z);

            // 상업·공업지구 개념이 없어지고 나서 Refresh()는 사실상 주거지 전용이다
            // (상점·공장 등 다른 "박스형" 건물은 전부 IsUtilityKind라 BindUtility
            // 경로를 타므로 여기까지 안 온다).
            SetColor(Color.Lerp(PoorColor, RichColor, tile.Wealth));

            if (!spawnPunchPlayed)
            {
                spawnPunchPlayed = true;
                SetScaleGrounded(target);
                PlayPunch(target);
            }
            else
            {
                SetScaleGrounded(Vector3.Lerp(transform.localScale, target, 0.6f));
            }
        }

        /// <summary>피벗이 중심인 큐브가 바닥(y=0)에서 위로 자라는 것처럼 보이게 보정. X/Z(발자국 중심 위치)는 건드리지 않는다 — 그건 Manager가 매 틱 따로 맞춘다.</summary>
        void SetScaleGrounded(Vector3 scale)
        {
            transform.localScale = scale;
            var p = transform.localPosition;
            p.y = scale.y * 0.5f;
            transform.localPosition = p;
        }
    }
}

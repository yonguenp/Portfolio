namespace TransitCity
{
    /// <summary>
    /// 도로 한 칸의 시뮬레이션 상태(순수 C#, GameObject 아님). 필드 하나 바뀔
    /// 때마다 이벤트를 쏘지 않는다 — CityGridModel이 틱 단위로 굵직한
    /// 이벤트(OnTickCompleted 등)만 묶어서 발행하고, 비주얼은 그때 최신
    /// 상태를 읽어간다(이벤트 스팸 방지). 어떤 등급(소로/대로)으로 지어졌는지
    /// 스스로 기억해 혼잡 판정·시각화·대로변 판정에 쓴다.
    /// </summary>
    public sealed class RoadCell
    {
        public readonly GridCoord Coord;
        public RoadTypeData Type { get; }
        public RoadConstructionState State { get; private set; }
        public int TripLoad { get; private set; }

        /// <summary>교차로 처리 방식 — 도로가 3방향 이상 만나는 칸에서만 의미가 있다(§CityGridModel.IsIntersection). 기본은 미설치.</summary>
        public IntersectionControl Control { get; set; } = IntersectionControl.Uncontrolled;

        public RoadCell(GridCoord coord, RoadTypeData type)
        {
            Coord = coord;
            Type = type;
            State = new BuildingState(type.buildTimeSeconds);
        }

        public bool IsTraversable => State.IsTraversable;

        /// <summary>상태가 바뀌었으면(Building→Completed) true를 돌려준다.</summary>
        public bool Tick(float deltaSeconds)
        {
            var next = State.Tick(this, deltaSeconds);
            bool changed = next != State;
            State = next;
            return changed;
        }

        public void ResetTripLoad() => TripLoad = 0;
        public void AddTrip() => TripLoad++;
    }
}

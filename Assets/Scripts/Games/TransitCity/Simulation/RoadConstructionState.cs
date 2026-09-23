namespace TransitCity
{
    /// <summary>
    /// §4.7(건설 시간) 상태 패턴 — 도로 한 칸이 지금 어느 상태인지.
    /// 상속을 적극 활용하는 이 프로젝트 컨벤션(§18)대로, enum 분기 대신
    /// 상태마다 클래스를 둔다.
    /// </summary>
    public abstract class RoadConstructionState
    {
        public abstract bool IsTraversable { get; }

        /// <summary>매 틱 호출. 다음 상태로 전이해야 하면 그 상태를 반환, 아니면 this.</summary>
        public abstract RoadConstructionState Tick(RoadCell road, float deltaSeconds);
    }

    public sealed class BuildingState : RoadConstructionState
    {
        readonly float buildTimeSeconds;
        float elapsed;

        public BuildingState(float buildTimeSeconds)
        {
            this.buildTimeSeconds = buildTimeSeconds;
        }

        public override bool IsTraversable => false;

        public float Progress01 => buildTimeSeconds <= 0f ? 1f : UnityEngine.Mathf.Clamp01(elapsed / buildTimeSeconds);

        public override RoadConstructionState Tick(RoadCell road, float deltaSeconds)
        {
            elapsed += deltaSeconds;
            if (elapsed >= buildTimeSeconds)
            {
                Logger.Log($"도로 완공: {road.Coord}", 1);
                return new CompletedState();
            }
            return this;
        }
    }

    public sealed class CompletedState : RoadConstructionState
    {
        public override bool IsTraversable => true;

        public override RoadConstructionState Tick(RoadCell road, float deltaSeconds) => this;
    }
}

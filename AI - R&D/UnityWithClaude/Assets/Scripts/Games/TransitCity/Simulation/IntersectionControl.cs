namespace TransitCity
{
    /// <summary>
    /// 교차로(도로 칸에 이어진 도로가 3방향 이상) 처리 방식. 기본은 Uncontrolled —
    /// 아무도 안 지어주면 사거리 전원이 멈췄다가 순서대로 지나가는(4-way stop)
    /// 취급이라 실효 용량이 크게 낮다. 로터리/신호등을 지으면 용량이 올라간다.
    /// </summary>
    public enum IntersectionControl
    {
        Uncontrolled,
        Roundabout,
        TrafficLight,
    }
}

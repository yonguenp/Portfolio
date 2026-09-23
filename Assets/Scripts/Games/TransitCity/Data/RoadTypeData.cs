using UnityEngine;

namespace TransitCity
{
    /// <summary>도로 등급 — 대로변에만 인프라/상업/공업이 들어서는 규칙(§5.8)의 판단 기준.</summary>
    public enum RoadTier
    {
        Local,
        Avenue,
    }

    /// <summary>
    /// 도로 계층 하나의 밸런스 수치(§4.1/§4.7). 코드에 하드코딩하지 않고
    /// 이 스키마로 정의해 .asset 인스턴스에서 값을 조정한다.
    /// </summary>
    [CreateAssetMenu(fileName = "RoadType", menuName = "TransitCity/Road Type")]
    public class RoadTypeData : ScriptableObject
    {
        public string displayName = "소로";
        public RoadTier tier = RoadTier.Local;
        public int cost = 50;
        public float buildTimeSeconds = 2.5f;

        [Tooltip("이 값을 넘는 통행량(trip)이 몰리면 혼잡으로 취급한다.")]
        public int tripCapacity = 15;

        public Color baseColor = new Color(0.55f, 0.55f, 0.58f);
        public Color congestedColor = new Color(0.85f, 0.2f, 0.15f);
    }
}

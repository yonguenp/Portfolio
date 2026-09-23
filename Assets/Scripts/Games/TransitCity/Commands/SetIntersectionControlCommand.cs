namespace TransitCity
{
    /// <summary>교차로에 로터리/신호등을 짓는다(또는 미설치로 되돌린다) — Command 패턴(§18.2 확정).</summary>
    public sealed class SetIntersectionControlCommand : ICommand
    {
        readonly CityGridModel model;
        readonly GridCoord coord;
        readonly IntersectionControl control;
        readonly int cost;

        public SetIntersectionControlCommand(CityGridModel model, GridCoord coord, IntersectionControl control, int cost)
        {
            this.model = model;
            this.coord = coord;
            this.control = control;
            this.cost = cost;
        }

        public bool CanExecute()
        {
            var road = model.GetRoad(coord);
            if (road != null && road.Control == control) return false; // 이미 같은 방식이면 다시 지을 필요 없음.
            return model.Money >= cost && model.CanSetIntersectionControl(coord);
        }

        public void Execute()
        {
            model.AddMoney(-cost);
            model.SetIntersectionControl(coord, control);
        }
    }
}

namespace TransitCity
{
    /// <summary>도로 한 칸 건설 — Command 패턴(§18.2 확정).</summary>
    public sealed class BuildRoadCommand : ICommand
    {
        readonly CityGridModel model;
        readonly RoadTypeData roadType;
        readonly GridCoord coord;

        public BuildRoadCommand(CityGridModel model, RoadTypeData roadType, GridCoord coord)
        {
            this.model = model;
            this.roadType = roadType;
            this.coord = coord;
        }

        public bool CanExecute() =>
            model.Money >= roadType.cost && model.CanBuildRoadAt(coord);

        public void Execute()
        {
            model.AddMoney(-roadType.cost);
            model.BuildRoad(coord, roadType);
        }
    }
}

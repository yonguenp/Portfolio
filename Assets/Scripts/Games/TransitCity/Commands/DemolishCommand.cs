namespace TransitCity
{
    /// <summary>도로 또는 건물 한 칸 철거 — Command 패턴(§18.2 확정).</summary>
    public sealed class DemolishCommand : ICommand
    {
        readonly CityGridModel model;
        readonly GridCoord coord;

        public DemolishCommand(CityGridModel model, GridCoord coord)
        {
            this.model = model;
            this.coord = coord;
        }

        public bool CanExecute() => model.CanDemolishAt(coord);
        public void Execute() => model.DemolishAt(coord);
    }
}

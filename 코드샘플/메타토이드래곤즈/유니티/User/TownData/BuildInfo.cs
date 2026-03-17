namespace SandboxNetwork
{
    public class BuildInfo
    {
        public BuildInfo(int tag, int level, eBuildingState state, int activeTime = -1)
        {
            SetData(tag, level, state, activeTime);
        }
        public int Tag { get; private set; } = -1;
        public eBuildingState State { get; private set; } = eBuildingState.NONE;
        public int Level { get; private set; } = -1;
        public int ActiveTime { get; private set; } = -1;
        public void SetData(int tag, int level, eBuildingState state, int activeTime = -1)
        {
            Tag = tag;
            Level = level;
            SetState(state);
            ActiveTime = activeTime;
        }
        public void SetState(eBuildingState state)
        {
            State = state;
            //if (Town.Instance != null)
            //{
                //var building = Town.Instance.GetBuilding(Tag);
                //if (state == eBuildingState.NORMAL && building != null)
                //{
                //    BuildCompleteEvent.Send(building, state);
                //}
            //}
        }
    }
}
namespace SandboxNetwork
{
    public struct ItemFrameEvent
    {
        public enum ItemFrameEventEnum
        {
            ITEM_UPDATE,
        }

        public ItemFrameEventEnum Event;

        public ItemFrameEvent(ItemFrameEventEnum _Event)
        {
            Event = _Event;
        }

        public static void ItemUpdate()
        {
            EventManager.TriggerEvent(new ItemFrameEvent(ItemFrameEventEnum.ITEM_UPDATE));
        }
    }
}
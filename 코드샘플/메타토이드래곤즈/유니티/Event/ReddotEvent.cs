namespace SandboxNetwork
{
    public struct ReddotEvent
    {
        static ReddotEvent Event;

        public eReddotEvent type;
        public bool state;
        

        public static void SendReddotEvent(eReddotEvent type, bool state)
        {
            Event.type = type;
            Event.state = state;

            EventManager.TriggerEvent(Event);
        }
    }
}
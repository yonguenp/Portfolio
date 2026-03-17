using SandboxNetwork;

public struct GuildEvent
{
    public enum eGuildEventType
    {
       LostGuild,    // 길드에 길드 없는 상태, 강퇴든 자의적으로 나가든
       GuildRefresh, // 
    }

    public eGuildEventType Event;
    static GuildEvent e;

    public GuildEvent(eGuildEventType _Event)
    {
        Event = _Event;
    }

    public static void RefreshGuildUI(eGuildEventType _event)
    {
        e.Event = _event;
        EventManager.TriggerEvent(e);
    }
   
}
namespace SandboxNetwork 
{ 
    public struct TutorialEvent
    {
        public enum TutorialEventEnum
        {
            START_TUTORIAL,
            NEXT_TUTORIAL,
            END_TUTORIAL
        }

        public int tutorialGroup;
        public int tutorialSeq;

        public TutorialEventEnum tutorialEventType;
        static TutorialEvent tutorialEvent;

        public TutorialEvent(TutorialEventEnum eventType, int group, int seq)
        {
            tutorialEventType = eventType;
            tutorialGroup = group;
            tutorialSeq = seq;
        }

        public static void OnTutorialEvent(TutorialEventEnum type, int group, int seq)
        {
            tutorialEvent.tutorialEventType = type;
            tutorialEvent.tutorialGroup = group;
            tutorialEvent.tutorialSeq = seq;

            EventManager.TriggerEvent(tutorialEvent);
        }
    }
}

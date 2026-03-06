namespace SandboxNetwork
{
    public class ChatWarningPrefab : ChatPrefab
    {
        public override void Init(eChatUIType _type, string _comment)//시간 표시 부분은 타임스탬프
        {
            chatUIType = _type;

            SetChatPrefabHardcode();

            SetCommentLabel(_comment);
            SetTimeLabel(false);//시간 포맷 끄기
            
            RebuildLayout();
        }

        public override void SetCommentLabel(string _comment)
        {
            base.SetCommentLabel(_comment);
        }
    }
}
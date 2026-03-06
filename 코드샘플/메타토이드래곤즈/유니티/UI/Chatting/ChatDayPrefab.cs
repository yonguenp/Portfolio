namespace SandboxNetwork
{
    public class ChatDayPrefab : ChatPrefab
    {
        public override void Init(eChatUIType _type, string _comment)//시간 표시 부분은 타임스탬프
        {
            chatUIType = _type;
            SetCommentLabel(_comment);//날짜 포맷 및 라벨 물어보기
        }

        public override void SetCommentLabel(string _comment)
        {
            if (commentLabel == null)
                return;

            string resultText = _comment;
            if(int.TryParse(_comment,out int result))
            {
                var dateTime = SBFunc.TimeStampToDateTime(result);
                resultText = dateTime.ToString("yyyy.MM.dd");
            }

            commentLabel.text = resultText;
        }
    }
}
using System.Collections.Generic;

namespace TransitCity
{
    /// <summary>
    /// 실행된 커맨드 이력을 들고 있는 순수 C# 클래스(비-MonoBehaviour).
    /// 지금은 이력만 쌓지만, §4.7의 "공사 목록" UI나 나중의 실행취소 기능이
    /// 이 이력을 그대로 소비할 수 있게 구조만 미리 잡아둔다.
    /// </summary>
    public sealed class CommandInvoker
    {
        readonly List<ICommand> history = new List<ICommand>();

        public IReadOnlyList<ICommand> History => history;

        public bool TryExecute(ICommand command)
        {
            if (command == null || !command.CanExecute())
            {
                Logger.Log("커맨드 실행 불가(CanExecute=false)", 2);
                return false;
            }

            command.Execute();
            history.Add(command);
            Logger.Log($"커맨드 실행: {command.GetType().Name}", 2);
            return true;
        }
    }
}

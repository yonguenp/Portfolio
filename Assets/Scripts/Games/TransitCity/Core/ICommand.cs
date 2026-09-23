namespace TransitCity
{
    /// <summary>
    /// 건설/철거 같은 플레이어 액션을 캡슐화하는 커맨드.
    /// 인터페이스를 쓰는 의도적인 예외 지점 — 커맨드는 서로 완전히 무관한
    /// 종류(도로 건설, 철거, 나중엔 철도 등)라 상속 트리로 묶을 이유가 없다.
    /// </summary>
    public interface ICommand
    {
        /// <summary>지금 이 커맨드를 실행할 수 있는 상태인지(예산 등).</summary>
        bool CanExecute();

        void Execute();
    }
}

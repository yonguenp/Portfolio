using System;
//using System.IO;
//using System.Threading.Tasks;

#region SafeHandle을 사용한 IDisposable 단독 구현 코드에서 사용
//using System.Runtime.InteropServices;
//using Microsoft.Win32.SafeHandles;
#endregion

namespace SBCommonLib
{
    /// <summary>
    /// 관리되지 않는(unmanaged) 자원을 해제하기 위해서 사용 하는 함수이다.
    /// unmanaged 자원은 무엇인가?
    /// 쉽게 생각해서 "메모리가 아닌 자원" 즉, 윈도우 핸들, 파일 핸들, 소켓 핸들 등 시스템 자원을 뜻한다. 반대로 managed 는 new List<int>() 등, 메모리처럼 쓰는 자원들이다.
    /// using 문에서 사용하기 위해 필요한 IDisposable 인터페이스를 구현한 베이스 클래스
    /// 메시지 클래스에서 이걸 상속 받아 Dispose 패턴을 사용할지 고민 중.
    /// </summary>
    #region IDisposable 단독 구현
    public class SBDisposedClass : IDisposable
    {
        /// <summary>
        /// Dispose 두번 이상 호출을 막기 위한 것.
        /// _isDisposed 매개 변수가 소멸자에서 호출되는 경우 false이고,
        /// IDisposable.Dispose 메서드에서 호출되는 경우 true 여야 함.
        /// 즉, 명시적으로 호출되는 경우에는 true이고, 그렇지 않은 경우에는 false 임.
        /// </summary>
        private bool _isDisposed = false;

        /// <summary>
        /// Operating System handle의 안전한 사용을 위한 랩퍼 클래스.
        /// unmanaged resource의 안전한 메모리 관리를 위해서 사용이 권장됨.
        /// Dispose pattern과 함께 사용이 권장되며, if (isDisposing_) 안에서 처리함.
        /// </summary>
        //SafeHandle _safeHandle = new SafeFileHandle(IntPtr.Zero, true);

        /// <summary>
        /// finalizer 가 호출된 시점에선 이미 Dispose()가 호출 되지 않았으므로 managed resource 는 지우지 않는다.
        /// 기본 클래스는 managed resource만 참조할 수 있으며 Dispose 패턴을 구현할 수 있다.
        /// 이러한 겨우 소멸자는 필요하지 않다.
        /// 소멸자는 unmanaged resource를 직접 참조하는 경우에만 필요함.
        /// </summary>
        ~SBDisposedClass() => Dispose(false);

        /// <summary>
        /// Dispose 호출
        /// </summary>
        //public void Dispose() => Dispose(true);
        public void Dispose()
        {
            // managed 까지 제거
            Dispose(true);
            // managed 까지 지웠으므로, 이 객체는 finalizer 호출하지 말라고 등록한다.
            // Object.Finalize를 별도로 구성하지 않고, SafeHandle을 사용하는 경우엔
            // 아래 코드가 아무런 영향을 주지 않는다.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing_)
        {
            if (_isDisposed)
                return;

            //var callStack = SBUtil.CallStackLog(SBLogLevel.Trace);
            //SBLog.PrintInfo($"Called SBDisposedClass.Dispose({isDisposing_}). IsDisposed: {_isDisposed}. CallStack: {callStack}");
            if (isDisposing_)
            {
                // SafeHandle에 대한 처리는 여기서 한다.
                //_safeHandle?.Dispose();
                // ToDo: dispose managed objects.
            }

            // ToDo: dispose unmanaged objects.

            _isDisposed = true;
        }
    }
    #endregion

    #region IDisposable, IAsyncDisposable 동시 구현 (미사용 - .Net Standard 2.0 이하에서 사용 불가)
#if false
    public class SBAsyncDisposedClass : IDisposable, IAsyncDisposable
    {
        IDisposable _disposableResource = new MemoryStream();
        IAsyncDisposable _asyncDisposableResource = new MemoryStream();

        void IDisposable.Dispose()
        {
            SBLog.PrintTrace($"Call SBAsyncDisposedClass.IDisposable.Dispose().");
            Dispose(true);
            GC.SuppressFinalize(this);
            SBLog.PrintTrace($"SBAsyncDisposedClass.IDisposable.Dispose() Done.");
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            SBLog.PrintTrace($"Call SBAsyncDisposedClass.IAsyncDisposable.DisposeAsync().");
            await DisposeAsyncCore();

            Dispose(false);
            GC.SuppressFinalize(this);
            SBLog.PrintTrace($"SBAsyncDisposedClass.IAsyncDisposable.DisposeAsync() Done.");
        }

        protected virtual void Dispose(bool isDisposing_)
        {
            SBLog.PrintTrace($"Call SBAsyncDisposedClass.Dispose({isDisposing_}).");
            if (isDisposing_)
            {
                _disposableResource?.Dispose();
                (_asyncDisposableResource as IDisposable)?.Dispose();
            }

            _disposableResource = null;
            _asyncDisposableResource = null;
            SBLog.PrintTrace($"SBAsyncDisposedClass.Dispose({isDisposing_}) Done.");
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            SBLog.PrintTrace($"Call SBAsyncDisposedClass.DisposeAsyncCore().");
            if (_asyncDisposableResource != null)
            {
                await _asyncDisposableResource.DisposeAsync().ConfigureAwait(false);
            }

            if (_disposableResource is IAsyncDisposable disposable)
            {
                await disposable.DisposeAsync().ConfigureAwait(false);
            }
            else
            {
                _disposableResource.Dispose();
            }

            _asyncDisposableResource = null;
            _disposableResource = null;
            SBLog.PrintTrace($"SBAsyncDisposedClass.DisposeAsyncCore() Done.");
        }
    }
#endif
    #endregion
}

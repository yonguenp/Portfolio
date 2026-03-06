#define Lazy

using System;

/// <summary>
/// SandBox Common Library
/// </summary>
namespace SBCommonLib
{
    /// <summary>
    /// Thread Safe Singleton 클래스
    /// </summary>
    public class SBSingleton<T> where T : class, new()
    {
#if Lazy

        /// <summary>
        /// Lazy 사용.
        /// Lazy는 lock을 걸지 않아도 thread safe를 보장함.
        /// </summary>
        #region Lazy

        private static readonly Lazy<T> _instance = new Lazy<T>(() => new T());
        // 다음의 라인들은 정확히 윗 라인과 동일한 효과를 가진다
        //() => new T(), true);
        //() => new T(), LazyThreadSafetyMode.ExecutionAndPublication);
        // 아래 부분으로 대체하여 테스트 해보는 것도 좋은 방법이다.
        //() => new T(), LazyThreadSafetyMode.PublicationOnly);

        /// <summary>
        /// 중요 : Lazy 초기화는 thread-safe하지만, 생성된 오브젝트는 보호받지 못한다
        ///        LargeObject는 thread-safe 하지 않은 오브젝트이므로,
        ///        멀티 스레드 접근시 lock으로 보호해 주어야 한다
        /// </summary>
        public static T Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        #endregion

#else

        /// <summary>
        /// Lazy를 사용하지 않은 버전.
        /// lock을 이용해 thread safe 확보.
        /// 매번 lock이 잡히므로 성능이 저하됨.
        /// </summary>
        #region Non-Lazy

        private static T _instance;
        private static readonly object _lockObject = new object();

        public static T Instance
        {
            get
            {
                lock (_lockObject)
                {
                    if (null == _instance)
                    {
                        T local = default(T);
                        _instance = (null == local) ? Activator.CreateInstance<T>() : local;
                    }
                }

                return _instance;
            }
        }

        #endregion

#endif
    }
}

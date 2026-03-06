using System;


namespace SBCommonLib
{
    public  class SBSingleton<T> where T : class, new ()
    {
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
    }
}

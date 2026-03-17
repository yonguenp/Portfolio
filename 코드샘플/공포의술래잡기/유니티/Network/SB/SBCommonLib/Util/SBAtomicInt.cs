using System.Threading;

/// <summary>
/// SandBox Common Library
/// </summary>
namespace SBCommonLib
{
    /// <summary>
    /// 원자성을 지닌 Int 값
    /// 스레드에 대한 안전성을 보장해야 할 플래그가 있다면 이것을 활용함.
    /// </summary>
    public class SBAtomicInt
    {
        /// <summary>
        /// 값이 갖는 의미는 실제 인스턴스화 했을 때 사용되는 용도에 따라 다르게 해석될 수 있기 때문에 명확하게 정의하지는 않음.
        /// 일단 0 == false, 1 == true 정도로 이해하면 됨.
        /// </summary>
        private int _value = 0;

        public int Value
        {
            get => _value;
            set
            {
                Interlocked.Exchange(ref _value, value);
            }
        }

        public SBAtomicInt()
        {
            _value = 0;
        }

        public SBAtomicInt(int initValue_)
        {
            _value = initValue_;
        }

        /// <summary>
        /// 충돌방지용. 확인 후 켬.(CAS: Compare-And-Swap)
        /// 값 확인 후 값이 0이면 1로 변경함.
        /// 원래 값이 0이었다면 true를 리턴함.
        /// 원래 값이 1이었다면 false를 리턴함.
        /// 값에 대한 비교와 변경을 원자성 작업으로 수행함.
        /// </summary>
        /// <returns>true/false</returns>
        public bool CasOn()
        {
            return Interlocked.CompareExchange(ref _value, 1, 0) == 0;
        }

        /// <summary>
        /// 충돌방지용. 확인 후 끔.(CAS: Compare-And-Swap)
        /// 값 확인 후 값이 1이면 0로 변경함.
        /// 원래 값이 0이었다면 true를 리턴함.
        /// 원래 값이 1이었다면 false를 리턴함.
        /// 값에 대한 비교와 변경을 원자성 작업으로 수행함.
        /// </summary>
        /// <returns>true/false</returns>
        public bool CasOff()
        {
            return Interlocked.CompareExchange(ref _value, 0, 1) == 0;
        }

        /// <summary>
        /// 지정된 값(1)으로 값 변경
        /// </summary>
        public void On()
        {
            Interlocked.Exchange(ref _value, 1);
        }

        /// <summary>
        /// 지정된 값(0)으로 값 변경
        /// </summary>
        public void Off()
        {
            Interlocked.Exchange(ref _value, 0);
        }

        /// <summary>
        /// 현재 값이 1인지 아닌지 비교
        /// </summary>
        /// <returns></returns>
        public bool IsOn()
        {
            return _value == 1;
        }

        /// <summary>
        /// 원자 단위 연산으로 지정된 변수를 증가시키고 결과를 저장
        /// </summary>
        public int Incrementer()
        {
            return Interlocked.Increment(ref _value);
        }

        /// <summary>
        /// 원자 단위 연산으로 지정된 변수를 감소시키고 결과를 저장
        /// </summary>
        public int Decrementer()
        {
            return Interlocked.Decrement(ref _value);
        }
    }
}

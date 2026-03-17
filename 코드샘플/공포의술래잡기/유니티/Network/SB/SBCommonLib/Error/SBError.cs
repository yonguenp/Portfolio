/// <summary>
/// SandBox Common Library
/// </summary>
namespace SBCommonLib
{
    /// <summary>
    /// 에러 코드 & 메시지 관련 클래스
    /// </summary>
    public class SBError
    {
        /// <summary>
        /// 에러 코드
        /// </summary>
        /// 
        public int ErrorCode { get; private set; }

        /// <summary>
        /// 에러 메시지
        /// </summary>
        /// 
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public SBError()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="errorCode_">에러 코드</param>
        /// <param name="errorMessage_">에러 메시지</param>
        public SBError(int errorCode_, string errorMessage_)
        {
            ErrorCode = errorCode_;
            ErrorMessage = errorMessage_;
        }

        /// <summary>
        /// 에러 코드 및 메시지 출력을 위한 override된 ToString 함수
        /// </summary>
        /// <returns>에러 출력 포멧으로 작성된 String값</returns>
        public override string ToString()
        {
            return string.Format("Code: {0}, Message: {1}", ErrorCode, ErrorMessage);
        }
    }
}
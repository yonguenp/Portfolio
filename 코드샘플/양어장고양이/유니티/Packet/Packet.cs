/**
 * Api 응답의 최상단 에러 코드. 0 == OK
 * 네트워크, 서버, 파라미터 등 기초적인 오류 여부만 판단하는데 쓰임
 * 컨텐츠별 상황에 따른 응답 구분은 response["api"] 어레이 밑에 각각 구현
 */
public enum eResponseCode
{
    OK = 0,
    
    /**
     *  1 ~ 10 : soft error response
     */
    // 서버 점검중
    SERVER_MAINTENANCE = 1,
    // 클라이언트 버전이 최소 허용 미만
    VERSION_TOO_LOW = 2,
    // 세션 토큰 불일치로 인한 로그아웃
    DUPLICATED_LOGIN = 3,
    // 기타 이유로 서버측 로그아웃 처리
    LOGGED_OUT_BY_SERVER = 4,

    /**
     * 11 ~ : client side errors
     */
    PARAM_ERR = 11,

    /**
     * 21 ~ : server side errors
     */
    // server down, no network, ...
    CANNOT_CONNECT = 21,
    // 알 수 없는 서버 오류
    SERVER_ERROR = 22,
    // busy
    SERVER_TOO_BUSY = 23,
    // SQL db 오류
    SQL_ERROR = 24,
    // Redis 오류
    REDIS_ERROR = 25,
    // script file error
    SCRIPT_NOT_FOUND = 26,
}

/**
 * Api 응답 최상단 값들을 빠르게 parsing하기 위한 스키
 * 
 */
[System.Serializable]
public class ResponseRoot
{
    public eResponseCode rs;
    public string uri;
    public int op;
    public int ts;
}
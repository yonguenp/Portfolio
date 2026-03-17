namespace SandboxPlatform.SAMANDA
{
    public enum LOGIN_STATE
    {
        UNKNOWN = -1,
        VALID_ACCOUNT = 0,
        NO_ACCOUNT_INFO = 1,
        TERMSOFUSE = 2,
        LOGIN_DONE = 3,
    };

    public enum AUTH_TYPE
    {
        NONE = -1, SB = 0, NV, GG, KK, IG, FB, AP, GE,
    };

    public enum DeviceEnvironment
    {
        None,
        None_Mobile,
        Google_Mobile,
        IOS_Mobile,
    }
}
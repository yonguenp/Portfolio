using System;
using System.Collections.Generic;
using System.Text;

using SBCommonLib;

namespace SBSocketSharedLib
{
    public enum ErrorCode : sbyte
    {
        Undefined = -2,
        Unknown = -1,
        Success = 0,

        SystemError = 1,
        ProcessingError = 2,
    }

    public static class Error
    {
        public static string GetMessage(ErrorCode errorCode)
        {
            if (_errorDictionary.ContainsKey(errorCode))
                return _errorDictionary[errorCode];

            return string.Empty;
        }

        public static string GetMessage(int errorCode)
        {
            return GetMessage((ErrorCode)errorCode);
        }

        public static SBError GetError(ErrorCode errorCode)
        {
            return new SBError((int)errorCode, GetMessage(errorCode));
        }

        public static SBError GetError(int errorCode)
        {
            return new SBError(errorCode, GetMessage(errorCode));
        }

        private static Dictionary<ErrorCode, string> _errorDictionary = new Dictionary<ErrorCode, string>
        {
            {
                ErrorCode.Undefined, "Undefined error"
            },
            {
                ErrorCode.Unknown, "Unknown error"
            },
            {
                ErrorCode.Success, "Success"
            },
            {
                ErrorCode.SystemError, "System error"
            },
            {
                ErrorCode.ProcessingError, "Processing error"
            },
        };
    }
}

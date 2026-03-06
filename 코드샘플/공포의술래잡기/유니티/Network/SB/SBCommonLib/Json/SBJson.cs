#define JsonNet
//#undef JsonNet
//#define LitJson
//#undef LitJson

using System;

#if true
using Newtonsoft.Json;
#else
using LitJson;
#endif

/// <summary>
/// SandBox Common Library
/// </summary>
namespace SBCommonLib.Json
{
    /// <summary>
    /// Json 클래스
    /// </summary>
    public class SBJson
    {

#if true

        #region JsonNet

        /// <summary>
        /// Object -> Json string 변환
        /// </summary>
        /// <param name="object_">변환할 Object</param>
        /// <returns>변환된 Json string</returns>
        public static string ToString(Object object_)
        {
            if (null == object_)
            {
                return string.Empty;
            }

            try
            {
                return JsonConvert.SerializeObject(object_);
            }
            catch (JsonException e_)
            {
                SBLog.PrintError($"ToString - JsonException Error", e_, typeof(SBJson).Name);
                return string.Empty;
            }
        }

        /// <summary>
        /// Json string -> Object 변환
        /// </summary>
        /// <typeparam name="T">변환될 Object 타입</typeparam>
        /// <param name="jsonString_">변환할 Json string</param>
        /// <returns>변환된 Object</returns>
        public static T ToObject<T>(string jsonString_)
        {
            //if (null == jsonString_ || string.Empty == jsonString_)
            if (string.IsNullOrWhiteSpace(jsonString_))
            {
                return default(T);
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(jsonString_);
            }
            catch (JsonException e_)
            {
                SBLog.PrintError($"ToObject - JsonException Error", e_, typeof(SBJson).Name);
                return default(T);
            }
        }

        #endregion

#else

        #region LitJson

        /// <summary>
        /// Object -> Json string 변환
        /// </summary>
        /// <param name="object_">변환할 Object</param>
        /// <returns>변환된 Json string</returns>
        public static string ToString(Object object_)
        {
            if (null == object_)
            {
                return string.Empty;
            }

            try
            {
                return JsonMapper.ToJson(object_);
            }
            catch (JsonException e_)
            {
                SBLog.PrintError($"ToString - JsonException Error(Message: {e_.Message})", typeof(SBJson).Name);
                return string.Empty;
            }
        }

        /// <summary>
        /// Json string -> Object 변환
        /// </summary>
        /// <typeparam name="T">변환될 Object 타입</typeparam>
        /// <param name="jsonString_">변환할 Json string</param>
        /// <returns>변환된 Object</returns>
        public static T ToObject<T>(string jsonString_)
        {
            //if (null == jsonString_ || string.Empty == jsonString_)
            if (string.IsNullOrWhiteSpace(jsonString_))
            {
                return default(T);
            }

            try
            {
                return (T)JsonMapper.ToObject<T>(jsonString_);
            }
            catch (JsonException e_)
            {
                SBLog.PrintError($"ToObject - JsonException Error(Message: {e_.Message})", typeof(SBJson).Name);
                return default(T);
            }
        }

        #endregion

#endif

    }
}

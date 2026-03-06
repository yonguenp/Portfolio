using System;
using System.IO;

/// <summary>
/// SandBox Common Library
/// </summary>
namespace SBCommonLib.Json
{
    /// <summary>
    /// Json 로드 클래스
    /// </summary>
    /// <typeparam name="T">변환할 Object 타입</typeparam>
    public static class SBJsonLoader<T>
    {
        /// <summary>
        /// 파일로 저장된 Json string -> Object 로드
        /// </summary>
        /// <param name="path_">파일 경로</param>
        /// <returns></returns>
        public static T LoadFromFileSystem(string path_)
        {
            try
            {
                string textValue = File.ReadAllText(path_);
                T obj = SBJson.ToObject<T>(textValue);
                return obj;
            }
            catch (Exception e_)
            {
                return default(T);
            }
        }
    }
}

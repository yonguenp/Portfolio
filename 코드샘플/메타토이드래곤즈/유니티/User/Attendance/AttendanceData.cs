using Newtonsoft.Json.Linq;
using System;

namespace SandboxNetwork
{
    public class AttendanceData
    {
        public int AttendanceTag { get; set; } = 0;
        public int AttendanceDay { get; set; } = 0;
        public bool IsAttendance { get; set; } = false;
        public DateTime LastDate { get; set; } = default;
        public int[] AttendanceList { get; set; } = null;
        public void SetData(JArray jsonData)
        {
            if (null == jsonData || jsonData.Count != 4)
                return;

            AttendanceTag = jsonData[0].Value<int>();
            if (SBFunc.IsJArray(jsonData[1]))
            {
                AttendanceList = jsonData[1].ToObject<int[]>();
                AttendanceDay = AttendanceList.Length;
            }
            LastDate = DateTime.MinValue;

            long date = jsonData[2].Value<long>();
            if(date > 0)
                LastDate = TimeManager.GetCustomDateTime(date);
            IsAttendance = jsonData[3].Value<bool>();
        }

        public void Clear()
        {
            AttendanceTag = 0;
            AttendanceDay = 0;
            IsAttendance = false;
        }
        public void CheckAttendance()
        {
            IsAttendance = false;
        }
        /// <returns>true -> 네트워크 갱신 필요</returns>
        public bool IsNeedUpdate()
        {
            if (LastDate < TimeManager.GetDailyStartTime())
                return true;

            return false;
        }
    }
}
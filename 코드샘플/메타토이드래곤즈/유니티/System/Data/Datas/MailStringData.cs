using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class MailStringData : TableData<DBMail_string>
    {
        static private StringDataManager table = null;
        static public string GetStringByIndex(int key)
        {
            if (table == null)
                table = StringDataManager.Instance;

            return table.GetMailStringByIndex(key.ToString());
        }

        public string KEY => UNIQUE_KEY;
        public string TEXT => SBFunc.Replace(Data.TEXT);
    }

    //메일 아이콘에 빨콩 띄워주기위한 그이상 그이하도 아님
    public class ReservMailData : TableData<DBReserv_mail>
    {
        public int KEY { get { return Int(UNIQUE_KEY); } }
        public DateTime send_start { get; private set; } = DateTime.MaxValue;
        private DateTime send_end = DateTime.MinValue;
        public bool is_activated { get; private set; } = false;

        public bool CheckActivate(DateTime cur)
        {
            if (!is_activated)
                return false;

            if(send_start > cur && send_end < cur)
            {
                is_activated = false;
                return true;
            }

            return false;
        }

        public override void SetData(DBReserv_mail _data)
        {
            base.SetData(_data);

            if (!string.IsNullOrEmpty(Data.SEND_START) && DateTime.TryParse(Data.SEND_START, out DateTime start))
                send_start = start;
            else
                send_start = DateTime.MaxValue;

            if (!string.IsNullOrEmpty(Data.SEND_END) && DateTime.TryParse(Data.SEND_END, out DateTime end))
                send_end = end;
            else
                send_end = DateTime.MinValue;

            is_activated = Data.IS_ACTIVATED > 0;

            if (is_activated)
            {
                DateTime cur = TimeManager.GetDateTime();
                is_activated = cur > send_start && cur < send_end;
            }
        }
    }
}
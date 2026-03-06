using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork {
    public class AttendancePopup : Popup<PopupData>
    {
        #region OpenPopup
        public static AttendancePopup OpenPopup(System.Action action = null)
        {
            var popup = PopupManager.OpenPopup<AttendancePopup>();
            if (popup != null)
                popup.SetExitCallback(action);

            return popup;
        }
        public static bool CheckAttendance(System.Action action = null)
        {
            bool check = User.Instance.Attendance.IsNeedUpdate();
            if (check)
            {
                NetworkManager.Send("attendance/check", null, (JObject jsonData) =>
                {
                    if (SBFunc.IsJTokenCheck(jsonData) && (int)eApiResCode.OK == jsonData["rs"].Value<int>())
                    {
                        if (SBFunc.IsJArray(jsonData["attendance"]))
                            User.Instance.Attendance.SetData((JArray)jsonData["attendance"]);

                        OpenPopup(action);
                    }
                });
            }
            return check;
        }
        #endregion

        [Header("Title")]
        [SerializeField]
        protected Text titleText = null;
        [Space(10f)]
        [Header("AttendanceObject")]
        [SerializeField]
        protected List<AttendanceObject> clones = null;

        [SerializeField]
        protected Sprite[] boxSprites = null;
        /// <summary>
        /// true -> 획득 연출 진행
        /// false -> 획득 연출 미진행
        /// </summary>
        protected bool IsAttendance { get; set; } = false;
        #region Initialize


        protected override IEnumerator OpenAnimation()
        {
            yield return base.OpenAnimation();

            if (clones is null)
                yield break;

            var day = User.Instance.Attendance.AttendanceDay - 1;
            if (clones.Count <= day || day < 0)
                yield break;

            if (clones[day] is null)
                yield break;

            clones[day].StartAnim();
        }

        protected virtual IEnumerator OpenPopupAnim()
        {
            return base.OpenAnimation();
        }

        public override void InitUI()
        {
            SetAttendenceFlag();
            InitializeTitle();
            InitializeClone();

            UICanvas.Instance.StartBackgroundBlurEffect();
        }

        protected virtual void SetAttendenceFlag()
        {
            IsAttendance = User.Instance.Attendance.IsAttendance;
            User.Instance.Attendance.CheckAttendance();
        }

        protected virtual void InitializeTitle()
        {
            if (titleText is not null)
            {
                titleText.text = StringData.GetStringFormatByIndex(100002505, User.Instance.Attendance.LastDate.Month);
            }

        }
        protected virtual void InitializeClone()
        {
            var group = DailyRewardData.GetGroup(User.Instance.Attendance.LastDate.Month);
            if (group is null)
                group = DailyRewardData.GetGroup(SBDefine.DEFAULT_ATTENDANCE_REWARD_GROUP);

            if (group is not null)
            {
                for(int i = 0, count = group.Count; i < count; ++i)
                {
                    var data = group[i];
                    if (data is null)
                        continue;

                    if (clones.Count <= i)
                        break;
                    if (data.RARITY == eDailyRewardRarity.NONE)
                        continue;
                    clones[i].SetData(data, boxSprites[(int)data.RARITY -1] ,IsAttendance);
                }
            }
        }
        #endregion

        public override void OnClickDimd()
        {
            if (IsFullAnimCompleted())
                base.OnClickDimd();
        }
        public override void ClosePopup()
        {
            if (clones == null || clones.Count <= 0)
            {
                base.ClosePopup();
                UICanvas.Instance.EndBackgroundBlurEffect();
            }

            if (IsFullAnimCompleted())
            {
                base.ClosePopup();
                UICanvas.Instance.EndBackgroundBlurEffect();
            }
        }

        bool IsFullAnimCompleted()
        {
            var checkCount = 0;
            foreach (var isAnim in clones)
            {
                if (isAnim.AnimPlaying())
                    checkCount++;
            }

            return checkCount == 0;
        }
    }
}
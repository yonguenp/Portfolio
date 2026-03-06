using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork {
    public class GuildOperatorPermissionPopup : Popup<PopupData>
    {
        [SerializeField]
        Button JoinAllowManageOnBtn;
        [SerializeField]
        Button JoinAllowManageOffBtn;

        [SerializeField]
        Button AppointManageOnBtn;
        [SerializeField]
        Button AppointManageOffBtn;

        [SerializeField]
        Button ExileManageOnBtn;
        [SerializeField]
        Button ExileManageOffBtn;

        [SerializeField]
        Button JoinTypeManageOnBtn;
        [SerializeField]
        Button JoinTypeManageOffBtn;

        [SerializeField]
        Button WithdrawManageOnBtn;
        [SerializeField]
        Button WithdrawManageOffBtn;

        bool joinPermissionState = false;
        bool appointPermissionState = false;
        bool exilePermissionState = false;
        bool joinTypePermissionState = false;
        bool withdrawTypePermissionState = false;
        public override void InitUI()
        {
            var authority = GuildManager.Instance.MyGuildInfo.OperatorAuthority;
            SetJoinAllowState(authority.IsManageJoin);
            SetAppointState(authority.IsAppointAble);
            SetExileState(authority.IsFireNormalUser);
            SetJoinTypeState(authority.IsChangeJoinType);
        }

        public void SetJoinAllowState(bool state)
        {
            JoinAllowManageOnBtn.interactable = !state;
            JoinAllowManageOnBtn.GetComponentInChildren<Text>().color = state ? Color.white : Color.gray;

            JoinAllowManageOffBtn.interactable = state;
            JoinAllowManageOffBtn.GetComponentInChildren<Text>().color = state ? Color.gray : Color.white;
            joinPermissionState = state;
        }

        public void SetAppointState(bool state)
        {
            AppointManageOnBtn.interactable = !state;
            AppointManageOnBtn.GetComponentInChildren<Text>().color = state ? Color.white : Color.gray;

            AppointManageOffBtn.interactable = state;
            AppointManageOffBtn.GetComponentInChildren<Text>().color = state ? Color.gray : Color.white;
            appointPermissionState = state;
        }

        public void SetExileState(bool state)
        {
            ExileManageOnBtn.interactable = !state;
            ExileManageOnBtn.GetComponentInChildren<Text>().color = state ? Color.white : Color.gray;

            ExileManageOffBtn.interactable = state;
            ExileManageOffBtn.GetComponentInChildren<Text>().color = state ? Color.gray : Color.white;
            exilePermissionState = state;
        }

        public void SetJoinTypeState(bool state)
        {
            JoinTypeManageOnBtn.interactable = !state;
            JoinTypeManageOnBtn.GetComponentInChildren<Text>().color = state ? Color.white : Color.gray;

            JoinTypeManageOffBtn.interactable = state;
            JoinTypeManageOffBtn.GetComponentInChildren<Text>().color = state ? Color.gray : Color.white;
            joinTypePermissionState = state;
        }

        public void SetWithdrawTypeState(bool state)
        {
            WithdrawManageOnBtn.interactable = !state;
            WithdrawManageOnBtn.GetComponentInChildren<Text>().color = state ? Color.white : Color.gray;

            WithdrawManageOffBtn.interactable = state;
            WithdrawManageOffBtn.GetComponentInChildren<Text>().color = state ? Color.gray : Color.white;
            withdrawTypePermissionState = state;
        }

        public override void ClosePopup()
        {
            var beforeAuthority = GuildManager.Instance.MyGuildInfo.OperatorAuthority;
            if(joinPermissionState == beforeAuthority.IsManageJoin && appointPermissionState == beforeAuthority.IsAppointAble &&
              exilePermissionState == beforeAuthority.IsFireNormalUser && joinTypePermissionState == beforeAuthority.IsChangeJoinType &&
              withdrawTypePermissionState == beforeAuthority.IsWalletWithdraw)
            {
                base.ClosePopup();
                return;
            }
            WWWForm form = new WWWForm();
            form.AddField("gno", GuildManager.Instance.GuildID);
            int bit = 0;
            bit |= joinPermissionState ? 1 : 0;
            bit |= appointPermissionState ? 1 << 1 : 0;
            bit |= exilePermissionState ? 1 << 2 : 0;
            bit |= joinTypePermissionState ? 1 << 3 : 0;
            bit |= withdrawTypePermissionState ? 1 << 4 : 0;
            form.AddField("manager_auth", bit);
            GuildManager.Instance.NetworkSend("guild/changemanagerauth", form, () =>
            {
                base.ClosePopup();
            });
        }
    }
}
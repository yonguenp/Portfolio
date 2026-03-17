var Account = require("AccountManager");
var MessagePopup = require("MessagePopup");
var CHAT = require('ChatManager');
var RemoteSprite = require('RemoteSprite');

cc.Class({
    extends: cc.Component,

    properties: {
        user_name : cc.Label,
        login_date : cc.Label,
        notification : cc.Node,
        ProfileImage : RemoteSprite,
        account_no : -1,
        background : cc.Node,
    },

    onLoad : function(){

    },

    setUserInfo : function(data, func = null)
    {           
        require('DateUtils');
        this.account_no = data.AccountNo;
        this.user_name.string = data.UserNick;
        if(data.ProfileUrl)
        {
            this.ProfileImage.setRemoteSprite(data.ProfileUrl, null, false);
            this.ProfileImage.node.color = cc.Color.WHITE;
        }
        var date = new Date(data.LoginTime * 1000);        
        this.login_date.string = date.yyyymmdd_kor() + " " + date.apm_hhmmss();

        if(Account.GetAccountNo() == data.AccountNo)
        {
            this.user_name.node.color = new cc.Color(5, 160, 255);
            this.node.setSiblingIndex(0);
        }

        this.selectfunc = func;
    },

    getUserName : function()
    {
        return this.user_name.string;
    },

    onProfileSelect : function()
    {   
        if(this.selectfunc)
        {
            this.selectfunc(this);
            return;
        }

        if(this.account_no == Account.GetAccountNo())
        {
            MessagePopup.openMessageBoxWithKey("POPUP_86");
            return;
        }
        
        if(this.account_no <= 0)
        {
            MessagePopup.openMessageBoxWithKey("POPUP_87");
            return;
        }

        CHAT.onInvateChat(this.account_no);
    },

    onNotification : function(active)
    {
        if(this.notification)
            this.notification.active = active;
    },
});

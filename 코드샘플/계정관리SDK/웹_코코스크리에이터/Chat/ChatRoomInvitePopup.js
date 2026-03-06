var ChatPage = require('ChatPage');
var CHAT = require('ChatManager');
var Account = require("AccountManager");

cc.Class({
    extends: cc.Component,

    properties: {
        parentChatPage : ChatPage,
        ProfileContainer : cc.Node,
        profile_prefab : cc.Prefab,

        inviteListUI : cc.Node,
        inviteListContainer : cc.Node,
        inviteListItem : cc.Prefab,

        profile_filter : cc.EditBox,
    },

    // LIFE-CYCLE CALLBACKS:

    onEnable() {
        this.ProfileContainer.removeAllChildren();
        this.inviteListContainer.removeAllChildren();

        this.inviteListUI.active = false;

        var room = CHAT.room_arr.get(this.parentChatPage.thisRoomUID);
        var myAccount = Account.GetAccountNo();
        CHAT.member_arr.forEach(data =>
        {
            if(data.AccountNo != myAccount)
            {   
                var isRoomPlayer = false;
                if(room && room.RoomMembers)
                {
                    room.RoomMembers.forEach(mb => {
                    if(mb == data.AccountNo)
                    {
                        isRoomPlayer = true;
                    }
                    });
                }
                
                if(isRoomPlayer == false)
                {
                    var item = cc.instantiate(this.profile_prefab);
                    item.parent = this.ProfileContainer;
                    item.name = data.AccountNo.toString();
                    item.getComponent('ChatProfile').setUserInfo(data, this.onProfileSelected.bind(this));
                }
            }
        });
    },


    start () {

    },

    onFilterProfile()
    {
        var filterText = this.profile_filter.string;
        if(filterText)
        {
            this.ProfileContainer.children.forEach(item => {                
                item.active = 0 <= item.getComponent('ChatProfile').getUserName().indexOf(filterText);
            });
        }
        else
        {
            this.ProfileContainer.children.forEach(item => {
                item.active = true;
            });
        }
    },

    onProfileSelected : function(profileUI)
    {
        this.inviteListUI.active = true;

        if(profileUI.background.active)
        {
            this.onRemoveSelectedProfile(null, profileUI.user_name.string);
            return;
        }

        profileUI.background.active = true;
        var item = cc.instantiate(this.inviteListItem);            
        item.getComponent(cc.Label).string = profileUI.user_name.string;
        item.parent = this.inviteListContainer;

        let handler = new cc.Component.EventHandler();
            handler.target = this;
            handler.component = "ChatRoomInvitePopup";
            handler.handler = "onRemoveSelectedProfile";
            handler.customEventData = profileUI.user_name.string;

            item.getComponentInChildren(cc.Button).clickEvents.push(handler);
    },

    onRemoveSelectedProfile : function(event, del_nick)
    {
        var children = this.inviteListContainer.children;
        var delIndex = new Array();        
        for(var i = 0; i < children.length; i++)
        {
            var nick = children[i].getComponent(cc.Label).string;
            if(del_nick == nick)
            {
                this.ProfileContainer.children.forEach(node => {
                    var chatprofile = node.getComponent('ChatProfile');
                    if(chatprofile && chatprofile.user_name.string == del_nick)
                    {   
                        chatprofile.background.active = false;
                    }
                });
                
                delIndex.push(i);
            }
        }

        for(var i = 0; i < delIndex.length; i++)
        {
            children[i].removeFromParent();
        }
    },

    addProfile(data)
    {

    },

    removeProfile(data)
    {

    },

    onClose()
    {
        this.node.active = false;
    },

    onCloseInviteGroupSelect : function()
    {
        var children = this.inviteListContainer.children;
        var users = Array();
        for(var i = 0; i < children.length; i++)
        {
            var nick = children[i].getComponent(cc.Label).string;
            var user = CHAT.getMemberDataWithNickName(nick);                            
            if(user)
            {
                users.push(user.AccountNo);
            }
        }

        this.inviteListContainer.removeAllChildren();

        this.ProfileContainer.children.forEach(node => {
            node.getComponent('ChatProfile').background.active = false;
        });

        if(users.length > 0)
        {
            CHAT.onGroupInvateAdd(users, this.parentChatPage.thisRoomUID);
        }

        this.onClose();
    },    
});

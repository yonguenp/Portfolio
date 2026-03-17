var NetworkManager = require("NetworkManager");
var ChatUI = require('Chat');
var CHAT = require('ChatManager');
var RemoteSprite = require("RemoteSprite");
var Account = require("AccountManager");
var AppUtils = require('AppUtils');

cc.Class({
    extends: ChatUI,

    properties: {
        chatLobbyGroup : cc.Node,
        chatRoomGroup : cc.Node,

        lobbyToggle : [cc.Toggle],
        
        chatRoomListUI : cc.Node,
        chatPeopleListUI : cc.Node,  
        
        roomListContainer : cc.Node,
        roomListItemPrefab : cc.Prefab,

        openchatIcon : RemoteSprite,
        openChatLabel : cc.Label,
        openChatCount : cc.Label,

        myProfileSprite : RemoteSprite,
        myProfileNick : cc.Label,
        myLoginTime : cc.Label,

        chatLobbyProfileContainer : cc.Node,
        chatRoomProfileContainer : cc.Node,

        chatRoomFoldIcon : cc.Node,
        chatRoomUnfoldIcon : cc.Node,
        chatRoomProfileUI : cc.Node,

        inviteSelectBtn : cc.Node,
        inviteListView : cc.Node,
        inviteListContainer : cc.Node,
        inviteListItem : cc.Prefab,

        chatRoomInvitePopup : cc.Node,
        chatRoomInviteBtn : cc.Node,
        chatRoomDeleteBtn : cc.Node,
    },

    // LIFE-CYCLE CALLBACKS:

    onLoad () {
        //this.chatLobbyGroup.active = true;
        //this.chatRoomGroup.active = false;

        this.chatLobbyGroup.active = false;
        this.chatRoomGroup.active = true;

        //this.onLobbyToggle();

        var open_chat_name = AppUtils.GetOverrideVal('open_chat_name');
        if(open_chat_name)
            this.openChatLabel.string = open_chat_name;
        else
            this.openChatLabel.string = '오픈 채팅'; 

        var open_chat_icon = AppUtils.GetOverrideVal('open_chat_icon');
        if(open_chat_icon)
            this.openchatIcon.setRemoteSprite(open_chat_icon);

        this.onOpenChatRoom();
    },

    start () {

    },

    onEnable() {
        CHAT.ready(new Array(), CHAT.msg_arr == null ? new Array() : CHAT.msg_arr, new Array());
        //NetworkManager.tryWebSocketConnect();

        this.items.splice(0, this.items.length);
        this.profile_container.removeAllChildren();
        this.chat_container.removeAllChildren();
        this.chat_container.getComponent(cc.Layout).updateLayout(); 
        this.chat_container.height = 0; // updateLayout에서 height를 초기화 안해줘서 Scroll View Error 발생합니다.
                
        this.chat_last_day = 0;
        this.profile_CurPeopleCount.string = '0명';
        this.profile_container.active = false;
        this.chat_container.active = false;
   
        this.needScrollToBottom = false;
        this.chat_container_height = -2;
        CHAT.setTarget(this); 

        this.needRefresh = true;

        this.openChatCount.string = (CHAT.getMemberArray() ? CHAT.getMemberArray().length : 0) + '명';

        this.myProfileSprite.setRemoteSprite(Account.GetUserProfilePath(), null, false);
        this.myProfileNick.string = Account.GetNickName();
        this.myLoginTime.string = '';

        this.onCloseInviteGroupSelect();
        this.onCloseRoomInvitePopup();        
    },

    onDisable() {

    },

    addProfile(data)
    {
        if(this.chatLobbyGroup.active && data.AccountNo == Account.GetAccountNo())
        {
            this.openChatCount.string = (CHAT.getMemberArray() ? CHAT.getMemberArray().length : 0) + '명';            
            return;
        }

        var item = cc.instantiate(this.profile_prefab);
        item.parent = this.profile_container;
        item.name = data.AccountNo.toString();
        item.getComponent('ChatProfile').setUserInfo(data, this.onProfileSelected.bind(this));

        this.items.push(item);

        this.profile_CurPeopleCount.string = this.items.length + '명';        
        this.openChatCount.string = (CHAT.getMemberArray() ? CHAT.getMemberArray().length : 0) + '명';
    },

    removeProfile(data)
    {
        var strAccountNo = data.AccountNo.toString();
        var index = this.items.findIndex(function(item) {
            return item.name == strAccountNo;
        });

        if(index > -1)
        {
            this.items[index].destroy();
            this.items.splice(index, 1);
        }

        this.profile_CurPeopleCount.string = this.items.length + '명';
        this.openChatCount.string = (CHAT.getMemberArray() ? CHAT.getMemberArray().length : 0) + '명';
    },

    onLobbyToggle()
    {
        this.chatRoomListUI.active   = this.lobbyToggle[0].isChecked;
        this.chatPeopleListUI.active = this.lobbyToggle[1].isChecked;
    },

    onRoomBtn()
    {

    },

    onRoomInfo()
    {
        var checker = new Array();
        for(var i = 0; i < this.roomListContainer.children.length; i++)
        {
            checker.push(this.roomListContainer.children[i]);
        }

        CHAT.room_arr.forEach(room => {
            var isNew = true;
            var roomID = room.RoomID;

            this.roomListContainer.children.forEach(node => {
                if(roomID == node.name)
                {
                    node.getComponent('ChatRoomUI').setRoomInfoUI(room);
                    var idx = checker.findIndex(function(item) {return node == item; })
                    if(idx > -1)
                    {
                        checker.splice(idx, 1);
                    }
                    isNew = false;
                }
            });

            if(isNew)
            {
                var node = cc.instantiate(this.roomListItemPrefab);
                node.getComponent('ChatRoomUI').setRoomInfoUI(room);
                node.name = roomID;
                node.parent = this.roomListContainer;
                
                
                let handler = new cc.Component.EventHandler();
                handler.target = this;
                handler.component = "ChatPage";
                handler.handler = "onRoomEnterWithID";
                handler.customEventData = roomID;

                node.getComponent(cc.Button).clickEvents.push(handler);
            }

            if(this.thisRoomUID == roomID)
            {
                this.chat_container.removeAllChildren();
                this.chat_container.getComponent(cc.Layout).updateLayout(); 
                if(room.RoomChats)
                {                    
                    for(var i = 0; i < room.RoomChats.length; i++)
                    {                        
                        this.addMessage(room.RoomChats[i]);
                    }
                }
            }
        });

        checker.forEach(node => {
            node.removeFromParent();
        });
    },

    onRoomEnter : function(data)
    {
        this.chatLobbyGroup.active = false;
        this.chatRoomGroup.active = true;
        this.profile_container = this.chatRoomProfileContainer;
        this.onRoomProfilefold();

        CHAT.setTarget(null); 

        if(data == null)
        {
            this.thisRoomUID = -1;
        }
        else
        {
            this.thisRoomUID = data.RoomID == null ? 0 : data.RoomID;
            data.isNew = false;
        }

        CHAT.prevRoomID = this.thisRoomUID;

        this.chatRoomDeleteBtn.active = this.thisRoomUID != -1;

        this.onEnable();
    },

    onRoomEnterWithID : function(event, roomID)
    {
        var data = CHAT.room_arr.get(roomID);
        if(data)
        {
            if(data.isDirty)
            {
                CHAT.onEnter(roomID);
                data.isDirty = false;
            }
        }
        
        this.onRoomEnter(data);
    },

    onRoomDelete : function()
    {
        this.roomListContainer.children.forEach(node => {
            if(this.thisRoomUID == node.name)
            {
                node.removeFromParent();
            }
        });

        CHAT.onLeave(this.thisRoomUID);
        this.onRoomExit();
    },

    onRoomExit : function()
    {
        if(CHAT.room_arr)
        {
            var room = CHAT.room_arr.get(this.thisRoomUID);
            if(room)
            {
                room.isNew = false;
            }
        }

        this.chatLobbyGroup.active = true;
        this.chatRoomGroup.active = false;
        this.profile_container = this.chatLobbyProfileContainer;

        CHAT.setTarget(null); 

        this.thisRoomUID = -1;
        CHAT.prevRoomID = this.thisRoomUID;

        this.onEnable();
    },

    onOpenChatRoom : function()
    {
        this.onRoomEnter(null);
    },

    onShowInviteGroupSelect : function()
    {
        this.inviteSelectBtn.active = false;
        this.inviteListView.active = true;

        this.inviteListContainer.removeAllChildren();
    },

    onCloseInviteGroupSelect : function()
    {
        this.inviteSelectBtn.active = true;
        this.inviteListView.active = false;

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

        this.items.forEach(node => {
            node.getComponent('ChatProfile').background.active = false;
        });

        if(users.length == 1)
        {
            CHAT.onInvateChat(users[0]);
        }
        else if(users.length >= 2)
        {
            CHAT.onInvateGroupChat(users);
        }
    },    

    onProfileSelected : function(profileUI)
    {
        if(this.chatRoomGroup.active)
            return;

        if(this.inviteListView.active)
        {
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
                handler.component = "ChatPage";
                handler.handler = "onRemoveSelectedProfile";
                handler.customEventData = profileUI.user_name.string;

                item.getComponentInChildren(cc.Button).clickEvents.push(handler);
        }
        else
        {
            CHAT.onInvateChat(profileUI.account_no);
        }
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
                console.log('try on onRemoveSelectedProfile');
                console.log(this.items);
                this.items.forEach(node => {
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

    onRoomFoldBtn : function()
    {
        if(this.chatRoomFoldIcon.active)
            this.onRoomProfilefold();
        else
            this.onRoomProfileUnfold();
    },

    onRoomProfilefold : function()
    {
        this.chatRoomFoldIcon.active = false;
        this.chatRoomUnfoldIcon.active = true;
        this.chatRoomProfileUI.active = false;
    },

    onRoomProfileUnfold : function()
    {
        this.chatRoomFoldIcon.active = true;
        this.chatRoomUnfoldIcon.active = false;
        this.chatRoomProfileUI.active = true;

        console.log('onRoomProfileUnfold : ' + this.thisRoomUID);
        if(this.thisRoomUID != -1)
        {
            var room = CHAT.room_arr.get(this.thisRoomUID);
            this.chatRoomInviteBtn.active = room.RoomType == 2;            
        }
        else
        {
            this.chatRoomInviteBtn.active = false;
        }
    },

    onShowRoomInvitePopup : function()
    {
        this.chatRoomInvitePopup.active = true;
    },

    onCloseRoomInvitePopup : function()
    {
        this.chatRoomInvitePopup.active = false;
    },
});

var CHAT = require('ChatManager');
var ChatUI = require('Chat');
var AppUtils = require('AppUtils')
var NetworkManager = require("NetworkManager");
var Account = require("AccountManager");

cc.Class({
    extends: ChatUI,

    properties: {
        
    },

    onLoad : function() {
        console.log('load custom chat');

        CHAT.setTarget(this);   
        CHAT.ready(new Array(), new Array(), new Array());
        this.thisRoomUID = CHAT.prevRoomID;         

        if(this.thisRoomUID == -1)
            AppUtils.sendChatRoomType('openchat');
        else
        {
            var room = CHAT.room_arr.get(this.thisRoomUID);
            if(room)
            {
                switch(room.RoomType)
                {
                    case 1 : 
                        AppUtils.sendChatRoomType('1:1chat');
                        break;
                    case 2 :
                        AppUtils.sendChatRoomType('groupchat');
                        break;
                    default :
                        AppUtils.sendChatRoomType('error');
                        break;
                }
            }
            else
            {
                AppUtils.sendChatRoomType('error');
            }
        }

        NetworkManager.doPullChat(CHAT.tailChatSeq);
        this.timer = 0;
    },

    onEnable : function() {

    },

    onDisable : function(){
        CHAT.clearTarget(null);
    },

    addProfile(data)
    {
        AppUtils.sendChatProfile(data.UserNick, Samanda.getSamandaUrl() + "resources/" + data.ProfileUrl);
    },

    removeProfile(data)
    {
        
    },

    onDisconnect : function()
    {
        
    },

    addMessage(data)
    {
        AppUtils.sendChatMessage(data);
    },

    update : function(dt)
    {
        if (this.timer > this.pullingDuration)
        {
            console.log('doPullChat');
            NetworkManager.doPullChat(CHAT.tailChatSeq);
            this.timer = 0;
            //this.needRefresh = true;
        }
        else
            this.timer += dt;
    }
});

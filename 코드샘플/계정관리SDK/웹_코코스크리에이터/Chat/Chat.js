var CHAT = require('ChatManager');
var Account = require("AccountManager");
var WebSocket = require('WebsocketManager');

// WebService ver. 오픈채팅 기능 추가를 위해 추가됨.
var NetworkManager = require("NetworkManager");

var ChatUI = cc.Class({
    extends: cc.Component,

    properties: {
        msgEditBox : cc.EditBox,
        items: [cc.Node],
        
        profile_CurPeopleCount : cc.Label,
        profile_scroll_view : cc.ScrollView,
        profile_container: cc.Node,
        profile_prefab: cc.Prefab,
        profile_filter: cc.EditBox,

        chat_scroll_view : cc.ScrollView,
        chat_container : cc.Node,
        chat_date_Prefab : cc.Prefab,
        chat_user_Prefab : cc.Prefab,
        chat_me_Prefab : cc.Prefab,
        chat_last_day : 0,
        
        needRefresh : false,
        needScrollToBottom : false,
        chat_container_height : -2,

        thisRoomUID : -1,
        room_Exit : cc.Node,

        /**
         * Http Request Ver. OpenChat을 위해 추가됨
         * 확인을 위해 임시로 만들어 둔 것.
         * 추후 운용방식에 따라 변경할 것.
         */
        pullingDuration : 5, // 오픈채팅 pulling interval
        timer : 5,
    },
  
    getRoomID : function()
    {
        return this.thisRoomUID;
    },

    onRoomEnter : function(data)
    {
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

        this.onEnable();
    },

    onRoomExit : function()
    {
        this.thisRoomUID = -1;

        CHAT.prevRoomID = this.thisRoomUID;

        this.onEnable();
    },

    onEnable : function() {
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

        console.log('this Room id is ' + this.thisRoomUID);
        if(this.room_Exit)
        {            
            this.room_Exit.active = this.thisRoomUID != -1;
        }
    },
    
    onDisable : function(){
        CHAT.clearTarget(this);
    },

    onDisconnect : function()
    {
        this.profile_container.removeAllChildren();
        
        var item = cc.instantiate(this.chat_date_Prefab);        
        item.parent = this.chat_container;
        
        item.getChildByName('Text_day').getComponent(cc.Label).string = '연결 해제';
        this.chat_container_height += item.height + 2;
    },

    onFilterProfile()
    {
        var filterText = this.profile_filter.string;
        if(filterText)
        {
            this.items.forEach(item => {                
                item.active = 0 <= item.getComponent('ChatProfile').getUserName().indexOf(filterText);
            });
        }
        else
        {
            this.items.forEach(item => {
                item.active = true;
            });
        }
    },

    addProfile(data)
    {
        var item = cc.instantiate(this.profile_prefab);
        item.getComponent('ChatProfile').setUserInfo(data);
        
        item.parent = this.profile_container;
        item.name = data.AccountNo.toString();
        this.items.push(item);

        this.profile_CurPeopleCount.string = this.items.length + '명';
    },

    removeProfile(data)
    {
        var strAccountNo = data.AccountNo.toString();
        var index = this.items.findIndex(function(item) {
            return item.name == data.AccountNo.toString();
        });

        if(index > -1)
        {
            this.items[index].destroy();
            this.items.splice(index, 1);
        }

        this.profile_CurPeopleCount.string = this.items.length + '명';
    },

    addMessage(data)
    {
        if(null !== data.Message.match(/e960cdb67f2cb7488f16347705580180/))
        {
            return;
        }

        data = JSON.parse(JSON.stringify(data));

        if(null !== data.Sender.match(/e960cdb67f2cb7488f16347705580180/))
        {
            data.Sender = data.Sender.replace(/\[e960cdb67f2cb7488f16347705580180.*e960cdb67f2cb7488f16347705580180\]/,'');            
        }

        // invalid packet
        if (!(data.Message) && !(data.Image)) {
            console.error('invalid chat message: ' + data.toString());
            return;
        }
        
        /*if (this.chat_container.children.length > 300)
        {
            console.log('children count: ' + this.chat_container.children.length)
            this.chat_container.removeAllChildren();
            this.chat_container_height = 0;
        }*/

        if(this.needScrollToBottom == false && this.chat_scroll_view.getScrollOffset().y >= this.chat_scroll_view.getMaxScrollOffset().y)
            this.needScrollToBottom = true;

        var sysdate = new Date(data.SendTime * 1000); 
        if(this.chat_last_day != 0)
        {            
            var day = sysdate.getDay();
            if(day != this.chat_last_day)
            {
                var item = cc.instantiate(this.chat_date_Prefab);        
                this.chat_container_height += item.height + 2;

                item.parent = this.chat_container;                
                item.getChildByName('Text_day').getComponent(cc.Label).string = sysdate.yyyymmdd_kor();
            }
        }

        var item = cc.instantiate(data.Sender == Account.GetNickName() ? this.chat_me_Prefab : this.chat_user_Prefab);
        if (data.Message) {
            // test
            // if ('aaa' == data.Message) {
            //     var strB64 = 'data:image/png;base64,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'
            //     + '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';
            //     data.Image = 'asdf.png,' + strB64;
            //     item.getComponent('ChatMsg').setImageMsg(data);
            // } else {
            item.getComponent('ChatMsg').setMsgText(data);
            // }
        } else if (data.Image) {
            item.getComponent('ChatMsg').setImageMsg(data);
        }
        
        this.chat_container_height += item.height + 2;
        item.parent = this.chat_container;
        
        this.chat_last_day = sysdate.getDay();
    },

    ProfileNotification : function(sender)
    {
        this.items.forEach(item => {                
            var notiNode = item.getChildByName('notification');
            if(notiNode)
                notiNode.active = item.getComponent('ChatProfile').getUserName() == sender;
        });
    },

    update : function(dt)
    {
        if(this.needRefresh)
            this.refresh();

        if(this.needScrollToBottom)
        {                    
            this.chat_scroll_view.setContentPosition(cc.v2(0, ((this.chat_scroll_view.node.height / 2) - this.chat_container_height * -1)));            
            this.needScrollToBottom = false;
        }

        if (this.timer > this.pullingDuration)
        {
            console.log('doPullChat');
            NetworkManager.doPullChat(CHAT.tailChatSeq);
            this.timer = 0;
            //this.needRefresh = true;
        }
        else
            this.timer += dt;
    },

    refresh : function()
    {
        this.needRefresh = false;
        
        this.profile_container.active = true;
        this.chat_container.active = true;

        this.profile_scroll_view.scrollToTop();
        this.needScrollToBottom = true;
    },

    sendMsg : function()
    {
        if(!this.msgEditBox.string)
            return;

        if(this.msgEditBox.string.indexOf('/') == 0)
        {
            var line = this.msgEditBox.string.split('/');         
            this.msgEditBox.string = "";
            this.msgEditBox.textLabel.string = "";     
            this.msgEditBox.blur();          

            if(line.length == 2)
            {   
                var param = line[1].split(' ');
                var command = param[0];
                
                if(command == '초대')
                {                    
                    if(param.length == 2)
                    {
                        var user = CHAT.getMemberDataWithNickName(param[1]);
                        if(user)
                        {
                            CHAT.onInvateChat(user.AccountNo);                            
                        }
                    }
                    else if(param.length >= 2)
                    {
                        var users = Array();
                        for(var i = 1; i < param.length; i++)
                        {
                            var user = CHAT.getMemberDataWithNickName(param[i]);                            
                            if(user)
                            {
                                users.push(user.AccountNo);
                            }
                        }

                        if(users.length >= 2)
                            CHAT.onInvateGroupChat(users);
                    }
                }

                if(command == '추가' && this.thisRoomUID != -1)
                {                    
                    if(param.length >= 2)
                    {
                        var users = Array();
                        for(var i = 1; i < param.length; i++)
                        {
                            var user = CHAT.getMemberDataWithNickName(param[i]);
                            if(user)
                            {
                                users.push(user.AccountNo);
                            }
                        }
                        
                        CHAT.onGroupInvateAdd(users, this.thisRoomUID);
                    }
                }

                if(command == '탈출')
                {                    
                    CHAT.onLeave(this.thisRoomUID);
                }

                
                return;
            }
        }

        this.needScrollToBottom = true;
        
        // WebService ver. 오픈채팅을 위한 코드.
        var channel = 'say';
        if(CHAT.targetUI && CHAT.targetUI.getRoomID() != -1)
            channel = CHAT.targetUI.getRoomID();

        // OpenChat이 아닌 경우는 기존 처리와 동일하게 WebSocket으로 처리함.
        if (channel != 'say')
            WebSocket.doChat(this.msgEditBox.string);
        else
        {
            // OpenChat인 경우 HttpRequest로 처리
            NetworkManager.doSendChat(this.msgEditBox.string, CHAT.tailChatSeq);
        }

        this.msgEditBox.blur();
        this.msgEditBox.string = "";
        this.msgEditBox.textLabel.string = "";
    },

    onRoomInfo()
    {
        
    }
});
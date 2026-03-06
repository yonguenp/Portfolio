var ChatRoom = require('ChatRoom');
var AccountManager = require('AccountManager');

module.exports = {
    isStarted : false,
    
    member_arr : null,
    msg_arr : null,
    targetUI : null,
    room_arr : null,
    
    prevRoomID : -1,

    /**
     * WebService Ver. 오프채팅을 위해 추가됨
     * 확인을 위해 임시로 만들어 둔 것.
     * 추후 운용방식에 따라 변경할 것.
     */
    headChatSeq : 0,
    tailChatSeq : 0,

    setTarget : function(target)
    {
        this.targetUI = target;
        
        this.syncUI();
    },

    clearTarget : function(target)
    {
        if(this.targetUI == target)
            this.targetUI = null;
    },

    ready : function(members, chats, rooms)
    {   
        this.member_arr = new Array();     
        this.msg_arr = new Array();
        this.room_arr = new Map();        
        this.isStarted = true;
        this.tailChatSeq = 0;
        
        members.forEach(element => {
            this.member_arr.push(element);    
        });
        
        
        for(var i = 0; i < chats.length; i++)
        {
            if(chats.length - 30 <= i)
            {
                this.msg_arr.push(chats[i]);        
            }
        }

        for(var i = 0; i < rooms.length; i++)
        {
            var { Type,
                ResultCode,
                RoomUid,
                RoomType,
                MemberList,
                LastChatMessage } = rooms[i];

            var ChatList = new Array();
            if(LastChatMessage)
            {            
                ChatList.push(LastChatMessage);
            }
            
            this.onCreateRoom(RoomUid, MemberList, ChatList, RoomType);
            var roomData = this.room_arr.get(RoomUid);
            if(roomData)
                roomData.isDirty = true;
        }

        this.syncUI();
    },

    done : function()
    {
        this.member_arr = null;     
        this.msg_arr = null;
        
        if(this.room_arr)
            this.room_arr.clear();
        this.room_arr = null;
        
        this.isStarted = false;
        this.syncUI();
    },

    syncUI : function()
    {
        if(this.targetUI)
        {
            if(this.isStarted)
            {
                if(this.targetUI.getRoomID() < 0)
                {
                    if(this.member_arr)
                    {
                        for(var i = this.member_arr.length - 1; i >= 0; i--)
                        {
                            this.targetUI.addProfile(this.member_arr[i]);
                        }
                    }
                    if(this.msg_arr)
                    {
                        for(var i = 0; i < this.msg_arr.length; i++)
                        {
                            this.targetUI.addMessage(this.msg_arr[i]);
                        }
                    }
                }
                else
                {
                    var room = this.room_arr.get(this.targetUI.getRoomID());
                    if(room)
                    {
                        if(room.RoomMembers)
                        {
                            for(var i = room.RoomMembers.length - 1; i >= 0; i--)
                            {
                                this.targetUI.addProfile(room.RoomMembers[i]);
                            }
                        }
                        if(room.RoomChats)
                        {
                            for(var i = 0; i < room.RoomChats.length; i++)
                            {
                                this.targetUI.addMessage(room.RoomChats[i]);
                            }
                        }
                    }
                }
            }
            else
            {
                this.targetUI.onDisconnect();
            }
        }
    },

    onMsg : function(data)
    {
        if(this.msg_arr.length > 30)
            this.msg_arr.splice(0, 1);

        this.msg_arr.push(data);

        // WebService ver. 오픈채팅 기능 추가를 위해 추가됨.
        if (data.Sequence < this.headChatSeq)
        {
            console.log(`chage headChatSeq. ${this.headChatSeq} => ${data.Sequence}`);
            this.headChatSeq = data.Sequence;
        }
        
        if (data.Sequence > this.tailChatSeq)
        {
            console.log(`chage tailChatSeq. ${this.tailChatSeq} => ${data.Sequence}`);
            this.tailChatSeq = data.Sequence;
        }
        /////////////////////////////////////////////////////

        // if("Login.fire" == this.getSceneName() && this.targetUI)
        if (this.targetUI && this.targetUI.getRoomID() === -1)
        {
            this.targetUI.addMessage(data);
        }
    },

    onUserJoin : function(data)
    {
        if(this.member_arr)
        {
            var index = this.member_arr.findIndex(function(d) {
                return d.AccountNo == data.AccountNo;
            });
        
            if(index > -1)
            {
                console.log('try already joined user !');
                this.member_arr.splice(index, 1);
            }
        }
        
        this.member_arr.push(data); 

        if(this.targetUI)
        {
            if(this.targetUI.getRoomID() === -1)
            {
                this.targetUI.addProfile(data);
            }
        }    
    },

    onUserLeft : function(data)
    {
        if(this.member_arr)
        {
            var index = this.member_arr.findIndex(function(d) {
                return d.AccountNo == data.AccountNo;
            });

            if(index > -1)
            {
                this.member_arr.splice(index, 1);
            }
        }

        if(this.targetUI)
        {
            if(this.targetUI.getRoomID() === -1)
            {
                this.targetUI.removeProfile(data);
            }
        }    
    },

    getMemberData(account_no)
    {
        if(this.member_arr == null)
            return null;

        var index = this.member_arr.findIndex(function(d) {
            return d.AccountNo == account_no;
        });

        if(index > -1)
            return this.member_arr[index];
        else
            return null;
    },

    getMemberDataWithNickName(nick)//임시
    {
        if(this.member_arr == null)
            return null;
            
        var index = this.member_arr.findIndex(function(d) {
            return d.UserNick == nick;
        });

        if(index > -1)
            return this.member_arr[index];
        else
            return null;
    },

    getMsgArray : function()
    {
        return this.msg_arr;
    },

    getMemberArray : function()
    {
        return this.member_arr;
    },

    onRoomMsg : function(room_id, data)
    {
        if(this.room_arr)
        {
            var room = this.room_arr.get(room_id);
            if(room)
            {
                room.onChatMsg(data.Sender, data.SendTime, data.Message);    
            }

            if(this.targetUI)
            {
                if(this.targetUI.getRoomID() === -1)
                {                
                    this.targetUI.ProfileNotification(data.Sender);
                }
                else if (room_id === this.targetUI.getRoomID())
                {                
                    this.targetUI.addMessage(data);
                    room.checkNewFlag();
                }
            }            
        }
    },

    onCreateRoom : function(RoomUid, MemberList, ChatList, RoomType)
    {
        if(this.room_arr)
        {
            var roomData = this.room_arr.get(RoomUid);
            var roomUI = null;

            if(roomData)
            {
                roomUI = roomData.RoomUI;
                this.room_arr.delete(RoomUid);
            }

            roomData = new ChatRoom();
            if(roomUI)
            {
                console.log('onCreateRoom UI set' + RoomUid);
                roomData.roomUI = roomUI;
            }

            roomData.init(RoomUid, MemberList, ChatList, RoomType);
            this.room_arr.set(RoomUid, roomData);

            if(this.targetUI)
            {
                if(this.targetUI.getRoomID() == 0)
                    this.targetUI.onRoomEnter(roomData);
                if(this.targetUI.getRoomID() == RoomUid)
                    this.targetUI.onRoomEnter(roomData);

                this.targetUI.onRoomInfo();
            }
            else
            {
                console.log('this.targetUI is null or undefined');
            }
        }
        else
        {
            console.log('this.room_arr is null or undefined');
        }
    },

    onRoomAddMember : function(RoomUid, MemberList)
    {
        var room = this.room_arr.get(RoomUid);
        if(room)
        {
            room.addMember(MemberList);
        }

        if(this.targetUI && this.targetUI.getRoomID() == RoomUid)
        {
            MemberList.forEach(member =>{
                this.targetUI.addProfile(member);
            });            
        }
    },

    onRoomRemoveMember : function(RoomUid, MemberList)
    {
        var room = this.room_arr.get(RoomUid);
        if(room)
        {
            room.removeMember(MemberList);
        }

        if(this.targetUI && this.targetUI.getRoomID() == RoomUid)
        {
            MemberList.forEach(member =>{
                this.targetUI.removeProfile(member);
            });            
        }
    },

    onInvateChat : function(account_no)
    {
        if(account_no > 0)
        {
            require('WebsocketManager').sendInvate(account_no);
        }

        var roomID = -1;
        this.room_arr.forEach(function(value, key, map){
            if(value.RoomMembers.length == 2)
            {
                value.RoomMembers.forEach(mb =>{
                    if(mb == account_no)
                    {
                        roomID = key;
                        return;
                    }
                });
            }

            if(roomID > 0)
                return;
        });

        if(this.targetUI)
        {
            var data = null;
            if(roomID > 0)
            {
                data = this.room_arr.get(roomID)
            }

            if(data == null)
            {
                data = new ChatRoom();
            }

            this.targetUI.onRoomEnter(data);
        }
    },

    onInvateGroupChat : function(accounts)
    {
        if(accounts.length <= 0)
            return;

        var roomID = -1;
        var myAccountNo = AccountManager.GetAccountNo();
        this.room_arr.forEach(function(value, key, map){
            var samePeople = true;
            accounts.forEach(account_no =>{
                if(account_no != myAccountNo)
                {
                    value.RoomMembers.forEach(mb =>{
                        if(mb != account_no)
                        {
                            samePeople = false;
                            return;
                        }
                    });
                }

                if(samePeople == false)
                    return;
            });

            if(samePeople)
            {
                roomID = key;            
                return;
            }
        });

        if(this.targetUI)
        {
            var data = null;
            if(roomID > 0)
            {
                data = this.room_arr.get(roomID)
            }
            
            if(data == null)
            {
                data = new ChatRoom();
            }

            this.targetUI.onRoomEnter(data);
        }

        require('WebsocketManager').sendGroupInvate(accounts);
    },

    onGroupInvateAdd(accounts, roomID)
    {
        if(accounts.length <= 0)
            return;

        if(this.targetUI)
        {
            var data = null;
            if(roomID > 0)
            {
                data = this.room_arr.get(roomID)
            }
            
            if(data == null)
            {
                data = new ChatRoom();
            }

            this.targetUI.onRoomEnter(data);
        }

        require('WebsocketManager').sendGroupAdd(accounts, roomID);
    },

    onEnter(roomID)
    {
        require('WebsocketManager').sendEnter(roomID);
    },

    onLeave(roomID)
    {
        var data = this.room_arr.get(roomID);
        if(data)
        {
            if(this.room_arr.delete(roomID))
            {
                require('WebsocketManager').sendLeave(roomID);
            }
        }
    },

    b64toBlob : function(b64Data, contentType, sliceSize)
    {
        contentType = contentType || 'image/png';
        sliceSize = sliceSize || 512;

        var byteCharacters = atob(b64Data);
        var byteArrays = [];

        for (var offset = 0; offset < byteCharacters.length; offset += sliceSize) {
            var slice = byteCharacters.slice(offset, offset + sliceSize);

            var byteNumbers = new Array(slice.length);
            for (var i = 0; i < slice.length; i++) {
                byteNumbers[i] = slice.charCodeAt(i);
            }

            var byteArray = new Uint8Array(byteNumbers);

            byteArrays.push(byteArray);
        }

        var blob = new Blob(byteArrays, {type: contentType});
        return blob;
    },

    uploadMainImage : function(b64Data, callback = null, failcallback = null)
    {
        var NetworkManager = require('NetworkManager');
        
        var formData = new FormData();        
        formData.append('chatimg', this.b64toBlob(b64Data, 'image/png', 512));
        formData.append('pid', NetworkManager.product_type);
        AccountManager.AppendFormData(formData);

        // TODO : 파일 전송시 효과 UI 사용한다면 onError에서도 제거해 주어야 함
        NetworkManager.SendRequestPost(
            'chat/upload_img',
            formData,
            callback,
            failcallback
        );
    },

    /**
     * WebService ver. 오픈채팅 기능 추가를 위해 추가됨.
     * @param {*} chats 채팅 오브젝트 리스트
     */
    setChatList(chats)
    {
        if(this.targetUI.chat_container)
        {
            if (this.targetUI.chat_container.children.length > 300)
            {
                console.log('children count: ' + this.targetUI.chat_container.children.length)
                this.targetUI.chat_container.removeAllChildren();
                this.targetUI.chat_container_height = 0;
            }
        }
        
        for(var i = 0; i < chats.length; ++i)
        {
            this.onMsg(chats[i]);        
        }
    }
}
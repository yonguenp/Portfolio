/**
 * Websocket handler
 */

var Samanda = require('Samanda');
var Toast = require('ToastScript');
var CHAT = require('ChatManager');

module.exports = {

// properties
uri : 'wss://sandbox-gs.mynetgear.com/websocket',
socket : null,
retry : 0,
reconnecting : false,
loggedIn : false,
// very bad
pid : -1,
ano : 0,
nick : '',
profileURL : '',
/**
 * handle connections
 */
connect : function(dest, pid, ano, nick, profileURL) {
    if(this.isConnected() || this.socket != null)
    {
        if(this.loggedIn && this.ano == ano)
        {
            console.log('already Socket');
            return;
        }
        else
        {
            console.log('cur socket disconnect and try new Socket');
            this.disconnect();
        }        
    }

    this.uri = (dest) ? dest : this.uri;
    this.pid = pid;
    this.ano = ano;
    this.nick = nick;
    this.profileURL = profileURL;

    console.log('try connect socket uri : ' + this.uri);

    try {
        this.socket = new WebSocket(this.uri);
    } catch (e) {
        console.log('failed to connect: ' + e.toString());
    }

    // callbacks
    this.socket.onopen      = this.onWsOpen.bind(this);
    this.socket.onclose     = this.onWsClose.bind(this);
    this.socket.onmessage   = this.onWsMessage.bind(this);
    this.socket.onerror     = this.onWsError.bind(this);
},

isConnected : function() {
    if (!this.socket) {
        return false;
    } else {
        return WebSocket.OPEN === this.socket.readyState;
    }
},

disconnect : function(code = 1000) {
    if (this.socket) 
    {
        if(WebSocket.CLOSED !== this.socket.readyState)
            this.socket.close(code);
        this.socket = null;
    }
},

tryReconnect : function() {
    if (0 >= this.retry || this.isConnected() || false === this.reconnecting) {
        this.reconnecting = false;
        this.retry = 0;
        return;
    }

    --this.retry;
    this.connect(this.uri, this.pid, this.ano, this.nick, this.profileURL);
},

/**
 * callbacks
 */

onWsOpen : function(event) {
    console.log('onOpen');
    this.retry = 0;

    this.doLogin();
},

onWsClose : function(event) {
    console.log('onWsClose : ' + event.code);

    this.socket = null;
    this.loggedIn = false;

    CHAT.done();

    if (1000 === event.code) {
        this.reconnecting = false;
        this.retry = 0;
    } else {
        if (false === this.reconnecting) {
            this.reconnecting = true;
            this.retry = 3;
        }

        window.setTimeout(this.tryReconnect.bind(this), 6000 - this.retry * 2000);
    }
},

onWsMessage : function(event) {
    try {
        
        var data = JSON.parse(event.data);
        
        console.log('onWsMessage');
        console.log(data);

        switch (data.Type) {
            case 'connect':
                break;
            case 'login':
                this.onMsgLogin(data);
                break;
            case 'join':
                CHAT.onUserJoin(data.UserInfo);
                break;
            case 'disconnect':
                CHAT.onUserLeft(data.UserInfo);
                break;
            case 'chat':
                this.onMsgChat(data);
                break;
            case 'chatroominfo':
                this.onMsgChatRoomInfo(data);
                break;
            case 'groupchat':
                this.onGroupChatInfo(data);
                break;
            default:
                console.log('unswitched msg: ' + event.data);
                break;
        }
    } catch (e) {
        console.log('error onMessage: ' + event.data);
        console.log(e.toString());
    }
},

onWsError : function(event) {
    console.log('onWsError: ' + event.data);
},

/**
 * Message handlers
 */
onMsgLogin : function(data) {
    console.log('onMsgLogin');
    console.log(data);
    if (data.IsSuccess) {
        this.loggedIn = true;
        CHAT.ready(data.MemberList, data.ChatList, data.ChatRoomList);
    }
},

onMsgChat : function(data) {
    switch(data.Channel) {
        case 'say':            
            CHAT.onMsg(data.Content);
            break;

        case 'system':
            //console.log('system msg: ' + data.Message);
            Toast.show(data.Message);
            break;

        case 'whisper':
            break;

        default:
            console.log('onMsgChat default');
            CHAT.onRoomMsg(data.Channel, data.Content);
            break;
    }
},

onMsgChatRoomInfo : function(data)
{
    var { Type,
        ResultCode,
        RoomUid,
        RoomType,
        MemberList,
        ChatList } = data;
    
    CHAT.onCreateRoom(RoomUid, MemberList, ChatList, RoomType);
},

onGroupChatInfo : function(data)
{
    var { Command,
        RoomUid,
        MemberList } = data;

    switch(Command)
    {
        case 'add':   
            CHAT.onRoomAddMember(RoomUid, MemberList);
            break;
        case 'remove':   
            CHAT.onRoomRemoveMember(RoomUid, MemberList);
            break;
        default:
            console.log('onGroupChatInfo unswitched Command :' + Command);
            break;
    }
},

doSend : function(data) {
    if (false === this.isConnected()) {
        console.log('doSend: socket not connected');
        return;
    }

    try {
        var msg = JSON.stringify(data);
        this.socket.send(msg);
    } catch (e) {
        console.log('doSend: ' + e.toString());
        this.socket.send(data);
    }
},

/**
 * operations
 */
doLogin : function() {
    console.log('doLogin');
    this.doSend({Type:'login',UserInfo:{UserId:'',UserNick:this.nick,PId:this.pid,AccountNo:this.ano,ProfileUrl:this.profileURL}});
},

doChat : function(msg) {
    var channel = 'say';
    if(CHAT.targetUI && CHAT.targetUI.getRoomID() != -1)
        channel = CHAT.targetUI.getRoomID();

    this.doSend({Type:'chat',Channel:channel,Content:{SenderAccountNo:this.ano,Sender:this.nick,Message:msg}});
},

sendImage : function(path, strBase64) {
    this.doSend({
        Type:'chat',
        Channel:'say',
        Content:{
            Sender:this.nick,
            Image:path + ',' + strBase64
        }
    });
},

sendInvate : function(account_no) {
    var user = CHAT.getMemberData(account_no);
    var pid = this.pid;
    if(user)
    {
        pid = user.PId;
    }

    this.doSend({
        Type:'invite',
        RoomType:1,
        MemberList:[{UserId:'',UserNick:'',PId:pid, AccountNo:account_no}],
    });
},

sendGroupInvate : function(accounts) {
    var members = new Array();
    
    accounts.forEach(account_no => {
        var user = CHAT.getMemberData(account_no);
        var pid = this.pid;
        if(user)
        {
            pid = user.PId;
        }
        members.push({UserId:'',UserNick:'',PId:pid,AccountNo:account_no});
    });

    if(members.length == accounts.length)
    {
        this.doSend({
            Type:'invite',
            RoomType:2,
            MemberList:members,
        });
    }
},

sendGroupAdd : function(accounts, roomID) {
    var members = new Array();
    
    accounts.forEach(account_no => {
        var user = CHAT.getMemberData(account_no);
        var pid = this.pid;
        if(user)
        {
            pid = user.PId;
        }
        members.push({UserId:'',UserNick:'',PId:pid,AccountNo:account_no});
    });

    if(members.length == accounts.length)
    {
        this.doSend({
            Type:'groupchat',
            Command:'add',
            RoomUid:roomID,
            MemberList:members,
        });
    }
},

sendEnter : function(roomID) {
    this.doSend({
        Type:'chatcmd',
        Command:'enter',
        RoomUid:roomID,
    });
},

sendLeave : function(roomID) {
    this.doSend({
        Type:'chatcmd',
        Command:'leave',
        RoomUid:roomID,
    });
},
/**
 * utilities
 */
uuidv4 : function() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'
            .replace(/[xy]/g, function(c) {
                var r = Math.random() * 16 | 0;
                var v = 'x' ? r : (r & 0x3 | 0x8);
                return v.toString(16);
            });
},

// End of Object

}// module.exports = {

// EOF

const MemberData = class
{
    AccountNo = '';
    UserNick = '';
    LoginTime = '';
    ProfileUrl = '';

    constructor() {  
        this.AccountNo = '';
        this.UserNick = '';
        this.LoginTime = '';     
        this.ProfileUrl = '';
    }
};

const ChatData = class
{
    Message = '';
    SendTime = '';
    Sender = '';

    constructor() {  
        this.Message = '';
        this.SendTime = '';
        this.Sender = '';    
    }
};

const ChatRoom = class
{
    RoomType = 0;
    RoomID = null;
    RoomMembers = null;
    RoomChats = null;    
    RoomUI = null;
    isNew = false;
    isDirty = false;

    constructor() {  
        this.RoomType = 0;
        this.RoomID = null;
        this.RoomMembers = null;
        this.RoomChats = null;    
        this.RoomUI = null;
        this.isNew = false;
        this.isDirty = false;
    }

    init(RoomUid, MemberList, ChatList, RoomType)
    {
        //console.log('ChatRoom init');
        // console.log(RoomUid);
        // console.log(MemberList);
        // console.log(ChatList);

        this.RoomID = RoomUid;
        this.RoomMembers = new Array();
        MemberList.forEach(mb => {
            var md = new MemberData();
            md.AccountNo = mb.AccountNo;
            md.UserNick = mb.UserNick;
            md.LoginTime = mb.LoginTime;
            md.ProfileUrl = mb.ProfileUrl;
            this.RoomMembers.push(md);
        });

        this.RoomChats = new Array();
        if(ChatList)
        {
            ChatList.forEach(cd => {
                this.onChatMsg(cd.Sender, cd.SendTime, cd.Message);
            });
        }
        this.RoomType = RoomType;
        this.isNew = true;

        this.UIRefrsh();
    }

    onChatMsg(sender, date, msg)
    {
        var data = new ChatData();
        data.Sender = sender;
        data.SendTime = date;
        data.Message = msg;

        this.RoomChats.push(data);

        this.isNew = true;
        this.UIRefrsh();
    }

    checkNewFlag()
    {
        this.isNew = false;
        this.UIRefrsh();
    }

    addMember(MemberList)
    {
        MemberList.forEach(mb => {
            var md = new MemberData();
            md.AccountNo = mb.AccountNo;
            md.UserNick = mb.UserNick;
            md.LoginTime = mb.LoginTime;
            md.ProfileUrl = mb.ProfileUrl;
            this.RoomMembers.push(md);
        });

        this.UIRefrsh();
    }

    removeMember(MemberList)
    {
        MemberList.forEach(mb => {
            var idx = this.RoomMembers.findIndex(function(item) {return mb.AccountNo == item.AccountNo; })
            if(idx > -1)
            {
                this.RoomMembers.splice(idx, 1);
            }
        });

        this.UIRefrsh();
    }

    UIRefrsh()
    {
        if(this.RoomUI)
        {
            this.RoomUI.setRoomInfoUI(this);
        }
    }
};


module.exports = ChatRoom;
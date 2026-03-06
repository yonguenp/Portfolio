// Learn cc.Class:
//  - https://docs.cocos.com/creator/manual/en/scripting/class.html
// Learn Attribute:
//  - https://docs.cocos.com/creator/manual/en/scripting/reference/attributes.html
// Learn life-cycle callbacks:
//  - https://docs.cocos.com/creator/manual/en/scripting/life-cycle-callbacks.html

cc.Class({
    extends: cc.Component,

    properties: {
        UsersListLabel : cc.Label,
        LastChatMsg : cc.Label,
        UserCount : cc.Label,
        DateLabel : cc.Label,
        RedDot : cc.Node,
    },

    // LIFE-CYCLE CALLBACKS:

    // onLoad () {},

    start () {

    },

    // update (dt) {},

    setRoomInfoUI(data)
    {
        var usersText = '';
        data.RoomMembers.forEach(member => {
            if(usersText)
                usersText += ',';

            usersText += member.UserNick            
        });

        if(usersText.length > 25)
        {
            usersText = usersText.substr(0, 22);
            usersText += '...';
        }

        this.UsersListLabel.string = usersText;
        this.UserCount.string = '' + data.RoomMembers.length + '명';
        
        var lastChatMsg = null;        
        if(data.RoomChats.length - 1 >= 0)
            lastChatMsg = data.RoomChats[data.RoomChats.length - 1];

        if(lastChatMsg)
        {
            this.LastChatMsg.string = lastChatMsg.Message;
            this.DateLabel.string = new Date(lastChatMsg.SendTime * 1000).apm_hhmmss();
        }
        else
        {
            this.LastChatMsg.string = '';
            this.DateLabel.string = '';
        }

        this.RedDot.active = data.isNew;
        
        data.RoomUI = this;
    }

});

var YoutubeChat = require('YoutubeChat');
var YTC_STATE = cc.Enum({
    STATE_LOBBY : 0,
    STATE_ROOM  : 1,
});

cc.Class({
    extends: cc.Component,
    
    properties: {
        Room : cc.Node,
        Lobby : cc.Node,
        State : {
            default: YTC_STATE.STATE_LOBBY,
            type: YTC_STATE
        },

        YoutubeChatControl : YoutubeChat
    },
    
    // LIFE-CYCLE CALLBACKS:

    onLoad () {
        this.SetState(YTC_STATE.STATE_LOBBY);
    },

    start () {

    },

    SetState(state)
    {
        if(this.State == state)
            return;

        this.State = state;

        this.Room.active    = this.State == YTC_STATE.STATE_ROOM;
        this.Lobby.active   = this.State == YTC_STATE.STATE_LOBBY;
    },

    OnEnterRoom(event, roomNo)
    {
        console.log(roomNo);
        this.YoutubeChatControl.SetRoomNo(roomNo);
        this.SetState(YTC_STATE.STATE_ROOM);
    },
    // update (dt) {},
});

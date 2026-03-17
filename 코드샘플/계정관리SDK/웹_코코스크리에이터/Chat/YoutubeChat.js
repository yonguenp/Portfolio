var CHAT = require('ChatManager');
var ChatUI = require('Chat');
var AppUtils = require('AppUtils')

cc.Class({
    extends: ChatUI,

    properties: {
        webview : cc.WebView,
        playListWebView : cc.WebView,
        playerWebview : cc.WebView,
        roomNo : 0
    },

    onLoad : function()
    {
        this.bufferPlayList = new Array();
        this.onLoadPlayer = false;
        cc.systemEvent.on(cc.SystemEvent.EventType.KEY_DOWN, this.onKeyDown, this);
        cc.systemEvent.on(cc.SystemEvent.EventType.KEY_UP, this.onKeyUp, this);
    },

    // destroy : function() {
    //     cc.systemEvent.off(cc.SystemEvent.EventType.KEY_DOWN, this.onKeyDown, this);
    //     cc.systemEvent.off(cc.SystemEvent.EventType.KEY_UP, this.onKeyUp, this);
    // },

    onKeyDown: function (event) {
        
    },

    SetRoomNo : function(roomNo) {
        this.roomNo = roomNo;
    },

    onKeyUp: function (event) {
        switch(event.keyCode) {
            case cc.macro.KEY.enter:
                this.webview.evaluateJS(`
                    if(onEnterKeyEvent != null)
                    {
                        onEnterKeyEvent();
                    }
                    `);
                break;
        }
    },

    onEnable : function() {
        console.log('onEnable CustomChat, this Room UID : ' + this.thisRoomUID);
        this.webview.node.on('loaded', this.webviewLoaded, this);   
        this.playListWebView.node.on('loaded', this.playListWebViewLoaded, this);
        this.playerWebview.node.on('loaded', this.playerWebViewLoaded, this);
        window.YGAPP = this;    
    },

    onDisable : function(){
        CHAT.clearTarget(null);

        window.YGAPP = null;
    },

    webviewLoaded : function(){
        CHAT.setTarget(this);    
    },

    playListWebViewLoaded : function() {
        console.log('playListWebViewLoaded');
        console.log(this.playListWebView);

        this.onLoadPlayer = true;        
    },

    playerWebViewLoaded : function() {
        console.log('playerWebViewLoaded');
        this.playerWebview.evaluateJS(`
        SetRoomID('` + this.roomNo + `');
        `);
    },

    addProfile(data)
    {
       
    },

    removeProfile(data)
    {
        
    },

    onDisconnect : function()
    {
        
    },

    addMessage(data)
    {
        this.webview.evaluateJS(`
            if(addMessage != null)
            {
                addMessage(` + JSON.stringify(data) + `);
            }
            else
            {
                tempArray.push(` + JSON.stringify(data) + `);
            }
            `);
    },

    update : function(dt)
    {
        
    },

    onAddPlayVideo : function(videoID)
    {
        this.playerWebview.evaluateJS(`
        AddPlayList('` + videoID + `');
        `);
    },

    onPlayVideoClear : function()
    {
        this.bufferPlayList = new Array();
        
        if(this.onLoadPlayer == false)
            return;

        this.playListWebView.evaluateJS(`
            onResultAddPlayList('clear');
        `);
    },

    onResultAddPlayVideo : function(videoID, playTime, title)
    {
        var param = title + ' | ' + playTime + '(' + videoID + ')';
        this.bufferPlayList.push(param);

        if(this.onLoadPlayer == false)
        {   
            return;
        }    

        this.playListWebView.evaluateJS(`
        try
        {
            onResultAddPlayList();
        }
        catch
        {
            
        }    
        `);
    },

    CheckLiveUser()
    {
        var Members = CHAT.getMemberArray();
        Members.forEach(user =>{
            var data = {Message:'생존', Sender:user.UserNick, SendTime:new Date().getTime()};
            this.addMessage(data);
        });
    }
});

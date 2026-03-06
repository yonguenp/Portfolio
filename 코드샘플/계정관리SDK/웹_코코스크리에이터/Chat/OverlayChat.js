var CHAT = require('ChatManager');
var NetworkManager = require("NetworkManager");
var ChatUI = require('Chat');
var Samanda = require('Samanda');
var OverayChatMsgUI = require('ChatMsg');
var WSM = require('WebsocketManager');
var Popup = require('MessagePopup');
var Account = require("AccountManager");
var WebSocket = require('WebsocketManager');

cc.Class({
    extends: ChatUI,

    properties: {
        isLeft : true,

        left_btn_image : cc.Node,
        right_btn_image : cc.Node,
        isFocus : true,

        /**
         * Http Request Ver. OpenChat을 위해 추가됨
         * 확인을 위해 임시로 만들어 둔 것.
         * 추후 운용방식에 따라 변경할 것.
         */
        //pullingDuration : 5, // 오픈채팅 pulling interval
        //timer : 5,
    },
    
    start : function()
    {
        if(!Samanda.isStandAlone())
        {
            Samanda.onPreLoadLoginScene();
        }
    },
    
    onEnable : function() {
        this.chat_container_height = -2;
        this.items.splice(0, this.items.length);
        this.chat_container.active = false;
        
        this.needRefresh = true;
        this.needScrollToBottom = false;

        var canvas = Samanda.findCanvas();
        var offset = (canvas.width - 467) / 2;
        this.node.x = this.isLeft ? offset * -1 : offset;

        this.left_btn_image.active = !this.isLeft;
        this.right_btn_image.active = this.isLeft;

        CHAT.setTarget(this);     

        //window.onfocus = this.onFocus.bind(this);
        //window.onblur = this.onBlur.bind(this);
    },

    onDisable : function(){
        CHAT.clearTarget(null);

        //window.onfocus = null;
        //window.onblur = null;
    },

    onFocus : function()
    {
        this.isFocus = true;
        this.onFocusAction();
    },

    onBlur : function()
    {
        this.isFocus = false;
        this.onFocusAction();
    },

    onDisconnect : function()
    {
        // var item = cc.instantiate(this.chat_date_Prefab);        
        // item.parent = this.chat_container;
        // this.chat_container.insertChild(item, 0);

        // this.chat_scroll_view.scrollToBottom();
    },

    addProfile(data)
    {
       
    },

    removeProfile(data)
    {
        
    },

    addMessage(data)
    {
        if (!(data.Message) && !(data.Image)){
            console.error('Invalid chat msg: ' + data.toString());
            return;
        }

        if(this.needScrollToBottom == false && this.chat_container.y >= (this.chat_scroll_view.node.height / 2) - this.chat_container_height)
            this.needScrollToBottom = true;
        
        var item = cc.instantiate(data.Sender == Account.GetNickName() ? this.chat_me_Prefab : this.chat_user_Prefab);
        if (data.Message) {
            item.getComponent('ChatMsg').setMsgText(data);
        } else if (data.Image) {
            item.getComponent('ChatMsg').setImageMsg(data);
        }
        
        this.chat_container_height += item.height + 2;
        item.parent = this.chat_container;
    },

    onFocusAction : function()
    {
        console.log('onFocusAction : ' + this.isFocus);

        var action = null;
        if(this.isFocus)
        {
            if(this.chat_scroll_view.node.opacity == 255)
                return;    
            
            this.chat_scroll_view.node.stopActionByTag(0);    
            action = cc.fadeIn(0.7);
        }
        else
        {
            if(this.chat_scroll_view.node.opacity == 0)
                return;

            this.chat_scroll_view.node.stopActionByTag(0);    
            action = cc.fadeOut(0.7);
        }

        action.setTag(0);
        this.chat_scroll_view.node.runAction(action);
    },

    update : function(dt)
    {
        if(this.needRefresh)
            this.refresh();

        if(this.needScrollToBottom)
        {               
            console.log('needScrollToBottom : ' + this.chat_scroll_view.node.height + " " + this.chat_container_height + " " + this.chat_container.height);
            this.chat_scroll_view.setContentPosition(cc.v2(0, ((this.chat_scroll_view.node.height / 2) - this.chat_container_height * -1)));    
            this.needScrollToBottom = false;
        }     
    },

    refresh : function()
    {
        this.needRefresh = false;
        
        this.chat_container.active = true;
        this.needScrollToBottom = true;
    },

    sendMsg : function()
    {
        if(!this.msgEditBox.string)
            return;

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

    onChatAlign : function()
    {
        this.isLeft = !this.isLeft;
        
        this.left_btn_image.active = !this.isLeft;
        this.right_btn_image.active = this.isLeft;

        var children = this.chat_container.children;
        for (var i = 0; i < children.length; ++i) {
            var ui = children[i].getComponent('ChatMsg');
            if(ui)
            {
                if(this.isLeft)
                    ui.setLeft();
                else
                    ui.setRight();
            }
        }

        require('AppUtils').sendWebState(this.isLeft ? 7 : 8);
    },

    onMiniMode : function()
    {
        this.isLeft = true;

        cc.director.getScene().opacity = 0;
        require("Samanda").setWebState(6);
        Samanda.onChangeLoginScene();
    },
});
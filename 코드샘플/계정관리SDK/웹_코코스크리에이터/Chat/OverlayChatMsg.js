var ChatMsgUI = require('ChatMsg');
var UIUtils = require('UIUtils');

cc.Class({
    extends: ChatMsgUI,

    setMsgText : function(data)
    {
        var msg = data.Message;        
        var user = data.Sender;

        var txt = msg;
        if(msg.length > 21)
        {
            txt = msg.substr(0, 20);
            txt += "\n"
            txt += msg.substr(20, 20);
        }

        this.msg_txt.string = txt;
        this.msg_txt._forceUpdateRenderData();
        
        this.msg_name.string = user;
        this.msg_name._forceUpdateRenderData();

        this.msg_box.width = Math.max(this.msg_txt.ui_width(), this.msg_name.ui_width()) + 25;
        this.msg_box.height = this.msg_txt.ui_height() + this.msg_name.ui_height() + 5;            
        
        this.node.height = this.msg_box.height;
    },

    setImageMsg : function(data)
    {
        this.msg_txt.node.color = cc.Color.ORANGE;
        this.setMsgText({msg:'[이미지]',Sender:data.Sender});
    },

    setLeft : function()
    {
        this.msg_box.anchorX = 0;
        this.msg_box.x = -233.5;
        
        this.msg_name.horizontalAlign = cc.Label.HorizontalAlign.LEFT;
        this.msg_name.anchorX = 0;
        this.msg_name.node.x = -219.5;

        this.msg_txt.horizontalAlign = cc.Label.HorizontalAlign.LEFT;
        this.msg_txt.anchorX = 0;
        this.msg_txt.node.x = -219.5;
    },

    setRight : function()
    {
        this.msg_box.anchorX = 1;
        this.msg_box.x = 233.5;

        this.msg_name.horizontalAlign = cc.Label.HorizontalAlign.RIGHT;
        this.msg_name.anchorX = 1;
        this.msg_name.node.x = 219.5 - this.msg_name.ui_width();

        this.msg_txt.horizontalAlign = cc.Label.HorizontalAlign.RIGHT;
        this.msg_txt.anchorX = 1;
        this.msg_txt.node.x = 219.5 - this.msg_txt.ui_width();
    }
});

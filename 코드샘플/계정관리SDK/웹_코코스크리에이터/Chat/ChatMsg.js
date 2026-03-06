var CHAT = require('ChatManager');
var RemoteSprite = require('RemoteSprite');

var ChatMsgUI = cc.Class({

    extends: cc.Component,

    properties: {
        isOther:false,

        msg_profile: RemoteSprite,
        msg_name: cc.Label,
        msg_txt: cc.Label,
        msg_box: cc.Node,
        msg_date: cc.Label,
        msg_img: cc.Sprite        
    },

    setMsgText : function(data)
    {
        this.msg_img.node.enabled = false;

        var msg = data.Message;
        var date = new Date(data.SendTime * 1000).apm_hhmmss();
        var user = data.Sender;
        
        var txt = msg;
        if(msg.length > 21)
        {
            txt = msg.substr(0, 20);
            txt += "\n"
            txt += msg.substr(20, 20);
        }

        this.msg_txt.string = txt;
        this.msg_date.string = date;
        
        if(this.isOther)
        {
            this.msg_name.string = user;
        }

        this.setLayoutTxt();

        if(this.msg_profile)
        {
            if(data.ProfileUrl)
            {
                this.msg_profile.setRemoteSprite(data.ProfileUrl, null, false);
                this.msg_profile.node.color = cc.Color.WHITE;
            }
            else
            {
                var user = null;
                if(data.hasOwnProperty('SenderAccountNo'))
                    user = CHAT.getMemberData(data.SenderAccountNo);            
                else
                    user = CHAT.getMemberDataWithNickName(data.Sender);            

                if(user && user.ProfileUrl)
                {
                    this.msg_profile.setRemoteSprite(user.ProfileUrl, null, false);
                    this.msg_profile.node.color = cc.Color.WHITE;
                }
            }
        }

        if(Samanda.isYGProduct())
        {
            this.node.x = 0;
        }
    },

    setImageMsg : function(data)
    {
        var user = data.Sender;
        if(this.isOther)
        {
            this.msg_name.string = user;
        }

        if(this.msg_profile)
        {
            if(data.ProfileUrl)
            {
                this.msg_profile.setRemoteSprite(data.ProfileUrl, null, false);
                this.msg_profile.node.color = cc.Color.WHITE;
            }
            else
            {
                var user = null;
                if(data.hasOwnProperty('SenderAccountNo'))
                    user = CHAT.getMemberData(data.SenderAccountNo);            
                else
                    user = CHAT.getMemberDataWithNickName(data.Sender);            

                if(user && user.ProfileUrl)
                {
                    this.msg_profile.setRemoteSprite(user.ProfileUrl, null, false);
                    this.msg_profile.node.color = cc.Color.WHITE;
                }
            }
        }

        // exclusive
        this.msg_txt.node.enabled = false;

        // UI object
        var spr = this.msg_img;
        this.msg_date.string = new Date(data.SendTime * 1000).apm_hhmmss();
        
        const fixedHeight = 128;        
        // HTML element for texture creation
        let img = new Image();
        var self = this;
        img.onload = function() {
            let tex = new cc.Texture2D();
            tex.initWithElement(img);
            tex.handleLoadedTexture();

            spr.spriteFrame = new cc.SpriteFrame(tex);

            // set scale
            //let w = spr.spriteFrame.getRect().width; //넓이 무시
            let h = spr.spriteFrame.getRect().height;

            //console.log('spr.spriteFrame.getRect() : ' + w + ',' + h);

            spr.node.setScale(fixedHeight / h);

            //const limit = 400;
            //if (limit < w || limit < h) {
            // if (limit < h) {
            //     let scale = limit / Math.max(w, h);
            //     spr.node.setScale(scale);
            // }
            self.setLayoutImg();     
        };
        
        // set base64 data
        var pos = data.Image.indexOf(',') + 1;
        var path = data.Image.substr(0, pos - 1);
        var src = data.Image.substr(pos);
        
        console.log('data.Image : ' + data.Image);

        if (0 > src.substr(0, 25).indexOf('data:image')) {
            src = 'data:image/png;base64,' + src;
        }

        img.setAttribute('src', src);
        //console.log('img.setAttribute : ' + img.width + ',' + img.height); 
        
        spr.node.height = fixedHeight;
        spr.node.name = path;
        self.setLayoutImg();       
    },

    setLayoutTxt : function() {
        // draw
        this.msg_txt._forceUpdateRenderData();
        this.msg_date._forceUpdateRenderData();

        // box size
        this.msg_box.width = this.msg_txt.ui_width() + 22;
        this.msg_box.height = this.msg_txt.ui_height() + 18;

        // date pos
        this.msg_date.node.y = this.msg_box.y - this.msg_box.height + this.msg_date.ui_height();

        // horizontal pos and node height
        if (this.isOther) {            
            this.msg_date.node.x = this.msg_box.x + this.msg_box.width + 5;
            this.node.height = this.msg_box.height + 35 + 10;
        }
        else
        {
            this.msg_date.node.x = this.msg_box.x - this.msg_box.width - 5;
            this.node.height = this.msg_box.height + 10;
        }
    },

    setLayoutImg : function() {
        // image scale
        
        // draw
        this.msg_txt._forceUpdateRenderData();
        //this.msg_img._forceUpdateRenderData();
        
        const fixedHeight = 128;

        // rendered size
        var node = this.msg_img.node;
        var w = node.width * node.scale;
        var h = node.height * node.scale;

        // box size
        this.msg_box.width = w + 22;
        this.msg_box.height = h + 27;

        // date pos
        this.msg_date.node.y = this.msg_box.y - this.msg_box.height + this.msg_date.ui_height();

        // horizontal pos and node height
        if (this.isOther) {            
            this.msg_date.node.x = this.msg_box.x + this.msg_box.width + 5;
            this.node.height = this.msg_box.height + 35 + 10;
        }
        else
        {
            this.msg_date.node.x = this.msg_box.x - this.msg_box.width - 5;
            this.node.height = this.msg_box.height + 10;
        }
    },

    onImageButtonClick : function()
    {
        console.log('onImageButtonClick : openPhotoViewer');
        var sprnode = this.msg_img.node;

        var NetworkManager = require("NetworkManager");
        var data = require("AccountManager").MakeAccountFormData();
        data.append('pid', NetworkManager.product_type);
        data.append('img', sprnode.name);

        console.log(sprnode.name);

        NetworkManager.SendRequestPostWithResponseTypeBlob(
            'chat/get_img',
            data,
            function(res) {   
                require("MessagePopup").openPhotoViewerWithImage(res);       
            }.bind(this),
            function() {                                    
                require("MessagePopup").openMessageBoxWithKey("POPUP_85");
            }.bind(this)
        )
    }
});

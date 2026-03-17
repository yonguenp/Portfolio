var RemoteSprite = require("RemoteSprite");
var Samanda = require("Samanda");
var POPUP_TYPE;
POPUP_TYPE = cc.Enum({    
  POPUPTYPE_BTN_CONFIRM : 0,
  POPUPTYPE_BTN_YESNO : 1,
  POPUPTYPE_PHOTO_VIEWER_URL : 2,
  POPUPTYPE_PHOTO_VIEWER_SPR : 3,
  POPUPTYPE_WITH_NODE_CONFIRM : 4,
});

cc.Class({
    extends: cc.Component,

    properties: {
        Background : cc.Sprite,
        Window : cc.Sprite,        
        Message : cc.Label,
        Image : RemoteSprite,

        ConfirmTypeNode : cc.Node,
        YesNoTypeNode : cc.Node,

        BtnOk : cc.Node,
        BtnYes : cc.Node,
        BtnNo : cc.Node,
    },

    statics:{
        self:null,
        openMessageBox: function(text, onClose = null)
        {
            cc.resources.load("Prefabs/Popup/MessagePopup", function (err, prefab) {
                var Popup = cc.instantiate(prefab);
                var popupScript = Popup.getComponent('MessagePopup');
                popupScript.close_callback = onClose;
                                
                let canvas = popupScript.constructor.findCanvas();
                canvas.addChild(Popup);
                
                popupScript.setPopupLayout(POPUP_TYPE.POPUPTYPE_BTN_CONFIRM, text, canvas);
                
                require("Samanda").cancelPopup = popupScript.onCancel.bind(popupScript);                
            });
        },

        openMessageBoxWithKey: function(key, onClose_1 = null, onClose_2 = null, param = null)
        {          
            cc.resources.load("Prefabs/Popup/Box_PopUp", function (err, prefab) {
                var Popup = cc.instantiate(prefab);
                var popupScript = Popup.getComponent('PopupTemplate');
                
                popupScript.close_callback1 = onClose_1;
                popupScript.close_callback2 = onClose_2;
                                
                let canvas = require('MessagePopup').findCanvas();
                canvas.addChild(Popup);
                
                popupScript.setPopupWithData(key, param);

                require("Samanda").cancelPopup = popupScript.onCancel.bind(popupScript);                                
            });
        },

        openMessageBoxWithNode: function(text, additionalNode, onClose = null)
        {
            cc.resources.load("Prefabs/Popup/MessagePopup", function (err, prefab) {
                var Popup = cc.instantiate(prefab);
                var popupScript = Popup.getComponent('MessagePopup');
                popupScript.close_callback = onClose;
                                
                let canvas = popupScript.constructor.findCanvas();
                canvas.addChild(Popup);
                
                popupScript.setPopupLayoutWithNode(POPUP_TYPE.POPUPTYPE_WITH_NODE_CONFIRM, text, additionalNode, canvas);

                require("Samanda").cancelPopup = popupScript.onCancel.bind(popupScript);
            });
        },

        openYesNoBox: function(text, onYes = null, onNo = null)
        {
            cc.resources.load("Prefabs/Popup/MessagePopup", function (err, prefab) {
                var Popup = cc.instantiate(prefab);
                var popupScript = Popup.getComponent('MessagePopup');
                popupScript.yes_callback = onYes;
                popupScript.no_callback = onNo;
                                
                let canvas = popupScript.constructor.findCanvas();
                canvas.addChild(Popup);
                
                popupScript.setPopupLayout(POPUP_TYPE.POPUPTYPE_BTN_YESNO, text, canvas);

                require("Samanda").cancelPopup = popupScript.onCancel.bind(popupScript);
            });
        },

        openPhotoViewerWithURL : function(url, callback = null)
        {
            cc.resources.load("Prefabs/Popup/PhotoViewerPopup", function (err, prefab) {
                var Popup = cc.instantiate(prefab);
                var popupScript = Popup.getComponent('MessagePopup');
                popupScript.close_callback = callback;
                                
                let canvas = popupScript.constructor.findCanvas();
                canvas.addChild(Popup);
                
                popupScript.setPopupLayout(POPUP_TYPE.POPUPTYPE_PHOTO_VIEWER_URL, url, canvas);

                require("Samanda").cancelPopup = popupScript.onCancel.bind(popupScript);
            });
        },

        openPhotoViewerWithImage : function(spr, callback = null)
        {
            cc.resources.load("Prefabs/Popup/PhotoViewerPopup", function (err, prefab) {
                var Popup = cc.instantiate(prefab);
                var popupScript = Popup.getComponent('MessagePopup');
                popupScript.close_callback = callback;
                                
                let canvas = popupScript.constructor.findCanvas();
                canvas.addChild(Popup);
                
                popupScript.setPopupLayout(POPUP_TYPE.POPUPTYPE_PHOTO_VIEWER_SPR, spr, canvas);

                require("Samanda").cancelPopup = popupScript.onCancel.bind(popupScript);
            });
        },

        findCanvas : function()
        {
            var scn = cc.director.getScene();
            if (null === scn) {
                console.log('null === scn');
                return scn;
            }

            var canvas = scn.getChildByName('Canvas');
            if (null === canvas) {
                console.log('find canvas failed #1');
                canvas = cc.find('Canvas');
            }
            
            if (null !== canvas) {
                return canvas;
            } else {
                console.log('find canvas failed #2');
                console.log(scn.children);
                return scn;
            }
        },
    },

    setPopupLayout : function(type, data, canvas)
    {
        var btnOffset = 25;
        var btnSizeHeight = 70;

        this.Background.node.width = canvas.width * 2;
        this.Background.node.height = canvas.height * 2;

        switch(type)
        {
            case POPUP_TYPE.POPUPTYPE_PHOTO_VIEWER_URL :
                this.setMessageImageURL(type, data, canvas);                
            break;
            case POPUP_TYPE.POPUPTYPE_PHOTO_VIEWER_SPR :
                this.setMessageImageSPR(type, data, canvas);                
            break;
            case POPUP_TYPE.POPUPTYPE_BTN_CONFIRM :
                this.setMessageText(type, data, canvas);
                this.ConfirmTypeNode.active = true;
                this.YesNoTypeNode.active = false;
                this.ConfirmTypeNode.y = this.Window.node.y - (this.Window.node.height / 2) - btnOffset - (btnSizeHeight / 2);
            break;
            case POPUP_TYPE.POPUPTYPE_BTN_YESNO :
                this.setMessageText(type, data, canvas);
                this.ConfirmTypeNode.active = false;
                this.YesNoTypeNode.active = true;
                this.YesNoTypeNode.y = this.Window.node.y - (this.Window.node.height / 2) - btnOffset - (btnSizeHeight / 2);
            break;
        }
    },

    setPopupLayoutWithNode : function(type, text, addtionalNode, canvas)
    {   
        if(POPUP_TYPE.POPUPTYPE_WITH_NODE_CONFIRM != type)
            return;

        var btnOffset = 25;
        var btnSizeHeight = 70;

        this.Background.node.width = canvas.width * 2;
        this.Background.node.height = canvas.height * 2;

        var maxWindowWidth = 560;
        var minWindowWidth = 400;
        var minWindowHeight = 150;
        
        var txtOffsetX = 30;
        var txtOffsetY = 50;

        this.Message.string = text;                 
        this.Message._forceUpdateRenderData();

        if(this.Message.ui_width() > maxWindowWidth - (txtOffsetX * 2))
        {
            this.Message.node.width = (maxWindowWidth - (txtOffsetX * 2)) / this.Message.node.scaleX;
            this.Message.overflow = cc.Label.Overflow.RESIZE_HEIGHT;
            this.Message._forceUpdateRenderData();
        }
        else if(this.Message.ui_width() < minWindowWidth - (txtOffsetX * 2))
        {
            this.Message.node.width = (minWindowWidth - (txtOffsetX * 2)) / this.Message.node.scaleX;
            this.Message.overflow = cc.Label.Overflow.CLAMP;
            this.Message._forceUpdateRenderData();
        }

        this.Message.node.y += addtionalNode.height / 2;
        addtionalNode.parent = this.Window.node;
        addtionalNode.x = 0;
        addtionalNode.y = this.Message.node.y - (this.Message.ui_height() / 2) - (addtionalNode.height / 2);

        this.Window.node.width = Math.max(addtionalNode.width, this.Message.ui_width()) + (txtOffsetX * 2);
        this.Window.node.height = this.Message.ui_height() + (txtOffsetY * 2) + addtionalNode.height;

        this.Window.node.y = minWindowHeight / 2;
        if(minWindowHeight < this.Window.node.height)
        {
            this.Window.node.y = this.Window.node.height * 0.25;
        }    
        
        this.ConfirmTypeNode.active = true;
        this.YesNoTypeNode.active = false;
        this.ConfirmTypeNode.y = this.Window.node.y - (this.Window.node.height / 2) - btnOffset - (btnSizeHeight / 2);
    },
    
    setMessageText : function(type, text, canvas)
    {      
        var maxWindowWidth = 560;
        var minWindowWidth = 400;
        var minWindowHeight = 150;
        
        var txtOffsetX = 30;
        var txtOffsetY = 50;

        this.Message.string = text;                 
        this.Message._forceUpdateRenderData();

        if(this.Message.ui_width() > maxWindowWidth - (txtOffsetX * 2))
        {
            this.Message.node.width = (maxWindowWidth - (txtOffsetX * 2)) / this.Message.node.scaleX;
            this.Message.overflow = cc.Label.Overflow.RESIZE_HEIGHT;
            this.Message._forceUpdateRenderData();
        }
        else if(this.Message.ui_width() < minWindowWidth - (txtOffsetX * 2))
        {
            this.Message.node.width = (minWindowWidth - (txtOffsetX * 2)) / this.Message.node.scaleX;
            this.Message.overflow = cc.Label.Overflow.CLAMP;
            this.Message._forceUpdateRenderData();
        }

        this.Window.node.width = this.Message.ui_width() + (txtOffsetX * 2);
        this.Window.node.height = this.Message.ui_height() + (txtOffsetY * 2);

        this.Window.node.y = minWindowHeight / 2;
        if(minWindowHeight < this.Window.node.height)
        {
            this.Window.node.y += (this.Window.node.height - minWindowHeight) / 2;
        }       
    },

    setMessageImageURL : function(type, url, canvas)
    {      
        var func = this.onImageLoaded.bind(this);
        this.Image.setRemoteSprite(url, func);                
    },

    setMessageImageSPR : function(type, spr, canvas)
    {   
        var url = URL.createObjectURL(spr);
        console.log(url);     

        var img = new Image();
        img.src = url;

        let tex = new cc.Texture2D();
        tex.initWithElement(img);
        tex.handleLoadedTexture();
        this.Image.spriteFrame = new cc.SpriteFrame(tex);   
        
        console.log(this.Image);

        this.onImageLoaded();
    },

    onImageLoaded : function()
    {
        var canvas = this.constructor.findCanvas();
        var OffsetX = 30;
        var OffsetY = 30;

        var ImageHeight = 360;        
        var ratio = (canvas.height - OffsetY) / ImageHeight;  
        this.Image.node.setScale(ratio);
        
        this.Window.node.width = canvas.width;
        this.Window.node.height = canvas.height;
    },

    onPressExit : function()
    {        
        Samanda.cancelPopup = null;
        if (this.close_callback != null && "function" === typeof this.close_callback) {
            this.close_callback();
        }
        this.node.destroy();
    },

    onPressYes : function()
    {        
        Samanda.cancelPopup = null;
        if (this.yes_callback != null && "function" === typeof this.yes_callback) {
            this.yes_callback();
        }
        this.node.destroy();
    },

    onPressNo : function()
    {        
        Samanda.cancelPopup = null;
        if (this.no_callback != null && "function" === typeof this.no_callback) {
            this.no_callback();
        }
        this.node.destroy();
    },

    onCancel : function()
    {
        if(this.YesNoTypeNode.active == true)
            this.onPressNo();
        else
            this.onPressExit();
    },
});

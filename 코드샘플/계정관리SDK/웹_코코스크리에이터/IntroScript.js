var NetworkManager = require("NetworkManager");
var Samanda = require("Samanda");

cc.Class({
    extends: cc.Component,

    properties: {  
        camera : cc.Camera,
        background : cc.Node,      
        logo : cc.Sprite,
        canvas : cc.Canvas,
        rate : 0,         
        splash_end : false,
        response_end : false,
        logoFrames : [cc.SpriteFrame],
        logoAudio : cc.AudioClip,
    },

    onLoad : function()
    {
        var canvasNode = Samanda.findCanvas();
        var canvas = null;
        if(canvasNode)
        {
            canvas = canvasNode.getComponent(cc.Canvas);
        }
        
        console.log('cur Orientation : ' + Samanda.getOrientationType());
        switch(Samanda.getOrientationType())
        {
            case 1:     
                if(canvas)
                {
                    canvas.fitHeight = true;
                    canvas.fitWidth = true;
                }       
                break;
            case 0:
                if(canvas)
                {
                    canvas.fitHeight = true;
                    canvas.fitWidth = true;
                }  
                break;
            default:
                break;
        } 
    },

    onNextScene : function()
    {
        console.log("[check] splash_end : " + this.splash_end + " response : " + this.response_end);
        
        if(this.splash_end && this.response_end)
        {
            if('SERVER_MAINTENANCE' == response_result)
            {
                this.logo.node.active = false;
                var callback = function(){
                    require('AppUtils').SetAppData('server', 'maintenance');
                    cc.director.loadScene('EmptyScene.fire');
                };
                require("MessagePopup").openMessageBoxWithKey("POPUP_19", callback);    
                return;
            }
            if('NEED_APP_UPDATE' == response_result)
            {
                this.logo.node.active = false;
                var callback = function(){
                    if(require("Samanda").isIPhone())
                        require('AppUtils').SetAppData('url', 'https://apps.apple.com/kr/app/양어장-고양이/id1570620778');                        
                    else
                        require('AppUtils').SetAppData('url', 'https://play.google.com/store/apps/details?id=com.sandboxgame.fishfarmcat');
                };

                require("MessagePopup").openMessageBoxWithKey("NEED_APP_UPDATE", callback);    
                return;
            }
            
            require("Samanda").setWebState(0);

            if(NetworkManager.IsValidAccount())
            {
                this.camera.backgroundColor = cc.Color(0,0,0,0);
                require("Samanda").setWebState(3);
                Samanda.onChangeLoginScene();
            }            
            else if(NetworkManager.IsNeedLogin())
            {
                require("Samanda").setWebState(2);
                if(Samanda.isAndroid() == false && Samanda.isZFBrower() == false && Samanda.isIPhone() == false)
                {
                    console.log("is standalone?");
                    Samanda.onChangeLoginScene();          
                }
                else
                {
                    cc.director.loadScene('EmptyScene.fire');
                }
            }
            else
            {
                console.log("[error] Network Response Error(standalone services is not yet)");
                Samanda.onChangeLoginScene();   
            }
        }
    },

    onResponseEnd : function()
    {
        this.response_end = true;
        this.onNextScene();        
    },

    onSplashEnd : function()
    {
        this.splash_end = true;
        this.onNextScene();        
    },

    splashSample : function()
    {
        this.camera.node.getComponent(cc.Camera).backgroundColor = cc.Color(255,255,255,255);
        
        console.log('audio Start');
        cc.audioEngine.play(this.logoAudio, false, 1);
            
        this.animSeq = 0;
        this.schedule(this.splashRun, 0.0333333333333333);
    },

    splashRun : function(dt)
    {
        if(this.logoFrames.length <= this.animSeq)
        {
            if(this.splash_end)
                return;

            this.onSplashEnd();
            return;
        }
        

        var frame = this.logoFrames[this.animSeq];
        if(frame != null)
            this.logo.spriteFrame = frame;        
        
        this.animSeq++;
    },

    start () {  
        if(require("PopupTemplate").self == null)
        {
            cc.resources.load("Data/Popup", function (err, file) {            
                require("PopupTemplate").Init(file.text);
            });
        }
              
        //this.splashSample();
        this.onSplashEnd();
    },

    update (dt) {
        if(!this.response_end)
        {
            if(NetworkManager.IsResponsedUserInfo())
            {            
                console.log('IntroScript.onResponseEnd');
                this.onResponseEnd();
            }
        }
    },
});

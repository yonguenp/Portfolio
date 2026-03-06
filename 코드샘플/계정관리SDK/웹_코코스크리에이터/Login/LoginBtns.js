var LoginEventScript = require('LoginEventScript');

var eLoginButtonType = cc.Enum({
    SANDBOX : 0,
    NAVER : 1,
    GOOGLE : 2,  
    KAKAO : 3,
    INSTAGRAM : 4,
    FACEBOOK : 5,
    APPLE : 6,
    GUEST : 7,
    TYPE_MAX : 8
});
  
cc.Class({
    extends: cc.Component,

    properties: {
        iconNode : [cc.Node],
        iconEnableArray : [Boolean],
        loginEventScript : LoginEventScript,
    },

    // LIFE-CYCLE CALLBACKS:

    onEnable () {
        //todo : iconOrderArray,iconEnableArray 추후 외부에서 컨트롤할수있게끔 변경하자
        // var iconOrderArray = [
        //     eLoginButtonType.SANDBOX,
        //     eLoginButtonType.NAVER,
        //     eLoginButtonType.GOOGLE,
        //     eLoginButtonType.KAKAO,
        //     eLoginButtonType.INSTAGRAM,
        //     eLoginButtonType.FACEBOOK,
        //     eLoginButtonType.APPLE,
        //     eLoginButtonType.GUEST,
        // ];

        // var iconEnableArray = [
        //     false, //Sandbox
        //     false, //Naver
        //     true, //Google
        //     false, //Kakao
        //     false, //Instagram
        //     false, //Facebook
        //     false, //Apple
        //     true, //Guest
        // ];
        
        for (let i = 0; i < this.iconNode.length; i++) {
            var iconNode = this.iconNode[i];
            iconNode.active = this.iconEnableArray[i];

            var anim = iconNode.getComponent(cc.Animation);
            if(anim)
                anim.play();
        }
    },

    start () {

    },

    setLoginEventScript(script)
    {
        this.loginEventScript = script;
    },

    openLoginBox : function()
    {
        this.loginEventScript.openLoginBox();
    },

    onLoginOtherPlatform : function(event, param)
    {
        switch(parseInt(param))
        {
            case eLoginButtonType.NAVER :
                this.loginEventScript.onLoginWithNaver();
                return;
            case eLoginButtonType.GOOGLE :
                this.loginEventScript.onLoginWithGoogle();
                return;
                case eLoginButtonType.APPLE :
                this.loginEventScript.onLoginWithApple();
                return;
        }
    },

    onLoginGuest : function()
    {
        this.loginEventScript.onLoginGuest();
    }
});

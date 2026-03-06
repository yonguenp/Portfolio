var LoginEventScript = require('LoginEventScript');
var NetworkManager = require("NetworkManager");

cc.Class({
    extends: cc.Component,

    properties: {
        id_box : cc.EditBox,
        pw_box : cc.EditBox,
        loginEventScript : LoginEventScript,
    },

    // LIFE-CYCLE CALLBACKS:

    // onLoad () {},

    start () {

    },

    setLoginEventScript(script)
    {
        this.loginEventScript = script;
    },

    onLoginWithIDPW : function()
    {
        var data = new FormData();
        data.append('pid', NetworkManager.product_type);
        data.append('typ', 0);
        data.append('id', this.id_box.string);
        data.append('pw', this.pw_box.string);

        this.pw_box.string = "";
        
        var res_callback = this.loginEventScript.onResponseLogin.bind(this.loginEventScript);
        NetworkManager.SendRequestPost("auth/signin", data, res_callback);
        
        this.loginEventScript.stateNext(/*eLoginState.STATE_NEXT_SCENE*/3);        
    },

    onBtnFindID : function()
    {
        this.loginEventScript.onBtnFindID();
    },

    onBtnFindPW : function()
    {
        this.loginEventScript.onBtnFindPW();
    },

    openSigninBox : function()
    {
        this.loginEventScript.openSigninBox();
    },

    offLoginBox : function()
    {
        this.loginEventScript.offLoginBox();
    },
});

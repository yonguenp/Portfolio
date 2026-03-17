var LoginEventScript = require("LoginEventScript");
var Account = require('AccountManager');
var NetworkManager = require("NetworkManager");

cc.Class({
    extends: cc.Component,

    properties: {
        nick_edit_box : cc.EditBox,

        loginEventScript : LoginEventScript
    },

    setLoginEventScript(script)
    {
        this.loginEventScript = script;
    },

    // LIFE-CYCLE CALLBACKS:

    // onLoad () {},

    start () {

    },

    onSummitNickName : function()
    {
        console.log('onSummitNickName');

        var data = new FormData();
        data.append('pid', NetworkManager.product_type);
        Account.AppendFormData(data);
        data.append('nic', this.nick_edit_box.string);
        
        for (var pair of data.entries()) { console.log(pair[0]+ ', ' + pair[1]); }

        var res_callback = this.loginEventScript.onResponseNick.bind(this.loginEventScript);
        NetworkManager.SendRequestPost("me/nick", data, res_callback, res_callback);        
    },
});

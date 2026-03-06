var LoginEventScript = require("LoginEventScript");
var NetworkManager = require("NetworkManager");
var MessagePopup = require("MessagePopup");
var Account = require('AccountManager');

cc.Class({
    extends: cc.Component,

    properties: {
        // 약관
        termsofuse_toggle_1 : cc.Toggle,
        termsofuse_toggle_2 : cc.Toggle,
        termsofuse_agree_btn : cc.Button,       
        termsofuse_script : cc.Node,
        termsofuse_script_agree1 : cc.Node,
        termsofuse_script_agree2 : cc.Node,
        termsofuse_webview_agree1 : cc.WebView,
        termsofuse_webview_agree2 : cc.WebView,

        loginEventScript : LoginEventScript
    },

    // LIFE-CYCLE CALLBACKS:

    onLoad () {
        console.log('new terms v' + NetworkManager.terms_ver);
        this.termsofuse_webview_agree1.url = NetworkManager.terms_uri1;
        console.log('new terms 1 : ' + this.termsofuse_webview_agree1.url);
        this.termsofuse_webview_agree2.url = NetworkManager.terms_uri2;
        console.log('new terms 2 : ' + this.termsofuse_webview_agree2.url);
    },

    start () {

    },

    setLoginEventScript(script)
    {
        this.loginEventScript = script;
    },

    onToggleAgree1 : function()
    {
        this.termsofuse_agree_btn.interactable = 
        this.termsofuse_toggle_1.isChecked && this.termsofuse_toggle_2.isChecked;
    },

    onToggleAgree2 : function()
    {
        this.termsofuse_agree_btn.interactable = 
        this.termsofuse_toggle_1.isChecked && this.termsofuse_toggle_2.isChecked;
    },

    clearTermsToggle : function()
    {
        this.termsofuse_toggle_1.isChecked = false;
        this.termsofuse_toggle_2.isChecked = false;
    },

    readAgree1 : function()
    {
        this.termsofuse_script.active = true;
        this.termsofuse_script_agree1.active = true;
        this.termsofuse_script_agree2.active = false;
    },

    readAgree2 : function()
    {
        this.termsofuse_script.active = true;
        this.termsofuse_script_agree1.active = false;
        this.termsofuse_script_agree2.active = true;
    },

    closeAgree : function()
    {
        this.termsofuse_script.active = false;
    },

    onBtnTermsAgree : function()
    {
        console.log('onTermsAgree');

        var data = new FormData();
        data.append('pid', NetworkManager.product_type);
        Account.AppendFormData(data);
        data.append('ver', NetworkManager.terms_ver);
        data.append('act', 1);

        var res_callback = this.loginEventScript.onResponseTerms.bind(this.loginEventScript);
        NetworkManager.SendRequestPost(
            "auth/terms", data, res_callback, null);

        this.clearTermsToggle();
    },

    onBtnTermsDisagree : function() {
        console.log('onBtnTermsDisagree');
        this.clearTermsToggle();
        NetworkManager.setLoggedOut();
                
        this.loginEventScript.start();       
        MessagePopup.openMessageBoxWithKey("POPUP_43");    
    },
});

var LoginEventScript = require("LoginEventScript");

cc.Class({
    extends: cc.Component,

    properties: {// banner
        banner_webview : cc.WebView,
        banner_toggle : cc.Toggle,
        banner_close_btn : cc.Button,

        idx : 0,
        loginEventScript : LoginEventScript,        
    },

    setLoginEventScript(script)
    {
        this.loginEventScript = script;
    },
    // LIFE-CYCLE CALLBACKS:

    // onLoad () {},

    start () {

    },

    // update (dt) {},

    onBtnCloseBanner : function()
    {
        var current = this.idx;

        if (this.banner_toggle.isChecked) {
            let data = Samanda.banners;
            if (data.length > current) {
                Samanda.setBannerDnd(data[current].bid);
            }
        }

        this.loginEventScript.setBannerUI(current + 1);
    },
});

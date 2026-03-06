var NetworkManager = require("NetworkManager");
var MessagePopup = require("MessagePopup");
var LoginEventScript = require("LoginEventScript");
var Account = require('AccountManager');

cc.Class({
    extends: cc.Component,

    properties: {
        id_box : cc.EditBox,
        pw_box : cc.EditBox,
        pwcheck_box : cc.EditBox,
        loginEventScript : LoginEventScript
    },

    setLoginEventScript(script)
    {
        this.loginEventScript = script;
    },
    
    onSignSubmit : function()
    {
        if(this.pw_box.string != this.pwcheck_box.string)
        {
            MessagePopup.openMessageBoxWithKey("POPUP_12");
            return;
        }

        var data = new FormData();
        data.append('pid', NetworkManager.product_type);
        data.append('typ', 0);
        data.append('id', this.id_box.string);
        data.append('pw', this.pw_box.string);
          
        var res_callback = this.onResponseSubmit.bind(this);
        var self = this;
        var onError = function() {
            MessagePopup.openMessageBoxWithKey("POPUP_60");    
            this.onOffSigninPopup();
        };
        NetworkManager.SendRequestPost(
            "auth/signup", data, res_callback, onError);
    },

    onResponseSubmit : function(response)
    {
        var data = JSON.parse(response);        
        var rs = parseInt(data["rs"]);  
        
        if(rs == 0)
        {
            MessagePopup.openMessageBoxWithKey("POPUP_61"); 
            this.onOffSigninPopup(this.id_box.string, this.pw_box.string);
            this.id_box.string = "";
            this.pw_box.string = "";
            this.pwcheck_box.string = "";
        }
        else
        {
            var typ = parseInt(data["err"]);            
            switch(typ)
            {
                case 1:
                    MessagePopup.openMessageBoxWithKey("POPUP_10");
                    break;
                case 2:
                    MessagePopup.openMessageBoxWithKey("POPUP_62"); 
                    break;
                case 3:
                    MessagePopup.openMessageBoxWithKey("POPUP_11");
                    break;
                case 11:
                    MessagePopup.openMessageBoxWithKey("POPUP_63"); 
                    break;
                case 12:
                    MessagePopup.openMessageBoxWithKey("POPUP_64"); 
                    break;
                case 13:
                    MessagePopup.openMessageBoxWithKey("POPUP_13");
                    break;
                case 14:
                    MessagePopup.openMessageBoxWithKey("POPUP_14");
                    break;
                default:                    
                    MessagePopup.openMessageBoxWithKey("POPUP_37"); 
                    this.onOffSigninPopup();
                    break;
            }
        }
    },
    
    onOffSigninPopup(id, pw)
    {
        if(this.loginEventScript)
        {
            this.loginEventScript.offSigninBox();
            return;
        }    

        this.node.destroy();
        if(id === undefined || pw === undefined)
        {
            return;
        }

        var param = new FormData();
        param.append('ano', Account.GetAccountNo());
        param.append('tok', Account.GetUserTokenAccount());
        param.append('pid', NetworkManager.product_type);
        param.append('typ', 0);
        param.append('id', id);
        param.append('pw', pw);

        var onSuccess = function () {
            MessagePopup.openMessageBoxWithKey("POPUP_66");   
        }.bind(this);
        var onError = function () {
            MessagePopup.openMessageBoxWithKey("POPUP_34");   
        }.bind(this);

        NetworkManager.SendRequestPost(
            "auth/link", param, onSuccess, onError);
    },

    onExitSignIn()
    {
        this.loginEventScript.offLoginBox();
    }
});

var Account = require('AccountManager');
var NetworkManager = require("NetworkManager");
var MessagePopup = require("MessagePopup");
var Samanda = require("Samanda");
var LoginEventScript = require('LoginEventScript');
var appUtils = require('AppUtils');

var eAccountType = cc.Enum({
    SB: 0,
    NV: 1,
    GG: 2,
    KK: 3,
    IG: 4,
    FB: 5,
    AP: 6,
    GUEST: 7,
    END: 8
});

cc.Class({
    extends: cc.Component,

    properties: {
        UserNickName: cc.Label,
        MailVerifiedLabel: cc.Label,
        MailVerifiedBtn: cc.Button,
        PasswordChangeBtn: cc.Button,
        LogoutBtn : cc.Button,
        GuestCaution : cc.Node,

        CurProfileImage: cc.Sprite,
        ProfileImagePrefab: cc.Prefab,
        ProfileImageContainer: cc.Node,
        loginEventScript : LoginEventScript,
        LinkPlatform : [cc.Button],
        VersionText : cc.Label,
        VersionHide : cc.Node,
    },

    onEnable() {
        if(this.VersionText != null)
        {
            cc.resources.load("version", function (err, version) {            
                this.VersionText.string = version;
            }.bind(this));

            this.VersionHide.active = true;
            this.VersionHideCount = 5;
        }

        console.log(Account);
        
        this.default_url = "../resources/";
        if (CC_DEBUG) {
            this.default_url = Samanda.getSamandaUrl() + 'resources/';            
        }

        cc.assetManager.loadRemote(this.default_url + Account.GetUserProfilePath(), this.onProfileImage.bind(this));

        var links = Account.GetUserLinkPlatforms();
        for(var i = 0; i < this.LinkPlatform.length; i++)
        {
            if(this.LinkPlatform[i])
            {
                var bLink = false;
                links.forEach(l => {
                    if(l == i)
                        bLink = true;
                });

                this.LinkPlatform[i].interactable = !bLink;
                var textNode = this.LinkPlatform[i].node.getChildByName('Icon_Linked');
                
                if(textNode)
                {                    
                    textNode.active = bLink;
                }
            }
        }

        this.PasswordChangeBtn.interactable = false;
        var guest = false;
        links.forEach(l => {
            if(l == 0)
            {
                this.PasswordChangeBtn.interactable = true;
            }
            if(l == 7)
            {
                guest = true;
            }
        });

        this.GuestCaution.active = false;
        if(links.length < 1 || (links.length == 1 && guest))
        {
            this.PasswordChangeBtn.node.active = false;
            this.LogoutBtn.node.active = false;
            this.GuestCaution.active = true;
        }
        
        this.UserNickName.string = Account.GetNickName();

        this.MailVerifiedLabel.string = "";
        this.MailVerifiedBtn.interactable = false;
        
        if(this.PasswordChangeBtn.interactable)
        {
            switch (Account.GetUserEmailState()) {
                case 0:
                    this.MailVerifiedLabel.string = "이메일을 인증해주세요.";
                    this.MailVerifiedBtn.interactable = true;
                    break;
                case 1:
                    this.MailVerifiedLabel.string = "인증 메일을 확인해주세요.";
                    this.MailVerifiedBtn.interactable = true;
                    break;
                case 2:
                    this.MailVerifiedLabel.string = Account.GetUserEmailAddress();
                    this.MailVerifiedBtn.interactable = false;
                    break;
                default:
                    this.MailVerifiedLabel.string = "샌드박스 계정과 연동되었을 때 메일 인증이 가능합니다.";
                    this.MailVerifiedBtn.interactable = false;
                    break;
            }
        }
        else
        {
            this.MailVerifiedLabel.string = "샌드박스 계정과 연동되었을 때 메일 인증이 가능합니다.";
            this.MailVerifiedBtn.interactable = false;
        }

        this.ProfileImageContainer.active = false;
    },

    setLoginEventScript(script)
    {
        this.loginEventScript = script;
    },

    onLogout: function () {
        var onYes = function () {
            var data = Account.MakeAccountFormData();
            data.append('pid', NetworkManager.product_type);

            var res_callback = this.onResponseLogout.bind(this);
            NetworkManager.SendRequestPost("auth/signout", data, res_callback);
        }.bind(this);

        MessagePopup.openMessageBoxWithKey("POPUP_30", onYes);
    },

    onResponseLogout : function(response)
    {
        var data = JSON.parse(response);                
        var rs = parseInt(data["rs"]);  

        if(rs !== 0)
        {
            console.error("로그아웃 실패 - " + rs + "\n" + data["msg"]);
        }
        
        NetworkManager.setLoggedOut();
        Samanda.onChangeLoginScene();
    },

    onTryVerifyMail() {
        console.log('onTryVerifyMail');
        MessagePopup.openMessageBoxWithKey("POPUP_18", function (popup) {
            var edit_box = popup.EditBox1.getChildByName('EditBox_1').getComponent(cc.EditBox);
            var mail_address = edit_box.string;

            if (0 > mail_address.indexOf('@')) {
                MessagePopup.openMessageBoxWithKey("POPUP_3");
                return;
            }

            var onSuccess = function (response) {
                var data = null;
                try {
                    data = JSON.parse(response);
                } catch (e) {
                    console.log('onTryVerifyMail returned not json');
                }

                var rs = -1;
                if(data != null)
                    rs = parseInt(data["rs"]);

                switch(rs)
                {
                    case 0:
                        this.MailVerifiedLabel.string = "인증 메일을 확인해주세요.";
                        this.MailVerifiedBtn.enabled = false;
                        MessagePopup.openMessageBoxWithKey("POPUP_20", null, null, mail_address);
                        break;
                    default:
                        MessagePopup.openMessageBoxWithKey("POPUP_21", null, null, mail_address);
                        break;
                }
                
            }.bind(this);

            var url = "../";
            if(CC_DEBUG)
                url = Samanda.getSamandaUrl();

            console.log(url + "mail/send_cert_mail.php?ano=" + Account.GetAccountNo() + "&email=" + mail_address);
            NetworkManager.SendRequestGet(
                url + "mail/send_cert_mail.php?ano=" + Account.GetAccountNo() + "&email=" + mail_address, onSuccess);
        }.bind(this));
    },

    onTryChangePassword() {
        console.log('onTryChangePassword');

        MessagePopup.openMessageBoxWithKey("POPUP_25", function (popup) {
            var old_editbox = popup.EditBox3.getChildByName('EditBox_1').getComponent(cc.EditBox);
            var new_editbox = popup.EditBox3.getChildByName('EditBox_2').getComponent(cc.EditBox);
            var chk_editbox = popup.EditBox3.getChildByName('EditBox_3').getComponent(cc.EditBox);
            
            if (new_editbox.string != chk_editbox.string) {
                MessagePopup.openMessageBoxWithKey("POPUP_27");
                return false;
            }

            var data = Account.MakeAccountFormData();
            data.append('pid', NetworkManager.product_type);
            data.append('old_pw', old_editbox.string);
            data.append('new_pw', new_editbox.string);

            var onSuccess = function (response) {
                console.log(response);

                var data = JSON.parse(response);
                var rs = parseInt(data["rs"]);
                switch(rs)
                {
                    case 0:
                        MessagePopup.openMessageBoxWithKey("POPUP_68");
                        break;
                    
                    case 13:
                        MessagePopup.openMessageBoxWithKey("POPUP_13");
                        break;
                    case 14:
                        MessagePopup.openMessageBoxWithKey("POPUP_14");
                        break;
                    case 15: 
                        MessagePopup.openMessageBoxWithKey("POPUP_27");
                        break;

                    default:
                        if(data.hasOwnProperty("msg"))
                            MessagePopup.openMessageBox(data["msg"]);
                        else
                            MessagePopup.openMessageBox('error : ' + rs);
                        break;
                }
                
            }.bind(this);

            var onError = function () {
                MessagePopup.openMessageBoxWithKey("POPUP_71");
            }.bind(this);

            NetworkManager.SendRequestPost(
                "auth/change_pw", data, onSuccess, onError);

            return true;
        });
    },

    onAccountLink: function (event, param) {
        var linkTargetName = '연동 오류';
        var onYes = null;

        switch (parseInt(param)) {
            case 0:
                linkTargetName = '샌드박스';
                onYes = function () {
                    cc.resources.load("Prefabs/Popup/Box_Sandbox_Sign", function (err, prefab) {
                        var Popup = cc.instantiate(prefab);
                        Popup.getComponent('SignInScript').account_ui = this;
                        
                        let canvas = MessagePopup.findCanvas ();
                        canvas.addChild(Popup);
                    });       
                }.bind(this);
                break;
            case 2:
                console.log('!!!!!!!!! google');
                linkTargetName = '구글';
                onYes = function () {
                    if (Samanda.isAndroid() || Samanda.isZFBrower() || Samanda.isIPhone()) {
                        // use android GoogleSignInClient
                        try {
                            // register callback
                            Samanda.oAUthCb = this.oAuthJavaCallback.bind(this);

                            var strJson = JSON.stringify({
                                key: 'oAuth',
                                value: eAccountType.GG
                            });

                            console.log('calling Unity cb with ' + strJson);
                            Samanda.call(strJson);
                        } catch (e) {
                            console.log('Exception thrown JSON.stringify ' + e.toString());
                            return;
                        }
                    } else {
                        // use web auth
                        //Get으로 처리시 보안문제 있으니 시간날때 호식님과 어떻게 할지 협의 할것!
                        var path = Account.AppendHttpParam("../auth/gauth_t") + "&pid=" + NetworkManager.product_type + "&link=1";
                        if (CC_DEBUG) {
                            path = Account.AppendHttpParam(Samanda.getSamandaUrl() + "auth/gauth_t") + '&pid=' + NetworkManager.debug_product_type + "&link=1";
                        }

                        this.loginEventScript.win = window.open(path, "Google oAuth", "width=500, height=500");
                        var intval_cb = this.oAuthWindowChecker.bind(this);
                        this.interval = window.setInterval(intval_cb, 500);
                    }
                }.bind(this);
                this.account_link_type = eAccountType.GG;

                MessagePopup.openMessageBoxWithKey("POPUP_22", onYes);
                return;
            case 1:
                linkTargetName = '네이버';
                onYes = function () {
                    var path = Account.AppendHttpParam("../auth/nauth_t") + "&pid=" + NetworkManager.product_type + "&link=1";
                    if (CC_DEBUG) {
                        path = Account.AppendHttpParam(Samanda.getSamandaUrl() + "auth/nauth_t") + '&pid=' + NetworkManager.debug_product_type + "&link=1";            
                    }

                    this.loginEventScript.win = window.open(path, "Naver oAuth", "width=500, height=500");
                    var intval_cb = this.oAuthWindowChecker.bind(this);
                    this.interval = window.setInterval(intval_cb, 500);
                }.bind(this);
                this.account_link_type = eAccountType.NV;

                MessagePopup.openMessageBoxWithKey("POPUP_23", onYes);
                return;
            case 6:
            {
                linkTargetName = '애플';
                onYes = function () {
                    if (Samanda.isIPhone()) {
                        try {
                            Samanda.oAUthCb = this.oAuthJavaCallback.bind(this);
    
                            var strJson = JSON.stringify({
                                key: 'oAuth',
                                value: eAccountType.AP
                            });
                            Samanda.call(strJson);    
                        } catch (e) {
                            console.log('Exception thrown JSON.stringify ' + e.toString());
                            return;
                        }
                    } else {
                    var path = Account.AppendHttpParam("../auth/aauth_t") + "&pid=" + NetworkManager.product_type + "&link=1";
                        if (CC_DEBUG) {
                            path = Account.AppendHttpParam(Samanda.getSamandaUrl() + "auth/aauth_t") + '&pid=' + NetworkManager.debug_product_type + "&link=1";            
                        }

                        this.loginEventScript.win = window.open(path, "Apple oAuth", "width=500, height=500");
                        var intval_cb = this.oAuthWindowChecker.bind(this);
                        this.interval = window.setInterval(intval_cb, 500);
                    }
                }.bind(this);
                this.account_link_type = eAccountType.AP;
                MessagePopup.openMessageBoxWithKey("POPUP_28", onYes);
                return;
            }
            default:
                console.log(param);
                MessagePopup.openMessageBoxWithKey("POPUP_72");
                return;
        }

        MessagePopup.openYesNoBox(linkTargetName + "계정에 연동하시겠습니까?", onYes);
    },

    oAuthJavaCallback: function (json) {
        Samanda.oAUthCb = null;
        try {
            var data = JSON.parse(json);
            if (false === 'rs' in data ||
                false === 'type' in data) {
                throw 'oAuthJavaCallback invalid format ' + json;
            }

            switch (parseInt(data.rs)) {
                case 0:
                    //success
                    break;

                case 1:
                    // unknown
                    throw 'android code returned rs 1';
                    break;

                case 12501:
                    // user cancelled
                    MessagePopup.openMessageBoxWithKey("POPUP_35");
                    return;

                case 12502:
                    // SIGN_IN_CURRENTLY_IN_PROGRESS - ignore
                    return;

                case 12500:
                    // SIGN_IN_FAILED
                    MessagePopup.openMessageBoxWithKey("POPUP_74");
                    return;

                default:
                    throw 'oAuthJavaCallback switched default: ' + json;
                    break;
            }
        } catch (e) {
            console.log('oAuthJavaCallback exception: ' + e.toString());
            MessagePopup.openMessageBoxWithKey("POPUP_75");
            return;
        }

        var param = new FormData();
        param.append('ano', Account.GetAccountNo());
        param.append('pid', NetworkManager.product_type);
        param.append('typ', this.account_link_type);
        param.append('tok', Account.GetUserTokenAccount());
        param.append('plt', Samanda.getBrowserType());
        param.append('id_tok', data.token);
        
        for (var pair of param.entries())
        {
            console.log("auth/link - " + pair[0] + ":" + pair[1]);
        }

        var onSuccess = function (response) {
            console.log(response);
            var data = JSON.parse(response);        
            var rs = parseInt(data["rs"]);  
            switch(rs)
            {
                case 0:
                    this.LinkPlatform[this.account_link_type].interactable = false;
                    var textNode = this.LinkPlatform[this.account_link_type].node.getChildByName('Icon_Linked');
                    if(textNode)
                    {
                        textNode.active = true;
                    }
                    MessagePopup.openMessageBoxWithKey("POPUP_66"); 
                    if(data.hasOwnProperty("ano") && data.hasOwnProperty("tok"))                      
                    {
                        console.log('changed user account no and token');

                        Account.data.PushAccountLink(this.account_link_type);
                        Account.data.user_account_no = data["ano"];
                        Account.data.user_token = data["tok"];

                        Samanda.setCookie('tok', data["tok"]);
                        Samanda.setCookie('ano', data["ano"]);
                        NetworkManager.terms_ver = 'needrefresh';
                        Account.data.user_nick_name = '#';
                        this.onEnable();
                        this.loginEventScript.onNextStep();
                    }
                    break;
                case 1:
                    MessagePopup.openMessageBoxWithKey("POPUP_80");  
                    break;
                case 2:
                    MessagePopup.openMessageBoxWithKey("POPUP_81");  
                    break;
                case 3:
                    MessagePopup.openMessageBoxWithKey("POPUP_82");  
                    break;
                case 4:
                    MessagePopup.openMessageBoxWithKey("POPUP_24");
                    break;
                case 99:
                    MessagePopup.openMessageBoxWithKey("POPUP_83");    
                    break;
                default:
                    MessagePopup.openMessageBoxWithKey("POPUP_37");    
                    break;
            }
            
            if (0 !== rs) {
                appUtils.SetGoogleAuthClear();
            }
                
        }.bind(this);
        var onError = function () {
            MessagePopup.openMessageBoxWithKey("POPUP_34");   
        }.bind(this);

        NetworkManager.SendRequestPost(
            "auth/link", param, onSuccess, onError);
    },

    oAuthWindowChecker: function () {
        try {
            var oAuthRes = Samanda.getCookie('oAuthRes');                
            if (oAuthRes)  
            {
                console.log(oAuthRes);
                
                this.loginEventScript.win.close();
                window.clearInterval(this.interval);
                this.loginEventScript.win = null;

                console.log('oAuth Popup Closed');

                if(oAuthRes == null)
                {
                    MessagePopup.openMessageBoxWithKey("POPUP_78");  
                    return;
                }

                var data = JSON.parse(oAuthRes);
                var rs = parseInt(data["rs"]);
                //this.account_link_type;
                switch(rs)
                {
                    case 0:
                        this.LinkPlatform[this.account_link_type].interactable = false;
                        var textNode = this.LinkPlatform[this.account_link_type].node.getChildByName('Icon_Linked');
                        if(textNode)
                        {
                            textNode.active = true;
                        }
                        MessagePopup.openMessageBoxWithKey("POPUP_66");      
                        if(data.hasOwnProperty("ano") && data.hasOwnProperty("tok"))                      
                        {
                            console.log('changed user account no and token');
                            Account.data.PushAccountLink(this.account_link_type);
                            Account.data.user_account_no = data["ano"];
                            Account.data.user_token = data["tok"];

                            Samanda.setCookie('tok', data["tok"]);
                            Samanda.setCookie('ano', data["ano"]);
                            NetworkManager.terms_ver = 'needrefresh';
                            Account.data.user_nick_name = '#';
                            this.onEnable();
                            this.loginEventScript.onNextStep();
                        }                 
                        break;
                    case 1:
                        MessagePopup.openMessageBoxWithKey("POPUP_80");  
                        break;
                    case 2:
                        MessagePopup.openMessageBoxWithKey("POPUP_81");  
                        break;
                    case 3:
                        MessagePopup.openMessageBoxWithKey("POPUP_82");  
                        break;
                    case 4:
                        MessagePopup.openMessageBoxWithKey("POPUP_24");
                        break;
                    case 99:
                        MessagePopup.openMessageBoxWithKey("POPUP_83");    
                        break;
                    default:
                        MessagePopup.openMessageBoxWithKey("POPUP_37");    
                        break;
                }
            }
            else
            {
                if(this.loginEventScript.win != null)
                {
                    if(this.loginEventScript.win.closed)
                    {
                        console.log('[closed] LoginEventScript : oAuthWindowChecker');

                        window.clearInterval(this.interval);          
                        this.loginEventScript.win = null;          
                        
                        this.stateChange(eLoginState.STATE_NORMAL);
                    }
                    else
                    {
                        console.log('[wait] LoginEventScript : oAuthWindowChecker');
                    }
                }
                else
                {
                    console.log('[error] LoginEventScript : oAuthWindowChecker');
                }
            }
        }
        catch (e) {
            console.log('뭔가 Error', e);
        }

        if(this.loginEventScript.win == null)
        {
            window.clearInterval(this.interval);
        }
    },

    onProfileImageSelect: function () {
        if (this.ProfileImageContainer.active)
            return;

        var callback = this.onResponseProfileList.bind(this);
        this.ProfileImageContainer.active = true;
        this.ProfileImageContainer.removeAllChildren();
        
        var url = "../";
        if(CC_DEBUG)
            url = Samanda.getSamandaUrl();
        
        NetworkManager.SendRequestGet(
            url + "me/get_profile_image_list?pid=" + NetworkManager.product_type, callback);
    },

    onResponseProfileList : function(response){
        var data = JSON.parse(response);
        var rs = parseInt(data["rs"]);
        if (rs == 0) {
            var list = data["list"];
            this.profileImageIndex = 0;
            
            list.forEach(r => {
                var RemoteSpriteNode = cc.instantiate(this.ProfileImagePrefab);                    
                RemoteSpriteNode.name = this.profileImageIndex.toString();
                RemoteSpriteNode.parent = this.ProfileImageContainer;

                this.profileImageIndex++;

                var RemoteSprite = RemoteSpriteNode.getComponent('RemoteSprite');
                //RemoteSprite.setRemoteSprite(r, null, false);
                RemoteSprite.setRemoteSprite(
                    r,
                    () => {                            
                        // render options
                        RemoteSprite.spriteFrame.getTexture().setPremultiplyAlpha(true);
                        //RemoteSprite.srcBlendFactor = cc.macro.BlendFactor.ONE;
                    },
                    false
                );                

                // render options
                // RemoteSprite.spriteFrame.getTexture().setPremultiplyAlpha(true);
                // RemoteSprite.srcBlendFactor = cc.macro.BlendFactor.ONE;
                
                let clickEventHandler = new cc.Component.EventHandler();
                clickEventHandler.target = this.node; // This node is the node to which your event handler code component belongs
                clickEventHandler.component = "AccountUI";// This is the code file name
                clickEventHandler.handler = "onBtnProfileItem";
                clickEventHandler.customEventData = RemoteSprite;

                RemoteSpriteNode.getChildByName('ProfileSelectBtn').getComponent(cc.Button).clickEvents.push(clickEventHandler);
                
                if(r == Account.GetUserProfilePath())
                {
                    var cursor = RemoteSpriteNode.getChildByName('Cursor_Select');
                    console.log(cursor);
                    if(cursor)
                    {
                        cursor.active = true;
                    }
                }
            });
        }
    },

    onBtnProfileItem : function(event, param)
    {
        var url = "../";
        if(CC_DEBUG)
            url = Samanda.getSamandaUrl();

        var callback = function(){
            this.setProfileSpriteFrame(param.spriteFrame);
            Account.ChangeProfile(param.texture_url);            
        }.bind(this);

        this.ProfileImageContainer.active = false;
        console.log(url + "me/set_profile_image?ano=" + Account.GetAccountNo() + "&image_idx=" + param.node.name + "&pid=" + NetworkManager.product_type);

        NetworkManager.SendRequestGet(
            url + "me/set_profile_image?ano=" + Account.GetAccountNo() + "&image_idx=" + param.node.name + "&pid=" + NetworkManager.product_type, callback);
    },

    onProfileImage : function(err, res){
        console.log('onProfile Image');
        
        if(res)
        {
            this.setProfileSpriteFrame(new cc.SpriteFrame(res));
        }
    },

    setProfileSpriteFrame : function(frame)
    {
        console.log(frame);
        this.CurProfileImage.spriteFrame = frame;
    },

    onVersionShowBtn : function()
    {
        if(this.VersionHideCount <= 0)
        {
            this.VersionHide.active = false;
        }
        else
        {
            this.VersionHideCount--;
        }
    }
});


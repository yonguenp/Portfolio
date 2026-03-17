
var NetworkManager = require("NetworkManager");
var StdResHandler = require("ResponseHandler");
var MessagePopup = require("MessagePopup");
var Samanda = require("Samanda");
var SCLogger = require("ScreenLogger");
var Account = require("AccountManager");

var eLoginState;
eLoginState = cc.Enum({
  STATE_NORMAL : 0,
  STATE_LOGIN_BOX : 1,
  STATE_SIGNIN_BOX : 2,    
  STATE_NEXT_SCENE : 3,
  STATE_TERMSOFUSE : 4,
  STATE_NICK_SET_BOX : 5,
  STATE_GOOGLE_SIGNIN : 6,
  STATE_OTHER_SIGNIN : 7,
  STATE_BANNER_BOX : 8,
  STATE_MAIN_BOX : 9,
});

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
        LoginState : {
            default: eLoginState.STATE_NORMAL,
            type: eLoginState
        },
        
        //for zoom Ratio contorl
        camera : cc.Camera,
        DebugOptionFlag : 0,
        DebugProductID : '',

        BoxPrefab : [cc.Prefab],
        LoadedBox : [cc.Node],

        main_box : cc.Prefab,
        main_box_vertical : cc.Prefab,
        banner_box : cc.Prefab,
        banner_box_vertical : cc.Prefab,
    },

    onLoad () {
        console.log('onLoad : loginEventScript');
        var canvasNode = Samanda.findCanvas();
        var canvas = null;
        
        if(canvasNode)
        {
            canvas = canvasNode.getComponent(cc.Canvas);
        }

        if(CC_DEBUG)
        {
            console.log('use debug optionflag : ' + this.DebugOptionFlag);
            Samanda._optionflag = parseInt(this.DebugOptionFlag); 
            if(this.DebugProductID)
            {
                NetworkManager.debug_product_type = this.DebugProductID;
                NetworkManager.product_type = this.DebugProductID;
            }
        }

        var orientationType = Samanda.getOrientationType();
        console.log('cur Orientation : ' + orientationType);
        //console.log('Samanda._optionflag :' + Samanda._optionflag);
        //console.log('Samanda.samanda_option.option_enable_chat :' + Samanda.samanda_option.option_enable_chat);
        console.log('enable chat : ' + Samanda.isEnableUseChat());
        switch(orientationType)
        {
            case 1:     
                this.BoxPrefab[eLoginState.STATE_MAIN_BOX] = this.main_box;
                this.BoxPrefab[eLoginState.STATE_BANNER_BOX] = this.banner_box;
                if(canvas)
                {
                    canvas.fitHeight = true;
                    canvas.fitWidth = true;
                    canvas.designResolution = cc.size(1280, 720);
                    this.camera.node.x = 640;
                    this.camera.node.y = 360;
                }       
                break;
            case 0:
                this.BoxPrefab[eLoginState.STATE_MAIN_BOX] = this.main_box_vertical;
                this.BoxPrefab[eLoginState.STATE_BANNER_BOX] = this.banner_box_vertical;
                if(canvas)
                {
                    canvas.fitHeight = true;
                    canvas.fitWidth = true;
                    canvas.designResolution = cc.size(720, 1280);
                    this.camera.node.x = 360;
                    this.camera.node.y = 640;
                }  
                break;
            default:
                break;
        }

        if(require("PopupTemplate").self == null)
        {
            cc.resources.load("Data/Popup", function (err, file) {            
                require("PopupTemplate").Init(file.text);
            });
        }
    },

    stateChange : function(state)
    {
        if(this.LoginState == eLoginState.LoginState && state == eLoginState.LoginState)
        {
            return;
        }

        if(state == eLoginState.STATE_MAIN_BOX)
        {
            if(Samanda.isYGProduct())
            {
                cc.director.loadScene("YoutubePlayerGaro.fire");
                return;
            }
        }
        
        console.log('state : ' + state);

        this.LoginState = state;
        
        this.LoadedBox.forEach(box => {
            if(box)
                box.active = false;
        });

		if(state == eLoginState.STATE_NORMAL)
        {
            if(NetworkManager.product_type == 'ktymtC')
            {
                var data = new FormData();
                data.append('pid', NetworkManager.product_type);
                data.append('typ', 7);
                data.append('plt', Samanda.getBrowserType());

                var res_callback = function(response){                
                    var scn = cc.director.getScene();
                    var eventNode = scn.getChildByName('EventScript');
                    if (null === eventNode) {                
                        eventNode = cc.find('EventScript');
                    }
                    
                    if(eventNode != null)
                    {
                        var LoginEventScript = eventNode.getComponent('LoginEventScript');
                        var data = JSON.parse(response);
                        StdResHandler(response);
                        LoginEventScript.onLoginWithTokken(data);
                    }
                }.bind(this);
                
                var onError = function() {
                    MessagePopup.openMessageBoxWithKey("POPUP_60");    
                };
                NetworkManager.SendRequestPost(
                    "auth/signup", data, res_callback, onError);

                return;
            }
        }
		
        if(this.LoadedBox[state] == null)
        {
            if(this.BoxPrefab[state] != null)
            {
                this.LoadedBox[state] = cc.instantiate(this.BoxPrefab[state]);
                var canvasNode = Samanda.findCanvas();
                canvasNode.addChild(this.LoadedBox[state]);
            }
        }

        if(this.LoadedBox[state])
        {
            this.LoadedBox[state].active = true;
        }
        
        switch(state)
        {
            case eLoginState.STATE_NORMAL:       
                this.LoadedBox[state].getComponent('LoginBtns').setLoginEventScript(this);                
                break;     
            case eLoginState.STATE_SIGNIN_BOX: 
                this.LoadedBox[state].getComponent('SignInScript').setLoginEventScript(this);
                break;           
            case eLoginState.STATE_LOGIN_BOX:    
                this.LoadedBox[state].getComponent('LoginSandboxAccount').setLoginEventScript(this);
                break;                        
            case eLoginState.STATE_TERMSOFUSE:
                this.LoadedBox[state].getComponent('LoginTermsofuse').setLoginEventScript(this);
                break;
            case eLoginState.STATE_NICK_SET_BOX:
                this.LoadedBox[state].getComponent('NickNameCreate').setLoginEventScript(this);
                break;
            case eLoginState.STATE_BANNER_BOX:
                this.LoadedBox[state].getComponent('LoginBanner').setLoginEventScript(this);
                break;
            case eLoginState.STATE_MAIN_BOX:
                this.LoadedBox[state].getComponent('MainPageScript').setLoginEventScript(this);
                break;
            case eLoginState.STATE_NEXT_SCENE:                        
                break;
            default:
                break;
        }        
    },

    stateNext : function(state)
    {
        this.prevState.push(this.LoginState);
        this.stateChange(state);
    },

    statePrev : function()
    {
        if(this.prevState.length <= 0)
            return false;

        if(this.LoginState == eLoginState.STATE_TERMSOFUSE || this.LoginState == eLoginState.STATE_NICK_SET_BOX)
            return false;

        var state = this.prevState.pop();
        this.stateChange(state);

        return true;
    },

    start () {
        this.prevState = [eLoginState];  

        var passLogin = NetworkManager.IsValidAccount();
        if(passLogin == false)
        {
            if(Account.GetAccountNo() != null)
            {
                var accountNo = Account.GetAccountNo();
                var guestFlag = 1 << 52;
                if(accountNo > guestFlag)
                {
                    passLogin = true;
                }
            }
        }

        if(passLogin)
        {
            if(Account.GetAccountNo() == null)
            {
                this.tryLoginWithSessionToken();
            }
            else 
            {
                this.onNextStep();
            }
        }
        else
        {
            this.stateChange(eLoginState.STATE_NORMAL);
        }

        if(0 && !Samanda.isStandAlone()) {
            SCLogger.init();
        }
    },

    openLoginBox : function()
    {
        this.stateNext(eLoginState.STATE_LOGIN_BOX);
    },
    
    offLoginBox : function()
    {
        this.statePrev();
    },

    openSigninBox : function()
    {
        this.stateNext(eLoginState.STATE_SIGNIN_BOX);        
    },
    
    offSigninBox : function()
    {
        this.statePrev();     
    },

    openTermsofuseBox : function()
    {
        this.stateNext(eLoginState.STATE_TERMSOFUSE);
        require('AppUtils').SetAppData('Termsofuse', 'show');        
    },
    
    openNickSetBox : function()
    {
        this.stateNext(eLoginState.STATE_NICK_SET_BOX);        
        require('AppUtils').SetAppData('NickSetBox', 'show');        
    },
    
    offNickSetBox : function()
    {
        this.statePrev();     
    },

    openBannerBox : function()
    {
        this.stateNext(eLoginState.STATE_BANNER_BOX);
    },

    offBannerBox : function()
    {
        this.onNextStep();
    },

    openMainBox : function()
    {
        this.stateNext(eLoginState.STATE_MAIN_BOX);
    },

    offMainBox : function()
    {
        //this.statePrev();
        this.onNextStep();
    },

    tryLoginWithToken : function()
    {
        var data = new FormData();
        data.append('pid', NetworkManager.product_type);
        Account.AppendFormData(data);
        
        var onSuccess = this.onResponseLogin.bind(this);
        var onError = this.onResponseLoginFail.bind(this);

        NetworkManager.SendRequestPost(
            "auth/signin", data, onSuccess, onError);
        
        this.stateNext(eLoginState.STATE_NEXT_SCENE);   
    },

    tryLoginWithSessionToken : function()
    {
        console.log('tryLoginWithSessionToken : ' + Account.GetAccountNo() + "," + Account.GetSessionTokenAccount());

        var data = new FormData();
        data.append('pid', NetworkManager.product_type);
        data.append('tok', Account.GetSessionTokenAccount());
        data.append('ano', Account.data.user_account_no);

        var onSuccess = this.onResponseLogin.bind(this);
        var onError = this.onResponseLoginFail.bind(this);

        NetworkManager.SendRequestPost(
            "auth/signin", data, onSuccess, onError);
        
        this.stateNext(eLoginState.STATE_NEXT_SCENE);  
    },

    onLoginWithGoogle : function()
    {
        console.log('onLogin with google');

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
            var path = "../auth/gauth_t?pid=" + NetworkManager.product_type;
            if (CC_DEBUG) {
                path = Samanda.getSamandaUrl() + 'auth/gauth_t?pid=' + NetworkManager.debug_product_type;            
            }
    
            this.win = window.open(path, "Google oAuth", "width=500, height=500");
            var intval_cb = this.oAuthWindowChecker.bind(this);
            this.interval = window.setInterval(intval_cb, 500);
        }

        this.stateNext(eLoginState.STATE_GOOGLE_SIGNIN);
    },

    onLoginWithNaver : function()
    {
        console.log('onLogin with naver');
        var path = "../auth/nauth_t?pid=" + NetworkManager.product_type;
        if (CC_DEBUG) {
            path = Samanda.getSamandaUrl() + 'auth/nauth_t?pid=' + NetworkManager.debug_product_type;            
        }

        this.win = window.open(path , "Naver oAuth", "width=500, height=500");
        var intval_cb = this.oAuthWindowChecker.bind(this);
        this.interval = window.setInterval(intval_cb, 500);

        this.stateNext(eLoginState.STATE_OTHER_SIGNIN);        
    },

    onLoginWithApple : function()
    {
        if (Samanda.isIPhone()) {
            // register callback
            Samanda.oAUthCb = this.signInWithAppleCallback.bind(this);

            var strJson = JSON.stringify({
                key: 'oAuth',
                value: eAccountType.AP
            });
            
            console.log('calling Unity cb with ' + strJson);
            Samanda.call(strJson);
        } else {
            console.log('onLogin with apple');
            var path = "../auth/aauth_t?pid=" + NetworkManager.product_type;
            if (CC_DEBUG) {
                path = Samanda.getSamandaUrl() + 'auth/aauth_t?pid=' + NetworkManager.debug_product_type;            
            }
            
            this.win = window.open(path , "Apple oAuth", "width=500, height=500");
            var intval_cb = this.oAuthWindowChecker.bind(this);
            this.interval = window.setInterval(intval_cb, 500);
        }

        this.stateNext(eLoginState.STATE_OTHER_SIGNIN); 
    },

    onLoginGuest : function()
    {
        MessagePopup.openMessageBoxWithKey("POPUP_7", function (popup) {
            var data = new FormData();
            data.append('pid', NetworkManager.product_type);
            data.append('typ', 7);
            data.append('plt', Samanda.getBrowserType());

            var res_callback = function(response){                
                var scn = cc.director.getScene();
                var eventNode = scn.getChildByName('EventScript');
                if (null === eventNode) {                
                    eventNode = cc.find('EventScript');
                }
                
                if(eventNode != null)
                {
                    var LoginEventScript = eventNode.getComponent('LoginEventScript');
                    var data = JSON.parse(response);
                    StdResHandler(response);    
                    LoginEventScript.onLoginWithTokken(data);
                }
            }.bind(this);
            
            var onError = function() {
                MessagePopup.openMessageBoxWithKey("POPUP_60");    
            };
            NetworkManager.SendRequestPost(
                "auth/signup", data, res_callback, onError);
        });        
    },

    onLoginWithTokken : function(data)
    {        
        NetworkManager.onResponseUserInfo(data);      
        Samanda.setCookie('tok', data.tok);
        Samanda.setCookie('ano', data.ano);

        this.stateNext(eLoginState.STATE_NEXT_SCENE);   
        this.onNextStep();
    },

    oAuthJavaCallback : function(json)
    {
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
                    this.statePrev();
                    return;

                case 12502:
                    // SIGN_IN_CURRENTLY_IN_PROGRESS - ignore
                    return;

                case 12500:
                    // SIGN_IN_FAILED
                    MessagePopup.openMessageBoxWithKey("POPUP_36");
                    this.statePrev();
                    return;;

                default:
                    throw 'oAuthJavaCallback switched default: ' + json;
                    break;
            }
        } catch(e) {
            console.log('oAuthJavaCallback exception: ' + e.toString());
            MessagePopup.openMessageBoxWithKey("POPUP_37");
            this.statePrev();
            return;
        }

        if (0 !== data.rs) {
            require('AppUtils').SetGoogleAuthClear();
        }
        
        // try signin with id token
        this.id_token = data.token;

        var param = new FormData();
        param.append('pid', NetworkManager.product_type);
        param.append('typ', eAccountType.GG);
        param.append('tok', data.token);
        param.append('plt', Samanda.getBrowserType());
        
        var onSuccess = this.onResponseGoogleSignIn.bind(this);
        var onError = this.onResponseLoginFail.bind(this);

        NetworkManager.SendRequestPost(
            "auth/signin", param, onSuccess, onError);
    },

    signInWithAppleCallback : function(json)
    {
        Samanda.oAUthCb = null;
        try {
            var data = JSON.parse(json);
            if (false === 'rs' in data) {
                throw 'oAuthJavaCallback invalid format ' + json;
            }

            switch (parseInt(data.rs)) {
                case 0:
                    //success
                    break;
                
                case 1001:
                    // user cancelled
                    MessagePopup.openMessageBoxWithKey("POPUP_35");
                    this.statePrev();
                    return;

                case 12502:
                    // SIGN_IN_CURRENTLY_IN_PROGRESS - ignore
                    return;

                case 1004:
                    // SIGN_IN_FAILED
                    MessagePopup.openMessageBoxWithKey("POPUP_36");
                    this.statePrev();
                    return;

                case 1:
                case 1000:
                default:
                    // UNKNOWN
                    MessagePopup.openMessageBoxWithKey("POPUP_37");
                    this.statePrev();
                    return;
            }
        } catch(e) {
            console.log('oAuthJavaCallback exception: ' + e.toString());
            MessagePopup.openMessageBoxWithKey("POPUP_37");
            this.statePrev();
            return;
        }

        if (0 !== data.rs) {
            //require('AppUtils').SetGoogleAuthClear();
        }
        
        // try signin with id token
        this.id_token = data.token;

        var param = new FormData();
        param.append('pid', NetworkManager.product_type);
        param.append('typ', eAccountType.AP);
        param.append('tok', data.token);
        param.append('plt', Samanda.getBrowserType());
        
        var onSuccess = this.onResponseAppleSignIn.bind(this);
        var onError = this.onResponseLoginFail.bind(this);

        NetworkManager.SendRequestPost(
            "auth/signin", param, onSuccess, onError);
    },

    oAuthWindowChecker : function()
    {
        try {
            var oAuthRes = Samanda.getCookie('oAuthRes');                
            if (oAuthRes && this.win != null)  
            {
                console.log(oAuthRes);
                
                this.win.close();
                this.statePrev();
                window.clearInterval(this.interval);
                this.win = null;

                console.log('oAuth Popup Closed');

                var data = JSON.parse(oAuthRes);
                var rs = parseInt(data["rs"]);
                var type = parseInt(data["typ"]);
                var tok = data["tok"];
                switch(rs)
                {
                    case 0 :
                        StdResHandler(oAuthRes);
                        this.onLoginWithTokken(data);
                        break;
                    case 1 :
                        //StdResHandler(oAuthRes);
                        this.onSignOtherPlatform(type, tok, null);                        
                        break;
                    case 2 :
                    case 99 :
                    default :
                        MessagePopup.openMessageBox("인증 중 오류가 발생했습니다."); 
                        break;
                }
            }
            else
            {
                if(this.win != null)
                {
                    if(this.win.closed)
                    {
                        console.log('[closed] LoginEventScript : oAuthWindowChecker');

                        window.clearInterval(this.interval);          
                        this.win = null;          
                        
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

        if(this.win == null)
        {
            window.clearInterval(this.interval);
        }
    },
    
    onSignOtherPlatform : function(type, ac_token, id_token)
    {
        console.log('onSignOtherPlatform : ' + NetworkManager.product_type + ',' + type + ',' + ac_token + ',' + id_token);
        
        var data = new FormData();
        data.append('pid', NetworkManager.product_type);
        data.append('typ', type);
        data.append('plt', Samanda.getBrowserType());

        if (ac_token) {
            data.append('tok', ac_token);
        }
        if (id_token) {
            data.append('id_tok', id_token);
        }
          
        var onSuccess = this.onResponseSignupOtherPlatform.bind(this);
        var onError = this.onResponseLoginFail.bind(this);

        NetworkManager.SendRequestPost(
            "auth/signup", data, onSuccess, onError);
    },
    
    onResponseSignupOtherPlatform : function(response)
    {
        console.log('onResponseSignupOtherPlatform');
        var data = JSON.parse(response);        
        var rs = parseInt(data["rs"]);  
        
        if(rs == 0)
        {
            StdResHandler(response);
            this.onLoginWithTokken(data);
        }
        else
        {           
            console.log(response);
            MessagePopup.openMessageBox(data["msg"], this.statePrev.bind(this));     
        }
    },

    onResponseLogin : function(response)
    {
        console.log(response);

        var data = JSON.parse(response);
        var rs = parseInt(data["rs"]);
        
        switch(rs)
        {
            case 0 :
                NetworkManager.onResponseUserInfo(data);                
                Samanda.setCookie('tok', data.tok);
                Samanda.setCookie('ano', data.ano);

                StdResHandler(data);
                this.onNextStep();
                break;
            case 1 :
                MessagePopup.openMessageBoxWithKey("POPUP_1");
                Samanda.clearCookies();
                this.statePrev(); 
                break;
            default:
                this.onResponseLoginFail();
                Samanda.clearCookies();
                break;
        }
    },

    onResponseLoginFail : function()
    {
        MessagePopup.openMessageBoxWithKey("POPUP_37");         
        this.statePrev();
    },

    // 매우 좋지 않은 코드 중복
    onResponseGoogleSignIn : function(response)
    {
        var data = JSON.parse(response);
        var rs = parseInt(data["rs"]);
        switch(rs) {
            case 0:
                StdResHandler(response);
                this.onResponseLogin(response);
                break;
            
            case 1:
                // google signin은 성공했으며 아직 계정 미생성 상태
                this.onSignOtherPlatform(eAccountType.GG, null, this.id_token);
                break;

            default:
                this.onResponseLoginFail();
                break;
        }
    },

    // 매우 좋지 않은 코드 중복
    onResponseAppleSignIn : function(response)
    {
        var data = JSON.parse(response);
        var rs = parseInt(data["rs"]);
        switch(rs) {
            case 0:
                StdResHandler(response);
                this.onResponseLogin(response);
                break;
            
            case 1:
                // google signin은 성공했으며 아직 계정 미생성 상태
                this.onSignOtherPlatform(eAccountType.AP, null, this.id_token);
                break;

            default:
                this.onResponseLoginFail();
                break;
        }
    },

    onResponseTerms : function(response)
    {
        console.log('onResponseTerms');
        console.log(response);
        
        var data = JSON.parse(response);
        var rs = parseInt(data["rs"]);

        switch (rs) {
            case 0:
                NetworkManager.onTermsAgreed();
                break;
            case 99:
                MessagePopup.openMessageBoxWithKey("POPUP_41");
                break;
            case 1:
            default:
                MessagePopup.openMessageBox(data["msg"]);
                break;
        }

        this.onNextStep();
    },
    
    onResponseNick : function(response)
    {
        console.log('onResponseNick');
        console.log(response);
        try {
            var data = JSON.parse(response);
            var rs = parseInt(data["rs"]);

            switch (rs) {
                case 0:
                    Account.ChangeNickName(data["new"]);
                    break;
                case 21:
                    MessagePopup.openMessageBoxWithKey("POPUP_15");
                    break;
                case 22:
                    MessagePopup.openMessageBoxWithKey("POPUP_16");
                    break;
                case 23:
                    MessagePopup.openMessageBoxWithKey("POPUP_17");
                    break;
                case 24:
                    MessagePopup.openMessageBoxWithKey("Nick_Msg05", null, null, "[" + data["msg"] + "]");
                    break;
                case 98:
                    MessagePopup.openMessageBoxWithKey("POPUP_45");
                    break;
                case 99:
                default:
                    throw 'beep';
                    break;
            }
        } catch (e) {
            MessagePopup.openMessageBoxWithKey("POPUP_41");
        }

        if(this.LoadedBox[eLoginState.STATE_MAIN_BOX] != null)
        {
            var MainPageScript = this.LoadedBox[eLoginState.STATE_MAIN_BOX].getComponent('MainPageScript');
            if(MainPageScript)
            {
                var AccountUI = MainPageScript.account_page.getComponent('AccountUI');
                if(AccountUI)
                {
                    AccountUI.onEnable();
                }
            }
        }
        
        this.onNextStep();
    },

    onNextStep : function()
    {
        /**
         * pre-hooks
         */

         // 약관 동의 필요한지
        if("" !== NetworkManager.terms_ver)
        {
            this.openTermsofuseBox();
            return;
        }

        // 닉네임 미정 상태
        if(Account.NeedMakeNickName())
        {
            console.log('openNickSetBox');
            this.openNickSetBox();
            return;
        }

        // banners
        if (false === Samanda.bannerShown)
        {
            if(!Samanda.isStandAlone())
            {
                require("Samanda").setWebState(4);
            }

            if(Samanda.hasBanner())
            {
                Samanda.bannerShown = true;
                this.setBannerUI(0);
                return;
            }
        }

        //강제업데이트
        if(false)
        {
            var callback = function(){
                if(require("Samanda").isIPhone())
                    require('AppUtils').SetAppData('url', 'https://sbsb.kr/ffcat_ts_ios_sb');                        
                else
                    require('AppUtils').SetAppData('url', 'https://play.google.com/store/apps/details?id=com.sandboxgame.fishfarmcat');
            };

            require("MessagePopup").openMessageBoxWithKey("NEED_APP_UPDATE", callback);
            return;
        }

        if(!Samanda.isStandAlone())
        {
            require("Samanda").setWebState(5);
        }

        // 공지사항
        if (this.LoginState != eLoginState.STATE_MAIN_BOX)
        {
            this.openMainBox();
            Samanda.notiShown = true;            
            return;
        }

        /**
         * ENDOF pre-hooks
         */

        if(NetworkManager.product_type == 1)
        {
            console.log('this product for Notion!');
            cc.director.loadScene("GameList.fire");
        }
        else if(NetworkManager.product_type == 2)
        {
            console.log('this product for Pilsung!');
            cc.director.loadScene("EndLoveDartsGame.fire");
        }
        else if(!Samanda.isStandAlone())
        {
            require("Samanda").setWebState(6);
        }
        else
        {
            cc.director.loadScene("Main.fire");
        }
    },

    onHasNotEvent : function()
    {
        MessagePopup.openMessageBox("미구현");  
    },

    reloadLoginScene : function()
    {
        console.log("reloadLoginScene");
        this.start();
    },

    // banner box ui setup
    setBannerUI : function(idx)
    {
        var lang = Samanda.getLanguageCode();
        Samanda.banner_up = false;        
        
        var banners = new Array();
        Samanda.banners.forEach(element => {
            if(element.language == lang)
                banners.push(element);
        });

        if (banners.length <= idx) {
            if(this.LoadedBox[eLoginState.STATE_BANNER_BOX])
            {
                this.LoadedBox[eLoginState.STATE_BANNER_BOX].getComponent('LoginBanner').idx = 0;
            }    

            this.offBannerBox();
            return;
        }

        var data = banners[idx];
        // is banner dnd?
        if (Samanda.isBannerDnd(data.bid)) {
            return this.setBannerUI(idx + 1);
        }

        Samanda.banner_up = true;

        if (eLoginState.STATE_BANNER_BOX !== this.LoginState) {
            this.openBannerBox();
        }

        var url = NetworkManager.post_url + 'banners?' + data.uri;

        try
        {
            if(this.LoadedBox[eLoginState.STATE_BANNER_BOX])
            {
                var LoginBanner = this.LoadedBox[eLoginState.STATE_BANNER_BOX].getComponent('LoginBanner');
                if(LoginBanner)
                {
                    LoginBanner.idx = idx;
                    // reset toggle
                    LoginBanner.banner_toggle.isChecked = false;            
                    // setup url                            
                    LoginBanner.banner_webview.url = url;
                }
            }
            else
            {
                console.log('not found loaded banner object');
            }            
        } catch(e) {
            console.log('errror on url setting ' + e.toString());
            this.setBannerUI(idx + 1);
        }
    },

    onBtnFindID : function()
    {
        MessagePopup.openMessageBoxWithKey("POPUP_2", function (popup) {
            var edit_box = popup.EditBox1.getChildByName('EditBox_1').getComponent(cc.EditBox);
            var mail_address = edit_box.string;

            if (0 > mail_address.indexOf('@')) {
                MessagePopup.openMessageBoxWithKey("POPUP_3");
                return false;
            }

            var onSuccess = function (response) {
                console.log(response);

                var data = JSON.parse(response);
                var rs = parseInt(data["rs"]);
                switch(rs)
                {
                    case 0:
                        MessagePopup.openMessageBoxWithKey("POPUP_5", null, null, mail_address);
                        break;
                    case 1:
                        MessagePopup.openMessageBoxWithKey("POPUP_4");
                        break;
                    case 2:
                        MessagePopup.openMessageBoxWithKey("POPUP_4");
                        break;
                    default:                            
                        MessagePopup.openMessageBox('error : ' + rs);
                        break;
                }
                
            }.bind(this);

            var url = "../";
            if(CC_DEBUG)
                url = Samanda.getSamandaUrl();

            NetworkManager.SendRequestGet(
                url + "auth/find_ID?email=" + mail_address, onSuccess);

            return true;
        });
    },

    onBtnFindPW : function()
    {
        MessagePopup.openMessageBoxWithKey("POPUP_6", function (popup) {
            var Email = popup.EditBox2.getChildByName('EditBox_1').getComponent(cc.EditBox);
            var ID = popup.EditBox2.getChildByName('EditBox_2').getComponent(cc.EditBox);
            
            var mail_address = Email.string;
            var id = ID.string;

            if (0 > mail_address.indexOf('@')) {
                MessagePopup.openMessageBoxWithKey("POPUP_3");
                return false;
            }

            var onSuccess = function (response) {
                var data = JSON.parse(response);
                var rs = parseInt(data["rs"]);
                switch(rs)
                {
                    case 0:
                        MessagePopup.openMessageBoxWithKey("POPUP_5", null, null, mail_address);
                        break;
                    case 2:
                        MessagePopup.openMessageBoxWithKey("POPUP_9");
                        break;
                    case 3:
                        MessagePopup.openMessageBoxWithKey("POPUP_8");
                        break;
                    default:
                        MessagePopup.openMessageBox('error : ' + rs);
                        break;
                }
                
            }.bind(this);

            var url = "../";
            if(CC_DEBUG)
                url = Samanda.getSamandaUrl();

            NetworkManager.SendRequestGet(
                url + "auth/find_PW?email=" + mail_address + "&id=" + id, onSuccess);

            return true;
        });
    },

    isStateMainBox()
    {
        return this.LoginState == eLoginState.STATE_MAIN_BOX;
    },

    getMainPageScript()
    {
        var boxNode = this.LoadedBox[eLoginState.STATE_MAIN_BOX];
        if(boxNode != null)
        {
            return boxNode.getComponent('MainPageScript');
        }

        return null;
    }
});

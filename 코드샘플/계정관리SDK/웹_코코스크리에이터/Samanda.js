/**
 * SAMANDA data manager
 * 
 * usage : var Samanda = require('Samanda')();
 */

var CKU = require('CookieUtils');
var Toast = require("ToastScript");
var WSM = require('WebsocketManager');
var Popup = require("MessagePopup");
var CHAT = require('ChatManager');

const SamandaProto = class
{
    static self = null;

    _notifications = Array();
    _banners = Array();
    _banner_shown = false;
    _banner_up = false;
    _noti_shown = false;  
    _browserType = 0;
    _samanda_page = 0;
    _optionflag = Number.MAX_SAFE_INTEGER;
    _languageCode = 10;//en
    Toast = null;
    Cookie = null;
    WsManager = null;

    keyboardHeight = 0;

    eMainPage = cc.Enum({
        PAGE_MAIN : 0,
        PAGE_NOTICE : 1,
        PAGE_CHAT : 2, 
        PAGE_OVERLAY_CHAT : 3,
        SCREENSHOT_SEND : 4,
        PAGE_ACCOUNT : 5,
    });

    samanda_option = {
        option_enable_chat : 1, //chat 1 : use 0 : not use
        option_orientation : 2, //orientation 1 : landscape 0 : portrait
        option_browser_pre : 4, //prefix browser type : 0 is standalone 1 is app base
        option_browser_suf : 8, //suffix browser type : 0 is mobile 1 is zfb(PC) 
        option_overlaychat : 16, //customoverlay 1 default web 0
    };

    static getInstance() {
        if (null === SamandaProto.self && !SamandaProto.init()) {
            return null;
        }
        
        return SamandaProto.self;
    }

    static init() {
        console.log('hello SAMANDA');
        if (SamandaProto.self) {
            return true;
        }
        var self = new SamandaProto();
        SamandaProto.self = self;
        if (!self) {
            return false;
        }

        self.Toast = Toast;
        self.Cookie = CKU;
        self.WsManager = WSM;
        try
        {
            if(typeof languageCode == 'undefined' || !languageCode)
            {
                self._languageCode = 10;//EN                
            }        
            else
            {
                self._languageCode = parseInt(languageCode);
            }    
        }
        catch
        {
            //setDefaultOption();
            //console.log('[error] Not Found Option Pram!! set full Option..');
            //self._optionflag = Number.MAX_SAFE_INTEGER;
        }
        
        try
        {
            if(typeof optionflag == 'undefined' || !optionflag)
            {
                console.log('Not Found Option Pram!! set default Option..');
                self._optionflag = Number.MAX_SAFE_INTEGER;
                self._optionflag &= ~self.samanda_option.option_browser_pre;
                self._optionflag &= ~self.samanda_option.option_browser_suf;
            }        
            else
            {
                self._optionflag = parseInt(optionflag);
            }    
        }
        catch
        {
            //setDefaultOption();
            //console.log('[error] Not Found Option Pram!! set full Option..');
            //self._optionflag = Number.MAX_SAFE_INTEGER;
        }

        //이전버전을 위하여
        self._browserType = (self._optionflag & self.samanda_option.option_browser_pre ? 
            self._optionflag & self.samanda_option.option_browser_suf ? 2 : 1 
            : 0);

        try{
            //todo : 옵션플레그 집어치우고 직관적으로 접속 Browser Type 세팅으로 변경하자.
            console.log('browser type set try : ' + browserType);
            if(typeof browserType == 'undefined' || !browserType)
            {
                
            }  
            else
            {
                var type = parseInt(browserType);
                self._browserType = type;
                console.log('browser type is : ' + type);
            }
        }
        catch
        {
            
        }        

        try
        {
            if(typeof samanda_orientation == 'undefined' || !samanda_orientation)
            {
                
            }
            else
            {
                if(parseInt(samanda_orientation) > 0)
                {
                    self._optionflag |= self.samanda_option.option_orientation;
                    console.log('orientation type reset : LANDSCAPE');
                }
                else
                {
                    self._optionflag &= ~self.samanda_option.option_orientation;
                    console.log('orientation type reset : PORTRAIT');
                }                
            }    
        }
        catch
        {

        }

        console.log('SAMANDA OPTION : ' 
                + (self._browserType == 0 ? 'STAND ALONE, ' :
                self._browserType == 1 ? 'ANDROID, ' :
                self._browserType == 2 ? 'PC, ' :
                self._browserType == 3 ? 'IOS, ' : 'UNKOWN')
                + (self._optionflag & self.samanda_option.option_orientation ? 'LANDSCAPE, ' : 'PORTRAIT, ') 
                + (self._optionflag & self.samanda_option.option_overlaychat ? 'CUSTOM OVERLAY CHAT, ' : 'WEB OVERALY CHAT, ')
                + 'END');

        delete SamandaProto.prototype.constructor;
        window.Samanda = self;
        
        return true;
    }

    // noti & banner
    get banners() {
        return this._banners;
    }

    set banners(arr) {
        this._banners = arr;
    }

    get bannerShown() {
        return this._banner_shown;
    }

    set bannerShown(val) {
        this._banner_shown = val;
    }

    hasBanner() {
        return (0 < this._banners.length);
    }

    set banner_up(up) {
        this._banner_up = up;
    }

    get banner_up() {
        return this._banner_up;
    }

    get notifications() {
        return this._notifications;
    }

    set notifications(arr) {
        this._notifications = arr;
    }

    get notiShown() {
        return this._noti_shown;
    }

    set notiShown(val) {
        this._noti_shown = val;
    }

    get samandaPage() {
        return this._samanda_page;
    }

    set samandaPage(val) {
        this._samanda_page = val;
    }

    hasNotification() {
        return (0 < this._notifications.length);
    }

    getSamandaUrl() {
        if (CC_DEBUG) {
            return "http://sandbox-gs.mynetgear.com/";
        }
        else
        {
            return "https://samanda.sandbox-gs.com/";
        }
    }


    onResNotiBannerList(data) {
        if (false === 'list' in data) {
            console.log("false === 'list' in data");
            console.log(data);
            return;
        }
        
        var op = parseInt(data.op);
        switch (op) {
            case 1: {
                // 공지사항일 때
                let tmp = Array();
                for (let i = 0; data.list.length > i; ++i) {
                    let title = data.list[i].title;
                    let uri = data.list[i].uri;
                    let time = data.list[i].time;
                    let language = data.list[i].lan;

                    if (title && uri) {
                        tmp.push({ title: title, uri: uri, time: time, language: language });
                    }
                }
                
                this.notifications = tmp;                
                break;
            }
            case 2: {
                // 배너 목록일 때
                let tmp = Array();
                for (let i = 0; data.list.length > i; ++i) {                        
                    let bid = data.list[i].b_id;
                    let uri = data.list[i].uri;
                    let language = data.list[i].lan;

                    if (bid && uri) {
                        tmp.push({ bid: bid, uri: uri, language: language });
                    }
                }

                this.banners = tmp;
                break;
            }
            default:
                console.log("1 !== op && 2 !== op");
                console.log(data);
                return;
                break;
        }
        
        // TODO::list scene on noti changed
    }

    isNotificationDnd() {
        var list = CKU.getCookie('_dnd');
        if (null ===list) {
            return false;
        }

        return (null !== list.match(`(^|,)0(,|$)`));
    }

    isBannerDnd(bNo) {
        if (0 == bNo) {
            return false;
        }

        var list = CKU.getCookie('_dnd');
        if (null ===list) {
            return false;
        }

        return (null !== list.match(`(^|,)${bNo}(,|$)`));
    }

    setNotificationDnd() {
        var list = CKU.getCookie('_dnd');
        if (null === list) {
            list = '';
        } else {
            list = list + ',';
        }

        if (null !== list.match(`(^|,)0(,|$)`)) {
            return;
        }

        CKU.setCookieUntil('_dnd', list + '0', CKU.getLocalTomorrowInUTC());
    }

    setBannerDnd(bNo) {
        if (0 == bNo) {
            return;
        }

        var list = CKU.getCookie('_dnd');
        if (null === list) {
            list = '';
        } else {
            list = list + ',';
        }

        if (null !== list.match(`(^|,)${bNo},`)) {
            return;
        }

        CKU.setCookieUntil('_dnd', list + bNo, CKU.getLocalTomorrowInUTC());
    }

    findCanvas() {
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
    }

    setCookie(key, value) {
        return CKU.setCookie(key, value, 30 * 24 * 60 * 60 * 1000);
    }

    getCookie(key){
        return CKU.getCookie(key);
    }

    setLogout()
    {
        this.clearCookies();
        this._banner_shown = false;
        this._noti_shown = false;
        this._samanda_page = 0;
        
        require('AppUtils').SetLogOut();        
    }

    clearCookies()
    {
        CKU.clearUserCookies();
    }

    setWebState(state)
    {
        require('AppUtils').sendWebState(state);
    }

    isAndroid()
    {
        return this.getBrowserType() == 1;
    }

    isIPhone()
    {
        return this.getBrowserType() == 3;
    }

    isZFBrower()
    {
        return this.getBrowserType() == 2;
    }

    isStandAlone()
    {
        return this.getBrowserType() == 0;
    }

    getBrowserType()
    {
        return this._browserType;
    }

    onOAuthJavaResponse(data)
    {
        if ('function' === typeof this.oAUthCb) {
            this.oAUthCb(data);
        } else {
            Samanda.call('Invalid callback');
        }
    }

    onKeyboardVisibility(kbHeight, scHeight)
    {
        var effHeight = kbHeight * (cc.winSize.height / scHeight);
        console.log("[KB] height: " + effHeight);
        
        this.setKeyboardHeight(parseInt(effHeight));
    }

    getKeyboardHeight()
    {
        return this.keyboardHeight;
    }

    setKeyboardHeight(height)
    {
        this.keyboardHeight = height;
    }

    onTryLogin()
    {
        this.onChangeLoginScene();
    }

    onSamandaShortcut(type)
    {
        if (this.eMainPage.SCREENSHOT_SEND == type) {
            // not a page
            return;
        }

        this.samandaPage = type;
        if(type == this.eMainPage.PAGE_OVERLAY_CHAT)
        {
            this.onOverlayMode();
            this.samandaPage = 0;
        }
        else
            this.onChangeLoginScene();
    }

    onScreenshotCaptured(data, thumbnail)
    {
        if (Samanda.isStandAlone()) {            
            MessagePopup.openMessageBoxWithKey("POPUP_50");
            return;
        } else 
        if (false == WSM.isConnected) {
            MessagePopup.openMessageBoxWithKey("POPUP_56");
            return;
        }
        else if (typeof this.thumnailCache !== 'undefined' && this.thumnailCache != null) 
        {
            MessagePopup.openMessageBoxWithKey("POPUP_34");
            return;
        }

        this.thumnailCache = thumbnail;

        CHAT.uploadMainImage(data, 
        function(res) {         
            var data = JSON.parse(res);  
            if(data.hasOwnProperty("file"))
            {
                this.WsManager.sendImage(data["file"], this.thumnailCache);
            }
            else
            {
                MessagePopup.openMessageBoxWithKey("POPUP_58");    
            }
            
            this.thumnailCache = null;
        }.bind(this),
        function() {                    
            this.thumnailCache = null;
            MessagePopup.openMessageBoxWithKey("POPUP_59");    
        }.bind(this));
    }

    sendChatMessage(msg)
    {
        require("NetworkManager").doSendChat(msg, CHAT.tailChatSeq);
    }

    sendChatMessageWithNick(nick, msg)
    {
        require("NetworkManager").doSendChatWithNick(nick, msg, CHAT.tailChatSeq);
    }

    onOverlayMode()
    {        
        var scn = cc.director.getScene();
        if (null != scn) {
            scn.opacity = 0;        
            var cameraNode = scn.getChildByName('Main Camera');
            if (null === cameraNode) {
                //console.log('find camera failed #1');
                cameraNode = cc.find('Main Camera');
            }

            if(cameraNode != null)
            {
                var camera = cameraNode.getComponent(cc.Camera);
                if(camera)
                    camera.backgroundColor = cc.color(0, 0, 0, 0);
            }
        }
        
        if(this.isCustomOverlayChat())
        {
            this.setWebState(9);
            cc.director.loadScene("CustomChat.fire");
        }
        else
        {
            this.setWebState(7);
            cc.director.loadScene("OverlayChat.fire");    
        }
    }

    onPreLoadLoginScene() {
        var login_scn = require('AppUtils').GetOverrideVal('login_scene');
        if (login_scn) {
            login_scn += '.fire';
        } else {
            login_scn = 'Login.fire';
        }
        
        cc.director.preloadScene(login_scn);
    }

    onChangeLoginScene() {
        var login_scn = require('AppUtils').GetOverrideVal('login_scene');
        if (login_scn) {
            login_scn += '.fire';
        } else {
            login_scn = 'Login.fire';
        }

        var scn = cc.director.getScene();
        if(scn)
        {
            scn.children.forEach(node =>{
                node.active = false;
                node.stopAllActions();
            });

            cc.director.loadScene(login_scn);
        }
        else
        {
            console.log('not found cur scene - just load to next scene');

            cc.director.loadScene(login_scn);
        }        
    }

    openUrl(url) {
        if(Samanda.getBrowserType() == 0)
        {
            window.open(url);
        }
        else
        {
            var param = JSON.stringify({ key: "url", value: url });
            Samanda.call(param);
        }
    }

    getSceneName() {
        //console.log(cc.game._sceneInfos)
        //console.log(cc.director._scene._id)
        var sceneName;
        var _sceneInfos = cc.game._sceneInfos;
        for (var i = 0; i < _sceneInfos.length; i++) {
            if(_sceneInfos[i].uuid == cc.director._scene._id) {
                sceneName = _sceneInfos[i].url;
                sceneName = sceneName.substring(sceneName.lastIndexOf('/')+1).match(/[^\.]+/)[0];
            }
    
        }
    
        return sceneName;
    }

    call(arg)
    {        
        switch(Samanda.getBrowserType())
        {
            case 1:  
                if(this.isIPhone())
                {
                    window.webkit.messageHandlers.unityControl.postMessage(arg);            
                }
                else
                {
                    Unity.call(arg);
                }
                break;
            case 2:
                WinWebCall(arg);
                break;
            case 3:
                window.webkit.messageHandlers.unityControl.postMessage(arg);
                break;
            default:
                break;
        }
    }

    isEnableUseChat()
    {
        return (Samanda._optionflag & this.samanda_option.option_enable_chat) > 0 ? 1 : 0;
    }

    getOrientationType()
    {
        return (Samanda._optionflag & this.samanda_option.option_orientation) > 0 ? 1 : 0;
    }

    isCustomOverlayChat()
    {
        return (Samanda._optionflag & this.samanda_option.option_overlaychat) > 0 ? 1 : 0;
    }
    
    isYGProduct()
    {
        var YGProduct = require('AppUtils').GetOverrideVal('yg_product');
        if(YGProduct == 'ON')
            return true;

        return false;
    }

    RequestJWT()
    {
        var NetworkManager = require("NetworkManager");
        //console.log('RequestJWT#1');

        var data = new FormData();
        data.append('pid', NetworkManager.product_type);
        require("AccountManager").AppendFormData(data);
        
        //console.log('RequestJWT#2');

        var onSuccess = this.onResponseJWT.bind(this);
        var onError = this.onResponseJWT_Error.bind(this);

        //console.log('RequestJWT#3');
        //console.log(NetworkManager);
        NetworkManager.SendRequestPost("auth/token/create", data, onSuccess, onError);

        //console.log('RequestJWT#4');
    }

    onResponseJWT(response)
    {
        //console.log('onResponseJWT#1');
        console.log(response);

        var data = JSON.parse(response);
        var rs = parseInt(data["rs"]);
        
        //console.log('onResponseJWT#2');

        switch(rs) {
            case 0:
                //console.log('onResponseJWT#3');
                require('AppUtils').SetAppData('jwt', data["jwt"]);
                break;
            
            default:
                this.onResponseJWT_Error();
                break;
        }
    }

    onResponseJWT_Error()
    {
        //console.log('onResponseJWT_Error');
        require('AppUtils').SetAppData('jwt', 'error');
    }

    setLanguageCode(lanCode)
    {
        this._languageCode = Number(lanCode); 
        this.onChangeLoginScene();
    }

    getLanguageCode()
    {
        switch(this._languageCode)
        {
            case 10:
                return 'en';
            case 23:
                return 'ko';
        }
        return 'en';
    }
    
    onBackKeyPressed()
    {    
        if ('function' === typeof this.oAUthCb && (this.isAndroid() || this.isIPhone()) ) 
        {
            return;
        }   

        if(('function' === typeof this.cancelPopup) && (this.cancelPopup != null))
        {
            this.cancelPopup();
            return;
        }

        if (this.banner_up) {
            return;
        }

        var scn = cc.director.getScene();
        if (null != scn) 
        {  
            var eventNode = scn.getChildByName('EventScript');
            if (null === eventNode) {                
                eventNode = cc.find('EventScript');
            }
            
            if(eventNode != null)
            {
                var LoginEventScript = eventNode.getComponent('LoginEventScript');
                if(typeof LoginEventScript.win != 'undefined' && LoginEventScript.win != null && LoginEventScript.win.closed == false)
                {
                    LoginEventScript.win.close();
                    return;
                }

                if(LoginEventScript != null)
                {
                    if(LoginEventScript.isStateMainBox())
                    {
                        var MainPageScript = LoginEventScript.getMainPageScript();
                        if(MainPageScript != null)
                        {
                            if(MainPageScript.home_page.active == false)
                            {
                                MainPageScript.onStateChange(Samanda.eMainPage.PAGE_MAIN);
                                return;
                            }
                            else
                            {
                                require('AppUtils').SetAppData('hide', '');
                                return;
                            }
                        }
                    }
                }
            }
        }

        cc.director.loadScene('EmptyScene.fire');
        require('AppUtils').SetAppData('hide', '');
    }

    setEmptyScene()
    {
        cc.director.loadScene('EmptyScene.fire');
    }

    OnServerMaintenance()
    {
        var callback = function(){
            require('AppUtils').SetAppData('server', 'maintenance');
            cc.director.loadScene('EmptyScene.fire');
        };
        Popup.openMessageBoxWithKey("POPUP_19", callback);        
    }

    OnNeedUpdate()
    {
        var callback = function(){
            require('AppUtils').SetAppData('url', 'https://play.google.com/store/apps/details?id=com.SandboxGame.CMMHCT');
            cc.director.loadScene('EmptyScene.fire');
        };
        require("MessagePopup").openMessageBoxWithKey("NEED_APP_UPDATE", callback);    
    }
};

module.exports = SamandaProto.getInstance();

var NetworkManager = require("NetworkManager");
var MessagePopup = require("MessagePopup");
var Samanda = require("Samanda");
var Account = require('AccountManager');
var LoginEventScript = require('LoginEventScript');

cc.Class({
    extends: cc.Component,

    properties: {
        toggle_buttons : [cc.Toggle],
        home_page : cc.Node,
        notice_page : cc.Node,
        chat_page : cc.Node,
        account_page : cc.Node,

        notifications_scrollview : cc.ScrollView,
        notifications_webview : cc.Node,

        main_banner_webview : cc.WebView,

        home_notice_bg : cc.Node,
        no_notice_label : cc.Node,
        loginEventScript : LoginEventScript,

        isCubeStyle : false,
        cubeVerticalButtons : [cc.Node],
        cubeHorizontalButtons : [cc.Node],
        backgroundNode : cc.Node,

        ProfileButtonSprite : cc.Sprite,
        ProfileButtonLabel : cc.Label,
        ProfileToggleSpriteFrame : [cc.SpriteFrame],
    },

    setLoginEventScript(script)
    {
        this.loginEventScript = script;

        if(this.account_page)
        {
            var AccountUI = this.account_page.getComponent('AccountUI');
            if(AccountUI != null)
            {
                AccountUI.setLoginEventScript(this.loginEventScript);
            }
        }
    },

    onLoad : function() {
        //this.main_banner_webview.url = "../_game_main";
        if (CC_DEBUG) {
            this.main_banner_url = 'https://sandbox-gs.mynetgear.com/main_banner/main_banner?game_type=dbMPZX&lang=' + Samanda.getLanguageCode();
        } else {
            var pid = NetworkManager.product_type || 3;
            this.main_banner_url = 'https://sandbox-gs.mynetgear.com/main_banner/main_banner?game_type=' + pid + '&lang=' + Samanda.getLanguageCode();            
        }
        this.backToMainBanner();

        if(Samanda.isEnableUseChat() == false || (Samanda.isCustomOverlayChat() && Samanda.isStandAlone()))
        {
            if(this.toggle_buttons[2])
                this.toggle_buttons[2].node.active = false;

            var Btn_Other = this.node.getChildByName('Btn_Other');
            if(Btn_Other)
            {
                var Btn_Overlay = Btn_Other.getChildByName('Btn_Overlay');
                Btn_Overlay.active = false;
            }
        }

        if(this.isCubeStyle == false)
            return;

        let delayTime = 1;
        let nextTime = 30;

        this.cubeHorizontalButtons.forEach(node =>{
            if(node)
            {
                let originColor = node.color;
                let darkColor = originColor * 0.5;
                node.runAction(
                    cc.repeatForever(                        
                        cc.sequence(
                            cc.delayTime(delayTime), 
                            cc.callFunc(function(){
                                node.children.forEach(child =>{
                                    child.stopAllActions();
                                    child.runAction(cc.scaleTo(0.3, 0, 0).easing(cc.easeSineIn()));
                                });
                            }.bind(this)), cc.delayTime(0.3),
                            cc.callFunc(function(){
                                node.x -= (node.anchorX - 0) * node.width;
                                node.anchorX = 0;
                            }.bind(this)), 
                            cc.spawn(cc.scaleTo(0.5, 0, 1).easing(cc.easeSineIn()), cc.tintTo(0.5, darkColor.r, darkColor.g, darkColor.b).easing(cc.easeSineIn())), 
                            cc.callFunc(function(){
                                node.x -= (node.anchorX - 1) * node.width;
                                node.anchorX = 1;
                            }.bind(this)), 
                            cc.spawn(cc.scaleTo(0.5, 1, 1).easing(cc.easeSineIn()), cc.tintTo(0.5, originColor.r, originColor.g, originColor.b).easing(cc.easeSineIn())), 
                            cc.callFunc(function(){
                                node.x -= (node.anchorX - 0.5) * node.width;
                                node.anchorX = 0.5;

                                node.children[0].stopAllActions();
                                node.children[0].runAction(
                                    cc.sequence(
                                        cc.delayTime(1.3), 
                                        cc.scaleTo(0.3, 0.25, 0.25).easing(cc.easeSineIn())
                                    )
                                );
                                node.children[1].stopAllActions();
                                node.children[1].runAction(
                                    cc.sequence(                                        
                                        cc.scaleTo(0.3, 0.5, 0.5).easing(cc.easeSineIn()),
                                        cc.delayTime(1.0), 
                                        cc.scaleTo(0.3, 0, 0).easing(cc.easeSineIn())
                                    )
                                );
                            }.bind(this)), 
                            cc.delayTime(0.3),
                            cc.delayTime(nextTime)
                        )
                    )
                );

                delayTime += 1;
            }
        });

        this.cubeVerticalButtons.forEach(node =>{            
            if(node)
            {
                let originColor = node.color;
                let darkColor = originColor * 0.5;
                node.runAction(
                    cc.repeatForever(                        
                        cc.sequence(
                            cc.delayTime(delayTime), 
                            cc.callFunc(function(){
                                node.children.forEach(child =>{
                                    child.stopAllActions();
                                    child.runAction(cc.scaleTo(0.3, 0, 0).easing(cc.easeSineIn()));
                                });
                            }.bind(this)), cc.delayTime(0.3),
                            cc.callFunc(function(){
                                node.y -= (node.anchorY - 0) * node.height;
                                node.anchorY = 0;
                            }.bind(this)), 
                            cc.spawn(cc.scaleTo(0.5, 1, 0).easing(cc.easeSineIn()), cc.tintTo(0.5, darkColor.r, darkColor.g, darkColor.b).easing(cc.easeSineIn())), 
                            cc.callFunc(function(){
                                node.y -= (node.anchorY - 1) * node.height;
                                node.anchorY = 1;
                            }.bind(this)), 
                            cc.spawn(cc.scaleTo(0.5, 1, 1).easing(cc.easeSineIn()), cc.tintTo(0.5, originColor.r, originColor.g, originColor.b).easing(cc.easeSineIn())), 
                            cc.callFunc(function(){
                                node.y -= (node.anchorY - 0.5) * node.height;
                                node.anchorY = 0.5;

                                node.children[0].stopAllActions();
                                node.children[0].runAction(
                                    cc.sequence(
                                        cc.delayTime(1.3), 
                                        cc.scaleTo(0.3, 0.25, 0.25).easing(cc.easeSineIn())
                                    )
                                );
                                node.children[1].stopAllActions();
                                node.children[1].runAction(
                                    cc.sequence(                                        
                                        cc.scaleTo(0.3, 0.5, 0.5).easing(cc.easeSineIn()),
                                        cc.delayTime(1.0), 
                                        cc.scaleTo(0.3, 0, 0).easing(cc.easeSineIn())
                                    )
                                );
                            }.bind(this)), cc.delayTime(0.3),
                            cc.delayTime(nextTime)
                        )
                    )
                );

                delayTime += 1;
            }
        });

        this.main_banner_webview.node.active = false;
        this.backgroundNode.scaleY = 0.0964673913043478;
        this.backgroundNode.runAction(cc.sequence(
            cc.scaleTo(1, 0.25, 0.25).easing(cc.easeSineIn()),
            cc.callFunc(function(){
                this.main_banner_webview.node.active = true;
            }.bind(this))
        ));
    },

    start : function() {
        this.setMainNotification();
        this.setNotificationListUI();

        if(!Samanda.isStandAlone())
        {
            cc.director.preloadScene("OverlayChat.fire");
        }
    },

    onEnable : function() {   
        this.onStateChange(Samanda.samandaPage); 
    },

    backToMainBanner : function() {
        this.main_banner_webview.url = this.main_banner_url;
        this.mainBannerOpen = true;
    },

    setMainNotification : function()
    {
        //console.log('setMainNotification' + Samanda.notifications);
        var lang = Samanda.getLanguageCode();
        
        var notiData = Samanda.notifications;
        var data = new Array();
        notiData.forEach(element => {
            if(element.language == lang)
                data.push(element);
        });

        var numItem = data.length;

        if (0 == numItem) {
            // 새 소식 없음
            if(this.no_notice_label)
                this.no_notice_label.active = true;
            return;
        }

        var self = this;
        var noti_bg = this.home_notice_bg;
        if(this.home_notice_bg)
        {
            var prefabPath = 'Prefabs/Default/Main_Notice_Item';
            var initPos = 0;
            var offsetPos = 78;
            
            var initItemCount = 1;
            switch(Samanda.getOrientationType())
            {
                case 0:
                    prefabPath = 'Prefabs/Default_Vertical/Main_Notice_Item_Vertical';
                    initPos = 45;
                    offsetPos = 78;
                    initItemCount = 2;
                    break;
                case 1:
                default:
                    prefabPath = 'Prefabs/Default/Main_Notice_Item';
                    break;
            }

            cc.resources.load(prefabPath, function(err, prefab) {
                if (!prefab) {
                    console.log('!prefab');
                    return;
                }
                
                // 최대 2개의 아이템
                for(let i = 0; initItemCount > i; ++i) {
                    // 더 표시할 내용 없으면 중단
                    if (numItem <= i)
                        return;

                    // 버튼인 목록 아이템 인스턴스
                    let item = cc.instantiate(prefab);
                    try {
                        // 제목
                        let title = item.getChildByName("Label_Title")
                                        .getComponent(cc.Label);
                        title.string = data[i].title;
                        // 작성 시각
                        let time = item.getChildByName("Label_Time")
                                        .getComponent(cc.Label);
                        time.string = data[i].time;

                        // 클릭 핸들러
                        let handler = new cc.Component.EventHandler();
                        handler.target = self.node;
                        handler.component = "MainPageScript";
                        handler.handler = "onMainNoticeButton";
                        handler.customEventData = i;

                        item.getComponent(cc.Button).clickEvents.push(handler);

                        // bg에 붙이고 좌표 조정
                        noti_bg.addChild(item);
                        item.y = initPos - i * offsetPos;
                    } catch(e) {
                        console.log(e.toString());
                        return;
                    }
                }
            });
        }
        else
        {
            var mainObj = this.node.getChildByName("Page_Main");
            if(mainObj)
            {
                var pageObj = mainObj.getChildByName("Page_Home");
                if(pageObj)
                {
                    var bannerNoticeObj = pageObj.getChildByName("Banner_Notice");
                    if(bannerNoticeObj)
                    {
                        var bgObj = bannerNoticeObj.getChildByName("Background");
                        if (bgObj)
                        {
                            var labelObj = bgObj.getChildByName("Label");
                            if(labelObj)
                            {
                                var noticeLabel = labelObj.getComponent(cc.Label);
                                if(noticeLabel)
                                {
                                    noticeLabel.string = data[0].title;
                                }
                            }
                        }
                    }
                }
            }
        }
    },

    onMainNoticeButton : function(event, param)
    {
        if (typeof param === 'undefined') 
        {
            param = 0;
        }

        var notiData = Samanda.notifications;
        var lang = Samanda.getLanguageCode();
        var data = new Array();
        notiData.forEach(element => {
            if(element.language == lang)
                data.push(element);
        });

        console.log(data);
        var numItem = data.length;
        if (0 === numItem) 
        {
            return;
        }
        
        this.onStateChange(Samanda.eMainPage.PAGE_NOTICE);
        if(data.length > param)
        {
            if(data[param].uri)
                this.onBtnNoticeItem(null, data[param].uri);
        }
    },
    // 공지사항 리스트 관리
    setNotificationListUI : function()
    {
        /**
         * 공지사항 scrollView에 각 항목을 배치하고 content 크기와 위치를 설정함
         * 
         */
        var scroll = this.notifications_scrollview;
        var content = scroll.content;
        var lang = Samanda.getLanguageCode();
        
        var notiData = Samanda.notifications;
        var data = new Array();
        notiData.forEach(element => {
            if(element.language == lang)
                data.push(element);
        });
        
        var numItem = data.length;
        if (0 === numItem) {
            console.log('0 === numItem');            
            return;
        }

        // content가 layout이며 padding, spacing은 여기에 설정된다
        var padT = 8;
        var padB = 8;
        var spc = 6;
        var layout = content.getComponent(cc.Layout);
        if (layout) {
            padT = layout.paddingTop;
            padB = layout.paddingBottom;
            spc = layout.spacingY;
        }

        // 에디터에서 배치된 dummy 아이템의 크기를 가져옴
        var itemHeight = 60;
        if (false === 'itemHeight' in content && content.childrenCount) {
            itemHeight = content.children[0].height;
        }

        // 컨텐츠 노드 크기 설정
        content.height = padT + (itemHeight + spc) * numItem - spc + padB;

        // 컨텐츠 노드는 top 앵커 기준이므로 이 위치를 view의 top에 맞춤
        content.y = scroll.node.height / 2;

        // prefab 노드를 제거하고
        content.removeAllChildren();
        // 공지 목록에 맞게 설정함
        var self = this;
        var prefabPath = 'Prefabs/Notice_List_ScrollItem';      
        if(require('AppUtils').GetOverrideVal('login_scene') === 'Login_default')                
        {
            switch(Samanda.getOrientationType())
            {
                case 0:
                    prefabPath = 'Prefabs/Default_Vertical/Notice_List_ScrollItem_default_Vertical';
                    break;
                case 1:
                default:
                    prefabPath = 'Prefabs/Default/Notice_List_ScrollItem_default';
                    break;
            }
        }
            
        for (let i = 0; numItem > i; ++i) {
            cc.resources.load(prefabPath, function(err, prefab) {
                let item = cc.instantiate(prefab);

                try {
                    let lbl = item.getChildByName('Background').
                                   getChildByName('Label').
                                   getComponent(cc.Label);
                    lbl.string = data[i].title;

                    let timeLbl = item.getChildByName('Background').
                                       getChildByName('TimeLabel').
                                       getComponent(cc.Label);
                    timeLbl.string = data[i].time;

                    // callback event
                    let clickEventHandler = new cc.Component.EventHandler();
                    clickEventHandler.target = self.node; // This node is the node to which your event handler code component belongs
                    clickEventHandler.component = "MainPageScript";// This is the code file name
                    clickEventHandler.handler = "onBtnNoticeItem";
                    clickEventHandler.customEventData = data[i].uri;

                    item.getComponent(cc.Button).clickEvents.push(clickEventHandler);
                } catch (e) {
                    console.log('exception while get label ' + e.toString());
                }

                content.addChild(item);

            });
        }
        
    },

    onBtnNoticeItem : function(event, param)
    {
        //console.log('onBtnNoticeItem : ' + param);
        // 공지 웹뷰
        this.notifications_webview.getComponent(cc.WebView).url = param;
        this.notifications_webview.getParent().active = true;
    },

    onBtnCloseMain : function(nid)
    {
        if(Samanda.isStandAlone())
        {
            this.node.active = false;
            var callback = function(){
                this.node.active = true;
            }.bind(this);
            MessagePopup.openMessageBoxWithKey("POPUP_50", callback);
            return;
        }

        Samanda.setWebState(5);
        Samanda.setWebState(6);
    },

    offNotificationWebview : function()
    {
        //console.log('offNotificationWebview');
        this.notifications_webview.getParent().active = false;
    },

    onPageChange : function(event, param)
    {
        if(param)//just btn
        {
            this.onStateChange(param);
        }
        else//togle
        {
            var page = event.checkEvents[0].customEventData;        
            this.onStateChange(page);
        }
    },

    onStateChange : function(page)
    {
        if(this.toggle_buttons.length > page && this.toggle_buttons[page])
        {
            this.toggle_buttons[page].isChecked = true;            
        }
        else
        {
            this.toggle_buttons.forEach(btn =>{ 
                if(btn)
                {
                    btn.node.children.forEach(node =>{ 
                        node.active = false; 
                    });
                }
            });            
        }

        if(page == Samanda.eMainPage.PAGE_OVERLAY_CHAT)
        {
            this.onOverlayMode();
        }

        // notice 창 열려있는지
        if (this.notifications_webview.getParent().active && !this.notice_page.active) {
            this.notifications_webview.getParent().active = false;
        }

        if(this.toggle_buttons.length > page)
        {
            if(this.toggle_buttons[page])
            {
                var anim = this.toggle_buttons[page].getComponent(cc.Animation);
                if(anim)
                    anim.play();
            }
        }

        //console.log(this.home_page);
        this.home_page.active       = page == Samanda.eMainPage.PAGE_MAIN;
        //console.log(this.notice_page);
        this.notice_page.active     = page == Samanda.eMainPage.PAGE_NOTICE;
        //console.log(this.chat_page);
        this.chat_page.active       = page == Samanda.eMainPage.PAGE_CHAT;
        //console.log(this.account_page);        
        this.account_page.active    = page == Samanda.eMainPage.PAGE_ACCOUNT;
        
        if(page == Samanda.eMainPage.PAGE_ACCOUNT)
        {
            var AccountUI = this.account_page.getComponent('AccountUI');
            if(AccountUI != null)
            {
                AccountUI.setLoginEventScript(this.loginEventScript);
            }
        }

        Samanda.samandaPage = 0;//reset
    },

    onOverlayMode : function()
    {   
        if(Samanda.isStandAlone() && Samanda.isCustomOverlayChat())
        {
            this.node.active = false;
            var callback = function(){
                this.node.active = true;
            }.bind(this);
            MessagePopup.openMessageBoxWithKey("POPUP_50", callback);
            return;
        }

        Samanda.onOverlayMode();
    },

    onProfileButton: function()
    {
        this.onStateChange(this.home_page.active ? 5 : 0);
        
        this.ProfileButtonSprite.spriteFrame = this.ProfileToggleSpriteFrame[this.home_page.active ? 1 : 0];  
        this.ProfileButtonLabel.string = this.home_page.active ? "계정" : "홈";
    },

    onCustomerServiceButton: function()
    {
        console.log('onCustomerServiceButton');
        try {
            var data = new Object();
            data.key = 'cs';
            data.value = '';
            var jsonMessage = JSON.stringify(data);
            Samanda.call(jsonMessage);
        } 
        catch(e){
            console.log("[error] not found app handler(standalone services is not yet)");
        }
    },

    onSandboxChannelButton: function()
    {
        console.log('onSandboxChannelButton');
        var url = require('AppUtils').GetOverrideVal('sandbox_channel');
        Samanda.openUrl(url);
    },

    onYoutubeChannelButton: function()
    {
        console.log('onYoutubeChannelButton');
        var url = require('AppUtils').GetOverrideVal('youtube_channel');
        Samanda.openUrl(url);
    },

    onMuchMerchButton: function()
    {
        console.log('onMuchMerchButton');
        var url = require('AppUtils').GetOverrideVal('muchmerch');
        Samanda.openUrl(url);
    },

    onNotificationButton: function()
    {
        // if (!this.mainBannerOpen) {
        //     this.backToMainBanner();
        //     return;
        // }

        var lang = Samanda.getLanguageCode();
        
        var notiData = Samanda.notifications;
        var noti_uri = false;
        notiData.forEach(element => {
            if(element.language == lang) {
                noti_uri = element.uri;
                return false;
            }
        });

        if (noti_uri) {
            Samanda.openUrl(noti_uri);
        }
        // if (!noti_uri) {
        //     return;
        // }

        // var pid = 'dbMPZX';
        // if (!CC_DEBUG) {
        //     pid = NetworkManager.product_type || 3;
        // }

        // var url = NetworkManager.post_url + noti_uri;
        // url += 'pid=' + pid;
        // url += '&lang=' + Samanda.getLanguageCode();

        //this.mainBannerOpen = false;
        //this.main_banner_webview.url = url;
    }
});

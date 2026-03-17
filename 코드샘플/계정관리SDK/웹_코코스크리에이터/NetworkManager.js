var Samanda = require("Samanda");
var StdResHandler = require("ResponseHandler");
var WSM = require('WebsocketManager');
var Account = require('AccountManager');
const AccountManager = require("./Account/AccountManager");

// WebService ver. 오픈채팅 기능 추가를 위해 추가됨.
var CHAT = require("ChatManager");

const NetworkManager = class
{
    post_url = '../';       
    responsed = false;
    product_type = '';
    
    otp = '';
    rs = '';
    msg = '';        
    terms_ver = '';
    terms_uri1 = '';
    terms_uri2 = '';
    debug_product_type = 3;
    
    static self = null;

    static getInstance()
    {
        if(NetworkManager.self === null)
        {
            NetworkManager.self = new NetworkManager();

            if (CC_DEBUG) {
                console.log('is DebugMode');
                NetworkManager.self.post_url = Samanda.getSamandaUrl();
                NetworkManager.self.product_type = NetworkManager.self.debug_product_type;
            }
        }

        return NetworkManager.self;
    }
 
    Ctor() // constructor
    {
        this.scene = new Array();
        NetworkManager.self = this;
    }

    onLoad () {
        cc.game.addPersistRootNode(this.node); //Resident node
    }

    SendRequestGet(url, onSuccess, onError) {
        var xhr = cc.loader.getXMLHttpRequest();
        this.StreamXHREvents(xhr, 'GET', onSuccess, onError);

        xhr.open("GET", url, true);
        if (cc.sys.isNative) {
            xhr.setRequestHeader("Accept-Encoding","gzip,deflate");
        }

        xhr.timeout = 10000;// 10 seconds for timeout

        xhr.send();
    }

    SendRequestPost(url, data, onSuccess, onError) {
        var xhr = cc.loader.getXMLHttpRequest();
        this.StreamXHREvents(xhr, "POST", onSuccess);
        
        xhr.open("POST", this.post_url + url, true);        
        xhr.timeout = 10000;// 10 seconds for timeout

        xhr.send(data);
    }

    // WebService ver. 오픈채팅 기능 추가를 위해 추가됨.
    SendRequestPostUsingJson(url, data, onSuccess, onError) {
        console.log(`SendRequestPostUsingJsons`);
        console.log(data);

        var xhr = cc.loader.getXMLHttpRequest();
        this.StreamXHREvents(xhr, "POST", onSuccess);

        xhr.open("POST", this.post_url + url, true);
        xhr.setRequestHeader("Content-Type", "application/json;charset=UTF-8");
        xhr.timeout = 10000;
        xhr.send(data);
    }

    SendRequestPostWithResponseTypeBlob(url, data, onSuccess, onError) {
        var xhr = cc.loader.getXMLHttpRequest();
        
        var handler = ('function' === typeof onSuccess) && onSuccess || null;

        xhr.onreadystatechange = function() {
            if (this.readyState == 4 && this.status == 200 && handler != null) {
                handler(this.response);
            }
        };

        xhr.open("POST", this.post_url + url, true);    
        xhr.responseType="blob";    
        xhr.timeout = 10000;// 10 seconds for timeout

        xhr.send(data);
    }
    
    StreamXHREvents( xhr, method, onSuccess, onError ) {        
        // var handler = responseHandler || function (response) {
        //     console.log("[error] network callback is null");            
        // };
        var handler = ('function' === typeof onSuccess) && onSuccess || StdResHandler;
        
        //var eventLabelOrigin = label.string;
        // Simple events
        // ['loadstart', 'abort', 'error', 'load', 'loadend', 'timeout'].forEach(function (eventname) {
        //     xhr["on" + eventname] = function () {
        //         //console.log("StreamXHREvents - on");
        //         //label.string = eventLabelOrigin + "\nEvent : " + eventname;
        //         if (eventname === 'timeout') {
        //             console.log("[error] network timeout!");
        //             //label.string += '(timeout)';
        //         }
        //         else if (eventname === 'loadend') {
        //             if (eventname !== '(timeout)') {
        //                 //console.log("StreamXHREvents - loaded");
        //                 //label.string += '...loadend!';
        //             }
        //         }
        //     };
        // });
        xhr.ontimeout = function() {
            console.log("[error] network timeout!");
        };
    
        // Special event
        xhr.onreadystatechange = function () {
            if (4 === xhr.readyState && 200 === xhr.status) {
                if('SERVER_MAINTENANCE' == xhr.responseText)
                {
                    Samanda.OnServerMaintenance();                    
                    return;
                }
                if('NEED_APP_UPDATE' == xhr.responseText)
                {
                    Samanda.OnServerMaintenance();                    
                    return;
                }

                handler(xhr.responseText);
            } else if ('function' === typeof onError) {
                onError(xhr.status);
            }

            // if (xhr.readyState === 4 && xhr.status >= 200) {
                
            //     if(xhr.status === 404) {
            //         console.log("[error] 404 page not found!");
            //     }
            //     else {
            //         StdResHandler(xhr.responseText);

            //         handler(xhr.responseText);
            //     }
            // }
            // else if (xhr.status === 404) {
            //     //console.log("[error] 404 page not found!");                
            // }
            // else if (xhr.readyState === 3) {
            //     //label.string = "Request dealing!";
            // }
            // else if (xhr.readyState === 2) {
            //     //label.string = "Request received!";
            // }
            // else if (xhr.readyState === 1) {
            //     //label.string = "Server connection established! Request hasn't been received";
            // }
            // else if (xhr.readyState === 0) {
            //     //label.string = "Request hasn't been initiated!";
            // }
        };
    }

    onResponseUserInfo(response)
    {
        console.log(response);
        
        let self = NetworkManager.self;

        self.responsed = false;        
        var data = response;   
        
        if(data.hasOwnProperty("pid"))
            self.product_type = data["pid"];

        if(data.hasOwnProperty("tok") && data.hasOwnProperty("nic") && data.hasOwnProperty("ano"))
        {
            Account.SetAccountData(data);
        }   
        
        if(data.hasOwnProperty("otp"))
            self.otp = data["otp"];
        if(data.hasOwnProperty("rs"))
            self.rs = data["rs"];
        if(data.hasOwnProperty("terms_ver") && data.hasOwnProperty("terms_uri")) {
            self.terms_ver = data["terms_ver"];
            
            var terms_path = data["terms_uri"]["path"];
            self.terms_uri1 = terms_path + data["terms_uri"]["privacy"];
            self.terms_uri2 = terms_path + data["terms_uri"]["service"];
        }
        if(data.hasOwnProperty("msg"))
            self.msg = data["msg"];
        if(data.hasOwnProperty("overrides"))
            require('AppUtils').SetOverrides(data["overrides"]);

        self.responsed = true;
    }

    onResponseSessionTokenInfo(response)
    {
        console.log(response);
        
        let self = NetworkManager.self;
        
        self.responsed = false; 
        var data = response; 

        if(data.hasOwnProperty("rs"))
            self.rs = data["rs"];

        if(data.hasOwnProperty("pid"))
            self.product_type = data["pid"];
        
        Account.SetSessionTokenAccount(data["tok"], data["ano"]);

        if(data.hasOwnProperty("otp"))
            self.otp = data["otp"];

        if(data.hasOwnProperty("rs"))
            self.rs = data["rs"];

        if(data.hasOwnProperty("terms_ver") && data.hasOwnProperty("terms_uri")) {
            self.terms_ver = data["terms_ver"];
            
            var terms_path = data["terms_uri"]["path"];
            self.terms_uri1 = terms_path + data["terms_uri"]["privacy"];
            self.terms_uri2 = terms_path + data["terms_uri"]["service"];
        }
        if(data.hasOwnProperty("msg"))
            self.msg = data["msg"];
            
        if(data.hasOwnProperty("overrides"))
            require('AppUtils').SetOverrides(data["overrides"]);
            
        self.responsed = true;
    }
    
    tryWebSocketConnect()
    {
        let self = NetworkManager.self;
        var account_no = Account.GetAccountNo();
        var nick_name = Account.GetNickName();
        var profile = Account.GetUserProfilePath();

        if (self.product_type && account_no) {
            WSM.connect(
                'wss://sandbox-gs.mynetgear.com/websocket',
                self.product_type,
                account_no,
                nick_name,
                profile,
            );
        }
    }

    IsResponsedUserInfo()
    {
        if(this.responsed)
            return this.responsed;
        else
        {
            try
            {
                if(response_result)
                {
                    this.onResponseSessionTokenInfo(response_result);
                }
            }
            catch(e)
            {
                console.log(e);
            }

            return this.responsed;
        }
    }

    IsValidAccount()
    {
        return this.rs == 0 && this.responsed;
    }

    IsNeedLogin()
    {
        return this.rs == 1 && this.responsed;
    }

    onTermsAgreed() {
        this.terms_ver = '';
        this.terms_uri1 = '';
        this.terms_uri2 = '';
    }

    setLoggedOut() {
        Account.ClearData();

        this.otp = '';
        this.rs = 1;
        this.msg = '';        
        this.terms_ver = '';
        this.terms_uri1 = '';
        this.terms_uri2 = '';
        
        WSM.disconnect();
        Samanda.setWebState(2);
        Samanda.setLogout();
    }

    /**
     * 오픈채팅의 메시지를 전송함.
     * 응답으로 기준 번호부터 새 메시지 까지의 오픈채팅 message List를 받음.
     * @param {string} message 전달할 메시지 
     * @param {Number} referenceSeq 가져올 메시지 기준 번호(일반적으로 tailChatSeq를 사용)
     */
    doSendChat(message, referenceSeq) {
        /**
         * OpenChat용 OpCode
         * None = 0, 
         * Say = 1, Chat message 전송
         * Pull = 2, 신규 Chat message List 가져오기
         * Previous = 3, 이전 Chat message List 가져오기
         */

          /**
           * 오픈채팅 Request용 메시지 구조
           * "Uri":"openchat",
           * "OpCode":OpenChat용 OpCode에서 골라서 사용,
           * "Rs":0,
           * "PId":"게임의 PId",
           * "ReferenceSeq":0,
           * - 기준이 되는 Chat message Sequence.
           * - OpCode가 Say일 때: 새 메시지 번호부터 이 순번 사이의 메시지를 최대 30개까지 리턴함.
           * - OpCode가 Pull일 때: 이 순번 이전의 메시지를 최대 30개까지 리턴함.
           * "Content":ChatMessage Object (WebSocket에서 사용하던 것 그대로 사용)
           */
        var data = {Uri:"openchat",OpCode:1,Rs:0,PId:this.product_type,ReferenceSeq:referenceSeq,Content:{SenderAccountNo:Account.GetAccountNo(),Sender:Account.GetNickName(),Message:message,ProfileUrl:Account.GetUserProfilePath()}};
        this.SendRequestPostUsingJson("openchat", JSON.stringify(data), this.onResponseOpenChat);
    
        // try {
        //     var msg = JSON.stringify(data);
        //     this.socket.send(msg);
        // } catch (e) {
        //     console.log('doSend: ' + e.toString());
        //     this.socket.send(data);
        // }
    }

    doSendChatWithNick(nick, message, referenceSeq) {
        /**
         * OpenChat용 OpCode
         * None = 0, 
         * Say = 1, Chat message 전송
         * Pull = 2, 신규 Chat message List 가져오기
         * Previous = 3, 이전 Chat message List 가져오기
         */

          /**
           * 오픈채팅 Request용 메시지 구조
           * "Uri":"openchat",
           * "OpCode":OpenChat용 OpCode에서 골라서 사용,
           * "Rs":0,
           * "PId":"게임의 PId",
           * "ReferenceSeq":0,
           * - 기준이 되는 Chat message Sequence.
           * - OpCode가 Say일 때: 새 메시지 번호부터 이 순번 사이의 메시지를 최대 30개까지 리턴함.
           * - OpCode가 Pull일 때: 이 순번 이전의 메시지를 최대 30개까지 리턴함.
           * "Content":ChatMessage Object (WebSocket에서 사용하던 것 그대로 사용)
           */
        var data = {Uri:"openchat",OpCode:1,Rs:0,PId:this.product_type,ReferenceSeq:referenceSeq,Content:{SenderAccountNo:Account.GetAccountNo(),Sender:nick,Message:message,ProfileUrl:Account.GetUserProfilePath()}};
        this.SendRequestPostUsingJson("openchat", JSON.stringify(data), this.onResponseOpenChat);
    
        // try {
        //     var msg = JSON.stringify(data);
        //     this.socket.send(msg);
        // } catch (e) {
        //     console.log('doSend: ' + e.toString());
        //     this.socket.send(data);
        // }
    }

    /**
     * 기준 번호 이후의 메시지를 최대 30개까지 가져옴.
     * @param {Number} referenceSeq 가져올 메시지 기준 번호(일반적으로 tailChatSeq를 사용)
     */
    doPullChat(referenceSeq) {
        /**
         * doSendChat()과 같이 오픈채팅 Request용 메시지를 사용.
         * OpCode 만 다름.
         */
        var data = {Uri:"openchat",OpCode:2,Rs:0,PId:this.product_type,ReferenceSeq:referenceSeq};
        this.SendRequestPostUsingJson("openchat", JSON.stringify(data), this.onResponseOpenChat);
    }

    /**
     * 기준 번호 이전의 메시지를 최대 30개까지 가져옴.
     * @param {Number} referenceSeq 가져올 메시지 기준 번호(일반적으로 headChatSeq를 사용)
     */
    doPreviousChat(referenceSeq) {
        /**
         * doSendChat()과 같이 오픈채팅 Request용 메시지를 사용.
         * OpCode 만 다름.
         */
        var data = {Uri:"openchat",OpCode:3,Rs:0,PId:this.product_type,ReferenceSeq:referenceSeq};
        this.SendRequestPostUsingJson("openchat", JSON.stringify(data), this.onResponseOpenChat);
    }

    /**
     * doSendChat()의 응답 처리 콜백
     * @param {JSON} response 오픈채팅 Response용 메시지 구조 참조.
     */
    onResponseOpenChat(response) {
        /**
         * OpenChat용 Rs
         * Ok = 0,
         * InvalidPId = 1, 잘못된 PId 값
         * InvalidMessage = 2, 비정상적인 메시지
         */

        /**
           * 오픈채팅 Response용 메시지 구조
           * "Uri":"openchat",
           * "OpCode":Request 메시지의 OpCode,
           * "Rs":0,
           * "PId":"게임의 PId",
           * "Contents":ChatMessage Object List
           */

          let self = NetworkManager.self;
          
          self.responsed = false;
          var json = JSON.parse(response);
          
          //console.log(`onResponseOpenChat`);
          //console.log(json);

        if (json.Uri.toLowerCase() != "openchat")
        {
            console.warn(`[onResponseOpenChat] Invalid Uri(${json.Uri.toLowerCase()})`);
            return;
        }
        
        if (json.Rs != 0)
        {
            console.warn(`[onResponseOpenChat] Rs is ${json.Rs}`);
            return;
        }

        if (1 != json.OpCode && 2 != json.OpCode)
        {
            console.warn(`[onResponseOpenChat] Invalid OpCode(${json.OpCode})`);
            return;
        }

        CHAT.setChatList(json.Contents);
        
        self.responsed = true;
    }
};

module.exports = NetworkManager.getInstance();
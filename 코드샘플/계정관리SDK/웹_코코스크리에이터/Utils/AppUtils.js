var Samanda = require("Samanda");

module.exports = 
{    
    //using keys - todo : enum?
    //token : user token
    //clear : clear all Data (Web -> App)
    overrides : {},

    SetAppData : function(key, value)
    {
        //console.log('try SetAppData');
        try {
            var data = new Object();
            data.key = key;
            data.value = value;
            var jsonMessage = JSON.stringify(data);
            Samanda.call(jsonMessage);
        } 
        catch(e){
            console.log("[error] not found app handler(standalone services is not yet)");
        }
    },

    SetLogOut : function()
    {
        console.log('try LogOut');
        try {
            var data = new Object();
            data.key = 'logout';
            data.value = '';
            var jsonMessage = JSON.stringify(data);
            Samanda.call(jsonMessage);
        } 
        catch(e){
            console.log("[error] not found app handler(standalone services is not yet)");
        }
    },

    SetGoogleAuthClear : function()
    {
        console.log('try SetGoogleAuthClear');
        try {
            var data = new Object();
            data.key = 'auth';
            data.value = 'cleargoogle';
            var jsonMessage = JSON.stringify(data);
            Samanda.call(jsonMessage);
        } 
        catch(e){
            console.log("[error] not found app handler(standalone services is not yet)");
        }
    },

    //mode 0 : FullScreen
    //mode 1 : HideScreen
    //mode 2 : ChatOnlyScreen
    sendWebState : function(state)
    {        
        //console.log('try SendWebState : ' + state);
        try {
            var data = new Object();
            data.key = 'state';
            switch(state)
            {
                case 0:
                    data.value = 'splash_done';
                    break;
                case 1:
                    data.value = 'init'; 
                    break;
                case 2:
                    data.value = 'need_login'; 
                    break;
                case 3:
                    data.value = 'pass_login'; 
                    break;
                case 4:
                    data.value = 'banner'; 
                    break;
                case 5:
                    data.value = 'main'; 
                    break;
                case 6:
                    data.value = 'hide'; 
                    break;
                case 7:
                    data.value = 'chat_left'; 
                    break;
                case 8:
                    data.value = 'chat_right'; 
                    break;
                case 9:
                    data.value = 'chat_custom'; 
                    break;
            }            
            var jsonMessage = JSON.stringify(data);
            Samanda.call(jsonMessage);
        } 
        catch(e){
            console.log("[error] not found app handler(standalone services is not yet)");
        }
    },

    sendChatRoomType : function(roomType)
    {
        //console.log('try sendChatRoom');
        try {
            var type = 'chatroomtype';
            var data = new Object();            
            data.key = type;
            data.value = roomType;

            var jsonMessage = JSON.stringify(data);
            Samanda.call(jsonMessage);
        } 
        catch(e){
            console.log("[error] not found app handler(standalone services is not yet)");
        }
    },

    sendChatProfile : function(nick, url)
    {
        //console.log('try sendChatRoom');
        try {
            var profile = new Object();
            profile.key = nick;
            profile.value = url;
            var val = JSON.stringify(profile);
            
            var type = 'chatprofile';
            var data = new Object();            
            data.key = type;
            data.value = val;

            var jsonMessage = JSON.stringify(data);
            Samanda.call(jsonMessage);
        } 
        catch(e){
            console.log("[error] not found app handler(standalone services is not yet)");
        }
    },

    sendNickName : function(nick)
    {
        try {
            var data = new Object();            
            data.key = 'nic';
            data.value = nick;

            var jsonMessage = JSON.stringify(data);
            Samanda.call(jsonMessage);
        } 
        catch(e){
            console.log("[error] not found app handler(standalone services is not yet)");
        }
    },

    sendLinks : function(links)
    {
        try {
            var data = new Object();            
            data.key = 'links';
            data.value = JSON.stringify(links);

            var jsonMessage = JSON.stringify(data);
            Samanda.call(jsonMessage);
        } 
        catch(e){
            console.log("[error] not found app handler(standalone services is not yet)");
        }
    },

    sendChatMessage : function(data)
    {
        //console.log('try sendChatMessage');
        try {
            var type = 'chat';
            var msg = new Object();     
            
            msg.key = data.Sender;
            
            if(data.Image)
            {
                type = 'chat_image';
                msg.value = data.Image;
            }   
            else if(data.Message)
            {
                type = 'chat';
                msg.value = data.Message;
            }
            
            var val = JSON.stringify(msg);
            
            var data2 = new Object();
            data2.key = type;
            data2.value = val;

            var jsonMessage = JSON.stringify(data2);
            Samanda.call(jsonMessage);
        } 
        catch(e){
            console.log("[error] not found app handler(standalone services is not yet)");
        }
    },

    SetOverrides(data) {
        if (false == Array.isArray(data)) {
            console.log('false == Array.isArray(data)');
            return;
        }
        
        for (let i = 0; data.length > i; ++i) {
            let o = data[i];
            if ('object' != typeof o || !('key' in o) || !('val' in o)) {
                continue;
            }
            this.overrides[o.key] = o.val;
        }
    },

    GetOverrideVal(key) {
        if (key in this.overrides) {
            return this.overrides[key];
        } else {
            return false;
        }
    },
}